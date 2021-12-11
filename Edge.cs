using System;
using System.Collections.Generic;
using System.Text;

namespace GraphTheorySketchPad
{

    /// <summary>
    /// Edge representation of graph
    /// </summary>
    public class Edge : Graph
    {
       

        public Edge(string v1, string v2)
        {
            Name = v1 + v2;
        }
        public string Name
        {
            get => this.Name;
            set => this.Name = value;
        }
        public double X1
        {
            get => this.X1;
            set => this.X1 = value;
        }
        public double Y1
        {
            get => this.Y1;
            set => this.Y1 = value;
        }
        public double X2
        {
            get => this.X2;
            set => this.X2 = value;
        }
        public double Y2
        {
            get => this.Y2;
            set => this.Y2 = value;
        }

    }
}
