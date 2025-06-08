namespace UDA.InstructionScreen.Shared.Entities.UI_Elements;

public class VlcControl_Element : GridDetails
{
    #region Allowed to be set by the config and the "Start" request
    /// <summary>
    /// Targeted element name (You get this name from the configuration layout)
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The element that has a higher number will be placed on top of the other elements.
    /// </summary>
    public int Z_Index { get; set; }

    /// <summary>
    /// This is to match the Height of the Default Config Img with another image, set the name of the Image that you want to get the Height from. (The height will be taken from Image element)
    /// </summary>
    public string? MatchHeightWithOtherElementName { get; set; }

    /// <summary>
    /// This is to match the Width of the Default Config Img with another image, set the name of the Image that you want to get the Width from. (The width will be taken from Image element)
    /// </summary>
    public string? MatchWidthWithOtherElementName { get; set; }

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
    #endregion

    #region Allowed to be set just by the "Start" request
    /// <summary>
    /// The RTSP URL, set it to null to close the stream connection.
    /// </summary>
    public string? RTSP_URL { get; set; }

    /// <summary>
    /// If "false" just "Name" and "ImgUrlOrBase64" properties will be used
    /// </summary>
    public bool UpdateElementProperties { get; set; }
    #endregion

    #region Allowed to be set just by the config
    public List<string>? StreamingOptions { get; set; }

    /// <summary>
    /// If "true", the element will not be collapsed if it was not sent from the client side
    /// </summary>
    public bool NeverCollapse { get; set; }
    #endregion
}
