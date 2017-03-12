using Grapevine;
using Grapevine.Interfaces.Server;
using Grapevine.Server;
using Grapevine.Server.Attributes;
using Grapevine.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using TeoriaDosGrafos.Classes;

namespace TeoriaDosGrafos.API
{
    [RestResource]
    public sealed class APIResources
    {

        #region Grafo (/api/grafo)

        /// <summary>
        /// Retorna o grafo em JSON.
        /// </summary>
        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "api/grafo")]
        public IHttpContext GetGrafo(IHttpContext context)
        {
            context.Response.ContentType = ContentType.JSON;
            context.Response.SendResponse(JsonConvert.SerializeObject(Servidor.Grafo));

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

            if (loArgs.ContainsKey("file"))
            {

            }
            else
            {
                Servidor.Grafo = new Grafo();

                if (loArgs.ContainsKey("vertice"))
                {
                    Servidor.Grafo.Vertices = new List<Vertice>();
                    Servidor.Grafo.Vertices = APIUtil.GetListaVerticesByArgs(loArgs["vertice"]);
                }

                if (loArgs.ContainsKey("aresta"))
                {
                    Servidor.Grafo.Arestas = new List<Aresta>();
                    Servidor.Grafo.Arestas = APIUtil.GetListaArestasFromArgs(loArgs["aresta"]);
                }
            }

            context.Response.SendResponse("");
            return context;
        }

        #endregion

        #region Vertice (/api/vertice)
        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "api/vertice")]
        public IHttpContext NovoVertice(IHttpContext context)
        {
            Dictionary<string, string> loArgs = APIUtil.GetDictionaryFromContext(context);

            Vertice loVertice = APIUtil.FindVerticeByArgs(loArgs, context);

            if (loVertice == null)
            {
                int liID = Convert.ToInt32(loArgs["id"]);
                string lsNome = (loArgs.ContainsKey("name")) ? loArgs["nome"] : "";

                Servidor.Grafo.Vertices.Add(new Vertice(liID, lsNome));

                context.Response.SendResponse(HttpStatusCode.Ok);
            }
            else
                context.Response.SendResponse(HttpStatusCode.Conflict);

            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.DELETE, PathInfo = "api/vertice")]
        public IHttpContext ApagarVertice(IHttpContext context)
        {
            Dictionary<string, string> loArgs = APIUtil.GetDictionaryFromContext(context);

            Vertice loVertice = APIUtil.FindVerticeByArgs(loArgs, context);

            if (loVertice != null)
            {
                APIUtil.RemoverVertice(loVertice);
                context.Response.SendResponse(HttpStatusCode.Ok);
            }
            else
                context.Response.SendResponse(HttpStatusCode.NotFound);

            return context;
        }

        #endregion

        #region Aresta (/api/aresta)


        #endregion
    }
}
