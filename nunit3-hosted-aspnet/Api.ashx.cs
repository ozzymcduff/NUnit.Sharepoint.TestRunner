using System;
using System.Web;

namespace NUnit.Hosted.AspNet
{
    /// <summary>
    /// Summary description for Api
    /// </summary>
    public class Api : IHttpHandler
    {
        private Serializer serializer;

        public Api():this(new Serializer())
        {

        }

        public Api(Serializer serializer)
        {
            this.serializer = serializer;
        }

        public void ProcessRequest(HttpContext context)
        {
            ProcessRequest(HttpUtility.Wrap(context));
        }

        public void ProcessRequest(IHttpContext context)
        {
            try
            {
                var command = HttpUtility.GetLastPath(context.Request.Url.AbsolutePath);
                var x = context.Request.Url;
                switch (command)
                {
                    case "Info":
                        context.Response.ContentType = "application/xml";
                        context.Response.BinaryWrite(serializer.Serialize(GetInfo()));
                        return;
                    case "Run":
                        if (context.Request.HttpMethod.Equals("POST"))
                        {
                            context.Response.ContentType = "application/xml";
                            context.Response.BinaryWrite(serializer.Serialize(RunTests()));
                            return;
                        }
                        break;
                    default:
                        context.Response.StatusCode = 404;
                        context.Response.Write("Not found");
                        return;
                }
                context.Response.StatusCode = 404;
                context.Response.Write("Not found");
            }
            catch (Exception ex)
            {
                context.Response.ContentType = "application/xml";
                context.Response.StatusCode = 500;
                context.Response.BinaryWrite(serializer.Serialize(ex));
            }
        }

        private object RunTests()
        {
            throw new NotImplementedException();
        }

        private object GetInfo()
        {
            throw new NotImplementedException();
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}