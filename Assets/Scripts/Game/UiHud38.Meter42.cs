// NIGHT42 builder U1 (design.n42.meter42, a view row: no sim bit; NIGHT42.md U1, GG's G7: "the meter or how the pulse or charge attack works ... needs
// to be polished and more accurate"). With the row on (and arc40 + hud38 on: it draws on arc40's UiArc40), the arc speaks ONE language:
//   - the OUTER BAND is the gun's special: the KESTREL's / THUNDERCLAP's charge hold, or the PULSE's BLAST (one segment per kill pip);
//       CHARGING  the fill IS the hold's level (Loadout.Charge35Level, linear, unrounded: arc40's level^1.6 showed 33 % at a half charge), bright bone,
//                 a white head at its front;
//       FULL      cyan, only when the hold is really FULL (or the glove full and the BLAST's cooldown run out): a white flash, then it breathes until
//                 it is spent (arc40 drew a charge gun at rest as a full cyan bar and flashed on every tap's release: the "false FULL");
//       COOLDOWN  the special locked: the band REFILLS left to right in a dim bone with a bone head (arc40 drained right to left);
//       READY     (a charge gun at rest) the empty track: "ready to charge"; a cooldown's end pings the track cyan and fades back.
//   - the INNER LINES are cooldowns only, refilling left to right: line 1 the BLAST's 15 s cooldown (as arc40's), line 2 the gun's NORMAL attack
//     recovering: the PULSE's swing (2.5 s today, 1.8 s with W1's gauntlet42; read from the gun's own cooldown, so W1's number shows as it is) and
//     the THUNDERCLAP's pump. arc40 never drew the swing. A line that completes flashes white, then goes.
// The STITCHER keeps arc40's heat. hud38's ready event (Hud38.Ready, FeelProbe hud38Ready) is untouched: it still fires on hud38's own edges.
// Measured: Hud42's counters compare the shown fill with the telemetry's `charge` (FeelProbe.Weapons41Frame's own read) every drawn frame, for
// meter42 and for what arc40 would have drawn in the same frame (the reel ui42 prints both). Off (meter42 off, -skygrindOff42 or any older switch):
// nothing here runs and arc40 draws as before. View only: reads the meter, writes nothing back. Numbers: UnityTuning.Ui42.cs.
using System;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>NIGHT42 meter42's statics: the reels read them (the Ui instance is private to SurvivalGame).</summary>
    public static class Hud42
    {
        /// <summary>meter42 drew the arc this frame; its state ("REST", "CHARGING", "FULL", "COOLDOWN", "PIPS", "" hidden), the outer fill, the lines
        /// (-1 none) and the true charge it was checked against (the telemetry's `charge`).</summary>
        public static bool Live;
        public static string State = "";
        public static float Fill = -1f, Line = -1f, Line2 = -1f, Truth;
        /// <summary>Since the run's start: frames drawn on a charge gun or the gauntlet; frames whose shown charge differs from the truth (meter42 /
        /// arc40 in the same frame); frames drawn cyan FULL while the hold was not FULL (meter42 / arc40); FULL flashes; cooldown pings; swing frames.</summary>
        public static int Frames, Mismatch, MismatchArc40, FalseFull, FalseFullArc40, Flashes, Pings, SwingFrames;
        /// <summary>The largest |shown - truth| seen (meter42 / arc40).</summary>
        public static float WorstErr, WorstErrArc40;

        public static void ResetCounters()
        {
            Frames = Mismatch = MismatchArc40 = FalseFull = FalseFullArc40 = Flashes = Pings = SwingFrames = 0;
            WorstErr = WorstErrArc40 = 0f;
        }
    }

    /// <summary>The arc's content this frame in meter42's language (pure: Hud42Math.Read).</summary>
    public struct Arc42
    {
        public bool show; // the arc has something to say (as Meter38.show)
        public bool charge; // a charge gun (the KESTREL / THUNDERCLAP with the charge row live)
        public int mode; // Hud42Math.Rest / Charging / Full / Cooling / Pips
        public int segs; // 1, or the BLAST's pip count
        public float fill; // the outer band: the charge's level (linear), a cooldown's REFILL (0 .. 1 = usable), the pips' share
        public float line; // the BLAST's cooldown refilled (0..1), -1 none
        public float line2; // the normal attack's recovery refilled (0..1): the PULSE's swing, a charge gun's own slow cycle (the pump); -1 none
    }

    /// <summary>NIGHT42 meter42's rules (pure; EditMode tests: Hud42Tests).</summary>
    public static class Hud42Math
    {
        public const int Rest = 0, Charging = 1, Full = 2, Cooling = 3, Pips = 4;

        /// <summary>A cooldown's refill: 0 just started .. 1 done (hud38's Refill: left 0 = 1).</summary>
        public static float Refill(int left, int full) => Hud38Math.Refill(left, Math.Max(full, left));

        /// <summary>
        /// The gun in hand's arc from hud38's inputs (Hud38Math.Read's): `charge` a charge gun; `holding` a real hold at `level`; the gun's own
        /// cooldown `cdLeft` / `cdFull` (the PULSE's swing, the THUNDERCLAP's pump; shown when it is at least Hud30MinCoolSeconds, hud38's rule);
        /// the charge shot's cooldown `chLeft` / `chFull`; the gauntlet's `pips` (`pipsNow` / `pipsMax`) and the BLAST's cooldown `blLeft` / `blFull`.
        /// </summary>
        public static Arc42 Read(bool charge, bool holding, double level, int cdLeft, int cdFull, int chLeft, int chFull, bool pips, int pipsNow, int pipsMax, int blLeft, int blFull, double dt)
        {
            var a = new Arc42 { segs = 1, line = -1f, line2 = -1f, mode = Rest };
            bool own = Hud38Math.Shows(cdFull, cdLeft, dt);
            if (pips)
            {
                a.show = true;
                a.segs = Math.Max(1, Math.Min(UnityTuning.Hud40MaxSegments, pipsMax));
                a.fill = pipsMax > 0 ? Clamp01(pipsNow / (float)pipsMax) : 1f;
                if (blLeft > 0) a.line = Refill(blLeft, blFull);
                a.mode = a.fill >= 1f && blLeft <= 0 ? Full : Pips;
                if (own) a.line2 = Refill(cdLeft, cdFull); // the swing (arc40 never drew it)
                return a;
            }
            if (charge)
            {
                a.show = true;
                a.charge = true;
                if (holding)
                {
                    a.fill = Clamp01((float)level);
                    a.mode = level >= 1 ? Full : Charging;
                }
                else if (chLeft > 0)
                {
                    a.mode = Cooling;
                    a.fill = Refill(chLeft, chFull);
                }
                if (own) a.line2 = Refill(cdLeft, cdFull); // the pump: the normal shot recovering, never the special's band
                return a;
            }
            if (own)
            {
                a.show = true;
                a.mode = Cooling;
                a.fill = Refill(cdLeft, cdFull);
            }
            return a;
        }

        /// <summary>The arc's FULL (cyan): only a real FULL hold or a full glove with the BLAST ready.</summary>
        public static bool IsFull(Arc42 a) => a.mode == Full;

        /// <summary>The state word the reels print.</summary>
        public static string Word(int mode) => mode == Charging ? "CHARGING" : mode == Full ? "FULL" : mode == Cooling ? "COOLDOWN" : mode == Pips ? "PIPS" : "REST";

        /// <summary>The ping's / flash's edges: a cooldown ending into rest pings; reaching FULL flashes.</summary>
        public static bool Pinged(int was, int now) => was == Cooling && now == Rest;
        public static bool Flashed(int was, int now) => was != Full && now == Full;

        /// <summary>What arc40 showed for a hold at `level` (its eased fill rounded to hud38's step): the "before" in the reel's count.</summary>
        public static float Arc40Shown(double level) => Mathf.Round(Hud40Math.Ease(level) / UnityTuning.Hud38FillStep) * UnityTuning.Hud38FillStep;

        static float Clamp01(float v) => v < 0 ? 0 : v > 1 ? 1 : v;
    }

    public sealed partial class Ui
    {
        bool meter42On, meter42Read, h42Live, h42Built;
        Arc42 h42Now;
        int h42Mode = -1;
        string h42Gun;
        double h42FlashAge = double.MaxValue, h42PingAge = double.MaxValue, h42LineDone = double.MaxValue, h42Line2Done = double.MaxValue;
        float h42LastLine = -1f, h42LastLine2 = -1f;

        bool Meter42On
        {
            get
            {
                if (!meter42Read && cfg != null)
                {
                    meter42On = N42View.RowOn(cfg, "meter42");
                    meter42Read = true;
                }
                return meter42On;
            }
        }

        /// <summary>The telemetry's `charge` this frame (FeelProbe.Weapons41Frame's read, the same order): the level the arc must show.</summary>
        double TrueCharge42()
        {
            Loadout lo = game.loadout;
            if (lo == null) return 0;
            double level = lo.Charge35Level;
            ChargeShots27 cs = lo.charge27 != null && lo.charge27.active ? lo.charge27 : null;
            if (level <= 0 && cs != null && cs.charging)
            {
                int gi = ChargeShots27.Index(lo.Weapon);
                if (gi >= 0) level = cs.Level(gi);
            }
            return Math.Max(0, Math.Min(1, level));
        }

        /// <summary>Arc40Draw's meter42 branch (h42Live): the arc in one language. `show` is Bar38's (hud38's hide rule).</summary>
        void Meter42Draw(bool show, double frameDt)
        {
            if (!h42Built)
            {
                h42Built = true;
                game.OnNewRun += () => { h42Gun = null; h42Mode = -1; h42FlashAge = h42PingAge = h42LineDone = h42Line2Done = double.MaxValue; h42LastLine = h42LastLine2 = -1f; Hud42.ResetCounters(); };
            }
            Arc42 a = h42Now;
            GameObject go = h40Arc.gameObject;
            if (go.activeSelf != show) go.SetActive(show);
            Hud38.BarShown = Hud40.ArcShown = Hud42.Live = show;
            Hud38.BarGun = show ? h38Gun ?? "" : "";
            // a switch: the new gun as it is, no flash, no ping
            if (h38Gun != h42Gun)
            {
                h42Gun = h38Gun;
                h42Mode = a.mode;
                h42FlashAge = h42PingAge = h42LineDone = h42Line2Done = double.MaxValue;
                h42LastLine = a.line;
                h42LastLine2 = a.line2;
            }
            if (Hud42Math.Flashed(h42Mode, a.mode)) { h42FlashAge = 0; Hud42.Flashes++; }
            if (Hud42Math.Pinged(h42Mode, a.mode)) { h42PingAge = 0; Hud42.Pings++; }
            if (h42LastLine > 0f && a.line < 0f) h42LineDone = 0;
            if (h42LastLine2 > 0f && a.line2 < 0f) h42Line2Done = 0;
            h42Mode = a.mode;
            h42LastLine = a.line;
            h42LastLine2 = a.line2;
            // the check against the telemetry's `charge` (every drawn frame on a charge gun or the gauntlet), meter42 and arc40 alike
            if (show && (a.charge || a.segs > 1 || a.line2 >= 0f || a.mode == Hud42Math.Full))
            {
                double truth = TrueCharge42();
                Hud42.Truth = (float)truth;
                Hud42.Frames++;
                if (a.line2 >= 0f && !a.charge) Hud42.SwingFrames++;
                if (a.charge)
                {
                    bool charging = a.mode == Hud42Math.Charging || a.mode == Hud42Math.Full;
                    float shown = charging ? a.fill : 0f; // a cooldown's refill is not a charge: it must never read as one
                    float err = Mathf.Abs(shown - (float)truth);
                    if (err > UnityTuning.Meter42MatchTol) Hud42.Mismatch++;
                    if (err > Hud42.WorstErr) Hud42.WorstErr = err;
                    if (a.mode == Hud42Math.Full && truth < 1) Hud42.FalseFull++;
                    // arc40 in the same frame: its eased, rounded fill while holding; cyan FULL whenever hud38's meter is ready
                    Arc40 b = h40Now;
                    float before = b.mode == Hud40Math.Charging ? Hud42Math.Arc40Shown(truth) : 0f;
                    float errB = Mathf.Abs(before - (float)truth);
                    if (errB > UnityTuning.Meter42MatchTol) Hud42.MismatchArc40++;
                    if (errB > Hud42.WorstErrArc40) Hud42.WorstErrArc40 = errB;
                    if (b.ready && truth < 1) Hud42.FalseFullArc40++;
                }
            }
            if (!show)
            {
                Hud38.BarFill = Hud40.ArcFill = Hud40.ArcLine = Hud42.Fill = Hud42.Line = Hud42.Line2 = -1f;
                Hud38.BarCells = Hud40.ArcSegs = 0;
                Hud38.BarState = Hud40.ArcState = Hud42.State = "";
                AgeMeter42(frameDt);
                return;
            }
            int n = Math.Max(1, Math.Min(UnityTuning.Hud40MaxSegments, a.segs));
            Color bone = UiStyle33.Bone, cyan = H50Live ? Token50 : UiStyle33.Cyan; // NIGHT50 H charge50: FULL in the weapon's Look49 token
            Color fillTone = H50Live ? Token50 : bone; // NIGHT50 H charge50: the charge's and the refill's fill in the token (the track and the lines stay bone)
            Color track = UiTheme.A(bone, UnityTuning.Meter42TrackAlpha), bar, headCol = Color.clear;
            float grow = 0f, head = -1f;
            bool full = a.mode == Hud42Math.Full;
            if (full)
            {
                if (h42FlashAge < UnityTuning.Hud38FlashTime)
                {
                    float k = (float)(h42FlashAge / UnityTuning.Hud38FlashTime);
                    bar = Color.Lerp(Color.white, cyan, k);
                    grow = UnityTuning.Meter42FlashGrow * (1f - UiTween.OutCubic(k));
                }
                else
                {
                    // held FULL: it breathes until it is spent
                    float br = 0.5f + 0.5f * Mathf.Cos((float)((h42FlashAge - UnityTuning.Hud38FlashTime) * UnityTuning.Meter42BreatheHz * 2.0 * Math.PI));
                    bar = UiTheme.A(cyan, Mathf.Round(Mathf.Lerp(UnityTuning.Meter42BreatheLow, 1f, br) * 50f) / 50f);
                }
            }
            else if (a.mode == Hud42Math.Cooling || (a.mode == Hud42Math.Pips && a.line >= 0f))
                bar = UiTheme.A(fillTone, H50Live ? UnityTuning.H50CoolAlpha : UnityTuning.Meter42CoolAlpha);
            else bar = UiTheme.A(fillTone, UnityTuning.Meter42ChargeAlpha);
            if (a.mode == Hud42Math.Charging && a.fill > 0f) { head = a.fill; headCol = UiTheme.A(Color.white, UnityTuning.Meter42HeadAlpha); }
            else if (a.mode == Hud42Math.Cooling && a.fill > 0f) { head = a.fill; headCol = UiTheme.A(bone, UnityTuning.Meter42HeadAlpha * 0.9f); }
            if (h42PingAge < UnityTuning.Meter42PingTime)
            {
                float k = (float)(h42PingAge / UnityTuning.Meter42PingTime);
                track = Color.Lerp(cyan, track, UiTween.OutCubic(k));
                grow = Mathf.Max(grow, UnityTuning.Meter42PingGrow * (1f - k));
            }
            float line = a.line, line2 = a.line2;
            Color lineCol = UiTheme.A(bone, UnityTuning.Hud40LineAlpha), line2Col = UiTheme.A(bone, UnityTuning.Meter42Line2Alpha);
            if (line < 0f && h42LineDone < UnityTuning.Meter42LineDoneTime) { line = 1f; lineCol = Color.white; }
            if (line2 < 0f && h42Line2Done < UnityTuning.Meter42LineDoneTime) { line2 = 1f; line2Col = Color.white; }
            float g = Mathf.Round(grow * 2f) / 2f;
            UiArc40 arc = h40Arc;
            bool dirty = false;
            if (arc.radius != Radius50) { arc.radius = Radius50; dirty = true; } // NIGHT50 H charge50: re-placed (Hud40Radius otherwise)
            if (arc.segs != n) { arc.segs = n; dirty = true; }
            if (arc.fill != a.fill) { arc.fill = a.fill; dirty = true; }
            if (arc.line != line) { arc.line = line; dirty = true; }
            if (arc.line2 != line2) { arc.line2 = line2; dirty = true; }
            if (arc.head != head) { arc.head = head; dirty = true; }
            if (arc.grow != g) { arc.grow = g; dirty = true; }
            if (arc.headGrow != UnityTuning.Meter42HeadGrow) { arc.headGrow = UnityTuning.Meter42HeadGrow; dirty = true; }
            if (arc.bar != bar) { arc.bar = bar; dirty = true; }
            if (arc.track != track) { arc.track = track; dirty = true; }
            if (arc.lineColor != lineCol) { arc.lineColor = lineCol; dirty = true; }
            if (arc.line2Color != line2Col) { arc.line2Color = line2Col; dirty = true; }
            if (arc.headColor != headCol) { arc.headColor = headCol; dirty = true; }
            if (dirty) arc.Changed();
            AgeMeter42(frameDt);
            Hud38.BarFill = Hud40.ArcFill = Hud42.Fill = a.fill;
            Hud38.BarCells = Hud40.ArcSegs = n;
            Hud40.ArcLine = Hud42.Line = a.line;
            Hud42.Line2 = a.line2;
            Hud38.BarState = full ? "FULL" : "CHARGING";
            Hud40.ArcState = Hud42.State = Hud42Math.Word(a.mode);
        }

        void AgeMeter42(double frameDt)
        {
            AgeFlash38(frameDt); // hud38's own flash clock keeps running as on arc40's path (Tint38's readers)
            if (h42FlashAge < double.MaxValue) h42FlashAge += frameDt;
            if (h42PingAge < double.MaxValue) h42PingAge = h42PingAge > UnityTuning.Meter42PingTime ? double.MaxValue : h42PingAge + frameDt;
            if (h42LineDone < double.MaxValue) h42LineDone = h42LineDone > UnityTuning.Meter42LineDoneTime ? double.MaxValue : h42LineDone + frameDt;
            if (h42Line2Done < double.MaxValue) h42Line2Done = h42Line2Done > UnityTuning.Meter42LineDoneTime ? double.MaxValue : h42Line2Done + frameDt;
        }
    }
}
