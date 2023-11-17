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
        private string imgPath = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName, "src/");
        private RadioButton pointerButton;
        private RadioButton penButton;
        private MenuStrip menu = new MenuStrip ();

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

            Controls.Add(menu);
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("File");
            ToolStripMenuItem openMenu = new ToolStripMenuItem("Open...", null, new EventHandler(open_Click));
            ToolStripMenuItem saveAsMenu = new ToolStripMenuItem("Save As...", null, new EventHandler(saveAs_Click));

            fileMenu.ForeColor = Globals.fontColor;

            fileMenu.DropDownItems.Add(openMenu);
            fileMenu.DropDownItems.Add(saveAsMenu);

            menu.Items.Add(fileMenu);
            menu.BackColor = Color.FromArgb(1, 120, 120, 120);

            pointerButton = new RadioButton();
            pointerButton.Appearance = Appearance.Button;
            panel5.Controls.Add(pointerButton);
            pointerButton.BackgroundImage = Image.FromFile(imgPath + "pointerIcon.png");
            pointerButton.BackgroundImageLayout = ImageLayout.Stretch;

            penButton = new RadioButton();
            penButton.Appearance = Appearance.Button;
            panel5.Controls.Add(penButton);
            penButton.BackgroundImage = Image.FromFile(imgPath + "penIcon.png");
            penButton.BackgroundImageLayout = ImageLayout.Stretch;

            pointerButton.CheckedChanged += radioButton_CheckedChanged;
            penButton.CheckedChanged += radioButton_CheckedChanged;
            this.KeyDown += workspace_KeyDown;
            this.FormClosing += Form1_Closing;
            this.Resize += Form1_Resize;
        }

        private void saveAs_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Json files (*.json)|*.json|Text files (*.txt)|*.txt";
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                currentWorkspace.Save(saveDialog.FileName);
            }
        }

        private void open_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "Json files (*.json)|*.json|Text files (*.txt)|*.txt";
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                currentWorkspace.LoadData(openDialog.FileName);
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            pointerButton.Width = (int)Math.Round(panel5.Width * 0.8);
            pointerButton.Height = pointerButton.Width;
            pointerButton.Left = (panel5.Width - pointerButton.Width) / 2;
            pointerButton.Top = pointerButton.Left;

            penButton.Width = pointerButton.Width;
            penButton.Height = penButton.Width;
            penButton.Left = pointerButton.Left;
            penButton.Top = penButton.Left + pointerButton.Bottom;
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
                switch (selectedRb.Name)
                {
                    case "pointerButton":
                        currentWorkspace.SetPointerMode("select");
                        return;
                    case "penButton":
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
                pointerButton.Checked = true;
                currentWorkspace.SetPointerMode("select");
            }
            else if (e.KeyCode == Keys.P)
            {
                //penButton.Checked = true;
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
    }
}
