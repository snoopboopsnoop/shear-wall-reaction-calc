using Word = Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;

namespace workspace_test
{
    public class ShearData
    {
        public ShearData()
        {
            rect = RectangleF.Empty;
            visual = Rectangle.Empty;
            direction = "both";
            LS = 0;
            wx = 0;
            wy = 0;
        }

        public ShearData(RectangleF paramRect)
        {
            rect = paramRect;
            visual = Rectangle.Empty;
            direction = "both";
            LS = 0;
            wx = 0;
            wy = 0;
        }
        public ShearData(RectangleF paramRect, float paramLS, string paramDirection = "both", string name = "", Word._Document doc = null)
        {
            rect = paramRect;
            LS = paramLS;
            direction = paramDirection;

            wx = (direction == "bottom") ? 0 : LS * rect.Width;
            Console.WriteLine("wx: " + wx);
            wy = (direction == "left") ? 0 : LS * rect.Height;
            Console.WriteLine("wy: " + wy);

            visual = Rectangle.Empty;

            if (doc != null)
            {
                Word.Range range = doc.Bookmarks.get_Item("\\endofdoc").Range;
                range.InsertAfter(name + " = ");
                if (direction == "bottom") range.InsertAfter(LS + "PSF x " + rect.Height + "\' = " + wy + " PLF\n");
                else if (direction == "left") range.InsertAfter(LS + "PSF x " + rect.Width + "\' = " + wx + " PLF\n");
            }
        }

        public void UpdateVisual()
        {
            //Console.WriteLine("wx ref " + wx / Globals.refMeasure);
            //Console.WriteLine("wy ref " + wy / Globals.refMeasure);

            visual = (direction == "bottom") ?
                new Rectangle((int)rect.X + 4, (int)(rect.Y + 5 + rect.Height), (int)rect.Width - 8, (int)wy / Globals.refMeasure + 10) :
                new Rectangle((int)(rect.X - ((int)wx / Globals.refMeasure + 10)), (int)rect.Y + 4, (int)wx / Globals.refMeasure + 5, (int)rect.Height - 8);

            //Console.WriteLine("updating w visual to " + visual);
        }

        public RectangleF rect { get; }
        public Rectangle visual { get; set; }
        public float LS { get; set; }
        public float wx { get; set; }
        public float wy { get; set; }
        public string direction { get; }
    }
}
