using System;
using System.Diagnostics;
using GTA;
using GTA.Math;
using GTA.Native;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace GlowingBrakes
{
    static class Utility
    {
        public static string Version
        {
            get
            {
                Assembly asm = Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);
                return $"{fvi.ProductMajorPart}.{fvi.ProductMinorPart}.{fvi.ProductBuildPart}";
            }
        }

        public static void WriteDefaultsFile()
        {
            Logger.Log(Logger.Level.INFO, "Writing a default config.");
            VehicleConfig cfg = new VehicleConfig();
            XmlSerializer serializer = new XmlSerializer(typeof(VehicleConfig));
            TextWriter writer = new StreamWriter(@"scripts\GlowingBrakes\Configs\defaultConfig.xml");
            serializer.Serialize(writer, cfg);
            writer.Close();
        }

        public static void DrawMarker(MarkerType marker, Vector3 pos, Vector3 dir, Vector3 rot, float scale, Color color)
        {
            World.DrawMarker(marker, pos, dir, rot, new Vector3(scale, scale, scale), color);
        }

        public static void ShowText(float x, float y, string text, float size = 0.2f)
        {
            Function.Call(Hash.SET_TEXT_FONT, 0);
            Function.Call(Hash.SET_TEXT_SCALE, size, size);
            Function.Call(Hash.SET_TEXT_COLOUR, 255, 0, 0, 255);
            Function.Call(Hash.SET_TEXT_WRAP, 0.0, 1.0);
            Function.Call(Hash.SET_TEXT_CENTRE, 0);
            Function.Call(Hash.SET_TEXT_OUTLINE, true);
            Function.Call(Hash.BEGIN_TEXT_COMMAND_DISPLAY_TEXT, "CELL_EMAIL_BCON");
            Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, text);
            Function.Call(Hash.END_TEXT_COMMAND_DISPLAY_TEXT, x, y);
        }
    }
}
