// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Web.Helpers;
using System.Web.Hosting;
using System.Web.Security;

namespace System.Web.WebPages.Administration
{
    internal static class AdminSecurity
    {
        private const string AuthCookieName = ".ASPXADMINAUTH";
        private const string AdminUserNameToken = "ADMIN";

        // Bug941370: Renamed the file to .config to prevent IIS6 from serving this files.
        internal static readonly string AdminPasswordFile = VirtualPathUtility.Combine(SiteAdmin.AdminSettingsFolder, "Password.config");
        internal static readonly string TemporaryPasswordFile = VirtualPathUtility.Combine(SiteAdmin.AdminSettingsFolder, "_Password.config");

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We catch all exceptions to prevent any decryption failure results from being exposed.")]
        internal static bool IsAuthenticated(HttpRequestBase request)
        {
            HttpCookie authCookie = request.Cookies[AuthCookieName];

            // Not authenticated if there is no cookie
            if (authCookie == null)
            {
                return false;
            }

            try
            {
                return IsValidAuthCookie(authCookie);
            }
            catch
            {
                // If decryption fails, it may be a bad cookie
                return false;
            }
        }

        private static bool IsValidAuthCookie(HttpCookie authCookie)
        {
            // Decrypt the cookie and check the expired flag
            FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);

            // Ensure that the ticket hasn't expired and that the custom UserData string is our AdminUserNameToken
            return !ticket.Expired && ticket.UserData != null && ticket.UserData.Equals(AdminUserNameToken);
        }

        internal static void SetAuthCookie(HttpResponseBase response)
        {
            // Get an auth admin cookie
            HttpCookie cookie = GetAuthCookie();

            // Set the name to our auth cookie name
            cookie.Name = AuthCookieName;

            // Add it to the response
            response.Cookies.Add(cookie);
        }

        internal static HttpCookie GetAuthCookie()
        {
            // Create a new forms auth ticket for the admin section
            // Add the admin user name as user data so that we distinguish between a regular
            // ticket issued by ASP.NET's auth system versus our own            
            var ticket = new FormsAuthenticationTicket(2,
                                                       AdminUserNameToken,
                                                       DateTime.Now,
                                                       DateTime.Now.Add(FormsAuthentication.Timeout),
                                                       false,
                                                       AdminUserNameToken,
                                                       FormsAuthentication.FormsCookiePath);

            // Encrypt the ticket and create the cookie
            string encryptedValue = FormsAuthentication.Encrypt(ticket);
            var cookie = new HttpCookie(AuthCookieName, encryptedValue);
            cookie.HttpOnly = true;
            cookie.Path = ticket.CookiePath;

            return cookie;
        }

        internal static void DeleteAuthCookie(HttpResponseBase response)
        {
            // Expire the cookie
            var cookie = new HttpCookie(AuthCookieName);
            cookie.Expires = DateTime.Now.AddDays(-1);
            response.Cookies.Add(cookie);
        }

        internal static bool HasAdminPassword()
        {
            return HasAdminPassword(HostingEnvironment.VirtualPathProvider);
        }

        internal static bool HasAdminPassword(VirtualPathProvider vpp)
        {
            // REVIEW: Do we need to check for content as well?
            return vpp.FileExists(AdminPasswordFile);
        }

        internal static bool HasTemporaryPassword()
        {
            return HasTemporaryPassword(HostingEnvironment.VirtualPathProvider);
        }

        internal static bool HasTemporaryPassword(VirtualPathProvider vpp)
        {
            // REVIEW: Do we need to check for content as well?
            return vpp.FileExists(TemporaryPasswordFile);
        }

        internal static bool SaveTemporaryPassword(string password)
        {
            // When saving the admin password we store it in a dummy file so that we don't enable to the admin UI by default
            return SaveTemporaryPassword(password, GetTemporaryPasswordFileStream);
        }

        private static Stream GetTemporaryPasswordFileStream()
        {
            // Get the password directory and file name
            string passwordFilePath = HostingEnvironment.MapPath(TemporaryPasswordFile);
            string passwordFileDir = Path.GetDirectoryName(passwordFilePath);

            // Ensure password directory exists
            Directory.CreateDirectory(passwordFileDir);

            // Return the stream
            return File.OpenWrite(passwordFilePath);
        }

        internal static bool SaveTemporaryPassword(string password, Func<Stream> getPasswordFileStream)
        {
            Stream stream = null;
            try
            {
                // Get the password file stream
                stream = getPasswordFileStream();
            }
            catch (UnauthorizedAccessException)
            {
                // The user doesn't have write access to App_Data or the site root
                return false;
            }

            using (var writer = new StreamWriter(stream))
            {
                // Write the salty password
                writer.WriteLine(Crypto.HashPassword(password));
            }

            return true;
        }

        internal static bool CheckPassword(string password)
        {
            return CheckPassword(password, () =>
            {
                VirtualFile passwordFile = HostingEnvironment.VirtualPathProvider.GetFile(AdminPasswordFile);
                Debug.Assert(passwordFile != null, "password file should not be null");
                return passwordFile.Open();
            });
        }

        internal static bool CheckPassword(string password, Func<Stream> getPasswordFileStream)
        {
            string saltyPassword = null;

            Stream stream = getPasswordFileStream();
            using (StreamReader reader = new StreamReader(stream))
            {
                // Read the salted password
                saltyPassword = reader.ReadLine();
            }

            return Crypto.VerifyHashedPassword(saltyPassword, password);
        }

        /// <summary>
        /// Ensure that the current request is authorized. 
        /// If the request is authenticated, then we skip all other checks.
        /// </summary>
        internal static void Authorize(StartPage page)
        {
            Authorize(page, HostingEnvironment.VirtualPathProvider, VirtualPathUtility.ToAppRelative);
        }

        internal static void Authorize(StartPage page, VirtualPathProvider vpp, Func<string, string> makeAppRelative)
        {
            if (!IsAuthenticated(page.Request))
            {
                if (HasAdminPassword(vpp))
                {
                    // If there is a password file (Password.config) then we redirect to the login page
                    RedirectSafe(page, SiteAdmin.LoginVirtualPath, makeAppRelative);
                }
                else if (HasTemporaryPassword(vpp))
                {
                    // Dev 10 941521: Admin: Pass through returnurl into page that tells the user to rename _password.config
                    // If there is a disabled password file (_Password.config) then we redirect to the instructions page
                    RedirectSafe(page, SiteAdmin.EnableInstructionsVirtualPath, makeAppRelative);
                }
                else
                {
                    // The user hasn't done anything so redirect to the register page.
                    RedirectSafe(page, SiteAdmin.RegisterVirtualPath, makeAppRelative);
                }
            }
        }

        /// <summary>
        /// Doesn't do a redirect if the requesting page is itself the same as the virtual path.
        /// We need to do this since it is called from the _pagestart.cshtml which always runs.
        /// </summary>
        private static void RedirectSafe(StartPage page, string virtualPath, Func<string, string> makeAppRelative)
        {
            // Make sure we get the virtual path
            virtualPath = SiteAdmin.GetVirtualPath(virtualPath);

            if (!IsRequestingPage(page, virtualPath))
            {
                // Append the redirect url querystring
                virtualPath = SiteAdmin.GetRedirectUrl(page.Request, virtualPath, makeAppRelative);

                page.Context.Response.Redirect(virtualPath);
            }
        }

        /// <summary>
        /// Determines if the specified virtualPath is being requested. We do this by walking the page hierarchy
        /// and comparing the virtualPath with the child page's VirtualPath property (which in the case of ApplicationParts are compiled into the assembly
        /// using the PageVirtualPath attribute).
        /// </summary>
        internal static bool IsRequestingPage(this StartPage page, string virtualPath)
        {
            WebPageRenderingBase webPage = GetRootPage(page);

            return webPage.VirtualPath.Equals(virtualPath, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Walks the page hierarcy to find the requested page.
        /// </summary>
        private static WebPageRenderingBase GetRootPage(StartPage page)
        {
            WebPageRenderingBase currentPage = null;
            while (page != null)
            {
                currentPage = page.ChildPage;
                page = currentPage as StartPage;
            }

            Debug.Assert(currentPage != null, "Should never be null");
            return currentPage;
        }
    }
}
