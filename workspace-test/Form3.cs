using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace workspace_test
{
    public partial class Form3 : Form
    {
        private DrawPanel workspace;
        private RectangleF rect;
        private int currentlySelected;

        public Form3()
        {
            InitializeComponent();
        }

        public Form3(DrawPanel paramWorkspace)
        {
            workspace = paramWorkspace;
            InitializeComponent();
            currentlySelected = workspace.GetCurrentlySelected();
            rect = workspace.GetRectangleFromIndex(currentlySelected);
            textBox3.Text = rect.Width.ToString();
            textBox4.Text = rect.Height.ToString();
        }

        //dimx box
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            try
            {
                rect.Width = float.Parse(textBox3.Text);
            }
            catch (FormatException)
            {
                return;
            }

            workspace.SetRectangle(rect, currentlySelected);
            
        }
        //dimy box
        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            try
            {
                rect.Height = float.Parse(textBox4.Text);
            }
            catch (FormatException)
            {
                return;
            }
            workspace.SetRectangle(rect, currentlySelected);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                float LA = float.Parse(textBox1.Text);
                float LD = float.Parse(textBox2.Text);
                ShearData data = workspace.Calculate(currentlySelected, LA, LD);
                ls.Text = data.LS.ToString();
                wx.Text = data.wx.ToString();
                wy.Text = data.wy.ToString();
            }
            catch (FormatException)
            {
                return;
            }
        }
    }
}
