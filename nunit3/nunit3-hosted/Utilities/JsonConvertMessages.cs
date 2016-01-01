using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
namespace NUnit.Hosted.Utilities
{
    public class JsonConvertMessages
    {
        private SerializationBinder _binder;
        public JsonConvertMessages()
        {
            _binder = new ShortNameSerializationBinder(typeof(IMessage));
        }

        public T Deserialize<T>(string val)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects, Binder = _binder };
            return JsonConvert.DeserializeObject<T>(val, settings);
        }
        public IEnumerable<IMessage> DeserializeStream(Stream stream)
        {
            using (var r = new StreamReader(stream))
                return DeserializeStream(r).ToArray();
        }
        private IEnumerable<IMessage> DeserializeStream(TextReader r)
        {
            string line;
            while (null != (line = r.ReadLine()))
            {
                yield return Deserialize<IMessage>(line);
            }
        }

        public string Serialize<T>(T obj)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects, Binder = _binder };
            return JsonConvert.SerializeObject(obj, settings);
        }
        private class ShortNameSerializationBinder : SerializationBinder
        {
            private readonly Type type;
            private readonly IDictionary<string, Type> types;

            public ShortNameSerializationBinder(Type type)
            {
                this.type = type;
                this.types = type.Assembly.GetTypes().Where(t => type.IsAssignableFrom(t)).ToDictionary(t => t.Name, t => t);
            }

            public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
            {
                if (this.type.IsAssignableFrom(serializedType))
                {
                    assemblyName = null;
                    typeName = serializedType.Name;
                }
                else
                {
                    assemblyName = serializedType.Assembly.FullName;
                    typeName = serializedType.FullName;
                }
            }

            public override Type BindToType(string assemblyName, string typeName)
            {
                if (assemblyName == null)
                {
                    if (types.ContainsKey(typeName))
                    {
                        return types[typeName];
                    }
                }
                return Type.GetType(string.Format("{0}, {1}", typeName, assemblyName), true);
            }
        }
    }
}
