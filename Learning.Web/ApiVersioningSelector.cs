using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Routing;

namespace Learning.Web
{
    public class ApiVersioningSelector : DefaultHttpControllerSelector
    {
        private HttpConfiguration _HttpConfiguration;
        public ApiVersioningSelector(HttpConfiguration httpConfiguration)
            : base(httpConfiguration)
        {
            _HttpConfiguration = httpConfiguration;
        }

        public override HttpControllerDescriptor SelectController(HttpRequestMessage request)
        {
            HttpControllerDescriptor controllerDescriptor = null;
            
            // get list of all controllers provided by the default selector
            IDictionary<string, HttpControllerDescriptor> controllers = GetControllerMapping();

            IHttpRouteData routeData = request.GetRouteData();

            if (routeData == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            //check if this route is actually an attribute route
            IEnumerable<IHttpRouteData> attributeSubRoutes = routeData.GetSubRoutes();

            var apiVersion = GetVersionFromMediaType(request);

            if (attributeSubRoutes == null)
            {
                string controllerName = GetRouteVariable<string>(routeData, "controller");
                if (controllerName == null)
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }

                string newControllerName = String.Concat(controllerName, "V", apiVersion);

                if (controllers.TryGetValue(newControllerName, out controllerDescriptor))
                {
                    return controllerDescriptor;
                }
                else
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }
            }
            else
            {
                // we want to find all controller descriptors whose controller type names end with
                // the following suffix (example: PeopleV1)
                string newControllerNameSuffix = String.Concat("V", apiVersion);

                //IEnumerable<HttpControllerDescriptor> descriptors = attributeSubRoutes
                //    .Select(attrRoute => GetControllerDescriptor(attrRoute)).ToList<HttpControllerDescriptor>();

                //IEnumerable<IHttpRouteData> filteredSubRoutes = attributeSubRoutes;

                IEnumerable<IHttpRouteData> filteredSubRoutes = attributeSubRoutes
                    .Where(attrRouteData =>
                    {
                        HttpControllerDescriptor currentDescriptor = GetControllerDescriptor(attrRouteData);

                        bool match = currentDescriptor.ControllerName.EndsWith(newControllerNameSuffix);

                        if (match && (controllerDescriptor == null))
                        {
                            controllerDescriptor = currentDescriptor;
                        }

                        return match;
                    });

                if (filteredSubRoutes.Count() == 0)
                {
                    var regularExpression = new Regex(@"^([a-zA-Z0-9_]+)V([0-9]+)$",
                                RegexOptions.IgnoreCase);
                    filteredSubRoutes = attributeSubRoutes
                        .Where(attrRouteData =>
                        {
                            HttpControllerDescriptor currentDescriptor = GetControllerDescriptor(attrRouteData);

                            Match match = regularExpression.Match(currentDescriptor.ControllerName);

                            if (!match.Success && (controllerDescriptor == null))
                            {
                                controllerDescriptor = currentDescriptor;
                            }

                            return !match.Success;
                        });
                }

                routeData.Values["MS_SubRoutes"] = filteredSubRoutes.ToArray();   

                if(controllerDescriptor == null)
                {
                    controllerDescriptor = GetControllerDescriptor(attributeSubRoutes.Where(route => !(((HttpActionDescriptor[])route.Route.DataTokens["actions"]).First().ControllerDescriptor.ControllerName.EndsWith(newControllerNameSuffix))).FirstOrDefault());
                }           
            }

            return controllerDescriptor;
        }

        private HttpControllerDescriptor GetControllerDescriptor(IHttpRouteData routeData)
        {
            return ((HttpActionDescriptor[])routeData.Route.DataTokens["actions"]).First().ControllerDescriptor;
        }

        // Get a value from the route data, if present.
        private static T GetRouteVariable<T>(IHttpRouteData routeData, string name)
        {
            object result = null;
            if (routeData.Values.TryGetValue(name, out result))
            {
                return (T)result;
            }
            return default(T);
        }


        //Accept: application/vnd.yournamespace.{yourresource}.v{version}+json
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
                    return "1";
                }
            }
            return "1";
        }

    }
}