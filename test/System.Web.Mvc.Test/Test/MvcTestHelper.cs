// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using System.Reflection.Emit;

namespace System.Web.Mvc.Test
{
    internal static class MvcTestHelper
    {
        private static bool _mvcAssembliesCreated;

        public static void CreateMvcAssemblies()
        {
            // Only create MVC assemblies once per appdomain. This method is called from the static ctor of several
            // test classes.
            if (_mvcAssembliesCreated)
            {
                return;
            }

            CreateMvcTestAssembly1();
            CreateMvcTestAssembly2();
            CreateMvcTestAssembly3();
            CreateMvcTestAssembly4();

            _mvcAssembliesCreated = true;
        }

        private static void CreateMvcTestAssembly1()
        {
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
                new AssemblyName("MvcAssembly1"),
                AssemblyBuilderAccess.Save);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(
                "MvcAssembly1", "MvcAssembly1.dll");

            CreateController(moduleBuilder, "NS1a.NS1b.C1Controller");
            CreateController(moduleBuilder, "NS2a.NS2b.C2Controller");

            assemblyBuilder.Save("MvcAssembly1.dll");
        }

        private static void CreateMvcTestAssembly2()
        {
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
                new AssemblyName("MvcAssembly2"),
                AssemblyBuilderAccess.Save);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(
                "MvcAssembly2", "MvcAssembly2.dll");

            CreateController(moduleBuilder, "NS3a.NS3b.C3Controller");
            CreateController(moduleBuilder, "NS4a.NS4b.C4Controller");

            assemblyBuilder.Save("MvcAssembly2.dll");
        }

        private static void CreateMvcTestAssembly3()
        {
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
                new AssemblyName("MvcAssembly3"),
                AssemblyBuilderAccess.Save);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(
                "MvcAssembly3", "MvcAssembly3.dll");

            // Type names (but not namespaces) are the same as those in TestAssembly1
            CreateController(moduleBuilder, "NS3a.NS3b.C1Controller");
            CreateController(moduleBuilder, "NS4a.NS4b.C2Controller");

            assemblyBuilder.Save("MvcAssembly3.dll");
        }

        private static void CreateMvcTestAssembly4()
        {
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
                new AssemblyName("MvcAssembly4"),
                AssemblyBuilderAccess.Save);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(
                "MvcAssembly4", "MvcAssembly4.dll");

            // Namespaces and type names are the same as those in TestAssembly1
            CreateController(moduleBuilder, "NS1a.NS1b.C1Controller");
            CreateController(moduleBuilder, "NS2a.NS2b.C2Controller");

            assemblyBuilder.Save("MvcAssembly4.dll");
        }

        private static void CreateController(ModuleBuilder moduleBuilder, string typeName)
        {
            //namespace {namespace} {
            //    public class {typename} : ControllerBase {
            //        protected virtual void ExecuteCore() {
            //            return;
            //        }
            //    }
            //}

            TypeBuilder controllerTypeBuilder = moduleBuilder.DefineType(typeName, TypeAttributes.Class | TypeAttributes.Public, typeof(ControllerBase));
            MethodBuilder executeMethodBuilder = controllerTypeBuilder.DefineMethod("ExecuteCore", MethodAttributes.Family | MethodAttributes.Virtual, typeof(void), Type.EmptyTypes);
            executeMethodBuilder.GetILGenerator().Emit(OpCodes.Ret);
            controllerTypeBuilder.DefineMethodOverride(executeMethodBuilder, typeof(ControllerBase).GetMethod("ExecuteCore", BindingFlags.Instance | BindingFlags.NonPublic));
            controllerTypeBuilder.CreateType();
        }
    }
}
