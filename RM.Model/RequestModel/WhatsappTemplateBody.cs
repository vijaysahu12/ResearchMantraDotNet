using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RM.Model.RequestModel
{
    public class WhatsappTemplateBody
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("params")]
        public List<string> Params { get; set; }
    }
}
