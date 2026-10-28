// NIGHT47 builder F, R4 idea 1: finale47 (a view row, design.n47.finale47; NIGHT47.md section 6's list: "a boss finale beat (a short slow motion
// on the last hit, the arena reacting)"; "boss names ... big and readable"). Today a boss's kill (BossKit.Down: "bossDown") has bossKit's own slow
// motion (bossDying, main's killSlowMo) and a reward, but no beat of its own: no word on screen, the room does not answer, and the camera does not
// move (read in code: only the BULWARK's death bits listen to bossDown). Research (Research/2026-10-18-wave47/F.md R15, R16): a boss's last hit holds
// longer than an ordinary hitstop and players accept it when it is the boss-defeated moment (Reported: an itch.io dev's reply); a boss finisher
// freezes the boss mid-motion while the camera moves in (Reported: Onimusha: Way of the Sword's wiki); the weight ladder's heavy tier is the
// camera's ceiling (F-cards 1). With the row on, on bossDown:
//   - the TITLE: "<NAME> DOWN" in big type (Finale47TitlePx, the boss font) in the upper third, in by Finale47InS, held Finale47HoldS, out by
//     Finale47OutS (never over the reticle: its centre is Finale47TitleY from the top);
//   - the ARENA (Fx47View.Weapons.cs OnFinale47): a white-cyan light flare from the boss (the LightPools flash slot: never a new Light), two floor
//     rings out from it to Finale47RingM, and the camera's heavy-tier hit (up, with a roll), off under REDUCED MOTION.
// Off: nothing. Numbers: UnityTuning.F47.cs Finale47*.
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>finale47's switch (the reel forces it) and probes.</summary>
    public static class Finale47
    {
        public static bool? Force;
        /// <summary>The a747 reel's beat: the views hear it as they hear bossDown; nothing in the sim does (no reward, no campaign step).</summary>
        public const string TestEvent = "finale47Test";
        static GameConfig readFor;
        static bool on;

        public static bool On(GameConfig cfg)
        {
            if (Force.HasValue) return Force.Value;
            if (cfg == null) return false;
            if (!ReferenceEquals(cfg, readFor)) { readFor = cfg; on = NightRows.ViewOn(cfg, "finale47"); }
            return on;
        }

        /// <summary>For the reel: the title shown last, the beats seen, the title's opacity now.</summary>
        public static string Title = "";
        public static int Beats;
        public static float TitleAlpha;
    }

    public sealed partial class Ui
    {
        UiStyle33.Text33 finale47;
        double finale47Age = -1;

        /// <summary>From BuildHud47: the title label (hidden) and the bossDown hook.</summary>
        void Finale47Build()
        {
            var top = new Vector2(0.5f, 1);
            finale47 = UiStyle33.Label(hud33, "Finale title 47", UnityTuning.Finale47TitlePx, UiStyle33.Bone, UiStyle33.FaceNumber, TextAlignmentOptions.Center, top, top,
                new Vector2(0, -UnityTuning.Finale47TitleY), new Vector2(UnityTuning.Finale47TitleW, UnityTuning.Finale47TitlePx + 16));
            finale47.main.textWrappingMode = finale47.back.textWrappingMode = TextWrappingModes.NoWrap;
            finale47.main.richText = finale47.back.richText = false; // a name, never markup
            Outline47(finale47);
            sized47.Add((finale47, UnityTuning.Finale47TitlePx));
            finale47.Alpha = 0f;
            System.Action<BossEvent> show = e =>
            {
                if (!Finale47.On(game.cfg) || e == null) return;
                string name = !string.IsNullOrEmpty(e.name) ? e.name : e.boss ?? "";
                string title = (name + " " + UnityTuning.Finale47Word).Trim().ToUpperInvariant();
                finale47.Set(title);
                finale47Age = 0;
                Finale47.Title = title;
                Finale47.Beats++;
            };
            game.events.On("bossDown", show);
            game.events.On(Finale47.TestEvent, show); // the reel's beat (no sim listener hears it: no reward, no level end)
            game.OnNewRun += () => { finale47Age = -1; if (finale47 != null) finale47.Alpha = 0f; };
        }

        /// <summary>Each HUD frame (from Line47Tick): the title's in / hold / out.</summary>
        void Finale47Tick(double frameDt)
        {
            if (finale47 == null || finale47Age < 0) return;
            finale47Age += frameDt;
            double t = finale47Age, inS = UnityTuning.Finale47InS, holdS = UnityTuning.Finale47HoldS, outS = UnityTuning.Finale47OutS;
            float a = t < inS ? (float)(t / inS) : t < inS + holdS ? 1f : (float)System.Math.Max(0, 1 - (t - inS - holdS) / outS);
            if (t >= inS + holdS + outS) { a = 0f; finale47Age = -1; }
            // in: a slam from 1.25x down to its size (the title lands with the kill)
            float k = t < inS ? 1f + 0.25f * (1f - (float)(t / inS)) : 1f;
            finale47.rt.localScale = new Vector3(k, k, 1);
            finale47.Alpha = a;
            Finale47.TitleAlpha = a;
        }
    }
}
