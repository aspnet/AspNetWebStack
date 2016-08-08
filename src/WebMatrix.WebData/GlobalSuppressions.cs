// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the 
// Error List, point to "Suppress Message(s)", and click 
// "In Project Suppression File".
// You do not need to add suppressions to this file manually.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "Username", Scope = "member", Target = "WebMatrix.WebData.ExtendedMembershipProvider.#GetUsernameFromId(System.Int32)", Justification = "Username is one word.")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "username", Scope = "member", Target = "WebMatrix.WebData.ExtendedMembershipProvider.#GetAccountsForUser(System.String)", Justification = "Username is one word.")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "username", Scope = "member", Target = "WebMatrix.WebData.ExtendedMembershipProvider.#CreateOrUpdateOAuthAccount(System.String,System.String,System.String)", Justification = "Username is one word.")]
