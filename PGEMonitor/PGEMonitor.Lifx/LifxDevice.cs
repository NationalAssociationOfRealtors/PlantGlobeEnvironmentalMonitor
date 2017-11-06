using Newtonsoft.Json;

namespace PGEMonitor.Lifx
{
    public class LifxDevice
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        public string MacAddressName { get; set; }
    }
}
