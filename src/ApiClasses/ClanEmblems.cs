using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WGApi
{
    public class ClanEmblems
    {
        [JsonProperty("x24")]
        public Emblem x24 { get; set; }
        [JsonProperty("x32")]
        public Emblem x32 { get; set; }
        [JsonProperty("x64")]
        public Emblem x64 { get; set; }
        [JsonProperty("x195")]
        public Emblem x195 { get; set; }
        [JsonProperty("x256")]
        public Emblem x256 { get; set; }

        public ClanEmblems()
        {
            x24 = new Emblem();
            x32 = new Emblem();
            x64 = new Emblem();
            x195 = new Emblem();
            x256 = new Emblem();
        }
    }
}
