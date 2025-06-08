namespace UDA.InstructionScreen.Shared.Entities.UI_Elements;

public class GIF_Element : GridDetails
{
    #region Allowed to be set by the config and the "Start" request
    /// <summary>
    /// Targeted element name (Should be initialized from the configuration layout)
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Required list length "4"
    /// </summary>
    public List<double>? Margin { get; set; }

    /// <summary>
    /// Negative values will be converted to "Auto"
    /// </summary>
    public double? Height { get; set; }

    /// <summary>
    /// Negative values will be converted to "Auto"
    /// </summary>
    public double? Width { get; set; }

    /// <summary>
    /// From 1-4 => Left, Center, Right, Stretch
    /// </summary>
    public int? HorizontalAlignment { get; set; }

    /// <summary>
    /// From 1-4 => Bottom, Center, Top, Stretch
    /// </summary>
    public int? VerticalAlignment { get; set; }

    /// <summary>
    /// From 1-4 => Fill, None, Uniform, UniformToFill
    /// </summary>
    public int? Stretch { get; set; }

    /// <summary>
    /// If the "InstructionImagesParentPath" is defined in config(if not null), you have to write just the image name not the full path.
    /// </summary>
    public string? GifUrl { get; set; }
    #endregion

    #region Allowed to be set just by the "Start" request
    /// <summary>
    /// If "false" just "Name" and "ImgUrlOrBase64" properties will be used.
    /// </summary>
    public bool UpdateElementProperties { get; set; }

    /// <summary>
    /// This is to match the Height of the Default Config Img with another image, set the name of the Image that you want to get the Height from. (The height will be taken from Image element)
    /// </summary>
    public string? MatchHeightWithOtherElementName { get; set; }

    /// <summary>
    /// This is to match the Width of the Default Config Img with another image, set the name of the Image that you want to get the Width from. (The width will be taken from Image element)
    /// </summary>
    public string? MatchWidthWithOtherElementName { get; set; }
    #endregion

    #region Allowed to be set just by the config
    /// <summary>
    /// If "true", the element will not be collapsed if it was not sent from the client side
    /// </summary>
    public bool NeverCollapse { get; set; }
    #endregion
}
