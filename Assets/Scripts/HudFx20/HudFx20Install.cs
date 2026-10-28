// NIGHT20 U3-34.10 row 5 (builder H): the HUD's package effects, installed into Skygrind.Game's hooks (HudFx20) at start-up.
// UIEffect 5.9.7 (mob-sakai, MIT) and LitMotion v2.0.2 (annulusgames, MIT), pinned by the lead (Packages/manifest.json;
// their LICENSE files read by the lead at the tags). This assembly (Skygrind.HudFx20) is the only one that references them,
// so Skygrind.Game's asmdef stays as it is; nothing references this one: Unity loads it and runs Install.
// Used in three places only (banned pattern 3: "glows on everything"):
// - Reveal (LitMotion + its TextMeshPro extension): the announcer's slab and the debut card's name reveal letter by letter,
//   each character fading in and rising Hud20FxCharRise px into place over Hud20FxCharIn s, Hud20FxCharStagger s apart (the
//   start value held through each one's delay, so nothing shows whole before its turn: M9). A new text cancels the old one.
// - EdgeGlow (UIEffect's 8-way outline, additive, CYAN): the slab's and the debut card's plates.
// - Shine (UIEffect's Shiny transition, WHITE, additive): one sweep across the style meter's letter on a rank-up, then the
//   effect switches off.
// Every motion runs on LitMotion's manual dispatcher, stepped by Ui.Render with the HUD's frame time (HudFx20.Tick): the
// reel's fixed clock drives them as it drives the HUD, in every flow (TITLE and RESULTS reveal too). Allocation: a reveal makes its motions
// (LitMotion's pooled storage) and one handle list per text, once; the shine's completion is one delegate, made once.
using System.Collections.Generic;
using Coffee.UIEffects;
using LitMotion;
using LitMotion.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    public static class HudFx20Install
    {
        static readonly Dictionary<TMP_Text, List<MotionHandle>> reveals = new Dictionary<TMP_Text, List<MotionHandle>>();
        static UIEffect shineFx;
        static MotionHandle shineHandle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Install()
        {
            HudFx20.Tick = Tick;
            HudFx20.Reveal = Reveal;
            // NIGHT21 U3-35.5 (the magenta square; UNCOMPILED, read from UIEffect's source: MaterialRepository / ShaderVariantRegistry
            // build the effect's material with Shader.Find on a variant shader, "TextMeshPro/Distance Field (UIEffect)" for a TMP
            // letter. That shader exists in the Editor but is not in a Player build (nothing references it and it isn't an
            // always-included shader), so on the Player Shader.Find gives null and the letter drew Unity's missing-material
            // magenta (NIGHT20's reels: the rank letter, the first rank-up's Shine). UIEffect is therefore used in the Editor
            // only; the Player gets neither the shine nor the edge glow (the rank letter's white-hot slam is the rank-up's
            // read, UiStyleMeter), and nothing draws with a material the build lacks. LitMotion's Reveal needs no shader.
            if (Application.isEditor)
            {
                HudFx20.EdgeGlow = EdgeGlow;
                HudFx20.Shine = Shine;
            }
        }

        static void Tick(double frameDt)
        {
            if (N30Fixes.On) SweepDead();
            if (frameDt > 0) ManualMotionDispatcher.Default.Update(frameDt);
        }

        // NIGHT30 P0 a (builder S): the 64 NullReferenceExceptions in LitMotion's TextMeshProMotionAnimator.UpdateCore. The manual dispatcher's
        // storage is static and survives a scene reload; a reveal's motions on a text that the reload destroyed kept running, and each one's
        // completion (the animator's completeAction -> UpdateCore -> target.ForceMeshUpdate) touched the dead text: one NRE per motion (2 per
        // visible character, about 32 characters = 64). A CANCEL calls only OnCancel (MotionStorage.TryCancelCore), never UpdateCore, and the
        // animator returns itself once its target reads null. So before each tick, every reveal whose text is gone is cancelled and forgotten.
        static readonly List<TMP_Text> dead = new List<TMP_Text>(8);

        static void SweepDead()
        {
            if (reveals.Count == 0) return;
            foreach (KeyValuePair<TMP_Text, List<MotionHandle>> kv in reveals)
                if (kv.Key == null) dead.Add(kv.Key); // a destroyed UnityEngine.Object reads == null; the dictionary still finds it by its instance id
            if (dead.Count == 0) return;
            for (int d = 0; d < dead.Count; d++)
            {
                List<MotionHandle> handles = reveals[dead[d]];
                for (int i = 0; i < handles.Count; i++) handles[i].TryCancel();
                handles.Clear();
                reveals.Remove(dead[d]);
            }
            dead.Clear();
        }

        static void Reveal(TMP_Text t)
        {
            if (!reveals.TryGetValue(t, out List<MotionHandle> handles))
            {
                handles = new List<MotionHandle>(64);
                reveals[t] = handles;
            }
            for (int i = 0; i < handles.Count; i++) handles[i].TryCancel();
            handles.Clear();
            t.ForceMeshUpdate();
            TMP_TextInfo info = t.textInfo;
            int n = info.characterCount;
            for (int i = 0; i < n; i++)
            {
                if (!info.characterInfo[i].isVisible) continue;
                float delay = i * UnityTuning.Hud20FxCharStagger;
                handles.Add(LMotion.Create(0f, 1f, UnityTuning.Hud20FxCharIn)
                    .WithDelay(delay, DelayType.FirstLoop, false)
                    .WithEase(Ease.OutCubic)
                    .WithScheduler(MotionScheduler.Manual)
                    .BindToTMPCharColorA(t, i));
                handles.Add(LMotion.Create(-UnityTuning.Hud20FxCharRise, 0f, UnityTuning.Hud20FxCharIn)
                    .WithDelay(delay, DelayType.FirstLoop, false)
                    .WithEase(Ease.OutCubic)
                    .WithScheduler(MotionScheduler.Manual)
                    .BindToTMPCharPositionY(t, i));
            }
        }

        static UIEffect Effect(Graphic g)
        {
            UIEffect fx = g.GetComponent<UIEffect>();
            if (fx == null) fx = g.gameObject.AddComponent<UIEffect>();
            return fx;
        }

        static void EdgeGlow(Graphic g)
        {
            UIEffect fx = Effect(g);
            fx.shadowMode = ShadowMode.Outline8;
            fx.shadowDistance = new Vector2(UnityTuning.Hud20FxEdgeWidth, UnityTuning.Hud20FxEdgeWidth);
            fx.shadowColorFilter = ColorFilter.Replace;
            fx.shadowColor = UiTheme.Cyan;
            fx.shadowFade = UnityTuning.Hud20FxEdgeAlpha;
            fx.shadowGlow = true;
        }

        static void Shine(Graphic g)
        {
            if (shineHandle.IsActive()) shineHandle.TryCancel();
            UIEffect fx = Effect(g);
            fx.enabled = true;
            fx.transitionFilter = TransitionFilter.Shiny;
            fx.transitionWidth = UnityTuning.Hud20FxShineWidth;
            fx.transitionColorFilter = ColorFilter.Additive;
            fx.transitionColor = Color.white;
            fx.transitionRate = 0f;
            shineFx = fx;
            shineHandle = LMotion.Create(0f, 1f, UnityTuning.Hud20FxShineTime)
                .WithEase(Ease.OutQuad)
                .WithScheduler(MotionScheduler.Manual)
                .WithOnComplete(ShineDone)
                .Bind(ShineRate);
        }

        static readonly System.Action<float> ShineRate = v =>
        {
            if (shineFx != null) shineFx.transitionRate = v;
        };

        static readonly System.Action ShineDone = () =>
        {
            if (shineFx == null) return;
            shineFx.transitionFilter = TransitionFilter.None;
            shineFx.enabled = false; // the letter as it was, until the next rank-up
        };
    }
}
