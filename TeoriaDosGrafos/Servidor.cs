using System;
using Grapevine.Server;
using System.Threading;
using TeoriaDosGrafos.Classes;
using System.Collections.Generic;
using System.Linq;

namespace TeoriaDosGrafos 
{
    class Servidor
	{
        public static List<Cliente> Clientes;
        public static RestServer Server { get; set; }

		public static void Main(string[] args)
		{
            Clientes = new List<Cliente>();

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
