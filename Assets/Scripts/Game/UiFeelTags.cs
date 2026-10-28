// NIGHT16 U3-29.8 polish #37 / #38 (web ports of hud.js cues the port never had):
// - #37 the tag under the crosshair (hud.js .hud-tag :83-84, tag() :578-582, its fade :669-670; main.js :2239-2241,
//   POWER P3): the player's ARSENAL moments get a word under the crosshair: PRIMED (#bff8ff) on `primed`, the combo
//   (COLLAPSE, OVERLOAD) or CROSS in the gun family's colour on `crossHit`, SWAP (#ffe6a8) on `swapKill`. t-h3 bold,
//   letter-spacing 0.3em, its top edge 38 px under the centre; opacity min(1, 3 (0.7 - age)): full for 0.37 s, out over
//   0.33 s. The web's other caller (main.js :1369, the aim-assist mode key's hint) has no key in the port: left.
// - #38 the charge edge (hud.js .hud-charge-edge :170 / :291 / :836-837; player.js :347-349, POWER P5): while a weapon
//   charges, an inset screen-edge tint (box-shadow: inset 0 0 140px 20px, the family's colour) at charge x
//   hud.chargeEdgeOpacity. The charge is loadout.RailCharge (the LONGWAVE; the web's max with the THUMPER's wind-up: the
//   THUMPER isn't ported). Its colour follows the gun in hand (weapons.families[catalog[gun].family]), read on a swap.
// The CSS literals are UnityTuning.FeelPolish.cs's. No allocation per frame (a tag's text is set on its event).
using System;
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        TextMeshProUGUI feelTag;
        double feelTagAge = double.MaxValue;
        float shownFeelTag = -1;
        float feelTagShownScale18 = 1; // NIGHT18 U3-31.6: its entrance's scale (written while it moves)

        UnityEngine.UI.Image chargeEdge;
        double chargeEdgeOpacity;
        string chargeGun;
        float shownChargeEdge = -1;

        void BuildFeelTags()
        {
            // #37
            feelTag = Text(sight, "Tag", "", tH3, Color.white, TextAlignmentOptions.Center, new Vector2(0, -UnityTuning.TagTop), new Vector2(UnityTuning.TagBoxWidth, tH3 * 1.5f), new Vector2(0.5f, 0.5f));
            feelTag.rectTransform.pivot = new Vector2(0.5f, 1); // CSS: its top edge at top: 38px
            feelTag.fontStyle = FontStyles.Bold;
            feelTag.characterSpacing = UnityTuning.TagSpacing;
            feelTag.gameObject.SetActive(false);
            game.events.On<ArsenalEvent>("primed", e => { if (e.owner == "player") FeelTag("PRIMED", Look.Hex(UnityTuning.TagPrimedHex)); });
            game.events.On<ArsenalEvent>("crossHit", e =>
            {
                if (e.owner != "player") return;
                string path = "weapons.families." + e.family;
                FeelTag(e.combo ?? "CROSS", e.family != null && cfg.Has(path) ? Look.Hex((uint)cfg.Num(path)) : Color.white);
            });
            game.events.On<ArsenalEvent>("swapKill", e => { if (e.owner == "player") FeelTag("SWAP", Look.Hex(UnityTuning.TagSwapHex)); });
            // #38
            chargeEdgeOpacity = cfg.Num("hud.chargeEdgeOpacity");
            chargeEdge = Img(values, "Charge edge", Color.white, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            chargeEdge.sprite = InsetShadowSprite(UnityTuning.ChargeEdgeBlur, UnityTuning.ChargeEdgeSpread, 1);
            chargeEdge.type = UnityEngine.UI.Image.Type.Sliced;
            chargeEdge.color = new Color(1, 1, 1, 0);
            chargeEdge.transform.SetSiblingIndex(exposedEdge.transform.GetSiblingIndex() + 1); // the web's DOM: after EXPOSED's edge
        }

        /// <summary>hud.js tag(text, color, 0.7): the text and colour now, the fade from the start.</summary>
        void FeelTag(string text, Color c)
        {
            feelTag.text = text;
            feelTag.color = new Color(c.r, c.g, c.b, feelTag.color.a);
            feelTagAge = 0;
            shownFeelTag = -1; // re-apply the alpha with the new colour
        }

        void RenderFeelTags(double frameDt)
        {
            // #37
            if (feelTagAge < UnityTuning.TagTime) feelTagAge += frameDt;
            float a = feelTagAge < UnityTuning.TagTime ? (float)Math.Round(Math.Min(1, UnityTuning.TagFadeRate * (UnityTuning.TagTime - feelTagAge)), 2) : 0;
            // NIGHT18 U3-31.6 round 1 (build 3: horde5's Tag whole on its first frame): it enters (grows from 0.6x and fades
            // in: HudEntrance18; FeelAB on); off: x 1
            float ts = 1;
            if (FeelAB.On && a > 0 && feelTagAge < UnityTuning.Hud18EnterTime + frameDt)
            {
                ts = HudEntrance18.Scale(feelTagAge);
                a = (float)Math.Round(a * HudEntrance18.Alpha(feelTagAge), 2);
            }
            if (ts != feelTagShownScale18)
            {
                feelTagShownScale18 = ts;
                feelTag.rectTransform.localScale = new Vector3(ts, ts, 1);
            }
            if (a != shownFeelTag)
            {
                shownFeelTag = a;
                Color c = feelTag.color;
                c.a = a;
                feelTag.color = c;
                if (feelTag.gameObject.activeSelf != a > 0) feelTag.gameObject.SetActive(a > 0);
            }
            // #38
            string gun = game.loadout != null ? game.loadout.Weapon : null;
            if (!ReferenceEquals(gun, chargeGun))
            {
                chargeGun = gun;
                string fam = !string.IsNullOrEmpty(gun) && cfg.Has("weapons.catalog." + gun + ".family") ? cfg.Str("weapons.catalog." + gun + ".family") : null;
                Color fc = fam != null && cfg.Has("weapons.families." + fam) ? Look.Hex((uint)cfg.Num("weapons.families." + fam)) : Color.white;
                chargeEdge.color = new Color(fc.r, fc.g, fc.b, chargeEdge.color.a);
            }
            double charge = game.Active && game.flow == Flow.Playing && game.loadout != null ? game.loadout.RailCharge : 0;
            float ca = (float)Math.Round(charge * chargeEdgeOpacity, 2);
            if (ca != shownChargeEdge)
            {
                shownChargeEdge = ca;
                Color c = chargeEdge.color;
                c.a = ca;
                chargeEdge.color = c;
            }
        }
    }
}
