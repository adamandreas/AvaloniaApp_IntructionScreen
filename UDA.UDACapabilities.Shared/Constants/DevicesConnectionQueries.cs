namespace UDA.UDACapabilities.Shared.Constants;
public static class DevicesConnectionQueries
{
    public const string DeviceConnectDetectionQuery = $"SELECT * FROM __InstanceCreationEvent  WITHIN 2 WHERE TargetInstance ISA 'Win32_PnPEntity'";
    public const string DeviceRemovedDetectionQuery = $"SELECT * FROM __InstanceDeletionEvent  WITHIN 2 WHERE TargetInstance ISA 'Win32_PnPEntity'";
}
