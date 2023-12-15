using Microsoft.Office.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

using Word = Microsoft.Office.Interop.Word;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Microsoft.Office.Interop.Word;
using static System.Collections.Specialized.BitVector32;

namespace workspace_test
{
    [XmlRoot("Shear", Namespace = "bobr", IsNullable = true)]
    [Serializable]
    public class Shear
    {
        [JsonProperty]
        private RectangleF dimensions { get; set; }
        [JsonProperty]
        private List<Tuple<PointF, PointF>> lines { get; set; }
        [JsonProperty]
        private Tuple<List<RectangleF>, List<RectangleF>> data { get; set; }
        [JsonProperty]
        private Tuple<List<ShearData>, List<ShearData>> shearData { get; set; }
        [JsonProperty]
        private Tuple<List<int>, List<int>> reactions { get; set; }
        [JsonProperty]
        private float LA { get; set; }
        [JsonProperty]
        private float LD { get; set; }
        [JsonProperty]
        private float LS { get; set; }
        [JsonProperty]
        int rangeStart { get; set; }
        [JsonProperty]
        int rangeEnd { get; set; }
        [JsonIgnore]
        Range reactRange { get; set; }


        public Shear()
        {
            dimensions = RectangleF.Empty;
            LA = 0;
            LD = 0;
            LS = 0;
            data = null;
            shearData = null;
        }

        // paramData: 2 lists of rectangles in form <lefts, bottoms> for drawing boxes
        public Shear(List<Tuple<PointF, PointF>> paramLines, Tuple<List<RectangleF>, List<RectangleF>> paramData, Tuple<List<int>, List<int>> paramReacts, RectangleF paramDims, float paramLA, float paramLD)
        {
            lines = paramLines;
            dimensions = paramDims;
            reactions = paramReacts;
            data = paramData;
            LA = paramLA;
            LD = paramLD;
            LS = LA * LD;

            List<int> reactionValsLeft = new List<int>(paramData.Item1.Count() + 1);

            Word.Paragraph intro;
            intro = Globals.doc.Content.Paragraphs.Add();
            intro.Range.Text = ("LA = " + LA + " g");
            intro.Format.SpaceBefore = 0;
            intro.Format.SpaceAfter = 0;
            intro.Range.Font.Bold = 0;
            intro.Range.Font.Size = 12;
            intro.Range.Text += ("LD = " + LD + " PSF");
            intro.Range.Text += ("LS = LA x LD = " + LA + " x " + LD + " = " + LA * LD + " PSF\n");
            intro.Range.Text += ("Wx = LS x dimX");
            intro.Range.Text += ("Wy = LS x dimY\n");
            intro.Range.InsertParagraphAfter();

            List<ShearData> tempLefts = new List<ShearData>();
            List<ShearData> tempBottoms = new List<ShearData>();

            foreach (var (left, i) in paramData.Item1.Select((left, i) => (left, i)))
            {
                Console.WriteLine("adding new data");
                tempLefts.Add(new ShearData(left, LS, "left", "Wx" + (i + 1)));
            }


            Word.Range range = Globals.doc.Bookmarks.get_Item("\\endofdoc").Range;
            range.InsertParagraphAfter();

            foreach(var (bottom, i) in paramData.Item2.Select((left, i) => (left, i)))
            {
                tempBottoms.Add(new ShearData(bottom, LS, "bottom", "Wy" + (i + 1)));
            }

            range = Globals.doc.Bookmarks.get_Item("\\endofdoc").Range;
            range.InsertBreak(Word.WdBreakType.wdPageBreak);
            //range.InsertParagraphAfter();

            LoadReactions(tempLefts, tempBottoms);

            int wxMeasure = tempLefts.Min(p => (int)p.wx);
            int wyMeasure = tempBottoms.Min(p => (int)p.wy);

            Globals.refMeasure = (wxMeasure > wyMeasure) ? wyMeasure : wxMeasure;
            Console.WriteLine("refmeasure: " + Globals.refMeasure);

            foreach (var shearData in tempLefts)
            {
                shearData.UpdateVisual();
            }
            foreach (var shearData in tempBottoms)
            {
                shearData.UpdateVisual();
            }

            //Console.WriteLine("sheardata left size: " + tempLefts.Count);
            //Console.WriteLine("sheardata bottom size: " + tempBottoms.Count);

            //Console.WriteLine("test0: " + tempLefts[0].visual);

            shearData = new Tuple<List<ShearData>, List<ShearData>>(tempLefts, tempBottoms);
        }

        public void updateReactions()
        {
            Console.WriteLine("updating reaction");
            reactRange.Text = "";
            reactRange.Text = "bobr\n";
            reactRange.Text += "bobr2\n";
            WriteReactions(reactRange, shearData.Item1, shearData.Item2, true);
        }

        private void LoadReactions(List<ShearData> tempLefts, List<ShearData> tempBottoms)
        {
            Word.Paragraph reactionHeader;
            reactionHeader = Globals.doc.Content.Paragraphs.Add();
            reactionHeader.Range.Underline = Word.WdUnderline.wdUnderlineSingle;
            reactionHeader.Range.Font.Size = 18;
            reactionHeader.Range.Font.Bold = 1;


            reactionHeader.Range.Text = "REACTIONS @ SHEAR LINES";
            reactionHeader.Format.SpaceAfter = 16;

            reactionHeader.Range.InsertParagraphAfter();

            reactRange = Globals.doc.Bookmarks.get_Item("\\endofdoc").Range;
            rangeStart = reactRange.End;

            Word.Paragraph reaction;
            reaction = Globals.doc.Content.Paragraphs.Add();
            reaction.Format.SpaceAfter = 0;
            reaction.Format.SpaceBefore = 0;
            reaction.Range.Font.Bold = 0;
            reaction.Range.Font.Size = 12;

            WriteReactions(reaction.Range, tempLefts, tempBottoms);

            reactRange = Globals.doc.Bookmarks.get_Item("\\endofdoc").Range;
            rangeEnd = reactRange.End;

            reactRange = Globals.doc.Range(rangeStart, rangeEnd);
        }

        private void WriteReactions(Range range, List<ShearData> tempLefts, List<ShearData> tempBottoms, bool update = false)
        {
            range.Text = ("Rx = 0.5 * wx * dimY lbs\n");
            range.Text += "bobr";

            string buffer = "";
            foreach (var (left, i) in data.Item1.Select((left, i) => (left, i)))
            {
                ShearData temp = tempLefts[i];
                if (tempLefts.Count() == 1)
                {

                    range.Text += "R1 = 0.5 * " +
                                            temp.wx.ToString("0,0.###") + " PLF" +
                                            " * " + (Math.Round(temp.rect.Height * Globals.scale / 0.5) * 0.5).ToString("0,0.###") + Globals.unit +
                                            " = " + (Math.Round(temp.wx * temp.rect.Height * Globals.scale) * 0.5).ToString("0,0.###") + " LBS\n";
                    if (update) range.Text += "\n";
                    range.Text += "R2 = 0.5 * " +
                                            temp.wx.ToString("0,0.###") + " PLF" +
                                            " * " + (Math.Round(temp.rect.Height * Globals.scale / 0.5) * 0.5).ToString("0,0.###") + Globals.unit +
                                            " = " + (Math.Round(temp.wx * temp.rect.Height * Globals.scale) * 0.5).ToString("0,0.###") + " LBS\n";
                    if (update) range.Text += "\n";
                    break;
                }
                else if (i == 0)
                {
                    range.Text += "R1 = 0.5 * " +
                                            temp.wx.ToString("0,0.###") + " PLF" +
                                            " * " + (Math.Round(temp.rect.Height * Globals.scale / 0.5) * 0.5).ToString("0,0.###") + Globals.unit +
                                            " = " + (Math.Round(temp.wx * temp.rect.Height * Globals.scale) * 0.5).ToString("0,0.###") + " LBS";
                    if (update) range.Text += "\n";

                    buffer = ("R" + (i + 2) + " = 0.5 * " + temp.wx.ToString("#,#0.###") + " PLF" +
                              " * " + (Math.Round(temp.rect.Height * Globals.scale / 0.5) * 0.5).ToString("#,#0.###") + Globals.unit);
                }
                else
                {
                    buffer += (" + 0.5 * " + temp.wx.ToString("#,#0.###") + " PLF" +
                               " * " + (Math.Round(temp.rect.Height * Globals.scale / 0.5) * 0.5).ToString("#,#0.###") + Globals.unit +
                               " = " + (Math.Round(temp.wx * temp.rect.Height * Globals.scale) * 0.5).ToString("#,#0.###") + " LBS");
                    range.Text += buffer;
                    if (update) range.Text += "\n";

                    buffer = ("R" + (i + 2) + " = 0.5 * " + temp.wx.ToString("#,#0.###") + " PLF" +
                              " * " + (Math.Round(temp.rect.Height * Globals.scale / 0.5) * 0.5).ToString("#,#0.###") + Globals.unit);

                    if (i == tempLefts.Count() - 1)
                    {
                        buffer += " = " + 0.5 * temp.wx * (Math.Round(temp.rect.Height * Globals.scale / 0.5) * 0.5) + " LBS\n";
                        if (update) range.Text += "\n";
                    }
                }
            }
            range.Text += buffer;
            if (update) range.Text += "\n";

            range.Text += ("Ry = 0.5 * wy * dimX lbs\n");

            foreach (var (bottom, i) in data.Item2.Select((left, i) => (left, i)))
            {
                ShearData temp = tempBottoms[i];

                if (tempBottoms.Count() == 1)
                {

                    range.Text += ("RA = 0.5 * " + temp.wy.ToString("#,#0.###") + " PLF" +
                                            " * " + (Math.Round(temp.rect.Width * Globals.scale / 0.5) * 0.5).ToString("#,#0.###") + Globals.unit +
                                            " = " + (Math.Round(temp.wy * temp.rect.Width * Globals.scale) * 0.5).ToString("#,#0.###") + " LBS");
                    range.Text += ("RB = 0.5 * " + temp.wy.ToString("#,#0.###") + " PLF" +
                                            " * " + (Math.Round(temp.rect.Width * Globals.scale / 0.5) * 0.5).ToString("#,#0.###") + Globals.unit +
                                            " = " + (Math.Round(temp.wy * temp.rect.Width * Globals.scale) * 0.5).ToString("#,#0.###") + " LBS\n");
                    break;
                }
                else if (i == 0)
                {
                    range.Text += ("RA = 0.5 * " + temp.wy.ToString("#,#0.###") + " PLF" +
                                            " * " + (Math.Round(temp.rect.Width * Globals.scale / 0.5) * 0.5).ToString("#,#0.###") + Globals.unit +
                                            " = " + (Math.Round(temp.wy * temp.rect.Width * Globals.scale) * 0.5).ToString("#,#0.###") + " LBS");

                    buffer = ("R" + (char)(65 + i + 1) + " = 0.5 * " + temp.wy.ToString("#,#0.###") + " PLF" +
                              " * " + (Math.Round(temp.rect.Width * Globals.scale / 0.5) * 0.5).ToString("#,#0.###") + Globals.unit);
                }
                else
                {
                    buffer += (" + 0.5 * " + temp.wy.ToString("#,#0.###") + " PLF" +
                               " * " + (Math.Round(temp.rect.Width * Globals.scale / 0.5) * 0.5).ToString("#,#0.###") + Globals.unit +
                               " = " + (Math.Round(temp.wy * temp.rect.Width * Globals.scale) * 0.5).ToString("#,#0.###"));
                    range.Text += buffer;

                    buffer = ("R" + (char)(65 + i + 1) + " = 0.5 * " + temp.wy.ToString("#,#0.###") + " PLF" +
                              " * " + (Math.Round(temp.rect.Width * Globals.scale / 0.5) * 0.5).ToString("#,#0.###") + Globals.unit);
                    if (i == tempBottoms.Count() - 1)
                    {
                        buffer += " = " + 0.5 * temp.wy * (Math.Round(temp.rect.Width * Globals.scale / 0.5) * 0.5) + " LBS\n";
                    }
                }
            }
            range.Text += buffer;
            range.InsertParagraphAfter();
        }

        public void Load()
        {
            foreach(ShearData data in shearData.Item1)
            {
                data.Load();
            }
            foreach (ShearData data in shearData.Item2)
            {
                data.Load();
            }
        }

        public Tuple<List<RectangleF>, List<RectangleF>> GetData()
        {
            return data;
        }

        public List<Tuple<PointF, PointF>> GetLines()
        {
            return lines;
        }

        public Tuple<List<ShearData>, List<ShearData>> GetShearData()
        {
            return shearData;
        }

        public RectangleF GetDimensions()
        {
            return dimensions;
        }

        public Tuple<List<int>, List<int>> GetReactions()
        {
            return reactions;
        }
    }
}
