// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Microsoft.Owin;

namespace System.Web.Http.Owin
{
    internal static class OwinResponseExtensions
    {
        private const string DisableResponseBufferingKey = "server.DisableResponseBuffering";

        public static void DisableBuffering(this IOwinResponse response)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }

            IDictionary<string, object> environment = response.Environment;

            if (environment == null)
            {
                return;
            }

            Action action;

            if (!environment.TryGetValue(DisableResponseBufferingKey, out action))
            {
                return;
            }

            Contract.Assert(action != null);
            action.Invoke();
        }
    }
}
