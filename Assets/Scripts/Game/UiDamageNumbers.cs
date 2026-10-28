// Port of skygrind/src/ui/damageNumbers.js (SPEC 2.0 Phase 8; settings.damageNumbers, off by default, the web's own SETTINGS
// row): a pooled label per player hit (config.ui.damageNumberPool) that rises (ui.damageNumberRise m over its life) and
// fades (1 - t^2) over ui.damageNumberTime, projected from the hit point each frame. Headshots are yellow, kills bigger and
// orange (the web's literals: UnityTuning). main.js update(camera, worldDt, !title ...): world time, hidden on TITLE.
// No allocation once the pool exists (TMP SetText with a number). NIGHT12, the lead.
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        sealed class DamageNumber
        {
            public TextMeshProUGUI t;
            public Vec3 pos;
            public double age = double.PositiveInfinity;
            public bool shown;
            public double enterAge18 = double.MaxValue; // NIGHT18 U3-31.6: its entrance, on frame time (it plays through a freeze)
            public float shownScale18 = 1;
        }

        DamageNumber[] dmgPool;
        RectTransform dmgLayer;
        int dmgNext, dmgSettings = -1;
        bool dmgOn;
        double dmgTime, dmgRise;
        Color dmgKill, dmgHead;

        void BuildDamageNumbers()
        {
            dmgTime = cfg.Num("ui.damageNumberTime");
            dmgRise = cfg.Num("ui.damageNumberRise");
            dmgKill = Look.Hex(UnityTuning.DamageNumberKillHex);
            dmgHead = Look.Hex(UnityTuning.DamageNumberHeadHex);
            dmgLayer = Layer("Damage numbers", false);
            dmgLayer.SetSiblingIndex(sight.GetSiblingIndex() + 1); // over the HUD's sight layer, under the menus
            dmgPool = new DamageNumber[cfg.Int("ui.damageNumberPool")];
            for (int i = 0; i < dmgPool.Length; i++)
            {
                TextMeshProUGUI t = Text(dmgLayer, $"Damage {i}", "", UnityTuning.DamageNumberSize, Color.white, TextAlignmentOptions.Center, Vector2.zero, new Vector2(160, 40), new Vector2(0.5f, 0.5f));
                t.fontStyle = FontStyles.Bold | FontStyles.Italic;
                t.gameObject.SetActive(false);
                dmgPool[i] = new DamageNumber { t = t };
            }
            game.events.On<HitEvent>("hit", e =>
            {
                if (e.owner != "player" || !dmgOn) return;
                DamageNumber n = dmgPool[dmgNext];
                dmgNext = (dmgNext + 1) % dmgPool.Length;
                n.pos = e.position;
                n.age = 0;
                n.enterAge18 = 0;
                n.t.SetText("{0:0}", (float)JsMath.Round(e.damage));
                n.t.color = e.killed ? dmgKill : e.headshot ? dmgHead : Color.white;
                n.t.fontSize = e.killed ? UnityTuning.DamageNumberKillSize : e.headshot ? UnityTuning.DamageNumberHeadSize : UnityTuning.DamageNumberSize;
            });
        }

        /// <summary>damageNumbers.js update: place every live number for this frame (world time; `visible` false on TITLE).</summary>
        public void RenderDamageNumbers(Camera cam, double worldDt, bool visible)
        {
            if (dmgPool == null) BuildDamageNumbers();
            if (game.settings.version != dmgSettings)
            {
                dmgSettings = game.settings.version;
                try
                {
                    dmgOn = game.settings.Bool("damageNumbers");
                }
                catch (System.Exception)
                {
                    dmgOn = false; // no such setting: off, as the web's default
                }
            }
            foreach (DamageNumber n in dmgPool)
            {
                if (double.IsPositiveInfinity(n.age)) continue;
                n.age += worldDt;
                double t = n.age / dmgTime;
                Vector3 world = WebSpace.ToUnity(new Vec3(n.pos.x, n.pos.y + dmgRise * t, n.pos.z));
                Vector3 sp = cam.WorldToScreenPoint(world);
                bool on = visible && t < 1 && sp.z > 0;
                if (on != n.shown)
                {
                    n.t.gameObject.SetActive(on);
                    n.shown = on;
                }
                if (t >= 1) n.age = double.PositiveInfinity;
                if (!on) continue;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(dmgLayer, sp, null, out Vector2 local);
                n.t.rectTransform.anchoredPosition = local;
                Color c = n.t.color;
                c.a = (float)(1 - t * t);
                // NIGHT18 U3-31.6 (FeelAB on): the number enters (grows from 0.6x with its overshoot, fades in: HudEntrance18)
                float es = 1;
                if (FeelAB.On && !HudEntrance18.Done(n.enterAge18))
                {
                    n.enterAge18 = HudEntrance18.Step(n.enterAge18, game.FrameDt);
                    es = HudEntrance18.Scale(n.enterAge18);
                    c.a *= HudEntrance18.Alpha(n.enterAge18);
                }
                if (es != n.shownScale18)
                {
                    n.shownScale18 = es;
                    n.t.rectTransform.localScale = new Vector3(es, es, 1);
                }
                n.t.color = c;
            }
        }
    }
}
