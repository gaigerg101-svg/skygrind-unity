// NIGHT63 builder G (leftover 6, S7: kiln's presses read as machines; design.n63.press63; Game/Press63Math.cs, Game/Press63.cs). Pinned:
//   1. the row: design.n63.press63 is a VIEW row (a _why, a bool enabled, an enabled_why, a numbers object, no sim key: no tape bit), the
//      checker finds no problem, -skygrindOff63 turns it off; the switch is off with no config
//   2. which boxes: a campaign box whose job starts "hero prop: a drop press" (n57-kiln's four, Design/levels/n57/kiln.plan.json), nothing else
//   S7b (the fix round after S7's GATE FAIL, Logs/n63/verdict-S7.md: the blind SPOT saw "tall, multi-part columns ... with hazard stripes"):
//   3. a press's silhouette, not a column's: a heavy bed, a GAP over it under the raised ram head, the head between the bed and the crown,
//      the crown the widest part; the gap at the player's eye (body.eyeHeight), so the stamp is in view at THE PRESS HALL's entry
//   4. the old pillar hidden: every side face of the press's box is covered floor to roof by a press part (its rose and blue bands never show),
//      and every part, moving or not, stays within body.capsuleRadius of the box (the eye never enters a part: the box is the collider)
//   5. the piston bright metal against the dark: a rod down each face from the head's top to the crown, in front of the dark throat; the
//      value contrast (sRGB luma) rod / guide columns / head / die over the throat, the bed's lit top line
//   6. a lamp big enough to read: a lens at least 0.6 x 0.4 m on the crown's face toward the hall's middle, lit bright, dark glass when off
//   7. the head's clock: within [Low, High], periodic, the drop faster than the lift, held up top (the lamp lit), dwelling on the bed
//   8. the colours: no warm hue at all (S7's bone stripe rendered hue 36, at the danger arc's edge), never the danger or the parry yellow (the
//      boss63 cue meter's classes), never the player's cyan or the RIVAL's magenta; and no stripes (no hazard sign on a thing that does not hurt)
// Written test-first: RED on its own commit (the S7 geometry fails 3-8). Not run in Unity by its author.
using System.Collections.Generic;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    public class N63PressTests
    {
        static GameConfig Cfg()
        {
            string root;
            try { root = System.IO.Path.GetFullPath(System.IO.Path.Combine(UnityEngine.Application.dataPath, "..", "PortExport")); }
            catch (System.Exception) { root = System.IO.Path.GetFullPath("PortExport"); }
            return GameConfig.Load(System.IO.Path.Combine(root, "config.json"));
        }

        // n57-kiln's first press (THE FIRST, east, 1.5 x 2.85 m) and its third (west, 1.5 x 2 m), from Design/levels/n57/kiln.plan.json
        static readonly Vec3 Min = new Vec3(4, 0, -202), Max = new Vec3(5.5, 14, -199.15);
        static readonly Vec3 Min3 = new Vec3(-5.5, 0, -207.5), Max3 = new Vec3(-4, 14, -205.5);

        static bool Within(Vec3 pMin, Vec3 pMax, Vec3 bMin, Vec3 bMax, double grow) =>
            pMin.x >= bMin.x - grow - 1e-9 && pMax.x <= bMax.x + grow + 1e-9 && pMin.z >= bMin.z - grow - 1e-9 && pMax.z <= bMax.z + grow + 1e-9
            && pMin.y >= bMin.y - 1e-9 && pMax.y <= bMax.y + 1e-9 && pMin.x <= pMax.x && pMin.y <= pMax.y && pMin.z <= pMax.z;

        static bool Inside(Vec3 p, Press63Math.Piece pc) =>
            p.x >= pc.min.x - 1e-9 && p.x <= pc.max.x + 1e-9 && p.y >= pc.min.y - 1e-9 && p.y <= pc.max.y + 1e-9 && p.z >= pc.min.z - 1e-9 && p.z <= pc.max.z + 1e-9;

        static double Out(Press63Math.Piece pc, Vec3 bMin, Vec3 bMax) =>
            System.Math.Max(System.Math.Max(bMin.x - pc.min.x, pc.max.x - bMax.x), System.Math.Max(bMin.z - pc.min.z, pc.max.z - bMax.z));

        static double Luma(uint hex) => 0.2126 * ((hex >> 16) & 255) / 255.0 + 0.7152 * ((hex >> 8) & 255) / 255.0 + 0.0722 * (hex & 255) / 255.0;

        static Press63Math.Piece One(List<Press63Math.Piece> ps, Press63Math.Part part)
        {
            foreach (Press63Math.Piece p in ps) if (p.part == part) return p;
            Assert.Fail("no " + part);
            return default;
        }

        [Test]
        public void Row_AViewRow_TheCheckerClean_OffUnderTheNightsSwitch()
        {
            GameConfig c = Cfg();
            const string at = "design.n63.press63";
            Assert.That(c.Has(at + "._why") && c.Get(at + "._why") is string w && w.Length > 0, Is.True, at + "._why");
            Assert.That(c.Has(at + ".enabled") && c.Get(at + ".enabled") is bool, Is.True, at + ".enabled is a bool");
            Assert.That(c.Has(at + ".enabled_why") && c.Get(at + ".enabled_why") is string ew && ew.Length > 0, Is.True, at + ".enabled_why");
            Assert.That(c.Get(at + ".numbers") is Dictionary<string, object>, Is.True, at + ".numbers is an object");
            Assert.That(c.Has(at + ".sim"), Is.False, "a view row: no sim, no tape bit");
            Assert.That(NightRows.From(c).IndexOf("press63"), Is.EqualTo(-1), "no tape bit");
            Assert.That(NightRows.Problems(c.Obj("design")).Count, Is.EqualTo(0), "the checker");
            int saved = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = 63;
                Assert.That(NightRows.ViewOn(c, "press63"), Is.False, "off under -skygrindOff63");
            }
            finally { NightRows.ForceOffFrom = saved; }
            Assert.IsNotNull(Press63Math.OffReason(null, new string[0]), "no config: off");
            Assert.AreEqual("-skygrindNoPress63", Press63Math.NoSwitch);
        }

        [Test]
        public void WhichBoxes_TheLevelsOwnDropPresses()
        {
            Assert.IsTrue(Press63Math.IsPress("hero prop: a drop press (the charger's stun post)"));
            Assert.IsTrue(Press63Math.IsPress("hero prop: a drop press (the charger's stun post); the foothold (R5)"));
            Assert.IsFalse(Press63Math.IsPress("spatial law: THE PRESS GANTRY (the mid tier along the west wall)"));
            Assert.IsFalse(Press63Math.IsPress("verb it celebrates: the forge wall's ride over the charge"));
            Assert.IsFalse(Press63Math.IsPress(null));
            Assert.IsFalse(Press63Math.IsPress(""));
        }

        [Test]
        public void ThePress_ASilhouette_BedGapHeadCrown_TheGapAtTheEye()
        {
            List<Press63Math.Piece> ps = Press63Math.Pieces(Min, Max, true);
            var count = new Dictionary<Press63Math.Part, int>();
            foreach (Press63Math.Piece p in ps) count[p.part] = (count.TryGetValue(p.part, out int k) ? k : 0) + 1;
            Assert.AreEqual(1, count.TryGetValue(Press63Math.Part.Bed, out int n1) ? n1 : 0, "a bed");
            Assert.AreEqual(1, count.TryGetValue(Press63Math.Part.Throat, out int n2) ? n2 : 0, "the throat (the dark gap's back)");
            Assert.AreEqual(4, count.TryGetValue(Press63Math.Part.Upright, out int n3) ? n3 : 0, "four guide columns");
            Assert.AreEqual(1, count.TryGetValue(Press63Math.Part.Crown, out int n4) ? n4 : 0, "a crown");
            Assert.AreEqual(1, count.TryGetValue(Press63Math.Part.Stack, out int n5) ? n5 : 0, "the cylinder stack to the roof");
            Assert.AreEqual(1, count.TryGetValue(Press63Math.Part.Housing, out int n6) ? n6 : 0, "the lamp's housing");
            Assert.AreEqual(1, count.TryGetValue(Press63Math.Part.Lens, out int n7) ? n7 : 0, "the lamp's lens");

            Press63Math.Piece bed = One(ps, Press63Math.Part.Bed), crown = One(ps, Press63Math.Part.Crown);
            double bedTop = Press63Math.BedTop(Min), crownBottom = Press63Math.CrownBottom(Min, Max);
            Assert.That(bed.max.y, Is.EqualTo(bedTop).Within(1e-9), "the bed's top");
            Assert.That(crown.min.y, Is.EqualTo(crownBottom).Within(1e-9), "the crown's bottom");
            double low = Press63Math.Low(Min), high = Press63Math.High(Min, Max);
            Press63Math.Piece atLow = Press63Math.Head(Min, Max, low), atHigh = Press63Math.Head(Min, Max, high);
            Assert.That(atLow.min.y, Is.GreaterThanOrEqualTo(bedTop - 1e-9), "the head stamps onto the bed, never into it");
            Assert.That(atHigh.max.y, Is.LessThanOrEqualTo(crownBottom - 1e-9), "the head never enters the crown");
            Assert.That(high - bedTop, Is.GreaterThanOrEqualTo(1.8), "a visible gap under the raised head");
            double eye = Cfg().Num("body.eyeHeight");
            Assert.That(System.Math.Abs((bedTop + high) / 2 - (Min.y + eye)), Is.LessThanOrEqualTo(1.0), "the gap at the player's eye (" + eye + " m)");
            double box = Max.x - Min.x;
            Assert.That(bed.max.x - bed.min.x, Is.GreaterThanOrEqualTo(box + 0.4), "the bed wider than the column");
            Assert.That(atLow.max.x - atLow.min.x, Is.GreaterThanOrEqualTo(box + 0.4), "the head wider than the column");
            Assert.That(crown.max.x - crown.min.x, Is.GreaterThanOrEqualTo(atLow.max.x - atLow.min.x), "the crown the widest");
            Assert.That(crown.max.y - crown.min.y, Is.GreaterThanOrEqualTo(1.0), "a heavy crown");
        }

        [Test]
        public void TheOldPillar_HiddenFloorToRoof_EveryPartWithinTheCapsule()
        {
            double capsule = Cfg().Num("body.capsuleRadius");
            foreach ((Vec3 lo, Vec3 hi) in new[] { (Min, Max), (Min3, Max3) })
            foreach (bool lampOnMinX in new[] { false, true })
            {
                List<Press63Math.Piece> ps = Press63Math.Pieces(lo, hi, lampOnMinX);
                // every side face, sampled 2 cm outside it: some static part covers it from the floor to the roof
                const double off = 0.02, step = 0.1;
                for (double y = lo.y + 0.001; y < hi.y; y += step)
                {
                    for (double z = lo.z + 0.001; z < hi.z; z += step)
                        foreach (Vec3 p in new[] { new Vec3(lo.x - off, y, z), new Vec3(hi.x + off, y, z) })
                            Assert.IsTrue(ps.Exists(pc => Inside(p, pc)), "the box's x face uncovered at " + p);
                    for (double x = lo.x + 0.001; x < hi.x; x += step)
                        foreach (Vec3 p in new[] { new Vec3(x, y, lo.z - off), new Vec3(x, y, hi.z + off) })
                            Assert.IsTrue(ps.Exists(pc => Inside(p, pc)), "the box's z face uncovered at " + p);
                }
                var all = new List<Press63Math.Piece>(ps);
                double low = Press63Math.Low(lo), high = Press63Math.High(lo, hi), crownBottom = Press63Math.CrownBottom(lo, hi);
                foreach (double b in new[] { low, high })
                {
                    all.Add(Press63Math.Head(lo, hi, b));
                    all.Add(Press63Math.Die(lo, hi, b));
                    all.AddRange(Press63Math.Rods(lo, hi, b + UnityTuning.Press63HeadH, crownBottom));
                }
                foreach (Press63Math.Piece pc in all)
                {
                    Assert.IsTrue(Within(pc.min, pc.max, lo, hi, capsule), pc.part + " within the box and its height: " + pc.min + " .. " + pc.max);
                    Assert.That(Out(pc, lo, hi), Is.LessThan(capsule), pc.part + " stands out less than body.capsuleRadius (" + capsule + " m): the eye never enters it");
                }
            }
        }

        [Test]
        public void ThePiston_BrightMetalAgainstTheDarkThroat()
        {
            List<Press63Math.Piece> ps = Press63Math.Pieces(Min, Max, true);
            Press63Math.Piece throat = One(ps, Press63Math.Part.Throat), crown = One(ps, Press63Math.Part.Crown), stack = One(ps, Press63Math.Part.Stack);
            double headTop = Press63Math.Low(Min) + UnityTuning.Press63HeadH, crownBottom = Press63Math.CrownBottom(Min, Max);
            List<Press63Math.Piece> rods = Press63Math.Rods(Min, Max, headTop, crownBottom);
            Assert.AreEqual(4, rods.Count, "a rod down each face");
            double cx = (Min.x + Max.x) / 2, cz = (Min.z + Max.z) / 2;
            foreach (Press63Math.Piece r in rods)
            {
                Assert.AreEqual(Press63Math.Part.Rod, r.part);
                Assert.That(r.min.y, Is.EqualTo(headTop).Within(1e-9), "from the head's top");
                Assert.That(r.max.y, Is.EqualTo(crownBottom).Within(1e-9), "to the crown");
                Assert.IsTrue(r.min.x <= cx && r.max.x >= cx || r.min.z <= cz && r.max.z >= cz, "centred on its face");
                double o = Out(r, Min, Max);
                Assert.That(o, Is.GreaterThan(Out(throat, Min, Max)), "in front of the dark throat");
                Assert.That(o, Is.LessThanOrEqualTo(Out(stack, Min, Max)), "behind the stack and the crown (a rod built long stays hidden there)");
                Assert.That(o, Is.LessThanOrEqualTo(Out(crown, Min, Max)));
            }
            uint dark = Press63Math.ToneOf(Press63Math.Part.Throat).front;
            Assert.That(Luma(dark), Is.LessThanOrEqualTo(0.08), "the throat near-black: the gap reads as a gap");
            Assert.That(Luma(Press63Math.ToneOf(Press63Math.Part.Rod).front) - Luma(dark), Is.GreaterThanOrEqualTo(0.6), "the rod: bright metal");
            Assert.That(Luma(Press63Math.ToneOf(Press63Math.Part.Upright).front) - Luma(dark), Is.GreaterThanOrEqualTo(0.5), "the guide columns: bright metal");
            Assert.That(Luma(Press63Math.ToneOf(Press63Math.Part.Head).front) - Luma(dark), Is.GreaterThanOrEqualTo(0.3), "the head reads against the throat");
            Assert.That(Luma(Press63Math.ToneOf(Press63Math.Part.Die).front) - Luma(Press63Math.ToneOf(Press63Math.Part.Head).front), Is.GreaterThanOrEqualTo(0.15), "the die: the head's bright working edge");
            Press63Math.Tone bed = Press63Math.ToneOf(Press63Math.Part.Bed);
            Assert.That(Luma(bed.top) - Luma(bed.front), Is.GreaterThanOrEqualTo(0.15), "the bed's lit top line");
            Assert.That(Luma(Press63Math.ToneOf(Press63Math.Part.Stack).front), Is.LessThanOrEqualTo(0.12), "the stack above the crown dark: the machine ends at the crown");
        }

        [Test]
        public void TheLamp_BigEnoughToRead_OnTheCrownTowardTheHall()
        {
            foreach (bool lampOnMinX in new[] { true, false })
            {
                List<Press63Math.Piece> ps = Press63Math.Pieces(Min, Max, lampOnMinX);
                Press63Math.Piece lens = One(ps, Press63Math.Part.Lens), housing = One(ps, Press63Math.Part.Housing), crown = One(ps, Press63Math.Part.Crown);
                Assert.That(lens.max.z - lens.min.z, Is.GreaterThanOrEqualTo(0.6), "the lens at least 0.6 m wide");
                Assert.That(lens.max.y - lens.min.y, Is.GreaterThanOrEqualTo(0.4), "the lens at least 0.4 m tall");
                Assert.That(lens.min.y, Is.GreaterThanOrEqualTo(crown.min.y - 1e-9), "on the crown");
                Assert.That(lens.max.y, Is.LessThanOrEqualTo(crown.max.y + 1e-9), "on the crown");
                if (lampOnMinX) Assert.That(lens.max.x, Is.LessThanOrEqualTo(crown.min.x + 1e-9), "on the crown's -x face, proud of it");
                else Assert.That(lens.min.x, Is.GreaterThanOrEqualTo(crown.max.x - 1e-9), "on the crown's +x face, proud of it");
                Assert.That(housing.max.z - housing.min.z, Is.GreaterThan(lens.max.z - lens.min.z), "a housing round the lens");
            }
            Assert.That(Luma(UnityTuning.Press63LensOnHex), Is.GreaterThanOrEqualTo(0.85), "lit: bright");
            Assert.That(Luma(UnityTuning.Press63LensOffHex) - Luma(Press63Math.ToneOf(Press63Math.Part.Housing).front), Is.GreaterThanOrEqualTo(0.15), "off: dark glass in a darker housing");
        }

        [Test]
        public void TheHead_ItsOwnClock_DropFasterThanLift_LampLitUpTop()
        {
            double low = Press63Math.Low(Min), high = Press63Math.High(Min, Max);
            Assert.That(high, Is.GreaterThan(low + 1.8), "a real stroke");
            double period = UnityTuning.Press63LiftS + UnityTuning.Press63HoldS + UnityTuning.Press63DropS + UnityTuning.Press63DwellS;
            for (double t = 0; t < 3 * period; t += 0.01)
            {
                double y = Press63Math.HeadY(t, 0, low, high);
                Assert.That(y, Is.InRange(low - 1e-9, high + 1e-9), "t " + t);
                Assert.That(Press63Math.HeadY(t + period, 0, low, high), Is.EqualTo(y).Within(1e-6), "periodic at " + t);
            }
            Assert.That(UnityTuning.Press63DropS, Is.LessThan(UnityTuning.Press63LiftS), "the drop is the hard part");
            double holdMid = UnityTuning.Press63LiftS + UnityTuning.Press63HoldS * 0.5;
            Assert.That(Press63Math.HeadY(holdMid, 0, low, high), Is.EqualTo(high).Within(1e-6), "held up top");
            Assert.IsTrue(Press63Math.LampLit(holdMid, 0), "the lamp lit while it waits to drop");
            double dwellMid = UnityTuning.Press63LiftS + UnityTuning.Press63HoldS + UnityTuning.Press63DropS + UnityTuning.Press63DwellS * 0.5;
            Assert.That(Press63Math.HeadY(dwellMid, 0, low, high), Is.EqualTo(low).Within(1e-6), "dwelling on the bed");
            Assert.IsFalse(Press63Math.LampLit(dwellMid, 0), "dark on the bed");
            Assert.IsFalse(Press63Math.LampLit(UnityTuning.Press63LiftS * 0.5, 0), "dark through the lift");
            bool differ = false;
            for (double t = 0; t < period; t += 0.05) if (System.Math.Abs(Press63Math.HeadY(t, Press63Math.Phase(0), low, high) - Press63Math.HeadY(t, Press63Math.Phase(1), low, high)) > 0.5) differ = true;
            Assert.IsTrue(differ, "two presses out of phase");
            Press63Math.Piece die = Press63Math.Die(Min, Max, low), head = Press63Math.Head(Min, Max, low);
            Assert.That(die.min.y, Is.EqualTo(head.min.y).Within(1e-9), "the die at the head's bottom");
            Assert.That(Out(die, Min, Max), Is.GreaterThan(Out(head, Min, Max)), "the die proud of the head (it shows)");
        }

        [Test]
        public void TheColours_NoWarmHue_NeverARolesColour()
        {
            var hexes = new List<uint> { UnityTuning.Press63LensOnHex, UnityTuning.Press63LensOffHex };
            foreach (Press63Math.Part p in System.Enum.GetValues(typeof(Press63Math.Part)))
            {
                if (p == Press63Math.Part.Lens) continue;
                Press63Math.Tone t = Press63Math.ToneOf(p);
                hexes.AddRange(new[] { t.front, t.side, t.top, t.bottom });
            }
            foreach (uint hex in hexes)
            {
                double r = ((hex >> 16) & 255) / 255.0, g = ((hex >> 8) & 255) / 255.0, b = (hex & 255) / 255.0;
                BossCue63.Hue63 k = BossCue63.Class(r, g, b);
                Assert.AreNotEqual(BossCue63.Hue63.Danger, k, hex.ToString("X6") + " is in the danger hue");
                Assert.AreNotEqual(BossCue63.Hue63.Parry, k, hex.ToString("X6") + " is the parry yellow");
                BossCue63.Hsv(r, g, b, out double h, out double s, out double v);
                if (s >= 0.1) Assert.IsFalse(h >= 330 || h <= 75, hex.ToString("X6") + " is warm (hue " + h.ToString("0") + "): S7's bone stripe rendered hue 36");
                Assert.IsFalse(s >= 0.3 && h >= 170 && h <= 200, hex.ToString("X6") + " reads as the player's cyan");
                Assert.IsFalse(s >= 0.3 && h >= 285 && h < 330, hex.ToString("X6") + " reads as the RIVAL's magenta");
            }
        }
    }
}
