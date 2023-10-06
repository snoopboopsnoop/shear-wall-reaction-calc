using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace workspace_test
{
    [Serializable]
    public struct Output
    {
        public Shear shear;
        public string docPath;
        public int refMeasure;
        public double scale;

        public Output(Shear paramShear, string paramPath, int paramRef, double paramScale)
        {
            shear = paramShear;
            docPath = paramPath;
            refMeasure = paramRef;
            scale = paramScale;
        }
    }

    
}
