namespace UDA.UDACapabilities.Shared.Enums;
public enum QualityScoreTypeEnum
{
    Undefined = 0,

    Simulation = 6,

    #region Fingerprint
    Nfiq = 1,
    Nfiq2 = 2,
    Nfiq10 = 7,
    Nfiq20 = 8,
    Nfiq21 = 9,
    PalmCode = 5,
    #endregion

    #region Iris
    ICamQualityScore = 3,
    ISO29794 = 4
    #endregion
}
