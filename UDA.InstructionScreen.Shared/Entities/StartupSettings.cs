using UDA.UDACapabilities.Shared.Enums;

namespace UDA.InstructionScreen.Shared.Entities;
/// <summary>
/// Just set the properties that you want to override
/// </summary>
public class StartupSettings
{
    public bool ForceToOverrideCurrentScreen { get; set; }

    /// <summary>
    /// English = 1, Arabic = 5, ArAndEn = 15
    /// </summary>
    public Language? Language { get; set; }

    public Layout? HeaderLayout { get; set; }
    public Layout? BodyLayout { get; set; }
    public Layout? FooterLayout { get; set; }


    public StartupSettings Clone() => (StartupSettings)MemberwiseClone();
}
