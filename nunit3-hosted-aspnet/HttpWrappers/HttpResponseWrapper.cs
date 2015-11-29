using System;
using System.Web;
namespace NUnit.Hosted.AspNet
{
    public class HttpResponseWrapper:IHttpResponse
    {
        private HttpResponse response;
        public HttpResponseWrapper(HttpResponse response)
        {
            this.response = response;
        }

        public void BinaryWrite(byte[] bytes)
        {
            response.BinaryWrite(bytes);
        }

        public void Write(string value)
        {
            response.Write(value);
        }

        public string ContentType
        {
            get
            {
                return response.ContentType;
            }
            set
            {
                response.ContentType = value;
            }
        }
        public int StatusCode
        {
            get{ return response.StatusCode;}
            set{ response.StatusCode = value;}
        }
    }
}
