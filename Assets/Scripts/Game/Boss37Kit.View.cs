// NIGHT37 builder K (boss37; NIGHT37.md card K1 "the shared view helpers"): two pieces every Boss37 boss view uses, so the four bosses read
// alike (BOSSES.md rule 7: one colour per answer; CYAN = your window).
//
//  - Boss37Glow: the CYAN window glow on a given transform (the boss's weak point: THE WICK's core, THE STAG's flank, THE LOOM's spinneret,
//    THE CROWN's centre). A boss view builds one and calls Render every frame:
//        glow = new Boss37Glow(game, coreTransform, radiusMetres);     // once
//        glow.Render(brain, frameDt);                                    // every frame (brain null or not open: it fades out)
//    While Boss37Brain.Open it pulses (Kit37GlowPulseHz), dims toward Kit37GlowEndShare as the window runs out (Progress), kicks out at the
//    opening with a spark burst, lights the arena cyan; after the window it fades in Kit37GlowFadeS. Set(open, progress, dt) drives it
//    without a brain (a reel, a rehearsal room).
//  - Boss37StaggerBar: the stagger meter as a thin bar under the HUD33 boss bar (a child of the HUD's "Boss bar", so it shows and hides
//    with it). Bone = the meter left (each drain flashes it); in the window the bar turns CYAN and counts the window down. A boss view calls
//        Boss37StaggerBar.Show(brain);                                    // every frame while its fight runs
//    and the bar hides itself on any frame no boss called it (a Boss36 / WYRM fight never shows it).
// Numbers: UnityTuning.Kit37.cs. No sim reads here (views only); nothing is built until a Boss37 boss calls it, so every n37 row off = no change.
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Skygrind.Game
{
    /// <summary>The CYAN window glow on one transform (see the file's head).</summary>
    public sealed class Boss37Glow
    {
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        readonly SurvivalGame game;
        readonly Transform shell;
        readonly Renderer shellR;
        readonly Light light;
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        readonly float radius;
        readonly Color cyan;
        float level, kick, t;
        bool wasOpen;

        /// <summary>The glow's brightness this frame (0 = off, 1 = the pulse's peak): the reels print it.</summary>
        public float Level => level;
        /// <summary>Windows drawn (opening edges seen).</summary>
        public int opens;

        public Boss37Glow(SurvivalGame game, Transform target, float radius)
        {
            this.game = game;
            this.radius = radius;
            cyan = Look.Hex(UnityTuning.Kit37CyanHex);
            var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.name = "boss37 window glow";
            Object.Destroy(ball.GetComponent<Collider>());
            shell = ball.transform;
            shell.SetParent(target, false);
            shell.localPosition = Vector3.zero;
            shellR = ball.GetComponent<Renderer>();
            shellR.sharedMaterial = FxDraw.Additive(game.mats.neon, "Boss37 window glow");
            shellR.shadowCastingMode = ShadowCastingMode.Off;
            shellR.receiveShadows = false;
            var lg = new GameObject("boss37 window light");
            lg.transform.SetParent(target, false);
            light = lg.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = cyan;
            light.range = UnityTuning.Kit37LightRange;
            light.shadows = LightShadows.None;
            light.intensity = 0;
            ball.SetActive(false);
            lg.SetActive(false);
        }

        /// <summary>Every frame: the brain's window (null: fade out).</summary>
        public void Render(Boss37Brain brain, float frameDt) => Set(brain != null && brain.Open, brain != null && brain.Open ? (float)brain.Progress : 1f, frameDt);

        /// <summary>Drive the glow directly: open, the window's progress 0..1, the frame's seconds.</summary>
        public void Set(bool open, float progress, float frameDt)
        {
            if (shell == null) return;
            t += frameDt;
            if (open && !wasOpen)
            {
                opens++;
                kick = UnityTuning.Kit37GlowKickS;
                Vector3 p = shell.position;
                game.ParticleBurst(new Vec3(p.x, p.y, -p.z), Look.Hex(UnityTuning.Kit37CyanHex, UnityTuning.Kit37GlowGain), UnityTuning.Kit37OpenSparks,
                    UnityTuning.Kit37OpenSparkSpeed, UnityTuning.Kit37OpenSparkLife, UnityTuning.Kit37OpenSparkSize);
            }
            wasOpen = open;
            if (open)
            {
                float pulse = Mathf.Lerp(UnityTuning.Kit37GlowPulseLow, 1f, 0.5f + 0.5f * Mathf.Sin(t * UnityTuning.Kit37GlowPulseHz * 2f * Mathf.PI));
                level = pulse * Mathf.Lerp(1f, UnityTuning.Kit37GlowEndShare, Mathf.Clamp01(progress));
            }
            else level = Mathf.Max(0, level - frameDt / Mathf.Max(0.01f, UnityTuning.Kit37GlowFadeS));
            kick = Mathf.Max(0, kick - frameDt);
            bool on = level > 0.001f;
            if (shell.gameObject.activeSelf != on) shell.gameObject.SetActive(on);
            if (light.gameObject.activeSelf != on) light.gameObject.SetActive(on);
            if (!on) return;
            float k = 1f + (UnityTuning.Kit37GlowOpenKick - 1f) * (UnityTuning.Kit37GlowKickS > 0 ? kick / UnityTuning.Kit37GlowKickS : 0);
            shell.localScale = Vector3.one * (2f * radius * UnityTuning.Kit37GlowScale * k);
            block.SetColor(BaseColor, cyan * (UnityTuning.Kit37GlowGain * level));
            shellR.SetPropertyBlock(block);
            light.intensity = UnityTuning.Kit37LightIntensity * level;
        }

        /// <summary>Remove the glow's objects (the boss view's teardown).</summary>
        public void Destroy()
        {
            if (shell != null) Object.Destroy(shell.gameObject);
            if (light != null) Object.Destroy(light.gameObject);
        }
    }

    /// <summary>The stagger meter under the HUD33 boss bar (see the file's head).</summary>
    public static class Boss37StaggerBar
    {
        static RectTransform root;
        static Image track, fill;
        static int shownFrame = -1;
        static float findAt, flash;
        static double lastStagger = -1;
        static Boss37Brain lastBrain;

        /// <summary>The bar was drawn this frame / the window colour is up (the reels print them).</summary>
        public static bool Shown => root != null && root.gameObject.activeSelf;
        public static bool Cyan;
        public static float FillNow;

        /// <summary>Every frame a Boss37 fight runs: draw the brain's meter (or the window's countdown) under the boss bar.</summary>
        public static void Show(Boss37Brain brain)
        {
            if (brain == null || N41View.BossBar41Live) return; // NIGHT41 (the lead's hook): bars41's bar draws the stagger meter while it runs
            if (root == null && !Build()) return;
            shownFrame = Time.frameCount;
            if (!root.gameObject.activeSelf) root.gameObject.SetActive(true);
            if (!ReferenceEquals(brain, lastBrain)) { lastBrain = brain; lastStagger = brain.stagger; }
            if (brain.stagger < lastStagger - 1e-9) flash = UnityTuning.Kit37BarDrainFlashS;
            lastStagger = brain.stagger;
            flash = Mathf.Max(0, flash - FeelClock.UnscaledDelta); // NIGHT63 D1: 0 in a same-run held frame
            Cyan = brain.Open;
            Color bone = Look.Hex(UnityTuning.Hud33BoneHex);
            if (Cyan)
            {
                FillNow = 1f - (float)brain.Progress; // the window counts down
                fill.color = UiStyle33.Cyan;
            }
            else
            {
                FillNow = (float)brain.StaggerFrac;
                Color c = UiTheme.A(bone, UnityTuning.Kit37BarFillAlpha);
                fill.color = flash > 0 ? Color.Lerp(c, Color.white, flash / UnityTuning.Kit37BarDrainFlashS) : c;
            }
            fill.fillAmount = Mathf.Clamp01(FillNow);
        }

        static bool Build()
        {
            if (Time.unscaledTime < findAt) return false;
            findAt = Time.unscaledTime + UnityTuning.Kit37BarFindEveryS;
            RectTransform bar = null;
            foreach (RectTransform rt in Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (rt.name == Hud33Layout.BossBarName) { bar = rt; break; }
            if (bar == null) return false; // no HUD33 boss bar (hud33 off): no stagger bar
            var go = new GameObject("Boss37 stagger bar", typeof(RectTransform), typeof(Boss37StaggerBarTick));
            root = (RectTransform)go.transform;
            root.SetParent(bar, false);
            root.anchorMin = new Vector2(0, 0);
            root.anchorMax = new Vector2(1, 0);
            root.pivot = new Vector2(0.5f, 1);
            root.anchoredPosition = new Vector2(0, -UnityTuning.Kit37BarGap);
            root.sizeDelta = new Vector2(0, UnityTuning.Kit37BarH);
            track = Stretch("Track", UiTheme.A(Look.Hex(UnityTuning.Hud33BoneHex), UnityTuning.Kit37BarTrackAlpha));
            fill = Stretch("Fill", Look.Hex(UnityTuning.Hud33BoneHex));
            fill.sprite = UiStyle33.WhiteSprite();
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            return true;
        }

        static Image Stretch(string name, Color c)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(root, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var img = go.GetComponent<Image>();
            img.color = c;
            img.raycastTarget = false;
            return img;
        }

        /// <summary>Called late each frame by the bar's own component: hide it on a frame no Boss37 boss drew it.</summary>
        internal static void Late()
        {
            if (root != null && root.gameObject.activeSelf && shownFrame < Time.frameCount)
            {
                root.gameObject.SetActive(false);
                Cyan = false;
            }
        }
    }

    /// <summary>The stagger bar's LateUpdate (hides it when no Boss37 boss drew it this frame). No Unity API in field initializers (L-035).</summary>
    public sealed class Boss37StaggerBarTick : MonoBehaviour
    {
        void LateUpdate() => Boss37StaggerBar.Late();
    }
}
