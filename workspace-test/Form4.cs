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
    public partial class Form4 : Form
    {
        private DrawPanel workspace;
        private int magnitude;

        public Form4(object sender, int paramMag, string unit = "")
        {
            InitializeComponent();
            workspace = sender as DrawPanel;
            comboBox1.Items.Add("m");
            comboBox1.Items.Add("ft");
            comboBox1.Items.Add("football fields");
            comboBox1.SelectedIndex = 0;
            if(unit != "" && unit != null)
            {
                comboBox1.SelectedItem = unit.Remove(0, 1);
            }
            textBox1.KeyDown += input_KeyDown;
            magnitude = paramMag;
        }


        private void input_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyData == Keys.Enter)
            {
                Process();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Process();
        }

        private void Process()
        {
            label2.Text = "";
            try
            {
                workspace.SetScale(double.Parse(textBox1.Text)/magnitude, " " + comboBox1.SelectedItem.ToString());
                this.Close();
            }
            catch (FormatException)
            {
                label2.Text = "Error: unreadable value";
            }
        }
    }
}
