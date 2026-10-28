// NIGHT65 builder T (the lead's ask at 1:35): the VERB-AWARE pilot, so the flow meter and the skip test measure a player who uses the room's
// verb. Harness-only (no Assets code): a wrapper round n46flow's pilot (F1Input: Walk38Pilot + the lava rule), switched on by `--verbs65`
// (n46flow, n65verb skip) and only in a room whose level row carries `verb65`; everywhere else it returns the inner pilot's frames untouched.
// Each step the inner pilot runs first (its state stays its own), then, while the room's verb trigger holds, an EPISODE takes the move and
// the combat input:
//   launch  a husk-shaped body (not a stand-in or a tank) within 5.5 m flat and 50 deg of the look: dash at it (slide with verb65 "launch+slide"),
//           punch inside 0.9 x punch.range while dashing / sliding or in launch36's grace (0.5 s at most, then a 1 s rest)
//   pierce  two or more live bodies on one line from the eye through a body in clear sight (30 m), each one the shot's own ray hits (NIGHT66
//           T1b amendment: Enemy.RayHit, the charged shot's reach; NIGHT65's rule took any body within 2 m of the line): the KESTREL in hand, held on
//           that body to charge35Full, released 2 steps later (4 s at most, then a 1 s rest)
//   ride    a ride face (the level's `ride36` boxes in the room) within 1.6 m of the feet on its side and inside its length, the feet in the air
//           (or on the ground within 2.5 m: a jump first): jump held, the stick along the face toward its far end, leaning in, until the
//           ride ends or 1.5 m from the end (3 s without a ride ends it)
//   parry   a spitter's glob, not yet reflected, inside 0.9 x orb.reflectRange of the eye: look at it, punch
//   slide   a tunnel (the level's `tunnels`) with a 1.0-1.35 m ceiling within 2.5 m ahead, or the feet in it: slide held, forward
// Any other verb65 is refused (counted, the inner pilot runs). Counters per room: fight steps, trigger steps, episodes, the verb performed
// (launch36 events, charged shots, WallRideStart, reflected globs, slides), refused steps. Bot numbers only, never gameplay.
// NIGHT66 T (T1b, skip v2's refused run; FORGE66 redteam.md finding 3): `refuse = true` keeps the same triggers and the same episodes, but
// each episode DECLINES the verb (one press refused, counted in refusedEpisodes) and releases everything that only serves it, so the run
// plays on as the walker does instead of standing to charge or dashing at a body it will not punch:
//   launch  the look stays on the episode's husk (the gun's aim), the inner pilot's move and fire run (no dash, no slide, no punch, no fire
//           held off); the episode ends on the husk's death or 0.8 s, as allowed
//   pierce  the look stays on the line's body for at most V65RefusedAimS (1.0 s) or its death, the inner pilot's move and fire run (no hold
//           to charge, no switch to the KESTREL, no forced release)
//   ride, parry, slide   nothing of the episode is played (every input of those episodes serves the verb only): counted, then the 1 s rest
// The press dropping of the inner pilot's own verb input is V65Filter's, wrapped round this pilot (Harness.N65.Verb.cs, skip v2).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    /// <summary>`--verbs65` on (n46flow); the planted rows (`--plant-verb65 a2:pierce,...`, also bot43 false) applied to every load.</summary>
    static bool V65Verbs;
    static readonly Dictionary<string, string> V65Plants = new Dictionary<string, string>();

    static void V65ParsePlants(string arg)
    {
        V65Plants.Clear();
        if (string.IsNullOrEmpty(arg)) return;
        foreach (string kv in arg.Split(','))
        {
            int c = kv.IndexOf(':');
            if (c > 0) V65Plants[kv.Substring(0, c)] = kv.Substring(c + 1);
        }
    }

    /// <summary>The level JSON with the planted rows: each planted room's row gets verb65 and bot43 false (the generic path, as a rebuilt row).
    /// No plant: the same dictionary untouched.</summary>
    static Dictionary<string, object> V65Planted(Dictionary<string, object> json)
    {
        if (V65Plants.Count == 0 && V68Only.Count == 0 && V68Limit.Count == 0) return json;
        foreach (object o in PortData.Arr(PortData.Obj(json["campaign"])["rooms"]))
        {
            var r = PortData.Obj(o);
            if (V65Plants.TryGetValue((string)r["id"], out string v)) { r["verb65"] = v; r["bot43"] = false; }
            // NIGHT68 I2: `--plant-only <room>:<kind>`, every body of the room's waves made that kind (in memory; the planted swarmer-only room)
            if (V68Only.TryGetValue((string)r["id"], out string k) && r.TryGetValue("waves", out object wo))
                foreach (object w in PortData.Arr(wo)) { var l = PortData.Arr(w); for (int i = 0; i < l.Count; i++) l[i] = k; }
            // NIGHT68 g7: `--plant-limit <room>:<flag>`, the room's row given the game's own `limit` field (Campaign24: meleeOnly puts the
            // gauntlet alone in hand on entry) in memory, as builder R's g7 card asks of the room file
            if (V68Limit.TryGetValue((string)r["id"], out string lim)) r["limit"] = new List<object> { lim };
        }
        return json;
    }

    /// <summary>NIGHT68 g7: `--plant-limit g7:meleeOnly,...` (n46flow, n65verb skip): the rooms given a `limit` row field in memory.</summary>
    static readonly Dictionary<string, string> V68Limit = new Dictionary<string, string>();

    static void V68ParseLimit(string arg)
    {
        V68Limit.Clear();
        if (string.IsNullOrEmpty(arg)) return;
        foreach (string kv in arg.Split(','))
        {
            int c = kv.IndexOf(':');
            if (c > 0) V68Limit[kv.Substring(0, c)] = kv.Substring(c + 1);
        }
    }

    /// <summary>NIGHT68 I2: `--plant-only a5:swarmer,...` (n46flow, n65verb skip): the rooms whose waves are planted as one kind.</summary>
    static readonly Dictionary<string, string> V68Only = new Dictionary<string, string>();

    static void V68ParseOnly(string arg)
    {
        V68Only.Clear();
        if (string.IsNullOrEmpty(arg)) return;
        foreach (string kv in arg.Split(','))
        {
            int c = kv.IndexOf(':');
            if (c > 0) V68Only[kv.Substring(0, c)] = kv.Substring(c + 1);
        }
    }

    /// <summary>NIGHT68 I2: the launch targets (kind and hp at the trigger) of a room's launch episodes, "kind hp x n" by count.</summary>
    static string V68Targets(Dictionary<string, int> t) => t.Count == 0 ? "none" : string.Join(", ", t.OrderByDescending(kv => kv.Value).ThenBy(kv => kv.Key).Select(kv => kv.Key + " x" + kv.Value));

    sealed class V65Stat
    {
        public long fight, trigger, refused; public int episodes, launches, launchesRoom, charged, rides, parries, slides, seeds, dashes, punches, dashStates; public string verb;
        // NIGHT68 I2: the launch episodes' targets (kind and hp at the trigger), the episodes' punches that landed and that killed their body
        // (the HitEvent of weapon "punch" in the episode's window: launch36 throws only a body the punch leaves alive), and the steps a body
        // passed every launch test but the punch would kill it (hp at or under the bound: no launch target)
        public Dictionary<string, int> targets = new Dictionary<string, int>();
        public int punchHits, punchKills, punchParries; public long skipped; public double bound; public Dictionary<string, int> skippedKinds = new Dictionary<string, int>();
        // NIGHT68 I1: why the verb was refused (null: an unknown verb), the line face the ride episodes use, the wall contacts in the air
        // where the noride plant blocked a ride start outside the line episode (off-line rides cut)
        public string refusedWhy, lineFace; public int offlineCut;
        public int swingParries; // NIGHT68 g7: the parry episodes' presses made with the glove's swing (the gauntlet in hand)
        public int parryHeld; // NIGHT68 I2 fix round: the launch episodes' steps the punch was held because it would be a parry
        // NIGHT70 Z3 (the committed run): the steps its fire was held (a launchable body inside 12 m, no episode running), the steps it closed
        // on that body, and the gun's shots ("fire" / "chargeShot27Fire" events) in a held step (the proof line: must be 0)
        public long commitHeld, commitClose, commitKept, commitSwitch; public int commitBad;
    }
    static readonly Dictionary<string, V65Stat> V65Stats = new Dictionary<string, V65Stat>();

    /// <summary>NIGHT70 Z3 round 4: `--commit-burst-cap` (skip v2): the committed run holds only for bodies its launch ends (hp at or under
    /// launch36's burst damage); off by default (the slate's literal rule: every husk-shaped body above the launch bound).</summary>
    static bool V70CommitBurstCap;

    static IPlayerInput V65Wrap(SurvivalSim sim, IPlayerInput inner, Dictionary<string, object> json) => V65Verbs ? new V65VerbPilot(sim, inner, json) : inner;

    static void V65Report(string id, Action<string> line)
    {
        if (!V65Verbs) return;
        line($"n46flow {id} VERBS65 (--verbs65: the verb-aware pilot in rooms with verb65{(V65Plants.Count > 0 ? "; planted " + string.Join(", ", V65Plants.Select(p => p.Key + ":" + p.Value)) + " with bot43 false" : "")}):");
        if (V65Stats.Count == 0) line("  no room with verb65 was entered");
        foreach (var kv in V65Stats)
        {
            V65Stat s = kv.Value;
            string done = s.verb.StartsWith("launch") ? $"launch36 {s.launches} after its punches (in the room {s.launchesRoom}; dashes {s.dashes}, steps at speed {s.dashStates}, punches {s.punches}) | {V68LaunchLine(s)}" : s.verb == "pierce" ? $"charged shots {s.charged}" : s.verb == "ride" ? $"wall rides {s.rides} (the face: {s.lineFace ?? "-"}; off-line ride starts cut {s.offlineCut} wall contacts)" : s.verb == "parry" ? $"parries {s.parries} (swing presses with the glove in hand {s.swingParries})" : s.verb == "slide" ? $"slides {s.slides}" : "-";
            line($"  {kv.Key,-6} verb65 {s.verb,-12} | trigger held {s.trigger} of {s.fight} fight steps ({(s.fight > 0 ? 100.0 * s.trigger / s.fight : 0).ToString("0.0", CultureInfo.InvariantCulture)} %), episodes {s.episodes}, performed: {done}{(s.refused > 0 ? $" | REFUSED: {s.refusedWhy ?? $"verb65 '{s.verb}' is not a verb this pilot performs"} ({s.refused} steps; the inner pilot ran)" : "")}");
        }
    }

    /// <summary>NIGHT68 I2: a launch room's target line (V65Report, skip v2's per-seed lines): the targets, the punches that landed and killed,
    /// and `no launch target` when no episode started while bodies in reach were skipped (hp at or under the bound).</summary>
    static string V68LaunchLine(V65Stat s) =>
        $"launch targets (kind hp at the trigger): {V68Targets(s.targets)}; episode punches landed {s.punchHits}, killed their body {s.punchKills}, parries {s.punchParries} (a parry never launches), punch held {s.parryHeld} steps while it would be a parry"
        + (s.bound > 0 ? $"; bound {s.bound.ToString("0.##", CultureInfo.InvariantCulture)} (the largest of the runs': max(punch.damage x momentum.meleeMax, the largest non-parry punch dealt in the run)): skipped {s.skipped} steps with the nearest body in reach at or under it ({V68Targets(s.skippedKinds)})" : "; no hp bound (the t0 rule)")
        + (s.episodes == 0 && s.skipped > 0 ? " | no launch target" : "");

    /// <summary>The verb-aware pilot (see the file's head).</summary>
    sealed class V65VerbPilot : IPlayerInput
    {
        readonly SurvivalSim sim;
        readonly IPlayerInput inner;
        readonly Dictionary<string, string> verbs = new Dictionary<string, string>();
        readonly List<double[]> rides = new List<double[]>(); // [axis 0 = x-plane / 1 = z-plane, plane, normal sign, lo, hi, top, zone index]
        readonly List<string> rideZones = new List<string>();
        readonly List<double[]> tunnels = new List<double[]>(); // [x0, z0, x1, z1, ceiling]
        double yaw, pitch;
        string ep; // the episode running ("launch", "pierce", "ride", "parry", "slide") or null
        int epSteps, rest, slot = -1;
        Enemy target;
        int fullAt = -1, fulls;
        bool wantPunch, firing, release, jumpHeld, jumpPress, slideHeld, wasRiding;
        bool swingParry68; // NIGHT68 g7: this parry episode presses the glove's swing (the gauntlet in hand), not the punch
        double alongX, alongZ, nX, nZ, endX, endZ;
        public V65Stat stat;
        string lastRoom;
        /// <summary>NIGHT66 T1b: the refused mode (see the file's head) and its count of declined episodes (one verb press refused each).</summary>
        public bool refuse;
        public int refusedEpisodes;
        /// <summary>NIGHT70 Z3 (slate Z3; DECIDE70 D-5): THE COMMITTED RUN, in a launch room only (verb65 launch / launch+slide, not refused):
        /// while no launch episode runs and a launchable body (husk-shaped: not a stand-in or a tank; hp above the launch bound and, round 2, at
        /// or under launch36's burst damage: a body its launch ends) is inside V70CommitM (12 m flat) of the feet, (a) the fire is held (no shot,
        /// no glove swing: c.fire up; round 3: a pistol / shotgun charge already building is kept held and dropped by a switch to the other gun,
        /// as is a tap queued before the hold, so nothing fires), and (b) the pilot closes on the
        /// nearest such body (the look on it, the stick forward) so a launch episode can start on it; the episode trigger then reads the
        /// angle against the look it was given (the inner pilot's look otherwise). Everything else is the plain verb-aware run. Off (false):
        /// the plain run, unchanged.</summary>
        public bool commit;
        const double V70CommitM = 12.0; // m: the committed run's hold-and-close radius (the slate's Z3 doneWhen; probe number, not gameplay)
        double closeYaw70 = double.NaN; // the look the close step gave (NaN: no close last step)
        bool held70; // this step's fire was held (the proof counts a shot in such a step)
        // NIGHT70 Z3 round 2 (the planted pair, kb3 seeds 1-3: holding fire on chargers, hp 42-150, made the committed run 57-79 s against the plain
        // run's 20-40 s, 33-46 launches a run): a body is held for only when its launch ENDS it: hp above the launch bound AND at or under the
        // throw's own burst (launch36 burstDamage, 60: the flier dies in its own burst, Launch36.cs:211-229); a charger above it (knockbackScale
        // 0.3: a 3.8 m flight it survives) is shot as in the plain run
        // round 4 (the lead: the slate's Z3 doneWhen is literal, and no rule is loosened after a measurement): the DEFAULT holds for every
        // husk-shaped body above the launch bound inside 12 m, chargers included; the round-2 cap (hp at or under launch36's burst) is the
        // option `--commit-burst-cap`, off by default (V70CommitBurstCap), named in skip v2's runs line
        Enemy CommitTarget70(RoomDef rd, Vec3 p) { double bound = LaunchBound; Launch36 lc = sim.launch36Core; if (lc == null) return null; double burst = lc.burstDamage; bool cap = V70CommitBurstCap; return Alive(rd).Where(e => e is Husk && !e.standIn29 && !e.tank29 && e.hp > bound && (!cap || e.hp <= burst) && Flat(e.position, p) <= V70CommitM).OrderBy(e => Flat(e.position, p)).FirstOrDefault(); }
        bool lastFireHeld70; // the fire this pilot gave last step (the gun's trigger state)
        bool CommitOn70(bool active, out RoomDef rd)
        {
            rd = null;
            if (!commit || refuse || !active || ep != null) return false;
            rd = Room();
            return rd != null && sim.campaign.phase == Campaign24.Phase.Fight && verbs.TryGetValue(rd.id, out string v) && v.StartsWith("launch");
        }
        const double V65RefusedAimS = 1.0; // s: a refused pierce episode keeps the look on its body this long at most (probe number, not gameplay)

        public V65VerbPilot(SurvivalSim sim, IPlayerInput inner, Dictionary<string, object> json)
        {
            this.sim = sim;
            this.inner = inner;
            var camp = PortData.Obj(json["campaign"]);
            foreach (object o in PortData.Arr(camp["rooms"]))
            {
                var r = PortData.Obj(o);
                if (r.TryGetValue("verb65", out object v) && v is string s) verbs[(string)r["id"]] = s;
                rows68[(string)r["id"]] = r; // NIGHT68 I1: the rows (line65)
            }
            faces68 = V68Faces(json); // NIGHT68 I1: the same boxes as `rides`, in the same order (indices match)
            skin68 = sim.cfg.movement.wallRide.probeDistance;
            if (camp.TryGetValue("boxes", out object bo))
                foreach (object o in PortData.Arr(bo))
                {
                    var b = PortData.Obj(o);
                    if (!(b.TryGetValue("ride", out object rv) && rv is bool rb && rb) || !b.TryGetValue("ride36", out object r36)) continue;
                    var f = PortData.Obj(r36);
                    string face = f.TryGetValue("face", out object fo) ? (string)fo : "E";
                    var mn = PortData.Arr(b["min"]); var mx = PortData.Arr(b["max"]);
                    double x0 = PortData.Num(mn[0]), z0 = PortData.Num(mn[2]), x1 = PortData.Num(mx[0]), z1 = PortData.Num(mx[2]), top = PortData.Num(mx[1]);
                    if (face == "E") rides.Add(new[] { 0, x1, 1, z0, z1, top });
                    else if (face == "W") rides.Add(new[] { 0, x0, -1, z0, z1, top });
                    else if (face == "S") rides.Add(new[] { 1, z1, 1, x0, x1, top });
                    else rides.Add(new[] { 1, z0, -1, x0, x1, top });
                    rideZones.Add(b.TryGetValue("zone", out object zo) ? (string)zo : "");
                }
            if (camp.TryGetValue("tunnels", out object to))
                foreach (object o in PortData.Arr(to))
                {
                    var t = PortData.Obj(o);
                    var rc = PortData.Arr(t["rect"]);
                    tunnels.Add(new[] { PortData.Num(rc[0]), PortData.Num(rc[1]), PortData.Num(rc[2]), PortData.Num(rc[3]), PortData.Num(t["ceiling"]) });
                }
            sim.events.On<ChargeShot27Event>("charge35Full", e => { fulls++; });
            // the punch and the release land on the sim step after the episode's last input: counted inside a short window
            sim.events.On(Launch36.LaunchEvent, o => { if (stat != null) { stat.launchesRoom++; if (launchWindow > 0) stat.launches++; } });
            // NIGHT68 I2: the episode's punch landed (and killed its body?): the punch's HitEvent inside the launch window; every punch of the run
            // raises the launch bound to the largest it dealt
            // (a parry punch never launches and deals more: shipped kb3's punches that killed 40 hp husks, 43.1-53.3, were all parries, the
            // others 24.4-26.9: so only a non-parry punch raises the bound; the punch's MeleeEvent, emitted before its Damage, says which)
            sim.events.On<MeleeEvent>("punch", e => { lastPunchParry = e.parry; if (stat != null && launchWindow > 0 && e.parry) stat.punchParries++; });
            sim.events.On<HitEvent>("hit", e =>
            {
                if (e.weapon != "punch") return;
                if (!lastPunchParry && e.damage > maxPunch) maxPunch = e.damage;
                if (stat != null && launchWindow > 0) { stat.punchHits++; if (e.killed) stat.punchKills++; }
            });
            launchBase = sim.cfg.Num("punch.damage") * sim.cfg.Num("momentum.meleeMax");
            sim.events.On<ChargeShot27Event>("chargeShot27Fire", e => { if (stat != null && (ep == "pierce" || releaseLeft > 0 || pierceWindow > 0)) stat.charged++; });
            sim.mover.Events += e => { if (e.type == MoverEventType.WallRideStart && stat != null && ep == "ride") stat.rides++; };
            // NIGHT70 Z3: the committed run's proof: a gun shot in a step whose fire was held (only the committed run sets held70)
            sim.events.On("fire", o => { if (held70 && stat != null) stat.commitBad++; });
            sim.events.On<ChargeShot27Event>("chargeShot27Fire", e => { if (held70 && stat != null) stat.commitBad++; });
        }

        // ---- NIGHT68 I1 (code-scout.md (a1), (a2); the slate's I1): the ride episode rides line65's face only, once per pass (disarmed at its
        // start, armed again once the feet are 4 m off the line's run), joined anywhere on the run from 2.5 m before line65's entry to 2 m before its
        // exit (the scout's 2.5 m round the entry alone left kb3c's seed 1 never exercised: the walk never passed it) and ridden toward the exit
        // in the line's direction; a ride room with no line65 on its row
        // keeps the old trigger (any ride36 face of the room, toward its far end) but once per pass too; line65 on no face: REFUSED (said once).
        // Outside its line episode the verb-aware run gives the room the game's noride rule (Mover.SetNoRide38) so no other ride starts (the
        // inner walker's own rides included: code-scout (a2), one pilot in all three runs); during the episode only the episode's face is
        // left out of it. Rooms of other verbs and the rest of the level: the level's own noride boxes, untouched.
        readonly Dictionary<string, Dictionary<string, object>> rows68 = new Dictionary<string, Dictionary<string, object>>();
        readonly Dictionary<string, V68Line> lines68 = new Dictionary<string, V68Line>();
        static readonly HashSet<string> V68Said = new HashSet<string>();
        List<V68Face> faces68;
        double skin68;
        bool armed68 = true;
        double armX68, armZ68;
        int epFace68 = -1;
        Ride38.Box[] own68, set68; // the level's own noride boxes (as the run started), the set this pilot last gave the Mover
        readonly Dictionary<string, Ride38.Box[]> sets68 = new Dictionary<string, Ride38.Box[]>();
        int blocks68 = -1; bool blockedLast68;

        V68Line LineOf(string room)
        {
            if (lines68.TryGetValue(room, out V68Line l)) return l;
            List<double[]> pts = rows68.TryGetValue(room, out var row) ? Tour65.Line(row) : new List<double[]>();
            l = pts.Count == 0 ? null : V68LineFace(pts, faces68, room); // null: no line65 (the old trigger, once per pass)
            lines68[room] = l;
            string say = l == null ? $"no line65 on {room}'s row: the ride pilot takes any ride36 face of the room, once per pass (the fallback)" : l.why != null ? "REFUSED: " + l.why : $"line65's face {faces68[l.face].key}, from {V66R(l.entryS)} toward {V66R(l.exitS)}";
            if (V68Said.Add(room + ":" + say)) Console.WriteLine($"  ride pilot {room}: {say}");
            return l;
        }

        /// <summary>The noride set for this step: in a ride room (not refused) the room under noride, less the episode's face during a ride episode;
        /// elsewhere the level's own. Also counts the wall contacts in the air the set blocked outside an episode (off-line rides cut).</summary>
        void Ride68NoRide(bool active)
        {
            Ride38.Box[] cur = sim.mover.flow34.ride35.noRide;
            if (set68 == null) { if (cur != own68) sets68.Clear(); own68 = cur; } // nothing of this pilot's in place: the run's own set
            else if (cur != set68) { own68 = cur; set68 = null; sets68.Clear(); } // the run replaced this pilot's set (a boss's switch): its own again
            int blocks = sim.mover.flow34.ride35.r38NoRideBlocks;
            bool blocked = blocks68 >= 0 && blocks > blocks68;
            if (blocked && !blockedLast68 && set68 != null && ep != "ride" && stat != null) stat.offlineCut++;
            blockedLast68 = blocked; blocks68 = blocks;
            RoomDef rd = active ? Room() : null;
            Ride38.Box[] want = own68;
            if (!refuse && rd != null && verbs.TryGetValue(rd.id, out string v) && v == "ride")
            {
                V68Face keep = ep == "ride" && epFace68 >= 0 ? faces68[epFace68] : null;
                string key = rd.id + "|" + (keep != null ? epFace68 : -1);
                if (!sets68.TryGetValue(key, out want)) sets68[key] = want = V68NoRide(own68, rd, keep, skin68);
            }
            if (want == own68) { if (set68 != null) { sim.mover.SetNoRide38(own68); set68 = null; } return; }
            if (want != set68) { sim.mover.SetNoRide38(want); set68 = want; }
        }

        public InputFrame SampleMove(bool active)
        {
            InputFrame f = SampleMove65(active);
            Ride68NoRide(active);
            return f;
        }

        public double Yaw => yaw;
        public double Pitch => pitch;
        public void SetLook(double y, double p) { yaw = y; pitch = p; inner.SetLook(y, p); }
        public bool Captured => inner.Captured;

        RoomDef Room()
        {
            Campaign24 c = sim.campaign;
            if (c == null || c.room < 0 || sim.campaignLevel == null || c.room >= sim.campaignLevel.rooms.Count) return null;
            return sim.campaignLevel.rooms[c.room];
        }

        static double Flat(Vec3 a, Vec3 b) => Math.Sqrt((a.x - b.x) * (a.x - b.x) + (a.z - b.z) * (a.z - b.z));
        static double YawTo(Vec3 a, Vec3 b) => Math.Atan2(-(b.x - a.x), -(b.z - a.z));
        static double PitchTo(Vec3 a, Vec3 b) => Math.Atan2(b.y - a.y, Math.Max(1e-6, Flat(a, b)));
        static double AngDiff(double a, double b) { double d = (a - b) % (2 * Math.PI); if (d > Math.PI) d -= 2 * Math.PI; if (d < -Math.PI) d += 2 * Math.PI; return Math.Abs(d); }

        IEnumerable<Enemy> Alive(RoomDef rd) => sim.enemies.all.Where(e => e.alive && e.position.x >= rd.x0 - 1 && e.position.x <= rd.x1 + 1 && e.position.z >= rd.z0 - 1 && e.position.z <= rd.z1 + 1);

        int launchWindow, pierceWindow, releaseLeft;
        bool parryThrown;
        /// <summary>NIGHT68 I2: the launch bound: a launch target's hp must be above max(punch.damage x momentum.meleeMax, the largest non-parry
        /// punch dealt in this run). The punch's damage is x lerp(1, meleeMax, momentum) and then the arsenal's tags, the bonus and vuln26
        /// (Combat.cs:1483-1511). Shipped kb3's dash punches on 40 hp husks (seeds 1-3): 24.4 survived and launched; 48.7 and 43.1 killed, both
        /// parries (a parry never launches: Combat.cs `launch = !parry && Wants`), so a parry's damage never raises the bound.</summary>
        double launchBase, maxPunch;
        bool lastPunchParry;
        double LaunchBound => Math.Max(launchBase, maxPunch);

        bool MeleeBlocked() => sim.loadout.rules30 != null && sim.loadout.rules30.MeleeBlocked(sim.loadout.Weapon == "gauntlet");

        void End() { if (ep == "pierce" && !refuse) { releaseLeft = 2; pierceWindow = 6; } parryThrown = false; ep = null; target = null; epSteps = 0; rest = MovementMath.StepsFor(1.0, sim.step); wantPunch = firing = release = jumpHeld = jumpPress = slideHeld = wasRiding = false; fullAt = -1; }

        InputFrame SampleMove65(bool active)
        {
            InputFrame f = inner.SampleMove(active);
            yaw = f.yaw; pitch = f.pitch;
            wantPunch = false; jumpPress = false;
            RoomDef rd = active ? Room() : null;
            if (rd == null || sim.campaign.phase == Campaign24.Phase.None || !verbs.TryGetValue(rd.id, out string verb)) { if (ep != null) End(); closeYaw70 = double.NaN; return f; }
            if (rd.id != lastRoom)
            {
                lastRoom = rd.id;
                if (!V65Stats.TryGetValue(rd.id, out stat)) V65Stats[rd.id] = stat = new V65Stat { verb = verb };
                stat.seeds++;
                End();
                rest = 0;
            }
            bool fight = sim.campaign.phase == Campaign24.Phase.Fight;
            if (fight) stat.fight++;
            if (rest > 0) rest--;
            Vec3 p = sim.mover.position, eye = sim.Eye;
            bool known = verb == "launch" || verb == "launch+slide" || verb == "pierce" || verb == "ride" || verb == "parry" || verb == "slide";
            if (!known) { if (fight) stat.refused++; return f; }
            // ---- start an episode when the trigger holds
            if (ep == null && rest == 0)
            {
                bool trig = false;
                if (verb.StartsWith("launch"))
                {
                    // NIGHT68 I2: only a body the punch leaves alive (hp above the launch bound); the bodies that pass every other test but not
                    // that one are counted (no launch target)
                    double bound = LaunchBound;
                    stat.bound = Math.Max(stat.bound, bound);
                    double look70 = commit && !double.IsNaN(closeYaw70) ? closeYaw70 : f.yaw; // NIGHT70 Z3: the committed run's close look (plain: the inner pilot's)
                    var inReach = Alive(rd).Where(e => e is Husk && !e.standIn29 && !e.tank29 && Flat(e.position, p) <= 5.5 && AngDiff(YawTo(p, e.position), look70) <= 50 * Math.PI / 180).OrderBy(e => Flat(e.position, p)).ToList();
                    // NIGHT68 I2 fix round (verdict-I2.md): and not a body whose punch would be a parry now (V68ParryWhy: winding up, or inside
                    // parry36's early window; a parry never launches), skipped with its reason
                    target = inReach.FirstOrDefault(e => e.hp > bound && V68ParryWhy(sim, e) == null);
                    trig = target != null;
                    if (!trig && inReach.Count > 0 && fight)
                    {
                        stat.skipped++;
                        Enemy e0 = inReach[0];
                        string why0 = e0.hp <= bound ? "at or under the bound" : "would parry: " + V68ParryWhy(sim, e0);
                        string sk = $"{e0.kind} hp {e0.hp.ToString("0", CultureInfo.InvariantCulture)} ({why0})";
                        stat.skippedKinds.TryGetValue(sk, out int sn); stat.skippedKinds[sk] = sn + 1;
                    }
                    if (trig && fight) { string key = $"{target.kind} hp {target.hp.ToString("0", CultureInfo.InvariantCulture)}"; stat.targets.TryGetValue(key, out int tn); stat.targets[key] = tn + 1; } // NIGHT68 I2: the target's kind and hp
                }
                else if (verb == "pierce")
                {
                    var bodies = Alive(rd).ToList();
                    foreach (Enemy a in bodies.OrderBy(e => (e.AimPoint - eye).Length))
                    {
                        Vec3 d = a.AimPoint - eye;
                        double len = d.Length;
                        if (len > 30 || len < 1) continue;
                        Vec3 u = d / len;
                        if (sim.world.RayIntersect(eye, u, out RayHit h) && h.distance < len - 0.6) continue; // not in clear sight
                        // NIGHT66 T1b amendment (builder R's round 3, via the lead): the charge's REAL reach. A body counts only when the shot's own
                        // ray hits it (Enemy.RayHit, the hit volume Enemies.Raycast and the charged KESTREL shot use: ChargeShots27.cs
                        // FireRevolverCharged27Body), before the wall; NIGHT65's rule counted any body within 2.0 m of the line, so most charges
                        // hit one body while the pilot stood holding the trigger (a2: 55.8 % of fight steps)
                        double wallAt = sim.world.RayIntersect(eye, u, out RayHit wh) ? wh.distance : double.PositiveInfinity;
                        int n = 0;
                        foreach (Enemy b in bodies)
                        {
                            double bd = b.RayHit(eye, u, out bool hd);
                            if (bd >= 0 && bd <= 30 && bd < wallAt) n++;
                        }
                        if (n >= 2) { target = a; trig = true; break; }
                    }
                }
                else if (verb == "ride")
                {
                    // NIGHT68 I1: line65's face only (else, with no line65, any face of the room), armed once per pass (see Ride68NoRide's block)
                    V68Line lf = LineOf(rd.id);
                    if (lf != null && lf.why != null) { if (fight) stat.refused++; stat.refusedWhy = lf.why; }
                    else
                    {
                        if (stat.lineFace == null) stat.lineFace = lf != null ? faces68[lf.face].key : "any face (no line65)";
                        if (!armed68)
                        {
                            if (lf != null)
                            {
                                // armed again once the feet leave the line's run: 4 m out from the face (or behind it), or 4 m past either end
                                double[] lw = rides[lf.face];
                                double offA = (lw[0] == 0 ? p.x - lw[1] : p.z - lw[1]) * lw[2], sA = lw[0] == 0 ? p.z : p.x;
                                if (offA > 4 || offA < -1 || sA < Math.Min(lf.entryS, lf.exitS) - 4 || sA > Math.Max(lf.entryS, lf.exitS) + 4) armed68 = true;
                            }
                            else if (Math.Sqrt((p.x - armX68) * (p.x - armX68) + (p.z - armZ68) * (p.z - armZ68)) > 4) armed68 = true;
                        }
                        for (int i = 0; i < rides.Count && !trig && armed68; i++)
                        {
                            if (rideZones[i] != rd.id) continue;
                            if (lf != null && i != lf.face) continue;
                            double[] w = rides[i];
                            double off = (w[0] == 0 ? p.x - w[1] : p.z - w[1]) * w[2]; // metres out from the face on its side
                            double s = w[0] == 0 ? p.z : p.x;
                            bool inLen = s > w[3] + 1 && s < w[4] - 1;
                            bool air = !sim.mover.grounded;
                            // on the line's run: from 2.5 m before its entry (the scout's entry radius) to 2 m before its exit (ride left to do)
                            bool onRun = lf == null || (s - lf.entryS) * lf.dir >= -2.5 && (lf.exitS - lf.dir * 2 - s) * lf.dir >= 0;
                            if (inLen && onRun && off > 0.2 && (air && off < 1.6 || !air && off < 2.5) && p.y < w[5] - 1.5)
                            {
                                trig = true;
                                epFace68 = i;
                                armed68 = false; armX68 = lf != null ? lf.entryX : p.x; armZ68 = lf != null ? lf.entryZ : p.z;
                                nX = w[0] == 0 ? w[2] : 0; nZ = w[0] == 1 ? w[2] : 0;
                                bool toHi = lf != null ? lf.dir > 0 : (w[4] - s) >= (s - w[3]);
                                alongX = w[0] == 1 ? (toHi ? 1 : -1) : 0; alongZ = w[0] == 0 ? (toHi ? 1 : -1) : 0;
                                double endS = lf != null ? lf.exitS : toHi ? w[4] - 1.5 : w[3] + 1.5;
                                endX = w[0] == 1 ? endS : p.x; endZ = w[0] == 0 ? endS : p.z;
                            }
                        }
                    }
                }
                else if (verb == "parry")
                {
                    double reach = sim.cfg.Num("orb.reflectRange") * 0.9;
                    foreach (SpitterAcid.Glob g in sim.husks.acid.globs) if (g.alive && !g.reflected && (g.position - eye).Length <= reach) { trig = true; break; }
                }
                else if (verb == "slide")
                {
                    double fx = -Math.Sin(f.yaw), fz = -Math.Cos(f.yaw);
                    foreach (double[] t in tunnels)
                    {
                        if (t[4] < 1.0 || t[4] > 1.35) continue;
                        for (double k = 0; k <= 2.5 && !trig; k += 0.5)
                        {
                            double x = p.x + fx * k, z = p.z + fz * k;
                            if (x > Math.Min(t[0], t[2]) && x < Math.Max(t[0], t[2]) && z > Math.Min(t[1], t[3]) && z < Math.Max(t[1], t[3])) trig = true;
                        }
                    }
                }
                if (trig) { if (fight) stat.trigger++; ep = verb.StartsWith("launch") ? "launch" : verb; epSteps = 0; stat.episodes++; fullAt = -1; fulls = 0; if (refuse) refusedEpisodes++; }
            }
            else if (ep != null && fight) stat.trigger++;
            if (ep == null)
            {
                // NIGHT70 Z3: the committed run closes on the nearest launchable body inside 12 m (the look on it, the stick forward)
                closeYaw70 = double.NaN;
                if (CommitOn70(active, out RoomDef rc))
                {
                    Enemy t = CommitTarget70(rc, p);
                    if (t != null)
                    {
                        stat.commitClose++;
                        f.yaw = YawTo(p, t.position); f.pitch = PitchTo(eye, t.AimPoint); f.moveZ = 1; f.moveX = 0;
                        yaw = f.yaw; pitch = f.pitch; inner.SetLook(yaw, pitch); closeYaw70 = f.yaw;
                    }
                }
                return f;
            }
            closeYaw70 = double.NaN;
            epSteps++;
            // ---- NIGHT66 T1b: the refused episode (the verb declined, its holds and approach released; the file's head)
            if (refuse)
            {
                if (ep == "launch")
                {
                    if (target == null || !target.alive || epSteps > MovementMath.StepsFor(0.8, sim.step)) { End(); return f; }
                    f.yaw = YawTo(eye, target.AimPoint); f.pitch = PitchTo(eye, target.AimPoint); // the aim only: the inner pilot's move and gun
                }
                else if (ep == "pierce")
                {
                    if (target == null || !target.alive || epSteps > MovementMath.StepsFor(V65RefusedAimS, sim.step)) { End(); return f; }
                    f.yaw = YawTo(eye, target.AimPoint); f.pitch = PitchTo(eye, target.AimPoint);
                }
                else { End(); return f; } // ride, parry, slide: every input of the episode serves the verb only
                yaw = f.yaw; pitch = f.pitch;
                inner.SetLook(yaw, pitch);
                return f;
            }
            // ---- run the episode
            switch (ep)
            {
                case "launch":
                {
                    if (target == null || !target.alive || epSteps > MovementMath.StepsFor(0.8, sim.step)) { End(); return f; }
                    f.yaw = YawTo(eye, target.AimPoint); f.pitch = PitchTo(eye, target.AimPoint); f.moveZ = 1; f.moveX = 0;
                    if (verb == "launch+slide") f.slide = new Button { held = true, pressed = epSteps == 1 };
                    else if (epSteps <= 2) f.dash = new Button { held = true, pressed = epSteps == 1 };
                    if (epSteps == 1) stat.dashes++;
                    Launch36 lc = sim.launch36Core;
                    bool speed = sim.mover.state == MoveState.Dash || sim.mover.state == MoveState.Slide || (lc != null && lc.graceLeft > 0);
                    if (speed) stat.dashStates++;
                    // NIGHT30's rule drops a melee while the gun's action runs (the autopilot has just fired): punch once it is over
                    if (speed && Flat(target.position, p) <= sim.cfg.Num("punch.range") * 0.9 && !MeleeBlocked())
                    {
                        // NIGHT68 I2 fix round: the punch held while it would be a parry (the episode's 0.8 s still bounds it)
                        if (V68PunchParryWhy(sim, target, f.yaw, f.pitch) != null) { if (stat != null) stat.parryHeld++; }
                        else wantPunch = true;
                    }
                    break;
                }
                case "pierce":
                {
                    if (target != null && target.alive) { f.yaw = YawTo(eye, target.AimPoint); f.pitch = PitchTo(eye, target.AimPoint); }
                    if (slot < 0) slot = sim.loadout.owned.IndexOf("revolver");
                    if (sim.loadout.Weapon != "revolver") { firing = false; if (epSteps > MovementMath.StepsFor(1.0, sim.step)) End(); break; }
                    if (fulls > 0 && fullAt < 0) fullAt = epSteps;
                    firing = fullAt < 0 || epSteps - fullAt < 2;
                    release = !firing;
                    if (release || epSteps > MovementMath.StepsFor(4, sim.step)) { firing = false; End(); }
                    break;
                }
                case "ride":
                {
                    bool riding = sim.mover.state == MoveState.WallRide;
                    if (riding) wasRiding = true;
                    double left = Math.Abs(alongX != 0 ? endX - p.x : endZ - p.z);
                    if (wasRiding && (!riding || left < 0.5) || epSteps > MovementMath.StepsFor(3, sim.step) && !wasRiding) { End(); return f; }
                    RideKill53Wall.Stick(new Vec3(alongX, 0, alongZ), new Vec3(nX, 0, nZ), 0.5, out double ry, out double mx, out double mz);
                    f.yaw = ry; f.moveX = mx; f.moveZ = mz;
                    bool jumpNow = sim.mover.grounded && epSteps <= 2;
                    f.jump = new Button { held = true, pressed = jumpNow };
                    break;
                }
                case "parry":
                {
                    double reach = sim.cfg.Num("orb.reflectRange") * 0.9;
                    SpitterAcid.Glob near = null; double best = double.MaxValue;
                    foreach (SpitterAcid.Glob g in sim.husks.acid.globs) { if (!g.alive || g.reflected) continue; double d = (g.position - eye).Length; if (d < best) { best = d; near = g; } }
                    if (near == null || best > reach || epSteps > 6) { End(); return f; }
                    f.yaw = YawTo(eye, near.position); f.pitch = PitchTo(eye, near.position);
                    // NIGHT68 g7: with the gauntlet in hand the punch is dropped (Rules30.MeleeBlocked) and the glove's own swing sends a glob back
                    // (Combat.SwingGauntlet: Foes40's SwingReflect40 first): the pilot swings (fire) at the glob instead, once per episode
                    swingParry68 = sim.loadout.Weapon == "gauntlet";
                    wantPunch = !parryThrown && (swingParry68 || !MeleeBlocked());
                    if (wantPunch) parryThrown = true;
                    break;
                }
                case "slide":
                {
                    bool inside = tunnels.Any(t => p.x > Math.Min(t[0], t[2]) && p.x < Math.Max(t[0], t[2]) && p.z > Math.Min(t[1], t[3]) && p.z < Math.Max(t[1], t[3]));
                    if (epSteps > MovementMath.StepsFor(2.5, sim.step) || (!inside && epSteps > MovementMath.StepsFor(1.0, sim.step))) { End(); return f; }
                    f.moveZ = 1; f.moveX = 0;
                    f.slide = new Button { held = true, pressed = epSteps == 1 };
                    if (epSteps == 1) stat.slides++;
                    break;
                }
            }
            yaw = f.yaw; pitch = f.pitch;
            inner.SetLook(yaw, pitch);
            return f;
        }

        int reflectsAtPunch = -1;

        // NIGHT70 Z3: the trigger state this pilot gave, recorded on every path (the committed run keeps a held pistol / shotgun trigger held)
        public CombatInput SampleCombat(bool active) { CombatInput c = SampleCombat0(active); lastFireHeld70 = c.fire.held; return c; }

        CombatInput SampleCombat0(bool active)
        {
            CombatInput c = inner.SampleCombat(active);
            if (stat != null && reflectsAtPunch >= 0 && sim.husks.acid.reflects > reflectsAtPunch) { stat.parries++; reflectsAtPunch = -1; }
            if (launchWindow > 0) launchWindow--;
            if (pierceWindow > 0) pierceWindow--;
            if (releaseLeft > 0) { releaseLeft--; c.fire = default; return c; } // the charged shot's release: fire up, whatever the inner pilot holds
            // NIGHT70 Z3: the committed run holds its fire while no episode runs and a launchable body is inside 12 m (CommitOn70 / CommitTarget70)
            held70 = false;
            if (CommitOn70(active, out RoomDef rh) && CommitTarget70(rh, sim.mover.position) != null)
            {
                held70 = true; if (stat != null) stat.commitHeld++;
                // round 2 (the pair's 1-3 shots in held steps): the pistol's / shotgun's trigger already held (a charge building) is KEPT held, never
                // released (charge35: a release always fires, a full charge or a tap: ChargeShots27.cs StepCharge35); every other trigger up
                // round 3 (round 2's pair: still 1-2 shots in held steps: charge35's auto-release of a full charge held holdMaxS more, and a tap
                // queued before the hold that fires when the gun's cooldown ends, Charge35.cs:281-297): the one input that drops a building charge
                // or a queued tap with no shot is a switch to the other gun (`the gun in hand changed under the hold: dropped, nothing spent`,
                // ChargeShots27.cs:212 / :225); so a held step with a charge building or a tap queued switches pistol <-> shotgun (owned), the
                // trigger kept held until the gun in hand changes (a release would fire), then up
                string w = sim.loadout.Weapon;
                Charge35 q35 = (sim.loadout.charge27 ?? sim.charge27)?.c35;
                bool gun = w == "revolver" || w == "shotgun";
                bool pending = gun && q35 != null && (q35.holding || q35.tapQueued);
                int other = gun ? sim.loadout.owned.IndexOf(w == "revolver" ? "shotgun" : "revolver") : -1;
                bool keep = lastFireHeld70 && gun && (q35 == null || q35.holding);
                c.fire = keep ? new Button { held = true, pressed = false } : default;
                if (pending && other >= 0) { c.weapon = other; if (stat != null) stat.commitSwitch++; }
                if (keep && stat != null) stat.commitKept++;
                return c;
            }
            if (!active || ep == null || refuse) return c; // NIGHT66 T1b: a refused episode never takes the combat input (the gun runs)
            if (ep == "parry" && swingParry68)
            {
                // NIGHT68 g7: the glove in hand: the swing (fire) is the parry press, the punch stays up
                c.weapon = -1;
                c.punch = default;
                c.fire = wantPunch ? new Button { held = true, pressed = true } : default;
                if (wantPunch) { reflectsAtPunch = sim.husks.acid.reflects; if (stat != null) stat.swingParries++; }
            }
            else if (ep == "launch" || ep == "parry")
            {
                c.fire = default;
                c.weapon = -1; // no switch queued during the episode (a queued switch blocks the punch too)
                c.punch = wantPunch ? new Button { held = true, pressed = true } : default;
                if (wantPunch && ep == "parry") reflectsAtPunch = sim.husks.acid.reflects;
                if (wantPunch && ep == "launch") { launchWindow = MovementMath.StepsFor(0.25, sim.step); stat.punches++; End(); }
            }
            else if (ep == "pierce")
            {
                if (sim.loadout.Weapon != "revolver" && slot >= 0) { c.weapon = slot; c.fire = default; }
                else c.fire = firing ? new Button { held = true, pressed = epSteps == 1 } : default;
            }
            return c;
        }
    }
}
