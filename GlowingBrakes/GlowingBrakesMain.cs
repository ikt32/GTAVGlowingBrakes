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
using GTA.UI;

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

            VehicleConfig.SetDefaultConfigFromFile();
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
                if (file.EndsWith(".xml") && !file.EndsWith("defaultConfig.xml"))
                {
                    try
                    {
                        FileStream fs = new FileStream(file, FileMode.Open);
                        XmlReader reader = new XmlTextReader(fs);
                        if (serializer.CanDeserialize(reader))
                        {
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

        private void UpdateVehicleListDistance()
        {
            float drawDistance = GlowingBrakes.Settings.Get().DrawDistance;
            float drawDistance2 = drawDistance * drawDistance;

            // Clean up lists every tick so we don't ever work with an invalid vehicle.
            for (int i = 0; i < _glowVehicles.Count; ++i)
            {
                bool exist = _glowVehicles[i].Vehicle.Exists();
                float distance2 = 0.0f;
                if (exist)
                    distance2 = Utility.Distance2(Game.Player.Character.Position, _glowVehicles[i].Vehicle.Position);

                if (!exist || distance2 > drawDistance2)
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
                    if (v.ClassType == VehicleClass.Boats ||
                        v.ClassType == VehicleClass.Helicopters ||
                        v.ClassType == VehicleClass.Planes)
                        continue;
                    if (v.GetNumWheels() != 4)
                        continue;
                    if (!v.IsEngineRunning)
                        continue;
                    if (Utility.Distance2(Game.Player.Character.Position, v.Position) > drawDistance2)
                        continue;
                    if (GlowingBrakes.Settings.Get().IgnoredModels.Exists(x => Game.GenerateHash(x) == v.Model))
                        continue;

                    if (!_glowVehicles.Exists(x => x.Vehicle == v))
                        _glowVehicles.Add(new GlowVehicle(v, _vehicleConfigs));
                }
            }
        }

        private void UpdateVehicleListPlayer()
        {
            Vehicle playerVehicle = Game.Player.Character.CurrentVehicle;

            // Clean up lists every tick so we don't ever work with an invalid vehicle.
            for (int i = 0; i < _glowVehicles.Count; ++i)
            {
                bool playerDifferentVehicle = false;
                if (playerVehicle != null)
                    playerDifferentVehicle = playerVehicle != _glowVehicles[i].Vehicle;

                bool exist = _glowVehicles[i].Vehicle.Exists();

                if (!exist || playerDifferentVehicle)
                {
                    _glowVehicles[i].ClearPtfx();
                    _glowVehicles.RemoveAt(i);
                    --i;
                }
            }

            if (_timer.Expired())
            {
                _timer.Reset();
                // Update player vehicle periodically

                if (playerVehicle == null)
                    return;
                if (playerVehicle.ClassType == VehicleClass.Boats ||
                    playerVehicle.ClassType == VehicleClass.Helicopters ||
                    playerVehicle.ClassType == VehicleClass.Planes)
                    return;
                if (playerVehicle.GetNumWheels() != 4)
                    return;
                if (!playerVehicle.IsEngineRunning)
                    return;
                if (GlowingBrakes.Settings.Get().IgnoredModels.Exists(x => Game.GenerateHash(x) == playerVehicle.Model))
                    return;

                if (!_glowVehicles.Exists(x => x.Vehicle == playerVehicle))
                    _glowVehicles.Add(new GlowVehicle(playerVehicle, _vehicleConfigs));
            }
        }

        private void OnTick(object source, EventArgs e)
        {
            if (GlowingBrakes.Settings.Get().PlayerOnly)
                UpdateVehicleListPlayer();
            else
                UpdateVehicleListDistance();

            // Glow
            foreach (var v in _glowVehicles)
            {
                v.Update();
                v.DrawDisks();
            }

            // _HAS_CHEAT_STRING_JUST_BEEN_ENTERED
            if (Function.Call<bool>(Hash._HAS_CHEAT_STRING_JUST_BEEN_ENTERED, Game.GenerateHash("glowDefaults")))
            {
                Notification.Show("Glowing Brakes\nWriting a default config.");
                Utility.WriteDefaultsFile();
            }
        }
    }
}
