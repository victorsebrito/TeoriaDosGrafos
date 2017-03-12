using Newtonsoft.Json;
using System;
namespace TeoriaDosGrafos.Classes
{
	public class Vertice
	{
		public Vertice()
		{
		}

        public Vertice(int aoID, string aoNome)
        {
            this.ID = aoID;
            this.Nome = aoNome;
        }

        public int ID { get; set; }
        public string Nome { get; set; }
    }
}
