using System;
using System.Collections.Generic;


namespace TeoriaDosGrafos.Classes
{
	public class Grafo
	{
		public Grafo()
		{
            Vertices = new List<Vertice>();
            Arestas = new List<Aresta>();
		}

	    //public int[,] Matriz { get; set; }
	    public List<Vertice> Vertices { get; set; }
	    public List<Aresta> Arestas { get; set; }

	    //public Vertice LerVertice()
	    //{
     //       Vertice vertice = new Vertice();
     //       Console.WriteLine("Vertice: ");
     //       vertice.Nome = Console.ReadLine();
     //       Vertices.Add(vertice);
	    //    return vertice;
	    //}

	    //public Aresta LerArestaSource(Vertice vertice)
	    //{
	    //    Aresta edge = new Aresta();

	    //    Console.WriteLine("De: ");
	    //    vertice.Nome = Console.ReadLine();
	    //    if (!Vertices.Contains(vertice))
	    //        return null;
	    //    else
	    //    {
	    //        edge.Origem = vertice.Nome;
	    //        Arestas.Add(edge);
	    //        return edge;
	    //    }
	        
	    //}

	    //public Aresta LerArestaTarget(Vertice vertice)
	    //{
     //       Aresta edge = new Aresta();
     //       Console.WriteLine("Para: ");
     //       string target = Console.ReadLine();
     //       if (!Vertices.Contains(vertice))
     //           return null;
     //       else
     //       {
     //           edge.Destino = target;
     //           Arestas.Add(edge);
     //           return edge;
     //       }
	    //}

	    //public int EdgeWeight()
	    //{
	    //    Aresta edge = new Aresta {Peso = int.Parse(Console.ReadLine())};
	    //    return edge.Peso;
	    //}

     //   public void PopularAdjacentes(Aresta edge, Vertice vertex)
     //   {
     //       Matriz[edge.ID, vertex.ID] = 1;
     //       Matriz[vertex.ID, edge.ID] = 1;
     //   }

	    //public string ExcluiVertice(Vertice vertice)
	    //{
	    //    if (Vertices.Contains(vertice))
	    //    {
	    //        Vertices.Remove(vertice);
	    //        return "Excluido com sucesso!";
	    //    }
	    //    else
     //           return "Vértice não encontrado!";
     //   }

	    //public Vertex VerticeAdjacente(Vertex vertice)
	    //{
	    //    return;
	    //}

	    //public int GrauVertice()
	    //{
	    //    return 
	    //}

	    //public int GrauMinimo();
	    //{
	    //    return ;
	    //}

     //   public int GrauMedio();
	    //{
	    //    return ;
	    //}

     //   public int GrauMaximo();
	    //{
	    //    return ;
	    //}

	    public bool Conexo()
	    {
	        return true;
	    }

	}
}
