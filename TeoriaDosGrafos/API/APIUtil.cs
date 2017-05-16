using Grapevine.Interfaces.Server;
using Grapevine.Server;
using Grapevine.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using TeoriaDosGrafos.Classes;

namespace TeoriaDosGrafos.API
{
    public static class APIUtil
    {
        public const int INF = 99999;
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

        /// <summary>
        /// Gera matriz booleana
        /// </summary>
        /// <param name="aoMatriz"></param>
        /// <returns></returns>
        public static object ConverteMatriz(object aoMatriz, int aiVerticesCount)
        {
            if (aoMatriz is int[,] loMatriz)
            {                
                bool[,] loBoolMatriz = new bool[aiVerticesCount, aiVerticesCount];

                for (int i = 0; i < aiVerticesCount; i++)
                {
                    for (int j = 0; j < aiVerticesCount; j++)
                    {
                        if (loMatriz[i, j] == 1)
                            loBoolMatriz[i, j] = true;
                        else
                            loBoolMatriz[i, j] = false;
                    }
                }

                return loBoolMatriz;
            }
            else if (aoMatriz is bool[,] loMatrizB)
            {
                int[,] loMatrizI = new int[aiVerticesCount, aiVerticesCount];
                for (int i = 0; i < aiVerticesCount; i++)
                {
                    for (int j = 0; j < aiVerticesCount; j++)
                    {
                        if (loMatrizB[i, j])
                            loMatrizI[i, j] = 1;
                        else
                            loMatrizI[i, j] = 0;
                    }
                }

                return loMatrizI;
            }
            return null;
        }

        /// <summary>
        /// Gera matriz de adjacência.
        /// </summary>
        /// <param name="aoGrafo"></param>
        /// <returns></returns>
        public static int[,] GetMatrizAcessibilidade(Grafo aoGrafo)
        {
            int[,] loMatriz = GetMatrizAdjacencia(aoGrafo);
            bool[,] loMatrizBool = (bool[,])ConverteMatriz(loMatriz, aoGrafo.Vertices.Count);

            //Algoritmo de Warshall
            for (int k = 0; k < aoGrafo.Vertices.Count; ++k)
            {
                for (int i = 0; i < aoGrafo.Vertices.Count; ++i)
                {
                    for (int j = 0; j < aoGrafo.Vertices.Count; ++j)
                        loMatrizBool[i, j] = loMatrizBool[i, j] && (loMatrizBool[i, k] || loMatrizBool[i, k]);
                }
            }

            return (int[,])ConverteMatriz(loMatrizBool, aoGrafo.Vertices.Count);
        }

        /// <summary>
        /// Gera matriz de adjacência.
        /// </summary>
        /// <param name="aoGrafo"></param>
        /// <returns></returns>
        public static int[,] GetMatrizAdjacenciaPeso(Grafo aoGrafo)
        {
            int[,] loMatriz = new int[aoGrafo.Vertices.Count, aoGrafo.Vertices.Count];

            int i, j;
            i = 0;

            foreach (Vertice loVertice in aoGrafo.Vertices)
            {
                j = 0;

                foreach (Vertice loVertice2 in aoGrafo.Vertices)
                {
                    List<Aresta> loListaArestas = APIUtil.FindArestasByVerticesIDs(loVertice.ID, loVertice2.ID, aoGrafo).OrderBy(a => a.Peso).Cast<Aresta>().ToList();
                    Aresta loMenorAresta = (loListaArestas.Count != 0) ? loListaArestas[0] : null;

                    loMatriz[i, j] = (loMenorAresta != null) ? loMenorAresta.Peso : -1;

                    j++;
                }
                i++;
            }

            return loMatriz;
        }

        /// <summary>
        /// Gera matriz de adjacência.
        /// </summary>
        /// <param name="aoGrafo"></param>
        /// <returns></returns>
        public static int[,] GetMatrizAdjacencia(Grafo aoGrafo)
        {
            int[,] loMatriz = new int[aoGrafo.Vertices.Count, aoGrafo.Vertices.Count];

            int i, j;
            i = 0;

            foreach (Vertice loVertice in aoGrafo.Vertices)
            {
                j = 0;

                foreach (Vertice loVertice2 in aoGrafo.Vertices)
                {
                    loMatriz[i, j] = (APIUtil.FindArestasByVerticesIDs(loVertice.ID, loVertice2.ID, aoGrafo).Count > 0) ? 1 : 0;
                    j++;
                }
                i++;
            }
            return loMatriz;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aoGrafo"></param>
        /// <returns></returns>
        public static int[,] GetMenorCaminhoFloydWarshall(Grafo aoGrafo)
        {
            int[,] loMatriz = GetMatrizAdjacenciaPeso(aoGrafo);
            int[,] loMenorCaminho = FloydWarshall(loMatriz, aoGrafo.Vertices.Count);           

            return loMenorCaminho;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="aoGrafo"></param>
        /// <param name="sourceID"></param>
        /// <returns></returns>
        public static int[] GetMenorCaminhoDijkstra(Grafo aoGrafo, int sourceID)
        {
            int[,] loMatriz = GetMatrizAdjacenciaPeso(aoGrafo);
            Vertice vertice = FindVerticeByID(sourceID, aoGrafo);

            int[] loMenorCaminho = Dijkstra(loMatriz, vertice.ID, aoGrafo.Vertices.Count);
                       
            return loMenorCaminho;
        }

        /// <summary>
        /// Recebe matriz de pesos e retorna menor caminho
        /// </summary>
        /// <param name="aoGrafo"></param>
        /// <param name="sourceID"></param>
        /// <returns></returns>
        public static int[] GetMenorCaminhoBellmanFord(Grafo aoGrafo, int sourceID)
        {
            int[,] loMatriz = GetMatrizAdjacenciaPeso(aoGrafo);
            Vertice vertice = FindVerticeByID(sourceID, aoGrafo);

            int[] loMenorCaminho = BellmanFord(aoGrafo, vertice.ID);

            for(int i = 0; i < aoGrafo.Vertices.Count;i++)            
                Console.Write(loMenorCaminho[i]);
            
            return loMenorCaminho;
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

            foreach (Aresta loAresta in aoListaArestas)
            {
                if (loAresta.IsArestaValida(aoGrafo))
                    loListaArestas.Add(loAresta);
            }

            return loListaArestas;
        }

        #endregion

        #region Algoritmos
        public static int[,] FloydWarshall(int[,] graph, int verticesCount)
        {
            int[,] distance = new int[verticesCount, verticesCount];

            for (int i = 0; i < verticesCount; ++i)
                for (int j = 0; j < verticesCount; ++j)
                    distance[i, j] = graph[i, j];

            for (int k = 0; k < verticesCount; ++k)
            {
                for (int i = 0; i < verticesCount; ++i)
                {
                    for (int j = 0; j < verticesCount; ++j)
                    {
                        if (distance[i, k] + distance[k, j] < distance[i, j])
                            distance[i, j] = distance[i, k] + distance[k, j];
                    }
                }
            }
            return distance;
        }
                

        public static int[] BellmanFord(Grafo aoGrafo, int sourceID)
        {
            int verticesCount = aoGrafo.Vertices.Count;
            int edgesCount = aoGrafo.Arestas.Count;
            int[] distance = new int[verticesCount];

            for (int i = 0; i < verticesCount; i++)
                distance[i] = int.MaxValue;

            distance[sourceID] = 0;

            for (int i = 1; i <= verticesCount - 1; ++i)
            {
                for (int j = 0; j < edgesCount; ++j)
                {
                    int u = aoGrafo.Arestas[j].Origem;
                    int v = aoGrafo.Arestas[j].Destino;
                    int weight = aoGrafo.Arestas[j].Peso;

                    if (distance[u] != int.MaxValue && distance[u] + weight < distance[v])
                        distance[v] = distance[u] + weight;
                }
            }

            for (int i = 0; i < edgesCount; ++i)
            {
                int u = aoGrafo.Arestas[i].Origem;
                int v = aoGrafo.Arestas[i].Destino;
                int weight = aoGrafo.Arestas[i].Peso;

                if (distance[u] != int.MaxValue && distance[u] + weight < distance[v])
                    Console.WriteLine("Graph contains negative weight cycle.");
            }
            return distance;

            //PrintBellmanFord(distance, verticesCount);
        }

        private static void PrintBellmanFord(int[] distance, int verticesCount)
        {
            Console.WriteLine("Vertex   Distance from source");

            for (int i = 0; i < verticesCount; ++i)
                Console.WriteLine("{0}\t {1}", i, distance[i]);
        }

        public static int[] Dijkstra(int[,] graph, int source, int verticesCount)
        {
            int[] distance = new int[verticesCount];
            bool[] shortestPathTreeSet = new bool[verticesCount];

            for (int i = 0; i < verticesCount; ++i)
            {
                distance[i] = int.MaxValue;
                shortestPathTreeSet[i] = false;
            }

            distance[source] = 0;

            for (int count = 0; count < verticesCount - 1; ++count)
            {
                int u = MinimumDistance(distance, shortestPathTreeSet, verticesCount);
                shortestPathTreeSet[u] = true;

                for (int v = 0; v < verticesCount; ++v)
                    if (!shortestPathTreeSet[v] && Convert.ToBoolean(graph[u, v]) && distance[u] != int.MaxValue && distance[u] + graph[u, v] < distance[v])
                        distance[v] = distance[u] + graph[u, v];
            }

            return distance;
        }

        private static int MinimumDistance(int[] distance, bool[] shortestPathTreeSet, int verticesCount)
        {
            int min = int.MaxValue;
            int minIndex = 0;

            for (int v = 0; v < verticesCount; ++v)
            {
                if (shortestPathTreeSet[v] == false && distance[v] <= min)
                {
                    min = distance[v];
                    minIndex = v;
                }
            }

            return minIndex;
        }
        public static void Floyd(Grafo aoGrafo)
        {
            //ToDo
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

