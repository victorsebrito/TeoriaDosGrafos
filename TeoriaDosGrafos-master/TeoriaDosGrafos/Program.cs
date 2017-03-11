using System;

namespace TeoriaDosGrafos 
{
    class MainClass
	{
		public static void Main(string[] args)
		{
		    Graph graph = new Graph();
		    do
		    {
		        var vertice = graph.LerVertice();
		        var source = graph.LerArestaSource(vertice);
		        var target = graph.LerArestaTarget(vertice);
		        var edgeWeight = graph.EdgeWeight();
		       
		    } while (Console.ReadLine() != "");

		}
        
	    public static void LerArquivo()
	    {
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(@"C:");

            string [] txt = System.IO.File.ReadAllLines(@"C:\");
           

            Console.ReadLine();
        }
        
	}
}
