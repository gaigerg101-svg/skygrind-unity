// NIGHT25 (builder F, carry-over 7): the weaponSwitch HUD cue. The pad's Y tap on layout v4 / v5 emits the port-only "weaponSwitch" (Core/WeaponSwitch24.cs); the weapon card
// answers it: a cyan WIPE runs across the weapon plate in UnityTuning.WeaponSwitch25Time (0.15 s), left to right, fading as it goes (alpha never above WeaponSwitch25Alpha, ramped
// in over its first frames: no pop-in), and the new gun's name is slammed into the card's header again (the swap name, S24, 18 px being the floor; WeaponCardSet shows it on the gun
// change, this restarts it so a switch to the SAME-named slot still answers). REDUCED MOTION: the wipe is a plain flash in place. View only: it reads the event, never the sim.
// Wiring: UiMeta's BuildLesson25 / RenderLesson25 call BuildWeaponSwitch25 / RenderWeaponSwitch25 (the plate exists by then; a plate built later is picked up lazily).
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        UnityEngine.UI.Image wsFlash25;
        RectTransform wsFlashRt25;
        double wsAge25 = double.MaxValue;
        bool wsSubscribed25;

        /// <summary>For the reel: switches seen, the wipe's last alpha, its peak, and the name's px.</summary>
        public static int WeaponSwitch25Count;
        public static float WeaponSwitch25AlphaNow, WeaponSwitch25Peak, WeaponSwitch25FirstAlpha = -1, WeaponSwitch25NamePx;

        void BuildWeaponSwitch25()
        {
            if (wsSubscribed25) return;
            wsSubscribed25 = true;
            game.events.On(WeaponSwitchEvent.Name, _ =>
            {
                WeaponSwitch25Count++;
                wsAge25 = 0;
                WeaponSwitch25Peak = 0;
                WeaponSwitch25FirstAlpha = -1;
                if (weaponSwapName != null && game.flow == Flow.Playing) weaponSwapAge = 0; // the new gun's name again
            });
        }

        void RenderWeaponSwitch25(double frameDt)
        {
            if (wsFlash25 == null)
            {
                if (weaponPlate == null) return;
                wsFlash25 = Img(weaponPlate.rectTransform, "Switch wipe", UiTheme.CyanHot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                wsFlashRt25 = wsFlash25.rectTransform;
                wsFlashRt25.offsetMin = wsFlashRt25.offsetMax = Vector2.zero;
                wsFlash25.color = UiTheme.A(UiTheme.CyanHot, 0);
                wsFlash25.enabled = false;
            }
            if (wsAge25 >= UnityTuning.WeaponSwitch25Time)
            {
                if (wsFlash25.enabled)
                {
                    wsFlash25.enabled = false;
                    WeaponSwitch25AlphaNow = 0;
                }
                return;
            }
            wsAge25 += frameDt;
            float t = Mathf.Clamp01((float)(wsAge25 / UnityTuning.WeaponSwitch25Time));
            bool still = game.settings != null && game.settings.View.reducedMotion;
            float ramp = Mathf.Clamp01((float)(wsAge25 / UnityTuning.WeaponSwitch25RampIn)); // alpha 0 on the first frame: no pop-in
            float a = UnityTuning.WeaponSwitch25Alpha * (1f - t) * ramp;
            wsFlash25.enabled = true;
            wsFlash25.color = UiTheme.A(UiTheme.CyanHot, a);
            wsFlashRt25.anchorMin = Vector2.zero;
            wsFlashRt25.anchorMax = new Vector2(still ? 1f : 1f - (1f - t) * (1f - t), 1f); // the wipe's leading edge, eased out
            wsFlashRt25.offsetMin = wsFlashRt25.offsetMax = Vector2.zero;
            if (WeaponSwitch25FirstAlpha < 0) WeaponSwitch25FirstAlpha = a;
            WeaponSwitch25AlphaNow = a;
            WeaponSwitch25Peak = Mathf.Max(WeaponSwitch25Peak, a);
            if (weaponSwapName != null) WeaponSwitch25NamePx = weaponSwapName.fontSize;
        }
    }
}
