using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
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
        private Cursor OpenHand = FromByteArray(Properties.Resources.OpenHandcursor);
        private Cursor CloseHand = FromByteArray(Properties.Resources.CloseHandCursor);

        public static Cursor FromByteArray(byte[] array)
        {
            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream(array))
            {
                return new Cursor(memoryStream);
            }
        }
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

                    // If path is not a loop
                    // else, path is a loop
                    if (edge.Name == ec.Name && edge.Uid == ec.Height.ToString() && ec.V1 != ec.V2)
                    {
                        Point curvePoint = this.GetCurvePoint(ec.X1, ec.Y1, ec.X2, ec.Y2, ec.Height);
                        Path edgeCurve = this.DrawCurve(ec.X1, ec.Y1, curvePoint.X, curvePoint.Y, ec.X2, ec.Y2);
                        edge.Data = edgeCurve.Data;
                    }
                    else if(edge.Name == ec.Name && edge.Uid == ec.Size.ToString())
                    {
                        Path edgeLoop = this.DrawLoop(ec.X1, ec.Y1, ec.X1, ec.Y1, ec.Size);
                        edge.Data = edgeLoop.Data;
                    }
                }

            }
            UpdateStroke();

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
                Ellipse vertex = new Ellipse() { SnapsToDevicePixels = false, Fill = Brushes.Navy, Width = 30, Height = 30  };
                TextBlock vLabel = new TextBlock() { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                Point p = e.GetPosition(this.myCanvas);
                // Set position of vertex on canvas and vertex class
                Vertex vc = myGraph.CreateVertex(p.X - (vertex.Width / 2), p.Y - (vertex.Height / 2), vertex.Width, vertex.Height);
                Canvas.SetLeft(vertex, vc.Vx);
                Canvas.SetTop(vertex, vc.Vy);
                Canvas.SetZIndex(vertex, 1);
                vertex.Name = vc.Point;
                vertex.Uid = vc.Point;

                // Subscribe vertex to events
                vertex.PreviewMouseDown += Vertex_PreviewMouseDown;
                vertex.PreviewMouseRightButtonDown += Vertex_PreviewMouseRightButtonDown;
                vertex.PreviewMouseLeftButtonUp += Vertex_PreviewMouseLeftButtonUp;
                vertex.MouseEnter += Vertex_MouseEnter;
                vertex.MouseLeave += Vertex_MouseLeave;
                vertex.PreviewKeyDown += Vertex_PreviewKeyDown;

                // Label Initial Position
                vLabel.Name = vc.Point;
                vLabel.Foreground = Brushes.Red;
                labelList.Add(vLabel);
                Canvas.SetLeft(vLabel, vc.Vx + (vertex.Width / 3));
                Canvas.SetTop(vLabel, vc.Vy + (vertex.Height / 5));
                Canvas.SetZIndex(vLabel, 0);

                // Add vertex and label to graph class
                myCanvas.Children.Add(vertex);
                myCanvas.Children.Add(vLabel);
                UpdateLabelText();


                // Edit textbox values
                this.vectorTextBox.Text = this.GenerateVectorText();

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
                myCanvas.Cursor = CloseHand;
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
                UpdateStroke();

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
                TextBlock label = labelList.Find(l => l.Name == v.Point);

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
                Line edge = clickedObject as Line;
                Edge ec = myGraph.CreateEdge(parentVertex.Name, vertex.Name, edge.X1, edge.Y1, edge.X2, edge.Y2);
                TextBlock label = labelList.Find(l => l.Name == vertex.Name);
                UpdateLabelText();

                double x1 = Canvas.GetLeft(parentVertex) + (parentVertex.Width / 2);
                double y1 = Canvas.GetTop(parentVertex) + (parentVertex.Width / 2);
                double x2 = Canvas.GetLeft(vertex) + (vertex.Width / 2);
                double y2 = Canvas.GetTop(vertex) + (vertex.Width / 2);
                string edgeName = ec.Name;
                int numberOfEdges = myGraph.GetNumberofEdgesWithSameName(parentVertex.Name, vertex.Name);

                // If we are drawing an edge or adjacent edge
                // else draw a loop if on same vertex
                if (vertex != parentVertex)
                {


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
                            UpdateStroke();

                            edgeCurve.MouseEnter += EdgeCurve_MouseEnter;
                            edgeCurve.MouseLeave += EdgeCurve_MouseLeave;
                            edgeCurve.PreviewMouseRightButtonDown += EdgeCurve_PreviewMouseRightButtonDown;
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
                            edgeCurve.MouseEnter += EdgeCurve_MouseEnter;
                            edgeCurve.MouseLeave += EdgeCurve_MouseLeave;
                            UpdateStroke();

                        }
                    }
                    else
                    {
                        UpdateStroke();
                        edge.MouseEnter += Edge_MouseEnter;
                        edge.MouseLeave += Edge_MouseLeave;
                        edge.X2 = x2;
                        edge.Y2 = y2;
                        edge.Name = edgeName;
                        // Create edge object only after the fact that it is connected to another vertex
                    }
                }
                else
                {
                    int size = (numberOfEdges * 8) + 5;

                    Path edgeLoop = DrawLoop(x1, y1, x2, y2, size);
                    ec.Size = size;
                    edgeLoop.Name = edgeName;
                    edgeLoop.Uid = size.ToString();
                    myCanvas.Children.Remove(edge);
                    myCanvas.Children.Add(edgeLoop);
                    UpdateStroke();

                    edgeLoop.PreviewMouseRightButtonDown += EdgeCurve_PreviewMouseRightButtonDown;
                    edgeLoop.MouseEnter += EdgeLoop_MouseEnter;
                    edgeLoop.MouseLeave += EdgeLoop_MouseLeave;
                }
            }

            // Update Text box
            this.edgeTextBox.Text = this.GenerateEdgeText();
            clickedObject = null;
            clickedObjectC = null;
        }

        private void EdgeCurve_MouseLeave(object sender, MouseEventArgs e)
        {
            Path edge = sender as Path;
            UpdateStroke();
            myCanvas.Cursor = Cursors.Arrow;
        }

        private void EdgeCurve_MouseEnter(object sender, MouseEventArgs e)
        {
            Path edge = sender as Path;
            edge.Stroke = Brushes.Black;
            myCanvas.Cursor = Cursors.Hand;
        }

        private void EdgeLoop_MouseEnter(object sender, MouseEventArgs e)
        {
            Path edge = sender as Path;
            edge.Stroke = Brushes.Black;
            myCanvas.Cursor = Cursors.Hand;
        }


        private void EdgeLoop_MouseLeave(object sender, MouseEventArgs e)
        {
            Path edge = sender as Path;
            UpdateStroke();
            myCanvas.Cursor = Cursors.Arrow;
        }

   
        private void Edge_MouseEnter(object sender, MouseEventArgs e)
        {
            Line edge = sender as Line;
            edge.Stroke = Brushes.Black;
            myCanvas.Cursor = Cursors.Hand;

        }
        private void Edge_MouseLeave(object sender, MouseEventArgs e)
        {
            Line edge = sender as Line;
            UpdateStroke();
            myCanvas.Cursor = Cursors.Arrow;
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
                clickedObjectC = null;
            }
            else if (e.MiddleButton == MouseButtonState.Released && clickedObject is Ellipse)
            {
                myCanvas.Cursor = Cursors.Hand;
                clickedObject = null;
                clickedObjectC = null;
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
            TextBlock vLabel = labelList.Find(l => l.Name == vertex.Name);
            Vertex vc = myGraph.GetVertex(vertex.Name);
            myCanvas.Cursor = Cursors.Hand;

            vLabel.Foreground = Brushes.Red;
            vertex.Focusable = true;
            vertex.StrokeThickness = 1.7;
            vertex.Stroke = Brushes.Black;
            vertex.Focus();

            Canvas.SetZIndex(vLabel, 1);
            Canvas.SetZIndex(vertex, 2);
        }

        /// <summary>
        /// End mouse hovering over vertex effect
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Vertex_MouseLeave(object sender, MouseEventArgs e)
        {
            Ellipse vertex = sender as Ellipse;
            TextBlock vLabel = labelList.Find(l => l.Name == vertex.Name);
            Vertex vc = myGraph.GetVertex(vertex.Name);
            myCanvas.Cursor = Cursors.Arrow;

            vLabel.Foreground = Brushes.OldLace;

            vertex.Focusable = false;
            vertex.Opacity = 1;
            vertex.StrokeThickness = 0;
            Canvas.SetZIndex(vLabel, 2);
            Canvas.SetZIndex(vertex, 1);
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
        /// Draws loop if vertex parent is same as current vertex
        /// </summary>
        private Path DrawLoop(double x1, double y1, double x2, double y2, int size)
        {
            Path path = new Path() { Stroke = Brushes.DimGray, StrokeThickness = 3 };
            PathFigure figure = new PathFigure() { StartPoint = new Point(x1, y1) };
            ArcSegment arc = new ArcSegment() { Point = new Point(x2, y2 + 1), IsLargeArc = true, Size = new Size(size, size) };
            PathGeometry geometry = new PathGeometry();


            figure.Segments.Add(arc);
            geometry.Figures.Add(figure);
            path.Data = geometry;
            UpdateStroke();


            return path;
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
            Path path = new Path() { Stroke = Brushes.DimGray, StrokeThickness = 4 };
            QuadraticBezierSegment bezier = new QuadraticBezierSegment() { Point1 = new Point(ctrlx, ctrly), Point2 = new Point(x2, y2) };
            PathFigure figure = new PathFigure() { StartPoint = new Point(x1, y1) };
            PathGeometry geometry = new PathGeometry();

            UpdateStroke();
            figure.Segments.Add(bezier);
            geometry.Figures.Add(figure);
            path.Data = geometry;

            return path;
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
            List<TextBlock> labelIttemstoRemove = new List<TextBlock>();
            List<Edge> deletionList = myGraph.RemoveEdgesConnectedTo(vertex.Name);

            foreach (UIElement child in myCanvas.Children)
            {
                if (child is TextBlock)
                {
                    labelIttemstoRemove.Add(child as TextBlock);
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
            foreach (TextBlock label in labelIttemstoRemove)
            {
                myCanvas.Children.Remove(label);
            }

            // unsubscribe vertex to events
            vertex.PreviewMouseDown -= Vertex_PreviewMouseDown;
            vertex.PreviewMouseRightButtonDown -= Vertex_PreviewMouseRightButtonDown;
            vertex.PreviewMouseLeftButtonUp -= Vertex_PreviewMouseLeftButtonUp;
            vertex.MouseEnter -= Vertex_MouseEnter;
            vertex.MouseLeave -= Vertex_MouseLeave;
            vertex.PreviewKeyDown -= Vertex_PreviewKeyDown;
            myGraph.RemoveVertex(vertex.Name);
            myCanvas.Children.Remove(vertex);
            UpdateLabels();
            UpdateEllipses();
            UpdateLines();
            UpdateLabelText();
            UpdateStroke();
            myCanvas.Cursor = Cursors.Arrow;

            // Edit textbox values
            this.vectorTextBox.Text = this.GenerateVectorText();
            this.edgeTextBox.Text = this.GenerateEdgeText();

        }

        private void UpdateLines()
        {
            int count = 0;
            // update each ellipse name
            foreach (UIElement child in myCanvas.Children)
            {
                if (child is Line)
                {
                    Line edge = child as Line;
                    edge.Name = myGraph.edgeList[count].Name;
                    count++;
                }
                else if (child is Path)
                {
                    Path edge = child as Path;
                    edge.Name = myGraph.edgeList[count].Name;
                    count++;
                }    
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdateLabels()
        {
            labelList.Clear();

            foreach(Vertex vc in myGraph.vertexList)
            {
                TextBlock vLabel = new TextBlock();
                vLabel.Name = vc.Point;
                UpdateLabelText();
                vLabel.Foreground = Brushes.OldLace;
                
                Canvas.SetLeft(vLabel, vc.Vx + (vc.Width / 3));
                Canvas.SetTop(vLabel, vc.Vy + (vc.Height / 5));
                Canvas.SetZIndex(vLabel, 1);

                labelList.Add(vLabel);
                myCanvas.Children.Add(vLabel);
            }
        }
        private void UpdateEllipses()
        {
            int count = 0;
            // update each ellipse name
            foreach (UIElement child in myCanvas.Children)
            {
                if (child is Ellipse)
                {
                    Ellipse vertex = child as Ellipse;
                    vertex.Name = myGraph.vertexList[count].Point;
                    count++;
                }
            }
        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            List<UIElement> deleteList = new List<UIElement>();
            foreach (UIElement child in myCanvas.Children)
            {
                // Delete all except for button
                if (!(child is Button))
                {
                    deleteList.Add(child);
                }
            }
            foreach (UIElement item in deleteList)
            {
                myCanvas.Children.Remove(item);
            }
            myGraph.edgeList.Clear();
            myGraph.vertexList.Clear();
            labelList.Clear();
            clickedObject = null;
            clickedObjectC = null;

            // Edit textbox values
            this.vectorTextBox.Text = this.GenerateVectorText();
            this.edgeTextBox.Text = this.GenerateEdgeText();
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

            // Edit textbox values
            UpdateLabels();
            UpdateLabelText();

            this.vectorTextBox.Text = this.GenerateVectorText();
            this.edgeTextBox.Text = this.GenerateEdgeText();
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

            UpdateLabels();
            UpdateLabelText();
            // Edit textbox values
            this.vectorTextBox.Text = this.GenerateVectorText();
            this.edgeTextBox.Text = this.GenerateEdgeText();
        }

        private string GenerateVectorText()
        {
            string strBuild = "{ ";
            foreach (Vertex v in myGraph.vertexList)
            {
                strBuild += v.Point + ", ";
            }
            strBuild.Remove(strBuild.Length - 1);
            strBuild.Remove(strBuild.Length - 2);
            strBuild += "}";
            return strBuild;
        }

        private string GenerateEdgeText()
        {
            string strBuild = "{ ";
            foreach (Edge e in myGraph.edgeList)
            {
                strBuild += e.Name + ", ";
            }
            strBuild.Remove(strBuild.Length - 1);
            strBuild.Remove(strBuild.Length - 2);
            strBuild += "}";
            return strBuild;

        }

        /// <summary>
        /// Toggle Directed Graph
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toggleDirected_Click(object sender, RoutedEventArgs e)
        {
            if (toggleDirected.Content.ToString() == "Toggle Directed")
            {
                toggleDirected.Content = "Toggle Normal";
                this.UpdateStroke();

            }
            else
            {
                toggleDirected.Content = "Toggle Directed";
                this.UpdateStroke();
            }
        }

        /// <summary>
        /// Update the stroke of each edge
        /// </summary>
        private void UpdateStroke()
        {
            if (toggleDirected.Content.ToString() == "Toggle Directed")
            {
                foreach (UIElement child in myCanvas.Children)
                {
                    if (child is Line)
                    {
                        Line edge = child as Line;
                        edge.Stroke = Brushes.DimGray;
                    }
                    else if (child is Path)
                    {
                        Path edge = child as Path;
                        edge.Stroke = Brushes.DimGray;
                    }
                }
            }
            else
            {
                foreach (UIElement child in myCanvas.Children)
                {
                    if (child is Line)
                    {
                        Line edge = child as Line;
                        if (edge.Name != "")
                        {
                            Edge ec = myGraph.edgeList.Find(e => e.Name == edge.Name);
                            LinearGradientBrush myLinearGradientBrush = new LinearGradientBrush();
                            myLinearGradientBrush.MappingMode = BrushMappingMode.Absolute;
                            myLinearGradientBrush.StartPoint = new Point(ec.X1, ec.Y1);
                            myLinearGradientBrush.EndPoint = new Point(ec.X2, ec.Y2);
                            myLinearGradientBrush.GradientStops.Add(new GradientStop(Colors.DimGray, .5));
                            myLinearGradientBrush.GradientStops.Add(new GradientStop(Colors.Red, 1.0));
                            edge.Stroke = myLinearGradientBrush;
                        }
                       
                    }
                    else if (child is Path)
                    {
                        Path edge = child as Path;
                        if (edge.Name != "")
                        {
                            Edge ec = myGraph.edgeList.Find(e => e.Name == edge.Name);
                            LinearGradientBrush myLinearGradientBrush = new LinearGradientBrush();
                            myLinearGradientBrush.MappingMode = BrushMappingMode.Absolute;
                            myLinearGradientBrush.StartPoint = new Point(ec.X1, ec.Y1);
                            myLinearGradientBrush.EndPoint = new Point(ec.X2, ec.Y2);
                            myLinearGradientBrush.GradientStops.Add(new GradientStop(Colors.DimGray, 0.5));
                            myLinearGradientBrush.GradientStops.Add(new GradientStop(Colors.Red, 1.0));
                            edge.Stroke = myLinearGradientBrush;
                        }
                    }
                }
            }
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toggleMode_Click(object sender, RoutedEventArgs e)
        {
            Button tm = sender as Button;

            // if the string is toggle degree then change all vertices labels
            if (toggleMode.Content.ToString() == "Toggle Degree")
            {
                foreach (UIElement child in myCanvas.Children)
                {
                    if (child is TextBlock)
                    {
                        TextBlock label = child as TextBlock;
                        label.Text = myGraph.GetVertexDegree(label.Name).ToString();
                    }
                }
                tm.Content = "Toggle Labels";
            }
            else
            {
                foreach (UIElement child in myCanvas.Children)
                {
                    if (child is TextBlock)
                    {
                        TextBlock label = child as TextBlock;
                        label.Text = label.Name;
                    }
                }
                tm.Content = "Toggle Degree";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdateLabelText()
        {
            if (toggleMode.Content.ToString() == "Toggle Degree")
            {
                foreach (UIElement child in myCanvas.Children)
                {
                    if (child is TextBlock)
                    {
                        TextBlock label = child as TextBlock;
                        label.Text = label.Name;
                    }
                }
            }
            else
            {
                foreach (UIElement child in myCanvas.Children)
                {
                    if (child is TextBlock)
                    {
                        TextBlock label = child as TextBlock;
                        label.Text = myGraph.GetVertexDegree(label.Name).ToString();
                    }
                }
                
            }
        }
    }
}
