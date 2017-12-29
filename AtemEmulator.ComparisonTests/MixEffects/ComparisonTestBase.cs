using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BMDSwitcherAPI;
using LibAtem.Util;
using Xunit.Abstractions;

namespace AtemEmulator.ComparisonTests.MixEffects
{
    public abstract class ComparisonTestBase
    {
        protected readonly ITestOutputHelper Output;
        protected readonly AtemClientWrapper Client;

        protected ComparisonTestBase(ITestOutputHelper output, AtemClientWrapper client)
        {
            Client = client;
            Output = output;
        }

        protected T GetMixEffect<T>() where T : class
        {
            Guid itId = typeof(IBMDSwitcherMixEffectBlockIterator).GUID;
            Client.SdkSwitcher.CreateIterator(ref itId, out IntPtr itPtr);
            IBMDSwitcherMixEffectBlockIterator iterator = (IBMDSwitcherMixEffectBlockIterator)Marshal.GetObjectForIUnknown(itPtr);

            iterator.Next(out IBMDSwitcherMixEffectBlock meBlock);
            return meBlock as T;
        }

        protected List<T> GetKeyers<T>() where T : class
        {
            IBMDSwitcherMixEffectBlock me = GetMixEffect<IBMDSwitcherMixEffectBlock>();

            Guid itId = typeof(IBMDSwitcherKeyIterator).GUID;
            me.CreateIterator(ref itId, out IntPtr itPtr);
            IBMDSwitcherKeyIterator iterator = (IBMDSwitcherKeyIterator)Marshal.GetObjectForIUnknown(itPtr);

            List<T> result = new List<T>();
            for (iterator.Next(out IBMDSwitcherKey r); r != null; iterator.Next(out r))
                result.AddIfNotNull(r as T);

            return result;
        }

        protected bool WriteAndFail(string s)
        {
            Output.WriteLine(s);
            return true;
        }
    }
}