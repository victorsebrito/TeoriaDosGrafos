using System;
using System.Collections.Generic;


namespace TeoriaDosGrafos
{
	public class Graph
	{
		public Graph()
		{
		}

	    public int[,] Matriz { get; set; }
	    public List<Vertex> Vertices { get; set; }
	    public List<Edge> Edges { get; set; }

	    public Vertex LerVertice()
	    {
            Vertex vertice = new Vertex();
            Console.WriteLine("Vertice: ");
            vertice.name = Console.ReadLine();
            Vertices.Add(vertice);
	        return vertice;
	    }

	    public Edge LerArestaSource(Vertex vertice)
	    {
	        Edge edge = new Edge();

	        Console.WriteLine("De: ");
	        vertice.name = Console.ReadLine();
	        if (!Vertices.Contains(vertice))
	            return null;
	        else
	        {
	            edge.Source = vertice.name;
	            Edges.Add(edge);
	            return edge;
	        }
	        
	    }

	    public Edge LerArestaTarget(Vertex vertice)
	    {
            Edge edge = new Edge();
            Console.WriteLine("Para: ");
            string target = Console.ReadLine();
            if (!Vertices.Contains(vertice))
                return null;
            else
            {
                edge.Target = target;
                Edges.Add(edge);
                return edge;
            }
	    }

	    public int EdgeWeight()
	    {
	        Edge edge = new Edge {Weight = int.Parse(Console.ReadLine())};
	        return edge.Weight;
	    }

        public void PopularAdjacentes(Edge edge, Vertex vertex)
        {
            Matriz[edge.id, vertex.id] = 1;
            Matriz[vertex.id, edge.id] = 1;
        }

	    public string ExcluiVertice(Vertex vertice)
	    {
	        if (Vertices.Contains(vertice))
	        {
	            Vertices.Remove(vertice);
	            return "Excluido com sucesso!";
	        }
	        else
                return "Vértice não encontrado!";
        }

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
