// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Web.Helpers.Resources;
using System.Web.WebPages;

namespace System.Web.Helpers
{
    /// <summary>
    /// Provides various info about ASP.NET server.
    /// </summary>
    public static class ServerInfo
    {
        /// <remarks>
        /// todo: figure out right place for this
        /// </remarks>
        private const string Style =
            "<style type=\"text/css\">" +
            "  div.server-info { text-align: center; }" +
            "  table.server-info { border-collapse:collapse; text-align:center; margin: auto; width:600px; direction: ltr; }" +
            "  table.server-info tbody tr:nth-child(even){ background-color: #EEE; }" +
            "  table.server-info, table.server-info th, table.server-info td { border:1px solid black; }" +
            "  table.server-info th, table.server-info td " +
            " { text-align:left; padding:2px; font-family:Tahoma, Arial, sans-serif; font-size:0.75em; }" +
            "  h1.server-info { font-family:Tahoma, Arial, sans-serif; font-size:150%; text-align:center; }" +
            "  table.server-info h2 { font-family:Tahoma, Arial, sans-serif; font-size:125%; text-align:center; }" +
            "  p.server-info { text-align:center; font-family:Tahoma, Arial, sans-serif; font-size:0.75em; }" +
            "  .ital { font-style: italic; } " +
            "  .warn { color: #F00; } " +
            "</style>";

        internal static IDictionary<string, string> EnvironmentVariables()
        {
            // todo: extract well defined subset for special use?

            // use a case-insensitive dictionary since environment variables are case-insensitive.
            IDictionary<string, string> environmentVariablesResult = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            IDictionary environmentVariables;

            // todo: better way to deal with security; query config for trust level?
            try
            {
                environmentVariables = Environment.GetEnvironmentVariables();
            }
            catch (SecurityException)
            {
                return environmentVariablesResult;
            }

            foreach (DictionaryEntry entry in environmentVariables)
            {
                environmentVariablesResult.Add(entry.Key.ToString(), InsertWhiteSpace(entry.Value.ToString()));
            }

            return environmentVariablesResult;
        }

        internal static IDictionary<string, string> ServerVariables()
        {
            var httpContext = HttpContext.Current;
            return ServerVariables(httpContext != null ? new HttpContextWrapper(httpContext) : null);
        }

        internal static IDictionary<string, string> ServerVariables(HttpContextBase context)
        {
            // todo: extract well defined subset for special use?

            IDictionary<string, string> serverVariablesResult = new SortedDictionary<string, string>();
            NameValueCollection serverVariables;

            // todo: better way to deal with security; query config for trust level?
            try
            {
                if ((context != null) && (context.Request != null))
                {
                    serverVariables = context.Request.ServerVariables;
                }
                else
                {
                    // Just return empty collection when there is no context available.
                    return serverVariablesResult;
                }
            }
            catch (SecurityException)
            {
                return serverVariablesResult;
            }

            foreach (string key in serverVariables.AllKeys)
            {
                // todo: these values contains very long strings with no spaces that distorts table layout - figure out how to deal with it
                if (key.Equals("ALL_HTTP", StringComparison.OrdinalIgnoreCase) ||
                    key.Equals("ALL_RAW", StringComparison.OrdinalIgnoreCase) ||
                    key.Equals("HTTP_AUTHORIZATION", StringComparison.OrdinalIgnoreCase) ||
                    key.Equals("HTTP_COOKIE", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                serverVariablesResult.Add(key, InsertWhiteSpace(serverVariables[key]));
            }

            return serverVariablesResult;
        }

        internal static IDictionary<string, string> Configuration()
        {
            IDictionary<string, string> info = new Dictionary<string, string>();

            // todo: do we need to localize these strings or would that be confusing
            // (Since we just display API names that are all in English)

            info.Add("Current Local Time", DateTime.Now.ToString(CultureInfo.CurrentCulture));
            info.Add("Current UTC Time", DateTime.UtcNow.ToString(CultureInfo.CurrentCulture));
            info.Add("Current Culture", CultureInfo.CurrentCulture.DisplayName);

            info.Add("Machine Name", Environment.MachineName);
            info.Add("OS Version", Environment.OSVersion.ToString());
            info.Add("ASP.NET Version", Environment.Version.ToString());
            info.Add("ASP.NET Web Pages Version", new AssemblyName(typeof(WebPage).Assembly.FullName).Version.ToString());
            info.Add("User Name", Environment.UserName);
            info.Add("User Interactive", Environment.UserInteractive.ToString());
            info.Add("Processor Count", Environment.ProcessorCount.ToString(CultureInfo.InvariantCulture));
            info.Add("Tick Count", Environment.TickCount.ToString(CultureInfo.InvariantCulture));

            // Calls bellow require full trust.

            try
            {
                info.Add("Current Directory", Environment.CurrentDirectory);
            }
            catch (SecurityException)
            {
                return info;
            }

            info.Add("System Directory", Environment.SystemDirectory);
            info.Add("User Domain Name", Environment.UserDomainName);
            info.Add("Working Set", Environment.WorkingSet.ToString(CultureInfo.InvariantCulture) + " bytes");

            return info;
        }

        internal static IDictionary<string, string> HttpRuntimeInfo()
        {
            IDictionary<string, string> info = new Dictionary<string, string>();

            // todo: better way to deal with security; query config for trust level?
            try
            {
                info.Add("CLR Install Directory", HttpRuntime.ClrInstallDirectory);
            }
            catch (SecurityException)
            {
                return info;
            }

            try
            {
                info.Add("Codegen Directory", HttpRuntime.CodegenDir);
                info.Add("Bin Directory", HttpRuntime.BinDirectory);
                info.Add("AppDomain Application Path", HttpRuntime.AppDomainAppPath);
            }
            catch (ArgumentException)
            {
                // do nothing
                // These APIs don't check if path is set before setting security demands, which causes exception.
                // So far this happens only when running from unit tests.
            }

            info.Add("Asp Install Directory", HttpRuntime.AspInstallDirectory);
            info.Add("Machine Configuration Directory", HttpRuntime.MachineConfigurationDirectory);

            info.Add("AppDomain Id", HttpRuntime.AppDomainId);
            info.Add("AppDomain Application Id", HttpRuntime.AppDomainAppId);
            info.Add("AppDomain Application Virtual Path", HttpRuntime.AppDomainAppVirtualPath);

            info.Add("Asp Client Script Physical Path", HttpRuntime.AspClientScriptPhysicalPath);

            info.Add("Asp Client Script Virtual Path", HttpRuntime.AspClientScriptVirtualPath);

            info.Add("Cache Size", HttpRuntime.Cache.Count.ToString(CultureInfo.InvariantCulture));
            info.Add("Cache Effective Percentage Physical Memory Limit", HttpRuntime.Cache.EffectivePercentagePhysicalMemoryLimit.ToString(CultureInfo.InvariantCulture));
            info.Add("Cache Effective Private Bytes Limit", HttpRuntime.Cache.EffectivePrivateBytesLimit.ToString(CultureInfo.InvariantCulture));

            info.Add("On UNC Share", HttpRuntime.IsOnUNCShare.ToString());

            return info;
        }

        internal static IDictionary<string, string> LegacyCAS()
        {
            return LegacyCAS(AppDomain.CurrentDomain);
        }

        internal static IDictionary<string, string> LegacyCAS(AppDomain appDomain)
        {
            IDictionary<string, string> info = new Dictionary<string, string>();

            try
            {
                bool legacyCasModeEnabled = !appDomain.IsHomogenous;
                if (legacyCasModeEnabled)
                {
                    info[HelpersResources.ServerInfo_LegacyCAS] = HelpersResources.ServerInfo_LegacyCasHelpInfo;
                }
            }
            catch (SecurityException)
            {
                return info;
            }
            return info;
        }

        /// <summary>
        /// Generates HTML required to display server information.
        /// </summary>
        /// <remarks>
        /// HTML generated is XHTML 1.0 compliant but not XHTML 1.1 or HTML5 compliant. The reason is that we
        /// generate &lt;style&gt; tag inside &lt;body&gt; tag, which is not allowed. This is by design for now since ServerInfo
        /// is debugging aid and should not be used as a permanent part of any web page.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "This could be time consuming operation that does not just retrieve a field.")]
        public static HtmlString GetHtml()
        {
            StringBuilder sb = new StringBuilder(Style);

            sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "<h1 class=\"server-info\">{0}</h1>",
                                        HttpUtility.HtmlEncode(HelpersResources.ServerInfo_Header)));

            var configuration = Configuration();
            Debug.Assert((configuration != null) && (configuration.Count > 0));
            PrintInfoSection(sb, HelpersResources.ServerInfo_ServerConfigTable, configuration);

            var serverVariables = ServerVariables();
            Debug.Assert((serverVariables != null));
            PrintInfoSection(sb, HelpersResources.ServerInfo_ServerVars, serverVariables);

            var legacyCAS = LegacyCAS();
            if (legacyCAS.Any())
            {
                PrintInfoSection(sb, HelpersResources.ServerInfo_LegacyCAS, legacyCAS);
            }

            // Info below is not available in medium trust.

            var httpRuntimeInfo = HttpRuntimeInfo();
            Debug.Assert(httpRuntimeInfo != null);

            if (!httpRuntimeInfo.Any())
            {
                sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "<p class=\"server-info\">{0}</p>",
                                            HttpUtility.HtmlEncode(HelpersResources.ServerInfo_AdditionalInfo)));
                return new HtmlString(sb.ToString());
            }
            else
            {
                PrintInfoSection(sb, HelpersResources.ServerInfo_HttpRuntime, httpRuntimeInfo);

                var envVariables = EnvironmentVariables();
                Debug.Assert(envVariables != null);
                PrintInfoSection(sb, HelpersResources.ServerInfo_EnvVars, envVariables);
            }

            return new HtmlString(sb.ToString());
        }

        /// <summary>
        /// Renders a table section printing out rows and columns.
        /// </summary>
        private static void PrintInfoSection(StringBuilder builder, string sectionTitle, IDictionary<string, string> entries)
        {
            builder.AppendLine("<div class=\"server-info\">");
            builder.AppendLine("<table class=\"server-info\" dir=\"ltr\">");
            if (!String.IsNullOrEmpty(sectionTitle))
            {
                builder.AppendLine("<caption>");
                builder.AppendFormat(CultureInfo.InvariantCulture, "<h2>{0}</h2>", HttpUtility.HtmlEncode(sectionTitle)).AppendLine();
                builder.AppendLine("</caption>");
            }
            builder.AppendLine("<colgroup><col style=\"width:30%;\" /> <col style=\"width:70%;\"  /></colgroup>");
            builder.AppendLine("<tbody>");
            foreach (var entry in entries)
            {
                var css = String.Empty;
                string value = entry.Value;
                if (entry.Key == HelpersResources.ServerInfo_LegacyCAS)
                {
                    // TODO: suboptimal solution, but its easier to do this than come up with something that works better
                    css = "warn";
                }
                else if (String.IsNullOrEmpty(entry.Value))
                {
                    css = "ital";
                    value = HelpersResources.ServerInfo_NoValue;
                }
                if (css.Any())
                {
                    css = " class=\"" + css + "\"";
                }
                builder.Append("<tr>");
                builder.AppendFormat(CultureInfo.InvariantCulture, "<th scope=\"row\">{0}</th>", HttpUtility.HtmlEncode(entry.Key));
                builder.AppendFormat(CultureInfo.InvariantCulture, "<td{0}>{1}</td>", css, HttpUtility.HtmlEncode(value));
                builder.AppendLine("</tr>");
            }
            builder.AppendLine("</tbody>");
            builder.AppendLine("</table>");
            builder.AppendLine("</div>");
        }

        /// <summary>
        /// Inserts spaces after ',' and ';' so table can be rendered properly.
        /// </summary>
        private static string InsertWhiteSpace(string s)
        {
            return s.Replace(",", ", ").Replace(";", "; ");
        }
    }
}
