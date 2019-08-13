using System;
using GTA;
using GTA.Math;
using GTA.Native;

namespace GlowingBrakes
{
    public class GlowVehicle
    {
        private readonly Vehicle _vehicle;

        public GlowVehicle(Vehicle vehicle)
        {
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

            var rotations = _vehicle.GetWheelRotations();

            for (int i = 0; i < 4; ++i)
            {
                if (float.IsNaN(BrakeTemps[i]))
                    BrakeTemps[i] = 0.0f;

                float targetVal = 0.0f;

                if (brakePressures[i] > 0.0f)
                {
                    targetVal = (brakePressures[i] + weightShiftFactor[i] / 40.0f) * Math.Abs(wheelRotSpeeds[i]);
                }
                targetVal = targetVal.Clamp(0.0f, 1.0f);
                if (targetVal > 0.0f)
                    BrakeTemps[i] = MathExt.Lerp(BrakeTemps[i], targetVal, 1.0f - (float)Math.Pow(0.100f, Game.LastFrameTime));
                else
                {
                    float coolRateMod = MathExt.Map(Math.Abs(wheelRotSpeeds[i]), 0.0f, 60.0f, 0.100f, -0.200f);
                    coolRateMod = coolRateMod.Clamp(-200.0f, 100.0f);
                    BrakeTemps[i] = MathExt.Lerp(BrakeTemps[i], 0.0f, 1.0f - (float)Math.Pow(0.800f + coolRateMod, Game.LastFrameTime));
                }

                BrakeTemps[i].Clamp(0.0f, 1.0f);
                if (float.IsNaN(BrakeTemps[i]))
                    BrakeTemps[i] = 0.0f;

                //BrakeTemps[i] = 1.0f; //TODO: Temporary
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
                            0.06f, 0.0f, 0.0f,
                            0.0f, 0.0f, 90.0f,
                            boneIdxs[i], 1.375f,
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

                // rip for this ptfx
                //Function.Call(Hash.SET_PARTICLE_FX_LOOPED_COLOUR, PtfxHandles[i],
                //0.0f, //R
                //0.0f, //G
                //255.0f, //B 
                //false);
            }
        }
    }
}
