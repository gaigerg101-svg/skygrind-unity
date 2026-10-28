// NIGHT46 builder V1 (design.n46.feel46, a VIEW row: no sim bit, no tape change; Research/2026-10-17-wave46/V1.md gaps G1-G5; the lead's
// rulings of round 2). One owner, self-hooked: a hidden object made at scene load finds the live SurvivalGame and hooks its bus and its
// OnRender (after the camera, the viewmodel and the impact layer), so no shared file changes. Row off (or -skygrindOff46): nothing is
// drawn, nothing is changed, every borrowed object is put back as it was (the parry flash's sprite, the CRT's static).
//   G1  the off-screen wind-up warning: a red-orange chevron on the screen's edge toward each live wind-up (Core's Tells36, raised on the
//       wind-up's first step) the player cannot see (Feel46Warn in UnityTuning.Feel46.cs: the same rule `n46feel unseen` measures).
//       Edge only, never over the middle (GG w30). No sound (GG's).
//   Round 4 (the lead's ruling on n46-r2's blind pairs: G1 and G5 picked ON by both judges, the kicks not): G2, G3 and G4 run only when
//   design.n46.feel46.numbers.kicks is true (false in config; KicksOn / ForceKicks). Their code stays for a later night.
//   G2  a gun hit that did not kill: a small view pitch kick, once a shot, by weight (light < heavy), below every kill's.
//   G3  the kill ladder on camera: a plain gun body kill gets a small FOV punch-in + kick; a headshot kill a bigger kick (its FOV punch is
//       NIGHT16's); a punch kill the biggest (body < head < punch). The gauntlet / FUSION keep KillFeel42's / Feel43's own.
//   G4  a heavy gun's SHOT (the THUNDERCLAP) shakes less than its kill: the shot's crunch and web trauma cut to Shake46HeavyFireKeep of
//       what they added (the kill keeps its 0.42 + 0.15 + 0.12). Through reflection on CameraFeel's crunch and the game's Trauma (their
//       fields are private; nothing else in those files changes).
//   G5  no full-screen effect: the parry's white flash keeps its timing and alpha but draws only in an edge band and a ring round the
//       crosshair (its sprite swapped); juice38's hurt static and colour split leave the full-screen CRT pass and draw as static in the
//       same edge band, at the same level and decay (the boss entrance's flicker, juice38's other use, is left as it is).
// NIGHT71 F3 (design.n71.kicks71, Kicks71.cs): with that row on, G2-G4 run whatever numbers.kicks says, with its numbers (bigger kill kicks,
// a hit kick at most once per hitGapS, kills inside killGapS merged), on their own stacks drawn only on frames the row reads on.
// Frame time throughout (the kicks play through a freeze, as KillFeel42's). x EFFECTS INTENSITY; the kicks x CAMERA SHAKE, the FOV punch x
// FOV KICK; REDUCED MOTION stills the kicks and the punch (never the warning: it is information, not motion). No allocation per frame
// (the payload reads box once per event, as Ev43's do). Numbers: UnityTuning.Feel46.cs.
using System;
using System.Reflection;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    /// <summary>NIGHT46 V1: what feel46 did (the a546 reel's line; counts since the run started).</summary>
    public static class Feel46Probe
    {
        public static int MarkersNow, MarkersShown, HitKicks, KillKicks, FovPunches, HeavyShotsCut;
        /// <summary>NIGHT71 F3 (design.n71.kicks71): the row on this frame (a state); the hits whose kick it held back (inside hitGapS of the
        /// last hit kick) and the kills merged into a kill kick (inside killGapS): the "rarer" half, counted since the run started.</summary>
        public static bool Kicks71On;
        public static int HitsHeld71, KillsMerged71;
        /// <summary>The parry flash's sprite is the edge band + ring now (a state, not a count: the swap happens once a game).</summary>
        public static bool ParrySwapped;
        public static float StaticNow, StaticMax;
        public static double LastKickDeg, LastFovDeg;

        public static void Clear()
        {
            MarkersNow = MarkersShown = HitKicks = KillKicks = FovPunches = HeavyShotsCut = 0;
            HitsHeld71 = KillsMerged71 = 0;
            StaticNow = StaticMax = 0;
            LastKickDeg = LastFovDeg = 0;
        }

        public static string Line() =>
            $"feel46 markers now {MarkersNow} shown {MarkersShown}, hit kicks {HitKicks}, kill kicks {KillKicks}, fov punches {FovPunches}, heavy shots cut {HeavyShotsCut}, parry sprite swapped {ParrySwapped}, static now {StaticNow:0.00} max {StaticMax:0.00}, kicks71 {(Kicks71On ? "on" : "off")} (hits held back {HitsHeld71}, kills merged {KillsMerged71}, last kick {LastKickDeg:0.00} deg, last fov {LastFovDeg:0.00} deg)";
    }

    public sealed class Feel46View : MonoBehaviour
    {
        /// <summary>The a546 reel's off / on passes (null: the design row, NightRows.ViewOn(cfg, "feel46")).</summary>
        public static bool? Force;

        /// <summary>feel46 now for this game.</summary>
        public static bool On(SurvivalGame g) => Force ?? (g != null && g.cfg != null && NightRows.ViewOn(g.cfg, "feel46"));

        /// <summary>A reel's override of the kicks (null: design.n46.feel46.numbers.kicks).</summary>
        public static bool? ForceKicks;

        /// <summary>NIGHT46 round 4 (the lead's ruling on n46-r2's blind pairs): G2 / G3 / G4 (the kicks, the FOV punch, the THUNDERCLAP's shot
        /// shake) only when design.n46.feel46.numbers.kicks is true; G1 and G5 run whenever feel46 is on.</summary>
        public static bool KicksOn(SurvivalGame g)
        {
            if (ForceKicks.HasValue) return ForceKicks.Value;
            if (g != null && Kicks71.On(g.cfg)) return true; // NIGHT71 F3: design.n71.kicks71 runs G2-G4 with its own numbers (Kicks71.cs)
            const string K = "design.n46.feel46.numbers.kicks";
            return g != null && g.cfg != null && g.cfg.Has(K) && g.cfg.Get(K) is bool b && b;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (FindAnyObjectByType<Feel46View>() != null) return;
            var go = new GameObject("Feel46");
            DontDestroyOnLoad(go);
            go.AddComponent<Feel46View>();
        }

        SurvivalGame game;
        Shift46View shift46;
        bool on, kicksOn;
        readonly KickStack kicks = new KickStack(UnityTuning.Kick46Slots);
        readonly KickStack fovs = new KickStack(UnityTuning.Kick46Slots);
        // NIGHT71 F3 (design.n71.kicks71): its own stacks, drawn only on a frame the row reads on (a same-run held off frame draws none of
        // them and keeps their state); the frame clock of the last hit kick and kill kick, and that kill kick's tier (deg)
        readonly KickStack kicks71 = new KickStack(UnityTuning.Kick46Slots);
        readonly KickStack fovs71 = new KickStack(UnityTuning.Kick46Slots);
        bool k71On;
        double lastHitKickAt71 = double.NegativeInfinity, lastKillKickAt71 = double.NegativeInfinity, lastKillDeg71;
        int lastHitShot = -1;
        readonly System.Collections.Generic.Dictionary<string, int> weaponClass = new System.Collections.Generic.Dictionary<string, int>();

        // G1
        Canvas canvas;
        RectTransform canvasRect;
        readonly Image[] markers = new Image[UnityTuning.Warn46Max];
        readonly double[] wAngle = new double[UnityTuning.Warn46Max], wProg = new double[UnityTuning.Warn46Max], wDist = new double[UnityTuning.Warn46Max];
        readonly int[] wSrc = new int[UnityTuning.Warn46Max];
        readonly System.Collections.Generic.HashSet<int> seenSources = new System.Collections.Generic.HashSet<int>();

        // G5
        Image parryFlash;
        Sprite parryOriginal, edgeRing;
        bool parrySwapped;
        RawImage staticLayer;
        Texture2D[] staticFrames;
        double realClock;

        // G4 (private fields of CameraFeel / CrunchShake / Trauma: read and written only on a heavy shot's "fire")
        static readonly FieldInfo FeelField = typeof(SurvivalGame).GetField("feel", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly FieldInfo CrunchTrauma = typeof(CrunchShake).GetField("trauma", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly FieldInfo WebTrauma = typeof(Trauma).GetField("trauma", BindingFlags.NonPublic | BindingFlags.Instance);

        void Update()
        {
            SurvivalGame g = SurvivalGame.Instance;
            if (g != game && g != null) Hook(g);
        }

        void Hook(SurvivalGame g)
        {
            game = g;
            shift46?.Dispose();
            shift46 = new Shift46View(g); // NIGHT46 V1 leftover: the room state lights (design.n46.shift46), driven from this one hook
            parryFlash = null;
            parryOriginal = null;
            parrySwapped = false;
            weaponClass.Clear();
            Clear();
            g.OnRender += Render;
            g.OnNewRun += Clear;
            EventBus ev = g.events;
            ev.On("fire", o => { if (on && kicksOn && o is string w) OnFire(w); });
            ev.On("hit", o => { if (on && kicksOn && o is HitEvent e) OnHit(e); });
            ev.On("kill", o => { if (on && kicksOn && o is KillEvent e) OnKill(e); });
        }

        void Clear()
        {
            kicks.Clear();
            fovs.Clear();
            kicks71.Clear();
            fovs71.Clear();
            lastHitKickAt71 = lastKillKickAt71 = double.NegativeInfinity;
            lastKillDeg71 = 0;
            lastHitShot = -1;
            seenSources.Clear();
            Feel46Probe.Clear();
        }

        // --- G2 / G3 / G4: the bus ------------------------------------------------------------------------------------------------------------

        /// <summary>A gun's weight class (feel.punch.weapons.&lt;w&gt;.cls, CrunchShake's lookup): 0 light, 1 medium, 2 heavy, -1 none.</summary>
        int ClassOf(string w)
        {
            if (w == null) return -1;
            if (weaponClass.TryGetValue(w, out int c)) return c;
            GameConfig cfg = game.cfg;
            string cls = cfg != null && cfg.Has($"feel.punch.weapons.{w}.cls") ? cfg.Str($"feel.punch.weapons.{w}.cls") : null;
            c = cls == "light" ? 0 : cls == "medium" ? 1 : cls == "heavy" ? 2 : -1;
            weaponClass[w] = c;
            return c;
        }

        void OnFire(string w)
        {
            if (ClassOf(w) != 2) return;
            // G4: CrunchShake's and the web's fire handlers ran first (both wired in SurvivalGame.Awake, before this hooked): take back the share
            // (NIGHT71 F3: with kicks71 on, its heavyFireKeep: the shot keeps less, so its kill reads bigger)
            Kicks71.Numbers n71 = k71On ? Kicks71.Of(game.cfg) : null;
            double cut = 1 - (n71 != null ? n71.heavyFireKeep : UnityTuning.Shake46HeavyFireKeep);
            if (FeelField != null && CrunchTrauma != null && FeelField.GetValue(game) is CameraFeel feel && feel.crunch != null)
            {
                double t = (double)CrunchTrauma.GetValue(feel.crunch);
                CrunchTrauma.SetValue(feel.crunch, Math.Max(0, t - UnityTuning.ShakeFireHeavy * cut));
            }
            if (WebTrauma != null && game.trauma != null && game.cfg != null && game.cfg.Has("feel.trauma.fire.heavy"))
            {
                double t = (double)WebTrauma.GetValue(game.trauma);
                WebTrauma.SetValue(game.trauma, Math.Max(0, t - game.cfg.Num("feel.trauma.fire.heavy") * cut));
            }
            Feel46Probe.HeavyShotsCut++;
        }

        void OnHit(HitEvent e)
        {
            if (e.owner != "player" || e.killed || e.melee || e.shock || e.shot == lastHitShot) return;
            if (IsSustained(e.weapon)) return; // a stream's ticks would kick ten times a second
            int cls = ClassOf(e.weapon);
            if (cls < 0) return;
            lastHitShot = e.shot;
            if (k71On)
            {
                // NIGHT71 F3: the n71 hit kick, at most once per hitGapS (rarer)
                Kicks71.Numbers n = Kicks71.Of(game.cfg);
                double d71 = Kicks71.HitKick(n, cls == 2, realClock, lastHitKickAt71);
                if (n == null || d71 <= 0)
                {
                    Feel46Probe.HitsHeld71++;
                    return;
                }
                kicks71.Add(d71, n.hitAttackS, n.hitRecoverS);
                lastHitKickAt71 = realClock;
                Feel46Probe.HitKicks++;
                Feel46Probe.LastKickDeg = d71;
                return;
            }
            double deg = cls == 2 ? UnityTuning.Kick46HitHeavyDeg : UnityTuning.Kick46HitLightDeg;
            kicks.Add(deg, UnityTuning.Kick46Attack, UnityTuning.Kick46Recover);
            Feel46Probe.HitKicks++;
            Feel46Probe.LastKickDeg = deg;
        }

        static bool IsSustained(string w)
        {
            foreach (string s in UnityTuning.Kick46SustainedWeapons) if (w == s) return true;
            return false;
        }

        void OnKill(KillEvent e)
        {
            if (e.owner != "player") return;
            if (k71On)
            {
                OnKill71(e);
                return;
            }
            double deg;
            if (e.melee)
            {
                if (e.weapon != "punch") return; // the gauntlet's kill kicks are KillFeel42's
                deg = UnityTuning.Kick46PunchKillDeg;
            }
            else if (IsSustained(e.weapon)) return;
            else if (e.headshot) deg = UnityTuning.Kick46HeadKillDeg;
            else
            {
                deg = UnityTuning.Kick46BodyKillDeg;
                fovs.Add(UnityTuning.Fov46BodyKillDeg, UnityTuning.Fov46Attack, UnityTuning.Fov46Recover);
                Feel46Probe.FovPunches++;
                Feel46Probe.LastFovDeg = UnityTuning.Fov46BodyKillDeg;
            }
            kicks.Add(deg, UnityTuning.Kick46Attack, UnityTuning.Kick46Recover);
            Feel46Probe.KillKicks++;
            Feel46Probe.LastKickDeg = deg;
        }

        /// <summary>NIGHT71 F3: G3 with design.n71.kicks71's numbers: the same ladder (body &lt; head &lt; punch; the gauntlet's and the streams'
        /// kills left to their own), bigger; a kill inside killGapS of the last kill kick adds only what its tier has over that kick's (rarer:
        /// a multi-kill is one kick, the biggest tier's), and no second FOV punch-in.</summary>
        void OnKill71(KillEvent e)
        {
            Kicks71.Numbers n = Kicks71.Of(game.cfg);
            if (n == null) return;
            double tier;
            bool body = false;
            if (e.melee)
            {
                if (e.weapon != "punch") return; // the gauntlet's kill kicks are KillFeel42's
                tier = n.punchKillDeg;
            }
            else if (IsSustained(e.weapon)) return;
            else if (e.headshot) tier = n.headKillDeg;
            else
            {
                tier = n.bodyKillDeg;
                body = true;
            }
            bool merged = Kicks71.InsideGap(realClock, lastKillKickAt71, n.killGapS); // the same rule as KillKick's (a sum: no float edge)
            double deg = Kicks71.KillKick(n, tier, realClock, lastKillKickAt71, lastKillDeg71);
            if (merged)
            {
                Feel46Probe.KillsMerged71++;
                lastKillDeg71 = Math.Max(lastKillDeg71, tier);
            }
            else
            {
                lastKillKickAt71 = realClock;
                lastKillDeg71 = tier;
                if (body)
                {
                    fovs71.Add(n.bodyKillFovDeg, n.fovAttackS, n.fovRecoverS);
                    Feel46Probe.FovPunches++;
                    Feel46Probe.LastFovDeg = n.bodyKillFovDeg;
                }
            }
            if (deg <= 0) return;
            kicks71.Add(deg, n.killAttackS, n.killRecoverS);
            Feel46Probe.KillKicks++;
            Feel46Probe.LastKickDeg = deg;
        }

        // --- every frame, after the camera (SurvivalGame.RenderFrame's OnRender) ---------------------------------------------------------------

        void Render(double frameDt, double alpha)
        {
            SurvivalGame g = game;
            if (g == null || g.cam == null) return;
            shift46?.Render(frameDt); // NIGHT46 V1 leftover: shift46 (its own row; it runs whether feel46 is on or not)
            bool now = On(g);
            if (now != on)
            {
                on = now;
                Clear();
            }
            kicksOn = on && KicksOn(g); // round 4: G2-G4 only with design.n46.feel46.numbers.kicks (or the reel's ForceKicks; NIGHT71: or kicks71)
            k71On = kicksOn && Kicks71.On(g.cfg); // NIGHT71 F3: read every frame (the same-run toggle); its kicks drawn only while on
            Feel46Probe.Kicks71On = k71On;
            realClock += frameDt;
            kicks.Advance(frameDt);
            fovs.Advance(frameDt);
            kicks71.Advance(frameDt);
            fovs71.Advance(frameDt);
            if (!on)
            {
                HideMarkers();
                RestoreParry();
                if (staticLayer != null && staticLayer.enabled) staticLayer.enabled = false;
                return;
            }
            EnsureCanvas();
            MovementConfig mc = g.cfg.movement;
            double intensity = Math.Max(0, Math.Min(mc.feel.effectsIntensityMax, mc.settings.effectsIntensity));
            bool calm = mc.settings.reducedMotion;
            // G2 / G3: the kicks and the punch-in, on top of the camera this frame (the next frame's CameraFeel.Apply sets it whole again)
            double kick = calm ? 0 : (kicks.Value() + (k71On ? kicks71.Value() : 0)) * intensity * CameraFeel.ShakeScale(g.settings);
            if (kick != 0) g.cam.transform.rotation = g.cam.transform.rotation * Quaternion.Euler((float)-kick, 0f, 0f);
            double fov = calm ? 0 : (fovs.Value() + (k71On ? fovs71.Value() : 0)) * intensity * CameraFeel.FovKickScale(g.settings);
            if (fov != 0) g.cam.fieldOfView = Mathf.Max(1f, g.cam.fieldOfView - (float)fov);
            Markers(g);
            Parry();
            Static();
        }

        // --- G1 ---------------------------------------------------------------------------------------------------------------------------------

        void Markers(SurvivalGame g)
        {
            SurvivalSim sim = g.sim;
            bool live = sim != null && sim.flow == Flow.Playing && sim.tells36 != null;
            int n = 0;
            if (live)
            {
                double tanV = Math.Tan(g.cam.fieldOfView * Math.PI / 360);
                n = Feel46Warn.Scan(sim.tells36, sim.Eye, g.input.yaw, g.input.pitch, tanV, g.cam.aspect, wAngle, wProg, wSrc, wDist);
            }
            float h = canvasRect.rect.height, w = canvasRect.rect.width;
            float hx = w * 0.5f - UnityTuning.Warn46InsetPx, hy = h * 0.5f - UnityTuning.Warn46InsetPx;
            for (int i = 0; i < markers.Length; i++)
            {
                Image m = markers[i];
                if (i >= n)
                {
                    if (m.enabled) m.enabled = false;
                    continue;
                }
                double a = wAngle[i];
                float c = (float)Math.Cos(a), s = (float)Math.Sin(a);
                float t = Mathf.Min(Mathf.Abs(c) > 1e-5f ? hx / Mathf.Abs(c) : float.MaxValue, Mathf.Abs(s) > 1e-5f ? hy / Mathf.Abs(s) : float.MaxValue);
                float x = c * t, y = s * t;
                // round 4 (n46-r2's V46-on-behind-050: the chevron on the bottom edge sat partly behind the gun): never on the bottom edge. A marker
                // in the lower half goes to its side's edge, at most Warn46LowestPx above the bottom inset (the gun, the HP plate and the ammo sit
                // in the bottom band; the side edges above it are clear).
                if (s < 0f)
                {
                    float side = Mathf.Abs(c) > 1e-4f ? Mathf.Sign(c) : 1f;
                    x = side * hx;
                    y = Mathf.Max(-hy + UnityTuning.Warn46LowestPx, Mathf.Abs(c) > 1e-4f ? s * hx / Mathf.Abs(c) : -hy);
                }
                RectTransform rt = m.rectTransform;
                rt.anchoredPosition = new Vector2(x, y);
                rt.localEulerAngles = new Vector3(0, 0, (float)(a * 180 / Math.PI) - 90f); // the sprite points up: turn it to point outward
                float p = Mathf.Clamp01((float)wProg[i]);
                float size = Mathf.Lerp(UnityTuning.Warn46SizeFromPx, UnityTuning.Warn46SizeToPx, p);
                rt.sizeDelta = new Vector2(size, size);
                float al = Mathf.Lerp(UnityTuning.Warn46AlphaFrom, UnityTuning.Warn46AlphaTo, p);
                if (p > 2f / 3f) al *= 0.75f + 0.25f * (0.5f + 0.5f * Mathf.Cos((float)(realClock * UnityTuning.Warn46PulseHz * 2 * Math.PI)));
                m.color = new Color(1, 1, 1, al);
                if (!m.enabled) m.enabled = true;
                if (seenSources.Add(wSrc[i])) Feel46Probe.MarkersShown++;
            }
            Feel46Probe.MarkersNow = n;
        }

        void HideMarkers()
        {
            foreach (Image m in markers) if (m != null && m.enabled) m.enabled = false;
            Feel46Probe.MarkersNow = 0;
        }

        // --- G5 ---------------------------------------------------------------------------------------------------------------------------------

        void Parry()
        {
            // NOTE (n46-r1's a546 frames): with hud33 on, this Image's layer (Ui's "HUD frame") sits at CanvasGroup alpha 0 (UiHud33.cs:120-127), so
            // the full-screen parry flash is not drawn in today's game at all; the swap keeps it edge-only for a game with hud33 off.
            if (parryFlash == null)
            {
                if (Time.frameCount < parryFindNext) return;
                parryFindNext = Time.frameCount + 30; // a look-up at most every 30 frames until the HUD exists
                GameObject go = GameObject.Find("Parry flash"); // UiFeel.BuildFeel's full-screen white Image
                if (go == null) return;
                parryFlash = go.GetComponent<Image>();
                if (parryFlash == null) return;
                parryOriginal = parryFlash.sprite;
            }
            Feel46Probe.ParrySwapped = true;
            if (parrySwapped) return;
            parryFlash.sprite = edgeRing;
            parryFlash.type = Image.Type.Simple;
            parryFlash.preserveAspect = false;
            parrySwapped = true;
        }

        int parryFindNext;

        void RestoreParry()
        {
            Feel46Probe.ParrySwapped = false;
            if (!parrySwapped || parryFlash == null) return;
            parryFlash.sprite = parryOriginal;
            parrySwapped = false;
        }

        void Static()
        {
            // juice38's Crt() ran in this frame's camera step (CameraFeel.Apply): take the hurt static's share out of the full-screen pass and
            // show the same level in the edge band. Its share is st x the static numbers (UnityTuning.Juice38); the flicker's is left.
            float st = Juice38Crt.Frame == Time.frameCount ? Juice38Probe.Static : 0f;
            if (st > 0f)
            {
                Juice38Crt.Grain = Mathf.Max(0f, Juice38Crt.Grain - st * UnityTuning.Juice38StaticGrain);
                Juice38Crt.SplitUv = Mathf.Max(0f, Juice38Crt.SplitUv - st * UnityTuning.Juice38StaticSplitUv);
                Juice38Crt.Scan = Mathf.Max(0f, Juice38Crt.Scan - st * UnityTuning.Juice38StaticScan);
                if (Juice38Crt.Tear <= st * UnityTuning.Juice38StaticTear + 1e-4f) Juice38Crt.Tear = 0f; // the tear was the static's (a max of the two)
            }
            Feel46Probe.StaticNow = st;
            if (st > Feel46Probe.StaticMax) Feel46Probe.StaticMax = st;
            float a = Mathf.Clamp01(st * UnityTuning.Static46Alpha);
            if (a <= 0.002f)
            {
                if (staticLayer.enabled) staticLayer.enabled = false;
                return;
            }
            int frame = (int)Math.Floor(realClock * UnityTuning.Static46Hz);
            staticLayer.texture = staticFrames[((frame % staticFrames.Length) + staticFrames.Length) % staticFrames.Length];
            staticLayer.color = new Color(1, 1, 1, a);
            if (!staticLayer.enabled) staticLayer.enabled = true;
        }

        // --- building (once) ---------------------------------------------------------------------------------------------------------------------

        void EnsureCanvas()
        {
            if (canvas != null) return;
            var go = new GameObject("Feel46 canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            go.transform.SetParent(transform, false);
            canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = UnityTuning.Warn46SortingOrder;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 1f; // px at 1080p
            canvasRect = (RectTransform)go.transform;

            edgeRing = Sprite.Create(EdgeTexture(true, 0), new Rect(0, 0, UnityTuning.Edge46TexW, UnityTuning.Edge46TexH), new Vector2(0.5f, 0.5f));
            staticFrames = new Texture2D[UnityTuning.Static46Frames];
            for (int i = 0; i < staticFrames.Length; i++) staticFrames[i] = EdgeTexture(false, i + 1);
            var sg = new GameObject("Feel46 static", typeof(RectTransform), typeof(RawImage));
            sg.transform.SetParent(go.transform, false);
            staticLayer = sg.GetComponent<RawImage>();
            staticLayer.raycastTarget = false;
            var srt = staticLayer.rectTransform;
            srt.anchorMin = Vector2.zero;
            srt.anchorMax = Vector2.one;
            srt.offsetMin = srt.offsetMax = Vector2.zero;
            staticLayer.enabled = false;

            Sprite chevron = Sprite.Create(ChevronTexture(), new Rect(0, 0, ChevronTex, ChevronTex), new Vector2(0.5f, 0.5f));
            for (int i = 0; i < markers.Length; i++)
            {
                var mg = new GameObject("Feel46 warn " + i, typeof(RectTransform), typeof(Image));
                mg.transform.SetParent(go.transform, false);
                Image m = mg.GetComponent<Image>();
                m.sprite = chevron;
                m.raycastTarget = false;
                m.preserveAspect = true;
                RectTransform rt = m.rectTransform;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                m.enabled = false;
                markers[i] = m;
            }
        }

        static float Smooth(float e0, float e1, float x)
        {
            float t = Mathf.Clamp01((x - e0) / Mathf.Max(1e-5f, e1 - e0));
            return t * t * (3 - 2 * t);
        }

        /// <summary>The edge band (+ the crosshair ring for the parry: white, alpha mask) or a frame of edge static (`seed` > 0: grey-white noise
        /// tinted toward the danger colour, the same band, no ring).</summary>
        static Texture2D EdgeTexture(bool ring, int seed)
        {
            int w = UnityTuning.Edge46TexW, h = UnityTuning.Edge46TexH;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = ring ? "Feel46 edge ring" : "Feel46 static " + seed };
            var px = new Color32[w * h];
            float aspect = w / (float)h;
            Color danger = Hex(UnityTuning.Warn46ColorHex);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    float sx = Mathf.Abs((x + 0.5f) / w * 2 - 1), sy = Mathf.Abs((y + 0.5f) / h * 2 - 1);
                    float a = Smooth(UnityTuning.Edge46From, UnityTuning.Edge46Full, Mathf.Max(sx, sy));
                    if (ring)
                    {
                        float dx = ((x + 0.5f) / w * 2 - 1) * aspect, dy = (y + 0.5f) / h * 2 - 1;
                        float r = Mathf.Sqrt(dx * dx + dy * dy);
                        a = Mathf.Max(a, 1 - Mathf.Clamp01(Mathf.Abs(r - UnityTuning.Ring46Radius) / UnityTuning.Ring46HalfWidth));
                        px[y * w + x] = new Color32(255, 255, 255, (byte)(a * 255));
                    }
                    else
                    {
                        // TV static: a value per texel (~5 px at 1080p; n46-r1: 2 x 2 cells read as coarse blocks) from the cosmetic hash, no random stream
                        float v = (float)(Cosmetic.Lattice(x + y * 4099, 46000 + seed) * 0.5 + 0.5);
                        Color c = Color.Lerp(new Color(v, v, v), danger * v, UnityTuning.Static46TintMix);
                        px[y * w + x] = new Color32((byte)(c.r * 255), (byte)(c.g * 255), (byte)(c.b * 255), (byte)(a * (0.35f + 0.65f * v) * 255));
                    }
                }
            tex.SetPixels32(px);
            tex.Apply();
            return tex;
        }

        const int ChevronTex = 64;

        /// <summary>The warning's chevron, pointing up: a red-orange arrowhead with a dark ink rim (it reads on level 2's white ice).</summary>
        static Texture2D ChevronTexture()
        {
            int n = ChevronTex;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = "Feel46 chevron" };
            var px = new Color32[n * n];
            Color danger = Hex(UnityTuning.Warn46ColorHex);
            Color ink = Hex(UnityTuning.Warn46InkHex);
            Vector2 a = new Vector2(-0.72f, -0.38f), b = new Vector2(0f, 0.46f), c = new Vector2(0.72f, -0.38f);
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    var p = new Vector2((x + 0.5f) / n * 2 - 1, (y + 0.5f) / n * 2 - 1);
                    float d = Mathf.Min(SegDist(p, a, b), SegDist(p, b, c));
                    float core = 1 - Smooth(UnityTuning.Warn46CoreHalf - 0.03f, UnityTuning.Warn46CoreHalf + 0.03f, d);
                    float rim = 1 - Smooth(UnityTuning.Warn46RimHalf - 0.03f, UnityTuning.Warn46RimHalf + 0.03f, d);
                    Color col = Color.Lerp(ink, danger, core);
                    float al = Mathf.Max(core, rim * UnityTuning.Warn46InkAlpha);
                    px[y * n + x] = new Color32((byte)(col.r * 255), (byte)(col.g * 255), (byte)(col.b * 255), (byte)(al * 255));
                }
            tex.SetPixels32(px);
            tex.Apply();
            return tex;
        }

        static float SegDist(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
            return (p - (a + ab * t)).magnitude;
        }

        static Color Hex(uint rgb) => new Color(((rgb >> 16) & 0xff) / 255f, ((rgb >> 8) & 0xff) / 255f, (rgb & 0xff) / 255f, 1f);
    }
}
