using LibAtem.State;
using LibAtem.State.Tolerance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.Util
{
    public static class AtemStateComparer
    {
        public static List<string> IgnoreNodes { get; }

        static AtemStateComparer()
        {
            IgnoreNodes = new List<string>
            {
                "Info.Version",
                "Info.Model",
                "Info.LastTimecode"
            };
        }

        public static List<string> AreEqual(AtemState state1, AtemState state2)
        {
            IReadOnlyList<string> ignoreNodes = IgnoreNodes.ToList();
            return CompareObject("", ignoreNodes, state1, state2).ToList();
        }
        public static bool AreEqual(ITestOutputHelper output, AtemState state1, AtemState state2)
        {
            IReadOnlyList<string> ignoreNodes = IgnoreNodes.ToList();
            List<string> res = CompareObject("", ignoreNodes, state1, state2).ToList();

            foreach (string r in res)
                output.WriteLine(r);

            return res.Count == 0;
        }


        public static IEnumerable<string> CompareObject(string name, IReadOnlyList<string> ignoreNodes, object state1, object state2, PropertyInfo prop = null)
        {
            if (ignoreNodes.Contains(name))
                yield break;

            if (state1 == null)
            {
                yield return "IsNull (Expected): " + name;
                yield break;
            }
            if (state2 == null)
            {
                yield return "IsNull (Actual): " + name;
                yield break;
            }

            var stateType = state1.GetType();
            if (stateType != state2.GetType())
                Assert.Fail("Mismatched types: " + stateType.Name + ", " + state2.GetType().Name);


            bool isDictionary = stateType.IsGenericType && stateType.GetGenericTypeDefinition() == typeof(Dictionary<,>);
            bool isList = stateType.IsGenericType && (stateType.GetGenericTypeDefinition() == typeof(List<>) || stateType.GetGenericTypeDefinition() == typeof(IReadOnlyList<>));
            if (isDictionary)
            {
                dynamic oldDict = Convert.ChangeType(state1, stateType);
                dynamic newDict = Convert.ChangeType(state2, stateType);

                if (newDict == null || newDict?.Count != oldDict?.Count)
                {
                    yield return "Value: " + name + " length mismatch Expected: " + oldDict?.Count + " Actual: " + newDict?.Count;
                    yield break;
                }

                string newName = name + ".";
                foreach (dynamic newInner in newDict)
                {
                    if (!oldDict.ContainsKey(newInner.Key))
                    {
                        yield return "Value: " + newName + newInner.Key + " missing from expected";
                        continue;
                    }

                    dynamic oldInner = oldDict[newInner.Key];

                    string newInnerName = newName + newInner.Key; 

                    IEnumerable<string> res = CompareObject(newInnerName, ignoreNodes, oldInner, newInner.Value);
                    foreach (string r in res)
                        yield return r;
                }
            }
            else if (isList)
            {
                dynamic oldList = state1;
                dynamic newList = state2;

                if (newList?.Count != oldList?.Count)
                {
                    yield return "Value: " + name + " length mismatch Expected: " + oldList?.Count +
                                 " Actual: " + newList?.Count;
                    yield break;
                }

                for (int i = 0; i < newList.Count; i++)
                {
                    IEnumerable<string> res = CompareObject($"{name}.{i}", ignoreNodes, oldList[i],
                        newList[i]);
                    foreach (string r in res)
                        yield return r;
                }
            }
            else if (stateType.IsArray)
            {
                dynamic oldList = state1;
                dynamic newList = state2;

                if (newList?.Length != oldList?.Length)
                {
                    yield return "Value: " + name + " length mismatch Expected: " + oldList?.Length + " Actual: " + newList?.Length;
                    yield break;
                }

                for (int i = 0; i < newList.Length; i++)
                {
                    IEnumerable<string> res = CompareObject($"{name}.{i}", ignoreNodes, oldList[i], newList[i]);
                    foreach (string r in res)
                        yield return r;
                }
            }
            else if (stateType == typeof(double))
            {
                ToleranceAttribute attr = prop.GetCustomAttribute<ToleranceAttribute>();
                if (attr != null)
                {
                    var oldDbl = (double)state1;
                    var newDbl = (double)state2;

                    if (!attr.AreEqual(oldDbl, newDbl))
                    {
                        yield return "Value: " + name + " Expected: " + state1 + " Actual: " + state2;
                    }
                }
                else if (!state1.Equals(state2))
                {
                    yield return "Value: " + name + " Expected: " + state1 + " Actual: " + state2;
                }
            }
            else if (stateType == typeof(double[]))
            {
                var oldDbl = (double[])state1;
                var newDbl = (double[])state2;

                string arrToStr(double[] arr) => "[" + string.Join(", ", arr.Select(a => $"{a:0.####}")) + "]";

                ToleranceAttribute attr = prop.GetCustomAttribute<ToleranceAttribute>();
                if (attr != null)
                {
                    if (!oldDbl.SequenceEqual(newDbl, attr))
                    {
                        yield return "Value: " + name + " Expected: " + arrToStr(oldDbl) + " Actual: " + arrToStr(newDbl);
                    }
                }
                else if (!oldDbl.SequenceEqual(newDbl))
                {
                    yield return "Value: " + name + " Expected: " + arrToStr(oldDbl) + " Actual: " + arrToStr(newDbl);
                }
            }
            else if (stateType == typeof(uint))
            {
                UintToleranceAttribute attr = prop.GetCustomAttribute<UintToleranceAttribute>();
                if (attr != null)
                {
                    var oldDbl = (uint)state1;
                    var newDbl = (uint)state2;

                    if (!attr.AreEqual(oldDbl, newDbl))
                    {
                        yield return "Value: " + name + " Expected: " + state1 + " Actual: " + state2;
                    }
                }
                else if (!state1.Equals(state2))
                {
                    yield return "Value: " + name + " Expected: " + state1 + " Actual: " + state2;
                }
            }
            else if (stateType == typeof(byte[]))
            {
                byte[] oldVal2 = (byte[])state1;
                byte[] newVal2 = (byte[])state2;
                if (oldVal2 == null || newVal2 == null || !oldVal2.SequenceEqual(newVal2))
                {
                    yield return "Value: " + name + " Expected: " + state1 + " Actual: " + state2;
                }
            }
            else if (!stateType.IsClass || stateType == typeof(string))
            {
                if (state1 == null || !state1.Equals(state2))
                {
                    yield return "Value: " + name + " Expected: " + state1 + " Actual: " + state2;
                }
            }
            else
            {
                foreach (PropertyInfo prop2 in state1.GetType().GetProperties())
                {
                    if (ignoreNodes.Contains(name + prop2.Name))
                        continue;

                    object newVal = prop2.GetValue(state2);
                    object oldVal = prop2.GetValue(state1);

                    // Both null is good
                    if (newVal == null && oldVal == null)
                        continue;

                    string newName = name.Length > 0 ? name + "." + prop2.Name : prop2.Name;
                    IEnumerable<string> res = CompareObject(newName, ignoreNodes, oldVal, newVal, prop2);
                    foreach (string r in res)
                        yield return r;

                }
            }

        }
    }
}