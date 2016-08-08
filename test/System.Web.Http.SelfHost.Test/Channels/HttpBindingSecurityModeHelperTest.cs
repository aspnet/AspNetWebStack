// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Http.SelfHost.Channels;
using Microsoft.TestCommon;

namespace System.Net.Http.Formatting
{
    public class HttpBindingSecurityModeHelperTest : EnumHelperTestBase<HttpBindingSecurityMode>
    {
        public HttpBindingSecurityModeHelperTest()
            : base(HttpBindingSecurityModeHelper.IsDefined, HttpBindingSecurityModeHelper.Validate, (HttpBindingSecurityMode)999)
        {
        }
    }
}
