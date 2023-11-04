using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Word = Microsoft.Office.Interop.Word;

namespace workspace_test
{
    internal static class Globals
    {
        public static int refMeasure { get; set; } = 1;
        public static Word._Application word = new Word.Application();
        public static Word._Document doc = null;
        public static object missing = System.Reflection.Missing.Value;
        public static object eod = "\\endofdoc";
        public static Color fontColor = Color.FromArgb(1, 230, 230, 230);
    }
}
