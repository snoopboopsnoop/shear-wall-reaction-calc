using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace workspace_test
{
    public partial class Form5 : Form
    {
        private string imgPath = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName, "src/");
        private int pageNum = 1;

        public Form5()
        {
            InitializeComponent();
            panel1.BackgroundImage = Image.FromFile($"{imgPath}Tutorial{pageNum}.png");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (pageNum < 6) pageNum++;
            panel1.BackgroundImage = Image.FromFile($"{imgPath}Tutorial{pageNum}.png");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (pageNum > 1) pageNum--;
            panel1.BackgroundImage = Image.FromFile($"{imgPath}Tutorial{pageNum}.png");
        }
    }
}
