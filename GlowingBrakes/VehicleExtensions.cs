using System;
using System.Collections.Generic;
using System.Text;
using GTA;

namespace GlowingBrakes
{
    static class VehicleExtensions
    {
        public static unsafe ulong GetWheelsPtr(this Vehicle handle)
        {
            var address = (ulong)handle.MemoryAddress;
            ulong offset = 0xBB0; //TODO: Future-proof

            return *((ulong*)(address + offset));
        }

        public static unsafe int GetNumWheels(this Vehicle v)
        {
            return *(int*)((ulong)v.MemoryAddress + 0xBB8); //TODO: Future-proof
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

            ulong offset = 0x170; // TODO: Future-proof

            List<float> speeds = new List<float>();

            foreach (var wheel in wheelPtrs)
            {
                speeds.Add(-*((float*)(wheel + offset)));
            }
            return speeds;
        }

        public static unsafe List<float> GetWheelRotations(this Vehicle handle)
        {
            List<ulong> wheelPtrs = GetWheelPtrs(handle);

            ulong offset = 0x1CC; // TODO: Future-proof

            List<float> rots = new List<float>();

            foreach (var wheel in wheelPtrs)
            {
                rots.Add(-*((float*)(wheel + offset)));
            }
            return rots;
        }

        public static unsafe List<float> GetWheelSteeringAngles(this Vehicle handle)
        {
            List<ulong> wheelPtrs = GetWheelPtrs(handle);

            ulong offset = 0x1CC; // TODO: Future-proof

            List<float> angles = new List<float>();

            foreach (var wheel in wheelPtrs)
            {
                angles.Add(*((float*)(wheel + offset)));
            }
            return angles;
        }

        public static unsafe List<float> GetBrakePressures(this Vehicle handle)
        {
            List<ulong> wheelPtrs = GetWheelPtrs(handle);

            ulong offset = 0x1D0; // TODO: Future-proof

            List<float> angles = new List<float>();

            foreach (var wheel in wheelPtrs)
            {
                angles.Add(*((float*)(wheel + offset)));
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
            return *(float*)(GetModelInfoPtr(handle) + 0x49c);
        }

    }
}
