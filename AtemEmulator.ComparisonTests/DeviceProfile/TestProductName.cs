using LibAtem.Commands.DeviceProfile;
using Xunit;

namespace AtemEmulator.ComparisonTests.DeviceProfile
{
    public class TestProductName
    {
        [Fact]
        public void Test1()
        {
            using (var conn = new AtemComparisonHelper() {LogLibAtemHandshake = true})
            {
                conn.SdkSwitcher.GetProductName(out string sdkName);
                Assert.NotNull(sdkName);

                var cmds = conn.GetReceivedCommands<ProductIdentifierCommand>();
                Assert.Single(cmds);

                Assert.Equal(sdkName, cmds[0].Name);
                Assert.InRange(sdkName.Length, 5, 40);
            }
        }
    }
}

