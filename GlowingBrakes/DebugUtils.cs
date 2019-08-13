using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using GTA;
using GTA.Math;
using GTA.Native;

namespace GlowingBrakes
{
    static class DebugUtils
    {
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
            Function.Call(Hash._SET_TEXT_ENTRY, "STRING");
            Function.Call(Hash._ADD_TEXT_COMPONENT_STRING, text);
            Function.Call(Hash._DRAW_TEXT, x, y);
        }
    }
}
