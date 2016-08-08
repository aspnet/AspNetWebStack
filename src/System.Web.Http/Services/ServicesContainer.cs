// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Properties;

namespace System.Web.Http.Controllers
{
    // This is common to both per-controller and global config. 
    // It facilitates sharing all the mutation operations between them.
    public abstract class ServicesContainer : IDisposable
    {
        // Wrapped/composite versions of the exception service interfaces designed for consumption in catch blocks.
        // See ExceptionServices.GetLogger/Handler for how these internal services are used.
        // These instances must be stored separately and not provided via GetService because existing stores for 
        // GetService do not provide concurrency control and these two wrappers are potentially initialized late.
        internal readonly Lazy<IExceptionLogger> ExceptionServicesLogger;
        internal readonly Lazy<IExceptionHandler> ExceptionServicesHandler;

        /// <summary>Initializes a new instance of the <see cref="ServicesContainer"/> class.</summary>
        protected ServicesContainer()
        {
            ExceptionServicesLogger = new Lazy<IExceptionLogger>(CreateExceptionServicesLogger);
            ExceptionServicesHandler = new Lazy<IExceptionHandler>(CreateExceptionServicesHandler);
        }

        public abstract object GetService(Type serviceType);
        public abstract IEnumerable<object> GetServices(Type serviceType);

        // critical method for mutation operations (Add,Insert,Clear,Replace, etc)
        // This is used for multi-services. 
        // There are other abstract methods to mutate the single services.
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "expose for mutation")]
        protected abstract List<object> GetServiceInstances(Type serviceType);

        protected virtual void ResetCache(Type serviceType)
        {
        }

        /// <summary>
        /// Determine whether the service type should be fetched with GetService or GetServices. 
        /// </summary>
        /// <param name="serviceType">type of service to query</param>
        /// <returns>true iff the service is singular. </returns>
        public abstract bool IsSingleService(Type serviceType);

        /// <summary>
        /// Adds a service to the end of services list for the given service type. 
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="service">The service instance.</param>
        public void Add(Type serviceType, object service)
        {
            Insert(serviceType, Int32.MaxValue, service);
        }

        /// <summary>
        /// Adds the services of the specified collection to the end of the services list for
        /// the given service type.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="services">The services to add.</param>
        public void AddRange(Type serviceType, IEnumerable<object> services)
        {
            InsertRange(serviceType, Int32.MaxValue, services);
        }

        /// <summary>
        /// Removes all the service instances of the given service type. 
        /// </summary>
        /// <param name="serviceType">The service type to clear from the services list.</param>
        public virtual void Clear(Type serviceType)
        {
            if (serviceType == null)
            {
                throw Error.ArgumentNull("serviceType");
            }

            if (IsSingleService(serviceType))
            {
                ClearSingle(serviceType);
            }
            else
            {
                ClearMultiple(serviceType);
            }
            ResetCache(serviceType);
        }

        protected abstract void ClearSingle(Type serviceType);

        protected virtual void ClearMultiple(Type serviceType)
        {
            List<object> instances = GetServiceInstances(serviceType);
            instances.Clear();
        }

        /// <summary>
        /// Searches for a service that matches the conditions defined by the specified
        /// predicate, and returns the zero-based index of the first occurrence.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="match">The delegate that defines the conditions of the element
        /// to search for. </param>
        /// <returns>The zero-based index of the first occurrence, if found; otherwise, -1.</returns>
        public int FindIndex(Type serviceType, Predicate<object> match)
        {
            if (serviceType == null)
            {
                throw Error.ArgumentNull("serviceType");
            }
            if (match == null)
            {
                throw Error.ArgumentNull("match");
            }

            List<object> instances = GetServiceInstances(serviceType);
            return instances.FindIndex(match);
        }

        /// <summary>
        /// Inserts a service into the collection at the specified index.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="index">The zero-based index at which the service should be inserted.
        /// If <see cref="Int32.MaxValue"/> is passed, ensures the element is added to the end.</param>
        /// <param name="service">The service to insert.</param>
        public void Insert(Type serviceType, int index, object service)
        {
            if (serviceType == null)
            {
                throw Error.ArgumentNull("serviceType");
            }
            if (service == null)
            {
                throw Error.ArgumentNull("service");
            }
            if (!serviceType.IsAssignableFrom(service.GetType()))
            {
                throw Error.Argument("service", SRResources.Common_TypeMustDriveFromType, service.GetType().Name, serviceType.Name);
            }

            List<object> instances = GetServiceInstances(serviceType);
            if (index == Int32.MaxValue)
            {
                index = instances.Count;
            }

            instances.Insert(index, service);

            ResetCache(serviceType);
        }

        /// <summary>
        /// Inserts the elements of the collection into the service list at the specified index.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="index">The zero-based index at which the new elements should be inserted.
        /// If <see cref="Int32.MaxValue"/> is passed, ensures the elements are added to the end.</param>
        /// <param name="services">The collection of services to insert.</param>
        public void InsertRange(Type serviceType, int index, IEnumerable<object> services)
        {
            if (serviceType == null)
            {
                throw Error.ArgumentNull("serviceType");
            }
            if (services == null)
            {
                throw Error.ArgumentNull("services");
            }

            object[] filteredServices = services.Where(svc => svc != null).ToArray();
            object incorrectlyTypedService = filteredServices.FirstOrDefault(svc => !serviceType.IsAssignableFrom(svc.GetType()));
            if (incorrectlyTypedService != null)
            {
                throw Error.Argument("services", SRResources.Common_TypeMustDriveFromType, incorrectlyTypedService.GetType().Name, serviceType.Name);
            }

            List<object> instances = GetServiceInstances(serviceType);
            if (index == Int32.MaxValue)
            {
                index = instances.Count;
            }

            instances.InsertRange(index, filteredServices);

            ResetCache(serviceType);
        }

        /// <summary>
        /// Removes the first occurrence of the given service from the service list for the given service type.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="service">The service instance to remove.</param>
        /// <returns> <c>true</c> if the item is successfully removed; otherwise, <c>false</c>.</returns>
        public bool Remove(Type serviceType, object service)
        {
            if (serviceType == null)
            {
                throw Error.ArgumentNull("serviceType");
            }
            if (service == null)
            {
                throw Error.ArgumentNull("service");
            }

            List<object> instances = GetServiceInstances(serviceType);
            bool result = instances.Remove(service);

            ResetCache(serviceType);

            return result;
        }

        /// <summary>
        /// Removes all the elements that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="match">The delegate that defines the conditions of the elements to remove.</param>
        /// <returns>The number of elements removed from the list.</returns>
        public int RemoveAll(Type serviceType, Predicate<object> match)
        {
            if (serviceType == null)
            {
                throw Error.ArgumentNull("serviceType");
            }
            if (match == null)
            {
                throw Error.ArgumentNull("match");
            }

            List<object> instances = GetServiceInstances(serviceType);
            int result = instances.RemoveAll(match);

            ResetCache(serviceType);

            return result;
        }

        /// <summary>
        /// Removes the service at the specified index.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="index">The zero-based index of the service to remove.</param>
        public void RemoveAt(Type serviceType, int index)
        {
            if (serviceType == null)
            {
                throw Error.ArgumentNull("serviceType");
            }

            List<object> instances = GetServiceInstances(serviceType);
            instances.RemoveAt(index);

            ResetCache(serviceType);
        }

        /// <summary>
        /// Replaces all existing services for the given service type with the given
        /// service instance. This works for both singular and plural services. 
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="service">The service instance.</param>
        /// <inheritdoc/>        
        public void Replace(Type serviceType, object service)
        {
            // Check this early, so we don't call RemoveAll before Insert would catch the null service.
            if (serviceType == null)
            {
                throw Error.ArgumentNull("serviceType");
            }

            if ((service != null) && (!serviceType.IsAssignableFrom(service.GetType())))
            {
                throw Error.Argument("service", SRResources.Common_TypeMustDriveFromType, service.GetType().Name, serviceType.Name);
            }

            if (IsSingleService(serviceType))
            {
                ReplaceSingle(serviceType, service);
            }
            else
            {
                ReplaceMultiple(serviceType, service);
            }
            ResetCache(serviceType);
        }

        protected abstract void ReplaceSingle(Type serviceType, object service);

        protected virtual void ReplaceMultiple(Type serviceType, object service)
        {
            RemoveAll(serviceType, _ => true);
            Insert(serviceType, 0, service);
        }

        /// <summary>
        /// Replaces all existing services for the given service type with the given
        /// service instances.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="services">The service instances.</param>
        public void ReplaceRange(Type serviceType, IEnumerable<object> services)
        {
            // Check this early, so we don't call RemoveAll before InsertRange would catch the null services.
            if (services == null)
            {
                throw Error.ArgumentNull("services");
            }

            RemoveAll(serviceType, _ => true);
            InsertRange(serviceType, 0, services);
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "Although this class is not sealed, end users cannot set instances of it so in practice it is sealed.")]
        [SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly", Justification = "Although this class is not sealed, end users cannot set instances of it so in practice it is sealed.")]
        public virtual void Dispose()
        {
        }

        private IExceptionLogger CreateExceptionServicesLogger()
        {
            return ExceptionServices.CreateLogger(this);
        }

        private IExceptionHandler CreateExceptionServicesHandler()
        {
            return ExceptionServices.CreateHandler(this);
        }
    }
}