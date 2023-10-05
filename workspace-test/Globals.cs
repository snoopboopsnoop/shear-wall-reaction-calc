using System;
using System.Collections.Generic;
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
        public static Word._Document doc;
        public static object missing = System.Reflection.Missing.Value;
        public static object eod = "\\endofdoc";
    }
}
