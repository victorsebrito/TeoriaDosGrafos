using Grapevine.Interfaces.Server;
using Grapevine.Server;
using Grapevine.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using TeoriaDosGrafos.Classes;
using TeoriaDosGrafos.Classes.Util;

namespace TeoriaDosGrafos.API
{
    public static class APIUtil
    {
        public const int INF = 9999;

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
                return Servidor.Clientes.Find(c => c.ID.Equals(aoContext.Request.Headers["X-Grafo-ID"]));
            else
                return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aoContext"></param>
        public static void UpdateClientes(IHttpContext aoContext)
        {
            if (GetCliente(aoContext) != null)
                GetCliente(aoContext).LastUpdated = DateTime.Now;

            try
            {
                foreach (Cliente loCliente in Servidor.Clientes)
                {
                    if ((DateTime.Now - loCliente.LastUpdated).Minutes >= 15)
                        Servidor.Clientes.Remove(loCliente);
                }
            }
            catch { };

        }
        
        public static string GetMatrizHTML(Dictionary<Vertice, Dictionary<Vertice, int>> aoMatriz)
        {
            StringBuilder loBuilder = new StringBuilder();
            loBuilder.Append("<table class=\"table table-bordered text-center\">");
            loBuilder.Append("<tr><th></th>");

            foreach (KeyValuePair<Vertice, int> loKeyValuePair in aoMatriz.First().Value)
                loBuilder.Append(String.Format("<th>{0} ({1})</th>", loKeyValuePair.Key.ID, loKeyValuePair.Key.Nome));

            loBuilder.Append("</tr>");

            foreach (KeyValuePair<Vertice, Dictionary<Vertice, int>> loKeyValuePair in aoMatriz)
            {
                loBuilder.Append("<tr>");
                loBuilder.Append(String.Format("<th>{0} ({1})</th>", loKeyValuePair.Key.ID, loKeyValuePair.Key.Nome));

                foreach (KeyValuePair<Vertice, int> loKeyValuePair2 in loKeyValuePair.Value)
                {
                    loBuilder.Append("<td>");

                    int loValue = aoMatriz[loKeyValuePair.Key][loKeyValuePair2.Key];
                    loBuilder.Append((loValue != INF) ? loValue.ToString() : "-");

                    loBuilder.Append("</td>");
                }

                loBuilder.Append("</tr>");
            }

            loBuilder.Append("</table>");

            return loBuilder.ToString();
        }

        /// <summary>
        /// Gera matriz de adjacência.
        /// </summary>
        /// <param name="aoGrafo"></param>
        /// <returns></returns>
        public static MultiKeyDictionary<Vertice, Vertice, int> GetMatrizAdjacenciaPeso(Grafo aoGrafo)
        {
            MultiKeyDictionary<Vertice, Vertice, int> loMatriz = new MultiKeyDictionary<Vertice, Vertice, int>();

            foreach (Vertice loVertice in aoGrafo.Vertices)
            {
                foreach (Vertice loVertice2 in aoGrafo.Vertices)
                {
                    List<Aresta> loListaArestas = APIUtil.FindArestasByVerticesIDs(aoGrafo, loVertice, loVertice2).OrderBy(a => a.Peso).Cast<Aresta>().ToList();
                    Aresta loMenorAresta = (loListaArestas.Count != 0) ? loListaArestas[0] : null;
                    loMatriz[loVertice][loVertice2] = (loMenorAresta != null) ? loMenorAresta.Peso : INF;
                }
            }

            return loMatriz;
        }

        /// <summary>
        /// Gera matriz de adjacência.
        /// </summary>
        /// <param name="aoGrafo"></param>
        /// <returns></returns>
        public static MultiKeyDictionary<Vertice, Vertice, int> GetMatrizAdjacencia(Grafo aoGrafo)
        {
            MultiKeyDictionary<Vertice, Vertice, int> loMatriz = new MultiKeyDictionary<Vertice, Vertice, int>();

            foreach (Vertice loVertice in aoGrafo.Vertices)
                foreach (Vertice loVertice2 in aoGrafo.Vertices)
                    loMatriz[loVertice][loVertice2] = (APIUtil.FindArestasByVerticesIDs(aoGrafo, loVertice, loVertice2, true).Count > 0) ? 1 : 0;

            return loMatriz;
        }

        #endregion

        #region Vertices

        public class Grau
        {
            public enum TiposGrau { Mínimo = 0, Médio = 1, Máximo = 2 }

            public Grau(TiposGrau aoTipoGrau)
            {
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
                if (Int32.TryParse(aoArgs["id"], out liID))
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

            foreach (Vertice loVertice in loListaVerticesAdjacentes)
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
        /// Valida a aresta, verificando se a origem e o destino existem.
        /// </summary>
        /// <returns></returns>
        public static bool IsArestaValida(Grafo aoGrafo, Aresta aoAresta)
        {
            return aoGrafo.Vertices.Exists(v => v.ID == aoAresta.Origem) && aoGrafo.Vertices.Exists(v => v.ID == aoAresta.Destino);
        }

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
        public static List<Aresta> FindArestasByVerticesIDs(Grafo aoGrafo, Vertice aoVertice, Vertice aoVertice2, bool abConsiderarDirecao = false)
        {
            return (abConsiderarDirecao)
                ? aoGrafo.Arestas.FindAll(a => a.Origem == aoVertice.ID && a.Destino == aoVertice2.ID)
                : aoGrafo.Arestas.FindAll(a => (a.Origem == aoVertice.ID && a.Destino == aoVertice2.ID) ||
                                                       (a.Origem == aoVertice2.ID && a.Destino == aoVertice.ID));
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

            foreach (Aresta loAresta in aoListaArestas)
            {
                if (IsArestaValida(aoGrafo, loAresta))
                    loListaArestas.Add(loAresta);
            }

            return loListaArestas;
        }

        #endregion

        #region Algoritmos

        /// <summary>
        /// Gera matriz de adjacência.
        /// </summary>
        /// <param name="aoGrafo"></param>
        /// <returns></returns>
        public static MultiKeyDictionary<Vertice, Vertice, int> Warshall(Grafo aoGrafo)
        {
            MultiKeyDictionary<Vertice, Vertice, int> loMatrizAdjacencia = GetMatrizAdjacencia(aoGrafo);

            foreach (Vertice loVertice in aoGrafo.Vertices)
                foreach (Vertice loVertice2 in aoGrafo.Vertices)
                    foreach (Vertice loVertice3 in aoGrafo.Vertices)
                        loMatrizAdjacencia[loVertice2][loVertice3] =
                            ((loMatrizAdjacencia[loVertice2][loVertice3] != 0) || ((loMatrizAdjacencia[loVertice2][loVertice] != 0) && (loMatrizAdjacencia[loVertice][loVertice3] != 0))) ? 1 : 0;

            return loMatrizAdjacencia;
        }


        public static MultiKeyDictionary<Vertice, Vertice, int> FloydWarshall(Grafo aoGrafo, out int aiIterationsCount)
        {
            MultiKeyDictionary<Vertice, Vertice, int> loDistance = GetMatrizAdjacenciaPeso(aoGrafo);
            aiIterationsCount = 0;

            foreach (Vertice loVertice in aoGrafo.Vertices)
            {
                foreach (Vertice loVertice2 in aoGrafo.Vertices)
                {
                    foreach (Vertice loVertice3 in aoGrafo.Vertices)
                    {
                        if (loDistance[loVertice2][loVertice] + loDistance[loVertice][loVertice3] < loDistance[loVertice2][loVertice3])
                            loDistance[loVertice2][loVertice3] = loDistance[loVertice2][loVertice] + loDistance[loVertice][loVertice3];

                        aiIterationsCount++;
                    }
                }
            }

            return loDistance;
        }


        public static MultiKeyDictionary<Vertice, Vertice, int> BellmanFord(Grafo aoGrafo, Vertice aoOrigem, out int aiIterationsCount)
        {
            MultiKeyDictionary<Vertice, Vertice, int> loDistanceMatriz = new MultiKeyDictionary<Vertice, Vertice, int>();
            Dictionary<Vertice, int> loDistance = loDistanceMatriz[aoOrigem];
            aiIterationsCount = 0;

            aoGrafo.Vertices.ForEach(v => loDistance[v] = INF);

            loDistance[aoOrigem] = 0;

            for (int i = 0; i < aoGrafo.Vertices.Count - 1; i++)
            {
                foreach (Vertice loVertice in aoGrafo.Vertices)
                {
                    if (loDistance[loVertice] != INF)
                    {
                        foreach (Aresta loAresta in GetArestasOfVertice(loVertice.ID, aoGrafo))
                        {
                            Vertice loDestino = FindVerticeByID(loAresta.Destino, aoGrafo);
                            int liDistance = loDistance[loVertice] + loAresta.Peso;

                            if (liDistance < loDistance[loDestino])
                            {
                                //lbChanged = true;
                                loDistance[loDestino] = liDistance;
                            }

                            aiIterationsCount++;
                        }
                    }
                }
            }
            
            return loDistanceMatriz;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aoGrafo"></param>
        /// <param name="sourceID"></param>
        /// <returns></returns>
        public static MultiKeyDictionary<Vertice, Vertice, int> Dijkstra(Grafo aoGrafo, Vertice aoVerticeOrigem, Vertice aoVerticeDestino, out int aiIterationsCount)
        {
            MultiKeyDictionary<Vertice, Vertice, int> loMatriz = GetMatrizAdjacenciaPeso(aoGrafo);

            MultiKeyDictionary<Vertice, Vertice, int> loDistanceMatriz = new MultiKeyDictionary<Vertice, Vertice, int>();
            Dictionary<Vertice, int> loDistance = loDistanceMatriz[aoVerticeOrigem];

            Dictionary<Vertice, bool> loShortestPathTreeSet = new Dictionary<Vertice, bool>();

            aiIterationsCount = 0;

            aoGrafo.Vertices.ForEach(v =>
            {
                loDistance[v] = INF;
                loShortestPathTreeSet[v] = false;
            });

            loDistance[aoVerticeOrigem] = 0;

            foreach (Vertice loVertice in aoGrafo.Vertices.Except(new List<Vertice> { aoVerticeOrigem }))
            {
                Vertice u = MinimumDistance(loDistance, loShortestPathTreeSet, aoGrafo.Vertices, ref aiIterationsCount);
                loShortestPathTreeSet[u] = true;

                if (!loShortestPathTreeSet[aoVerticeDestino] && Convert.ToBoolean(loMatriz[u][aoVerticeDestino]) && loDistance[u] != INF && loDistance[u] + loMatriz[u][aoVerticeDestino] < loDistance[aoVerticeDestino])
                    loDistance[aoVerticeDestino] = loDistance[u] + loMatriz[u][aoVerticeDestino];
            }

            int liValue = loDistance[aoVerticeDestino];
            loDistance.Clear();
            loDistance[aoVerticeDestino] = liValue;

            return loDistanceMatriz;
        }

        /// <summary>
        /// Uma função utilitária para achar o vértice com a menor distância do conjunto de
        /// vértices que ainda não foram incluídos na árvore de menor caminho.
        /// </summary>
        /// <param name="aoDistance"></param>
        /// <param name="aoShortestPathTreeSet"></param>
        /// <param name="aoVertices"></param>
        /// <returns></returns>
        private static Vertice MinimumDistance(Dictionary<Vertice, int> aoDistance, Dictionary<Vertice, bool> aoShortestPathTreeSet, List<Vertice> aoVertices, ref int aiIterationsCount)
        {
            int liMin = INF;
            Vertice loMinVertice = null;

            foreach (Vertice loVertice in aoVertices)
            {
                if (!aoShortestPathTreeSet[loVertice] && aoDistance[loVertice] <= liMin)
                {
                    liMin = aoDistance[loVertice];
                    loMinVertice = loVertice;
                }

                aiIterationsCount++;
            }

            return loMinVertice;
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

