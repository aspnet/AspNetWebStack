// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
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

        public static TheoryDataSet<LambdaExpression, string> ComplicatedLambdaExpressions
        {
            get
            {
                var collection = new List<DummyModelContainer>();
                var index = 20;
                var data = new TheoryDataSet<LambdaExpression, string>
                {

                    {
                        Lambda((List<DummyModelContainer> m) => collection[10].Model.FirstName),
                        "collection[10].Model.FirstName"
                    },
                    {
                        Lambda((List<DummyModelContainer> m) => m[10].Model.FirstName),
                        "[10].Model.FirstName"
                    },
                    {
                        Lambda((List<DummyModelContainer> m) => collection[index].Model.FirstName),
                        "collection[20].Model.FirstName"
                    },
                    {
                        Lambda((List<DummyModelContainer> m) => m[index].Model.FirstName),
                        "[20].Model.FirstName"
                    },
                };

                return data;
            }
        }

        [Theory]
        [PropertyData("ComplicatedLambdaExpressions")]
        public void GetExpressionText_WithComplicatedLambdaExpressions_ReturnsExpectedText(
            LambdaExpression expression,
            string expectedText)
        {
            // Arrange & Act
            var result = ExpressionHelper.GetExpressionText(expression);

            // Assert
            Assert.Equal(expectedText, result);
        }

        [Fact]
        public void GetExpressionText_WithinALoop_ReturnsExpectedText()
        {
            // Arrange 0
            var collection = new List<DummyModelContainer>();

            for (var i = 0; i < 2; i++)
            {
                // Arrange 1
                var expectedText = string.Format("collection[{0}].Model.FirstName", i);

                // Act 1
                var result = ExpressionHelper.GetExpressionText(Lambda(
                    (List<DummyModelContainer> m) => collection[i].Model.FirstName));

                // Assert 1
                Assert.Equal(expectedText, result);

                // Arrange 2
                expectedText = string.Format("[{0}].Model.FirstName", i);

                // Act 2
                result = ExpressionHelper.GetExpressionText(Lambda(
                    (List<DummyModelContainer> m) => m[i].Model.FirstName));

                // Assert 2
                Assert.Equal(expectedText, result);
            }
        }

        // Helpers

        private static LambdaExpression Lambda<T1, T2>(Expression<Func<T1, T2>> expression)
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
