// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.WebPages.TestUtils;
using Microsoft.TestCommon;

namespace System.Web.WebPages.Test
{
    public class WebPageHttpModuleTest
    {
        [Fact]
        public void InitializeApplicationTest()
        {
            AppDomainUtils.RunInSeparateAppDomain(() =>
            {
                var moduleEvents = new ModuleEvents();
                var app = new MyHttpApplication();
                WebPageHttpModule.InitializeApplication(app,
                                                        moduleEvents.OnApplicationPostResolveRequestCache,
                                                        moduleEvents.Initialize);
                Assert.True(moduleEvents.CalledInitialize);
            });
        }

        [Fact]
        public void StartApplicationTest()
        {
            AppDomainUtils.RunInSeparateAppDomain(() =>
            {
                var moduleEvents = new ModuleEvents();
                var app = new MyHttpApplication();
                WebPageHttpModule.StartApplication(app, moduleEvents.ExecuteStartPage, moduleEvents.ApplicationStart);
                Assert.Equal(1, moduleEvents.CalledExecuteStartPage);
                Assert.Equal(1, moduleEvents.CalledApplicationStart);

                // Call a second time to make sure the methods are only called once
                WebPageHttpModule.StartApplication(app, moduleEvents.ExecuteStartPage, moduleEvents.ApplicationStart);
                Assert.Equal(1, moduleEvents.CalledExecuteStartPage);
                Assert.Equal(1, moduleEvents.CalledApplicationStart);
            });
        }

        public class MyHttpApplication : HttpApplication
        {
            public MyHttpApplication()
            {
            }
        }

        public class ModuleEvents
        {
            public void OnApplicationPostResolveRequestCache(object sender, EventArgs e)
            {
            }

            public void OnBeginRequest(object sender, EventArgs e)
            {
            }

            public void OnEndRequest(object sender, EventArgs e)
            {
            }

            public bool CalledInitialize;

            public void Initialize(object sender, EventArgs e)
            {
                CalledInitialize = true;
            }

            public int CalledExecuteStartPage;

            public void ExecuteStartPage(HttpApplication application)
            {
                CalledExecuteStartPage++;
            }

            public int CalledApplicationStart;

            public void ApplicationStart(object sender, EventArgs e)
            {
                CalledApplicationStart++;
            }
        }
    }
}
