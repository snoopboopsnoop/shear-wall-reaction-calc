using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace workspace_test
{
    internal class Shear
    {
        private RectangleF dimensions;
        private List<Tuple<PointF, PointF>> lines;
        private Tuple<List<RectangleF>, List<RectangleF>> data;
        private Tuple<List<ShearData>, List<ShearData>> shearData;
        private Tuple<List<int>, List<int>> reactions;
        private List<int> reactionVals;
        private float LA;
        private float LD;
        private float LS;

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
        public Shear(List<Tuple<PointF, PointF>> paramLines, Tuple<List<RectangleF>, List<RectangleF>> paramData, Tuple<List<int>, List<int>> paramReacts, RectangleF paramDims, float paramLA, float paramLD, string outputPath)
        {
            lines = paramLines;
            dimensions = paramDims;
            reactions = paramReacts;
            data = paramData;
            LA = paramLA;
            LD = paramLD;
            LS = LA * LD;

            List<int> reactionValsLeft = new List<int>(paramData.Item1.Count() + 1);

            using (StreamWriter output = new StreamWriter(outputPath))
            {
                output.WriteLine("SEISMIC WT @ ROOF");

                output.WriteLine();

                output.WriteLine("LA = " + LA + " g");
                output.WriteLine("LD = " + LD + " PSF");
                output.WriteLine("LS = LA x LD = " + LA + " x " + LD + " = " + LA * LD + " PSF");
                output.WriteLine();

                output.WriteLine("Wx = LS x dimX");
                output.WriteLine("Wy = LS x dimY");
                output.WriteLine();
            }

            List<ShearData> tempLefts = new List<ShearData>();
            List<ShearData> tempBottoms = new List<ShearData>();

            foreach (var (left, i) in paramData.Item1.Select((left, i) => (left, i)))
            {
                Console.WriteLine("adding new data");
                tempLefts.Add(new ShearData(left, LS, "left", "Wx" + (i + 1), outputPath));

            }
            foreach(var (bottom, i) in paramData.Item2.Select((left, i) => (left, i)))
            {
                tempBottoms.Add(new ShearData(bottom, LS, "bottom", "Wy" + (i + 1), outputPath));
            }

            using(StreamWriter output = new StreamWriter(outputPath, true))
            {
                output.WriteLine("REACTIONS @ SHEAR LINES");
                output.WriteLine();
                //output.WriteLine("Rx = 0.5 * wx * dimY lbs");
                //output.WriteLine();

                foreach (var (left, i) in paramData.Item1.Select((left, i) => (left, i)))
                {
                    ShearData temp = tempLefts[i];

                    if (i == 0)
                    {
                        output.WriteLine("R1 = 0.5 * " + temp.wx + " * " + temp.rect.Height + " = " + 0.5 * temp.wx * temp.rect.Height + " LBS");

                        output.Write("R" + (i + 2) + " = 0.5 * " + temp.wx + " * " + temp.rect.Height + " = " + 0.5 * temp.wx * temp.rect.Height);
                    }
                    else
                    {
                        output.WriteLine(" + 0.5 * " + temp.wx + " * " + temp.rect.Height + " = " + 0.5 * temp.wx * temp.rect.Height + " LBS");

                        output.Write("R" + (i + 2) + " = 0.5 * " + temp.wx + " * " + temp.rect.Height + " = " + 0.5 * temp.wx * temp.rect.Height);
                    }

                }

                output.WriteLine(" LBS");
                output.WriteLine();
                //output.WriteLine("Ry = 0.5 * wy * dimX lbs");
                //output.WriteLine();

                foreach (var (bottom, i) in paramData.Item2.Select((left, i) => (left, i)))
                {
                    ShearData temp = tempBottoms[i];

                    if (i == 0)
                    {
                        output.WriteLine("RA = 0.5 * " + temp.wy + " * " + temp.rect.Width + " = " + 0.5 * temp.wy * temp.rect.Width + " LBS");

                        output.Write("R" + (char)(65 + i + 1) + " = 0.5 * " + temp.wy + " * " + temp.rect.Width + " = " + 0.5 * temp.wy * temp.rect.Width);
                    }
                    else
                    {
                        output.WriteLine(" + 0.5 * " + temp.wy + " * " + temp.rect.Width + " = " + 0.5 * temp.wy * temp.rect.Width + " LBS");

                        output.Write("R" + (char)(65 + i + 1) + " = 0.5 * " + temp.wy + " * " + temp.rect.Width + " = " + 0.5 * temp.wy * temp.rect.Width);
                    }
                }
                output.WriteLine(" LBS");
            }

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
