// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;

#if ASPNETWEBAPI
using TRouteDictionary = System.Collections.Generic.IDictionary<string, object>;
#else
using TRouteDictionary = System.Web.Routing.RouteValueDictionary;
#endif

#if ASPNETWEBAPI
namespace System.Web.Http.Routing
#else
namespace System.Web.Mvc.Routing
#endif
{
    /// <summary>Represents an attribute route that may contain custom constraints.</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public abstract class RouteFactoryAttribute : Attribute, IDirectRouteFactory
    {
        private readonly string _template;

        /// <summary>Initializes a new instance of the <see cref="RouteFactoryAttribute"/> class.</summary>
        /// <param name="template">The route template.</param>
        protected RouteFactoryAttribute(string template)
        {
            _template = template;
        }

        /// <summary>Gets the route template.</summary>
        public string Template
        {
            get { return _template; }
        }

        /// <summary>Gets or sets the route name, if any; otherwise <see langword="null"/>.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the route order.</summary>
        public int Order { get; set; }

        /// <summary>Gets the route defaults, if any; otherwise <see langword="null"/>.</summary>
        public virtual TRouteDictionary Defaults
        {
            get { return null; }
        }

        /// <summary>Gets the route constraints, if any; otherwise <see langword="null"/>.</summary>
        public virtual TRouteDictionary Constraints
        {
            get { return null; }
        }

        /// <summary>Gets the route data tokens, if any; otherwise <see langword="null"/>.</summary>
        public virtual TRouteDictionary DataTokens
        {
            get { return null; }
        }

        /// <inheritdoc />
        public RouteEntry CreateRoute(DirectRouteFactoryContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            IDirectRouteBuilder builder = context.CreateBuilder(Template);
            Contract.Assert(builder != null);
            builder.Name = Name;
            builder.Order = Order;

            TRouteDictionary builderDefaults = builder.Defaults;

            if (builderDefaults == null)
            {
                builder.Defaults = Defaults;
            }
            else
            {
                TRouteDictionary defaults = Defaults;

                if (defaults != null)
                {
                    foreach (KeyValuePair<string, object> defaultItem in defaults)
                    {
                        builderDefaults[defaultItem.Key] = defaultItem.Value;
                    }
                }
            }

            TRouteDictionary builderConstraints = builder.Constraints;

            if (builderConstraints == null)
            {
                builder.Constraints = Constraints;
            }
            else
            {
                TRouteDictionary constraints = Constraints;

                if (constraints != null)
                {
                    foreach (KeyValuePair<string, object> constraint in constraints)
                    {
                        builderConstraints[constraint.Key] = constraint.Value;
                    }
                }
            }

            TRouteDictionary builderDataTokens = builder.DataTokens;

            if (builderDataTokens == null)
            {
                builder.DataTokens = DataTokens;
            }
            else
            {
                TRouteDictionary dataTokens = DataTokens;

                if (dataTokens != null)
                {
                    foreach (KeyValuePair<string, object> dataToken in dataTokens)
                    {
                        builderDataTokens[dataToken.Key] = dataToken.Value;
                    }
                }
            }

            return builder.Build();
        }
    }
}
