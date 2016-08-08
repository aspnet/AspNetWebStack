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
    [CLSCompliant(false)]
    public abstract class HubController : HubControllerBase
    {
        private string _hubName;

        /// <summary>
        /// Initializes a new instance of the <see cref="HubController" /> class.
        /// </summary>
        /// <param name="hubName">Name of the hub as specified by the <see cref="HubNameAttribute"/>.</param>
        protected HubController(string hubName)
        {
            if (hubName == null)
            {
                throw Error.ArgumentNull("hubName");
            }
            _hubName = hubName;
        }

        /// <summary>
        /// Gets the <see cref="IHubContext"/> for the associated hub.
        /// </summary>
        protected override IHubContext HubContext
        {
            get
            {
                Contract.Assert(ConnectionManager != null);
                return ConnectionManager.GetHubContext(_hubName);
            }
        }
    }
}
