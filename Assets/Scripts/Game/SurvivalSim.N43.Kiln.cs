// NIGHT43 builder K1 (row design.n43.kiln43, N43Blocks bit 9; GG 2026-10-16 G26: "The tutorial should introduce the mechanic using smaller,
// easier jumps first and progressively increase the required height"): n43-kiln's boost chain. NIGHT40 S1's chain (SurvivalSim.N40.Kiln.cs,
// design.n40.kiln40.chain) taught the boost as five verbs in a row wherever the player stood; n43-kiln's first room is THE STEPS (three boost
// steps, small then big: Tools/night/gen-n43-kiln.cjs), so this chain's lines belong to the steps: each "top" line stands until the feet stand on
// its step's top (the level file's own heights: the room row's `tops43`), so the words and the geometry never drift apart.
// With kiln43 on (the level is n43-kiln: Curriculum25.N43Levels) and kiln40 on, kiln40's chain stands down for the run (its index is set past
// its last line at the run's start) and this one runs on n43-kiln. Done kinds (design.n43.kiln43.chain {id, text, done, room?, top?}):
//   jump      the feet leave the ground going up;
//   lookDown  the look pitch at or below -lookDownDeg;
//   top       grounded in room `room` with the feet at or above that room's tops43[`top`] - topSlackM (a step climbed), or that room cleared
//             or left behind (a climb that overshoots a step does not leave its line standing);
//   seen      nothing to do: the line stands seenSeconds.
// A line with `at` (a room id) is not shown until the player is in that room (or past it): a room's own line on entry (3.2c's burrower).
// A line stands at least minLineSeconds; a line not done in lineFallbackSeconds moves on (never stuck). Each line is a "teach35" event with the
// trigger "kiln40" (UiTeach35 ends a chain line when the next is queued: the same slot as kiln40's). It only emits events: no world, player or RNG
// state changes. Off (the row off, --off43 and every older switch): nothing runs, kiln40's chain is untouched.
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        const string Kiln43Root = "design.n43.kiln43.";

        sealed class Kiln43Line
        {
            public string id, text, textPad, done, room, top, at;
        }

        int[] k43At; // per line: the room index it waits for before it shows (-1: none)
        bool k43Waiting; // the standing line waits for its `at` room (not shown yet)

        readonly List<Kiln43Line> k43Lines = new List<Kiln43Line>();
        readonly List<Teach35Event> k43Events = new List<Teach35Event>();
        double[] k43Top; // per line: the feet height its "top" asks (resolved from the level file when the chain starts); NaN otherwise
        int[] k43Room; // per line: the room index its "top" asks (-1: any)
        bool k43Read, k43On, k43Active, k43PrevGrounded = true, k43Latched;
        int k43Index = -1, k43Steps, k43FallbackSteps, k43MinSteps, k43SeenSteps;
        double k43LookDown, k43Slack;
        string k43LevelTo;

        /// <summary>The harness's view: the lines shown this run (id @ seconds) and the index of the line standing.</summary>
        public readonly List<string> kiln43Shown = new List<string>();
        public int Kiln43Index => k43Index;

        partial void N43KilnStartRun()
        {
            k43On = k43Active = k43Latched = k43Waiting = false;
            k43Index = -1;
            k43Steps = 0;
            k43PrevGrounded = true;
            kiln43Shown.Clear();
            if (!n43.kiln43 || !n40.kiln40) return; // no kiln40 chain to replace: nothing (the level still swaps: it is data)
            if (!k43Read)
            {
                k43Read = true;
                k43LevelTo = cfg.Get(Kiln43Root + "levelTo") as string;
                k43LookDown = cfg.Num(Kiln43Root + "lookDownDeg") * System.Math.PI / 180;
                k43Slack = cfg.Num(Kiln43Root + "topSlackM");
                foreach (object o in cfg.Arr(Kiln43Root + "chain"))
                {
                    var d = PortData.Obj(o);
                    var l = new Kiln43Line { id = (string)d["id"], text = (string)d["text"], done = (string)d["done"] };
                    l.textPad = d.TryGetValue("textPad", out object tp) && tp is string tps ? tps : l.text;
                    l.room = d.TryGetValue("room", out object ro) ? ro as string : null;
                    l.top = d.TryGetValue("top", out object to) ? to as string : null;
                    l.at = d.TryGetValue("at", out object at) ? at as string : null;
                    k43Lines.Add(l);
                    k43Events.Add(new Teach35Event { id = "kiln43." + l.id, text = l.text, textPad = l.textPad, trigger = "kiln40", seconds = l.done == "seen" ? cfg.Num(Kiln43Root + "seenSeconds") : cfg.Num(Kiln43Root + "lineFallbackSeconds") });
                }
                k43Top = new double[k43Lines.Count];
                k43Room = new int[k43Lines.Count];
                k43At = new int[k43Lines.Count];
            }
            k43FallbackSteps = MovementMath.StepsFor(cfg.Num(Kiln43Root + "lineFallbackSeconds"), step);
            k43MinSteps = MovementMath.StepsFor(cfg.Num(Kiln43Root + "minLineSeconds"), step);
            k43SeenSteps = MovementMath.StepsFor(cfg.Num(Kiln43Root + "seenSeconds"), step);
            // kiln40's chain stands down for this run (whenever kiln43 is on, level 3 is n43-kiln, the only level kiln40's chain ran on)
            k40Index = k40Lines.Count;
            k40Active = false;
            k43On = k43Lines.Count > 0;
        }

        partial void N43KilnStep(double dt)
        {
            if (!k43On) return;
            if (!k43Active)
            {
                if (k43Index >= 0 || campaign == null || campaignLevel == null || campaign.room < 0 || N45Levels.BaseId(cfg, campaignLevel.id) != k43LevelTo) return; // NIGHT45: n45-kiln is n43-kiln here
                k43Active = true;
                k43PrevGrounded = mover.grounded;
                Kiln43Resolve();
                Kiln43Show(0);
                return;
            }
            bool grounded = mover.grounded;
            bool jumped = k43PrevGrounded && !grounded && mover.velocity.y > 0;
            k43PrevGrounded = grounded;
            if (k43Waiting)
            {
                if (campaign.room < k43At[k43Index]) return; // the line waits for its room (an `at` line: shown on entry)
                k43Waiting = false;
                Kiln43Emit(k43Index);
                return;
            }
            k43Steps++;
            Kiln43Line line = k43Lines[k43Index];
            bool done = line.done == "jump" ? jumped
                : line.done == "lookDown" ? input.Pitch <= -k43LookDown
                : line.done == "top" ? Kiln43TopDone(k43Index, grounded)
                : false;
            if (done) k43Latched = true;
            if ((k43Latched && k43Steps >= k43MinSteps) || k43Steps >= (line.done == "seen" ? k43SeenSteps : k43FallbackSteps))
            {
                if (k43Index + 1 < k43Lines.Count) Kiln43Show(k43Index + 1);
                else
                {
                    k43Active = false;
                    k43Index = k43Lines.Count;
                }
            }
        }

        /// <summary>A "top" line is done when the feet stand on its step (grounded in its room at or above the step's top - slack), or when the
        /// player is past it (its room cleared or a later room: a climb that overshoots the step never leaves its line standing).</summary>
        bool Kiln43TopDone(int i, bool grounded)
        {
            int ri = k43Room[i];
            if (ri < 0 || double.IsNaN(k43Top[i])) return false;
            if (campaign.room > ri || (campaign.room == ri && campaign.phase == Campaign24.Phase.Cleared)) return true;
            return grounded && campaign.room == ri && mover.position.y >= k43Top[i];
        }

        /// <summary>Each "top" line's feet height and room from the level file (the room rows' tops43, written by gen-n43-kiln.cjs). A line whose
        /// room or key is missing gets NaN: it is never done and moves on by its fallback.</summary>
        void Kiln43Resolve()
        {
            for (int i = 0; i < k43Lines.Count; i++)
            {
                k43Top[i] = double.NaN;
                k43Room[i] = -1;
                Kiln43Line l = k43Lines[i];
                k43At[i] = l.at == null ? -1 : campaignLevel.rooms.FindIndex(q => q.id == l.at);
                if (l.done != "top" || l.room == null || l.top == null) continue;
                int ri = campaignLevel.rooms.FindIndex(q => q.id == l.room);
                if (ri < 0 || map.json == null || !map.json.TryGetValue("campaign", out object co) || !(co is Dictionary<string, object> camp)
                    || !camp.TryGetValue("rooms", out object ro) || !(ro is List<object> rows)) continue;
                foreach (object o in rows)
                    if (o is Dictionary<string, object> row && row.TryGetValue("id", out object id) && id as string == l.room
                        && row.TryGetValue("tops43", out object t) && t is Dictionary<string, object> tops && tops.TryGetValue(l.top, out object v))
                    {
                        k43Top[i] = PortData.Num(v) - k43Slack;
                        k43Room[i] = ri;
                    }
            }
        }

        void Kiln43Show(int i)
        {
            k43Index = i;
            k43Steps = 0;
            k43Latched = false;
            k43Waiting = k43At[i] >= 0 && campaign.room < k43At[i];
            if (!k43Waiting) Kiln43Emit(i);
        }

        void Kiln43Emit(int i)
        {
            kiln43Shown.Add(k43Lines[i].id + " @ " + SimTime.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture));
            events.Emit("teach35", k43Events[i]);
        }
    }
}
