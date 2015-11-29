using System;
using System.Web;
namespace NUnit.Hosted.AspNet
{
    public class HttpContextWrapper:IHttpContext
    {
        private IHttpRequest request;
        private IHttpResponse response;
        public HttpContextWrapper(HttpContext context)
        {
            this.request = new HttpRequestWrapper(context.Request);
            this.response = new HttpResponseWrapper(context.Response);
        }
        public IHttpRequest Request
        {
            get{ return request;}
        }
        public IHttpResponse Response
        {
            get{ return response;}
        }
    }
}

