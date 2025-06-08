using UDA.UDACapabilities.Shared.Enums;

namespace UDA.UDACapabilities.Shared.Entities;

public class DeviceInitializedDataDto
{
    public string? DeviceSerialNumber { get; set; }

    public required DeviceNameEnum UsedDeviceNameEnum { get; set; } = DeviceNameEnum.Undefined;
    public string UsedDeviceName { get { return UsedDeviceNameEnum.ToString(); } }
}

public class DeviceInitializedEventArgs : EventArgs
{
    public DeviceInitializedDataDto Data { get; init; }

    public DeviceInitializedEventArgs(DeviceInitializedDataDto data) { Data = data; }
}
