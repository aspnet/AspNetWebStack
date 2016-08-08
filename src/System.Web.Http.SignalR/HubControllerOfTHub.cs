// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace System.Web.Http
{
    /// <summary>
    /// Defines a base class for <see cref="ApiController"/> that exposes functionality for calling back
    /// to clients connected to a particular SignalR hub.
    /// </summary>
    /// <typeparam name="THub">The type of the hub. Must implement the <see cref="IHub"/> interface.</typeparam>
    [CLSCompliant(false)]
    public abstract class HubController<THub> : HubControllerBase where THub : IHub
    {
        /// <summary>
        /// Gets the <see cref="IHubContext"/> for the associated hub.
        /// </summary>
        protected override IHubContext HubContext
        {
            get
            {
                Contract.Assert(ConnectionManager != null);
                return ConnectionManager.GetHubContext<THub>();
            }
        }
    }
}
