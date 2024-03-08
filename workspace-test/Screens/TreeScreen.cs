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
        Project linkedProject;
        TreeNode floors;

        public TreeScreen()
        {
            InitializeComponent();
            treeView1.NodeMouseDoubleClick += treeView_NodeMouseDoubleClick;
        }
        public TreeScreen(Project project) : this()
        {
            linkedProject = project;
            treeView1.Nodes.Add("Project: Untitled");
            floors = treeView1.Nodes.Add(project.GetBuilding().GetName());
            floors.Expand();
            foreach(Floor floor in project.GetBuilding().GetFloors())
            {
                floors.Nodes.Add(floor.GetName());
            }
        }

        public void UpdateView()
        {
            floors.Nodes.Clear();
            foreach (Floor floor in linkedProject.GetBuilding().GetFloors())
            {
                floors.Nodes.Add(floor.GetName());
            }
        }

        void treeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            Console.WriteLine("pressed " + e.Node.Text);
            if(e.Node.Nodes.Count == 0)
            {
                Globals.main.Open(e.Node.Text);
            }
        }
    }
}
