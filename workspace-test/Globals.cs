﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Word = Microsoft.Office.Interop.Word;

namespace workspace_test
{
    internal static class Globals
    {
        public static int refMeasure { get; set; } = 1;
        public static Word._Application word;
        public static Word._Document doc = null;
        public static object missing = System.Reflection.Missing.Value;
        public static object eod = "\\endofdoc";
        public static Color fontColor = Color.FromArgb(255, 247, 247, 247);
        public static double scale = 1;
        public static string unit = "\'";
        public static int weightWidth = 5;
        public static int gap = 5;
        public static Main main;

        public static void CloseWord()
        {
            try
            {
                if(doc != null) doc.Close();
                word.Quit();
            }
            catch (COMException)
            {
                return;
            }
            catch (NullReferenceException)
            {
                return;
            }
        }
    }
}
