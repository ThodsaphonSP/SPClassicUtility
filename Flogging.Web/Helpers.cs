using Flogging.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Claims;
using System.Web;

namespace Flogging.Web
{
    public static class Helpers
    {
        public static void LogWebUsage(string product, string layer, string activityName,
            Dictionary<string, object> additionalInfo = null)
        {
            string userId, userName, location;
            var webInfo = GetWebFloggingData(out userId, out userName, out location);

            if (additionalInfo != null)
            {
                foreach (var key in additionalInfo.Keys)
                    webInfo.Add($"Info-{key}", additionalInfo[key]);
            }

            var usageInfo = new FlogDetail()
            {
                Product = product,
                Layer = layer,
                Location = location,
                UserId = userId,
                UserName = userName,
                Hostname = Environment.MachineName,
                CorrelationId = HttpContext.Current.Session.SessionID,
                Message = activityName,
                AdditionalInfo = webInfo
            };

            Flogger.WriteUsage(usageInfo);
        }

        public static void LogWebDiagnostic(string product, string layer, string message,
            Dictionary<string, object> diagnosticInfo = null)
        {
            var writeDiagnostics = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableDiagnostics"]);
            if (!writeDiagnostics)  // doing this to avoid going through all the data - user, session, etc.
                return;

            string userId, userName, location;
            var webInfo = GetWebFloggingData(out userId, out userName, out location);
            if (diagnosticInfo != null)
            {
                foreach (var key in diagnosticInfo.Keys)
                    webInfo.Add(key, diagnosticInfo[key]);
            }

            var diagInfo = new FlogDetail()
            {
                Product = product,
                Layer = layer,
                Location = location,
                UserId = userId,
                UserName = userName,
                Hostname = Environment.MachineName,
                CorrelationId = HttpContext.Current.Session.SessionID,
                Message = message,
                AdditionalInfo = webInfo
            };

            Flogger.WriteDiagnostic(diagInfo);
        }

        public static void LogWebError(string product, string layer, Exception ex)
        {
            string userId, userName, location;
            var webInfo = GetWebFloggingData(out userId, out userName, out location);

            var errorInformation = new FlogDetail()
            {
                Product = product,
                Layer = layer,
                Location = location,
                UserId = userId,
                UserName = userName,
                Hostname = Environment.MachineName,
                CorrelationId = HttpContext.Current.Session?.SessionID,
                Exception = ex,
                AdditionalInfo = webInfo
            };

            Flogger.WriteError(errorInformation);
        }

        public static void GetHttpStatus(Exception ex, out int httpStatus)
        {
            httpStatus = 500;  // default is server error
            if (ex is HttpException)
            {
                var httpEx = ex as HttpException;
                httpStatus = httpEx.GetHttpCode();
            }
        }



        // Add Assemblies ref to Flogger.Core and System.Web
        // Add NuGet package for Microsoft.AspNet.Mvc
        public static Dictionary<string, object> GetWebFloggingData(out string userId,
            out string userName, out string location)
        {
            var data = new Dictionary<string, object>();

            GetRequestData(data, out location);
            GetUserData(data, out userId, out userName);
            GetSessionData(data);
            // got cookies?  

            return data;
        }

        private static void GetRequestData(Dictionary<string, object> data, out string location)
        {
            location = "";
            var request = HttpContext.Current.Request;
            // rich object - you may want to explore this
            if (request != null)
            {
                location = request.Path;

                string type, version;
                // MS Edge requires special detection logic
                GetBrowserInfo(request, out type, out version);
                data.Add("Browser", $"{type}{version}");
                data.Add("UserAgent", request.UserAgent);
                data.Add("Languages", request.UserLanguages);  // non en-US preferences here??
                foreach (var qsKey in request.QueryString.Keys)
                {
                    data.Add(string.Format("QueryString-{0}", qsKey),
                        request.QueryString[qsKey.ToString()]);
                }
            }
        }
        private static void GetBrowserInfo(HttpRequest request, out string type, out string version)
        {
            type = request.Browser.Type;
            if (type.StartsWith("Chrome") && request.UserAgent.Contains("Edge/"))
            {
                type = "Edge";
                version = " (v " + request.UserAgent
                    .Substring(request.UserAgent.IndexOf("Edge/") + 5) + ")";
            }
            else
            {
                version = " (v " + request.Browser.MajorVersion + "." +
                    request.Browser.MinorVersion + ")";
            }
        }

        private static void GetUserData(Dictionary<string, object> data, out string userId,
            out string userName)
        {
            userId = "";
            userName = "";
            var user = ClaimsPrincipal.Current;
            if (user != null)
            {
                var i = 1; // i included in dictionary key to ensure uniqueness
                foreach (var claim in user.Claims)
                {
                    if (claim.Type == ClaimTypes.NameIdentifier)
                        userId = claim.Value;
                    else if (claim.Type == ClaimTypes.Name)
                        userName = claim.Value;
                    else
                        // example dictionary key: UserClaim-4-role 
                        data.Add(string.Format("UserClaim-{0}-{1}", i++, claim.Type), claim.Value);
                }
            }
        }
        private static void GetSessionData(Dictionary<string, object> data)
        {
            if (HttpContext.Current.Session != null)
            {
                foreach (var key in HttpContext.Current.Session.Keys)
                {
                    var keyName = key.ToString();
                    if (HttpContext.Current.Session[keyName] != null)
                    {
                        data.Add(string.Format("Session-{0}", keyName),
                            HttpContext.Current.Session[keyName].ToString());
                    }
                }
                data.Add("SessionId", HttpContext.Current.Session.SessionID);
            }
        }
    }
}
