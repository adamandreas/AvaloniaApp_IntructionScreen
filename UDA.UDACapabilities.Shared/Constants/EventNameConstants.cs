namespace UDA.UDACapabilities.Shared.Constants;

public class EventNameConstants
{
    public const string PAD_TOUCHED = $"PadTouched";
    public const string PAD_UNTOUCHED = $"PadUntouched";
    public const string LIVE_STREAM = $"LiveStream";
    public const string DATA_PROCESSED = $"DataProcessed";
    public const string PROCESS_FINISHED = $"ProcessFinished"; //EventArgs

    //Shared (Should be in all the capabilities' interfaces)
    public const string PROCESS_STARTED = $"ProcessStarted"; //EventArgs
    public const string PROCESS_ABORTED = $"ProcessAborted"; //EventArgs
    public const string ERROR_OCCURRED = $"ErrorOccurred"; //SharedErrorEventArgs
    public const string DEVICE_INITIALIZED = $"DeviceInitialized"; //DeviceInitializedEventArgs
}
