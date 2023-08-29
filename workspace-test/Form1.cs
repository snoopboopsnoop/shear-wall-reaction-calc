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
    public partial class Form1 : Form
    {
        private DrawPanel workspace;
        private RadioButton selectedRb;

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
            System.Console.WriteLine("key");
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
        }
    }
}
