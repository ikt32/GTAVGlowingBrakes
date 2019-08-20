using System;
using System.Collections.Generic;
using GTA;
using GTA.Math;
using GTA.Native;

namespace GlowingBrakes
{
    public class GlowVehicle
    {
        private readonly Vehicle _vehicle;
        private readonly VehicleConfig _config;

        // config is passed as reference, right?
        public GlowVehicle(Vehicle vehicle, List<VehicleConfig> configs)
        {
            bool found = false;
            foreach(var config in configs)
            {
                if (Game.GenerateHash(config.Model) == vehicle.Model)
                {
                    _config = config;
                    found = true;
                    break;
                }
            }
            if (!found)
                _config = new VehicleConfig();

            _vehicle = vehicle;
            CurrVelocity = Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, Vehicle, true);
            LastVelocity = CurrVelocity;
            Acceleration = (CurrVelocity - LastVelocity) / Game.LastFrameTime;
        }

        public Vehicle Vehicle
        {
            get { return _vehicle; }
        }

        public float[] BrakeTemps = new float[4];

        public int[] PtfxHandles = new int[4] { 0, 0, 0, 0 };

        public Vector3 CurrVelocity { get; private set; }
        public Vector3 LastVelocity { get; private set; }
        public Vector3 Acceleration { get; private set; }

        public void Update()
        {
            LastVelocity = CurrVelocity;
            CurrVelocity = Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, Vehicle, true);
            Acceleration = (CurrVelocity - LastVelocity) / Game.LastFrameTime;
        }

        public void ClearPtfx()
        {
            for (int i = 0; i < 4; ++i)
            {
                if (this.PtfxHandles[i] != 0)
                {
                    Function.Call(Hash.REMOVE_PARTICLE_FX, this.PtfxHandles[i], 0);
                    this.PtfxHandles[i] = 0;
                }
            }
        }

        public void DrawDisks()
        {
            if (_vehicle.GetBoneIndex("wheel_lf") == -1) return;
            if (_vehicle.GetBoneIndex("wheel_rf") == -1) return;
            if (_vehicle.GetBoneIndex("wheel_lr") == -1) return;
            if (_vehicle.GetBoneIndex("wheel_rr") == -1) return;

            Vector3[] wheelCoords = new Vector3[4];
            wheelCoords[0] = _vehicle.GetBoneCoord("wheel_lf");
            wheelCoords[1] = _vehicle.GetBoneCoord("wheel_rf");
            wheelCoords[2] = _vehicle.GetBoneCoord("wheel_lr");
            wheelCoords[3] = _vehicle.GetBoneCoord("wheel_rr");

            int[] boneIdxs = new int[4];
            boneIdxs[0] = _vehicle.GetBoneIndex("wheel_lf");
            boneIdxs[1] = _vehicle.GetBoneIndex("wheel_rf");
            boneIdxs[2] = _vehicle.GetBoneIndex("wheel_lr");
            boneIdxs[3] = _vehicle.GetBoneIndex("wheel_rr");

            float[] weightShiftFactor = new float[4];
            weightShiftFactor[0] = -Acceleration.Y;
            weightShiftFactor[1] = -Acceleration.Y;
            weightShiftFactor[2] = Acceleration.Y;
            weightShiftFactor[3] = Acceleration.Y;

            var wheelRotSpeeds = _vehicle.GetWheelRotationSpeeds();
            //var steeredWheelAngles = _vehicle.GetWheelSteeringAngles();
            var brakePressures = _vehicle.GetBrakePressures();

            Vector3 p = _vehicle.Position;

            //var rotations = _vehicle.GetWheelRotations();

            for (int i = 0; i < 4; ++i)
            {
                if (float.IsNaN(BrakeTemps[i]))
                    BrakeTemps[i] = 0.0f;

                float targetVal = 0.0f;

                if (brakePressures[i] > 0.0f)
                {
                    targetVal = (brakePressures[i] + weightShiftFactor[i] * _config.AccelerationMult) * Math.Abs(wheelRotSpeeds[i]);
                }
                targetVal = targetVal.Clamp(0.0f, 1.0f);
                if (targetVal > 0.0f)
                {
                    float heatRate = _config.HeatRate;
                    heatRate.Clamp(0.0f, 1.0f);

                    BrakeTemps[i] = MathExt.Lerp(BrakeTemps[i], targetVal, 1.0f - (float)Math.Pow(1.0f - heatRate, Game.LastFrameTime));
                }
                else
                {
                    float coolRateMoving = _config.CoolRateMoving;
                    coolRateMoving.Clamp(0.0f, 1.0f);
                    float coolRateStopped = _config.CoolRateStopped;
                    coolRateStopped.Clamp(0.0f, 1.0f);

                    float coolRateMod = MathExt.Map(Math.Abs(wheelRotSpeeds[i]), 0.0f, 60.0f, 1.0f - coolRateStopped, 1.0f - coolRateMoving);
                    coolRateMod = coolRateMod.Clamp(1.0f - coolRateMoving, 1.0f - coolRateStopped);
                    BrakeTemps[i] = MathExt.Lerp(BrakeTemps[i], 0.0f, 1.0f - (float)Math.Pow(coolRateMod, Game.LastFrameTime));
                }

                BrakeTemps[i].Clamp(0.0f, 1.0f);
                if (float.IsNaN(BrakeTemps[i]))
                    BrakeTemps[i] = 0.0f;

                // Set to always hot, for testing/positioning purposes.
                if (Settings.Get().Debug)
                {
                    BrakeTemps[i] = 1.0f;
                }

                // Drawing ptfx
                if (BrakeTemps[i] > 0.066f)
                {
                    if (PtfxHandles[i] == 0)
                    {
                        Ptfx.RequestEffectLibrary("veh_impexp_rocket");
                        //Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, "veh_impexp_rocket");
                        //while (!Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, "veh_impexp_rocket"))
                        //    Wait(0);

                        Function.Call(Hash._0x6C38AF3693A69A91, "veh_impexp_rocket"); //USE_PARTICLE_FX_ASSET
                        PtfxHandles[i] = Function.Call<int>(Hash._START_PARTICLE_FX_LOOPED_ON_ENTITY_BONE,
                            "veh_rocket_boost",
                            _vehicle,
                            _config.Offset.X, _config.Offset.Y, _config.Offset.Z, //0.06f, 0.0f, 0.0f,
                            _config.Rotation.X, _config.Rotation.Y, _config.Rotation.Z, //0.0f, 0.0f, 90.0f,
                            boneIdxs[i], _config.PtfxSize, //1.375f,
                            false, false, false);
                        Function.Call(Hash.SET_PARTICLE_FX_LOOPED_EVOLUTION, PtfxHandles[i], "boost", 0.0f, 0);
                        Function.Call(Hash.SET_PARTICLE_FX_LOOPED_EVOLUTION, PtfxHandles[i], "charge", 1.0f, 0);
                        Function.Call(Hash.SET_PARTICLE_FX_LOOPED_ALPHA, PtfxHandles[i], 100.0f);
                    }
                }
                if (BrakeTemps[i] < 0.050f)
                {
                    if (PtfxHandles[i] != 0)
                    {
                        Function.Call(Hash.REMOVE_PARTICLE_FX, PtfxHandles[i], 0);
                        PtfxHandles[i] = 0;
                    }
                }

                Function.Call(Hash.SET_PARTICLE_FX_LOOPED_ALPHA, PtfxHandles[i], BrakeTemps[i] * 2.0f);
                //Function.Call(Hash.SET_PARTICLE_FX_LOOPED_EVOLUTION, PtfxHandles[i], "charge", BrakeTemps[i], 0);

                // want the outer edges to glow before the inner parts
                //Function.Call(Hash.SET_PARTICLE_FX_LOOPED_SCALE, PtfxHandles[i], 2.375f - BrakeTemps[i], 0);

                //Function.Call(Hash.SET_PARTICLE_FX_LOOPED_OFFSETS, PtfxHandles[i], 
                //    0.06f, 0.0f, 0.0f,
                //    0.0f, 0.0f, 90.0f);
                //MathExt.RadToDeg(rotations[i])
                // lmao hot
                //float boostFire = BrakeTemps[i] > 0.66f ? ( BrakeTemps[i] - 0.66f )/0.33f: 0.0f;
                //Function.Call(Hash.SET_PARTICLE_FX_LOOPED_EVOLUTION, PtfxHandles[i], "boost", boostFire, 0);
                
                // TODO: Re-enable when found ptfx with colorable disc
                // rip for this ptfx
                //Function.Call(Hash.SET_PARTICLE_FX_LOOPED_COLOUR, PtfxHandles[i],
                //0.0f, //R
                //0.0f, //G
                //255.0f, //B 
                //false);

                //// Defunct - Less preferred method since it won't show in R* editor. Code should be working "enough",
                //// so I'm just leaving this for future reference.
                //// Drawing/Positioning marker
                //float mult = GetDrawnWheelAngleMult(_vehicle);
                //float actualAngle = steeredWheelAngles[i] * mult;
                //float debugAngle = actualAngle - MathExt.DegToRad(90.0f);
                //float steeringAngleRelX = (float)-Math.Sin(debugAngle);
                //float steeringAngleRelY = (float)Math.Cos(debugAngle);
                //Vector3 forward = _vehicle.GetOffsetInWorldCoords(new Vector3(steeringAngleRelX, steeringAngleRelY, 0.0f));
                //Vector3 dir = forward - p;
                //Vector3 rot = new Vector3
                //{
                //    X = 90.0f
                //};
                //
                //float sgn = -1.0f;
                //if (i % 2 != 0) sgn = 1.0f;
                //Vector3 coord = MathExt.GetOffsetInWorldCoords(wheelCoords[i], rot, dir, new Vector3(0, sgn * 0.011f, 0));
                //
                //Color color = Color.FromArgb(
                //    (int)(BrakeTemps[i] * 160.0f),
                //    (int)(BrakeTemps[i] * 255.0f),
                //    BrakeTemps[i] > 0.66f ? (int)(((BrakeTemps[i] - 0.66f) / (0.33f)) * 128.0f) : 0,
                //    0);
                //
                //DrawMarker(MarkerType.HorizontalCircleFat,
                //    coord, dir, rot, 0.31f, color);
                //DrawMarker(MarkerType.HorizontalCircleFat,
                //    coord, dir, rot, 0.21f, color);
            }
        }
    }
}
