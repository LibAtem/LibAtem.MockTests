using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.State;
using Xunit;

namespace LibAtem.MockTests.SdkState
{
    public static class RecordingStateBuilder
    {
        public static void Build(AtemState state, IBMDSwitcher switcher)
        {
#if !ATEM_v8_1
            var recordingSwitcher = switcher as IBMDSwitcherRecordAV;
            if (recordingSwitcher == null) return;

            state.Recording = new RecordingState();

            //recordingSwitcher.IsRecording(out int recording);
            recordingSwitcher.GetStatus(out _BMDSwitcherRecordAVState avState, out _BMDSwitcherRecordAVError error);
            recordingSwitcher.GetFilename(out string filename);
            recordingSwitcher.GetRecordInAllCameras(out int recordInAllCameras);
            recordingSwitcher.GetWorkingSetLimit(out uint workingSetLimit);
            recordingSwitcher.GetActiveDiskIndex(out uint activeDiskIndex);
            recordingSwitcher.GetDuration(out byte hours, out byte minutes, out byte seconds, out byte frames, out int dropFrame);

            recordingSwitcher.GetWorkingSetDisk(0, out uint workingSet1Id);
            recordingSwitcher.GetWorkingSetDisk(1, out uint workingSet2Id);

            recordingSwitcher.GetTotalRecordingTimeAvailable(out uint totalRecordingTimeAvailable);
            state.Recording.Status.State = AtemEnumMaps.RecordingStateMap.FindByValue(avState);
            state.Recording.Status.Error = AtemEnumMaps.RecordingErrorMap.FindByValue(error);
            state.Recording.Status.TotalRecordingTimeAvailable = totalRecordingTimeAvailable;
            state.Recording.Status.Duration = new Timecode
            {
                Hour = hours,
                Minute = minutes,
                Second = seconds,
                Frame = frames,
                DropFrame = dropFrame != 0,
            };

            Assert.Equal(2u, workingSetLimit);
            state.Recording.Properties.Filename = filename;
            state.Recording.Properties.WorkingSet1DiskId = workingSet1Id;
            state.Recording.Properties.WorkingSet2DiskId = workingSet2Id;
            state.Recording.Properties.RecordInAllCameras = recordInAllCameras != 0;

            recordingSwitcher.DoesSupportISORecording(out int supportsIso);
            state.Recording.CanISORecordAllInputs = supportsIso != 0;
            if (supportsIso != 0)
            {
                recordingSwitcher.GetRecordAllISOInputs(out int recordIso);
                state.Recording.ISORecordAllInputs = recordIso != 0;
            }

            var diskIterator = AtemSDKConverter.CastSdk<IBMDSwitcherRecordDiskIterator>(recordingSwitcher.CreateIterator);
            AtemSDKConverter.IterateList<IBMDSwitcherRecordDisk, RecordingState.RecordingDiskState>(diskIterator.Next,
                (sdk, id) =>
                {
                    sdk.GetId(out uint diskId);
                    sdk.GetVolumeName(out string volumeName);
                    sdk.GetRecordingTimeAvailable(out uint recordingTimeAvailable);
                    sdk.GetStatus(out _BMDSwitcherRecordDiskStatus diskStatus);

                    var res = new RecordingState.RecordingDiskState
                    {
                        DiskId = diskId,
                        VolumeName = volumeName,
                        RecordingTimeAvailable = recordingTimeAvailable,
                        Status = AtemEnumMaps.RecordingDiskStatusMap.FindByValue(diskStatus),
                    };
                    state.Recording.Disks.Add(diskId, res);
                    return res;
                });

            // TODO DoesSupportISORecording 
            // TODO activeDiskIndex

#endif
        }
    }
}