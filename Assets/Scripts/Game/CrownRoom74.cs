// NIGHT74 builder B (leftover 2, design.n74.crownroom74, a VIEW row read at the map's build: NightRows.ViewOn(cfg, "crownroom74"); ON for a run with -skygrindRows72 crownroom74:1, OFF plain: two launches
// of one build, the way a level row is paired; it is NOT a same-run row: the stone's vertex colours are baked once per map by Palette72.Offer). gg-proxy NIGHT73 close #2: "THE CROWN's room is five unrelated
// hues: an olive-green ceiling grid, magenta-pink walls, brown slabs, a pink-red block and the cyan ring; magenta is reserved for RIVAL". GG 2026-10-20: "each boss should have its own distinct color palette
// and lighting identity"; 2026-10-22: "effect colores everything doenst feel alligned".
// WHAT THE FRAMES SAY (Logs/feel/n73-freeze/bp/boss63, the vivid pixels' hue families, n74-palette.py): at rest 63 % of the frame is vivid and its largest family is VIOLET (hue 230-284: 19.9 %), then
// yellow-green 9.4, green 8.7, cyan 7.2 (the ring: weak74's) ; in a red tell the violet becomes MAGENTA (hue 285-344: 3.7 % idle, 17.0 % windup, 16.1 % tell, with violet 19.9 -> 3.3): the violet walls
// under the danger column's and the gems' red. Seven hue families hold 3 % or more of the idle frame. ROUND 2 (the lead's n74-t4 pair, Logs/feel/n74-t4/croff and cron; my round-1 reading "the violet is the
// stone's albedo, not the sky" was half wrong): the t4 idle frame's violet + magenta is 19.1 % of its pixels, and round 1's wall fix took 7.8 points of it (41 %: the wall, OFF (79, 79, 118): a slate-blue, hue 240,
// S 0.3-0.4, which the meter's violet band 230-284 counts); the 11.3 points that stay are the top-right plane (85 % of its cell) and the band under it (a 4 x 4 grid of the frame): the dusk SKY seen through the spire's
// open top, which no wall multiplier reaches (CrownSky74.cs). colour56 gives the
// spire a SLATE wall ramp (0x1d1f24 -> 0x6e7280) and palette72 multiplies the wall class by x(1.23, 0.93, 2.92) (UnityTuning.Palette72WallMul, calibrated on the spire's amber-lit rooms: the light is blue-starved, so
// a neutral albedo is OLIVE on screen: round 1's uniform 1.15 made the walls hue 77-90 and yellow-green 18 -> 47 % of the vivid pixels).
// WHAT THE ROW DOES: inside THE CROWN's boss room (the campaign room whose boss is crown37 in the LOADED MAP's own JSON, SurvivalGame.map.json, so it is found whether or not a campaign run has begun:
// the first version asked the run's campaign level and found none when decor54 baked the stone; its floor rect, grown N74CrownRoomMarginM) the stone's wall and ceiling vertices take
// UnityTuning.N74CrownRoomWallMul, (1.26, 0.95, 2.25): palette72's multiplier times target / OFF, from the measured fact that the screen's channels scale linearly with the vertex multiplier (its comment has the
// pixels): the wall lands on a desaturated slate at its own luma, (81, 81, 91), S 0.11, under the meter's vivid floor and out of the olive. THE CROWN's room is then slate walls, palette72's sand floors and bone
// ivory accents, bronze blades, red tells and the player's cyan, under CrownSky74's slate-and-ivory sky: the identity grade54 wrote for it ("ivory and gold at height over slate") with no grade. The rest of the
// spire keeps palette72's (the whole level is one stone mesh; the rooms before the boss are untouched).
// HOOK (the lead's: Palette72.cs is outside builder B's list, so the patch is a script, Research/2026-10-27-wave74/B/crownroom74-hook.cjs): three lines in Palette72.Offer, `CrownRoom74.Begin(cfg)` before its
// vertex loop, `if (cls == "wall") m = CrownRoom74.Wall(m, t.TransformPoint(v[i]))` after the multiplier, `CrownRoom74.End(key)` after the loop. Without them this file does nothing.
// VIEW ONLY: vertex colours of the stone mesh (palette72's own bake); no collider, level file, sim or tape change. THE PROOF (L-089): `[crownroom74] <level> ON|off: THE CROWN's room x0..x1 z0..z1 (grown ..):
// <n> of <m> wall vertices inside took x(r, g, b) in place of palette72's (r, g, b)`, once per stone mesh the pass offers; `[crownroom74] <level>: no crown37 boss room in this level (nothing)` elsewhere.
using System;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>The pure maths (no Unity object): is a web-space point inside a rect grown by a margin. EditMode: N74BCrownRoomTests.</summary>
    public static class CrownRoom74Math
    {
        /// <summary>Is (x, z) inside [x0, x1] x [z0, z1] grown by `margin` on every side (the rect's corners in either order).</summary>
        public static bool Inside(double x, double z, double x0, double z0, double x1, double z1, double margin)
        {
            double lx = Math.Min(x0, x1) - margin, hx = Math.Max(x0, x1) + margin, lz = Math.Min(z0, z1) - margin, hz = Math.Max(z0, z1) + margin;
            return x >= lx && x <= hx && z >= lz && z <= hz;
        }

        /// <summary>Rec. 709 luminance weight of a multiplier (what it does to a grey): the number the replacement keeps near palette72's.</summary>
        public static double Weight(double r, double g, double b) => 0.2126 * r + 0.7152 * g + 0.0722 * b;

        /// <summary>The floor rect [x0, z0, x1, z1] of the boss room whose boss is `boss`, from a level's own JSON (the loaded SurvivalMap.json: campaign.rooms[], each with `kind`, `boss` and `rect`).
        /// False when the JSON has no campaign, no such room, or a rect that is not four numbers. NIGHT74 B: the first version asked the run's campaign level (SurvivalSim.campaignLevel), which is set
        /// at the campaign's StartRun, AFTER decor54's stone pass has baked the map (the boss63 reel boots the map directly: "no campaign level"); the map's own JSON is there from the load.</summary>
        public static bool BossRect(System.Collections.Generic.IDictionary<string, object> json, string boss, out string levelName, out double x0, out double z0, out double x1, out double z1)
        {
            levelName = null; x0 = z0 = x1 = z1 = 0;
            if (json == null) return false;
            if (json.TryGetValue("name", out object nm) && nm is string ns) levelName = ns;
            if (!json.TryGetValue("campaign", out object co) || !(co is System.Collections.Generic.Dictionary<string, object> camp)) return false;
            if (!camp.TryGetValue("rooms", out object ro) || !(ro is System.Collections.Generic.List<object> rooms)) return false;
            foreach (object r in rooms)
            {
                if (!(r is System.Collections.Generic.Dictionary<string, object> room)) continue;
                if (!room.TryGetValue("kind", out object k) || !(k is string ks) || ks != "boss") continue;
                if (!room.TryGetValue("boss", out object b) || !(b is string bs) || !string.Equals(bs, boss, StringComparison.Ordinal)) continue;
                if (!room.TryGetValue("rect", out object rc) || !(rc is System.Collections.Generic.List<object> rl) || rl.Count != 4) continue;
                try { x0 = PortData.Num(rl[0]); z0 = PortData.Num(rl[1]); x1 = PortData.Num(rl[2]); z1 = PortData.Num(rl[3]); }
                catch (Exception) { continue; }
                return true;
            }
            return false;
        }
    }

    public static class CrownRoom74
    {
        public const string Row = "crownroom74";

        /// <summary>The row, uncached (tests; read at the map's build by Begin, and by the reel's shot line).</summary>
        public static bool Decide(GameConfig cfg) => cfg != null && NightRows.ViewOn(cfg, Row);

        /// <summary>For the reel and the tests: this map's state (the last Begin), the vertices seen and changed, the room's rect.</summary>
        public static bool Active;
        public static int Seen, Changed, ChangedFlipped, ChangedRaw;
        public static double X0, Z0, X1, Z1;
        public static float SeenMinZ, SeenMaxZ;
        public static string Line = "crownroom74: not attached";

        static Vector3 mul;

        /// <summary>Palette72.Offer, before its vertex loop: reads the row and finds THE CROWN's boss room in the loaded map's own JSON (no such room, or no map: inactive).</summary>
        public static void Begin(GameConfig cfg)
        {
            Active = false;
            Seen = Changed = ChangedFlipped = ChangedRaw = 0;
            SeenMinZ = float.MaxValue;
            SeenMaxZ = float.MinValue;
            mul = new Vector3(UnityTuning.N74CrownRoomWallMul[0], UnityTuning.N74CrownRoomWallMul[1], UnityTuning.N74CrownRoomWallMul[2]);
            SurvivalGame g = SurvivalGame.Instance;
            var json = g != null && g.map != null ? g.map.json : null; // the loaded map's own JSON: there from the load, before any campaign run starts (decor54's pass bakes the stone first)
            bool on = Decide(cfg);
            if (!CrownRoom74Math.BossRect(json, UnityTuning.N74CrownRoomBoss, out string name, out X0, out Z0, out X1, out Z1))
            {
                Line = "[crownroom74] " + (name ?? (json != null ? "a map without a name" : "no loaded map")) + ": no " + UnityTuning.N74CrownRoomBoss + " boss room in this map (nothing)";
                return;
            }
            Active = on;
            Line = "[crownroom74] " + (name ?? "?") + " " + (on ? "ON" : "off") + ": THE CROWN's room x " + X0.ToString("0") + ".." + X1.ToString("0") + " z " + Z0.ToString("0") + ".." + Z1.ToString("0");
        }

        /// <summary>Palette72.Offer, for a wall-class vertex with palette72's multiplier `current` at world point `world` (Unity space): the replacement multiplier inside THE CROWN's room, else `current`.</summary>
        public static Vector3 Wall(Vector3 current, Vector3 world)
        {
            if (!Active) return current;
            Seen++;
            if (world.z < SeenMinZ) SeenMinZ = world.z;
            if (world.z > SeenMaxZ) SeenMaxZ = world.z;
            // the stone is Unity space, where web z is -z (WebSpace.ToUnity): the rect is tested flipped. The raw sign is tested too, and the line counts both: if the stone were in web space the
            // flipped test would find nothing, and the one mesh the level has stands on one side only (the spire's rooms run web z +115 to -323: nothing stands where the other sign would look)
            bool flipped = CrownRoom74Math.Inside(world.x, -world.z, X0, Z0, X1, Z1, UnityTuning.N74CrownRoomMarginM);
            bool raw = CrownRoom74Math.Inside(world.x, world.z, X0, Z0, X1, Z1, UnityTuning.N74CrownRoomMarginM);
            if (flipped) ChangedFlipped++;
            if (raw) ChangedRaw++;
            if (!flipped && !raw) return current;
            Changed++;
            return mul;
        }

        /// <summary>Palette72.Offer, after its vertex loop: the proof line for the stone mesh just offered.</summary>
        public static void End(string key)
        {
            string line = Active
                ? Line + " (grown " + UnityTuning.N74CrownRoomMarginM.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + " m): " + Changed + " of " + Seen + " wall vertices inside (z flipped " + ChangedFlipped + ", raw " + ChangedRaw + "; the stone's z " + SeenMinZ.ToString("0") + ".." + SeenMaxZ.ToString("0") + ") took x(" + mul.x.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)
                    + ", " + mul.y.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + ", " + mul.z.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + ") in place of palette72's"
                : Line;
            Line = line;
            Debug.Log(line + " | stone " + (key ?? "?"));
        }
    }
}
