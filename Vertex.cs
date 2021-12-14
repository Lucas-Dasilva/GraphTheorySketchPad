using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace GraphTheorySketchPad
{
    /// <summary>
    /// Vertex representation of graph
    /// </summary>
    public class Vertex : INotifyPropertyChanged
    {
        public string currentVertex;
        private string point = string.Empty;
        private string color = "Black";
        private List<string> edges = new List<string>();
        private double width = 0.0;
        private double height = 0.0;
        private double vx = 0.0;
        private double vy = 0.0;

        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.  
        // The CallerMemberName attribute that is applied to the optional propertyName  
        // parameter causes the property name of the caller to be substituted as an argument.  
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Create Vertex point
        /// </summary>
        /// <param name="num"></param>
        public Vertex(int num, double vx, double vy, double width, double height)
        {
            Point = "v" + num.ToString();
            Width = width;
            Height = height;
            Vx = vx;
            Vy = vy;
        }
        
        /// <summary>
        /// Gets or sets the vertex Uid
        /// </summary>
        public string Point
        {
            get => this.point;
            set
            {
                if (this.point != value)
                {
                    this.point = value;
                    this.NotifyPropertyChanged();
                }
            }
        }
        public double Width
        {
            get => this.width;
            set => this.width = value;
        }
        public double Height
        {
            get => this.height;
            set => this.height = value;
        }
        public double Vx
        {
            get => this.vx;
            set
            {
                if (this.vx != value)
                {
                    this.vx = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public double Vy
        {
            get => this.vy;
            set
            {
                if (this.vy != value)
                {
                    this.vy = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

    }
}
