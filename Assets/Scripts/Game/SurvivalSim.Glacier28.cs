// NIGHT28 builder B2 (boss): THE GLACIER's lap chase in the sim (design.n28.glacier; Core/Enemies/Glacier28.cs) on builder A2's rink (Core/Enemies/Rink28Arena.cs: the triangle soup, the ONE ice zone,
// Spawn at s = playerStartS; the boss starts at BossStartS). A campaign boss room (plan boss id "glacier" with design.n28.glacier.enabled on; the lead routes SurvivalSim.Boss25.cs's BossRoomBegin /
// BossRoomSystem to Glacier28Begin / Glacier28Spawn / Glacier28SpawnYawDeg below). The room has its OWN world layer "glacier28Room" and its own Surfaces26 (so the NIGHT27 room's state is not touched).
// Stepped from Boss27.cs's N27BossStep (one line). Nothing here runs with the row off. No UnityEngine: SimHarness compiles this file.
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        public Glacier28 glacier28;
        /// <summary>The rink this fight skates on (builder A2's arena, built from design.n28.rink whatever that row's own switch says).</summary>
        public Rink28Arena glacier28Rink;
        readonly BoxLayer glacier28Layer = new BoxLayer();
        Surfaces26 glacier28Surf;
        public int glacier28Retries;
        /// <summary>The harness forces the row for one sim (null: the config's switch; false: the RED run).</summary>
        public bool? glacier28Override;
        public bool Glacier28On => glacier28Override ?? (BossRoomsOn && cfg.Design("n28.glacier.enabled"));
        /// <summary>The fight's surface rules (the skate view reads the ice zone from it); null outside the room.</summary>
        public Surfaces26 Glacier28Surf => glacier28Surf;

        void Glacier28Step(double dt)
        {
            if (glacier28 == null) return;
            if (!glacier28.Alive)
            {
                enemies.all.Remove(glacier28.body);
                if (glacier28Surf != null)
                {
                    glacier28Surf = null;
                    if (surfaces26 == null) mover.groundFrictionMult = 1;
                }
                return;
            }
            glacier28.Step(dt);
            if (!BossOn) bossKit.Step(dt);
            if (glacier28Surf != null && n26.surfaces) glacier28Surf.Step(mover);
        }

        void Glacier28StartRun()
        {
            if (glacier28 == null) return;
            glacier28.Clear();
            enemies.all.Remove(glacier28.body);
            world.SetLayer("glacier28Room", null);
            glacier28Surf = null;
            if (mover != null && surfaces26 == null) mover.groundFrictionMult = 1;
        }

        public Glacier28 Glacier28Ensure()
        {
            if (glacier28 != null && glacier28Is30 == Glacier30On) return glacier28;
            if (glacier28 != null) { glacier28.Clear(); enemies.all.Remove(glacier28.body); glacier29 = null; glacier30 = null; } // NIGHT30 B: the rink changed size with the row: a new boss on it
            glacier28Is30 = Glacier30On;
            var target = new BossTarget { position = () => mover.position, health = health, impulse = (x, y, z) => mover.Impulse(x, y, z) };
            glacier28 = new Glacier28(cfg, glacier28Is30 ? Glacier28Params.From(cfg, Glacier30RinkParams()) : Glacier28Params.From(cfg), events, target, enemies.NextId);
            glacier28.playerBody = () => mover.Body;
            glacier28.playerVelocity = () => mover.velocity;
            glacier28.playerAim = () => Aim().Item2;
            glacier28.pulseReady = () => n27.pulse27 && pulse27 != null ? pulse27.Ready : (fist24 == null || !fist24.Active || fist24.CanFlick); // with pulse27 on the window opens only on a FULL glove off cooldown
            return glacier28;
        }

        /// <summary>The player's spawn: on the lane at the rink's playerStartS, facing the way the boss runs.</summary>
        public Vec3 Glacier28Spawn { get { if (glacier28Rink == null) return default; glacier28Rink.Spawn(out Vec3 p, out _); return p; } }
        public double Glacier28SpawnYawDeg { get { if (glacier28Rink == null) return 0; glacier28Rink.Spawn(out _, out double y); return y; } }

        /// <summary>Start the fight (or, with a live boss, retry it: back to the START of its phase). false: the row is off or no BossKit exists yet (no run has started). The caller then puts the
        /// player at Glacier28Spawn facing Glacier28SpawnYawDeg.</summary>
        public bool Glacier28Begin()
        {
            if (!Glacier28On || bossKit == null || Glacier28Ensure() == null) return false;
            glacier28.pulseSink = pulseKillSink27;
            if (glacier28.Alive)
            {
                glacier28Retries++;
                if (glacier28.RestorePhaseStart() || glacier28.phase == "dying") { Glacier28BankGlove(); return true; }
            }
            glacier28.NewRun(runSeed ^ glacier28.p.salt);
            glacier28Rink = glacier28Is30 ? new Rink28Arena(Glacier30RinkParams()) : Rink28Arena.Build(cfg); // NIGHT30 B (glacier30): the bigger rink
            glacier28Rink.FillLayer(glacier28Layer);
            glacier28Surf = new Surfaces26(SurfaceRules.From(cfg));
            glacier28Surf.zones.Add(glacier28Rink.IceZone()); // the ONE ice rect over the whole rink (doc 3.2)
            glacier28Surf.Reset(mover);
            world.SetLayer("glacier28Room", glacier28Layer, true);
            bossKit.bosses["glacier"] = glacier28;
            if (!enemies.all.Contains(glacier28.body)) enemies.all.Add(glacier28.body);
            bossKit.Start("glacier", 0, 0, null, glacier28.p.name);
            Glacier28BankGlove();
            return true;
        }

        /// <summary>The glove starts the fight banked: all pips lit (the NIGHT27 rule: a hit on the boss refills them, there are no kills on the ice).</summary>
        void Glacier28BankGlove()
        {
            if (pulse27 != null && pulse27.active) while (pulse27.AddKill()) { }
        }
    }
}
