using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDA.UDACapabilities.Shared.Configuration
{
    public class CognitecFaceVACSConfig
    {
        public string? FaceVACSConfigPath { get; set; }

        public int FaceVACSCropWidth { get; set; } = 640;

        public int FaceVACSCropHeight { get; set; } = 800;
    }
}
