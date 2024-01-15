using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace workspace_test
{
    [Serializable]
    public struct Output
    {
        public Project project { get; set; }
        //public Shear shear;
        //public List<Tuple<PointF, PointF>> lines;
        //public string docPath;
        //public int refMeasure;
        //public double scale;

        //public Output(List<Tuple<PointF, PointF>> paramLines, Shear paramShear, int paramRef, double paramScale, string paramPath = "")
        //{
        //    shear = paramShear;
        //    lines = paramLines;
        //    docPath = paramPath;
        //    refMeasure = paramRef;
        //    scale = paramScale;
        //}
        public Output(Project project) { this.project = project; }
    }

    
}
