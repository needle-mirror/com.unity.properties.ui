using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Properties.Tests;
using Unity.Properties.UI;

namespace Tests.Unity.Properties.UI.Editor.Tests
{
    class TypeConstructionUtilityTests
    {
        static class Types
        {
            public class NotConstructableBaseClass
            {
                protected NotConstructableBaseClass() {}
            }

            public class NotConstructableDerivedClass : NotConstructableBaseClass
            {
                protected NotConstructableDerivedClass() {}
            }
            
            public class ConstructableDerivedClass : NotConstructableBaseClass {}
            public class A : ConstructableDerivedClass {}
            public class B : ConstructableDerivedClass {}
            public class C : ConstructableDerivedClass {}
        }
        
        [Test]
        public void GettingAllConstructableTypes_FromGenericType_ReturnsProperCount()
        {
            var types = new List<Type>();
            TypeConstructionUtility.GetAllConstructableTypes<Types.NotConstructableBaseClass>(types);
            Assert.That(types.Count, Is.EqualTo(4));
            
            types.Clear();
            TypeConstructionUtility.GetAllConstructableTypes<Types.A>(types);
            Assert.That(types.Count, Is.EqualTo(1));
            
            types.Clear();
            TypeConstructionUtility.GetAllConstructableTypes<Types.NotConstructableDerivedClass>(types);
            Assert.That(types.Count, Is.EqualTo(0));
        }
        
        [Test]
        public void GettingAllConstructableTypes_FromType_ReturnsProperCount()
        {
            var types = new List<Type>();
            TypeConstructionUtility.GetAllConstructableTypes(typeof(Types.NotConstructableBaseClass), types);
            Assert.That(types.Count, Is.EqualTo(4));
            
            types.Clear();
            TypeConstructionUtility.GetAllConstructableTypes(typeof(Types.A), types);
            Assert.That(types.Count, Is.EqualTo(1));
            
            types.Clear();
            TypeConstructionUtility.GetAllConstructableTypes(typeof(Types.NotConstructableDerivedClass), types);
            Assert.That(types.Count, Is.EqualTo(0));
        }
        
        [Test]
        public void CanBeConstructedFromDerivedType_FromConstructableDerivedType_ReturnsTrue()
        {
            Assert.That(TypeConstructionUtility.CanBeConstructedFromDerivedType<IConstructInterface>(), Is.True);
            Assert.That(TypeConstructionUtility.CanBeConstructedFromDerivedType<AbstractConstructibleBaseType>(), Is.True);
            Assert.That(TypeConstructionUtility.CanBeConstructedFromDerivedType<ConstructibleBaseType>(), Is.True);
        }
        
        [Test]
        public void CanBeConstructedFromDerivedType_FromNonConstructableDerivedType_ReturnsFalse()
        {
            Assert.That(TypeConstructionUtility.CanBeConstructedFromDerivedType<ConstructibleDerivedType>(), Is.False);
            Assert.That(TypeConstructionUtility.CanBeConstructedFromDerivedType<NonConstructibleDerivedType>(), Is.False);
            Assert.That(TypeConstructionUtility.CanBeConstructedFromDerivedType<NoConstructorType>(), Is.False);
            Assert.That(TypeConstructionUtility.CanBeConstructedFromDerivedType<ParameterLessConstructorType>(), Is.False);
            Assert.That(TypeConstructionUtility.CanBeConstructedFromDerivedType<ParameterConstructorType>(), Is.False);
            Assert.That(TypeConstructionUtility.CanBeConstructedFromDerivedType<ScriptableObjectType>(), Is.False);
        }
    }
}