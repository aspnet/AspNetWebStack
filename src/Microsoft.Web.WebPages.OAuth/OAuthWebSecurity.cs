// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using DotNetOpenAuth.AspNet.Clients;
using Microsoft.Web.WebPages.OAuth.Properties;
using WebMatrix.WebData;

namespace Microsoft.Web.WebPages.OAuth
{
    /// <summary>
    /// Contains APIs to manage authentication against OAuth &amp; OpenID service providers
    /// </summary>
    public static class OAuthWebSecurity
    {
        internal static IOpenAuthDataProvider OAuthDataProvider = new WebPagesOAuthDataProvider();

        // contains all registered authentication clients
        private static readonly Dictionary<string, AuthenticationClientData> _authenticationClients =
            new Dictionary<string, AuthenticationClientData>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets a value indicating whether the current user is authenticated by an OAuth provider.
        /// </summary>
        public static bool IsAuthenticatedWithOAuth
        {
            get
            {
                if (HttpContext.Current == null)
                {
                    throw new InvalidOperationException(WebResources.HttpContextNotAvailable);
                }

                return GetIsAuthenticatedWithOAuthCore(new HttpContextWrapper(HttpContext.Current));
            }
        }

        /// <summary>
        /// Gets the collection of all registered authentication client;
        /// </summary>
        /// <returns></returns>
        public static ICollection<AuthenticationClientData> RegisteredClientData
        {
            get
            {
                // the Values property returns a read-only collection.
                // so we don't need to worry about clients of this method modifying our internal collection.
                return _authenticationClients.Values;
            }
        }

        /// <summary>
        /// Registers the Facebook client.
        /// </summary>
        /// <param name="appId">The app id.</param>
        /// <param name="appSecret">The app secret.</param>
        public static void RegisterFacebookClient(string appId, string appSecret)
        {
            RegisterFacebookClient(appId, appSecret, displayName: "Facebook");
        }

        /// <summary>
        /// Registers the Facebook client.
        /// </summary>
        /// <param name="appId">The app id.</param>
        /// <param name="appSecret">The app secret.</param>
        /// <param name="displayName">The display name of the client.</param>
        public static void RegisterFacebookClient(string appId, string appSecret, string displayName)
        {
            RegisterFacebookClient(appId, appSecret, displayName, extraData: new Dictionary<string, object>());
        }

        /// <summary>
        /// Registers the Facebook client.
        /// </summary>
        /// <param name="appId">The app id.</param>
        /// <param name="appSecret">The app secret.</param>
        /// <param name="displayName">The display name.</param>
        /// <param name="extraData">The data bag used to store extra data about this client</param>
        public static void RegisterFacebookClient(string appId, string appSecret, string displayName, IDictionary<string, object> extraData)
        {
            RegisterClient(new FacebookClient(appId, appSecret), displayName, extraData);
        }

        /// <summary>
        /// Registers the Microsoft account client.
        /// </summary>
        /// <param name="clientId">The client id.</param>
        /// <param name="clientSecret">The client secret.</param>
        public static void RegisterMicrosoftClient(string clientId, string clientSecret)
        {
            RegisterMicrosoftClient(clientId, clientSecret, displayName: "Microsoft");
        }

        /// <summary>
        /// Registers the Microsoft account client.
        /// </summary>
        /// <param name="clientId">The client id.</param>
        /// <param name="clientSecret">The client secret.</param>
        /// <param name="displayName">The display name.</param>
        public static void RegisterMicrosoftClient(string clientId, string clientSecret, string displayName)
        {
            RegisterMicrosoftClient(clientId, clientSecret, displayName, new Dictionary<string, object>());
        }

        /// <summary>
        /// Registers the Microsoft account client.
        /// </summary>
        /// <param name="clientId">The client id.</param>
        /// <param name="clientSecret">The client secret.</param>
        /// <param name="displayName">The display name.</param>
        /// <param name="extraData">The data bag used to store extra data about this client</param>
        public static void RegisterMicrosoftClient(string clientId, string clientSecret, string displayName, IDictionary<string, object> extraData)
        {
            RegisterClient(new MicrosoftClient(clientId, clientSecret), displayName, extraData);
        }

        /// <summary>
        /// Registers the Twitter client.
        /// </summary>
        /// <param name="consumerKey">The consumer key.</param>
        /// <param name="consumerSecret">The consumer secret.</param>
        public static void RegisterTwitterClient(string consumerKey, string consumerSecret)
        {
            RegisterTwitterClient(consumerKey, consumerSecret, displayName: "Twitter");
        }

        /// <summary>
        /// Registers the Twitter client.
        /// </summary>
        /// <param name="consumerKey">The consumer key.</param>
        /// <param name="consumerSecret">The consumer secret.</param>
        /// <param name="displayName">The display name.</param>
        public static void RegisterTwitterClient(string consumerKey, string consumerSecret, string displayName)
        {
            RegisterTwitterClient(consumerKey, consumerSecret, displayName, new Dictionary<string, object>());
        }

        /// <summary>
        /// Registers the Twitter client.
        /// </summary>
        /// <param name="consumerKey">The consumer key.</param>
        /// <param name="consumerSecret">The consumer secret.</param>
        /// <param name="displayName">The display name.</param>
        /// <param name="extraData">The data bag used to store extra data about this client</param>
        public static void RegisterTwitterClient(string consumerKey, string consumerSecret, string displayName, IDictionary<string, object> extraData)
        {
            var twitterClient = new TwitterClient(consumerKey, consumerSecret);
            RegisterClient(twitterClient, displayName, extraData);
        }

        /// <summary>
        /// Registers the LinkedIn client.
        /// </summary>
        /// <param name="consumerKey">The consumer key.</param>
        /// <param name="consumerSecret">The consumer secret.</param>
        public static void RegisterLinkedInClient(string consumerKey, string consumerSecret)
        {
            RegisterLinkedInClient(consumerKey, consumerSecret, displayName: "LinkedIn");
        }

        /// <summary>
        /// Registers the LinkedIn client.
        /// </summary>
        /// <param name="consumerKey">The consumer key.</param>
        /// <param name="consumerSecret">The consumer secret.</param>
        /// <param name="displayName">The display name.</param>
        public static void RegisterLinkedInClient(string consumerKey, string consumerSecret, string displayName)
        {
            RegisterLinkedInClient(consumerKey, consumerSecret, displayName, new Dictionary<string, object>());
        }

        /// <summary>
        /// Registers the LinkedIn client.
        /// </summary>
        /// <param name="consumerKey">The consumer key.</param>
        /// <param name="consumerSecret">The consumer secret.</param>
        /// <param name="displayName">The display name.</param>
        /// <param name="extraData">The data bag used to store extra data about this client</param>
        public static void RegisterLinkedInClient(string consumerKey, string consumerSecret, string displayName, IDictionary<string, object> extraData)
        {
            var linkedInClient = new LinkedInClient(consumerKey, consumerSecret);
            RegisterClient(linkedInClient, displayName, extraData);
        }

        /// <summary>
        /// Registers the Google client.
        /// </summary>
        public static void RegisterGoogleClient()
        {
            RegisterGoogleClient(displayName: "Google");
        }

        /// <summary>
        /// Registers the Google client.
        /// </summary>
        /// <param name="displayName">The display name.</param>
        public static void RegisterGoogleClient(string displayName)
        {
            RegisterClient(new GoogleOpenIdClient(), displayName, new Dictionary<string, object>());
        }

        /// <summary>
        /// Registers the Google client.
        /// </summary>
        /// <param name="displayName">The display name.</param>
        /// <param name="extraData">The data bag.</param>
        public static void RegisterGoogleClient(string displayName, IDictionary<string, object> extraData)
        {
            RegisterClient(new GoogleOpenIdClient(), displayName, extraData);
        }

        /// <summary>
        /// Registers the Yahoo client.
        /// </summary>
        public static void RegisterYahooClient()
        {
            RegisterYahooClient(displayName: "Yahoo");
        }

        /// <summary>
        /// Registers the Yahoo client.
        /// </summary>
        /// <param name="displayName">The display name.</param>
        public static void RegisterYahooClient(string displayName)
        {
            RegisterYahooClient(displayName, new Dictionary<string, object>());
        }

        /// <summary>
        /// Registers the Yahoo client.
        /// </summary>
        /// <param name="displayName">The display name.</param>
        /// <param name="extraData">The data bag.</param>
        public static void RegisterYahooClient(string displayName, IDictionary<string, object> extraData)
        {
            RegisterClient(new YahooOpenIdClient(), displayName, extraData);
        }

        /// <summary>
        /// Registers an authentication client.
        /// </summary>
        /// <param name="client">The client to be registered.</param>
        [CLSCompliant(false)]
        public static void RegisterClient(IAuthenticationClient client)
        {
            RegisterClient(client, displayName: null, extraData: new Dictionary<string, object>());
        }

        /// <summary>
        /// Registers an authentication client.
        /// </summary>
        /// <param name="client">The client to be registered</param>
        /// <param name="displayName">The display name.</param>
        /// <param name="extraData">The data bag used to store extra data about the specified client</param>
        [CLSCompliant(false)]
        public static void RegisterClient(IAuthenticationClient client, string displayName, IDictionary<string, object> extraData)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }

            if (String.IsNullOrEmpty(client.ProviderName))
            {
                throw new ArgumentException(WebResources.InvalidServiceProviderName, "client");
            }

            if (_authenticationClients.ContainsKey(client.ProviderName))
            {
                throw new ArgumentException(WebResources.ServiceProviderNameExists, "client");
            }

            var clientData = new AuthenticationClientData(client, displayName, extraData);
            _authenticationClients.Add(client.ProviderName, clientData);
        }

        /// <summary>
        /// Requests the specified provider to start the authentication by directing users to an external website
        /// </summary>
        /// <param name="provider">The provider.</param>
        public static void RequestAuthentication(string provider)
        {
            RequestAuthentication(provider, returnUrl: null);
        }

        /// <summary>
        /// Requests the specified provider to start the authentication by directing users to an external website
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="returnUrl">The return url after user is authenticated.</param>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#", Justification = "We want to allow relative app path, and support ~/")]
        public static void RequestAuthentication(string provider, string returnUrl)
        {
            if (HttpContext.Current == null)
            {
                throw new InvalidOperationException(WebResources.HttpContextNotAvailable);
            }

            RequestAuthenticationCore(new HttpContextWrapper(HttpContext.Current), provider, returnUrl);
        }

        /// <summary>
        /// Requests the authentication core.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="provider">The provider.</param>
        /// <param name="returnUrl">The return URL.</param>
        internal static void RequestAuthenticationCore(HttpContextBase context, string provider, string returnUrl)
        {
            IAuthenticationClient client = GetOAuthClient(provider);
            var securityManager = new OpenAuthSecurityManager(context, client, OAuthDataProvider);
            securityManager.RequestAuthentication(returnUrl);
        }

        /// <summary>
        /// Checks if user is successfully authenticated when user is redirected back to this user.
        /// </summary>
        [CLSCompliant(false)]
        public static AuthenticationResult VerifyAuthentication()
        {
            return VerifyAuthentication(returnUrl: null);
        }

        /// <summary>
        /// Checks if user is successfully authenticated when user is redirected back to this user.
        /// </summary>
        /// <param name="returnUrl">The return URL which must match the one passed to RequestAuthentication earlier.</param>
        [CLSCompliant(false)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#", Justification = "We want to allow relative app path, and support ~/")]
        public static AuthenticationResult VerifyAuthentication(string returnUrl)
        {
            if (HttpContext.Current == null)
            {
                throw new InvalidOperationException(WebResources.HttpContextNotAvailable);
            }

            return VerifyAuthenticationCore(new HttpContextWrapper(HttpContext.Current), returnUrl);
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#", Justification = "We want to allow relative app path, and support ~/")]
        internal static AuthenticationResult VerifyAuthenticationCore(HttpContextBase context, string returnUrl)
        {
            string providerName = OpenAuthSecurityManager.GetProviderName(context);
            if (String.IsNullOrEmpty(providerName))
            {
                return AuthenticationResult.Failed;
            }

            IAuthenticationClient client;
            if (TryGetOAuthClient(providerName, out client))
            {
                var securityManager = new OpenAuthSecurityManager(context, client, OAuthDataProvider);
                return securityManager.VerifyAuthentication(returnUrl);
            }
            else
            {
                throw new InvalidOperationException(WebResources.InvalidServiceProviderName);
            }
        }

        /// <summary>
        /// Checks if the specified provider user id represents a valid account.
        /// If it does, log user in.
        /// </summary>
        /// <param name="providerName">Name of the provider.</param>
        /// <param name="providerUserId">The provider user id.</param>
        /// <param name="createPersistentCookie">if set to <c>true</c> create persistent cookie as part of the login.</param>
        /// <returns>
        ///   <c>true</c> if the login is successful.
        /// </returns>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Login", Justification = "Login is used more consistently in ASP.Net")]
        public static bool Login(string providerName, string providerUserId, bool createPersistentCookie)
        {
            if (HttpContext.Current == null)
            {
                throw new InvalidOperationException(WebResources.HttpContextNotAvailable);
            }

            return LoginCore(new HttpContextWrapper(HttpContext.Current), providerName, providerUserId, createPersistentCookie);
        }

        internal static bool LoginCore(HttpContextBase context, string providerName, string providerUserId, bool createPersistentCookie)
        {
            var provider = GetOAuthClient(providerName);
            var securityManager = new OpenAuthSecurityManager(context, provider, OAuthDataProvider);
            return securityManager.Login(providerUserId, createPersistentCookie);
        }

        internal static bool GetIsAuthenticatedWithOAuthCore(HttpContextBase context)
        {
            return new OpenAuthSecurityManager(context).IsAuthenticatedWithOpenAuth;
        }

        /// <summary>
        /// Creates or update the account with the specified provider, provider user id and associate it with the specified user name.
        /// </summary>
        /// <param name="providerName">Name of the provider.</param>
        /// <param name="providerUserId">The provider user id.</param>
        /// <param name="userName">The user name.</param>
        public static void CreateOrUpdateAccount(string providerName, string providerUserId, string userName)
        {
            ExtendedMembershipProvider provider = VerifyProvider();
            provider.CreateOrUpdateOAuthAccount(providerName, providerUserId, userName);
        }

        /// <summary>
        /// Gets the registered user name corresponding to the specified provider and provider user id.
        /// </summary>
        /// <param name="providerName">Name of the provider.</param>
        /// <param name="providerUserId">The provider user id.</param>
        /// <returns></returns>
        public static string GetUserName(string providerName, string providerUserId)
        {
            return OAuthDataProvider.GetUserNameFromOpenAuth(providerName, providerUserId);
        }

        /// <summary>
        /// Gets all OAuth &amp; OpenID accounts which are associted with the specified user name.
        /// </summary>
        /// <param name="userName">The user name.</param>
        public static ICollection<OAuthAccount> GetAccountsFromUserName(string userName)
        {
            if (String.IsNullOrEmpty(userName))
            {
                throw new ArgumentException(
                    String.Format(CultureInfo.CurrentCulture, WebResources.Argument_Cannot_Be_Null_Or_Empty, "userName"),
                    "userName");
            }

            ExtendedMembershipProvider provider = VerifyProvider();
            return provider.GetAccountsForUser(userName).Select(p => new OAuthAccount(p.Provider, p.ProviderUserId)).ToList();
        }

        /// <summary>
        /// Determines whether there exists a local account (as opposed to OAuth account) with the specified userId.
        /// </summary>
        /// <param name="userId">The user id to check for local account.</param>
        /// <returns>
        ///   <c>true</c> if there is a local account with the specified user id]; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasLocalAccount(int userId)
        {
            ExtendedMembershipProvider provider = VerifyProvider();
            Debug.Assert(provider != null); // VerifyProvider checks this
            return provider.HasLocalAccount(userId);
        }

        /// <summary>
        /// Delete the specified OAuth &amp; OpenID account
        /// </summary>
        /// <param name="providerName">Name of the provider.</param>
        /// <param name="providerUserId">The provider user id.</param>
        public static bool DeleteAccount(string providerName, string providerUserId)
        {
            ExtendedMembershipProvider provider = VerifyProvider();

            string username = GetUserName(providerName, providerUserId);
            if (String.IsNullOrEmpty(username))
            {
                // account doesn't exist
                return false;
            }

            provider.DeleteOAuthAccount(providerName, providerUserId);
            return true;
        }

        /// <summary>
        /// Gets the OAuth client data of the specified provider name.
        /// </summary>
        /// <param name="providerName">Name of the provider.</param>
        /// <returns>The AuthenticationClientData of the specified provider name.</returns>
        public static AuthenticationClientData GetOAuthClientData(string providerName)
        {
            if (providerName == null)
            {
                throw new ArgumentNullException("providerName");
            }

            return _authenticationClients[providerName];
        }

        /// <summary>
        /// Tries getting the OAuth client data of the specified provider name.
        /// </summary>
        /// <param name="providerName">Name of the provider.</param>
        /// <param name="clientData">The client data of the specified provider name.</param>
        /// <returns><c>true</c> if the client data is found for the specified provider name. Otherwise, <c>false</c></returns>
        public static bool TryGetOAuthClientData(string providerName, out AuthenticationClientData clientData)
        {
            if (providerName == null)
            {
                throw new ArgumentNullException("providerName");
            }

            return _authenticationClients.TryGetValue(providerName, out clientData);
        }

        internal static IAuthenticationClient GetOAuthClient(string providerName)
        {
            if (!_authenticationClients.ContainsKey(providerName))
            {
                throw new ArgumentException(WebResources.ServiceProviderNotFound, "providerName");
            }

            return _authenticationClients[providerName].AuthenticationClient;
        }

        internal static bool TryGetOAuthClient(string provider, out IAuthenticationClient client)
        {
            if (_authenticationClients.ContainsKey(provider))
            {
                client = _authenticationClients[provider].AuthenticationClient;
                return true;
            }
            else
            {
                client = null;
                return false;
            }
        }

        /// <summary>
        /// for unit tests
        /// </summary>
        internal static void ClearProviders()
        {
            _authenticationClients.Clear();
        }

        private static ExtendedMembershipProvider VerifyProvider()
        {
            var provider = Membership.Provider as ExtendedMembershipProvider;
            if (provider == null)
            {
                throw new InvalidOperationException();
            }
            return provider;
        }

        /// <summary>
        /// Securely serializes a providerName/providerUserId pair.
        /// </summary>
        /// <param name="providerName">The provider name.</param>
        /// <param name="providerUserId">The provider-specific user id.</param>
        /// <returns>A cryptographically protected serialization of the inputs which is suitable for round-tripping.</returns>
        /// <remarks>Do not persist the return value to permanent storage. This implementation is subject to change.</remarks>
        public static string SerializeProviderUserId(string providerName, string providerUserId)
        {
            if (providerName == null)
            {
                throw new ArgumentNullException("providerName");
            }
            if (providerUserId == null)
            {
                throw new ArgumentNullException("providerUserId");
            }

            return ProviderUserIdSerializationHelper.ProtectData(providerName, providerUserId);
        }

        /// <summary>
        /// Deserializes a string obtained from <see cref="SerializeProviderUserId(string, string)"/> back into a 
        /// providerName/providerUserId pair.
        /// </summary>
        /// <param name="data">The input data.</param>
        /// <param name="providerName">Will contain the deserialized provider name.</param>
        /// <param name="providerUserId">Will contain the deserialized provider user id.</param>
        /// <returns><c>True</c> if successful.</returns>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#", Justification = "This design is acceptable")]
        public static bool TryDeserializeProviderUserId(string data, out string providerName, out string providerUserId)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            return ProviderUserIdSerializationHelper.UnprotectData(data, out providerName, out providerUserId);
        }
    }
}