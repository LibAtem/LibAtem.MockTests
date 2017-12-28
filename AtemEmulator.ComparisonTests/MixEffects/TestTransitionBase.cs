using System;
using System.Runtime.InteropServices;
using BMDSwitcherAPI;
using Xunit.Abstractions;

namespace AtemEmulator.ComparisonTests.MixEffects
{
    public abstract class TestTransitionBase
    {
        protected readonly ITestOutputHelper Output;
        protected readonly AtemClientWrapper Client;

        protected TestTransitionBase(ITestOutputHelper output, AtemClientWrapper client)
        {
            Client = client;
            Output = output;
        }

        protected T GetMixEffect<T>(AtemComparisonHelper helper) where T : class
        {
            Guid itId = typeof(IBMDSwitcherMixEffectBlockIterator).GUID;
            helper.SdkSwitcher.CreateIterator(ref itId, out var itPtr);
            IBMDSwitcherMixEffectBlockIterator iterator = (IBMDSwitcherMixEffectBlockIterator)Marshal.GetObjectForIUnknown(itPtr);

            iterator.Next(out IBMDSwitcherMixEffectBlock meBlock);
            return meBlock as T;
        }

        protected bool WriteAndFail(string s)
        {
            Output.WriteLine(s);
            return true;
        }
    }
}