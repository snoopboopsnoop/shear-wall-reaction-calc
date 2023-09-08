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
        private Tuple<List<RectangleF>, List<RectangleF>> data;
        private Tuple<List<ShearData>, List<ShearData>> shearData;
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
        public Shear(Tuple<List<RectangleF>, List<RectangleF>> paramData, RectangleF paramDims, float paramLA, float paramLD, string outputPath)
        {
            dimensions = paramDims;
            data = paramData;
            LA = paramLA;
            LD = paramLD;
            LS = LA * LD;

            using (StreamWriter output = new StreamWriter(outputPath))
            {
                output.WriteLine("SEISMIC WT @ ROOF");

                output.WriteLine();

                output.WriteLine("LA = " + LA + " g");
                output.WriteLine("LD = " + LD + " PSF");
                output.WriteLine("LS = LA * LD = " + LA + " * " + LD + " = " + LA * LD + " PSF");
                output.WriteLine();

                output.WriteLine("Wx = LS * dimX");
                output.WriteLine("Wy = LS * dimY");
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

            shearData = new Tuple<List<ShearData>, List<ShearData>>(tempLefts, tempBottoms);
        }

        public Tuple<List<RectangleF>, List<RectangleF>> GetData()
        {
            return data;
        }

        public Tuple<List<ShearData>, List<ShearData>> GetShearData()
        {
            return shearData;
        }
    }
}
