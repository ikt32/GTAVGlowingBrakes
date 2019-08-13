using System;
using GTA.Math;

namespace GlowingBrakes
{
    static class MathExt
    {
        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }

        public static float Lerp(float firstFloat, float secondFloat, float by)
        {
            return firstFloat * (1 - by) + secondFloat * by;
        }

        public static float Map(float x, float in_min, float in_max, float out_min, float out_max)
        {
            return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
        }

        public static float DegToRad(float angle)
        {
            return (float)(Math.PI * (double)angle / 180.0);
        }

        public static float RadToDeg(float angle)
        {
            return (float)((double)angle * 180.0 / Math.PI);
        }


        public static Vector3 GetOffsetInWorldCoords(Vector3 position, Vector3 rotation, Vector3 forward, Vector3 offset)
        {
            const float deg2Rad = (float)0.01745329251994329576923690768489;
            float num1 = (float)System.Math.Cos(rotation.Y * deg2Rad);
            float x = num1 * (float)System.Math.Cos(-rotation.Z * deg2Rad);
            float y = num1 * (float)System.Math.Sin(rotation.Z * deg2Rad);
            float z = (float)System.Math.Sin(-rotation.Y * deg2Rad);
            Vector3 right = new Vector3((float)x, (float)y, (float)z);
            Vector3 up = Vector3.Cross(right, forward);
            return position + (right * offset.X) + (forward * offset.Y) + (up * offset.Z);
        }
    }
}
