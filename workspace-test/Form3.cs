using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace workspace_test
{
    public partial class Form3 : Form
    {
        private float wAdd = 0;
        private float HP = 0;
        private float HA = 0;
        private float HB = 0;
        private float HS = 0;
        private float WW = 0;
        private float LD = 0;

        public Form3()
        {
            InitializeComponent();
        }

        public Form3(float paramLD)
        {
            InitializeComponent();
            LD = paramLD;
            textBox1.Text = HP.ToString();
            textBox2.Text = HA.ToString();
            textBox3.Text = HB.ToString();
            textBox4.Text = HS.ToString();
            textBox5.Text = WW.ToString();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                HP = float.Parse(textBox1.Text);
            }
            catch(FormatException)
            {

            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            try
            {
                HA = float.Parse(textBox2.Text);
            }
            catch (FormatException)
            {

            }
        }
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            try
            {
                HB = float.Parse(textBox3.Text);
            }
            catch (FormatException)
            {

            }
        }
        //dimy box
        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            try
            {
                HS = float.Parse(textBox4.Text);
            }
            catch (FormatException)
            {

            }
        }
        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            try
            {
                WW = float.Parse(textBox5.Text);
            }
            catch (FormatException)
            {

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            wAdd = LD * WW * (0.5F * (HA + HB) + HP + HS);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public float GetWAdd()
        {
            return wAdd;
        }
    }
}
