// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;

namespace Microsoft.TestCommon
{
    /// <summary>
    /// This class allocates ports while ensuring that:
    /// 1. Ports that are permanently taken (or taken for the duration of the test) are not being attempted to be used.
    /// 2. Ports are not shared across different tests (but you can allocate two different ports in the same test).
    ///
    /// Gotcha: If another application grabs a port during the test, we have a race condition.
    /// </summary>
    [DebuggerDisplay("Port: {PortNumber}, Port count for this app domain: {_appDomainOwnedPorts.Count}")]
    public class PortReserver : IDisposable
    {
        private Mutex _portMutex;
        private Thread _acquiredOn;

        // We use this list to hold on to all the ports used because the Mutex will be blown through on the same thread.
        // Theoretically we can do a thread local HashSet, but that makes dispose thread-dependent, or requires more complicated concurrency checks.
        // Since practically there is no perf issue or concern here, this keeps the code the simplest possible.
        private static HashSet<int> _appDomainOwnedPorts = new HashSet<int>();

        public int PortNumber
        {
            get;
            private set;
        }

        public PortReserver(int basePort = 50231)
        {
            if (basePort <= 0)
            {
                throw new ArgumentOutOfRangeException("basePort", "Argument must be greater than 0.");
            }

            // Grab a cross appdomain/cross process/cross thread lock, to ensure only one port is reserved at a time.
            using (Mutex mutex = GetGlobalMutex())
            {
                try
                {
                    var usedTCPPorts = new HashSet<int>();
                    foreach (var endPoint in ListUsedTCPPort())
                    {
                        usedTCPPorts.Add(endPoint.Port);
                    }

                    int port = basePort - 1;
                    while (true)
                    {
                        port++;

                        if (port > 65535)
                        {
                            throw new InvalidOperationException("Exceeded port range");
                        }

                        // AppDomainOwnedPorts check enables reserving two ports from the same thread in sequence.
                        // ListUsedTCPPort prevents port contention with other apps.
                        if (_appDomainOwnedPorts.Contains(port) || usedTCPPorts.Contains(port))
                        {
                            continue;
                        }

                        // Create a well known mutex
                        string mutexName = "WebStack-Port-" + port.ToString(CultureInfo.InvariantCulture);
                        _portMutex = new Mutex(initiallyOwned: false, name: mutexName);

                        // If no one else is using this port grab it.
                        if (_portMutex.WaitOne(millisecondsTimeout: 0))
                        {
                            _acquiredOn = Thread.CurrentThread;
                            break;
                        }

                        // dispose this mutex since the port it represents is not available.
                        _portMutex.Dispose();
                        _portMutex = null;
                    }

                    PortNumber = port;
                    _appDomainOwnedPorts.Add(port);
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }

        public string BaseUri
        {
            get
            {
                return String.Format(CultureInfo.InvariantCulture, "http://localhost:{0}/", PortNumber);
            }
        }

        public void Dispose()
        {
            if (PortNumber == -1)
            {
                // Object already disposed
                return;
            }

            using (Mutex mutex = GetGlobalMutex())
            {
                try
                {
                    using (_portMutex)
                    {
                        if (_acquiredOn == Thread.CurrentThread)
                        {
                            _portMutex.ReleaseMutex();
                        }

                        _portMutex = null;
                    }

                    _appDomainOwnedPorts.Remove(PortNumber);
                    PortNumber = -1;
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }

        private static Mutex GetGlobalMutex()
        {
            const int timeoutInSeconds = 20;

            Mutex mutex = new Mutex(initiallyOwned: false, name: "WebStack-RandomPortAcquisition");
            if (!mutex.WaitOne(TimeSpan.FromSeconds(timeoutInSeconds)))
            {
                throw new InvalidOperationException(
                    String.Format("Unable to reserve global Mutex within {0} seconds.", timeoutInSeconds));
            }

            return mutex;
        }

        private static IPEndPoint[] ListUsedTCPPort()
        {
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();

            return ipGlobalProperties.GetActiveTcpListeners()
                .Concat(ipGlobalProperties.GetActiveUdpListeners())
                .ToArray();
        }
    }
}
