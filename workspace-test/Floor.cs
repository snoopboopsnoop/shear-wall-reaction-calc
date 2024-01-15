using Microsoft.Office.Interop.Word;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Word = Microsoft.Office.Interop.Word;

namespace workspace_test
{
    [Serializable]
    public class Floor
    {
        // List containing drawn lines
        [JsonProperty]
        private List<Tuple<PointF, PointF>> lines = new List<Tuple<PointF, PointF>>();

        // i should change its name but it's just all the shear data collected
        // when analysis is performed on a compound object
        [JsonProperty]
        private Shear shear = null;

        [JsonProperty]
        private Word._Document doc = null;

        [JsonProperty]
        private string docPath = "";

        [JsonProperty]
        private string name = "";

        public Floor(int floorNum) {
            name = "Floor " + floorNum;
        }

        public string GetDocPath()
        {
            return docPath;
        }

        public List<Tuple<PointF, PointF>> GetLines()
        {
            return lines;
        }

        public Shear GetShear()
        {
            return shear;
        }

        public string GetName()
        {
            return name;
        }

        public void SetLines(List<Tuple<PointF, PointF>> lines)
        {
            this.lines = lines;
        }
        public void SetShear(Shear shear)
        {
            this.shear = shear;
        }
    }
}
