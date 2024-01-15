using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace workspace_test
{
    [Serializable]
    public class Building
    {
        [JsonProperty]
        private string name;
        [JsonProperty]
        private List<Floor> floors = new List<Floor>();

        public Building()
        {
            
        }

        public Building(string name) : this()
        {
            this.name = name;
        }

        public void AddFloor(Floor floor)
        {
            this.floors.Add(floor);
        }

        public List<Floor> GetFloors()
        {
            return floors;
        }
    }
}
