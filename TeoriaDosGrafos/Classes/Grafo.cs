using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using TeoriaDosGrafos.API;

namespace TeoriaDosGrafos.Classes
{
	public class Grafo
	{
		public Grafo()
		{
            Vertices = new List<Vertice>();
            Arestas = new List<Aresta>();
		}        
        
	    public List<Vertice> Vertices { get; set; }
	    public List<Aresta> Arestas { get; set; }

    }
}
