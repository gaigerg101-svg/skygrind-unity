// NIGHT40 builder S1 (row design.n40.kiln40, N40Blocks bit 1; GG 2026-10-13: level 3's boost "needs clearer text and better explanation", "I love the
// mechanic itself"; DECIDE40 D-2): the boost taught as a CHAIN on n40-kiln (ULTRAKILL's HudMessage, PRINCIPLES 2: each step shown when the last is done).
// The lines are design.n40.kiln40.chain ({id, text, done}); each is emitted as a "teach35" event (trigger "kiln40") so the teaching slot shows it with the
// bound buttons ({jump}, {fire}: UiTeach35 resolves them live); a line stands until its step is done (UiTeach35 ends a kiln40 line when the next one is
// queued) or lineFallbackSeconds pass (the line's own `seconds`; the chain then moves on: never stuck). The steps, read from the player each step:
//   jump       the feet leave the ground going up;
//   lookDown   the look pitch at or below -lookDownDeg;
//   boost      a THUNDERCLAP shot in the air with the pitch at or below -boostPitchDeg (the blast throws you up);
//   boostAgain a second such shot in the same air time;
//   airJump    a double jump in an air time that had a boost;
//   seen       nothing to do: the line stands its own seconds (the slug's line, W1's rule).
// The chain starts when the level's first room is entered (with start40 on, during the start hold: the empty room's first lesson). It only emits
// events: no world, player or RNG state changes, so the fingerprint is the same with the row on or off. Off (the row off, --off40): nothing runs.
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        const string Kiln40Root = "design.n40.kiln40.";

        sealed class Kiln40Line
        {
            public string id, text, textPad, done;
        }

        readonly List<Kiln40Line> k40Lines = new List<Kiln40Line>();
        readonly List<Teach35Event> k40Events = new List<Teach35Event>();
        bool k40Read, k40Hooked, k40Active, k40PrevGrounded = true, k40DoubledThisAir, k40Latched;
        int k40Index = -1, k40Steps, k40FallbackSteps, k40MinSteps, k40AirId, k40BoostAir = -1, k40BoostsThisAir, k40PrevDoubles, k40FiredShotgun;
        double k40LookDown, k40BoostPitch, k40Fallback, k40SeenSeconds, k40MinLine;
        string k40LevelTo;

        /// <summary>The harness's view: the chain's lines emitted this run (id @ seconds since the chain began) and the index of the line standing.</summary>
        public readonly List<string> kiln40Shown = new List<string>();
        public int Kiln40Index => k40Index;

        /// <summary>Called from N40StartStartRun (S1's run start).</summary>
        void Kiln40StartRun()
        {
            k40Active = false;
            k40Index = -1;
            k40Steps = k40AirId = k40BoostsThisAir = k40FiredShotgun = 0;
            k40BoostAir = -1;
            k40PrevGrounded = true;
            kiln40Shown.Clear();
            if (!n40.kiln40) return;
            if (!k40Read)
            {
                k40Read = true;
                k40LevelTo = cfg.Get(Kiln40Root + "levelTo") as string;
                k40LookDown = cfg.Num(Kiln40Root + "lookDownDeg") * System.Math.PI / 180;
                k40BoostPitch = cfg.Num(Kiln40Root + "boostPitchDeg") * System.Math.PI / 180;
                k40Fallback = cfg.Num(Kiln40Root + "lineFallbackSeconds");
                k40SeenSeconds = cfg.Num(Kiln40Root + "seenSeconds");
                k40MinLine = cfg.Num(Kiln40Root + "minLineSeconds");
                foreach (object o in cfg.Arr(Kiln40Root + "chain"))
                {
                    var d = PortData.Obj(o);
                    var l = new Kiln40Line { id = (string)d["id"], text = (string)d["text"], done = (string)d["done"] };
                    l.textPad = d.TryGetValue("textPad", out object tp) && tp is string tps ? tps : l.text;
                    k40Lines.Add(l);
                    k40Events.Add(new Teach35Event { id = "kiln40." + l.id, text = l.text, textPad = l.textPad, trigger = "kiln40", seconds = l.done == "seen" ? k40SeenSeconds : k40Fallback });
                }
            }
            k40FallbackSteps = MovementMath.StepsFor(k40Fallback, step);
            k40MinSteps = MovementMath.StepsFor(k40MinLine, step);
            k40DoubledThisAir = k40Latched = false;
            if (!k40Hooked)
            {
                k40Hooked = true; // only ever subscribed with the row on
                events.On<string>("fire", w => { if (k40Active && w == "shotgun") k40FiredShotgun++; });
            }
        }

        /// <summary>Called from N40StartStep every step.</summary>
        void Kiln40Step()
        {
            if (!n40.kiln40) return;
            if (!k40Active)
            {
                if (k40Index >= 0 || campaign == null || campaignLevel == null || campaign.room < 0 || N41BaseId(campaignLevel.id) != k40LevelTo || k40Lines.Count == 0) return; // NIGHT41: n41-kiln keeps the boost chain (N41BaseId)
                k40Active = true;
                k40PrevGrounded = mover.grounded;
                k40PrevDoubles = mover.doubleJumpsLeft;
                k40FiredShotgun = 0;
                Kiln40Show(0);
                return;
            }
            // the air time: a new id at every take-off; the boosts counted per air time
            bool grounded = mover.grounded;
            bool tookOff = k40PrevGrounded && !grounded;
            if (tookOff)
            {
                k40AirId++;
                k40BoostsThisAir = 0;
                k40DoubledThisAir = false;
            }
            bool jumped = tookOff && mover.velocity.y > 0;
            if (!grounded && mover.doubleJumpsLeft < k40PrevDoubles) k40DoubledThisAir = true; // a double jump in this air time (before or after a shot)
            double pitch = input.Pitch;
            bool boosted = false;
            for (; k40FiredShotgun > 0; k40FiredShotgun--)
                if (!grounded && pitch <= -k40BoostPitch)
                {
                    k40BoostsThisAir++;
                    k40BoostAir = k40AirId;
                    boosted = true;
                }
            k40PrevGrounded = grounded;
            k40PrevDoubles = mover.doubleJumpsLeft;
            k40Steps++;
            string need = k40Lines[k40Index].done;
            // a step done in the current air time counts even if it came first (the climb's double jump often comes between the two shots);
            // the line stands at least minLineSeconds so a fast climb still shows each step
            bool done = need == "jump" ? jumped
                : need == "lookDown" ? pitch <= -k40LookDown
                : need == "boost" ? boosted || (!grounded && k40BoostAir == k40AirId)
                : need == "boostAgain" ? !grounded && k40BoostsThisAir >= 2
                : need == "airJump" ? !grounded && k40DoubledThisAir && k40BoostAir == k40AirId
                : false;
            if (done) k40Latched = true;
            if ((k40Latched && k40Steps >= k40MinSteps) || k40Steps >= (need == "seen" ? MovementMath.StepsFor(k40SeenSeconds, step) : k40FallbackSteps))
            {
                if (k40Index + 1 < k40Lines.Count) Kiln40Show(k40Index + 1);
                else
                {
                    k40Active = false; // the chain is over for this run
                    k40Index = k40Lines.Count;
                }
            }
        }

        void Kiln40Show(int i)
        {
            k40Index = i;
            k40Steps = 0;
            k40Latched = false;
            kiln40Shown.Add(k40Lines[i].id + " @ " + SimTime.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture));
            events.Emit("teach35", k40Events[i]);
        }
    }
}
