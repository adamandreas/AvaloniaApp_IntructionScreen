namespace UDA.InstructionScreen.Shared.Entities.UI_Elements;

public class Media_Element : GridDetails
{
    #region Allowed to be set by the config and the "Start" request
    /// <summary>
    /// Targeted element name (Should be initialized from the configuration layout)
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The element that has a higher number will be placed on top of the other elements.
    /// </summary>
    public int Z_Index { get; set; }

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
    /// If "false" just "Name" and "SourceURL" properties will be used.
    /// </summary>
    public bool UpdateElementProperties { get; set; }

    /// <summary>
    /// The video URL. Supported Videos formats => MP4, AVI, WMV, MPEG, ASF, 3GP
    /// </summary>
    public string? SourceURL { get; set; }

    /// <summary>
    /// If this is true, the "SourceURL" will be considered as a directory for the videos not a single file path
    /// </summary>
    public bool PlayAllVideosInDirectory { get; set; }

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
    public bool ReleaseMemoryOnPause { get; set; }

    /// <summary>
    /// This will stop the video and on resume, it will start the video from the beginning.
    /// </summary>
    public bool StopNotPauseOnCollapse { get; set; }

    /// <summary>
    /// If "true", the element will not be collapsed if it was not sent from the client side
    /// </summary>
    public bool NeverCollapse { get; set; }
    #endregion
}
