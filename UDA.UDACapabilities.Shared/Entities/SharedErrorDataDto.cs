using UDA.UDACapabilities.Shared.Enums;

namespace UDA.UDACapabilities.Shared.Entities;
public class SharedErrorDataDto
{
    public ErrorCode ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}

public class SharedErrorEventArgs : EventArgs
{
    public SharedErrorDataDto Data { get; init; }

    public SharedErrorEventArgs(SharedErrorDataDto data)
    {
        Data = data;
    }
}
