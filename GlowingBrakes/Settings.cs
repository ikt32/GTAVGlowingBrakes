using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using GTA;

namespace GlowingBrakes
{
    [Serializable]
    public class Settings
    {
        // The instance
        private static readonly Settings _instance = Load();

        public bool Debug = false;
        public float DrawDistance = 50.0f;
        public List<string> IgnoredModels = new List<string>()
        {
            "dump"
        };
        public SerializableDictionary<string, uint> Offsets = 
            new SerializableDictionary<string, uint>()
        {
            {"WheelsPtrOffset", 0},
            {"WheelCountOffset", 0},
            {"WheelAngularVelocityOffset", 0},
            {"WheelSteeringAnglesOffset", 0},
            {"WheelBrakePressureOffset", 0}
        };

        public static Settings Get() {
            return _instance;
        }

        private static Settings Load()
        {
            var file = @"scripts\GlowingBrakes\settings.xml";
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                FileStream fs = new FileStream(file, FileMode.Open);
                XmlReader reader = new XmlTextReader(fs);
                if (serializer.CanDeserialize(reader))
                {
                    // why do i need to cast if it should already be able to figure out the return type
                    return (Settings) serializer.Deserialize(reader);
                }

                throw new System.IO.FileNotFoundException();
            }
            catch (System.IO.FileNotFoundException)
            {
                UI.Notify("Glowing Brakes\nCouldn't find settings.xml, writing defaults.");
                Settings settings = new Settings();
                XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                TextWriter writer = new StreamWriter(file);
                serializer.Serialize(writer, settings);
                writer.Close();
                return settings;
            }
        }

        private Settings()
        {
        }
    }
}
