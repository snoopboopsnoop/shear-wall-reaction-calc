using System;
using System.Collections.Generic;
using System.Drawing;
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
        public Shear(Tuple<List<RectangleF>, List<RectangleF>> paramData, RectangleF paramDims, float paramLA, float paramLD)
        {
            dimensions = paramDims;
            data = paramData;
            LA = paramLA;
            LD = paramLD;
            LS = LA * LD;

            List<ShearData> tempLefts = new List<ShearData>();
            List<ShearData> tempBottoms = new List<ShearData>();

            foreach (var left in paramData.Item1)
            {
                Console.WriteLine("adding new data");
                tempLefts.Add(new ShearData(left, LS, "left"));
            }
            foreach(var bottom in paramData.Item2)
            {
                tempBottoms.Add(new ShearData(bottom, LS, "bottom"));
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
