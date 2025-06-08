namespace UDA.InstructionScreen.Shared.Entities;
public class GridDetails
{
    /// <summary>
    /// Config Setter only
    /// </summary>
    public int RowIndex { get; set; }

    /// <summary>
    /// Config Setter only
    /// </summary>
    public int ColumnIndex { get; set; }

    /// <summary>
    /// Config Setter only
    /// </summary>
    public GridDefinitionDetails? RowGridDefinition { get; set; }

    /// <summary>
    /// Config Setter only
    /// </summary>
    public GridDefinitionDetails? ColumnGridDefinition { get; set; }
}
