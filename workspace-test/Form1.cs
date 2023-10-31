using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace workspace_test
{
    public partial class Form1 : Form
    {
        private List<DrawPanel> workspaces = new List<DrawPanel> ();
        private DrawPanel currentWorkspace;
        private TabControl tabPages;
        private List<TabPage> tabs = new List<TabPage>();
        private RadioButton selectedRb;
        private float opacity = 0.0F;
        private string logoPath = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName, "src/gorillahat.jpg");
        private PictureBox logoBox = new PictureBox();

        public Form1()
        {
            InitializeComponent();
            tabPages = new TabControl();
            tabPages.Dock = DockStyle.Fill;
            tabPages.Padding = new Point(0, 0);
            tabPages.Margin = new Padding(0, 0, 0, 0);
            tabs.Add(new TabPage("Untitled"));
            tabs[0].Dock = DockStyle.Fill;
            tabs[0].BorderStyle = BorderStyle.FixedSingle;
            tabs[0].Margin = new Padding(0, 0, 0, 0);
            tableLayoutPanel2.Controls.Add(tabPages, 1, 0);
            tabPages.Controls.AddRange(new Control[] { tabs[0] });
            workspaces.Add(new DrawPanel());
            tabs[0].Controls.Add(workspaces[0]);
            currentWorkspace = workspaces[0];

            logoBox.SizeMode = PictureBoxSizeMode.StretchImage;
            logoBox.Image = Image.FromFile(logoPath);
            logoBox.Anchor = AnchorStyles.None;
            logoBox.Dock = DockStyle.None;

            flowLayoutPanel1.Controls.Add(logoBox);

            radioButton1.Left = (flowLayoutPanel3.Width - radioButton1.Width) / 2;
            radioButton3.Left = (flowLayoutPanel3.Width - radioButton3.Width) / 2;

            pictureBox1.Top = (flowLayoutPanel1.Height - pictureBox1.Height) / 2;
            pictureBox1.Margin = new Padding(pictureBox1.Top, 0, 0, 0);

            radioButton1.CheckedChanged += radioButton_CheckedChanged;
            radioButton3.CheckedChanged += radioButton_CheckedChanged; 
            this.KeyDown += workspace_KeyDown;
            this.FormClosing += Form1_Closing;
            this.Resize += Form1_Resize;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            logoBox.Height = flowLayoutPanel1.Height - 20;
            Console.WriteLine("panel height: " + flowLayoutPanel1.Height);
            Console.WriteLine("image height: " + logoBox.Height);
            logoBox.Width = logoBox.Height;
            logoBox.Top = (flowLayoutPanel1.Height - logoBox.Height) / 2;
            Console.WriteLine("math: " + ((flowLayoutPanel1.Height - logoBox.Height) / 2));
            Console.WriteLine("top: " + logoBox.Top);
            logoBox.Left = logoBox.Top;
        }

        private void Form1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Console.WriteLine("closing word");
            //workspace.CloseWord();
        }

        public DrawPanel GetWorkspace()
        {
            return currentWorkspace;
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
                        currentWorkspace.SetPointerMode("select");
                        return;
                    case "Rectangle Tool":
                        currentWorkspace.SetPointerMode("rectangle");
                        break;
                    case "Pen Tool":
                        currentWorkspace.SetPointerMode("pen");
                        break;
                    default:
                        currentWorkspace.SetPointerMode("select");
                        break;
                }
            }
        }

        // this kind of works but it sucks
        private void workspace_KeyDown(object sender, KeyEventArgs e)
        {
            System.Console.WriteLine(e.KeyCode);
            if (currentWorkspace.GetCurrentlySelected() != -1)
            {
                if (e.KeyCode == Keys.W)
                {
                    //System.Console.WriteLine("w pressed");
                    currentWorkspace.moveSelected(new Point(0, -1));
                }
                else if (e.KeyCode == Keys.A)
                {
                    //System.Console.WriteLine("a pressed");
                    currentWorkspace.moveSelected(new Point(-1, 0));
                }
                else if (e.KeyCode == Keys.S)
                {
                    //System.Console.WriteLine("a pressed");
                    currentWorkspace.moveSelected(new Point(0, 1));
                }
                else if (e.KeyCode == Keys.D)
                {
                    //System.Console.WriteLine("a pressed");
                    currentWorkspace.moveSelected(new Point(1, 0));
                }
            }
            if (e.KeyCode == Keys.Delete)
            {
                Console.WriteLine("delete");
                currentWorkspace.deleteSelected();
            }
            else if(e.KeyCode == Keys.V)
            {
                radioButton1.Checked = true;
                currentWorkspace.SetPointerMode("select");
            }
            else if (e.KeyCode == Keys.P)
            {
                radioButton3.Checked = true;
                currentWorkspace.SetPointerMode("pen");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                currentWorkspace.BackgroundImage = SetImageOpacity(Image.FromFile(openFileDialog1.FileName), opacity);
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
            currentWorkspace.BackgroundImage = null;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                currentWorkspace.SetLA(float.Parse(textBox1.Text));
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
                currentWorkspace.SetLD(float.Parse(textBox2.Text));
            }
            catch (FormatException)
            {
                return;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            currentWorkspace.Export();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            label3.Text = "Opacity: " + trackBar1.Value / 10.0;
            opacity = trackBar1.Value / 10.0F;
            //Console.WriteLine( "\"" + openFileDialog1.FileName + "\"");
            if (openFileDialog1.FileName != "openFileDialog1")
            {
                currentWorkspace.BackgroundImage = SetImageOpacity(Image.FromFile(openFileDialog1.FileName), opacity);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Json files (*.json)|*.json|Text files (*.txt)|*.txt";
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                currentWorkspace.Save(saveDialog.FileName);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "Json files (*.json)|*.json|Text files (*.txt)|*.txt";
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                currentWorkspace.LoadData(openDialog.FileName);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            currentWorkspace.clearShear();
        }
    }
}
