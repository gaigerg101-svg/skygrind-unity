// NIGHT20 U3-34.9 A1 (builder EN owns this file): the boss rounds in the sim, behind design.adds.bulwark: main.js's
// survivalHooks.onRoundBegin / startBossRound / isDone and its bossDown / bossPart listeners, with the port's one boss so
// far (the BULWARK, Core/Enemies/Bulwark.cs; bossKit.js's schedule: round 5, then every rotation's bulwark slot). A boss
// round: the Grinder opens for free if it is still locked ("THE GRINDER OPENS"), takes the boss's shape
// (bossKit.arenas.bulwark: the 'pen', over boss.roundReshape s), the director plans no squads (the boss calls its adds
// in: the six hatch husks), the kit starts the boss in the Grinder's centre, and the round lasts while it lives. Its kill
// pays bossKit.rewards (the points, a free perk token, a guaranteed power-up at the core), says "<NAME> DOWN" and flattens
// the Grinder; a broken plate pays platePoints. `specialRound` "boss": announce (its break), start (roundBegin), end (the
// kill), as E's special rounds say theirs (H's slab: BOSS INBOUND / BOSS DOWN). A round the schedule gives a boss the
// port doesn't have yet (MONOLITH, RIVAL PRIME, ORBITER, HIVE) stays the director's ordinary round. Off: no boss, NIGHT19's
// round 5 exactly (the director's hooks stay null). Port difference (logged): the nav field stays the map's (the web
// rebuilds it after a reshape: navDirty); the pen's walls are the core's pillars, which the bodies collide with, and the
// hatch husks leave the pen through its four gaps by the watchdog when the field leads them into a wall.
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>bossKit.js (the BULWARK registered); null until the first run. H's boss bar: bossKit.Bar().</summary>
        public BossKit bossKit;
        /// <summary>The BULWARK (the view reads its pose, plates, ram / ring / beam).</summary>
        public Bulwark bulwark;
        /// <summary>design.adds.bulwark this run.</summary>
        public bool BossOn { get; private set; }
        bool bossWired;
        readonly SpecialRoundEvent bossSaid = new SpecialRoundEvent { name = "boss" };

        void Boss20StartRun()
        {
            if (!bossWired)
            {
                bossWired = true;
                var target = new BossTarget { position = () => mover.position, health = health, impulse = (x, y, z) => mover.Impulse(x, y, z) };
                bossKit = new BossKit(cfg, events, null, target);
                bulwark = new Bulwark(cfg, events, world, target, enemies.NextId, (kinds, p) => { if (!bulwark25Room) director.SpawnGroupAt(kinds, p); }); // NIGHT25 W: no hatch adds in a campaign boss room
                bossKit.bosses["bulwark"] = bulwark;
                world.SetLayer("bulwark", bulwark.layer, false); // collider.setLayer('bulwark', layer, {rays: false})
                events.On<RoundEvent>("roundBreak", e =>
                {
                    if (BossOn && Due(e.number).HasValue) SayBoss(e.number, "announce");
                });
                events.On<RoundEvent>("roundBegin", e =>
                {
                    if (BossOn) BossRoundBegin(e.number);
                });
                events.On<BossEvent>("bossPart", e =>
                {
                    if (BossOn) economy.Grant(e.points, "boss");
                });
                events.On<BossEvent>("bossDown", OnBossDown);
            }
            bossKit.Clear();
            BossOn = n20.Add(cfg, "bulwark");
            bulwark.NewRun(runSeed ^ (cfg.Has("design.adds.bulwark.salt") ? (uint)cfg.Num("design.adds.bulwark.salt") : 0));
            director.bossRound = BossOn ? n => Due(n).HasValue : (System.Func<int, bool>)null;
            director.bossAlive = BossOn ? bossKit.AnyAlive : (System.Func<bool>)null;
        }

        void Boss20Step(double dt)
        {
            if (!BossOn) return;
            bulwark.Step(dt); // the enemies facade's step (main.js: the boss systems with the enemies)
            bossKit.Step(dt);
        }

        /// <summary>The boss round `round` brings that the port has (null: none, or not ported yet).</summary>
        BossDue? Due(int round)
        {
            BossDue? due = bossKit.ForRound(round);
            return due.HasValue && bossKit.bosses.ContainsKey(due.Value.boss) ? due : null;
        }

        void SayBoss(int round, string phase)
        {
            bossSaid.round = round;
            bossSaid.phase = phase;
            bossSaid.count = 1;
            events.Emit("specialRound", bossSaid);
        }

        /// <summary>main.js startBossRound (the director's empty plan ran in StartWave, before roundBegin).</summary>
        void BossRoundBegin(int round)
        {
            BossDue? due = Due(round);
            if (!due.HasValue) return;
            if (!zones.IsOpen("grinder"))
            {
                zones.Open("grinder", true);
                events.Emit("banner", new BannerEvent { text = "THE GRINDER OPENS" }); // a new payload (twice a fight): banner listeners may keep it
            }
            ReshapeGrinder(bossKit.k.arenas.TryGetValue(due.Value.boss, out string shape) ? shape : "flat", cfg.Num("boss.roundReshape"));
            if (!enemies.all.Contains(bulwark.body)) enemies.all.Add(bulwark.body); // the registry's target (a new run clears it)
            bossKit.Start(due.Value.boss, round, due.Value.cycle, due.Value.mutator);
            SayBoss(round, "start");
        }

        void ReshapeGrinder(string shape, double seconds)
        {
            if (core == null) return;
            MovementConfig mc = cfg.movement;
            core.Reshape(CoreGrid.PresetHeights(shape, core.spec.cells, mc.core.maxHeight, mc.core.heightStep, new Mulberry32(1)), seconds, step);
        }

        /// <summary>main.js bossDown: the reward table (points, a free perk token, a guaranteed power-up), the banner, the
        /// Grinder flat again.</summary>
        void OnBossDown(BossEvent e)
        {
            if (!BossOn) return;
            enemies.all.Remove(bulwark.body); // out of the registry before the break (Husks.Save takes only the pools' bodies)
            BossReward reward = e.reward ?? (bossKit.k.rewards.TryGetValue(e.boss, out BossReward r) ? r : null);
            if (reward != null)
            {
                economy.Grant(reward.points, "boss");
                if (reward.token) perks.tokens++;
                if (reward.powerUp && run.Live) powerUps.Spawn(e.position);
            }
            events.Emit("banner", new BannerEvent { text = bossKit.NameOf(e.boss) + " DOWN" });
            ReshapeGrinder("flat", cfg.Num("boss.roundReshape"));
            SayBoss(e.round, "end");
        }
    }
}
