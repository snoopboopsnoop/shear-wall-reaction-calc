using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.ComponentModel;

namespace workspace_test
{
    public class DrawPanel : Panel
    {
        // select | rectangle |
        private string pointerMode = "select";
        private Boolean drawing = false;
        private Boolean dragging = false;

        private PointF lastPos = PointF.Empty;

        // start end points for mouse drag
        private PointF start = PointF.Empty;
        private PointF end = PointF.Empty;

        private PointF hover = PointF.Empty;

        //index of currently selected rectangle
        private int currentlySelected = -1;

        // List containing drawn rectangles
        //private List<RectangleF> rects = new List<RectangleF>();
        //private List<Tuple<RectangleF, ShearData>> analyzed = new List<Tuple<RectangleF, ShearData>>();
        private List<Tuple<RectangleF, ShearData>> rects = new List<Tuple<RectangleF, ShearData>>();
        private List<Tuple<PointF, PointF>> lines = new List<Tuple<PointF, PointF>>();

        private int dotSize = 6;

        private Font font = new Font("Arial", 8);

        private StringFormat formatWidth = new StringFormat();
        private StringFormat formatHeight = new StringFormat();
        private StringFormat formatwx = new StringFormat();
        private StringFormat formatwy = new StringFormat();

        

        // default constructor
        public DrawPanel()
        {
            Dock = DockStyle.Fill;
            DoubleBuffered = true;
            BorderStyle = BorderStyle.FixedSingle;
            this.GotFocus += workspace_GotFocus;
        }

        // named panel constructor
        public DrawPanel(string name)
        {
            Dock = DockStyle.Fill;
            DoubleBuffered = true;
            BorderStyle = BorderStyle.FixedSingle;
            Name = name;

            // string formatting settings
            formatWidth.Alignment = StringAlignment.Center;
            formatWidth.LineAlignment = StringAlignment.Far;

            formatHeight.Alignment = StringAlignment.Near;
            formatHeight.LineAlignment = StringAlignment.Center;

            formatwx.Alignment = StringAlignment.Center;
            formatwx.LineAlignment = StringAlignment.Far;

            formatwy.Alignment = StringAlignment.Near;
            formatwy.LineAlignment = StringAlignment.Center;
        }

        public void SetPointerMode(string mode)
        {
            System.Console.WriteLine("switching pointer to mode " + mode);
            pointerMode = mode;
        }

        public int GetCurrentlySelected()
        {
            return currentlySelected;
        }

        public RectangleF GetRectangleFromIndex(int i)
        {
            return rects[i].Item1;
        }

        public void SetRectangle(RectangleF paramRect, int i)
        {
            Tuple<RectangleF, ShearData> temp = new Tuple<RectangleF, ShearData>(paramRect, new ShearData(paramRect));
            rects[i] = temp;
            Invalidate();
        }

        // do the math for shear wall reaction
        public ShearData Calculate(int i, float LA, float LD)
        {
            RectangleF rect = rects[i].Item1;
            ShearData data = new ShearData(rect, LA, LD);
            Tuple<RectangleF, ShearData> temp = new Tuple<RectangleF, ShearData>(rect, data);
            rects[i] = temp;
            Invalidate();
            return data;
        }

        // returns rectangle data from 2 points in form (starting point, (width, length))
        private RectangleF GetRectangle(PointF start, PointF end)
        {
            PointF A = start;
            PointF B = end;

            float width;
            float height;

            PointF top;

            //      b
            //
            // a
            if (A.X < B.X && A.Y > B.Y)
            {
                top = new PointF(A.X, B.Y);
                width = B.X - A.X;
                height = A.Y - B.Y;
            }
            // a
            //
            //      b
            else if (A.X < B.X && A.Y < B.Y)
            {
                top = new PointF(A.X, A.Y);
                width = B.X - A.X;
                height = B.Y - A.Y;
            }
            //      a
            //
            // b
            else if (A.X > B.X && A.Y < B.Y)
            {
                top = new PointF(B.X, A.Y);
                width = A.X - B.X;
                height = B.Y - A.Y;
            }
            // b
            //
            //      a
            else
            {
                top = new PointF(B.X, B.Y);
                width = A.X - B.X;
                height = A.Y - B.Y;
            }

            RectangleF rect = new RectangleF(top, new SizeF(width, height));
            return rect;
        }

        private Tuple<PointF, PointF> RectToPP(RectangleF paramRect)
        {
            // point = (Ax + width, Ay + height)
            PointF end = new PointF(paramRect.X + paramRect.Width, paramRect.Y + paramRect.Height);

            return new Tuple<PointF, PointF>(new PointF(paramRect.X, paramRect.Y), end);
        }

        //draws rectangle without any physics data
        private void DrawRect(PaintEventArgs e, RectangleF paramRect, Color color)
        {
            Brush brush = new SolidBrush(color);
            Pen pen = new Pen(brush);

            RectangleF[] temp = new RectangleF[1] { paramRect };

            StringFormat formatWidth = new StringFormat();
            StringFormat formatHeight = new StringFormat();

            // string formatting settings
            formatWidth.Alignment = StringAlignment.Center;
            formatWidth.LineAlignment = StringAlignment.Far;

            formatHeight.Alignment = StringAlignment.Near;
            formatHeight.LineAlignment = StringAlignment.Center;

            // draw rectangle
            e.Graphics.DrawRectangles(pen, temp);
            e.Graphics.DrawString(paramRect.Width.ToString() + "ft", font, brush,
                paramRect.X + paramRect.Width / 2, paramRect.Y - 5, formatWidth);
            e.Graphics.DrawString(paramRect.Height.ToString() + "ft", font, brush,
                paramRect.X + paramRect.Width + 5, paramRect.Y + paramRect.Height / 2, formatHeight);

            // destructors
            brush.Dispose();
            pen.Dispose();
        }

        // draws rectangle on screen from parameter data
        private void DrawRect(PaintEventArgs e, Tuple<RectangleF, ShearData> rect, Color color)
        {
            Font font = new Font("Arial", 8);
            Brush brush = new SolidBrush(color);
            Pen pen = new Pen(brush);
            
            RectangleF paramRect = rect.Item1;
            ShearData data = rect.Item2;

            RectangleF[] temp = new RectangleF[1] { rect.Item1 };

            // draw rectangle and measurements
            e.Graphics.DrawRectangles(pen, temp);
            e.Graphics.DrawString(paramRect.Width.ToString() + "ft", font, brush,
                paramRect.X + paramRect.Width/2, paramRect.Y - 5, formatWidth);
            e.Graphics.DrawString(paramRect.Height.ToString() + "ft", font, brush,
                paramRect.X + paramRect.Width + 5, paramRect.Y + paramRect.Height/2, formatHeight);

            if (data.wx != 0 && data.wy != 0)
            {
                // wx box
                e.Graphics.DrawRectangle(Pens.Red, paramRect.X - (data.wx + 20), paramRect.Y, data.wx, paramRect.Height);
                e.Graphics.DrawString(data.wx.ToString("0.###") + "ft", font, Brushes.Red,
                    paramRect.X  - (data.wx/2 + 20), paramRect.Y - 5, formatwx);

                // wy box
                e.Graphics.DrawRectangle(Pens.Red, paramRect.X, paramRect.Y + 20 + paramRect.Height, paramRect.Width, data.wy);
                e.Graphics.DrawString(data.wy.ToString("0.###") + "ft", font, Brushes.Red, 
                    paramRect.X + paramRect.Width + 5, paramRect.Y + paramRect.Height + 20 + data.wy / 2, formatwy);

                // for arrows
                Pen arrowPen = pen;
                arrowPen.CustomEndCap = new AdjustableArrowCap(2, 2);

                // rx1
                e.Graphics.DrawLine(arrowPen,
                    paramRect.X + paramRect.Width + 15, paramRect.Y + paramRect.Height,
                    paramRect.X + paramRect.Width + 5, paramRect.Y + paramRect.Height);
                e.Graphics.DrawString(data.rx1.ToString("0.###") + "lbs", font, brush,
                    paramRect.X + paramRect.Width + 20, paramRect.Y + paramRect.Height, formatwy);

                // rx2
                e.Graphics.DrawLine(arrowPen,
                    paramRect.X + paramRect.Width + 15, paramRect.Y,
                    paramRect.X + paramRect.Width + 5, paramRect.Y);
                e.Graphics.DrawString("rx2", font, brush,
                    paramRect.X + paramRect.Width + 20, paramRect.Y, formatwy);

                // ry1
                e.Graphics.DrawLine(arrowPen, paramRect.X, paramRect.Y - 15, paramRect.X, paramRect.Y - 5);
                e.Graphics.DrawString(data.ry1.ToString("0.###") + "lbs", font, brush,
                    paramRect.X, paramRect.Y - 20, formatwx);

                // ry2
                e.Graphics.DrawLine(arrowPen, paramRect.X + paramRect.Width, paramRect.Y - 15, paramRect.X + paramRect.Width, paramRect.Y - 5);
                e.Graphics.DrawString("ry2", font, brush,
                    paramRect.X + paramRect.Width, paramRect.Y - 20, formatwx);
            }
            brush.Dispose();
            pen.Dispose();
        }

        private void DrawLine(Tuple<Point, Point> line)
        {

        }

        // move currently selected rectangle by values given by translation Point
        public void moveSelected(PointF translation)
        {
            RectangleF current = rects[currentlySelected].Item1;
            RectangleF newRect = new RectangleF(current.X + translation.X, current.Y + translation.Y, current.Width, current.Height);
            rects[currentlySelected] = new Tuple<RectangleF, ShearData>(newRect, rects[currentlySelected].Item2);
        }

        private string getDirection(PointF start, PointF location)
        {
            float x = location.X - start.X;
            float y = location.Y - start.Y;

            if (Math.Abs(x) > Math.Abs(y))
            {
                return "horizontal";
            }
            else return "vertical";
        }

        // begin new rectangle if mouse down while rectangle selection
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (pointerMode == "pen" && drawing)
            {
                lines.Add(new Tuple<PointF, PointF>(start, end));

                drawing = false;
                start = PointF.Empty;
                end = PointF.Empty;
                Invalidate();
            }
            else if (e.Button == MouseButtons.Left && (pointerMode == "rectangle" || pointerMode == "pen"))
            {
                drawing = true;
                dragging = false;
                start = e.Location;
            }
            else if(e.Button == MouseButtons.Left && pointerMode == "select")
            {
                drawing = false;
                PointF currPoint = e.Location;

                // check if clicked a rectangle -- if so, make it currently selected
                foreach (var (rectangle, i) in rects.Select((value, i) => (value, i)))
                {
                    Tuple<PointF, PointF> currRect = RectToPP(rectangle.Item1);
                    if (currPoint.X >= currRect.Item1.X && currPoint.X <= currRect.Item2.X
                        && currPoint.Y >= currRect.Item1.Y && currPoint.Y <= currRect.Item2.Y)
                    {
                        currentlySelected = i;

                        // enable dragging on clicked rectangle
                        dragging = true;
                        lastPos = e.Location;
                        break;
                    }
                    else
                    {
                        currentlySelected = -1;
                    }
                }
            }
            else
            {
                dragging = false;
                drawing = false;
                start = PointF.Empty;
            }

            end = PointF.Empty;
            Invalidate();
        }

        // if mouse up when end point, adds new rectangle data
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (drawing && !start.IsEmpty && !end.IsEmpty)
            {
                if (pointerMode == "rectangle")
                {
                    RectangleF rect = GetRectangle(start, end);
                    rects.Add(new Tuple<RectangleF, ShearData>(rect, new ShearData(rect)));
                }
                

                // resets
                drawing = false;
                start = PointF.Empty;
                end = PointF.Empty;
                Invalidate();
            }
            if (dragging) dragging = false;
        }

        // move end location if mouse moves
        protected override void OnMouseMove(MouseEventArgs e)
        {
            hover = PointF.Empty;
            base.OnMouseMove(e);
            if (e.Button == MouseButtons.Left)
            {
                if (drawing) end = e.Location;
                else if (dragging)
                {
                    // draw moved rectangle, update positions
                    moveSelected(new PointF(e.Location.X - lastPos.X, e.Location.Y - lastPos.Y));
                    lastPos = e.Location;
                }
            }
            else if(drawing)
            {
                string direction = getDirection(start, e.Location);
                if(direction == "vertical")
                {
                    end = new PointF(start.X, e.Location.Y);
                }
                else if(direction == "horizontal")
                {
                    end = new PointF(e.Location.X, start.Y);
                }
            }

            foreach(var line in lines)
            {
                if(Math.Sqrt(Math.Pow(line.Item1.X - e.Location.X, 2) + Math.Pow(line.Item1.Y - e.Location.Y, 2)) <= 6)
                {
                    hover = line.Item1;
                }
                else if (Math.Sqrt(Math.Pow(line.Item2.X - e.Location.X, 2) + Math.Pow(line.Item2.Y - e.Location.Y, 2)) <= 6)
                {
                    hover = line.Item2;
                }
            }

            Invalidate();
        }

        private void workspace_GotFocus(Object sender, EventArgs e)
        {
            System.Console.WriteLine("focused");
        }

        // paint solid rectangles and currently drawing rectangles
        protected override void OnPaint(PaintEventArgs e)
        {
            SolidBrush solidBrush = new SolidBrush(Color.White);

            

            foreach (var (rectangle, i) in rects.Select((value, i) => (value, i)))
            {
                if(i == currentlySelected)
                {
                    DrawRect(e, rectangle, Color.Blue);
                }
                else DrawRect(e, rectangle, Color.Black);
            }
            foreach (var line in lines)
            {
                e.Graphics.DrawLine(Pens.Black, line.Item1, line.Item2);

                
                e.Graphics.FillEllipse(solidBrush, new RectangleF(line.Item1.X - 3, line.Item1.Y - 3, 6, 6));
                e.Graphics.DrawEllipse(Pens.Black, new RectangleF(line.Item1.X - 3, line.Item1.Y - 3, 6, 6));
                
                e.Graphics.FillEllipse(solidBrush, new RectangleF(line.Item2.X - 3, line.Item2.Y - 3, 6, 6));
                e.Graphics.DrawEllipse(Pens.Black, new RectangleF(line.Item2.X - 3, line.Item2.Y - 3, 6, 6));

                double magnitude = Math.Sqrt(Math.Pow(line.Item2.X - line.Item1.X, 2) + Math.Pow(line.Item2.Y - line.Item1.Y, 2));

                if (line.Item1.X == line.Item2.X)
                {
                    e.Graphics.DrawString(magnitude.ToString("0.###"), font, Brushes.Black, line.Item1.X + 10, line.Item1.Y + (line.Item2.Y - line.Item1.Y) / 2, formatHeight);
                }
                else
                {
                    e.Graphics.DrawString(magnitude.ToString("0.###"), font, Brushes.Black, line.Item1.X + (line.Item2.X - line.Item1.X) / 2, line.Item1.Y - 10, formatWidth);
                }
            }

            if (!start.IsEmpty && !end.IsEmpty)
            {
                if(pointerMode == "rectangle") {
                    RectangleF measurements = GetRectangle(start, end);
                    DrawRect(e, measurements, Color.Blue);
                }
                else if(pointerMode == "pen")
                {
                    e.Graphics.DrawLine(Pens.Blue, start, end);
                    e.Graphics.FillEllipse(solidBrush, new RectangleF(start.X - dotSize/2, start.Y - dotSize/2, dotSize, dotSize));
                    e.Graphics.DrawEllipse(Pens.Blue, new RectangleF(start.X - dotSize / 2, start.Y - dotSize / 2, dotSize, dotSize));

                    e.Graphics.FillEllipse(solidBrush, new RectangleF(end.X - dotSize / 2, end.Y - dotSize / 2, dotSize, dotSize));
                    e.Graphics.DrawEllipse(Pens.Blue, new RectangleF(end.X - dotSize / 2, end.Y - dotSize / 2, dotSize, dotSize));

                    double magnitude = Math.Sqrt(Math.Pow(end.X - start.X, 2) + Math.Pow(end.Y - start.Y, 2));
                    if(start.X == end.X)
                    {
                        e.Graphics.DrawString(magnitude.ToString("0.###"), font, Brushes.Blue, start.X + 10, start.Y + (end.Y - start.Y) / 2, formatHeight);
                    }
                    else
                    {
                        e.Graphics.DrawString(magnitude.ToString("0.###"), font, Brushes.Blue, start.X + (end.X - start.X) / 2, start.Y - 10, formatWidth);
                    }
                    
                }
                
            }
            e.Graphics.FillEllipse(solidBrush, new RectangleF(hover.X - 5, hover.Y - 5, 10, 10));
            e.Graphics.DrawEllipse(Pens.Black, new RectangleF(hover.X - 5, hover.Y - 5, 10, 10));

            solidBrush.Dispose();
        }
    }
}
