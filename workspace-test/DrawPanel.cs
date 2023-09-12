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
using System.IO;
using System.Drawing.Imaging;

namespace workspace_test
{
    public class DrawPanel : Panel
    {
        // select | rectangle | pen
        private string pointerMode = "select";

        // mouse statuses
        private Boolean drawing = false;
        private Boolean dragging = false;
        private Boolean selecting = false;
        private Boolean selected = false;
        
        // used to check if mouse clicked in one spot
        private PointF clickOff = Point.Empty;

        // previous mouse position, for stuff
        private PointF lastPos = PointF.Empty;

        // start end points for mouse drag
        private PointF start = PointF.Empty;
        private PointF end = PointF.Empty;

        // which point is currently being hovered over, draws bigger one over it later
        private PointF hover = PointF.Empty;

        // the funny red line that tells you if you're lined up
        private PointF suggestLine = PointF.Empty;

        //index of currently selected rectangle
        private int currentlySelected = -1;

        // the funny blue rectangle when you drag to select
        private Rectangle selection = Rectangle.Empty;

        // List containing drawn rectangles
        private List<Tuple<RectangleF, ShearData>> rects = new List<Tuple<RectangleF, ShearData>>();

        // List containing drawn lines
        private List<Tuple<PointF, PointF>> lines = new List<Tuple<PointF, PointF>>();

        // list containing positions of drawn lines that are currently selected
        private List<int> selectedLines = new List<int>();

        // how big to make the vertex dots
        private int dotSize = 6;

        // default font
        private Font font = new Font("Arial", 8);

        // formatting for measurement display text
        private StringFormat formatWidth = new StringFormat();
        private StringFormat formatHeight = new StringFormat();
        private StringFormat formatwx = new StringFormat();
        private StringFormat formatwy = new StringFormat();
        private StringFormat formatrx = new StringFormat();
        private StringFormat formatry = new StringFormat();

        private int arrowDist = 10;
        private int arrowLength = 20;

        // right click menu
        private ContextMenuStrip cm;

        // calculator values
        private float LA = 0;
        private float LD = 0;

        // i should change its name but it's just all the shear data collected
        // when analysis is performed on a compound object
        private List<Shear> shears = new List<Shear>();

        private string outputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "output/.txt");

        // default constructor
        public DrawPanel()
        {
            Dock = DockStyle.Fill;
            DoubleBuffered = true;
            BorderStyle = BorderStyle.FixedSingle;
        }

        // named panel constructor
        public DrawPanel(string name)
        {
            Dock = DockStyle.Fill;
            DoubleBuffered = true;
            BorderStyle = BorderStyle.FixedSingle;
            Name = name;
            
            // settings to allow importing images behind
            BackColor = Color.Transparent;
            BackgroundImageLayout = ImageLayout.Center;

            // string formatting settings
            formatWidth.Alignment = StringAlignment.Center;
            formatWidth.LineAlignment = StringAlignment.Far;

            formatHeight.Alignment = StringAlignment.Near;
            formatHeight.LineAlignment = StringAlignment.Center;

            formatwx.Alignment = StringAlignment.Far;
            formatwx.LineAlignment = StringAlignment.Center;

            formatwy.Alignment = StringAlignment.Center;
            formatwy.LineAlignment = StringAlignment.Near;

            formatrx.Alignment = StringAlignment.Center;
            formatrx.LineAlignment = StringAlignment.Far;

            formatry.Alignment = StringAlignment.Near;
            formatry.LineAlignment = StringAlignment.Center;

            // basic right click menu code that i stole from stack overflow and modified
            cm = new ContextMenuStrip();
            this.ContextMenuStrip = cm;

            cm.Items.Add("Create Rectangle");
            cm.Items.Add("Run Shear Reaction");
            cm.Items.Add("Pears");

            cm.ItemClicked += new ToolStripItemClickedEventHandler(contextMenu_ItemClicked);
        }

        public void SetPointerMode(string mode)
        {
            System.Console.WriteLine("switching pointer to mode " + mode);
            pointerMode = mode;
        }

        public void SetLA(float paramLA)
        {
            LA = paramLA;
        }

        public void SetLD(float paramLD)
        {
            LD = paramLD;
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
            ShearData data = new ShearData(rect, LA * LD);
            Tuple<RectangleF, ShearData> temp = new Tuple<RectangleF, ShearData>(rect, data);
            rects[i] = temp;
            Invalidate();
            return data;
        }

        public void Export()
        {
            SaveFileDialog sf = new SaveFileDialog();
            sf.Filter = " JPEG Image(.jpeg) | *.jpeg | Png Image(.png) | *.png | Gif Image(.gif) | *.gif | Bitmap Image(.bmp) | *.bmp | Tiff Image(.tiff) | *.tiff | Wmf Image(.wmf) | *.wmf";
            if (sf.ShowDialog() == DialogResult.OK)
            {
                var path = sf.FileName;

                Bitmap bm = new Bitmap(this.Width, this.Height);
                this.DrawToBitmap(bm, new Rectangle(0, 0, this.Width, this.Height));

                bm.Save(path);

                bm.Dispose();
            }
        }

        // makes a rectangle out of 4 selected lines if possible, kind of a useless feature now but it's still cool
        private RectangleF CreateRectangle()
        {
            if(selectedLines.Count != 4)
            {
                return RectangleF.Empty;
            }

            selectedLines.Sort();

            // get the unique points from the lines, should be 4 if it's a rectangle
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

            // wait this might be superfluous but i'll come back to it because this feature sucks
            // don't need this code right why don't you just check if points.count == 4 if not then it can't be a rectangle if so then it must be a rectangle
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

        // returns if 4 coordinates from a rectangle
        private bool isRectangle(List<PointF> points)
        {
            // this cool math i foundo n stack overflow might be useless, see function above
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

        // deal with right click menu selections
        private void contextMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            ToolStripItem item = e.ClickedItem;
            System.Console.WriteLine(item.Text);
            if (item.Text == "Create Rectangle")
            {
                RectangleF rect = CreateRectangle();

                if (!rect.IsEmpty)
                {
                    rects.Add(new Tuple<RectangleF, ShearData>(rect, new ShearData()));
                    Invalidate();
                }
            }
            else if (item.Text == "Run Shear Reaction")
            {
                if (!(LA == 0 || LD == 0)) {
                    cm.Close();

                    SaveFileDialog sf = new SaveFileDialog();
                    sf.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                    if(sf.ShowDialog() == DialogResult.OK)
                    {
                        outputPath = sf.FileName;

                        Algorithm();
                        selectedLines.Clear();
                    }
                }
            }
        }

        // all the shear wall stuff in one bundle
        private void Algorithm()
        {
            // splits all unique vertices by drawing a line through the x and y values
            // (theoretically, though it actually just stores all the unique x and ys)
            List<int> y = new List<int>();
            List<int> x = new List<int>();

            foreach (var pos in selectedLines)
            {
                if (!y.Contains((int)lines[pos].Item1.Y))
                {
                    y.Add((int)lines[pos].Item1.Y);
                }
                if (!y.Contains((int)lines[pos].Item2.Y))
                {
                    y.Add((int)lines[pos].Item2.Y);
                }
                if (!x.Contains((int)lines[pos].Item1.X))
                {
                    x.Add((int)lines[pos].Item1.X);
                }
                if (!x.Contains((int)lines[pos].Item2.X))
                {
                    x.Add((int)lines[pos].Item2.X);
                }
            }

            // vertical: all the vertical line poisitions
            // horizontal: all the horziontal line positions
            List<int> vertical = new List<int>();
            List<int> horizontal = new List<int>();
            List<Tuple<PointF, PointF>> selectLines = new List<Tuple<PointF, PointF>>();

            int shift = 0;

            // initilize the horiozntal and verticals
            foreach (var pos in selectedLines)
            {
                Tuple<PointF, PointF> temp = lines[pos - shift];
                lines.Remove(temp);
                selectLines.Add(temp);
                string direction = getDirection(temp.Item1, temp.Item2);
                if (direction == "vertical") vertical.Add(selectLines.Count() - 1);
                else horizontal.Add(selectLines.Count() - 1);

                ++shift;
            }

            vertical = vertical.OrderBy(p => selectLines[p].Item1.X).ToList();
            horizontal = horizontal.OrderBy(p => selectLines[p].Item1.Y).ToList();

            foreach(var i in vertical)
            {
                Console.WriteLine("line " + selectLines[i].Item1 + " => " + selectLines[i].Item2);
            }

            Console.WriteLine("horizontal: ");

            foreach (var i in horizontal)
            {
                Console.WriteLine("line " + selectLines[i].Item1 + " => " + selectLines[i].Item2);
            }

            x.Sort();
            y.Sort();

            Point min = new Point(9999, 9999);
            Point max = Point.Empty;

            // get minimum and max points to form a big rectangle around the shape
            // useless rn but will be useful for the r value implementation (i hope)
            foreach (var pos in vertical)
            {
                var line = selectLines[pos];
                if (line.Item1.X < min.X) min.X = (int)line.Item1.X;
                else if (line.Item1.X > max.X) max.X = (int)line.Item1.X;
                if (line.Item1.Y < min.Y) min.Y = (int)line.Item1.Y;
                else if (line.Item1.Y > max.Y) max.Y = (int)line.Item1.Y;
                if (line.Item2.X < min.X) min.X = (int)line.Item2.X;
                else if (line.Item2.X > max.X) max.X = (int)line.Item2.X;
                if (line.Item2.Y < min.Y) min.Y = (int)line.Item2.Y;
                else if (line.Item2.Y > max.Y) max.Y = (int)line.Item2.Y;
            }

            // get all the rectangles to shear by forming a rectangle out of the fake lines and the real lines
            // its a lot and tbh i forgot most of the logic
            List<RectangleF> leftRects = new List<RectangleF>();
            for(int i = 0; i < y.Count - 1; i++)
            {
                Tuple<int, int> range = new Tuple<int, int>(y[i], y[i + 1]);
                List<int> lookAt = new List<int>();

                foreach(var pos in vertical)
                {
                    if((range.Item1 >= selectLines[pos].Item1.Y && range.Item2 <= selectLines[pos].Item2.Y) ||
                        range.Item1 >= selectLines[pos].Item2.Y && range.Item2 <= selectLines[pos].Item1.Y)
                    {
                        lookAt.Add(pos);
                    }
                }

                //Console.WriteLine("From y=" + range.Item1 + " to y=" + range.Item2 + ", look at lines " + selectLines[lookAt[0]] + " and " + selectLines[lookAt[1]]);

                RectangleF tempRect;

                for(int j = 0; j < lookAt.Count() - 1; j += 2)
                {
                    Tuple<PointF, PointF> a = selectLines[lookAt[j]];
                    Tuple<PointF, PointF> b = selectLines[lookAt[j+1]];

                    if (a.Item1.X > b.Item1.X)
                    {
                        tempRect = new RectangleF((int)b.Item1.X, range.Item1, a.Item1.X - b.Item1.X, range.Item2 - range.Item1);
                    }
                    else
                    {
                        tempRect = new RectangleF((int)a.Item1.X, range.Item1, b.Item1.X - a.Item1.X, range.Item2 - range.Item1);
                    }
                    leftRects.Add(tempRect);
                }

            }

            // same thing but do it using the vertical lines along the x axis
            List<RectangleF> bottomRects = new List<RectangleF>();
            for (int i = 0; i < x.Count - 1; i++)
            {
                Tuple<int, int> range = new Tuple<int, int>(x[i], x[i + 1]);
                List<int> lookAt = new List<int>();

                foreach (var pos in horizontal)
                {
                    if ((range.Item1 >= selectLines[pos].Item1.X && range.Item2 <= selectLines[pos].Item2.X) ||
                        range.Item1 >= selectLines[pos].Item2.X && range.Item2 <= selectLines[pos].Item1.X)
                    {
                        lookAt.Add(pos);
                    }
                }

                //Console.WriteLine("From x=" + range.Item1 + " to x=" + range.Item2 + ", look at lines " + selectLines[lookAt[0]] + " and " + selectLines[lookAt[1]]);

                RectangleF tempRect;

                for (int j = 0; j < lookAt.Count() - 1; j += 2)
                {
                    Tuple<PointF, PointF> a = selectLines[lookAt[j]];
                    Tuple<PointF, PointF> b = selectLines[lookAt[j + 1]];

                    if (a.Item1.Y > b.Item1.Y)
                    {
                        tempRect = new RectangleF((int)range.Item1, b.Item1.Y, range.Item2 - range.Item1, a.Item1.Y - b.Item2.Y);
                    }
                    else
                    {
                        tempRect = new RectangleF((int)range.Item1, a.Item1.Y, range.Item2 - range.Item1, b.Item1.Y - a.Item1.Y);
                    }
                    bottomRects.Add(tempRect);
                }
            }

            // take all that and send it somewhere else
            shears.Add(new Shear(selectLines, new Tuple<List<RectangleF>, List<RectangleF>>(leftRects, bottomRects), new Tuple<List<int>, List<int>>(x, y), GetRectangle(min, max), LA, LD, outputPath));

            Invalidate();
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
        private void DrawRect(PaintEventArgs e, Tuple<RectangleF, ShearData> rect, Color color, bool show = true, string name = "", int refMeasure = 1)
        {
            Color opaque = Color.FromArgb(25, Color.Blue);
            SolidBrush selectBrush = new SolidBrush(opaque);
            Font font = new Font("Arial", 8);
            Brush brush = new SolidBrush(color);
            Pen pen = new Pen(brush);
            
            RectangleF paramRect = rect.Item1;
            ShearData data = rect.Item2;

            RectangleF[] temp = new RectangleF[1] { rect.Item1 };

            // draw rectangle and measurements
            if (show)
            {
                e.Graphics.DrawRectangles(pen, temp);
                e.Graphics.DrawString(paramRect.Width.ToString() + "ft", font, brush,
                    paramRect.X + paramRect.Width / 2, paramRect.Y - 5, formatWidth);
                e.Graphics.DrawString(paramRect.Height.ToString() + "ft", font, brush,
                    paramRect.X + paramRect.Width + 5, paramRect.Y + paramRect.Height / 2, formatHeight);
            }

            if (data.wx != 0)
            {
                int width = (int)data.wx / refMeasure;
                Rectangle wxBox = new Rectangle((int)(paramRect.X - (width + 5)), (int)paramRect.Y + 4, width, (int)paramRect.Height - 8);
                // wx box
                e.Graphics.FillRectangle(selectBrush, wxBox);
                e.Graphics.DrawRectangle(Pens.Blue, wxBox);
                //e.Graphics.DrawString(data.wx.ToString("0.###") + "ft", font, Brushes.Blue,
                //    paramRect.X - (data.wx + 7), (paramRect.Y + paramRect.Height / 2), formatwx);
                e.Graphics.DrawString(name, font, Brushes.Blue,
                    paramRect.X - (width + 7), (paramRect.Y + paramRect.Height / 2), formatwx);
            }
            if (data.wy != 0)
            {
                int height = (int)data.wy / refMeasure;
                Rectangle wyBox = new Rectangle((int)paramRect.X + 4, (int)(paramRect.Y + 5 + paramRect.Height), (int)paramRect.Width - 8, height);
                // wy box
                e.Graphics.FillRectangle(selectBrush, wyBox);
                e.Graphics.DrawRectangle(Pens.Blue, wyBox);
                //e.Graphics.DrawString(data.wy.ToString("0.###") + "ft", font, Brushes.Blue,
                //    paramRect.X + paramRect.Width / 2, paramRect.Y + paramRect.Height + data.wy + 7, formatwy);
                e.Graphics.DrawString(name, font, Brushes.Blue,
                    paramRect.X + paramRect.Width / 2, paramRect.Y + paramRect.Height + height + 7, formatwy);
            }
                //// for arrows
                //Pen arrowPen = pen;
                //arrowPen.CustomEndCap = new AdjustableArrowCap(2, 2);


                //// rx1
                //e.Graphics.DrawLine(arrowPen,
                //    paramRect.X + paramRect.Width + 15, paramRect.Y + paramRect.Height,
                //    paramRect.X + paramRect.Width + 5, paramRect.Y + paramRect.Height);
                //e.Graphics.DrawString(data.rx1.ToString("0.###") + "lbs", font, brush,
                //    paramRect.X + paramRect.Width + 20, paramRect.Y + paramRect.Height, formatwy);
                //// rx2
                //e.Graphics.DrawLine(arrowPen,
                //    paramRect.X + paramRect.Width + 15, paramRect.Y,
                //    paramRect.X + paramRect.Width + 5, paramRect.Y);
                //e.Graphics.DrawString("rx2", font, brush,
                //    paramRect.X + paramRect.Width + 20, paramRect.Y, formatwy);
                //// ry1
                //e.Graphics.DrawLine(arrowPen, paramRect.X, paramRect.Y - 15, paramRect.X, paramRect.Y - 5);
                //e.Graphics.DrawString(data.ry1.ToString("0.###") + "lbs", font, brush,
                //    paramRect.X, paramRect.Y - 20, formatwx);

                //// ry2
                //e.Graphics.DrawLine(arrowPen, paramRect.X + paramRect.Width, paramRect.Y - 15, paramRect.X + paramRect.Width, paramRect.Y - 5);
                //e.Graphics.DrawString("ry2", font, brush,
                //    paramRect.X + paramRect.Width, paramRect.Y - 20, formatwx);
            
            brush.Dispose();
            pen.Dispose();
            selectBrush.Dispose();
            //arrowPen.Dispose();
        }
        private void DrawArrows(PaintEventArgs e, RectangleF outline, Tuple<List<int>, List<int>> levels)
        {
            Brush brush = new SolidBrush(Color.Black);
            Pen pen = new Pen(brush);

            // for arrows
            Pen arrowPen = pen;
            arrowPen.CustomEndCap = new AdjustableArrowCap(2, 2);

            List<int> xs = levels.Item1;
            List<int> ys = levels.Item2;

            foreach (var (x, i) in xs.Select((x, i) => (x, i)))
            {
                e.Graphics.DrawLine(arrowPen, x, outline.Y - (arrowLength + arrowDist), x, outline.Y - arrowDist);
                e.Graphics.DrawString("R" + (char)(65 + i), font, brush,
                    x, outline.Y - (arrowLength + arrowDist + 5), formatrx);
            }

            foreach (var (y, i) in ys.Select((y, i) => (y, i)))
            {
                e.Graphics.DrawLine(arrowPen,
                    outline.X + outline.Width + (arrowLength + arrowDist), y,
                    outline.X + outline.Width + (arrowDist), y);
                e.Graphics.DrawString("R" + (i + 1), font, brush,
                    outline.X + outline.Width + (arrowLength + arrowDist + 5), y, formatry);
            }
        }

        // draws line lmao
        private void DrawLine(PaintEventArgs e, Tuple<PointF, PointF> line, Color color)
        {
            Font font = new Font("Arial", 8);
            Brush brush = new SolidBrush(color);
            Pen pen = new Pen(brush);
            SolidBrush solidBrush = new SolidBrush(Color.White);

            e.Graphics.DrawLine(pen, line.Item1, line.Item2);

            // 2 vertices
            e.Graphics.FillEllipse(solidBrush, new RectangleF(line.Item1.X - dotSize/2, line.Item1.Y - dotSize/2, dotSize, dotSize));
            e.Graphics.DrawEllipse(pen, new RectangleF(line.Item1.X - dotSize / 2, line.Item1.Y - dotSize / 2, dotSize, dotSize));

            e.Graphics.FillEllipse(solidBrush, new RectangleF(line.Item2.X - dotSize / 2, line.Item2.Y - dotSize / 2, dotSize, dotSize));
            e.Graphics.DrawEllipse(pen, new RectangleF(line.Item2.X - dotSize / 2, line.Item2.Y - dotSize / 2, dotSize, dotSize));

            double magnitude = Math.Sqrt(Math.Pow(line.Item2.X - line.Item1.X, 2) + Math.Pow(line.Item2.Y - line.Item1.Y, 2));

            // text display depends on if the line is horizontal or vertical
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

        // paint solid rectangles and currently drawing rectangles
        protected override void OnPaint(PaintEventArgs e)
        {
            Color color = Color.FromArgb(25, Color.Blue);
            SolidBrush selectBrush = new SolidBrush(color);
            SolidBrush solidBrush = new SolidBrush(Color.White);

            if (!suggestLine.IsEmpty)
            {
                e.Graphics.DrawLine(Pens.Red, end, suggestLine);
            }
            
            foreach (var (line, i) in lines.Select((value, i) => (value, i)))
            {
                if(selectedLines.Contains(i))
                {
                    DrawLine(e, line, Color.Blue);
                }
                else DrawLine(e, line, Color.Black);
            }

            foreach (var (rectangle, i) in rects.Select((value, i) => (value, i)))
            {
                if (i == currentlySelected)
                {
                    DrawRect(e, rectangle, Color.Blue);
                }
                else DrawRect(e, rectangle, Color.Red);
            }

            foreach (var shear in shears)
            {
                foreach (var (line, i) in shear.GetLines().Select((value, i) => (value, i)))
                {
                    DrawLine(e, line, Color.Black);
                }

                Tuple<List<RectangleF>, List<RectangleF>> data = shear.GetData();
                Tuple<List<ShearData>, List<ShearData>> shearData = shear.GetShearData();

                if (data != null)
                {
                    int wxMeasure = shearData.Item1.Min(p => (int)p.wx);
                    int wyMeasure = shearData.Item2.Min(p => (int)p.wy);

                    int refMeasure = (wxMeasure > wyMeasure) ? wyMeasure : wxMeasure;

                    //Console.WriteLine("ref: " + refMeasure);

                    //Console.WriteLine("not empty data");
                    foreach (var (rect, i) in data.Item1.Select((rect, i) => (rect, i)))
                    {
                        DrawRect(e, new Tuple<RectangleF, ShearData>(rect, shearData.Item1[i]), Color.Black, false, "Wx" + (i+1), refMeasure);
                    }
                    foreach (var (rect, i) in data.Item2.Select((rect, i) => (rect, i)))
                    {

                        DrawRect(e, new Tuple<RectangleF, ShearData>(rect, shearData.Item2[i]), Color.Black, false, "Wy" + (i+1), refMeasure);
                    }

                    DrawArrows(e, shear.GetDimensions(), shear.GetReactions());
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
