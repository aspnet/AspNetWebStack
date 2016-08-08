// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Provider;
using System.Globalization;
using System.Linq;
using System.Web.Security;
using WebMatrix.WebData.Resources;

namespace WebMatrix.WebData
{
    public class SimpleRoleProvider : RoleProvider
    {
        private RoleProvider _previousProvider;
        private SimpleMembershipProviderCasingBehavior _casingBehavior;

        public SimpleRoleProvider()
            : this(null)
        {
        }

        public SimpleRoleProvider(RoleProvider previousProvider)
        {
            _previousProvider = previousProvider;
        }

        private RoleProvider PreviousProvider
        {
            get
            {
                if (_previousProvider == null)
                {
                    throw new InvalidOperationException(WebDataResources.Security_InitializeMustBeCalledFirst);
                }
                else
                {
                    return _previousProvider;
                }
            }
        }

        private string SafeUserTableName
        {
            get { return "[" + UserTableName + "]"; }
        }

        private string SafeUserNameColumn
        {
            get { return "[" + UserNameColumn + "]"; }
        }

        private string SafeUserIdColumn
        {
            get { return "[" + UserIdColumn + "]"; }
        }

        internal static string RoleTableName
        {
            get { return "webpages_Roles"; }
        }

        internal static string UsersInRoleTableName
        {
            get { return "webpages_UsersInRoles"; }
        }

        // represents the User table for the app
        public string UserTableName { get; set; }

        // represents the User created UserName column, i.e. Email
        public string UserNameColumn { get; set; }

        // Represents the User created id column, i.e. ID;
        // REVIEW: we could get this from the primary key of UserTable in the future
        public string UserIdColumn { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="WebMatrix.WebData.SimpleMembershipProviderCasingBehavior"/> for this provider.
        /// </summary>
        /// <remarks>
        /// This value configures whether or not queries for user names normalize the user name to uppercase. See
        /// <see cref="WebMatrix.WebData.SimpleMembershipProviderCasingBehavior"/> for a full description.
        /// </remarks>
        public SimpleMembershipProviderCasingBehavior CasingBehavior
        {
            get
            {
                return _casingBehavior;
            }
            set
            {
                if (value < SimpleMembershipProviderCasingBehavior.NormalizeCasing || value > SimpleMembershipProviderCasingBehavior.RelyOnDatabaseCollation)
                {
                    throw new InvalidEnumArgumentException("value", (int)value, typeof(SimpleMembershipProviderCasingBehavior));
                }

                _casingBehavior = value;
            }
        }

        internal DatabaseConnectionInfo ConnectionInfo { get; set; }
        internal bool InitializeCalled { get; set; }

        // Inherited from RoleProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override string ApplicationName
        {
            get
            {
                if (InitializeCalled)
                {
                    throw new NotSupportedException();
                }
                else
                {
                    return PreviousProvider.ApplicationName;
                }
            }
            set
            {
                if (InitializeCalled)
                {
                    throw new NotSupportedException();
                }
                else
                {
                    PreviousProvider.ApplicationName = value;
                }
            }
        }

        private void VerifyInitialized()
        {
            if (!InitializeCalled)
            {
                throw new InvalidOperationException(WebDataResources.Security_InitializeMustBeCalledFirst);
            }
        }

        private IDatabase ConnectToDatabase()
        {
            return new DatabaseWrapper(ConnectionInfo.Connect());
        }

        internal void CreateTablesIfNeeded()
        {
            using (var db = ConnectToDatabase())
            {
                if (!SimpleMembershipProvider.CheckTableExists(db, RoleTableName))
                {
                    db.Execute(@"CREATE TABLE " + RoleTableName + @" (
                        RoleId                                  int                 NOT NULL PRIMARY KEY IDENTITY,
                        RoleName                                nvarchar(256)       NOT NULL UNIQUE)");

                    db.Execute(@"CREATE TABLE " + UsersInRoleTableName + @" (
                        UserId                                  int                 NOT NULL,
                        RoleId                                  int                 NOT NULL,
                        PRIMARY KEY (UserId, RoleId),
                        CONSTRAINT fk_UserId FOREIGN KEY (UserId) REFERENCES " + SafeUserTableName + "(" + SafeUserIdColumn + @"),
                        CONSTRAINT fk_RoleId FOREIGN KEY (RoleId) REFERENCES " + RoleTableName + "(RoleId) )");
                }
            }
        }

        private List<int> GetUserIdsFromNames(IDatabase db, string[] usernames)
        {
            List<int> userIds = new List<int>(usernames.Length);
            foreach (string username in usernames)
            {
                int id = SimpleMembershipProvider.GetUserId(db, SafeUserTableName, SafeUserNameColumn, SafeUserIdColumn, CasingBehavior, username);
                if (id == -1)
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, WebDataResources.Security_NoUserFound, username));
                }
                userIds.Add(id);
            }
            return userIds;
        }

        private static List<int> GetRoleIdsFromNames(IDatabase db, string[] roleNames)
        {
            List<int> roleIds = new List<int>(roleNames.Length);
            foreach (string role in roleNames)
            {
                int id = FindRoleId(db, role);
                if (id == -1)
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, WebDataResources.SimpleRoleProvider_NoRoleFound, role));
                }
                roleIds.Add(id);
            }
            return roleIds;
        }

        // Inherited from RoleProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            if (!InitializeCalled)
            {
                PreviousProvider.AddUsersToRoles(usernames, roleNames);
            }
            else
            {
                using (var db = ConnectToDatabase())
                {
                    int userCount = usernames.Length;
                    int roleCount = roleNames.Length;
                    List<int> userIds = GetUserIdsFromNames(db, usernames);
                    List<int> roleIds = GetRoleIdsFromNames(db, roleNames);

                    // Generate a INSERT INTO for each userid/rowid combination, where userIds are the first params, and roleIds follow
                    for (int uId = 0; uId < userCount; uId++)
                    {
                        for (int rId = 0; rId < roleCount; rId++)
                        {
                            if (IsUserInRole(usernames[uId], roleNames[rId]))
                            {
                                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, WebDataResources.SimpleRoleProvder_UserAlreadyInRole, usernames[uId], roleNames[rId]));
                            }

                            // REVIEW: is there a way to batch up these inserts?
                            int rows = db.Execute("INSERT INTO " + UsersInRoleTableName + " VALUES (" + userIds[uId] + "," + roleIds[rId] + "); ");
                            if (rows != 1)
                            {
                                throw new ProviderException(WebDataResources.Security_DbFailure);
                            }
                        }
                    }
                }
            }
        }

        // Inherited from RoleProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override void CreateRole(string roleName)
        {
            if (!InitializeCalled)
            {
                PreviousProvider.CreateRole(roleName);
            }
            else
            {
                using (var db = ConnectToDatabase())
                {
                    int roleId = FindRoleId(db, roleName);
                    if (roleId != -1)
                    {
                        throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, WebDataResources.SimpleRoleProvider_RoleExists, roleName));
                    }

                    int rows = db.Execute("INSERT INTO " + RoleTableName + " (RoleName) VALUES (@0)", roleName);
                    if (rows != 1)
                    {
                        throw new ProviderException(WebDataResources.Security_DbFailure);
                    }
                }
            }
        }

        // Inherited from RoleProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            if (!InitializeCalled)
            {
                return PreviousProvider.DeleteRole(roleName, throwOnPopulatedRole);
            }
            using (var db = ConnectToDatabase())
            {
                int roleId = FindRoleId(db, roleName);
                if (roleId == -1)
                {
                    return false;
                }

                if (throwOnPopulatedRole)
                {
                    int usersInRole = db.Query(@"SELECT * FROM " + UsersInRoleTableName + " WHERE (RoleId = @0)", roleId).Count();
                    if (usersInRole > 0)
                    {
                        throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, WebDataResources.SimpleRoleProvder_RolePopulated, roleName));
                    }
                }
                else
                {
                    // Delete any users in this role first
                    db.Execute(@"DELETE FROM " + UsersInRoleTableName + " WHERE (RoleId = @0)", roleId);
                }

                int rows = db.Execute(@"DELETE FROM " + RoleTableName + " WHERE (RoleId = @0)", roleId);
                return (rows == 1); // REVIEW: should this ever be > 1?
            }
        }

        // Inherited from RoleProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            if (!InitializeCalled)
            {
                return PreviousProvider.FindUsersInRole(roleName, usernameToMatch);
            }
            using (var db = ConnectToDatabase())
            {
                // REVIEW: Is there any way to directly get out a string[]?
                List<dynamic> userNames = db.Query(@"SELECT u." + SafeUserNameColumn + " FROM " + SafeUserTableName + " u, " + UsersInRoleTableName + " ur, " + RoleTableName + " r Where (r.RoleName = @0 and ur.RoleId = r.RoleId and ur.UserId = u." + SafeUserIdColumn + " and u." + SafeUserNameColumn + " LIKE @1)", new object[] { roleName, usernameToMatch }).ToList();
                string[] users = new string[userNames.Count];
                for (int i = 0; i < userNames.Count; i++)
                {
                    users[i] = (string)userNames[i][0];
                }
                return users;
            }
        }

        // Inherited from RoleProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override string[] GetAllRoles()
        {
            if (!InitializeCalled)
            {
                return PreviousProvider.GetAllRoles();
            }
            using (var db = ConnectToDatabase())
            {
                return db.Query(@"SELECT RoleName FROM " + RoleTableName).Select<dynamic, string>(d => (string)d[0]).ToArray();
            }
        }

        // Inherited from RoleProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override string[] GetRolesForUser(string username)
        {
            if (!InitializeCalled)
            {
                return PreviousProvider.GetRolesForUser(username);
            }
            using (var db = ConnectToDatabase())
            {
                int userId = SimpleMembershipProvider.GetUserId(db, SafeUserTableName, SafeUserNameColumn, SafeUserIdColumn, CasingBehavior, username);
                if (userId == -1)
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, WebDataResources.Security_NoUserFound, username));
                }

                string query = @"SELECT r.RoleName FROM " + UsersInRoleTableName + " u, " + RoleTableName + " r Where (u.UserId = @0 and u.RoleId = r.RoleId) GROUP BY RoleName";
                return db.Query(query, new object[] { userId }).Select<dynamic, string>(d => (string)d[0]).ToArray();
            }
        }

        // Inherited from RoleProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override string[] GetUsersInRole(string roleName)
        {
            if (!InitializeCalled)
            {
                return PreviousProvider.GetUsersInRole(roleName);
            }
            using (var db = ConnectToDatabase())
            {
                string query = @"SELECT u." + SafeUserNameColumn + " FROM " + SafeUserTableName + " u, " + UsersInRoleTableName + " ur, " + RoleTableName + " r Where (r.RoleName = @0 and ur.RoleId = r.RoleId and ur.UserId = u." + SafeUserIdColumn + ")";
                return db.Query(query, new object[] { roleName }).Select<dynamic, string>(d => (string)d[0]).ToArray();
            }
        }

        // Inherited from RoleProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override bool IsUserInRole(string username, string roleName)
        {
            if (!InitializeCalled)
            {
                return PreviousProvider.IsUserInRole(username, roleName);
            }
            using (var db = ConnectToDatabase())
            {
                var count = db.QuerySingle("SELECT COUNT(*) FROM " + SafeUserTableName + " u, " + UsersInRoleTableName + " ur, " + RoleTableName + " r Where (u." + SafeUserNameColumn + " = @0 and r.RoleName = @1 and ur.RoleId = r.RoleId and ur.UserId = u." + SafeUserIdColumn + ")", username, roleName);
                return (count[0] == 1);
            }
        }

        // Inherited from RoleProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            if (!InitializeCalled)
            {
                PreviousProvider.RemoveUsersFromRoles(usernames, roleNames);
            }
            else
            {
                foreach (string rolename in roleNames)
                {
                    if (!RoleExists(rolename))
                    {
                        throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, WebDataResources.SimpleRoleProvider_NoRoleFound, rolename));
                    }
                }

                foreach (string username in usernames)
                {
                    foreach (string rolename in roleNames)
                    {
                        if (!IsUserInRole(username, rolename))
                        {
                            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, WebDataResources.SimpleRoleProvder_UserNotInRole, username, rolename));
                        }
                    }
                }

                using (var db = ConnectToDatabase())
                {
                    List<int> userIds = GetUserIdsFromNames(db, usernames);
                    List<int> roleIds = GetRoleIdsFromNames(db, roleNames);

                    foreach (int userId in userIds)
                    {
                        foreach (int roleId in roleIds)
                        {
                            // Review: Is there a way to do these all in one query?
                            int rows = db.Execute("DELETE FROM " + UsersInRoleTableName + " WHERE UserId = " + userId + " and RoleId = " + roleId);
                            if (rows != 1)
                            {
                                throw new ProviderException(WebDataResources.Security_DbFailure);
                            }
                        }
                    }
                }
            }
        }

        private static int FindRoleId(IDatabase db, string roleName)
        {
            var result = db.QuerySingle(@"SELECT RoleId FROM " + RoleTableName + " WHERE (RoleName = @0)", roleName);
            if (result == null)
            {
                return -1;
            }
            return (int)result[0];
        }

        // Inherited from RoleProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override bool RoleExists(string roleName)
        {
            if (!InitializeCalled)
            {
                return PreviousProvider.RoleExists(roleName);
            }
            using (var db = ConnectToDatabase())
            {
                return (FindRoleId(db, roleName) != -1);
            }
        }
    }
}
