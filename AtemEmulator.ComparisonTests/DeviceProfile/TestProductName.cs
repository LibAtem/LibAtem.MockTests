using LibAtem.Commands.DeviceProfile;
using Xunit;

namespace AtemEmulator.ComparisonTests.DeviceProfile
{
    [Collection("Client")]
    public class TestProductName
    {
        private AtemClientWrapper _client;

        public TestProductName(AtemClientWrapper client)
        {
            _client = client;
        }

        [Fact]
        public void Test1()
        {
            using (var conn = new AtemComparisonHelper(_client))
            {
                conn.SdkSwitcher.GetProductName(out string sdkName);
                Assert.NotNull(sdkName);

                var cmd = conn.FindWithMatching(new ProductIdentifierCommand());
                Assert.NotNull(cmd);

                Assert.Equal(sdkName, cmd.Name);
                Assert.InRange(sdkName.Length, 5, 40);
            }
        }
    }
}

