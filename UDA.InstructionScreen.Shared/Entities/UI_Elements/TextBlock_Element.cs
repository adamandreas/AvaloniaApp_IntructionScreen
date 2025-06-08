namespace UDA.InstructionScreen.Shared.Entities.UI_Elements;

public class TextBlock_Element : GridDetails
{
    #region Allowed to be set by the config and the "Start" request
    /// <summary>
    /// Targeted element name (You get this name from the configuration layout)
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Set from config for the default value and from the start to change
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// The element that has a higher number will be placed on top of the other elements.
    /// </summary>
    public int Z_Index { get; set; }

    public double? FontSize { get; set; }

    /// <summary>
    /// Required format "#RRGGBB"
    /// </summary>
    public string? Foreground { get; set; }

    /// <summary>
    /// Required format "#RRGGBB"
    /// </summary>
    public string? Background { get; set; }

    /// <summary>
    /// Negative values will be converted to "Auto"
    /// </summary>
    public double? Height { get; set; }

    /// <summary>
    /// Negative values will be converted to "Auto"
    /// </summary>
    public double? Width { get; set; }

    /// <summary>
    /// From 1-16 => Black, Bold, DemiBold, ExtraBlack, ExtraBold, ExtraLight, Heavy, Light, Medium, Normal, Regular, SemiBold, Thin, UltraBlack, UltraBold, UltraLight
    /// </summary>
    public int? FontWeight { get; set; }

    /// <summary>
    /// Config Setter only.
    /// </summary>
    public string? FontFamily { get; set; }

    /// <summary>
    /// From 1-4 => Left, Center, Right, Justify
    /// </summary>
    public int? TextAlignment { get; set; }

    /// <summary>
    /// From 1-4 => Left, Center, Right, Stretch
    /// </summary>
    public int? HorizontalAlignment { get; set; }

    /// <summary>
    /// From 1-4 => Bottom, Center, Top, Stretch
    /// </summary>
    public int? VerticalAlignment { get; set; }

    /// <summary>
    /// From 1-3 => NoWrap, Wrap, WrapWithOverflow
    /// </summary>
    public int? TextWrapping { get; set; }

    /// <summary>
    /// Required list length "4"
    /// </summary>
    public List<double>? Margin { get; set; }

    /// <summary>
    /// Required list length "4"
    /// </summary>
    public List<double>? Padding { get; set; }
    #endregion

    #region Allowed to be set just by the "Start" request
    /// <summary>
    /// To be used by the "Start" request only. If "false" just "Name" and "Text" properties will be used
    /// </summary>
    public bool UpdateElementProperties { get; set; }

    /// <summary>
    /// To be used by the "Start" request only. If "null" the timer will be disposed. If zero or negative value, the time of the timer will not be changed.
    /// </summary>
    public int? CountDownTimer { get; set; }
    #endregion

    #region Allowed to be set just by the config
    /// <summary>
    /// If "true", the element will not be collapsed if it was not sent from the client side
    /// </summary>
    public bool NeverCollapse { get; set; }

    /// <summary>
    /// Create a Border to wrap around the element for rounded corners and background
    /// </summary>
    public Border_Element? ElementBorder { get; set; }
    #endregion
}
