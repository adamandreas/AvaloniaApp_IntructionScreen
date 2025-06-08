namespace UDA.InstructionScreen.Shared.Entities;
public class CustomClient_StartupSettings
{
    /// <summary>
    /// This is for the face camera mode where we start the live stream with certain settings for some time then start the live stream with different settings
    /// </summary>
    public bool IsTwoStepLiveStream { get; set; }

    public bool UseDifferentSettingsForFirstFrame { get; set; }

    public int SecondsBeforeStartingSecondStream { get; set; }

    public StartupSettings? FirstStream_StartupSettings { get; set; }
    public StartupSettings? FirstStream_FirstFrame_StartupSettings { get; set; }
    public StartupSettings? SecondStream_StartupSettings { get; set; }
    public StartupSettings? SecondStream_FirstFrame_StartupSettings { get; set; }


    public CustomClient_StartupSettings Clone() => (CustomClient_StartupSettings)MemberwiseClone();
}
