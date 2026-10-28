// NIGHT43 builder F1 round 2 (design.n43.look43): the look kit's pure half (Game/Look43Math.cs) on a hand-made two-room level in LEVEL-FORMAT's
// shape (n42-foundry's a2 -> a3 -> a4 numbers): the exit door and the next one, the posts' axis, the ride panels (the tag, `noride`), the lava pools,
// the arena test, the dashes, the stone's drain. No engine objects; the view's Unity half is the reel's (feel43 with -skygrindLook43).
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class Look43Tests
    {
        static List<object> L(params object[] a) => new List<object>(a);

        static Dictionary<string, object> D(params object[] kv)
        {
            var d = new Dictionary<string, object>();
            for (int i = 0; i < kv.Length; i += 2) d[(string)kv[i]] = kv[i + 1];
            return d;
        }

        static Dictionary<string, object> Level() => D(
            "lava", L(D("rect", L(-8.0, -81.0, 6.0, -72.0), "y", -0.8, "room", "a3")),
            "campaign", D(
                "wallHeight", 14.0,
                "rooms", L(
                    D("id", "a2", "kind", "room", "rect", L(-7.0, -55.0, 7.0, -35.0), "height", 8.0, "exit", L(0.0, 0.0, -53.0)),
                    D("id", "a3", "kind", "room", "rect", L(-8.0, -85.0, 8.0, -59.0), "height", 9.0, "exit", L(0.0, 0.0, -83.0)),
                    D("id", "a4", "kind", "room", "rect", L(-4.0, -108.0, 4.0, -89.0), "exit", L(0.0, 0.0, -106.5))),
                "doors", L(
                    D("id", "d2", "between", L("a2", "a3"), "rect", L(-2.0, -59.0, 2.0, -55.0), "height", 4.5),
                    D("id", "d3", "between", L("a3", "a4"), "rect", L(-2.0, -89.0, 2.0, -85.0), "height", 4.5)),
                "boxes", L(
                    D("zone", "a3", "kind", "wall", "min", L(0.6, -0.8, -82.0), "max", L(1.2, 6.0, -71.0), "ride", true, "why", "ride panel P2 over the LAVA"),
                    D("zone", "a3", "kind", "wall", "min", L(-8.0, 0.0, -85.0), "max", L(-7.5, 9.0, -59.0), "why", "the room's west wall"),
                    D("zone", "a3", "kind", "wall", "min", L(5.0, 0.0, -70.0), "max", L(5.5, 6.0, -60.0), "ride", true, "noride", true, "why", "a tagged wall marked noride"),
                    D("zone", "a3", "kind", "deck", "min", L(-6.0, 0.0, -84.0), "max", L(-4.0, 1.5, -82.0), "ride", true, "why", "a deck, not a wall"))));

        [Test]
        public void TheLevel_ReadsRoomsDoorsLavaAndOnlyTheRidePanels()
        {
            LevelLook43 l = LevelLook43.From(Level());
            Assert.AreEqual(3, l.rooms.Count);
            Assert.AreEqual(2, l.doors.Count);
            Assert.AreEqual(1, l.lava.Count);
            Assert.AreEqual(-0.8, l.lava[0].y0, 1e-9);
            Assert.AreEqual(1, l.ride.Count, "V-3: the tagged wall only (not the room wall, not the noride wall, not the deck)");
            Assert.AreEqual(0.6, l.ride[0].x0, 1e-9);
            Assert.AreEqual(14, l.wallHeight, 1e-9);
            Assert.AreEqual(14, l.rooms[2].height, 1e-9, "a room with no height takes the level's wallHeight");
            Assert.AreEqual(0, LevelLook43.From(D("name", "survival")).rooms.Count, "a map with no campaign block: nothing to light");
        }

        [Test]
        public void TheExitDoor_ThenTheNextDoor_AndTheirPosts()
        {
            LevelLook43 l = LevelLook43.From(Level());
            Assert.AreEqual("d2", l.doors[l.ExitDoor("a2")].id, "a2 leaves through d2");
            Assert.AreEqual("d3", l.doors[l.NextDoor("a2")].id, "when a2 clears, the door after it (a3's exit) lights");
            Assert.AreEqual(-1, l.ExitDoor("a4"), "the last room has no exit door");
            Assert.AreEqual(-1, l.NextDoor("a3"));
            Assert.IsTrue(l.PassAlongZ(0), "a2 -> a3 runs south (z): the posts stand at the door's x edges");
        }

        [Test]
        public void ArenasGetALandmark_CorridorsDoNot()
        {
            LevelLook43 l = LevelLook43.From(Level());
            Assert.IsTrue(LevelLook43.IsArena(l.rooms[0]), "a2 is 14 x 20 m");
            Assert.IsFalse(LevelLook43.IsArena(l.rooms[2]), "a4 is 8 m wide: a corridor");
            Assert.IsFalse(LevelLook43.IsArena(new LevelLook43.Room { kind = "boss", x0 = 0, x1 = 30, z0 = 0, z1 = 30 }), "the boss room is B1's language");
            Assert.AreEqual(0, l.LavaIn(l.rooms[1], 0, -75), "a3's pool");
            Assert.AreEqual(-1, l.LavaIn(l.rooms[0], 0, -45), "a2 has none");
            LevelLook43.Room a3 = l.RoomAt(-1, -76.5);
            Assert.AreEqual("a3", a3.id, "the pool's room");
            Assert.IsTrue(LevelLook43.OnRoomWall(a3, -8, -81, -8, -72), "the pool's west edge is a3's west wall: the glow climbs it");
            Assert.IsFalse(LevelLook43.OnRoomWall(a3, 6, -81, 6, -72), "its east edge is open floor: the glow spills across the floor");
            Assert.IsFalse(LevelLook43.OnRoomWall(a3, -8, -81, 6, -81), "its south edge too");
        }

        [Test]
        public void TheDashes_KeepTheirRhythm_AndBothMargins()
        {
            var s = new List<double>();
            int n = LevelLook43.Dashes(11, 0.8, 1.2, s);
            Assert.AreEqual(6, n, "6 x 0.8 + 5 x 1.2 = 10.8 m fit in 11");
            Assert.AreEqual(s[0], 11 - (s[n - 1] + 0.8), 1e-9, "the same margin at both ends");
            Assert.AreEqual(2.0, s[1] - s[0], 1e-9, "a beat every dash + gap");
            Assert.AreEqual(0, LevelLook43.Dashes(0.5, 0.8, 1.2, s), "too short for a dash");
        }

        [Test]
        public void TheStone_IsDrainedTowardGrey_ItsHueKept()
        {
            var violet = new Color(0.30f, 0.22f, 0.45f, 1);
            Color d = LevelLook43.Drain(violet, UnityTuning.Look43StoneDesat, UnityTuning.Look43StoneValue);
            Color.RGBToHSV(violet, out float h0, out float s0, out float v0);
            Color.RGBToHSV(d, out float h1, out float s1, out float v1);
            Assert.Less(s1, s0, "less saturated");
            Assert.Less(v1, v0, "a touch darker");
            Assert.AreEqual(h0, h1, 0.01f, "the same hue (the dusk stays violet)");
            Assert.AreEqual(1, d.a);
            Assert.AreEqual(1f, LevelLook43.Pulse(3, 0.35f, 0.25f, true), "REDUCED MOTION: no pulse");
        }
    }
}
