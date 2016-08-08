// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Threading;
using System.Threading.Tasks;

namespace System.Web.Http
{
    public class CustomMessageHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            IPrincipal originalPrincipal = Thread.CurrentPrincipal;

            // here you can see the requestor's identity via the request message
            // convert the Generic Identity to some IPrincipal object, and set it in the request's property
            // later the authorization filter will use the role information to authorize request.
            SecurityMessageProperty property = request.GetSecurityMessageProperty();
            if (property != null)
            {
                ServiceSecurityContext context = property.ServiceSecurityContext;

                if (context.PrimaryIdentity.Name == "username")
                {
                    Thread.CurrentPrincipal = new GenericPrincipal(context.PrimaryIdentity, new string[] { "Administrators" });
                }
            }

            try
            {
                return await base.SendAsync(request, cancellationToken);
            }
            finally
            {
                Thread.CurrentPrincipal = originalPrincipal;
            }
        }
    }
}
