// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Filters;
using System.Web.Http.ModelBinding;
using System.Web.Http.Tracing.Tracers;
using Microsoft.TestCommon;

namespace System.Web.Http.Tracing
{
    /// <summary>
    /// This test class verifies that all tracers are correctly written.
    /// </summary>
    public class TracerCorrectnessTest
    {
        // Theory data for Tracers.  Tuple is:
        // [0] a framework type known to have a tracer
        // [1] the type of the tracer for that framework type
        // [2] member exclusion list,
        //     A string array containing member names the tracer handles in some way hidden from analysis.
        //     They will not be reported as issues.
        // !!! Exercise care in adding to the exclusion list !!!
        //     It means you have dealt with the issue, not that you're hiding from it.
        public static TheoryDataSet<Type, Type, string[]> AllKnownTracers
        {
            get
            {
                return new TheoryDataSet<Type, Type, string[]>
                {
                    { typeof(ActionFilterAttribute), typeof(ActionFilterAttributeTracer), new string[0] },
                    { typeof(IActionValueBinder), typeof(ActionValueBinderTracer), new string[0] },
                    { typeof(IActionFilter), typeof(ActionFilterTracer), new string[0] },
                    { typeof(AuthorizationFilterAttribute), typeof(AuthorizationFilterAttributeTracer), new string[0] },
                    { typeof(IAuthorizationFilter), typeof(AuthorizationFilterTracer), new string[0] },
                    { typeof(IAuthenticationFilter), typeof(AuthenticationFilterTracer), new string[0] },
                    { typeof(IOverrideFilter), typeof(OverrideFilterTracer), new string[0] },
                    { typeof(BufferedMediaTypeFormatter), typeof(BufferedMediaTypeFormatterTracer), new string[]
                        {
                            // Values copied in ctor
                            "get_BufferSize", "set_BufferSize",
                            "get_SupportedMediaTypes",
                            "get_SupportedEncodings",
                            "get_MediaTypeMappings",
                            "get_RequiredMemberSelector", "set_RequiredMemberSelector",
                            // Cannot override, inner handles correctly
                            "ReadFromStreamAsync", "WriteToStreamAsync", "SelectCharacterEncoding"
                        }
                    },
                    { typeof(IContentNegotiator), typeof(ContentNegotiatorTracer), new string[0] },
                    { typeof(ExceptionFilterAttribute), typeof(ExceptionFilterAttributeTracer), new string[0] },
                    { typeof(IExceptionFilter), typeof(ExceptionFilterTracer), new string[0] },
                    { typeof(IFilter), typeof(FilterTracer), new string[0] },
                    { typeof(FormatterParameterBinding), typeof(FormatterParameterBindingTracer), new string[]
                        {
                            // Handled in base ctor
                            "get_Formatters", "set_Formatters",
                            "get_BodyModelValidator", "set_BodyModelValidator",
                            "get_Descriptor",
                            // Cannot override but handled by overriding ErrorMessage
                            "get_IsValid",
                            "GetValue", "SetValue"
                        }
                    },
                    { typeof(FormUrlEncodedMediaTypeFormatter), typeof(FormUrlEncodedMediaTypeFormatterTracer), new string[]
                        {
                            // Values copied in ctor
                            "get_MaxDepth", "set_MaxDepth",
                            "get_ReadBufferSize", "set_ReadBufferSize",
                            "get_SupportedMediaTypes",
                            "get_SupportedEncodings",
                            "get_MediaTypeMappings",
                            "get_RequiredMemberSelector", "set_RequiredMemberSelector",
                            // Cannot override, base handles correctly via SupportedEncodings
                            "SelectCharacterEncoding",
                        }
                    },
                    { typeof(HttpActionBinding), typeof(HttpActionBindingTracer), new string[]
                        {
                            // Values copied in ctor
                            "get_ActionDescriptor", "set_ActionDescriptor",
                            "get_ParameterBindings", "set_ParameterBindings"
                        }
                    },
                    { typeof(HttpActionDescriptor), typeof(HttpActionDescriptorTracer), new string[]
                        {
                            // Values copied in ctor
                            "get_Configuration", "set_Configuration",
                            "get_ControllerDescriptor", "set_ControllerDescriptor"
                        }
                    },

                    { typeof(IHttpActionInvoker), typeof(HttpActionInvokerTracer), new string[0] },
                    { typeof(IHttpActionSelector), typeof(HttpActionSelectorTracer), new string[0] },
                    { typeof(IHttpControllerActivator), typeof(HttpControllerActivatorTracer), new string[0] },
                    { typeof(HttpControllerDescriptor), typeof(HttpControllerDescriptorTracer), new string[]
                        {
                            // Values copied in ctor
                            "get_Configuration", "set_Configuration",
                            "get_ControllerType", "set_ControllerType",
                            "get_ControllerName", "set_ControllerName"
                        }
                    },
                    { typeof(IHttpControllerSelector), typeof(HttpControllerSelectorTracer), new string[0] },
                    { typeof(IHttpController), typeof(HttpControllerTracer), new string[0] },
                    { typeof(HttpParameterBinding), typeof(HttpParameterBindingTracer), new string[]
                        {
                            // Handled in base ctor
                            "get_Descriptor",
                            // Cannot override but handled by overriding ErrorMessage
                            "get_IsValid",
                            "GetValue", "SetValue",
                        }
                    },
                    { typeof(JsonMediaTypeFormatter), typeof(JsonMediaTypeFormatterTracer), new string[]
                        {
                            // Values copied in ctor
                            "get_MaxDepth", "set_MaxDepth",
                            "get_SupportedMediaTypes",
                            "get_SupportedEncodings",
                            "get_MediaTypeMappings",
                            "get_RequiredMemberSelector", "set_RequiredMemberSelector",
                            "get_Indent", "set_Indent",
                            "get_UseDataContractJsonSerializer", "set_UseDataContractJsonSerializer",
                            "get_SerializerSettings", "set_SerializerSettings",
                            // Cannot override, base handles correctly
                            "SelectCharacterEncoding",
                            // Cannot override behavior, but copying SerializerSettings in ctor captures inner's result
                            "CreateDefaultSerializerSettings"
                        }
                    },
                    { typeof(MediaTypeFormatter), typeof(MediaTypeFormatterTracer), new string[]
                        {
                            // Values copied in ctor
                            "get_SupportedMediaTypes", "get_SupportedEncodings",
                            "get_MediaTypeMappings",
                            "get_RequiredMemberSelector", "set_RequiredMemberSelector",
                            // Cannot override, base handles correctly
                            "SelectCharacterEncoding"
                        }
                    },
                    { typeof(DelegatingHandler), typeof(MessageHandlerTracer),  new string[]
                        {
                            // Value set by framework before we trace
                            "get_InnerHandler", "set_InnerHandler",
                            // Not meant to be delegated to inner, just to base
                            "Dispose"
                        }
                    },
                    { typeof(DelegatingHandler), typeof(RequestMessageHandlerTracer),new string[]
                        {
                            // Value set by framework before we trace
                            "get_InnerHandler", "set_InnerHandler",
                            // Not meant to be delegated to inner, just to base
                            "Dispose"
                        }
                    },
                    { typeof(XmlMediaTypeFormatter), typeof(XmlMediaTypeFormatterTracer), new string[]
                        {
                            // Values copied in ctor
                            "get_SupportedMediaTypes",
                            "get_SupportedEncodings",
                            "get_MediaTypeMappings",
                            "get_RequiredMemberSelector", "set_RequiredMemberSelector",
                            "get_UseXmlSerializer", "set_UseXmlSerializer",
                            "get_Indent", "set_Indent",
                            "get_WriterSettings",
                            "get_MaxDepth", "set_MaxDepth",
                            "InvokeCreateXmlReader", "InvokeCreateXmlWriter",
                            "InvokeGetDeserializer", "InvokeGetSerializer",
                            // Cannot override, base handles correctly
                            "SelectCharacterEncoding",
                            // Assume these are called before starting app.
                            // Tracer does not need to see them,
                            // and inner will uses its copies in read or write
                            "SetSerializer", "RemoveSerializer",
                        }
                    },
                    { typeof(DefaultHttpControllerTypeResolver), typeof(DefaultHttpControllerTypeResolverTracer), new string[0] },
                };
            }
        }

        // Following filters work best if there are no duplicate tracer types in AllKnownTracers. Currently the case.
        public static TheoryDataSet<Type, Type> AllKnownTracers_NoExclusions
        {
            get
            {
                var dataSet = new TheoryDataSet<Type, Type>();
                foreach (var item in AllKnownTracers)
                {
                    dataSet.Add((Type)item[0], (Type)item[1]);
                }

                return dataSet;
            }
        }

        public static TheoryDataSet<Type, string[]> AllKnownTracers_NoInnerType
        {
            get
            {
                var dataSet = new TheoryDataSet<Type, string[]>();
                foreach (var item in AllKnownTracers)
                {
                    dataSet.Add((Type)item[1], (string[])item[2]);
                }

                return dataSet;
            }
        }

        public static TheoryDataSet<Type> AllKnownTracers_JustTracerType
        {
            get
            {
                var dataSet = new TheoryDataSet<Type>();
                foreach (var item in AllKnownTracers)
                {
                    dataSet.Add((Type)item[1]);
                }

                return dataSet;
            }
        }

        [Fact]
        public void All_Tracers_Are_Tested()
        {
            // Arrange & Act
            Type[] allTracerTypes = typeof(ITraceWriter).Assembly.GetTypes().Where(t => !t.IsAbstract && t.Name.EndsWith("Tracer")).ToArray();
            Type[] allKnownTracerTypes = AllKnownTracers.Select<object[], Type>(tds => (Type)tds[1]).ToArray();
            Type[] untestedTypes = allTracerTypes.Where(tAll => !allKnownTracerTypes.Any(tKnown => tKnown == tAll)).ToArray();
            untestedTypes = untestedTypes.OrderBy<Type, string>(t => t.Name).ToArray();

            // Assert
            Assert.True(untestedTypes.Length == 0,
                        String.Format("These tracer types must be added to {0}.AllKnownTracers:{1}        {2}",
                        this.GetType().Name,
                        Environment.NewLine,
                        string.Join(Environment.NewLine + "        ", untestedTypes.Select<Type, string>(t => t.Name))));
        }

        [Theory]
        [PropertyData("AllKnownTracers_NoExclusions")]
        public void All_Tracers_Are_Internal_And_Disposable_When_Inner_Is_Disposable(Type innerType, Type tracerType)
        {
            // Arrange
            TypeAssert.TypeProperties typeProperties = TypeAssert.TypeProperties.IsClass;

            // If the inner is IDisposable, the tracer must be too.
            // The IHttpController case is special -- it must be disposable to invoke the inner.Dispose
            // only if the implementation type is IDisposable.
            if (typeof(IDisposable).IsAssignableFrom(innerType) || innerType == typeof(IHttpController))
            {
                typeProperties |= TypeAssert.TypeProperties.IsDisposable;
            }

            // Act & Assert
            Assert.Type.HasProperties(tracerType, typeProperties);
        }

        [Theory]
        [PropertyData("AllKnownTracers_JustTracerType")]
        public void All_Tracers_Use_Correct_Namespace(Type tracerType)
        {
            // Arrange & Act & Assert
            Assert.Equal("System.Web.Http.Tracing.Tracers", tracerType.Namespace);
        }

        [Theory]
        [PropertyData("AllKnownTracers_NoInnerType")]
        public void All_Excluded_Members_Are_Declared(Type tracerType, string[] exclusions)
        {
            // Arrange & Act
            string[] declaredMembers = tracerType.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Select<MemberInfo, string>(m => m.Name).ToArray();
            string[] unDeclaredExcludedMembers = exclusions.Where(e => !declaredMembers.Contains(e.Substring(e.LastIndexOf('.') + 1))).ToArray();

            // Assert
            Assert.True(unDeclaredExcludedMembers.Length == 0,
                        String.Format("The tracer '{0}' does not declare these members listed for exclusion: {1}",
                                        tracerType.Name,
                                        string.Join(",", unDeclaredExcludedMembers)));
        }

        [Theory]
        [PropertyData("AllKnownTracers")]
        public void All_Tracers_Handle_All_Inner_Members(Type innerType, Type tracerType, string[] excludedMembers)
        {
            // Arrange & Act
            IList<string> issues = DetermineIssues(innerType, tracerType, excludedMembers);

            // Assert
            Assert.True(issues.Count == 0,
                        String.Format("'{0}' does not handle these members from '{1}':{2}        {3}",
                        tracerType.Name,
                        innerType.Name,
                        Environment.NewLine,
                        string.Join(Environment.NewLine + "        ", issues)));
        }

        private static IList<string> DetermineIssues(Type innerType, Type tracerType, string[] excludedMembers)
        {
            List<string> issues = new List<string>();

            MemberInfo[] typeMembers = innerType.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (MemberInfo memberInfo in typeMembers)
            {
                if (memberInfo is ConstructorInfo)
                {
                    continue;
                }

                MethodInfo methodInfo = memberInfo as MethodInfo;
                if (methodInfo != null)
                {
                    AddIssues(tracerType, issues, methodInfo, excludedMembers);
                }
            }

            return issues;
        }

        private static void AddIssues(Type tracerType, IList<string> issues, MethodInfo methodInfo, string[] excludedMembers)
        {
            if (methodInfo == null ||
                !(methodInfo.IsPublic || methodInfo.IsFamily) ||
                methodInfo.DeclaringType == typeof(Object))
            {
                return;
            }

            // Allow exclusion list to be short name or long name, because some members are up the inheritance chain
            string visibleMemberName = String.Format("{0}.{1}", methodInfo.DeclaringType.Name, GetSignature(methodInfo));
            if (!DoesTracerDeclare(tracerType, methodInfo) && !excludedMembers.Contains(visibleMemberName) && !excludedMembers.Contains(methodInfo.Name))
            {
                bool isOverrideable = IsOverrideable(methodInfo);
                bool isSetter = methodInfo.IsSpecialName && methodInfo.Name.StartsWith("set_");
                bool isGetter = methodInfo.IsSpecialName && methodInfo.Name.StartsWith("get_");
                issues.Add(String.Format("{0} [{1}]",
                            visibleMemberName,
                            isOverrideable
                                ? "Override this virtual in the tracer"
                                : isGetter
                                    ? "Capture this value from inner in the tracer's ctor, and add to it to the exclude list"
                                    : isSetter
                                        ? "Ensure this non-virtual setter cannot be called after the tracer has captured it, and add it to the exclude list"
                                        : "Make this member virtual and override it, or add it to the exclude list"));
            }
        }

        private static bool IsOverrideable(MethodInfo methodInfo)
        {
            return !methodInfo.IsFinal && (methodInfo.IsVirtual || methodInfo.IsAbstract);
        }

        private static bool DoMethodsMatch(MethodInfo originalMethodInfo, MethodInfo candidateMethodInfo)
        {
            if (!GetSignature(candidateMethodInfo).Equals(GetSignature(originalMethodInfo)))
            {
                return false;
            }

            return true;
        }

        private static string GetSignature(MethodInfo methodInfo)
        {
            return
                String.Format("{0}({1})", methodInfo.Name.Substring(methodInfo.Name.LastIndexOf(".") + 1),
                    String.Join(",", methodInfo.GetParameters().Select(p => p.ParameterType.Name)));
        }

        private static bool DoesTracerDeclare(Type tracerType, MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                return true;
            }

            MethodInfo matchingMethod = tracerType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(m => DoMethodsMatch(methodInfo, m));
            return matchingMethod != null && matchingMethod.DeclaringType != methodInfo.DeclaringType;
        }
    }
}
