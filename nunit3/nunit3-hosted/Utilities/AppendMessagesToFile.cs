using System.Collections.Generic;
using System.IO;

namespace NUnit.Hosted.Utilities
{
    public class AppendMessagesToFile : Messages.ISubscriber
    {
        private JsonConvertMessages jsonConvert;
        private readonly string fileName;

        public AppendMessagesToFile(string fileName)
        {
            this.fileName = fileName;
            jsonConvert = new JsonConvertMessages();
        }

        public void OnMessage(IMessage message)
        {
            using (var fs = File.Open(fileName, FileMode.Append, FileAccess.Write, FileShare.Read))
            using (var w = new StreamWriter(fs))
            {
                w.WriteLine(jsonConvert.Serialize(message));
                fs.Flush();
            }
        }

        public virtual IEnumerable<IMessage> ReadAll()
        {
            using (var fs = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                return jsonConvert.DeserializeStream(fs);
        }
    }
}
