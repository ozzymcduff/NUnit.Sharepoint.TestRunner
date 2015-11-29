using System;
using System.Web;

namespace NUnit.Hosted.AspNet
{
    public class HttpUtility
    {
        public HttpUtility()
        {
        }

        public static string GetLastPath(string path)
        {
            var pieces = (path ?? "").Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            return pieces[pieces.Length - 1];
        }

        public static IHttpContext Wrap(HttpContext context)
        {
            return new HttpContextWrapper(context);
        }
    }
}

