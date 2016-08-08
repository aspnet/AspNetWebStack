// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Helpers.AntiXsrf.Test
{
    // An IAntiForgeryTokenSerializer that can be passed to MoQ.
    public abstract class MockableAntiForgeryTokenSerializer : IAntiForgeryTokenSerializer
    {
        public abstract object Deserialize(string serializedToken);
        public abstract string Serialize(object token);

        AntiForgeryToken IAntiForgeryTokenSerializer.Deserialize(string serializedToken)
        {
            return (AntiForgeryToken)Deserialize(serializedToken);
        }

        string IAntiForgeryTokenSerializer.Serialize(AntiForgeryToken token)
        {
            return Serialize(token);
        }
    }
}
