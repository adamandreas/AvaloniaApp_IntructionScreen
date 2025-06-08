namespace UDA.UDACapabilities.Shared.Enums;
public enum DeviceNameEnum
{
    Undefined = -1,
    Simulation = 1,

    #region InstructionScreen (#1000 - #1099)
    InstructionScreen = 1000,
    #endregion

    #region SmartCardReader (#1100 - #1199)
    SmartCard_Kuwaiti = 1101,
    SmartCard_Saudi = 1102,
    SmartCard_Bahraini = 1103,
    SmartCard_Qatari = 1104,
    SmartCard_Emirati = 1105,
    SmartCard_Omani = 1106,
    #endregion

    #region DocumentReader (#1200 - #1299)
    Regula7028M = 1201,
    #endregion

    #region IrisCamera (#1300 - #1399)
    CMITechEFM_70 = 1300,
    #endregion

    #region FingerprintScanner (#1400 - #1499)
    LF10 = 1400,
    UareU4500 = 1401,
    HidGuardian = 1402,
    ZF2 = 1403,
    #endregion

    #region FaceCamera (#1500 - #1599)
    WebCam = 1501,
    CanonCamera = 1502,
    #endregion

    #region SignaturePad (#1600 - #1699)
    Topaz_TL460HSBR = 1600,
    #endregion

    #region PalmScanner (#1700 - #1799)
    #endregion

    #region Printing (#1800 - #1899)
    #endregion

    #region DocumentScanner (#1900 - #1999)
    EpsonV600 = 1900,
    A6Scanner = 1901,
    HPScanjetG4050 = 1902,
    #endregion

    #region ExtraHardware (#2000 - #2099)
    Arduino = 2000,
    #endregion

    #region Helper (#2100 - #2199)
    #endregion

    #region QrCodeReader (#2200 - #2299)
    #endregion

    #region Multi Functional Devices (#2300 - #2399)
    ArhOsmond = 2300,
    VF1 = 2301,
    Palm3 = 2302,
    ICamR100 = 2303,
    ICamD2000 = 2304,
    #endregion

    #region Cognitec FaceVACS (#2400 - 2499)
    CognitecFaceVACS = 2400
    #endregion
}
