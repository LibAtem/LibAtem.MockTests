using System;
using System.Collections.Generic;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.DataTransfer;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests
{
    [Collection("Client")]
    public class TestLocks
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemClientWrapper _client;

        public TestLocks(ITestOutputHelper output, AtemClientWrapper client)
        {
            _client = client;
            _output = output;
        }

        #region Helpers

        private class LockCallback : IBMDSwitcherLockCallback
        {
            public bool Locked { get; private set; }

            public void Obtained()
            {
                Locked = true;
            }
        }

        private class LockHelper
        {
            private readonly LockCallback _callback;

            public Action Lock { get; }
            public Action Unlock { get; }

            public bool Locked => _callback.Locked;

            public LockHelper(IBMDSwitcherStills stills)
            {
                _callback = new LockCallback();
                Lock = () => stills.Lock(_callback);
                Unlock = () => stills.Unlock(_callback);
            }

            public LockHelper(IBMDSwitcherClip clip)
            {
                _callback = new LockCallback();
                Lock = () => clip.Lock(_callback);
                Unlock = () => clip.Unlock(_callback);
            }
        }

        private LockHelper GetLockHelperByIndex(uint index)
        {
            var pool = _client.SdkSwitcher as IBMDSwitcherMediaPool;
            Assert.NotNull(pool);

            if (index == 0)
            {
                pool.GetStills(out IBMDSwitcherStills stills);
                Assert.NotNull(stills);

                return new LockHelper(stills);
            }

            pool.GetClip(index - 1, out IBMDSwitcherClip clip);
            Assert.NotNull(clip);

            return new LockHelper(clip);
        }

        #endregion Helpers

        [Fact]
        public void TestLock()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                // get lkst of each type and ensure unlocked
                var lockCommands = helper.FindAllOfType<LockStateChangedCommand>();
                // Assert.NotEmpty(lockCommands);
                // Assert.True(lockCommands.Count <= 3); // Any more types are unknown

                foreach (LockStateChangedCommand lck in lockCommands)
                    Assert.False(lck.Locked);

                var pool = helper.SdkSwitcher as IBMDSwitcherMediaPool;
                Assert.NotNull(pool);

                // for each type
                foreach (LockStateChangedCommand lck in lockCommands)
                {
                    helper.ClearReceivedCommands();

                    // try and lock each type via sdk
                    var lockHelper = GetLockHelperByIndex(lck.Index);

                    lockHelper.Lock();
                    helper.Sleep();
                    Assert.True(lockHelper.Locked);

                    // check stored lkst
                    // ensure no lkob
                    LockStateChangedCommand lockCmd = helper.FindWithMatching(lck);
                    Assert.True(lockCmd.Locked);
                    Assert.Empty(helper.GetReceivedCommands<LockObtainedCommand>());

                    // unlock via sdk
                    lockHelper.Unlock();
                    helper.Sleep();

                    // check stored lkst
                    // ensure no lkob
                    lockCmd = helper.FindWithMatching(lck);
                    Assert.False(lockCmd.Locked);
                    Assert.Empty(helper.GetReceivedCommands<LockObtainedCommand>());

                    // lock via lib
                    helper.SendAndWaitForMatching($"Locks.{lck.Index:D}", new LockStateSetCommand() { Index = lck.Index, Locked = true });
                    helper.Sleep();

                    // check stored lkst
                    // ensure got lkob
                    lockCmd = helper.FindWithMatching(lck);
                    Assert.True(lockCmd.Locked);
                    IReadOnlyList<LockObtainedCommand> obCmd = helper.GetReceivedCommands<LockObtainedCommand>();
                    Assert.Equal(1, obCmd.Count);

                    // unlock via lib
                    helper.SendAndWaitForMatching($"Locks.{lck.Index:D}", new LockStateSetCommand() { Index = lck.Index, Locked = false });

                    // check stored lkst
                    // ensure no new lkob
                    lockCmd = helper.FindWithMatching(lck);
                    Assert.False(lockCmd.Locked);
                    obCmd = helper.GetReceivedCommands<LockObtainedCommand>();
                    Assert.Equal(1, obCmd.Count);
                }
            }
        }
    }
}