using BMDSwitcherAPI;
using LibAtem.Commands.DeviceProfile;
using Xunit;

namespace AtemEmulator.ComparisonTests.DeviceProfile
{
    public class TestPowerStatus : AtemCommandTestBase
    {
        protected override bool LogLibAtemHandshake => true;

        [Fact]
        public void TestStatus()
        {
            var cmd = GetSingleReceivedCommands<PowerStatusCommand>();
            Assert.True(cmd.Pin1);
            Assert.False(cmd.Pin2);

            _BMDSwitcherPowerStatus status;
            _sdkSwitcher.GetPowerStatus(out status);

            Assert.True(status.HasFlag(_BMDSwitcherPowerStatus.bmdSwitcherPowerStatusSupply1));
            Assert.False(status.HasFlag(_BMDSwitcherPowerStatus.bmdSwitcherPowerStatusSupply2));
        }
    }
}