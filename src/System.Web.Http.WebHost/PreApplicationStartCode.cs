// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;

namespace System.Web.Http.WebHost
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Use of this type is not recommended because it no longer has initialization logic.")]
    public static class PreApplicationStartCode
    {
        private static bool _startWasCalled;

        public static void Start()
        {
            // Guard against multiple calls. All Start calls are made on same thread, so no lock needed here
            if (_startWasCalled)
            {
                return;
            }
            _startWasCalled = true;

            // We need to keep this for App-Compat reasons. Prior to 4.5 we registered a module here.
        }
    }
}
