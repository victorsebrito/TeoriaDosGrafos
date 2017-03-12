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

        /// <summary>
        /// Converte uma string com uma lista de vértices em um objeto List<Vertices>.
        /// </summary>
        public static List<Vertice> GetListaVerticesByArgs(string asVertices)
        {
            string lsVertices = '[' + asVertices + ']';
            return JsonConvert.DeserializeObject<List<Vertice>>(lsVertices);
        }

        public static Vertice FindVerticeByArgs(Dictionary<string, string> aoArgs, IHttpContext context)
        {
            Vertice loVertice = null;

            if (aoArgs.ContainsKey("id"))
            {
                try
                {
                    int liID = Convert.ToInt32(aoArgs["id"]);
                    loVertice = Servidor.Grafo.Vertices.Find(v => v.ID == liID);
                }
                catch (Exception ex)
                {
                    Servidor.Server.Logger.Error(ex.Message);
                    context.Response.SendResponse(HttpStatusCode.BadRequest);
                }

            }
            else if (aoArgs.ContainsKey("nome"))
                loVertice = Servidor.Grafo.Vertices.Find(v => v.Nome.Equals(aoArgs["nome"]));

            return loVertice;

        }

        public static void RemoverVertice(Vertice aoVertice)
        {
            List<Aresta> loListArestas = APIUtil.GetArestasByVertice(aoVertice);

            foreach (Aresta loAresta in loListArestas)
                Servidor.Grafo.Arestas.Remove(loAresta);

            Servidor.Grafo.Vertices.Remove(aoVertice);
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


        public static List<Aresta> GetArestasByVertice(Vertice aoVertice)
        {
            return Servidor.Grafo.Arestas.FindAll(a => a.Origem == aoVertice.ID || a.Destino == aoVertice.ID);
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
