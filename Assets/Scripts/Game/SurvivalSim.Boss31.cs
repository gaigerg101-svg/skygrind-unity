// NIGHT31 builder B (lantern31 / stag31 / wyrm31 / loom31): the NIGHT31 boss rules in the sim. Implements the lead's declaration-only partials N31BossStartRun / N31BossStep
// (SurvivalSim.N31.cs). Every effect is gated on the run's n31 blocks; with them off nothing here runs (the NIGHT30 bosses are untouched).
// THE STAG (design.n31.stag31; Core/Enemies/Stag31.cs): level 2's boss room (plan boss id "glacier") starts the Stag instead of the Glacier: the lead routes BossRoomBegin("glacier") to
// Stag31Begin and BossRoomSystem("glacier") to stag31 when n31.stag31 is on. The room has its OWN world layer "stag31Room" (the NIGHT30 Glacier rink with low banks) and its own Surfaces26
// (the ice), and the glide reads that ice. No UnityEngine: SimHarness compiles this file.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        public Stag31 stag31;
        public Rink28Arena stag31Rink;
        readonly BoxLayer stag31Layer = new BoxLayer();
        Surfaces26 stag31Surf;
        bool stag31GlideHooked;

        /// <summary>The Stag's ice (the skate view and the glide read it); null outside the fight.</summary>
        public Surfaces26 Stag31Surf => stag31Surf;

        partial void N31BossStartRun()
        {
            if (wyrm31CrumbleState != 0) Wyrm31CrumbleRestore(); // a new run: every worm platform is back
            Loom31PitClose(); // and the Loom deck is whole
            wyrm31FightId = -1;
            if (stag31 != null)
            {
                stag31.Clear();
                enemies.all.Remove(stag31.body);
                world.SetLayer("stag31Room", null);
                stag31Surf = null;
                if (mover != null && surfaces26 == null) mover.groundFrictionMult = 1;
            }
        }

        partial void N31BossStep(double dt)
        {
            Stag31Step(dt);
            Wyrm31Step(dt);
            Lantern31Step(dt);
            Loom31Step(dt);
        }

        // --- THE LOOM rebuilt (design.n31.loom31; Core/Enemies/Loom31.cs) -------------------------------------------------------------------

        public Loom31 loom31;
        Loom30 loom31Hooked;
        int loom31FightId = -1;

        /// <summary>The rope's anchor is on the Loom (a joint, or within lineReach of a leg's line): the player hangs on it.</summary>
        bool Loom31Hanging()
        {
            if (loom30 == null || mover.rope.phase != RopePhase.Attached) return false;
            Vec3 a = mover.rope.anchor;
            foreach (Vec3 j in loom30.joints) if (Vec3.DistanceTo(a, j) <= loom30.p.lineReach) return true;
            return false;
        }

        void Loom31Step(double dt)
        {
            bool on = n31.loom31 && loom30 != null;
            if (!on)
            {
                if (loom31Hooked != null) { Loom31.Unhook(loom31Hooked); loom31Hooked = null; }
                Loom31PitClose();
                return;
            }
            if (loom31 == null)
            {
                var target = new BossTarget { position = () => mover.position, health = health, impulse = (x, y, z) => mover.Impulse(x, y, z) };
                loom31 = new Loom31(cfg, Loom31Params.From(cfg), events, target);
                loom31.Reset();
                // builder L's s8.deckBreak31 rect from the level JSON, when the level has it
                if (map?.json != null && map.json.ContainsKey("campaign"))
                {
                    var camp = PortData.Obj(map.json["campaign"]);
                    if (camp.ContainsKey("rooms"))
                        foreach (object ro in PortData.Arr(camp["rooms"]))
                        {
                            var r = PortData.Obj(ro);
                            if (!r.ContainsKey("deckBreak31")) continue;
                            var c = PortData.Obj(r["deckBreak31"]);
                            if (!c.ContainsKey("rect")) continue;
                            var arr = PortData.Arr(c["rect"]);
                            if (arr.Count == 4) loom31.rect = new[] { PortData.Num(arr[0]), PortData.Num(arr[1]), PortData.Num(arr[2]), PortData.Num(arr[3]) };
                            if (c.ContainsKey("lavaY")) loom31.lavaY = PortData.Num(c["lavaY"]);
                        }
                }
            }
            if (loom31Hooked != loom30) { loom31Hooked = loom30; loom31.Hook(loom30); }
            if (!loom30.Alive) { Loom31PitClose(); return; }
            if (loom30.body.id != loom31FightId) { loom31FightId = loom30.body.id; loom31.Reset(); Loom31PitClose(); }
            loom31.Step(loom30, Loom31Hanging(), mover.grounded, dt);
            if (loom31.breakState == 2 && !loom31.pitOpen) Loom31PitOpen();
            if (loom31.breakState < 2 && loom31.pitOpen) Loom31PitClose(); // a retry above half health: the deck is whole again
        }

        // the deck break as a PIT (the lead, 12:18): the deck box's triangles in the static octree are moved far below (restored by Loom31PitClose; the octree's nodes keep the same
        // Triangle objects), a layer puts back the deck frame around the hole and a pit floor; Loom31 burns the player in the pit with design.n30.lava's numbers
        readonly System.Collections.Generic.List<(ThreeOctree.Triangle t, Vec3 a, Vec3 b, Vec3 c)> loom31Moved = new System.Collections.Generic.List<(ThreeOctree.Triangle, Vec3, Vec3, Vec3)>();
        readonly BoxLayer loom31DeckLayer = new BoxLayer();

        /// <summary>The pit's floor top (world y): the deck box's bottom (the reel and the view read it).</summary>
        public double Loom31PitFloorY { get; private set; } = double.NaN;

        void Loom31PitOpen()
        {
            double[] r = loom31.rect;
            if (r == null || r.Length != 4 || map?.json == null) return;
            double rx0 = System.Math.Min(r[0], r[2]), rx1 = System.Math.Max(r[0], r[2]), rz0 = System.Math.Min(r[1], r[3]), rz1 = System.Math.Max(r[1], r[3]);
            // the deck: the map box that holds the rect with its top at the deck
            Vec3 dmin = default, dmax = default;
            bool found = false;
            var col = PortData.Obj(map.json["collision"]);
            foreach (object bo in PortData.Arr(col["boxes"]))
            {
                var b = PortData.Obj(bo);
                Vec3 mn = PortData.V3(b["min"]), mx = PortData.V3(b["max"]);
                if (mn.x <= rx0 && mx.x >= rx1 && mn.z <= rz0 && mx.z >= rz1 && System.Math.Abs(mx.y - loom30.centre.y) < 0.05) { dmin = mn; dmax = mx; found = true; break; }
            }
            if (!found) return;
            const double eps = 1e-6;
            bool In(Vec3 p) => p.x >= dmin.x - eps && p.x <= dmax.x + eps && p.y >= dmin.y - eps && p.y <= dmax.y + eps && p.z >= dmin.z - eps && p.z <= dmax.z + eps;
            Vec3 away = new Vec3(0, -1000, 0);
            foreach (ThreeOctree.Triangle t in world.octree.StoredTriangles())
            {
                if (!In(t.a) || !In(t.b) || !In(t.c)) continue;
                loom31Moved.Add((t, t.a, t.b, t.c));
                t.a = t.a + away; t.b = t.b + away; t.c = t.c + away;
            }
            double y0 = dmin.y, y1 = dmax.y;
            var boxes = new System.Collections.Generic.List<(Vec3 min, Vec3 max)>
            {
                (new Vec3(dmin.x, y0, dmin.z), new Vec3(dmax.x, y1, rz0)),
                (new Vec3(dmin.x, y0, rz1), new Vec3(dmax.x, y1, dmax.z)),
                (new Vec3(dmin.x, y0, rz0), new Vec3(rx0, y1, rz1)),
                (new Vec3(rx1, y0, rz0), new Vec3(dmax.x, y1, rz1)),
                (new Vec3(rx0, y0 - 1, rz0), new Vec3(rx1, y0, rz1)), // the pit floor (the lava's bed)
            };
            loom31DeckLayer.SetBoxes(boxes);
            world.SetLayer("loom31Deck", loom31DeckLayer, true);
            Loom31PitFloorY = y0;
            // the lava: Loom31's own burn (design.n30.lava's numbers) at the level row's lavaY, the player only. Not a Lava30 zone: Lava30 burns every enemy whose feet are in a
            // zone, and the Loom's body hangs under the deck's middle (the first harness run killed the Loom with its own pit)
            if (double.IsNaN(loom31.lavaY)) loom31.lavaY = y0 + 1;
            loom31.pitOpen = true;
        }

        void Loom31PitClose()
        {
            if (loom31Moved.Count > 0)
            {
                foreach (var (t, a, b, c) in loom31Moved) { t.a = a; t.b = b; t.c = c; }
                loom31Moved.Clear();
                world.SetLayer("loom31Deck", null);
            }


            Loom31PitFloorY = double.NaN;
            if (loom31 != null) loom31.pitOpen = false;
        }

        // --- THE LANTERN's shield and sinking platforms (design.n31.lantern31; Core/Enemies/Lantern31.cs) -------------------------------------

        public Lantern31 lantern31;
        Lantern27 lantern31Hooked;
        int lantern31FightId = -1;
        bool lantern31Moved;
        int lantern31OffWall = 1 << 20;


        public bool Lantern31PlatformsMoved => n31.lantern31 && lantern31 != null && Lantern30On; // with the row on Boss31View owns the platforms (coloured tops, the sink tell, the sink)

        /// <summary>The ring's layer with the platforms at `top` (arena-local; none when flush with the floor).</summary>
        void Lantern31Refill(double top)
        {
            if (lantern27 == null || !Lantern30On) return;
            Lantern30ArenaParams prm = Lantern30ArenaParams.From(cfg, n32.arenas32); // NIGHT32 A: the run's ring (round arc platforms while arenas32 is on)
            if (top <= 0.01) prm.platformPanels = new int[0];
            else prm.platformTop = top;
            new Lantern30Arena(prm).FillLayer(lantern27Layer, lantern27Origin);
            lantern31Moved = System.Math.Abs(top - lantern31.platformTop0) > 1e-9;
        }

        void Lantern31Step(double dt)
        {
            bool on = n31.lantern31 && Lantern30On && lantern27 != null && lantern30 != null;
            if (!on)
            {
                if (lantern31Hooked != null) { lantern31Hooked.scale31 = null; lantern31Hooked = null; }
                if (lantern31Moved && lantern31 != null) Lantern31Refill(lantern31.platformTop0);
                return;
            }
            if (lantern31 == null)
            {
                lantern31 = new Lantern31(cfg, Lantern31Params.From(cfg), events, cfg.Num("design.n30.lantern30.arena.platformTop"));
                lantern31.Reset();
            }
            if (lantern31Hooked != lantern27)
            {
                if (lantern31Hooked != null) lantern31Hooked.scale31 = null;
                lantern31Hooked = lantern27;
                lantern27.scale31 = (amount, weapon) => lantern31.Scale(amount, weapon, mover.state == MoveState.WallRide || lantern31OffWall <= MovementMath.StepsFor(lantern31.p.rideGraceSeconds, cfg.Dt), lantern30.PlayerOnPlatform(), System.Math.Min(lantern30.HitScale(), lantern29 != null ? lantern29.p.lightDamage : 1), lantern30.p.platformScale); // the light share keeps NIGHT30 floor scale; never full without the break
            }
            if (!lantern27.Alive) return;
            if (lantern27.body.id != lantern31FightId)
            {
                lantern31FightId = lantern27.body.id;
                lantern31.Reset();
                if (lantern31Moved) Lantern31Refill(lantern31.platformTop0);
            }
            lantern31OffWall = mover.state == MoveState.WallRide ? 0 : lantern31OffWall + 1;
            if (lantern31.Step(lantern27.HpFrac, lantern27.Hurtable)) Lantern31Refill(lantern31.SinkTop);
        }

        // --- THE KILN WYRM rebuilt (design.n31.wyrm31; Core/Enemies/Wyrm31.cs) ------------------------------------------------------------

        public Wyrm31 wyrm31;
        Wyrm wyrm31Hooked;
        int wyrm31FightId = -1;

        /// <summary>The crumble (C3.5, builder L's k5.crumble31): 0 none, 1 the tell (red-orange cracks on the boxes), 2 fallen (the boxes are out of the worm's layer).</summary>
        public int wyrm31CrumbleState;
        public double Wyrm31CrumbleProgress => wyrm31CrumbleState == 1 && wyrm31 != null ? 1.0 - (double)wyrm31CrumbleLeft / System.Math.Max(1, MovementMath.StepsFor(wyrm31.p.crumbleTellSeconds, cfg.Dt)) : wyrm31CrumbleState == 2 ? 1 : 0;
        /// <summary>The arena box indices that crumble (the level's k5.crumble31 boxes, else design.n31.wyrm31.crumbleBoxes); the views skip state-2 boxes.</summary>
        public readonly System.Collections.Generic.List<int> wyrm31CrumbleBoxes = new System.Collections.Generic.List<int>();
        int wyrm31CrumbleLeft;
        readonly Wyrm31Event wyrm31CrumbleSaid = new Wyrm31Event();

        /// <summary>Is arena box i fallen now (WyrmRoomView skips it)?</summary>
        public bool Wyrm31Fallen(int i) => wyrm31CrumbleState == 2 && wyrm31CrumbleBoxes.Contains(i);

        void Wyrm31CrumbleIndices()
        {
            wyrm31CrumbleBoxes.Clear();
            if (map?.json != null && map.json.ContainsKey("campaign"))
            {
                var camp = PortData.Obj(map.json["campaign"]);
                if (camp.ContainsKey("rooms"))
                    foreach (object ro in PortData.Arr(camp["rooms"]))
                    {
                        var r = PortData.Obj(ro);
                        if (!r.ContainsKey("crumble31") || !r.ContainsKey("boss") || (string)r["boss"] != "wyrm") continue;
                        var c = PortData.Obj(r["crumble31"]);
                        if (c.ContainsKey("boxes")) foreach (object b in PortData.Arr(c["boxes"])) wyrm31CrumbleBoxes.Add((int)PortData.Num(b));
                    }
            }
            if (wyrm31CrumbleBoxes.Count == 0) foreach (double b in wyrm31.p.crumbleBoxes) wyrm31CrumbleBoxes.Add((int)b);
        }

        /// <summary>The worm's layer with every arena box, or without the crumbled ones.</summary>
        void Wyrm31Layer(bool fallen)
        {
            if (wyrm24Arena == null) return;
            var boxes = new System.Collections.Generic.List<(Vec3 min, Vec3 max)>();
            for (int i = 0; i < wyrm24Arena.boxes.Count; i++)
            {
                if (fallen && wyrm31CrumbleBoxes.Contains(i)) continue;
                var (min, max, _) = wyrm24Arena.boxes[i];
                boxes.Add((min + wyrm24Origin, max + wyrm24Origin));
            }
            wyrm24Layer.SetBoxes(boxes);
        }

        void Wyrm31CrumbleRestore()
        {
            if (wyrm31CrumbleState == 2) Wyrm31Layer(false);
            wyrm31CrumbleState = 0;
            wyrm31CrumbleLeft = 0;
        }

        void Wyrm31CrumbleSay(string stage)
        {
            wyrm31CrumbleSaid.stage = stage;
            wyrm31CrumbleSaid.at = wyrm24Origin;
            wyrm31CrumbleSaid.seconds = stage == "crumbleTell" ? wyrm31.p.crumbleTellSeconds : 0;
            events.Emit("wyrm31", wyrm31CrumbleSaid);
        }

        void Wyrm31Crumble()
        {
            if (wyrm31CrumbleState == 2 && wyrm24.HpFrac > wyrm31.p.crumbleAt) { Wyrm31CrumbleRestore(); return; } // a retry put the bar back over half: the platforms are back
            if (wyrm31CrumbleState == 0 && wyrm24.HpFrac <= wyrm31.p.crumbleAt && wyrm24.phase != "dying")
            {
                wyrm31CrumbleState = 1;
                wyrm31CrumbleLeft = System.Math.Max(1, MovementMath.StepsFor(wyrm31.p.crumbleTellSeconds, cfg.Dt));
                Wyrm31CrumbleSay("crumbleTell");
            }
            else if (wyrm31CrumbleState == 1 && --wyrm31CrumbleLeft <= 0)
            {
                wyrm31CrumbleState = 2;
                Wyrm31Layer(true);
                Wyrm31CrumbleSay("crumble");
            }
        }

        void Wyrm31Step(double dt)
        {
            bool on = n31.wyrm31 && wyrm24 != null;
            if (!on)
            {
                if (wyrm31Hooked != null) { wyrm31Hooked.feintNext31 = null; wyrm31Hooked.filter31 = null; wyrm31Hooked.aggroCool31 = -1; wyrm31Hooked = null; }
                if (wyrm31CrumbleState != 0) Wyrm31CrumbleRestore();
                return;
            }
            if (wyrm31 == null)
            {
                var target = new BossTarget { position = () => mover.position, health = health, impulse = (x, y, z) => mover.Impulse(x, y, z) };
                wyrm31 = new Wyrm31(cfg, Wyrm31Params.From(cfg), events, target);
                wyrm31.playerVelocity = () => mover.velocity;
                wyrm31.playerGrounded = () => mover.grounded;
                wyrm31.punchable = () => Wyrm30Punchable;
                wyrm31.Reset();
                events.On<Wyrm30Event>("wyrmStagger", e => { if (n31.wyrm31) wyrm31?.OnStagger(); });
            }
            if (wyrm31Hooked != wyrm24)
            {
                if (wyrm31Hooked != null) { wyrm31Hooked.feintNext31 = null; wyrm31Hooked.filter31 = null; wyrm31Hooked.aggroCool31 = -1; }
                wyrm31Hooked = wyrm24;
                wyrm24.feintNext31 = wyrm31.FeintNext;
                wyrm24.feintShort31 = wyrm31.p.feintShort;
                wyrm24.filter31 = wyrm31.Filter;
                wyrm24.aggroCool31 = wyrm31.p.aggroCooldown;
            }
            if (!wyrm24.Alive) { if (wyrm31CrumbleState != 0) Wyrm31CrumbleRestore(); return; }
            if (wyrm24.body.id != wyrm31FightId) { wyrm31FightId = wyrm24.body.id; wyrm31.Reset(); Wyrm31CrumbleRestore(); Wyrm31CrumbleIndices(); }
            wyrm31.Step(wyrm24, dt);
            Wyrm31Crumble();
        }

        // --- THE STAG ------------------------------------------------------------------------------------------------------------------------

        /// <summary>The player's spawn on the Stag's rink (the Glacier's playerStartS) and its yaw.</summary>
        public Vec3 Stag31Spawn { get { if (stag31Rink == null) return default; stag31Rink.Spawn(out Vec3 p, out _); return p; } }
        public double Stag31SpawnYawDeg { get { if (stag31Rink == null) return 0; stag31Rink.Spawn(out _, out double y); return y; } }

        /// <summary>Start THE STAG (or, with a live Stag, a retry: its HP kept). false: the stag31 block is off or no BossKit exists yet. Builds the rink layer and its ice.</summary>
        public bool Stag31Begin(out Vec3 spawn, out double yawDeg)
        {
            spawn = default;
            yawDeg = 0;
            if (!n31.stag31 || bossKit == null) return false;
            if (stag31Rink == null) stag31Rink = new Rink28Arena(Rink28Params.From(cfg, "design.n31.stag31.rink."));
            if (stag31 == null)
            {
                var target = new BossTarget { position = () => mover.position, health = health, impulse = (x, y, z) => mover.Impulse(x, y, z) };
                stag31 = new Stag31(cfg, Stag31Params.From(cfg), stag31Rink, events, target, enemies.NextId);
                stag31.playerVelocity = () => mover.velocity;
                stag31.playerGrounded = () => mover.grounded;
                stag31.backPunched = () => { if (n27.pulse27 && pulse27 != null && pulse27.active) pulse27.AddKill(); }; // C2.4: a back punch refills one gauntlet pip
            }
            spawn = Stag31Spawn;
            yawDeg = Stag31SpawnYawDeg;
            if (stag31.Alive) { stag31.Retry(); return true; }
            stag31Rink.FillLayer(stag31Layer);
            world.SetLayer("stag31Room", stag31Layer, true);
            stag31Surf = new Surfaces26(SurfaceRules.From(cfg));
            stag31Surf.zones.Add(stag31Rink.IceZone());
            stag31Surf.Reset(mover);
            if (!stag31GlideHooked)
            {
                stag31GlideHooked = true;
                System.Func<Vec3, bool> prev = glideIceAt30;
                glideIceAt30 = feet =>
                {
                    if (stag31 != null && stag31.Alive && stag31Surf != null)
                        foreach (SurfaceZone z in stag31Surf.zones) if (z.kind == SurfaceKind.Ice && z.Contains(feet.x, feet.y, feet.z)) return true;
                    return prev != null && prev(feet);
                };
            }
            // the Stag starts across the rink from the player (the Glacier's boss start), facing him
            Vec3 at = stag31Rink.FloorPoint(stag31Rink.BossStartS, (stag31Rink.latIsland + stag31Rink.latLaneOut) / 2);
            stag31.Place(at, spawn - at);
            bossKit.bosses["stag"] = stag31;
            if (!enemies.all.Contains(stag31.body)) enemies.all.Add(stag31.body);
            bossKit.Start("stag", 0, 0, null, stag31.p.name);
            return true;
        }

        void Stag31Step(double dt)
        {
            if (stag31 == null) return;
            if (!stag31.Alive)
            {
                enemies.all.Remove(stag31.body);
                if (stag31Surf != null)
                {
                    stag31Surf = null;
                    if (surfaces26 == null) mover.groundFrictionMult = 1;
                }
                return;
            }
            stag31.Step(dt);
            if (!BossOn) bossKit.Step(dt);
            if (stag31Surf != null && n26.surfaces) stag31Surf.Step(mover);
        }
    }
}
