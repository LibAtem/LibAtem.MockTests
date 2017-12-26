using LibAtem.Commands.DeviceProfile;
using Xunit;

namespace AtemEmulator.ComparisonTests.DeviceProfile
{
    public class TestProductName : AtemCommandTestBase
    {
        protected override bool LogLibAtemHandshake => true;

        [Fact]
        public void Test1()
        {
            _sdkSwitcher.GetProductName(out string sdkName);
            Assert.NotNull(sdkName);

            var cmds = GetReceivedCommands<ProductIdentifierCommand>();
            Assert.Single(cmds);

            Assert.Equal(sdkName, cmds[0].Name);
            Assert.InRange(sdkName.Length, 5, 40);
        }
    }
}

