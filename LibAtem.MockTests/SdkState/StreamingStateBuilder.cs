using System.Collections.Generic;
using BMDSwitcherAPI;
using LibAtem.State;

namespace LibAtem.MockTests.SdkState
{
    public static class RecordingStateBuilder
    {
        public static void Build(AtemState state, IBMDSwitcher switcher)
        {
            var recordingSwitcher = switcher as IBMDSwitcherRecordAV;
            if (recordingSwitcher == null) return;

            state.Recording = new RecordingState();

            recordingSwitcher.IsRecording(out int recording);
            recordingSwitcher.GetStatus(out _BMDSwitcherRecordAVState avState, out _BMDSwitcherRecordAVError error);
            recordingSwitcher.GetFilename(out string filename);
            recordingSwitcher.GetRecordInAllCameras(out int recordInAllCameras);
            recordingSwitcher.GetWorkingSetLimit(out uint workingSetLimit);
            recordingSwitcher.GetActiveDiskIndex(out uint activeDiskIndex);
            recordingSwitcher.GetDuration(out byte hours, out byte minutes, out byte seconds, out byte frames, out int dropFrame);

            var diskIterator = AtemSDKConverter.CastSdk<IBMDSwitcherRecordDiskIterator>(recordingSwitcher.CreateIterator);
            state.Recording.Disks = AtemSDKConverter.IterateList<IBMDSwitcherRecordDisk, RecordingDiskState>(diskIterator.Next,
                (sdk, id) =>
                {
                    sdk.GetId(out uint diskId);
                    sdk.GetVolumeName(out string volumeName);
                    sdk.GetRecordingTimeAvailable(out uint recordingTimeAvailable);
                    sdk.GetStatus(out _BMDSwitcherRecordDiskStatus diskStatus);

                    return new RecordingDiskState
                    {
                        DiskId = diskId,
                        VolumeName = volumeName,
                        RecordingTimeAvailable = recordingTimeAvailable,
                        // Status = diskStatus
                    }; 
                });
        }
    }
    public static class StreamingStateBuilder
    {
        public static void Build(AtemState state, IBMDSwitcher switcher)
        {
            var streamingSwitcher = switcher as IBMDSwitcherStreamRTMP;
            if (streamingSwitcher == null) return;

            state.Streaming = new StreamingState();

            streamingSwitcher.IsStreaming(out int isStreaming);
            streamingSwitcher.GetStatus(out _BMDSwitcherStreamRTMPState status, out _BMDSwitcherStreamRTMPError error);
            streamingSwitcher.GetServiceName(out string serviceName);
            streamingSwitcher.GetUrl(out string url);
            streamingSwitcher.GetKey(out string key);
            streamingSwitcher.GetBitrates(out uint lowBitrate, out uint highBitrate);
            streamingSwitcher.GetDuration(out byte hours, out byte minutes, out byte seconds, out byte frames, out int isDropFrame);
            streamingSwitcher.GetEncodingBitrate(out uint encodingBitrate);
            streamingSwitcher.GetCacheUsed(out double cacheUsed);

            state.Streaming.Status.IsStreaming = isStreaming != 0;
            state.Streaming.Status.CacheUsed = cacheUsed;
            state.Streaming.Status.EncodingBitrate = encodingBitrate;
            state.Streaming.Status.Duration = new Timecode
            {
                Hour = hours,
                Minute = minutes,
                Second = seconds,
                Frame = frames,
                DropFrame = isDropFrame != 0
            };
            state.Streaming.Status.State = AtemEnumMaps.StreamingStatusMap.FindByValue(status);
            state.Streaming.Status.Error = (int)error;

            state.Streaming.Settings.ServiceName = serviceName;
            state.Streaming.Settings.Url = url;
            state.Streaming.Settings.Key = key;
            state.Streaming.Settings.LowBitrate = lowBitrate;
            state.Streaming.Settings.HighBitrate = highBitrate;


        }
    }
}