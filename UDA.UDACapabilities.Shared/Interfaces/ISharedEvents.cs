using UDA.Shared;
using UDA.UDACapabilities.Shared.Entities;

namespace UDA.UDACapabilities.Shared.Interfaces;

public interface ISharedEvents : IDisposable
{
    //Shared
    public event EventHandler<DeviceInitializedDataDto>? DeviceInitialized;
    public event EventHandler? ProcessStarted;
    public event EventHandler? ProcessAborted;
    public event EventHandler<SharedErrorDataDto>? ErrorOccurred;
    //Events Just For Controller
    public event EventHandler<DeviceStatusEnum>? UpdateDeviceStatus;
    public event EventHandler<Tuple<LogType, string>>? Logger;
}
