// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Web.WebPages.Administration.PackageManager;
using Microsoft.TestCommon;
using Moq;
using NuGet.Runtime;

namespace System.Web.WebPages.Administration.Test
{
    public class RemoteAssemblyTest
    {
        [Fact]
        public void GetAssembliesForBindingRedirectReturnsEmptySequenceIfNoBinAssembliesAreFound()
        {
            // Act
            var assemblies = RemoteAssembly.GetAssembliesForBindingRedirect(AppDomain.CurrentDomain, @"x:\site\bin", (_, __) => Enumerable.Empty<IAssembly>());

            // Assert
            Assert.Empty(assemblies);
        }

        [Fact]
        public void RemoteAssemblyComparesById()
        {
            // Arrange
            var assemblyA = new RemoteAssembly("A", null, null, null);
            var assemblyB = new Mock<IAssembly>(MockBehavior.Strict);
            assemblyB.SetupGet(b => b.Name).Returns("Z").Verifiable();

            // Act
            var result = RemoteAssembly.Compare(assemblyA, assemblyB.Object);

            // Assert
            Assert.Equal(-25, result);
            assemblyB.Verify();
        }

        [Fact]
        public void RemoteAssemblyComparesByVersionIfIdsAreIdentical()
        {
            // Arrange
            var assemblyA = new RemoteAssembly("A", new Version("2.0.0.0"), null, null);
            var assemblyB = new Mock<IAssembly>(MockBehavior.Strict);
            assemblyB.SetupGet(b => b.Name).Returns("A").Verifiable();
            assemblyB.SetupGet(b => b.Version).Returns(new Version("1.0.0.0")).Verifiable();

            // Act
            var result = RemoteAssembly.Compare(assemblyA, assemblyB.Object);

            // Assert
            Assert.Equal(1, result);
            assemblyB.Verify();
        }

        [Fact]
        public void RemoteAssemblyComparesByPublicKeyIfIdsAndVersionAreIdentical()
        {
            // Arrange
            var assemblyA = new RemoteAssembly("A", new Version("1.0.0.0"), "C", null);
            var assemblyB = new Mock<IAssembly>(MockBehavior.Strict);
            assemblyB.SetupGet(b => b.Name).Returns("A").Verifiable();
            assemblyB.SetupGet(b => b.Version).Returns(new Version("1.0.0.0")).Verifiable();
            assemblyB.SetupGet(b => b.PublicKeyToken).Returns("E").Verifiable();

            // Act
            var result = RemoteAssembly.Compare(assemblyA, assemblyB.Object);

            // Assert
            Assert.Equal(-2, result);
            assemblyB.Verify();
        }

        [Fact]
        public void RemoteAssemblyComparesByCultureIfIdVersionAndPublicKeyAreIdentical()
        {
            // Arrange
            var assemblyA = new RemoteAssembly("A", new Version("1.0.0.0"), "public-key", "en-us");
            var assemblyB = new Mock<IAssembly>(MockBehavior.Strict);
            assemblyB.SetupGet(b => b.Name).Returns("A").Verifiable();
            assemblyB.SetupGet(b => b.Version).Returns(new Version("1.0.0.0")).Verifiable();
            assemblyB.SetupGet(b => b.PublicKeyToken).Returns("public-key").Verifiable();
            assemblyB.SetupGet(b => b.Culture).Returns("en-uk").Verifiable();

            // Act
            var result = RemoteAssembly.Compare(assemblyA, assemblyB.Object);

            // Assert
            Assert.Equal(8, result);
            assemblyB.Verify();
        }

        [Fact]
        public void RemoteAssemblyReturns0IfAllValuesAreIdentical()
        {
            // Arrange
            var assemblyA = new RemoteAssembly("A", new Version("1.0.0.0"), "public-key", "en-us");
            var assemblyB = new RemoteAssembly("A", new Version("1.0.0.0"), "public-key", "en-us");

            // Act
            var result = RemoteAssembly.Compare(assemblyA, assemblyB);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void RemoteAssemblyReturns1IfValueToBeComparedToIsNull()
        {
            // Arrange
            RemoteAssembly assemblyA = new RemoteAssembly("A", new Version("1.0.0.0"), "public-key", "en-us");
            RemoteAssembly assemblyB = null;

            // Act
            var result = assemblyA.CompareTo(assemblyB);

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public void EqualReturnsTrueIfValuesAreIdentical()
        {
            // Arrange
            var assemblyA = new RemoteAssembly("A", new Version("1.0.0.0"), "public-key", "en-us");
            var assemblyB = new RemoteAssembly("A", new Version("1.0.0.0"), "public-key", "en-us");

            // Act
            var result = assemblyA.Equals(assemblyB);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void EqualReturnsFalseIfValuesAreNotIdentical()
        {
            // Arrange
            var assemblyA = new RemoteAssembly("A", new Version("1.0.0.0"), "public-key", "en-us");
            var assemblyB = new RemoteAssembly("A", new Version("1.0.0.1"), "public-key", "en-us");

            // Act
            var result = assemblyA.Equals(assemblyB);

            // Assert
            Assert.False(result);
        }
    }
}
