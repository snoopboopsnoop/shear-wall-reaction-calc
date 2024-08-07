using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace workspace_test
{
    public class Line
    {
        // require p1 < p2
        private Point p1 = Point.Empty;
        private Point p2 = Point.Empty;

        private double magnitude = 0;

        private Color color = Color.Black;
        private int opacity = 100;

        public Line()
        {

        }

        // initialize p1 and p2 by requiring p1 < p2, unless diagonal lines then order by x value
        public Line(Point p1, Point p2)
        {
            // vertical line
            if(p1.X == p2.X)
            {
                if(p1.Y < p2.Y)
                {
                    this.p1 = p1;
                    this.p2 = p2;
                }
                else
                {
                    this.p1 = p2;
                    this.p2 = p1;
                }
            }
            // horizontal
            else
            {
                if(p1.X < p2.X)
                {
                    this.p1 = p1;
                    this.p2 = p2;
                }
                else
                {
                    this.p1 = p2;
                    this.p2 = p1;
                }
            }

            RefreshMagnitude();
        }

        // constructor with unique color/opacity
        public Line(Point p1, Point p2, Color color, int opacity = 100) : this(p1, p2)
        {
            this.color = color;
            this.opacity = opacity;

            RefreshColor();
        }

        private void RefreshMagnitude()
        {
            this.magnitude = Math.Sqrt(Math.Pow(Math.Abs(p1.X - p2.X), 2) + Math.Pow(Math.Abs(p1.Y - p2.Y), 2));
        }

        private void RefreshColor()
        {
            this.color = Color.FromArgb(opacity, color);
        }

        public void DrawLine(PaintEventArgs e)
        {
            using (Font font = new Font("Arial", 8))
            using (Pen pen = new Pen(color))
            using (SolidBrush solidBrush = new SolidBrush(Color.White))
            {
                e.Graphics.DrawLine(pen, p1, p2);
            }
        }
    }
}
