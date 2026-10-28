// NIGHT13 U3-26.5 (audit-perf section 4: UI glue that lived in SurvivalGame): the run shape's part of the HUD line
// (main.js topText): the objective (HOLD THE RING / GOLD RUSH), THE PROTOCOL's step and exfil's phase, and the key that
// changes when that text would (the line is rebuilt only then). Moved from SurvivalGame unchanged.
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        static long Mix(long key, long v) => key * 1000003 + v + 1;

        /// <summary>
        /// NIGHT11: the state of the HUD line's run-shape part (0: nothing to add). Changes when its text would: the HOLD's
        /// seconds or inside flag, GOLD RUSH, the PROTOCOL's lit count / next relay / surge, exfil's phase, seconds, kills.
        /// </summary>
        long RunShapeKey()
        {
            Objectives objectives = game.objectives;
            Protocol protocol = game.protocol;
            Exfil exfil = game.exfil;
            long key = 0;
            if (game.run.state == SurvivalRun.State.Fight && objectives.HudOn)
                key = Mix(Mix(Mix(key, objectives.HudType == Objectives.Hold ? 1 : 2), objectives.HudLeft), objectives.HudInside ? 1 : 0);
            if (protocol.HudOn) key = Mix(Mix(Mix(Mix(key, 7), protocol.HudLit), protocol.HudTotal), protocol.HudSurge ? 1 : 0) + (protocol.HudNext?.Length ?? 0);
            if (exfil.enabled)
            {
                bool radio = game.run.state == SurvivalRun.State.Break && exfil.CanCall();
                if (radio || exfil.Holding || exfil.phase == ExfilPhase.Called)
                    key = Mix(Mix(Mix(Mix(key, 11 + (int)exfil.phase + (radio ? 20 : 0)), (int)Math.Ceiling(exfil.SecondsLeft)), exfil.kills), exfil.needed);
            }
            return key;
        }

        /// <summary>NIGHT11 (main.js topText): the run shape's part of the HUD line (" · HOLD THE RING 23 S", " · PROTOCOL 2 / 4: ...").</summary>
        void AppendRunShape(System.Text.StringBuilder sb, bool fight)
        {
            Objectives objectives = game.objectives;
            Protocol protocol = game.protocol;
            Exfil exfil = game.exfil;
            if (fight && objectives.HudOn)
            {
                if (objectives.HudType == Objectives.Hold)
                {
                    sb.Append(" · HOLD THE RING ").Append(objectives.HudLeft).Append(" S");
                    if (!objectives.HudInside) sb.Append(" (GET IN)");
                }
                else sb.Append(" · GOLD RUSH");
            }
            // main.js topText: the PROTOCOL's step rides the line (in the break and the fight).
            if (protocol.HudOn)
            {
                sb.Append(" · PROTOCOL ").Append(protocol.HudLit).Append(" / ").Append(protocol.HudTotal);
                if (protocol.HudSurge) sb.Append(": SURGE");
                else if (protocol.HudNext != null) sb.Append(": ").Append(protocol.HudNext);
            }
            // design.exfil (port-only): the radio in its break, then the exfil's phase.
            if (exfil.enabled)
            {
                if (!fight && exfil.CanCall()) sb.Append(" · EXFIL: THE RADIO AT THE SPAWN");
                else if (exfil.phase == ExfilPhase.Called) sb.Append(" · EXFIL CALLED");
                else if (exfil.phase == ExfilPhase.Reach) sb.Append(" · EXFIL: GET TO THE BEACON ").Append((int)Math.Ceiling(exfil.SecondsLeft)).Append(" S");
                else if (exfil.phase == ExfilPhase.Horde) sb.Append(" · EXFIL: ").Append(exfil.kills).Append(" / ").Append(exfil.needed).Append(" KILLS · ").Append((int)Math.Ceiling(exfil.SecondsLeft)).Append(" S");
            }
        }
    }
}
