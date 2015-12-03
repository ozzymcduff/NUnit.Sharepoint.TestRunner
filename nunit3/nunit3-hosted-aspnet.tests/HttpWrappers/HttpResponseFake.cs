using System;
using NUnit.Hosted.AspNet;
using System.Text;
using System.Xml.Linq;

namespace Tests
{
    public class HttpResponseFake:IHttpResponse
    {
        public StringBuilder Content;
        public HttpResponseFake()
        {
            Content = new StringBuilder();
        }
        public void BinaryWrite(byte[] bytes)
        {
            Content.Append(Encoding.UTF8.GetString(bytes));
        }

        public void Write(string v)
        {
            Content.Append(v);
        }

        public string ContentType
        {
            get;
            set;
        }

        public int StatusCode
        {
            get;
            set;
        }
        public XDocument AsXml()
        {
            return XDocument.Parse(Content.ToString());
        }
    }
}

