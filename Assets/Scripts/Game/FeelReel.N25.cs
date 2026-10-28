// NIGHT25 (the lead, phase H): the reel scenarios for NIGHT25's rows, on the Player, proof-first (NIGHT25.md): a scenario is written and
// seen RED (the row's switch off or its feature absent) before the feature is built. Each scenario is in its owner's partial file
// (FeelReel.N25.<Topic>.cs) as a `static partial void N25<Topic>Body(...)` hook: the lead's switch below calls the hook, and a hook with
// no body answers "NOT WRITTEN (RED)". Every result line starts with the scenario's PRECONDITIONS (FeelReel.Preconditions / SafeSpot).
//   meleefire   C   shots and punches in the same 3 s
//   shockwave   C   no shockwave on the shotgun
//   pulse       C   the PULSE builds by use, launches, small damage
//   wyrmvoid    W   no boot path shows the worm over a void
//   wyrmbar     W   the worm has a boss bar
//   wyrmdies    W   the worm dies: bossDown and the finisher
//   wyrmaim     W   the worm aims at the player: hits per minute
//   tutorial    L   the six tutorial stages
//   curriculum  L   levels 2-10
//   lesson      F   the lesson prompts
//   hitch25     F   the first PLAY hitch
using System;
using System.Collections;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        public static readonly string[] PhaseH = { "meleefire", "shockwave", "pulse", "wyrmvoid", "wyrmbar", "wyrmdies", "wyrmaim", "tutorial", "curriculum", "lesson", "hitch25", "campresults", "bosses", "levelwalk" };

        static bool IsPhaseH(string s) => Array.IndexOf(PhaseH, s) >= 0;

        /// <summary>The owners' scenario bodies. A hook that no builder has written leaves `res[0]` as it is (NOT WRITTEN).</summary>
        static partial void N25MeleefireBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N25ShockwaveBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N25PulseBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N25WyrmvoidBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N25WyrmbarBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N25WyrmdiesBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N25LevelwalkBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N25WyrmaimBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N25TutorialBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N25CurriculumBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N25LessonBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N25HitchBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);

        static IEnumerator N25Scenario(SurvivalGame g, string s, string dir, string[] res, List<Action> offs, double yaw, int pre)
        {
            res[0] = "NOT WRITTEN (RED): no scenario body for '" + s + "'";
            IEnumerator body = null;
            switch (s)
            {
                case "meleefire": N25MeleefireBody(g, dir, res, offs, pre, ref body); break;
                case "shockwave": N25ShockwaveBody(g, dir, res, offs, pre, ref body); break;
                case "pulse": N25PulseBody(g, dir, res, offs, pre, ref body); break;
                case "wyrmvoid": N25WyrmvoidBody(g, dir, res, offs, pre, ref body); break;
                case "wyrmbar": N25WyrmbarBody(g, dir, res, offs, pre, ref body); break;
                case "wyrmdies": N25WyrmdiesBody(g, dir, res, offs, pre, ref body); break;
                case "bosses": N25BossesBody(g, dir, res, offs, pre, ref body); break;
                case "levelwalk": body = N25LevelwalkScenario(g, dir, res, offs, pre); break;
                case "wyrmaim": N25WyrmaimBody(g, dir, res, offs, pre, ref body); break;
                case "tutorial": N25TutorialBody(g, dir, res, offs, pre, ref body); break;
                case "curriculum": N25CurriculumBody(g, dir, res, offs, pre, ref body); break;
                case "campresults": body = CampResults25Scenario(g, dir, res, offs, pre); break;
                case "lesson": N25LessonBody(g, dir, res, offs, pre, ref body); break;
                case "hitch25": N25HitchBody(g, dir, res, offs, pre, ref body); break;
            }
            if (body == null) yield break;
            yield return body;
        }
    }
}
