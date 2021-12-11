using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GraphTheorySketchPad
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Graph myGraph;
        private Point dragStartPoint, dragEndPoint, objectStartLocation;
        private Ellipse parentVertex;
        private object clickedObject;
        private Canvas canvas = new Canvas();
        
        public MainWindow()
        {
            InitializeComponent();
            this.myGraph = new Graph();
            //this.canvas.DataContext 
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
                Ellipse vertex = new Ellipse() { Fill = Brushes.Navy, Width = 20, Height = 20  };
                Point p = e.GetPosition(this.myCanvas);
                var point = new TextBlock();

                Canvas.SetLeft(vertex, p.X - (vertex.Width / 2));
                Canvas.SetTop(vertex, p.Y - (vertex.Height / 2));
                vertex.PreviewMouseDown += Vertex_PreviewMouseDown;
                vertex.PreviewMouseRightButtonDown += Vertex_PreviewMouseRightButtonDown;
                vertex.PreviewMouseLeftButtonUp += Vertex_PreviewMouseLeftButtonUp;
                vertex.MouseEnter += Vertex_MouseEnter;
                vertex.MouseLeave += Vertex_MouseLeave;
                Canvas.SetZIndex(vertex, 1);
                // Add vertex to graph class
                myCanvas.Children.Add(vertex);
                vertex.Uid = myGraph.CreateVertex().Point;
            }
        }

        private void Vertex_MouseLeave(object sender, MouseEventArgs e)
        {
            Ellipse vertex = sender as Ellipse;
            vertex.Opacity = 1;
            vertex.Width = 20;
            vertex.Height = 20;
            vertex.StrokeThickness = 0;
            vertex.Stroke = Brushes.Gray;
        }

        /// <summary>
        /// Add hover effect
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Vertex_MouseEnter(object sender, MouseEventArgs e)
        {
            Ellipse vertex = sender as Ellipse;
            vertex.Width = 21;
            vertex.Height = 21;
            vertex.Opacity = .75;
            vertex.StrokeThickness = 1;
            vertex.Stroke = Brushes.DarkGray;
        }

        private void Vertex_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (clickedObject is Line)
            {
                Ellipse vertex = sender as Ellipse;
                if (vertex != parentVertex)
                {
                    Line edge = clickedObject as Line;
                    edge.X2 = Canvas.GetLeft(vertex) + (vertex.Width / 2);
                    edge.Y2 = Canvas.GetTop(vertex) + (vertex.Height / 2);
                    clickedObject = null;
                }
            }
        }

        /// <summary>
        /// Drag node around canvas and create edge start point
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Vertex_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                // Get the object thats clicked and convert it to an ellipse
                Ellipse vertex = sender as Ellipse;

                // Save initial position
                dragStartPoint.X = e.GetPosition(this).X;
                dragStartPoint.Y = e.GetPosition(this).Y;

                objectStartLocation.X = Canvas.GetLeft(vertex);
                objectStartLocation.Y = Canvas.GetTop(vertex);

                clickedObject = vertex as Ellipse;
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                Line edge = new Line() { Stroke = Brushes.Black, StrokeThickness = 4 };
                Point p = e.GetPosition(this.myCanvas);
                Ellipse vertex = sender as Ellipse;
                this.parentVertex = vertex;

                edge.X1 = Canvas.GetLeft(vertex) + (vertex.Width / 2);
                edge.Y1 = Canvas.GetTop(vertex) + (vertex.Height / 2);
                edge.X2 = p.X;
                edge.Y2 = p.Y;

                Canvas.SetZIndex(edge, 0);
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

                // Save initial position
                dragEndPoint.X = e.GetPosition(this).X;
                dragEndPoint.Y = e.GetPosition(this).Y;

                // Get the change in distance
                double deltaX = dragEndPoint.X - dragStartPoint.X;
                double deltaY = dragEndPoint.Y - dragStartPoint.Y;

                // Add the difference
                Canvas.SetLeft(vertex, objectStartLocation.X + deltaX);
                Canvas.SetTop(vertex, deltaY + objectStartLocation.Y);
            }

            else if (clickedObject is Line)
            {
                Line edge = clickedObject as Line;

                edge.X2 = e.GetPosition(this.myCanvas).X;
                edge.Y2 = e.GetPosition(this.myCanvas).Y;
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
        /// Remove node on right click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Vertex_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Ellipse vertex = sender as Ellipse;
            myCanvas.Children.Remove(vertex);
            vertex = null;
        }
    }
}
