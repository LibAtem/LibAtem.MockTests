using LibAtem.Common;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests
{
    [Collection("Client")]
    public class TestMisc
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemClientWrapper _client;

        public TestMisc(AtemClientWrapper client, ITestOutputHelper output)
        {
            _client = client;
            _output = output;
        }

        [Fact]
        public void TestModel()
        {
            Assert.NotEqual(ModelId.Unknown, _client.Profile.Model);
        }
    }
}