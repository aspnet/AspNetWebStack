// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TestCommon;
using Moq;

namespace System.Net.Http.Formatting
{
    public class JsonNetValidationTest
    {
        public static TheoryDataSet<string, Type, int> Theories
        {
            get
            {
                return new TheoryDataSet<string, Type, int>()
                {
                    // Type coercion

                    {"null", typeof(int), 1},
                    {"45", typeof(string), 0},
                    {"random text", typeof(DateTimeOffset), 1},
                    {"[1,2,3]", typeof(string[]), 0},

                    {"\"foo\"", typeof(int), 1},
                    {"\"foo\"", typeof(DateTime), 1},

                    {"[\"a\",\"b\",\"45\",34]", typeof(int[]), 2},
                    {
                        "[\"a\",\"b\",\"45\",34]",
                        typeof(DateTime[]),
#if NEWTONSOFTJSON10 // Json.NET 10 detects an additional error over earlier versions.
                        5
#else
                        4
#endif
                    },

                    // Required members

                    {"{}", typeof(DataContractWithRequiredMembers), 2},
                    {"[{},{},{}]", typeof(DataContractWithRequiredMembers[]), 6},

                    // Throwing setters

                    {"{\"Throws\":\"foo\"}", typeof(TypeWithThrowingSetter), 1},
                    {"[{\"Throws\":\"foo\"},{\"Throws\":\"foo\"}]", typeof(TypeWithThrowingSetter[]), 2},
                };
            }
        }

#if !NETFX_CORE // IRequiredMemeberSelector is not in portable libraries because there is no model state on the client.
        [Theory]
        [PropertyData("Theories")]
        public async Task ModelErrorsPopulatedWithValidationErrors(string json, Type type, int expectedErrors)
        {
            JsonMediaTypeFormatter formatter = new JsonMediaTypeFormatter();
            formatter.RequiredMemberSelector = new SimpleRequiredMemberSelector();
            Mock<IFormatterLogger> mockLogger = new Mock<IFormatterLogger>() { };

            await JsonNetSerializationTest.DeserializeAsync(json, type, formatter, mockLogger.Object);

            mockLogger.Verify(mock => mock.LogError(It.IsAny<string>(), It.IsAny<Exception>()), Times.Exactly(expectedErrors));
        }
#endif

        [Fact]
        public async Task HittingMaxDepthRaisesOnlyOneValidationError()
        {
            // Arrange
            JsonMediaTypeFormatter formatter = new JsonMediaTypeFormatter();
            Mock<IFormatterLogger> mockLogger = new Mock<IFormatterLogger>();

            StringBuilder sb = new StringBuilder("{'A':null}");
            for (int i = 0; i < 5000; i++)
            {
                sb.Insert(0, "{'A':");
                sb.Append('}');
            }
            string json = sb.ToString();

            // Act
            await JsonNetSerializationTest.DeserializeAsync(json, typeof(Nest), formatter, mockLogger.Object);

            // Assert
            mockLogger.Verify(mock => mock.LogError(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once());
        }
    }

#if !NETFX_CORE // IRequiredMemeberSelector is not in portable libraries because there is no model state on the client.
    // this IRMS treats all member names that start with "Required" as required
    public class SimpleRequiredMemberSelector : IRequiredMemberSelector
    {
        public bool IsRequiredMember(MemberInfo member)
        {
            return member.Name.StartsWith("Required");
        }
    }
#endif

    public class DataContractWithRequiredMembers
    {
        public string Required1;
        public string Required2;
        public string Optional;
    }

    public class TypeWithThrowingSetter
    {
        public string Throws
        {
            get
            {
                return "foo";
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }

    public class Nest
    {
        public Nest A { get; set; }
    }
}