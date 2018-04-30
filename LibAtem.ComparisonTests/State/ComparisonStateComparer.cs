using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests.State
{
    public static class ComparisonStateComparer
    {
        public static bool AreEqual(ITestOutputHelper output, ComparisonState state1, ComparisonState state2)
        {
            return CompareObject(output, "", state1, state2);
        }

        private static bool CompareObject(ITestOutputHelper output, string name, object state1, object state2)
        {
            if (state1 == null || state2 == null)
            {
                output.WriteLine("IsNull: " + name);
                return false;
            }

            if (state1.GetType() != state2.GetType())
                Assert.True(false, "Mismatched types: " + state1.GetType().Name + ", " + state2.GetType().Name);

            bool res = true;
            foreach (PropertyInfo prop in state1.GetType().GetProperties())
            {
                object newVal = prop.GetValue(state2);
                if (newVal == null)
                    continue;

                object oldVal = prop.GetValue(state1);

                bool isDictionary = prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>);
                bool isList = prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>);
                if (prop.PropertyType == typeof(double))
                {
                    ToleranceAttribute attr = prop.GetCustomAttribute<ToleranceAttribute>();
                    if (attr != null)
                    {
                        var oldDbl = (double) oldVal;
                        var newDbl = (double) newVal;

                        if (Math.Abs(oldDbl-newDbl) > attr.Tolerance)
                        {
                            output.WriteLine("Value: " + name + prop.Name + " Expected: " + oldVal + " Actual: " + newVal);
                            res = false;
                        }
                    }
                    else if (!oldVal.Equals(newVal))
                    {
                        output.WriteLine("Value: " + name + prop.Name + " Expected: " + oldVal + " Actual: " + newVal);
                        res = false;
                    }
                } 
                else if (!prop.PropertyType.IsClass)
                {
                    if (!oldVal.Equals(newVal))
                    {
                        output.WriteLine("Value: " + name + prop.Name + " Expected: " + oldVal + " Actual: " + newVal);
                        res = false;
                    }
                }
                else if (isDictionary)
                {
                    dynamic oldDict = Convert.ChangeType(oldVal, prop.PropertyType);
                    dynamic newDict = Convert.ChangeType(newVal, prop.PropertyType);

                    string newName = name + prop.Name + ".";
                    foreach (dynamic newInner in newDict)
                    {
                        dynamic oldInner = oldDict[newInner.Key];

                        res = res && CompareObject(output, newName, oldInner, newInner.Value);
                    }
                }
                else if (isList)
                {
                    dynamic oldList = Convert.ChangeType(oldVal, prop.PropertyType);
                    dynamic newList = Convert.ChangeType(newVal, prop.PropertyType);

                    string newName = name + prop.Name + ".";
                    for (int i=0; i < newList.Count; i++)
                    {
                        res = res && CompareObject(output, newName, oldList[i], newList[i]);
                    }
                }
                else
                {
                    string newName = name + prop.Name + ".";
                    res = res && CompareObject(output, newName, oldVal, newVal);
                }
            }

            return res;
        }
    }
}