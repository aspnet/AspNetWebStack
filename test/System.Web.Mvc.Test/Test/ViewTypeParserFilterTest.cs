// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.UI;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class ViewTypeParserFilterTest
    {
        // Non-generic directives

        [Fact]
        public void NonGenericPageDirectiveDoesNotChangeInheritsDirective()
        {
            var filter = new ViewTypeParserFilter();
            var attributes = new Dictionary<string, string> { { "inherits", "foobar" } };
            var builder = new MvcBuilder();

            filter.PreprocessDirective("page", attributes);
            filter.ParseComplete(builder);

            Assert.Equal("foobar", attributes["inherits"]);
            Assert.Null(builder.Inherits);
        }

        [Fact]
        public void NonGenericControlDirectiveDoesNotChangeInheritsDirective()
        {
            var filter = new ViewTypeParserFilter();
            var attributes = new Dictionary<string, string> { { "inherits", "foobar" } };
            var builder = new MvcBuilder();

            filter.PreprocessDirective("control", attributes);
            filter.ParseComplete(builder);

            Assert.Equal("foobar", attributes["inherits"]);
            Assert.Null(builder.Inherits);
        }

        [Fact]
        public void NonGenericMasterDirectiveDoesNotChangeInheritsDirective()
        {
            var filter = new ViewTypeParserFilter();
            var attributes = new Dictionary<string, string> { { "inherits", "foobar" } };
            var builder = new MvcBuilder();

            filter.PreprocessDirective("master", attributes);
            filter.ParseComplete(builder);

            Assert.Equal("foobar", attributes["inherits"]);
            Assert.Null(builder.Inherits);
        }

        // C#-style generic directives

        [Fact]
        public void CSGenericUnknownDirectiveDoesNotChangeInheritsDirective()
        {
            var filter = new ViewTypeParserFilter();
            var attributes = new Dictionary<string, string> { { "inherits", "foobar<baz>" } };
            var builder = new MvcBuilder();

            filter.PreprocessDirective("unknown", attributes);
            filter.ParseComplete(builder);

            Assert.Equal("foobar<baz>", attributes["inherits"]);
            Assert.Null(builder.Inherits);
        }

        [Fact]
        public void CSGenericPageDirectiveChangesInheritsDirective()
        {
            var filter = new ViewTypeParserFilter();
            var attributes = new Dictionary<string, string> { { "inherits", "foobar<baz>" } };
            var builder = new MvcBuilder();

            filter.PreprocessDirective("page", attributes);
            filter.ParseComplete(builder);

            Assert.Equal(typeof(ViewPage).FullName, attributes["inherits"]);
            Assert.Equal("foobar<baz>", builder.Inherits);
        }

        [Fact]
        public void CSGenericControlDirectiveChangesInheritsDirective()
        {
            var filter = new ViewTypeParserFilter();
            var attributes = new Dictionary<string, string> { { "inherits", "foobar<baz>" } };
            var builder = new MvcBuilder();

            filter.PreprocessDirective("control", attributes);
            filter.ParseComplete(builder);

            Assert.Equal(typeof(ViewUserControl).FullName, attributes["inherits"]);
            Assert.Equal("foobar<baz>", builder.Inherits);
        }

        [Fact]
        public void CSGenericMasterDirectiveChangesInheritsDirective()
        {
            var filter = new ViewTypeParserFilter();
            var attributes = new Dictionary<string, string> { { "inherits", "foobar<baz>" } };
            var builder = new MvcBuilder();

            filter.PreprocessDirective("master", attributes);
            filter.ParseComplete(builder);

            Assert.Equal(typeof(ViewMasterPage).FullName, attributes["inherits"]);
            Assert.Equal("foobar<baz>", builder.Inherits);
        }

        [Fact]
        public void CSDirectivesAfterPageDirectiveProperlyPreserveInheritsDirective()
        {
            var filter = new ViewTypeParserFilter();
            var pageAttributes = new Dictionary<string, string> { { "inherits", "foobar<baz>" } };
            var importAttributes = new Dictionary<string, string> { { "inherits", "dummyvalue<baz>" } };
            var builder = new MvcBuilder();

            filter.PreprocessDirective("page", pageAttributes);
            filter.PreprocessDirective("import", importAttributes);
            filter.ParseComplete(builder);

            Assert.Equal(typeof(ViewPage).FullName, pageAttributes["inherits"]);
            Assert.Equal("foobar<baz>", builder.Inherits);
        }

        // VB.NET-style generic directives

        [Fact]
        public void VBGenericUnknownDirectiveDoesNotChangeInheritsDirective()
        {
            var filter = new ViewTypeParserFilter();
            var attributes = new Dictionary<string, string> { { "inherits", "foobar(of baz)" } };
            var builder = new MvcBuilder();

            filter.PreprocessDirective("unknown", attributes);
            filter.ParseComplete(builder);

            Assert.Equal("foobar(of baz)", attributes["inherits"]);
            Assert.Null(builder.Inherits);
        }

        [Fact]
        public void VBGenericPageDirectiveChangesInheritsDirective()
        {
            var filter = new ViewTypeParserFilter();
            var attributes = new Dictionary<string, string> { { "inherits", "foobar(of baz)" } };
            var builder = new MvcBuilder();

            filter.PreprocessDirective("page", attributes);
            filter.ParseComplete(builder);

            Assert.Equal(typeof(ViewPage).FullName, attributes["inherits"]);
            Assert.Equal("foobar(of baz)", builder.Inherits);
        }

        [Fact]
        public void VBGenericControlDirectiveChangesInheritsDirective()
        {
            var filter = new ViewTypeParserFilter();
            var attributes = new Dictionary<string, string> { { "inherits", "foobar(of baz)" } };
            var builder = new MvcBuilder();

            filter.PreprocessDirective("control", attributes);
            filter.ParseComplete(builder);

            Assert.Equal(typeof(ViewUserControl).FullName, attributes["inherits"]);
            Assert.Equal("foobar(of baz)", builder.Inherits);
        }

        [Fact]
        public void VBGenericMasterDirectiveChangesInheritsDirective()
        {
            var filter = new ViewTypeParserFilter();
            var attributes = new Dictionary<string, string> { { "inherits", "foobar(of baz)" } };
            var builder = new MvcBuilder();

            filter.PreprocessDirective("master", attributes);
            filter.ParseComplete(builder);

            Assert.Equal(typeof(ViewMasterPage).FullName, attributes["inherits"]);
            Assert.Equal("foobar(of baz)", builder.Inherits);
        }

        [Fact]
        public void VBDirectivesAfterPageDirectiveProperlyPreserveInheritsDirective()
        {
            var filter = new ViewTypeParserFilter();
            var pageAttributes = new Dictionary<string, string> { { "inherits", "foobar(of baz)" } };
            var importAttributes = new Dictionary<string, string> { { "inherits", "dummyvalue(of baz)" } };
            var builder = new MvcBuilder();

            filter.PreprocessDirective("page", pageAttributes);
            filter.PreprocessDirective("import", importAttributes);
            filter.ParseComplete(builder);

            Assert.Equal(typeof(ViewPage).FullName, pageAttributes["inherits"]);
            Assert.Equal("foobar(of baz)", builder.Inherits);
        }

        // Helpers

        private class MvcBuilder : RootBuilder, IMvcControlBuilder
        {
            public string Inherits { get; set; }
        }
    }
}
