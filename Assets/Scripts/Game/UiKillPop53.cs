// NIGHT53 builder U, polish53 item 5 (KillPop53.cs): the kill pop's shards in the HUD's sight layer, built with the kill marker (UiMarkers.cs)
// when the pop is wanted, drawn each frame from KillPop53's motion values. Nothing allocates after the build. Numbers: UnityTuning.N53.cs.
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        readonly UnityEngine.UI.Image[] pop53 = new UnityEngine.UI.Image[4];
        bool pop53Shown;
        float pop53ShownOut = -1f, pop53ShownAlpha = -1f, pop53ShownHeat = -1f;

        /// <summary>After BuildKillMarker17: the four shards (bone, on the axes between the marker's diagonal wedges), hidden.</summary>
        void BuildKillPop53()
        {
            KillPop53.Built = false;
            KillPop53.FxOn = false;
            if (!KillPop53.Wanted(cfg)) return;
            bool fx = true;
            for (int i = 0; i < 4; i++)
            {
                var img = Img(sight, $"Kill pop shard {i}", UiTheme.White, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                    new Vector2(UnityTuning.Polish53PopShardW, UnityTuning.Polish53PopShardLen));
                img.raycastTarget = false;
                img.rectTransform.localRotation = Quaternion.Euler(0, 0, 90 * i);
                img.enabled = false;
                pop53[i] = img;
                fx &= KillPop53.Hot != null && KillPop53.Hot(i, img);
            }
            KillPop53.FxOn = fx;
            KillPop53.Built = true;
        }

        /// <summary>Each frame (after RenderKillMarker17): the shards at the motions' values while the pop runs.</summary>
        void RenderKillPop53(bool playing)
        {
            if (!KillPop53.Built) return;
            bool on = playing && KillPop53.ShardAlpha > 0.004f;
            if (on != pop53Shown)
            {
                pop53Shown = on;
                for (int i = 0; i < 4; i++) pop53[i].enabled = on;
                if (!on) return;
            }
            if (!on) return;
            float o = KillPop53.ShardOut, a = KillPop53.ShardAlpha, h = KillPop53.Heat;
            if (o != pop53ShownOut)
            {
                pop53ShownOut = o;
                float r = Mathf.LerpUnclamped(UnityTuning.Polish53PopShardFrom, UnityTuning.Polish53PopShardTo, o) + UnityTuning.Polish53PopShardLen * 0.5f;
                for (int i = 0; i < 4; i++)
                {
                    float rad = 90 * i * Mathf.Deg2Rad;
                    pop53[i].rectTransform.anchoredPosition = new Vector2(-Mathf.Sin(rad), Mathf.Cos(rad)) * r;
                }
            }
            if (a != pop53ShownAlpha)
            {
                pop53ShownAlpha = a;
                for (int i = 0; i < 4; i++) pop53[i].color = UiTheme.A(UiTheme.White, a);
            }
            if (h != pop53ShownHeat)
            {
                pop53ShownHeat = h;
                if (KillPop53.FxOn && KillPop53.SetHeat != null)
                    for (int i = 0; i < 4; i++) KillPop53.SetHeat(i, h * (float)Polish53.Num(cfg, "killPop", "heat", 1));
            }
        }
    }
}
