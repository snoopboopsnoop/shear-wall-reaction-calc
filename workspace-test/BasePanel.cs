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
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Net;

namespace workspace_test
{
    public class BasePanel : Panel
    {
        // snapping distance for drawing
        public const int snap = 10;

        // dist from vertex before hover
        public const int hoverDist = 10;

        // near distance for selecting lines
        public const int near = 3;

        // how big to make the vertex dots
        public const int dotSize = 6;

        // default font
        public readonly Font font = new Font("Arial", 8);

        // formatting for measurement display text
        public StringFormat formatWidth = new StringFormat();
        public StringFormat formatHeight = new StringFormat();
        

        // select | pen | point | readOnly
        private string pointerMode = "select";

        // mouse statuses
        private Boolean drawing = false;
        private Boolean dragging = false;
        private Boolean selecting = false;

        // used to check if mouse clicked in one spot
        private PointF clickOff = Point.Empty;

        // previous mouse position, for stuff
        private PointF lastPos = PointF.Empty;

        // start end points for mouse drag
        private PointF start = PointF.Empty;
        private PointF end = PointF.Empty;

        private Size translate = new Size();
        private Size prevTranslate = new Size();
        private float scale = 1.0f;

        // which point is currently being hovered over, draws bigger one over it later
        private PointF hover = PointF.Empty;

        // point appearing when hovering over line in point mode
        private PointF snapPoint = PointF.Empty;
        private int snapLine = -1;
        private int snapLineDraw = -1;

        // the funny red line that tells you if you're lined up
        private PointF suggestLineH = PointF.Empty;
        private PointF suggestLineV = PointF.Empty;

        // the funny blue rectangle when you drag to select
        private Rectangle selection = Rectangle.Empty;

        // List containing drawn lines
        private List<Tuple<PointF, PointF>> lines = new List<Tuple<PointF, PointF>>();

        // list containing positions of drawn lines that are currently selected
        private List<int> selectedLines = new List<int>();

        // scale
        private Label scaleLabel;
        private bool scaleEnabled = true;

        // cm
        ContextMenuStrip cm = new ContextMenuStrip();

        // parent
        private Main main;

        // default constructor
        public BasePanel()
        {
            Dock = DockStyle.Fill;
            DoubleBuffered = true;
            BorderStyle = BorderStyle.None;
            Margin = new Padding(0, 0, 0, 0);

            // settings to allow importing images behind
            BackColor = Color.White;
            BackgroundImage = null;
            BackgroundImageLayout = ImageLayout.Center;
            DoubleBuffered = true;

            // string formatting settings
            formatWidth.Alignment = StringAlignment.Center;
            formatWidth.LineAlignment = StringAlignment.Far;

            formatHeight.Alignment = StringAlignment.Near;
            formatHeight.LineAlignment = StringAlignment.Center;

            scaleLabel = new Label();
            scaleLabel.ForeColor = Color.Black;
            scaleLabel.Parent = this;
            scaleLabel.BackColor = Color.Transparent;
            scaleLabel.Location = new Point(0, 0);
            scaleLabel.AutoSize = true;
            scaleLabel.Padding = new Padding(5);

            scaleLabel.Text = $"Scale: 1 pixel = {Globals.scale}{Globals.unit}";

            this.Controls.Add(scaleLabel);

            //cm = new ContextMenuStrip();
            base.ContextMenuStrip = cm;

            //// NOTE: IF YOU CHANGE THE CM CHANGE INDEXES IN CONTEXTMENU_OPENING FUNC

            // 0
            cm.Items.Add("Run Shear Reaction");
            //// 1
            //ToolStripItem clearButton = cm.Items.Add("Clear Shear");
            //clearButton.Enabled = false;
            //// 2
            cm.Items.Add("Set Scale");
            //// 3
            //ToolStripItem weightButton = cm.Items.Add("Add Weight...");
            //weightButton.Enabled = false;
            //// 4
            //ToolStripItem printButton = cm.Items.Add("Print Workspace to Active Word Doc");
            //printButton.Enabled = false;
            //// 5
            //ToolStripItem dragButton = cm.Items.Add("Drag Calculations");
            //dragButton.Enabled = false;

        }

        public BasePanel(Main main) : this()
        {
            this.main = main;
        }

        public void ToggleScaleLabel()
        {
            if (scaleEnabled)
            {
                scaleLabel.Visible = false;
                scaleEnabled = false;
            }
            else
            {
                scaleLabel.Visible = true;
                scaleEnabled = true;
            }
        }

        /* retrieves list of lines in panel
         * @return lines
         */
        public List<Tuple<PointF, PointF>> Lines()
        {
            return lines;
        }

        /* retrieves list of selected line indexes in panel
         * @return lines
         */
        public List<int> SelectedLines()
        {
            return selectedLines;
        }

        /* changes pointer type to parameter mode
         * @param mode: the mode to change to
         */
        public void SetPointerMode(string mode)
        {
            drawing = false;
            dragging = false;
            selecting = false;

            pointerMode = mode;

            Invalidate();
        }

        // set control contextmenu
        public void SetCM(ContextMenuStrip c)
        {
            //Console.WriteLine("cm size: " + cm.Items.Count);
            //Console.WriteLine("adfhdakjfe");
            this.cm.Items.Add("bobr");
        }

        // set scale of lines in panel
        public void SetScale(float value, string paramUnit)
        {
            Globals.scale = value;
            Globals.unit = paramUnit;
            scaleLabel.Text = $"Scale: 1 pixel = {Globals.scale.ToString("N2")}{Globals.unit}";
        }

        // set list of lines in panel
        public void SetLines(List<Tuple<PointF, PointF>> lines)
        {
            this.lines = lines;
            Invalidate();
        }

        // add line to panel
        public void AddLine(Tuple<PointF, PointF> line)
        {
            lines.Add(line);
            Invalidate();
        }

        public void AddRectangle(float x, float y, float width, float height)
        {
            PointF topLeft = new PointF(x, y);
            PointF topRight = new PointF(x + width, y);
            PointF bottomLeft = new PointF(x, y + height);
            PointF bottomRight = new PointF(x + width, y + height);

            lines.Add(new Tuple<PointF, PointF>(topLeft, topRight));
            lines.Add(new Tuple<PointF, PointF>(topLeft, bottomLeft));
            lines.Add(new Tuple<PointF, PointF>(topRight, bottomRight));
            lines.Add(new Tuple<PointF, PointF>(bottomLeft, bottomRight));

            Invalidate();
        }

        // deletes currently selected lines
        public void deleteSelected()
        {
            foreach (var (pos, i) in selectedLines.Select((value, i) => (value, i)))
            {
                lines.RemoveAt(pos - i);
            }

            selectedLines.Clear();

            Invalidate();
        }

        public void ClearSelected()
        {
            selectedLines.Clear();
        }

        // clear all lines from panel
        public void ClearLines()
        {
            lines.Clear();

            Invalidate();
        }

        /* returns magnitude of point to origin
         * @param point: point to consider
         * @return double: magnitude
         */
        private double Magnitude(PointF point)
        {
            return Math.Sqrt(Math.Pow(point.X, 2) + Math.Pow(point.Y, 2));
        }

        /* returns magnitude between two points
         * @param a, b: points to consider
         * @return double: magnitude
         */
        private double Magnitude(PointF a, PointF b)
        {
            return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        /* returns magnitude between two vertices of a line
         * @param line: line to consider
         * @return double: magnitude
         */
        private double Magnitude(Tuple<PointF, PointF> line)
        {
            return Math.Sqrt(Math.Pow(line.Item2.X - line.Item1.X, 2) + Math.Pow(line.Item2.Y - line.Item1.Y, 2));
        }

        // get direction of line between two points
        private string GetDirection(PointF start, PointF location)
        {
            float x = location.X - start.X;
            float y = location.Y - start.Y;

            if (Math.Abs(x) > Math.Abs(y))
            {
                return "horizontal";
            }
            else return "vertical";
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

        // algorithm for getting lines within selection
        private void checkSelection()
        {

            foreach (var (line, i) in lines.Select((value, i) => (value, i)))
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
        }

        private void DrawLine(PaintEventArgs e, Tuple<PointF, PointF> line, Color color, bool drawText = true)
        {
            Font font = new Font("Arial", 8);
            Brush brush = new SolidBrush(color);
            Pen pen = new Pen(brush);
            SolidBrush solidBrush = new SolidBrush(Color.White);

            e.Graphics.DrawLine(pen, new PointF(line.Item1.X * scale, line.Item1.Y * scale), new PointF(line.Item2.X * scale, line.Item2.Y * scale));

            // 2 vertices
            if (pointerMode != "readOnly")
            {
                e.Graphics.FillEllipse(solidBrush, new RectangleF((line.Item1.X - dotSize / 2) * scale, (line.Item1.Y - dotSize / 2) * scale, dotSize, dotSize));
                e.Graphics.DrawEllipse(pen, new RectangleF((line.Item1.X - dotSize / 2) * scale, (line.Item1.Y - dotSize / 2) * scale, dotSize, dotSize));

                e.Graphics.FillEllipse(solidBrush, new RectangleF((line.Item2.X - dotSize / 2) * scale, (line.Item2.Y - dotSize / 2) * scale, dotSize, dotSize));
                e.Graphics.DrawEllipse(pen, new RectangleF((line.Item2.X - dotSize / 2) * scale, (line.Item2.Y - dotSize / 2) * scale, dotSize, dotSize));
            }

            // text display depends on if the line is horizontal or vertical
            if (drawText && pointerMode != "readOnly")
            {
                double magnitude = Magnitude(line);

                if (line.Item1.X == line.Item2.X)
                {
                    e.Graphics.DrawString((Math.Round((magnitude * Globals.scale) / 0.5) * 0.5).ToString("#,#0.###") + Globals.unit, font, brush, (line.Item1.X + 5) * scale, (line.Item1.Y + (line.Item2.Y - line.Item1.Y) / 2) * scale, formatHeight);
                }
                else
                {
                    e.Graphics.DrawString((Math.Round((magnitude * Globals.scale) / 0.5) * 0.5).ToString("#,#0.###") + Globals.unit, font, brush, (line.Item1.X + (line.Item2.X - line.Item1.X) / 2) * scale, (line.Item1.Y - 5) * scale, formatWidth);
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            suggestLineH = PointF.Empty;
            suggestLineV = PointF.Empty;

            if (pointerMode == "pen" && drawing)
            {
                if (start == end)
                {
                    drawing = false;
                    start = PointF.Empty;
                }
                else
                {
                    if (snapLineDraw != -1)
                    {
                        PointF p1 = lines[snapLineDraw].Item1;
                        PointF p2 = lines[snapLineDraw].Item2;
                        lines.RemoveAt(snapLineDraw);
                        lines.Add(new Tuple<PointF, PointF>(p1, start));
                        lines.Add(new Tuple<PointF, PointF>(p2, start));
                        snapLine = -1;
                    }
                    if (snapLine != -1)
                    {
                        PointF p1 = lines[snapLine].Item1;
                        PointF p2 = lines[snapLine].Item2;
                        lines.RemoveAt(snapLine);
                        lines.Add(new Tuple<PointF, PointF>(p1, snapPoint));
                        lines.Add(new Tuple<PointF, PointF>(p2, snapPoint));
                        snapLine = -1;
                    }

                    lines.Add(new Tuple<PointF, PointF>(start, end));
                    start = end;
                }

                snapLineDraw = -1;
                end = PointF.Empty;
                Invalidate();
            }

            else if (e.Button == MouseButtons.Left)
            {
                if (pointerMode == "point")
                {
                    if (snapLine != -1)
                    {
                        PointF p1 = lines[snapLine].Item1;
                        PointF p2 = lines[snapLine].Item2;
                        lines.RemoveAt(snapLine);
                        lines.Add(new Tuple<PointF, PointF>(p1, snapPoint));
                        lines.Add(new Tuple<PointF, PointF>(p2, snapPoint));
                        snapLine = -1;
                    }
                    selectedLines.Clear();
                }

                else if (pointerMode == "pen")
                {
                    drawing = true;
                    dragging = false;
                    if (!hover.IsEmpty)
                    {
                        start = hover;
                    }
                    else if (!snapPoint.IsEmpty)
                    {
                        start = snapPoint;
                        snapLineDraw = snapLine;
                    }
                    else start = e.Location;
                }

                else if (pointerMode == "select")
                {
                    start = e.Location;
                    selecting = true;
                    drawing = false;
                }
            }

            else if (e.Button == MouseButtons.Middle)
            {
                dragging = true;
                start = e.Location;
                prevTranslate = translate;
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

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (drawing && !start.IsEmpty && !end.IsEmpty)
            {
                // resets
                drawing = false;
                start = PointF.Empty;
                end = PointF.Empty;
            }
            else if (dragging)
            {
                dragging = false;
            }
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

        protected override void OnMouseMove(MouseEventArgs e)
        {
            suggestLineV = PointF.Empty;
            suggestLineH = PointF.Empty;
            hover = PointF.Empty;
            snapPoint = Point.Empty;
            snapLine = -1;
            base.OnMouseMove(e);

            if (e.Button == MouseButtons.Left)
            {
                if (drawing) end = e.Location;
                else if (selecting)
                {
                    selection = Rectangle.Round(GetRectangle(start, e.Location));
                }
            }

            // keeps drawn line straight
            else if (drawing)
            {
                string direction = GetDirection(start, e.Location);
                if (direction == "vertical")
                {
                    end = new PointF(start.X, e.Location.Y);
                }
                else if (direction == "horizontal")
                {
                    end = new PointF(e.Location.X, start.Y);
                }
            }

            else if (dragging)
            {
                end = e.Location;
                translate += new Size((int)(end.X - start.X), (int)(end.Y - start.Y));

                start = e.Location;
            }

            // changes pointer to hand if near to line
            else if (pointerMode == "select")
            {
                // get all lines that are close to pointer and pointer is between the vertices
                bool match = lines.Any(item =>
                {
                    if (item.Item1.X == item.Item2.X)
                    {
                        if (Math.Abs(e.Location.X - item.Item1.X) <= near && e.Location.Y >= Math.Min(item.Item1.Y, item.Item2.Y) && e.Location.Y <= Math.Max(item.Item1.Y, item.Item2.Y))
                        {
                            return true;
                        }
                        else return false;
                    }
                    else
                    {
                        if (Math.Abs(e.Location.Y - item.Item1.Y) <= near && e.Location.X >= Math.Min(item.Item1.X, item.Item2.X) && e.Location.X <= Math.Max(item.Item1.X, item.Item2.X))
                        {
                            return true;
                        }
                        else return false;
                    }
                });
                if (match) this.Cursor = Cursors.Hand;
                else this.Cursor = Cursors.Default;
            }

            for (int i = 0; i < lines.Count; ++i)
            {
                Tuple<PointF, PointF> line = lines[i];

                float distX1 = Math.Abs(e.Location.X - line.Item1.X);
                float distX2 = Math.Abs(e.Location.X - line.Item2.X);
                float distY1 = Math.Abs(e.Location.Y - line.Item1.Y);
                float distY2 = Math.Abs(e.Location.Y - line.Item2.Y);

                // check if within hovering distance of point
                if (distX1 <= hoverDist && distY1 <= hoverDist)
                {
                    hover = line.Item1;
                    if (drawing && (hover.X == start.X || hover.Y == start.Y))
                    {
                        end = hover;
                    }
                }
                else if (distX2 <= snap && distY2 <= snap)
                {
                    hover = line.Item2;
                    if (drawing && (hover.X == start.X || hover.Y == start.Y))
                    {
                        end = hover;
                    }
                }

                // don't place point suggestion if hovering
                if (hover.IsEmpty)
                {
                    // check if pointer is close enough between lines to place point suggestion
                    if (pointerMode == "point" || (pointerMode == "pen"))
                    {
                        // check for snapping and line intersection
                        if (distX1 <= snap)
                        {
                            if (drawing)
                            {
                                snapPoint = new PointF(line.Item1.X, end.Y);
                                end = snapPoint;
                            }
                            else if (snapPoint.IsEmpty)
                            {
                                snapPoint = new PointF(line.Item1.X, e.Location.Y);
                            }
                            else
                            {
                                snapPoint = new PointF(line.Item1.X, snapPoint.Y);
                            }

                            suggestLineV = new PointF(line.Item1.X, line.Item1.Y);

                            // if hover intersects line
                            if (line.Item1.X == line.Item2.X)
                            {
                                // if hover intersects segment
                                if (e.Location.Y > Math.Min(line.Item1.Y, line.Item2.Y) + hoverDist && e.Location.Y < Math.Max(line.Item1.Y, line.Item2.Y) - hoverDist)
                                {
                                    snapLine = i;
                                }
                                else if (pointerMode == "point")
                                {
                                    snapPoint = PointF.Empty;
                                    suggestLineV = PointF.Empty;
                                }
                            }
                        }
                        else if (distY1 <= snap)
                        {
                            if (drawing)
                            {
                                snapPoint = new PointF(end.X, line.Item1.Y);
                                end = snapPoint;
                            }
                            else if (snapPoint.IsEmpty)
                            {
                                snapPoint = new PointF(e.Location.X, line.Item1.Y);
                            }
                            else
                            {
                                snapPoint = new PointF(snapPoint.X, line.Item1.Y);
                            }

                            suggestLineH = new PointF(line.Item1.X, line.Item1.Y);

                            if (line.Item1.Y == line.Item2.Y)
                            {
                                if (e.Location.X > Math.Min(line.Item1.X, line.Item2.X) + hoverDist && e.Location.X < Math.Max(line.Item1.X, line.Item2.X) - hoverDist)
                                {
                                    snapLine = i;
                                }
                                else if (pointerMode == "point")
                                {
                                    snapPoint = PointF.Empty;
                                    suggestLineV = PointF.Empty;
                                }
                            }
                        }
                        else if (distX2 <= 10 && pointerMode != "point")
                        {
                            if (snapPoint.IsEmpty)
                            {
                                snapPoint = new PointF(line.Item2.X, e.Location.Y);
                            }
                            else
                            {
                                snapPoint = new PointF(line.Item2.X, snapPoint.Y);
                            }

                            suggestLineV = new PointF(line.Item2.X, line.Item2.Y);
                        }
                        else if (distY2 <= 10 && pointerMode != "point")
                        {
                            if (snapPoint.IsEmpty)
                            {
                                snapPoint = new PointF(e.Location.X, line.Item2.Y);
                            }
                            else
                            {
                                snapPoint = new PointF(snapPoint.X, line.Item2.Y);
                            }

                            suggestLineH = new PointF(line.Item2.X, line.Item2.Y);
                        }
                    }
                }
            }

            Invalidate();
        }
        protected override void OnMouseClick(MouseEventArgs e)
        {
            if(this.Cursor == Cursors.Hand)
            {
                List<float> l = lines.Select(item =>
                {
                    if(item.Item1.X == item.Item2.X)
                    {
                        if (e.Location.Y >= Math.Min(item.Item1.Y, item.Item2.Y) && e.Location.Y <= Math.Max(item.Item1.Y, item.Item2.Y))
                        {
                            return Math.Abs(item.Item1.X - e.Location.X);
                        }
                        else return -1;
                    }
                    else
                    {
                        if (e.Location.X >= Math.Min(item.Item1.X, item.Item2.X) && e.Location.X <= Math.Max(item.Item1.X, item.Item2.X))
                        {
                            return Math.Abs(item.Item1.Y - e.Location.Y);
                        }
                        else return -1;
                    }
                }).ToList();

                if(l.Count > 0)
                {
                    int closest = l.IndexOf(l.Where(i => i >= 0).Min());

                    if (selectedLines.Contains(closest))
                    {
                        selectedLines.Remove(closest);
                        if (Control.ModifierKeys != Keys.Shift)
                        {
                            selectedLines.Clear();
                        }
                    }
                    else
                    {
                        if (Control.ModifierKeys != Keys.Shift)
                        {
                            selectedLines.Clear();
                        }
                        selectedLines.Add(closest);
                    }
                }
                
            }
            else
            {
                selectedLines.Clear();
            }
            Invalidate();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            scale += (float)(0.1 * e.Delta / 120);

            Console.WriteLine("scale:" + scale);

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Color color = Color.FromArgb(25, Color.Blue);
            SolidBrush selectBrush = new SolidBrush(color);
            SolidBrush solidBrush = new SolidBrush(Color.White);

            Color suggestColor = Color.FromArgb(75, Color.Red);
            Pen suggestPen = new Pen(suggestColor);

            e.Graphics.TranslateTransform(translate.Width, translate.Height);

            if (!suggestLineH.IsEmpty)
            {
                if (drawing)
                {
                    e.Graphics.DrawLine(suggestPen, new PointF(end.X * scale, end.Y * scale), new PointF(suggestLineH.X * scale, suggestLineH.Y * scale));
                }
                else if (hover.IsEmpty)
                {
                    e.Graphics.DrawLine(suggestPen, new PointF(snapPoint.X * scale, snapPoint.Y * scale), new PointF(suggestLineH.X * scale, suggestLineH.Y * scale));
                }
            }

            if (!suggestLineV.IsEmpty)
            {
                if (drawing)
                {
                    e.Graphics.DrawLine(suggestPen, new PointF(end.X * scale, end.Y * scale), new PointF(suggestLineV.X * scale, suggestLineV.Y * scale));
                }
                else if (hover.IsEmpty)
                {
                    e.Graphics.DrawLine(suggestPen, new PointF(snapPoint.X * scale, snapPoint.Y * scale), new PointF(suggestLineV.X * scale, suggestLineV.Y * scale));
                }
            }

            foreach (var (line, i) in lines.Select((value, i) => (value, i)))
            {
                Color lineColor = Color.Black;

                if (selectedLines.Contains(i))
                {
                    lineColor = Color.Blue;
                }

                if (snapLine == i)
                {
                    DrawLine(e, new Tuple<PointF, PointF>(line.Item1, snapPoint), lineColor);
                    DrawLine(e, new Tuple<PointF, PointF>(line.Item2, snapPoint), lineColor);
                }

                else if (snapLineDraw == i && drawing)
                {
                    DrawLine(e, new Tuple<PointF, PointF>(line.Item1, start), lineColor);
                    DrawLine(e, new Tuple<PointF, PointF>(line.Item2, start), lineColor);
                }

                else DrawLine(e, line, lineColor);
            }

            if (!start.IsEmpty && !end.IsEmpty)
            {
                if (pointerMode == "pen")
                {
                    DrawLine(e, new Tuple<PointF, PointF>(start, end), Color.Blue);

                }
            }

            if (!hover.IsEmpty)
            {
                e.Graphics.FillEllipse(solidBrush, new RectangleF((hover.X - 5) * scale, (hover.Y - 5) * scale, 10, 10));
                e.Graphics.DrawEllipse(Pens.Black, new RectangleF((hover.X - 5) * scale, (hover.Y - 5) * scale, 10, 10));
            }

            else if (!snapPoint.IsEmpty)
            {
                e.Graphics.FillEllipse(solidBrush, new RectangleF((snapPoint.X - dotSize / 2) * scale, (snapPoint.Y - dotSize / 2) * scale, dotSize, dotSize));
                e.Graphics.DrawEllipse(Pens.Black, new RectangleF((snapPoint.X - dotSize / 2) * scale, (snapPoint.Y - dotSize / 2) * scale, dotSize, dotSize));
            }

            e.Graphics.FillRectangle(selectBrush, selection);
            e.Graphics.DrawRectangle(Pens.Blue, selection);

            solidBrush.Dispose();
            selectBrush.Dispose();
            suggestPen.Dispose();
        }
    }
}
