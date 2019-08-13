using System;
using System.Collections.Generic;
using System.Drawing;
using GTA;
using GTA.Math;
using GTA.Native;

namespace GlowingBrakes
{
    public class GlowingBrakesMain : Script
    {
        private List<GlowVehicle> _glowVehicles = null;

        public GlowingBrakesMain()
        {
            _glowVehicles = new List<GlowVehicle>();
            Tick += OnTick;
            Aborted += OnAbort;
        }

        private void OnAbort(object sourc, EventArgs e)
        {
            foreach (var v in _glowVehicles)
            {
                v.ClearPtfx();
            }
        }

        private void OnTick(object source, EventArgs e)
        {
            Vehicle[] allVehicles = World.GetAllVehicles();

            for (int i = 0; i < _glowVehicles.Count; ++i)
            {
                bool exist = _glowVehicles[i].Vehicle.Exists();
                float distance = 0.0f;
                if (exist)
                    distance = World.GetDistance(Game.Player.Character.Position, _glowVehicles[i].Vehicle.Position);

                if (!exist || distance > 50.0f)
                {
                    _glowVehicles.RemoveAt(i);
                    --i;
                }
            }

            // Add newly discovered vehicles
            foreach (var v in allVehicles)
            {
                if (v.GetNumWheels() != 4)
                    continue;
                if (World.GetDistance(Game.Player.Character.Position, v.Position) > 20.0f)
                    continue;
                if (!_glowVehicles.Exists(x => x.Vehicle == v))
                {
                    _glowVehicles.Add(new GlowVehicle(v));
                }
            }

            // Glow
            foreach (var v in _glowVehicles)
            {
                v.Update();
                v.DrawDisks();
            }
        }
    }
}
