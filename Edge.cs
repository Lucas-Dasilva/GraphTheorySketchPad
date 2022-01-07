using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace GraphTheorySketchPad
{

    /// <summary>
    /// Want the edges to subscribe to the two vertices it is connected to
    /// </summary>
    public class Edge : INotifyPropertyChanged
    {
        private string v1;
        private string v2;
        private string name;
        private double x1;
        private double x2;
        private double y1;
        private double y2;
        private double edgeCurveX;
        private double edgeCurveY;
        private double height;
        private int size;

        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.  
        // The CallerMemberName attribute that is applied to the optional propertyName  
        // parameter causes the property name of the caller to be substituted as an argument.  
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public Edge(string v1, string v2, double x1, double y1, double x2, double y2)
        {
            this.name = v1 + v2;
            this.v1 = v1;
            this.v2 = v2;
            this.x1 = x1;
            this.y1 = y1;
            this.x2 = x2;
            this.y2 = y2;
            this.height = -1.0;
            this.size = -1;
            this.EdgeCurveX = -1.0;
        }
        public string V1
        {
            get => this.v1;
            set => this.v1 = value;
        }
        public string V2
        {
            get => this.v2;
            set => this.v2 = value;
        }
        public string Name
        {
            get => this.name;
            set => this.name = value;
        }
        public double X1
        {
            get => this.x1;
            set
            {
                if (this.y1 != value)
                {
                    this.x1 = value;
                    this.NotifyPropertyChanged();
                }
            }
        }
        public double Y1
        {
            get => this.y1;
            set
            {
                if (this.y1 != value)
                {
                    this.y1 = value;
                    this.NotifyPropertyChanged();
                }
            }
        }
        public double X2
        {
            get => this.x2;
            set
            {
                if (this.x2 != value)
                {
                    this.x2 = value;
                    this.NotifyPropertyChanged();
                }
            }
        }
        public double Y2
        {
            get => this.y2;
            set
            {
                if (this.y2 != value)
                {
                    this.y2 = value;
                    this.NotifyPropertyChanged();
                }
            }
        }
        public double EdgeCurveX
        {
            get => this.edgeCurveX;
            set => this.edgeCurveX = value;
        }
        public double EdgeCurveY
        {
            get => this.edgeCurveY;
            set => this.edgeCurveY = value;

        }
        public double Height
        {
            get => this.height;
            set => this.height = value;
        }
        public int Size
        {
            get => this.size;
            set => this.size = value;
        }

    }
}
