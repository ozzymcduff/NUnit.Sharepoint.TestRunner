using Newtonsoft.Json;
using System.Text;

namespace NUnit.Hosted.AspNet
{
    public class Serializer
    {
        public byte[] Serialize(object p)
        {
            return Encoding.UTF8.GetBytes( JsonConvert.SerializeObject(p));
        }
    }
}