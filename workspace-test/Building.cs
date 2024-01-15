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
        private string name = "Floors";
        [JsonProperty]
        private List<Floor> floors = new List<Floor>();
        [JsonProperty]
        private int floorCount = 0;

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
    }
}
