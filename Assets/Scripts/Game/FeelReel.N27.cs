// NIGHT27 (the lead): the reel's phase J: the NIGHT27 scenarios. Each owner writes its body in its own file
// (FeelReel.N27.<Topic>.cs) as a `static partial void N27<Topic>Body(...)` hook; a hook with no body leaves `res[0]` as NOT WRITTEN (RED).
//   roof, skate, slowaim   V   the roof's cost and frames, the ice's floor motion, the wall-ride aim slow-mo (FeelReel.N27.View.cs)
//   lantern, glacier27     B   the two bosses on the Player
//   pulse27, charge27      W   the PULSE and the RT-hold charge on the real input path
//   tutorial27, levelwalk27 A  the tutorial and the three levels walked
using System;
using System.Collections;
using System.Collections.Generic;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        public static readonly string[] PhaseJ = { "roof", "skate", "slowaim", "lantern", "glacier27", "pulse27", "charge27", "tutorial27", "levelwalk27", "slowworld", "jumppad", "rink28", "glacier28" };

        static bool IsPhaseJ(string s) => Array.IndexOf(PhaseJ, s) >= 0 || IsPhaseN29(s) || IsPhaseN30(s) || IsPhaseN31(s) || IsPhaseN32(s) || IsPhaseN33(s) || IsPhaseN34(s) || IsPhaseN35(s) || IsPhaseN36(s) || IsPhaseN37(s); // NIGHT29: lantern29, glacier29 (FeelReel.N29.Bosses.cs); NIGHT30, NIGHT31, NIGHT32, NIGHT33, NIGHT34, NIGHT35, NIGHT36, NIGHT37: below

        // NIGHT37: the reel scenarios by owner; each owner's FeelReel.N37.<Owner>.cs implements its body (no body: NOT WRITTEN, RED)
        public static readonly string[] PhaseN37K = { "verbs37" };
        public static readonly string[] PhaseN37W = { "wick37" };
        public static readonly string[] PhaseN37S = { "stag37" };
        public static readonly string[] PhaseN37O = { "loom37" };
        public static readonly string[] PhaseN37C = { "crown37" };
        static bool IsPhaseN37(string s) => Array.IndexOf(PhaseN37K, s) >= 0 || Array.IndexOf(PhaseN37W, s) >= 0 || Array.IndexOf(PhaseN37S, s) >= 0 || Array.IndexOf(PhaseN37O, s) >= 0 || Array.IndexOf(PhaseN37C, s) >= 0 || Array.IndexOf(PhaseN40B, s) >= 0 || Array.IndexOf(PhaseN40E, s) >= 0 || Array.IndexOf(PhaseN41T, s) >= 0 || Array.IndexOf(PhaseN42, s) >= 0 || Array.IndexOf(PhaseN43, s) >= 0 || Array.IndexOf(PhaseN44V2, s) >= 0 || Array.IndexOf(PhaseN45, s) >= 0 || Array.IndexOf(PhaseN46, s) >= 0 || Array.IndexOf(PhaseN47, s) >= 0 || Array.IndexOf(PhaseN48, s) >= 0 || Array.IndexOf(PhaseN49, s) >= 0 || Array.IndexOf(PhaseN50, s) >= 0 || Array.IndexOf(PhaseN52, s) >= 0 || Array.IndexOf(PhaseN53, s) >= 0 || Array.IndexOf(PhaseN54, s) >= 0 || Array.IndexOf(PhaseN55, s) >= 0 || Array.IndexOf(PhaseN56, s) >= 0 || Array.IndexOf(PhaseN57, s) >= 0 || Array.IndexOf(PhaseN58, s) >= 0; // NIGHT44 V2: look44 (FeelReel.N44.Look.cs); NIGHT43: the builders' proof reels (FeelReel.N43.cs); NIGHT42: the builders' proof reels (FeelReel.N42.cs); NIGHT41 T2: weapons41 ... (FeelReel.N41.cs); NIGHT40 B2: brawl40 (FeelReel.N40.Brawl.cs); E3: lineup40 (FeelReel.N40.Foes.cs)
        static partial void N37KitBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N37WickBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N37StagBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N37LoomBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N37CrownBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);

        // NIGHT36: the reel scenarios by owner; each owner's FeelReel.N36.<Owner>.cs implements its body (no body: NOT WRITTEN, RED)
        public static readonly string[] PhaseN36F = { "tap36", "crunch36", "fist36" };
        public static readonly string[] PhaseN36P = { "parry36", "launch36", "heal36" };
        public static readonly string[] PhaseN36M = { "wallgrip36", "text36", "verbs36" };
        public static readonly string[] PhaseN36L = { "level36" };
        public static readonly string[] PhaseN36B = { "boss36" };
        static bool IsPhaseN36(string s) => Array.IndexOf(PhaseN36F, s) >= 0 || Array.IndexOf(PhaseN36P, s) >= 0 || Array.IndexOf(PhaseN36M, s) >= 0 || Array.IndexOf(PhaseN36L, s) >= 0 || Array.IndexOf(PhaseN36B, s) >= 0;
        static partial void N36GunsBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N36CoreBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N36MoveBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N36LevelBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N36BossBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);

        // NIGHT35: the reel scenarios by owner; each owner's FeelReel.N35.<Owner>.cs implements its body (no body: NOT WRITTEN, RED)
        public static readonly string[] PhaseN35W = { "charge35", "pickup35" };
        public static readonly string[] PhaseN35T = { "teach35", "verbs35" };
        public static readonly string[] PhaseN35M = { "wallride35", "grapple35" };
        public static readonly string[] PhaseN35V = { "look35" };
        public static readonly string[] PhaseN35A = { "levels35" };
        static bool IsPhaseN35(string s) => Array.IndexOf(PhaseN35W, s) >= 0 || Array.IndexOf(PhaseN35T, s) >= 0 || Array.IndexOf(PhaseN35M, s) >= 0 || Array.IndexOf(PhaseN35V, s) >= 0 || Array.IndexOf(PhaseN35A, s) >= 0;
        static partial void N35WeaponsBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N35TeachBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N35MoveBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N35LookBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N35LevelsBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);

        // NIGHT34: the reel scenarios by owner; each owner's FeelReel.N34.<Owner>.cs implements its body (no body: NOT WRITTEN, RED)
        public static readonly string[] PhaseN34B = { "bend34" };
        public static readonly string[] PhaseN34H = { "hud34" };
        public static readonly string[] PhaseN34A = { "lines34" };
        public static readonly string[] PhaseN34M = { "flow34" };
        static bool IsPhaseN34(string s) => Array.IndexOf(PhaseN34B, s) >= 0 || Array.IndexOf(PhaseN34H, s) >= 0 || Array.IndexOf(PhaseN34A, s) >= 0 || Array.IndexOf(PhaseN34M, s) >= 0;
        static partial void N34BendBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N34HudBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N34LinesBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N34FlowBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);

        // NIGHT33: the reel scenarios by owner; each owner's FeelReel.N33.<Owner>.cs implements its body (no body: NOT WRITTEN, RED)
        public static readonly string[] PhaseN33U = { "hud33" };
        public static readonly string[] PhaseN33L = { "look33" };
        public static readonly string[] PhaseN33K = { "kit33" };
        public static readonly string[] PhaseN33R = { "arenas33" };
        public static readonly string[] PhaseN33E = { "ai33" };
        public static readonly string[] PhaseN33W = { "feel33" };
        public static readonly string[] PhaseN33P = { "shop33" };
        static bool IsPhaseN33(string s) => Array.IndexOf(PhaseN33U, s) >= 0 || Array.IndexOf(PhaseN33L, s) >= 0 || Array.IndexOf(PhaseN33K, s) >= 0 || Array.IndexOf(PhaseN33R, s) >= 0 || Array.IndexOf(PhaseN33E, s) >= 0 || Array.IndexOf(PhaseN33W, s) >= 0 || Array.IndexOf(PhaseN33P, s) >= 0;
        static partial void N33HudBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N33LookBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N33KitBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N33ArenasBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N33EnemiesBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N33WeaponsBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N33ShopBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);

        // NIGHT32: the reel scenarios by owner; each owner's FeelReel.N32.<Owner>.cs implements its body (no body: NOT WRITTEN, RED)
        public static readonly string[] PhaseN32B = { "bosses32", "camp32" };
        public static readonly string[] PhaseN32C = { "boss5" };
        public static readonly string[] PhaseN32G = { "grapple32", "balance32", "hud32" };
        public static readonly string[] PhaseN32A = { "arenas32", "ring32" };
        public static readonly string[] PhaseN32S = { "span32" };
        public static readonly string[] PhaseN32F = { "level5" };
        static bool IsPhaseN32(string s) => Array.IndexOf(PhaseN32B, s) >= 0 || Array.IndexOf(PhaseN32C, s) >= 0 || Array.IndexOf(PhaseN32G, s) >= 0 || Array.IndexOf(PhaseN32A, s) >= 0 || Array.IndexOf(PhaseN32S, s) >= 0 || Array.IndexOf(PhaseN32F, s) >= 0;
        static partial void N32BossesBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N32CrownBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N32GrappleBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N32ArenasBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N32SpanBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N32FinaleBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);

        // NIGHT31: the reel scenarios by owner; each owner's FeelReel.N31.<Owner>.cs implements its body (no body: NOT WRITTEN, RED)
        public static readonly string[] PhaseN31G = { "grapple31", "edge31", "ring31", "ride31" };
        public static readonly string[] PhaseN31W = { "charge31", "lt31", "unlocks31" };
        public static readonly string[] PhaseN31B = { "camp31", "lantern31", "stag31", "wyrm31", "loom31" };
        public static readonly string[] PhaseN31L = { "arenas31", "rooms31", "spawn31" }; // spawn31: builder L's (P0 c)
        static bool IsPhaseN31(string s) => Array.IndexOf(PhaseN31G, s) >= 0 || Array.IndexOf(PhaseN31W, s) >= 0 || Array.IndexOf(PhaseN31B, s) >= 0 || Array.IndexOf(PhaseN31L, s) >= 0;
        static partial void N31GrappleBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N31WeaponsBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N31BossesBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N31LevelsBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);

        // NIGHT30: the reel scenarios by owner; each owner's FeelReel.N30.<Owner>.cs implements its body (no body: NOT WRITTEN, RED)
        public static readonly string[] PhaseN30S = { "rules30", "lava30", "glide30", "lesson30", "hudfx30", "arcline30" };
        public static readonly string[] PhaseN30B = { "lantern30", "glacier30", "wyrm30", "loom30" };
        public static readonly string[] PhaseN30V = { "weaponhud30", "menus30", "layout30", "look30" };
        public static readonly string[] PhaseN30L = { "walk30", "rooms30", "span30" };
        static bool IsPhaseN30(string s) => Array.IndexOf(PhaseN30S, s) >= 0 || Array.IndexOf(PhaseN30B, s) >= 0 || Array.IndexOf(PhaseN30V, s) >= 0 || Array.IndexOf(PhaseN30L, s) >= 0;
        static partial void N30SystemsBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N30BossesBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N30ViewBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N30LevelsBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);

        static partial void N27RoofBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N27SkateBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N27SlowaimBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N27RoofBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = RoofScenario27(g, dir, res, offs, pre);
        static partial void N27SkateBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = SkateScenario27(g, dir, res, offs, pre);
        static partial void N27SlowaimBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = SlowAimScenario27(g, dir, res, offs, pre);
        static partial void N27LanternBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N27Glacier27Body(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N27Pulse27Body(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N27Charge27Body(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N27Tutorial27Body(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N27Levelwalk27Body(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N27SlowworldBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N27Rink28Body(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // builder A2: FeelReel.N28.Rink.cs
        static partial void N27JumppadBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // builder P: FeelReel.N27.Pad.cs
        static partial void N28Glacier28Body(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // NIGHT28 B2: FeelReel.N28.Glacier.cs

        static IEnumerator N27Scenario(SurvivalGame g, string s, string dir, string[] res, List<Action> offs, double yaw, int pre)
        {
            res[0] = "NOT WRITTEN (RED): no scenario body for '" + s + "'";
            IEnumerator body = null;
            switch (s)
            {
                case "roof": N27RoofBody(g, dir, res, offs, pre, ref body); break;
                case "skate": N27SkateBody(g, dir, res, offs, pre, ref body); break;
                case "slowaim": N27SlowaimBody(g, dir, res, offs, pre, ref body); break;
                case "lantern": N27LanternBody(g, dir, res, offs, pre, ref body); break;
                case "glacier27": N27Glacier27Body(g, dir, res, offs, pre, ref body); break;
                case "pulse27": N27Pulse27Body(g, dir, res, offs, pre, ref body); break;
                case "charge27": N27Charge27Body(g, dir, res, offs, pre, ref body); break;
                case "tutorial27": N27Tutorial27Body(g, dir, res, offs, pre, ref body); break;
                case "levelwalk27": N27Levelwalk27Body(g, dir, res, offs, pre, ref body); break;
                case "slowworld": N27SlowworldBody(g, dir, res, offs, pre, ref body); break;
                case "jumppad": N27JumppadBody(g, dir, res, offs, pre, ref body); break;
                case "rink28": N27Rink28Body(g, dir, res, offs, pre, ref body); break;
                case "glacier28": N28Glacier28Body(g, dir, res, offs, pre, ref body); break;
                case "lantern29": case "glacier29": N29BossesBody(s, g, dir, res, offs, pre, ref body); break;
                default: // NIGHT30: by owner
                    if (Array.IndexOf(PhaseN30S, s) >= 0) N30SystemsBody(s, g, dir, res, offs, pre, ref body);
                    else if (Array.IndexOf(PhaseN30B, s) >= 0) N30BossesBody(s, g, dir, res, offs, pre, ref body);
                    else if (Array.IndexOf(PhaseN30V, s) >= 0) N30ViewBody(s, g, dir, res, offs, pre, ref body);
                    else if (Array.IndexOf(PhaseN30L, s) >= 0) N30LevelsBody(s, g, dir, res, offs, pre, ref body);
                    else if (Array.IndexOf(PhaseN31G, s) >= 0) N31GrappleBody(s, g, dir, res, offs, pre, ref body); // NIGHT31: by owner
                    else if (Array.IndexOf(PhaseN31W, s) >= 0) N31WeaponsBody(s, g, dir, res, offs, pre, ref body);
                    else if (Array.IndexOf(PhaseN31B, s) >= 0) N31BossesBody(s, g, dir, res, offs, pre, ref body);
                    else if (Array.IndexOf(PhaseN31L, s) >= 0) N31LevelsBody(s, g, dir, res, offs, pre, ref body);
                    else if (Array.IndexOf(PhaseN32B, s) >= 0) N32BossesBody(s, g, dir, res, offs, pre, ref body); // NIGHT32: by owner
                    else if (Array.IndexOf(PhaseN32C, s) >= 0) N32CrownBody(s, g, dir, res, offs, pre, ref body);
                    else if (Array.IndexOf(PhaseN32G, s) >= 0) N32GrappleBody(s, g, dir, res, offs, pre, ref body);
                    else if (Array.IndexOf(PhaseN32A, s) >= 0) N32ArenasBody(s, g, dir, res, offs, pre, ref body);
                    else if (Array.IndexOf(PhaseN32S, s) >= 0) N32SpanBody(s, g, dir, res, offs, pre, ref body);
                    else if (Array.IndexOf(PhaseN32F, s) >= 0) N32FinaleBody(s, g, dir, res, offs, pre, ref body);
                    else if (Array.IndexOf(PhaseN33U, s) >= 0) N33HudBody(s, g, dir, res, offs, pre, ref body); // NIGHT33: by owner
                    else if (Array.IndexOf(PhaseN33L, s) >= 0) N33LookBody(s, g, dir, res, offs, pre, ref body);
                    else if (Array.IndexOf(PhaseN33K, s) >= 0) N33KitBody(s, g, dir, res, offs, pre, ref body);
                    else if (Array.IndexOf(PhaseN33R, s) >= 0) N33ArenasBody(s, g, dir, res, offs, pre, ref body);
                    else if (Array.IndexOf(PhaseN33E, s) >= 0) N33EnemiesBody(s, g, dir, res, offs, pre, ref body);
                    else if (Array.IndexOf(PhaseN33W, s) >= 0) N33WeaponsBody(s, g, dir, res, offs, pre, ref body);
                    else if (Array.IndexOf(PhaseN33P, s) >= 0) N33ShopBody(s, g, dir, res, offs, pre, ref body);
                    else if (Array.IndexOf(PhaseN34B, s) >= 0) N34BendBody(s, g, dir, res, offs, pre, ref body); // NIGHT34: by owner
                    else if (Array.IndexOf(PhaseN34H, s) >= 0) N34HudBody(s, g, dir, res, offs, pre, ref body);
                    else if (Array.IndexOf(PhaseN34A, s) >= 0) N34LinesBody(s, g, dir, res, offs, pre, ref body);
                    else if (Array.IndexOf(PhaseN34M, s) >= 0) N34FlowBody(s, g, dir, res, offs, pre, ref body);
                    else if (Array.IndexOf(PhaseN35W, s) >= 0) N35WeaponsBody(s, g, dir, res, offs, pre, ref body); // NIGHT35: by owner
                    else if (Array.IndexOf(PhaseN35T, s) >= 0) N35TeachBody(s, g, dir, res, offs, pre, ref body);
                    else if (Array.IndexOf(PhaseN35M, s) >= 0) N35MoveBody(s, g, dir, res, offs, pre, ref body);
                    else if (Array.IndexOf(PhaseN35V, s) >= 0) N35LookBody(s, g, dir, res, offs, pre, ref body);
                    else if (Array.IndexOf(PhaseN35A, s) >= 0) N35LevelsBody(s, g, dir, res, offs, pre, ref body);
                    else if (Array.IndexOf(PhaseN36F, s) >= 0) N36GunsBody(s, g, dir, res, offs, pre, ref body); // NIGHT36: by owner
                    else if (Array.IndexOf(PhaseN36P, s) >= 0) N36CoreBody(s, g, dir, res, offs, pre, ref body);
                    else if (Array.IndexOf(PhaseN36M, s) >= 0) N36MoveBody(s, g, dir, res, offs, pre, ref body);
                    else if (Array.IndexOf(PhaseN36L, s) >= 0) N36LevelBody(s, g, dir, res, offs, pre, ref body);
                    else if (Array.IndexOf(PhaseN36B, s) >= 0) N36BossBody(s, g, dir, res, offs, pre, ref body);
                    else if (Array.IndexOf(PhaseN37K, s) >= 0) N37KitBody(s, g, dir, res, offs, pre, ref body); // NIGHT37: by owner
                    else if (Array.IndexOf(PhaseN37W, s) >= 0) N37WickBody(s, g, dir, res, offs, pre, ref body);
                    else if (Array.IndexOf(PhaseN37S, s) >= 0) N37StagBody(s, g, dir, res, offs, pre, ref body);
                    else if (Array.IndexOf(PhaseN37O, s) >= 0) N37LoomBody(s, g, dir, res, offs, pre, ref body);
                    else if (Array.IndexOf(PhaseN37C, s) >= 0) N37CrownBody(s, g, dir, res, offs, pre, ref body);
                    else if (Array.IndexOf(PhaseN40B, s) >= 0) N40BrawlBody(s, g, dir, res, offs, pre, ref body); // NIGHT40 B2
                    else if (Array.IndexOf(PhaseN40E, s) >= 0) N40FoesBody(s, g, dir, res, offs, pre, ref body); // NIGHT40 E3
                    else if (Array.IndexOf(PhaseN41T, s) >= 0) N41ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT41 T2 (FeelReel.N41.cs)
                    else if (Array.IndexOf(PhaseN42, s) >= 0) N42ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT42 (FeelReel.N42.cs)
                    else if (Array.IndexOf(PhaseN43, s) >= 0) N43ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT43 (FeelReel.N43.cs)
                    else if (Array.IndexOf(PhaseN44V2, s) >= 0) N44Look(g, dir, res, offs, pre, ref body); // NIGHT44 V2 (FeelReel.N44.Look.cs)
                    else if (Array.IndexOf(PhaseN45, s) >= 0) N45ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT45 A1 / A2 / A3 (FeelReel.N45.cs)
                    else if (Array.IndexOf(PhaseN46, s) >= 0) N46ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT46 A1 / A2 / A3 / B1 / V1 (FeelReel.N46.cs)
                    else if (Array.IndexOf(PhaseN47, s) >= 0) N47ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT47 W / B / L / F (FeelReel.N47.cs)
                    else if (Array.IndexOf(PhaseN48, s) >= 0) N48ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT48 W / B / L / F (FeelReel.N48.cs)
                    else if (Array.IndexOf(PhaseN49, s) >= 0) N49ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT49 H / X / V / L (FeelReel.N49.cs)
                    else if (Array.IndexOf(PhaseN50, s) >= 0) N50ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT50 H (FeelReel.N50.cs)
                    else if (Array.IndexOf(PhaseN52, s) >= 0) N52ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT52 U / K (FeelReel.N52.cs)
                    else if (Array.IndexOf(PhaseN53, s) >= 0) N53ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT53 U (FeelReel.N53.cs)
                    else if (Array.IndexOf(PhaseN54, s) >= 0) N54ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT54 L / G / M / W (FeelReel.N54.cs)
                    else if (Array.IndexOf(PhaseN55, s) >= 0) N55ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT55 B / C / K / R / S (FeelReel.N55.cs)
                    else if (Array.IndexOf(PhaseN56, s) >= 0) N56ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT56 L / G / V (FeelReel.N56.cs)
                    else if (Array.IndexOf(PhaseN57, s) >= 0) N57ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT57 L / G / V (FeelReel.N57.cs)
                    else if (Array.IndexOf(PhaseN58, s) >= 0) N58ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT58 L / G / V (FeelReel.N58.cs)
                    break;
            }
            if (body != null) yield return body;
        }
    }
}
