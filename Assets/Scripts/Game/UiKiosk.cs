// NIGHT14 U3-27.7 (agent K): Ui's kiosk half (kiosk.js's DOM; the logic is Kiosk.cs, the attract SurvivalGame.Kiosk.cs).
// - The hints (kiosk.js drawHints, only with -skygrindKiosk): CONTROLS, then a row per control (the key in a dark box in
//   gold, what it does in the text colour; each line right-aligned), then the foot, at the bottom right of TITLE (a child
//   of the TITLE screen, so it shows exactly while TITLE does, over the attract too: kiosk.js shows it while isTitle()).
//   The keys are the LIVE bindings' labels (Ui.KeyLabel, as PAUSED's YOUR KEYS card), refilled when the bindings change
//   (Bindings.Version: bindings.onChange(drawHints)); the sizes are kiosk.js's CSS (UnityTuning.Kiosk.cs).
// - No way out (kiosk.js: "the pause menu has no way out (no QUIT)"): TITLE's QUIT and PAUSED's QUIT TO TITLE hidden at
//   build (found by their labels: Ui.Button names each button's object after its label), and PAUSED's SAVE & QUIT (it
//   leaves to TITLE too) hidden whenever Ui shows it.
// - The attract's lock on TITLE (kiosk or not): while the attract plays, TITLE's controls take no click, hover or submit
//   (a CanvasGroup), so the input that stops the attract can't also press PLAY; SurvivalGame.Kiosk.cs unlocks the frame
//   after. The title's buttons keep their look while locked (their disabled tint is their normal one: nothing else ever
//   disables them).
// Per frame: two compares (the bindings' version, SAVE & QUIT shown); a refill only on a rebind. Nothing allocates.
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        /// <summary>One hints row: its bindings (null: a fixed key), the key's label and box, the label shown.</summary>
        sealed class KioskHintRow
        {
            public string[] actions;
            public string fixedKey;
            public TextMeshProUGUI key;
            public RectTransform keyBack;
            public float labelWidth;
            public string shown;
        }

        readonly List<KioskHintRow> kioskHintRows = new List<KioskHintRow>(8);
        readonly StringBuilder kioskKeySb = new StringBuilder(32);
        RectTransform kioskHints;
        int kioskHintsVersion = int.MinValue;
        float kioskRowHeight;
        CanvasGroup kioskTitleGroup;
        bool kioskNoWayOut, kioskTitleLocked;

        /// <summary>TITLE shows on its own (no SETTINGS, CONTROLS, CONTRACTS / WEAPONS or MODS draft over it): the attract may start.</summary>
        public bool KioskTitleBare => title.activeSelf && !settingsOpen && !controlsOpen && !MetaOpen && !game.DraftOpen;

        partial void BuildKiosk()
        {
            // The attract's lock (both modes: D-54 plays it on a normal TITLE too).
            kioskTitleGroup = title.GetComponent<CanvasGroup>();
            if (kioskTitleGroup == null) kioskTitleGroup = title.AddComponent<CanvasGroup>();
            foreach (UnityEngine.UI.Selectable s in title.GetComponentsInChildren<UnityEngine.UI.Selectable>(true))
            {
                UnityEngine.UI.ColorBlock c = s.colors;
                c.disabledColor = c.normalColor;
                s.colors = c;
            }
            if (!Kiosk.Enabled) return;
            kioskNoWayOut = true;
            if (KioskHide(title.transform, "QUIT")) LayoutTitleColumn();
            if (KioskHide(pause.transform, "QUIT")) LayoutPauseColumn(); // NIGHT17: PAUSED's QUIT (SAVE & QUIT at a break) is the way out
            BuildKioskHints();
        }

        /// <summary>Hide `parent`'s button labelled `label` (Ui.Button names its object after the label). False if it isn't there.</summary>
        static bool KioskHide(Transform parent, string label)
        {
            Transform t = parent.Find(label);
            if (t == null)
            {
                Debug.LogWarning($"[kiosk] no '{label}' button under {parent.name}: it stays");
                return false;
            }
            t.gameObject.SetActive(false);
            return true;
        }

        /// <summary>kiosk.js drawHints' panel: CONTROLS, the rows, the foot; bottom right, every line right-aligned.</summary>
        void BuildKioskHints()
        {
            kioskHints = new GameObject("Kiosk hints", typeof(RectTransform)).GetComponent<RectTransform>();
            kioskHints.SetParent(title.transform, false);
            kioskHints.anchorMin = kioskHints.anchorMax = kioskHints.pivot = new Vector2(1, 0);
            kioskHints.anchoredPosition = new Vector2(-UnityTuning.KioskHintsRight, UnityTuning.KioskHintsBottom);
            kioskHints.sizeDelta = Vector2.zero;
            var corner = new Vector2(1, 0);
            float w = UnityTuning.KioskHintsWidth;

            // Bottom up: the foot, the rows (last first), the head.
            float footH = UnityTuning.KioskFootSize * UnityTuning.KioskLineHeight;
            Color footColor = text;
            footColor.a = UnityTuning.KioskFootAlpha;
            TextMeshProUGUI foot = Text(kioskHints, "Foot", UnityTuning.KioskFoot, UnityTuning.KioskFootSize, footColor, TextAlignmentOptions.Right, Vector2.zero, new Vector2(w, footH), corner, Face.Body);
            foot.characterSpacing = UnityTuning.KioskFootSpacing;
            foot.fontStyle = FontStyles.Bold;

            kioskRowHeight = UnityTuning.KioskRowSize * UnityTuning.KioskLineHeight;
            var rows = new List<(string[] actions, string fixedKey, string label)>();
            foreach (var r in UnityTuning.KioskRows)
                if (!KioskSkipped(r.actions)) rows.Add(r);
            Color keyBack = panel;
            keyBack.a = UnityTuning.KioskKeyBackAlpha;
            float y = footH + UnityTuning.KioskFootGap;
            for (int i = rows.Count - 1; i >= 0; i--)
            {
                var (actions, fixedKey, label) = rows[i];
                TextMeshProUGUI does = Text(kioskHints, "Does " + label, label, UnityTuning.KioskRowSize, text, TextAlignmentOptions.Right, new Vector2(0, y), new Vector2(w, kioskRowHeight), corner, Face.Body);
                does.characterSpacing = UnityTuning.KioskRowSpacing;
                does.fontStyle = FontStyles.Bold;
                float labelWidth = does.GetPreferredValues(label).x;
                UnityEngine.UI.Image back = Img(kioskHints, "Key " + label, keyBack, corner, corner, Vector2.zero, new Vector2(UnityTuning.KioskKeyMinWidth, kioskRowHeight));
                RectTransform br = back.rectTransform;
                br.pivot = corner;
                br.anchoredPosition = new Vector2(-(labelWidth + UnityTuning.KioskKeyGap), y);
                TextMeshProUGUI key = Text(br, "Key", fixedKey ?? "", UnityTuning.KioskRowSize, gold, TextAlignmentOptions.Center, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f), Face.Body);
                RectTransform kr = key.rectTransform;
                kr.anchorMin = Vector2.zero;
                kr.anchorMax = Vector2.one;
                kr.offsetMin = kr.offsetMax = Vector2.zero;
                key.characterSpacing = UnityTuning.KioskRowSpacing;
                key.fontStyle = FontStyles.Bold;
                kioskHintRows.Add(new KioskHintRow { actions = actions, fixedKey = fixedKey, key = key, keyBack = br, labelWidth = labelWidth });
                y += kioskRowHeight;
            }

            TextMeshProUGUI head = Text(kioskHints, "Head", UnityTuning.KioskHead, UnityTuning.KioskHeadSize, usable, TextAlignmentOptions.Right, new Vector2(0, y + UnityTuning.KioskHeadGap), new Vector2(w, UnityTuning.KioskHeadSize * UnityTuning.KioskHeadLine), corner, Face.Display);
            head.characterSpacing = UnityTuning.KioskHeadSpacing;
            RefreshKioskHints();
        }

        /// <summary>A row left out of the port's hints: every action of it is one PAUSED's card skips (the ARCLINE's).</summary>
        static bool KioskSkipped(string[] actions)
        {
            if (actions == null || actions.Length == 0) return false;
            foreach (string a in actions)
                if (System.Array.IndexOf(UnityTuning.PauseKeysSkip, a) < 0) return false;
            return true;
        }

        /// <summary>The hints' stamp: the bindings' version and (NIGHT19) PromptText's (the device, the pad's set and layout).</summary>
        int KioskHintsStamp => (LiveBindings != null ? LiveBindings.Version : 0) * 7919 + PromptText.Version;

        /// <summary>drawHints: each row's key from the live bindings (set, and its box sized, only when its label changed).
        /// NIGHT19 U3-33.4: on a pad (design.pad.n19), the drawn buttons: MOVE the left stick, the MOUSE row the right stick,
        /// RT and LT, HOLD TO BUY the ringed glyph.</summary>
        void RefreshKioskHints()
        {
            kioskHintsVersion = KioskHintsStamp;
            bool pad = PlayerDevices.N19 && PlayerDevices.Last == InputDevice.Pad;
            foreach (KioskHintRow r in kioskHintRows)
            {
                string k = r.actions == null ? (pad ? KioskPadMouse() : r.fixedKey) : KioskKeyText(r.actions);
                if (k == r.shown) continue;
                r.shown = k;
                r.key.text = k;
                float kw = Mathf.Max(UnityTuning.KioskKeyMinWidth, r.key.GetPreferredValues(k).x + 2 * UnityTuning.KioskKeyPad);
                r.keyBack.sizeDelta = new Vector2(kw, kioskRowHeight);
            }
        }

        /// <summary>The keys of `actions` run together (kiosk.js: `${key('forward')}${key('left')}...`); "-" for one with no binding.</summary>
        string KioskKeyText(string[] actions)
        {
            bool hold = PlayerDevices.Last == InputDevice.Pad; // NIGHT19: HOLD TO BUY's ring on the pad (the keyboard's row says HOLD)
            if (actions.Length == 1) return PromptText.For(actions[0], hold && actions[0] == "interact", KeyLabel(cfg, actions[0]) ?? "-");
            kioskKeySb.Clear();
            foreach (string a in actions) kioskKeySb.Append(PromptText.For(a, false, KeyLabel(cfg, a) ?? "-"));
            return kioskKeySb.ToString();
        }

        /// <summary>NIGHT19: the MOUSE row on a pad: LOOK · FIRE · ALT as the right stick, RT and LT (the live layout's).</summary>
        string KioskPadMouse()
        {
            kioskKeySb.Clear();
            kioskKeySb.Append(PromptText.For("look", false, "")).Append(PromptText.For("fire", false, "")).Append(PromptText.For("altFire", false, ""));
            return kioskKeySb.ToString();
        }

        partial void RenderKiosk(double frameDt)
        {
            // The kiosk's PAUSED has no way out: Ui shows SAVE & QUIT at a break where a save is allowed; it goes again.
            if (kioskNoWayOut && pauseSaveQuit != null && pauseSaveQuit.activeSelf)
            {
                pauseSaveQuit.SetActive(false);
                LayoutPauseColumn();
            }
            if (kioskHints != null && title.activeSelf && KioskHintsStamp != kioskHintsVersion) RefreshKioskHints(); // bindings.onChange(drawHints); NIGHT19: the device too
        }

        /// <summary>
        /// The attract's lock on TITLE: locked, its controls take no click, hover or submit and nothing is selected (no focus
        /// frame over the attract); unlocked, UiNav relinks them and selects PLAY again.
        /// </summary>
        public void KioskTitleLock(bool locked)
        {
            if (kioskTitleGroup == null || locked == kioskTitleLocked) return;
            kioskTitleLocked = locked;
            kioskTitleGroup.interactable = !locked;
            kioskTitleGroup.blocksRaycasts = !locked;
            if (locked && EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            navDirty = true; // UiNav: the live controls changed (its list skips the non-interactable)
        }
    }
}
