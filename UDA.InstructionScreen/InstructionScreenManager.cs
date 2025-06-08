/*
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
using Avalonia.Platform;
using Avalonia.Threading;
using AvaloniaApp_Play.Views;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json; 
using UDA.Shared;
using UDA.UDACapabilities.Shared.Enums;
using UDA.UDACapabilities.Shared.Entities;
using UDA.InstructionScreen.Shared.Entities;
using UDA.Shared.Abstraction;
using UDA.InstructionScreen.Shared.Entities.UI_Elements;
using UDA.InstructionScreen.Helper;
using Path = System.IO.Path; 
using FontFamily = Avalonia.Media.FontFamily;
using Image = Avalonia.Controls.Image;
using Timer = System.Threading.Timer;

namespace UDA.InstructionScreen.Capability;

public class InstructionScreenManager(ISettings settings)
{
    //To add a new element in the instruction screen search for "For new elements add the new implementation here." and add the elements following the same be way of other elements' implementation

    #region Manager Events
    //Shared
    public event EventHandler<DeviceInitializedDataDto>? DeviceInitialized;
    public event EventHandler? ProcessAborted;
    public event EventHandler? ProcessStarted;
    public event EventHandler<SharedErrorDataDto>? ErrorOccurred;
    //Events Just For Controller
    public event EventHandler<DeviceStatusEnum>? UpdateDeviceStatus;
    public event EventHandler<Tuple<LogType, string>>? Logger;
    #endregion

    #region Fields
    private readonly ConfigSettings _config = settings.Configuration
            .GetSection("CapabilitySettings")
            .GetSection("InstructionScreenConfig")
            .GetSection("CapabilitySharedConfig")
            .Get<ConfigSettings>();
    private LayoutConfig? _layoutConfig;

    public MainWindow? MainWindow;
    private bool _rerouteToLanguageFolder;
    private string? _instructionImagesParentPath;
    private Language _defaultLanguage;
    private ImageFormat _displayImageFormat = ImageFormat.Png;
    private bool _enableDetailedLogging;

    #region Count down timer (supports only one timer at a time)
    private int _countdownSeconds;
    private Timer? _countDownTimer;
    private TextBlock? _countDownTextBlock; 

    #endregion
    #endregion

    public async Task InitAsync()
    {
        try
        {
            #region Read the LayoutConfig
            try
            {
                var directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (directoryName is null)
                {
                    FireNewError(ErrorCode.NullConfig, $"Failed to Init() the instruction screen. The 'directoryName' is null.");
                    return;
                }
                if (_config.ActiveLayoutJsonFileName is null)
                    FireNewError(ErrorCode.NullConfig, $"The 'ActiveLayoutJsonFileName' in the appsettings is null.");
                else
                    _layoutConfig = JsonConvert.DeserializeObject<LayoutConfig>(await File.ReadAllTextAsync(Path.Combine(directoryName, _config.ActiveLayoutJsonFileName)));
            }
            catch (Exception ex)
            {
                FireNewError(ErrorCode.FileNotFound, $"Failed to read the layout config '{_config.ActiveLayoutJsonFileName}'. Exception: {ex}");
            }

            if (_layoutConfig is null)
                return;
            #endregion

            _enableDetailedLogging = _config.EnableLogging;
            if (_enableDetailedLogging)
                Logger_Method(LogType.Debug, $"InitAsync(instructionScreenConfig: {JsonConvert.SerializeObject(_config)})");
            
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

            CheckForDuplicateNames(_layoutConfig.HeaderLayout, _layoutConfig.BodyLayout, _layoutConfig.FooterLayout);

            ValidateIndexes(_layoutConfig.HeaderLayout);
            ValidateIndexes(_layoutConfig.BodyLayout);
            ValidateIndexes(_layoutConfig.FooterLayout);

            if (_enableDetailedLogging)
                Logger_Method(LogType.Debug, $"MainWindow is null?: {MainWindow is null}");

            if (MainWindow is null)
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    try
                    {
                        MainWindow = new();
                    }
                    catch (Exception e)
                    {
                        FireNewError(ErrorCode.GENERAL_ERROR, $"Exception while creating a new MainWindow: {e}");
                    }
                });
            if (MainWindow is not null)
            {
                //Show the instruction screen if not 
                if (!MainWindow.IsVisible)
                {
                    if (_enableDetailedLogging)
                        Logger_Method(LogType.Debug, $"MainWindow.IsVisible?: {MainWindow.IsVisible}");

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        try
                        {
                            MainWindow.Show();
                        }
                        catch (Exception e)
                        {
                            FireNewError(ErrorCode.GENERAL_ERROR, $"Exception while showing the MainWindow: {e}");
                        }
                    });
                }

                //TODO: Validate the "_layoutConfig" if there are any two elements with a similar names

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    try
                    {
                        #region Create the Header
                        if (_layoutConfig.HeaderLayout is not null && _layoutConfig.HeaderLayout.ShowThisLayout)
                        {
                            if (_layoutConfig.HeaderRowGridDefinition is not null && _layoutConfig.HeaderRowGridDefinition.Dimension is not null)
                            {
                                var heightType = _layoutConfig.HeaderRowGridDefinition.DimensionType switch
                                {
                                    1 => GridUnitType.Star,
                                    2 => GridUnitType.Auto,
                                    3 => GridUnitType.Pixel,
                                    _ => GridUnitType.Pixel
                                };
                                MainWindow.Header_RowDefinition.Height = new GridLength((double)_layoutConfig.HeaderRowGridDefinition.Dimension, heightType);
                            }
                            for (var i = 0; i < _layoutConfig.HeaderLayout.ColumnDefinitions_Count; i++)
                                if (MainWindow.Grid_Header.ColumnDefinitions.Count < _layoutConfig.HeaderLayout.ColumnDefinitions_Count)
                                    MainWindow.Grid_Header.ColumnDefinitions.Add(new ColumnDefinition());
                            for (var i = 0; i < _layoutConfig.HeaderLayout.RowDefinitions_Count; i++)
                                if (MainWindow.Grid_Header.RowDefinitions.Count < _layoutConfig.HeaderLayout.RowDefinitions_Count)
                                    MainWindow.Grid_Header.RowDefinitions.Add(new RowDefinition());

                            CreateLayout(_layoutConfig.HeaderLayout, MainWindow.Grid_Header);
                        }
                        else
                            MainWindow.Header_RowDefinition.Height = new GridLength(0);
                        #endregion

                         #region Create the Body
                        if (_layoutConfig.BodyLayout is not null && _layoutConfig.BodyLayout.ShowThisLayout)
                        {
                            if (_layoutConfig.BodyRowGridDefinition is not null && _layoutConfig.BodyRowGridDefinition.Dimension is not null)
                            {
                                var heightType = _layoutConfig.BodyRowGridDefinition.DimensionType switch
                                {
                                    1 => GridUnitType.Star,
                                    2 => GridUnitType.Auto,
                                    3 => GridUnitType.Pixel,
                                    _ => GridUnitType.Pixel
                                };
                                MainWindow.Body_RowDefinition.Height = new GridLength((double)_layoutConfig.BodyRowGridDefinition.Dimension, heightType);
                            }
                            for (var i = 0; i < _layoutConfig.BodyLayout.ColumnDefinitions_Count; i++)
                                if (MainWindow.Grid_Body.ColumnDefinitions.Count < _layoutConfig.BodyLayout.ColumnDefinitions_Count)
                                    MainWindow.Grid_Body.ColumnDefinitions.Add(new ColumnDefinition());
                            for (var i = 0; i < _layoutConfig.BodyLayout.RowDefinitions_Count; i++)
                                if (MainWindow.Grid_Body.RowDefinitions.Count < _layoutConfig.BodyLayout.RowDefinitions_Count)
                                    MainWindow.Grid_Body.RowDefinitions.Add(new RowDefinition());

                            CreateLayout(_layoutConfig.BodyLayout, MainWindow.Grid_Body);
                        }
                        else
                            MainWindow.Body_RowDefinition.Height = new GridLength(0);
                        #endregion

                        #region Create the Footer
                        if (_layoutConfig.FooterLayout is not null && _layoutConfig.FooterLayout.ShowThisLayout)
                        {
                            if (_layoutConfig.FooterRowGridDefinition is not null && _layoutConfig.FooterRowGridDefinition.Dimension is not null)
                            {
                                var heightType = _layoutConfig.FooterRowGridDefinition.DimensionType switch
                                {
                                    1 => GridUnitType.Star,
                                    2 => GridUnitType.Auto,
                                    3 => GridUnitType.Pixel,
                                    _ => GridUnitType.Pixel
                                };
                                MainWindow.Footer_RowDefinition.Height = new GridLength((double)_layoutConfig.FooterRowGridDefinition.Dimension, heightType);
                            }
                            for (var i = 0; i < _layoutConfig.FooterLayout.ColumnDefinitions_Count; i++)
                                if (MainWindow.Grid_Footer.ColumnDefinitions.Count < _layoutConfig.FooterLayout.ColumnDefinitions_Count)
                                    MainWindow.Grid_Footer.ColumnDefinitions.Add(new ColumnDefinition());
                            for (var i = 0; i < _layoutConfig.FooterLayout.RowDefinitions_Count; i++)
                                if (MainWindow.Grid_Footer.RowDefinitions.Count < _layoutConfig.FooterLayout.RowDefinitions_Count)
                                    MainWindow.Grid_Footer.RowDefinitions.Add(new RowDefinition());

                            CreateLayout(_layoutConfig.FooterLayout, MainWindow.Grid_Footer);
                        }
                        else
                            MainWindow.Footer_RowDefinition.Height = new GridLength(0);
                        #endregion
                    }
                    catch (Exception e)
                    {
                        FireNewError(ErrorCode.GENERAL_ERROR, $"Exception while creating the layouts: {e}");
                    }
                });
            }

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
        try
        {
            if (_enableDetailedLogging)
                Logger_Method(LogType.Debug, $"InstructionScreen Manager: Start(...)");
 
            UpdateInstructionScreen(startupSettings);

            ProcessStarted?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception e)
        {
            FireNewError(ErrorCode.GENERAL_ERROR, $"Exception while Starting the instruction screen. Exception: {e}");
        }
    }

    private void UpdateInstructionScreen(StartupSettings startupSettings)
    {
        try
        {
            if (_enableDetailedLogging)
                Logger_Method(LogType.Debug, $"InstructionScreen Manager: UpdateInstructionScreen(...). MainWindow is null: {MainWindow is null}, MainWindow.IsVisible: {MainWindow?.IsVisible}");

            if (MainWindow is not null && MainWindow.IsVisible)
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    try
                    {
                        #region Combine the path if "InstructionImagesParentPath" is not null in "appsettings.json"
                        if (_instructionImagesParentPath is not null)
                        {
                            {
                                if (startupSettings.HeaderLayout?.Image_Elements is not null)
                                    foreach (var img in startupSettings.HeaderLayout.Image_Elements)
                                        if (img.ImgUrlOrBase64 is not null && img.TheImgIsUrl)
                                            img.ImgUrlOrBase64 = img.ImgUrlOrBase64.ManageThePathUrl(_instructionImagesParentPath);

                                if (startupSettings.HeaderLayout?.GIF_Elements is not null)
                                    foreach (var gif in startupSettings.HeaderLayout.GIF_Elements)
                                        if (gif.GifUrl is not null)
                                            gif.GifUrl = gif.GifUrl.ManageThePathUrl(_instructionImagesParentPath);
                            }

                            {
                                if (startupSettings.BodyLayout?.Image_Elements is not null)
                                    foreach (var img in startupSettings.BodyLayout.Image_Elements)
                                        if (img.ImgUrlOrBase64 is not null && img.TheImgIsUrl)
                                            img.ImgUrlOrBase64 = img.ImgUrlOrBase64.ManageThePathUrl(_instructionImagesParentPath);

                                if (startupSettings.BodyLayout?.GIF_Elements is not null)
                                    foreach (var gif in startupSettings.BodyLayout.GIF_Elements)
                                        if (gif.GifUrl is not null)
                                            gif.GifUrl = gif.GifUrl.ManageThePathUrl(_instructionImagesParentPath);
                            }

                            {
                                if (startupSettings.FooterLayout?.Image_Elements is not null)
                                    foreach (var img in startupSettings.FooterLayout.Image_Elements)
                                        if (img.ImgUrlOrBase64 is not null && img.TheImgIsUrl)
                                            img.ImgUrlOrBase64 = img.ImgUrlOrBase64.ManageThePathUrl(_instructionImagesParentPath);

                                if (startupSettings.FooterLayout?.GIF_Elements is not null)
                                    foreach (var gif in startupSettings.FooterLayout.GIF_Elements)
                                        if (gif.GifUrl is not null)
                                            gif.GifUrl = gif.GifUrl.ManageThePathUrl(_instructionImagesParentPath);
                            }
                        }
                        #endregion

                        #region Collapse All Grid Unwanted Elements
                        {
                            List<string> requiredElements = [];
                            if (startupSettings.HeaderLayout is not null)
                            {
                                MainWindow.Grid_Header.IsVisible = true;

                                //For new elements add the new implementation here.
                                if (startupSettings.HeaderLayout.Image_Elements is not null)
                                    requiredElements.AddRange(startupSettings.HeaderLayout.Image_Elements.Select(item => item.Name).ToList());

                                if (startupSettings.HeaderLayout.GIF_Elements is not null)
                                    requiredElements.AddRange(startupSettings.HeaderLayout.GIF_Elements.Select(item => item.Name).ToList());

                                if (startupSettings.HeaderLayout.TextBlock_Elements is not null)
                                    requiredElements.AddRange(startupSettings.HeaderLayout.TextBlock_Elements.Select(item => item.Name).ToList());

                                if (startupSettings.HeaderLayout.VlcControl_Elements is not null)
                                    requiredElements.AddRange(startupSettings.HeaderLayout.VlcControl_Elements.Select(item => item.Name).ToList());

                                if (startupSettings.HeaderLayout.Media_Elements is not null)
                                    requiredElements.AddRange(startupSettings.HeaderLayout.Media_Elements.Select(item => item.Name).ToList());
                            }
                            else
                                MainWindow.Grid_Header.IsVisible = false;

                            await CollapseAllGridUnwantedElements(MainWindow.Grid_Header, requiredElements);
                        }
                        {
                            List<string> requiredElements = [];
                            if (startupSettings.BodyLayout is not null)
                            {
                                MainWindow.Grid_Body.IsVisible = true;

                                //For new elements add the new implementation here.
                                if (startupSettings.BodyLayout.Image_Elements is not null)
                                    requiredElements.AddRange(startupSettings.BodyLayout.Image_Elements.Select(item => item.Name).ToList());

                                if (startupSettings.BodyLayout.GIF_Elements is not null)
                                    requiredElements.AddRange(startupSettings.BodyLayout.GIF_Elements.Select(item => item.Name).ToList());

                                if (startupSettings.BodyLayout.TextBlock_Elements is not null)
                                    requiredElements.AddRange(startupSettings.BodyLayout.TextBlock_Elements.Select(item => item.Name).ToList());

                                if (startupSettings.BodyLayout.VlcControl_Elements is not null)
                                    requiredElements.AddRange(startupSettings.BodyLayout.VlcControl_Elements.Select(item => item.Name).ToList());

                                if (startupSettings.BodyLayout.Media_Elements is not null)
                                    requiredElements.AddRange(startupSettings.BodyLayout.Media_Elements.Select(item => item.Name).ToList());
                            }
                            else
                                MainWindow.Grid_Body.IsVisible = false;

                            await CollapseAllGridUnwantedElements(MainWindow.Grid_Body, requiredElements);
                        }
                        {
                            List<string> requiredElements = [];
                            if (startupSettings.FooterLayout is not null)
                            {
                                MainWindow.Grid_Footer.IsVisible = true;

                                //For new elements add the new implementation here.
                                if (startupSettings.FooterLayout.Image_Elements is not null)
                                    requiredElements.AddRange(startupSettings.FooterLayout.Image_Elements.Select(item => item.Name).ToList());

                                if (startupSettings.FooterLayout.GIF_Elements is not null)
                                    requiredElements.AddRange(startupSettings.FooterLayout.GIF_Elements.Select(item => item.Name).ToList());

                                if (startupSettings.FooterLayout.TextBlock_Elements is not null)
                                    requiredElements.AddRange(startupSettings.FooterLayout.TextBlock_Elements.Select(item => item.Name).ToList());

                                if (startupSettings.FooterLayout.VlcControl_Elements is not null)
                                    requiredElements.AddRange(startupSettings.FooterLayout.VlcControl_Elements.Select(item => item.Name).ToList());

                                if (startupSettings.FooterLayout.Media_Elements is not null)
                                    requiredElements.AddRange(startupSettings.FooterLayout.Media_Elements.Select(item => item.Name).ToList());
                            }
                            else
                                MainWindow.Grid_Footer.IsVisible = false;

                            await CollapseAllGridUnwantedElements(MainWindow.Grid_Footer, requiredElements);
                        }
                        #endregion

                        if (startupSettings.HeaderLayout is not null)
                            await UpdateLayout(startupSettings.HeaderLayout, MainWindow.Grid_Header, startupSettings.Language);
                        if (startupSettings.BodyLayout is not null)
                            await UpdateLayout(startupSettings.BodyLayout, MainWindow.Grid_Body, startupSettings.Language);
                        if (startupSettings.FooterLayout is not null)
                            await UpdateLayout(startupSettings.FooterLayout, MainWindow.Grid_Footer, startupSettings.Language);
                    }
                    catch (Exception e)
                    {
                        FireNewError(ErrorCode.GENERAL_ERROR, $"Exception in updating the layouts: {e}");
                    }
                });
            }
        }
        catch (Exception e)
        {
            FireNewError(ErrorCode.GENERAL_ERROR, $"Exception in UpdateInstructionScreen(): {e}");
        }
    }

    /// <param name="grid"></param>
    /// <param name="requiredElements">Tuple(string Name, bool TheImgIsGif)</param>
    /// The use of the bool is just for the "Image_Elements"
    private async Task CollapseAllGridUnwantedElements(Grid grid, List<string> requiredElements)
    {
        foreach (var element in grid.Children)
        {
            #region For new elements add the new implementation here.
            { //Image: This bracket to limit the scope of the content fields
                if (element is Image image &&
                    image.IsVisible != false && requiredElements.All(elementName => elementName != image.Name))
                {
                    var collapseElement = true;

                    if (image.Tag is Dictionary<string, object?> storedDictionary &&
                        (bool?)storedDictionary["NeverCollapse"] == true) //Get the stored data from the image "Tag" property
                    {
                        collapseElement = false;
                        storedDictionary["DisplayedUrl"] = null;
                    }

                    if (collapseElement)
                        image.IsVisible = false;
                }
            }

            { //WebBrowser: This bracket to limit the scope of the content fields
                if (element is WebBrowser webBrowser && webBrowser.IsVisible != false && requiredElements.All(elementName => elementName != webBrowser.Name))
                {
                    var collapseElement = true;

                    if (webBrowser.Tag is Dictionary<string, object?> storedDictionary &&
                        (bool?)storedDictionary["NeverCollapse"] == true) //Get the stored data from the WebBrowser "Tag" property
                    {
                        collapseElement = false;
                        storedDictionary["DisplayedUrl"] = null;
                    }

                    if (collapseElement)
                        webBrowser.Visibility = Visibility.Collapsed;
                }
            }

            { //TextBlock: This bracket to limit the scope of the content fields
                if (element is TextBlock textBlock && textBlock.IsVisible != false && requiredElements.All(elementName => elementName != textBlock.Name))
                {
                    var collapseElement = !(textBlock.Tag is Dictionary<string, object?> storedDictionary &&
                                            (bool?)storedDictionary["NeverCollapse"] == true);

                    if (collapseElement)
                        textBlock.IsVisible = false;
                }
            }

            { //VlcControl: This bracket to limit the scope of the content fields
                if (element is VlcControl vlcControl && vlcControl.IsVisible != false && requiredElements.All(elementName => elementName != vlcControl.Name))
                {
                    var collapseElement = true;

                    if (vlcControl.Tag is Dictionary<string, object?> storedDictionary &&
                        (bool?)storedDictionary["NeverCollapse"] == true) //Get the stored data from the VlcControl "Tag" property
                    {
                        collapseElement = false;

                        try
                        {
                            Logger_Method(LogType.Debug, $"Disposing the VLC Control '{vlcControl.Name}'");
                            if ((string?)storedDictionary["DisplayedUrl"] is not null)
                                await StopRtspStreamAsync(vlcControl);
                        }
                        catch (Exception ex)
                        {
                            FireNewError(ErrorCode.GENERAL_ERROR, $"Exception while disposing the VLC components. Exception: {ex}");
                        }
                    }

                    if (collapseElement)
                        vlcControl.Visibility = Visibility.Collapsed;
                }
            }

            { //Media: This bracket to limit the scope of the content fields
                if (element is MediaElement mediaElement && mediaElement.Visibility != Visibility.Collapsed && requiredElements.All(elementName => elementName != mediaElement.Name))
                {
                    var collapseElement = true;

                    if (mediaElement.Tag is Dictionary<string, object?> storedDictionary &&
                        (bool?)storedDictionary["NeverCollapse"] == true) //Get the stored data from the image "Tag" property
                    {
                        collapseElement = false;

                        try
                        {
                            Logger_Method(LogType.Debug, $"Pausing the mp4 '{mediaElement.Name}'");

                            var videoPlayerObject = (VideoPlayer?)storedDictionary["VideoPlayerObject"];
                            if (videoPlayerObject is not null)
                            {
                                var releaseMemoryOnPause = (bool?)storedDictionary["ReleaseMemoryOnPause"];
                                var stopNotPauseOnCollapse = (bool?)storedDictionary["StopNotPauseOnCollapse"];
                                if (releaseMemoryOnPause is not null)
                                {
                                    if (stopNotPauseOnCollapse == true)
                                        videoPlayerObject.Stop();
                                    else
                                    {
                                        if (releaseMemoryOnPause == true)
                                            videoPlayerObject.PauseAndReleaseMemory();
                                        else
                                            videoPlayerObject.Pause();
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            FireNewError(ErrorCode.GENERAL_ERROR, $"Exception while pausing the Media Element. Exception: {ex}");
                        }
                    }

                    if (collapseElement)
                        mediaElement.IsVisible = false;
                }
            }
            #endregion
        }
    }

    public void Stop()
    {
        try
        {
            if (_enableDetailedLogging)
                Logger_Method(LogType.Debug, $"Stop(). MainWindow is null: {MainWindow is null}");

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                try
                {
                    //TODO: dispose the elements here before clearing all of them

                    MainWindow.Grid_Header.Children.Clear();
                    MainWindow.Grid_Body.Children.Clear();
                    MainWindow.Grid_Footer.Children.Clear();

                    MainWindow.Grid_Header.ColumnDefinitions.Clear();
                    MainWindow.Grid_Body.ColumnDefinitions.Clear();
                    MainWindow.Grid_Footer.ColumnDefinitions.Clear();

                    // MainWindow.CanStop = true;
                    MainWindow.Close();
                    MainWindow = null;

                    ProcessAborted?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception e)
                {
                    FireNewError(ErrorCode.GENERAL_ERROR, $"Exception while resetting the UI: {e}");
                }
            });
        }
        catch (Exception e)
        {
            FireNewError(ErrorCode.GENERAL_ERROR, $"Exception in Stop(): {e}");
        }
    }

    #region Create UI Element
    private void CreateLayout(Layout layout, Grid grid)
    {
        layout.MapHeight(grid);
        layout.MapWidth(grid);
        layout.MapRenderingMode(grid);
        layout.MapMargins(grid);
        layout.MapHorizontalAlignment(grid);
        layout.MapVerticalAlignment(grid);
        layout.MapBackground(grid); 

        #region For new elements add the new implementation here.
        #region Create Image Elements in the Grid
        if (layout.Image_Elements is not null && layout.Image_Elements.Count > 0 && layout.ShowThisLayout)
        {
            foreach (var imageElement in layout.Image_Elements)
            { 
                Image image = CreateImageElement(imageElement,_displayImageFormat);

                grid.MapRowGridDefinition(imageElement, image);
                grid.MapColumnGridDefinition(imageElement, image);
                grid.Children.Add(image);
            }
        }
        #endregion

        #region Create WebBrowser Elements in the Grid
        if (layout.GIF_Elements is not null && layout.GIF_Elements.Count > 0 && layout.ShowThisLayout)
        {
            foreach (var gifElement in layout.GIF_Elements)
            { 
                var webBrowser =  CreateWebBrowserElement(gifElement);

                grid.MapRowGridDefinition(gifElement, webBrowser);
                grid.MapColumnGridDefinition(gifElement, webBrowser);
                grid.Children.Add(webBrowser);
            }
        }
        #endregion

        #region Create TextBlock Elements in the Grid
        if (layout.TextBlock_Elements is not null && layout.TextBlock_Elements.Count > 0 && layout.ShowThisLayout)
        {
            foreach (var textBlockElement in layout.TextBlock_Elements)
            { 
                var textBlock =  CreateTextBlockElement(textBlockElement);

                grid.MapRowGridDefinition(textBlockElement, textBlock, false);
                grid.MapColumnGridDefinition(textBlockElement, textBlock, false);
                grid.Children.Add(textBlock);
            }
        }
        #endregion

        #region Create VlcControl ELements in the Grid
        if (layout.VlcControl_Elements is not null && layout.VlcControl_Elements.Count > 0 && layout.ShowThisLayout)
        {
            foreach (var vlcControlElement in layout.VlcControl_Elements)
            { 
                VlcControl vlcControl = CreateVlcControlElement(vlcControlElement);

                grid.MapRowGridDefinition(vlcControlElement, vlcControl, false);
                grid.MapColumnGridDefinition(vlcControlElement, vlcControl, false);
                grid.Children.Add(vlcControl); 
            }
        }
        #endregion

        #region Create Media ELements in the Grid
        if (layout.Media_Elements is not null && layout.Media_Elements.Count > 0 && layout.ShowThisLayout)
        {
            foreach (var element in layout.Media_Elements)
            { 
                MediaElement mediaElement = CreateMediaElement(element);

                grid.MapRowGridDefinition(element, mediaElement, false);
                grid.MapColumnGridDefinition(element, mediaElement,false);
                grid.Children.Add(mediaElement); 
            }
        }
        #endregion
        #endregion
    }

    #region For new elements add the new implementation here.
    private static Image CreateImageElement(Image_Element imgDetails, ImageFormat format)
    {
        Image newImg = new();
        
        imgDetails.MapName(newImg);
        imgDetails.MapImageTag(newImg);
        imgDetails.MapImageSource(format, newImg);
        imgDetails.MapZIndex(newImg);
        imgDetails.MapMargin(newImg);
        imgDetails.MapHeight(newImg);
        imgDetails.MapWidth(newImg);
        imgDetails.MapHorizontalAlignment(newImg);
        imgDetails.MapVerticalAlignment(newImg);
        imgDetails.MapStretch(newImg);

        return newImg;
    }
    
    //Use "WebBrowser" to support GIF
    private static WebBrowser CreateWebBrowserElement(GIF_Element gifDetails)
    {
        WebBrowser newWebBrowser = new();
        gifDetails.MapName(newWebBrowser);
        gifDetails.MapTag(newWebBrowser);
        gifDetails.MapHeight(newWebBrowser);
        gifDetails.MapWidth(newWebBrowser);

        if (!string.IsNullOrEmpty(gifDetails.GifUrl))
        {
            var html = gifDetails.GifUrl.GenerateHtmlCodeForGif(newWebBrowser.ActualWidth, newWebBrowser.ActualHeight);

            LoadCompletedEventHandler? onBlankLoaded = null;
            onBlankLoaded = (_, e) =>
            {
                if (e.Uri != null && e.Uri.ToString() == "about:blank")
                {
                    newWebBrowser.LoadCompleted -= onBlankLoaded;
                    newWebBrowser.NavigateToString(html);
                }
            };
            newWebBrowser.LoadCompleted += onBlankLoaded;
            newWebBrowser.Navigate("about:blank");

            if (newWebBrowser.Tag is Dictionary<string, object?> storedDictionary) //Get the stored data from the image "Tag" property
                storedDictionary["DisplayedUrl"] = gifDetails.GifUrl;
        }

        gifDetails.MapMargins(newWebBrowser);
        gifDetails.MapHorizontalAlignment(newWebBrowser);
        gifDetails.MapVerticalAlignment(newWebBrowser);

        return newWebBrowser;
    }
    
    private TextBlock CreateTextBlockElement(TextBlock_Element textBlockDetails)
    {
        TextBlock newTextBlock = new();

        newTextBlock.MapName(textBlockDetails);
        newTextBlock.MapText(textBlockDetails);
        newTextBlock.MapTag(textBlockDetails);
        newTextBlock.MapZIndex(textBlockDetails);
        newTextBlock.MapFontSize(textBlockDetails);
        newTextBlock.MapForeground(textBlockDetails);
        newTextBlock.MapBackground(textBlockDetails);
        newTextBlock.MapHeight(textBlockDetails);
        newTextBlock.MapWidth(textBlockDetails);
        newTextBlock.MapFontWeight(textBlockDetails);
        
        try
        {
            if (textBlockDetails.FontFamily is not null)
                newTextBlock.FontFamily = textBlockDetails.FontFamily.GetFontFamily();
        }
        catch(Exception ex)
        {
            FireNewError(ErrorCode.GENERAL_ERROR, $"Error loading font '{textBlockDetails.FontFamily}': {ex.Message}");
            newTextBlock.FontFamily = new FontFamily("Arial");
        }

        newTextBlock.MapTextAlignment(textBlockDetails);
        newTextBlock.MapHorizontalAlignment(textBlockDetails);
        newTextBlock.MapVerticalAlignment(textBlockDetails);
        newTextBlock.MapWrapping(textBlockDetails);
        newTextBlock.MapMargins(textBlockDetails);
        newTextBlock.MapPadding(textBlockDetails);

        return newTextBlock;
    }
    
    //To display RTSP stream using VLC media player (VLC should be installed on your machine)
    private static VlcControl CreateVlcControlElement(VlcControl_Element vlcControlDetails)
    {
        VlcControl newVlcControl = new();
        
        newVlcControl.MapName(vlcControlDetails);
        newVlcControl.MapTag(vlcControlDetails);
        newVlcControl.MapZIndex(vlcControlDetails);
        newVlcControl.MapMargins(vlcControlDetails);
        newVlcControl.MapHeight(vlcControlDetails);
        newVlcControl.MapWidth(vlcControlDetails);
        newVlcControl.MapHorizontalAlignment(vlcControlDetails);
        newVlcControl.MapVerticalAlignment(vlcControlDetails);

        return newVlcControl;
    }
    
    private MediaElement CreateMediaElement(Media_Element mediaDetails)
    {
        MediaElement newMediaElement = new();
        mediaDetails.MapName(newMediaElement);
        
        VideoPlayer videoPlayer = new(newMediaElement, Logger);
        videoPlayer.ErrorOccurred += (errorMessage) => { FireNewError(ErrorCode.GENERAL_ERROR, $"Error in the VideoPlayer of the element '{mediaDetails.Name}'. Exception: {errorMessage}"); };
        
        mediaDetails.MapTag(videoPlayer, newMediaElement);
        newMediaElement.MapZIndex(mediaDetails);
        newMediaElement.LoadedBehavior = MediaState.Manual;
        newMediaElement.UnloadedBehavior = MediaState.Manual;
        newMediaElement.MapMargin(mediaDetails);
        newMediaElement.MapHeight(mediaDetails);
        newMediaElement.MapWidth(mediaDetails);
        newMediaElement.MapHorizontalAlignment(mediaDetails);
        newMediaElement.MapVerticalAlignment(mediaDetails);
        
        return newMediaElement;
    }
    #endregion
    #endregion

    #region Update UI Element

    /// <summary>
    /// On each start, this should be called
    /// </summary>
    /// <param name="layout"></param>
    /// <param name="grid">The parent grid that contains the elements.</param>
    /// <param name="imageLanguage"></param>
    private async Task UpdateLayout(Layout layout, Grid grid, Language? imageLanguage)
    { 
        if (layout.FixElementProperties)
        {
            layout.MapWidth(grid);
            layout.MapHeight(grid);
            layout.MapMargins(grid);
            layout.MapHorizontalAlignment(grid);
            layout.MapVerticalAlignment(grid);
            layout.MapBackground(grid);
        } 

        #region For new elements add the new implementation here.
        #region Update Image Elements in the Grid
        if (layout.Image_Elements is not null && layout.Image_Elements.Count > 0 && layout.ShowThisLayout)
        {
            foreach (var imageElement in layout.Image_Elements)
            { 
                Image? image = grid.Children.OfType<Image>().FirstOrDefault(q => q.Name == imageElement.Name);

                if (image is not null)
                    UpdateImageElement(image, imageElement, imageLanguage, grid);
                else
                    FireNewError(ErrorCode.GENERAL_ERROR, $"#1 Didn't find an element in the Grid '{grid.Name}' with the name '{imageElement.Name}' and is to display Image, please make sure to have an element in the '***_Layout.json' with such name.");
            }
        }
        #endregion

        #region Update WebBrowser Elements in the Grid
        if (layout.GIF_Elements is not null && layout.GIF_Elements.Count > 0 && layout.ShowThisLayout)
        {
            foreach (var gifElement in layout.GIF_Elements)
            {
                WebBrowser? webBrowser = grid.Children.OfType<WebBrowser>().FirstOrDefault(q => q.Name == gifElement.Name);

                if (webBrowser is not null)
                    UpdateWebBrowserElement(webBrowser, gifElement, imageLanguage, grid);
                else
                    FireNewError(ErrorCode.GENERAL_ERROR, $"#2 Didn't find an element in the Grid '{grid.Name}' with the name '{gifElement.Name}' and is to display GIF, please make sure to have an element in the '***_Layout.json' with such name.");
            }
        }
        #endregion

        #region Update TextBlock Elements in the Grid
        if (layout.TextBlock_Elements is not null && layout.TextBlock_Elements.Count > 0 && layout.ShowThisLayout)
        {
            foreach (var textBlockElement in layout.TextBlock_Elements)
            {
                var textBlock = grid.Children.OfType<TextBlock>().FirstOrDefault(q => q.Name == textBlockElement.Name);

                if (textBlock is not null)
                    UpdateTextBlockElement(textBlock, textBlockElement);
                else
                    FireNewError(ErrorCode.GENERAL_ERROR, $"#3 Didn't find an element in the Grid '{grid.Name}' with the name '{textBlockElement.Name}', please make sure to have an element in the '***_Layout.json' with such name.");
            }
        }
        #endregion

        #region Update VLC Control Element
        if (layout.VlcControl_Elements is not null && layout.VlcControl_Elements.Count > 0 && layout.ShowThisLayout)
        {
            foreach (var vlcControlElement in layout.VlcControl_Elements)
            {
                VlcControl? vlcControl = grid.Children.OfType<VlcControl>().FirstOrDefault(q => q.Name == vlcControlElement.Name);

                if (vlcControl is not null)
                    await UpdateVlcControlElement(vlcControl, vlcControlElement);
                else
                    FireNewError(ErrorCode.GENERAL_ERROR, $"#4 Didn't find an element in the Grid '{grid.Name}' with the name '{vlcControlElement.Name}', please make sure to have an element in the '***_Layout.json' with such name.");
            }
        }
        #endregion

        #region Update Media Element
        if (layout.Media_Elements is not null && layout.Media_Elements.Count > 0 && layout.ShowThisLayout)
        {
            foreach (var mediaElementDetails in layout.Media_Elements)
            {
                MediaElement? mediaElement = grid.Children.OfType<MediaElement>().FirstOrDefault(q => q.Name == mediaElementDetails.Name);

                if (mediaElement is not null)
                    UpdateMediaElement(mediaElement, mediaElementDetails);
                else
                    FireNewError(ErrorCode.GENERAL_ERROR, $"#5 Didn't find an element in the Grid '{grid.Name}' with the name '{mediaElementDetails.Name}', please make sure to have an element in the '***_Layout.json' with such name.");
            }
        }
        #endregion
        #endregion
    }

    #region For new elements add the new implementation here.
    private void UpdateImageElement(Image img, Image_Element imgDetails, Language? imageLanguage, Grid grid)
    {
        img.IsVisible = true;

        #region If the same URL is already displayed, return
        if (imgDetails.TheImgIsUrl)
        {
            if (_rerouteToLanguageFolder)
                imgDetails.ImgUrlOrBase64 = imgDetails.ImgUrlOrBase64.RerouteToLanguageFolder(imageLanguage, _defaultLanguage);

            if (img.Tag is Dictionary<string, object?> storedDictionary) //Get the stored data from the image "Tag" property
            {
                var displayedUrl = (string?)storedDictionary["DisplayedUrl"];

                if (displayedUrl == imgDetails.ImgUrlOrBase64)
                    return;
            }
        }
        #endregion

        if (imgDetails.ImgUrlOrBase64 == "")
        { }
        else if (imgDetails.ImgUrlOrBase64 is not null)
        {
            img.Source = null;
            img.Source = imgDetails.ImgUrlOrBase64.ImgUrlOrBase64_To_BitmapImage(imgDetails.TheImgIsUrl, _displayImageFormat);

            if (img.Tag is Dictionary<string, object?> storedDictionary) //Get the stored data from the image "Tag" property
                storedDictionary["DisplayedUrl"] = $"{imgDetails.ImgUrlOrBase64}";

            UpdateHeightAndWidth(img, grid, imgDetails.MatchHeightWithOtherElementName, imgDetails.MatchWidthWithOtherElementName, "1", "2");
        }
        else
            FireNewError(ErrorCode.GENERAL_ERROR, $"2- The 'ImgUrlOrBase64' property for the element '{imgDetails.Name}' is null.");

        if (imgDetails.UpdateElementProperties)
        {
            imgDetails.MapZIndex(img);
            imgDetails.MapMargin(img); 
            imgDetails.MapHeight(img);
            imgDetails.MapWidth(img);
            imgDetails.MapHorizontalAlignment(img);
            imgDetails.MapVerticalAlignment(img);
            img.MapStretch( imgDetails);
        }
    }
    private void UpdateWebBrowserElement(WebBrowser webBrowser, GIF_Element gifDetails, Language? imageLanguage, Grid grid)
    {
        //If the "ImgUrlOrBase64" is GIF, it must be URL path not base64 (Not yet supported).
        webBrowser.IsVisible = true;
        webBrowser.IsHitTestVisible = false;

        if (_rerouteToLanguageFolder)
            gifDetails.GifUrl = gifDetails.GifUrl.RerouteToLanguageFolder(imageLanguage, _defaultLanguage);

        #region If the same URL is already displayed, return
        {
            if (webBrowser.Tag is Dictionary<string, object?> storedDictionary) //Get the stored data from the image "Tag" property
            {
                var displayedUrl = (string?)storedDictionary["DisplayedUrl"];

                if (displayedUrl == gifDetails.GifUrl)
                    return;
            }
        }
        #endregion

        if (gifDetails.GifUrl == "")
        { }
        else if (gifDetails.GifUrl is not null)
        {
            if (File.Exists(gifDetails.GifUrl))
            {
                var html = gifDetails.GifUrl.GenerateHtmlCodeForGif(webBrowser.ActualWidth, webBrowser.ActualHeight);

                LoadCompletedEventHandler? onBlankLoaded = null;
                onBlankLoaded = (_, e) =>
                {
                    if (e.Uri != null && e.Uri.ToString() == "about:blank")
                    {
                        webBrowser.LoadCompleted -= onBlankLoaded;
                        webBrowser.NavigateToString(html);
                    }
                };
                webBrowser.LoadCompleted += onBlankLoaded;
                webBrowser.Navigate("about:blank");

                if (webBrowser.Tag is Dictionary<string, object?> storedDictionary) //Get the stored data from the image "Tag" property
                    storedDictionary["DisplayedUrl"] = gifDetails.GifUrl;

                UpdateHeightAndWidth(webBrowser, grid, gifDetails.MatchHeightWithOtherElementName, gifDetails.MatchWidthWithOtherElementName, "3","4");
            }
            else
                FireNewError(ErrorCode.GENERAL_ERROR, $"1- The value of the property 'GifUrl' is not valid as an Image path! GifUrl: {gifDetails.GifUrl}, Element Name: {gifDetails.Name}");
        }
        else
            FireNewError(ErrorCode.GENERAL_ERROR, $"3- The 'GifUrl' property for the element '{gifDetails.Name}' is null.");

        if (gifDetails.UpdateElementProperties)
        {
            gifDetails.MapMargin(webBrowser);
            gifDetails.MapHeight(webBrowser);
            gifDetails.MapWidth(webBrowser);
            gifDetails.MapHorizontalAlignment(webBrowser);
            gifDetails.MapVerticalAlignment(webBrowser);
        }
    }
    private void UpdateTextBlockElement(TextBlock textBlock, TextBlock_Element textBlockDetails)
    {
        textBlock.IsVisible = true;
        
        CheckCountDown(textBlock, textBlockDetails);
        
        if (!textBlockDetails.UpdateElementProperties) return;
        
        textBlock.MapZIndex( textBlockDetails);
        textBlock.MapFontSize(textBlockDetails);
        textBlock.MapForeground(textBlockDetails);
        textBlock.MapBackground(textBlockDetails);
        textBlock.MapHeight(textBlockDetails);
        textBlock.MapWidth(textBlockDetails);
        textBlock.MapFontWeight(textBlockDetails);
        textBlock.MapFontFamily(textBlockDetails);
        textBlock.MapTextAlignment(textBlockDetails);
        textBlock.MapHorizontalAlignment(textBlockDetails);
        textBlock.MapVerticalAlignment(textBlockDetails);
        textBlock.MapWrapping(textBlockDetails);
        textBlock.MapMargins(textBlockDetails);
        textBlock.MapPadding(textBlockDetails);
    }
    private async Task UpdateVlcControlElement(VlcControl vlcControl, VlcControl_Element vlcControlDetails)
    {
        vlcControl.IsVisible = true;

        if (_config.EnableLogging)
            Logger_Method(LogType.Debug, $"#1 Entered 'UpdateVlcControlElement()' vlcControlDetails: {JsonConvert.SerializeObject(vlcControlDetails)}");

        if (_config.EnableLogging)
            Logger_Method(LogType.Debug, $"#2 vlcControlDetails.RTSP_URL: {vlcControlDetails.RTSP_URL}");

        if (vlcControlDetails.RTSP_URL == "")
        { }
        else if (vlcControlDetails.RTSP_URL is not null) //Connect to RTSP URL
        {
            if (vlcControl.Tag is Dictionary<string, object?> storedDictionary) //Get the stored data from the element's "Tag" property
            {
                if (_config.EnableLogging)
                    Logger_Method(LogType.Debug, $"#3: storedDictionary: {JsonConvert.SerializeObject(storedDictionary)}");

                var displayedUrl = (string?)storedDictionary["DisplayedUrl"];

                if (vlcControlDetails.RTSP_URL == displayedUrl) //return if the requested URL is already displayed
                {
                    if (_config.EnableLogging)
                        Logger_Method(LogType.Debug, $"#4 vlcControlDetails.RTSP_URL == displayedUrl");
                    return;
                }

                if (displayedUrl is not null) //Stop the running RTSP stream to run a new one
                    await StopRtspStreamAsync(vlcControl);

                storedDictionary["DisplayedUrl"] = $"{vlcControlDetails.RTSP_URL}";

                #region Play the RTSP
                try
                {
                    if (_config.VlcLibDirectory is null
                        || !File.Exists(Path.Combine(_config.VlcLibDirectory, "libvlc.dll"))
                        || !File.Exists(Path.Combine(_config.VlcLibDirectory, "libvlccore.dll"))
                        )
                    {
                        FireNewError(ErrorCode.FileNotFound, $"Looks like VLC library is not installed on this computer! Please make sure to install it and having the correct path in 'appsettings.json' in 'VlcLibDirectory' in 'InstructionScreenConfig' section.");
                        return;
                    }

                    if (_config.EnableLogging)
                        Logger_Method(LogType.Debug, $"#5 Playing the RTSP");

                    var streamingOptions = (List<string>?)storedDictionary["StreamingOptions"];
                    DirectoryInfo? vlcLibDirectory = _config.VlcLibDirectory is null ? null : new(_config.VlcLibDirectory);

                    if (_config.EnableLogging)
                    {
                        Logger_Method(LogType.Debug, $"#6 streamingOptions: {JsonConvert.SerializeObject(streamingOptions)}");
                        Logger_Method(LogType.Debug, $"#7 _config.VlcLibDirectory: {_config.VlcLibDirectory}");
                    }

                    vlcControl.SourceProvider.CreatePlayer(vlcLibDirectory, streamingOptions?.ToArray());
                    if (_config.EnableLogging)
                    {
                        vlcControl.SourceProvider.MediaPlayer.Log += (_, e) =>
                        { Logger_Method(LogType.Debug, $"#8 [{JsonConvert.SerializeObject(e)}]"); };
                    }
                    vlcControl.SourceProvider.MediaPlayer.Play(new Uri(vlcControlDetails.RTSP_URL));

                    if (_config.EnableLogging)
                        Logger_Method(LogType.Debug, $"#9 Done Playing the RTSP");
                }
                catch (Exception ex)
                {
                    FireNewError(ErrorCode.GENERAL_ERROR, $"Failed to Play the RTSP stream. Exception: {ex}");
                }
                #endregion
            }
        }
        else //Close the connection
        {
            if (vlcControl.Tag is Dictionary<string, object?> storedDictionary) //Get the stored data from the image "Tag" property
            {
                var displayedUrl = (string?)storedDictionary["DisplayedUrl"];
                if (displayedUrl is not null)
                    await StopRtspStreamAsync(vlcControl);
            }
            return;
        }

        if (vlcControlDetails.UpdateElementProperties)
        {
            vlcControl.MapZIndex(vlcControlDetails);
            vlcControl.MapMargins(vlcControlDetails);
            vlcControl.MapHeight(vlcControlDetails);
            vlcControl.MapWidth(vlcControlDetails);
            vlcControl.MapHorizontalAlignment(vlcControlDetails);
            vlcControl.MapVerticalAlignment(vlcControlDetails);
        }
    }
    private void UpdateMediaElement(MediaElement mediaElement, Media_Element mediaDetails)
    {
        mediaElement.IsVisible = true;

        if (mediaDetails.SourceURL is null)
        {
            FireNewError(ErrorCode.GENERAL_ERROR, $"Failed updating the media, the mediaDetails.SourceURL is null");
            return;
        }

        Logger_Method(LogType.Debug, $"Debug #1: mediaDetails.SourceURL: {mediaDetails.SourceURL}");
        if (mediaDetails.PlayAllVideosInDirectory)
        {
            if (File.Exists(mediaDetails.SourceURL))
                mediaDetails.SourceURL = Path.GetDirectoryName(mediaDetails.SourceURL);
            else if (!Directory.Exists(mediaDetails.SourceURL))
                FireNewError(ErrorCode.GENERAL_ERROR, $"The path of the 'SourceURL' does not exist. mediaDetails.SourceURL: {mediaDetails.SourceURL}");

            if (mediaElement.Tag is Dictionary<string, object?> storedDictionary) //Get the stored data from the element's "Tag" property
            {
                var videoPlayer = (VideoPlayer?)storedDictionary["VideoPlayerObject"];

                if (videoPlayer is not null)
                    videoPlayer.Resume(mediaDetails.SourceURL);
                else
                    FireNewError(ErrorCode.GENERAL_ERROR, $"The 'VideoPlayer' instance stored in the Tag property");
            }
        }
        else
        {
            mediaElement.Source = new Uri(mediaDetails.SourceURL);
            mediaElement.Play();
        }

        if (mediaDetails.UpdateElementProperties)
        {
            mediaElement.MapZIndex(mediaDetails);
            mediaElement.MapMargin(mediaDetails);
            mediaElement.MapHeight(mediaDetails);
            mediaElement.MapWidth(mediaDetails);
            mediaElement.MapHorizontalAlignment(mediaDetails);
            mediaElement.MapVerticalAlignment(mediaDetails);
        }
    }

    #endregion
    #endregion

    #region Helper Methods
    #region Count Down Functions
    private void StartCountDownTimer(TextBlock textBlock, int countDownTimeSec)
    {
        _countDownTimer?.Dispose(); //Stop the _countDownTimer before setting it again

        _countdownSeconds = countDownTimeSec;
        _countDownTextBlock = textBlock;
        _countDownTimer = new Timer(UpdateCountdownTimer, null, 0, 1000);
    }
    private void CheckCountDown(TextBlock textBlock, TextBlock_Element textBlockDetails)
    {
        if (textBlockDetails.Text == "")
        { }
        else if (textBlockDetails.Text is not null)
        {
            if (textBlockDetails.CountDownTimer is null)
                textBlock.Text = textBlockDetails.Text;
        }
        else
            Logger_Method(LogType.Warning, $"The text for the element '{textBlockDetails.Name}' is null.");

        if (textBlockDetails.CountDownTimer is not null)
        {
            if (textBlockDetails.CountDownTimer > 0)
                StartCountDownTimer(textBlock, (int)textBlockDetails.CountDownTimer);
        }
        else if (_countDownTextBlock?.Name == textBlock.Name)
            _countDownTimer?.Dispose(); //Stop the _countDownTimer when the request send the timer value "null"
    }
    private void UpdateCountdownTimer(object? _)
    {
        if (_countdownSeconds > 0)
        {
            _countdownSeconds--;
            if (_countDownTextBlock is not null)
            {
                if (MainWindow is null)
                    FireNewError(ErrorCode.GENERAL_ERROR, $"#1: MainWindow is null, unable to update the timer.");
                else
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        try
                        {
                            _countDownTextBlock.Text = _countdownSeconds.ToString();
                        }
                        catch (Exception e)
                        {
                            FireNewError(ErrorCode.GENERAL_ERROR, $"Exception while updating the count down timer: {e}");
                        }
                    });
            }
        }
        else
            _countDownTimer?.Dispose(); //Stop the _countDownTimer when countdown reaches 0
    }
    #endregion
    
    private async Task StopRtspStreamAsync(VlcControl vlcControl)
    {
        try
        {
            if (_config.EnableLogging)
                Logger_Method(LogType.Debug, $"#10 Stopping the RTSP");

            if (vlcControl.SourceProvider.MediaPlayer != null)
            {
                if (vlcControl.SourceProvider.MediaPlayer.IsPlaying())
                {
                    await Task.Run(() =>
                    {
                        try
                        {
                            vlcControl.SourceProvider.MediaPlayer.Stop();
                        }
                        catch (Exception ex)
                        {
                            FireNewError(ErrorCode.GENERAL_ERROR, $"Failed in 'vlcControl.SourceProvider.MediaPlayer.Stop()'. Exception: {ex}");
                        }
                    });
                }
                await Task.Run(() =>
                {
                    try
                    {
                        vlcControl.SourceProvider.MediaPlayer.Dispose();
                    }
                    catch (Exception ex)
                    {
                        FireNewError(ErrorCode.GENERAL_ERROR, $"Failed in 'vlcControl.SourceProvider.MediaPlayer.Dispose()'. Exception: {ex}");
                    }
                });
            }

            await Task.Run(() =>
            {
                try
                {
                    vlcControl.SourceProvider.Dispose();
                }
                catch (Exception ex)
                {
                    FireNewError(ErrorCode.GENERAL_ERROR, $"Failed in 'vlcControl.SourceProvider.Dispose()'. Exception: {ex}");
                }
            });
            
            if (vlcControl.Tag is Dictionary<string, object?> storedDictionary) //Get the stored data from the image "Tag" property
                storedDictionary["DisplayedUrl"] = null;
        }
        catch (Exception ex)
        {
            FireNewError(ErrorCode.GENERAL_ERROR, $"Failed to Stop the RTSP stream. Exception: {ex}");
        }
    }

    //TODO: this should find the element with any type to get the height/width of. + Support the same feature in the VLC control element
    private void UpdateHeightAndWidth(FrameworkElement img, Grid grid, string? matchHeightWithOtherImageName, string? matchWidthWithOtherImageName, string erroNum1, string? erroNum2)
    {
        if (matchHeightWithOtherImageName is not null)
        {
            Image? imageToGetHeightFrom = grid.Children.OfType<Image>().FirstOrDefault(q => q.Name == matchHeightWithOtherImageName);

            if (imageToGetHeightFrom is not null)
            {
                Task.Run(() =>
                {
                    if (MainWindow is null)
                        FireNewError(ErrorCode.GENERAL_ERROR, $"#{erroNum1}: MainWindow is null, unable to update the height.");
                    else
                        Dispatcher.UIThread.InvokeAsync(new Action(() =>
                        {
                            try
                            {
                                //Use Loaded event to ensure the element is part of the visual tree
                                imageToGetHeightFrom.Loaded += (_, _) =>
                                {
                                    //Ensure layout has completed and the size is known
                                    imageToGetHeightFrom.SizeChanged += (_, _) =>
                                    {
                                        if (imageToGetHeightFrom.ActualHeight > 0)
                                            img.Height = imageToGetHeightFrom.ActualHeight;
                                        else if (_enableDetailedLogging)
                                            FireNewError(ErrorCode.GENERAL_ERROR, $"#{erroNum1}: Failed to get valid ActualHeight.");
                                    };
                                };
                            }
                            catch (Exception ex) { FireNewError(ErrorCode.GENERAL_ERROR, $"#{erroNum1}: Failed to update the Height. Exception: {ex}"); }
                        }));
                });
            }
        }

        if (matchWidthWithOtherImageName is not null)
        {
            Image? imageToGetWidthFrom = grid.Children.OfType<Image>().FirstOrDefault(q => q.Name == matchWidthWithOtherImageName);

            if (imageToGetWidthFrom is not null)
            {
                Task.Run(() =>
                {
                    if (MainWindow is null)
                        FireNewError(ErrorCode.GENERAL_ERROR, $"#{erroNum2}: MainWindow is null, unable to update the width.");
                    else
                        Dispatcher.UIThread.InvokeAsync(new Action(() =>
                        {
                            try
                            {
                                //Use Loaded event to ensure the element is part of the visual tree
                                imageToGetWidthFrom.Loaded += (_, _) =>
                                {
                                    //Ensure layout has completed and the size is known
                                    imageToGetWidthFrom.SizeChanged += (_, _) =>
                                    {
                                        if (imageToGetWidthFrom.ActualWidth > 0)
                                            img.Width = imageToGetWidthFrom.ActualWidth;
                                        else if (_enableDetailedLogging)
                                            FireNewError(ErrorCode.GENERAL_ERROR, $"#{erroNum2}: Failed to get valid ActualWidth.");
                                    };
                                };
                            }
                            catch (Exception ex) { FireNewError(ErrorCode.GENERAL_ERROR, $"#{erroNum2}: Failed to update the Width. Exception: {ex}"); }
                        }));
                });
            }
        }
    }

    public static bool IsExtraMonitorConnected(Window window)
    {
        var screens = window.Screens?.All;
        return screens != null && screens.Count > 1;
    }

    private void ValidateIndexes(Layout? layout)
    {
        if (layout is null)
            return;

        if (layout.Image_Elements is not null)
        {
            if (layout.ColumnDefinitions_Count > 0)
            {
                foreach (var imageElement in layout.Image_Elements.Where(imageElement => imageElement.ColumnIndex >= layout.ColumnDefinitions_Count))
                {
                    FireNewError(ErrorCode.GENERAL_ERROR, $"1- Error: {imageElement.Name} has ColumnIndex exceeding ColumnDefinitions_Count.");
                }
            }
            else
                FireNewError(ErrorCode.GENERAL_ERROR, $"1- Error: layout.ColumnDefinitions_Count value is not > 0.");

            if (layout.RowDefinitions_Count > 0)
            {
                foreach (var imageElement in layout.Image_Elements.Where(imageElement => imageElement.RowIndex >= layout.RowDefinitions_Count))
                {
                    FireNewError(ErrorCode.GENERAL_ERROR, $"2- Error: {imageElement.Name} has RowIndex exceeding RowDefinitions_Count.");
                }
            }
            else
                FireNewError(ErrorCode.GENERAL_ERROR, $"2- Error: layout.RowDefinitions_Count value is not > 0.");
        }

        if (layout.TextBlock_Elements is not null)
        {
            if (layout.ColumnDefinitions_Count > 0)
            {
                foreach (var textBlockElement in layout.TextBlock_Elements.Where(textBlockElement => textBlockElement.ColumnIndex >= layout.ColumnDefinitions_Count))
                {
                    FireNewError(ErrorCode.GENERAL_ERROR, $"3- Error: {textBlockElement.Name} has ColumnIndex exceeding ColumnDefinitions_Count.");
                }
            }
            else
                FireNewError(ErrorCode.GENERAL_ERROR, $"3- Error: layout.ColumnDefinitions_Count value is not > 0.");

            if (layout.RowDefinitions_Count > 0)
            {
                foreach (var textBlockElement in layout.TextBlock_Elements.Where(textBlockElement => textBlockElement.RowIndex >= layout.RowDefinitions_Count))
                {
                    FireNewError(ErrorCode.GENERAL_ERROR, $"4- Error: {textBlockElement.Name} has RowIndex exceeding RowDefinitions_Count.");
                }
            }
            else
                FireNewError(ErrorCode.GENERAL_ERROR, $"4- Error: layout.RowDefinitions_Count value is not > 0.");
        }
    }

    private void CheckForDuplicateNames(Layout? layout1, Layout? layout2, Layout? layout3)
    {
        List<string> nameList =
        [
            .. layout1.GetListOfNames(),
            .. layout2.GetListOfNames(),
            .. layout3.GetListOfNames(),
        ];

        var duplicateStrings = nameList
            .GroupBy(s => s)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateStrings.Count > 0)
            FireNewError(ErrorCode.GENERAL_ERROR, $"The configured elements' names should be unique. The following names are duplicated {JsonConvert.SerializeObject(duplicateStrings)}");
    }
 
    #endregion
    
    #region Logger & Device Status Update
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
    #endregion
}
*/
