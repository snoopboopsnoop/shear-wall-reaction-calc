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

using Word = Microsoft.Office.Interop.Word;

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

        private double scale = 1;
        private string unit;

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

        private Label scaleLabel;

        // calculator values
        private float LA = 0;
        private float LD = 0;

        // i should change its name but it's just all the shear data collected
        // when analysis is performed on a compound object
        private List<Shear> shears = new List<Shear>();

        private string outputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "output/");

        //private string projectPath = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;
        private object docPath = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName, "src/template.docx");

        private string tempFile = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName, "src/temp.jpg");

        private Word._Application word;
        private Word._Document doc;
        private object missing = System.Reflection.Missing.Value;
        private object eod = "\\endofdoc";

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
            cm.Items.Add("Set Scale");

            cm.ItemClicked += new ToolStripItemClickedEventHandler(contextMenu_ItemClicked);

            scaleLabel = new Label();
            scaleLabel.Location = new Point(0, 0);
            scaleLabel.AutoSize = true;
            scaleLabel.Padding = new Padding(5);
            scaleLabel.Text = $"Scale: 1 pixel = {scale}{unit}";
            this.Controls.Add(scaleLabel);

            Console.WriteLine(docPath);

            word = new Word.Application();
            doc = word.Documents.Add(ref docPath, ref missing, ref missing, ref missing);
            word.Visible = false;
        }

        public void CloseWord()
        {
            try
            {
                doc.Close();
                word.Quit();
            }
            catch(COMException)
            {
                return;
            }
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
            Bitmap bm;

            if (shears.Count != 0)
            {
                RectangleF dims = shears[0].GetDimensions();
                //rects.Add(new Tuple<RectangleF, ShearData>(new RectangleF(((int)dims.Left) - 50, ((int)dims.Top) - 50, ((int)dims.Width) + 100, ((int)dims.Height) + 100), new ShearData()));

                Rectangle bounds = new Rectangle(((int)dims.Left) - 100, ((int)dims.Top) - 100, ((int)dims.Width) + 200, ((int)dims.Height) + 200);

                bm = new Bitmap(bounds.Width, bounds.Height);
                Graphics g = Graphics.FromImage(bm);

                Console.WriteLine("control: " + this.Left + ", " + this.Top);

                g.CopyFromScreen(bounds.Left + this.Left, bounds.Top + this.Top, 0, 0, bm.Size, CopyPixelOperation.SourceCopy);

                Console.WriteLine("rectangel: " + dims.Width + " x " + dims.Height);
                Console.WriteLine("location: " + dims.Left + ", " + dims.Top);
                
                Console.WriteLine("bitmap: " + bm.PhysicalDimension);
            }
            else
            {
                bm = new Bitmap(this.Width, this.Height);
                this.DrawToBitmap(bm, new Rectangle(0, 0, this.Width, this.Height));
            }
            Invalidate();

            if(bm.Width > bm.Height)
            {
                bm.RotateFlip(RotateFlipType.Rotate90FlipNone);
            }

            bm.Save(tempFile, ImageFormat.Jpeg);

            Word.Paragraph image;
            object range = doc.Content.Start;
            Word.Range top = doc.Range(range, range);

            image = doc.Content.Paragraphs.Add(top);

            image.Format.SpaceBefore = 16;

            image.Range.Underline = Word.WdUnderline.wdUnderlineNone;

            word.Visible = true;

            Word.InlineShape shape = image.Range.InlineShapes.AddPicture(tempFile, missing, missing, top);

            shape.Range.Underline = Word.WdUnderline.wdUnderlineNone;
            shape.Range.Font.Bold = 0;
            image.Format.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

            Console.WriteLine("Height: " + shape.Height + ", measure: " + word.InchesToPoints(8.5F) + ", scale: " + shape.ScaleHeight);

            float scale = (word.InchesToPoints(8.5F) / shape.Height);

            if(shape.Width * ((shape.ScaleWidth * scale) / 100) >= word.InchesToPoints(6.0F))
            {
                scale = word.InchesToPoints(6.0F) / shape.Width;
            }

            shape.ScaleHeight = shape.ScaleHeight * scale;
            shape.ScaleWidth = shape.ScaleHeight;

            Console.WriteLine(shape.ScaleWidth);

            Console.WriteLine("top1: " + top.Text);

            top.Underline = Word.WdUnderline.wdUnderlineNone;

            top.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

            // this needs to be here or else it breaks and i don't know why
            top.Text = "bbobr";

            Console.WriteLine("top2: " + top.Text);

            top.Underline = Word.WdUnderline.wdUnderlineNone;

            //top.Text = "bobr";
            Console.WriteLine("top3: " + top.Text);

            top.InsertBreak(Word.WdBreakType.wdPageBreak);

            //top.Text = "bob";

            bm.Dispose();
            File.Delete(tempFile);
        }

        public void SetScale(double value, string paramUnit)
        {
            scale = value;
            unit = paramUnit;
            scaleLabel.Text = $"Scale: 1 pixel = {scale.ToString("N2")}{unit}";
        }

        private double Magnitude(Tuple<PointF, PointF> line)
        {
            return Math.Sqrt(Math.Pow(line.Item2.X - line.Item1.X, 2) + Math.Pow(line.Item2.Y - line.Item1.Y, 2));
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
                        Algorithm();
                        selectedLines.Clear();
                }
            }
            else if(item.Text == "Set Scale")
            {
                if (selectedLines.Count() != 1)
                {
                    Form2 error = new Form2("Error: Need to select 1 line to set scale");
                    error.ShowDialog();
                }
                else
                {
                    Form4 scaleForm = new Form4(this, (int)Magnitude(lines[selectedLines[0]]), unit);
                    scaleForm.ShowDialog();
                }
            }
        }

        // all the shear wall stuff in one bundle
        private void Algorithm()
        {
            word.Visible = true;

            Word.Paragraph header;
            header = doc.Content.Paragraphs.Add();
            header.Range.Underline = Word.WdUnderline.wdUnderlineSingle;
            header.Range.Font.Size = 18;
            header.Range.Text = "SEISMIC WT @ ROOF";
            header.Range.Font.Bold = 1;
            header.Format.SpaceAfter = 16;
            
            header.Range.InsertParagraphAfter();

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
            shears.Add(new Shear(selectLines, new Tuple<List<RectangleF>, List<RectangleF>>(leftRects, bottomRects), new Tuple<List<int>, List<int>>(x, y), GetRectangle(min, max), LA, LD, doc));

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
        private void DrawRect(PaintEventArgs e, Tuple<RectangleF, ShearData> rect, Color color, bool show = true, string name = "")
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

            e.Graphics.FillRectangle(selectBrush, data.visual);
            e.Graphics.DrawRectangle(Pens.Blue, data.visual);
            Console.WriteLine("drawing " + data.visual);

            if (data.wx != 0)
            {
                e.Graphics.DrawString(name, font, Brushes.Blue,
                    paramRect.X - (data.visual.Width + 7), (paramRect.Y + paramRect.Height / 2), formatwx);
            }
            if (data.wy != 0)
            {
                e.Graphics.DrawString(name, font, Brushes.Blue,
                    paramRect.X + paramRect.Width / 2, paramRect.Y + paramRect.Height + data.visual.Height + 7, formatwy);
            }
            
            brush.Dispose();
            pen.Dispose();
            selectBrush.Dispose();
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

            double magnitude = Magnitude(line);

            // text display depends on if the line is horizontal or vertical
            if (line.Item1.X == line.Item2.X)
            {
                e.Graphics.DrawString((Math.Round((magnitude * scale)/0.5) * 0.5).ToString("0.###") + unit, font, brush, line.Item1.X + 5, line.Item1.Y + (line.Item2.Y - line.Item1.Y) / 2, formatHeight);
            }
            else
            {
                e.Graphics.DrawString((Math.Round((magnitude * scale)/0.5)*0.5).ToString("0.###") + unit, font, brush, line.Item1.X + (line.Item2.X - line.Item1.X) / 2, line.Item1.Y - 5, formatWidth);
            }
        }

        private void DrawShear(PaintEventArgs e, Shear shear)
        {
            foreach (var (line, i) in shear.GetLines().Select((value, i) => (value, i)))
            {
                DrawLine(e, line, Color.Black);
            }

            Tuple<List<RectangleF>, List<RectangleF>> data = shear.GetData();
            Tuple<List<ShearData>, List<ShearData>> shearData = shear.GetShearData();

            Console.WriteLine("test: " + shearData.Item1[0].visual);

            if (data != null)
            {
                //Console.WriteLine("not empty data");
                foreach (var (rect, i) in data.Item1.Select((rect, i) => (rect, i)))
                {
                    DrawRect(e, new Tuple<RectangleF, ShearData>(rect, shearData.Item1[i]), Color.Black, false, "Wx" + (i + 1));
                }
                foreach (var (rect, i) in data.Item2.Select((rect, i) => (rect, i)))
                {

                    DrawRect(e, new Tuple<RectangleF, ShearData>(rect, shearData.Item2[i]), Color.Black, false, "Wy" + (i + 1));
                }

                DrawArrows(e, shear.GetDimensions(), shear.GetReactions());
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
            suggestLine = PointF.Empty;
            base.OnMouseDown(e);

            if(e.Button == MouseButtons.Right)
            {
                return;
            }

            if (pointerMode == "pen" && drawing)
            {
                if (start == end)
                {
                    drawing = false;
                    start = PointF.Empty;
                }
                else
                {
                    lines.Add(new Tuple<PointF, PointF>(start, end));
                    start = end;
                }

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

            if(start != end)
            {
                foreach (var line in lines)
                {
                    if (Math.Sqrt(Math.Pow(line.Item1.X - e.Location.X, 2) + Math.Pow(line.Item1.Y - e.Location.Y, 2)) <= 10)
                    {
                        hover = line.Item1;
                        if (drawing)
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

                    if (pointerMode == "pen" && drawing)
                    {
                        if (!line.Item1.Equals(end) && !line.Item2.Equals(end))
                        {
                            string direction = getDirection(start, e.Location);

                            if (direction == "vertical")
                            {
                                if (Math.Abs(line.Item1.Y - e.Location.Y) <= 5)
                                {
                                    suggestLine = line.Item1;
                                    Console.WriteLine("suggest point " + suggestLine);
                                    end = new PointF(start.X, line.Item1.Y);
                                }
                                else if (Math.Abs(line.Item2.Y - e.Location.Y) <= 5)
                                {
                                    suggestLine = line.Item2;
                                    Console.WriteLine("suggest point " + suggestLine);
                                    end = new PointF(start.X, line.Item2.Y);
                                }
                            }
                            else if (direction == "horizontal")
                            {
                                if (Math.Abs(line.Item1.X - e.Location.X) <= 5)
                                {
                                    suggestLine = line.Item1;
                                    Console.WriteLine("suggest point " + suggestLine);
                                    end = new PointF(line.Item1.X, start.Y);
                                }

                                else if (Math.Abs(line.Item2.X - e.Location.X) <= 5)
                                {
                                    suggestLine = line.Item2;
                                    Console.WriteLine("suggest point " + suggestLine);
                                    end = new PointF(line.Item2.X, start.Y);
                                }
                            }
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
                Console.WriteLine("suggest point " +  suggestLine);
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
                DrawShear(e, shear);
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
