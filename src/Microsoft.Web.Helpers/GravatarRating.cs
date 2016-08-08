// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Web.Helpers
{
    public enum GravatarRating
    {
        Default,
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "G", Justification = "Matches the gravatar.com rating. Suppressed in source because this is a one-time occurrence")]
        G,
        PG,
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "R", Justification = "Matches the gravatar.com rating. Suppressed in source because this is a one-time occurrence")]
        R,
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "X", Justification = "Matches the gravatar.com rating. Suppressed in source because this is a one-time occurrence")]
        X
    }
}
