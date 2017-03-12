using System;
using Grapevine.Server;
using System.Threading;
using TeoriaDosGrafos.Classes;

namespace TeoriaDosGrafos 
{
    class Servidor
	{
        public static Grafo Grafo { get; set; }
        public static RestServer Server { get; set; }

		public static void Main(string[] args)
		{
            Server = new RestServer();
            Server.LogToConsole().Start();

            while (Server.IsListening)
            {
                Thread.Sleep(300);
            }

            Console.WriteLine("Pressione Enter para continuar...");
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
