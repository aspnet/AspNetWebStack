// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
#if ASPNETWEBAPI
using System.Web.Http.Routing.Constraints;
using ErrorResources = System.Web.Http.Properties.SRResources;
#else
using System.Web.Mvc.Routing.Constraints;
using System.Web.Routing;
using ErrorResources = System.Web.Mvc.Properties.MvcResources;
#endif

#if ASPNETWEBAPI
namespace System.Web.Http.Routing
#else
namespace System.Web.Mvc.Routing
#endif
{
    /// <summary>
    /// The default implementation of <see cref="IInlineConstraintResolver"/>. Resolves constraints by parsing
    /// a constraint key and constraint arguments, using a map to resolve the constraint type, and calling an
    /// appropriate constructor for the constraint type.
    /// </summary>
    public class DefaultInlineConstraintResolver : IInlineConstraintResolver
    {
        private readonly IDictionary<string, Type> _inlineConstraintMap = GetDefaultConstraintMap();

        /// <summary>
        /// Gets the mutable dictionary that maps constraint keys to a particular constraint type.
        /// </summary>
        public IDictionary<string, Type> ConstraintMap
        {
            get
            {
                return _inlineConstraintMap;
            }
        }

        private static IDictionary<string, Type> GetDefaultConstraintMap()
        {
            return new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
            {
                // Type-specific constraints
                { "bool", typeof(BoolRouteConstraint) },
                { "datetime", typeof(DateTimeRouteConstraint) },
                { "decimal", typeof(DecimalRouteConstraint) },
                { "double", typeof(DoubleRouteConstraint) },
                { "float", typeof(FloatRouteConstraint) },
                { "guid", typeof(GuidRouteConstraint) },
                { "int", typeof(IntRouteConstraint) },
                { "long", typeof(LongRouteConstraint) },

                // Length constraints
                { "minlength", typeof(MinLengthRouteConstraint) },
                { "maxlength", typeof(MaxLengthRouteConstraint) },
                { "length", typeof(LengthRouteConstraint) },
                
                // Min/Max value constraints
                { "min", typeof(MinRouteConstraint) },
                { "max", typeof(MaxRouteConstraint) },
                { "range", typeof(RangeRouteConstraint) },

                // Regex-based constraints
                { "alpha", typeof(AlphaRouteConstraint) },
                { "regex", typeof(RegexRouteConstraint) }
            };
        }

        /// <inheritdoc />
#if ASPNETWEBAPI
        public virtual IHttpRouteConstraint ResolveConstraint(string inlineConstraint)
#else
        public virtual IRouteConstraint ResolveConstraint(string inlineConstraint)
#endif
        {
            if (inlineConstraint == null)
            {
                throw Error.ArgumentNull("inlineConstraint");
            }

            string constraintKey;
            string argumentString;
            int indexOfFirstOpenParens = inlineConstraint.IndexOf('(');
            if (indexOfFirstOpenParens >= 0 && inlineConstraint.EndsWith(")", StringComparison.Ordinal))
            {
                constraintKey = inlineConstraint.Substring(0, indexOfFirstOpenParens);
                argumentString = inlineConstraint.Substring(indexOfFirstOpenParens + 1, inlineConstraint.Length - indexOfFirstOpenParens - 2);
            }
            else
            {
                constraintKey = inlineConstraint;
                argumentString = null;
            }

            Type constraintType;
            if (!_inlineConstraintMap.TryGetValue(constraintKey, out constraintType))
            {
                // Cannot resolve the constraint key
                return null;
            }

#if ASPNETWEBAPI
            if (!typeof(IHttpRouteConstraint).IsAssignableFrom(constraintType))
#else
            if (!typeof(IRouteConstraint).IsAssignableFrom(constraintType))
#endif
            {
                throw Error.InvalidOperation(ErrorResources.DefaultInlineConstraintResolver_TypeNotConstraint, constraintType.Name, constraintKey);
            }

#if ASPNETWEBAPI
            return (IHttpRouteConstraint)CreateConstraint(constraintType, argumentString);
#else
            return (IRouteConstraint)CreateConstraint(constraintType, argumentString);
#endif
        }

        private static object CreateConstraint(Type constraintType, string argumentString)
        {
            // No arguments - call the default constructor
            if (argumentString == null)
            {
                return Activator.CreateInstance(constraintType);
            }

            ConstructorInfo activationConstructor = null;
            object[] parameters = null;
            ConstructorInfo[] constructors = constraintType.GetConstructors();

            // If there is only one constructor and it has a single parameter, pass the argument string directly
            // This is necessary for the Regex RouteConstraint to ensure that patterns are not split on commas.
            if (constructors.Length == 1 && constructors[0].GetParameters().Length == 1)
            {
                activationConstructor = constructors[0];
                parameters = ConvertArguments(activationConstructor.GetParameters(), new string[] { argumentString });
            }
            else
            {
                string[] arguments = argumentString.Split(',').Select(argument => argument.Trim()).ToArray();

                ConstructorInfo[] matchingConstructors = constructors.Where(ci => ci.GetParameters().Length == arguments.Length).ToArray();
                int constructorMatches = matchingConstructors.Length;

                if (constructorMatches == 0)
                {
                    throw Error.InvalidOperation(ErrorResources.DefaultInlineConstraintResolver_CouldNotFindCtor, constraintType.Name, argumentString.Length);
                }
                else if (constructorMatches == 1)
                {
                    activationConstructor = matchingConstructors[0];
                    parameters = ConvertArguments(activationConstructor.GetParameters(), arguments);
                }
                else
                {
                    throw Error.InvalidOperation(ErrorResources.DefaultInlineConstraintResolver_AmbiguousCtors, constraintType.Name, argumentString.Length);
                }
            }

            return activationConstructor.Invoke(parameters);
        }

        private static object[] ConvertArguments(ParameterInfo[] parameterInfos, string[] arguments)
        {
            object[] parameters = new object[parameterInfos.Length];
            for (int i = 0; i < parameterInfos.Length; i++)
            {
                ParameterInfo parameter = parameterInfos[i];
                Type parameterType = parameter.ParameterType;
                parameters[i] = Convert.ChangeType(arguments[i], parameterType, CultureInfo.InvariantCulture);
            }
            return parameters;
        }
    }
}
