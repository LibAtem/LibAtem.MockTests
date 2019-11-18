﻿using LibAtem.State;
using LibAtem.State.Tolerance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests.State
{
    public static class AtemStateComparer
    {
        public static List<string> IgnoreNodes { get; }

        static AtemStateComparer()
        {
            IgnoreNodes = new List<string>();
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

        private static IEnumerable<string> CompareObject(string name, IReadOnlyList<string> ignoreNodes, object state1, object state2)
        {
            if (state1 == null || state2 == null)
            {
                yield return "IsNull: " + name;
                yield break;
            }

            if (state1.GetType() != state2.GetType())
                Assert.True(false, "Mismatched types: " + state1.GetType().Name + ", " + state2.GetType().Name);

            foreach (PropertyInfo prop in state1.GetType().GetProperties())
            {
                if (ignoreNodes.Contains(name + prop.Name))
                    continue;

                object newVal = prop.GetValue(state2);
                if (newVal == null)
                    continue;

                object oldVal = prop.GetValue(state1);

                bool isDictionary = prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>);
                bool isList = prop.PropertyType.IsGenericType && (prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>) || prop.PropertyType.GetGenericTypeDefinition() == typeof(IReadOnlyList<>));
                if (isDictionary)
                {
                    dynamic oldDict = Convert.ChangeType(oldVal, prop.PropertyType);
                    dynamic newDict = Convert.ChangeType(newVal, prop.PropertyType);

                    string newName = name + prop.Name + ".";
                    foreach (dynamic newInner in newDict)
                    {
                        if (!oldDict.ContainsKey(newInner.Key))
                        {
                            yield return "Value: " + newName + newInner.Key + " missing from expected";
                            continue;
                        }

                        dynamic oldInner = oldDict[newInner.Key];

                        string newInnerName = name + prop.Name + "." + newInner.Key + ".";

                        IEnumerable<string> res = CompareObject(newInnerName, ignoreNodes, oldInner, newInner.Value);
                        foreach (string r in res)
                            yield return r;
                    }
                }
                else if (isList)
                {
                    var oldList = (IReadOnlyList<object>)oldVal;
                    var newList = (IReadOnlyList<object>)newVal;

                    if (newList.Count != oldList.Count)
                    {
                        yield return "Value: " + name + prop.Name + " length mismatch Expected: " + oldList.Count + " Actual: " + newList.Count;
                        continue;
                    }

                    string newName = name + prop.Name + ".";
                    for (int i = 0; i < newList.Count; i++)
                    {
                        IEnumerable<string> res = CompareObject($"{newName}{i}.", ignoreNodes, oldList[i], newList[i]);
                        foreach (string r in res)
                            yield return r;
                    }
                }
                else if (prop.PropertyType == typeof(double))
                {
                    ToleranceAttribute attr = prop.GetCustomAttribute<ToleranceAttribute>();
                    if (attr != null)
                    {
                        var oldDbl = (double) oldVal;
                        var newDbl = (double) newVal;

                        if (!attr.AreEqual(oldDbl, newDbl))
                        {
                            yield return "Value: " + name + prop.Name + " Expected: " + oldVal + " Actual: " + newVal;
                        }
                    }
                    else if (!oldVal.Equals(newVal))
                    {
                        yield return "Value: " + name + prop.Name + " Expected: " + oldVal + " Actual: " + newVal;
                    }
                }
                else if (prop.PropertyType == typeof(uint))
                {
                    UintToleranceAttribute attr = prop.GetCustomAttribute<UintToleranceAttribute>();
                    if (attr != null)
                    {
                        var oldDbl = (uint)oldVal;
                        var newDbl = (uint)newVal;

                        if (!attr.AreEqual(oldDbl, newDbl))
                        {
                            yield return "Value: " + name + prop.Name + " Expected: " + oldVal + " Actual: " + newVal;
                        }
                    }
                    else if (!oldVal.Equals(newVal))
                    {
                        yield return "Value: " + name + prop.Name + " Expected: " + oldVal + " Actual: " + newVal;
                    }
                }
                else if (prop.PropertyType == typeof(byte[]))
                {
                    byte[] oldVal2 = (byte[]) oldVal;
                    byte[] newVal2 = (byte[]) newVal;
                    if (!oldVal2.SequenceEqual(newVal2))
                    {
                        yield return "Value: " + name + prop.Name + " Expected: " + oldVal + " Actual: " + newVal;
                    }
                }
                else if (!prop.PropertyType.IsClass || prop.PropertyType == typeof(string))
                {
                    if (!oldVal.Equals(newVal))
                    {
                        yield return "Value: " + name + prop.Name + " Expected: " + oldVal + " Actual: " + newVal;
                    }
                }
                else
                {
                    string newName = name + prop.Name + ".";
                    IEnumerable<string> res = CompareObject(newName, ignoreNodes, oldVal, newVal);
                    foreach (string r in res)
                        yield return r;
                }
            }

        }
    }
}