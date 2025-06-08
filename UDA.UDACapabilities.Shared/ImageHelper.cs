using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using UDA.UDACapabilities.Shared.Enums;

namespace UDA.UDACapabilities.Shared;

[SupportedOSPlatform("windows")]
public static class ImageHelper
{
    private static readonly string _emptyImage = "iVBORw0KGgoAAAANSUhEUgAAACgAAAAoCAIAAAADnC86AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAySURBVFhH7c0xAQAwEAOh+jeduuCXwwBvR4qZYqaYKWaKmWKmmClmiplippgpZoqR7QMs0K5PppfCWAAAAABJRU5ErkJggg==";

    private static readonly ImageCodecInfo _jpegEncoder = ImageCodecInfo.GetImageEncoders()
        .FirstOrDefault(codec => codec.FormatID == ImageFormat.Jpeg.Guid)
        ?? throw new InvalidOperationException("JPEG encoder not found");

    private static EncoderParameters? _cachedEncoderParameters;
    private static long _cachedQuality = -1;

    public static ImageFormat? ImgFormatEnum_To_ImageFormat(ImgFormatEnum imgFormat)
    {
        return imgFormat switch
        {
            //ImgFormatEnum.Jpg => ImageFormat.Jpg,
            ImgFormatEnum.Jpeg => ImageFormat.Jpeg,
            //ImgFormatEnum.Jpeg2000 => ImageFormat.Jpeg2000,
            ImgFormatEnum.Png => ImageFormat.Png,
            ImgFormatEnum.Bmp => ImageFormat.Bmp,
            ImgFormatEnum.Gif => ImageFormat.Gif,
            _ => null
        };
    }

    public static string BytesToPngBase64(byte[] imageBytes)
    {
        return BitmapToPngBase64(BytesToBitmap(imageBytes));
    }

    #region Change Bitmap DPI
    public static Bitmap ChangeDpi(Bitmap bitmap, int dpi_X, int dpi_Y)
    {
        try
        {
            bitmap.SetResolution(dpi_X, dpi_Y);
            return bitmap;
        }
        catch (Exception)
        {
            return bitmap;
        }
    }

    public static Bitmap ChangeDpi(byte[] imageBytes, int dpi_X, int dpi_Y)
    {
        try
        {
            Bitmap bitmap = BytesToBitmap(imageBytes);
            bitmap.SetResolution(dpi_X, dpi_Y);
            return bitmap;
        }
        catch (Exception)
        {
            return new(40, 40);
        }
    }

    public static Bitmap ChangeDpi(string imageBase64, int dpi_X, int dpi_Y)
    {
        try
        {
            Bitmap bitmap = BytesToBitmap(Convert.FromBase64String(imageBase64));
            bitmap.SetResolution(dpi_X, dpi_Y);
            return bitmap;
        }
        catch (Exception)
        {
            return new(40, 40);
        }
    }
    #endregion

    public static Bitmap BytesToBitmap(byte[] imageData)
    {
        try
        {
            MemoryStream inStream = new();
            inStream.Write(imageData, 0, imageData.Length);
            inStream.Flush();
            Bitmap bitmap = new(inStream);
            return bitmap;
        }
        catch (Exception)
        {
            return new(40, 40);
        }
    }

    public static (float width, float height) GetBoundingRotatedBox(float width, float height, float angle)
    {
        if (angle is 0)
            return (width, height);
        angle = (float)(angle * Math.PI / 180);
        var absCos = Math.Abs(Math.Cos(angle));
        var absSin = Math.Abs(Math.Sin(angle));
        var w = width * absCos + height * absSin;
        var h = width * absSin + height * absCos;
        return ((float)w, (float)h);
    }

    public static Bitmap CropBitmap(Bitmap bitmap, float x, float y, float width, float height)
    {
        Bitmap croppedBitmap = new((int)width, (int)height);
        croppedBitmap.SetResolution(bitmap.HorizontalResolution, bitmap.VerticalResolution);
        using Graphics gfx = Graphics.FromImage(croppedBitmap);
        gfx.DrawImage(bitmap, new Rectangle(0, 0, (int)width, (int)height),
                    x, y, width, height, GraphicsUnit.Pixel);
        return croppedBitmap;
    }

    public static Bitmap ReducePixels(Bitmap bitmap, int width, int height)
    {
        return new Bitmap(bitmap, new Size(width, height));
    }

    public static Bitmap RotateBitmap(Bitmap inputBitmap, float angle)
    {
        if (angle is 0)
            return inputBitmap;
        Bitmap newBitmap = new(inputBitmap.Width, inputBitmap.Height);
        newBitmap.SetResolution(inputBitmap.HorizontalResolution, inputBitmap.VerticalResolution);
        using Graphics gfx = Graphics.FromImage(newBitmap);
        gfx.Clear(Color.White);
        gfx.TranslateTransform(inputBitmap.Width / 2f, inputBitmap.Height / 2f);
        gfx.RotateTransform(-angle);
        gfx.ScaleTransform(1f, 1f);
        gfx.DrawImage(inputBitmap, -inputBitmap.Width / 2f, -inputBitmap.Height / 2f);
        return newBitmap;
    }

    public static Bitmap MakeGrayscale3(Bitmap bmp)
    {
        var result = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format8bppIndexed);
        result.SetResolution(bmp.HorizontalResolution, bmp.VerticalResolution);
        var resultPalette = result.Palette;

        for (int i = 0; i < 256; i++)
        {
            resultPalette.Entries[i] = Color.FromArgb(255, i, i, i);
        }

        result.Palette = resultPalette;

        BitmapData data = result.LockBits(new Rectangle(0, 0, result.Width, result.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

        //Copy the bytes from the bitmap into a byte array
        byte[] bytes = new byte[data.Height * data.Stride];
        Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);

        for (int y = 0; y < bmp.Height; y++)
        {
            for (int x = 0; x < bmp.Width; x++)
            {
                var c = bmp.GetPixel(x, y);
                var rgb = (byte)((c.R + c.G + c.B) / 3);

                bytes[y * data.Stride + x] = rgb;
            }
        }

        //Copy the bytes from the byte array into the bitmap
        Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);

        result.UnlockBits(data);

        return result;
    }

    #region Bitmap to byte[]
    public static byte[] BitmapToPngBytes(Bitmap bitmap)
    {
        try
        {
            using MemoryStream ms = new();
            bitmap.Save(ms, ImageFormat.Png);
            ms.Flush();
            return ms.ToArray();
        }
        catch (Exception)
        {
            return Convert.FromBase64String(_emptyImage);
        }
    }

    public static byte[] BitmapToBmpBytes(Bitmap bitmap)
    {
        try
        {
            using MemoryStream ms = new();
            bitmap.Save(ms, ImageFormat.Bmp);
            ms.Flush();
            return ms.ToArray();
        }
        catch (Exception)
        {
            return Convert.FromBase64String(_emptyImage);
        }
    }

    public static byte[] BitmapToJpegBytes(Bitmap bitmap)
    {
        try
        {
            using MemoryStream ms = new();
            bitmap.Save(ms, ImageFormat.Jpeg);
            ms.Flush();
            return ms.ToArray();
        }
        catch (Exception)
        {
            return Convert.FromBase64String(_emptyImage);
        }
    }
    #endregion

    #region Bitmap to base64
    public static string BitmapToPngBase64(Bitmap bitmap)
    {
        try
        {
            using var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png);
            ms.Flush();
            return Convert.ToBase64String(ms.ToArray());
        }
        catch (Exception)
        {
            return _emptyImage;
        }
    }

    public static string BitmapToBmpBase64(Bitmap bitmap)
    {
        try
        {
            using var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Bmp);
            ms.Flush();
            return Convert.ToBase64String(ms.ToArray());
        }
        catch (Exception)
        {
            return _emptyImage;
        }
    }

    public static string BitmapToJpegBase64(Bitmap img)
    {
        try
        {
            using MemoryStream ms = new();
            img.Save(ms, ImageFormat.Jpeg);
            ms.Flush();
            return Convert.ToBase64String(ms.ToArray());
        }
        catch (Exception)
        {
            return _emptyImage;
        }
    }
    #endregion

    public static Bitmap CropRectangle(Bitmap source, float x, float y, float width, float height)
    {
        RectangleF crop = new(x, y, width, height);

        Bitmap cropped = new((int)crop.Width, (int)crop.Height);
        using var gr = Graphics.FromImage(cropped);
        gr.DrawImage(source, new RectangleF(0, 0, width, height), crop, GraphicsUnit.Pixel);
        return cropped;
    }

    public static Bitmap AutoCropWhiteBorder(Bitmap bitmap, int margin, int tolerance)
    {
        try
        {
            int bitmapWidth = bitmap.Width;
            int bitmapHeight = bitmap.Height;

            var valueToCheckAgainst = 255 * (100 - tolerance) / 100;

            int leftMost = 0;
            int rightMost = bitmapWidth - 1;
            int topMost = 0;
            int bottomMost = bitmapHeight - 1;

            bool IsAllWhiteColumn(int col)
            {
                for (int i = bottomMost; i > 0; i -= 2)
                {
                    Color color = bitmap.GetPixel(col, i);
                    var av = (color.R + color.G + color.B) / 3;
                    if (av < valueToCheckAgainst)
                    {
                        return false;
                    }
                }
                return true;
            };

            bool IsAllWhiteRow(int row)
            {
                for (int i = leftMost; i < rightMost; i += 2)
                {
                    Color c = bitmap.GetPixel(i, row);
                    var av = (c.R + c.G + c.B) / 3;
                    if (av < valueToCheckAgainst)
                    {
                        return false;
                    }
                }
                return true;
            };

            //Iterate through all columns, find leftMost
            for (int col = 0; col < bitmapWidth; col++)
            {
                if (IsAllWhiteColumn(col))
                    leftMost = col + 1;
                else
                    break;
            }

            //Bottom to top every other row, find bottomMost
            for (int row = bottomMost; row > 0; row -= 2)
            {
                if (IsAllWhiteRow(row))
                    bottomMost = row - 1;
                else
                    break;
            }

            //Right to left every other column, find rightmost
            for (int col = rightMost; col > 0; col -= 2)
            {
                if (IsAllWhiteColumn(col))
                    rightMost = col - 1;
                else
                    break;
            }

            //Iterate through every other row, find topMost
            for (int row = 0; row < bitmapHeight; row += 2)
            {
                if (IsAllWhiteRow(row))
                    topMost = row + 1;
                else
                    break;
            }

            if (rightMost == 0 && bottomMost == 0 && leftMost == bitmapWidth && topMost == bitmapHeight)
                return bitmap;

            leftMost = Math.Max(0, leftMost - margin);
            topMost = Math.Max(0, topMost - margin);
            rightMost = Math.Min(bitmapWidth - 1, rightMost + margin);
            bottomMost = Math.Min(bitmapHeight - 1, bottomMost + margin);

            int croppedWidth = rightMost - leftMost + 1;
            int croppedHeight = bottomMost - topMost + 1;

            try
            {
                if (croppedWidth < 0 || croppedHeight < 0)
                    return bitmap;

                Bitmap target = new(croppedWidth, croppedHeight);
                using (Graphics g = Graphics.FromImage(target))
                {
                    g.DrawImage(bitmap,
                        new RectangleF(0, 0, croppedWidth, croppedHeight),
                        new RectangleF(leftMost, topMost, croppedWidth, croppedHeight),
                        GraphicsUnit.Pixel);
                }
                return target;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format($"Values are top={topMost} bottom={bottomMost} left={leftMost} right={rightMost}"), ex);
            }
        }
        catch
        {
            return bitmap;
        }
    }

    public static Bitmap Base64ToBitmap(string base64)
    {
        try
        {
            MemoryStream ms = new(Convert.FromBase64String(base64));
            Bitmap bitmap = (Bitmap)Image.FromStream(ms);
            return bitmap;
        }
        catch (Exception)
        {
            return new(40, 40);
        }
    }

    public static Bitmap Url_To_Bitmap(string? url)
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

    public static Bitmap ResizeImage(Bitmap bitmapToResize, Size size)
    {
        return new Bitmap(bitmapToResize, size);
    }

    public static Bitmap ScaleImage(Bitmap bitmapToScale, double scaleFactor)
    {
        try
        {
            return new Bitmap(bitmapToScale, new Size((int)((double)bitmapToScale.Width * scaleFactor), (int)((double)bitmapToScale.Height * scaleFactor)));
        }
        catch (Exception)
        {
            return bitmapToScale;
        }
    }

    public static Bitmap MergeTwoBitmaps(Bitmap mainBitmap, Bitmap innerBitmap, int imgMergeCenterPosition_X, int imgMergeCenterPosition_Y, int imgMaxMerge_W, int imgMaxMerge_H)
    {
        if (mainBitmap is not null && innerBitmap is not null)
        {
            double scaleFactor = CalculateScaleFactor(innerBitmap, imgMaxMerge_W, imgMaxMerge_H);

            if (scaleFactor != 1)
                innerBitmap = ImageHelper.ScaleImage(innerBitmap, scaleFactor);

            try
            {
                Bitmap bitmap = new(mainBitmap.Width, mainBitmap.Height); //Size of the large Image
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.DrawImage(mainBitmap, 0, 0);
                    g.DrawImage(innerBitmap, imgMergeCenterPosition_X - (innerBitmap.Width / 2), imgMergeCenterPosition_Y - (innerBitmap.Height / 2));
                }
                return bitmap;
            }
            catch (Exception)
            {
            }
        }

        return new(40, 40);
    }

    /// <summary>
    /// Calculates the scaling factor to make the image size same as required width or height without losing the aspect ratio
    /// </summary>
    /// <param name="targetedImg"></param>
    /// <param name="imgMaxMerge_W"></param>
    /// <param name="imgMaxMerge_H"></param>
    /// <returns></returns>
    public static double CalculateScaleFactor(Image targetedImg, int imgMaxMerge_W, int imgMaxMerge_H)
    {
        int imgWidth = targetedImg.Width;
        int imgHeight = targetedImg.Height;
        double scaleFactor_Result = 1;

        double heightScaleFactor = (double)imgMaxMerge_H / (double)imgHeight;
        double widthScaleFactor = (double)imgMaxMerge_W / (double)imgWidth;

        double scaledWidth = heightScaleFactor * imgWidth;
        double scaledHeight = widthScaleFactor * imgHeight;

        if (scaledWidth <= imgMaxMerge_W)
            scaleFactor_Result = heightScaleFactor;
        else if (scaledHeight <= imgMaxMerge_H)
            scaleFactor_Result = widthScaleFactor;

        return scaleFactor_Result;
    }

    public static string MakeFrameAroundBase64(string base64, int frameThickness, Color frameColor, bool changeImageSizeBasedOnFrame, ImageFormat outputFormat)
    {
        using MemoryStream originalMs = new(Convert.FromBase64String(base64));
        using Bitmap originalImage = (Bitmap)Image.FromStream(originalMs);

        int newWidth = changeImageSizeBasedOnFrame ? originalImage.Width + (2 * frameThickness) : originalImage.Width;
        int newHeight = changeImageSizeBasedOnFrame ? originalImage.Height + (2 * frameThickness) : originalImage.Height;

        using Bitmap resizedBitmap = new(newWidth, newHeight);
        using Graphics resizedGraphics = Graphics.FromImage(resizedBitmap);
        using Pen pen = new(frameColor, frameThickness);

        int imgOffset = changeImageSizeBasedOnFrame ? frameThickness : 0;

        resizedGraphics.FillRectangle(new SolidBrush(frameColor), 0, 0, resizedBitmap.Width, resizedBitmap.Height);
        resizedGraphics.DrawImage(originalImage, imgOffset, imgOffset, originalImage.Width, originalImage.Height);

        using MemoryStream outputMs = new();
        resizedBitmap.Save(outputMs, outputFormat);

        return Convert.ToBase64String(outputMs.ToArray());
    }

    public static bool IsBase64StringValidImage(string base64)
    {
        try
        {
            using MemoryStream stream = new(Convert.FromBase64String(base64));
            using Bitmap bitmap = new(stream); //If the Bitmap constructor does not throw an exception, it's a valid image.

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static string CompressBitmapToJpegBase64(Bitmap? inputBitmap, int quality)
    { 
        if (inputBitmap == null) throw new ArgumentNullException();

        if (quality == -1) quality = 100;
        quality = Math.Clamp(quality, 1, 100);

        if (_cachedEncoderParameters == null || _cachedQuality != quality)
        {
            _cachedEncoderParameters = new EncoderParameters(1)
            {
                Param = { [0] = new EncoderParameter(Encoder.Quality, quality) }
            };
            _cachedQuality = quality;
        }

        using MemoryStream ms = new();
        inputBitmap.Save(ms, _jpegEncoder, _cachedEncoderParameters);
        return Convert.ToBase64String(ms.ToArray());
    }

}
