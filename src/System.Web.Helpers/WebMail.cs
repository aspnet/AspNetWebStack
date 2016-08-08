// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web.Helpers.Resources;
using System.Web.WebPages.Scope;
using Microsoft.Internal.Web.Utils;

namespace System.Web.Helpers
{
    [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WebMail", Justification = "The name of this class is consistent with the naming convention followed in other helpers")]
    public static class WebMail
    {
        internal static readonly object SmtpServerKey = new object();
        internal static readonly object SmtpPortKey = new object();
        internal static readonly object SmtpUseDefaultCredentialsKey = new object();
        internal static readonly object EnableSslKey = new object();
        internal static readonly object PasswordKey = new object();
        internal static readonly object UserNameKey = new object();
        internal static readonly object FromKey = new object();
        internal static readonly Lazy<IDictionary<object, object>> SmtpDefaults = new Lazy<IDictionary<object, object>>(ReadSmtpDefaults);

        /// <summary>
        /// MailMessage dictates that headers values that have equivalent properties would be discarded or overwritten. The list of values is available at 
        /// http://msdn.microsoft.com/en-us/library/system.net.mail.mailmessage.aspx
        /// </summary>
        private static readonly Dictionary<string, Action<MailMessage, string>> _actionableHeaders = new Dictionary<string, Action<MailMessage, string>>(StringComparer.OrdinalIgnoreCase)
        {
            { "Bcc", (message, value) => message.Bcc.Add(value) },
            { "Cc", (message, value) => message.CC.Add(value) },
            { "From", (mailMessage, value) =>
            {
                mailMessage.From = new MailAddress(value);
            }
                },
            { "Priority", SetPriority },
            { "Reply-To", (mailMessage, value) =>
            {
                mailMessage.ReplyToList.Add(value);
            }
                },
            { "Sender", (mailMessage, value) =>
            {
                mailMessage.Sender = new MailAddress(value);
            }
                },
            { "To", (mailMessage, value) =>
            {
                mailMessage.To.Add(value);
            }
                },
        };

        ///////////////////////////////////////////////////////////////////////////
        // Public Properties
        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "SmtpServer is more descriptive as compared to the actual argument \"value\"")]
        public static string SmtpServer
        {
            get { return ReadValue<string>(SmtpServerKey); }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "SmtpServer");
                }
                ScopeStorage.CurrentScope[SmtpServerKey] = value;
            }
        }

        public static int SmtpPort
        {
            get { return ReadValue<int>(SmtpPortKey); }
            set { ScopeStorage.CurrentScope[SmtpPortKey] = value; }
        }

        public static string From
        {
            get { return ReadValue<string>(FromKey); }
            set { ScopeStorage.CurrentScope[FromKey] = value; }
        }

        public static bool SmtpUseDefaultCredentials
        {
            get { return ReadValue<bool>(SmtpUseDefaultCredentialsKey); }
            set { ScopeStorage.CurrentScope[SmtpUseDefaultCredentialsKey] = value; }
        }

        public static bool EnableSsl
        {
            get { return ReadValue<bool>(EnableSslKey); }
            set { ScopeStorage.CurrentScope[EnableSslKey] = value; }
        }

        public static string UserName
        {
            get { return ReadValue<string>(UserNameKey); }
            set { ScopeStorage.CurrentScope[UserNameKey] = value; }
        }

        public static string Password
        {
            get { return ReadValue<string>(PasswordKey); }
            set { ScopeStorage.CurrentScope[PasswordKey] = value; }
        }

        public static void Send(string to,
                                string subject,
                                string body,
                                string from = null,
                                string cc = null,
                                IEnumerable<string> filesToAttach = null,
                                bool isBodyHtml = true,
                                IEnumerable<string> additionalHeaders = null,
                                string bcc = null,
                                string contentEncoding = null,
                                string headerEncoding = null,
                                string priority = null,
                                string replyTo = null)
        {
            if (filesToAttach != null)
            {
                foreach (string fileName in filesToAttach)
                {
                    if (String.IsNullOrEmpty(fileName))
                    {
                        throw new ArgumentException(HelpersResources.WebMail_ItemInCollectionIsNull, "filesToAttach");
                    }
                }
            }

            if (additionalHeaders != null)
            {
                foreach (string header in additionalHeaders)
                {
                    if (String.IsNullOrEmpty(header))
                    {
                        throw new ArgumentException(HelpersResources.WebMail_ItemInCollectionIsNull, "additionalHeaders");
                    }
                }
            }

            MailPriority priorityValue = MailPriority.Normal;
            if (!String.IsNullOrEmpty(priority) && !ConversionUtil.TryFromStringToEnum(priority, out priorityValue))
            {
                throw new ArgumentException(HelpersResources.WebMail_InvalidPriority, "priority");
            }

            if (String.IsNullOrEmpty(SmtpServer))
            {
                throw new InvalidOperationException(HelpersResources.WebMail_SmtpServerNotSpecified);
            }

            using (MailMessage message = new MailMessage())
            {
                SetPropertiesOnMessage(message, to, subject, body, from, cc, bcc, replyTo, contentEncoding, headerEncoding, priorityValue,
                                       filesToAttach, isBodyHtml, additionalHeaders);
                using (SmtpClient client = new SmtpClient())
                {
                    SetPropertiesOnClient(client);
                    client.Send(message);
                }
            }
        }

        private static TValue ReadValue<TValue>(object key)
        {
            return (TValue)(ScopeStorage.CurrentScope[key] ?? SmtpDefaults.Value[key]);
        }

        private static IDictionary<object, object> ReadSmtpDefaults()
        {
            Dictionary<object, object> smtpDefaults = new Dictionary<object, object>();
            try
            {
                // Create a new SmtpClient object: this will read config & tell us what the default value is
                using (SmtpClient client = new SmtpClient())
                {
                    smtpDefaults[SmtpServerKey] = client.Host;
                    smtpDefaults[SmtpPortKey] = client.Port;
                    smtpDefaults[EnableSslKey] = client.EnableSsl;
                    smtpDefaults[SmtpUseDefaultCredentialsKey] = client.UseDefaultCredentials;

                    var credentials = client.Credentials as NetworkCredential;
                    if (credentials != null)
                    {
                        smtpDefaults[UserNameKey] = credentials.UserName;
                        smtpDefaults[PasswordKey] = credentials.Password;
                    }
                    else
                    {
                        smtpDefaults[UserNameKey] = null;
                        smtpDefaults[PasswordKey] = null;
                    }
                    using (MailMessage message = new MailMessage())
                    {
                        smtpDefaults[FromKey] = (message.From != null) ? message.From.Address : null;
                    }
                }
            }
            catch (InvalidOperationException)
            {
                // Due to Bug Dev10 PS 337470 ("SmtpClient reports InvalidOperationException when disposed"), we need to ignore the spurious InvalidOperationException
            }
            return smtpDefaults;
        }

        internal static void SetPropertiesOnClient(SmtpClient client)
        {
            // If no value has been assigned to these properties, at the very worst we will simply 
            // write back the values we just read from the SmtpClient
            if (SmtpServer != null)
            {
                client.Host = SmtpServer;
            }
            client.Port = SmtpPort;
            client.UseDefaultCredentials = SmtpUseDefaultCredentials;
            client.EnableSsl = EnableSsl;
            if (!String.IsNullOrEmpty(UserName))
            {
                client.Credentials = new NetworkCredential(UserName, Password);
            }
        }

        internal static void SetPropertiesOnMessage(MailMessage message, string to, string subject,
                                                    string body, string from, string cc, string bcc, string replyTo,
                                                    string contentEncoding, string headerEncoding, MailPriority priority,
                                                    IEnumerable<string> filesToAttach, bool isBodyHtml,
                                                    IEnumerable<string> additionalHeaders)
        {
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = isBodyHtml;

            if (additionalHeaders != null)
            {
                AssignHeaderValues(message, additionalHeaders);
            }

            if (to != null)
            {
                message.To.Add(to);
            }

            if (!String.IsNullOrEmpty(cc))
            {
                message.CC.Add(cc);
            }

            if (!String.IsNullOrEmpty(bcc))
            {
                message.Bcc.Add(bcc);
            }

            if (!String.IsNullOrEmpty(replyTo))
            {
                message.ReplyToList.Add(replyTo);
            }

            if (!String.IsNullOrEmpty(contentEncoding))
            {
                message.BodyEncoding = Encoding.GetEncoding(contentEncoding);
            }

            if (!String.IsNullOrEmpty(headerEncoding))
            {
                message.HeadersEncoding = Encoding.GetEncoding(headerEncoding);
            }

            message.Priority = priority;

            if (from != null)
            {
                message.From = new MailAddress(from);
            }
            else if (!String.IsNullOrEmpty(From))
            {
                message.From = new MailAddress(From);
            }
            else if (message.From == null || String.IsNullOrEmpty(message.From.Address))
            {
                var httpContext = HttpContext.Current;
                if (httpContext != null)
                {
                    message.From = new MailAddress("DoNotReply@" + httpContext.Request.Url.Host);
                }
                else
                {
                    throw new InvalidOperationException(HelpersResources.WebMail_UnableToDetermineFrom);
                }
            }

            if (filesToAttach != null)
            {
                foreach (string file in filesToAttach)
                {
                    if (!Path.IsPathRooted(file) && HttpRuntime.AppDomainAppPath != null)
                    {
                        message.Attachments.Add(new Attachment(Path.Combine(HttpRuntime.AppDomainAppPath, file)));
                    }
                    else
                    {
                        message.Attachments.Add(new Attachment(file));
                    }
                }
            }
        }

        internal static void AssignHeaderValues(MailMessage message, IEnumerable<string> headerValues)
        {
            // Parse the header value. If this 
            foreach (var header in headerValues)
            {
                string key, value;
                if (TryParseHeader(header, out key, out value))
                {
                    // Verify if the header key maps to a property on MailMessage. 
                    Action<MailMessage, string> action;
                    if (_actionableHeaders.TryGetValue(key, out action))
                    {
                        try
                        {
                            action(message, value);
                        }
                        catch (FormatException)
                        {
                            // If the mail address is invalid, swallow the exception.
                        }
                    }
                    message.Headers.Add(key, value);
                }
            }
        }

        /// <summary>
        /// Parses a SMTP Mail header of the format "name: value"
        /// </summary>
        /// <returns>True if the header was parsed.</returns>
        internal static bool TryParseHeader(string header, out string key, out string value)
        {
            int pos = header.IndexOf(':');
            if (pos > 0)
            {
                key = header.Substring(0, pos).TrimEnd();
                value = header.Substring(pos + 1).TrimStart();
                return key.Length > 0 && value.Length > 0;
            }
            key = null;
            value = null;
            return false;
        }

        private static void SetPriority(MailMessage message, string priority)
        {
            MailPriority priorityValue;
            if (!String.IsNullOrEmpty(priority) && ConversionUtil.TryFromStringToEnum(priority, out priorityValue))
            {
                // If we can parse it, set it. Do nothing otherwise
                message.Priority = priorityValue;
            }
        }
    }
}
