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
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using System.Collections.Specialized;

namespace workspace_test
{
    [Serializable]
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
            aWeight = new AddiWeight();
        }

        public ShearData(RectangleF paramRect)
        {
            rect = paramRect;
            visual = Rectangle.Empty;
            direction = "both";
            LS = 0;
            wx = 0;
            wy = 0;
            aWeight = new AddiWeight();
        }
        public ShearData(RectangleF paramRect, float paramLS, string paramDirection = "both", string name = "")
        {
            this.name = name;
            rect = paramRect;
            LS = paramLS;
            direction = paramDirection;
            aWeight = new AddiWeight();

            wx = (direction == "bottom") ? 0 : (float)Math.Round(LS * rect.Width * (float)Globals.scale / 0.5F) * 0.5F;
            Console.WriteLine("wx: " + wx.ToString("#,#0.###"));
            wy = (direction == "left") ? 0 : (float)Math.Round(LS * rect.Height * (float)Globals.scale / 0.5F) * 0.5F;
            Console.WriteLine("wy: " + wy.ToString("#,#0.###"));

            visual = Rectangle.Empty;

            range = Globals.doc.Bookmarks.get_Item("\\endofdoc").Range;

            range.InsertAfter("\n");

            range = Globals.doc.Bookmarks.get_Item("\\endofdoc").Range;

            Update();
            Console.WriteLine(range.Text);

            rangeStart = range.Start;
            rangeEnd = range.End;
        }

        public void Update()
        {
            //Console.WriteLine("wx ref " + wx / Globals.refMeasure);
            //Console.WriteLine("wy ref " + wy / Globals.refMeasure);

            string text = name + " = ";
            if (direction == "bottom") text += (LS + " PSF x " + (Math.Round(rect.Height * Globals.scale / 0.5) * 0.5).ToString("#,#0.###") + Globals.unit + aWeight.str + " = " + (wy + aWeight.wAdd).ToString("#,#0.###") + " PLF");
            else if (direction == "left") text += (LS + " PSF x " + (Math.Round(rect.Width * Globals.scale / 0.5) * 0.5).ToString("#,#0.###") + Globals.unit + aWeight.str + " = " + (wx + aWeight.wAdd).ToString("#,#0.###") + " PLF");
            range.Text = text;

            Console.WriteLine("bobr: " + aWeight.wAdd);

            visual = (direction == "bottom") ?
                new Rectangle((int)rect.X + Globals.gap, (int)(rect.Y + Globals.gap + rect.Height), (int)rect.Width - 2 * Globals.gap, (int)((wy + aWeight.wAdd) / Globals.refMeasure * Globals.weightWidth)) :
                new Rectangle((int)(rect.X - ((wx + aWeight.wAdd) / Globals.refMeasure * Globals.weightWidth) - Globals.gap), (int)rect.Y + Globals.gap, (int)((wx + aWeight.wAdd) / Globals.refMeasure * Globals.weightWidth), (int)rect.Height - 2 * Globals.gap);

            //Console.WriteLine("updating w visual to " + visual);
        }

        public void Load()
        {
            range = Globals.doc.Range(rangeStart, rangeEnd);
        }

        [JsonIgnore]
        private Word.Range range { get; set; }
        public RectangleF rect { get; set; }
        public Rectangle visual { get; set; }
        public int rangeStart { get; set; }
        public int rangeEnd { get; set; }
        public float LS { get; set; }
        public float wx { get; set; }
        public float wy { get; set; }
        public string direction { get; set; }
        public AddiWeight aWeight { get; set; }
        public string name { get; set; }
    }
}
