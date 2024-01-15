using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace workspace_test.Screens
{
    public partial class TreeScreen : Form
    {
        public TreeScreen()
        {
            InitializeComponent();
            TreeNode project = treeView1.Nodes.Add("Project: Untitled");
            TreeNode building = treeView1.Nodes.Add("Floors");
            building.Nodes.Add("Floor 1");
            building.Nodes.Add("Floor 2");
            building.Nodes.Add("Floor 3");
        }
    }
}
