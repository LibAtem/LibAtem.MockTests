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
            _client.SdkSwitcher.GetPowerStatus(out _BMDSwitcherPowerStatus status);

            Assert.Equal(cmd.Pin1, status.HasFlag(_BMDSwitcherPowerStatus.bmdSwitcherPowerStatusSupply1));
            Assert.Equal(cmd.Pin2, status.HasFlag(_BMDSwitcherPowerStatus.bmdSwitcherPowerStatusSupply2));
        }
    }
}