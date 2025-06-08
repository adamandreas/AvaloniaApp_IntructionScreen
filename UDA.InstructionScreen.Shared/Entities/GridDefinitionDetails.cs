namespace UDA.InstructionScreen.Shared.Entities;

public class GridDefinitionDetails
{
    public double? Dimension { get; set; }

    /// <summary>
    /// From 1-3 => Star, Auto, Pixel
    /// </summary>
    public int DimensionType { get; set; }
}
