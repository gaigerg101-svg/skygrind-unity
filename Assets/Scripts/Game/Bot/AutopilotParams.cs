// NIGHT14 U3-27.5: design.autopilot read once (PORT-RULES 6: no config lookup on the step path). Every number here is a
// Unity-side gameplay number with its _why in Design/unity-config.json: the web has no SURVIVAL bot, so it has no target
// choice for a horde, no shopping, no gun choice past the duel's two guns and no MODS / FORGE choices. No UnityEngine use
// (Tools/SimHarness compiles this file).
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed class AutopilotParams
    {
        public bool enabled, takeFirstMod, forge;
        public string difficulty;
        /// <summary>The shopping order (Buyables ids), THE FORGE ("forge") appended when design.autopilot.forge and not listed.</summary>
        public readonly List<string> shopping = new List<string>();
        public double targetSwitchMeters;
        /// <summary>A shopping goal not bought after this much trying is dropped for the run (an unreachable machine).</summary>
        public double shopGiveUpSeconds;
        public int shopRetryRounds; // NIGHT14 B-7
        /// <summary>The guns the autopilot prefers near and far (ids in hand order of preference; the first one owned wins).</summary>
        public readonly List<string> gunsNear = new List<string>(), gunsFar = new List<string>();
        /// <summary>botBrain.js's own weapon hysteresis around bot.shotgunRange (a literal 1 in the web's code).</summary>
        public double gunHysteresisMeters;
        /// <summary>Without line of sight, the nearest this many enemies (by straight distance) are costed on the graph.</summary>
        public int graphCandidates;
        /// <summary>The brain's own Mulberry32 stream: the run seed XOR this salt (NIGHT14 rule 8).</summary>
        public uint rngSalt;
        /// <summary>A HOLD ring is held from within this share of objectives.radius (the bot walks in until then).</summary>
        public double holdInsideShare;
        /// <summary>objectives.js's inside test's height (|dy| under 2.5, a literal in the web's code).</summary>
        public double holdHeightMeters;
        /// <summary>On the way to a HOLD ring, an enemy in sight this near is fought first.</summary>
        public double holdFightMeters;

        // config.bot numbers the adapter reads (the web's own: the eye, the chest, the line-of-sight cadence, the gun ranges)
        public double eyeHeight, chestHeight, losInterval, replanInterval, shotgunRange;

        public static AutopilotParams From(GameConfig cfg)
        {
            var p = new AutopilotParams
            {
                enabled = cfg.Design("autopilot.enabled"),
                takeFirstMod = cfg.Design("autopilot.takeFirstMod"),
                forge = cfg.Design("autopilot.forge"),
                difficulty = cfg.Str("design.autopilot.difficulty"),
                targetSwitchMeters = cfg.Num("design.autopilot.targetSwitchMeters"),
                shopGiveUpSeconds = cfg.Num("design.autopilot.shopGiveUpSeconds"),
                shopRetryRounds = cfg.Int("design.autopilot.shopRetryRounds"),
                gunHysteresisMeters = cfg.Num("design.autopilot.gunHysteresisMeters"),
                graphCandidates = cfg.Int("design.autopilot.graphCandidates"),
                rngSalt = (uint)cfg.Num("design.autopilot.rngSalt"),
                holdInsideShare = cfg.Num("design.autopilot.holdInsideShare"),
                holdHeightMeters = cfg.Num("design.autopilot.holdHeightMeters"),
                holdFightMeters = cfg.Num("design.autopilot.holdFightMeters"),
                eyeHeight = cfg.Num("bot.eyeHeight"),
                chestHeight = cfg.Num("bot.chestHeight"),
                losInterval = cfg.Num("bot.losInterval"),
                replanInterval = cfg.Num("bot.replanInterval"),
                shotgunRange = cfg.Num("bot.shotgunRange"),
            };
            p.shopping.AddRange(cfg.Strs("design.autopilot.shopping"));
            if (p.forge && !p.shopping.Contains("forge")) p.shopping.Add("forge");
            p.gunsNear.AddRange(cfg.Strs("design.autopilot.guns.near"));
            p.gunsFar.AddRange(cfg.Strs("design.autopilot.guns.far"));
            return p;
        }
    }
}
