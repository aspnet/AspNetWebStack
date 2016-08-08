// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI;

namespace Microsoft.Web.Mvc.Controls
{
    [ParseChildren(true)]
    [PersistChildren(false)]
    public class ActionLink : MvcControl
    {
        private string _actionName;
        private string _controllerName;
        private string _text;
        private string _routeName;
        private RouteValues _values;

        [DefaultValue("")]
        public string ActionName
        {
            get { return _actionName ?? String.Empty; }
            set { _actionName = value; }
        }

        [DefaultValue("")]
        public string ControllerName
        {
            get { return _controllerName ?? String.Empty; }
            set { _controllerName = value; }
        }

        [DefaultValue("")]
        public string RouteName
        {
            get { return _routeName ?? String.Empty; }
            set { _routeName = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public RouteValues Values
        {
            get
            {
                if (_values == null)
                {
                    _values = new RouteValues();
                }
                return _values;
            }
        }

        public string Text
        {
            get { return _text ?? String.Empty; }
            set { _text = value; }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            RouteValueDictionary routeValues = new RouteValueDictionary();
            foreach (var attribute in Values.Attributes)
            {
                routeValues.Add(attribute.Key, attribute.Value);
            }

            if (!String.IsNullOrEmpty(ActionName) && !routeValues.ContainsKey("action"))
            {
                routeValues.Add("action", ActionName);
            }
            if (!String.IsNullOrEmpty(ControllerName) && !routeValues.ContainsKey("controller"))
            {
                routeValues.Add("controller", ControllerName);
            }

            string href = null;
            if (DesignMode)
            {
                href = "/";
            }
            else
            {
                VirtualPathData vpd = RouteTable.Routes.GetVirtualPathForArea(ViewContext.RequestContext, RouteName, routeValues);
                if (vpd == null)
                {
                    throw new InvalidOperationException("A route that matches the requested values could not be located in the route table.");
                }
                href = vpd.VirtualPath;
            }

            foreach (var attribute in Attributes)
            {
                writer.AddAttribute(attribute.Key, attribute.Value);
            }

            if (!Attributes.ContainsKey("href"))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Href, href);
            }

            writer.RenderBeginTag(HtmlTextWriterTag.A);

            writer.WriteEncodedText(Text);

            writer.RenderEndTag();
        }
    }
}
