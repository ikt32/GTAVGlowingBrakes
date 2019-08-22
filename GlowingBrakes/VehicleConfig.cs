using System;
using System.Collections.Generic;
using System.Text;
using GTA.Math;

namespace GlowingBrakes
{
    [Serializable]
    public class VehicleConfig
    {
        public enum DrawModes
        {
            Ptfx,
            Marker
        }

        public VehicleConfig()
        {
            Model = "Model";
            DrawMode = DrawModes.Ptfx;
            PtfxSize = 1.375f;
            MarkerSizeIn = 0.22f;
            MarkerSizeOut = 0.31f;
            Offset = new Vector3(0.06f, 0.0f, 0.0f);
            Rotation = new Vector3(0.0f, 0.0f, 90.0f);
            HeatRate = 0.25f;
            CoolRateMoving = 0.2f;
            CoolRateStopped = 0.05f;
            AccelerationMult = 0.045f;
        }

        public string Model;
        public DrawModes DrawMode;
        public float PtfxSize;
        public float MarkerSizeIn;
        public float MarkerSizeOut;
        public Vector3 Offset;
        public Vector3 Rotation;
        public float HeatRate;
        public float CoolRateMoving;
        public float CoolRateStopped;
        public float AccelerationMult;
    }
}
