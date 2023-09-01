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
using System.Collections.Specialized;
using System.Configuration;
using System.Data.SqlClient;
using Microsoft.Win32;

namespace workspace_test
{
    public class DrawPanel : Panel
    {
        // select | rectangle |
        private string pointerMode = "select";
        private Boolean drawing = false;
        private Boolean dragging = false;
        private Boolean selecting = false;
        private Boolean selected = false;
        private PointF clickOff = Point.Empty;

        private PointF lastPos = PointF.Empty;

        // start end points for mouse drag
        private PointF start = PointF.Empty;
        private PointF end = PointF.Empty;

        private PointF hover = PointF.Empty;

        private PointF suggestLine = PointF.Empty;

        //index of currently selected rectangle
        private int currentlySelected = -1;

        private Rectangle selection = Rectangle.Empty;

        // List containing drawn rectangles
        //private List<RectangleF> rects = new List<RectangleF>();
        //private List<Tuple<RectangleF, ShearData>> analyzed = new List<Tuple<RectangleF, ShearData>>();
        private List<Tuple<RectangleF, ShearData>> rects = new List<Tuple<RectangleF, ShearData>>();
        private List<Tuple<PointF, PointF>> lines = new List<Tuple<PointF, PointF>>();
        private List<int> selectedLines = new List<int>();

        private int dotSize = 6;

        private Font font = new Font("Arial", 8);

        private StringFormat formatWidth = new StringFormat();
        private StringFormat formatHeight = new StringFormat();
        private StringFormat formatwx = new StringFormat();
        private StringFormat formatwy = new StringFormat();

        private ContextMenuStrip cm;

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
            BackColor = Color.White;

            // string formatting settings
            formatWidth.Alignment = StringAlignment.Center;
            formatWidth.LineAlignment = StringAlignment.Far;

            formatHeight.Alignment = StringAlignment.Near;
            formatHeight.LineAlignment = StringAlignment.Center;

            formatwx.Alignment = StringAlignment.Center;
            formatwx.LineAlignment = StringAlignment.Far;

            formatwy.Alignment = StringAlignment.Near;
            formatwy.LineAlignment = StringAlignment.Center;

            cm = new ContextMenuStrip();
            this.ContextMenuStrip = cm;

            //cm.Opening += new System.ComponentModel.CancelEventHandler(cms_Opening);
            cm.Items.Add("Create Rectangle");
            cm.Items.Add("Oranges");
            cm.Items.Add("Pears");

            cm.ItemClicked += new ToolStripItemClickedEventHandler(contextMenu_ItemClicked);
        }

        //void cms_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        //{
        //    Control c = cm.SourceControl as Control;

        //    cm.Items.Clear();

        //    cm.Items.Add("Create Rectangle");
        //    cm.Items.Add("Oranges");
        //    cm.Items.Add("Pears");

        //    e.Cancel = false;
        //}

        private void contextMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            ToolStripItem item = e.ClickedItem;
            System.Console.WriteLine(item.Text);
            if(item.Text == "Create Rectangle")
            {
                RectangleF rect = CreateRectangle();

                if(!rect.IsEmpty)
                {
                    rects.Add(new Tuple<RectangleF, ShearData>(rect, new ShearData()));
                    Invalidate();
                }
            }
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

        public void deleteSelected()
        {
            foreach (var (pos, i) in selectedLines.Select((value, i) => (value, i)))
            {
                lines.RemoveAt(pos - i);
            }

            selectedLines.Clear();

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

        private RectangleF CreateRectangle()
        {
            if(selectedLines.Count != 4)
            {
                return RectangleF.Empty;
            }

            selectedLines.Sort();

            List<PointF> points = new List<PointF>();
            foreach (var pos in selectedLines)
            {
                Tuple<PointF, PointF> currLine = lines[pos];
                if(!points.Contains(currLine.Item1))
                {
                    points.Add(currLine.Item1);

                }
                if (!points.Contains(currLine.Item2))
                {
                    points.Add(currLine.Item2);
                }
            }

            if (isRectangle(points))
            {
                points = points.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
                foreach (var point in points)
                {
                    System.Console.WriteLine(point.X + ", " + point.Y);
                }

                foreach (var (pos, i) in selectedLines.Select((value, i) => (value, i)))
                {
                    lines.RemoveAt(pos - i);
                }

                return (new RectangleF(points[0], new Size((int)(points[3].X - points[0].X), (int)(points[3].Y - points[0].Y))));
            }
            else Console.WriteLine("not a rectangle");

            return RectangleF.Empty;
        }

        private bool isRectangle(List<PointF> points)
        {
            if (points.Count != 4)
            {
                Console.WriteLine("not enough points");
                return false;
            }

            double cx = (points[0].X + points[1].X + points[2].X + points[3].X) / 4;
            double cy = (points[0].Y + points[1].Y + points[2].Y + points[3].Y) / 4;

            double dd1 = Math.Pow(cx - points[0].X, 2) + Math.Pow(cy - points[0].Y, 2);
            double dd2 = Math.Pow(cx - points[1].X, 2) + Math.Pow(cy - points[1].Y, 2);
            double dd3 = Math.Pow(cx - points[2].X, 2) + Math.Pow(cy - points[2].Y, 2);
            double dd4 = Math.Pow(cx - points[3].X, 2) + Math.Pow(cy - points[3].Y, 2);

            return Math.Abs(dd1 - dd2) < 1E-6 && Math.Abs(dd1 - dd3) < 1E-6 && Math.Abs(dd1 - dd4) < 1E-6;
        }

        private void checkSelection()
        {

            foreach(var (line, i) in lines.Select((value, i) => (value, i)))
            {
                //    2 
                //  |----|
                //  | 1  |
                //  |____|
                //
                if (selection.Contains(Point.Round(line.Item1)))
                {
                    selectedLines.Add(i);
                }
                //    1 
                //  |----|
                //  | 2  |
                //  |____|
                //
                else if (selection.Contains(Point.Round(line.Item2)))
                {
                    selectedLines.Add(i);
                }
                else if (line.Item1.X <= selection.Right && line.Item1.X >= selection.Left)
                {
                    //    1 
                    //  |----|
                    //  |    |
                    //  |____|
                    //    
                    if (line.Item1.Y < selection.Top)
                    {
                        //    1 
                        //  |----|
                        //  |    |
                        //  |____|
                        //    2
                        if (line.Item2.X == line.Item1.X && line.Item2.Y > selection.Bottom)
                        {
                            selectedLines.Add(i);
                        }
                    }
                    //     
                    //  |----|
                    //  |    |
                    //  |____|
                    //    1
                    else if (line.Item1.Y >= selection.Bottom)
                    {
                        //    2 
                        //  |----|
                        //  |    |
                        //  |____|
                        //    1
                        if (line.Item2.X == line.Item1.X && line.Item2.Y < selection.Top)
                        {
                            selectedLines.Add(i);
                        }
                    }
                }
                else if (line.Item1.Y <= selection.Bottom && line.Item1.Y >= selection.Top)
                {
                    //     
                    //  |----|
                    // 1|    |
                    //  |____|
                    //    
                    if (line.Item1.X < selection.Left)
                    {
                        //     
                        //  |----|
                        // 1|    |  2
                        //  |____|
                        // 
                        if (line.Item2.X > selection.Right)
                        {
                            selectedLines.Add(i);
                        }
                    }
                    //     
                    //  |----|
                    //  |    | 1
                    //  |____|
                    // 
                    else if (line.Item1.X > selection.Right)
                    {
                        //     
                        //  |----|
                        // 2|    | 1
                        //  |____|
                        // 
                        if (line.Item2.X < selection.Left)
                        {
                            selectedLines.Add(i);
                        }
                    }
                }
            }

            if (selectedLines.Count == 0)
            {
                selected = false;
            }
            else selected = true;
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

        private void DrawLine(PaintEventArgs e, Tuple<PointF, PointF> line, Color color)
        {
            Font font = new Font("Arial", 8);
            Brush brush = new SolidBrush(color);
            Pen pen = new Pen(brush);
            SolidBrush solidBrush = new SolidBrush(Color.White);

            e.Graphics.DrawLine(pen, line.Item1, line.Item2);

            e.Graphics.FillEllipse(solidBrush, new RectangleF(line.Item1.X - dotSize/2, line.Item1.Y - dotSize/2, dotSize, dotSize));
            e.Graphics.DrawEllipse(pen, new RectangleF(line.Item1.X - dotSize / 2, line.Item1.Y - dotSize / 2, dotSize, dotSize));

            e.Graphics.FillEllipse(solidBrush, new RectangleF(line.Item2.X - dotSize / 2, line.Item2.Y - dotSize / 2, dotSize, dotSize));
            e.Graphics.DrawEllipse(pen, new RectangleF(line.Item2.X - dotSize / 2, line.Item2.Y - dotSize / 2, dotSize, dotSize));

            double magnitude = Math.Sqrt(Math.Pow(line.Item2.X - line.Item1.X, 2) + Math.Pow(line.Item2.Y - line.Item1.Y, 2));

            if (line.Item1.X == line.Item2.X)
            {
                e.Graphics.DrawString(magnitude.ToString("0.###"), font, brush, line.Item1.X + 5, line.Item1.Y + (line.Item2.Y - line.Item1.Y) / 2, formatHeight);
            }
            else
            {
                e.Graphics.DrawString(magnitude.ToString("0.###"), font, brush, line.Item1.X + (line.Item2.X - line.Item1.X) / 2, line.Item1.Y - 5, formatWidth);
            }
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
                suggestLine = PointF.Empty;
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
                if(!hover.IsEmpty)
                {
                    start = hover;
                }
                else start = e.Location;
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
                        selecting = true;
                        start = e.Location;
                        currentlySelected = -1;
                        System.Console.WriteLine("selecting");
                    }
                }
                if(rects.Count == 0)
                {
                    selecting = true;
                    start = e.Location;
                    currentlySelected = -1;
                    System.Console.WriteLine("selecting");
                }

                if(selected)
                {
                    clickOff = e.Location;
                }
            }
            else
            {
                dragging = false;
                drawing = false;
                start = PointF.Empty;
                clickOff = Point.Empty;
            }




            end = PointF.Empty;
            Invalidate();
        }

        // if mouse up when end point, adds new rectangle data
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (clickOff == e.Location || selecting)
            {
                selectedLines.Clear();
                clickOff = Point.Empty;
            }

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
            }
            else if (dragging) dragging = false;
            else if (selecting)
            {
                checkSelection();

                selecting = false;
                start = PointF.Empty;
                end = PointF.Empty;
                selection = Rectangle.Empty;
            }

            Invalidate();
        }

        // move end location if mouse moves
        protected override void OnMouseMove(MouseEventArgs e)
        {
            suggestLine = PointF.Empty;
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
                else if (selecting)
                {
                    selection = Rectangle.Round(GetRectangle(start, e.Location));
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


            foreach (var line in lines)
            {
                if(Math.Sqrt(Math.Pow(line.Item1.X - e.Location.X, 2) + Math.Pow(line.Item1.Y - e.Location.Y, 2)) <= 10)
                {
                    hover = line.Item1;
                    if(drawing)
                    {
                        end = hover;
                    }
                }
                else if (Math.Sqrt(Math.Pow(line.Item2.X - e.Location.X, 2) + Math.Pow(line.Item2.Y - e.Location.Y, 2)) <= 10)
                {
                    hover = line.Item2;
                    if (drawing)
                    {
                        end = hover;
                    }
                }

                if(pointerMode == "pen" && drawing)
                {
                    if(!line.Item1.Equals(e.Location) && !line.Item2.Equals(e.Location))
                    {
                        string direction = getDirection(start, e.Location);

                        if (Math.Abs(line.Item1.Y - e.Location.Y) <= 5 && direction == "vertical")
                        {
                            suggestLine = line.Item1;
                            end = new PointF(start.X, line.Item1.Y);
                        }
                        else if (Math.Abs(line.Item1.X - e.Location.X) <= 5 && direction == "horizontal")
                        {
                            suggestLine = line.Item1;
                            end = new PointF(line.Item1.X, start.Y);
                        }
                        else if (Math.Abs(line.Item2.Y - e.Location.Y) <= 5 && direction == "vertical")
                        {
                            suggestLine = line.Item2;
                            end = new PointF(start.X, line.Item2.Y);
                        }
                        else if (Math.Abs(line.Item2.X - e.Location.X) <= 5 && direction == "horizontal")
                        {
                            suggestLine = line.Item2;
                            end = new PointF(line.Item2.X, start.Y);
                        }
                    }
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
            Color color = Color.FromArgb(25, Color.Blue);
            SolidBrush selectBrush = new SolidBrush(color);
            SolidBrush solidBrush = new SolidBrush(Color.White);

            if(!suggestLine.IsEmpty)
            {
                e.Graphics.DrawLine(Pens.Red, end, suggestLine);
            }

            foreach (var (rectangle, i) in rects.Select((value, i) => (value, i)))
            {
                if(i == currentlySelected)
                {
                    DrawRect(e, rectangle, Color.Blue);
                }
                else DrawRect(e, rectangle, Color.Black);
            }
            foreach (var (line, i) in lines.Select((value, i) => (value, i)))
            {
                if(selectedLines.Contains(i))
                {
                    DrawLine(e, line, Color.Blue);
                }
                else DrawLine(e, line, Color.Black);
            }

            if (!start.IsEmpty && !end.IsEmpty)
            {
                if(pointerMode == "rectangle") {
                    RectangleF measurements = GetRectangle(start, end);
                    DrawRect(e, measurements, Color.Blue);
                }
                else if(pointerMode == "pen")
                {
                    DrawLine(e, new Tuple<PointF, PointF> (start, end), Color.Blue);
                    
                }
                
            }
            if(!hover.IsEmpty)
            {
                e.Graphics.FillEllipse(solidBrush, new RectangleF(hover.X - 5, hover.Y - 5, 10, 10));
                e.Graphics.DrawEllipse(Pens.Black, new RectangleF(hover.X - 5, hover.Y - 5, 10, 10));

            }

            e.Graphics.FillRectangle(selectBrush, selection);
            e.Graphics.DrawRectangle(Pens.Blue, selection);

            solidBrush.Dispose();
            selectBrush.Dispose();
        }
    }
}
