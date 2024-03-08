using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace workspace_test
{
    public struct LAVals
    {
        public double k;
        public double SDS;
        public double R;
        public double I;

        public LAVals(double k, double SDS, double R, double I)
        {
            this.k = k;
            this.SDS = SDS;
            this.R = R;
            this.I = I;
        }
    }
    

    [Serializable]
    public class Building
    {
        [JsonProperty]
        private string name = "Floors";
        [JsonProperty]
        private List<Floor> floors = new List<Floor>();
        [JsonProperty]
        private int floorCount = 0;

        [JsonProperty]
        private LAVals accelVals = new LAVals(1, 0.0, 0.0, 0.0); 

        public Building()
        {
            
        }

        public Building(string name) : this()
        {
            this.name = name;
        }

        public Floor AddFloor()
        {
            floorCount++;
            Floor newFloor = new Floor(floorCount);
            this.floors.Add(newFloor);

            return newFloor;
        }

        public List<Floor> GetFloors()
        {
            return floors;
        }

        public string GetName()
        {
            return name;
        }

        public LAVals GetVals()
        {
            return accelVals;
        }

        public void SetVals(LAVals vals)
        {
            this.accelVals = vals;
        }
    }
}
