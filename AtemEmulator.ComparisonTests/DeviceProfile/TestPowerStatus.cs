using BMDSwitcherAPI;
using LibAtem.Commands.DeviceProfile;
using Xunit;

namespace AtemEmulator.ComparisonTests.DeviceProfile
{
    public class TestPowerStatus
    {
        [Fact]
        public void TestStatus()
        {
            using (var conn = new AtemComparisonHelper() {LogLibAtemHandshake = true})
            {
                var cmd = conn.GetSingleReceivedCommands<PowerStatusCommand>();
                Assert.True(cmd.Pin1);
                Assert.False(cmd.Pin2);

                _BMDSwitcherPowerStatus status;
                conn.SdkSwitcher.GetPowerStatus(out status);

                Assert.True(status.HasFlag(_BMDSwitcherPowerStatus.bmdSwitcherPowerStatusSupply1));
                Assert.False(status.HasFlag(_BMDSwitcherPowerStatus.bmdSwitcherPowerStatusSupply2));
            }
        }
    }
}