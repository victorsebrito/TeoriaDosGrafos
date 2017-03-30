using Grapevine;
using Grapevine.Interfaces.Server;
using Grapevine.Server;
using Grapevine.Server.Attributes;
using Grapevine.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using TeoriaDosGrafos.Classes;
using System.Linq;
using System.Text;
using System.IO;

namespace TeoriaDosGrafos.API
{
    [RestResource]
    public sealed class APIResources
    {

        [RestRoute]
        public IHttpContext SetAccessControlHeader(IHttpContext context)
        {
            context.Response.AddHeader("Access-Control-Allow-Origin", "*");
            context.Response.AddHeader("Access-Control-Allow-Methods", "POST, GET, DELETE");
            context.Response.AddHeader("Access-Control-Allow-Headers", "X-Grafo-ID");
            context.Response.AddHeader("Access-Control-Expose-Headers", "X-Grafo-ID");

            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.OPTIONS)]
        public IHttpContext ReturnOptions(IHttpContext context)
        {
            context.Response.SendResponse(HttpStatusCode.Ok);
            return context;
        }

        #region Grafo (/api/grafo)

        /// <summary>
        /// Retorna o grafo em JSON.
        /// </summary>
        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "api/grafo")]
        public IHttpContext GetGrafo(IHttpContext context)
        {
            Cliente loCliente = APIUtil.ValidarCliente(context);
            if (context.WasRespondedTo) return context;

            context.Response.ContentType = ContentType.JSON;
            context.Response.SendResponse(JsonConvert.SerializeObject(loCliente.Grafo));

            return context;
        }

        /// <summary>
        /// Retorna a matriz de adjacência.
        /// </summary>
        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "api/grafo/matriz")]
        public IHttpContext GetMatrizAdjacencia(IHttpContext context)
        {
            //int[,] loMatriz = new int[Servidor.Grafo.Vertices.Count, Servidor.Grafo.Vertices.Count];

            //int i, j;
            //i = 0;

            //foreach(Vertice loVertice in Servidor.Grafo.Vertices)
            //{
            //    j = 0;

            //    foreach(Vertice loVertice2 in Servidor.Grafo.Vertices)
            //    {
            //        if (APIUtil.FindArestasByVerticesIDs(loVertice.ID, loVertice2.ID).Count > 0)
            //            loMatriz[i, j] = 1;
            //        else
            //            loMatriz[i, j] = 0;

            //        j++;
            //    }
            //    i++;
            //}

            Cliente loCliente = APIUtil.ValidarCliente(context);
            if (context.WasRespondedTo) return context;

            StringBuilder loBuilder = new StringBuilder();
            loBuilder.Append("<table class=\"table table-bordered text-center\">");

            loBuilder.Append("<tr><th></th>");
            foreach (Vertice loVertice in loCliente.Grafo.Vertices)
                loBuilder.Append(String.Format("<th>{0} ({1})</th>", loVertice.ID, loVertice.Nome));
            loBuilder.Append("</tr>");

            int i, j;
            i = 0;
            foreach (Vertice loVertice in loCliente.Grafo.Vertices)
            {
                j = 0;

                loBuilder.Append("<tr>");
                loBuilder.Append(String.Format("<th>{0} ({1})</th>", loVertice.ID, loVertice.Nome));

                foreach (Vertice loVertice2 in loCliente.Grafo.Vertices)
                {
                    loBuilder.Append("<td>");
                    loBuilder.Append((APIUtil.FindArestasByVerticesIDs(loVertice.ID, loVertice2.ID, loCliente.Grafo).Count > 0) ? 1 : 0);
                    loBuilder.Append("</td>");
                    j++;
                }
                i++;

                loBuilder.Append("</tr>");
            }

            loBuilder.Append("</table>");

            context.Response.ContentType = ContentType.HTML;
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.SendResponse(loBuilder.ToString());

            return context;
        }

        /// <summary>
        /// Retorna o grau mínimo, médio e máximo do grafo.
        /// </summary>
        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "api/grafo/grau")]
        public IHttpContext GetGrauGrafo(IHttpContext context)
        {
            Cliente loCliente = APIUtil.ValidarCliente(context);
            if (context.WasRespondedTo) return context;

            APIUtil.Grau[] loListGrau = { new APIUtil.Grau(APIUtil.Grau.TiposGrau.Mínimo),
                                          new APIUtil.Grau(APIUtil.Grau.TiposGrau.Médio),
                                          new APIUtil.Grau(APIUtil.Grau.TiposGrau.Máximo) };
            int liSomaGraus = 0;

            foreach(Vertice loVertice in loCliente.Grafo.Vertices)
            {                
                int liGrau = APIUtil.GetGrauVertice(loVertice.ID, loCliente.Grafo);
                liSomaGraus += liGrau;

                if(loListGrau[0].Vertice == null)
                {
                    loListGrau[0].Vertice = loListGrau[2].Vertice = loVertice;
                    loListGrau[0].NumGrau = loListGrau[2].NumGrau = liGrau;
                }
                else
                {
                    if (loListGrau[0].NumGrau > liGrau)
                    {
                        loListGrau[0].Vertice = loVertice;
                        loListGrau[0].NumGrau = liGrau;
                    }

                    if (loListGrau[2].NumGrau < liGrau)
                    {
                        loListGrau[2].Vertice = loVertice;
                        loListGrau[2].NumGrau = liGrau;
                    }
                }
            }

            loListGrau[1].NumGrau = liSomaGraus / loCliente.Grafo.Vertices.Count;

            context.Response.ContentType = ContentType.JSON;
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.SendResponse(JsonConvert.SerializeObject(loListGrau));

            return context;
        }

        /// <summary>
        /// Verifica se o grafo é conexo.
        /// </summary>
        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "api/grafo/conexo")]
        public IHttpContext GetIsGrafoConexo(IHttpContext context)
        {
            Cliente loCliente = APIUtil.ValidarCliente(context);
            if (context.WasRespondedTo) return context;

            context.Response.ContentType = ContentType.JSON;

            foreach (Vertice loVertice in loCliente.Grafo.Vertices)
            {
                List<Vertice> loListaOutrosVertices = loCliente.Grafo.Vertices.Except(new List<Vertice>() { loVertice }).ToList();

                foreach (Vertice loVertice2 in loListaOutrosVertices)
                {
                    if (!APIUtil.ExisteCaminhoEntreVertices(loVertice.ID, loVertice2.ID, loCliente.Grafo))
                    {
                        context.Response.SendResponse(JsonConvert.SerializeObject(false));
                        return context;
                    }
                }
            }
            
            context.Response.SendResponse(JsonConvert.SerializeObject(true));
            return context;
        }

        /// <summary>
        /// Verifica se o grafo possui caminho de Euler.
        /// </summary>
        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "api/grafo/euler")]
        public IHttpContext GetPossuiCaminhoEuler(IHttpContext context)
        {
            Cliente loCliente = APIUtil.ValidarCliente(context);
            if (context.WasRespondedTo) return context;

            context.Response.ContentType = ContentType.JSON;

            int liNumVerticesGrauImpar = 0;

            foreach (Vertice loVertice in loCliente.Grafo.Vertices)
            {
                if (APIUtil.GetGrauVertice(loVertice.ID, loCliente.Grafo) % 2 != 0)
                    liNumVerticesGrauImpar++;

                if (liNumVerticesGrauImpar > 2)
                {
                    context.Response.SendResponse(JsonConvert.SerializeObject(false));
                    return context;
                }
                    
            }

            context.Response.SendResponse(JsonConvert.SerializeObject(true));
            return context;
        }

        /// <summary>
        /// Cria um novo grafo no servidor.
        /// Lê o grafo do arquivo se o parâmetro "file" (endereço do arquivo local) for enviado no corpo da solicitação.
        /// </summary>
        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "api/grafo")]
        public IHttpContext NovoGrafo(IHttpContext context)
        {
            Dictionary<string, string> loArgs = APIUtil.GetDictionaryFromContext(context);
            Cliente loCliente = APIUtil.NovoCliente();

            if (loArgs.ContainsKey("json"))
                loCliente.Grafo = JsonConvert.DeserializeObject<Grafo>(loArgs["json"]);
            else if (loArgs.ContainsKey("arquivo"))
            {
                string lsJSON = File.ReadAllText(loArgs["arquivo"]);
                loCliente.Grafo = JsonConvert.DeserializeObject<Grafo>(lsJSON);
            }
            else
            {
                loCliente.Grafo = new Grafo();

                if (loArgs.ContainsKey("vertice"))
                {
                    loCliente.Grafo.Vertices = APIUtil.GetListaVerticesByArgs(loArgs["vertice"]);

                    if (loArgs.ContainsKey("aresta"))
                    {
                        List<Aresta> loListaArestas = new List<Aresta>();
                        loListaArestas = APIUtil.GetListaArestasFromArgs(loArgs["aresta"]);
                        loListaArestas = APIUtil.ValidaListaArestas(loListaArestas, loCliente.Grafo);

                        loCliente.Grafo.Arestas = loListaArestas;
                    }
                }
            }
            context.Response.AddHeader("X-Grafo-ID", loCliente.ID);
            context.Response.SendResponse("");
            return context;
        }

        #endregion

        #region Vertice (/api/vertice)

        /// <summary>
        /// Insere um vértice.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "api/vertice")]
        public IHttpContext NovoVertice(IHttpContext context)
        {
            Cliente loCliente = APIUtil.ValidarCliente(context);
            if (context.WasRespondedTo) return context;

            Dictionary<string, string> loArgs = APIUtil.GetDictionaryFromContext(context);

            Vertice loVertice = APIUtil.FindVerticeByArgs(loArgs, context);

            if (context.WasRespondedTo) return context;

            if (loVertice == null)
            {
                int liID = Convert.ToInt32(loArgs["id"]);
                string lsNome = (loArgs.ContainsKey("nome")) ? loArgs["nome"] : "";

                loCliente.Grafo.Vertices.Add(new Vertice(liID, lsNome));

                context.Response.SendResponse(HttpStatusCode.Ok);
            }
            else
                context.Response.SendResponse(HttpStatusCode.Conflict);

            return context;
        }

        /// <summary>
        /// Remove um vértice.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [RestRoute(HttpMethod = HttpMethod.DELETE, PathInfo = "api/vertice")]
        public IHttpContext ApagarVertice(IHttpContext context)
        {
            Cliente loCliente = APIUtil.ValidarCliente(context);
            if (context.WasRespondedTo) return context;

            Dictionary<string, string> loArgs = APIUtil.GetDictionaryFromContext(context);

            Vertice loVertice = APIUtil.FindVerticeByArgs(loArgs, context);

            if (loVertice != null)
            {
                APIUtil.RemoverVertice(loVertice, loCliente.Grafo);
                context.Response.SendResponse(HttpStatusCode.Ok);
            }
            else
                context.Response.SendResponse(HttpStatusCode.NotFound);

            return context;
        }

        /// <summary>
        /// Retorna a lista de arestas do vértice.
        /// </summary>
        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "api/vertice/arestas")]
        public IHttpContext GetArestasOfVertice(IHttpContext context)
        {
            Cliente loCliente = APIUtil.ValidarCliente(context);
            if (context.WasRespondedTo) return context;

            Dictionary<string, string> loArgs = APIUtil.GetDictionaryFromContext(context);

            Vertice loVertice = APIUtil.FindVerticeByArgs(loArgs, context);

            if (loVertice != null)
            {
                List<Aresta> loListaArestas = APIUtil.GetArestasOfVertice(loVertice.ID, loCliente.Grafo);

                context.Response.ContentType = ContentType.JSON;
                context.Response.SendResponse(JsonConvert.SerializeObject(loListaArestas));
            }
            else
                context.Response.SendResponse(HttpStatusCode.NotFound);

            return context;
        }

        /// <summary>
        /// Retorna a lista de vértices adjacentes ao vértice passado por parâmetro.
        /// </summary>
        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "api/vertice/adjacentes")]
        public IHttpContext ListarVerticesAdjacentes(IHttpContext context)
        {
            Cliente loCliente = APIUtil.ValidarCliente(context);
            if (context.WasRespondedTo) return context;

            Dictionary<string, string> loArgs = APIUtil.GetDictionaryFromContext(context);

            Vertice loVertice = APIUtil.FindVerticeByArgs(loArgs, context);

            if (loVertice != null)
            {
                List<Vertice> loListaVertices = APIUtil.FindVerticesAdjacentesByID(loVertice.ID, loCliente.Grafo).OrderBy(v => v.ID).ToList();

                context.Response.ContentType = ContentType.JSON;
                context.Response.SendResponse(JsonConvert.SerializeObject(loListaVertices));
            }
            else
                context.Response.SendResponse(HttpStatusCode.NotFound);

            return context;
        }

        /// <summary>
        /// Retorna o grau do vértice passado por parâmetro.
        /// </summary>
        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "api/vertice/grau")]
        public IHttpContext GetGrauVertice(IHttpContext context)
        {
            Cliente loCliente = APIUtil.ValidarCliente(context);
            if (context.WasRespondedTo) return context;

            Dictionary<string, string> loArgs = APIUtil.GetDictionaryFromContext(context);

            Vertice loVertice = APIUtil.FindVerticeByArgs(loArgs, context);

            if (loVertice != null)             
                context.Response.SendResponse(APIUtil.GetGrauVertice(loVertice.ID, loCliente.Grafo).ToString());            
            else
                context.Response.SendResponse(HttpStatusCode.NotFound);

            return context;
        }

        #endregion

        #region Aresta (/api/aresta)

        /// <summary>
        /// Lista todas as arestas relacionadas aos vértices indicados pelos parâmetros da requisição.
        /// </summary>
        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "api/aresta/lista")]
        public IHttpContext ListaArestas(IHttpContext context)
        {
            Cliente loCliente = APIUtil.ValidarCliente(context);
            if (context.WasRespondedTo) return context;

            Dictionary<string, string> loArgs = APIUtil.GetDictionaryFromContext(context);

            if (loArgs.ContainsKey("vertice1") && loArgs.ContainsKey("vertice2"))
            {
                try
                {
                    int liVertice1 = Convert.ToInt32(loArgs["vertice1"]);
                    int liVertice2 = Convert.ToInt32(loArgs["vertice2"]);

                    if (APIUtil.FindVerticeByID(liVertice1, loCliente.Grafo) != null && APIUtil.FindVerticeByID(liVertice2, loCliente.Grafo) != null)
                    {
                        List<Aresta> loListaArestas = APIUtil.FindArestasByVerticesIDs(liVertice1, liVertice2, loCliente.Grafo);

                        context.Response.ContentType = ContentType.JSON;
                        context.Response.SendResponse(JsonConvert.SerializeObject(loListaArestas));
                    }
                    else
                        context.Response.SendResponse(HttpStatusCode.NotFound);
                }
                catch (Exception ex)
                {
                    Servidor.Server.Logger.Error(ex.Message);
                    context.Response.SendResponse(HttpStatusCode.BadRequest);
                }
            }
            return context;
        }

        /// <summary>
        /// Insere uma aresta.
        /// </summary>
        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "api/aresta")]
        public IHttpContext NovaAresta(IHttpContext context)
        {
            Cliente loCliente = APIUtil.ValidarCliente(context);
            if (context.WasRespondedTo) return context;

            Dictionary<string, string> loArgs = APIUtil.GetDictionaryFromContext(context);

            if (loArgs.ContainsKey("origem") && loArgs.ContainsKey("destino") && loArgs.ContainsKey("peso"))
            {
                try
                {
                    int liOrigem = Convert.ToInt32(loArgs["origem"]);
                    int liDestino = Convert.ToInt32(loArgs["destino"]);
                    int liPeso = Convert.ToInt32(loArgs["peso"]);
                    
                    if (APIUtil.FindVerticeByID(liOrigem, loCliente.Grafo) != null && APIUtil.FindVerticeByID(liDestino, loCliente.Grafo) != null)
                    {
                        loCliente.Grafo.Arestas.Add(new Aresta(liOrigem, liDestino, liPeso));
                        context.Response.SendResponse(HttpStatusCode.Ok);
                    }
                    else                    
                        context.Response.SendResponse(HttpStatusCode.NotFound);                    
                }
                catch(Exception ex)
                {
                    Servidor.Server.Logger.Error(ex.Message);
                    context.Response.SendResponse(HttpStatusCode.BadRequest);
                }
            }            
            return context;
        }

        /// <summary>
        /// Remove todas as arestas relacionadas aos vértices indicados pelos parâmetros da requisição.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [RestRoute(HttpMethod = HttpMethod.DELETE, PathInfo = "api/aresta")]
        public IHttpContext DeletaAresta(IHttpContext context)
        {
            Cliente loCliente = APIUtil.ValidarCliente(context);
            if (context.WasRespondedTo) return context;

            Dictionary<string, string> loArgs = APIUtil.GetDictionaryFromContext(context);

            if (loArgs.ContainsKey("vertice1") && loArgs.ContainsKey("vertice2"))
            {
                try
                {
                    int liVertice1 = Convert.ToInt32(loArgs["vertice1"]);
                    int liVertice2 = Convert.ToInt32(loArgs["vertice2"]);
                    
                    if (APIUtil.FindVerticeByID(liVertice1, loCliente.Grafo) != null && APIUtil.FindVerticeByID(liVertice2, loCliente.Grafo) != null)
                    {
                        List<Aresta> loListaArestas = APIUtil.FindArestasByVerticesIDs(liVertice1, liVertice2, loCliente.Grafo);
                        loCliente.Grafo.Arestas = loCliente.Grafo.Arestas.Except(loListaArestas).ToList();
                        context.Response.SendResponse(HttpStatusCode.Ok);
                    }
                    else
                        context.Response.SendResponse(HttpStatusCode.NotFound);
                }
                catch (Exception ex)
                {
                    Servidor.Server.Logger.Error(ex.Message);
                    context.Response.SendResponse(HttpStatusCode.BadRequest);
                }
            }
            return context;
        }
        #endregion
    }
}
