using System;
using System.Collections.Generic;
using System.Text;

namespace GraphTheorySketchPad
{
    public class Graph
    {
        // Allows for listening to event, the middle man between trigger and listener
        public event EventHandler<string> VertexCreatedEvent;

        public int numberOfVertices;
        List<Vertex> vertices = new List<Vertex>();
        //List<Edge> edges;

        public Vertex CreateVertex()
        {
            Vertex v = new Vertex(vertices.Count);
            vertices.Add(v);
            numberOfVertices = vertices.Count;
            return v;
        }

        public void CreateEdge()
        {

        }




    }
}
