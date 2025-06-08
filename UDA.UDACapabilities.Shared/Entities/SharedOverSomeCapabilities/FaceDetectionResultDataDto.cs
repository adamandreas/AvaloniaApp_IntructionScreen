namespace UDA.UDACapabilities.Shared.Entities.SharedOverSomeCapabilities;
public class FaceDetectionResultDataDto
{
    public bool FaceDetected_ThresholdCrossed { get; set; }
    public bool Expression_ThresholdCrossed { get; set; }
    public bool DarkGlasses_ThresholdCrossed { get; set; }
    public bool Blink_ThresholdCrossed { get; set; } //Eyes open
    public bool MouthOpen_ThresholdCrossed { get; set; } // Mouth closed
    public bool LookingAway_ThresholdCrossed { get; set; }
    public bool RedEye_ThresholdCrossed { get; set; }
    public bool FaceDarkness_ThresholdCrossed { get; set; }
    public bool UnnaturalSkinTone_ThresholdCrossed { get; set; }
    public bool ColorsWashedOut_ThresholdCrossed { get; set; }
    public bool Pixelation_ThresholdCrossed { get; set; }
    public bool SkinReflection_ThresholdCrossed { get; set; }
    public bool GlassesReflection_ThresholdCrossed { get; set; }
    public bool Roll_ThresholdCrossed { get; set; }
    public bool Yaw_ThresholdCrossed { get; set; }
    public bool Pitch_ThresholdCrossed { get; set; }
    public bool TooClose_ThresholdCrossed { get; set; }
    public bool TooFar_ThresholdCrossed { get; set; }
    public bool TooNorth_ThresholdCrossed { get; set; }
    public bool TooSouth_ThresholdCrossed { get; set; }
    public bool TooWest_ThresholdCrossed { get; set; }
    public bool TooEast_ThresholdCrossed { get; set; }
    public bool HeavyFrame_ThresholdCrossed { get; set; }
    public bool Sharpness_ThresholdCrossed { get; set; } //IsImageSharp
    public bool Saturation_ThresholdCrossed { get; set; }
    public bool GrayscaleDensity_ThresholdCrossed { get; set; }
    public bool BackgroundUniformity_ThresholdCrossed { get; set; }
    public bool Liveness_ThresholdCrossed { get; set; }

    public double DarkGlasses_Score { get; set; }
    public double MouthOpen_Score { get; set; }
    public double LookingAway_Score { get; set; }
    public double RedEye_Score { get; set; }
    public double FaceDarkness_Score { get; set; }
    public double Pixelation_Score { get; set; }
    public double SkinReflection_Score { get; set; }
    public double GlassesReflection_Score { get; set; }
    public double Sharpness_Score { get; set; }
    public double Saturation_Score { get; set; }
    public double GrayscaleDensity_Score { get; set; }
    public double BackgroundUniformity_Score { get; set; }
    public double Liveness_Score { get; set; }

    //ICAO 
    public bool IsFaceHorizontalyCentered { get; set; }
    public bool IsWidthOfHeadValid { get; set; }
    public bool IsLengthOfHeadValid { get; set; }
    public bool IsResolutionValid { get; set; }
    public bool IsImageWidthToHeightRatioValid { get; set; }
    public bool IsFrontal { get; set; }
    public bool IsLightingUniform { get; set; }
    public bool HasGoodExposure { get; set; }
    public bool HasGoodVerticalFacePosition { get; set; }
    public bool HasGoodGrayScaleProfile { get; set; }
    public bool horizontallyCenteredFace { get; set; }
    public bool widthOfHead { get; set; }
    public bool lengthOfHead { get; set; }
    public bool Resolution { get; set; }
    public bool WidthToHeightRatio { get; set; }
    public bool Exposure { get; set; }
    public bool Frontal { get; set; }
    public bool LightingUniform { get; set; }
    public bool GrayScale { get; set; }
    public bool VerticalFacePosition { get; set; }
}

public class FaceDetectionResultEventArgs : EventArgs
{
    public FaceDetectionResultDataDto Data { get; init; }

    public FaceDetectionResultEventArgs(FaceDetectionResultDataDto data)
    {
        Data = data;
    }
}
