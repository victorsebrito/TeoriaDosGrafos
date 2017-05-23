using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TeoriaDosGrafos.Classes.Util
{
    public class VerticeDistance
    {
        public Vertice Vertice;
        public int Distance;

        public VerticeDistance(Vertice aoVertice, int aiDistance)
        {
            Vertice = aoVertice;
            Distance = aiDistance;
        }
    }

}
