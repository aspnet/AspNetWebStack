// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace WebMatrix.WebData
{
    /// <summary>
    /// Configures the behavior of SimpleMembershipProvider for the casing of user name queries.
    /// </summary>
    public enum SimpleMembershipProviderCasingBehavior
    {
        /// <summary>
        /// Uses the SQL Upper function to normalize the casing of user names for a case-insensitive comparison. 
        /// This is the default value.
        /// </summary>
        /// <remarks>
        /// This option uses the SQL Upper function to perform case-normalization. This guarantees that the 
        /// the user name is searched case-insensitively, but can have a performance impact when a large number
        /// of users exist.
        /// </remarks>
        NormalizeCasing,

        /// <summary>
        /// Relies on the database's configured collation to normalize casing for the comparison of user names. User
        /// names are provided to the database exactly as entered by the user.
        /// </summary>
        /// <remarks>
        /// This option relies on the configured collection of database table for user names to perform a correct comparison.
        /// This is guaranteed to be correct for the chosen collation and performant. Only choose this option if the table storing
        /// user names is configured with the desired collation.
        /// </remarks>
        RelyOnDatabaseCollation,
    }
}
