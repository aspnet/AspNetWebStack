// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Web.Http.Properties;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Infrastructure;

namespace System.Web.Http
{
    /// <summary>
    /// Defines a base class for <see cref="ApiController"/> that exposes functionality for calling back
    /// to clients connected to a particular SignalR hub.
    /// </summary>
    [CLSCompliant(false)]
    public abstract class HubControllerBase : ApiController
    {
        private IConnectionManager _connectionManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="HubControllerBase" /> class.
        /// </summary>
        protected HubControllerBase()
        {
        }

        /// <summary>
        /// Gets an <see cref="IHubConnectionContext"/> that represents the clients connected to the hub.
        /// </summary>
        public IHubConnectionContext Clients
        {
            get
            {
                if (HubContext == null)
                {
                    throw Error.InvalidOperation(SRResources.NullHubContext, GetType().Name);
                }
                return HubContext.Clients;
            }
        }

        /// <summary>
        /// Gets the <see cref="IGroupManager"/> for the hub.
        /// </summary>
        public IGroupManager Groups
        {
            get
            {
                if (HubContext == null)
                {
                    throw Error.InvalidOperation(SRResources.NullHubContext, GetType().Name);
                }
                return HubContext.Groups;
            }
        }

        /// <summary>
        /// Gets the <see cref="IHubContext"/> for the associated hub.
        /// </summary>
        protected abstract IHubContext HubContext
        {
            get;
        }

        /// <summary>
        /// Gets the <see cref="IConnectionManager"/> used to resolve hub contexts. The connection manager will
        /// be resolved by the controller's <see cref="HttpConfiguration"/> dependency resolver if the service is
        /// available. Otherwise, the default GlobalHost <see cref="IConnectionManager"/> will be returned instead.
        /// </summary>
        protected IConnectionManager ConnectionManager
        {
            get
            {
                if (_connectionManager == null)
                {
                    _connectionManager = ResolveConnectionManager();
                }
                return _connectionManager;
            }
        }

        private IConnectionManager ResolveConnectionManager()
        {
            if (Configuration != null)
            {
                Contract.Assert(Configuration.DependencyResolver != null);
                IConnectionManager connectionManager = Configuration.DependencyResolver.GetService(typeof(IConnectionManager)) as IConnectionManager;
                if (connectionManager != null)
                {
                    return connectionManager;
                }
            }

            // If connection manager cannot be resolved by DependencyResolver, use the default connection manager instead.
            return GlobalHost.ConnectionManager;
        }
    }
}