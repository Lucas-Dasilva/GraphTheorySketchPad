using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Shapes;

namespace GraphTheorySketchPad
{
    /// <summary>
    /// Vertex representation of graph
    /// </summary>
    public class Vertex : Graph
    {
        public string currentVertex;

        /// <summary>
        /// Create Vertex point
        /// </summary>
        /// <param name="num"></param>
        public Vertex(int num)
        {
            Point = "v" + num.ToString();
        }
        
        /// <summary>
        /// Gets or sets the vertex Uid
        /// </summary>
        public string Point
        {
            get => this.Point;
            set => this.Point = value;
        }
        public double X
        {
            get => this.X;
            set => this.X = value;
        }
        public double Y
        {
            get => this.X;
            set => this.X = value;
        }

    }
}
