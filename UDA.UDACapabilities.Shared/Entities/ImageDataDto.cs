using UDA.UDACapabilities.Shared.Enums;

namespace UDA.UDACapabilities.Shared.Entities;

public class ImageDataDto
{
    public required UdaImage ImageDetails { get; set; }

    public required DeviceNameEnum UsedDeviceNameEnum { get; set; } = DeviceNameEnum.Undefined;
    public string UsedDeviceName { get { return UsedDeviceNameEnum.ToString(); } }
}

public class ImageEventArgs : EventArgs
{
    public ImageDataDto Data { get; init; }

    public ImageEventArgs(ImageDataDto data)
    {
        Data = data;
    }
}
