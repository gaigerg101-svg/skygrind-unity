// NIGHT20 U3-34.9 C1 (builder H): the kinds' debut cards (design.adds.debut; Research/2026-10-06-additions-backlog.md C1:
// DOOM Eternal stops on a new demon to name its weak point; the Marauder's lesson: introduce a threat on purpose; Hodent's
// one at a time). The first time a kind with a card (Debut20: SWARMER, SPITTER, CHARGER, DRONE, LANCER, WARDEN, BULWARK; the
// husk has none) is on the field in this profile, at a calm moment (nothing within 8 m, no boss intro, no announcer slab
// up): a 1.2 s card under the slab line, the kind's name (S42 DISPLAY) over its one counter (S24, CYAN-PALE, <= 4 words, the
// web's own counter from its design note), on an INK slanted plate with a DANGER foot (an enemy's card). It slams in as a
// slab does, holds, fades. The profile flag "debut20.<kind>" marks it seen (saved at the round's clear, not mid-fight).
// The bus event `debutCard` (DebutCardEvent: the kind, the card's seconds) is the card's moment for GG's sounds and for the
// world's slow-down (the Game loop's: the lead's line; not drawn here). FeelProbe event "debutCard".
// Scans the live enemies at most once a frame while a card could still show; no allocation per frame.
using System;
using System.Collections.Generic;
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>C1: a debut card's moment on the bus (port-only `debutCard`): the kind and the card's seconds.</summary>
    public sealed class DebutCardEvent
    {
        public string kind;
        public double seconds;
    }

    public sealed partial class Ui
    {
        RectTransform debut20;
        UiShape debutPlate20, debutFoot20;
        TextMeshProUGUI debutName20, debutCounter20;
        bool debutCfg20, debutSavePending20, debutAllSeen20;
        double debutAge20 = -1;
        string debutKind20;
        float debutScaleShown20 = -1, debutAlphaShown20 = -1;
        readonly DebutCardEvent debutEvent20 = new DebutCardEvent();

        void BuildDebut20()
        {
            debutCfg20 = cfg.Design("adds.debut.enabled");
            float h = UnityTuning.Hud20DebutH;
            debut20 = Hud17Rect(values, "Debut card 20", new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f), new Vector2(0, -UnityTuning.Hud20DebutTop - h * 0.5f), new Vector2(600, h));
            debutPlate20 = Hud17Shape(debut20, "Plate", UiTheme.Ink, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(600, h));
            debutFoot20 = Hud17Shape(debut20, "Foot", UiTheme.Danger, new Vector2(0.5f, 0), new Vector2(0.5f, 0), Vector2.zero, new Vector2(600, UnityTuning.ThemeAccentBar * 1.5f));
            debutFoot20.SetCuts(0, 0);
            float namePx = UiTheme.Px(UiTheme.Size.S42), counterPx = UiTheme.Px(UiTheme.Size.S24);
            debutName20 = Hud17Text(debut20, "Name", "", namePx, UiTheme.White, UiTheme.Display, TextAlignmentOptions.Center, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -6), new Vector2(1000, namePx * 1.15f), true);
            debutCounter20 = Hud17Text(debut20, "Counter", "", counterPx, UiTheme.CyanPale, UiTheme.BodyBold, TextAlignmentOptions.Center, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 10), new Vector2(1000, counterPx * 1.25f), true);
            debutCounter20.characterSpacing = UnityTuning.ThemeLabelTracking;
            debut20.gameObject.SetActive(false);
            HudFx20.DoEdgeGlow(debutPlate20); // row 5: the card's plate wears the CYAN edge (UIEffect; the other plate that does)
            game.events.On("roundClear", _ =>
            {
                if (!debutSavePending20) return;
                debutSavePending20 = false;
                game.SaveProfile();
            });
            game.OnNewRun += () =>
            {
                debutAge20 = -1;
                debutAllSeen20 = game.profile != null && Debut20.AllSeen(game.profile); // every card seen: no more scans
                if (debutSavePending20)
                {
                    debutSavePending20 = false;
                    game.SaveProfile();
                }
            };
        }

        void RenderDebut20(double frameDt)
        {
            if (debut20 == null) return;
            bool live = debutCfg20 && game.sim != null && game.sim.n20.adds && game.profile != null;
            bool playing = game.flow == Flow.Playing;
            if (live && playing && debutAge20 < 0 && !debutAllSeen20 && !Announcer20Holds) Debut20Look();
            bool on = live && playing && debutAge20 >= 0;
            if (debut20.gameObject.activeSelf != on) debut20.gameObject.SetActive(on);
            if (!on) return;
            float s = game.settings.View.reducedMotion ? 1f : Slab20.Scale(debutAge20);
            if (s != debutScaleShown20)
            {
                debutScaleShown20 = s;
                debut20.localScale = new Vector3(s, s, 1);
            }
            float a = Mathf.Round(Debut20.Alpha(debutAge20) * 100f) / 100f;
            if (a != debutAlphaShown20)
            {
                debutAlphaShown20 = a;
                debutName20.alpha = a;
                debutCounter20.alpha = a;
                debutPlate20.color = UiTheme.A(UiTheme.Ink, UiTheme.Ink.a * a);
                debutFoot20.color = UiTheme.A(UiTheme.Danger, a);
            }
            debutAge20 += frameDt;
            if (debutAge20 >= UnityTuning.Hud20DebutLife) debutAge20 = -1;
        }

        /// <summary>A kind on the field whose card this profile hasn't seen, at a calm moment: start its card.</summary>
        void Debut20Look()
        {
            List<Enemy> all = game.enemies != null ? game.enemies.all : null;
            if (all == null) return;
            Vec3 feet = game.Feet;
            string kind = null;
            double nearest = double.PositiveInfinity;
            for (int i = 0; i < all.Count; i++)
            {
                Enemy e = all[i];
                if (e == null || !e.alive) continue;
                double dx = e.position.x - feet.x, dy = e.position.y - feet.y, dz = e.position.z - feet.z;
                double d = Math.Sqrt(dx * dx + dy * dy + dz * dz);
                if (d < nearest) nearest = d;
                if (kind == null && Debut20.Has(e.kind) && !game.profile.Flag(Debut20.Flag(e.kind))) kind = e.kind;
            }
            if (kind == null) return; // nothing new on the field (keep looking as new kinds come)
            bool bossIntro = false; // no boss intro in the port yet (A1 BULWARK: E's; its intro gates this)
            if (!Debut20.CanShow(false, nearest, bossIntro, true)) return;
            game.profile.SetFlag(Debut20.Flag(kind));
            debutSavePending20 = true;
            debutAllSeen20 = Debut20.AllSeen(game.profile);
            debutKind20 = kind;
            debutAge20 = 0;
            debutScaleShown20 = debutAlphaShown20 = -1;
            string name = Debut20.Name(kind), counter = Debut20.Counter(kind);
            debutName20.SetText(name);
            debutCounter20.SetText(counter);
            HudFx20.DoReveal(debutName20, game.settings.View.reducedMotion); // row 5: the name reveals letter by letter (LitMotion)
            float w = Mathf.Ceil(Mathf.Max(debutName20.GetPreferredValues(name).x, debutCounter20.GetPreferredValues(counter).x) + 2 * UnityTuning.Hud20DebutPadX);
            debut20.sizeDelta = new Vector2(w, UnityTuning.Hud20DebutH);
            debutPlate20.rectTransform.sizeDelta = new Vector2(w, UnityTuning.Hud20DebutH);
            debutFoot20.rectTransform.sizeDelta = new Vector2(w, UnityTuning.ThemeAccentBar * 1.5f);
            debutEvent20.kind = kind;
            debutEvent20.seconds = UnityTuning.Hud20DebutLife;
            game.events.Emit(UnityTuning.Hud20DebutEvent, debutEvent20);
            FeelProbe.Event(UnityTuning.Hud20DebutEvent);
        }
    }
}
