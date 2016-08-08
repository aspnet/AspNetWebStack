// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;

namespace Microsoft.Web.Mvc.Controls.Test
{
    public class DesignModeSite : ISite
    {
        IComponent ISite.Component
        {
            get { throw new NotImplementedException(); }
        }

        IContainer ISite.Container
        {
            get { throw new NotImplementedException(); }
        }

        bool ISite.DesignMode
        {
            get { return true; }
        }

        string ISite.Name
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        object IServiceProvider.GetService(Type serviceType)
        {
            throw new NotImplementedException();
        }
    }
}
