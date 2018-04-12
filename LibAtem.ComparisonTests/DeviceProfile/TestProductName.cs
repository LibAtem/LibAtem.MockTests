using LibAtem.Commands.DeviceProfile;
using Xunit;

namespace AtemEmulator.ComparisonTests.DeviceProfile
{
    [Collection("Client")]
    public class TestProductName
    {
        private readonly AtemClientWrapper _client;

        public TestProductName(AtemClientWrapper client)
        {
            _client = client;
        }

        [Fact]
        public void TestName()
        {
            _client.SdkSwitcher.GetProductName(out string sdkName);
            Assert.NotNull(sdkName);

            var cmd = _client.FindWithMatching(new ProductIdentifierCommand());
            Assert.NotNull(cmd);

            Assert.Equal(sdkName, cmd.Name);
            Assert.InRange(sdkName.Length, 5, 40);
        }
    }
}

