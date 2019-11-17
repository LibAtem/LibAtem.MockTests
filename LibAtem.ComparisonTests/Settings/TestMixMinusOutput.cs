using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BMDSwitcherAPI;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests2.Settings
{
    [Collection("Client")]
    public class TestMixMinusOutput
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemClientWrapper _client;

        public TestMixMinusOutput(ITestOutputHelper output, AtemClientWrapper client)
        {
            _output = output;
            _client = client;
        }

        private static List<IBMDSwitcherMixMinusOutput> GetOutputs(AtemComparisonHelper helper)
        {
            Guid itId = typeof(IBMDSwitcherMixMinusOutputIterator).GUID;
            helper.SdkSwitcher.CreateIterator(ref itId, out IntPtr itPtr);
            IBMDSwitcherMixMinusOutputIterator iterator = (IBMDSwitcherMixMinusOutputIterator)Marshal.GetObjectForIUnknown(itPtr);

            List<IBMDSwitcherMixMinusOutput> result = new List<IBMDSwitcherMixMinusOutput>();
            for (iterator.Next(out IBMDSwitcherMixMinusOutput r); r != null; iterator.Next(out r))
                result.Add(r);

            return result;
        }
        
        [Fact]
        public void TestMixMinusOutputCount()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                List<IBMDSwitcherMixMinusOutput> outputs = GetOutputs(helper);
                Assert.Empty(outputs);
                // TODO - not yet supported by LibAtem
            }
        }
    }
}