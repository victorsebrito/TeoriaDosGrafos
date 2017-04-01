using System;
using Grapevine.Server;
using System.Threading;
using TeoriaDosGrafos.Classes;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;

namespace TeoriaDosGrafos 
{
    class Servidor
	{
        public static List<Cliente> Clientes;
        public static RestServer Server { get; set; }

		public static void Main(string[] args)
		{
            Clientes = new List<Cliente>();

            string loHost = string.IsNullOrEmpty(ConfigurationManager.AppSettings["Host"]) ? "localhost" : ConfigurationManager.AppSettings["Host"];
            string loPort = string.IsNullOrEmpty(ConfigurationManager.AppSettings["Port"]) ? "1234" : ConfigurationManager.AppSettings["Port"];

            Server = new RestServer(new ServerSettings() {
                Host = loHost,
                Port = loPort
            });
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
