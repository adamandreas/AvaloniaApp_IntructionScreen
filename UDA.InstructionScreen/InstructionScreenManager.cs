
using System;
using System.IO;
using System.Windows;
using System.Reflection;
using System.Linq; 
using System.Threading.Tasks;
using System.Drawing.Imaging;
using Avalonia.Controls; 
using System.Runtime.Versioning;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Labs.Gif;
using Avalonia.Platform;
using Avalonia.Threading;
using AvaloniaApp_Play.Views;
using LibVLCSharp.Shared;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json; 
using UDA.Shared;
using UDA.UDACapabilities.Shared.Enums;
using UDA.UDACapabilities.Shared.Entities;
using UDA.InstructionScreen.Shared.Entities;
using UDA.Shared.Abstraction;
using UDA.InstructionScreen.Shared.Entities.UI_Elements;
using Path = System.IO.Path; 
using FontFamily = Avalonia.Media.FontFamily;
using Image = Avalonia.Controls.Image;
using Timer = System.Threading.Timer;
using System.Diagnostics.CodeAnalysis;
using AvaloniaApp_Play.ViewModels;
using UDA.InstructionScreen.Helper;

namespace UDA.InstructionScreen.Capability;

public class InstructionScreenManager(ISettings settings) : ViewModelBase
{
    //Shared
    public event EventHandler<DeviceInitializedDataDto>? DeviceInitialized;
    public event EventHandler? ProcessAborted;
    public event EventHandler? ProcessStarted;
    public event EventHandler<SharedErrorDataDto>? ErrorOccurred;
    
    //Events Just For Controller
    public event EventHandler<DeviceStatusEnum>? UpdateDeviceStatus;
    public event EventHandler<Tuple<LogType, string>>? Logger;
    
    private readonly ConfigSettings? _config = settings.Configuration
            .GetSection("CapabilitySettings")
            .GetSection("InstructionScreenConfig")
            .GetSection("CapabilitySharedConfig")
            .Get<ConfigSettings>();
    
    private LayoutConfig? _layoutConfig;
    public InstructionSectionViewModel HeaderSection { get; } = new();
    public InstructionSectionViewModel BodySection { get; } = new();
    public InstructionSectionViewModel FooterSection { get; } = new();
    private bool _rerouteToLanguageFolder;
    private string? _instructionImagesParentPath;
    private Language _defaultLanguage;
    private ImageFormat _displayImageFormat = ImageFormat.Png;
    private bool _enableDetailedLogging;
 
    private int _countdownSeconds;
    private Timer? _countDownTimer;
    private TextBlock? _countDownTextBlock;

    public async Task InitAsync()
    {
        
        try
        { 
            try
            {
                var directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (_config?.ActiveLayoutJsonFileName is null)
                    FireNewError(ErrorCode.NullConfig, $"The 'ActiveLayoutJsonFileName' in the appsettings is null.");
                else
                    _layoutConfig = JsonConvert.DeserializeObject<LayoutConfig>(await File.ReadAllTextAsync(Path.Combine(directoryName, _config.ActiveLayoutJsonFileName)));
            }
            catch (Exception ex)
            {
                FireNewError(ErrorCode.FileNotFound, $"Failed to read the layout config '{_config?.ActiveLayoutJsonFileName}'. Exception: {ex}");
            }

            if (_layoutConfig is null)
                return; 
            
            _displayImageFormat = _config.DisplayImageFormat switch
            {
                1 => ImageFormat.Png,
                2 => ImageFormat.Wmf,
                3 => ImageFormat.Tiff,
                4 => ImageFormat.Png,
                5 => ImageFormat.MemoryBmp,
                6 => ImageFormat.Jpeg,
                7 => ImageFormat.Icon,
                8 => ImageFormat.Exif,
                9 => ImageFormat.Emf,
                10 => ImageFormat.Bmp,
                11 => ImageFormat.Gif,
                _ => ImageFormat.Png
            };

            _rerouteToLanguageFolder = _config.RerouteToLanguageFolder;
            _instructionImagesParentPath = _config.InstructionImagesParentPath.GetFullPath();

            _defaultLanguage = _config.DefaultLanguage switch
            {
                (int)Language.English => Language.English,
                (int)Language.Arabic => Language.Arabic,
                _ => Language.ArAndEn
            };

            CheckForDuplicateNames(_layoutConfig.HeaderLayout, 
                                   _layoutConfig.BodyLayout,
                                   _layoutConfig.FooterLayout);

            ValidateIndexes(_layoutConfig.HeaderLayout);
            ValidateIndexes(_layoutConfig.BodyLayout);
            ValidateIndexes(_layoutConfig.FooterLayout);
        }
        catch (Exception e)
        {
            UpdateDeviceStatusMethod(DeviceStatusEnum.Undefined);
            FireNewError(ErrorCode.GENERAL_ERROR, $"Exception in Init(): {e}");
        }
        DeviceInitialized?.Invoke(this, new() { UsedDeviceNameEnum = DeviceNameEnum.InstructionScreen });
        UpdateDeviceStatusMethod(DeviceStatusEnum.Ready);
    }

    public void Start(StartupSettings startupSettings)
    {
    }

    private void UpdateInstructionScreen(StartupSettings startupSettings)
    { 
        
    }

    /// <param name="grid"></param>
    /// <param name="requiredElements">Tuple(string Name, bool TheImgIsGif)</param>
    /// The use of the bool is just for the "Image_Elements"
    private async Task CollapseAllGridUnwantedElements(Grid grid, List<string> requiredElements)
    {
         
    }

    public void Stop()
    {
        
    }
 
    private void CreateLayout(Layout layout, Grid grid)
    { 
        
    }
 
    private static Image CreateImageElement(Image_Element imgDetails, ImageFormat format)
    {
        return new Image();
    }

    //Use "WebBrowser" to support GIF
    private static GifImage CreateWebBrowserElement(GIF_Element gifDetails)
    {
        return new GifImage(); 
    }

    private TextBlock CreateTextBlockElement(TextBlock_Element textBlockDetails)
    {
        return new TextBlock();
    }

    //To display RTSP stream using VLC media player (VLC should be installed on your machine)
    private static Media CreateVlcControlElement(VlcControl_Element vlcControlDetails)
    {
        return new Media(null);
    }

    private Media CreateMediaElement(Media_Element mediaDetails)
    {
        return new Media(null);
    }

    private async Task UpdateLayout(Layout layout, Grid grid, Language? imageLanguage)
    {
        
    }


    private void UpdateImageElement(Image img, Image_Element imgDetails, Language? imageLanguage, Grid grid)
    { 
        
    }
    private void UpdateWebBrowserElement(GifImage webBrowser, GIF_Element gifDetails, Language? imageLanguage, Grid grid)
    {
        
    }
    private void UpdateTextBlockElement(TextBlock textBlock, TextBlock_Element textBlockDetails)
    {
        
    }
    private async Task UpdateVlcControlElement(Media vlcControl, VlcControl_Element vlcControlDetails)
    {
       
    }
    private void UpdateMediaElement(Media mediaElement, Media_Element mediaDetails)
    {
        
    }

    private void StartCountDownTimer(TextBlock textBlock, int countDownTimeSec)
    {
        
    }
    private void CheckCountDown(TextBlock textBlock, TextBlock_Element textBlockDetails)
    {
        
    }
    private void UpdateCountdownTimer(object? _)
    {
        
    } 

    private async Task StopRtspStreamAsync(Media vlcControl)
    { 
        
    }

    //TODO: this should find the element with any type to get the height/width of. + Support the same feature in the VLC control element
    private void UpdateHeightAndWidth(Image img, Grid grid, string? matchHeightWithOtherImageName, string? matchWidthWithOtherImageName, string erroNum1, string? erroNum2)
    {
        
    }

    public static bool IsExtraMonitorConnected(Window window)
    {
        return true;
    }

    private void ValidateIndexes(Layout? layout)
    {
        
    }

    private void CheckForDuplicateNames(Layout? layout1, Layout? layout2, Layout? layout3)
    {
        
    }


    private void UpdateDeviceStatusMethod(DeviceStatusEnum statusId)
    {
        UpdateDeviceStatus?.Invoke(this, statusId);
    }

    private void FireNewError(ErrorCode errorCode, string errorMessage)
    {
        ErrorOccurred?.Invoke(this, new SharedErrorDataDto { ErrorCode = errorCode, ErrorMessage = $"Manager: {errorMessage}" });
    }

    private void Logger_Method(LogType type, string message)
    {
        Logger?.Invoke(this, Tuple.Create(type, $"{message}"));
    }
}