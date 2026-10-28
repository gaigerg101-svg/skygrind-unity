// NIGHT33 builder E (row design.n33.spawns33; DESIGN33 E "Trigger spawns", D-7; GG 2026-10-11: ULTRAKILL-style trigger spawns): the campaign room's waves come
// in on lines the player crosses, each enemy behind a short red-orange spawn tell.
// - triggers33 (design.n33.spawns33.triggers33, keyed by ROOM ID so it reads on n32-* and n33-* levels alike: builder R keeps the ids): per room a list of
//   { "wave": w, "rect": [u0, v0, u1, v1], "nodes": [[u, v], ...], "early": bool } in FRACTIONS of the room's floor rect (RoomDef x0..x1 / z0..z1: a reshaped room
//   keeps its triggers in place). Wave w waits until the player's feet enter the rect; its enemies use the trigger's own spots (each at the nearest nav node,
//   the campaign's minimum distance from the player still applies); `early` lets it come in on top of the living.
// - lavaFirstWave: a room with lava and no trigger for its first wave gets one: the line just past the lava's far edge along the room's spawn -> exit axis, its
//   spots the room's nodes beyond that line, so the room pulls the player across the lava before the fight starts. waitSeconds after the room's entry the wave
//   comes anyway (never a soft-lock).
// - the tell: every campaign spawn is held tellSeconds (whole steps) after the event enemySpawnTell33 {kind, x, y, z, seconds}; SpawnTell33View draws a red-orange
//   column there. A told enemy counts as alive (Campaign24.pending33): the room never clears or moves on under a tell. Entry and retry clear the tells.
// Off (the row off, --off33): every Campaign24 hook stays null: NIGHT32's rooms bit for bit.
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    /// <summary>enemySpawnTell33's payload.</summary>
    public sealed class SpawnTell33Event
    {
        public string kind;
        public double x, y, z, seconds;
    }

    public sealed partial class SurvivalSim
    {
        /// <summary>Harness only (`spawns33`): force the spawns33 bit for every run this process starts (null: the run's own). Never set by the game.</summary>
        public static bool? Force33Spawns;

        const string Spawns33Root = "design.n33.spawns33.";

        sealed class Trig33
        {
            public int wave;
            public bool early, line; // line: the lava rule's half-plane (ax, az, from spawn, past `at`) instead of a rect
            public double x0, z0, x1, z1, ax, az, sx, sz, at;
            public List<Vec3> nodes;
            public bool Inside(Vec3 f) => line ? (f.x - sx) * ax + (f.z - sz) * az >= at : f.x >= x0 && f.x <= x1 && f.z >= z0 && f.z <= z1;
        }

        struct Tell33
        {
            public string kind;
            public Vec3 at;
            public int left;
        }

        bool s33On, s33Hooked;
        Campaign24 s33Campaign;
        int s33TellSteps, s33WaitSteps, s33Room = -1, s33RoomSteps;
        readonly List<Trig33> s33Trigs = new List<Trig33>();
        readonly List<bool> s33Fired = new List<bool>();
        readonly List<Tell33> s33Tells = new List<Tell33>();
        readonly SpawnTell33Event s33Ev = new SpawnTell33Event();

        /// <summary>The harness's counts this run: tells shown; waves that waited for a line; waves the wait timer let in.</summary>
        public int spawns33Tells, spawns33Waited, spawns33TimedOut, spawns33Moved;
        /// <summary>The harness's view of the current room's first wave: "no line" (none: at once), "waiting", "line" (crossed), "timer" (waitSeconds ran out).</summary>
        public string spawns33FirstHow = "no line";

        partial void N33SpawnsStartRun()
        {
            if (Force33Spawns.HasValue) n33.spawns33 = Force33Spawns.Value;
            s33On = n33.spawns33;
            s33Tells.Clear();
            s33Trigs.Clear();
            s33Fired.Clear();
            s33Room = -1;
            s33RetryInstant = false;
            s33Campaign = null;
            spawns33Tells = spawns33Waited = spawns33TimedOut = 0;
            if (!s33On) return;
            s33TellSteps = System.Math.Max(1, MovementMath.StepsFor(cfg.Num(Spawns33Root + "tellSeconds"), step));
            s33WaitSteps = MovementMath.StepsFor(cfg.Num(Spawns33Root + "waitSeconds"), step);
            s33Stagger = cfg.Design("n33.spawns33.tellStagger");
            if (!s33Hooked)
            {
                s33Hooked = true; // only ever subscribed with the row on
                events.On<RoomEvent>("roomEnter", e => { Spawns33Room(e.room); s33RetryInstant = false; });
                events.On<SpawnEvent>("enemySpawn", e => s33LastSpawn = e.id); // the tell's stagger finds the body it just placed
                events.On<RoomEvent>("roomRetry", e => { Spawns33Room(e.room); s33RetryInstant = true; });
            }
        }

        /// <summary>Hook (or unhook) the run's campaign: it is made on the run's first live step.</summary>
        void Spawns33Hook()
        {
            if (campaign == s33Campaign) return;
            s33Campaign = campaign;
            if (campaign == null) return;
            if (!s33On)
            {
                campaign.waveReady33 = campaign.waveEarly33 = null;
                campaign.waveNodes33 = null;
                campaign.spawn33 = null;
                campaign.pending33 = null;
                return;
            }
            campaign.waveReady33 = Spawns33Ready;
            campaign.waveEarly33 = (r, w) => Spawns33Trig(w) is Trig33 t && t.early && s33Fired[s33Trigs.IndexOf(t)];
            campaign.waveNodes33 = (r, w) => Spawns33Trig(w) is Trig33 t && t.nodes != null && t.nodes.Count > 0 ? t.nodes : null;
            campaign.spawn33 = Spawns33Tell;
            campaign.pending33 = null; // round 4: placed at once (held by a stagger): nothing pending
            if (campaign.room >= 0 && campaign.room != s33Room) Spawns33Room(campaign.room);
        }

        int s33IdleSteps;

        /// <summary>Harness only: the longest spell any run in this process sat in a fighting room with waves left, nothing alive and nothing told (s), and where.</summary>
        public static double Spawns33IdleMax;
        public static string Spawns33IdleWhere = "";

        /// <summary>Does a lava rect lie on the straight line from a to b (sampled every 0.5 m)?</summary>
        bool Spawns33LavaBetween(Vec3 a, Vec3 b)
        {
            if (lava30 == null || lava30.zones.Count == 0) return false;
            double len = JsMath.Hypot(b.x - a.x, b.z - a.z);
            int n = System.Math.Max(1, (int)System.Math.Ceiling(len / 0.5));
            for (int s = 0; s <= n; s++)
            {
                double t = (double)s / n, x = a.x + (b.x - a.x) * t, z = a.z + (b.z - a.z) * t;
                foreach (Lava30Zone zn in lava30.zones) if (x >= zn.x0 && x <= zn.x1 && z >= zn.z0 && z <= zn.z1) return true;
            }
            return false;
        }

        bool Spawns33AnyAlive()
        {
            List<Enemy> all = enemies.all;
            for (int i = 0; i < all.Count; i++) if (all[i].alive) return true;
            return false;
        }

        Trig33 Spawns33Trig(int w)
        {
            for (int i = 0; i < s33Trigs.Count; i++) if (s33Trigs[i].wave == w) return s33Trigs[i];
            return null;
        }

        bool Spawns33Ready(RoomDef r, int w)
        {
            Trig33 t = Spawns33Trig(w);
            if (t == null) return true;
            int i = s33Trigs.IndexOf(t);
            if (s33Fired[i]) return true;
            if (s33IdleSteps >= s33WaitSteps) // ANY held wave: the room has sat with nothing alive and nothing told for waitSeconds: it comes now (never a soft-lock, every path)
            {
                s33Fired[i] = true;
                spawns33TimedOut++;
                if (w == 0) spawns33FirstHow = "timer";
                return true;
            }
            return false;
        }

        bool s33RetryInstant;

        bool Spawns33Tell(string kind, Vec3 at)
        {
            // round 4: a RETRIED room starts as it was (its first wave at once, no tell): a tell after every retry kept the walk bot in a death loop (f5b seed 7: 166 lava deaths)
            if (s33RetryInstant && campaign != null) return campaign.SpawnNow33(kind, at);
            // never on top of the player: a spot nearer than minSpawnDist (the campaign's own pick falls back to any node on its last try) moves to the room's
            // nearest node that is far enough (deterministic: the room's node order, no draw)
            Vec3 f = mover.position;
            double min = cfg.Num(Spawns33Root + "minSpawnDist");
            if (JsMath.Hypot(at.x - f.x, at.z - f.z) < min && campaignLevel != null && s33Room >= 0 && s33Room < campaignLevel.rooms.Count)
            {
                double best = double.PositiveInfinity;
                Vec3 pick = at;
                foreach (Vec3 n in campaignLevel.rooms[s33Room].nodes)
                {
                    double fromMe = JsMath.Hypot(n.x - f.x, n.z - f.z);
                    if (fromMe < min || Spawns33LavaBetween(at, n)) continue; // round 4: never across lava from the campaign's own pick (f5b: a spitter moved over the lava drew the walk bot into it, 166 deaths)
                    double d = JsMath.Hypot(n.x - at.x, n.z - at.z);
                    if (d < best) { best = d; pick = n; }
                }
                Vec3 was = at;
                at =best < double.PositiveInfinity ? pick : was; // none far enough on this side: the campaign's own pick stands
                if (at.x != was.x || at.z != was.z) spawns33Moved++;
            }
            // round 4 (the lead 12:09, R's walk bot): the enemy is placed AT ONCE (alive and counted, as NIGHT32's rooms; an empty room under a tell sent the walk bot
            // into f5b's lava 166 times) but held still for the tell (a stagger of tellSeconds: it cannot move or attack) inside the red-orange column
            s33LastSpawn = -1;
            if (!campaign.SpawnNow33(kind, at)) return false;
            Enemy e = null;
            for (int i = enemies.all.Count - 1; i >= 0 && e == null; i--) if (enemies.all[i].id == s33LastSpawn) e = enemies.all[i];
            if (e != null)
            {
                if (s33Stagger) e.Stun(s33TellSteps * step, step); // design.n33.spawns33.tellStagger (off: the column only, NIGHT32's timing)
                at = e.position;
            }
            spawns33Tells++;
            s33Ev.kind = kind;
            s33Ev.x = at.x;
            s33Ev.y = at.y;
            s33Ev.z = at.z;
            s33Ev.seconds = s33TellSteps * step;
            events.Emit("enemySpawnTell33", s33Ev);
            return true;
        }

        int s33LastSpawn = -1;
        bool s33Stagger;

        /// <summary>A lesson other than HusksDead (a reach, a jump, a verb, an unlock): the room's crossing may hang on it, so it never holds a wave.</summary>
        static bool Spawns33CrossingLesson(RoomDef r)
        {
            foreach (LessonDef l in r.lessons) if (l.kind != LessonKind.HusksDead) return true;
            if (r.lessons31 != null) foreach (LessonDef l in r.lessons31) if (l.kind != LessonKind.HusksDead) return true;
            return false;
        }


        /// <summary>A room entered (or retried): its triggers from the config and the lava rule, no tell left over.</summary>
        void Spawns33Room(int room)
        {
            s33Tells.Clear();
            s33Trigs.Clear();
            s33Fired.Clear();
            s33Room = room;
            s33RoomSteps = 0;
            s33IdleSteps = 0;
            spawns33FirstHow = "no line";
            if (!s33On || campaignLevel == null || room < 0 || room >= campaignLevel.rooms.Count) return;
            RoomDef r = campaignLevel.rooms[room];
            if (r.kind == "boss" || r.WaveCount == 0) return;
            if (Spawns33CrossingLesson(r)) return; // the lead 12:09: a room with a lesson other than "kill them all" (a reach, an unlock, a verb) never holds a wave (i2: the PULSE launch unlocks after five kills)
            string key = Spawns33Root + "triggers33." + r.id;
            if (cfg.Has(key))
            {
                foreach (object o in PortData.Arr(cfg.Get(key)))
                {
                    var d = PortData.Obj(o);
                    var rc = PortData.Arr(d["rect"]);
                    var t = new Trig33
                    {
                        wave = (int)PortData.Num(d["wave"]),
                        early = d.TryGetValue("early", out object eo) && eo is bool eb && eb,
                        x0 = RoomU(r, PortData.Num(rc[0])), z0 = RoomV(r, PortData.Num(rc[1])), x1 = RoomU(r, PortData.Num(rc[2])), z1 = RoomV(r, PortData.Num(rc[3])),
                    };
                    if (t.x0 > t.x1) (t.x0, t.x1) = (t.x1, t.x0);
                    if (t.z0 > t.z1) (t.z0, t.z1) = (t.z1, t.z0);
                    if (d.TryGetValue("nodes", out object no) && no != null)
                    {
                        t.nodes = new List<Vec3>();
                        foreach (object nn in PortData.Arr(no))
                        {
                            var uv = PortData.Arr(nn);
                            double x = RoomU(r, PortData.Num(uv[0])), z = RoomV(r, PortData.Num(uv[1]));
                            int n = nav.grid.NearestNode(x, r.spawn.y, z, 3, 0.25);
                            if (n >= 0) t.nodes.Add(new Vec3(nav.grid.NodeX(n), nav.grid.y[n], nav.grid.NodeZ(n)));
                        }
                    }
                    if (t.wave >= 0 && t.wave < r.WaveCount) s33Trigs.Add(t);
                }
            }
            if (Spawns33Trig(0) == null && cfg.Design("n33.spawns33.lavaFirstWave") && lava30 != null) Spawns33LavaLine(r);
            for (int i = 0; i < s33Trigs.Count; i++) s33Fired.Add(false);
            if (Spawns33Trig(0) != null) spawns33FirstHow = "waiting";
        }

        static double RoomU(RoomDef r, double u) => r.x0 + u * (r.x1 - r.x0);
        static double RoomV(RoomDef r, double v) => r.z0 + v * (r.z1 - r.z0);

        /// <summary>The lava rule: wave 0 waits for the line just past the room's lava (its far edge along spawn -> exit); its spots are the room's nodes past the line.</summary>
        void Spawns33LavaLine(RoomDef r)
        {
            double ax = r.exit.x - r.spawn.x, az = r.exit.z - r.spawn.z, len = JsMath.Hypot(ax, az);
            if (len < 1) return;
            ax /= len;
            az /= len;
            double far = double.NegativeInfinity;
            foreach (Lava30Zone z in lava30.zones)
            {
                if (z.room != r.id) continue;
                bool across = false; // only lava across the way (the spawn -> exit line passes over it, 1 m slack): a side gutter pulls nobody across
                for (double s = 0; s <= len && !across; s += 0.5)
                {
                    double px = r.spawn.x + ax * s, pz = r.spawn.z + az * s;
                    across = px >= z.x0 - 1 && px <= z.x1 + 1 && pz >= z.z0 - 1 && pz <= z.z1 + 1;
                }
                if (!across) continue;
                foreach (double cx in new[] { z.x0, z.x1 })
                    foreach (double cz in new[] { z.z0, z.z1 })
                        far = System.Math.Max(far, (cx - r.spawn.x) * ax + (cz - r.spawn.z) * az);
            }
            if (double.IsNegativeInfinity(far) || far >= len) return; // no lava here, or none before the exit
            double at = far + cfg.Num(Spawns33Root + "lineBeyond");
            var t = new Trig33 { wave = 0, line = true, ax = ax, az = az, sx = r.spawn.x, sz = r.spawn.z, at = at, nodes = new List<Vec3>() };
            foreach (Vec3 n in r.nodes) if ((n.x - r.spawn.x) * ax + (n.z - r.spawn.z) * az > at) t.nodes.Add(n);
            s33Trigs.Add(t);
        }

        partial void N33SpawnsStep(double dt)
        {
            if (!s33On) return;
            Spawns33Hook();
            if (campaign == null) return;
            s33RoomSteps++;
            if (s33RetryInstant && campaign.Wave33 > 0) s33RetryInstant = false; // the retried room's first wave is in
            // the hold watchdog: a fighting room with waves left, nothing alive and nothing told counts idle steps; Spawns33Ready releases any held wave
            // at waitSeconds (the harness's assertion reads the longest spell: Spawns33IdleMax)
            int cr = campaign.room;
            bool idle = cr >= 0 && campaign.phase == Campaign24.Phase.Fight && campaignLevel != null && cr < campaignLevel.rooms.Count
                && campaignLevel.rooms[cr].kind != "boss" && campaign.Wave33 < campaignLevel.rooms[cr].WaveCount && s33Tells.Count == 0 && !Spawns33AnyAlive();
            s33IdleSteps = idle ? s33IdleSteps + 1 : 0;
            if (idle && s33IdleSteps * step > Spawns33IdleMax)
            {
                Spawns33IdleMax = s33IdleSteps * step;
                Spawns33IdleWhere = campaignLevel.rooms[cr].id + " wave " + campaign.Wave33;
            }
            Vec3 f = mover.position;
            for (int i = 0; i < s33Trigs.Count; i++)
                if (!s33Fired[i] && s33Trigs[i].Inside(f))
                {
                    s33Fired[i] = true;
                    spawns33Waited++;
                    if (s33Trigs[i].wave == 0) spawns33FirstHow = "line";
                }
            for (int i = 0; i < s33Tells.Count; i++)
            {
                Tell33 t = s33Tells[i];
                if (--t.left > 0) { s33Tells[i] = t; continue; }
                s33Tells.RemoveAt(i--);
                campaign.SpawnNow33(t.kind, t.at);
            }
        }
    }
}
