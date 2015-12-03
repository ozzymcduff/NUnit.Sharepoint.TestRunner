using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text;

namespace NUnit.Hosted.AspNet
{
    public class Serializer
    {
        private JsonSerializerSettings settings;

        public Serializer()
        {
            settings = new JsonSerializerSettings();
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }
        public byte[] Serialize(object p)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(p, Formatting.Indented, settings));
        }
    }
}