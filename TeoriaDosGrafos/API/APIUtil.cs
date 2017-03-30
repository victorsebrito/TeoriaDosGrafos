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
        /// Verificar se o ID de autenticação é valido.
        /// </summary>
        /// <param name="aoContext"></param>
        /// <returns></returns>
        public static Cliente ValidarCliente(IHttpContext aoContext)
        {
            Cliente loCliente = GetCliente(aoContext);

            if (loCliente == null)
                aoContext.Response.SendResponse(HttpStatusCode.Unauthorized);

            return loCliente;
        }

        /// <summary>
        /// Criar nova sessão.
        /// </summary>
        /// <returns></returns>
        public static Cliente NovoCliente()
        {
            Cliente loCliente = new Cliente();
            Servidor.Clientes.Add(loCliente);
            return loCliente;
        }

        /// <summary>
        /// Carregar sessão.
        /// </summary>
        /// <param name="aoContext"></param>
        /// <returns></returns>
        public static Cliente GetCliente(IHttpContext aoContext)
        {
            if (aoContext.Request.Headers["X-Grafo-ID"] != null)
                return Servidor.Clientes.Find(c => c.ID == aoContext.Request.Headers["X-Grafo-ID"]);
            else
                return null;
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
        /// <param name="asVertices"></param>
        /// <returns></returns>
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
            Grafo loGrafo = GetCliente(context).Grafo;
            Vertice loVertice = null;

            if (aoArgs.ContainsKey("id"))
            {
				int liID;
                if(Int32.TryParse(aoArgs["id"], out liID))                
                    loVertice = FindVerticeByID(liID, loGrafo);
                else
                    context.Response.SendResponse(HttpStatusCode.BadRequest);
            }
            else if (aoArgs.ContainsKey("nome"))
                loVertice = FindVerticeByNome(aoArgs["nome"], loGrafo);

            return loVertice;

        }

        /// <summary>
        /// Verifica se existe um caminho entre dois vértices.
        /// </summary>
        /// <param name="aiVertice1"></param>
        /// <param name="aiVertice2"></param>
        /// <param name="aoGrafo"></param>
        /// <param name="aoListaVerticesVisitados"></param>
        /// <returns></returns>
        public static bool ExisteCaminhoEntreVertices(int aiVertice1, int aiVertice2, Grafo aoGrafo, List<int> aoListaVerticesVisitados)
        {

            if (aoListaVerticesVisitados == null)
                aoListaVerticesVisitados = new List<int>();
            aoListaVerticesVisitados.Add(aiVertice1);

            List<Vertice> loListaVerticesAdjacentes = FindVerticesAdjacentesByID(aiVertice1, aoGrafo);

            foreach(Vertice loVertice in loListaVerticesAdjacentes)
            {
                if (loVertice.ID == aiVertice2)
                    return true;
                else
                {
                    if (!aoListaVerticesVisitados.Contains(loVertice.ID))
                    {
                        if (ExisteCaminhoEntreVertices(loVertice.ID, aiVertice2, aoGrafo, aoListaVerticesVisitados))
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
        /// <param name="aoGrafo"></param>
        /// <returns></returns>
        public static bool ExisteCaminhoEntreVertices(int aiVertice1, int aiVertice2, Grafo aoGrafo)
        {
            return ExisteCaminhoEntreVertices(aiVertice1, aiVertice2, aoGrafo, null);
        }

        /// <summary>
        /// Retorna a lista de vértices adjacentes ao vértice passado por parâmetro.
        /// </summary>
        /// <param name="aiID"></param>
        /// <param name="aoGrafo"></param>
        /// <returns></returns>
        public static List<Vertice> FindVerticesAdjacentesByID(int aiID, Grafo aoGrafo)
        {
            List<Aresta> loListArestas = GetArestasOfVertice(aiID, aoGrafo);
            List<Vertice> loListVertices = new List<Vertice>();
            Vertice loVerticeAux;

            foreach (Aresta loAresta in loListArestas)
            {
                if (loAresta.Origem == aiID)
                {
                    loVerticeAux = FindVerticeByID(loAresta.Destino, aoGrafo);
                    if (!loListVertices.Contains(loVerticeAux)) loListVertices.Add(loVerticeAux);
                }
                else
                {
                    loVerticeAux = FindVerticeByID(loAresta.Origem, aoGrafo);
                    if (!loListVertices.Contains(loVerticeAux)) loListVertices.Add(loVerticeAux);
                }
            }

            return loListVertices;
        }

        /// <summary>
        /// Procura um vértice no grafo a partir do seu ID.
        /// </summary>
        /// <param name="aiID"></param>
        /// <param name="aoGrafo"></param>
        /// <returns></returns>
        public static Vertice FindVerticeByID(int aiID, Grafo aoGrafo)
        {
            return aoGrafo.Vertices.Find(v => v.ID == aiID);
        }

        /// <summary>
        /// Procura um vértice no grafo a partir do seu nome.
        /// </summary>
        /// <param name="asNome"></param>
        /// <param name="aoGrafo"></param>
        /// <returns></returns>
        public static Vertice FindVerticeByNome(string asNome, Grafo aoGrafo)
        {
            return aoGrafo.Vertices.Find(v => v.Nome.Equals(asNome));
        }

        /// <summary>
        /// Remove um vértice do grafo.
        /// </summary>
        /// <param name="aoVertice"></param>
        /// <param name="aoGrafo"></param>
        public static void RemoverVertice(Vertice aoVertice, Grafo aoGrafo)
        {
            List<Aresta> loListArestas = APIUtil.GetArestasOfVertice(aoVertice.ID, aoGrafo);

            foreach (Aresta loAresta in loListArestas)
                aoGrafo.Arestas.Remove(loAresta);

            aoGrafo.Vertices.Remove(aoVertice);
        }

        /// <summary>
        /// Retorna o grau de um determinado vértice passado por parâmetro.
        /// </summary>
        /// <param name="aiID"></param>
        /// <param name="aoGrafo"></param>
        /// <returns></returns>
        public static int GetGrauVertice(int aiID, Grafo aoGrafo)
        {
            return GetArestasOfVertice(aiID, aoGrafo).Count;
        }

        #endregion

        #region Arestas

        /// <summary>
        /// Converte uma string com uma lista de arestas em um objeto List<Aresta>.
        /// </summary>
        /// <param name="asArestas"></param>
        /// <returns></returns>
        public static List<Aresta> GetListaArestasFromArgs(string asArestas)
        {
            string lsArestas = '[' + asArestas + ']';
            return JsonConvert.DeserializeObject<List<Aresta>>(lsArestas);
        }

        /// <summary>
        /// Procura as arestas ligadas a um vértice.
        /// </summary>
        /// <param name="aiID"></param>
        /// <param name="aoGrafo"></param>
        /// <returns></returns>
        public static List<Aresta> GetArestasOfVertice(int aiID, Grafo aoGrafo)
        {
            return aoGrafo.Arestas.FindAll(a => a.Origem == aiID || a.Destino == aiID);
        }

        /// <summary>
        /// Retorna todas as arestas ligando dois vértices.
        /// </summary>
        /// <param name="aiIDVertice1"></param>
        /// <param name="aiIDVertice2"></param>
        /// <param name="aoGrafo"></param>
        /// <returns></returns>
        public static List<Aresta> FindArestasByVerticesIDs(int aiIDVertice1, int aiIDVertice2, Grafo aoGrafo)
        {
            return aoGrafo.Arestas.FindAll(a => (a.Origem == aiIDVertice1 && a.Destino == aiIDVertice2) ||
                                                       (a.Origem == aiIDVertice2 && a.Destino == aiIDVertice1));
        }

        /// <summary>
        /// Valida uma lista de arestas, retornando uma lista apenas com as válidas.
        /// </summary>
        /// <param name="aoListaArestas"></param>
        /// <param name="aoGrafo"></param>
        /// <returns></returns>
        public static List<Aresta> ValidaListaArestas(List<Aresta> aoListaArestas, Grafo aoGrafo)
        {
            List<Aresta> loListaArestas = new List<Aresta>();

            foreach(Aresta loAresta in aoListaArestas)
            {
                if (loAresta.IsArestaValida(aoGrafo))
                    loListaArestas.Add(loAresta);
            }

            return loListaArestas;
        }

        #endregion


        /// <summary>
        /// Transforma a query string do contexto ("var1=a&var2=b") em dicionário.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetDictionaryFromContext(IHttpContext context)
        {
            HttpRequest loRequest = (HttpRequest)context.Request;

            string lsBody = new StreamReader(loRequest.Advanced.InputStream, context.Request.ContentEncoding).ReadToEnd();
            NameValueCollection loNVC = System.Web.HttpUtility.ParseQueryString(lsBody);
            return loNVC.AllKeys.ToDictionary(k => k, k => loNVC[k]);
        }

    }
}
