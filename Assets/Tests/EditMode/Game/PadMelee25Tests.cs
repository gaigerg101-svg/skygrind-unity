// NIGHT25 melee-fire (builder C; design.n25.meleeFire, GG 2026-10-08: "if you're shooting a weapon and you're trying to melee you can't do both at the same
// time it's either one action or the other"). What was found, reading CombatInput -> sim step -> view: nothing in the sim, the keyboard / mouse reader
// (CombatReader latches fire and punch separately) or the viewmodel (the left arm is its own object; the gun stays up) stops a punch while the trigger is
// held. The pad did: PUNCH sat on R3, the click of the stick that aims. Layout v5 (RB PUNCH, R3 SPECIAL + ARCLINE) is the row's fix.
// - OFF (row false, or N25Blocks.ForceOff): the reader is layout v4 (R3 PUNCH);
// - the merge: a frame with fire held and punch pressed keeps both, from any source;
// - ON: RT held and RB pressed in one poll give Fire held + Punch latched (the finger rolls from RT to RB, both thumbs stay on the sticks), and R3 is no
//   longer the punch;
// - the store: v5 default = v4 with RB and R3 traded; a v4 default migrates, a customised v4 keeps the player's RB / R3.
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class PadMelee25Tests
    {
        static GameConfig cfg;

        static string Root()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..")); }
            catch (System.Exception) { return Path.GetFullPath("."); }
        }

        bool off24, off25;

        [OneTimeSetUp]
        public void Load() => cfg = GameConfig.Load(Path.Combine(Root(), "PortExport", "config.json"));

        [SetUp]
        public void Save()
        {
            off24 = N24Blocks.ForceOff;
            off25 = N25Blocks.ForceOff;
            N24Blocks.ForceOff = N25Blocks.ForceOff = false;
        }

        [TearDown]
        public void Restore()
        {
            N24Blocks.ForceOff = off24;
            N25Blocks.ForceOff = off25;
        }

        static PadReader Reader(bool meleeFire) => new PadReader(cfg) { n19 = true, n20 = true, padY = true, meleeFire = meleeFire, Connected = true };

        static void Frame(PadReader r, params PadButtonName[] down)
        {
            r.state.Clear();
            foreach (PadButtonName b in down) r.state.Set(b, true, 1);
            r.Step(1.0 / 60, true);
        }

        [Test]
        public void Off_WithTheRowOff_TheReaderIsV4_AndPunchIsOnR3()
        {
            PadReader r = Reader(false);
            Assert.That(r.LayoutVersion, Is.EqualTo(4));
            Assert.That(GamepadMath.ButtonFor(PadAction.Punch, GamepadMath.DefaultLayoutV4), Is.EqualTo(PadButtonName.RS));
            N25Blocks.ForceOff = true;
            Assert.That(Reader(true).LayoutVersion, Is.EqualTo(4), "--off25 puts v4 back");
        }

        [Test]
        public void TheMerge_KeepsFireHeldAndPunchPressed_InOneFrame()
        {
            var frame = new InputFrame();
            var combat = new CombatInput { fire = new Button { held = true, pressed = false } };
            var pad = new PadFrame { held = GamepadMath.Bit(PadAction.Punch), latched = GamepadMath.Bit(PadAction.Punch) };
            GamepadMath.MergePad(ref frame, ref combat, pad);
            Assert.That(combat.fire.held, Is.True, "the held trigger survives the pad's punch");
            Assert.That(combat.punch.pressed && combat.punch.held, Is.True, "the punch press survives the held trigger");
        }

        [Test]
        public void On_RightTriggerHeld_AndRbPressed_GiveFireAndPunchTogether()
        {
            PadReader r = Reader(true);
            Assert.That(r.LayoutVersion, Is.EqualTo(5));
            Frame(r, PadButtonName.RT);
            Frame(r, PadButtonName.RT, PadButtonName.RB);
            uint fire = GamepadMath.Bit(PadAction.Fire), punch = GamepadMath.Bit(PadAction.Punch);
            Assert.That(r.frame.held & fire, Is.EqualTo(fire), "RT stays held");
            Assert.That(r.frame.held & punch, Is.EqualTo(punch), "RB is PUNCH");
            Assert.That(r.frame.latched & punch, Is.EqualTo(punch), "the punch press is latched for the next step");
        }

        [Test]
        public void On_TheStickClick_IsNoLongerThePunch()
        {
            PadReader r = Reader(true);
            Frame(r, PadButtonName.RT, PadButtonName.RS);
            Assert.That(r.frame.held & GamepadMath.Bit(PadAction.Punch), Is.EqualTo(0u));
            Assert.That(GamepadMath.ButtonFor(PadAction.Punch, GamepadMath.DefaultLayoutV5), Is.EqualTo(PadButtonName.RB));
            Assert.That(GamepadMath.DefaultLayoutV5.MaskOf(PadButtonName.RS), Is.EqualTo(GamepadMath.DefaultLayoutV4.MaskOf(PadButtonName.RB)), "R3 carries RB's old SPECIAL + ARCLINE");
        }

        [Test]
        public void TheStore_V4Migrates_AndACustomV4KeepsItsButtons()
        {
            Assert.That(GamepadMath.MigrateV4(GamepadMath.DefaultLayoutV4).SameAs(GamepadMath.DefaultLayoutV5), Is.True);
            Assert.That(GamepadMath.LoadPadLayoutV5(null, null, null, null, null, out bool m0).SameAs(GamepadMath.DefaultLayoutV5) && !m0, Is.True);
            string v4Json = GamepadMath.PadLayoutV4Json(GamepadMath.DefaultLayoutV4);
            Assert.That(GamepadMath.LoadPadLayoutV5(null, v4Json, null, null, null, out bool m1).SameAs(GamepadMath.DefaultLayoutV5) && m1, Is.True, "a stored v4 migrates once");
            string v5Json = GamepadMath.PadLayoutV5Json(GamepadMath.DefaultLayoutV5);
            Assert.That(GamepadMath.LoadPadLayoutV5(v5Json, v4Json, null, null, null, out bool m2).SameAs(GamepadMath.DefaultLayoutV5) && !m2, Is.True, "v5 wins");
            Assert.That(GamepadMath.LoadPadLayoutV5(v4Json, null, null, null, null, out _).SameAs(GamepadMath.DefaultLayoutV5), Is.True, "another version under the v5 key reads as missing");
            Assert.That(GamepadMath.DefaultForVersion(5), Is.SameAs(GamepadMath.DefaultLayoutV5));
            Assert.That(GamepadMath.DefaultForVersion(4), Is.SameAs(GamepadMath.DefaultLayoutV4));
        }
    }
}
