using System;
using System.IO;

namespace NUnit.Hosted.AspNet
{
    public interface IHttpRequest
    {
        Uri Url{get;}
        Stream InputStream{get;}
        string HttpMethod{ get;}
        string MapPath(string path);
    }
}

