// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc.Properties;

namespace System.Web.Mvc
{
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "Unsealed because type contains virtual extensibility points.")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class RequireHttpsAttribute : FilterAttribute, IAuthorizationFilter
    {
        public RequireHttpsAttribute()
            : this(permanent: false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequireHttpsAttribute"/> class.
        /// </summary>
        /// <param name="permanent">Whether the redirect to HTTPS should be a permanent redirect.</param>
        public RequireHttpsAttribute(bool permanent)
        {
            this.Permanent = permanent;
        }

        /// <summary>
        /// Gets a value indicating whether the redirect to HTTPS should be a permanent redirect.
        /// </summary>
        public bool Permanent { get; private set; }

        public virtual void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            if (!filterContext.HttpContext.Request.IsSecureConnection)
            {
                HandleNonHttpsRequest(filterContext);
            }
        }

        protected virtual void HandleNonHttpsRequest(AuthorizationContext filterContext)
        {
            // only redirect for GET requests, otherwise the browser might not propagate the verb and request
            // body correctly.

            if (!String.Equals(filterContext.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(MvcResources.RequireHttpsAttribute_MustUseSsl);
            }

            // redirect to HTTPS version of page
            string url = "https://" + filterContext.HttpContext.Request.Url.Host + filterContext.HttpContext.Request.RawUrl;
            filterContext.Result = new RedirectResult(url, this.Permanent);
        }
    }
}
