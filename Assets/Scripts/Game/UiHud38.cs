// NIGHT38 builder H (design.n38.hud38, a view row: no sim bit; PLAN.md section 4; GG 2026-10-13: the charge readiness "similar to ultra kill ... three
// bars or ... a bar under the crosshair", the crosshair rotating while reloading "more of a thing", the grapple indicator "too bland"). With the row on
// (and hud33's HUD on: it lives on hud33's reticle), under the crosshair:
//   - THE BAR: three cells for the gun in hand (Hud38Math.Read): the KESTREL's / THUNDERCLAP's charge while held (Loadout.Charge35Level or
//     ChargeShots27.Level), then the charge shot's cooldown refilling; a slow gun's own cooldown (Loadout.CooldownStepsOf, at least Hud30MinCoolSeconds:
//     the THUNDERCLAP's pump; no flicker per pistol shot); the gauntlet's BLAST: one cell per kill pip (PulseCharge27; hidden today under hud33's values
//     layer, UiHotbar.cs:143 / UiHud33.cs:119-124) and the blast's cooldown. ULTRAKILL's rule (RailcannonMeter / StaminaMeter): bone while charging, CYAN
//     when full, a white flash fading to cyan on the frame it fills (+ the event hud38Ready: Hud38.Ready(id) and a FeelProbe event, for GG's sound).
//     A gun with no charge and no pips shows the bar only while it cools and Hud38HideAfter s after.
//   - THE SPIN: hud33's four ticks turn a full turn through each reload / cooldown of the gun in hand (the same cooldowns as the bar, never the charge
//     hold: the arcs show that) and snap square when it is ready (the ticks push out and flash white for Hud38SnapTime). The kill's 45-degree X adds on.
//   - THE GRAPPLE: a hook icon (a J with a barb and an eye bar) with two charge pips beside it, on its own row under the bar (hud33's two slash ticks
//     hide); the hook is cyan with a charge, dim without; a returning charge flashes its pip white (hud38Ready "grapple"); a throw pops the hook.
// Off (the row off, -skygrindOff38 / --off38 or any older switch, or hud33 off): nothing here is built and the reticle is hud33's.
// View only: reads sim / loadout state, writes nothing back. Numbers: UnityTuning.Hud38.cs.
using System;
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    /// <summary>NIGHT38 hud38's statics: the reels read them (the Ui instance is private to SurvivalGame) and GG's sound hook.</summary>
    public static class Hud38
    {
        /// <summary>A NIGHT38 view row is on: its `enabled` and not --off38 / -skygrindOff38 (N38Blocks.ForceOff).</summary>
        public static bool RowOn(GameConfig cfg, string row) => !N38Blocks.ForceOff && cfg != null && cfg.Design("n38." + row + ".enabled");

        /// <summary>The bar and the grapple row are drawn this frame.</summary>
        public static bool Live;
        /// <summary>The bar now: shown, its fill (0..1; -1 hidden), its cells, "CHARGING" / "FULL" / "", the gun it reads.</summary>
        public static bool BarShown;
        public static float BarFill = -1f;
        public static int BarCells;
        public static string BarState = "", BarGun = "";
        /// <summary>The ticks' spin now (deg, the reload's share of a full turn; 0 at rest) and the snaps / ready flashes so far this run.</summary>
        public static float SpinDeg;
        public static int Snaps, ReadyFlashes;
        /// <summary>The grapple's full charges (-1: no grapple shown).</summary>
        public static int GrappleFull = -1;

        /// <summary>A meter filled this frame (id: the gun, "blast" or "grapple"): GG's sound hook (no audio here: CLAUDE.md rule 7).</summary>
        public static event Action<string> Ready;
        internal static void RaiseReady(string id)
        {
            ReadyFlashes++;
            FeelProbe.Event(UnityTuning.Hud38ReadyEvent);
            Ready?.Invoke(id);
        }
    }

    /// <summary>One gun's meter this frame, from the sim's numbers (pure: Hud38Math.Read).</summary>
    public struct Meter38
    {
        public bool show; // the bar has something to say for this gun (a charge gun, the gauntlet's pips, a cooldown running)
        public float fill; // 0..1 across the cells
        public int cells; // 3, or the BLAST's pip count
        public bool ready; // full and usable (CYAN)
        public float spin; // 0..1: the share of a reload / cooldown cycle done (-1: no cycle running)
    }

    /// <summary>NIGHT38 hud38's rules (pure; EditMode tests: Hud38Tests).</summary>
    public static class Hud38Math
    {
        /// <summary>Cell i's fill (0..1) of a bar at `fill` with `n` cells: the cells fill in turn.</summary>
        public static float Cell(float fill, int n, int i) => Clamp01(fill * n - i);

        /// <summary>A cooldown's refill (0 just started .. 1 done) from the steps left of its full length.</summary>
        public static float Refill(int left, int full) => left <= 0 ? 1f : full <= 0 ? 0f : Clamp01(1f - left / (float)full);

        /// <summary>A cooldown long enough to show (and to spin the ticks): Hud30's rule (at least Hud30MinCoolSeconds), so a pistol's cycle stays still.</summary>
        public static bool Shows(int full, int left, double dt) => left > 0 && Math.Max(full, left) * dt >= UnityTuning.Hud30MinCoolSeconds - 1e-9;

        /// <summary>The ready flash's edge: the meter is full and usable now and was not on the last frame drawn for the same gun.</summary>
        public static bool TurnedReady(bool wasReady, bool ready) => !wasReady && ready;

        /// <summary>The ticks' spin (deg) at a cycle's share (0..1): a full turn per cycle (Hud38SpinDeg).</summary>
        public static float SpinAngle(float share) => share < 0 ? 0f : UnityTuning.Hud38SpinDeg * Clamp01(share);

        /// <summary>
        /// The gun in hand's meter. `charge`: the gun has a charge shot (KESTREL / THUNDERCLAP with the charge row live); `level` the hold's level (0..1;
        /// `holding` while a real hold runs); `cdLeft` / `cdFull` its own cooldown (steps, the full length learned); `chLeft` / `chFull` its charge shot's
        /// cooldown; `pips`: the gauntlet's BLAST is live (`pipsNow` / `pipsMax`, the blast's cooldown `blLeft` / `blFull`); `dt` the sim step.
        /// </summary>
        public static Meter38 Read(bool charge, bool holding, double level, int cdLeft, int cdFull, int chLeft, int chFull, bool pips, int pipsNow, int pipsMax, int blLeft, int blFull, double dt)
        {
            var m = new Meter38 { cells = UnityTuning.Hud38Segments, spin = -1f };
            if (pips)
            {
                m.show = true;
                m.cells = Math.Max(1, Math.Min(UnityTuning.Hud38MaxSegments, pipsMax));
                m.fill = pipsMax > 0 ? Clamp01(pipsNow / (float)pipsMax) : 1f;
                if (blLeft > 0) m.spin = Refill(blLeft, Math.Max(blFull, blLeft));
                m.ready = m.fill >= 1f && blLeft <= 0;
                return m;
            }
            bool own = Shows(cdFull, cdLeft, dt);
            if (charge)
            {
                m.show = true;
                if (holding) m.fill = Clamp01((float)level); // the hold: the arcs and the bar fill together; no spin
                else if (own) { m.fill = Refill(cdLeft, Math.Max(cdFull, cdLeft)); m.spin = m.fill; }
                else if (chLeft > 0) { m.fill = Refill(chLeft, Math.Max(chFull, chLeft)); m.spin = m.fill; }
                else m.fill = 1f;
                m.ready = m.fill >= 1f && !(holding && level < 1);
                return m;
            }
            if (own)
            {
                m.show = true;
                m.fill = Refill(cdLeft, Math.Max(cdFull, cdLeft));
                m.spin = m.fill;
                m.ready = false;
                return m;
            }
            m.fill = 1f;
            m.ready = true;
            return m;
        }

        static float Clamp01(float v) => v < 0 ? 0 : v > 1 ? 1 : v;
    }

    public sealed partial class Ui
    {
        bool hud38On, hud38Read;

        bool Hud38On
        {
            get
            {
                if (!hud38Read && cfg != null)
                {
                    hud38On = Hud38.RowOn(cfg, "hud38");
                    hud38Read = true;
                }
                return hud38On;
            }
        }

        RectTransform h38Root, h38BarRt, h38GrRt, h38HookRt;
        Image[] h38Fill, h38Track, h38Hook, h38PipFill, h38PipTrack;
        int h38ShownCells = -1;
        readonly float[] h38ShownCell = new float[UnityTuning.Hud38MaxSegments];
        Color h38ShownColor = new Color(-1, -1, -1, -1);
        string h38Gun;
        bool h38WasReady = true, h38WasSpinning;
        double h38FlashAge = double.MaxValue, h38ReadyAge = double.MaxValue, h38SnapAge = double.MaxValue, h38HookAge = double.MaxValue;
        readonly double[] h38PipFlash = { double.MaxValue, double.MaxValue };
        int h38GrPrev = -1;
        Color h38ShownHook = new Color(-1, -1, -1, -1);
        float h38ShownHookScale = -1f;
        readonly Dictionary<string, int> h38CdFull = new Dictionary<string, int>(), h38CdPrev = new Dictionary<string, int>();
        int h38ChFull, h38BlFull;
        /// <summary>This frame's spin (deg), the ticks' push (px) and their white snap: Reticle33 reads them.</summary>
        float h38Spin, h38Push;
        bool h38White;

        void BuildHud38(RectTransform ret)
        {
            Vector2 mid = new Vector2(0.5f, 0.5f);
            Color bone = UiStyle33.Bone;
            h38Root = UiStyle33.Rect(ret, "HUD38", mid, mid, Vector2.zero, Vector2.zero);
            // the bar: up to Hud38MaxSegments cells, laid out per gun (LayoutBar38)
            h38BarRt = UiStyle33.Rect(h38Root, "Charge bar", mid, mid, new Vector2(0, UnityTuning.Hud38BarY), new Vector2(UnityTuning.Hud38BarW, UnityTuning.Hud38BarH));
            h38Fill = new Image[UnityTuning.Hud38MaxSegments];
            h38Track = new Image[UnityTuning.Hud38MaxSegments];
            var left = new Vector2(0f, 0.5f);
            for (int i = 0; i < h38Fill.Length; i++)
            {
                h38Fill[i] = UiStyle33.SlashBar(h38BarRt, "Cell " + i, bone, left, left, Vector2.zero, new Vector2(10, UnityTuning.Hud38BarH), out h38Track[i], UiTheme.A(bone, UnityTuning.Hud38TrackAlpha));
                h38Fill[i].gameObject.SetActive(false);
                h38Track[i].gameObject.SetActive(false);
            }
            h38BarRt.gameObject.SetActive(false);
            if (Arc40On || Charge50On) BuildArc40(h38Root); // NIGHT40 H1 (design.n40.arc40, UiHud38.Arc40.cs): one curved bar replaces the cells (they stay built, never shown). NIGHT50 H charge50 builds it too
            if (Charge50On) BuildCharge50(); // NIGHT50 H (design.n50.charge50, UiHud38.Arc40.cs): arc40 + meter42 back under hud47's reticle, re-placed, in the weapon's token
            else if (Charge49On) BuildCharge49(h38Root); // NIGHT49 H (design.n49.charge49, UiHud38.Arc40.cs): the one charge element under the reticle (never with charge50: never two at once)
            // the grapple row: the hook (shaft, the J's bottom, the barb, the eye bar) and two slash pips
            h38GrRt = UiStyle33.Rect(h38Root, "Grapple", mid, mid, new Vector2(0, GrappleRowY), Vector2.zero); // NIGHT40 H1: lower under the arc (Hud38GrappleY with arc40 off)
            h38HookRt = UiStyle33.Rect(h38GrRt, "Hook", mid, mid, new Vector2(UnityTuning.Hud38HookX, 0), Vector2.zero);
            float sh = UnityTuning.Hud38HookShaftH, w = UnityTuning.Hud38HookW, cw = UnityTuning.Hud38HookCurveW;
            Color cy = UiStyle33.Cyan;
            h38Hook = new Image[4];
            h38Hook[0] = UiStyle33.Line(h38HookRt, "Shaft", cy, mid, mid, new Vector2(-cw * 0.5f, 0), new Vector2(w, sh));
            h38Hook[1] = UiStyle33.Line(h38HookRt, "Bend", cy, mid, mid, new Vector2(0, -sh * 0.5f + w * 0.5f), new Vector2(cw + w, w));
            h38Hook[2] = UiStyle33.Line(h38HookRt, "Barb", cy, mid, mid, new Vector2(cw * 0.5f, -sh * 0.5f + UnityTuning.Hud38HookBarbH * 0.5f), new Vector2(w, UnityTuning.Hud38HookBarbH));
            h38Hook[3] = UiStyle33.Line(h38HookRt, "Eye", cy, mid, mid, new Vector2(-cw * 0.5f, sh * 0.5f - w * 0.5f), new Vector2(UnityTuning.Hud38HookEyeW, w));
            h38PipFill = new Image[2];
            h38PipTrack = new Image[2];
            for (int i = 0; i < 2; i++)
            {
                var at = new Vector2(UnityTuning.Hud38PipX0 + i * UnityTuning.Hud38PipPitch, 0);
                h38PipFill[i] = UiStyle33.SlashFill(h38GrRt, "Pip " + i, cy, mid, mid, at, new Vector2(UnityTuning.Hud38PipW, UnityTuning.Hud38PipH), out h38PipTrack[i], UiTheme.A(cy, UnityTuning.Hud38PipSpentAlpha));
            }
            h38GrRt.gameObject.SetActive(false);
            game.OnNewRun += () =>
            {
                h38Gun = null;
                h38WasReady = true;
                h38WasSpinning = false;
                h38FlashAge = h38ReadyAge = h38SnapAge = h38HookAge = double.MaxValue;
                h38PipFlash[0] = h38PipFlash[1] = double.MaxValue;
                h38GrPrev = -1;
                h38CdFull.Clear();
                h38CdPrev.Clear();
                Hud38.Snaps = Hud38.ReadyFlashes = 0;
            };
        }

        /// <summary>A gun's own cooldown left and its full length (the longest seen since it last reached 0; Rules30's record when the rules row is on).</summary>
        int Cool38(Loadout lo, string id, out int full)
        {
            int left = Hud30Reads.CooldownSteps(lo, id);
            h38CdPrev.TryGetValue(id, out int prev);
            h38CdFull.TryGetValue(id, out full);
            if (left > prev || left > full) full = left;
            if (left <= 0) full = 0;
            int ruled = Hud30Reads.FullSteps(lo, id);
            if (left > 0 && ruled >= left) full = ruled;
            h38CdFull[id] = full;
            h38CdPrev[id] = left;
            return left;
        }

        static int Track38(int left, ref int full)
        {
            if (left <= 0) full = 0;
            else if (left > full) full = left;
            return full;
        }

        /// <summary>Every frame from Reticle33 (before the ticks are placed): the meter, the bar, the spin, the grapple row.</summary>
        void Hud38Step(double frameDt)
        {
            SurvivalSim sim = game.sim;
            Loadout lo = game.loadout;
            h38Spin = h38Push = 0f;
            h38White = false;
            Hud38.Live = true;
            Hud40.Live = h40Arc != null;
            h40Now = default;
            h42Live = false; // NIGHT42 U1 meter42: set again below when the gun in hand is read
            // --- the meter of the gun in hand ---
            Meter38 m = default;
            string gun = lo != null ? lo.Weapon : null;
            bool have = sim != null && lo != null && !string.IsNullOrEmpty(gun), blast = false;
            double snapNow = UnityTuning.Hud38SnapTime; // NIGHT40 H1: the snap's length if a turn ends now (Kestrel40Spin's; hud38's 0.12 s otherwise)
            if (have)
            {
                double dt = sim.step;
                ChargeShots27 cs = (sim.n27.chargeShots27 || sim.n31.charge) && lo.charge27 != null && lo.charge27.active ? lo.charge27 : null;
                int gi = cs != null ? ChargeShots27.Index(gun) : -1;
                double level = 0;
                bool holding = false;
                if (gi >= 0)
                {
                    Charge35 q = cs.c35;
                    if (q != null && q.holding && q.gun == gi) { level = lo.Charge35Level; holding = !q.tap36 || q.started; }
                    else if (cs.charging) { level = cs.Level(gi); holding = true; }
                }
                int cdLeft = Cool38(lo, gun, out int cdFull);
                int chLeft = gi >= 0 ? cs.cooldown[gi] : 0;
                int chFull = gi >= 0 ? cs.CooldownSteps(gi) : 0;
                PulseCharge27 pc = sim.pulse27;
                bool pips = gun == "gauntlet" && sim.n27.pulse27 && pc != null && pc.active;
                int blLeft = pips ? pc.cooldownSteps : 0;
                int blFull = pips ? Math.Max(pc.BlastCooldownSteps, Track38(blLeft, ref h38BlFull)) : 0;
                blast = pips;
                int chFullT = Math.Max(chFull, Track38(chLeft, ref h38ChFull));
                m = Hud38Math.Read(gi >= 0, holding, level, cdLeft, cdFull, chLeft, chFullT, pips, pips ? pc.pips : 0, pips ? pc.p.pips : 0, blLeft, blFull, dt);
                if (h40Arc != null)
                {
                    // NIGHT40 H1: the arc's content; the STITCHER's heat (its overheat lock is its cooldown: the ready edge is the lock's end)
                    if (gun == "nailgun")
                    {
                        h40Now = Hud40Math.Heat(lo.Heat, lo.Overheated);
                        m.show = h40Now.show;
                        m.ready = h40Now.ready;
                    }
                    else h40Now = Hud40Math.Read(gi >= 0, holding, level, cdLeft, cdFull, chLeft, chFullT, pips, pips ? pc.pips : 0, pips ? pc.p.pips : 0, blLeft, blFull, dt);
                    // NIGHT42 U1 meter42 (UiHud38.Meter42.cs): the same inputs in one language (the STITCHER keeps arc40's heat)
                    h42Live = gun != "nailgun" && Meter42On;
                    if (h42Live) h42Now = Hud42Math.Read(gi >= 0, holding, level, cdLeft, cdFull, chLeft, chFullT, pips, pips ? pc.pips : 0, pips ? pc.p.pips : 0, blLeft, blFull, dt);
                }
                if (h49El != null) Charge49Read(lo, gun, gi >= 0, holding, level, chLeft, chFullT, pips, pips ? pc.pips : 0, pips ? pc.p.pips : 0, blLeft, blFull); // NIGHT49 H charge49 (UiHud38.Arc40.cs): the element's read
                Kestrel39Spin(ref m, gun, holding, level, cdLeft, cdFull); // NIGHT39 P1: the KESTREL's spin through its charge and its tap (UiHud38.Kestrel39.cs)
                Kestrel40Spin(ref m, ref snapNow, gun, holding, level, cdLeft, cdFull, chLeft, chFullT, dt); // NIGHT40 H1: re-timed to W1's rhythm while charge40 is live (UiHud38.Kestrel40.cs)
                Reticle42Spin(ref m, ref snapNow, gun, holding, level, cdLeft, cdFull, chLeft, chFullT, dt); // NIGHT42 W1 reticle42: every turn faster, over before the next shot (UiHud38.Reticle42.cs; off: nothing)
            }
            else Hud40.SpinKind = Kestrel40Math.None;
            if (h50Built) Charge50Step(ref m, have, sim, frameDt); // NIGHT50 H charge50 (UiHud38.Arc40.cs): the BEND on the arc when the gun has nothing to say; the reel's forced content
            if (gun != h38Gun)
            {
                // a switch: the new gun's state as it is, no flash, no snap
                h38Gun = gun;
                h38WasReady = m.ready;
                h38WasSpinning = m.spin >= 0;
                h38FlashAge = double.MaxValue;
                h38ReadyAge = m.ready ? double.MaxValue : 0;
            }
            // the ready edge: the flash, the event
            if (have && Hud38Math.TurnedReady(h38WasReady, m.ready))
            {
                h38FlashAge = 0;
                h38ReadyAge = 0;
                Hud38.RaiseReady(blast ? "blast" : gun);
            }
            if (!m.ready) h38ReadyAge = 0;
            h38WasReady = m.ready;
            // the spin through a cycle; the snap when it ends
            bool spinning = m.spin >= 0f && m.spin < 1f;
            if (spinning)
            {
                h38Spin = Hud38Math.SpinAngle(m.spin);
                h38SnapLen = snapNow; // NIGHT40 H1: the snap that ends this turn (Hud38SnapTime unless charge40's KESTREL turn)
            }
            else if (h38WasSpinning)
            {
                h38SnapAge = 0;
                Hud38.Snaps++;
            }
            h38WasSpinning = spinning;
            if (h38SnapAge < h38SnapLen)
            {
                float k = (float)(h38SnapAge / h38SnapLen);
                h38Push = UnityTuning.Hud38SnapPush * (1f - UiTween.OutCubic(k));
                h38White = true;
                h38SnapAge += frameDt;
            }
            else h38SnapAge = double.MaxValue;
            Spin40Clock(frameDt); // NIGHT40 H1: the KESTREL turn's measured end (s from its first frame to the snap's end), for the reels
            Hud38.SpinDeg = h38Spin;
            Bar38(m, have, frameDt);
            Grapple38(sim, frameDt);
        }

        void Bar38(Meter38 m, bool have, double frameDt)
        {
            if (h38ReadyAge < double.MaxValue && m.ready) h38ReadyAge += frameDt;
            bool show = have && (m.show || h38ReadyAge < UnityTuning.Hud38HideAfter);
            bool old49 = h49El != null && Charge49Probe.OldArc; // NIGHT49 H: the a149 reel's "before" (NIGHT46's arc40 + meter42, not hidden by hud47)
            if (h50Built) show = Charge50Show(show, m.show); // NIGHT50 H charge50 (UiHud38.Arc40.cs; GG 2026-10-19: "The charge bar under the AIM reticle needs to come back"): over hud47's rule
            else if (hud47Built && !old49) show = false; // NIGHT47 F hud47 (UiHud47.cs): the reticle alone (GG: "remove the strange blue bar under the aim reticle"); its reads moved to the gun and the rail
            if (h49El != null)
            {
                Charge49Draw(have, frameDt); // NIGHT49 H charge49 (UiHud38.Arc40.cs; GG 2026-10-19: "The charge bar under the AIM reticle needs to come back"): the one element
                if (!old49)
                {
                    if (h40Arc != null && h40Arc.gameObject.activeSelf) h40Arc.gameObject.SetActive(false); // never two at once: arc40 / meter42 / the cells stay hidden
                    if (h38BarRt.gameObject.activeSelf) h38BarRt.gameObject.SetActive(false);
                    return;
                }
            }
            if (h40Arc != null) { Arc40Draw(show, h50Built ? Charge50Ready(m.ready) : m.ready, frameDt); if (h50Built) Charge50Probe50(); return; } // NIGHT40 H1 (arc40): the arc draws instead of the cells. NIGHT50 H charge50: the BEND / the reel's force set `ready`; the probe after the draw
            if (h38BarRt.gameObject.activeSelf != show) h38BarRt.gameObject.SetActive(show);
            Hud38.BarShown = show;
            Hud38.BarGun = show ? h38Gun ?? "" : "";
            if (!show)
            {
                Hud38.BarFill = -1f;
                Hud38.BarCells = 0;
                Hud38.BarState = "";
                if (h38FlashAge < double.MaxValue) h38FlashAge += frameDt;
                return;
            }
            int n = Math.Max(1, m.cells);
            if (n != h38ShownCells) LayoutBar38(n);
            float fill = Mathf.Round(m.fill / UnityTuning.Hud38FillStep) * UnityTuning.Hud38FillStep;
            for (int i = 0; i < n; i++)
            {
                float c = Hud38Math.Cell(fill, n, i);
                if (c != h38ShownCell[i]) { h38ShownCell[i] = c; h38Fill[i].fillAmount = c; }
            }
            // the colour: bone while charging; full: the white flash fading to cyan, cyan held, then quieter at rest (Tint38; NIGHT40 H1 shares it with the arc)
            float grow = UnityTuning.Hud38FlashGrow * Tint38(m.ready, out Color col);
            AgeFlash38(frameDt);
            if (col != h38ShownColor)
            {
                h38ShownColor = col;
                for (int i = 0; i < n; i++) h38Fill[i].color = col;
            }
            float h = UnityTuning.Hud38BarH + Mathf.Round(grow);
            for (int i = 0; i < n; i++)
                if (h38Fill[i].rectTransform.sizeDelta.y != h) h38Fill[i].rectTransform.sizeDelta = new Vector2(h38Fill[i].rectTransform.sizeDelta.x, h);
            Hud38.BarFill = fill;
            Hud38.BarCells = n;
            Hud38.BarState = m.ready ? "FULL" : "CHARGING";
        }

        /// <summary>The meter's colour now: bone while charging; full: the white flash fading to cyan, cyan held, then quieter at rest. Returns the
        /// flash's grow share (1 on its first frame .. 0; 0 outside the flash).</summary>
        float Tint38(bool ready, out Color col)
        {
            if (!ready)
            {
                col = UiTheme.A(UiStyle33.Bone, UnityTuning.Hud38ChargingAlpha);
                return 0f;
            }
            if (h38FlashAge < UnityTuning.Hud38FlashTime)
            {
                float k = (float)(h38FlashAge / UnityTuning.Hud38FlashTime);
                col = Color.Lerp(Color.white, UiStyle33.Cyan, k);
                return 1f - k;
            }
            double since = h38FlashAge < double.MaxValue ? h38FlashAge - UnityTuning.Hud38FlashTime : double.MaxValue;
            float rest = since < UnityTuning.Hud38FullHold ? 1f
                : since < UnityTuning.Hud38FullHold + UnityTuning.Hud38RestEase ? Mathf.Lerp(1f, UnityTuning.Hud38RestAlpha, (float)((since - UnityTuning.Hud38FullHold) / UnityTuning.Hud38RestEase))
                : UnityTuning.Hud38RestAlpha;
            col = UiTheme.A(UiStyle33.Cyan, Mathf.Round(rest * 50f) / 50f);
            return 0f;
        }

        /// <summary>The flash's clock, once per drawn frame after Tint38.</summary>
        void AgeFlash38(double frameDt)
        {
            if (h38FlashAge < double.MaxValue) h38FlashAge = h38FlashAge > UnityTuning.Hud38FlashTime + UnityTuning.Hud38FullHold + UnityTuning.Hud38RestEase ? double.MaxValue : h38FlashAge + frameDt;
        }

        void LayoutBar38(int n)
        {
            h38ShownCells = n;
            float w = (UnityTuning.Hud38BarW - (n - 1) * UnityTuning.Hud38BarGap) / n;
            for (int i = 0; i < h38Fill.Length; i++)
            {
                bool on = i < n;
                h38Fill[i].gameObject.SetActive(on);
                h38Track[i].gameObject.SetActive(on);
                h38ShownCell[i] = -1f;
                if (!on) continue;
                var pos = new Vector2(i * (w + UnityTuning.Hud38BarGap), 0);
                h38Fill[i].rectTransform.anchoredPosition = h38Track[i].rectTransform.anchoredPosition = pos;
                h38Fill[i].rectTransform.sizeDelta = h38Track[i].rectTransform.sizeDelta = new Vector2(w, UnityTuning.Hud38BarH);
            }
            h38ShownColor = new Color(-1, -1, -1, -1);
        }

        void Grapple38(SurvivalSim sim, double frameDt)
        {
            bool gr = sim != null && sim.Grapple32On && sim.flow == Flow.Playing && sim.arcline21 != null && sim.arcline21.Owned;
            if (h38GrRt.gameObject.activeSelf != gr) h38GrRt.gameObject.SetActive(gr);
            if (!gr) { Hud38.GrappleFull = -1; h38GrPrev = -1; return; }
            int n = Math.Min(2, sim.grapple32Rules.charges);
            int full = Mathf.Clamp(sim.grappleCharges32, 0, n);
            if (h38GrPrev >= 0 && full > h38GrPrev)
            {
                for (int i = h38GrPrev; i < full && i < 2; i++) h38PipFlash[i] = 0;
                Hud38.RaiseReady("grapple");
            }
            if (h38GrPrev >= 0 && full < h38GrPrev) h38HookAge = 0; // a throw: the hook pops
            h38GrPrev = full;
            float next = sim.grappleRechargeSteps32 > 0 && full < n ? 1f - (float)sim.grappleRechargeLeft32 / sim.grappleRechargeSteps32 : 0f;
            for (int i = 0; i < 2; i++)
            {
                bool on = i < n;
                if (h38PipFill[i].gameObject.activeSelf != on) { h38PipFill[i].gameObject.SetActive(on); h38PipTrack[i].gameObject.SetActive(on); }
                if (!on) continue;
                float share = i < full ? 1f : i == full ? Mathf.Round(next * 50f) / 50f : 0f;
                if (h38PipFill[i].fillAmount != share) h38PipFill[i].fillAmount = share;
                Color c = i < full ? UiStyle33.Cyan : UiStyle33.CyanDeep;
                if (h38PipFlash[i] < UnityTuning.Hud38FlashTime)
                {
                    c = Color.Lerp(Color.white, UiStyle33.Cyan, (float)(h38PipFlash[i] / UnityTuning.Hud38FlashTime));
                    h38PipFlash[i] += frameDt;
                }
                else h38PipFlash[i] = double.MaxValue;
                if (h38PipFill[i].color != c) h38PipFill[i].color = c;
            }
            Color hc = full > 0 ? UiStyle33.Cyan : UiTheme.A(UiStyle33.Cyan, UnityTuning.Hud38HookEmptyAlpha);
            if (hc != h38ShownHook)
            {
                h38ShownHook = hc;
                for (int i = 0; i < h38Hook.Length; i++) h38Hook[i].color = hc;
            }
            float sc = 1f;
            if (h38HookAge < UnityTuning.Hud38HookPopTime)
            {
                sc = 1f + UnityTuning.Hud38HookPop * (1f - UiTween.OutCubic(h38HookAge / UnityTuning.Hud38HookPopTime));
                h38HookAge += frameDt;
            }
            else h38HookAge = double.MaxValue;
            if (sc != h38ShownHookScale) { h38ShownHookScale = sc; h38HookRt.localScale = new Vector3(sc, sc, 1f); }
            Hud38.GrappleFull = full;
        }
    }
}
