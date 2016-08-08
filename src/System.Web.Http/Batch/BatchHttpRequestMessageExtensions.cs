// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;

namespace System.Web.Http.Batch
{
    /// <summary>
    /// Provides extension methods for the <see cref="HttpRequestMessage"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class BatchHttpRequestMessageExtensions
    {
        private const string HttpBatchContextKey = "MS_HttpBatchContext";

        private static readonly string[] BatchRequestPropertyExclusions =
        {
            HttpPropertyKeys.HttpRouteDataKey,
            HttpPropertyKeys.DisposableRequestResourcesKey,
            HttpPropertyKeys.SynchronizationContextKey,
            HttpPropertyKeys.HttpConfigurationKey,
            HttpRoute.RoutingContextKey,
            HttpBatchContextKey
        };

        /// <summary>
        /// Copies the properties from another <see cref="HttpRequestMessage"/>.
        /// </summary>
        /// <param name="subRequest">The sub-request.</param>
        /// <param name="batchRequest">The batch request that contains the properties to copy.</param>
        public static void CopyBatchRequestProperties(this HttpRequestMessage subRequest, HttpRequestMessage batchRequest)
        {
            if (subRequest == null)
            {
                throw new ArgumentNullException("subRequest");
            }
            if (batchRequest == null)
            {
                throw new ArgumentNullException("batchRequest");
            }

            foreach (KeyValuePair<string, object> property in batchRequest.Properties)
            {
                if (!BatchRequestPropertyExclusions.Contains(property.Key))
                {
                    subRequest.Properties.Add(property);
                }
            }

            HttpRequestContext originalContext = subRequest.GetRequestContext();

            if (originalContext != null)
            {
                BatchHttpRequestContext subRequestContext = new BatchHttpRequestContext(originalContext)
                {
                    Url = new UrlHelper(subRequest)
                };

                subRequest.SetRequestContext(subRequestContext);
            }
        }
    }
}