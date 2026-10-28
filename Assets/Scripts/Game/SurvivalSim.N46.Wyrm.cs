// NIGHT46 builder B1 (NIGHT46.md card B1; DECIDE46 N-1 / N-10; docs/ROOMS.md "NIGHT46 identity cards", k5 THE KILN'S HEART, walked into).
// Two sim rows (design.n46.*, "sim": true; read with rows.On through the ids SurvivalSim.N46.cs resolves once a run):
//   wyrm46   THE KILN WYRM in the bot band (30-60 s) by its damage windows first (DECIDE39 D-1: stagger before health): while the row is on the
//            NIGHT31 / 32 worm system (Wyrm31) takes design.n46.wyrm46.numbers in place of its own: staggerSeconds (the OPEN window), open (what
//            every weapon lands while staggered), gunScale (the chip outside a stagger), aggroCooldown (seconds between the head's charges: each
//            charge is a punch window), and poolScale (x the fight's pool, after NIGHT39's boss39 x4; the phases are fractions of the pool, so
//            they move with it). A number missing from the row leaves that lever as it was. Every value is put back at the next run's start.
//   walkin46 the fight is walked into (FLOW-AUDIT row 2; GG w35 "you just teleport into these random rooms"): on a level whose worm arena file
//            (the k5 row's wyrmArena) declares `walkIn46.origin` (Design/maps/wyrm46-arena.json, n46-kiln), the worm's room is placed THERE (on
//            k5 itself, which n46-kiln builds at the arena's size) instead of design.n24.wyrm.room.origin, and walkIn46 is set, so Campaign24's
//            boss enter keeps the player where he walked in (Boss36WalkIn: no Respawn; the speed kept). The move is made before the fight
//            begins (the worm is built in N24StartRun, after this StartRun), on a level without it (or with the row off) the origin goes back.
// Off (the rows off, --off46 / -skygrindOff46): nothing here runs and nothing is changed. No allocation per step (the numbers are read at the
// run's start). No UnityEngine: SimHarness compiles this file.
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>Harness only (Tools/SimHarness `n46wyrm ... --set key=v`): a trial's value in place of design.n46.wyrm46.numbers.&lt;key&gt;
        /// (read at each run's start). Null in the game.</summary>
        public static Dictionary<string, double> Probe46Wyrm;

        const string Wyrm46Root = "design.n46.wyrm46.numbers.";

        // this run's numbers (NaN: the lever is not changed)
        double w46Stagger = double.NaN, w46Open = double.NaN, w46Gun = double.NaN, w46Aggro = double.NaN, w46Pool = double.NaN;
        // the Wyrm31 object the row changed, and its values before (put back at the next run's start)
        Wyrm31 w46For;
        double w46StaggerWas, w46OpenWas, w46GunWas, w46AggroWas;
        int w46Body = -1;

        /// <summary>The harness's count: worm fights whose pool wyrm46 scaled (this sim's life).</summary>
        public int wyrm46Pools;

        // walkin46: this run's target (the level's arena declares it), the config's far room, and whether the worm's room was moved
        bool w46WalkHere, w46Moved, w46Checked;
        Vec3 w46WalkOrigin, w46FarOrigin;
        string w46ArenaRead;
        bool w46ArenaHas;
        Vec3 w46ArenaOrigin;

        /// <summary>walkin46: this run's worm room is the level's own k5 (the views may show it before the fight: it is in the door's view).</summary>
        public bool Wyrm46ArenaHere => w46WalkHere;

        /// <summary>The level's worm arena file's walkIn46.origin (read once per file), or false.</summary>
        bool Wyrm46ArenaOrigin(string arena, out Vec3 origin)
        {
            if (arena != w46ArenaRead)
            {
                w46ArenaRead = arena;
                w46ArenaHas = false;
                string file = string.IsNullOrEmpty(Wyrm24Dir) || arena == null ? null : System.IO.Path.Combine(Wyrm24Dir, arena);
                if (file != null && System.IO.File.Exists(file))
                {
                    var j = PortData.Obj(PortData.LoadJson(file));
                    if (j.TryGetValue("walkIn46", out object wo) && wo is Dictionary<string, object> w && w.TryGetValue("origin", out object oo))
                    {
                        var o = PortData.Arr(oo);
                        w46ArenaOrigin = new Vec3(PortData.Num(o[0]), PortData.Num(o[1]), PortData.Num(o[2]));
                        w46ArenaHas = true;
                    }
                }
            }
            origin = w46ArenaOrigin;
            return w46ArenaHas;
        }

        /// <summary>Put the worm's room at `o` (its origin, its collision layer, the worm's centre): only while the worm is not fighting.</summary>
        void Wyrm46MoveRoom(Vec3 o)
        {
            wyrm24Origin = o;
            if (wyrm24Arena != null)
            {
                var boxes = new List<(Vec3 min, Vec3 max)>();
                foreach (var (min, max, _) in wyrm24Arena.boxes) boxes.Add((min + o, max + o));
                wyrm24Layer.SetBoxes(boxes);
            }
            if (wyrm24 != null) wyrm24.arenaCenter = o;
        }

        void Wyrm46WalkStep()
        {
            if (wyrm24 == null || wyrm24.Alive) return; // never mid-fight
            if (w46WalkHere && !w46Checked)
            {
                w46Checked = true;
                // a worm built for another level's arena (a session that played another level 3 first) is rebuilt now, before the begin, as
                // Wyrm24BeginCore would at the begin (where it would take the far origin again)
                if (wyrm24Is26 && Wyrm39ArenaStale(cfg.Str("design.n26.bosses.wyrm26.arena")))
                {
                    enemies.all.Remove(wyrm24.body);
                    world.SetLayer("wyrm", null);
                    wyrm24 = null;
                    wyrm24Arena = null;
                    if (Wyrm24Ensure() == null) return;
                }
            }
            if (w46WalkHere)
            {
                if (!wyrm24Origin.Equals(w46WalkOrigin)) { Wyrm46MoveRoom(w46WalkOrigin); w46Moved = true; }
            }
            else if (w46Moved)
            {
                Wyrm46MoveRoom(w46FarOrigin);
                w46Moved = false;
            }
        }

        double Wyrm46Num(string key)
        {
            if (Probe46Wyrm != null && Probe46Wyrm.TryGetValue(key, out double v)) return v;
            return cfg.Has(Wyrm46Root + key) ? cfg.Num(Wyrm46Root + key) : double.NaN;
        }

        partial void N46WyrmStartRun()
        {
            if (w46For != null)
            {
                w46For.p.staggerSeconds = w46StaggerWas;
                w46For.open32 = w46OpenWas;
                w46For.p.gunScale = w46GunWas;
                w46For.p.aggroCooldown = w46AggroWas;
                if (wyrm24 != null && wyrm24.aggroCool31 >= 0) wyrm24.aggroCool31 = w46AggroWas;
                w46For = null;
            }
            w46Body = -1;
            bool on = rows.On(idWyrm46);
            w46Stagger = on ? Wyrm46Num("staggerSeconds") : double.NaN;
            w46Open = on ? Wyrm46Num("open") : double.NaN;
            w46Gun = on ? Wyrm46Num("gunScale") : double.NaN;
            w46Aggro = on ? Wyrm46Num("aggroCooldown") : double.NaN;
            w46Pool = on ? Wyrm46Num("poolScale") : double.NaN;
            w46Keep = on ? Wyrm46Num("retryKeep") : double.NaN;
            if (w46AggroFor != null) { w46AggroFor.p.aggroSpeed = w46AggroSpeedWas; w46AggroFor = null; }
            w46AggroP3 = on ? Wyrm46Num("aggroSpeedP3") : double.NaN;
            w46Retries = wyrm24Retries;
            w46LastHp = 0;

            // walkin46 (walkIn46 itself is reset to false by Night46StartRun)
            w46WalkHere = false;
            w46Checked = false;
            if (rows.On(idWalkin46) && Wyrm46ArenaOrigin(Wyrm39LevelArena(), out Vec3 o))
            {
                w46WalkOrigin = o;
                w46WalkHere = true;
                walkIn46 = true;
            }
            if (w46Moved && !w46WalkHere) // a past run moved the room: this one puts it back (Wyrm46WalkStep, before any fight)
            {
                double[] far = cfg.Nums("design.n24.wyrm.room.origin");
                w46FarOrigin = new Vec3(far[0], far[1], far[2]);
            }
        }

        partial void N46WyrmStep(double dt)
        {
            if (w46WalkHere || w46Moved) Wyrm46WalkStep();
            if (!rows.On(idWyrm46)) return;
            if (wyrm31 != null)
            {
                if (w46For != wyrm31)
                {
                    w46For = wyrm31;
                    w46StaggerWas = wyrm31.p.staggerSeconds;
                    w46OpenWas = wyrm31.open32;
                    w46GunWas = wyrm31.p.gunScale;
                    w46AggroWas = wyrm31.p.aggroCooldown;
                }
                // every step: NIGHT32's Set32 (when its row toggles) would put its own values back
                if (!double.IsNaN(w46Stagger)) wyrm31.p.staggerSeconds = w46Stagger;
                if (!double.IsNaN(w46Open)) wyrm31.open32 = w46Open;
                if (!double.IsNaN(w46Gun)) wyrm31.p.gunScale = w46Gun;
                if (!double.IsNaN(w46Aggro))
                {
                    wyrm31.p.aggroCooldown = w46Aggro;
                    if (wyrm24 != null && wyrm24.aggroCool31 >= 0) wyrm24.aggroCool31 = w46Aggro; // the hooked worm's copy (SurvivalSim.Boss31.cs)
                }
            }
            // the pool: once a fight, in the step the body first lives at full health (NIGHT39's boss39 has scaled it earlier in this step)
            if (!double.IsNaN(w46Pool) && wyrm24 != null && wyrm24.Alive && wyrm24.body.id != w46Body)
            {
                w46Body = wyrm24.body.id;
                if (wyrm24.body.hp >= wyrm24.body.maxHp)
                {
                    wyrm24.body.maxHp *= w46Pool;
                    wyrm24.body.hp = wyrm24.body.maxHp;
                    if (wyrm31 != null && wyrm31.on32) wyrm31.maxHp32 = wyrm24.body.maxHp;
                    wyrm46Pools++;
                }
            }
            // the retry (retryKeep): a death puts the worm back at its phase start (NIGHT25: SurvivalSim.Wyrm24.cs Wyrm24BeginCore ->
            // Wyrm.RestorePhaseStart; in phase 1 a NewRun at full health). With retryKeep k the bar keeps k of what the player took off it
            // since that start (1: the bar as it was at the death; 0: NIGHT25's rule). The leg / phase stays the one the bar is in.
            if (!double.IsNaN(w46AggroP3)) Wyrm46AggroP3();
            if (!double.IsNaN(w46Keep) && wyrm24 != null)
            {
                if (wyrm24Retries != w46Retries)
                {
                    w46Retries = wyrm24Retries;
                    if (wyrm24.Alive && w46LastHp > 0 && wyrm24.body.hp > w46LastHp)
                    {
                        wyrm24.body.hp -= w46Keep * (wyrm24.body.hp - w46LastHp);
                        wyrm46RetriesKept++;
                    }
                }
                if (wyrm24.Alive) w46LastHp = wyrm24.body.hp;
            }
        }

        // aggroSpeedP3: the head's charge speed in phase 3 x this (the charge is the punch window: Wyrm.p.aggroSpeed, read where the aggro dives)
        double w46AggroP3 = double.NaN, w46AggroSpeedWas;
        Wyrm w46AggroFor;

        void Wyrm46AggroP3()
        {
            if (wyrm24 == null) return;
            if (w46AggroFor != wyrm24) { w46AggroFor = wyrm24; w46AggroSpeedWas = wyrm24.p.aggroSpeed; }
            wyrm24.p.aggroSpeed = wyrm24.phase == "p3" ? w46AggroSpeedWas * w46AggroP3 : w46AggroSpeedWas;
        }

        // retryKeep's state: the worm's retry count seen, its bar at the end of the last step, this run's k (NaN: off)
        int w46Retries;
        double w46LastHp, w46Keep = double.NaN;

        /// <summary>The harness's count: retries whose bar wyrm46 kept (this sim's life).</summary>
        public int wyrm46RetriesKept;

        partial void N46WyrmCampaign()
        {
        }
    }
}
