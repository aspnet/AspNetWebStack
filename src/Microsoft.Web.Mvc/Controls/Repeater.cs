// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;
using System.Web.UI;

namespace Microsoft.Web.Mvc.Controls
{
    [ParseChildren(true)]
    [PersistChildren(false)]
    public class Repeater : MvcControl
    {
        private string _name;

        [DefaultValue(null)]
        [Browsable(false)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [TemplateContainer(typeof(RepeaterItem))]
        [TemplateInstance(TemplateInstance.Multiple)]
        public ITemplate ItemTemplate { get; set; }

        [DefaultValue(null)]
        [Browsable(false)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [TemplateContainer(typeof(RepeaterItem))]
        [TemplateInstance(TemplateInstance.Single)]
        public ITemplate EmptyDataTemplate { get; set; }

        [DefaultValue("")]
        public string Name
        {
            get { return _name ?? String.Empty; }
            set { _name = value; }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The child objects are disposed when their container is disposed")]
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // Dummy control to which we parent all the data item controls
            Control containerControl = new Control();

            IEnumerable dataItems = ViewData.Eval(Name) as IEnumerable;
            bool hasData = false;
            if (dataItems != null)
            {
                int index = 0;
                foreach (object dataItem in dataItems)
                {
                    hasData = true;
                    RepeaterItem repeaterItem = new RepeaterItem(index, dataItem)
                    {
                        ViewData = new ViewDataDictionary(dataItem),
                    };
                    ItemTemplate.InstantiateIn(repeaterItem);
                    containerControl.Controls.Add(repeaterItem);

                    index++;
                }
            }

            if (!hasData)
            {
                // If there was no data, instantiate the EmptyDataTemplate
                Control emptyDataContainer = new Control();
                EmptyDataTemplate.InstantiateIn(emptyDataContainer);
                containerControl.Controls.Add(emptyDataContainer);
            }

            Controls.Add(containerControl);

            containerControl.DataBind();
        }
    }
}
