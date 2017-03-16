using Grapevine.Interfaces.Server;
using Grapevine.Server;
using Grapevine.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using TeoriaDosGrafos.Classes;

namespace TeoriaDosGrafos.API
{
    public static class APIUtil
    {
        #region Grafos

        /// <summary>
        /// Verificar se o grafo da aplicação já foi instanciado.
        /// </summary>
        public static bool ValidarGrafo()
        {
            return (Servidor.Grafo != null);
        }

        #endregion

        #region Vertices

        public class Grau
        {
            public enum TiposGrau { Mínimo = 0, Médio = 1, Máximo = 2 }

            public Grau(TiposGrau aoTipoGrau) {
                this.TipoGrau = aoTipoGrau.ToString();
                this.Vertice = null;
                this.NumGrau = null;
            }
            
            public string TipoGrau { get; }
            public Vertice Vertice { get; set; }
            public int? NumGrau { get; set; }
        }

        /// <summary>
        /// Converte uma string com uma lista de vértices em um objeto List<Vertices>.
        /// </summary>
        public static List<Vertice> GetListaVerticesByArgs(string asVertices)
        {
            string lsVertices = '[' + asVertices + ']';
            return JsonConvert.DeserializeObject<List<Vertice>>(lsVertices);
        }

        /// <summary>
        /// Procura um vértice no grafo a partir do dicionário de argumentos enviados com a solicitação.
        /// </summary>
        /// <param name="aoArgs"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Vertice FindVerticeByArgs(Dictionary<string, string> aoArgs, IHttpContext context)
        {
            Vertice loVertice = null;

            if (aoArgs.ContainsKey("id"))
            {
				int liID;
                if(Int32.TryParse(aoArgs["id"], out liID))                
                    loVertice = FindVerticeByID(liID);
                else
                    context.Response.SendResponse(HttpStatusCode.BadRequest);
            }
            else if (aoArgs.ContainsKey("nome"))
                loVertice = FindVerticeByNome(aoArgs["nome"]);

            return loVertice;

        }

        /// <summary>
        /// Verifica se existe um caminho entre dois vértices.
        /// </summary>
        /// <param name="aiVertice1"></param>
        /// <param name="aiVertice2"></param>
        /// <param name="aoListaVerticesVisitados"></param>
        /// <returns></returns>
        public static bool ExisteCaminhoEntreVertices(int aiVertice1, int aiVertice2, List<int> aoListaVerticesVisitados)
        {

            if (aoListaVerticesVisitados == null)
                aoListaVerticesVisitados = new List<int>();
            aoListaVerticesVisitados.Add(aiVertice1);

            List<Vertice> loListaVerticesAdjacentes = FindVerticesAdjacentesByID(aiVertice1);

            foreach(Vertice loVertice in loListaVerticesAdjacentes)
            {
                if (loVertice.ID == aiVertice2)
                    return true;
                else
                {
                    if (!aoListaVerticesVisitados.Contains(loVertice.ID))
                    {
                        bool lbExisteCaminho = ExisteCaminhoEntreVertices(loVertice.ID, aiVertice2, aoListaVerticesVisitados);
                        if (lbExisteCaminho)
                            return true;
                    }
                        
                }

            }

            return false;
        }

        /// <summary>
        /// Verifica se existe um caminho entre dois vértices.
        /// </summary>
        /// <param name="aiVertice1"></param>
        /// <param name="aiVertice2"></param>
        /// <returns></returns>
        public static bool ExisteCaminhoEntreVertices(int aiVertice1, int aiVertice2)
        {
            return ExisteCaminhoEntreVertices(aiVertice1, aiVertice2, null);
        }

        /// <summary>
        /// Retorna a lista de vértices adjacentes ao vértice passado por parâmetro.
        /// </summary>
        /// <param name="aiID"></param>
        /// <returns></returns>
        public static List<Vertice> FindVerticesAdjacentesByID(int aiID)
        {
            List<Aresta> loListArestas = GetArestasOfVertice(aiID);
            List<Vertice> loListVertices = new List<Vertice>();
            Vertice loVerticeAux;

            foreach (Aresta loAresta in loListArestas)
            {
                if (loAresta.Origem == aiID)
                {
                    loVerticeAux = FindVerticeByID(loAresta.Destino);
                    if (!loListVertices.Contains(loVerticeAux)) loListVertices.Add(loVerticeAux);
                }
                else
                {
                    loVerticeAux = FindVerticeByID(loAresta.Origem);
                    if (!loListVertices.Contains(loVerticeAux)) loListVertices.Add(loVerticeAux);
                }
            }

            return loListVertices;
        }

        /// <summary>
        /// Procura um vértice no grafo a partir do seu ID.
        /// </summary>
        /// <param name="aiID"></param>
        /// <returns></returns>
        public static Vertice FindVerticeByID(int aiID)
        {
            return Servidor.Grafo.Vertices.Find(v => v.ID == aiID);
        }

        /// <summary>
        /// Procura um vértice no grafo a partir do seu nome.
        /// </summary>
        /// <param name="asNome"></param>
        /// <returns></returns>
        public static Vertice FindVerticeByNome(string asNome)
        {
            return Servidor.Grafo.Vertices.Find(v => v.Nome.Equals(asNome));
        }

        /// <summary>
        /// Remove um vértice do grafo.
        /// </summary>
        /// <param name="aoVertice"></param>
        public static void RemoverVertice(Vertice aoVertice)
        {
            List<Aresta> loListArestas = APIUtil.GetArestasOfVertice(aoVertice.ID);

            foreach (Aresta loAresta in loListArestas)
                Servidor.Grafo.Arestas.Remove(loAresta);

            Servidor.Grafo.Vertices.Remove(aoVertice);
        }
        /// <summary>
        /// Retorna o grau de um determinado vértice passado por parâmetro.
        /// </summary>
        /// <param name="aiID"></param>
        /// <returns></returns>
        public static int GetGrauVertice(int aiID)
        {
            return FindVerticesAdjacentesByID(aiID).Count;
        }

        #endregion

        #region Arestas
        /// <summary>
        /// Converte uma string com uma lista de arestas em um objeto List<Aresta>.
        /// </summary>
        public static List<Aresta> GetListaArestasFromArgs(string asArestas)
        {
            string lsArestas = '[' + asArestas + ']';
            return JsonConvert.DeserializeObject<List<Aresta>>(lsArestas);
        }

        /// <summary>
        /// Procura as arestas ligadas a um vértice.
        /// </summary>
        /// <param name="aoVertice"></param>
        /// <returns></returns>
        public static List<Aresta> GetArestasOfVertice(int aiID)
        {
            return Servidor.Grafo.Arestas.FindAll(a => a.Origem == aiID || a.Destino == aiID);
        }

        /// <summary>
        /// Retorna todas as arestas ligando dois vértices.
        /// </summary>
        /// <param name="aiIDVertice1"></param>
        /// <param name="aiIDVertice2"></param>
        /// <returns></returns>
        public static List<Aresta> FindArestasByVerticesIDs(int aiIDVertice1, int aiIDVertice2)
        {
            return Servidor.Grafo.Arestas.FindAll(a => (a.Origem == aiIDVertice1 && a.Destino == aiIDVertice2) ||
                                                       (a.Origem == aiIDVertice2 && a.Destino == aiIDVertice1));
        }

        /// <summary>
        /// Valida uma lista de arestas, retornando uma lista apenas com as válidas.
        /// </summary>
        /// <param name="aoListaArestas"></param>
        public static List<Aresta> ValidaListaArestas(List<Aresta> aoListaArestas)
        {
            List<Aresta> loListaArestas = new List<Aresta>();

            foreach(Aresta loAresta in aoListaArestas)
            {
                if (loAresta.IsArestaValida())
                    loListaArestas.Add(loAresta);
            }

            return loListaArestas;
        }

        #endregion


        /// <summary>
        /// Transformar uma query string ("var1=a&var2=b") em dicionário.
        /// </summary>
        public static Dictionary<string, string> GetDictionaryFromContext(IHttpContext context)
        {
            HttpRequest loRequest = (HttpRequest)context.Request;

            string lsBody = new StreamReader(loRequest.Advanced.InputStream, context.Request.ContentEncoding).ReadToEnd();
            NameValueCollection loNVC = System.Web.HttpUtility.ParseQueryString(lsBody);
            return loNVC.AllKeys.ToDictionary(k => k, k => loNVC[k]);
        }

    }
}
