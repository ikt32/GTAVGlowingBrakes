using System;
using System.Collections.Generic;
using System.Text;
using GTA;
using GTA.Math;
using GTA.Native;

namespace GlowingBrakes
{
    static class Ptfx
    {
        public static void RequestEffectLibrary(string lib)
        {
            Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, lib);
            while (!Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, lib))
                Script.Wait(0);
        }
    }
}
