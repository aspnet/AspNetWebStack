// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class ExpressionHelperTest
    {
        [Fact]
        public void StringBasedExpressionTests()
        {
            ViewDataDictionary vdd = new ViewDataDictionary();

            // Uses the given expression as the expression text
            Assert.Equal("?", ExpressionHelper.GetExpressionText("?"));

            // Exactly "Model" (case-insensitive) is turned into empty string
            Assert.Equal(String.Empty, ExpressionHelper.GetExpressionText("Model"));
            Assert.Equal(String.Empty, ExpressionHelper.GetExpressionText("mOdEl"));

            // Beginning with "Model" is untouched
            Assert.Equal("Model.Foo", ExpressionHelper.GetExpressionText("Model.Foo"));
        }

        [Fact]
        public void LambdaBasedExpressionTextTests()
        {
            // "Model" at the front of the expression is excluded (case insensitively)
            DummyContactModel Model = null;
            Assert.Equal(String.Empty, ExpressionHelper.GetExpressionText(Lambda<object, DummyContactModel>(m => Model)));
            Assert.Equal("FirstName", ExpressionHelper.GetExpressionText(Lambda<object, string>(m => Model.FirstName)));

            DummyContactModel mOdeL = null;
            Assert.Equal(String.Empty, ExpressionHelper.GetExpressionText(Lambda<object, DummyContactModel>(m => mOdeL)));
            Assert.Equal("FirstName", ExpressionHelper.GetExpressionText(Lambda<object, string>(m => mOdeL.FirstName)));

            // Model property of model is passed through
            Assert.Equal("Model", ExpressionHelper.GetExpressionText(Lambda<DummyModelContainer, DummyContactModel>(m => m.Model)));

            // "Model" in the middle of the expression is not excluded
            DummyModelContainer container = null;
            Assert.Equal("container.Model", ExpressionHelper.GetExpressionText(Lambda<object, DummyContactModel>(m => container.Model)));
            Assert.Equal("container.Model.FirstName", ExpressionHelper.GetExpressionText(Lambda<object, string>(m => container.Model.FirstName)));

            // The parameter is excluded
            Assert.Equal(String.Empty, ExpressionHelper.GetExpressionText(Lambda<DummyContactModel, DummyContactModel>(m => m)));
            Assert.Equal("FirstName", ExpressionHelper.GetExpressionText(Lambda<DummyContactModel, string>(m => m.FirstName)));

            // Integer indexer is included and properly computed from captured values
            int x = 2;
            Assert.Equal("container.Model[42].Length", ExpressionHelper.GetExpressionText(Lambda<object, int>(m => container.Model[x * 21].Length)));
            Assert.Equal("[42]", ExpressionHelper.GetExpressionText(Lambda<int[], int>(m => m[x * 21])));

            // String indexer is included and properly computed from captured values
            string y = "Hello world";
            Assert.Equal("container.Model[Hello].Length", ExpressionHelper.GetExpressionText(Lambda<object, int>(m => container.Model[y.Substring(0, 5)].Length)));

            // Back to back indexer is included
            Assert.Equal("container.Model[1024][2]", ExpressionHelper.GetExpressionText(Lambda<object, char>(m => container.Model[x * 512][x])));

            // Multi-parameter indexer is excluded
            Assert.Equal("Length", ExpressionHelper.GetExpressionText(Lambda<object, int>(m => container.Model[42, "Hello World"].Length)));

            // Single array indexer is included
            Assert.Equal("container.Model.Array[1024]", ExpressionHelper.GetExpressionText(Lambda<object, int>(m => container.Model.Array[x * 512])));

            // Double array indexer is excluded
            Assert.Equal("", ExpressionHelper.GetExpressionText(Lambda<object, int>(m => container.Model.DoubleArray[1, 2])));

            // Non-indexer method call is excluded
            Assert.Equal("Length", ExpressionHelper.GetExpressionText(Lambda<object, int>(m => container.Model.Method().Length)));

            // Lambda expression which involves indexer which references lambda parameter throws
            Assert.Throws<InvalidOperationException>(
                () => ExpressionHelper.GetExpressionText(Lambda<string, char>(s => s[s.Length - 4])),
                "The expression compiler was unable to evaluate the indexer expression '(s.Length - 4)' because it references the model parameter 's' which is unavailable.");
        }

        // Helpers

        private LambdaExpression Lambda<T1, T2>(Expression<Func<T1, T2>> expression)
        {
            return expression;
        }

        class DummyContactModel
        {
            public string FirstName { get; set; }

            public string this[int index]
            {
                get { return index.ToString(); }
            }

            public string this[string index]
            {
                get { return index; }
            }

            public string this[int index, string index2]
            {
                get { return index2; }
            }

            public int[] Array { get; set; }

            public int[,] DoubleArray { get; set; }

            public string Method()
            {
                return String.Empty;
            }
        }

        class DummyModelContainer
        {
            public DummyContactModel Model { get; set; }
        }
    }
}
