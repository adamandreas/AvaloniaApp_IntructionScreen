/*using UDA.InstructionScreen.Shared.Entities;
using UDA.Shared;
using UDA.Shared.Abstraction;
using UDA.UDACapabilities.Shared.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using UDA.UDACapabilities.Shared.Constants;
using UDA.InstructionScreen.Licensing;
using UDA.UDACapabilities.Shared.Entities;
using UDA.UDACapabilities.Shared;

namespace UDA.InstructionScreen.Capability;

[SupportedOSPlatform("windows")]
public sealed class InstructionScreenController : CapabilityController<object?, StartupSettings, object?, object?, object?, object?, InstructionScreenController>
{
    #region Fields & Constructor
    private Guid? _requestId;
    private readonly ConfigSettings _configSettings;
    private readonly InstructionScreenManager _instructionScreenManager;
    private bool _isSubscribed;
    private static readonly object _lockObj = new();

    public InstructionScreenController(ILogger<InstructionScreenController> logger, ISettings settings,
        InstructionScreenManager instructionScreenManager) : base(logger)
    {
        #region Licensing
        if (LicenseManager.Instance.LicenseDetails is not null &&
            LicenseManager.Instance.LicenseDetails.IsTimeLimited &&
            LicenseManager.Instance.LicenseDetails.ExpiryDate < DateTime.Now)
        {
            ErrorOccurred(ErrorCode.GeneralAvailabilityError, $"{LicenseCapabilitiesNames.INSTRUCTION_SCREEN}: General Availability Error");
            throw new Exception();
        }

        if (LicenseManager.Instance.LicenseDetails is not null &&
            !(
            LicenseManager.Instance.LicenseDetails.EnabledModules.Contains(LicenseCapabilitiesNames.INSTRUCTION_SCREEN) ||
            LicenseManager.Instance.LicenseDetails.EnabledModules.Contains("instructionScreen")
            ))
        {
            ErrorOccurred(ErrorCode.GeneralAvailabilityError, $"{LicenseCapabilitiesNames.INSTRUCTION_SCREEN}: General Availability Error");
            throw new Exception();
        }
        #endregion

        _instructionScreenManager = instructionScreenManager;

        _configSettings = settings.Configuration
            .GetSection("CapabilitySettings")
            .GetSection("InstructionScreenConfig")
            .GetSection("CapabilitySharedConfig")
            .Get<ConfigSettings>();

        Logger_EventHandler(this, Tuple.Create(LogType.Debug, $"Created New Controller Instance. _configSettings: {JsonConvert.SerializeObject(_configSettings)}"));
    }
    #endregion

    public override bool Init(InitRequestDto<object?> dto)
    {
        lock (_lockObj)
        {
            Logger_EventHandler(this, Tuple.Create(LogType.Debug, $"Entered Controller Init."));

            try
            {
                Logger_EventHandler(this, Tuple.Create(LogType.Debug, $"Controller. Init()"));
                UpdateDeviceStatus(DeviceStatusEnum.Initializing);

                SubscribeEvents();

                Task task = _instructionScreenManager.InitAsync();
                while (!task.IsCompleted)
                    Thread.Sleep(100);

                if (DeviceStatus != DeviceStatusEnum.Ready)
                    UpdateDeviceStatus(DeviceStatusEnum.Undefined);

                return true;
            }
            catch (Exception ex)
            {
                UpdateDeviceStatus(DeviceStatusEnum.Undefined);
                ErrorOccurred(ErrorCode.GENERAL_ERROR, $"{ErrorCode.GENERAL_ERROR}: {ex}");
                return false;
            }
        }
    }

    public override bool Start(StartRequestDto<StartupSettings> dto)
    {
        lock (_lockObj)
        {
            try
            {
                _requestId = dto.RequestId;
                if (_configSettings.EnableLogging)
                    Logger_EventHandler(this, Tuple.Create(LogType.Debug, $"Start(...) requestId: {_requestId}, StartupSettings: {System.Text.Json.JsonSerializer.Serialize(dto.Payload)}"));

                DeviceStatusEnum deviceStatus = DeviceStatus;
                if (deviceStatus is not DeviceStatusEnum.Ready)
                {
                    Logger_EventHandler(this, Tuple.Create(LogType.Warning, $"Unable to Start: device status is '{deviceStatus}' it should be 'Ready'"));
                    return false;
                }

                UpdateDeviceStatus(DeviceStatusEnum.Busy, _configSettings.EnableLogging);

                if (_configSettings.EnableLogging)
                    Logger_EventHandler(this, Tuple.Create(LogType.Debug, $"Controller. Start(...) _instructionScreen_Manager.MainWindow is null?: {_instructionScreenManager.MainWindow}"));

                if (_instructionScreenManager.MainWindow is not null)
                    _instructionScreenManager.Start(dto.Payload);

                UpdateDeviceStatus(DeviceStatusEnum.Ready, _configSettings.EnableLogging);

                return true;
            }
            catch (Exception ex)
            {
                ErrorOccurred(ErrorCode.GENERAL_ERROR, $"{ErrorCode.GENERAL_ERROR}: {ex}");
                return false;
            }
        }
    }

    public override bool Stop(StopRequestDto<object?> dto)
    {
        lock (_lockObj)
        {
            try
            {
                _requestId = dto.RequestId;
                Logger_EventHandler(this, Tuple.Create(LogType.Debug, $"Stop(...) requestId: {_requestId}"));

                DeviceStatusEnum deviceStatus = DeviceStatus;
                if (deviceStatus is not DeviceStatusEnum.Ready)
                {
                    Logger_EventHandler(this, Tuple.Create(LogType.Warning, $"Unable to Stop: device status is '{deviceStatus}' it should be 'Ready'"));
                    return false;
                }

                UpdateDeviceStatus(DeviceStatusEnum.Busy);

                Logger_EventHandler(this, Tuple.Create(LogType.Debug, $"Controller. _instructionScreen_Manager.MainWindow is null?: {_instructionScreenManager.MainWindow}"));

                if (_instructionScreenManager.MainWindow is not null)
                {
                    Logger_EventHandler(this, Tuple.Create(LogType.Debug, $"Controller. Stop()"));

                    _instructionScreenManager.Stop();
                }

                UpdateDeviceStatus(DeviceStatusEnum.Undefined);

                return true;
            }
            catch (Exception ex)
            {
                ErrorOccurred(ErrorCode.GENERAL_ERROR, $"{ErrorCode.GENERAL_ERROR}: {ex}");
                return false;
            }
        }
    }

    public override bool ReInit(ReInitRequestDto<object?> dto)
    {
        lock (_lockObj)
        {
            try
            {
                DeviceStatusEnum deviceStatus = DeviceStatus;
                if (deviceStatus != DeviceStatusEnum.Ready)
                {
                    Logger_EventHandler(this, Tuple.Create(LogType.Warning, $"Unable to ReInit: device status is '{deviceStatus}' it should be 'Ready'"));
                    return false;
                }

                UpdateDeviceStatus(DeviceStatusEnum.Busy);

                if (_instructionScreenManager.MainWindow is not null)
                {
                    Logger_EventHandler(this, Tuple.Create(LogType.Debug, $"Controller. reinit: Stop()"));

                    _instructionScreenManager.Stop();

                    Logger_EventHandler(this, Tuple.Create(LogType.Debug, $"Controller. Init(config)."));

                    var task = _instructionScreenManager.InitAsync();
                    while (!task.IsCompleted)
                        Thread.Sleep(100);
                }

                UpdateDeviceStatus(DeviceStatusEnum.Ready);

                return true;
            }
            catch (Exception e)
            {
                ErrorOccurred(ErrorCode.GENERAL_ERROR, $"{ErrorCode.GENERAL_ERROR}: {e}");
                return false;
            }
        }
    }

    public override bool Other(OtherRequestDto<object?> dto)
    {
        return true;
    }

    public override bool IsDeviceConnected()
    {
        return InstructionScreenManager.IsExtraMonitorConnected();
    }

    #region Events Subscription
    //Subscribe
    private void SubscribeEvents()
    {
        if (_isSubscribed)
            return;
        Logger_EventHandler(this, Tuple.Create(LogType.Debug, $"Controller. SubscribeEvents()"));

        //Shared
        _instructionScreenManager.DeviceInitialized += DeviceInitialized_EventHandler;
        _instructionScreenManager.ProcessStarted += ProcessStarted_EventHandler;
        _instructionScreenManager.ProcessAborted += ProcessAborted_EventHandler;
        _instructionScreenManager.ErrorOccurred += ErrorOccurred_EventHandler;
        //Events Just For Controller
        _instructionScreenManager.UpdateDeviceStatus += UpdateDeviceStatus_FiredEvent;
        _instructionScreenManager.Logger += Logger_EventHandler;

        _isSubscribed = true;
    }

    //Unsubscribe
    private void UnsubscribeEvents()
    {
        if (!_isSubscribed)
            return;
        Logger_EventHandler(this, Tuple.Create(LogType.Debug, $"Controller. UnsubscribeEvents()"));

        //Shared
        _instructionScreenManager.DeviceInitialized -= DeviceInitialized_EventHandler;
        _instructionScreenManager.ProcessStarted -= ProcessStarted_EventHandler;
        _instructionScreenManager.ProcessAborted -= ProcessAborted_EventHandler;
        _instructionScreenManager.ErrorOccurred -= ErrorOccurred_EventHandler;
        //Events Just For Controller
        _instructionScreenManager.UpdateDeviceStatus -= UpdateDeviceStatus_FiredEvent;
        _instructionScreenManager.Logger -= Logger_EventHandler;

        _isSubscribed = false;
    }
    #endregion

    #region Shared Methods
    public override void Dispose()
    {
        Logger_EventHandler(this, Tuple.Create(LogType.Debug, $"Controller. Entered Dispose()."));

        //Never Stop() the InstructionScreen on this Dispose(). Since some implementations do not keep disposing and
        //the Stop() and that closes the InstructionScreen window.

        UnsubscribeEvents();
    }

    private void ErrorOccurred(ErrorCode errorCode, string errorMessage)
    {
        ErrorOccurred_EventHandler(this, new() { ErrorCode = errorCode, ErrorMessage = $"{errorMessage}" });
    }

    private void Logger_EventHandler(object? sender, Tuple<LogType, string> args)
        => _logger.Log(SharedHelper.UdaLogType_To_LogLevel(args.Item1), args.Item2);
    #endregion

    //===================================================================================================

    #region Fire Agent "CallMeConnection" Events
    #region Shared
    private void DeviceInitialized_EventHandler(object? _, DeviceInitializedDataDto args)
    {
        Logger_EventHandler(this, Tuple.Create(LogType.Debug, $"DeviceInitialized_EventHandler(args)"));
        CallMeConnection.FireEvent(EventNameConstants.DEVICE_INITIALIZED, new DeviceInitializedEventArgs(args));
    }
    private void ProcessStarted_EventHandler(object? _, EventArgs args)
    {
        if (_configSettings.EnableLogging)
            Logger_EventHandler(this, Tuple.Create(LogType.Debug, $"ProcessStarted_EventHandler(args)"));
        CallMeConnection.FireEvent(EventNameConstants.PROCESS_STARTED, args);
    }
    private void ProcessAborted_EventHandler(object? _, EventArgs args)
    {
        Logger_EventHandler(this, Tuple.Create(LogType.Debug, $"ProcessAborted_EventHandler(args)"));
        CallMeConnection.FireEvent(EventNameConstants.PROCESS_ABORTED, args);
    }
    private void ErrorOccurred_EventHandler(object? _, SharedErrorDataDto args)
    {
        Logger_EventHandler(this, Tuple.Create(LogType.Error, $"ErrorOccurred_EventHandler: {JsonConvert.SerializeObject(args)}"));
        CallMeConnection.FireEvent(EventNameConstants.ERROR_OCCURRED, new SharedErrorEventArgs(args));
    }
    #endregion
    #endregion
}*/