using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;

namespace workspace_test
{
    internal class DragScreen : TableLayoutPanel
    {
        private DrawPanel panel;
        //private DrawPanel sidePanel;
        private List<DrawPanel> displayPanels = new List<DrawPanel>() { new DrawPanel(), new DrawPanel(), new DrawPanel(), new DrawPanel()};
        private List<Panel> sidePanels = new List<Panel>() { new Panel(), new Panel(), new Panel(), new Panel() };

        private List<int> shears = new List<int>();

        private float reaction = 0;
        private float factor = 0;

        private TextBox rBox;
        private TextBox fBox;

        public DragScreen()
        {
            base.Dock = DockStyle.Fill;
            base.ColumnCount = 2;
            base.RowCount = 5;

            base.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            base.RowStyles.Add(new RowStyle(SizeType.Percent, 15F));
            base.RowStyles.Add(new RowStyle(SizeType.Percent, 15F));
            base.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            base.RowStyles.Add(new RowStyle(SizeType.Percent, 15F));

            base.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 90F));
            base.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));

            panel = new DrawPanel();

            foreach (DrawPanel p in displayPanels)
            {
                p.ToggleScaleLabel();
                p.SetPointerMode("readOnly");
                p.BackColor = Color.White;
            }

            foreach (Panel p in sidePanels)
            {
                p.Dock = DockStyle.Fill;
                p.BackColor = Color.White;
            }

            base.Controls.Add(displayPanels[0], 0, 0);
            base.Controls.Add(displayPanels[1], 0, 1);
            base.Controls.Add(displayPanels[2], 0, 2);
            base.Controls.Add(panel, 0, 3);
            base.Controls.Add(displayPanels[3], 0, 4);

            base.Controls.Add(sidePanels[0], 1, 0);
            base.Controls.Add(sidePanels[1], 1, 1);
            base.Controls.Add(sidePanels[2], 1, 2);
            base.Controls.Add(sidePanels[3], 1, 3);

            Button runButton = new Button();
            runButton.Text = "Go";
            runButton.BackColor = Color.Gray;
            runButton.Click += runButton_Pressed;

            Label rLabel = new Label();
            rBox = new TextBox();

            rLabel.Text = "Reaction = ";
            rLabel.ForeColor = Color.Black;
            rLabel.TextAlign = ContentAlignment.MiddleRight;
            rLabel.AutoSize = true;

            rBox.BorderStyle = BorderStyle.FixedSingle;
            //rBox.TextAlign = HorizontalAlignment.Center;
            rBox.Multiline = false;

            rBox.TextChanged += rBox_TextChanged;

            Panel rPanel = new Panel();
            rPanel.Controls.Add(rLabel);
            rPanel.Controls.Add(rBox);
            rPanel.Width = rLabel.Width + rBox.Width + 10;
            rPanel.Height = rBox.Height + 3;

            rLabel.MinimumSize = new Size(0, rBox.Height);
            rLabel.Height = rBox.Height;
            rLabel.Location = new Point(0, 1);

            displayPanels[3].Controls.Add(runButton);
            displayPanels[3].Controls.Add(rPanel);

            rLabel = new Label();
            fBox = new TextBox();

            rLabel.Text = "Factor = ";
            rLabel.ForeColor = Color.Black;
            rLabel.TextAlign = ContentAlignment.MiddleRight;
            rLabel.AutoSize = true;

            fBox.BorderStyle = BorderStyle.FixedSingle;
            fBox.Multiline = false;
            fBox.TextChanged += fBox_TextChanged;

            Console.WriteLine(rPanel.Height + " - " + rLabel.Height);

            runButton.Location = new Point(displayPanels[3].Width - 10 - runButton.Width, displayPanels[3].Height - 10 - runButton.Height);
            runButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            rBox.Location = new Point(rLabel.Location.X + rLabel.Width + 10, 0);
            rPanel.Location = new Point(10, displayPanels[3].Height - 10 - rPanel.Height);
            rPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        }

        private void runButton_Pressed(object sender, EventArgs e)
        {
            RunAnalysis();
        }

        private void rBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                reaction = float.Parse(rBox.Text);
            }
            catch (Exception ex)
            {

            }
        }

        private void fBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                factor = float.Parse(fBox.Text);
            }
            catch (Exception ex)
            {

            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            Console.WriteLine("bobr");
            base.OnMouseDown(e);
            rBox.Enabled = false;
            rBox.Enabled = true;
        }

        private void RunAnalysis()
        {
            foreach (DrawPanel panel in displayPanels)
            {
                panel.ClearLines();
            }

            foreach(Panel panel in sidePanels)
            {
                panel.Controls.Clear();
            }

            List<Tuple<PointF, PointF>> lines = panel.Lines();

            // data from drawn lines
            float leftX = lines.Min(line => (line.Item1.X < line.Item2.X) ? line.Item1.X : line.Item2.X);
            float rightX = lines.Max(line => (line.Item1.X > line.Item2.X) ? line.Item1.X : line.Item2.X);

            List<float> xCoords = new List<float>();
            foreach(Tuple<PointF, PointF> line in lines)
            {
                xCoords.Add(line.Item1.X);
                xCoords.Add(line.Item2.X);
            }
            xCoords.Sort();

            float dist = (rightX - leftX);

            float coveredDist = lines.Sum(line => Math.Abs(line.Item1.X - line.Item2.X));

            float topHeight = panel.Lines()[0].Item1.Y;

            Panel sidePanel = sidePanels[3];
            Label label = new Label();

            label.Text = (Math.Round((coveredDist * Globals.scale) / 0.5) * 0.5) + "' (" + (Math.Round((dist * Globals.scale) / 0.5) * 0.5) + "' total)";
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.AutoSize = true;
            label.Location = new Point(10, (int)(topHeight - (label.Height / 2)));
            label.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            label.ForeColor = Color.Black;

            sidePanel.Controls.Add(label);

            DrawPanel currPanel = displayPanels[2];
            sidePanel = sidePanels[2];

            // p / L
            float rPerTot = reaction / (float)(Math.Round((dist * Globals.scale) / 0.5) * 0.5);
            // p / sum L
            float rPerCov = reaction / (float)(Math.Round((coveredDist * Globals.scale) / 0.5) * 0.5);
            // p / sumL - p / L
            float slope = rPerCov - rPerTot;

            // height to draw p / L box
            float totBoxHeight = currPanel.Height - 30 - rPerTot;

            currPanel.AddRectangle(leftX, totBoxHeight, dist, rPerTot);

            label = new Label();
            label.Text = rPerTot.ToString();
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.AutoSize = true;
            label.Location = new Point(10, (int)(totBoxHeight + (rPerTot / 2) - (label.Height / 2)));
            label.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            label.ForeColor = Color.Black;

            sidePanel.Controls.Add(label);

            // individual boxes per wall (p/sum L)
            topHeight = 30;
            label = new Label();
            label.Text = rPerCov.ToString();
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.AutoSize = true;
            label.Location = new Point(10, (int)(topHeight + (rPerCov / 2) - (label.Height / 2)));
            label.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            label.ForeColor = Color.Black;

            sidePanel.Controls.Add(label);

            // 2
            sidePanel = sidePanels[1];

            // p / L
            label = new Label();
            label.Text = rPerTot.ToString();
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.AutoSize = true;
            label.Location = new Point(10, (int)(displayPanels[1].Height / 2));
            label.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            label.ForeColor = Color.Black;

            sidePanel.Controls.Add(label);

            // p / sumL - p / L
            label = new Label();
            label.Text = (rPerCov - rPerTot).ToString();
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.AutoSize = true;
            label.Location = new Point(10, (int)((displayPanels[1].Height / 2) - label.Height));
            label.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            label.ForeColor = Color.Black;

            sidePanel.Controls.Add(label);

            foreach (Tuple<PointF, PointF> line in lines)
            {
                float xVal = Math.Min(line.Item1.X, line.Item2.X);
                float width = Math.Abs(line.Item1.X - line.Item2.X);
                float xMax = Math.Max(line.Item1.X, line.Item2.X);

                // p / sum L
                displayPanels[2].AddRectangle(xVal, topHeight, width, rPerCov);

                // 2
                currPanel = displayPanels[1];

                float height2 = currPanel.Height / 2 - slope;
                displayPanels[1].AddRectangle(xVal, height2, width, slope);

                if(xMax != rightX)
                {
                    float width2 = xCoords[xCoords.IndexOf(xMax) + 1] - xMax;
                    displayPanels[1].AddRectangle(xMax, currPanel.Height / 2, width2, rPerTot);
                }
            }

            displayPanels[0].AddLine(new Tuple<PointF, PointF>(new PointF(leftX, displayPanels[0].Height / 2), new PointF(rightX, displayPanels[0].Height / 2)));

            float y = 0;

            List<float> yCoords = new List<float>() { 0 };

            for(int i = 0; i < xCoords.Count - 1; i++)
            {
                float start = xCoords[i];
                float end = xCoords[i + 1];

                if(i % 2 == 0)
                {
                    //float temp = y - (end - start) * slope;
                    float temp = y - (float)(Math.Round(((end - start) * Globals.scale) / 0.5) * 0.5) * slope;
                    yCoords.Add(temp);
                    y = temp;
                }
                else
                {
                    //float temp = y + (end - start) * rPerTot;
                    float temp = y + (float)(Math.Round(((end - start) * Globals.scale) / 0.5) * 0.5) * rPerTot;
                    yCoords.Add(temp);
                    y = temp;
                }
            }

            foreach(var c in yCoords)
            {
                Console.WriteLine(c);
            }
            Console.WriteLine();

            float maxY = yCoords.Max();
            float minY = yCoords.Min();

            float max = yCoords.Select(Math.Abs).Max();

            float offset = (displayPanels[0].Height / 2);

            float scale = (offset - 20) / max;

            yCoords = yCoords.Select(item => (item * scale) + offset).ToList();

            for(int i = 0; i < yCoords.Count - 1; i++)
            {
                Console.WriteLine("(" + xCoords[i] + ", " + yCoords[i] + ")");

                displayPanels[0].AddLine(new Tuple<PointF, PointF>(new PointF(xCoords[i], yCoords[i]), new PointF(xCoords[i + 1], yCoords[i + 1])));
            }

            sidePanel = sidePanels[0];

            label = new Label();
            label.Text = (-minY).ToString();
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.AutoSize = true;
            label.Location = new Point(10, (int)((minY * scale) + offset - label.Height / 2));
            label.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            label.ForeColor = Color.Black;

            sidePanel.Controls.Add(label);

            label = new Label();
            label.Text = (-maxY).ToString();
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.AutoSize = true;
            label.Location = new Point(10, (int)((maxY * scale) + offset - label.Height / 2));
            label.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            label.ForeColor = Color.Black;

            sidePanel.Controls.Add(label);
        }

        public DrawPanel GetPanel()
        {
            return this.panel;
        }

        

    }
}
