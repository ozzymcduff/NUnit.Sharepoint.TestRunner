using System;

namespace NUnit.Hosted.AspNet
{
    public interface IHttpResponse
    {
        string ContentType{get;set;}
        void BinaryWrite(byte[] bytes);
        int StatusCode{get;set;}
        void Write(string v);
    }
}

