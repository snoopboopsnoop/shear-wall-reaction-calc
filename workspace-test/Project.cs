using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace workspace_test
{
    [Serializable]
    public class Project
    {
        [JsonProperty]
        private string name { get; set; }
        [JsonProperty]
        private Building building { get; set; }
        [JsonProperty]
        private int refMeasure { get; set; }
        [JsonProperty]
        private double scale { get; set; }

        public Project()
        {

        }

        public Project(string name, Building building)
        {
            this.name = name;
            this.building = building;
            this.refMeasure = Globals.refMeasure;
            this.scale = Globals.scale;
        }

        public void Save()
        {
            this.refMeasure = Globals.refMeasure;
            this.scale = Globals.scale;
        }

        public string GetName()
        {
            return name;
        }
        public Building GetBuilding() { return building; }
        public int GetRefMeasure() {  return refMeasure; }
        public double GetScale() { return scale; }
    }
}
