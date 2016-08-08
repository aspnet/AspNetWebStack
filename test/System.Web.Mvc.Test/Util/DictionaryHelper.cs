// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.TestCommon;

namespace System.Web.TestUtil
{
    public class DictionaryHelper<TKey, TValue>
    {
        public IEqualityComparer<TKey> Comparer { get; set; }

        public Func<IDictionary<TKey, TValue>> Creator { get; set; }

        public IList<TKey> SampleKeys { get; set; }

        public IList<TValue> SampleValues { get; set; }

        public bool SkipItemPropertyTest { get; set; }

        public bool ThrowOnKeyNotFound { get; set; }

        public void Execute()
        {
            ValidateProperties();

            Executor executor = new Executor()
            {
                Comparer = Comparer,
                Creator = Creator,
                ThrowOnKeyNotFound = ThrowOnKeyNotFound,
                Values = SampleValues.ToArray()
            };
            SeparateKeys(out executor.ExcludedKey, out executor.ConflictingKeys, out executor.NonConflictingKeys);

            executor.TestAdd1();
            executor.TestAdd1ThrowsArgumentExceptionIfKeyAlreadyInDictionary();
            executor.TestAdd2();
            executor.TestClear();
            executor.TestContains();
            executor.TestContainsKey();
            executor.TestCopyTo();
            executor.TestCountProperty();
            executor.TestGetEnumerator();
            executor.TestGetEnumeratorGeneric();
            executor.TestIsReadOnlyProperty();

            if (!SkipItemPropertyTest)
            {
                executor.TestItemProperty();
            }

            executor.TestKeysProperty();
            executor.TestRemove1();
            executor.TestRemove2();
            executor.TestTryGetValue();
            executor.TestValuesProperty();
        }

        private void SeparateKeys(out TKey excludedKey, out TKey[] conflictingKeys, out TKey[] nonConflictingKeys)
        {
            List<TKey> nonConflictingKeyList = new List<TKey>();
            TKey[] newConflictingKeys = null;

            var keyLookup = SampleKeys.ToLookup(key => key, Comparer);
            foreach (var entry in keyLookup)
            {
                if (entry.Count() == 1)
                {
                    // not a conflict
                    nonConflictingKeyList.AddRange(entry);
                }
                else
                {
                    // conflict
                    newConflictingKeys = entry.ToArray();
                }
            }

            excludedKey = nonConflictingKeyList[nonConflictingKeyList.Count - 1];
            nonConflictingKeyList.RemoveAt(nonConflictingKeyList.Count - 1);
            conflictingKeys = newConflictingKeys;
            nonConflictingKeys = nonConflictingKeyList.ToArray();
        }

        private void ValidateProperties()
        {
            if (Creator == null)
            {
                throw new InvalidOperationException("The Creator property must not be null.");
            }
            if (SampleKeys == null || SampleKeys.Count < 4)
            {
                throw new InvalidOperationException("The SampleKeys property must contain at least 4 elements.");
            }
            if (SampleValues == null || SampleValues.Count != SampleKeys.Count)
            {
                throw new InvalidOperationException("The SampleValues property must contain as many elements as the SampleKeys property.");
            }

            HashSet<TKey> keys = new HashSet<TKey>(SampleKeys, Comparer);
            if (keys.Count != SampleKeys.Count - 1)
            {
                throw new InvalidOperationException("The SampleKeys property must contain exactly one colliding keypair using the given comparer.");
            }
        }

        private class Executor
        {
            public IEqualityComparer<TKey> Comparer;
            public Func<IDictionary<TKey, TValue>> Creator;
            public TKey ExcludedKey;
            public TKey[] ConflictingKeys;
            public TKey[] NonConflictingKeys;
            public bool ThrowOnKeyNotFound;
            public TValue[] Values;

            private IEnumerable<KeyValuePair<TKey, TValue>> MakeKeyValuePairs()
            {
                return MakeKeyValuePairs(false /* includeConflictingKeys */);
            }

            private IEnumerable<KeyValuePair<TKey, TValue>> MakeKeyValuePairs(bool includeConflictingKeys)
            {
                for (int i = 0; i < NonConflictingKeys.Length; i++)
                {
                    TKey key = NonConflictingKeys[i];
                    TValue value = Values[i];
                    yield return new KeyValuePair<TKey, TValue>(key, value);
                }
                if (includeConflictingKeys)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        TKey key = ConflictingKeys[i];
                        TValue value = Values[NonConflictingKeys.Length + i];
                        yield return new KeyValuePair<TKey, TValue>(key, value);
                    }
                }
            }

            public void TestAdd1()
            {
                // Arrange
                Dictionary<TKey, TValue> controlDictionary = new Dictionary<TKey, TValue>(Comparer);
                IDictionary<TKey, TValue> testDictionary = Creator();

                // Act
                foreach (var entry in MakeKeyValuePairs())
                {
                    controlDictionary.Add(entry.Key, entry.Value);
                    testDictionary.Add(entry.Key, entry.Value);
                }

                // Assert
                VerifyDictionaryEntriesEqual(controlDictionary, testDictionary);
            }

            public void TestAdd1ThrowsArgumentExceptionIfKeyAlreadyInDictionary()
            {
                // Arrange
                IDictionary<TKey, TValue> testDictionary = Creator();

                // Act & assert
                var pairs = MakeKeyValuePairs(true /* includeConflictingKeys */).Skip(NonConflictingKeys.Length).ToArray();
                testDictionary.Add(pairs[0].Key, pairs[1].Value);

                Assert.Throws<ArgumentException>(
                    delegate { testDictionary.Add(pairs[1].Key, pairs[1].Value); },
                    "An item with the same key has already been added."
                    );
            }

            public void TestAdd2()
            {
                // Arrange
                Dictionary<TKey, TValue> controlDictionary = new Dictionary<TKey, TValue>(Comparer);
                IDictionary<TKey, TValue> testDictionary = Creator();

                // Act
                foreach (var entry in MakeKeyValuePairs())
                {
                    ((IDictionary<TKey, TValue>)controlDictionary).Add(entry);
                    testDictionary.Add(entry);
                }

                // Assert
                VerifyDictionaryEntriesEqual(controlDictionary, testDictionary);
            }

            public void TestClear()
            {
                // Arrange
                IDictionary<TKey, TValue> testDictionary = Creator();

                // Act
                foreach (var entry in MakeKeyValuePairs())
                {
                    testDictionary.Add(entry);
                }
                testDictionary.Clear();

                // Assert
                Assert.Empty(testDictionary);
            }

            public void TestCountProperty()
            {
                // Arrange
                Dictionary<TKey, TValue> controlDictionary = new Dictionary<TKey, TValue>(Comparer);
                IDictionary<TKey, TValue> testDictionary = Creator();

                // Act & assert
                foreach (var entry in MakeKeyValuePairs())
                {
                    controlDictionary.Add(entry.Key, entry.Value);
                    testDictionary.Add(entry.Key, entry.Value);
                    Assert.Equal(controlDictionary.Count, testDictionary.Count);
                }
            }

            public void TestContains()
            {
                // Arrange
                IDictionary<TKey, TValue> testDictionary = Creator();

                // Act
                foreach (var entry in MakeKeyValuePairs())
                {
                    testDictionary.Add(entry);
                }

                // Assert
                var shouldBeFound = MakeKeyValuePairs().First();
                var shouldNotBeFound = new KeyValuePair<TKey, TValue>(ExcludedKey, Values[Values.Length - 1]);
                Assert.True(testDictionary.Contains(shouldBeFound), String.Format("Test dictionary should have contained entry for KVP '{0}'.", shouldBeFound));
                Assert.False(testDictionary.Contains(shouldNotBeFound), String.Format("Test dictionary should not have contained entry for KVP '{0}'.", shouldNotBeFound));
            }

            public void TestContainsKey()
            {
                // Arrange
                IDictionary<TKey, TValue> testDictionary = Creator();

                // Act
                foreach (var entry in MakeKeyValuePairs())
                {
                    testDictionary.Add(entry);
                }

                // Assert
                Assert.True(testDictionary.ContainsKey(NonConflictingKeys[0]), String.Format("Test dictionary should have contained entry for key '{0}'.", NonConflictingKeys[0]));
                Assert.False(testDictionary.ContainsKey(ExcludedKey), String.Format("Test dictionary should not have contained entry for key '{0}'.", ExcludedKey));
            }

            public void TestCopyTo()
            {
                // Arrange
                IDictionary<TKey, TValue> controlDictionary = new Dictionary<TKey, TValue>(Comparer);
                IDictionary<TKey, TValue> testDictionary = Creator();

                foreach (var entry in MakeKeyValuePairs())
                {
                    controlDictionary.Add(entry.Key, entry.Value);
                    testDictionary.Add(entry.Key, entry.Value);
                }
                KeyValuePair<TKey, TValue>[] testKvps = new KeyValuePair<TKey, TValue>[testDictionary.Count + 2];

                // Act
                testDictionary.CopyTo(testKvps, 2);

                // Assert
                for (int i = 0; i < 2; i++)
                {
                    var defaultValue = default(KeyValuePair<TKey, TValue>);
                    var entry = testKvps[i];
                    Assert.Equal(defaultValue, entry);
                }
                for (int i = 2; i < testKvps.Length; i++)
                {
                    var entry = testKvps[i];
                    Assert.True(controlDictionary.Contains(entry), String.Format("The value '{0}' wasn't present in the control dictionary.", entry));
                    controlDictionary.Remove(entry);
                }

                Assert.Empty(controlDictionary);
            }

            public void TestGetEnumerator()
            {
                // Arrange
                Dictionary<TKey, TValue> controlDictionary = new Dictionary<TKey, TValue>(Comparer);
                IDictionary<TKey, TValue> testDictionary = Creator();

                foreach (var entry in MakeKeyValuePairs())
                {
                    controlDictionary.Add(entry.Key, entry.Value);
                    testDictionary.Add(entry.Key, entry.Value);
                }

                IEnumerable testDictionaryAsEnumerable = (IEnumerable)testDictionary;

                // Act
                Dictionary<TKey, TValue> newTestDictionary = new Dictionary<TKey, TValue>(Comparer);
                foreach (object entry in testDictionaryAsEnumerable)
                {
                    var kvp = Assert.IsType<KeyValuePair<TKey, TValue>>(entry);
                    newTestDictionary.Add(kvp.Key, kvp.Value);
                }

                // Assert
                VerifyDictionaryEntriesEqual(controlDictionary, newTestDictionary);
            }

            public void TestGetEnumeratorGeneric()
            {
                // Arrange
                Dictionary<TKey, TValue> controlDictionary = new Dictionary<TKey, TValue>(Comparer);
                IDictionary<TKey, TValue> testDictionary = Creator();

                foreach (var entry in MakeKeyValuePairs())
                {
                    controlDictionary.Add(entry.Key, entry.Value);
                    testDictionary.Add(entry.Key, entry.Value);
                }

                // Act & assert
                VerifyDictionaryEntriesEqual(controlDictionary, testDictionary);
            }

            public void TestIsReadOnlyProperty()
            {
                // Arrange
                IDictionary<TKey, TValue> testDictionary = Creator();

                // Act & assert
                Assert.False(testDictionary.IsReadOnly, "Dictionary should not be read only.");
            }

            public void TestItemProperty()
            {
                // Arrange
                Dictionary<TKey, TValue> controlDictionary = new Dictionary<TKey, TValue>(Comparer);
                IDictionary<TKey, TValue> testDictionary = Creator();

                var shouldBeFound = MakeKeyValuePairs().First();
                var shouldNotBeFound = new KeyValuePair<TKey, TValue>(ExcludedKey, Values[Values.Length - 1]);

                // Act & assert
                foreach (var entry in MakeKeyValuePairs())
                {
                    controlDictionary.Add(entry.Key, entry.Value);
                    testDictionary[entry.Key] = entry.Value;
                }
                VerifyDictionaryEntriesEqual(controlDictionary, testDictionary);

                TValue value = testDictionary[shouldBeFound.Key];
                Assert.Equal(shouldBeFound.Value, value);

                if (ThrowOnKeyNotFound)
                {
                    Assert.Throws<KeyNotFoundException>(
                        delegate { TValue valueNotFound = testDictionary[shouldNotBeFound.Key]; }, allowDerivedExceptions: true);
                }
                else
                {
                    TValue valueNotFound = testDictionary[shouldNotBeFound.Key];
                    Assert.Equal(default(TValue), valueNotFound); // Should not throw
                }
            }

            public void TestKeysProperty()
            {
                // Arrange
                Dictionary<TKey, TValue> controlDictionary = new Dictionary<TKey, TValue>(Comparer);
                IDictionary<TKey, TValue> testDictionary = Creator();

                foreach (var entry in MakeKeyValuePairs())
                {
                    controlDictionary.Add(entry.Key, entry.Value);
                    testDictionary.Add(entry.Key, entry.Value);
                }

                // Act
                HashSet<TKey> controlKeys = new HashSet<TKey>(controlDictionary.Keys, Comparer);
                HashSet<TKey> testKeys = new HashSet<TKey>(testDictionary.Keys, Comparer);

                // Assert
                Assert.True(controlKeys.SetEquals(testKeys), "Control dictionary and test dictionary key sets were not equal.");
            }

            public void TestRemove1()
            {
                // Arrange
                Dictionary<TKey, TValue> controlDictionary = new Dictionary<TKey, TValue>(Comparer);
                IDictionary<TKey, TValue> testDictionary = Creator();

                foreach (var entry in MakeKeyValuePairs())
                {
                    controlDictionary.Add(entry.Key, entry.Value);
                    testDictionary.Add(entry.Key, entry.Value);
                }

                // Act
                bool removalSuccess = testDictionary.Remove(NonConflictingKeys[0]);
                bool removalFailure = testDictionary.Remove(ExcludedKey);

                // Assert
                Assert.True(removalSuccess, "Remove() should return true if the key was removed.");
                Assert.False(removalFailure, "Remove() should return false if the key was not removed.");

                controlDictionary.Remove(NonConflictingKeys[0]);
                VerifyDictionaryEntriesEqual(controlDictionary, testDictionary);
            }

            public void TestRemove2()
            {
                // Arrange
                Dictionary<TKey, TValue> controlDictionary = new Dictionary<TKey, TValue>(Comparer);
                IDictionary<TKey, TValue> testDictionary = Creator();

                foreach (var entry in MakeKeyValuePairs())
                {
                    ((IDictionary<TKey, TValue>)controlDictionary).Add(entry);
                    testDictionary.Add(entry);
                }

                // Act
                var shouldBeFound = MakeKeyValuePairs().First();
                var shouldNotBeFound = new KeyValuePair<TKey, TValue>(ExcludedKey, Values[Values.Length - 1]);
                bool removalSuccess = testDictionary.Remove(shouldBeFound);
                bool removalFailure = testDictionary.Remove(shouldNotBeFound);

                // Assert
                Assert.True(removalSuccess, "Remove() should return true if the key was removed.");
                Assert.False(removalFailure, "Remove() should return false if the key was not removed.");

                ((IDictionary<TKey, TValue>)controlDictionary).Remove(shouldBeFound);
                VerifyDictionaryEntriesEqual(controlDictionary, testDictionary);
            }

            public void TestTryGetValue()
            {
                // Arrange
                Dictionary<TKey, TValue> controlDictionary = new Dictionary<TKey, TValue>(Comparer);
                IDictionary<TKey, TValue> testDictionary = Creator();

                foreach (var entry in MakeKeyValuePairs())
                {
                    controlDictionary.Add(entry.Key, entry.Value);
                    testDictionary.Add(entry.Key, entry.Value);
                }

                var shouldBeFound = MakeKeyValuePairs().First();
                var shouldNotBeFound = new KeyValuePair<TKey, TValue>(ExcludedKey, Values[Values.Length - 1]);

                // Act
                TValue value1;
                bool returned1 = testDictionary.TryGetValue(shouldBeFound.Key, out value1);
                TValue value2;
                bool returned2 = testDictionary.TryGetValue(shouldNotBeFound.Key, out value2);

                // Assert
                Assert.True(returned1, String.Format("The entry '{0}' should have been found.", shouldBeFound));
                Assert.Equal(shouldBeFound.Value, value1);
                Assert.False(returned2, String.Format("The entry '{0}' should not have been found.", shouldNotBeFound));
                Assert.Equal(default(TValue), value2);
            }

            public void TestValuesProperty()
            {
                // Arrange
                Dictionary<TKey, TValue> controlDictionary = new Dictionary<TKey, TValue>(Comparer);
                IDictionary<TKey, TValue> testDictionary = Creator();

                foreach (var entry in MakeKeyValuePairs())
                {
                    controlDictionary.Add(entry.Key, entry.Value);
                    testDictionary.Add(entry.Key, entry.Value);
                }

                // Act
                List<TValue> controlValues = controlDictionary.Values.ToList();
                List<TValue> testValues = testDictionary.Values.ToList();

                // Assert
                foreach (var entry in controlValues)
                {
                    Assert.True(testValues.Contains(entry), String.Format("Test dictionary did not contain value '{0}'.", entry));
                }
                foreach (var entry in testValues)
                {
                    Assert.True(controlValues.Contains(entry), String.Format("Control dictionary did not contain value '{0}'.", entry));
                }
            }

            private void VerifyDictionaryEntriesEqual(Dictionary<TKey, TValue> controlDictionary, IDictionary<TKey, TValue> testDictionary)
            {
                Assert.Equal(controlDictionary.Count, testDictionary.Count);

                Dictionary<TKey, TValue> clonedControlDictionary = new Dictionary<TKey, TValue>(controlDictionary, controlDictionary.Comparer);

                foreach (var entry in testDictionary)
                {
                    var key = entry.Key;
                    Assert.True(clonedControlDictionary.ContainsKey(entry.Key), String.Format("Control dictionary did not contain key '{0}'.", key));
                    clonedControlDictionary.Remove(key);
                }

                foreach (var entry in clonedControlDictionary)
                {
                    var key = entry.Key;
                    Assert.True(false, String.Format("Test dictionary did not contain key '{0}'.", key));
                }
            }
        }
    }
}
