using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace workspace_test
{
    public struct ShearData
    {

        public ShearData(RectangleF paramRect)
        {
            rect = paramRect;
            LA = 0;
            LD = 0;
            LS = 0;
            wx = 0;
            wy = 0;
            rx1 = 0;
            ry1 = 0;
        }
        public ShearData(RectangleF paramRect, float paramLA, float paramLD)
        {
            rect = paramRect;
            LA = paramLA;
            LD = paramLD;
            LS = LA * LD;
            wx = LS * rect.Width;
            //wy = LS * rect.Height;
            wy = 0;
            rx1 = (float)0.5 * wx * rect.Height;
            //ry1 = (float)0.5 * wy * rect.Width;
            ry1 = 0;
        }

        public RectangleF rect { get; }
        public float LA { get; }
        public float LD { get; }
        public float LS { get; set; }
        public float wx { get; set; }
        public float wy { get; set; }
        public float rx1 { get; set; }
        public float ry1 { get; set; }
    }
}
