// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Reflection.Emit;
using System.Web.Http.Internal;
using System.Web.Http.Properties;

namespace System.Web.Http.Metadata.Providers
{
    public abstract class AssociatedMetadataProvider<TModelMetadata> : ModelMetadataProvider
        where TModelMetadata : ModelMetadata
    {
        private ConcurrentDictionary<Type, TypeInformation> _typeInfoCache = new ConcurrentDictionary<Type, TypeInformation>();

        public sealed override IEnumerable<ModelMetadata> GetMetadataForProperties(object container, Type containerType)
        {
            if (containerType == null)
            {
                throw Error.ArgumentNull("containerType");
            }

            return GetMetadataForPropertiesImpl(container, containerType);
        }

        private IEnumerable<ModelMetadata> GetMetadataForPropertiesImpl(object container, Type containerType)
        {
            TypeInformation typeInfo = GetTypeInformation(containerType);
            foreach (KeyValuePair<string, PropertyInformation> kvp in typeInfo.Properties)
            {
                PropertyInformation propertyInfo = kvp.Value;
                Func<object> modelAccessor = null;
                if (container != null)
                {
                    Func<object, object> propertyGetter = propertyInfo.ValueAccessor;
                    modelAccessor = () => propertyGetter(container);
                }
                yield return CreateMetadataFromPrototype(propertyInfo.Prototype, modelAccessor);
            }
        }

        public sealed override ModelMetadata GetMetadataForProperty(Func<object> modelAccessor, Type containerType, string propertyName)
        {
            if (containerType == null)
            {
                throw Error.ArgumentNull("containerType");
            }
            if (String.IsNullOrEmpty(propertyName))
            {
                throw Error.ArgumentNullOrEmpty("propertyName");
            }

            TypeInformation typeInfo = GetTypeInformation(containerType);
            PropertyInformation propertyInfo;
            if (!typeInfo.Properties.TryGetValue(propertyName, out propertyInfo))
            {
                throw Error.Argument("propertyName", SRResources.Common_PropertyNotFound, containerType, propertyName);
            }

            return CreateMetadataFromPrototype(propertyInfo.Prototype, modelAccessor);
        }

        public sealed override ModelMetadata GetMetadataForType(Func<object> modelAccessor, Type modelType)
        {
            if (modelType == null)
            {
                throw Error.ArgumentNull("modelType");
            }

            TModelMetadata prototype = GetTypeInformation(modelType).Prototype;
            return CreateMetadataFromPrototype(prototype, modelAccessor);
        }

        // Override for creating the prototype metadata (without the accessor)
        protected abstract TModelMetadata CreateMetadataPrototype(IEnumerable<Attribute> attributes, Type containerType, Type modelType, string propertyName);

        // Override for applying the prototype + modelAccess to yield the final metadata
        protected abstract TModelMetadata CreateMetadataFromPrototype(TModelMetadata prototype, Func<object> modelAccessor);

        private TypeInformation GetTypeInformation(Type type)
        {
            // This retrieval is implemented as a TryGetValue/TryAdd instead of a GetOrAdd to avoid the performance cost of creating instance delegates
            TypeInformation typeInfo;
            if (!_typeInfoCache.TryGetValue(type, out typeInfo))
            {
                typeInfo = CreateTypeInformation(type);
                _typeInfoCache.TryAdd(type, typeInfo);
            }
            return typeInfo;
        }

        private TypeInformation CreateTypeInformation(Type type)
        {
            TypeInformation info = new TypeInformation();
            ICustomTypeDescriptor typeDescriptor = TypeDescriptorHelper.Get(type);
            info.TypeDescriptor = typeDescriptor;
            info.Prototype = CreateMetadataPrototype(AsAttributes(typeDescriptor.GetAttributes()), containerType: null, modelType: type, propertyName: null);
            
            Dictionary<string, PropertyInformation> properties = new Dictionary<string, PropertyInformation>();
            foreach (PropertyDescriptor property in typeDescriptor.GetProperties())
            {
                // Avoid re-generating a property descriptor if one has already been generated for the property name
                if (!properties.ContainsKey(property.Name))
                {
                    properties.Add(property.Name, CreatePropertyInformation(type, property));
                }
            }
            info.Properties = properties;

            return info;
        }

        private PropertyInformation CreatePropertyInformation(Type containerType, PropertyDescriptor property)
        {
            PropertyInformation info = new PropertyInformation();
            info.ValueAccessor = CreatePropertyValueAccessor(property);
            info.Prototype = CreateMetadataPrototype(AsAttributes(property.Attributes), containerType, property.PropertyType, property.Name);
            return info;
        }

        // Optimization: yield provides much better performance than the LINQ .Cast<Attribute>() in this case
        private static IEnumerable<Attribute> AsAttributes(IEnumerable attributes)
        {
            foreach (object attribute in attributes)
            {
                yield return attribute as Attribute;
            }
        }

        private static Func<object, object> CreatePropertyValueAccessor(PropertyDescriptor property)
        {
            Type declaringType = property.ComponentType;
            if (declaringType.IsVisible)
            {
                string propertyName = property.Name;
                PropertyInfo propertyInfo = declaringType.GetProperty(propertyName, property.PropertyType);

                if (propertyInfo != null && propertyInfo.CanRead)
                {
                    MethodInfo getMethodInfo = propertyInfo.GetGetMethod();
                    if (getMethodInfo != null)
                    {
                        return CreateDynamicValueAccessor(getMethodInfo, declaringType, propertyName);
                    }
                }
            }

            // If either the type isn't public or we can't find a public getter, use the slow Reflection path
            return container => property.GetValue(container);
        }

        // Uses Lightweight Code Gen to generate a tiny delegate that gets the property value
        // This is an optimization to avoid having to go through the much slower System.Reflection APIs
        // e.g. generates (object o) => (Person)o.Id
        private static Func<object, object> CreateDynamicValueAccessor(MethodInfo getMethodInfo, Type declaringType, string propertyName)
        {
            Contract.Assert(getMethodInfo != null && getMethodInfo.IsPublic && !getMethodInfo.IsStatic);

            Type propertyType = getMethodInfo.ReturnType;
            DynamicMethod dynamicMethod = new DynamicMethod("Get" + propertyName + "From" + declaringType.Name, typeof(object), new Type[] { typeof(object) });
            ILGenerator ilg = dynamicMethod.GetILGenerator();

            // Load the container onto the stack, convert from object => declaring type for the property
            ilg.Emit(OpCodes.Ldarg_0);
            if (declaringType.IsValueType)
            {
                ilg.Emit(OpCodes.Unbox, declaringType);
            }
            else
            {
                ilg.Emit(OpCodes.Castclass, declaringType);
            }

            // if declaring type is value type, we use Call : structs don't have inheritance
            // if get method is sealed or isn't virtual, we use Call : it can't be overridden
            if (declaringType.IsValueType || !getMethodInfo.IsVirtual || getMethodInfo.IsFinal)
            {
                ilg.Emit(OpCodes.Call, getMethodInfo);
            }
            else
            {
                ilg.Emit(OpCodes.Callvirt, getMethodInfo);
            }

            // Box if the property type is a value type, so it can be returned as an object
            if (propertyType.IsValueType)
            {
                ilg.Emit(OpCodes.Box, propertyType);
            }

            // Return property value
            ilg.Emit(OpCodes.Ret);

            return (Func<object, object>)dynamicMethod.CreateDelegate(typeof(Func<object, object>));
        }

        private class TypeInformation
        {
            public ICustomTypeDescriptor TypeDescriptor { get; set; }
            public TModelMetadata Prototype { get; set; }
            public Dictionary<string, PropertyInformation> Properties { get; set; }
        }

        private class PropertyInformation
        {
            public Func<object, object> ValueAccessor { get; set; }
            public TModelMetadata Prototype { get; set; }
        }
    }
}
