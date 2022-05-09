using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VRKf_WMS_Prototype.Models
{
    public class PositionData
    {
        public float latitude { get; set; }
        public float longitude { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public string number { get; set; }
        public string postal_code { get; set; }
        public string street { get; set; }
        public int confidence { get; set; }
        public string region { get; set; }
        public string region_code { get; set; }
        public string county { get; set; }
        public string locality { get; set; }
        public string administrative_area { get; set; }
        public string neighbourhood { get; set; }
        public string country { get; set; }
        public string country_code { get; set; }
        public string continent { get; set; }
        public string label { get; set; }
    }
}
