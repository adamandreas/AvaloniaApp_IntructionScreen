using System.Runtime.Versioning;
using UDA.UDACapabilities.Shared.Constants;
using UDA.UDACapabilities.Shared.Enums;

namespace UDA.UDACapabilities.Shared.Entities;

[SupportedOSPlatform("windows")]
public class UdaImage
{
    public required string ImageBase64 { get; set; }
    public required ImgFormatEnum ImageFormat { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public int Width { get { GetImageDetails(); return _width; } }
    private int _width;

    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public int Height { get { GetImageDetails(); return _height; } }
    private int _height;

    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public double DpiX { get { GetImageDetails(); return _dpiX; } }
    private double _dpiX;

    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public double DpiY { get { GetImageDetails(); return _dpiY; } }
    private double _dpiY;

    private bool _imageProcessed = false;
    private void GetImageDetails()
    {
        if (_imageProcessed)
            return;

        using System.Drawing.Bitmap? image = ImageHelper.Base64ToBitmap(ImageBase64);
        _dpiX = Math.Round(image.HorizontalResolution, GeneralConstants.DPI_ROUND_VALUE);
        _dpiY = Math.Round(image.VerticalResolution, GeneralConstants.DPI_ROUND_VALUE);
        _width = image.Width;
        _height = image.Height;

        _imageProcessed = true;
    }
}
