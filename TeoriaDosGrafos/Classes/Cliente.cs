using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TeoriaDosGrafos.Classes
{
    public class Cliente
    {
        public string ID { get; set; }
        public Grafo Grafo { get; set; }

        public Cliente()
        {
            ID = Guid.NewGuid().ToString();
            Grafo = new Grafo();
        }
    }
}
