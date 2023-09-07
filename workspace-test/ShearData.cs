﻿using System;
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
            LS = 0;
            wx = 0;
            wy = 0;
            rx1 = 0;
            ry1 = 0;
        }
        public ShearData(RectangleF paramRect, float paramLS, string direction = "both")
        {
            rect = paramRect;
            LS = paramLS;

            wx = (direction == "bottom") ? 0 : LS * rect.Width;
            Console.WriteLine("wx: " + wx);
            wy = (direction == "left") ? 0 : LS * rect.Height;
            Console.WriteLine("wy: " + wy);
            //wx = LS * rect.Width;
            //wy = LS * rect.Height;
            //rx1 = (float)0.5 * wx * rect.Height;
            //ry1 = (float)0.5 * wy * rect.Width;
            rx1 = 0;
            ry1 = 0;
        }

        public RectangleF rect { get; }
        public float LS { get; set; }
        public float wx { get; set; }
        public float wy { get; set; }
        public float rx1 { get; set; }
        public float ry1 { get; set; }
    }
}
