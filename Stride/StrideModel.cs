using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace StrideWebHooks.Stride
{
    public class StrideRoot
    {
        [JsonProperty("body")] public StrideMessageModel Body { get; set; }
    }
    public class StrideMessageModel : StrideContentModel
    {
        [JsonProperty("version")] public int Version { get; set; } = 1;
    }

    public class StrideContentModel
    {
        [JsonProperty("type")] public string Type { get; set; }
        [JsonProperty("text")] public string Text { get; set; }

        [JsonProperty("attrs")]
        public Dictionary<string, string> Attributes { get; set; }
        [JsonProperty("marks")] public object[] Marks { get; set; }
        [JsonProperty("content")] public List<StrideContentModel> Content { get; set; }
    }
}
