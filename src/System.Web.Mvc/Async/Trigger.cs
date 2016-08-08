// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Mvc.Async
{
    // Provides a trigger for the TriggerListener class.

    internal sealed class Trigger
    {
        private readonly Action _fireAction;

        // Constructor should only be called by TriggerListener.
        internal Trigger(Action fireAction)
        {
            _fireAction = fireAction;
        }

        public void Fire()
        {
            _fireAction();
        }
    }
}
