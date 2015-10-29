using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Routing;

namespace Learning.Web
{
    internal class VersionedRoute : RouteFactoryAttribute
    {

        public int allowedVersion { get; private set; }

        public VersionedRoute(string template, int AllowedVersion, string Name = "")
            : base(template)
        {
            allowedVersion = AllowedVersion;
            if(!string.IsNullOrEmpty(Name))
            {
                this.Name = Name;
            }
        }

        public override IDictionary<string, object> Constraints
        {
            get
            {
                var constraints = new HttpRouteValueDictionary();
                constraints.Add("version", new VersionConstraint(allowedVersion));
                return constraints;
            }
        }
    }
}