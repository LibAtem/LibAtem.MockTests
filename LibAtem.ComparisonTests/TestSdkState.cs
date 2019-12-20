using System.Collections.Generic;
using LibAtem.ComparisonTests.State;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests
{
    public class TestSdkState
    {
        private readonly ITestOutputHelper _output;

        public TestSdkState(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestStatesMatchAfterConnect()
        {
            using (var helper = new AtemClientWrapper())
            {
                List<string> before = AtemStateComparer.AreEqual(helper.SdkState, helper.LibState);
                if (before.Count != 0 && _output != null)
                {
                    _output.WriteLine("state mismatch:");
                    before.ForEach(_output.WriteLine);
                }
                Assert.Empty(before);
            }
        }
    }
}