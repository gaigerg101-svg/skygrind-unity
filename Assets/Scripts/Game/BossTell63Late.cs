// NIGHT63 builder G (B2b, design.n63.bossTell63): THE BOSS'S OWN CUE IN THE DANGER HUE. During the wind-up of an attack on B2's fix list
// (design.n63.bossTell63.numbers.fix, computed from the before measure by Tools/night/n63-b2-fixlist.cjs; BossTell63.Draw records it in
// BossTell63.ActiveFix), every renderer and light under the boss view's own root whose colour is a warm off-hue glow (BossTell63.ToDanger: a hue
// between the danger band and the parry band's top, saturated: THE CROWN's yellow-ish point edges at 39 deg, THE KILN WYRM's amber seams at
// 32 deg) is drawn in the danger red-orange at the same brightness; when the wind-up ends each is put back (unless its view already drew it
// again). So the cue the player reads is the danger hue, and the boss63 measure's strongest riser follows it. Parryable attacks (the sim's
// kind yellow) are never on the list, and the column itself is never pulled. Self-installed (no hook line); runs in Update AND LateUpdate at
// execution order 32000: after SurvivalGame.Update draws most boss views and before the reel's coroutine reads them, and after
// Crown37View.LateUpdate draws the crown. Off (the row off: Draw records nothing): nothing is touched. Proof (L-089): "[tell63] recolour
// <boss> <attack>: <n> renderers, <m> lights to the danger hue" once per attack; BossTell63Probe.Recoloured per frame.
using System.Collections.Generic;
using UnityEngine;

namespace Skygrind.Game
{
    [DefaultExecutionOrder(32000)]
    public sealed class BossTell63Late : MonoBehaviour
    {
        struct Was
        {
            public Color orig, set;
        }

        readonly Dictionary<Renderer, Was> rends = new Dictionary<Renderer, Was>();
        readonly Dictionary<Light, Was> lights = new Dictionary<Light, Was>();
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        Transform listedFor;
        int listedAt = -1000;
        Renderer[] rs = new Renderer[0];
        Light[] ls = new Light[0];
        string said;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (!Application.isPlaying) return;
            var go = new GameObject("BossTell63Late") { hideFlags = HideFlags.HideAndDontSave };
            DontDestroyOnLoad(go);
            go.AddComponent<BossTell63Late>();
        }

        void Update() => Pass();

        void LateUpdate() => Pass();

        void Pass()
        {
            BossTell63.Fix63 f = BossTell63.ActiveFix;
            bool live = f.root != null && f.frame >= Time.frameCount - 1; // the crown draws in its LateUpdate: last frame's record still holds in Update
            if (!live) { Restore(); said = null; return; }
            if (listedFor != f.root || Time.frameCount - listedAt >= 30)
            {
                listedFor = f.root;
                listedAt = Time.frameCount;
                rs = f.root.GetComponentsInChildren<Renderer>(false);
                ls = f.root.GetComponentsInChildren<Light>(false);
            }
            int nr = 0, nl = 0;
            foreach (Renderer r in rs)
            {
                if (r == null || !r.enabled || r.name == BossTell63.CueName || !r.HasPropertyBlock()) continue;
                r.GetPropertyBlock(block);
                Color c = block.GetColor(BaseColor);
                if (rends.TryGetValue(r, out Was w) && c == w.set) { nr++; continue; } // still ours from the last pass
                if (!BossTell63.ToDanger(c, out Color d)) continue;
                block.SetColor(BaseColor, d);
                r.SetPropertyBlock(block);
                rends[r] = new Was { orig = c, set = d };
                nr++;
            }
            foreach (Light l in ls)
            {
                if (l == null || !l.enabled) continue;
                Color c = l.color;
                if (lights.TryGetValue(l, out Was w) && c == w.set) { nl++; continue; }
                if (!BossTell63.ToDanger(c, out Color d)) continue;
                l.color = d;
                lights[l] = new Was { orig = c, set = d };
                nl++;
            }
            BossTell63Probe.Recoloured = nr + nl;
            string key = f.boss + ":" + f.attack;
            if (said != key && nr + nl > 0)
            {
                said = key;
                Debug.Log($"[tell63] recolour {f.boss} {f.attack}: {nr} renderers, {nl} lights to the danger hue (warm off-hue glows under the view's root)");
            }
        }

        /// <summary>The wind-up is over: each colour this pass set goes back to what its view drew, unless the view drew it again since.</summary>
        void Restore()
        {
            if (rends.Count == 0 && lights.Count == 0) return;
            foreach (KeyValuePair<Renderer, Was> kv in rends)
            {
                if (kv.Key == null) continue;
                kv.Key.GetPropertyBlock(block);
                if (block.GetColor(BaseColor) != kv.Value.set) continue; // its view drew it again: leave it
                block.SetColor(BaseColor, kv.Value.orig);
                kv.Key.SetPropertyBlock(block);
            }
            foreach (KeyValuePair<Light, Was> kv in lights)
                if (kv.Key != null && kv.Key.color == kv.Value.set) kv.Key.color = kv.Value.orig;
            rends.Clear();
            lights.Clear();
            BossTell63Probe.Recoloured = 0;
        }
    }
}
