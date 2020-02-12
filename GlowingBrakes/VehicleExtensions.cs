using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;
using GTA;

namespace GlowingBrakes
{
    static class VehicleExtensions
    {
        private static SerializableDictionary<string, uint> Offsets = 
            new SerializableDictionary<string, uint>();

        public static unsafe ulong GetWheelsPtr(this Vehicle handle)
        {
            var address = (ulong)handle.MemoryAddress;
            return *((ulong*)(address + Offsets["WheelsPtrOffset"]));
        }

        public static unsafe int GetNumWheels(this Vehicle v)
        {
            return *(int*)((ulong)v.MemoryAddress + Offsets["WheelCountOffset"]);
        }

        public static unsafe List<ulong> GetWheelPtrs(this Vehicle handle)
        {
            var wheelPtr = GetWheelsPtr(handle);
            var numWheels = GetNumWheels(handle);
            List<ulong> wheelPtrs = new List<ulong>();
            for (int i = 0; i < numWheels; i++)
            {
                var wheelAddr = *((ulong*)(wheelPtr + 0x008 * (ulong)i));
                wheelPtrs.Add(wheelAddr);
            }
            return wheelPtrs;
        }


        public static unsafe List<float> GetWheelRotationSpeeds(this Vehicle handle)
        {
            List<ulong> wheelPtrs = GetWheelPtrs(handle);
            List<float> speeds = new List<float>();

            foreach (var wheel in wheelPtrs)
            {
                speeds.Add(-*((float*) (wheel + Offsets["WheelAngularVelocityOffset"])));
            }
            return speeds;
        }

        public static unsafe List<float> GetWheelSteeringAngles(this Vehicle handle)
        {
            List<ulong> wheelPtrs = GetWheelPtrs(handle);
            List<float> angles = new List<float>();

            foreach (var wheel in wheelPtrs)
            {
                angles.Add(*((float*)(wheel + Offsets["WheelSteeringAnglesOffset"])));
            }
            return angles;
        }

        public static unsafe List<float> GetBrakePressures(this Vehicle handle)
        {
            List<ulong> wheelPtrs = GetWheelPtrs(handle);

            List<float> angles = new List<float>();

            foreach (var wheel in wheelPtrs)
            {
                angles.Add(*((float*)(wheel + Offsets["WheelBrakePressureOffset"])));
            }
            return angles;
        }

        public static unsafe ulong GetModelInfoPtr(this Vehicle handle)
        {
            var address = (ulong)handle.MemoryAddress;
            ulong offset = 0x020; //TODO: Future-proof

            return *((ulong*)(address + offset));
        }

        public static unsafe float GetDrawnWheelAngleMult(this Vehicle handle)
        {
            // TODO: Future-proofing?
            return *(float*)(GetModelInfoPtr(handle) + 0x49c);
        }

        static unsafe ulong FindPattern(string pattern, string mask)
        {
            ProcessModule module = Process.GetCurrentProcess().MainModule;

            ulong address = (ulong)module.BaseAddress.ToInt64();
            ulong endAddress = address + (ulong)module.ModuleMemorySize;

            for (; address < endAddress; address++)
            {
                for (int i = 0; i < pattern.Length; i++)
                {
                    if (mask[i] != '?' && ((byte*)address)[i] != pattern[i])
                        break;
                    else if (i + 1 == pattern.Length)
                        return address;
                }
            }

            return 0;
        }

        private static unsafe void GetAndSetOffset(Settings settings, string field, 
            string pattern, string mask, long off1, long off2)
        {
            if (settings.Offsets[field] == 0)
            {
                ulong addr = FindPattern(pattern, mask);
                Offsets[field] = addr == 0 ? 0 : (uint)(*(uint*)((long)addr + off1) + off2);
                Logger.Log(Logger.Level.DEBUG, $"[VehExt] Found address [0x{Offsets[field]:X}] for [{field}]");
            }
            else
            {
                Offsets[field] = settings.Offsets[field];
                Logger.Log(Logger.Level.DEBUG, $"[VehExt] Using address [0x{Offsets[field]:X}] for [{field}]");
            }
        }

        public static void InitializeOffsets(Settings settings)
        {
            GetAndSetOffset(settings, "WheelsPtrOffset",
                "\x3B\xB7\x48\x0B\x00\x00\x7D\x0D", "xx????xx",
                2, -8);

            GetAndSetOffset(settings, "WheelCountOffset",
                "\x3B\xB7\x48\x0B\x00\x00\x7D\x0D", "xx????xx",
                2, 0);

            GetAndSetOffset(settings, "WheelAngularVelocityOffset",
                "\x45\x0f\x57\xc9\xf3\x0f\x11\x83\x60\x01\x00\x00\xf3\x0f\x5c", "xxx?xxx???xxxxx",
                8, 0xC);

            if (Game.Version >= GameVersion.v1_0_1737_0_Steam)
            {
                GetAndSetOffset(settings, "WheelSteeringAnglesOffset",
                    "\x0F\x2F\x81\xBC\x01\x00\x00\x0F\x97\xC0\xEB\x00\xD1\x00", "xx???xxxxxx?x?",
                    3, 0);
            }
            else
            {
                GetAndSetOffset(settings, "WheelSteeringAnglesOffset",
                    "\x0F\x2F\x81\xBC\x01\x00\x00\x0F\x97\xC0\xEB\xDA", "xx???xxxxxxx",
                    3, 0);
            }

            if (Game.Version >= GameVersion.v1_0_1737_0_Steam)
            {
                GetAndSetOffset(settings, "WheelBrakePressureOffset",
                    "\x0F\x2F\x81\xBC\x01\x00\x00\x0F\x97\xC0\xEB\x00\xD1\x00", "xx???xxxxxx?x?",
                    3, 0x4);
            }
            else
            {
                GetAndSetOffset(settings, "WheelBrakePressureOffset",
                    "\x0F\x2F\x81\xBC\x01\x00\x00\x0F\x97\xC0\xEB\xDA", "xx???xxxxxxx",
                    3, 0x4);
            }
        }
    }
}
