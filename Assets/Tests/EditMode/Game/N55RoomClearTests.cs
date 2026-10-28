// NIGHT55 builder R (P2-clear; design.n55.clear55, Game/Clear55.cs): the room-clear beat's rules. Pinned:
//   1. the row is data: off by default (the lead flips it), numbers.holdS inside the slate's 0.4-0.6 s band, a _why on the row and on holdS;
//      the beat reads its hold from the config (never typed in code)
//   2. a combat room's clear holds the world (share 0) from the next frame for holdS of real time, then the share passes through untouched
//   3. one hold per event at most: a clear inside a running hold (or one still waiting for its first frame) neither extends nor restarts it;
//      a clear after it holds again
//   4. only combat rooms: a link's or a boss room's clear holds nothing; no hold while not allowed (not Playing, HIT FREEZE off), and a
//      running hold ends there
//   5. the card's words: the time to 0.1 s, the par whole when whole; UNDER / OVER PAR; no par shows the time only
//   6. the par is data: every combat room (kind room) of the five n52 campaign levels carries a par > 0 in its level file (campaign.rooms[].par)
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class N55RoomClearTests
    {
        const double Dt = 1.0 / 60;

        static GameConfig cfg;
        static GameConfig Cfg() => cfg ??= GameConfig.Load(Path.Combine(Root(), "PortExport", "config.json"));

        static string Root()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..")); }
            catch (System.Exception) { return Path.GetFullPath("."); }
        }

        static RoomEvent Clear(string kind = "room", double time = 31.24, double par = 50) => new RoomEvent { id = "g7", name = "2.7 THE MIX", kind = kind, time = time, par = par };

        /// <summary>Frames until the share passes through again; the held frames counted (share 0).</summary>
        static int HeldFrames(Clear55 c, bool allowed = true, int max = 600)
        {
            int held = 0;
            for (int i = 0; i < max; i++)
            {
                double s = c.Frame(Dt, 1, allowed);
                if (s == 0) held++;
                else if (held > 0 || i > 2) break;
            }
            return held;
        }

        [Test]
        public void TheRow_IsData_OffByDefault_HoldInTheBand()
        {
            GameConfig c = Cfg();
            Assert.That(c.Has("design.n55.clear55._why"), Is.True, "the row's _why");
            // NIGHT56 freeze turned clear55 ON (gate-r5a PASS): off until the lead flips it, then ON with its "ON ..." reason
            Assert.That(!c.Design("n55.clear55.enabled") || (c.Get("design.n55.clear55.enabled_why") as string ?? "").StartsWith("ON", System.StringComparison.Ordinal), Is.True, "clear55: off, or ON with its reason");
            Assert.That(c.Has(Clear55.Numbers + "_why") && c.Has(Clear55.Numbers + "holdS_why"), Is.True, "numbers._why and holdS_why");
            double h = c.Num(Clear55.Numbers + "holdS");
            Assert.That(h, Is.InRange(0.4, 0.6), "the slate's band (P2-clear doneWhen)");
            Assert.That(new Clear55(c).holdS, Is.EqualTo(h), "the beat reads its hold from the config");
        }

        [Test]
        public void ACombatRoomsClear_HoldsTheWorld_ForHoldS_FromTheNextFrame()
        {
            var c = new Clear55(0.5);
            Assert.That(c.Frame(Dt, 1, true), Is.EqualTo(1), "nothing asked: the share passes through");
            Assert.That(c.OnRoomClear(Clear()), Is.True, "a combat room's clear counts");
            Assert.That(c.Holding, Is.False, "the hold starts on the next frame's loop line, not inside the step");
            int held = HeldFrames(c);
            Assert.That(held, Is.EqualTo(30), "0.5 s at 60 fps: 30 frames held (the frame that crosses 0 is the last)");
            Assert.That(c.lastServed, Is.InRange(0.5 - 1e-9, 0.5 + Dt), "served on the loop's real-time clock");
            Assert.That(c.holds, Is.EqualTo(1));
            Assert.That(c.Frame(Dt, 0.3, true), Is.EqualTo(0.3), "after the hold another source's share (a slow-mo) passes through untouched");
            Assert.That(c.Frame(Dt, 0, true), Is.EqualTo(0), "a freeze's 0 passes through");
            Assert.That(c.clears, Is.EqualTo(1));
            Assert.That(c.lastRoom, Is.EqualTo("g7"));
            Assert.That(c.lastTime, Is.EqualTo(31.24));
            Assert.That(c.lastPar, Is.EqualTo(50));
            Assert.That(c.lastWithin, Is.True);
        }

        [Test]
        public void OneHoldPerEvent_NeverExtendedByASecondClearInsideIt()
        {
            var c = new Clear55(0.5);
            c.OnRoomClear(Clear());
            c.OnRoomClear(Clear()); // a second clear before the first frame: still one hold
            int held = 0;
            for (int i = 0; i < 600; i++)
            {
                if (i == 10) c.OnRoomClear(Clear()); // a clear inside the hold: not extended, not restarted
                double s = c.Frame(Dt, 1, true);
                if (s == 0) held++;
                else if (held > 0) break;
            }
            Assert.That(held, Is.EqualTo(30), "the hold's length is one holdS");
            Assert.That(c.holds, Is.EqualTo(1), "one hold for the three clears inside it");
            Assert.That(c.clears, Is.EqualTo(3), "every combat clear is counted");
            c.OnRoomClear(Clear());
            Assert.That(HeldFrames(c), Is.EqualTo(30), "a clear after the hold holds again");
            Assert.That(c.holds, Is.EqualTo(2));
        }

        [Test]
        public void OnlyCombatRooms_AndOnlyWhenAllowed()
        {
            var c = new Clear55(0.5);
            Assert.That(c.OnRoomClear(Clear("link")), Is.False, "a link has no fight");
            Assert.That(c.OnRoomClear(Clear("boss")), Is.False, "a boss room ends on the boss's own finish");
            Assert.That(c.OnRoomClear(null), Is.False);
            Assert.That(HeldFrames(c), Is.EqualTo(0), "nothing held");
            c.OnRoomClear(Clear());
            Assert.That(HeldFrames(c, allowed: false), Is.EqualTo(0), "HIT FREEZE off / not Playing: the hold asked is dropped");
            Assert.That(c.holds, Is.EqualTo(0));
            c.OnRoomClear(Clear());
            Assert.That(c.Frame(Dt, 1, true), Is.EqualTo(0), "the hold runs");
            Assert.That(c.Frame(Dt, 1, false), Is.EqualTo(1), "a running hold ends when it is no longer allowed (a pause, DOWN)");
            Assert.That(c.Holding, Is.False);
            c.OnRoomClear(Clear());
            c.Halt();
            Assert.That(HeldFrames(c), Is.EqualTo(0), "a new run (Halt): no hold waiting");
            Assert.That(new Clear55(0).Frame(Dt, 1, true), Is.EqualTo(1), "holdS 0: never a hold");
            var z = new Clear55(0);
            z.OnRoomClear(Clear());
            Assert.That(HeldFrames(z), Is.EqualTo(0));
        }

        [Test]
        public void TheCardsWords_TimeAgainstPar()
        {
            Assert.That(Clear55.Head(31.24, 50), Is.EqualTo("ROOM CLEAR · UNDER PAR"));
            Assert.That(Clear55.Line(31.24, 50), Is.EqualTo("31.2 S · PAR 50 S"));
            Assert.That(Clear55.Head(55.06, 50), Is.EqualTo("ROOM CLEAR · OVER PAR"));
            Assert.That(Clear55.Line(55.06, 50), Is.EqualTo("55.1 S · PAR 50 S"));
            Assert.That(Clear55.Head(50, 50), Is.EqualTo("ROOM CLEAR · UNDER PAR"), "on par is within par (Campaign24's withinPar)");
            Assert.That(Clear55.Line(12, 22.5), Is.EqualTo("12.0 S · PAR 22.5 S"), "a par that is not whole keeps its decimal");
            Assert.That(Clear55.Head(12, 0), Is.EqualTo("ROOM CLEAR"), "no par");
            Assert.That(Clear55.Line(12, 0), Is.EqualTo("12.0 S"), "no par: the time only");
            Assert.That(Clear55.Within(51, 50), Is.False);
            Assert.That(Clear55.Within(51, 0), Is.True);
        }

        [Test]
        public void ThePar_IsData_EveryCombatRoomOfTheCampaignHasOne()
        {
            int rooms = 0;
            foreach (string lv in new[] { "kiln", "foundry", "iceline", "spire", "span" })
            {
                string f = Path.Combine(Root(), "Design", "levels", "n52-" + lv + ".json");
                Assert.That(File.Exists(f), Is.True, f);
                var root = (Dictionary<string, object>)JsJson.Parse(File.ReadAllText(f));
                var campaign = (Dictionary<string, object>)root["campaign"];
                foreach (object o in (List<object>)campaign["rooms"])
                {
                    var r = (Dictionary<string, object>)o;
                    if (!(r.TryGetValue("kind", out object k) && k as string == "room")) continue;
                    rooms++;
                    Assert.That(r.TryGetValue("par", out object p) && PortData.Num(p) > 0, Is.True, lv + " " + r["id"] + ": a combat room's par (the card's source)");
                }
            }
            Assert.That(rooms, Is.GreaterThanOrEqualTo(20), "the five levels' combat rooms");
        }
    }
}
