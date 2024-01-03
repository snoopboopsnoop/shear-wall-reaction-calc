using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace workspace_test
{
    public class AddiWeight
    {
        public float wAdd { get; set; }
        public float LA { get; set; }
        public bool same { get; set; }

        public float HP1 { get; set; }
        public float HA1 { get; set; }
        public float HB1 { get; set; }
        public float HS1 { get; set; }
        public float WW1 { get; set; }

        public float HP2 { get; set; }
        public float HA2 { get; set; }
        public float HB2 { get; set; }
        public float HS2 { get; set; }
        public float WW2 { get; set; }

        public bool active { get; set; }

        public string str { get; set; }

        public AddiWeight() {
            wAdd = 0;
            LA = 0;
            HP1 = 0;
            HA1 = 0;
            HB1 = 0;
            HS1 = 0;
            WW1 = 0;

            HP2 = 0;
            HA2 = 0;
            HB2 = 0;
            HS2 = 0;
            WW2 = 0;

            active = false;
            same = false;
            str = "";
        }

        public void updateWeight()
        {
            float w1 = LA * WW1 * (0.5F * (HA1 + HB1) + HP1 + HS1);
            float w2 = LA * WW2 * (0.5F * (HA2 + HB2) + HP2 + HS2);

            wAdd = w1 + w2;

            if (active)
            {
                str = " + (" + LA + " g * " + WW1 + " LBS * (0.5 * (" + HA1 + "\' + " + HB1 + "\') + " + HP1 + "\' + " + HS1 + "\'))";
                str += " + (" + LA + " g * " + WW2 + " LBS * (0.5 * (" + HA2 + "\' + " + HB2 + "\') + " + HP2 + "\' + " + HS2 + "\'))";
            }
            else str = "";
        }
    }
}
