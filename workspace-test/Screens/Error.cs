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
    public partial class Error : Form
    {
        public Error()
        {
            InitializeComponent();
        }

        public Error(string errorText)
        {
            InitializeComponent();
            label1.Text = errorText;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
