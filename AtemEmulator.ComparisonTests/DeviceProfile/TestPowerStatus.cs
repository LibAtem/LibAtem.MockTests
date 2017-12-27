using BMDSwitcherAPI;
using LibAtem.Commands.DeviceProfile;
using Xunit;

namespace AtemEmulator.ComparisonTests.DeviceProfile
{
    [Collection("Client")]
    public class TestPowerStatus
    {
        private AtemClientWrapper _client;

        public TestPowerStatus(AtemClientWrapper client)
        {
            _client = client;
        }

        [Fact]
        public void TestStatus()
        {
            using (var conn = new AtemComparisonHelper(_client))
            {
                var cmd = conn.FindWithMatching(new PowerStatusCommand());
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