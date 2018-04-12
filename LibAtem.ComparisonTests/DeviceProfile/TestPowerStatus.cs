using BMDSwitcherAPI;
using LibAtem.Commands.DeviceProfile;
using Xunit;

namespace LibAtem.ComparisonTests.DeviceProfile
{
    [Collection("Client")]
    public class TestPowerStatus
    {
        private readonly AtemClientWrapper _client;

        public TestPowerStatus(AtemClientWrapper client)
        {
            _client = client;
        }

        [Fact]
        public void TestStatus()
        {
            var cmd = _client.FindWithMatching(new PowerStatusCommand());
            Assert.True(cmd.Pin1);
            Assert.False(cmd.Pin2);

            _client.SdkSwitcher.GetPowerStatus(out _BMDSwitcherPowerStatus status);

            Assert.True(status.HasFlag(_BMDSwitcherPowerStatus.bmdSwitcherPowerStatusSupply1));
            Assert.False(status.HasFlag(_BMDSwitcherPowerStatus.bmdSwitcherPowerStatusSupply2));
        }
    }
}