// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Facebook.Test.Types
{
    public class UserTypeWithFieldModifiers
    {
        public string Id { get; set; }
        public string Name { get; set; }

        [FacebookFieldModifier("type(large)")]
        public FacebookConnection<FacebookPicture> Picture { get; set; }

        [FacebookFieldModifier("limit(5)")]
        public FacebookGroupConnection<SimpleUser> Friends { get; set; }
    }
}
