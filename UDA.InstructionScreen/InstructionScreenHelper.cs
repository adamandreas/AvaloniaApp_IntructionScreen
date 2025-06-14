using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Microsoft.Win32;
using UDA.InstructionScreen.Shared.Entities;
using UDA.InstructionScreen.Shared.Entities.UI_Elements;
using UDA.UDACapabilities.Shared;
using UDA.UDACapabilities.Shared.Enums;
using LibVLCSharp.Shared;
using Bitmap = Avalonia.Media.Imaging.Bitmap;
using Color = Avalonia.Media.Color;
using FontFamily = Avalonia.Media.FontFamily;
using Image = Avalonia.Controls.Image;

namespace UDA.InstructionScreen.Helper;

public static class InstructionScreenHelper
{
    
    
    public static string? GetFullPath(this string? urlPath)
    {
        if (urlPath is null)
            return null;

        if (Path.IsPathRooted(urlPath)) //Absolute path
            return urlPath;

        //Relative path
        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var basePath = string.IsNullOrEmpty(urlPath) ? appDirectory : Path.Combine(appDirectory, urlPath);

        return Path.GetFullPath(basePath);
    }
/*
#region Grid
public static void MapColumnGridDefinition(this Grid grid, GridDetails gridDetails, FrameworkElement element , bool checkCountGreaterThanColumnIndex=true)
{
    if (gridDetails.ColumnGridDefinition?.Dimension != null)
    {
        GridUnitType gridUnitType = gridDetails.ColumnGridDefinition.DimensionType switch
        {
            1 => GridUnitType.Star,
            2 => GridUnitType.Auto,
            3 => GridUnitType.Pixel,
            _ => GridUnitType.Pixel
        };

        if (!checkCountGreaterThanColumnIndex)
            grid.ColumnDefinitions[gridDetails.ColumnIndex].Width = new GridLength((double)gridDetails.ColumnGridDefinition.Dimension, gridUnitType);
        else if (grid.ColumnDefinitions.Count >= gridDetails.ColumnIndex)
            grid.ColumnDefinitions[gridDetails.ColumnIndex].Width = new GridLength((double)gridDetails.ColumnGridDefinition.Dimension, gridUnitType);

        Grid.SetColumn(element, gridDetails.ColumnIndex);
    }
}
public static void MapRowGridDefinition(this Grid grid, GridDetails gridDetails, FrameworkElement element, bool checkCountGreaterThanRowIndex=true)
{
    if (gridDetails.RowGridDefinition?.Dimension != null)
    {
        GridUnitType gridUnitType = gridDetails.RowGridDefinition.DimensionType switch
        {
            1 => GridUnitType.Star,
            2 => GridUnitType.Auto,
            3 => GridUnitType.Pixel,
            _ => GridUnitType.Pixel
        };

        if (!checkCountGreaterThanRowIndex)
            grid.RowDefinitions[gridDetails.RowIndex].Height = new GridLength((double)gridDetails.RowGridDefinition.Dimension, gridUnitType);
        else if ( grid.RowDefinitions.Count >= gridDetails.RowIndex)
            grid.RowDefinitions[gridDetails.RowIndex].Height = new GridLength((double)gridDetails.RowGridDefinition.Dimension, gridUnitType);

        Grid.SetRow(element, gridDetails.RowIndex);
    }
}
public static void MapBackground(this Layout layout, Grid grid)
{
    if (layout.Background_Color is not null)
        grid.Background = layout.Background_Color.String_To_Brush();
}
public static void MapHeight(this Layout layout, Grid grid)
{
    grid.Height = layout.Height is null or < 0 ? double.NaN : (double)layout.Height;
}
public static void MapWidth(this Layout layout, Grid grid)
{
    grid.Width = layout.Width is null or < 0 ? double.NaN : (double)layout.Width;
}
public static void MapRenderingMode(this Layout layout, Grid grid)
{
    if (layout.RenderingMode is not null)
    {
        EdgeMode edgeMode = layout.RenderingMode switch
        {
            1 => EdgeMode.Aliased,
            _ => EdgeMode.Unspecified
        };
        RenderOptions.SetEdgeMode(grid, edgeMode);
    }
}
public static void MapMargins(this Layout layout, Grid grid)
{
    if (layout.Margin is not null && layout.Margin.Count == 4)
        grid.Margin = new Thickness(layout.Margin[0], layout.Margin[1], layout.Margin[2], layout.Margin[3]);
}
public static void MapHorizontalAlignment(this Layout layout, Grid grid)
{
    if (layout.HorizontalAlignment is not null)
        grid.HorizontalAlignment = layout.HorizontalAlignment switch
        {
            1 => HorizontalAlignment.Left,
            2 => HorizontalAlignment.Center,
            3 => HorizontalAlignment.Right,
            4 => HorizontalAlignment.Stretch,
            _ => HorizontalAlignment.Center
        };
}
public static void MapVerticalAlignment(this Layout layout, Grid grid)
{
    if (layout.VerticalAlignment is not null)
        grid.VerticalAlignment = layout.VerticalAlignment switch
        {
            1 => VerticalAlignment.Bottom,
            2 => VerticalAlignment.Center,
            3 => VerticalAlignment.Top,
            4 => VerticalAlignment.Stretch,
            _ => VerticalAlignment.Center
        };
}
#endregion

#region Image
public static void MapName(this Image_Element gridDetails, Image newImg)
{
    newImg.Name = gridDetails.Name;
}
public static void MapImageTag(this Image_Element gridDetails, Image newImg)
{
    //Set the Default properties in the "Tag" of the element
    Dictionary<string, object?> imgTagDictionary = new()
    {
        { "DisplayedUrl", null },
        { "OriginalHeight", gridDetails.Height },
        { "OriginalWidth", gridDetails.Width },
        { "NeverCollapse", gridDetails.NeverCollapse }
    };
    newImg.Tag = imgTagDictionary;
}
public static void MapImageSource(this Image_Element gridDetails, ImageFormat format, Image newImg)
{
    if (!string.IsNullOrEmpty(gridDetails.ImgUrlOrBase64))
    {
        newImg.Source = null;
        newImg.Source = gridDetails.ImgUrlOrBase64.ImgUrlOrBase64_To_BitmapImage(gridDetails.TheImgIsUrl, format);

        if (newImg.Tag is Dictionary<string, object?> storedDictionary) //Get the stored data from the image "Tag" property
            storedDictionary["DisplayedUrl"] = $"{gridDetails.ImgUrlOrBase64}";
    }
}
public static void MapStretch(this Image_Element gridDetails, Image newImg)
{
    if (gridDetails.Stretch is not null)
        newImg.Stretch = gridDetails.Stretch switch
        {
            1 => Stretch.Fill,
            2 => Stretch.None,
            3 => Stretch.Uniform,
            _ => Stretch.UniformToFill
        };
}
public static void MapVerticalAlignment(this Image_Element gridDetails, Image newImg)
{
    if (gridDetails.VerticalAlignment is not null)
        newImg.VerticalAlignment = gridDetails.VerticalAlignment switch
        {
            1 => VerticalAlignment.Bottom,
            3 => VerticalAlignment.Top,
            4 => VerticalAlignment.Stretch,
            _ => VerticalAlignment.Center
        };
}
public static void MapHorizontalAlignment(this Image_Element gridDetails, Image newImg)
{
    if (gridDetails.HorizontalAlignment is not null)
        newImg.HorizontalAlignment = gridDetails.HorizontalAlignment switch
        {
            1 => HorizontalAlignment.Left,
            3 => HorizontalAlignment.Right,
            4 => HorizontalAlignment.Stretch,
            _ => HorizontalAlignment.Center
        };
}
public static void MapMargin(this Image_Element gridDetails, Image newImg)
{
    if (gridDetails.Margin is not null && gridDetails.Margin.Count == 4)
        newImg.Margin = new Thickness(gridDetails.Margin[0], gridDetails.Margin[1], gridDetails.Margin[2], gridDetails.Margin[3]);
}
public static void MapHeight(this Image_Element gridDetails, Image newImg)
{
    newImg.Height = gridDetails.Height is null || gridDetails.Height < 0 ? double.NaN : (double)gridDetails.Height;
}
public static void MapWidth(this Image_Element gridDetails, Image newImg)
{
    newImg.Width = gridDetails.Width is null || gridDetails.Width < 0 ? double.NaN : (double)gridDetails.Width;
}
public static void MapZIndex(this Image_Element gridDetails, Image newImg)
{
    Panel.SetZIndex(newImg, gridDetails.Z_Index);
}
public static void MapStretch(this Image img, Image_Element gridDetails)
{
    if (gridDetails.Stretch is not null)
        img.Stretch = gridDetails.Stretch switch
        {
            1 => Stretch.Fill,
            2 => Stretch.None,
            3 => Stretch.Uniform,
            _ => Stretch.UniformToFill
        };
}
#endregion

#region MediaElement
public static void MapZIndex(this MediaElement mediaElement, Media_Element gridDetails)
{
    Panel.SetZIndex(mediaElement, gridDetails.Z_Index);
}
public static void MapMargin(this MediaElement mediaElement, Media_Element gridDetails)
{
    if (gridDetails.Margin is not null && gridDetails.Margin.Count == 4)
        mediaElement.Margin = new Thickness(gridDetails.Margin[0], gridDetails.Margin[1], gridDetails.Margin[2], gridDetails.Margin[3]);
}
public static void MapHeight(this MediaElement mediaElement, Media_Element gridDetails)
{
    mediaElement.Height = gridDetails.Height is null || gridDetails.Height < 0 ? double.NaN : (double)gridDetails.Height;
}
public static void MapWidth(this MediaElement mediaElement, Media_Element gridDetails)
{
    mediaElement.Width = gridDetails.Width is null || gridDetails.Width < 0 ? double.NaN : (double)gridDetails.Width;
}
public static void MapHorizontalAlignment(this MediaElement mediaElement, Media_Element gridDetails)
{
    if (gridDetails.HorizontalAlignment is not null)
        mediaElement.HorizontalAlignment = gridDetails.HorizontalAlignment switch
        {
            1 => HorizontalAlignment.Left,
            3 => HorizontalAlignment.Right,
            4 => HorizontalAlignment.Stretch,
            _ => HorizontalAlignment.Center
        };
}
public static void MapVerticalAlignment(this MediaElement mediaElement, Media_Element gridDetails)
{
    if (gridDetails.VerticalAlignment is not null)
        mediaElement.VerticalAlignment = gridDetails.VerticalAlignment switch
        {
            1 => VerticalAlignment.Bottom,
            3 => VerticalAlignment.Top,
            4 => VerticalAlignment.Stretch,
            _ => VerticalAlignment.Center
        };
}
public static void MapTag(this Media_Element gridDetails, VideoPlayer videoPlayer, MediaElement newMediaElement)
{
    //Set the Default properties in the "Tag" of the element
    Dictionary<string, object?> imgTagDictionary = new()
    {
        { "OriginalHeight", gridDetails.Height },
        { "OriginalWidth", gridDetails.Width },
        { "ReleaseMemoryOnPause", gridDetails.ReleaseMemoryOnPause },
        { "StopNotPauseOnCollapse", gridDetails.StopNotPauseOnCollapse },
        { "VideoPlayerObject", videoPlayer},
        { "NeverCollapse", gridDetails.NeverCollapse }
    };
    newMediaElement.Tag = imgTagDictionary;
}
public static void MapName(this Media_Element gridDetails, MediaElement newMediaElement)
{
    newMediaElement.Name = gridDetails.Name;
}
#endregion

#region TextBox
public static void MapZIndex(this TextBlock textBlock, TextBlock_Element gridDetails)
{
    Panel.SetZIndex(textBlock, gridDetails.Z_Index);
}
public static void MapFontSize(this TextBlock textBlock, TextBlock_Element gridDetails)
{
    if (gridDetails.FontSize is not null)
        textBlock.FontSize = (double)gridDetails.FontSize;
}
public static void MapForeground(this TextBlock textBlock, TextBlock_Element gridDetails)
{
    if (gridDetails.Foreground is not null)
        textBlock.Foreground = gridDetails.Foreground.String_To_Brush();
}
public static void MapBackground(this TextBlock textBlock, TextBlock_Element gridDetails)
{
    if (gridDetails.Background is not null)
        textBlock.Background = gridDetails.Background.String_To_Brush();
}
public static void MapHeight(this TextBlock textBlock, TextBlock_Element gridDetails)
{
    textBlock.Height = gridDetails.Height is null or < 0 ? double.NaN : (double)gridDetails.Height;
}
public static void MapWidth(this TextBlock textBlock, TextBlock_Element gridDetails)
{
    textBlock.Width = gridDetails.Width is null or < 0 ? double.NaN : (double)gridDetails.Width;
}
public static void MapPadding(this TextBlock textBlock, TextBlock_Element gridDetails)
{
    if (gridDetails.Padding is not null && gridDetails.Padding.Count == 4)
        textBlock.Padding = new Thickness(gridDetails.Padding[0], gridDetails.Padding[1], gridDetails.Padding[2], gridDetails.Padding[3]);
}
public static void MapMargins(this TextBlock textBlock, TextBlock_Element gridDetails)
{
    if (gridDetails.Margin is not null && gridDetails.Margin.Count == 4)
        textBlock.Margin = new Thickness(gridDetails.Margin[0], gridDetails.Margin[1], gridDetails.Margin[2], gridDetails.Margin[3]);
}
public static void MapFontWeight(this TextBlock textBlock, TextBlock_Element gridDetails)
{
    if (gridDetails.FontWeight is not null)
        textBlock.FontWeight = gridDetails.FontWeight switch
        {
            1 => FontWeight.Black,
            2 => FontWeight.Bold,
            3 => FontWeight.DemiBold,
            4 => FontWeight.ExtraBlack,
            5 => FontWeight.ExtraBold,
            6 => FontWeight.ExtraLight,
            7 => FontWeight.Heavy,
            8 => FontWeight.Light,
            9 => FontWeight.Medium,
            10 => FontWeight.Normal,
            11 => FontWeight.Regular,
            12 => FontWeight.SemiBold,
            13 => FontWeight.Thin,
            14 => FontWeight.UltraBlack,
            15 => FontWeight.UltraBold,
            16 => FontWeight.UltraLight,
            _ => FontWeight.Normal
        };
}
public static void MapHorizontalAlignment(this TextBlock textBlock, TextBlock_Element gridDetails)
{
    if (gridDetails.HorizontalAlignment is not null)
        textBlock.HorizontalAlignment = gridDetails.HorizontalAlignment switch
        {
            1 => HorizontalAlignment.Left,
            2 => HorizontalAlignment.Center,
            3 => HorizontalAlignment.Right,
            4 => HorizontalAlignment.Stretch,
            _ => HorizontalAlignment.Center
        };
}
public static void MapVerticalAlignment(this TextBlock textBlock, TextBlock_Element gridDetails)
{
    if (gridDetails.VerticalAlignment is not null)
        textBlock.VerticalAlignment = gridDetails.VerticalAlignment switch
        {
            1 => VerticalAlignment.Bottom,
            2 => VerticalAlignment.Center,
            3 => VerticalAlignment.Top,
            4 => VerticalAlignment.Stretch,
            _ => VerticalAlignment.Center
        };
}
public static void MapWrapping(this TextBlock textBlock, TextBlock_Element gridDetails)
{
    if (gridDetails.TextWrapping is not null)
        textBlock.TextWrapping = gridDetails.TextWrapping switch
        {
            1 => TextWrapping.NoWrap,
            2 => TextWrapping.Wrap,
            3 => TextWrapping.WrapWithOverflow,
            _ => TextWrapping.NoWrap
        };
}
public static void MapTextAlignment(this TextBlock textBlock, TextBlock_Element gridDetails)
{
    if (gridDetails.TextAlignment is not null)
        textBlock.TextAlignment = gridDetails.TextAlignment switch
        {
            1 => TextAlignment.Left,
            2 => TextAlignment.Center,
            3 => TextAlignment.Right,
            4 => TextAlignment.Justify,
            _ => TextAlignment.Center
        };
}
public static void MapFontFamily(this TextBlock textBlock, TextBlock_Element gridDetails)
{
    if (gridDetails.FontFamily is not null)
        textBlock.FontFamily = gridDetails.FontFamily.GetFontFamily();
}
public static void MapTag(this TextBlock newTextBlock, TextBlock_Element gridDetails)
{
    #region Set the Default properties in the "Tag" of the element

    Dictionary<string, object?> textBlockTagDictionary = new()
    {
        { "NeverCollapse", gridDetails.NeverCollapse }
    };

    newTextBlock.Tag = textBlockTagDictionary;

    #endregion
}
public static void MapName(this TextBlock newTextBlock, TextBlock_Element gridDetails)
{
    newTextBlock.Name = gridDetails.Name;
}
public static void MapText(this TextBlock newTextBlock, TextBlock_Element gridDetails)
{
    if (gridDetails.Text is not null)
        newTextBlock.Text = gridDetails.Text;
}
#endregion

#region VLC
public static void MapZIndex(this VlcControl vlcControl, VlcControl_Element gridDetails)
{
    Panel.SetZIndex(vlcControl, gridDetails.Z_Index);
}
public static void MapMargins(this VlcControl vlcControl, VlcControl_Element gridDetails)
{
    if (gridDetails.Margin is not null && gridDetails.Margin.Count == 4)
        vlcControl.Margin = new Thickness(gridDetails.Margin[0], gridDetails.Margin[1], gridDetails.Margin[2], gridDetails.Margin[3]);
}
public static void MapHeight(this VlcControl vlcControl, VlcControl_Element gridDetails)
{
    vlcControl.Height = gridDetails.Height is null || gridDetails.Height < 0 ? double.NaN : (double)gridDetails.Height;
}
public static void MapWidth(this VlcControl vlcControl, VlcControl_Element gridDetails)
{
    vlcControl.Width = gridDetails.Width is null || gridDetails.Width < 0 ? double.NaN : (double)gridDetails.Width;
}
public static void MapVerticalAlignment(this VlcControl vlcControl, VlcControl_Element gridDetails)
{
    if (gridDetails.VerticalAlignment is not null)
        vlcControl.VerticalAlignment = gridDetails.VerticalAlignment switch
        {
            1 => VerticalAlignment.Bottom,
            3 => VerticalAlignment.Top,
            4 => VerticalAlignment.Stretch,
            _ => VerticalAlignment.Center
        };
}
public static void MapHorizontalAlignment(this VlcControl vlcControl, VlcControl_Element gridDetails)
{
    if (gridDetails.HorizontalAlignment is not null)
        vlcControl.HorizontalAlignment = gridDetails.HorizontalAlignment switch
        {
            1 => HorizontalAlignment.Left,
            3 => HorizontalAlignment.Right,
            4 => HorizontalAlignment.Stretch,
            _ => HorizontalAlignment.Center
        };
}
public static void MapTag(this VlcControl vlcControl, VlcControl_Element gridDetails)
{
    // Set the Default properties in the "Tag" of the element
    Dictionary<string, object?> imgTagDictionary = new()
    {
        { "DisplayedUrl", null },
        { "OriginalHeight", gridDetails.Height },
        { "OriginalWidth", gridDetails.Width },
        { "StreamingOptions", gridDetails.StreamingOptions },
        { "NeverCollapse", gridDetails.NeverCollapse }
    };
    vlcControl.Tag = imgTagDictionary;
}
public static void MapName(this VlcControl vlcControl, VlcControl_Element gridDetails)
{
    vlcControl.Name = gridDetails.Name;
}
#endregion

#region WebBrowser
public static void MapHorizontalAlignment(this GIF_Element gridDetails, WebBrowser newWebBrowser)
{
    if (gridDetails.HorizontalAlignment is not null)
        newWebBrowser.HorizontalAlignment = gridDetails.HorizontalAlignment switch
        {
            1 => HorizontalAlignment.Left,
            3 => HorizontalAlignment.Right,
            4 => HorizontalAlignment.Stretch,
            _ => HorizontalAlignment.Center
        };
}
public static void MapVerticalAlignment(this GIF_Element gridDetails, WebBrowser newWebBrowser)
{
    if (gridDetails.VerticalAlignment is not null)
        newWebBrowser.VerticalAlignment = gridDetails.VerticalAlignment switch
        {
            1 => VerticalAlignment.Bottom,
            3 => VerticalAlignment.Top,
            4 => VerticalAlignment.Stretch,
            _ => VerticalAlignment.Center
        };
}
public static void MapHeight(this GIF_Element gridDetails, WebBrowser newWebBrowser)
{
    newWebBrowser.Height = gridDetails.Height is null or < 0 ? double.NaN : (double)gridDetails.Height;
}
public static void MapWidth(this GIF_Element gridDetails, WebBrowser newWebBrowser)
{
    newWebBrowser.Width = gridDetails.Width is null or < 0 ? double.NaN : (double)gridDetails.Width;
}
public static void MapMargins(this GIF_Element gridDetails, WebBrowser newWebBrowser)
{
    if (gridDetails.Margin is not null && gridDetails.Margin.Count == 4)
        newWebBrowser.Margin = new Thickness(gridDetails.Margin[0], gridDetails.Margin[1], gridDetails.Margin[2], gridDetails.Margin[3]);
}
public static void MapTag(this GIF_Element gridDetails, WebBrowser newWebBrowser)
{
    // Set the Default properties in the "Tag" of the element
    Dictionary<string, object?> webBrowserTagDictionary = new()
    {
        { "DisplayedUrl", null },
        { "OriginalHeight", gridDetails.Height },
        { "OriginalWidth", gridDetails.Width },
        { "NeverCollapse", gridDetails.NeverCollapse }
    };
    newWebBrowser.Tag = webBrowserTagDictionary;
}
public static void MapName(this GIF_Element gridDetails, WebBrowser newWebBrowser)
{
    newWebBrowser.Name = gridDetails.Name;
}
public static void MapMargin(this GIF_Element gridDetails, WebBrowser newWebBrowser)
{
    if (gridDetails.Margin is not null && gridDetails.Margin.Count == 4)
        newWebBrowser.Margin = new Thickness(gridDetails.Margin[0], gridDetails.Margin[1], gridDetails.Margin[2], gridDetails.Margin[3]);
}
#endregion

#region Converters
public static async Task<Bitmap?> ImgPathOrBase64_ToBitmap(this string pathOrBase64, bool isFilePath)
{
    try
    {
        if (!isFilePath)
        {
            // Base64 to Bitmap
            byte[] imageBytes = Convert.FromBase64String(pathOrBase64);
            using MemoryStream stream = new(imageBytes);
            return new Bitmap(stream);
        }
        if(!File.Exists(pathOrBase64))
        {
            // Load from local file path
            await using FileStream stream = File.OpenRead(pathOrBase64);
            return new Bitmap(stream);
        }
    }
    catch (Exception ex)
    {
        throw new Exception($"Error loading image: {ex.Message}", ex);
    }
}


public static Bitmap Url_To_Bitmap(this string? url)
{
    if (url is not null && Path.HasExtension(url))
    {
        try
        {
            Stream stream = File.OpenRead(url);
            return new Bitmap(stream);
        }
        catch (Exception)
        {
        }
    }

    return new(40, 40);
}


// i have something specail here but will try a temporary thing
public static FontFamily GetFontFamily(this string fontName)
{
    fontName = fontName
        .Replace(".otf", "", StringComparison.OrdinalIgnoreCase)
        .Replace(".ttf", "", StringComparison.OrdinalIgnoreCase);

    var systemFont = new FontFamily(fontName);
    if (systemFont.Name.Equals(fontName, StringComparison.OrdinalIgnoreCase))
        return systemFont;
    string basePath = "avares://UDA.InstructionScreen/Fonts/";
    var resourceUri = $"{basePath}#{fontName}";

    return new FontFamily(resourceUri);
}

private static SolidColorBrush String_To_Brush(this string colorString)
{
    if ((colorString.Length != 7 && colorString.Length != 9) || colorString[0] != '#')
        throw new ArgumentException("Invalid color string format. It should be in the format '#RRGGBB' or '#AARRGGBB'.");

    try
    {
        byte a = 255; //Default alpha value if not specified
        if (colorString.Length == 9)
        {
            a = Convert.ToByte(colorString.Substring(1, 2), 16);
            colorString = colorString[3..]; //Remove alpha component
            colorString = $"#{colorString}";
        }

        byte r = Convert.ToByte(colorString.Substring(1, 2), 16);
        byte g = Convert.ToByte(colorString.Substring(3, 2), 16);
        byte b = Convert.ToByte(colorString.Substring(5, 2), 16);

        Color mediaColor = Color.FromArgb(a, r, g, b);

        return new SolidColorBrush(mediaColor);
    }
    catch (Exception ex)
    {
        throw new Exception($"ErrorCode.GENERAL_ERROR,Error converting color '{colorString}' string to Brush. Exception: {ex}");
    }
}

#endregion

#region To disable touch in web view
public static string GenerateHtmlCodeForGif(this string gifUrlPath, double webBrowserActualWidth, double webBrowserActualHeight)
{
    #region Calculate dev margin
    double aspectRatio = 1.0;

    try
    {
        using System.Drawing.Image img = System.Drawing.Image.FromFile(gifUrlPath);
        aspectRatio = (double)img.Width / img.Height;
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error reading image: " + ex.Message);
    }

    double imgWidth, imgHeight;
    if (aspectRatio > 1.0)
    {
        imgWidth = webBrowserActualWidth;
        imgHeight = webBrowserActualWidth / aspectRatio;

        if (imgHeight > webBrowserActualHeight)
        {
            imgHeight = webBrowserActualHeight;
            imgWidth = webBrowserActualHeight * aspectRatio;
        }
    }
    else
    {
        imgHeight = webBrowserActualHeight;
        imgWidth = webBrowserActualHeight * aspectRatio;

        if (imgWidth > webBrowserActualWidth)
        {
            imgWidth = webBrowserActualWidth;
            imgHeight = webBrowserActualWidth / aspectRatio;
        }
    }
    #endregion

    string encodedImgUrl = System.Web.HttpUtility.HtmlEncode(gifUrlPath);

    SetWebBrowserVersionAndDisableZoom();

    string html =
        $@"<html>
        <head>
            <style>
                html, body {{
                    margin: 0;
                    overflow: hidden;
                }}
                body, img {{
                    pointer-events: none;  /* Disable all interactions - any kind of mouse interactions#1#
                    user-select: none;     /* Disable selection - useful if you have text in the HTML #1#
                }}
                div {{
                    display: flex;
                    justify-content: center;
                    align-items: center;
                    height: 100vh;  /* Full viewport height #1#
                }}
            </style>
        </head>
        <body>
            <div>
                <img src='{encodedImgUrl}' width='{imgWidth}' height='{imgHeight}' />
            </div>
        </body>
    </html>";

    return html;
}
private static void SetWebBrowserVersionAndDisableZoom()
{
    string appName = Path.GetFileName(System.Diagnostics.Process.GetCurrentProcess().MainModule!.FileName);

    try
    {
        // Set WebBrowser to use IE11 mode - added this because 7/8 break alignments
        using (RegistryKey regKey = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION"))
        {
            regKey.SetValue(appName, 11001, RegistryValueKind.DWord);  // IE11 mode
        }

        // Disable zoom
        using (RegistryKey regKeyZoom = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Internet Explorer\Zoom"))
        {
            regKeyZoom.SetValue("ZoomDisabled", 1, RegistryValueKind.DWord);  // Disable zoom
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Failed to set WebBrowser emulation version or disable zoom: " + ex.Message);
    }
}
#endregion

public static string ManageThePathUrl(this string urlPath, string instructionImagesParentPath) => File.Exists(urlPath) ? Path.GetFullPath(urlPath) : Path.Combine(instructionImagesParentPath, urlPath);
public static string? RerouteToLanguageFolder(this string? imageUrl, Language? imgLanguage, Language? defaultLanguage)
{
    if (imageUrl is null)
        return null;

    string? directory = Path.GetDirectoryName(imageUrl);
    if (directory is not null)
    {
        Array languages = Enum.GetValues(typeof(Language));
        foreach (Language eachLanguage in languages)
        {
            string lastFolder = directory.Split(Path.DirectorySeparatorChar).Last();
            if (lastFolder == eachLanguage.ToString())
            {
                directory = Directory.GetParent(directory)!.FullName;
                break;
            }
        }
    }
    else
        return null;

    return (imgLanguage ?? defaultLanguage) switch
    {
        Language.English => Path.Combine($"{directory}/EN/", Path.GetFileName(imageUrl)),
        Language.Arabic => Path.Combine($"{directory}/AR/", Path.GetFileName(imageUrl)),
        _ => Path.Combine($"{directory}/ArAndEn/", Path.GetFileName(imageUrl)),
    };
}
public static List<string> GetListOfNames(this Layout? layout)
{
    List<string> nameList = [];
    if (layout?.Image_Elements is not null)
        nameList = layout.Image_Elements.Select(item => item.Name).ToList();
    if (layout?.TextBlock_Elements is not null)
        nameList = layout.TextBlock_Elements.Select(item => item.Name).ToList();

    return nameList;
}
*/
}