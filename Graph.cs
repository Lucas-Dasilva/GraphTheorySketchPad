using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;


namespace GraphTheorySketchPad
{
    public class Graph : INotifyPropertyChanged
    {
        // Allows for listening to event, the middle man between trigger and listener

        public int numberOfVertices;
        List<Vertex> vertexList = new List<Vertex>();
        List<Edge> edgeList = new List<Edge>();

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Invokes the property changed event which alerts the form of which property of the edge is changed
        /// </summary>
        /// <param name="name">The name of property that's being changed</param>
        /// <param name="edge">The edge whose property is being changed</param>
        protected void EdgePropertyChanged(Edge edge, string name)
        {
            this.PropertyChanged?.Invoke(edge, new PropertyChangedEventArgs(name));
        }

        public Vertex CreateVertex(double vx, double vy, double width, double height)
        {
            Vertex v = new Vertex(vertexList.Count, vx, vy, width, height);

            vertexList.Add(v);
            numberOfVertices = vertexList.Count;
            v.PropertyChanged += new PropertyChangedEventHandler(this.VextexPropertyChanged);
            return v;
        }
        public Edge CreateEdge(string v1, string v2, double x1, double y1, double x2, double y2)
        {
            Edge e = new Edge(v1, v2, x1, y1, x2, y2);
            edgeList.Add(e);
            return e;
        }


        /// Create the OnPropertyChanged method to raise the event
        /// The calling member's name will be used as the parameter.
        /// </summary>
        /// <param name="sender">Sender Object</param>
        /// <param name="e">Event argument e</param>
        protected void VextexPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Vertex v = sender as Vertex;

            // if Vx or Vy is changed update edges connected to this vertex
            // else if the vertex is getting deleted
            if (e.PropertyName == "Vx" || e.PropertyName == "Vy")
            {
                Edge connectedEdge = FindEdge(v.Point);
                if (connectedEdge != null)
                {
                    foreach (Edge edge in edgeList)
                    {
                        // Find edge point in edge list that is connected to the vertex
                        if (edge.V1 == v.Point)
                        {
                            edge.X1 = v.Vx + (v.Width/2);
                            edge.Y1 = v.Vy + (v.Height/2);
                            this.EdgePropertyChanged(edge, "x1y1");
                        }
                        else if (edge.V2 == v.Point)
                        {
                            edge.X2 = v.Vx + (v.Width / 2);
                            edge.Y2 = v.Vy + (v.Height / 2);
                            this.EdgePropertyChanged(edge, "x2y2");
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Removes raise property changed for deleting all edges connected to a vertex
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<Edge> RemoveEdgesConnectedTo(string name)
        {
            List<Edge> deletionList = new List<Edge>();
            foreach (Edge e in edgeList.ToArray())
            {
                if (e.Name.Contains(name))
                {
                    deletionList.Add(e);
                    edgeList.Remove(e);
                }
            }
            return deletionList;
        }
        public Edge FindEdge(string uid)
        {
            foreach (Edge e in edgeList)
            {
                if (e.V1 == uid || e.V2 == uid)
                {
                    return e;
                }
            }
            return null;
        }
        public Vertex GetVertex(string name)
        {
            return vertexList.Find(v => v.Point == name);
        }

        /// <summary>
        /// Remove vertex from vertex list
        /// </summary>
        /// <param name="name"></param>
        public void RemoveVertex(string name)
        {
            foreach (Vertex v in vertexList.ToArray())
            {
                if (v.Point == name)
                {
                    vertexList.Remove(v);
                }
            }
        }

        /// <summary>
        /// Remove vertex from vertex list
        /// </summary>
        /// <param name="name"></param>
        public void RemoveEdge(string name)
        {
            foreach (Edge e in edgeList.ToArray())
            {
                if (e.Name == name)
                {
                    edgeList.Remove(e);
                }
            }
        }

        /// <summary>
        /// Get how many edges are connecting two particular nodes
        /// </summary>
        /// <param name="name">The name of the edge</param>
        /// <returns>Number of edges</returns>
        public int GetNumberofEdgesWithSameName(string v1, string v2)
        {
            int count = 0;

            string edgeName = v1 + v2;
            string revEdgeName = v2 + v1;
            foreach (Edge e in edgeList)
            {
                if (e.Name == edgeName || e.Name == revEdgeName)
                {
                    count += 1;
                }
            }
            return count;
        }

        public bool CheckExistanceOfCurve(double x, double y)
        {
            foreach (Edge e in edgeList)
            {
                if (e.EdgeCurveX == x && e.EdgeCurveY == y)
                {
                    return true;
                }
            }
            return false;
        }




    }
}
