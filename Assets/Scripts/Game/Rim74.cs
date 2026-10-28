// PG3 V2 REWORK (after pg3-c's Player pair): the first glow (4.7 m across, 0.16 x a distance share of 0.26 = 0.04 additive) showed in NO frame; the glow is now an upright ELLIPSE that hugs the pill (half width
// 2 x its radius, half height 1.7 x half its height: 0.4 m of glow past a husk's sides, 0.6 m past its ends) at a peak of 0.70 x a share of 0.6 at 6 m rising to 1 at 18 m (about ten times as bright), and the boss
// entrance is an aura behind the boss (peak 0.90, 3 x its height) plus a wash in front of it for the swell only (peak 0.30, at most 50 degrees of view). The numbers in the text below that name the first sizes are the first design's.
// PG3 builder V2 (LIGHT AND MOTION), row design.n74.rim74 (a VIEW row: NightRows.ViewOn every frame; off: -skygrindRows72 rim74:0). Two pieces, both red-orange or its warm cousin and both on enemies only
// (CLAUDE.md rule 8: red-orange is enemies and danger; the RIVAL, whose colour is magenta, gets neither).
//   1. THE GLOW: enemies read against the dusk. The enemies already carry a fresnel rim (Enemy.shader's rimGlow.js rim, rim56's lifts, ink51's outline), and those are all N.V on the body itself, so a
//      body against a bright wall or a dusk sky still loses its edge. The glow is the other half of a rim light: one soft red-orange sprite (the Kenney flare, colors.enemy x 0.16, additive) behind each
//      live enemy (45 % of its radius away from the eye, 2.6 x its height across, at most 7 m), depth-tested, so the body's own pixels hide the middle and what shows is a warm edge around the silhouette.
//      Distance-scaled (a quarter of the peak at 6 m where the enemy is already the brightest warm thing, none at arm's length (gone at 1 m, whole again from 3.5 m: a whole-body "jelly" glow close up
//      was removed once, Rim56's header), whole from 40 m, fading out over the last 10 m before 75 m; the sprite itself shrinks to 45 % toward the eye); thinned in a crowd (past 6 glows in view the glows share sqrt(6 / n) of their strength, eased over 0.4 s: 24 husks in a knot would
//      otherwise add up to an orange fog); it eases in over 0.35 s at a spawn and out over 0.2 s at a death. Not on THE RIVAL, the burrower (its own trail view) or a boss (the key below). No shader,
//      material or model change: nothing of the enemy is touched.
//   2. THE BOSS KEY: when a boss enters, a warm key light swells on it for about 2 s, then settles. The entrance is the EventBus "bossIntro" (the Survival kit's card) or "bossRise" (every boss's own rise,
//      which is what the campaign's boss rooms emit; the two are one entrance: a 1 s dedupe). The key is a point light (#FF6A3A, hue 14: the enemies' red-orange a step warmer; range 4 x the boss's height,
//      10-20 m; intensity 9 x LightScale at the peak) placed toward the player's side of the body and a little above its centre, PLUS a soft glow sprite behind the boss in the same colour and envelope:
//      many surfaces here are unlit shaders (a light cannot lift them; crown74's lesson), so the glow carries the beat where the light cannot. Envelope: a smooth swell to the peak over 0.45 s, held 0.55 s,
//      down to 30 % over 1.0 s, where it stays while the boss lives; it goes in 0.9 s when the boss dies. The boss is found as the live boss-kind body with the most HP (looked for up to 3 s after the
//      entrance). THE RIVAL's rise is ignored.
// View only: it reads the sim's public state (the enemy registry) and the EventBus and writes nothing to the sim; no audio, no tape bit. The state (the glow fades, the key's clock) runs while the row is on and
// while the reel's same-run toggle is flipping this row, and is cleared with the row off in play (D1's contract: the frame the row turns on is the frame it would have drawn). Self-hooked (Pg3Audio's way).
// No random numbers, no per-frame random intensity: every level is a smooth function of distance or time (the night's moving-camera flicker meter, Tools/night/n74-flick.py, measures it). Pooled: fixed arrays,
// FxSprites' batched quads, no allocation per frame.
// Probe: Rim74Probe.Line() is logged at quit and at each new run: glows drawn, entrances seen (counted with the row off too), beats lit, the key's peak.
using System;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>PG3 V2: what rim74 saw and drew (the Player log's `[rim74]` line).</summary>
    public static class Rim74Probe
    {
        public static bool On;
        /// <summary>Frames with at least one glow drawn, glows drawn in all, the most at once.</summary>
        public static int GlowFrames, GlowMax;
        /// <summary>Frames on which at least one LIVE eligible enemy was tracked, the most at once, and the kinds met alive ("husk+" wears a glow, "wick-" does not): a glow drawn only as a dying enemy's fade-out tail would show here as 0 frames.</summary>
        public static int SeenFrames, SeenMax;
        public static string Kinds = "";
        public static long GlowDrawn;
        /// <summary>Boss entrances heard (bossIntro / bossRise, row on or off), beats started, beats that found their boss, frames the key was lit.</summary>
        public static int Entrances, Beats, Found, KeyFrames;
        /// <summary>Exceptions caught in the row's code (each switches the row off for the session; 0 is the answer), and the crowd thinning's lowest share seen (1: never thinned).</summary>
        public static int Faults;
        public static double CrowdMin = 1;
        /// <summary>The key light's highest intensity set, its highest level (0..1), the last boss kind it lit, its range in m.</summary>
        public static double KeyPeakIntensity, KeyPeakLevel, KeyRangeM;
        public static string KeyKind = "-";
        /// <summary>The last glow drawn, where the picture has it: its centre in viewport 0..1 and its distance in m, its half extents in px of the screen height, the colour's red channel as submitted.</summary>
        public static float GlowVx, GlowVy, GlowDist, GlowHalfWPx, GlowHalfHPx, GlowRed;
        /// <summary>The factors of that red: fade, distance share, crowd share, effects intensity, and the enemy colour's red.</summary>
        public static float GlowFade, GlowShare, GlowCrowd, GlowIntensity, GlowEnemyRed;

        public static void Reset()
        {
            GlowFrames = GlowMax = 0;
            SeenFrames = SeenMax = 0;
            Kinds = "";
            GlowDrawn = 0;
            Entrances = Beats = Found = KeyFrames = Faults = 0;
            CrowdMin = 1;
            KeyPeakIntensity = KeyPeakLevel = KeyRangeM = 0;
            KeyKind = "-";
            GlowVx = GlowVy = GlowDist = GlowHalfWPx = GlowHalfHPx = GlowRed = 0;
            GlowFade = GlowShare = GlowCrowd = GlowIntensity = GlowEnemyRed = 0;
        }

        public static string Line() =>
            $"[rim74] on {On}; live eligible enemies tracked on {SeenFrames} frames (most {SeenMax}; kinds met alive: {Kinds}); glows: {GlowDrawn} drawn over {GlowFrames} frames (most at once {GlowMax}, thinnest crowd share {CrowdMin:0.00}; last glow at viewport ({GlowVx:0.00}, {GlowVy:0.00}) {GlowDist:0.0} m, half {GlowHalfWPx:0} x {GlowHalfHPx:0} px of the screen's height, red {GlowRed:0.00} = fade {GlowFade:0.00} x share {GlowShare:0.00} x crowd {GlowCrowd:0.00} x intensity {GlowIntensity:0.00} x peak {UnityTuning.N74HaloPeak:0.00} x enemy red {GlowEnemyRed:0.00}); boss key: entrances heard {Entrances}, beats {Beats}, found {Found}, lit {KeyFrames} frames, peak level {KeyPeakLevel:0.00}, peak intensity {KeyPeakIntensity:0.00}, last {KeyKind}, range {KeyRangeM:0.0} m; faults {Faults}; draw: {V2Draw.Describe()}";
    }

    public sealed class Rim74View : MonoBehaviour
    {
        public const string Row = Rim74Math.Row;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (FindAnyObjectByType<Rim74View>() != null) return;
            var go = new GameObject("Rim74");
            DontDestroyOnLoad(go);
            go.AddComponent<Rim74View>();
        }

        SurvivalGame game;
        bool built;
        Color enemyCol;
        // the glows: one slot an enemy id, fixed arrays
        readonly int[] hid = new int[UnityTuning.N74HaloPool];
        readonly float[] hfade = new float[UnityTuning.N74HaloPool], hhw = new float[UnityTuning.N74HaloPool], hhh = new float[UnityTuning.N74HaloPool], hdist = new float[UnityTuning.N74HaloPool];
        readonly Vector3[] hpos = new Vector3[UnityTuning.N74HaloPool];
        readonly bool[] hseen = new bool[UnityTuning.N74HaloPool];
        int hn;
        float crowdK = 1f;
        // the boss key
        double clock, lastTriggerAt = -100, keyT = -1;
        bool keyFound;
        float aliveFade;
        Vector3 keyCentre;
        float keyHeight, keyRadius;
        string keyKind = "-";
        GameObject keyGo;
        Light keyLight;
        bool lightOn;

        void Update()
        {
            SurvivalGame g = SurvivalGame.Instance;
            if (g != game && g != null) Hook(g);
        }

        // the Player proof: what was drawn, printed to the Player log at quit
        void OnApplicationQuit() => Debug.Log(Rim74Probe.Line());

        void Hook(SurvivalGame g)
        {
            game = g;
            built = false;
            Clear();
            g.OnRender += Render;
            g.OnNewRun += () =>
            {
                if (Rim74Probe.Entrances > 0 || Rim74Probe.GlowDrawn > 0) Debug.Log(Rim74Probe.Line());
                Clear();
            };
            g.events.On("bossIntro", o => OnEntrance(o as BossEvent));
            g.events.On("bossRise", o => OnEntrance(o as BossEvent));
        }

        void Clear()
        {
            hn = 0;
            crowdK = 1f;
            keyT = -1;
            keyFound = false;
            aliveFade = 0;
            SetLight(false, 0, 0, Vector3.zero);
        }

        void Build(SurvivalGame g)
        {
            if (g.mats == null || g.mats.neon == null) return;
            V2Draw.Ensure(g.mats.neon); // V1's Fx74Draw recipe (pg3-c / pg3-d: the FxSprites batches drew nothing visible in the Player pair frames)
            enemyCol = Look.C(g.cfg, "colors.enemy");
            built = true;
        }

        // ---------------------------------------------------------------- the boss entrance

        void OnEntrance(BossEvent e)
        {
            if (faulted) return;
            Rim74Probe.Entrances++;
            if (e != null && e.boss == "rival") return; // magenta is the RIVAL's: no warm key on it
            if (clock - lastTriggerAt < UnityTuning.N74BossKeyDedupeS) return; // bossIntro and bossRise are one entrance
            if (game == null || game.cfg == null || !NightRows.ViewOn(game.cfg, Row)) return; // counted above (the input fired); no beat with the row off
            lastTriggerAt = clock;
            keyT = 0;
            keyFound = false;
            Rim74Probe.Beats++;
        }

        /// <summary>The live boss-kind body with the most HP, or null.</summary>
        static Enemy FindBoss(SurvivalGame g)
        {
            var all = g.enemies != null ? g.enemies.all : null;
            if (all == null) return null;
            Enemy best = null;
            for (int i = 0; i < all.Count; i++)
            {
                Enemy en = all[i];
                if (en == null || !en.alive || !Rim74Math.IsBossKind(en.kind)) continue;
                if (best == null || en.maxHp > best.maxHp) best = en;
            }
            return best;
        }

        void EnsureLight()
        {
            if (keyLight != null) return;
            keyGo = new GameObject("Rim74 boss key");
            keyGo.transform.SetParent(transform, false);
            keyLight = keyGo.AddComponent<Light>();
            keyLight.type = LightType.Point;
            keyLight.shadows = LightShadows.None;
            keyLight.color = UnityTuning.N74BossKeyColor;
            keyLight.intensity = 0f;
            keyLight.range = UnityTuning.N74BossKeyRangeMin;
            keyLight.enabled = false;
            Light54.Transient.Add(keyLight); // the room landmark's read (LightKit29.L49) skips transient lights: the swell never moves it
        }

        void SetLight(bool on, float intensity, float range, Vector3 at)
        {
            if (!on)
            {
                if (keyLight != null && lightOn)
                {
                    keyLight.enabled = false;
                    lightOn = false;
                }
                return;
            }
            EnsureLight();
            keyGo.transform.position = at;
            keyLight.range = range;
            keyLight.intensity = intensity;
            if (!lightOn)
            {
                keyLight.enabled = true;
                lightOn = true;
            }
        }

        // ---------------------------------------------------------------- the frame

        bool faulted;

        /// <summary>A view row must never take the game down: an exception in this row's code switches the row off for the session and says so once.</summary>
        void Fault(string where, Exception ex)
        {
            faulted = true;
            Rim74Probe.Faults++;
            Debug.LogError("[rim74] " + where + " threw; rim74 is off for this session: " + ex);
            SetLight(false, 0, 0, Vector3.zero);
        }

        void Render(double frameDt, double frameAlpha)
        {
            if (faulted) return;
            try { RenderInner(frameDt); }
            catch (Exception ex) { Fault("Render", ex); }
        }

        void RenderInner(double frameDt)
        {
            SurvivalGame g = game;
            if (g == null || g.cam == null || g.sim == null || g.cfg == null || g.mats == null) return;
            clock += frameDt;
            bool on = NightRows.ViewOn(g.cfg, Row); // read every frame (D1)
            Rim74Probe.On = on;
            bool toggling = NightRows.ReelOverride.Count > 0 && NightRows.ReelOverride.ContainsKey(Row); // the reel's same-run toggle: the state runs through the held off frame
            if (!on && !toggling)
            {
                if (hn > 0) hn = 0; // the row off in play: nothing is built (no sprite, no material, no light), nothing drawn
                if (keyT >= 0 || lightOn) Clear();
                return;
            }
            if (!built) Build(g); // first frame on: the texture and materials, once
            if (!built) return;
            Transform camT = g.cam.transform;
            SettingsConfig st = g.cfg.movement.settings;
            float intensity = (float)Math.Max(0, Math.Min(g.cfg.movement.feel.effectsIntensityMax, st.effectsIntensity));
            if (on)
            {
                V2Draw.Ensure(g.mats.neon);
                V2Draw.Begin();
            }
            // the glows' fades run on FRAME time (a glow is a presentation, not a world motion: a hitstop or a held reel frame must not freeze one half-faded in)
            // (pg3-e: the real elapsed time if the game's own frame dt is smaller: a reel's held or slowed frames must not stall a fade)
            int drawn = UpdateGlows(g, camT, on, Math.Max(frameDt, (double)Time.unscaledDeltaTime), intensity);
            UpdateKey(g, camT, on, frameDt);
            if (on) V2Draw.End();
            if (drawn > 0)
            {
                Rim74Probe.GlowFrames++;
                Rim74Probe.GlowDrawn += drawn;
                if (drawn > Rim74Probe.GlowMax) Rim74Probe.GlowMax = drawn;
            }
        }

        readonly System.Collections.Generic.HashSet<string> kindsNoted = new System.Collections.Generic.HashSet<string>();

        /// <summary>The probe's kinds list: each kind met alive, once ("+" wears the glow, "-" does not).</summary>
        void NoteKind(string kind)
        {
            if (kind == null || !kindsNoted.Add(kind)) return;
            Rim74Probe.Kinds += kind + (Rim74Math.WearsHalo(kind) ? "+ " : "- ");
        }

        int Slot(int id)
        {
            for (int i = 0; i < hn; i++) if (hid[i] == id) return i;
            if (hn >= hid.Length) return -1;
            hid[hn] = id;
            hfade[hn] = UnityTuning.N74HaloFadeStart; // pg3-e: the lead's probe read red 0.02-0.04 of an expected 0.7: the fade factor sat near 0.03-0.1 in the Player; a glow now starts part-way up
            hhw[hn] = 0f;
            hhh[hn] = 0f;
            hdist[hn] = 0f;
            hseen[hn] = false;
            return hn++;
        }

        /// <summary>The glows: track every live enemy that wears one, ease each, draw (when the row is on). Returns how many were drawn.</summary>
        int UpdateGlows(SurvivalGame g, Transform camT, bool draw, double dt, float intensity)
        {
            Vector3 eye = camT.position, fwd = camT.forward;
            for (int i = 0; i < hn; i++) hseen[i] = false;
            var all = g.enemies != null ? g.enemies.all : null;
            if (all != null && g.flow == Flow.Playing)
            {
                for (int e = 0; e < all.Count; e++)
                {
                    Enemy en = all[e];
                    if (en == null || !en.alive) continue;
                    NoteKind(en.kind);
                    if (!Rim74Math.WearsHalo(en.kind)) continue;
                    Vec3 p = en.previous + (en.position - en.previous) * g.Alpha;
                    Vector3 centre = WebSpace.ToUnity(p) + Vector3.up * (float)(en.Height * 0.5);
                    Vector3 to = centre - eye;
                    float dist = to.magnitude;
                    if (dist < 0.5f || dist >= UnityTuning.N74HaloMaxM) continue;
                    if (Vector3.Dot(to, fwd) < 0f) continue; // behind the eye
                    int s = Slot(en.id);
                    if (s < 0) continue;
                    hseen[s] = true;
                    hdist[s] = dist;
                    hhh[s] = (float)Rim74Math.HaloHalfHeight(en.Height);
                    hhw[s] = (float)Rim74Math.HaloHalfWidth(en.Radius, en.Height);
                    hpos[s] = centre + to / dist * (float)(en.Radius * UnityTuning.N74HaloBehindRadii); // behind the body, away from the eye
                }
            }
            int seenN = 0;
            for (int i = 0; i < hn; i++) if (hseen[i]) seenN++;
            crowdK = (float)Rim74Math.Step(crowdK, Rim74Math.CrowdShare(seenN), dt, UnityTuning.N74HaloCrowdEaseS, UnityTuning.N74HaloCrowdEaseS);
            if (draw && crowdK < Rim74Probe.CrowdMin) Rim74Probe.CrowdMin = crowdK;
            if (draw && seenN > 0) Rim74Probe.SeenFrames++;
            if (seenN > Rim74Probe.SeenMax) Rim74Probe.SeenMax = seenN;
            int drawn = 0;
            for (int i = hn - 1; i >= 0; i--)
            {
                hfade[i] = (float)Rim74Math.Step(hfade[i], hseen[i] ? 1 : 0, dt, UnityTuning.N74HaloFadeInS, UnityTuning.N74HaloFadeOutS);
                if (!hseen[i] && hfade[i] <= 0f)
                {
                    hn--; // swap-remove: a gone enemy's slot is the last one's
                    hid[i] = hid[hn]; hfade[i] = hfade[hn]; hhw[i] = hhw[hn]; hhh[i] = hhh[hn]; hdist[i] = hdist[hn]; hpos[i] = hpos[hn]; hseen[i] = hseen[hn];
                    continue;
                }
                if (!draw) continue;
                float k = hfade[i] * (float)Rim74Math.HaloShare(hdist[i]) * crowdK * intensity;
                if (k <= 0.004f) continue;
                float sc = (float)Rim74Math.HaloScale(hdist[i]);
                // an upright ellipse in the picture's plane: the pill's glow hugs it (a Billboard is a square: the quad takes the two half-extents)
                V2Draw.Quad(V2Tex.Glow, hpos[i], camT.right * (hhw[i] * sc), camT.up * (hhh[i] * sc), enemyCol * (UnityTuning.N74HaloPeak * Mathf.Clamp01(k)));
                if (g.cam != null)
                {
                    // the proof of where the picture has it (the lead reads this beside the frame): viewport centre, distance, half extents in px of the screen's height, the red submitted
                    Vector3 vp = g.cam.WorldToViewportPoint(hpos[i]);
                    float pxPerM = vp.z > 0.1f ? g.cam.pixelHeight * 0.5f / (vp.z * Mathf.Tan(g.cam.fieldOfView * 0.5f * Mathf.Deg2Rad)) : 0f;
                    Rim74Probe.GlowVx = vp.x;
                    Rim74Probe.GlowVy = vp.y;
                    Rim74Probe.GlowDist = vp.z;
                    Rim74Probe.GlowHalfWPx = hhw[i] * sc * pxPerM;
                    Rim74Probe.GlowHalfHPx = hhh[i] * sc * pxPerM;
                    Rim74Probe.GlowRed = V2Draw.LastColour.r;
                    // the factors of the red: fade x distance share x crowd x intensity (x the peak and the enemy colour's red): which one collapsed shows in the next Player line
                    Rim74Probe.GlowFade = hfade[i];
                    Rim74Probe.GlowShare = (float)Rim74Math.HaloShare(hdist[i]);
                    Rim74Probe.GlowCrowd = crowdK;
                    Rim74Probe.GlowIntensity = intensity;
                    Rim74Probe.GlowEnemyRed = enemyCol.r;
                }
                drawn++;
            }
            return drawn;
        }

        /// <summary>The boss key: find the boss, run the envelope, set the light and add the glow behind the boss.</summary>
        void UpdateKey(SurvivalGame g, Transform camT, bool draw, double dt)
        {
            if (keyT < 0)
            {
                if (lightOn) SetLight(false, 0, 0, Vector3.zero);
                return;
            }
            keyT += dt;
            Enemy body = FindBoss(g);
            if (body != null)
            {
                Vec3 p = body.previous + (body.position - body.previous) * g.Alpha;
                keyHeight = (float)body.Height;
                keyRadius = (float)body.Radius;
                keyCentre = WebSpace.ToUnity(p) + Vector3.up * (keyHeight * 0.5f);
                keyKind = body.kind;
                if (!keyFound)
                {
                    keyFound = true;
                    Rim74Probe.Found++;
                    Debug.Log($"[rim74] boss key found: {body.kind} ({keyHeight:0.0} m tall) {keyT:0.00} s after the entrance, range {Rim74Math.KeyRange(keyHeight):0.0} m");
                }
            }
            else if (!keyFound && keyT > UnityTuning.N74BossKeyFindS)
            {
                keyT = -1; // never found: no key (the entrance of a boss this row does not know)
                SetLight(false, 0, 0, Vector3.zero);
                return;
            }
            aliveFade = (float)Rim74Math.Step(aliveFade, body != null ? 1 : 0, dt, 0.15, UnityTuning.N74BossKeyFadeOutS);
            if (body == null && aliveFade <= 0f)
            {
                keyT = -1; // the boss is gone and the key with it
                SetLight(false, 0, 0, Vector3.zero);
                return;
            }
            double level = Rim74Math.KeyLevel(keyT) * aliveFade;
            if (!draw || level <= 0.002)
            {
                SetLight(false, 0, 0, Vector3.zero);
                return;
            }
            Vector3 eye = camT.position;
            Vector3 toEye = eye - keyCentre;
            float len = toEye.magnitude;
            Vector3 dirEye = len > 1e-3f ? toEye / len : Vector3.back;
            float range = (float)Rim74Math.KeyRange(keyHeight);
            Vector3 lightAt = keyCentre + dirEye * (keyRadius * UnityTuning.N74BossKeyFrontRadii) + Vector3.up * (keyHeight * UnityTuning.N74BossKeyUpHeights);
            float lightI = (float)(UnityTuning.N74BossKeyLight * UnityTuning.LightScale * level);
            SetLight(true, lightI, range, lightAt);
            // two sprites that need no light (an unlit surface ignores one; pg3-c: 661 lit frames, none showed it):
            // the aura BEHIND the boss, strong, keeping the settled share while the boss lives (an edge of warm light round its silhouette) ...
            float spread = Mathf.Max(keyRadius, keyHeight * 0.3f);
            V2Draw.Billboard(V2Tex.Glow, camT, keyCentre - dirEye * (spread * 0.6f), (float)Rim74Math.KeyGlowSize(keyHeight), 0f, UnityTuning.N74BossKeyColor * (UnityTuning.N74BossKeyGlowPeak * Mathf.Clamp01((float)level)));
            // ... and the wash IN FRONT of it, over its own surfaces, for the swell only (none once settled): what a key light on an unlit body looks like
            float swell = (float)Rim74Math.KeySwell(level);
            if (swell > 0.004f)
            {
                float dist = Mathf.Max(0.5f, len - spread * UnityTuning.N74BossKeyWashFrontRadii);
                V2Draw.Billboard(V2Tex.Glow, camT, keyCentre + dirEye * (spread * UnityTuning.N74BossKeyWashFrontRadii), (float)Rim74Math.KeyWashSize(keyHeight, dist), 0f, UnityTuning.N74BossKeyColor * (UnityTuning.N74BossKeyWashPeak * Mathf.Clamp01(swell)));
            }
            Rim74Probe.KeyFrames++;
            Rim74Probe.KeyKind = keyKind;
            Rim74Probe.KeyRangeM = range;
            if (level > Rim74Probe.KeyPeakLevel) Rim74Probe.KeyPeakLevel = level;
            if (lightI > Rim74Probe.KeyPeakIntensity) Rim74Probe.KeyPeakIntensity = lightI;
        }
    }
}
