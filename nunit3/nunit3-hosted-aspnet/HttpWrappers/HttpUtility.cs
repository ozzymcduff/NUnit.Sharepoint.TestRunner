using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;

namespace NUnit.Hosted.AspNet
{
    public class HttpUtility
    {
        public HttpUtility()
        {
        }

        public static string GetLastPath(Uri uri)
        {
            var pieces = (uri.AbsolutePath ?? "").Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            return pieces[pieces.Length - 1];
        }
        public static NameValueCollection ParseQuery(Uri uri)
        {
            return System.Web.HttpUtility.ParseQueryString(uri.Query);
        }

        public static IHttpContext Wrap(HttpContext context)
        {
            return new HttpContextWrapper(context);
        }
    }
}

