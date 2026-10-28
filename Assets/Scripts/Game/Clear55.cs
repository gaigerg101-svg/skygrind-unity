// NIGHT55 builder R (P2-clear; design.n55.clear55, a view row; beyond.md #2, Neon White's level-end card): the room-clear beat's rules, pure C#
// (no UnityEngine: Tests/EditMode/Game/N55RoomClearTests.cs pins them).
//   THE HOLD   a combat room's clear (Campaign24's `roomClear`, RoomEvent.kind "room": not a link, not a boss room, whose fight has its own end)
//              holds the world for numbers.holdS of real time: the loop's freeze (SurvivalGame.N55R.cs hands ImpactBeginFrame a share of 0, the
//              same 0 a hitstop gives it: no sim step runs, WorldDt is 0 so the views stop, HeldThisFrame is set, the look stays live and a press
//              waits in the latches). One hold per clear at most: a clear inside a running hold (or one already waiting for its first frame)
//              neither extends nor restarts it. None while the run is not Playing or with HIT FREEZE off (the player's own switch for freezes).
//              Why not a Sim-tier hitstop (SurvivalSim.Hitstop -> ImpactRules): the impact budget (design.impact.budget.perSecond 0.2 s in any
//              rolling second) cuts a 0.5 s request to at most 0.2 s, less the last kill's own freeze in the same second; the hold is its own
//              event beside the kill's freeze (move54's 0.12 s kill cap untouched), combined on the loop's one time-share line.
//   THE CARD   the room's time (RoomEvent.time: the room's fight steps x dt) against its par (RoomEvent.par: the level file's campaign.rooms[].par,
//              from the plan's room row; never typed here). Two lines: "ROOM CLEAR · UNDER PAR" / "31.2 S · PAR 50 S"; a room with no par
//              (par <= 0) shows its time only. The strings are built once per clear (no allocation per frame).
// With the row off nothing is built (SurvivalGame.N55R.cs, UiClear55.cs): NIGHT54's game bit for bit.
using System;
using System.Globalization;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed class Clear55
    {
        public const string Row = "clear55";
        public const string Numbers = "design.n55.clear55.numbers.";

        /// <summary>The row (a view row: no tape bit; --off55 / --off54 turn it off).</summary>
        public static bool On(GameConfig cfg) => cfg != null && NightRows.ViewOn(cfg, Row);

        /// <summary>The real seconds the world holds at a combat room's clear (numbers.holdS).</summary>
        public readonly double holdS;

        double holdLeft;
        bool pending;

        /// <summary>For the reel (a455) and the tests: clears seen (combat rooms only), holds started, and the last hold's real seconds served.</summary>
        public int clears, holds;
        public double lastServed;
        /// <summary>The last combat room's clear: its id, time, par (the event's), and whether it was within par.</summary>
        public string lastRoom = "";
        public double lastTime, lastPar;
        public bool lastWithin;

        public Clear55(GameConfig cfg) : this(cfg != null && cfg.Has(Numbers + "holdS") ? cfg.Num(Numbers + "holdS") : 0) { }

        public Clear55(double holdS) { this.holdS = holdS > 0 ? holdS : 0; }

        /// <summary>A combat room's clear: kind "room" (links have no fight; a boss room ends on the boss's own finish).</summary>
        public static bool Counts(RoomEvent e) => e != null && e.kind == "room";

        /// <summary>Within par: no par (<= 0) counts as within, as Campaign24's withinPar.</summary>
        public static bool Within(double time, double par) => par <= 0 || time <= par;

        /// <summary>The bus's `roomClear` (inside a sim step): a combat room's clear asks for the hold at the next frame. True when it counted.</summary>
        public bool OnRoomClear(RoomEvent e)
        {
            if (!Counts(e)) return false;
            clears++;
            lastRoom = e.id ?? "";
            lastTime = e.time;
            lastPar = e.par;
            lastWithin = Within(e.time, e.par);
            if (!Holding && !pending) pending = true; // one hold per event at most: never extended or restarted by a clear inside it
            return true;
        }

        /// <summary>True while the world is held (the hold started and has time left).</summary>
        public bool Holding => holdLeft > Eps;

        /// <summary>s: what is left of a hold under this is none (30 frames of 1/60 s from 0.5 s leave ~1e-16, not a 31st frame).</summary>
        const double Eps = 1e-9;

        /// <summary>
        /// Once a frame, last on the loop's share line (SurvivalGame.ImpactBeginFrame): 0 while the hold runs, else `share` unchanged. `allowed`:
        /// the run is Playing and HIT FREEZE is not off; a hold asked while not allowed is dropped, one running ends.
        /// </summary>
        public double Frame(double frameDt, double share, bool allowed)
        {
            if (pending)
            {
                pending = false;
                if (allowed && holdS > 0)
                {
                    holdLeft = holdS;
                    lastServed = 0;
                    holds++;
                }
            }
            if (!Holding) return share;
            if (!allowed)
            {
                holdLeft = 0;
                return share;
            }
            double dt = frameDt > 0 ? frameDt : 0;
            holdLeft -= dt; // real time: the frame that crosses 0 is the last held
            lastServed += dt;
            return 0;
        }

        /// <summary>A new run (or the way to TITLE): no hold running or waiting; the counters stay (the reel reads them across its rooms).</summary>
        public void Halt()
        {
            holdLeft = 0;
            pending = false;
        }

        // ---- the card's words (UiClear55.cs shows them; built once per clear) ----

        static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        /// <summary>The time as the card prints it: one decimal, "31.2 S".</summary>
        public static string Seconds(double s) => (s < 0 ? 0 : s).ToString("0.0", Inv) + " S";

        /// <summary>The par as the card prints it: whole seconds when whole ("50 S"), else one decimal.</summary>
        public static string Par(double par) => (Math.Abs(par - Math.Round(par)) < 1e-9 ? Math.Round(par).ToString("0", Inv) : par.ToString("0.0", Inv)) + " S";

        /// <summary>The card's head line: "ROOM CLEAR · UNDER PAR" / "ROOM CLEAR · OVER PAR" ("ROOM CLEAR" with no par).</summary>
        public static string Head(double time, double par) => par <= 0 ? "ROOM CLEAR" : Within(time, par) ? "ROOM CLEAR · UNDER PAR" : "ROOM CLEAR · OVER PAR";

        /// <summary>The card's number line: "31.2 S · PAR 50 S" ("31.2 S" with no par).</summary>
        public static string Line(double time, double par) => par <= 0 ? Seconds(time) : Seconds(time) + " · PAR " + Par(par);
    }
}
