namespace UDA.InstructionScreen.Shared.Entities;
public class LayoutConfig
{
    #region Header Properties
    public Layout? HeaderLayout { get; set; }
    public GridDefinitionDetails? HeaderRowGridDefinition { get; set; }
    #endregion

    #region Body Properties
    public Layout? BodyLayout { get; set; }
    public GridDefinitionDetails? BodyRowGridDefinition { get; set; }
    #endregion

    #region Footer Properties
    public Layout? FooterLayout { get; set; }
    public GridDefinitionDetails? FooterRowGridDefinition { get; set; }
    #endregion
}
