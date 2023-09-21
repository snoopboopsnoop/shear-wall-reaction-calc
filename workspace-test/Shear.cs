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

namespace workspace_test
{
    internal class Shear
    {
        private RectangleF dimensions;
        private List<Tuple<PointF, PointF>> lines;
        private Tuple<List<RectangleF>, List<RectangleF>> data;
        private Tuple<List<ShearData>, List<ShearData>> shearData;
        private Tuple<List<int>, List<int>> reactions;
        private float LA;
        private float LD;
        private float LS;
        private Word.Range range;

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
        public Shear(List<Tuple<PointF, PointF>> paramLines, Tuple<List<RectangleF>, List<RectangleF>> paramData, Tuple<List<int>, List<int>> paramReacts, RectangleF paramDims, float paramLA, float paramLD, Word._Document doc = null)
        {
            lines = paramLines;
            dimensions = paramDims;
            reactions = paramReacts;
            data = paramData;
            LA = paramLA;
            LD = paramLD;
            LS = LA * LD;

            List<int> reactionValsLeft = new List<int>(paramData.Item1.Count() + 1);

            if(doc != null)
            {
                Word.Paragraph intro;
                intro = doc.Content.Paragraphs.Add();
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
            }

            List<ShearData> tempLefts = new List<ShearData>();
            List<ShearData> tempBottoms = new List<ShearData>();

            foreach (var (left, i) in paramData.Item1.Select((left, i) => (left, i)))
            {
                Console.WriteLine("adding new data");
                tempLefts.Add(new ShearData(left, LS, "left", "Wx" + (i + 1), doc));

            }

            if(doc != null)
            {
                range = doc.Bookmarks.get_Item("\\endofdoc").Range;
                range.InsertParagraphAfter();
            }

            foreach(var (bottom, i) in paramData.Item2.Select((left, i) => (left, i)))
            {
                tempBottoms.Add(new ShearData(bottom, LS, "bottom", "Wy" + (i + 1), doc));
            }

            if (doc != null)
            {
                range = doc.Bookmarks.get_Item("\\endofdoc").Range;
                range.InsertBreak(Word.WdBreakType.wdPageBreak);
                //range.InsertParagraphAfter();

                Word.Paragraph reactionHeader;
                reactionHeader = doc.Content.Paragraphs.Add();
                reactionHeader.Range.Underline = Word.WdUnderline.wdUnderlineSingle;
                reactionHeader.Range.Font.Size = 18;
                reactionHeader.Range.Font.Bold = 1;
                

                reactionHeader.Range.Text = "REACTIONS @ SHEAR LINES";
                reactionHeader.Format.SpaceAfter = 16;

                reactionHeader.Range.InsertParagraphAfter();

                Word.Paragraph reaction;
                reaction = doc.Content.Paragraphs.Add();
                reaction.Range.Text = ("Rx = 0.5 * wx * dimY lbs");
                reaction.Format.SpaceAfter = 0;
                reaction.Format.SpaceBefore = 0;
                reaction.Range.Font.Bold = 0;
                reaction.Range.Font.Size = 12;
                reaction.Range.Text += "";

                string buffer = "";
                foreach (var (left, i) in paramData.Item1.Select((left, i) => (left, i)))
                {
                    ShearData temp = tempLefts[i];

                    if (i == 0)
                    {
                        reaction.Range.Text += ("R1 = 0.5 * " + temp.wx + " * " + temp.rect.Height + " = " + 0.5 * temp.wx * temp.rect.Height + " LBS");

                        buffer = ("R" + (i + 2) + " = 0.5 * " + temp.wx + " * " + temp.rect.Height + " = " + 0.5 * temp.wx * temp.rect.Height);
                    }
                    else
                    {
                        buffer += (" + 0.5 * " + temp.wx + " * " + temp.rect.Height + " = " + 0.5 * temp.wx * temp.rect.Height + " LBS");
                        reaction.Range.Text += buffer;

                        buffer = ("R" + (i + 2) + " = 0.5 * " + temp.wx + " * " + temp.rect.Height + " = " + 0.5 * temp.wx * temp.rect.Height);
                    }
                }

                buffer += (" LBS\n");
                reaction.Range.Text += buffer;

                reaction.Range.Text += ("Ry = 0.5 * wy * dimX lbs\n");

                foreach (var (bottom, i) in paramData.Item2.Select((left, i) => (left, i)))
                {
                    ShearData temp = tempBottoms[i];

                    if (i == 0)
                    {
                        reaction.Range.Text += ("RA = 0.5 * " + temp.wy + " * " + temp.rect.Width + " = " + 0.5 * temp.wy * temp.rect.Width + " LBS");

                        buffer = ("R" + (char)(65 + i + 1) + " = 0.5 * " + temp.wy + " * " + temp.rect.Width + " = " + 0.5 * temp.wy * temp.rect.Width);
                    }
                    else
                    {
                        buffer += (" + 0.5 * " + temp.wy + " * " + temp.rect.Width + " = " + 0.5 * temp.wy * temp.rect.Width + " LBS");
                        reaction.Range.Text += buffer;

                        buffer = ("R" + (char)(65 + i + 1) + " = 0.5 * " + temp.wy + " * " + temp.rect.Width + " = " + 0.5 * temp.wy * temp.rect.Width);
                    }
                }
                buffer += (" LBS");
                reaction.Range.Text += buffer;
                reaction.Range.InsertParagraphAfter();
            }

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

            Console.WriteLine("sheardata left size: " + tempLefts.Count);
            Console.WriteLine("sheardata bottom size: " + tempBottoms.Count);

            Console.WriteLine("test0: " + tempLefts[0].visual);

            shearData = new Tuple<List<ShearData>, List<ShearData>>(tempLefts, tempBottoms);
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
