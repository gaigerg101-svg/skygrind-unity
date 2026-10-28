// NIGHT53 builder U, polish53 item 5 (Skygrind.Game's KillPop53.cs): the kill pop's package half, installed at load as HudFx20Install is (this
// assembly is the only one that references LitMotion and UIEffect). LitMotion v2.0.2: four motions on the manual dispatcher (HudFx20.Tick steps
// it from Ui.Render with the HUD's frame time): the marker's punch (peak -> 1, OutBack), the shards' flight (0 -> 1, OutExpo) and fade (1 -> 0,
// InQuad), the heat (1 -> 0, OutQuad). A new kill cancels the last kill's motions. The binds are static lambdas with no capture (made once).
// UIEffect 5.9.7: a colour filter (Additive, white) on each shard, only when "Hidden/UI/Default (UIEffect)" is in this build (the project's
// registry holds its COLOR_FILTER variant; Shader.Find null: no effect, the shards plain bone, never a missing material).
using Coffee.UIEffects;
using LitMotion;
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    public static class KillPop53Install
    {
        const string FxShader = "Hidden/UI/Default (UIEffect)";
        static MotionHandle punch, fly, fade, heat;
        static readonly UIEffect[] fx = new UIEffect[4];

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Install()
        {
            KillPop53.Animate = Animate;
            KillPop53.Hot = Hot;
            KillPop53.SetHeat = SetHeat;
        }

        static void Animate(float peak, float punchS, float shardS, float heatS, bool fling)
        {
            punch.TryCancel();
            fly.TryCancel();
            fade.TryCancel();
            heat.TryCancel();
            if (peak != 1f && punchS > 0f)
                punch = LMotion.Create(peak, 1f, punchS).WithEase(Ease.OutBack).WithScheduler(MotionScheduler.Manual).Bind(v => KillPop53.ScaleK = v);
            else KillPop53.ScaleK = 1f;
            if (fling && shardS > 0f)
                fly = LMotion.Create(0f, 1f, shardS).WithEase(Ease.OutExpo).WithScheduler(MotionScheduler.Manual).Bind(v => KillPop53.ShardOut = v);
            else KillPop53.ShardOut = 1f;
            fade = LMotion.Create(1f, 0f, Mathf.Max(shardS, 0.05f)).WithEase(Ease.InQuad).WithScheduler(MotionScheduler.Manual).Bind(v => KillPop53.ShardAlpha = v);
            heat = LMotion.Create(1f, 0f, Mathf.Max(heatS, 0.02f)).WithEase(Ease.OutQuad).WithScheduler(MotionScheduler.Manual).Bind(v => KillPop53.Heat = v);
        }

        static bool Hot(int i, Graphic g)
        {
            if (i < 0 || i >= fx.Length || g == null || g is TMPro.TMP_Text || Shader.Find(FxShader) == null) return false;
            UIEffect e = g.GetComponent<UIEffect>();
            if (e == null) e = g.gameObject.AddComponent<UIEffect>();
            e.colorFilter = ColorFilter.Additive;
            e.color = Color.white;
            e.colorIntensity = 0f;
            fx[i] = e;
            return true;
        }

        static void SetHeat(int i, float k)
        {
            if (i < 0 || i >= fx.Length || fx[i] == null) return;
            float v = Mathf.Clamp01(k);
            if (fx[i].colorIntensity != v) fx[i].colorIntensity = v;
        }
    }
}
