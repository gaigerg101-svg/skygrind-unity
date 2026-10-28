// NIGHT72 builder B (slate B4, spring72, a VIEW row: NightRows.ViewOn(cfg, "spring72"); GG 2026-10-26 "I asked previously about stuff related colors and animations ... I just haven't seen it",
// 2026-10-22 "boss animations are too rigid"; scout-animation.md section 1: spring-driven secondary motion on every part, volume-preserving squash and stretch, idle breathing). EnemyLook's
// common reaction (React: every enemy view calls it once a frame for its body: husk, swarmer, spitter, charger, drone, warden ...) gets one more layer, on the body's whole squash:
//   - IDLE BREATHING: a slow volume-preserving swell (UnityTuning.Spring72BreathAmp of the height at Spring72BreathHz, the phase of its id: a crowd never breathes in step), while it is not
//     winding up; the rigid pose of NIGHT71 had none;
//   - WIND-UP SQUASH WITH ANTICIPATION: while the sim's wind-up runs (Enemy.windupSteps > 0) a damped spring (Spring72WindHz, z Spring72WindZeta) gathers the body down by Spring72WindSquash
//     of its height; the strike (the wind-up ends) lets the spring go: it overshoots into a stretch and settles (squash and stretch: anticipation, snap, settle);
//   - FLINCH FOLLOW-THROUGH: a hit (the sim's hp falling) kicks a second spring (Spring72FlinchHz, z Spring72FlinchZeta) that squashes the body and rings back through a stretch: the
//     body jiggles after the existing flinch (React's squash, hitzone47's head snap, the vibration) instead of stopping with it.
//   NIGHT73 (the lead, build n73-a: "applied 61 ON vs 0 OFF" but no player can see it; kill12 kills its husk with one shot so the flinch never fired, and its 8 % squash was about 10 px at 12 m):
//   the flinch is retuned to a 17 % squash that rings back through a 5 % stretch and is quiet by 0.15 s (UnityTuning.Spring72FlinchKick / Hz / Zeta), the spring73 reel (FeelReel.N73.B.cs) hits a
//   husk at 5 m five times without killing it, then kills it in its wind-up, and Spring72Probe.Last hands the reel each body's springs frame by frame.
//   Every squash is VOLUME-PRESERVING by construction (y by 1 + s, x and z by 1 / sqrt(1 + s): the product of scales is 1), pivots on the feet (the root's origin), is capped at +-Spring72MaxSquash,
//   and is not drawn under REDUCED MOTION or EFFECTS 0 (the springs still run). RENDER ONLY: no hit shape, no sim read for writing, no timing, no fingerprint input; the springs run on FRAME time.
//   The bosses' own springs are bossMotion63's, wyrm71's and body72's: this layer is the bodies the sim keeps in game.enemies.
// THE SWITCH is read every frame (D1's contract: the springs are stepped the same with the row off, so the next frame on is the frame it would have drawn; only the application waits).
// Proof (L-014 / L-115; Player.log, every Spring72SayEveryS s of bodies drawn, row on or off):
//   [spring72] row spring72 on|off: <b> body-frames, applied <a>, scale rms applied <r> | the springs' own <o> (idle breath peak <p>, wind-up squash peak <w> at <kind>, flinch squash peak <h> at
//   <kind>) | volume product <lo>..<hi> of the applied
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>What spring72's springs did since the last line (the reel and the log read it): the applied numbers are zero with the row off.</summary>
    public static class Spring72Probe
    {
        public static int Bodies, Applied, Kicks;
        public static double SumSqOwn, SumSqApplied, WindPeak, FlinchPeak, BreathPeak, ProductLo = 2, ProductHi = 0;
        public static string WindKind = "", FlinchKind = "";
        public static bool On;
        public static void Reset() { Bodies = Applied = 0; SumSqOwn = SumSqApplied = WindPeak = FlinchPeak = BreathPeak = ProductHi = 0; ProductLo = 2; WindKind = FlinchKind = ""; }

        /// <summary>NIGHT73 (the spring73 reel, FeelReel.N73.B.cs): one body's springs as of the last frame its view drew them: the flinch and wind-up springs' shares (signed: a squash is
        /// negative), the idle breath, the clamped sum (the height's change when the row draws it), the frame, and whether it was drawn (row on and motion allowed). Written once a frame per
        /// body, row on or off; read by the reel only (the per-second totals above are reset by the proof line, so a reel cannot read a hit's peak from them).</summary>
        public struct Sample { public float flinch, wind, breath, own; public int frame; public bool drawn; }

        /// <summary>The latest Sample of each live body by enemy id (a body that is gone is forgotten in EnemyLook.Spring72Frame).</summary>
        public static readonly Dictionary<int, Sample> Last = new Dictionary<int, Sample>();
    }

    public sealed partial class EnemyLook
    {
        public const string Spring72Row = "spring72";

        sealed class S72
        {
            public float breathPhase, windX, windV, flinchX, flinchV;
            public float hit74X, hit74V, lastKick74 = -99f, kick74, hold74; // NIGHT74 B spring74 (EnemyViews.Spring74.cs): the hit squash spring, the time of its last kick, the kick's squash and the hold left (s)
            public double lastHp74 = double.NaN;
            public double lastHp = double.NaN;
            public int stamp = -1;
            public string kind = "";
        }

        readonly Dictionary<int, S72> st72 = new Dictionary<int, S72>();
        readonly Dictionary<int, Enemy> byId72 = new Dictionary<int, Enemy>();
        readonly List<int> gone72 = new List<int>();
        float sayClock72;

        /// <summary>From Frame (once a frame, before the views tint): the live bodies by id, the springs of the gone forgotten, the row read, the proof line's clock.</summary>
        void Spring72Frame()
        {
            byId72.Clear();
            if (game.enemies != null) foreach (Enemy e in game.enemies.all) if (e != null && e.alive) byId72[e.id] = e;
            gone72.Clear();
            foreach (int id in st72.Keys) if (!byId72.ContainsKey(id)) gone72.Add(id);
            foreach (int id in gone72) { st72.Remove(id); Spring72Probe.Last.Remove(id); }
            Spring72Probe.On = NightRows.ViewOn(game.cfg, Spring72Row);
            if (Spring72Probe.Bodies > 0)
            {
                sayClock72 += Mathf.Min(Time.deltaTime, 0.05f);
                if (sayClock72 >= UnityTuning.Spring72SayEveryS) { sayClock72 = 0f; Spring72Say(); }
            }
        }

        /// <summary>From React: this body's squash with the springs' layer (the row on); the springs run either way.</summary>
        void Spring72React(int id, ref Vector3 squash)
        {
            if (!byId72.TryGetValue(id, out Enemy e)) return;
            if (!st72.TryGetValue(id, out S72 s))
            {
                s = new S72 { breathPhase = (id * 0.6180339f % 1f) * 2f * Mathf.PI, kind = e.kind ?? "" };
                st72[id] = s;
            }
            if (s.stamp == Time.frameCount) return; // once a frame per body (React may be asked twice by a reel)
            s.stamp = Time.frameCount;
            if (!wired72) Spring72Wire();
            if (!first72)
            {
                first72 = true;
                MovementConfig mc0 = game.cfg.movement;
                Debug.Log("[spring72] first body: " + (e.kind ?? "?") + " id " + id + ", row spring72 " + (Spring72Probe.On ? "on" : "off") + ", motion "
                    + (FxGates.MotionOn(mc0.settings.reducedMotion, mc0.settings.effectsIntensity) ? "allowed" : "off (reduced motion or effects 0: the springs run, nothing is drawn)")
                    + ", frame " + Time.frameCount);
            }
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            // the wind-up spring: down while it runs, loose when it ends
            float windTarget = e.windupSteps > 0 ? -UnityTuning.Spring72WindSquash : 0f;
            Spring72Math.Step(ref s.windX, ref s.windV, windTarget, UnityTuning.Spring72WindHz, UnityTuning.Spring72WindZeta, dt);
            // the flinch spring: a hit (hp fell) kicks it; it rings back to 0
            if (!double.IsNaN(s.lastHp) && e.hp < s.lastHp - 1e-6)
            {
                s.flinchV += UnityTuning.Spring72FlinchKick;
                Spring72Probe.Kicks++;
                if (Spring72Probe.Kicks <= UnityTuning.Spring72KickLines)
                    Debug.Log("[spring72] flinch kick " + Spring72Probe.Kicks + ": " + (e.kind ?? "?") + " id " + id + ", hp " + s.lastHp.ToString("0.0", CultureInfo.InvariantCulture) + " -> "
                        + e.hp.ToString("0.0", CultureInfo.InvariantCulture) + ", speed " + UnityTuning.Spring72FlinchKick.ToString("0.0", CultureInfo.InvariantCulture) + " | row spring72 " + (Spring72Probe.On ? "on" : "off"));
            }
            s.lastHp = e.hp;
            Spring72Math.Step(ref s.flinchX, ref s.flinchV, 0f, UnityTuning.Spring72FlinchHz, UnityTuning.Spring72FlinchZeta, dt);
            // the breath (not while it winds up)
            float breath = e.windupSteps > 0 ? 0f : UnityTuning.Spring72BreathAmp * Mathf.Sin(Time.time * 2f * Mathf.PI * UnityTuning.Spring72BreathHz + s.breathPhase);
            float sy = Mathf.Clamp(breath + s.windX + s.flinchX, -UnityTuning.Spring72MaxSquash, UnityTuning.Spring72MaxSquash);
            // the probe: the springs' own motion (the row on or off), then what was applied
            Spring72Probe.Bodies++;
            Spring72Probe.SumSqOwn += sy * sy;
            if (Mathf.Abs(breath) > Spring72Probe.BreathPeak) Spring72Probe.BreathPeak = Mathf.Abs(breath);
            if (-s.windX > Spring72Probe.WindPeak) { Spring72Probe.WindPeak = -s.windX; Spring72Probe.WindKind = s.kind; }
            if (Mathf.Abs(s.flinchX) > Spring72Probe.FlinchPeak) { Spring72Probe.FlinchPeak = Mathf.Abs(s.flinchX); Spring72Probe.FlinchKind = s.kind; }
            MovementConfig mc = game.cfg.movement;
            bool draw = Spring72Probe.On && FxGates.MotionOn(mc.settings.reducedMotion, mc.settings.effectsIntensity);
            Spring72Probe.Last[id] = new Spring72Probe.Sample { flinch = s.flinchX, wind = s.windX, breath = breath, own = sy, frame = Time.frameCount, drawn = draw }; // NIGHT73: the reel's read-back
            Spring74React(id, s, e, dt, mc, ref squash); // NIGHT74 B spring74 (EnemyViews.Spring74.cs): the hit squash a glance sees (its own row; the springs above run and draw as before)
            if (!draw) return;
            float y = 1f + sy, xz = 1f / Mathf.Sqrt(y);
            squash = Vector3.Scale(squash, new Vector3(xz, y, xz));
            Spring72Probe.Applied++;
            Spring72Probe.SumSqApplied += sy * sy;
            double prod = (double)y * xz * xz;
            if (prod < Spring72Probe.ProductLo) Spring72Probe.ProductLo = prod;
            if (prod > Spring72Probe.ProductHi) Spring72Probe.ProductHi = prod;
        }

        bool wired72, first72;

        /// <summary>Once a session: the quit prints what is left (the t1 round's kill12 ended before any interval line).</summary>
        void Spring72Wire()
        {
            wired72 = true;
            Application.quitting += () => { if (Spring72Probe.Bodies > 0) Spring72Say(); };
            game.OnNewRun += () => { if (Spring72Probe.Bodies > 0) Spring72Say(); first72 = false; Spring72Probe.Kicks = 0; };
        }

        void Spring72Say()
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            double rmsApplied = Spring72Probe.Applied > 0 ? System.Math.Sqrt(Spring72Probe.SumSqApplied / Spring72Probe.Applied) : 0;
            double rmsOwn = Spring72Probe.Bodies > 0 ? System.Math.Sqrt(Spring72Probe.SumSqOwn / Spring72Probe.Bodies) : 0;
            Debug.Log("[spring72] row spring72 " + (Spring72Probe.On ? "on" : "off") + ": " + Spring72Probe.Bodies + " body-frames, applied " + Spring72Probe.Applied + ", scale rms applied " + rmsApplied.ToString("0.0000", ci)
                + " | the springs' own " + rmsOwn.ToString("0.0000", ci) + " (idle breath peak " + Spring72Probe.BreathPeak.ToString("0.000", ci) + ", wind-up squash peak " + Spring72Probe.WindPeak.ToString("0.000", ci)
                + " at " + (Spring72Probe.WindKind.Length == 0 ? "-" : Spring72Probe.WindKind) + ", flinch squash peak " + Spring72Probe.FlinchPeak.ToString("0.000", ci) + " at "
                + (Spring72Probe.FlinchKind.Length == 0 ? "-" : Spring72Probe.FlinchKind) + ") | volume product "
                + (Spring72Probe.Applied > 0 ? Spring72Probe.ProductLo.ToString("0.0000", ci) + ".." + Spring72Probe.ProductHi.ToString("0.0000", ci) : "-") + " of the applied");
            Spring72Probe.Reset();
        }
    }
}
