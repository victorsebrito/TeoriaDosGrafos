using System;
using Grapevine.Server;
using System.Threading;

namespace TeoriaDosGrafos 
{
    class MainClass
	{
        public static Graph graph { get; set; }

		public static void Main(string[] args)
		{
            var server = new RESTServer();
            server.Start();

            while (server.IsListening)
            {
                Thread.Sleep(300);
            }

            Console.WriteLine("Press Enter to Continue...");
            Console.ReadLine();

      //      Graph graph = new Graph();
		    //do
		    //{
		    //    var vertice = graph.LerVertice();
		    //    var source = graph.LerArestaSource(vertice);
		    //    var target = graph.LerArestaTarget(vertice);
		    //    var edgeWeight = graph.EdgeWeight();
		       
		    //} while (Console.ReadLine() != "");

		}
        
	    public static void LerArquivo()
	    {
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(@"C:");

            string [] txt = System.IO.File.ReadAllLines(@"C:\");
           

            Console.ReadLine();
        }
        
	}
}
