using UDA.UDACapabilities.Shared.Constants;
using System.Management;
using System.Runtime.Versioning;

namespace UDA.UDACapabilities.Shared;
[SupportedOSPlatform("windows")]
public static class SharedManager
{
    #region Devices Events
    public static event EventHandler? ICamR100_DeviceConnected;
    public static event EventHandler? LF10_DeviceConnected;
    public static event EventHandler? Palm3_DeviceConnected;
    public static event EventHandler? Arduino_DeviceConnected;
    public static event EventHandler? GemaltoCardReader_DeviceConnected;
    public static event EventHandler? SignaturePad_DeviceConnected;
    public static event EventHandler? EpsonV600_DeviceConnected;
    public static event EventHandler? VF1_DeviceConnected;
    #endregion

    private static bool _isSubscribed = false;
    private static WqlEventQuery deviceConnectedQuery = new(DevicesConnectionQueries.DeviceConnectDetectionQuery);
    private static ManagementEventWatcher deviceConnectionWatcher = new(deviceConnectedQuery);


    //private SharedManager()
    //{
    //    ManagementEventWatcher insertWatcher = new(deviceConnectedQuery);
    //    insertWatcher.EventArrived += new EventArrivedEventHandler(DeviceConnectedEventHandler);
    //    insertWatcher.Start();
    //}

    private static void DeviceConnectedEventHandler(object sender, EventArrivedEventArgs e)
    {
        ManagementBaseObject instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
        var connectedDeviceCaption = instance.GetPropertyValue("Caption");
        string processedCaption = string.Join("", ((string)connectedDeviceCaption).Split(default(string[]), StringSplitOptions.RemoveEmptyEntries)).ToLower();

        if (processedCaption.Contains("icamr100"))
        {
            ICamR100_DeviceConnected?.Invoke(sender, new());
            return;
        }
        if (processedCaption.Contains("lf10"))
        {
            LF10_DeviceConnected?.Invoke(sender, new());
            return;
        }
        if (processedCaption.Contains("palm3"))
        {
            Palm3_DeviceConnected?.Invoke(sender, new());
            return;
        }
        if (processedCaption.Contains("arduino"))
        {
            Arduino_DeviceConnected?.Invoke(sender, new());
            return;
        }
        if (processedCaption.Contains("gemalto"))
        {
            GemaltoCardReader_DeviceConnected?.Invoke(sender, new());
            return;
        }
        if (processedCaption.Contains("vendor-defined"))
        {
            SignaturePad_DeviceConnected?.Invoke(sender, new());
            return;
        }
        if (processedCaption.Contains("v600"))
        {
            EpsonV600_DeviceConnected?.Invoke(sender, new());
            return;
        }
        if (processedCaption.Contains("vf1"))
        {
            VF1_DeviceConnected?.Invoke(sender, new());
            return;
        }
    }

    private static void DeviceRemovedEvent(object sender, EventArrivedEventArgs e)
    {

    }

    #region Events Subscription
    //Subscribe
    public static void SubscribeEvents()
    {
        if (_isSubscribed)
            return;

        deviceConnectionWatcher.EventArrived += new EventArrivedEventHandler(DeviceConnectedEventHandler);
        deviceConnectionWatcher.Start();

        _isSubscribed = true;
    }

    //Unsubscribe
    private static void UnsubscribeEvents()
    {
        if (!_isSubscribed)
            return;



        _isSubscribed = false;
    }
    #endregion
}
