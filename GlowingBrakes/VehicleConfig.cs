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
            PtfxSize = 1.375f;
            Offset = new Vector3(0.06f, 0.0f, 0.0f);
            Rotation = new Vector3(0.0f, 0.0f, 90.0f);
        }

        [System.Xml.Serialization.XmlElement("Model")]
        public String Model;

        [System.Xml.Serialization.XmlElement("PtfxSize")]
        public float PtfxSize;

        [System.Xml.Serialization.XmlElement("Offset")]
        public Vector3 Offset;

        [System.Xml.Serialization.XmlElement("Rotation")]
        public Vector3 Rotation;
    }
}
