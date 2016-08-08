// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Web.Http.Tracing;
using Microsoft.TestCommon;

namespace System.Web.Http.Common
{
    public class TraceWriterExceptionMapperTest
    {
        public static TheoryDataSet<Exception, TraceLevel?> GetMappedTraceLevelTestData
        {
            get
            {
                return new TheoryDataSet<Exception, TraceLevel?>
                {
                    { new Exception(), null},
                    { new HttpResponseException(new HttpResponseMessage(HttpStatusCode.OK)), TraceLevel.Info },
                    { new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Accepted)), TraceLevel.Info },
                    { new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)), TraceLevel.Warn },
                    { new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Conflict)), TraceLevel.Warn },
                    { new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)), null },
                };
            }
        }

        [Theory]
        [PropertyData("GetMappedTraceLevelTestData")]
        public void GetMappedTraceLevel_ReturnsExpectedTraceLevel(Exception exception, TraceLevel? expectedTraceLevel)
        {
            Assert.Equal(expectedTraceLevel, TraceWriterExceptionMapper.GetMappedTraceLevel(exception));
        }
    }
}
