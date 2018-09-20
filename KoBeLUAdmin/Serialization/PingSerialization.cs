using Newtonsoft.Json;

namespace KoBeLUAdmin.Serialization
{
    public class PingSerialization
    {
        private string mCall = "ping";

        public PingSerialization()
        { }

        [JsonProperty(PropertyName = "call")]
        public string Call { get => mCall; set => mCall = value; }
    }
}
