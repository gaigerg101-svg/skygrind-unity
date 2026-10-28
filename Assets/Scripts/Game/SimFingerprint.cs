// NIGHT13 U3-26.2: the composed fingerprint (audit-perf section 5 move 1). A read-only 64-bit FNV-1a hash over the
// run's Core state (the mover and the look, health, the economy, the run, the director, the live enemies, style, the
// loadout, zones, perks and mods, and the state of every Mulberry32 stream), and the fixed input script a fingerprint
// run plays (move, jump, dash, slide, aim at the nearest enemy, fire, punch, the special, hold BUY, switch guns).
// A fingerprint run is a seeded run on this script with OnScreen pinned, settings at their defaults and a fresh
// profile, hashed every 600 steps: no refactor of the composition may change one hash (NIGHT13 rule 5).
// No UnityEngine use here: the same code hashes the Player's run and the headless test's (U3-26.6).
using System;
using System.Collections.Generic;
using System.Reflection;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class SimFingerprint
    {
        public const int Steps = 7200; // 60 s of 120 Hz steps
        public const int Every = 600; // a hash every 5 s
        public const uint Seed = 1; // the baseline's seed (FingerprintBaseline)
        public const uint RedSeed = 2; // another seed must print other hashes (seen red)

        /// <summary>FNV-1a, 64 bits, over raw bits: the same on Mono, IL2CPP and .NET.</summary>
        public struct Fnv
        {
            public ulong h;

            public static Fnv Start() => new Fnv { h = 14695981039346656037UL };

            public void Byte(byte b)
            {
                h ^= b;
                h *= 1099511628211UL;
            }

            public void U64(ulong v)
            {
                for (int i = 0; i < 8; i++) Byte((byte)(v >> (8 * i)));
            }

            public void I(long v) => U64(unchecked((ulong)v));

            public void B(bool v) => Byte(v ? (byte)1 : (byte)0);

            public void D(double v) => U64(unchecked((ulong)BitConverter.DoubleToInt64Bits(v)));

            public void V(Vec3 v)
            {
                D(v.x);
                D(v.y);
                D(v.z);
            }

            public void S(string s)
            {
                if (s == null)
                {
                    I(-1);
                    return;
                }
                I(s.Length);
                foreach (char c in s)
                {
                    Byte((byte)c);
                    Byte((byte)(c >> 8));
                }
            }
        }


        static uint RngOf(object owner, string field)
        {
            if (owner == null) return 0;
            FieldInfo f = owner.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (f == null) throw new InvalidOperationException($"fingerprint: no Mulberry32 field '{field}' on {owner.GetType().Name}");
            var rng = (Mulberry32)f.GetValue(owner);
            return rng == null ? 0 : rng.State; // NIGHT13 U3-26.10: Mulberry32.State (the same uint the reflection read)
        }

        /// <summary>The run's Core state, hashed (read only).</summary>
        public static ulong Hash(SurvivalSim g)
        {
            Fnv f = Fnv.Start();
            Mover m = g.mover;
            f.V(m.position);
            f.V(m.velocity);
            f.I((int)m.state);
            f.B(m.grounded);
            f.B(m.crouched);
            f.D(g.input.Yaw);
            f.D(g.input.Pitch);
            f.D(g.health.hp);
            f.D(g.health.max);
            f.D(g.health.hard);
            f.D(g.economy.total);
            f.D(g.economy.earned);
            f.D(g.economy.spent);
            SurvivalRun run = g.run;
            f.I((int)run.state);
            f.I(run.round);
            f.I(run.cleared);
            f.I(run.steps);
            f.I(run.phaseSteps);
            f.I(run.stats.kills);
            f.I(run.stats.headshots);
            f.I(run.stats.shots);
            f.I(run.stats.shotsHit);
            DirectorState ds = g.director.state;
            f.D(ds.intensity);
            f.S(ds.phase);
            f.D(ds.phaseTime);
            f.D(ds.relaxTime);
            f.D(ds.camp);
            f.B(ds.hunted);
            f.B(ds.exposed);
            int alive = 0;
            foreach (Enemy e in g.enemies.all)
            {
                if (!e.alive) continue;
                alive++;
                f.S(e.kind);
                f.D(e.hp);
                f.V(e.position);
                f.V(e.velocity);
                f.D(e.yaw);
                f.I(e.windupSteps);
                f.I(e.staggerSteps);
                f.I(e.knockSteps);
            }
            f.I(alive);
            StyleMeter st = g.style;
            f.D(st.core.meter);
            f.D(st.core.total);
            f.I(st.rank);
            f.I(st.core.bestRank);
            Loadout lo = g.loadout;
            f.I(lo.owned.Count);
            foreach (string w in lo.owned) f.S(w);
            f.I(lo.slot);
            f.I(lo.targetSlot);
            f.I(lo.shotSeq);
            f.B(lo.ready);
            for (int z = 0; z < g.map.zones.Count; z++) f.B(g.zones.IsLocked(z));
            f.I(g.perks.owned.Count);
            foreach (string p in g.perks.owned) f.S(p);
            var mods = new List<string>(g.mods.stacks.Keys);
            mods.Sort(StringComparer.Ordinal);
            foreach (string k in mods)
            {
                f.S(k);
                f.I(g.mods.stacks[k]);
            }
            // The streams (NIGHT14-RESEARCH section 4's table, rows 1-9).
            f.U64(RngOf(g.loadout, "rng"));
            f.U64(RngOf(g.projectiles, "rng"));
            f.U64(RngOf(g.director, "rng"));
            f.U64(RngOf(g.director, "paceRng"));
            f.U64(RngOf(g.brain, "rng"));
            f.U64(RngOf(g.husks.acid, "rng"));
            f.U64(RngOf(g.powerUps, "killRng"));
            f.U64(RngOf(g.powerUps, "rng"));
            f.U64(RngOf(g.mods, "draftRng"));
            f.U64(RngOf(g.mods, "rollRng"));
            f.U64(RngOf(g.forge, "rng"));
            f.U64(RngOf(g.box, "rng"));
            f.U64(RngOf(g.exfil, "rng"));
            return f.h;
        }

        /// <summary>
        /// The input of fingerprint step i (0 at the run's first step): strafe left / right every 2 s, forward in alternate
        /// 5 s, jump, dash, slide; aim at the nearest live enemy (else turn slowly); fire held with a press every 10 steps,
        /// a punch, the special, BUY held 3 s in every 10, a gun switch every 12.5 s.
        /// </summary>
        public static void Script(int i, Vec3 eye, EnemyRegistry enemies, ref double yaw, ref double pitch, out InputFrame move, out CombatInput combat)
        {
            Enemy near = null;
            double best = 60 * 60;
            foreach (Enemy e in enemies.all)
            {
                if (!e.alive) continue;
                Vec3 a = e.AimPoint;
                double dx = a.x - eye.x, dy = a.y - eye.y, dz = a.z - eye.z;
                double d2 = dx * dx + dy * dy + dz * dz;
                if (d2 < best)
                {
                    best = d2;
                    near = e;
                }
            }
            if (near != null)
            {
                Vec3 a = near.AimPoint;
                double dx = a.x - eye.x, dy = a.y - eye.y, dz = a.z - eye.z;
                yaw = JsMath.Atan2(-dx, -dz); // yaw 0 looks toward -z (SurvivalGame.Aim)
                pitch = JsMath.Atan2(dy, Math.Sqrt(dx * dx + dz * dz));
            }
            else yaw += 0.01;
            move = new InputFrame
            {
                moveX = (i / 240) % 2 == 0 ? 1 : -1,
                moveZ = (i / 600) % 2 == 0 ? 0 : 1,
                jump = new Button { pressed = i % 150 == 0, held = i % 150 < 10 },
                // NIGHT17 U3-30.4: the press is held 1 s (the web's mover never reads dash.held: with design.stamina off the run
                // is NIGHT16's), so with the stamina rules on the boost runs on into the sprint and the fingerprint covers it
                dash = new Button { pressed = i % 420 == 210, held = i % 420 >= 210 && i % 420 < 330 },
                slide = new Button { pressed = i % 500 == 300, held = i % 500 >= 300 && i % 500 < 360 },
            };
            combat = new CombatInput
            {
                fire = new Button { held = true, pressed = i % 10 == 0 },
                special = new Button { pressed = i % 700 == 350, held = i % 700 == 350 },
                punch = new Button { pressed = i % 333 == 100, held = i % 333 == 100 },
                interact = new Button { held = i % 1200 >= 900, pressed = i % 1200 == 900 },
                weapon = i > 0 && i % 1500 == 0 ? 0 : i % 1500 == 750 ? 1 : -1,
            };
        }
    }
}
