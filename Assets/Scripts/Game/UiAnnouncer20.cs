// NIGHT20 U3-34.9 F1 (builder H): the announcer (design.adds.announcer; Research/2026-10-06-additions-backlog.md F1: "Without
// audio from us, text must carry what CoD's announcer and music carry"; UT's announcer, CoD's voiced round beats).
// - One slab per run beat (Announcer20's table): `specialRound` (E's special rounds by name and phase: BOSS INBOUND, THE
//   HUNT, ELITE INBOUND, the twists, LAST ONE, BOSS DOWN), `swarmRound` (THE SWARM), `exfilCall` (EXFIL CALLED),
//   `exfilStart` (EXFIL OPEN), `powerOn` (POWER ON), and LAST ONE from the director's count (LastOne20: once a round that
//   had a crowd). A special round names itself once a round (an elite's announce and start are one beat).
// - At most one slab on screen, one start per 1.5 s, a priority queue (AnnouncerQueue20: the boss cuts in, a beat that
//   waited 2.5 s is stale), <= 3 words.
// - The slab: S56 DISPLAY words on an INK slanted plate (UiShape, the theme's cut) with an accent bar along its foot:
//   DANGER for a threat, CYAN for the player's beats (Announcer20.Danger); it SLAMS in (HudSlam18: 1.6x at full alpha), its
//   plate WHITE-hot for its first frames (game.flashGate, FLASH INTENSITY 0: none), holds, fades (ease-in) by 1.5 s.
// - It stands at the banner's place (top centre, out of S14's box); the banner waits under it (Ui.cs: its time paused, hidden)
//   and re-enters when the slab is gone, so two texts never stack.
// The announcer's clock is frame time while playing (PAUSED holds it). Events go through the bus (no per-frame allocation:
// the slabs are cached strings, SetText on change only). FeelProbe event "announcerSlab" on a slab's first frame.
using System;
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        RectTransform slab20;
        UiShape slabPlate20, slabAccent20;
        TextMeshProUGUI slabWords20;
        readonly AnnouncerQueue20 slabQueue20 = new AnnouncerQueue20();
        readonly LastOne20 slabLastOne20 = new LastOne20();
        bool slabCfg20, slabLive20;
        double slabClock20;
        string slabShown20;
        float slabScaleShown20 = -1, slabAlphaShown20 = -1;
        int slabWhiteShown20 = -1;
        bool slabWhiteOk20;
        int slabLastOneRound20 = -1;
        string slabSpecialKey20; // the last special round named (name), and its round: once a round
        int slabSpecialRound20 = -1;
        bool slabHolding20;

        /// <summary>Ui.cs: a slab is on screen (the banner waits under it).</summary>
        bool Announcer20Holds => slabHolding20;

        void BuildAnnouncer20()
        {
            slabCfg20 = cfg.Design("adds.announcer.enabled");
            float h = UnityTuning.Hud20SlabH;
            slab20 = Hud17Rect(values, "Announcer 20", new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f), new Vector2(0, -UnityTuning.Hud20SlabTop - h * 0.5f), new Vector2(600, h));
            slabPlate20 = Hud17Shape(slab20, "Plate", UiTheme.Ink, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(600, h));
            slabAccent20 = Hud17Shape(slab20, "Accent", UiTheme.Cyan, new Vector2(0.5f, 0), new Vector2(0.5f, 0), Vector2.zero, new Vector2(600, UnityTuning.ThemeAccentBar * 1.5f));
            slabAccent20.SetCuts(0, 0);
            float px = UiTheme.Px(UiTheme.Size.S56);
            slabWords20 = Hud17Text(slab20, "Words", "", px, UiTheme.White, UiTheme.Display, TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 2), new Vector2(1200, h), true);
            slab20.gameObject.SetActive(false);
            HudFx20.DoEdgeGlow(slabPlate20); // row 5: the slab's plate wears the CYAN edge (UIEffect; one of two plates that do)

            game.events.On<SpecialRoundEvent>(N20Events.SpecialRound, e =>
            {
                if (!slabLive20 || e == null) return;
                string s = Announcer20.ForSpecial(e, out int prio);
                if (s == null) return;
                if (e.phase != "last" && e.phase != "end")
                {
                    // a special round names itself once a round (an elite's announce and its start are one beat)
                    if (e.name == slabSpecialKey20 && e.round == slabSpecialRound20) return;
                    slabSpecialKey20 = e.name;
                    slabSpecialRound20 = e.round;
                }
                if (s == Announcer20.LastOne)
                {
                    if (slabLastOneRound20 == e.round) return;
                    slabLastOneRound20 = e.round;
                }
                slabQueue20.Push(s, prio, slabClock20);
            });
            game.events.On("swarmRound", _ => { if (slabLive20) slabQueue20.Push(Announcer20.TheSwarm, Announcer20.Special, slabClock20); });
            game.events.On("exfilCall", _ => { if (slabLive20) slabQueue20.Push(Announcer20.ExfilCalled, Announcer20.Exfil, slabClock20); });
            game.events.On("exfilStart", _ => { if (slabLive20) slabQueue20.Push(Announcer20.ExfilOpen, Announcer20.Exfil, slabClock20); });
            game.events.On("powerOn", _ => { if (slabLive20) slabQueue20.Push(Announcer20.PowerOn, Announcer20.Info, slabClock20); });
            game.OnNewRun += () =>
            {
                slabQueue20.Clear();
                slabLastOne20.Reset();
                slabLastOneRound20 = slabSpecialRound20 = -1;
                slabSpecialKey20 = null;
            };
        }

        void RenderAnnouncer20(double frameDt)
        {
            if (slab20 == null) return;
            slabLive20 = slabCfg20 && game.sim != null && game.sim.n20.adds;
            bool playing = game.flow == Flow.Playing;
            if (slabLive20 && playing)
            {
                slabClock20 += frameDt;
                // LAST ONE from the director's count (once a round that had a crowd; E's own "last" counts as the same beat)
                SurvivalRun run = game.run;
                bool fight = run.state == SurvivalRun.State.Fight;
                if (slabLastOne20.Step(run.round, fight, fight ? game.director.Remaining : 0) && slabLastOneRound20 != run.round)
                {
                    slabLastOneRound20 = run.round;
                    slabQueue20.Push(Announcer20.LastOne, Announcer20.Last, slabClock20);
                }
                if (slabQueue20.Step(slabClock20))
                {
                    FeelProbe.Event(UnityTuning.Hud20SlabEvent);
                    slabWhiteOk20 = game.settings.View.flashIntensity > 0 && game.flashGate.TryFlash(SurvivalGame.RealNow);
                }
            }
            string showing = slabLive20 ? slabQueue20.Showing : null;
            bool on = showing != null && playing;
            if (slab20.gameObject.activeSelf != on) slab20.gameObject.SetActive(on);
            bool hold = showing != null;
            if (hold != slabHolding20)
            {
                slabHolding20 = hold;
                if (!hold && bannerLeft > 0) bannerEnterAge = 0; // the banner re-enters once the slab is gone
            }
            if (!on) return;
            if (!ReferenceEquals(showing, slabShown20))
            {
                slabShown20 = showing;
                slabWords20.SetText(showing);
                HudFx20.DoReveal(slabWords20, game.settings.View.reducedMotion); // row 5: the words reveal letter by letter (LitMotion)
                float w = Mathf.Ceil(slabWords20.GetPreferredValues(showing).x + 2 * UnityTuning.Hud20SlabPadX);
                slab20.sizeDelta = new Vector2(w, UnityTuning.Hud20SlabH);
                slabPlate20.rectTransform.sizeDelta = new Vector2(w, UnityTuning.Hud20SlabH);
                slabAccent20.rectTransform.sizeDelta = new Vector2(w, UnityTuning.ThemeAccentBar * 1.5f);
                slabAccent20.color = Announcer20.Danger(showing) ? UiTheme.Danger : UiTheme.Cyan;
                slabWhiteShown20 = -1;
                slabScaleShown20 = slabAlphaShown20 = -1;
            }
            double age = slabClock20 - slabQueue20.StartedAt;
            int white = slabWhiteOk20 && Slab20.White(age) ? 1 : 0;
            if (white != slabWhiteShown20)
            {
                slabWhiteShown20 = white;
                slabPlate20.color = white == 1 ? UiTheme.A(UiTheme.White, UiTheme.Ink.a) : UiTheme.Ink; // NIGHT22 T-12: the white beat keeps the plate's alpha (1.0 vs 0.96 made M9 take the white frames as the rest and never settle)
                // with the letter-by-letter reveal (row 5) the words stay WHITE: the reveal holds each letter's colour from its
                // start, and its letters are still near-transparent through the plate's three white frames
                slabWords20.color = white == 1 && !HudFx20.On ? UiTheme.InkSolid : UiTheme.White;
                slabAlphaShown20 = -1;
            }
            float s = game.settings.View.reducedMotion ? 1f : Slab20.Scale(age);
            if (s != slabScaleShown20)
            {
                slabScaleShown20 = s;
                slab20.localScale = new Vector3(s, s, 1);
            }
            float a = Mathf.Round(Slab20.Alpha(age) * UiFade25.In(age, frameDt) * 100f) / 100f; // NIGHT25 (T-12, M9): the slam keeps its scale; its alpha ramps in
            if (a != slabAlphaShown20)
            {
                slabAlphaShown20 = a;
                slabWords20.alpha = a;
                slabPlate20.color = UiTheme.A(white == 1 ? UiTheme.White : UiTheme.Ink, UiTheme.Ink.a * a);
                Color acc = Announcer20.Danger(showing) ? UiTheme.Danger : UiTheme.Cyan;
                slabAccent20.color = UiTheme.A(acc, a);
            }
        }
    }
}
