using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using GTA;
using GTA.Math;
using GTA.Native;

namespace GlowingBrakes
{
    public class GlowingBrakesMain : Script
    {
        private List<GlowVehicle> _glowVehicles = null;
        private List<VehicleConfig> _vehicleConfigs = null;

        public GlowingBrakesMain()
        {
            _glowVehicles = new List<GlowVehicle>();
            _vehicleConfigs = new List<VehicleConfig>();
            Tick += OnTick;
            Aborted += OnAbort;
            ReadConfigs();
        }

        private void OnAbort(object sourc, EventArgs e)
        {
            foreach (var v in _glowVehicles)
            {
                v.ClearPtfx();
            }
        }

        private void ReadConfigs()
        {
            _vehicleConfigs.Clear();
            var files = Directory.GetFiles(@"scripts\GlowingBrakes\Configs\");
            XmlSerializer serializer = new XmlSerializer(typeof(VehicleConfig));
            foreach (var file in files)
            {
                if (file.EndsWith(".xml"))
                {
                    FileStream fs = new FileStream(file, FileMode.Open);
                    XmlReader reader = new XmlTextReader(fs);
                    if (serializer.CanDeserialize(reader))
                    {
                        // why do i need to cast if it should already be able to figure out the return type
                        _vehicleConfigs.Add((VehicleConfig)serializer.Deserialize(reader));
                    }
                }
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
                    _glowVehicles.Add(new GlowVehicle(v, _vehicleConfigs));
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
