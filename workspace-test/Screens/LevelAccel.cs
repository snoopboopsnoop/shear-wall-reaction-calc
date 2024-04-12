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

    public partial class LevelAccel : Form
    {
        Building building;
        private LAVals vals;

        private double Cs;
        private double V;
        private double SumWeight;

        private int selectedIndex = 0;

        public LevelAccel(Building building)
        {
            InitializeComponent();

            this.building = building;
            vals = building.GetVals();

            kBox.Text = vals.k.ToString();
            SDSBox.Text = vals.SDS.ToString();
            RBox.Text = vals.R.ToString();
            IBox.Text = vals.I.ToString();

            sumBox.Text = "0.00";
            sumWeightBox.Text = "0.00";

            listView1.View = View.Details;
            listView1.FullRowSelect = true;
            listView1.GridLines = true;

            listView1.SelectedIndexChanged += listView1_SelectedIndexChanged;

            foreach(Floor floor in building.GetFloors())
            {
                ListViewItem temp = new ListViewItem(floor.GetName());
                temp.Tag = floor;
                temp.SubItems.Add(floor.GetWeight().ToString("0.00"));
                temp.SubItems.Add(floor.GetHeight().ToString("0.00"));
                temp.SubItems.Add((floor.GetWeight() * Math.Pow(floor.GetHeight(), vals.k)).ToString("0.00"));
                temp.SubItems.Add("0.00");
                temp.SubItems.Add("0.00");
                temp.SubItems.Add("0.00");

                listView1.Items.Add(temp);
            }

            weightBox.Text = ((Floor)listView1.Items[0].Tag).GetWeight().ToString("0.00");
            heightBox.Text = ((Floor)listView1.Items[0].Tag).GetHeight().ToString("0.00");

            listView1.Columns.Add("Level", -2, HorizontalAlignment.Left);
            listView1.Columns.Add("Weight, w", -2, HorizontalAlignment.Center);
            listView1.Columns.Add("Elevation, h", -2, HorizontalAlignment.Center);
            listView1.Columns.Add("w * h^k", -2, HorizontalAlignment.Center);
            listView1.Columns.Add("Cvx = w * h^k / sum(w*h^k)", -2, HorizontalAlignment.Center);
            listView1.Columns.Add("Fx = Cvx * V", -2, HorizontalAlignment.Center);
            listView1.Columns.Add("LA = Fx/w", -2, HorizontalAlignment.Center);

            UpdateListView();

            this.FormClosing += new FormClosingEventHandler(Form_Closing);
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(listView1.SelectedIndices.Count > 0)
            {
                if(selectedIndex != listView1.SelectedIndices[0])
                {
                    selectedIndex = listView1.SelectedIndices[0];
                    groupBox3.Text = listView1.SelectedItems[0].Text + $" ({selectedIndex})";
                    weightBox.Text = listView1.SelectedItems[0].SubItems[1].Text.Split(' ')[0];
                    heightBox.Text = listView1.SelectedItems[0].SubItems[2].Text.Split(' ')[0];
                }
            }
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            ErrorLabel.Text = "";
            try {
                ((Floor)listView1.Items[selectedIndex].Tag).SetWeight(double.Parse(weightBox.Text));
                ((Floor)listView1.Items[selectedIndex].Tag).SetHeight(double.Parse(heightBox.Text));
                UpdateListView();
            }
            catch(Exception ex)
            {
                ErrorLabel.Text = ex.Message;
            }
        }

        private void UpdateListView()
        {
            foreach (ListViewItem item in listView1.Items)
            {
                item.SubItems[1].Text = ((Floor)item.Tag).GetWeight().ToString("0.00") + " k";
                item.SubItems[2].Text = ((Floor)item.Tag).GetHeight().ToString("0.00") + " ft";
                item.SubItems[3].Text = (double.Parse(item.SubItems[1].Text.Split(' ')[0]) * Math.Pow(double.Parse(item.SubItems[2].Text.Split(' ')[0]), vals.k)).ToString("0.00") + " k-ft";
                
            }

            UpdateSumWeight();
            UpdateSumBox();

            foreach (ListViewItem item in listView1.Items)
            {
                item.SubItems[4].Text = (double.Parse(item.SubItems[3].Text.Split(' ')[0]) / double.Parse(sumBox.Text.Split(' ')[0])).ToString("0.00");
                item.SubItems[5].Text = (double.Parse(item.SubItems[4].Text) * V).ToString("0.00") + " k";
                item.SubItems[6].Text = (double.Parse(item.SubItems[5].Text.Split(' ')[0]) / double.Parse(item.SubItems[1].Text.Split(' ')[0])).ToString("0.00") + " g";
            }

            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void SDSBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                this.vals.SDS = double.Parse(SDSBox.Text);
                UpdateCs();
            }
            catch (Exception)
            {

            }
        }

        private void RBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                this.vals.R = double.Parse(RBox.Text);
                UpdateCs();
            }
            catch (Exception)
            {

            }
        }

        private void IBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                this.vals.I = double.Parse(IBox.Text);
                UpdateCs();

            }
            catch (Exception)
            {

            }
        }

        private void UpdateCs()
        {
            if (vals.R == 0 || vals.I == 0)
            {
                Cs = 0;
            }
            else Cs = vals.SDS / (vals.R / vals.I);
            CsBox.Text = Cs.ToString("0.00");

            UpdateV();
        }

        private void UpdateV()
        {
            this.V = Cs * SumWeight;
            VBox.Text = V.ToString("0.00") + " k";
        }

        private void UpdateSumWeight()
        {
            SumWeight = 0;
            foreach (ListViewItem item in listView1.Items)
            {
                SumWeight += double.Parse(item.SubItems[1].Text.Split(' ')[0]);
            }

            sumWeightBox.Text = SumWeight.ToString("0.00") + " k";
            UpdateV();
        }

        private void UpdateSumBox()
        {
            double temp = 0;
            foreach (ListViewItem item in listView1.Items)
            {
                temp += double.Parse(item.SubItems[3].Text.Split(' ')[0]);
            }

            sumBox.Text = temp.ToString("0.00") + " k-ft";
        }

        private void kApplyButton_Click(object sender, EventArgs e)
        {
            try
            {
                vals.k = double.Parse(kBox.Text);
                UpdateListView();
            }
            catch(Exception)
            {
                Error errorForm = new Error("Error: Invalid k value");
            }
        }

        private void Form_Closing(Object sender, FormClosingEventArgs e)
        {
            building.SetVals(vals);
            foreach(ListViewItem item in listView1.Items)
            {
                ((Floor)item.Tag).SetLA(float.Parse(item.SubItems[6].Text.Split(' ')[0]));
            }
        }
    }
}
