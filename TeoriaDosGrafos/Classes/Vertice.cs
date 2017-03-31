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

        [JsonProperty(PropertyName = "id")]
        public int ID { get; set; }

        [JsonProperty(PropertyName = "nome")]
        public string Nome { get; set; }
    }
}
