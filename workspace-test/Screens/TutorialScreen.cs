using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace workspace_test
{
    public partial class TutorialScreen : Form
    {
        private int pageNum = 0;
        private List<Image> tutorials = new List<Image>() { 
            Properties.Resources.Tutorial1,
            Properties.Resources.Tutorial2,
            Properties.Resources.Tutorial3,
            Properties.Resources.Tutorial4,
            Properties.Resources.Tutorial5,
            Properties.Resources.Tutorial6
        };

        public TutorialScreen()
        {
            InitializeComponent();
            panel1.BackgroundImage = tutorials[pageNum];
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (pageNum < tutorials.Count - 1) pageNum++;
            panel1.BackgroundImage = tutorials[pageNum];
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (pageNum > 0) pageNum--;
            panel1.BackgroundImage = tutorials[pageNum];
        }
    }
}
