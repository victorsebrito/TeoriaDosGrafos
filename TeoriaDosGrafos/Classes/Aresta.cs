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

        public int Origem { get; set; }
        public int Destino { get; set; }
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
