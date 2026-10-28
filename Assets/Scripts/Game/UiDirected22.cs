// NIGHT22 leftover 1 (builder M): DIRECTED STEPS, the HUD cue (design.n22.directed; the rule is Core/Directed22.cs). UNCOMPILED when written.
// RENDER SIDE ONLY: it listens to the bus and reads the player's position; the sim, the tape and the fingerprint are untouched.
// A small step list under the round banner: a header ("DIRECTED · THE BEACON" / "DIRECTED · THE HUNT") and three steps, the current one bright
// CYAN with a ">" and, on the beacon's first step, the distance and which way to turn ("34 m · AHEAD"); done steps muted, coming steps dim.
// Never red (rule 8: red-orange is enemies and danger; this is the player-usable cyan). The hunt has no place to walk to, so its list
// has no distance. Text is the theme's S18 (18 px at 1080p). The cue enters with HudEntrance18 (no pop-in); the string is rebuilt on change only.
// Shown only with sim.n22.directed live, in play (the break included).
// Wiring (the lead): Ui.BuildN22Hud: `BuildDirected22Cue();`  Ui.RenderN22Hud(frameDt): `RenderDirected22Cue(frameDt);`
using System;
using System.Text;
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        Directed22 dir22;
        RectTransform dir22Root;
        CanvasGroup dir22Group;
        TextMeshProUGUI dir22Head, dir22Body;
        readonly StringBuilder dir22Sb = new StringBuilder(160);
        double dir22Enter = -1;
        int dir22Key = -1;
        double dir22HuntMin, dir22HuntMax;

        /// <summary>For the reel's scenario (the Ui instance is private to SurvivalGame): the cue was built, what it shows, its colour.</summary>
        public static bool Directed22Built;
        public static Directed22Kind Directed22KindNow;
        public static int Directed22StepNow;
        public static string Directed22TextNow = "";
        public static Color Directed22ColorNow;

        void BuildDirected22Cue()
        {
            Directed22Built = true;
            GameConfig cfg = game.cfg;
            dir22 = new Directed22(cfg);
            dir22HuntMin = cfg.Has("design.adds.hunt.ringMin") ? cfg.Num("design.adds.hunt.ringMin") : 0;
            dir22HuntMax = cfg.Has("design.adds.hunt.ringMax") ? cfg.Num("design.adds.hunt.ringMax") : 0;
            dir22Root = Hud17Rect(values, "Directed cue", new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(UnityTuning.Directed22X, UnityTuning.Directed22Y), new Vector2(UnityTuning.Directed22W, UnityTuning.Directed22H));
            dir22Group = dir22Root.gameObject.AddComponent<CanvasGroup>();
            dir22Group.blocksRaycasts = dir22Group.interactable = false;
            dir22Group.alpha = 0;
            float px = UiTheme.Px(UiTheme.Size.S18);
            dir22Head = Hud17Text(dir22Root, "Head", "", px, UiTheme.CyanDeep, UiTheme.BodyBold, TextAlignmentOptions.Center, new Vector2(0.5f, 1), new Vector2(0.5f, 1), Vector2.zero, new Vector2(UnityTuning.Directed22W, UnityTuning.Directed22LineH), false);
            dir22Body = Hud17Text(dir22Root, "Steps", "", px, UiTheme.White, UiTheme.BodyBold, TextAlignmentOptions.Top, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -UnityTuning.Directed22LineH), new Vector2(UnityTuning.Directed22W, UnityTuning.Directed22LineH * Directed22.StepCount), false);
            dir22Body.richText = true;
            dir22Root.gameObject.SetActive(false);
            var bus = game.events;
            bus.On<BeaconEvent>(Beacon20.StandEvent, e => dir22.BeaconStood(e.round, e.position));
            bus.On<BeaconEvent>(Beacon20.TakeEvent, _ => dir22.BeaconTaken());
            bus.On<RoundEvent>("roundBegin", _ => dir22.RoundBegan());
            bus.On<RoundEvent>("roundClear", _ => dir22.RoundCleared());
            bus.On<SpecialRoundEvent>(N20Events.SpecialRound, e => dir22.Special(e.name, e.round, e.phase));
            game.OnNewRun += () =>
            {
                dir22.Reset();
                dir22Key = -1;
            };
        }

        void RenderDirected22Cue(double frameDt)
        {
            if (dir22Root == null) return;
            bool live = game.sim != null && game.sim.n22.directed && game.flow == Flow.Playing && dir22.Active;
            if (live) dir22.Update(game.Feet);
            live &= dir22.Active;
            Directed22KindNow = live ? dir22.Kind : Directed22Kind.None;
            Directed22StepNow = live ? dir22.Step : 0;
            if (dir22Root.gameObject.activeSelf != live)
            {
                dir22Root.gameObject.SetActive(live);
                if (live) dir22Enter = 0; // M9: it enters, never whole on its first frame
                dir22Key = -1;
            }
            if (!live)
            {
                Directed22TextNow = "";
                return;
            }
            // the string, on change only: kind, step, the distance in whole metres and the side
            double dist = dir22.Distance(game.Feet);
            Directed22Side side = dir22.HasTarget ? Directed22.SideOf(Directed22.BearingDeg(game.Feet, game.sim.input.Yaw, dir22.Target)) : Directed22Side.Ahead;
            int key = ((int)dir22.Kind * 8 + dir22.Step) * 4096 + (int)Math.Min(4000, dist) * 4 + (int)side;
            if (key != dir22Key)
            {
                dir22Key = key;
                BuildDirected22Text(dist, side);
            }
            Directed22ColorNow = dir22Head.color;
            if (dir22Enter >= 0)
            {
                dir22Enter = HudEntrance18.Step(dir22Enter, frameDt);
                bool done = HudEntrance18.Done(dir22Enter);
                float es = done || game.settings.View.reducedMotion ? 1f : HudEntrance18.Scale(dir22Enter);
                dir22Root.localScale = new Vector3(es, es, 1);
                dir22Group.alpha = done ? 1f : HudEntrance18.Alpha(dir22Enter);
                if (done) dir22Enter = -1;
            }
            else if (dir22Group.alpha != 1f) dir22Group.alpha = 1f;
        }

        void BuildDirected22Text(double dist, Directed22Side side)
        {
            bool beacon = dir22.Kind == Directed22Kind.Beacon;
            dir22Head.SetText(beacon ? UnityTuning.Directed22HeadBeacon : UnityTuning.Directed22HeadHunt);
            string[] steps = new string[Directed22.StepCount];
            if (beacon)
            {
                steps[0] = dir22.Step == 0 ? $"GO TO THE BEACON · {dist:0} m · {SideWord22(side)}" : "GO TO THE BEACON";
                steps[1] = PromptText.For("interact", true, "HOLD E") + " TO LIGHT IT";
                steps[2] = "SURVIVE THE ELITE ROUND";
            }
            else
            {
                steps[0] = "THE HUNT COMES";
                steps[1] = dir22HuntMax > 0 ? $"SQUADS CLOSE IN FROM {dir22HuntMin:0}-{dir22HuntMax:0} m" : "SQUADS CLOSE IN";
                steps[2] = "FINISH THE LAST ONE";
            }
            string cur = ColorHex22(UiTheme.Cyan), done = ColorHex22(UiTheme.Muted), next = ColorHex22(UiTheme.A(UiTheme.White, 0.6f));
            dir22Sb.Clear();
            for (int i = 0; i < steps.Length; i++)
            {
                if (i > 0) dir22Sb.Append('\n');
                string c = i < dir22.Step ? done : i == dir22.Step ? cur : next;
                dir22Sb.Append("<color=#").Append(c).Append('>').Append(i == dir22.Step ? "> " : "").Append(i + 1).Append("  ").Append(steps[i]).Append("</color>");
            }
            dir22Body.SetText(dir22Sb);
            Directed22TextNow = dir22Head.text + " | " + string.Join(" | ", steps) + " | current " + (dir22.Step + 1);
        }

        static string SideWord22(Directed22Side s) => s == Directed22Side.Ahead ? "AHEAD" : s == Directed22Side.Right ? "TURN RIGHT" : s == Directed22Side.Left ? "TURN LEFT" : "BEHIND YOU";

        static string ColorHex22(Color c) => ColorUtility.ToHtmlStringRGBA(c);
    }
}
