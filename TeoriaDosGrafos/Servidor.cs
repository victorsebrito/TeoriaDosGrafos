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
		}
        
	}
}
