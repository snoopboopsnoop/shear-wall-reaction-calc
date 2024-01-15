using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Word = Microsoft.Office.Interop.Word;

namespace workspace_test
{
    public class Floor
    {
        // List containing drawn lines
        private List<Tuple<PointF, PointF>> lines = new List<Tuple<PointF, PointF>>();

        // i should change its name but it's just all the shear data collected
        // when analysis is performed on a compound object
        private Shear shear = null;

        private Word._Document doc = null;

        private string name = "";

        public Floor(int floorNum) {
            name = "Floor " + floorNum;
        }
    }
}
