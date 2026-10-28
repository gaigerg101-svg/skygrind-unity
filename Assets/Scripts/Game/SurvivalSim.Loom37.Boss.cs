// NIGHT37 builder O (NIGHT37.md O1; BOSSES.md "L4 THE LOOM"): THE LOOM's fight in the sim (Core/Enemies/Loom37.cs on Core/Boss37.cs), the second half
// of SurvivalSim.Loom37.cs (its partials Loom37Boss*), behind design.n37.loom37 on its level only.
// - Begin ("loom37", the canyon room's boss id): THE LOOM at its first station, its six knees + the spinneret appended to map.grappleNodes (moving
//   nodes; a torn knee and a shut spinneret are parked out of reach), the bar through BossKit. A retry (a death in the fight): the leg restarts
//   (Boss37 RestartLeg: HP and stagger kept) and the player is back on the last anchor ledge.
// - Step: the brain's context from the player; the knees move as nodes (a rope on one keeps its anchor on it); the CUTS (cutDelayS after an anchor
//   falls behind the last ledge stood on, a cutPulseS RED pulse first, then dark); the HIDE rule (one tower > hideS, leg 2+ three quarters of it:
//   that tower's anchors are severed after a cutPulseS pulse); the WEB WALL's layer while it stands; a fall restarts the leg. The kill: every anchor back.
// - A punch is offered to the spools (the parry); the NAILGUN's detonate (nailDetonate) tears the nailed knees; a parried spool lands as a parry hit.
// No UnityEngine: SimHarness compiles this file.
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        public Loom37 loom37;
        public bool loom37Owns;
        /// <summary>The first of the 7 nodes the fight appends (6 knees, then the spinneret); -1 none.</summary>
        public int loom37KneeNode = -1;
        public readonly List<(Vec3 min, Vec3 max)> loom37Webs = new List<(Vec3, Vec3)>();
        public readonly List<(int pair, string ledge, double x0, double z0, double x1, double z1, double top)> loom37Towers = new List<(int, string, double, double, double, double, double)>();
        readonly List<Vec3> loom37Stations = new List<Vec3>();
        readonly BoxLayer loom37WebLayer = new BoxLayer();
        int l37WebShown = -1, l37HideSteps, l37HideTower = -1, l37CutSteps, l37PulseSteps;
        bool l37Hooked;
        /// <summary>Counters this run (the harness): anchors cut, towers severed, fight begins / retries.</summary>
        public int loom37Cuts, loom37Severs, loom37Begins, loom37Retries, loom37TearRefills;
        static readonly Vec3 Loom37Parked = new Vec3(0, -1000, 0);

        partial void Loom37BossStartRun()
        {
            if (!l37Hooked)
            {
                l37Hooked = true;
                events.On<MeleeEvent>("punch", e =>
                {
                    if (loom37 == null || !loom37Owns || !loom37.Alive || e.owner != "player") return;
                    var (eye, dir) = Aim();
                    loom37.Punch(eye, dir);
                });
                events.On("nailDetonate", o => { if (loom37 != null && loom37Owns && loom37.Alive) loom37.Detonated(); });
                events.On("loom37Tear", o => { if (Grapple32On && cfg.Design("n37.loom37.tearRefill")) G32Give(ref loom37TearRefills); }); // a torn knee winds a grapple charge back
            }
            if (loom37 != null)
            {
                loom37.Clear();
                enemies.all.Remove(loom37.body);
            }
            Loom37NodesRemove();
            loom37Owns = false;
            loom37Webs.Clear();
            loom37Towers.Clear();
            loom37Stations.Clear();
            loom37Cuts = loom37Severs = loom37Begins = loom37Retries = loom37TearRefills = 0;
            l37HideSteps = 0;
            l37HideTower = -1;
            if (l37WebShown >= 0) world?.SetLayer("loom37Web", null);
            l37WebShown = -1;
        }

        partial void Loom37BossLevel(Dictionary<string, object> lr)
        {
            foreach (object so in PortData.Arr(lr["stations"])) loom37Stations.Add(PortData.V3(so));
            foreach (object wo in PortData.Arr(lr["webWalls"]))
            {
                var w = PortData.Obj(wo);
                loom37Webs.Add((PortData.V3(w["min"]), PortData.V3(w["max"])));
            }
            foreach (object to in PortData.Arr(lr["towers"]))
            {
                var t = PortData.Obj(to);
                int pair = (int)PortData.Num(t["pair"]);
                double top = PortData.Num(t["top"]);
                foreach (string k in new[] { "rectW", "rectE" })
                {
                    var r = PortData.Arr(t[k]);
                    loom37Towers.Add((pair, "t" + pair + (k == "rectW" ? "w" : "e"), PortData.Num(r[0]), PortData.Num(r[1]), PortData.Num(r[2]), PortData.Num(r[3]), top));
                }
            }
            const string R = "design.n37.loom37.";
            l37CutSteps = MovementMath.StepsFor(cfg.Num(R + "cutDelayS"), step);
            l37PulseSteps = MovementMath.StepsFor(cfg.Num(R + "cutPulseS"), step);
        }

        void Loom37NodesAdd()
        {
            if (loom37KneeNode >= 0) return;
            loom37KneeNode = map.grappleNodes.Count;
            for (int i = 0; i < 7; i++) map.grappleNodes.Add(Loom37Parked);
        }

        void Loom37NodesRemove()
        {
            if (loom37KneeNode < 0) return;
            if (map.grappleNodes.Count >= loom37KneeNode + 7) map.grappleNodes.RemoveRange(loom37KneeNode, 7);
            loom37KneeNode = -1;
        }

        partial void Loom37BossSystem(ref IBossSystem boss)
        {
            if (loom37Owns && loom37 != null) boss = loom37;
        }

        partial void Loom37BossBegin(ref bool ok, ref Vec3 spawn, ref double yawDeg)
        {
            RoomDef room = campaign?.Room;
            if (bossKit == null || loom37Stations.Count < 3) return;
            Loom37Ledge back = loom37LastLedge ?? (loom37Ledges.Count > 0 ? loom37Ledges[0] : null);
            spawn = back != null ? new Vec3(back.at.x, back.y, back.at.z) : (room != null ? room.spawn : mover.position);
            yawDeg = 0;
            if (loom37 == null)
            {
                var target = new BossTarget { position = () => mover.position, health = health, impulse = (x, y, z) => mover.Impulse(x, y, z) };
                loom37 = new Loom37(cfg, Loom37Params.From(cfg), Boss37Params.From(cfg, "loom37"), events, target, enemies.NextId, runSeed ^ 0x100937u);
                loom37.ropeOut = () => mover.rope.phase != RopePhase.None;
                loom37.parryHit = (dmg, at) => loadout.Damage(loom37.body, dmg, false, Loom37.ParryWeapon, at);
                loom37.webAhead = me =>
                {
                    for (int i = 0; i < loom37Webs.Count; i++)
                    {
                        double zc = (loom37Webs[i].min.z + loom37Webs[i].max.z) * 0.5;
                        if (zc < me.z - 2) return i;
                    }
                    return -1;
                };
            }
            loom37.brain.tells = tells36;
            if (loom37.Alive && loom37Owns)
            {
                loom37Retries++;
                loom37.brain.RestartLeg();
                ok = true;
                return;
            }
            loom37Begins++;
            Loom40StayBegin(); // NIGHT40 B4 (loom40): THE LOOM stays in THE CANYON (SurvivalSim.N40.Loom.cs); off: the flag false, nothing changes
            loom37.Place(loom37Stations.ToArray(), (loom37CanyonX0 + loom37CanyonX1) * 0.5);
            bossKit.bosses["loom37"] = loom37;
            loom37Owns = true;
            bossKit.Start("loom37", 0, 0, null, loom37.p.name);
            // NIGHT38 O2 (loom38): THE LOOM's entrance for builder H's juice38 (the shake + flicker; H owns the event: the name only, the payload the
            // boss's display name, as bossRise's card; Juice38 merges the two on one beat). Once per fight (not on a leg's restart, above). Nothing with --off38.
            if (!N38Blocks.ForceOff) events.Emit("boss38Entrance", loom37.p.name);
            if (!enemies.all.Contains(loom37.body)) enemies.all.Add(loom37.body);
            Loom37NodesAdd();
            Loom37Nodes();
            ok = true;
        }

        /// <summary>The knees and the spinneret as nodes now (a torn knee and a shut spinneret parked out of reach).</summary>
        void Loom37Nodes()
        {
            if (loom37KneeNode < 0 || loom37 == null) return;
            for (int i = 0; i < 6; i++)
                Loom37MoveNode(loom37KneeNode + i, loom37.Alive && !loom37.Falling && !loom37.knees[i].torn && (!n40.loom40 || loom37.ActiveKnee(i)) ? loom37.knees[i].at : Loom37Parked); // NIGHT40 B4 (loom40): only the LIT pair hooks (MEASURED round 3: the pilot's hook took a past leg's knee, which never latches, and pulled it into the void)
            Loom37MoveNode(loom37KneeNode + 6, loom37.Alive && loom37.Open ? loom37.Spinneret : Loom37Parked);
        }

        partial void Loom37BossFell(Loom37Ledge back)
        {
            if (loom37 != null && loom37Owns && loom37.Alive && !loom37.Falling) loom37.brain.RestartLeg(); // D-6: the leg restarts, the boss keeps its progress
        }

        /// <summary>Is the player on (or riding) tower `i` of loom37Towers (its top, or within 1.2 m of its faces near the top)?</summary>
        bool Loom37OnTower(int i, Vec3 f)
        {
            var t = loom37Towers[i];
            return f.x >= t.x0 - 1.2 && f.x <= t.x1 + 1.2 && f.z >= t.z0 - 1.2 && f.z <= t.z1 + 1.2 && f.y > t.top - 9 && f.y < t.top + 2.5;
        }

        partial void Loom37BossStep(double dt)
        {
            if (loom37 == null || !loom37Owns) return;
            Vec3 feet = mover.position;
            if (loom37.Alive)
            {
                var ctx = new Boss37Context
                {
                    player = feet, playerVel = mover.velocity, grounded = mover.grounded, airborne = !mover.grounded,
                    riding = mover.state == MoveState.WallRide, movingTheWay = mover.velocity.z < -2, lineOfSight = true, outOfReach = false,
                };
                Loom40LatchStep(); // NIGHT40 B4 (loom40): the rope on a knee latches it (SurvivalSim.N40.Loom.cs); off: returns at once
                loom37.Step(dt, ctx);
                if (!BossOn) bossKit.Step(dt);
                Loom37Nodes();
                bool fight = loom37.brain.Fighting;
                // the cuts: anchors behind the last ledge stood on go dark cutDelayS later (a RED pulse first)
                foreach (Loom37Anchor a in loom37Anchors)
                {
                    if (a.dark) continue;
                    if (a.cutLeft < 0 && fight && loom37LastLedge != null && a.at.z > loom37LastLedge.z1 + 1 && !Loom47Live()) a.cutLeft = l37CutSteps; // NIGHT47 L: loom47's frame never cuts the rings behind you
                    if (a.cutLeft > 0 && --a.cutLeft == 0)
                    {
                        a.dark = true;
                        loom37Cuts++;
                        Loom37MoveNode(a.node, Loom37Parked);
                        events.Emit("loom37Cut", a.id);
                    }
                }
                // the hide rule: one tower > hideS (leg 2+: three quarters of it) severs that tower's anchors
                int on = -1;
                for (int i = 0; i < loom37Towers.Count && on < 0; i++) if (Loom37OnTower(i, feet)) on = i;
                if (on >= 0 && on == l37HideTower && fight) l37HideSteps++;
                else { l37HideTower = on; l37HideSteps = 0; }
                double hs = cfg.Num("design.n37.loom37.hideS") * (loom37.brain.leg >= 2 ? cfg.Num("design.n37.loom37.hideLeg2Scale") : 1);
                if (on >= 0 && l37HideSteps == MovementMath.StepsFor(hs, step))
                {
                    string tl = loom37Towers[on].ledge;
                    bool any = false;
                    foreach (Loom37Anchor a in loom37Anchors)
                        if (!a.dark && a.ledge == tl && (a.cutLeft < 0 || a.cutLeft > l37PulseSteps)) { a.cutLeft = l37PulseSteps; any = true; }
                    if (any) { loom37Severs++; events.Emit("loom37Sever", tl); }
                }
                // the web wall's layer while it stands
                int web = loom37.WebSolid ? loom37.webPair : -1;
                if (web != l37WebShown)
                {
                    l37WebShown = web;
                    if (web >= 0 && web < loom37Webs.Count)
                    {
                        loom37WebLayer.SetBoxes(new[] { loom37Webs[web] });
                        world.SetLayer("loom37Web", loom37WebLayer, true);
                    }
                    else world.SetLayer("loom37Web", null);
                }
            }
            if (!loom37.Alive)
            {
                enemies.all.Remove(loom37.body);
                Loom37NodesRemove();
                if (l37WebShown >= 0) { world.SetLayer("loom37Web", null); l37WebShown = -1; }
                foreach (Loom37Anchor a in loom37Anchors)
                    if (a.dark || a.cutLeft >= 0) { a.dark = false; a.cutLeft = -1; Loom37MoveNode(a.node, a.at); }
            }
        }

        /// <summary>The reel's way to show a cut on demand: the anchor `id` starts its cutPulseS RED pulse now, then goes dark. False: unknown or dark.</summary>
        public bool Loom37CutNow(string id)
        {
            foreach (Loom37Anchor a in loom37Anchors)
                if (a.id == id && !a.dark) { a.cutLeft = System.Math.Max(1, l37PulseSteps); return true; }
            return false;
        }

        /// <summary>An anchor's RED pulse now (0: none; 0..1 through its last cutPulseS) for the view.</summary>
        public double Loom37Pulse(Loom37Anchor a) => a.dark || a.cutLeft < 0 || a.cutLeft > l37PulseSteps ? 0 : 1 - a.cutLeft / (double)System.Math.Max(1, l37PulseSteps);
    }
}
