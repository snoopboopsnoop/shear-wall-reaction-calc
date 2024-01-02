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
        public ShearData(RectangleF paramRect, float paramLS, string paramDirection = "both", string name = "")
        {
            rect = paramRect;
            LS = paramLS;
            direction = paramDirection;

            wx = (direction == "bottom") ? 0 : (float)Math.Round(LS * rect.Width * (float)Globals.scale / 0.5F) * 0.5F;
            Console.WriteLine("wx: " + wx.ToString("#,#0.###"));
            wy = (direction == "left") ? 0 : (float)Math.Round(LS * rect.Height * (float)Globals.scale / 0.5F) * 0.5F;
            Console.WriteLine("wy: " + wy.ToString("#,#0.###"));

            visual = Rectangle.Empty;

            range = Globals.doc.Bookmarks.get_Item("\\endofdoc").Range;
            range.InsertAfter(name + " = ");
            if (direction == "bottom") range.InsertAfter(LS +  " PSF x " + (Math.Round(rect.Height * Globals.scale / 0.5) * 0.5).ToString("#,#0.###") + Globals.unit + " = " + wy.ToString("#,#0.###") + " PLF\n");
            else if (direction == "left") range.InsertAfter(LS + " PSF x " + (Math.Round(rect.Width * Globals.scale / 0.5) * 0.5).ToString("#,#0.###") + Globals.unit + " = " + wx.ToString("#,#0.###") + " PLF\n");

            rangeIndex = range.End;
        }

        public void UpdateVisual(float addW = 0)
        {
            //Console.WriteLine("wx ref " + wx / Globals.refMeasure);
            //Console.WriteLine("wy ref " + wy / Globals.refMeasure);

            visual = (direction == "bottom") ?
                new Rectangle((int)rect.X + 4, (int)(rect.Y + 5 + rect.Height), (int)rect.Width - 8, (int)(wy + addW) / Globals.refMeasure + 10) :
                new Rectangle((int)(rect.X - ((int)(wx + addW) / Globals.refMeasure + 10)), (int)rect.Y + 4, (int)(wx + addW) / Globals.refMeasure + 5, (int)rect.Height - 8);

            //Console.WriteLine("updating w visual to " + visual);
        }

        public void AddWeight(float addW, string str)
        {
            Word.Range tempRange = Globals.doc.Range(range.End, range.End);
            tempRange.Delete(Word.WdUnits.wdWord, -4);

            if (wx == 0)
            {
                wy += addW;
            }
            else wx += addW;

            range.InsertAfter("+ " + str + " = " + ((wx == 0) ? wy.ToString("#,#0.###") : wx.ToString("#,#0.###")) + " PLF\n");
            UpdateVisual(addW);
        }

        public void Load()
        {
            range = Globals.doc.Range(rangeIndex, rangeIndex);
        }

        [JsonIgnore]
        private Word.Range range { get; set; }
        public RectangleF rect { get; set; }
        public Rectangle visual { get; set; }
        public int rangeIndex { get; set; }
        public float LS { get; set; }
        public float wx { get; set; }
        public float wy { get; set; }
        public string direction { get; set; }
    }
}
