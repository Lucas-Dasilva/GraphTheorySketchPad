using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace GraphTheorySketchPad
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Graph myGraph = new Graph();
        private Point dragStartPoint, dragEndPoint, objectStartLocation;
        private Ellipse parentVertex;
        private object clickedObject;
        private object clickedObjectC;
        private List<TextBlock> labelList = new List<TextBlock>();

        public MainWindow()
        {
            InitializeComponent();
            this.myGraph.PropertyChanged += MyGraph_PropertyChanged;
        }



        /// <summary>
        /// When a vertex is moved, update the edge location that corresponds to that vertex
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MyGraph_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Edge ec = sender as Edge;
            foreach (UIElement child in myCanvas.Children)
            {
                if (child is Line)
                {
                    Line edge = child as Line;
                    if (edge.Name == ec.Name)
                    {
                        // Check which edge point is changed
                        if (e.PropertyName == "x1y1")
                        {
                            edge.X1 = ec.X1;
                            edge.Y1 = ec.Y1;
                        }
                        else if (e.PropertyName == "x2y2")
                        {
                            edge.X2 = ec.X2;
                            edge.Y2 = ec.Y2;
                        }
                    }
                }
                else if (child is Path)
                {
                    Path edge = child as Path;
                    //{ 145,203.5,203.5,39.45965576171875}
                    //{ M145,241Q252.18671788622567,251.7532557292512,348.5,203.5}
                    if (edge.Name == ec.Name && edge.Uid == ec.Height.ToString())
                    {
                        Point curvePoint = this.GetCurvePoint(ec.X1, ec.Y1, ec.X2, ec.Y2, ec.Height);
                        Path edgeCurve = this.DrawCurve(ec.X1, ec.Y1, curvePoint.X, curvePoint.Y, ec.X2, ec.Y2);
                        edge.Data = edgeCurve.Data;
                    }
                }

            }
        }


        /// <summary>
        /// Upon left clicking canvas, create a node
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void myCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // if clicked on blank canvas, created node object
            if (clickedObject is null)
            {
                // Create ellipse on vancas and get mouse coordinates
                Ellipse vertex = new Ellipse() { Fill = Brushes.Navy, Width = 30, Height = 30  };
                TextBlock vLabel = new TextBlock();
                Point p = e.GetPosition(this.myCanvas);

                // Set position of vertex on canvas and vertex class
                Vertex vc = myGraph.CreateVertex(p.X - (vertex.Width / 2), p.Y - (vertex.Height / 2), vertex.Width, vertex.Height);
                Canvas.SetLeft(vertex, vc.Vx);
                Canvas.SetTop(vertex, vc.Vy);
                Canvas.SetZIndex(vertex, 1);
                vertex.Name = vc.Point;

                // Subscribe vertex to events
                vertex.PreviewMouseDown += Vertex_PreviewMouseDown;
                vertex.PreviewMouseRightButtonDown += Vertex_PreviewMouseRightButtonDown;
                vertex.PreviewMouseLeftButtonUp += Vertex_PreviewMouseLeftButtonUp;
                vertex.MouseEnter += Vertex_MouseEnter;
                vertex.MouseLeave += Vertex_MouseLeave;
                vertex.PreviewKeyDown += Vertex_PreviewKeyDown;

                // Label Initial Position
                vLabel.Text = vertex.Name;
                vLabel.Foreground = Brushes.Red;
                labelList.Add(vLabel);
                Canvas.SetLeft(vLabel, vc.Vx + (vertex.Width / 3));
                Canvas.SetTop(vLabel, vc.Vy + (vertex.Height / 5));
                Canvas.SetZIndex(vLabel, 0);

                // Add vertex and label to graph class
                myCanvas.Children.Add(vertex);
                myCanvas.Children.Add(vLabel);

            }
        }


        /// <summary>
        /// Drag node around canvas or add edge start point
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Vertex_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                // Get the object thats clicked and convert it to an ellipse
                Ellipse vertex = sender as Ellipse;
                Vertex v = myGraph.GetVertex(vertex.Name);

                // Save initial position
                dragStartPoint.X = e.GetPosition(this).X;
                dragStartPoint.Y = e.GetPosition(this).Y;

                // Get object start location
                objectStartLocation.X = Canvas.GetLeft(vertex);
                objectStartLocation.Y = Canvas.GetTop(vertex);

                clickedObject = vertex as Ellipse;
                clickedObjectC = v as Vertex;
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                Line edge = new Line() { Stroke = Brushes.Black, StrokeThickness = 4 };
                Canvas.SetZIndex(edge, 0);
                Point p = e.GetPosition(this.myCanvas);
                Ellipse vertex = sender as Ellipse;

                this.parentVertex = vertex;
                edge.X1 = Canvas.GetLeft(vertex) + (vertex.Width / 2);
                edge.Y1 = Canvas.GetTop(vertex) + (vertex.Height / 2);
                edge.X2 = p.X;
                edge.Y2 = p.Y;
                edge.PreviewMouseRightButtonDown += Edge_PreviewMouseRightButtonDown;

                // Add edge to canvas
                myCanvas.Children.Add(edge);
                clickedObject = edge as Line;
            }

        }


        /// <summary>
        /// Move node around
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void myCanvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (clickedObject is null)
            {
                return;
            }

            if (clickedObject is Ellipse)
            {
                Ellipse vertex = clickedObject as Ellipse;
                Vertex v = clickedObjectC as Vertex;
                TextBlock label = labelList.Find(l => l.Text == v.Point);

                // Save initial position
                dragEndPoint.X = e.GetPosition(this).X;
                dragEndPoint.Y = e.GetPosition(this).Y;

                // Get the change in distance
                double deltaX = dragEndPoint.X - dragStartPoint.X;
                double deltaY = dragEndPoint.Y - dragStartPoint.Y;

                // Add the difference
                v.Vx = objectStartLocation.X + deltaX;
                v.Vy = objectStartLocation.Y + deltaY;
                Canvas.SetLeft(vertex, v.Vx);
                Canvas.SetTop(vertex, v.Vy);
                Canvas.SetLeft(label, v.Vx + (v.Width / 3));
                Canvas.SetTop(label, v.Vy + (v.Height / 4));
            }

            else if (clickedObject is Line)
            {
                Line edge = clickedObject as Line;

                edge.X2 = e.GetPosition(this.myCanvas).X;
                edge.Y2 = e.GetPosition(this.myCanvas).Y;
            }

        }

        /// <summary>
        /// When either left clicking or middle mouse button clicking, move vertex or create edge
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Vertex_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (clickedObject is Line)
            {
                Ellipse vertex = sender as Ellipse;
                double x1 = Canvas.GetLeft(parentVertex) + (parentVertex.Width / 2);
                double y1 = Canvas.GetTop(parentVertex) + (parentVertex.Width / 2);
                double x2 = Canvas.GetLeft(vertex) + (vertex.Width / 2);
                double y2 = Canvas.GetTop(vertex) + (vertex.Width / 2);
    
                if (vertex != parentVertex)
                {
                    Line edge = clickedObject as Line;
                    Edge ec = myGraph.CreateEdge(parentVertex.Name, vertex.Name, edge.X1, edge.Y1, edge.X2, edge.Y2);
                    string edgeName = ec.Name;
                    int numberOfEdges = myGraph.GetNumberofEdgesWithSameName(parentVertex.Name, vertex.Name);

                    // If this edge is a parallel line
                    // else it's a regular line
                    if ( numberOfEdges > 1)
                    {
                        int height = 30 * (numberOfEdges / 2);
                        Point curvePoint = this.GetCurvePoint(x1, y1, x2, y2, height);

                        // Check if CurvePointX or CurvePointY already exists
                        // determines what side the curve is drawn on
                        if (!myGraph.CheckExistanceOfCurve(curvePoint.X, curvePoint.Y))
                        {
                            // find mid point of line
                            Path edgeCurve = this.DrawCurve(x1, y1, curvePoint.X, curvePoint.Y, x2, y2);
                            Canvas.SetZIndex(edgeCurve, 0);
                            ec.Height = height;
                            ec.EdgeCurveX = curvePoint.X;
                            ec.EdgeCurveY = curvePoint.Y;
                            edgeCurve.Name = edgeName;
                            edgeCurve.Uid = height.ToString();
                            myCanvas.Children.Remove(edge);
                            myCanvas.Children.Add(edgeCurve);
                            edgeCurve.PreviewMouseRightButtonDown += EdgeCurve_PreviewMouseRightButtonDown;
                            clickedObject = null;
                        }
                        else
                        {
                            height = -height;
                            // find mid point of line
                            curvePoint = this.GetCurvePoint(x1, y1, x2, y2, height);
                            Path edgeCurve = this.DrawCurve(x1, y1, curvePoint.X, curvePoint.Y, x2, y2);
                            myCanvas.Children.Remove(edge);
                            Canvas.SetZIndex(edgeCurve, 0);
                            ec.Height = height;
                            ec.EdgeCurveX = curvePoint.X;
                            ec.EdgeCurveY = curvePoint.Y;
                            edgeCurve.Name = edgeName;
                            edgeCurve.Uid = height.ToString();
                            edgeCurve.PreviewMouseRightButtonDown += EdgeCurve_PreviewMouseRightButtonDown;
                            myCanvas.Children.Add(edgeCurve);
                            clickedObject = null;
                        }
                    }
                    else
                    {
                        edge.X2 = x2;
                        edge.Y2 = y2;
                        edge.Name = edgeName;
                        // Create edge object only after the fact that it is connected to another vertex
                        clickedObject = null;
                    }
                }
            }
        }




        /// <summary>
        /// Release the node at the position
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void myCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // If edge is not connected to a vertex, remove it
            // Else vertex is finished moving around
            if (e.LeftButton == MouseButtonState.Released && clickedObject is Line)
            {
                Line edge = clickedObject as Line;
                myCanvas.Children.Remove(edge);
                clickedObject = null;
            }
            else if (e.MiddleButton == MouseButtonState.Released && clickedObject is Ellipse)
            {
                clickedObject = null;
            }
        }


        /// <summary>
        /// Add hover effect
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Vertex_MouseEnter(object sender, MouseEventArgs e)
        {
            Ellipse vertex = sender as Ellipse; 
            TextBlock vLabel = labelList.Find(l => l.Text == vertex.Name);
            Vertex vc = myGraph.GetVertex(vertex.Name);
    
            Canvas.SetZIndex(vLabel, 0);
            vLabel.Foreground = Brushes.Black;
            vertex.Focusable = true;
            vertex.Width = vc.Width + 5;
            vertex.Height = vc.Height + 5;
            vertex.Opacity = .75;
            vertex.StrokeThickness = 1;
            vertex.Stroke = Brushes.DarkGray;
            vertex.Focus();
            Canvas.SetLeft(vLabel, Canvas.GetLeft(vertex) + (vertex.Width / 3));
            Canvas.SetTop(vLabel, Canvas.GetTop(vertex) + (vertex.Height / 4));


        }

        /// <summary>
        /// End mouse hovering over vertex effect
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Vertex_MouseLeave(object sender, MouseEventArgs e)
        {
            Ellipse vertex = sender as Ellipse;
            TextBlock vLabel = labelList.Find(l => l.Text == vertex.Name);
            Vertex vc = myGraph.GetVertex(vertex.Name);
      
            vLabel.Foreground = Brushes.OldLace;

            Canvas.SetZIndex(vLabel, 2);
            vertex.Focusable = false;
            vertex.Opacity = 1;
            vertex.Width = vc.Width;
            vertex.Height = vc.Height;
            vertex.StrokeThickness = 0;
            vertex.Stroke = Brushes.Gray;
            Canvas.SetLeft(vLabel, vc.Vx + (vertex.Width / 3));
            Canvas.SetTop(vLabel, vc.Vy + (vertex.Height / 5));
        }

        private void Vertex_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Ellipse vertex = sender as Ellipse;
            if (e.Key == Key.R)
            {
                vertex.Fill = Brushes.Crimson;
            }
            else if (Keyboard.IsKeyDown(Key.B))
            {
                vertex.Fill = Brushes.Navy;
            }
            else if (Keyboard.IsKeyDown(Key.G))
            {
                vertex.Fill = Brushes.ForestGreen;
            }
            else if (Keyboard.IsKeyDown(Key.Y))
            {
                vertex.Fill = Brushes.Gold;
            }
            else if (Keyboard.IsKeyDown(Key.O))
            {
                vertex.Fill = Brushes.OrangeRed;
            }
        }



        /// <summary>
        /// Draws a curve from one point to another
        /// </summary>
        /// <param name="x1">Starting point x</param>
        /// <param name="y1">Starting point y</param>
        /// <param name="ctrlx">This should (X2 - X1)/ 2</param>
        /// <param name="ctrly">This defines how high the curve goes</param>
        /// <param name="x2">End point of line x</param>
        /// <param name="y2">End point of line y</param>
        public Path DrawCurve(double x1, double y1, double ctrlx, double ctrly, double x2, double y2)
        {
            Path path = new Path() { Stroke = Brushes.Black, StrokeThickness = 4 };

            QuadraticBezierSegment bezier = new QuadraticBezierSegment() { Point1 = new Point(ctrlx, ctrly), Point2 = new Point(x2, y2) };

            PathFigure figure = new PathFigure() { StartPoint = new Point(x1, y1) };
            figure.Segments.Add(bezier);

            PathGeometry geometry = new PathGeometry();
            geometry.Figures.Add(figure);

            path.Data = geometry;

            return path;
            //Debug.WriteLine("Curve Start Point: ( " + x1 + ", " + y1 + ")");
            //Debug.WriteLine("Curve End Point: ( " + x2 + ", " + y2 + ")");
        }

        public Point GetCurvePoint(double x1, double y1, double x2, double y2, double height)
        {
            Point A = new Point(y1 - y2, x2 - x1);
            Point midPoint = new Point((x1 + x2) / 2, (y1 + y2) / 2);
            // C = B / |B|
            double magB = Math.Sqrt((A.X * A.X) + (A.Y * A.Y));
            Point C = new Point(A.X / magB, A.Y / magB);
            Point perpendicularPoint = new Point(midPoint.X + (height * C.X), midPoint.Y + (height * C.Y));

            return perpendicularPoint;
        }

        /// <summary>
        /// Remove node on right click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Vertex_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Ellipse vertex = sender as Ellipse;
            List<Line> lineItemstoremove = new List<Line>();
            List<Path> pathItemstoremove = new List<Path>();
            List<Edge> deletionList = myGraph.RemoveEdgesConnectedTo(vertex.Name);

            foreach (UIElement child in myCanvas.Children)
            {
                if (child is TextBlock)
                {

                }
                foreach (Edge ec in deletionList)
                {
                    if (child is Line)
                    {
                        Line edge = child as Line;
                        if (edge.Name == ec.Name)
                        {
                            lineItemstoremove.Add(edge);
                        }
                    }
                    else if (child is Path)
                    {
                        Path edge = child as Path;
                        if (edge.Name == ec.Name)
                        {
                            pathItemstoremove.Add(edge);
                        }
                    }
                }
            }
            foreach (Line edge in lineItemstoremove)
            {
                myCanvas.Children.Remove(edge);
            }
            foreach (Path edge in pathItemstoremove)
            {
                myCanvas.Children.Remove(edge);
            }
            TextBlock label = labelList.Find(l => l.Text == vertex.Name);

            // unsubscribe vertex to events
            vertex.PreviewMouseDown -= Vertex_PreviewMouseDown;
            vertex.PreviewMouseRightButtonDown -= Vertex_PreviewMouseRightButtonDown;
            vertex.PreviewMouseLeftButtonUp -= Vertex_PreviewMouseLeftButtonUp;
            vertex.MouseEnter -= Vertex_MouseEnter;
            vertex.MouseLeave -= Vertex_MouseLeave;
            vertex.PreviewKeyDown -= Vertex_PreviewKeyDown;
            myGraph.RemoveVertex(vertex.Name);
            myCanvas.Children.Remove(label);
            labelList.Remove(label);
            myCanvas.Children.Remove(vertex);
        }

        /// <summary>
        /// Remove edge curve on right click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EdgeCurve_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Path edge = sender as Path;
            myGraph.RemoveEdge(edge.Name);
            myCanvas.Children.Remove(edge);

        }

        /// <summary>
        /// Remove edge on right click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Edge_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Line edge = sender as Line;
     
            myGraph.RemoveEdge(edge.Name);
            myCanvas.Children.Remove(edge);

        }

    }
}
