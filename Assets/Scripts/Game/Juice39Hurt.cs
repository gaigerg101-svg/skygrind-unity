// NIGHT39 builder V (juice38's damage static, the lead's n39-r1 frame f0900 in THE LANTERN's room: the full-screen static REFIRED on every 0.25 s
// floor tick, HP 999968 -> 999966 between f0870 and f0900, so standing on a live floor was a constant buzz; GG asked for "a static TV flicker when
// you take damage": a hit's burst). Which player hurt is a BURST (juice38's full static + shake, as NIGHT38) and which a TICK (a small cue only):
//   a TICK when
//     (a) its source is a damage-over-time source (UnityTuning.Juice38TickSources: the hazard floors and sweeps, acid, vents, the DEADLINE), or
//     (b) it lands within Juice38BurstCooldown of the last burst (the burst is still on screen), or
//     (c) the same source hit within Juice38RepeatWindow before it (a burn's cadence: THE WICK's hot wall and Lava30's grace re-touch, both 0.5 s);
//   else a BURST. A tick extends (c)'s chain, so a stand on a burn stays ticks however long it lasts.
// Pure and UnityEngine-free (the harness compiles it: `n39views hurt`; EditMode: Juice39HurtTests). View only: nothing in the sim reads it.
using System;

namespace Skygrind.Game
{
    public sealed class Juice39Hurt
    {
        double lastBurst = double.NegativeInfinity, lastHit = double.NegativeInfinity;
        string lastSource;

        /// <summary>Bursts and ticks so far (the reels and the tests read them).</summary>
        public int Bursts, Ticks;

        /// <summary>Is `source` a damage-over-time source (UnityTuning.Juice38TickSources)?</summary>
        public static bool TickSource(string source)
        {
            if (string.IsNullOrEmpty(source)) return false;
            foreach (string s in UnityTuning.Juice38TickSources) if (s == source) return true;
            return false;
        }

        /// <summary>A player hurt at `now` (s, real time) from `source`: true for a BURST, false for a TICK (the rules above). Updates the state.</summary>
        public bool Hit(double now, string source)
        {
            bool tick = TickSource(source)
                || now - lastBurst < UnityTuning.Juice38BurstCooldown
                || (source == lastSource && now - lastHit < UnityTuning.Juice38RepeatWindow);
            lastHit = now;
            lastSource = source;
            if (tick)
            {
                Ticks++;
                return false;
            }
            lastBurst = now;
            Bursts++;
            return true;
        }

        /// <summary>A respawn / a new run: nothing carries over.</summary>
        public void Clear()
        {
            lastBurst = lastHit = double.NegativeInfinity;
            lastSource = null;
        }
    }
}
