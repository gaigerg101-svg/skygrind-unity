// NIGHT35 builder W (charge35 on THE ARSENAL RAIL; audit.md item 1: "the rail has no CHARGING state"). Called once a frame at the end of RenderRail34
// (one hook line in UiRail34.cs). With charge35 live in the sim (the loadout's ChargeShots27 has its c35) and not -skygrindOff35:
//   - a hold on the KESTREL / THUNDERCLAP in hand: the hand bar under its slot FILLS cyan with the charge's level (Loadout.Charge35Level, 0..1 from the
//     press); the label over the rail (hud33's LT label, 16 px) reads CHARGING, then CHARGED in white at full; the hand icon pulses at full;
//   - the charge's cooldown after a charged shot (ChargeShots27.cooldown, charge35's short cooldownS): the hand bar (gun in hand) or the slot's line
//     (holstered) drains in CyanDeep as it runs out (the rail's own cooling read), so the player sees when the next charge is back.
// No new rect (the label and the bars are the rail's and hud33's own: the hud33 rule checker's boxes are unchanged). View only: reads sim state.
// HUD33's two reticle arcs stay (UiHud33.cs, untouched). Numbers: UnityTuning.Weapons35.cs.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        bool c35RailOn, c35RailRead, c35LabelOwned;
        double c35RailClock;
        static readonly Color Invalid35 = new Color(-1f, -1f, -1f, -1f); // a cache value no real colour equals: the rail re-applies its colour

        /// <summary>For the reels (charge35): the rail's charge read this frame: "CHARGING" / "CHARGED" / "" and the hand bar's fill (-1: not overridden).</summary>
        public static string Charge35RailText = "";
        public static float Charge35RailFill = -1f;
        /// <summary>For the reels: the charge's cooldown share the rail drew (0: none).</summary>
        public static float Charge35RailCool;

        void RenderRail35Charge(int handIdx, double frameDt)
        {
            if (!c35RailRead) { c35RailOn = N35Fixes.On; c35RailRead = true; }
            c35RailClock += frameDt;
            Charge35RailText = "";
            Charge35RailFill = -1f;
            Charge35RailCool = 0f;
            Loadout lo = game.loadout;
            ChargeShots27 c = lo != null ? lo.charge27 : null;
            Charge35 q = c35RailOn && c != null && c.active ? c.c35 : null;
            if (q == null) { ReleaseLabel35(); return; }
            float level = 0f;
            for (int i = 0; i < r34Slots.Count; i++)
            {
                Rail34Slot s = r34Slots[i];
                if (s.id == null || !s.rt.gameObject.activeSelf) continue;
                int gi = ChargeShots27.Index(s.id);
                if (gi < 0) continue;
                bool hand = i == handIdx;
                if (hand && q.holding && q.gun == gi && (!q.tap36 || q.started)) // NIGHT36 F tap36: CHARGING only once a real hold has started
                {
                    level = Mathf.Round((float)lo.Charge35Level * 50f) / 50f;
                    // the hand bar fills with the charge; white at full
                    Color hc = level >= 1f ? Color.white : UiStyle33.Cyan;
                    r34Hand.fillAmount = level;
                    r34Hand.color = hc;
                    r34ShownHandFill = -1; // the rail re-applies its own next frame
                    r34ShownHandC = Invalid35;
                    Charge35RailFill = level;
                    float pulse = level >= 1f ? Mathf.Abs(Mathf.Sin((float)(c35RailClock * UnityTuning.Charge35PulseHz * Mathf.PI))) : 0f;
                    float sc = s.shownScale > 0 ? s.shownScale : 1f;
                    var size = new Vector2(UnityTuning.Rail34IconW * sc, UnityTuning.Rail34IconH * sc) * (1f + UnityTuning.Charge35RailPulseScale * pulse);
                    s.fill.rectTransform.sizeDelta = size;
                    s.shownScale = -1; // re-applied by the rail next frame
                    continue;
                }
                if (c.cooldown[gi] <= 0) continue;
                float share = Mathf.Round((float)c.CooldownShare(gi) * 50f) / 50f;
                if (hand)
                {
                    if (share > 0f)
                    {
                        r34Hand.fillAmount = share;
                        r34Hand.color = UiStyle33.CyanDeep;
                        r34ShownHandFill = -1;
                        r34ShownHandC = Invalid35;
                        Charge35RailCool = share;
                    }
                }
                else if (share > 0f && s.line != null)
                {
                    s.line.fillAmount = share;
                    s.line.color = UiTheme.A(UiStyle33.CyanDeep, UnityTuning.Rail34CoolLineAlpha);
                    s.line.enabled = true;
                    s.shownLine = s.shownLineA = -1; // re-applied by the rail next frame
                    s.shownLineC = Invalid35;
                }
            }
            if (level > 0f && h33Lt != null)
            {
                string t = level >= 1f ? UnityTuning.Charge35RailFull : UnityTuning.Charge35RailCharging;
                h33Lt.Set(t);
                h33Lt.Color = level >= 1f ? Color.white : UiStyle33.Cyan;
                c35LabelOwned = true;
                Charge35RailText = t;
            }
            else ReleaseLabel35();
        }

        /// <summary>The LT label back to hud33's colour (its text is hud33's own Set next frame).</summary>
        void ReleaseLabel35()
        {
            if (!c35LabelOwned || h33Lt == null) return;
            c35LabelOwned = false;
            h33Lt.Color = UiStyle33.Cyan;
        }
    }
}
