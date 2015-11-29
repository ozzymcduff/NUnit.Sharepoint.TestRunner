using System;
using NUnit.Hosted.AspNet;

namespace Tests
{
    public class HttpContextFake:IHttpContext
    {
        public HttpContextFake()
        {
            RequestFake = new HttpRequestFake();
            ResponseFake = new HttpResponseFake();
        }

        public readonly HttpRequestFake RequestFake;
        public IHttpRequest Request
        {
            get
            {
                return RequestFake;
            }
        }
        public readonly HttpResponseFake ResponseFake;
        public IHttpResponse Response
        {
            get
            {
                return ResponseFake;
            }
        }
    }
}

