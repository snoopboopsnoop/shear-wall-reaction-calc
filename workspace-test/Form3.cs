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
        private float HP1 = 0;
        private float HA1 = 0;
        private float HB1 = 0;
        private float HS1 = 0;
        private float WW1 = 0;
        private float LD = 0;

        private float HP2 = 0;
        private float HA2 = 0;
        private float HB2 = 0;
        private float HS2 = 0;
        private float WW2 = 0;

        private bool same = false;

        public Form3()
        {
            InitializeComponent();
        }

        public Form3(float paramLD)
        {
            InitializeComponent();
            LD = paramLD;
            textBox1.Text = HP1.ToString();
            textBox2.Text = HA1.ToString();
            textBox3.Text = HB1.ToString();
            textBox4.Text = HS1.ToString();
            textBox5.Text = WW1.ToString();

            textBox6.Text = HP2.ToString();
            textBox7.Text = HA2.ToString();
            textBox8.Text = HB2.ToString();
            textBox9.Text = HS2.ToString();
            textBox10.Text = WW2.ToString();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                HP1 = float.Parse(textBox1.Text);
                if (same) {
                    HP2 = HP1;
                    textBox6.Text = HP2.ToString();
                }
            }
            catch(FormatException)
            {

            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            try
            {
                HA1 = float.Parse(textBox2.Text);
                if (same)
                {
                    HA2 = HA1;
                    textBox7.Text = HA2.ToString();
                }
            }
            catch (FormatException)
            {

            }
        }
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            try
            {
                HB1 = float.Parse(textBox3.Text);
                if (same)
                {
                    HB2 = HB1;
                    textBox8.Text = HB2.ToString();
                }
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
                HS1 = float.Parse(textBox4.Text);
                if (same)
                {
                    HS2 = HS1;
                    textBox9.Text = HS2.ToString();
                }
            }
            catch (FormatException)
            {

            }
        }
        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            try
            {
                WW1 = float.Parse(textBox5.Text);
                if (same)
                {
                    WW2 = WW1;
                    textBox10.Text = WW2.ToString();
                }
            }
            catch (FormatException)
            {

            }
        }


        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            try
            {
                HP2 = float.Parse(textBox6.Text);
                if (same)
                {
                    HP1 = HP2;
                    textBox1.Text = HP1.ToString();
                }
            }
            catch (FormatException)
            {

            }
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            try
            {
                HA2 = float.Parse(textBox7.Text);
                if (same)
                {
                    HA1 = HA2;
                    textBox2.Text = HA1.ToString();
                }
            }
            catch (FormatException)
            {

            }
        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {
            try
            {
                HB2 = float.Parse(textBox8.Text);
                if (same)
                {
                    HB1 = HB2;
                    textBox3.Text = HB1.ToString();
                }
            }
            catch (FormatException)
            {

            }
        }

        private void textBox9_TextChanged(object sender, EventArgs e)
        {
            try
            {
                HA2 = float.Parse(textBox9.Text);
                if (same)
                {
                    HA1 = HA2;
                    textBox4.Text = HA1.ToString();
                }
            }
            catch (FormatException)
            {

            }
        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {
            try
            {
                WW2 = float.Parse(textBox10.Text);
                if (same)
                {
                    WW1 = WW2;
                    textBox5.Text = WW1.ToString();
                }
            }
            catch (FormatException)
            {

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            float w1 = LD * WW1 * (0.5F * (HA1 + HB1) + HP1 + HS1);
            float w2 = LD * WW2 * (0.5F * (HA2 + HB2) + HP2 + HS2);

            wAdd = w1 + w2;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public float GetWAdd()
        {
            return wAdd;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            same = checkBox1.Checked;
        }
    }
}
