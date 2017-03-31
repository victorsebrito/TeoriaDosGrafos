using Newtonsoft.Json;
using System;
using TeoriaDosGrafos.API;

namespace TeoriaDosGrafos.Classes
{
	public class Aresta
	{
        public Aresta(int aoOrigem, int aoDestino, int aoPeso)
        {
            this.Origem = aoOrigem;
            this.Destino = aoDestino;
            this.Peso = aoPeso;
        }

        [JsonProperty(PropertyName = "source")]
        public int Origem { get; set; }

        [JsonProperty(PropertyName = "target")]
        public int Destino { get; set; }

        [JsonProperty(PropertyName = "peso")]
        public int Peso { get; set; }


        /// <summary>
        /// Valida a aresta, verificando se a origem e o destino existem.
        /// </summary>
        /// <returns></returns>
        public bool IsArestaValida(Grafo aoGrafo)
        {
            return (APIUtil.FindVerticeByID(Origem, aoGrafo) != null && APIUtil.FindVerticeByID(Destino, aoGrafo) != null);
        }
    }
}
