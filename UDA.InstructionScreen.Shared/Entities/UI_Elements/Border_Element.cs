namespace UDA.InstructionScreen.Shared.Entities.UI_Elements;

public class Border_Element : GridDetails
{
    #region Allowed to be set by the config and the "Start" request
    /// <summary>
    /// Corners radius
    /// </summary>
    public double CornerRadius { get; set; }
    #endregion

    #region Allowed to be set just by the "Start" request
    /// <summary>
    /// To be used by the "Start" request only. If "false" just "Name" and "Text" properties will be used
    /// </summary>
    public bool UpdateElementProperties { get; set; }
    #endregion

    #region Allowed to be set just by the config
    /// <summary>
    /// If "true", the element will not be collapsed if it was not sent from the client side
    /// </summary>
    public bool NeverCollapse { get; set; }
    #endregion
}
