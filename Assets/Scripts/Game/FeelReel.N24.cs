// NIGHT24 (the lead, phase G): the reel scenarios for NIGHT24's rows, on the Player, proof-first (NIGHT24.md): a scenario is written and
// seen RED (the row's switch off or its feature absent) before the feature is built. Each scenario is in its owner's partial file
// (FeelReel.N24.<Topic>.cs) as a `static partial void N24<Topic>Body(...)` hook: the lead's switch below calls the hook, and a hook with
// no body answers "NOT WRITTEN (RED)". Every result line starts with the scenario's PRECONDITIONS (FeelReel.Preconditions / SafeSpot).
//   remove      R   FeelReel.N24.Remove.cs     TITLE has no DAILY / WEEKLY row, RESULTS no replay prompt, no replay view after DOWN
//   title       R   FeelReel.N24.Remove.cs     TITLE nav on keyboard and pad: PLAY starts a run (the NIGHT23 smoke's `pad only` cause)
//   padweapons  R   FeelReel.N24.PadY.cs       a virtual pad Y press switches weapons; RB is the special
//   shotgun     R   FeelReel.N24.Shotgun.cs    the shotgun's measured fire interval, before and after
//   bigpick     F   FeelReel.N24.BigPick.cs    round 5: the BIG PICK screen, one press, a reward
//   reticle     F   FeelReel.N24.Reticle.cs    the cooldown arc under the reticle: its size, alpha, fill
//   fist        F   FeelReel.N24.Fist.cs       kills charge the fist; idle time does not; a flick
//   room        L   FeelReel.N24.Room.cs       a closed room: the door locked, an encounter, the door opens, a checkpoint
//   limit       L   FeelReel.N24.Limit.cs      a LIMIT room: the weapon / movement limit holds and restores at the door
//   layer1      L   FeelReel.N24.Layer.cs      layer 1 door to door
//   lit         L   FeelReel.N24.Lit.cs        the room's accent colour and guide light
//   wyrm        B   FeelReel.N24.Wyrm.cs       the worm fight
//   hitch       --  FeelReel.N24.Hitch.cs      the screen-switch hitch tour (the lead)
using System;
using System.Collections;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        public static readonly string[] PhaseG = { "remove", "title", "padweapons", "shotgun", "bigpick", "reticle", "fist", "room", "limit", "layer1", "lit", "wyrm", "hitch", "cue", "monolith" };

        static bool IsPhaseG(string s) => Array.IndexOf(PhaseG, s) >= 0;

        /// <summary>The owners' scenario bodies. A hook that no builder has written leaves `res[0]` as it is (NOT WRITTEN).</summary>
        static partial void N24RemoveBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N24TitleBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N24PadWeaponsBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N24ShotgunBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N24BigPickBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N24ReticleBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N24FistBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N24RoomBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N24LimitBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N24LayerBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N24LitBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N24WyrmBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N24HitchBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);

        static IEnumerator N24Scenario(SurvivalGame g, string s, string dir, string[] res, List<Action> offs, double yaw, int pre)
        {
            res[0] = "NOT WRITTEN (RED): no scenario body for '" + s + "'";
            IEnumerator body = null;
            switch (s)
            {
                case "remove": N24RemoveBody(g, dir, res, offs, pre, ref body); break;
                case "title": N24TitleBody(g, dir, res, offs, pre, ref body); break;
                case "padweapons": N24PadWeaponsBody(g, dir, res, offs, pre, ref body); break;
                case "shotgun": N24ShotgunBody(g, dir, res, offs, pre, ref body); break;
                case "bigpick": N24BigPickBody(g, dir, res, offs, pre, ref body); break;
                case "reticle": N24ReticleBody(g, dir, res, offs, pre, ref body); break;
                case "fist": N24FistBody(g, dir, res, offs, pre, ref body); break;
                case "room": N24RoomBody(g, dir, res, offs, pre, ref body); break;
                case "limit": N24LimitBody(g, dir, res, offs, pre, ref body); break;
                case "layer1": N24LayerBody(g, dir, res, offs, pre, ref body); break;
                case "lit": N24LitBody(g, dir, res, offs, pre, ref body); break;
                case "wyrm": N24WyrmBody(g, dir, res, offs, pre, ref body); break;
                case "hitch": N24HitchBody(g, dir, res, offs, pre, ref body); break;
                case "cue": body = N24CueScenario(g, dir, res, offs, pre); break;
                case "monolith": body = MonolithScenario(g, dir, res, pre); break;
            }
            if (body == null) yield break;
            yield return body;
        }
    }
}
