// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Filters
{
    public class FilterInfoComparerTest
    {
        public static TheoryDataSet<FilterInfo, FilterInfo, int> CompareTestData
        {
            get
            {
                IFilter f = new Mock<IFilter>().Object;
                return new TheoryDataSet<FilterInfo, FilterInfo, int>()
                {
                    { null, null, 0 },
                    { new FilterInfo(f, FilterScope.Action), null, 1 },
                    { null, new FilterInfo(f, FilterScope.Action), -1 },
                    { new FilterInfo(f, FilterScope.Action), new FilterInfo(f, FilterScope.Action), 0 },
                    { new FilterInfo(f, FilterScope.Controller), new FilterInfo(f, FilterScope.Action), -1 },
                    { new FilterInfo(f, FilterScope.Action), new FilterInfo(f, FilterScope.Controller), 1 },
                };
            }
        }

        [Theory]
        [PropertyData("CompareTestData")]
        public void Compare(FilterInfo x, FilterInfo y, int expectedSign)
        {
            int result = FilterInfoComparer.Instance.Compare(x, y);

            Assert.Equal(expectedSign, Math.Sign(result));
        }
    }
}
