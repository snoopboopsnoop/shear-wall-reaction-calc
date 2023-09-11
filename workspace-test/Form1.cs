using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace workspace_test
{
    public partial class Form1 : Form
    {
        private DrawPanel workspace;
        private RadioButton selectedRb;
        private float opacity = 0.0F;

        public Form1()
        {
            InitializeComponent();
            tableLayoutPanel1.Controls.Add(new DrawPanel("workspace"), 0, 1);
            workspace = tableLayoutPanel1.Controls["workspace"] as DrawPanel;
            radioButton1.CheckedChanged += radioButton_CheckedChanged;
            radioButton2.CheckedChanged += radioButton_CheckedChanged;
            radioButton3.CheckedChanged += radioButton_CheckedChanged; 
            this.KeyDown += workspace_KeyDown;
        }

        void radioButton_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if(rb == null)
            {
                return;
            }
            if(rb.Checked)
            {
                selectedRb = rb;
                switch (selectedRb.Text)
                {
                    case "Pointer":
                        workspace.SetPointerMode("select");
                        return;
                    case "Rectangle Tool":
                        workspace.SetPointerMode("rectangle");
                        break;
                    case "Pen Tool":
                        workspace.SetPointerMode("pen");
                        break;
                    default:
                        workspace.SetPointerMode("select");
                        break;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(workspace.GetCurrentlySelected() < 0)
            {
                Form2 error = new Form2();
                error.ShowDialog();
            }
            else
            {
                Form3 analysis = new Form3(workspace);
                analysis.Show();
            }
        }

        // this kind of works but it sucks
        private void workspace_KeyDown(object sender, KeyEventArgs e)
        {
            System.Console.WriteLine(e.KeyCode);
            if (workspace.GetCurrentlySelected() != -1)
            {
                if (e.KeyCode == Keys.W)
                {
                    //System.Console.WriteLine("w pressed");
                    workspace.moveSelected(new Point(0, -1));
                }
                else if (e.KeyCode == Keys.A)
                {
                    //System.Console.WriteLine("a pressed");
                    workspace.moveSelected(new Point(-1, 0));
                }
                else if (e.KeyCode == Keys.S)
                {
                    //System.Console.WriteLine("a pressed");
                    workspace.moveSelected(new Point(0, 1));
                }
                else if (e.KeyCode == Keys.D)
                {
                    //System.Console.WriteLine("a pressed");
                    workspace.moveSelected(new Point(1, 0));
                }
            }
            if (e.KeyCode == Keys.Delete)
            {
                Console.WriteLine("delete");
                workspace.deleteSelected();
            }
            else if(e.KeyCode == Keys.V)
            {
                radioButton1.Checked = true;
                workspace.SetPointerMode("select");
            }
            else if (e.KeyCode == Keys.R)
            {
                radioButton2.Checked = true;
                workspace.SetPointerMode("rectangle");
            }
            else if (e.KeyCode == Keys.P)
            {
                radioButton3.Checked = true;
                workspace.SetPointerMode("pen");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                workspace.BackgroundImage = SetImageOpacity(Image.FromFile(openFileDialog1.FileName), opacity);
            }
        }

        public Image SetImageOpacity(Image image, float opacity)
        {
            Bitmap bmp = new Bitmap(image.Width, image.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                ColorMatrix matrix = new ColorMatrix();
                matrix.Matrix33 = opacity;
                ImageAttributes attributes = new ImageAttributes();
                attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default,
                                                  ColorAdjustType.Bitmap);
                g.DrawImage(image, new Rectangle(0, 0, bmp.Width, bmp.Height),
                                   0, 0, image.Width, image.Height,
                                   GraphicsUnit.Pixel, attributes);
            }
            return bmp;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            workspace.BackgroundImage = null;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                workspace.SetLA(float.Parse(textBox1.Text));
            }
            catch(FormatException)
            {
                return;
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            try
            {
                workspace.SetLD(float.Parse(textBox2.Text));
            }
            catch (FormatException)
            {
                return;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            workspace.Export();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            label3.Text = "Opacity: " + trackBar1.Value / 10.0;
            opacity = trackBar1.Value / 10.0F;
            if (openFileDialog1.FileName != null)
            {
                workspace.BackgroundImage = SetImageOpacity(Image.FromFile(openFileDialog1.FileName), opacity);
            }
        }
    }
}
