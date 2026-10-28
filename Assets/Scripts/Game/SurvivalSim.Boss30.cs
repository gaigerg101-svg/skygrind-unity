// NIGHT30 builder B (lantern30 / wyrm30 / glacier30): the NIGHT30 boss rules in the sim. Implements the lead's declaration-only partials N30BossStartRun / N30BossStep
// (SurvivalSim.N30.cs). Every effect is gated on the run's n30 blocks; with them off nothing here runs and the NIGHT29 bosses are untouched (Lantern30RingFill returns false,
// the Lantern29 tell stays design.n29.boss1's, the Lantern27 retract cycle runs). No UnityEngine: SimHarness compiles this file.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        public Lantern30 lantern30;
        public Lantern30Arena lantern30Arena;
        int lantern30FightId = -1;
        System.Func<string, string, bool> lantern30Gate;

        /// <summary>THE LANTERN RING is on for this run (design.n30.lantern30 over NIGHT29's boss1 rule, which it wraps).</summary>
        public bool Lantern30On => n30.lantern30 && n29.boss1;

        partial void N30BossStartRun()
        {
            lantern30FightId = -1;
            lantern30?.Reset();
            if (lantern27 != null) { lantern27.noRetract30 = false; lantern27.ringLimit30 = 0; }
            if (lantern29 != null) lantern29.Tell30(0);
            Wyrm30StartRun();
            Loom30StartRun();
            glacier30FightId = -1;
            glacier30?.Reset();
            if (glacier29 != null) { glacier29.noLane30 = false; glacier29.exposedScale30 = 1; glacier29.windowSeconds30 = 0; }
        }

        partial void N30BossStep(double dt)
        {
            Lantern30Step(dt);
            Wyrm30Step(dt);
            Glacier30Step(dt);
            Loom30Step(dt);
        }

        // --- THE LOOM (design.n30.span; DESIGN30 C4 / D; LEVEL 4's boss room s8, plan boss id "loom") ---------------------------------------

        public Loom30 loom30;
        int loom30NodeStart = -1;
        bool loom30LineSaid, glacier30VolleyWas, wyrm30PunchWas;
        readonly BossLine30 bossLine30Said = new BossLine30();

        /// <summary>A boss's teaching line through builder S's one budget (SurvivalSim.BossLine30: Teach30's per-fight count and the lesson widget; nothing with design.n30.teach
        /// off), and the "bossLine30" event for the reels when it shows.</summary>
        void BossLine30Say(string boss, string key, string text)
        {
            if (!BossLine30(boss + "." + key, text)) return;
            bossLine30Said.boss = boss;
            bossLine30Said.key = key;
            bossLine30Said.text = text;
            bossLine30Said.seconds = 3;
            bossLine30Said.show = teach30 != null ? teach30.Shown(boss + "." + key) : 1;
            events.Emit("bossLine30", bossLine30Said);
        }

        /// <summary>Start THE LOOM in the current campaign room (or, with a live Loom, a retry: its HP kept, the cut's clock restarted). The legs grip the plan's loomLegs (the
        /// level JSON's room row), else the room's middle +- legHalf. Its eight joints join the level's grapple nodes for the fight. false: the span block is off, no campaign room,
        /// or no BossKit yet.</summary>
        public bool Loom30Begin(out Vec3 spawn, out double yawDeg)
        {
            spawn = default;
            yawDeg = 0;
            RoomDef room = campaign?.Room;
            if (!n30.span || bossKit == null || room == null) return false;
            spawn = room.spawn;
            yawDeg = room.spawnYawDeg;
            if (loom30 == null)
            {
                var target = new BossTarget { position = () => mover.position, health = health, impulse = (x, y, z) => mover.Impulse(x, y, z) };
                loom30 = new Loom30(cfg, Loom30Params.From(cfg), events, target, enemies.NextId);
                loom30.playerHanging = () => mover.rope.phase == RopePhase.Attached;
                loom30.ropeAnchor = () => mover.rope.anchor;
                loom30.cutLine = drop => mover.Launch(-drop, mover.velocity.x * 0.5, mover.velocity.z * 0.5); // the line plucked: the rope released (no swing boost), you drop
                loom30.playerGrounded = () => mover.grounded;
            }
            if (loom30.Alive) { loom30.Retry(); return true; }
            loom30LineSaid = false;
            Vec3[] legs = Loom30Legs(room.id);
            Vec3 c = new Vec3((room.x0 + room.x1) * 0.5, room.spawn.y, (room.z0 + room.z1) * 0.5);
            if (legs != null) c = new Vec3((legs[0].x + legs[1].x + legs[2].x + legs[3].x) * 0.25, legs[0].y, (legs[0].z + legs[1].z + legs[2].z + legs[3].z) * 0.25);
            loom30.Place(c, legs);
            bossKit.bosses["loom"] = loom30;
            if (!enemies.all.Contains(loom30.body)) enemies.all.Add(loom30.body);
            bossKit.Start("loom", 0, 0, null, loom30.p.name);
            Loom30NodesAdd();
            return true;
        }

        /// <summary>The room row's loomLegs (four [x, y, z]) from the level JSON, or null.</summary>
        Vec3[] Loom30Legs(string roomId)
        {
            if (map?.json == null || !map.json.ContainsKey("campaign")) return null;
            var camp = PortData.Obj(map.json["campaign"]);
            if (!camp.ContainsKey("rooms")) return null;
            foreach (object ro in PortData.Arr(camp["rooms"]))
            {
                var r = PortData.Obj(ro);
                if (!r.ContainsKey("id") || (string)r["id"] != roomId || !r.ContainsKey("loomLegs")) continue;
                var arr = PortData.Arr(r["loomLegs"]);
                if (arr.Count != 4) return null;
                var v = new Vec3[4];
                for (int i = 0; i < 4; i++) v[i] = PortData.V3(arr[i]);
                return v;
            }
            return null;
        }

        void Loom30NodesAdd()
        {
            if (loom30NodeStart >= 0) return;
            loom30NodeStart = map.grappleNodes.Count;
            foreach (Vec3 j in loom30.joints) map.grappleNodes.Add(j);
        }

        void Loom30NodesRemove()
        {
            if (loom30NodeStart < 0) return;
            if (map.grappleNodes.Count >= loom30NodeStart + loom30.joints.Length) map.grappleNodes.RemoveRange(loom30NodeStart, loom30.joints.Length);
            loom30NodeStart = -1;
        }

        void Loom30Step(double dt)
        {
            if (loom30 == null) return;
            if (!loom30.Alive)
            {
                enemies.all.Remove(loom30.body);
                Loom30NodesRemove();
                return;
            }
            loom30.Step(dt);
            if (!BossOn) bossKit.Step(dt);
            if (loom30.phase == "p1" && !loom30LineSaid) { loom30LineSaid = true; BossLine30Say("loom", "start", cfg.Str("design.n30.span.loom.lineStart")); }
            // the joints are grapple nodes that move with the legs: the node list follows them, and a rope hooked on one keeps its anchor on it
            if (loom30NodeStart >= 0 && map.grappleNodes.Count >= loom30NodeStart + loom30.joints.Length)
                for (int i = 0; i < loom30.joints.Length; i++)
                {
                    Vec3 j = loom30.joints[i];
                    if (mover.rope.phase == RopePhase.Attached && Vec3.DistanceTo(mover.rope.anchor, map.grappleNodes[loom30NodeStart + i]) < 1e-6) mover.rope.anchor = j;
                    map.grappleNodes[loom30NodeStart + i] = j;
                }
        }

        void Loom30StartRun()
        {
            if (loom30 == null) return;
            loom30.Clear();
            enemies.all.Remove(loom30.body);
            Loom30NodesRemove();
        }

        // --- THE GLACIER REBUILT (design.n30.glacier30; DESIGN30 C2) ---------------------------------------------------------------------

        public Glacier30 glacier30;
        bool glacier28Is30;
        int glacier30FightId = -1;

        /// <summary>The rebuilt glacier is on for this run (design.n30.glacier30 over NIGHT29's boss2 rule, which it extends).</summary>
        public bool Glacier30On => n30.glacier30 && n29.boss2;

        /// <summary>design.n28.rink with design.n30.glacier30.rink's numbers over it (the bigger closed rink).</summary>
        Rink28Params Glacier30RinkParams() => Rink28Params.From(cfg, "design.n30.glacier30.rink.");

        void Glacier30Step(double dt)
        {
            if (glacier29 != null)
            {
                glacier29.noLane30 = Glacier30On;
                // with builder S's glide on the chase is much easier (harness: median 48 s): the exposed window is shorter and lands less (design.n30.glacier30.*Glide); off: NIGHT29's
                bool gl30 = Glacier30On && n30.glide;
                glacier29.exposedScale30 = gl30 ? cfg.Num("design.n30.glacier30.exposedScaleGlide") : 1;
                glacier29.windowSeconds30 = gl30 ? cfg.Num("design.n30.glacier30.windowSecondsGlide") : 0;
            }
            if (!Glacier30On || glacier28 == null || glacier29 == null || !glacier28Is30) return;
            // builder S's glide (design.n30.glide) reads the rink's ice through this test (the fight's own Surfaces26, not the level's)
            if (glideIceAt30 == null) glideIceAt30 = feet =>
            {
                if (!Glacier30On || glacier28 == null || !glacier28.Alive || glacier28Surf == null) return false;
                foreach (SurfaceZone z in glacier28Surf.zones) if (z.kind == SurfaceKind.Ice && z.Contains(feet.x, feet.y, feet.z)) return true;
                return false;
            };
            if (glacier30 == null)
            {
                var target = new BossTarget { position = () => mover.position, health = health, impulse = (x, y, z) => mover.Impulse(x, y, z) };
                glacier30 = new Glacier30(cfg, glacier28, glacier29, events, target);
                glacier30.playerBody = () => mover.Body;
                glacier30.mover = mover;
                glacier30.lineOpen = (a, b) =>
                {
                    Vec3 d = b - a;
                    double len = d.Length;
                    if (len < 1e-6) return true;
                    return !(world.RayIntersect(a, d / len, out RayHit hit) && hit.distance < len - 0.3);
                };
                glacier30.Reset();
            }
            if (glacier28.Alive && glacier28.body.id != glacier30FightId)
            {
                glacier30FightId = glacier28.body.id;
                glacier30.Reset();
            }
            glacier30.Step(dt);
            bool vt = glacier30.VolleyTelling;
            if (vt && !glacier30VolleyWas) BossLine30Say("glacier", "volley", cfg.Str("design.n30.glacier30.lineVolley")); // the budget keeps it to the first two volleys of a fight
            glacier30VolleyWas = vt;
        }

        Lantern30Arena Lantern30ArenaEnsure() => lantern30Arena != null && lantern30Arena.Round == (n32.arenas32 && cfg.Num("design.n32.arenas32.facetsPerPanel") > 1) ? lantern30Arena : (lantern30Arena = Lantern30Arena.Build(cfg, n32.arenas32)); // NIGHT32 A: the run's ring (round while arenas32 is on)

        /// <summary>Lantern27Rebuild's hook (SurvivalSim.Boss27.cs): with the row on, the boss room's layer is the ring of flat panels (no retracting segments); false: the courtyard.</summary>
        bool Lantern30RingFill(BoxLayer layer)
        {
            if (!Lantern30On) return false;
            Lantern30ArenaEnsure().FillLayer(layer, lantern27Origin);
            return true;
        }

        void Lantern30Step(double dt)
        {
            if (lantern27 == null) return;
            if (!Lantern30On || lantern29 == null)
            {
                lantern27.noRetract30 = false;
                lantern27.ringLimit30 = 0;
                return;
            }
            lantern27.ringLimit30 = Lantern30ArenaEnsure().p.apothem - cfg.Num("design.n30.lantern30.rainInset");
            if (lantern30 == null)
            {
                var target = new BossTarget { position = () => mover.position, health = health, impulse = (x, y, z) => mover.Impulse(x, y, z) };
                lantern30 = new Lantern30(cfg, lantern27, lantern29, Lantern30ArenaEnsure(), events, target);
                lantern30.playerRiding = () => mover.state == MoveState.WallRide;
                lantern30.playerVelocity = () => mover.velocity;
            }
            lantern30Gate ??= (k, t) => BossLine30("lantern." + k, t);
            lantern30.lineGate = n30.teach ? lantern30Gate : null; // one budget: builder S's Teach30 when the teach block is on
            lantern27.rule29 = lantern30; // after N29BossStep set Lantern29: the ring's rule wraps it (platform shots land nothing)
            lantern27.noRetract30 = true;
            if (lantern27.Alive && lantern27.body.id != lantern30FightId)
            {
                lantern30FightId = lantern27.body.id;
                lantern30.Reset();
            }
            lantern30.Step(dt);
        }

        // --- THE KILN WYRM HEAD-PUNCH (design.n30.wyrm30; D-8) ----------------------------------------------------------------------------

        /// <summary>The head is punchable now (the view glows it cyan): the worm charges the player and contact is at most windowSeconds away. Set at the end of each step
        /// (what the player sees when he swings in the next).</summary>
        public bool Wyrm30Punchable { get; private set; }
        /// <summary>Seconds to the head's contact while it charges (the harness and the view's ramp; 99 when not charging).</summary>
        public double Wyrm30Contact { get; private set; } = 99;
        /// <summary>This fight's staggers (the harness line).</summary>
        public int wyrm30Staggers, wyrm30WindowSteps;
        bool wyrm30Hooked;
        int wyrm30RecoilSteps;
        readonly Wyrm30Event wyrm30Said = new Wyrm30Event();

        /// <summary>The payload of "wyrmStagger": where the head was punched (the white flash and the ring burst), the bonus damage, the recoil's length.</summary>
        public sealed class Wyrm30Event
        {
            public Vec3 position;
            public double bonus, recoilSeconds;
        }

        void Wyrm30StartRun()
        {
            Wyrm30Punchable = false;
            Wyrm30Contact = 99;
            wyrm30Staggers = wyrm30WindowSteps = wyrm30RecoilSteps = 0;
            if (!wyrm30Hooked)
            {
                wyrm30Hooked = true;
                events.On<HitEvent>("hit", Wyrm30OnHit);
            }
        }

        /// <summary>A PULSE gauntlet hit on the worm while its head is punchable and the head's box is in the swing's reach: the stagger (in the hit's own step, before the
        /// worm moves again, so the head never lands its contact after a good punch).</summary>
        void Wyrm30OnHit(HitEvent e)
        {
            if (!n30.wyrm30 || !Wyrm30Punchable || wyrm24 == null || !wyrm24.Alive || e.owner != "player" || e.weapon != "gauntlet" || e.enemy != wyrm24.body.id) return;
            Vec3 me = mover.position, eye = new Vec3(me.x, me.y + cfg.Num("body.eyeHeight"), me.z);
            double reach = cfg.Num("weapons.gauntlet.range") + cfg.Num("design.n30.wyrm30.headReachMargin");
            if (Reach34Margin > 0) reach += Reach34Margin; // NIGHT34 reach34: the gauntlet's boss margin (SurvivalSim.Reach34.cs; 0 off: NIGHT33)
            if (!wyrm24.boxes[0].Touches(eye, reach)) return;
            double rec = cfg.Num("design.n30.wyrm30.recoilSeconds");
            Vec3 head = wyrm24.boxes[0].c;
            if (!wyrm24.Stagger30(rec, cfg.Num("design.n30.wyrm30.recoilBack"), cfg.Num("design.n30.wyrm30.recoilUp"), head.x - me.x, head.z - me.z)) return;
            // the bonus: (bonusMult - 1) more of the same hit, through the same part rule the swing itself landed with, so the punch lands bonusMult x in all
            double bonus = 0, extra = e.damage * (cfg.Num("design.n30.wyrm30.bonusMult") - 1);
            if (extra > 0)
            {
                wyrm24.hitPart = "body";
                bonus = wyrm24.Filter(extra, out bool blocked);
                if (!blocked) wyrm24.Apply(bonus); else bonus = 0;
            }
            Hitstop(cfg.Num("design.n30.wyrm30.hitstopSteps") * cfg.Dt, "wyrmStagger");
            wyrm30Staggers++;
            Wyrm30Punchable = false;
            wyrm30RecoilSteps = MovementMath.StepsFor(rec, cfg.Dt);
            wyrm30Said.position = head;
            wyrm30Said.bonus = bonus;
            wyrm30Said.recoilSeconds = rec;
            events.Emit("wyrmStagger", wyrm30Said);
        }

        void Wyrm30Step(double dt)
        {
            if (!n30.wyrm30 || wyrm24 == null || !wyrm24.Alive || !wyrm24.Charging30)
            {
                Wyrm30Punchable = false;
                wyrm30PunchWas = false;
                Wyrm30Contact = 99;
                return;
            }
            // seconds to contact: the player's chest ahead of the head along its velocity, the head's front face and the capsule's radius taken off
            WyrmBox h = wyrm24.boxes[0];
            Vec3 v = wyrm24.body.velocity;
            double sp = v.Length;
            Capsule c = mover.Body;
            Vec3 chest = new Vec3((c.start.x + c.end.x) * 0.5, (c.start.y + c.end.y) * 0.5, (c.start.z + c.end.z) * 0.5);
            double ttc = 99;
            if (sp > 1e-6)
            {
                Vec3 u = v / sp, d = chest - h.c;
                double along = Vec3.Dot(d, u);
                Vec3 lat = d - u * along;
                double side = JsMath.Max(h.hr, h.hu) + c.radius + cfg.Num("design.n30.wyrm30.laneMargin");
                if (lat.Length <= side) ttc = (along - h.hf - c.radius) / sp;
            }
            Wyrm30Contact = ttc;
            Wyrm30Punchable = ttc >= -cfg.Num("design.n30.wyrm30.lateSeconds") && ttc <= Wyrm47PunchWindow(cfg.Num("design.n30.wyrm30.windowSeconds")); // NIGHT47 B: wyrm47's wider window (SurvivalSim.N47.Wyrm.cs; off: the same number)
            if (Wyrm30Punchable) wyrm30WindowSteps++;
            if (Wyrm30Punchable && !wyrm30PunchWas) BossLine30Say("wyrm", "punch", Wyrm47Line(cfg.Str("design.n30.wyrm30.linePunch"))); // NIGHT47 B: wyrm47's line while the row is on // the first punchable windows of a fight (the budget: two)
            wyrm30PunchWas = Wyrm30Punchable;
        }
    }
}
