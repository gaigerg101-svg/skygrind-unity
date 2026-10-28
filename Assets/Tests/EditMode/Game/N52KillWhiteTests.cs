// NIGHT52 builder K (T-111, NIGHT50 G4 SHORT: the enemy kill's wash +0.27 with fx49 on). Chunks.KillWhiteCap: a gun kill's blocks white for
// UnityTuning.Fx49KillWhiteFrames drawn frames at most, only with fx49 AND skin52 on. Pinned:
//   1. the gate: the cap only for a gun kill with FeelAB on, fx49 on and skin52 on; every other case -1 (today's chunks exactly); the number
//      is the kill burst's frames (white under the crush disc, none after it)
//   2. the off switch: fx49 is ON in the shipping config and -skygrindOff52 (ForceOffFrom 52) does not cover NIGHT49's row, so skin52 is the
//      gate: with skin52 on the cap applies, with the switch or skin52 off it does not (NIGHT51's chunks)
using System;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class N52KillWhiteTests
    {
        static string PortRoot()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport")); }
            catch (Exception) { return Path.GetFullPath("PortExport"); }
        }

        static GameConfig Cfg(bool skin52)
        {
            GameConfig c = GameConfig.Load(Path.Combine(PortRoot(), "config.json"));
            c.Obj("design.n52.skin52")["enabled"] = skin52;
            return c;
        }

        static void Switch(int offFrom, Action body)
        {
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = offFrom;
                body();
            }
            finally
            {
                NightRows.ForceOffFrom = was;
            }
        }

        [Test]
        public void TheCap_OnlyForAGunKill_WithFx49AndSkin52()
        {
            Assert.That(UnityTuning.Fx49KillWhiteFrames, Is.EqualTo(UnityTuning.KillBurstFullFrames + UnityTuning.KillBurstFadeFrames),
                "white on the kill burst's frames (under the crush disc), none after it");
            Assert.That(Chunks.KillWhiteCap(true, false, true, true), Is.EqualTo(UnityTuning.Fx49KillWhiteFrames), "a gun kill, FeelAB, fx49, skin52: capped");
            for (int k = 0; k < 16; k++)
            {
                bool n18 = (k & 1) != 0, melee = (k & 2) != 0, fx49 = (k & 4) != 0, skin52 = (k & 8) != 0;
                if (n18 && !melee && fx49 && skin52) continue;
                Assert.That(Chunks.KillWhiteCap(n18, melee, fx49, skin52), Is.EqualTo(-1),
                    $"n18 {n18} melee {melee} fx49 {fx49} skin52 {skin52}: today's chunks (no cap)");
            }
        }

        [Test]
        public void TheOffSwitch_Skin52Gates_Fx49DoesNot()
        {
            GameConfig on = Cfg(true), off = Cfg(false);
            Switch(NightRows.NoOff, () =>
            {
                Assert.That(NightRows.ViewOn(on, "fx49"), Is.True, "fx49 is ON in the shipping config (design.n49.fx49.enabled)");
                Assert.That(Chunks.KillWhiteCap(true, false, NightRows.ViewOn(on, "fx49"), NightRows.ViewOn(on, "skin52")), Is.EqualTo(UnityTuning.Fx49KillWhiteFrames), "skin52 on: capped");
                Assert.That(Chunks.KillWhiteCap(true, false, NightRows.ViewOn(off, "fx49"), NightRows.ViewOn(off, "skin52")), Is.EqualTo(-1), "skin52 off: today's chunks");
            });
            Switch(52, () =>
            {
                Assert.That(NightRows.ViewOn(on, "fx49"), Is.True, "-skygrindOff52 does not cover NIGHT49's fx49 (why skin52 gates the cap)");
                Assert.That(NightRows.ViewOn(on, "skin52"), Is.False, "-skygrindOff52 turns skin52 off");
                Assert.That(Chunks.KillWhiteCap(true, false, NightRows.ViewOn(on, "fx49"), NightRows.ViewOn(on, "skin52")), Is.EqualTo(-1), "the off switch: NIGHT51's chunks");
            });
        }
    }
}
