// NIGHT40 builder B5 (crown40; NIGHT40.md B5, <boss_rules> 5; Research/2026-10-14-wave40/B5.md D2 / D6): THE CROWN read as the player's to
// fly through, and its words. View only (no sim change). On while the run's crown40 bit is on (sim.n40.crown40); off: NIGHT39's look.
//  - THE RING (D2: the idle ring read as fire, frames f0300-f1000): when it may be passed (no tell, no window) its blades, rim and halo are
//    CYAN (player-usable, rule 8), dim; LIT bright and pulsing while his flat speed is at the pass speed (design.n37.crown37.crown.passSpeed,
//    12 m/s: a jump-dash) so the go moment shows; its gems pale bone (the thing to break). Its tells keep their YELLOW / RED, the window
//    its cyan pulse (NIGHT37 / NIGHT38 code, unchanged). v10's rehearsal ring: the same idle cyan.
//  - THE WORDS (D6; boss_rules 5): the gem counter "GEMS 14 / 20" under the boss bar; "OPEN: SHOOT IT" (cyan) under the crosshair while the
//    window is open; "BLOCKED" there for 0.6 s after a gun hit outside the window (the TORCH's flame excluded: it is held through every pass).
// Numbers: UnityTuning.Crown40.cs. Reel mirrors: Crown40Lit, Crown40Counter, Crown40Call.
using Skygrind.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    public sealed partial class Crown37View
    {
        /// <summary>For the reel: the ring drawn lit now, the counter's text, the call under the crosshair ("" / "OPEN: SHOOT IT" / "BLOCKED").</summary>
        public static bool Crown40Lit, Crown40Decal;
        public static string Crown40Counter = "", Crown40Call = "";

        Canvas canvas40;
        TextMeshProUGUI counter40, call40;
        int counterGems40 = -1, counterMax40 = -1;
        Color bone40;

        bool On40(SurvivalSim sim) => sim != null && sim.n40.crown40 && !N40Blocks.ForceOff;

        void Build40()
        {
            var go = new GameObject("Crown40 words");
            go.transform.SetParent(root, false);
            canvas40 = go.AddComponent<Canvas>();
            canvas40.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas40.sortingOrder = UnityTuning.Crown40CanvasOrder;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 1f;
            bone40 = Look.Hex(UnityTuning.Crown40GemHex);
            counter40 = Text40(go.transform, "gems", new Vector2(0.5f, 1f), UnityTuning.Crown40CounterY, UnityTuning.Crown40CounterPx, bone40);
            call40 = Text40(go.transform, "call", new Vector2(0.5f, 0.5f), UnityTuning.Crown40CallY, UnityTuning.Crown40CallPx, cyan);
            counterGems40 = counterMax40 = -1;
            Crown40Counter = Crown40Call = "";
        }

        TextMeshProUGUI Text40(Transform parent, string name, Vector2 anchor, float y, float px, Color c)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<TextMeshProUGUI>();
            var rt = t.rectTransform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0, y);
            rt.sizeDelta = new Vector2(900, px * 1.6f);
            t.alignment = TextAlignmentOptions.Center;
            t.raycastTarget = false;
            if (UiTheme.Ready) UiTheme.Style(t, px, c, UiTheme.BodyBold, true);
            else { t.fontSize = Mathf.Max(16f, px); t.color = c; }
            t.text = "";
            return t;
        }

        partial void Crown40Hide()
        {
            if (canvas40 != null && canvas40.gameObject.activeSelf) canvas40.gameObject.SetActive(false);
            Crown40Lit = false;
            Crown40Counter = Crown40Call = "";
            Voice71Hide(); // NIGHT71 B voice71: the totals once the crown's words go
        }

        partial void Crown40Draw(Crown37 c, SurvivalSim sim, float now, bool window, bool telling, bool flash)
        {
            if (!On40(sim)) { Crown40Hide(); return; }
            if (canvas40 == null) Build40(); // (again after Build() cleared the root)
            // THE RING: cyan when it may be passed; lit at the pass speed
            Vec3 v = sim.mover.velocity;
            bool lit = !window && !telling && c.brain.Fighting && System.Math.Sqrt(v.x * v.x + v.z * v.z) >= c.p.passSpeed;
            if (!window && !telling && !flash)
            {
                float gain = lit ? UnityTuning.Crown40LitGain * (0.85f + 0.15f * Mathf.Sin(now * 2 * Mathf.PI * UnityTuning.Crown40LitPulseHz)) : UnityTuning.Crown40IdleGain;
                foreach (Renderer r in blades) Tint(r, cyan, gain);
                foreach (Renderer r in rim38) Tint(r, cyan, gain);
                foreach (Renderer r in halo38) Tint(r, cyan, gain * 0.6f);
                int left = c.GemsLeft;
                Color gem40 = BossColours63.Part(game.cfg, "crown.gems", bone40); // NIGHT63 B3: the weak points' cyan, with bossColours63
                for (int k = 0; k < gems.Count && k < left; k++)
                {
                    Tint(gems[k], gem40, UnityTuning.Crown40GemGain);
                    if (k < gemCross38.Count) { Tint(gemCross38[k], gem40, UnityTuning.Crown40GemGain); Tint(gemHalo38[k], cyan, 0.4f); }
                }
                crownLight.color = cyan;
            }
            Crown40Lit = lit;
            // THE BLADE RING's decal (boss_rules 1: an area attack shows where it will cut): through its RED tell, a dim red-orange band at the
            // cut's height at its full reach (NIGHT37 drew the band only once it swept)
            Boss37Brain br = c.brain;
            if (telling && br.Current != null && br.Current.id == "bladeRing" && c.cutR <= 0)
            {
                cutT.gameObject.SetActive(true);
                int m = cutSegs.Count;
                float rr = (float)c.p.bladeReach, len = 2 * Mathf.PI * rr / m * 1.08f, gain = UnityTuning.Crown40DecalGain * (0.4f + 0.6f * (float)br.Progress);
                for (int k = 0; k < m; k++)
                {
                    float an = k * 2 * Mathf.PI / m;
                    Transform t = cutSegs[k].transform;
                    t.position = U(c.axis.x + Mathf.Cos(an) * rr, c.cutY, c.axis.z + Mathf.Sin(an) * rr);
                    Vector3 tan = U(-Mathf.Sin(an), 0, Mathf.Cos(an)) - U(0, 0, 0);
                    t.rotation = Quaternion.LookRotation(tan.normalized);
                    t.localScale = new Vector3(0.12f, UnityTuning.Crown37CutHeight, len);
                    Tint(cutSegs[k], danger, gain);
                }
                Crown40Decal = true;
            }
            else Crown40Decal = false;
            // THE WORDS
            if (!canvas40.gameObject.activeSelf) canvas40.gameObject.SetActive(true);
            int gemsLeft = c.GemsLeft;
            if (gemsLeft != counterGems40 || c.p.gems != counterMax40)
            {
                counterGems40 = gemsLeft;
                counterMax40 = c.p.gems;
                counter40.text = "GEMS " + gemsLeft + " / " + c.p.gems;
                Crown40Counter = counter40.text;
            }
            string call = "";
            float sinceBlock = (c.stepNo - c.lastBlockStep) * (float)sim.step;
            if (window) call = "OPEN: SHOOT IT";
            else if (sinceBlock >= 0 && sinceBlock < UnityTuning.Crown40BlockedSeconds && c.lastBlockWeapon != "torch") call = "BLOCKED";
            if (call != Crown40Call)
            {
                call40.text = call;
                call40.color = call == "BLOCKED" ? bone40 : cyan;
                Crown40Call = call;
            }
            Voice71Late(window); // NIGHT71 B voice71 (Crown37View.Voice71.cs): the window call not drawn while the HUD draws the window order (drawn as before with the row off); its counter, the row on or off
        }

        /// <summary>v10's rehearsal ring's idle tint with crown40 on (Crown37View.cs's hoop loop asks).</summary>
        bool Hoop40Tint(SurvivalSim sim, Renderer r)
        {
            if (!On40(sim)) return false;
            Tint(r, cyan, UnityTuning.Crown40HoopGain);
            return true;
        }
    }
}
