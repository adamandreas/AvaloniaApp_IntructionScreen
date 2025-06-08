namespace UDA.InstructionScreen.Shared.Entities;
public class ConfigSettings
{
    public bool EnableLogging { get; set; }

    public string? VlcLibDirectory { get; set; }

    public int ScreenTimeout_ms { get; set; }

    /// <summary>
    /// Some formats Doesn't support transparency in the image. From 1-11 => Png, Wmf, Tiff, Png, MemoryBmp, Jpeg, Icon, Exif, Emf, Bmp, Gif
    /// </summary>
    public int? DisplayImageFormat { get; set; }

    /// <summary>
    /// Based on the selected "Language" property the URL will be changed. Example: "..\SamplePath\SampleImage.png" will be converted to "..\SamplePath\==>AR or ARandEN or EN<==\SampleImage.png"
    /// </summary>
    public bool RerouteToLanguageFolder { get; set; }

    /// <summary>
    /// This will be used if "RerouteToLanguageFolder" is enabled => English = 1, Arabic = 5, ArAndEn = 15
    /// </summary>
    public int DefaultLanguage { get; set; }

    /// <summary>
    /// To set the path from the config and just use the name of the image in the URL path
    /// </summary>
    public string? InstructionImagesParentPath { get; set; }

    /// <summary>
    /// The name of the json file that contains the layout design (it should be in the Instruction screen capability folder in the bin)
    /// </summary>
    public string? ActiveLayoutJsonFileName { get; set; }
}
