// NIGHT52 builder U, round 5: design.n52.look52 (a view row; DECIDE52 D-16 / D-17, the GG-persona review: "the healing item don't show what
// they are ('you need to see the silhouette ...')", rule 8 on the grapple's rope and rings). Its switch: NightRows.ViewOn(cfg, "look52"), and
// -skygrindOff52 read from the command line too (the map, its neon and the ARCLINE's view are built before PlayerChecks applies the switch, as
// skin52's enemies and guns52's viewmodel are). With it on:
//   - the HEALTH PICKUP is a chunky plus (bone, a cyan plus inlaid on both faces, a slow rock about the camera-facing yaw): its own silhouette
//     says "health" at 15 m; its floating "MEND +N" label is not drawn (Pickup35View.Look52.cs). The weapon pickups unchanged.
//   - THE GRAPPLE: in a campaign map the node's 0.9 m neon cube (SurvivalBuilder's; it bloomed into a flat cyan "octagon disc" around the ring)
//     is not drawn, so GrappleNodes30's thin cyan ring is the node; the rope (and its hook) pale cyan instead of the saturated usable cyan.
// The red-orange vertical line in the persona review's spire frames is TelegraphView's spawn beam (an enemy's arrival: red-orange is right there);
// no player-usable thing was red-orange (the rope is config arcline.ropeColor 0x19D3E6). Off: NIGHT51's pickups, cubes and rope exactly.
// Numbers: UnityTuning.U52.cs.
using System;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>Round 6 (leftover 3, T-116): look49's grade on THE ICE LINE and THE SPAN. n50-r1's a349 combat frames (look49 forced on, V50's row)
    /// held 7 % (iceline) and 4 % (span) saturated against the 10-30 % band: the lit pixels there are ~90 % floor (HSV s 0.2-0.5) and the walls
    /// (s 0.65-0.85, the level's hue) sit just under the meter's v 0.3. An offline pass over those frames (the meter's definitions; scratch, not a
    /// Player): a global saturation lift that reaches 10 % on the combat frame saturates the floor (iceline's floor frame 5 -> 31 %, span's 4 -> 41 %;
    /// rejected: floors stay under the walls); a lift of the WALLS' value alone, x1.2 in sRGB, lands combat at 16 % / 14 %, the room frame at 21 % / 24 %
    /// and keeps the floor frame under 10 %. In the engine the walls' own light in their hue is the stone's unshadowed rim (V49RimHex, the level
    /// hue) and the SH probe's equator (the middle term: what a vertical face takes): both x Look52WallLift on a V50 level while look52 and look49
    /// draw there. An estimate of the engine's mapping (~x1.5 linear on a wall's light); the a349 reel's combat meter is the measure.</summary>
    public static class Look52Lift
    {
        /// <summary>From PostFx.Globals49 (once per map, after the rim and the probe are made): on a V50 slot (iceline 0, span 1) with look52 on, the
        /// rim x the lift and the probe rebuilt with its equator x the lift. Off (look52 off, another level): nothing.</summary>
        public static void Walls(GameConfig cfg, int slot, ref Color rim)
        {
            Lift = 1f;
            if (slot < 0 || slot >= UnityTuning.Look52WallLift.Length || !Look52.On(cfg)) return;
            float k = LiftFor(cfg, slot);
            if (k == 1f) return; // round 9: no lift on this slot (span): the rim and the probe as PostFx made them
            rim *= k;
            rim.a = 1f;
            int t = UnityTuning.V50Themes[slot];
            Color sky = Look.Hex(UnityTuning.V49AmbientSkyHex[t]).linear, eq = Look.Hex(UnityTuning.V49AmbientEquatorHex[t]).linear * k, gr = Look.Hex(UnityTuning.V49AmbientGroundHex[t]).linear;
            var sh = new UnityEngine.Rendering.SphericalHarmonicsL2();
            sh.AddAmbientLight(eq);
            sh.AddDirectionalLight(Vector3.up, (sky - gr) * 0.5f, 1f);
            RenderSettings.ambientProbe = sh;
            Lift = k;
        }

        /// <summary>The lift applied on this map (1: none), for the reels' lines.</summary>
        public static float Lift = 1f;

        /// <summary>A V50 slot's wall lift: UnityTuning.Look52WallLift, or (NIGHT53 U polish53, T-116; Polish53.cs IcelineGrade53) iceline's
        /// slot at design.n53.polish53.numbers.icelineGrade.lift while icelineGrade is on.</summary>
        public static float LiftFor(GameConfig cfg, int slot)
        {
            if (slot < 0 || slot >= UnityTuning.Look52WallLift.Length) return 1f;
            float k53 = IcelineGrade53.Lift(cfg, slot);
            return k53 > 0 ? k53 : UnityTuning.Look52WallLift[slot];
        }

        /// <summary>Round 7 (the lead): look52's own grade list, design.n52.look52.numbers.gradeLevels (lower case), read while look52 is on: the ids
        /// that look49 treats as listed besides its own design.n49.look49.numbers.levels. look52 off (or -skygrindOff52): empty, so look49's gate is
        /// today's exactly (its own list untouched: adding iceline / span THERE would change NIGHT51's game under the off switch).</summary>
        public static string[] GradeLevelsOf(GameConfig cfg)
        {
            const string key = "design.n52.look52.numbers.gradeLevels";
            if (cfg == null || !Look52.On(cfg) || !cfg.Has(key) || !(cfg.Get(key) is System.Collections.Generic.List<object> list)) return new string[0];
            var ids = new System.Collections.Generic.List<string>();
            foreach (object o in list) if (o is string id && id.Length > 0) ids.Add(id.ToLowerInvariant());
            IcelineGrade53.AddLevels(cfg, ids); // NIGHT53 U polish53 (T-116): iceline's ids while icelineGrade is on (Polish53.cs)
            return ids.ToArray();
        }

        /// <summary>The level (a map's id, lower case) is on look52's grade list with look52 on.</summary>
        public static bool GradeListed(GameConfig cfg, string levelId) => PostFx.Look49Listed(GradeLevelsOf(cfg), levelId);
    }

    /// <summary>Round 8 (the GG-persona review of spire, OWNER.md "the colors and stuff are way too bland ... it all needs to be unified"; T-112):
    /// on spire's frames the floor, the walls and the raised decks read as one olive; v6's island face (2.4 m) merged into the far wall. With look52
    /// on, on the levels of UnityTuning.Look52TierLevels (spire, foundry), the stone's vertex colours take three values by the render box's kind
    /// (the level JSON's data.boxes: floor / wall / ledge): the FLOOR darker (x Look52TierFloor), the WALLS as they are (the mid value), the LEDGES
    /// (decks, islands, steps) lifted: their tops x Look52TierDeckTop, their faces x Look52TierLedgeFace, so a tier's face reads against the wall
    /// behind it. A value scale only: the hue and the HSV saturation of each colour are kept (no new saturated pixels past the meter's band); the
    /// grade, the lights and the HUD are untouched. Off (look52 off, another level): the one top / side pair as today.
    /// Round 9 (n52-r13): the floor stays NIGHT51's (Look52TierFloor 1: the sunlit floor top could not go under the walls' sides without going
    /// near-black, UnityTuning.U52.cs); the ledges' faces x2.</summary>
    public static class Look52Tier
    {
        /// <summary>The map is a tier level and look52 is on (SurvivalBuilder asks once per build).</summary>
        public static bool On(GameConfig cfg, SurvivalMap map)
        {
            Applied = false;
            if (map == null || !Look52.On(cfg)) return false;
            string id = PostFx.LevelIdOf(map.json);
            foreach (string l in UnityTuning.Look52TierLevels) if (string.Equals(l, id, StringComparison.Ordinal)) { Applied = true; return true; }
            return false;
        }

        /// <summary>A render box's top and side colours by its kind (floor / wall / ledge; any other kind as a wall).</summary>
        public static void Colours(string kind, Color top, Color side, out Color t, out Color s)
        {
            float kt = 1f, ks = 1f;
            if (kind == "floor") { kt = ks = UnityTuning.Look52TierFloor; }
            else if (kind == "ledge") { kt = UnityTuning.Look52TierDeckTop; ks = UnityTuning.Look52TierLedgeFace; }
            t = Scale(top, kt);
            s = Scale(side, ks);
        }

        static Color Scale(Color c, float k) => new Color(c.r * k, c.g * k, c.b * k, c.a);

        /// <summary>The tiers were applied on the map built last (the reels' lines).</summary>
        public static bool Applied;
    }

    /// <summary>look52's switch and the a252 reel's probes.</summary>
    public static class Look52
    {
        /// <summary>The reel only: forces the row (null: the config's design.n52.look52).</summary>
        public static bool? Force;
        static GameConfig readFor;
        static bool on;

        public static bool On(GameConfig cfg)
        {
            if (Force.HasValue) return Force.Value;
            if (cfg == null) return false;
            if (!ReferenceEquals(cfg, readFor))
            {
                readFor = cfg;
                bool offSwitch = NightRows.OffFrom(Environment.GetCommandLineArgs(), "-skygrindOff") <= 52;
                on = NightRows.ViewOn(cfg, "look52") && !offSwitch;
                Built = on;
            }
            return on;
        }

        /// <summary>The row was read on (the reel's line).</summary>
        public static bool Built;
        /// <summary>The ARCLINE rope's colour as built (the reel prints it), node cubes left out of a campaign map's neon.</summary>
        public static Color RopeColor;
        public static int NodeCubesSkipped;

        /// <summary>The rope's pale cyan: the usable cyan lifted toward white (still the usable's hue, never a saturated line).</summary>
        public static Color Pale(Color usable)
        {
            Color c = Color.Lerp(usable, Color.white, UnityTuning.Look52RopePale);
            c.a = usable.a;
            return c;
        }
    }
}
