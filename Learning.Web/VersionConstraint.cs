using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Routing;

namespace Learning.Web
{
    public class VersionConstraint : IHttpRouteConstraint
    {

        public const string VersionHeaderName = "api-version";

        private const int DefaultVersion = 1;

        public int allowedVersion { get; private set; }

        public VersionConstraint(int AllowedVersion)
        {
            allowedVersion = AllowedVersion;
        }

        private int? GetVersionHeader(HttpRequestMessage request)
        {
            string versionAsString;

            IEnumerable<string> headerValues;
            
            if (request.Headers.TryGetValues(VersionHeaderName, out headerValues) && headerValues.Count() == 1)
            {
                versionAsString = headerValues.First();
            } else
            {
                versionAsString = GetVersionFromMediaType(request);
            }

            int version;
            if (versionAsString != null && Int32.TryParse(versionAsString, out version))
            {
                return version;
            }

            return null;
        }

        public bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection)
        {
            if (routeDirection == HttpRouteDirection.UriResolution)
            {
                int version = GetVersionHeader(request) ?? DefaultVersion;

                return (version == allowedVersion);
            }

            return true;
        }

        private string GetVersionFromMediaType(HttpRequestMessage request)
        {
            var acceptHeader = request.Headers.Accept;

            foreach (var mime in acceptHeader)
            {
                if (mime.MediaType == "application/json" || mime.MediaType == "application/xml")
                {
                    var version = mime.Parameters
                    .Where(v => v.Name.Equals("version", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                    if (version != null)
                    {
                        return version.Value;
                    }
                }
            }
            return null;
        }
    }
}