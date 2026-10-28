// NIGHT71 builder B (slate B3 "a wyrm motion that reads", design.n71.wyrm71, a VIEW row: no tape bit). T-154 / GG 2026-10-22 "boss animations
// are too rigid": NIGHT63's bossMotion63 moves THE KILN WYRM inside the tightest guard of the five (design.n63.bossMotion63.numbers.bosses.wyrm:
// gain 0.6, the head's offset at most 0.25 of its half width = 0.375 m and 14 deg), so its stagger slump is set by the guard, not the slump
// (sink 0.6 x 1.5 m x 0.6 = 0.54 m, held to 0.375 m) and its breath is 2.4 cm: the b10 pairs read stagger 16,649 px (rule 8's bar 41,472) and
// idle 389 px (Logs/n63/verdict-B1.md:69,73). With the row on, THE KILN WYRM's motion parameters are bossMotion63's own (its `bosses.wyrm` entry
// over the shared numbers, BossMotionParams63.From) with design.n71.wyrm71.numbers over them: the gain, a deeper stagger squash and droop, a
// body wave that travels to the tail (breath lift / scale, tail share), ALL INSIDE BossMotion63's own guard for the wyrm (limitShare 0.25,
// limitDeg 14, limitSquash 0.32: Params holds the limits at bossMotion63's own; NIGHT71 B3 critic, Logs/n71/verdict-B3.md point 3, failed the
// first cut's 0.5 / 26). The sim's segments, hitboxes and damage frames are untouched (WyrmView draws the sim's boxes; only their drawing
// moves). A number the block does not give keeps bossMotion63's. Proof line: "[wyrm71] row on|off ..." (Line), the first drawn frame and
// each change (a same-run pair prints its off half; a -skygrindNoWyrm71 run prints why).
// THE SWITCH is read every frame (NightRows.ViewOn; FrameRows): the rig's springs are stepped the same with the row on or off (BossMotionRig63:
// only its params change, Spring63.Set keeps the state), so a same-run toggle's off frame is bossMotion63's own wyrm. Off: the row off,
// -skygrindOff71 (or older), -skygrindNoWyrm71. Hook: WyrmView.Wyrm71.cs (WyrmView.N63V.cs DrawMotion63 picks the params).
using System;
using System.Globalization;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class Wyrm71
    {
        public const string Row = "wyrm71";
        public const string NoSwitch = "-skygrindNoWyrm71";
        public const int Night = 71;
        public const string Root = "design.n71.wyrm71.numbers.";
        const string RowOff = "design.n71." + Row + " off";

        static string[] args;

        static string[] Args()
        {
            if (args == null)
            {
                try { args = Environment.GetCommandLineArgs() ?? new string[0]; }
                catch (Exception) { args = new string[0]; }
            }
            return args;
        }

        /// <summary>Why the row is off (null: on): the row (NightRows.ViewOn, the reel's live override first), then the off switches.</summary>
        public static string OffReason(GameConfig cfg, string[] argv)
        {
            if (cfg == null) return "no config";
            if (!NightRows.ViewOn(cfg, Row)) return RowOff;
            if (NightRows.OffFrom(argv, "-skygrindOff") <= Night) return "-skygrindOff71 (or older)";
            if (NightRows.HasSwitch(argv, NoSwitch)) return NoSwitch;
            return null;
        }

        /// <summary>Is the row on now? Read every frame (D1's same-run toggle flips ViewOn).</summary>
        public static bool On(GameConfig cfg) => OffReason(cfg, Args()) == null;

        /// <summary>THE KILN WYRM's parameters with the row on: bossMotion63's (BossMotionParams63.From(cfg, "wyrm")) with every number the block
        /// gives over them, EXCEPT THE GUARD: limitShare, limitDeg and limitSquash never pass bossMotion63's own for the wyrm (bosses.wyrm 0.25 /
        /// 14 deg, the shared 0.32), because the wyrm's drawn boxes are its hitboxes (WyrmView.N63V.cs, "a shot finds what you see"): the head's
        /// offset is held by the rig at limitShare x its half width and the body pieces' at limitShare x theirs (WyrmView.N63V.cs:51-53), so the
        /// limit IS the hitbox guard. NIGHT71 B3 critic (Logs/n71/verdict-B3.md point 3): the first cut set 0.5 / 26, 2.00 of that guard.</summary>
        public static BossMotionParams63 Params(GameConfig cfg)
        {
            BossMotionParams63 p = BossMotionParams63.From(cfg, BossAttackClock63.Wyrm);
            if (cfg == null) return p;
            float ownShare = p.limitShare, ownDeg = p.limitDeg, ownSquash = p.limitSquash;
            void O(string k, ref float f) { if (cfg.Has(Root + k)) f = (float)cfg.Num(Root + k); }
            O("gain", ref p.gain);
            O("limitShare", ref p.limitShare);
            O("limitDeg", ref p.limitDeg);
            O("limitSquash", ref p.limitSquash);
            O("rippleS", ref p.rippleS);
            O("tail", ref p.tail);
            O("wave", ref p.wave);
            O("breathHz", ref p.breathHz);
            O("breathLift", ref p.breathLift);
            O("breathScale", ref p.breathScale);
            O("sink", ref p.sink);
            O("slumpDeg", ref p.slumpDeg);
            O("slumpSquash", ref p.slumpSquash);
            O("flinchSquash", ref p.flinchSquash);
            O("recoil", ref p.recoil);
            // THE GUARD: a block number may only tighten it
            p.limitShare = Math.Min(p.limitShare, ownShare);
            p.limitDeg = Math.Min(p.limitDeg, ownDeg);
            p.limitSquash = Math.Min(p.limitSquash, ownSquash);
            return p;
        }

        /// <summary>The proof line (L-089 / L-115: the row's input printed), when the row reads on or off for the drawn wyrm (WyrmView.Wyrm71.cs:
        /// the first frame and each change, so a same-run pair prints its off half and a -skygrindNoWyrm71 run prints why).</summary>
        public static string Line(GameConfig cfg, bool on)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            if (!on) return "[wyrm71] row off (" + (OffReason(cfg, Args()) ?? "the reel's held frame") + "): THE KILN WYRM drawn with bossMotion63's own numbers";
            BossMotionParams63 p = Params(cfg), own = BossMotionParams63.From(cfg, BossAttackClock63.Wyrm);
            return "[wyrm71] row on: THE KILN WYRM drawn with design.n71.wyrm71 (gain " + p.gain.ToString("0.##", ci) + ", tail " + p.tail.ToString("0.##", ci)
                + ", breath " + p.breathHz.ToString("0.##", ci) + " Hz lift " + p.breathLift.ToString("0.###", ci) + " scale " + p.breathScale.ToString("0.###", ci)
                + ", slump deg " + p.slumpDeg.ToString("0.#", ci) + " squash " + p.slumpSquash.ToString("0.##", ci) + ") inside BossMotion63's own guard: limitShare "
                + p.limitShare.ToString("0.###", ci) + " (own " + own.limitShare.ToString("0.###", ci) + "), limitDeg " + p.limitDeg.ToString("0.#", ci) + " (own "
                + own.limitDeg.ToString("0.#", ci) + "), limitSquash " + p.limitSquash.ToString("0.##", ci) + " (own " + own.limitSquash.ToString("0.##", ci) + ")";
        }
    }
}
