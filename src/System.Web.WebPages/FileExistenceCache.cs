// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Web.Hosting;

namespace System.Web.WebPages
{
    /// <summary>
    /// This class caches the result of VirtualPathProvider.FileExists for a short
    /// period of time, and recomputes it if necessary.
    /// 
    /// The default VPP MapPathBasedVirtualPathProvider caches the result of
    /// the FileExists call with the appropriate dependencies, so it is less
    /// expensive on subsequent calls, but it still needs to do MapPath which can 
    /// take quite some time.
    /// </summary>
    internal class FileExistenceCache
    {
        private const int TicksPerMillisecond = 10000;
        private readonly Func<VirtualPathProvider> _virtualPathProviderFunc;
        private readonly Func<string, bool> _virtualPathFileExists;
        private ConcurrentDictionary<string, bool> _cache;
        private long _creationTick;
        private int _ticksBeforeReset;

        // Overload used mainly for testing
        public FileExistenceCache(VirtualPathProvider virtualPathProvider, int milliSecondsBeforeReset = 1000)
            : this(() => virtualPathProvider, milliSecondsBeforeReset)
        {
            Contract.Assert(virtualPathProvider != null);
        }

        public FileExistenceCache(Func<VirtualPathProvider> virtualPathProviderFunc, int milliSecondsBeforeReset = 1000)
        {
            Contract.Assert(virtualPathProviderFunc != null);

            _virtualPathProviderFunc = virtualPathProviderFunc;
            _virtualPathFileExists = path => _virtualPathProviderFunc().FileExists(path);
            _ticksBeforeReset = milliSecondsBeforeReset * TicksPerMillisecond;
            Reset();
        }

        // Use the VPP returned by the HostingEnvironment unless a custom vpp is passed in (mainly for testing purposes)
        public VirtualPathProvider VirtualPathProvider
        {
            get { return _virtualPathProviderFunc(); }
        }

        public int MilliSecondsBeforeReset
        {
            get { return _ticksBeforeReset / TicksPerMillisecond; }
            internal set { _ticksBeforeReset = value * TicksPerMillisecond; }
        }

        internal IDictionary<string, bool> CacheInternal
        {
            get { return _cache; }
        }

        public bool TimeExceeded
        {
            get { return (DateTime.UtcNow.Ticks - Interlocked.Read(ref _creationTick)) > _ticksBeforeReset; }
        }

        public void Reset()
        {
            _cache = new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

            DateTime now = DateTime.UtcNow;
            long tick = now.Ticks;

            Interlocked.Exchange(ref _creationTick, tick);
        }

        public bool FileExists(string virtualPath)
        {
            if (TimeExceeded)
            {
                Reset();
            }

            return _cache.GetOrAdd(virtualPath, _virtualPathFileExists);
        }
    }
}
