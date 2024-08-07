﻿using System;
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

using Word = Microsoft.Office.Interop.Word;
using System.Security.Policy;

namespace workspace_test
{
    public class DrawPanel : BasePanel
    {
        private Floor floor;

        private List<ShearData> selectWeight = new List<ShearData>();

        private int arrowDist = 10;
        private int arrowLength = 20;

        // right click menu
        private ContextMenuStrip cm;

        // calculator values
        private float LA = 0;
        private float LD = 0;

        public StringFormat formatwx = new StringFormat();
        public StringFormat formatwy = new StringFormat();
        public StringFormat formatrx = new StringFormat();
        public StringFormat formatry = new StringFormat();

        // i should change its name but it's just all the shear data collected
        // when analysis is performed on a compound object
        private Shear shear = null;

        private string outputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "output/");

        //private string projectPath = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;
        private object docPath = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName, "src/template.docx");

        private string tempFile = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName, "src/temp.jpg");

        private string loadedFile = "";

        // default constructor
        public DrawPanel() : base()
        {

            Console.WriteLine("child constructor");
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

            // NOTE: IF YOU CHANGE THE CM CHANGE INDEXES IN CONTEXTMENU_OPENING FUNC

            // 0
            cm.Items.Add("Run Shear Reaction");
            // 1
            ToolStripItem clearButton = cm.Items.Add("Clear Shear");
            clearButton.Enabled = false;
            // 2
            cm.Items.Add("Set Scale");
            // 3
            ToolStripItem weightButton = cm.Items.Add("Add Weight...");
            weightButton.Enabled = false;
            // 4
            ToolStripItem printButton = cm.Items.Add("Print Workspace to Active Word Doc");
            printButton.Enabled = false;
            // 5
            ToolStripItem dragButton = cm.Items.Add("Drag Calculations");
            dragButton.Enabled = false;

            //cm.ItemClicked += new ToolStripItemClickedEventHandler(contextMenu_ItemClicked);
            //cm.Opening += contextMenu_Opening;



            //Console.WriteLine(base.cm.Items.Count);
        }

        // named panel constructor
        public DrawPanel(Floor floor, Main main) : this(main)
        {
            this.floor = floor;
            //Load(floor);
        }

        public DrawPanel(Main main) : base(main)
        {
        }

        //public void Save()
        //{
        //    floor.SetLines(base.lines);
        //    if(shear != null)
        //    {
        //        floor.SetShear(shear);
        //    }
        //}

        //public void Load(Floor floor)
        //{
        //    lines = floor.GetLines();
        //    LA = floor.GetLA();
        //    if (floor.GetShear() != null)
        //    {
        //        shear = floor.GetShear();
        //        shear.Load();
        //    }
        //    Invalidate();
        //}

        public void SetLA(float paramLA)
        {
            LA = paramLA;
        }

        public void SetLD(float paramLD)
        {
            LD = paramLD;
            Console.WriteLine("Set LD wiorrskapce: " + LD);
        }

        public void clearShear()
        {
            shear = null;
            Invalidate();
        }

        public string GetLoadedFile()
        {
            return loadedFile;
        }

        // exports formatted screenshot of workspace to current word document
        public void Export()
        {
            Bitmap screenshot;
            using (Bitmap bm = new Bitmap(this.Width, this.Height))
            {
                // screenshot around building if analysis run
                if (shear != null)
                {
                    RectangleF dims = shear.GetDimensions();

                    // draw 100 pixel margin around shear and resize if out of bounds
                    float left = (dims.Left - 100 < 0) ? 0 : dims.Left - 100;
                    float temp = dims.Width + 2 * (dims.Left - left);
                    float width = (temp > this.Width) ? this.Width : temp;

                    float top = (dims.Top - 100 < 0) ? 0 : dims.Top - 100;
                    temp = dims.Height + 2 * (dims.Top - top);
                    float height = (temp > this.Height) ? this.Height : temp;

                    Rectangle bounds = new Rectangle((int)left, (int)top, (int)width, (int)height);

                    // debug
                    //Console.WriteLine("max bounds: " + this.Left + ", " + this.Top + ", " + this.Width + ", " + this.Height);
                    //Console.WriteLine("bounds: " + bounds.Left + ", " + bounds.Top + ", " + this.Width + ", " + this.Height);

                    DrawToBitmap(bm, new Rectangle(this.Left, this.Top, this.Width, this.Height));

                    screenshot = bm.Clone(bounds, bm.PixelFormat);
                    
                    // debug
                    //Console.WriteLine("bitmap: " + bm.PhysicalDimension);
                }
                // if no shear then screenshot whole workspace
                else
                {
                    screenshot = new Bitmap(this.Width, this.Height);
                    this.DrawToBitmap(bm, new Rectangle(0, 0, this.Width, this.Height));
                }
                Invalidate();
            }



            // assign portrait or landscape based on screenshot dimensions
            if (screenshot.Width > screenshot.Height)
            {
                screenshot.RotateFlip(RotateFlipType.Rotate270FlipNone);
            }

            // Docs doesn't take bmps I don't think, save temporary jpg to be inserted
            screenshot.Save(tempFile, ImageFormat.Jpeg);

            Word.Paragraph image;
            object range = Globals.doc.Content.Start;
            Word.Range rangeStart = Globals.doc.Range(range, range);

            image = Globals.doc.Content.Paragraphs.Add(rangeStart);
            image.Format.SpaceBefore = 16;
            image.Range.Underline = Word.WdUnderline.wdUnderlineNone;

            Globals.word.Visible = true;

            // insert workspace export
            Word.InlineShape shape = image.Range.InlineShapes.AddPicture(tempFile, Globals.missing, Globals.missing, rangeStart);

            shape.Range.Underline = Word.WdUnderline.wdUnderlineNone;
            shape.Range.Font.Bold = 0;
            image.Format.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

            // scale image to fit page using 8.5" x 11" size
            float scale = (Globals.word.InchesToPoints(8.5F) / shape.Height);

            if (shape.Width * ((shape.ScaleWidth * scale) / 100) >= Globals.word.InchesToPoints(6.0F))
            {
                scale = Globals.word.InchesToPoints(6.0F) / shape.Width;
            }

            shape.ScaleHeight = shape.ScaleHeight * scale;
            shape.ScaleWidth = shape.ScaleHeight;

            rangeStart.Underline = Word.WdUnderline.wdUnderlineNone;

            rangeStart.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

            // this needs to be here or else it breaks and i don't know why
            rangeStart.Text = "bobr";

            rangeStart.Underline = Word.WdUnderline.wdUnderlineNone;

            rangeStart.InsertBreak(Word.WdBreakType.wdPageBreak);

            screenshot.Dispose();
            File.Delete(tempFile);
        }

        private double Magnitude(PointF point)
        {
            return Math.Sqrt(Math.Pow(point.X, 2) + Math.Pow(point.Y, 2));
        }

        private double Magnitude(PointF a, PointF b)
        {
            return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        private double Magnitude(Tuple<PointF, PointF> line)
        {
            return Math.Sqrt(Math.Pow(line.Item2.X - line.Item1.X, 2) + Math.Pow(line.Item2.Y - line.Item1.Y, 2));
        }


        // deal with right click menu selections
        private void contextMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            ToolStripItem item = e.ClickedItem;
            System.Console.WriteLine(item.Text);
            if (item.Text == "Run Shear Reaction")
            {
                Console.WriteLine(LA + ", " + LD);
                if (!(LA == 0 || LD == 0)) {
                    cm.Close();
                    //SaveFileDialog saveDialog = new SaveFileDialog();
                    //saveDialog.Filter = "Word Document (*.docx)|*.docx";
                    //if(saveDialog.ShowDialog() == DialogResult.OK)
                    //{
                    //    this.Cursor = Cursors.WaitCursor;
                    //    if (!File.Exists(saveDialog.FileName))
                    //    {
                    //        Globals.doc = Globals.word.Documents.Add(ref docPath, ref Globals.missing, ref Globals.missing, ref Globals.missing);
                    //    }
                    //    else
                    //    {
                    //        Globals.doc = Globals.word.Documents.Open(saveDialog.FileName);
                    //    }
                    //    Globals.doc.Content.Delete();
                    //    Globals.doc.SaveAs(saveDialog.FileName);
                    //    AlgorithmNoWord();
                    //    selectedLines.Clear();
                    //    selectedPoints.Clear();
                    //    this.Cursor = Cursors.Default;
                    //}
                    AlgorithmNoWord();
                    ClearSelected();

                    this.Cursor = Cursors.Default;
                }
                else
                {
                    Console.WriteLine("one of the levels is 0");
                }
            }
            else if(item.Text == "Set Scale")
            {
                if (SelectedLines().Count() != 1)
                {
                    Error error = new Error("Error: Need to select 1 line to set scale");
                    error.ShowDialog();
                }
                else
                {
                    ScaleScreen scaleForm = new ScaleScreen(this, (int)Magnitude(Lines()[SelectedLines()[0]]), Globals.unit);
                    scaleForm.ShowDialog();
                }
            }
            else if(item.Text == "Clear Shear")
            {
                clearShear();
            }
            else if(item.Text == "Add Weight...")
            {
                if(selectWeight.Count != 0)
                {
                    List<AddiWeight> weights = new List<AddiWeight>();
                    foreach(ShearData data in selectWeight)
                    {
                        weights.Add(data.aWeight);
                    }
                    AddWeightScreen addWeight = new AddWeightScreen(weights, LA);
                    if (addWeight.ShowDialog() == DialogResult.OK)
                    {
                        foreach (ShearData data in selectWeight)
                        {
                            data.Update();
                        }
                        shear.updateReactions();
                    }
                }
                else
                {
                    //AddWeightScreen addWeight = new AddWeightScreen(hoverWeight.aWeight, LA);
                    //if (addWeight.ShowDialog() == DialogResult.OK)
                    //{
                    //    hoverWeight.Update();
                    //    shear.updateReactions();
                    //}
                }
            }
            else if(item.Text == "Print Workspace to Active Word Doc")
            {
                Export();
            }
            else if(item.Text == "Drag Calculations")
            {
                //main.ToggleDragPage();
            }
        }

        private void contextMenu_Opening(object sender, EventArgs e)
        {
            if (Control.ModifierKeys != Keys.Shift)
            {
                selectWeight.Clear();
            }
            else
            {
                //hoverWeight = null;
            }

            //if (hoverWeight != null || selectWeight.Count() != 0)
            //{
            //    cm.Items[3].Enabled = true;
            //}
            //else
            //{
            //    cm.Items[3].Enabled = false;
            //}
            if (shear != null)
            {
                cm.Items[1].Enabled = true;
            }
            else
            {
                cm.Items[1].Enabled = false;
            }

            if (Globals.doc != null)
            {
                cm.Items[4].Enabled = true;
            }
            else
            {
                cm.Items[4].Enabled = false;
            }

            List<Tuple<PointF, PointF>> lines = Lines();
            List<int> selectedLines = SelectedLines();
            // allow drag calculation if selected lines are all the same horizontally or vertically
            if ((selectedLines.All(item => lines[item].Item1.X == lines[selectedLines[0]].Item1.X) && selectedLines.All(item => lines[item].Item2.X == lines[selectedLines[0]].Item2.X))
                || (selectedLines.All(item => lines[item].Item1.Y == lines[selectedLines[0]].Item1.Y) && selectedLines.All(item => lines[item].Item2.Y == lines[selectedLines[0]].Item2.Y)))
            {
                cm.Items[5].Enabled = true;
            }
            else
            {
                cm.Items[5].Enabled = false;
            }
        }

        private void AlgorithmNoWord()
        {
            List<Tuple<PointF, PointF>> lines = Lines();
            List<int> selectedLines = SelectedLines();

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

            // initilize the horiozntal and verticals
            foreach (var pos in selectedLines)
            {
                Tuple<PointF, PointF> temp = lines[pos];
                //lines.Remove(temp);
                selectLines.Add(temp);

                if (temp.Item1.X == temp.Item2.X)
                {
                    vertical.Add(selectLines.Count() - 1);
                }
                else
                {
                    horizontal.Add(selectLines.Count() - 1);
                }
            }

            vertical = vertical.OrderBy(p => selectLines[p].Item1.X).ToList();
            horizontal = horizontal.OrderBy(p => selectLines[p].Item1.Y).ToList();

            foreach (var i in vertical)
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

            Console.WriteLine("minmax: " + min + ", " + max);

            // get all the rectangles to shear by forming a rectangle out of the fake lines and the real lines
            // its a lot and tbh i forgot most of the logic
            List<RectangleF> leftRects = new List<RectangleF>();
            for (int i = 0; i < y.Count - 1; i++)
            {
                Tuple<int, int> range = new Tuple<int, int>(y[i], y[i + 1]);
                List<int> lookAt = new List<int>();

                foreach (var pos in vertical)
                {
                    if ((range.Item1 >= selectLines[pos].Item1.Y && range.Item2 <= selectLines[pos].Item2.Y) ||
                        range.Item1 >= selectLines[pos].Item2.Y && range.Item2 <= selectLines[pos].Item1.Y)
                    {
                        lookAt.Add(pos);
                    }
                }

                //Console.WriteLine("From y=" + range.Item1 + " to y=" + range.Item2 + ", look at lines " + selectLines[lookAt[0]] + " and " + selectLines[lookAt[1]]);

                RectangleF tempRect;

                for (int j = 0; j < lookAt.Count() - 1; j += 2)
                {
                    Tuple<PointF, PointF> a = selectLines[lookAt[j]];
                    Tuple<PointF, PointF> b = selectLines[lookAt[j + 1]];

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
            shear = new Shear(selectLines, new Tuple<List<RectangleF>,
                              List<RectangleF>>(leftRects, bottomRects),
                              new Tuple<List<int>, List<int>>(x, y),
                              GetRectangle(min, max), LA, LD);

            Invalidate();
        }

        // all the shear wall stuff in one bundle
        private void Algorithm()
        {

            List<Tuple<PointF, PointF>> lines = Lines();
            List<int> selectedLines = SelectedLines();
            Globals.word.Visible = true;

            Word.Paragraph header;
            header = Globals.doc.Content.Paragraphs.Add();
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
                //lines.Remove(temp);
                selectLines.Add(temp);

                if (temp.Item1.X == temp.Item2.X)
                {
                    vertical.Add(selectLines.Count() - 1);
                }
                else
                {
                    horizontal.Add(selectLines.Count() - 1);
                }
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

            Console.WriteLine("minmax: " + min + ", " + max);

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
            shear = new Shear(selectLines, new Tuple<List<RectangleF>,
                              List<RectangleF>>(leftRects, bottomRects),
                              new Tuple<List<int>, List<int>>(x, y),
                              GetRectangle(min, max), LA, LD);

            Invalidate();
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

        // draws rectangle on screen from parameter data
        private void DrawRect(PaintEventArgs e, Tuple<RectangleF, ShearData> rect, Color color, Color weightColor = default(Color), bool show = true, string name = "")
        {
            Color opaque = (weightColor != Color.Empty) ? Color.FromArgb(25, weightColor) : Color.FromArgb(25, Color.Blue);
            SolidBrush selectBrush = new SolidBrush(opaque);
            Font font = new Font("Arial", 8);
            SolidBrush brush = new SolidBrush(color);
            SolidBrush textBrush = new SolidBrush(weightColor);
            Pen pen = new Pen(brush);
            Pen weightPen = new Pen(textBrush);
            
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

            //if(hoverWeight != null && data.visual == hoverWeight.visual)
            //{
            //    selectBrush.Color = Color.FromArgb(25, Color.Red);
            //    textBrush.Color = Color.Red;
            //    e.Graphics.FillRectangle(selectBrush, data.visual);
            //    e.Graphics.DrawRectangle(Pens.Red, data.visual);
            //}
            //else if(selectWeight.Any(x => x.visual == data.visual))
            //{
            //    selectBrush.Color = Color.FromArgb(25, Color.Red);
            //    textBrush.Color = Color.Red;
            //    e.Graphics.FillRectangle(selectBrush, data.visual);
            //    e.Graphics.DrawRectangle(Pens.Red, data.visual);
            //}
            //else
            //{
            //    e.Graphics.FillRectangle(selectBrush, data.visual);
            //    e.Graphics.DrawRectangle(weightPen, data.visual);
            //}
            
            //Console.WriteLine("drawing " + data.visual);

            if (data.wx != 0)
            {
                e.Graphics.DrawString(name, font, textBrush,
                    paramRect.X - (data.visual.Width + 7), (paramRect.Y + paramRect.Height / 2), formatwx);
            }
            if (data.wy != 0)
            {
                e.Graphics.DrawString(name, font, textBrush,
                    paramRect.X + paramRect.Width / 2, paramRect.Y + paramRect.Height + data.visual.Height + 7, formatwy);
            }
            
            brush.Dispose();
            pen.Dispose();
            weightPen.Dispose();
            selectBrush.Dispose();
            textBrush.Dispose();
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

        private void DrawShear(PaintEventArgs e, Shear shear)
        {
            //foreach (var (line, i) in shear.GetLines().Select((value, i) => (value, i)))
            //{
            //    DrawLine(e, line, Color.Black);
            //}

            Tuple<List<RectangleF>, List<RectangleF>> data = shear.GetData();
            Tuple<List<ShearData>, List<ShearData>> shearData = shear.GetShearData();

            //Console.WriteLine("test: " + shearData.Item1[0].visual);

            if (data != null)
            {
                //Console.WriteLine("not empty data");
                foreach (var (rect, i) in data.Item1.Select((rect, i) => (rect, i)))
                {
                    //if(rect.Equals(hoverWeight)) DrawRect(e, new Tuple<RectangleF, ShearData>(rect, shearData.Item1[i]), Color.Black, Color.Red, false, "Wx" + (i + 1));
                    DrawRect(e, new Tuple<RectangleF, ShearData>(rect, shearData.Item1[i]), Color.Black, Color.Blue, false, "Wx" + (i + 1));
                }
                foreach (var (rect, i) in data.Item2.Select((rect, i) => (rect, i)))
                {
                    //if (rect.Equals(hoverWeight)) DrawRect(e, new Tuple<RectangleF, ShearData>(rect, shearData.Item2[i]), Color.Black, Color.Red, false, "Wy" + (i + 1));
                    DrawRect(e, new Tuple<RectangleF, ShearData>(rect, shearData.Item2[i]), Color.Black, Color.Blue, false, "Wy" + (i + 1));
                }

                DrawArrows(e, shear.GetDimensions(), shear.GetReactions());
            }
        }
    }
}
