using System;
using System.Collections.Generic;
using System.Text;
using GTA.Math;

namespace GlowingBrakes
{
    [Serializable]
    public class VehicleConfig
    {
        public VehicleConfig()
        {
            Model = "Model";
            PtfxSize = 1.375f;
            Offset = new Vector3(0.06f, 0.0f, 0.0f);
            Rotation = new Vector3(0.0f, 0.0f, 90.0f);
        }

        public string Model;
        public float PtfxSize;
        public Vector3 Offset;
        public Vector3 Rotation;
    }
}
