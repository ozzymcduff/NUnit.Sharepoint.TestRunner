using System;
using System.Web;
using System.Web.UI;

namespace nunit2hostedaspnet
{
    
    public class Api : System.Web.IHttpHandler
    {
    
        public void ProcessRequest (HttpContext context)
        {
            
        }

        public bool IsReusable {
            get {
                return false;
            }
        }
    }
}
    
