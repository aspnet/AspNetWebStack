// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Web.WebPages.Html;

namespace System.Web.WebPages
{
    // Class for containing various pieces of data required by a WebPage
    public class WebPageContext
    {
        private static readonly object _sourceFileKey = new object();
        private Stack<TextWriter> _outputStack;
        private Stack<Dictionary<string, SectionWriter>> _sectionWritersStack;
        private IDictionary<object, dynamic> _pageData;
        private ValidationHelper _validation;
        private ModelStateDictionary _modelStateDictionary;

        public WebPageContext()
            : this(context: null, page: null, model: null)
        {
        }

        public WebPageContext(HttpContextBase context, WebPageRenderingBase page, object model)
        {
            HttpContext = context;
            Page = page;
            Model = model;
        }

        public static WebPageContext Current
        {
            get
            {
                // The TemplateStack stores instances of WebPageRenderingBase. 
                // Retrieve the top-most item from the stack and cast it to WebPageBase. 
                var httpContext = Web.HttpContext.Current;
                if (httpContext != null)
                {
                    var contextWrapper = new HttpContextWrapper(httpContext);
                    var currentTemplate = TemplateStack.GetCurrentTemplate(contextWrapper);
                    var currentPage = (currentTemplate as WebPageRenderingBase);

                    return (currentPage == null) ? null : currentPage.PageContext;
                }
                return null;
            }
        }

        internal HttpContextBase HttpContext { get; set; }

        public object Model { get; internal set; }

        internal ModelStateDictionary ModelState
        {
            get
            {
                if (_modelStateDictionary == null)
                {
                    _modelStateDictionary = new ModelStateDictionary();
                }
                return _modelStateDictionary;
            }
            private set { _modelStateDictionary = value; }
        }

        internal ValidationHelper Validation
        {
            get
            {
                if (_validation == null)
                {
                    Debug.Assert(HttpContext != null, "HttpContext must be initalized for Validation to work.");
                    _validation = new ValidationHelper(HttpContext, ModelState);
                }
                return _validation;
            }
            private set { _validation = value; }
        }

        internal Action<TextWriter> BodyAction { get; set; }

        internal Stack<TextWriter> OutputStack
        {
            get
            {
                if (_outputStack == null)
                {
                    _outputStack = new Stack<TextWriter>();
                }
                return _outputStack;
            }
            set { _outputStack = value; }
        }

        public WebPageRenderingBase Page { get; internal set; }

        public IDictionary<object, dynamic> PageData
        {
            get
            {
                if (_pageData == null)
                {
                    _pageData = new PageDataDictionary<dynamic>();
                }
                return _pageData;
            }
            internal set { _pageData = value; }
        }

        internal Stack<Dictionary<string, SectionWriter>> SectionWritersStack
        {
            get
            {
                if (_sectionWritersStack == null)
                {
                    _sectionWritersStack = new Stack<Dictionary<string, SectionWriter>>();
                }
                return _sectionWritersStack;
            }
            set { _sectionWritersStack = value; }
        }

        // NOTE: We use a hashset because order doesn't matter and we want to eliminate duplicates
        internal HashSet<string> SourceFiles
        {
            get
            {
                HashSet<string> sourceFiles = HttpContext.Items[_sourceFileKey] as HashSet<string>;
                if (sourceFiles == null)
                {
                    sourceFiles = new HashSet<string>();
                    HttpContext.Items[_sourceFileKey] = sourceFiles;
                }
                return sourceFiles;
            }
        }

        internal static WebPageContext CreateNestedPageContext<TModel>(WebPageContext parentContext, IDictionary<object, dynamic> pageData, TModel model, bool isLayoutPage)
        {
            var nestedContext = new WebPageContext
            {
                HttpContext = parentContext.HttpContext,
                OutputStack = parentContext.OutputStack,
                Validation = parentContext.Validation,
                PageData = pageData,
                Model = model,
                ModelState = parentContext.ModelState,
            };

            if (isLayoutPage)
            {
                nestedContext.BodyAction = parentContext.BodyAction;
                nestedContext.SectionWritersStack = parentContext.SectionWritersStack;
            }
            return nestedContext;
        }
    }
}
