using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
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
        private Timer _timer = null;

        public GlowingBrakesMain()
        {
            Logger.Clear();
            Logger.Log(Logger.Level.INFO, $"Glowing Brakes {Utility.Version}");
            Logger.Log(Logger.Level.INFO, $"Game version {Game.Version}");
            _glowVehicles = new List<GlowVehicle>();
            _vehicleConfigs = new List<VehicleConfig>();
            _timer = new Timer(500);
            Tick += OnTick;
            Aborted += OnAbort;
            ReadConfigs();
            VehicleExtensions.InitializeOffsets(GlowingBrakes.Settings.Get());
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
                    try
                    {
                        FileStream fs = new FileStream(file, FileMode.Open);
                        XmlReader reader = new XmlTextReader(fs);
                        if (serializer.CanDeserialize(reader))
                        {
                            // why do i need to cast if it should already be able to figure out the return type
                            var cfg = (VehicleConfig) serializer.Deserialize(reader);
                            Logger.Log(Logger.Level.DEBUG, $"{cfg.Model}: Visible.Count: {cfg.Visible.Count}");
                            _vehicleConfigs.Add(cfg);
                        }
                        reader.Close();
                        fs.Close();
                    }
                    catch(Exception e)
                    {
                        Logger.Log(Logger.Level.ERROR, $"{file} Read error: {e.Message}");
                    }
                }
            }
        }

        private void OnTick(object source, EventArgs e)
        {
            // Clean up lists every tick so we don't ever work with an invalid vehicle.
            for (int i = 0; i < _glowVehicles.Count; ++i)
            {
                bool exist = _glowVehicles[i].Vehicle.Exists();
                float distance = 0.0f;
                if (exist)
                    distance = World.GetDistance(Game.Player.Character.Position, _glowVehicles[i].Vehicle.Position);

                if (!exist || distance > GlowingBrakes.Settings.Get().DrawDistance)
                {
                    _glowVehicles[i].ClearPtfx();
                    _glowVehicles.RemoveAt(i);
                    --i;
                }
            }

            if (_timer.Expired())
            {
                _timer.Reset();
                // Add newly discovered vehicles periodically
                Vehicle[] allVehicles = World.GetAllVehicles();
                foreach (var v in allVehicles)
                {
                    if (v.GetNumWheels() != 4)
                        continue;
                    if (!v.EngineRunning)
                        continue;
                    if (World.GetDistance(Game.Player.Character.Position, v.Position) > GlowingBrakes.Settings.Get().DrawDistance)
                        continue;
                    if (GlowingBrakes.Settings.Get().IgnoredModels.Exists(x => Game.GenerateHash(x) == v.Model))
                        continue;
                    if (!_glowVehicles.Exists(x => x.Vehicle == v))
                    {
                        _glowVehicles.Add(new GlowVehicle(v, _vehicleConfigs));
                    }
                }
            }

            // Glow
            foreach (var v in _glowVehicles)
            {
                v.Update();
                v.DrawDisks();
            }

            // _HAS_CHEAT_STRING_JUST_BEEN_ENTERED
            if (Function.Call<bool>(Hash._0x557E43C447E700A8, Game.GenerateHash("glowDefaults")))
            {
                UI.Notify("Glowing Brakes\nWriting a default config.");
                Utility.WriteDefaultsFile();
            }
        }
    }
}
