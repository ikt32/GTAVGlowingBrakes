using System;
using System.Collections.Generic;
using System.Text;
using GTA.Math;

namespace GlowingBrakes
{
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Xml;
    using System.Xml.Serialization;

    [Serializable]
    public class VehicleConfig
    {
        private static VehicleConfig loadedDefaultConfig = null;

        public static void SetDefaultConfigFromFile()
        {
            string file = @"scripts\GlowingBrakes\Configs\defaultConfig.xml";
            if (File.Exists(file))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(VehicleConfig));
                try
                {
                    FileStream fs = new FileStream(file, FileMode.Open);
                    XmlReader reader = new XmlTextReader(fs);
                    if (serializer.CanDeserialize(reader))
                    {
                        // why do i need to cast if it should already be able to figure out the return type
                        loadedDefaultConfig = (VehicleConfig)serializer.Deserialize(reader);
                        Logger.Log(Logger.Level.INFO, "Using custom defaultConfig.xml");
                    }
                    reader.Close();
                    fs.Close();
                }
                catch (Exception e)
                {
                    Logger.Log(Logger.Level.ERROR, $"{file} Read error: {e.Message}");
                    Logger.Log(Logger.Level.WARN, "Using hardcoded defaults");
                }
            }
            else
            {
                Logger.Log(Logger.Level.INFO, "No defaultConfig.xml found - using hardcoded defaults");
                Logger.Log(Logger.Level.INFO, "Default config written to defaultConfig.xml, check it out!");
                Utility.WriteDefaultsFile();
            }
        }

        public enum DrawModes
        {
            Ptfx,
            Marker
        }

        public VehicleConfig()
        {
            if (loadedDefaultConfig != null)
            {
                Model = loadedDefaultConfig.Model;
                DrawMode = loadedDefaultConfig.DrawMode;

                PtfxSize = loadedDefaultConfig.PtfxSize;
                PtfxRearSize = loadedDefaultConfig.PtfxRearSize;
                PtfxRearSizeOverride = loadedDefaultConfig.PtfxRearSizeOverride;

                MarkerSizeIn = loadedDefaultConfig.MarkerSizeIn;
                MarkerSizeOut = loadedDefaultConfig.MarkerSizeOut;
                
                Offset = loadedDefaultConfig.Offset;
                OffsetRear = loadedDefaultConfig.OffsetRear;
                OffsetRearOverride = loadedDefaultConfig.OffsetRearOverride;

                Rotation = loadedDefaultConfig.Rotation;
                HeatRate = loadedDefaultConfig.HeatRate;
                CoolRateMoving = loadedDefaultConfig.CoolRateMoving;
                CoolRateStopped = loadedDefaultConfig.CoolRateStopped;
                AccelerationMult = loadedDefaultConfig.AccelerationMult;
                Visible = new List<bool>(loadedDefaultConfig.Visible);
            }
            else
            {
                Model = "Model";
                DrawMode = DrawModes.Ptfx;

                PtfxSize = 1.375f;
                PtfxRearSize = 1.375f;
                PtfxRearSizeOverride = false;

                MarkerSizeIn = 0.22f;
                MarkerSizeOut = 0.31f;
                
                Offset = new Vector3(0.06f, 0.0f, 0.0f);
                OffsetRear = new Vector3(0.06f, 0.0f, 0.0f);
                OffsetRearOverride = false;

                Rotation = new Vector3(0.0f, 0.0f, 90.0f);
                HeatRate = 0.25f;
                CoolRateMoving = 0.2f;
                CoolRateStopped = 0.05f;
                AccelerationMult = 0.045f;
                Visible = new List<bool>() { true, true, true, true };
            }

        }

        public string Model;
        public DrawModes DrawMode;

        public float PtfxSize;
        public float PtfxRearSize;
        public bool PtfxRearSizeOverride;

        public float MarkerSizeIn;
        public float MarkerSizeOut;
        
        public Vector3 Offset;
        public Vector3 OffsetRear;
        public bool OffsetRearOverride;

        public Vector3 Rotation;
        public float HeatRate;
        public float CoolRateMoving;
        public float CoolRateStopped;
        public float AccelerationMult;

        // Because XmlSerializer was written while MSFT devs were doing crack
        // https://stackoverflow.com/questions/13046474/xml-deserialization-appends-to-list
        [XmlIgnore]
        public List<bool> Visible;

        [XmlArray(ElementName = "Visible")]
        public bool[] Dummy
        {
            get
            {
                return Visible.ToArray();
            }
            set
            {
                if (value != null && value.Length > 0) Visible = new List<bool>(value);
            }
        }
    }
}
