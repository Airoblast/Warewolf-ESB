﻿using System;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using Dev2.Common;

namespace Dev2.Runtime.WebServer
{
    public static class Extensions
    {
        public static bool IsAuthenticated(this IPrincipal user)
        {
            if(user == null)
            {
                Dev2Logger.Log.Debug("Null User");
            }

            return user != null && user.Identity.IsAuthenticated;
        }

        public static Encoding GetContentEncoding(this HttpContent content)
        {
            var encoding = content == null ? String.Empty : content.Headers.ContentEncoding.FirstOrDefault();
            if(!String.IsNullOrEmpty(encoding))
            {
                try
                {
                    return Encoding.GetEncoding(encoding);
                }
                catch(Exception ex)
                {
                    Dev2Logger.Log.Error("Dev2.Runtime.WebServer.Extensions", ex);
                }
            }
            return Encoding.UTF8;
        }
    }
}
