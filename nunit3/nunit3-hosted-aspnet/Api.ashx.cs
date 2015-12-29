using System;
using System.IO;
using System.Web;

namespace NUnit.Hosted.AspNet
{
    /// <summary>
    /// Summary description for Api
    /// </summary>
    public class Api : IHttpHandler
    {
        private Serializer serializer;

        public Api() : this(new Serializer())
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
                var command = HttpUtility.GetLastPath(context.Request.Url);
                switch (command)
                {
                    case "run":
                        //if (context.Request.HttpMethod.Equals("GET"))
                        {
                            context.Response.ContentType = "application/json";
                            context.Response.BinaryWrite(serializer.Serialize(RunTests(context)));
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
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 500;
                context.Response.BinaryWrite(serializer.Serialize(ex));
            }
        }

        private object RunTests(IHttpContext context)
        {
            return Runner.Run(new HostedOptions
            {
                InputFiles = Path.Combine(Path.Combine(context.Request.MapPath("/"), "bin"), "TestsInWebContext.dll"),
                WorkDirectory = this.GetType().Assembly.Location,
            }, null);
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