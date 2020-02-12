# Glowing Brake Discs

_We high performance GT cars now._

This is a script that draws glowing brake discs.

## Downloads

* [Glowing Brake Discs on GTA5-Mods.com](https://www.gta5-mods.com/scripts/glowing-brake-discs)
* [Release archive on GitHub](https://github.com/E66666666/GTAVGlowingBrakes/releases)

## Usage instructions

Please consult the description on the [GTA5-Mods](https://www.gta5-mods.com/scripts/glowing-brake-discs) page.

## Build instructions

The project is built against [ScriptHookVDotNet v3](https://www.nuget.org/packages/ScriptHookVDotNet3/). (after v1.0.0)

## Todo-list

* Separate front/rear offsets
* Front/mid/rear index and IDs (LeeC: `As for wheel bones, the game scripts only ever seem to use tyre indexes 0 - 5. 0 + 1 being front, 2 + 3 being mid and 4 + 5 being rear. So wheel_lm1 and wheel_rm1 are the bones for those indexes.`)
* Control marker limit (LeeC: 224?)
* Control looped Ptfx limit
* Player-only mode
* Optimize GetDistance to 2d check

Less attainable goals, but still interesting:

* Rotate Ptfx with wheel for smooth motion-blurred screenshots
* Find a less obviously rocket-engine-exhaust Ptfx
