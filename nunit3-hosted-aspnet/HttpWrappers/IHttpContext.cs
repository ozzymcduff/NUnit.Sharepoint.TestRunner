using System;

namespace NUnit.Hosted.AspNet
{
    public interface IHttpContext
    {
        IHttpRequest Request{get;}
        IHttpResponse Response{get;}
    }
}

