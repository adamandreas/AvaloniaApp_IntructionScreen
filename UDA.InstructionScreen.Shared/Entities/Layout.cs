using UDA.InstructionScreen.Shared.Entities.UI_Elements;

namespace UDA.InstructionScreen.Shared.Entities;

public class Layout
{
    public bool ShowThisLayout { get; set; }

    /// <summary>
    /// To be used by the "Start" request only. If "false" just the properties of the UI elements will be used.
    /// </summary>
    public bool FixElementProperties { get; set; }

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
    /// Required format "#RRGGBB"
    /// </summary>
    public string? Background_Color { get; set; }

    public int ColumnDefinitions_Count { get; set; }

    public int RowDefinitions_Count { get; set; }

    /// <summary>
    /// - 0 (Unspecified): Default setting, renders edges with smooth lines (anti-aliasing). Better for visual quality. Slower processing.
    /// - 1 (Aliased): Renders edges with sharp, jagged lines (no anti-aliasing). May improve processing speed for simple shapes. Fast processing.
    /// </summary>
    public int? RenderingMode { get; set; }

    public List<Image_Element>? Image_Elements { get; set; }
    public List<GIF_Element>? GIF_Elements { get; set; }
    public List<TextBlock_Element>? TextBlock_Elements { get; set; }
    public List<VlcControl_Element>? VlcControl_Elements { get; set; }
    public List<Media_Element>? Media_Elements { get; set; }
}
