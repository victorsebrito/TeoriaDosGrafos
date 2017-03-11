using System;
namespace TeoriaDosGrafos
{
	public class Edge
	{
		public Edge()
		{
		}
        public int id { get; set; }
        public string Source { get; set; }
        public string Target { get; set; }
        public int Weight { get; set; }
    }
}
