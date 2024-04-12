using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using workspace_test.Screens;
using Word = Microsoft.Office.Interop.Word;

namespace workspace_test
{
    public partial class Main : Form
    {
        private List<DrawPanel> workspaces = new List<DrawPanel> ();
        private DrawPanel currentWorkspace;
        private TabControl tabPages;
        private List<TabPage> tabs = new List<TabPage>();
        private RadioButton selectedRb;
        private float opacity = 1.0F;

        private RadioButton pointerButton;
        private RadioButton penButton;
        private Button helpButton;
        private Image background;

        private MenuStrip menu = new MenuStrip ();

        private TreeScreen fs;

        private string loadedFile = "";

        private Project project;

        public Main()
        {
            this.Font = SystemFonts.MessageBoxFont;
            this.ForeColor = Globals.fontColor;
            InitializeComponent();

            Building building = new Building();
            Floor firstFloor = building.AddFloor();

            project = new Project("Untitled", building);

            tabPages = new TabControl();
            tabPages.Dock = DockStyle.Fill;
            tabPages.Padding = new Point(0, 0);
            tabPages.Margin = new Padding(0, 10, 0, 0);

            TabPage firstPage = new TabPage("Floor 1");
            firstPage.Name = firstPage.Text;
            tabs.Add(firstPage);

            tabs[0].Dock = DockStyle.Fill;
            tabs[0].BorderStyle = BorderStyle.FixedSingle;
            tabs[0].Margin = new Padding(0, 0, 0, 0);

            tableLayoutPanel2.Controls.Add(tabPages, 1, 0);
            tabPages.Controls.AddRange(new Control[] { tabs[0] });

            workspaces.Add(new DrawPanel(firstFloor));

            tabs[0].Controls.Add(workspaces[0]);
            tabs[0].BackgroundImageLayout = ImageLayout.Stretch;

            currentWorkspace = workspaces[0];
            currentWorkspace.BackgroundImageLayout = ImageLayout.Center;
            background = null;

            trackBar1.Value = trackBar1.Maximum;

            ToolTip toolTip1 = new ToolTip();

            // Set up the delays for the ToolTip.
            toolTip1.InitialDelay = 500;
            toolTip1.ReshowDelay = 250;
            // Force the ToolTip text to be displayed whether or not the form is active.
            toolTip1.ShowAlways = true;

            Controls.Add(menu);
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("File");
            ToolStripMenuItem openMenu = new ToolStripMenuItem("Open...", Properties.Resources.openIcon, new EventHandler(open_Click));
            ToolStripMenuItem saveMenu = new ToolStripMenuItem("Save", Properties.Resources.saveIcon, new EventHandler(saveAs_Click));
            ToolStripMenuItem saveAsMenu = new ToolStripMenuItem("Save As...", Properties.Resources.saveAsIcon, new EventHandler(saveAs_Click));

            fileMenu.DropDownItems.Add(openMenu);
            fileMenu.DropDownItems.Add(saveMenu);
            fileMenu.DropDownItems.Add(saveAsMenu);

            ToolStripMenuItem imgMenu = new ToolStripMenuItem("Image");
            ToolStripMenuItem openImgMenu = new ToolStripMenuItem("Load Image...", null, new EventHandler(imgLoad_Click));
            ToolStripMenuItem clearImgMenu = new ToolStripMenuItem("Clear Image", null, new EventHandler(imgClear_Click));

            imgMenu.DropDownItems.Add(openImgMenu);
            imgMenu.DropDownItems.Add(clearImgMenu);

            ToolStripMenuItem viewMenu = new ToolStripMenuItem("View");
            ToolStripMenuItem openProjView = new ToolStripMenuItem("Project View", null, new EventHandler(projView_Click));

            viewMenu.DropDownItems.Add(openProjView);

            ToolStripMenuItem projMenu = new ToolStripMenuItem("Project");
            ToolStripMenuItem addFloorMenu = new ToolStripMenuItem("Add Floor", null, new EventHandler(addFloor_Click));
            ToolStripMenuItem levelAccelMenu = new ToolStripMenuItem("Calculate Level Accel.", null, new EventHandler(levelAccel_Click));

            projMenu.DropDownItems.Add(addFloorMenu);
            projMenu.DropDownItems.Add(levelAccelMenu);

            fileMenu.ForeColor = Globals.fontColor;
            imgMenu.ForeColor = Globals.fontColor;
            viewMenu.ForeColor = Globals.fontColor;
            projMenu.ForeColor = Globals.fontColor;

            menu.Items.Add(fileMenu);
            menu.Items.Add(viewMenu);
            menu.Items.Add(projMenu);
            menu.Items.Add(imgMenu);
            menu.BackColor = Color.FromArgb(255, 120, 120, 120);

            pointerButton = new RadioButton();
            pointerButton.Name = "pointerButton";
            pointerButton.Appearance = Appearance.Button;
            panel5.Controls.Add(pointerButton);
            pointerButton.BackgroundImage = Properties.Resources.pointerIcon;
            pointerButton.BackgroundImageLayout = ImageLayout.Stretch;
            toolTip1.SetToolTip(pointerButton, "Select Tool");

            penButton = new RadioButton();
            penButton.Name = "penButton";
            penButton.Appearance = Appearance.Button;
            panel5.Controls.Add(penButton);
            penButton.BackgroundImage = Properties.Resources.penIcon;
            penButton.BackgroundImageLayout = ImageLayout.Stretch;
            toolTip1.SetToolTip(penButton, "Pen Tool");

            pointerButton.CheckedChanged += radioButton_CheckedChanged;
            penButton.CheckedChanged += radioButton_CheckedChanged;

            helpButton = new Button();
            panel5.Controls.Add(helpButton);
            helpButton.BackgroundImage = Properties.Resources.helpIcon;
            helpButton.BackgroundImageLayout = ImageLayout.Stretch;
            helpButton.Click += help_Click;

            fileMenu.DropDownOpening += menu_Opening;
            fileMenu.DropDownClosed += menu_Closing;
            imgMenu.DropDownOpening += menu_Opening;
            imgMenu.DropDownClosed += menu_Closing;
            this.KeyDown += workspace_KeyDown;
            this.FormClosing += Form1_Closing;
            this.Resize += Form1_Resize;

            this.ActiveControl = workspaces[0];
            this.Click += on_Click;

            fs = new TreeScreen(project);
            fs.Text = "Project View";

            tabPages.SelectedIndexChanged += tabs_SelectedIndexChanged;

            Globals.main = this;
        }

        private void on_Click(object sender, EventArgs e)
        {
            this.ActiveControl = workspaces[0];
        }


        private void help_Click(object sender, EventArgs e)
        {
            TutorialScreen tutorial = new TutorialScreen();
            tutorial.ShowDialog();
        }

        private void saveAs_Click(object sender, EventArgs e)
        {
            save();
        }

        private void addFloor_Click(object sender, EventArgs e)
        {
            Floor newFloor = project.GetBuilding().AddFloor();
            fs.UpdateView();
            TabPage newPage = new TabPage(newFloor.GetName());
            newPage.Name = newPage.Text;
            newPage.Dock = DockStyle.Fill;
            newPage.BorderStyle = BorderStyle.FixedSingle;
            newPage.Margin = new Padding(0, 0, 0, 0);
            newPage.Controls.Add(new DrawPanel(newFloor));
            //tabs.Insert(tabs.Count - 2, new TabPage(newFloor.GetName()));
            tabPages.TabPages.Insert(tabPages.TabPages.Count, newPage);
            tabPages.SelectedTab = tabPages.TabPages[tabPages.TabPages.Count - 2];
            //currentWorkspace = tabPages.SelectedTab.Controls[0] as DrawPanel;
        }

        private void projView_Click(object sender, EventArgs e)
        {
            try
            {
                fs.Show();
            }
            catch(ObjectDisposedException)
            {
                fs = new TreeScreen(project);
                fs.Show();
            }
        }
        private void levelAccel_Click(object sender, EventArgs e)
        {
            LevelAccel laScreen = new LevelAccel(project.GetBuilding());
            laScreen.ShowDialog();
            UpdateLA();
        }
        private void menu_Opening(object sender, EventArgs e)
        {
            ToolStripMenuItem temp = sender as ToolStripMenuItem;
            if (temp.Text == "File")
            {
                if (currentWorkspace.GetLoadedFile() != "")
                {
                    temp.DropDownItems[1].Enabled = true;
                }
                else temp.DropDownItems[1].Enabled = false;
            }
            temp.ForeColor = Color.FromArgb(255, 48, 48, 48);
        }

        private void menu_Closing(object sender, EventArgs e)
        {
            (sender as ToolStripMenuItem).ForeColor = Globals.fontColor;
        }

        private void tabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            Console.WriteLine("changed tab");
            // sometimes selectedtab will go null when clearing it (temp?>)
            if(tabPages.SelectedTab != null)
            {
                if (tabPages.SelectedTab.Text.StartsWith("Floor"))
                {
                    currentWorkspace = tabPages.SelectedTab.Controls[0] as DrawPanel;
                    UpdateLA();
                }
            }
        
            
        }

        private void UpdateLA()
        {
            int floor = int.Parse(tabPages.SelectedTab.Text.Split(' ')[1]) - 1;
            textBox1.Text = project.GetBuilding().GetFloors()[floor].GetLA().ToString("0.00");
        }

        private void save()
        {
            Console.WriteLine("loaded file: " + currentWorkspace.GetLoadedFile());
            //if (currentWorkspace.GetLoadedFile() != "")
            //{
            //    this.Cursor = Cursors.WaitCursor;
            //    System.Threading.Thread.Sleep(50);
            //    currentWorkspace.Save("");
            //    this.Cursor = Cursors.Default;
            //}
            //else
            //{
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Json files (*.json)|*.json|Text files (*.txt)|*.txt";
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    this.Cursor = Cursors.WaitCursor;
                    if (saveDialog.FileName == "") saveDialog.FileName = loadedFile;

                    foreach (DrawPanel workspace in workspaces)
                    {
                        workspace.Save();  
                    }

                Console.WriteLine("liens: " + project.GetBuilding().GetFloors()[0].GetLines().Count);

                    JsonSerializer serializer = new JsonSerializer();
                    serializer.TypeNameHandling = TypeNameHandling.Auto;
                    serializer.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;

                    using (StreamWriter sw = new StreamWriter(saveDialog.FileName))
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        serializer.Serialize(writer, project);
                    }
                    this.Cursor = Cursors.Default;
                //}
            }
            
        }

        public void Open(string tabName)
        {
            Console.WriteLine("name: " + tabName);
            int index = tabPages.TabPages.IndexOfKey(tabName);
            foreach(TabPage page in tabPages.TabPages)
            {
                Console.WriteLine("tab name: " + page.Name);
            }
            tabPages.SelectedTab = tabPages.TabPages[index];
            currentWorkspace = tabPages.SelectedTab.Controls[0] as DrawPanel;
        }

        public void LoadData(string path)
        {
            using (StreamReader file = File.OpenText(path))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.TypeNameHandling = TypeNameHandling.Auto;
                serializer.NullValueHandling = NullValueHandling.Ignore;

                project = (Project)serializer.Deserialize(file, typeof(Project));

                Globals.refMeasure = project.GetRefMeasure();
                Globals.scale = project.GetScale();
                //if (project.GetBuilding().GetFloors()[0].GetDocPath() != "")
                //{
                //    try
                //    {
                //        if (Globals.word.Visible == false) Globals.word.Visible = true;
                //    }
                //    catch (Exception)
                //    {
                //        Globals.word = new Word.Application();
                //        Globals.word.Visible = true;
                //    }
                //    Globals.doc = Globals.word.Documents.Open(input.project.GetBuilding().GetFloors()[0].GetDocPath());
                //}

                workspaces.Clear();
                tabPages.TabPages.Clear();
                foreach (Floor floor in project.GetBuilding().GetFloors())
                {
                    TabPage newPage = new TabPage(floor.GetName());

                    newPage.Name = newPage.Text;
                    newPage.Dock = DockStyle.Fill;
                    newPage.BorderStyle = BorderStyle.FixedSingle;
                    newPage.Margin = new Padding(0, 0, 0, 0);
                    newPage.Controls.Add(new DrawPanel(floor));

                    tabPages.TabPages.Insert(tabPages.TabPages.Count, newPage);
                }
                //tabPages.SelectedTab = tabPages.TabPages[tabPages.TabPages.Count - 2];
            }
            UpdateLA();
            currentWorkspace = tabPages.SelectedTab.Controls[0] as DrawPanel;
            loadedFile = path;
        }

        private void open_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "Json files (*.json)|*.json|Text files (*.txt)|*.txt";
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                LoadData(openDialog.FileName);
                this.Text = Path.GetFileName(openDialog.FileName.TrimEnd(Path.DirectorySeparatorChar));
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

            helpButton.Width = pointerButton.Width;
            helpButton.Height = helpButton.Width;
            helpButton.Left = penButton.Left;
            helpButton.Top = panel5.Height - 10 - helpButton.Height;
        }

        private void Form1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Console.WriteLine("closing word");
            Globals.CloseWord();
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
            //System.Console.WriteLine(e.KeyCode);
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
                //Console.WriteLine("delete");
                currentWorkspace.deleteSelected();
            }
            else if(e.KeyCode == Keys.V)
            {
                pointerButton.Checked = true;
                //penButton.Checked = false;
                currentWorkspace.SetPointerMode("select");
            }
            else if (e.KeyCode == Keys.P)
            {
                penButton.Checked = true;
                //pointerButton.Checked = false;
                currentWorkspace.SetPointerMode("pen");
            }
            else if(e.KeyCode == Keys.S && (ModifierKeys & Keys.Control) == Keys.Control)
            {
                save();
            }
        }

        private void imgLoad_Click(object sender, EventArgs e)
        {
            if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                background = ResizeImage(openFileDialog1.FileName, new Size(1920, 1080));
                currentWorkspace.BackgroundImage = background;
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

        private void imgClear_Click(object sender, EventArgs e)
        {
            background = null;
            currentWorkspace.BackgroundImage = background;
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
            Console.WriteLine("set LD");
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            opacity = trackBar1.Value / 10.0F;
            label3.Text = "Opacity: " + opacity;
            
            //Console.WriteLine( "\"" + openFileDialog1.FileName + "\"");
            if (background != null)
            {
                currentWorkspace.BackgroundImage = SetImageOpacity(background, opacity);
            }
        }

        private Image ResizeImage(string path, Size size)
        {

            Console.WriteLine($"width: {size.Width}, height: {size.Height}");

            Image img = Image.FromFile(path);
            Bitmap src = new Bitmap(img);

            int sourceWidth = src.Width;
            int sourceHeight = src.Height;

            float nPercentW = (float)size.Width / (float)sourceWidth;
            float nPercentH = (float)size.Height / (float)sourceHeight;

            Console.WriteLine($"sourcew: {sourceWidth}, sourceheight: {sourceHeight}");

            Console.WriteLine($"percentw: {nPercentW}, percentH: {nPercentH}");

            float nPercent = Math.Min(nPercentW, nPercentH);

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Console.WriteLine($"destwidth: {destWidth}, destHeight: {destHeight}");

            Bitmap b = new Bitmap(destWidth, destHeight);
            Graphics g = Graphics.FromImage(b);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(src, 0, 0, destWidth, destHeight);
            g.Dispose();

            b.Save(Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName, "src/tempImg/temp.jpg"));

            return b;
        }
    }
}
