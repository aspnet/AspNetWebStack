// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Threading;
using System.Web.Hosting;
using System.Web.WebPages;
using Microsoft.Internal.Web.Utils;

namespace System.Web.Mvc
{
    public abstract class BuildManagerViewEngine : VirtualPathProviderViewEngine
    {
        private static object _isPrecompiledNonUpdateableSiteInitializedLock = new object();
        private static bool _isPrecompiledNonUpdateableSite;
        private static bool _isPrecompiledNonUpdateableSiteInitialized;
        private static FileExistenceCache _sharedFileExistsCache;

        private IBuildManager _buildManager;
        private IViewPageActivator _viewPageActivator;
        private IResolver<IViewPageActivator> _activatorResolver;
        private FileExistenceCache _fileExistsCache;

        protected BuildManagerViewEngine()
            : this(null, null, null, null)
        {
        }

        protected BuildManagerViewEngine(IViewPageActivator viewPageActivator)
            : this(viewPageActivator, null, null, null)
        {
        }

        internal BuildManagerViewEngine(IViewPageActivator viewPageActivator, IResolver<IViewPageActivator> activatorResolver,
            IDependencyResolver dependencyResolver, VirtualPathProvider pathProvider)
        {
            if (viewPageActivator != null)
            {
                _viewPageActivator = viewPageActivator;
            }
            else
            {
                _activatorResolver = activatorResolver ?? new SingleServiceResolver<IViewPageActivator>(
                                                              () => null,
                                                              new DefaultViewPageActivator(dependencyResolver),
                                                              "BuildManagerViewEngine constructor");
            }

            if (pathProvider != null)
            {
                Func<VirtualPathProvider> providerFunc = () => pathProvider;
                _fileExistsCache = new FileExistenceCache(providerFunc);
                VirtualPathProviderFunc = providerFunc;
            }
            else
            {
                if (_sharedFileExistsCache == null)
                {
                    // Startup initialization race is OK providing service remains read-only
                    _sharedFileExistsCache = new FileExistenceCache(() => HostingEnvironment.VirtualPathProvider);
                }

                _fileExistsCache = _sharedFileExistsCache;
            }
        }

        internal IBuildManager BuildManager
        {
            get
            {
                if (_buildManager == null)
                {
                    _buildManager = new BuildManagerWrapper();
                }
                return _buildManager;
            }
            set { _buildManager = value; }
        }

        protected IViewPageActivator ViewPageActivator
        {
            get
            {
                if (_viewPageActivator != null)
                {
                    return _viewPageActivator;
                }
                _viewPageActivator = _activatorResolver.Current;
                return _viewPageActivator;
            }
        }

        protected virtual bool IsPrecompiledNonUpdateableSite
        {
            get
            {
                return LazyInitializer.EnsureInitialized(ref _isPrecompiledNonUpdateableSite,
                                                         ref _isPrecompiledNonUpdateableSiteInitialized,
                                                         ref _isPrecompiledNonUpdateableSiteInitializedLock,
                                                         GetPrecompiledNonUpdateable);
            }
        }

        protected override bool FileExists(ControllerContext controllerContext, string virtualPath)
        {
            // When dealing with non-updateable precompiled views, the view files may not exist on disk. The correct
            // way to check for existence of a file in this case is by querying the BuildManager.
            // For all other scenarios, checking for files on disk is faster and should suffice.
            Contract.Assert(_fileExistsCache != null);
            return _fileExistsCache.FileExists(virtualPath) ||
                   (IsPrecompiledNonUpdateableSite && BuildManager.FileExists(virtualPath));
        }

        private static bool GetPrecompiledNonUpdateable()
        {
            IVirtualPathUtility virtualPathUtility = new VirtualPathUtilityWrapper();
            return WebPages.BuildManagerWrapper.IsNonUpdateablePrecompiledApp(HostingEnvironment.VirtualPathProvider,
                                                                              virtualPathUtility);
        }

        internal class DefaultViewPageActivator : IViewPageActivator
        {
            private Func<IDependencyResolver> _resolverThunk;

            public DefaultViewPageActivator()
                : this(null)
            {
            }

            public DefaultViewPageActivator(IDependencyResolver resolver)
            {
                if (resolver == null)
                {
                    _resolverThunk = () => DependencyResolver.Current;
                }
                else
                {
                    _resolverThunk = () => resolver;
                }
            }

            public object Create(ControllerContext controllerContext, Type type)
            {
                try
                {
                    return _resolverThunk().GetService(type) ?? Activator.CreateInstance(type);
                }
                catch (MissingMethodException exception)
                {
                    // Ensure thrown exception contains the type name.  Might be down a few levels.
                    MissingMethodException replacementException =
                        TypeHelpers.EnsureDebuggableException(exception, type.FullName);
                    if (replacementException != null)
                    {
                        throw replacementException;
                    }

                    throw;
                }
            }
        }
    }
}
