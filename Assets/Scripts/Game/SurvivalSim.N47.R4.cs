// NIGHT47 builder B, round 4 (the beyond round, G44 / G45; Research/2026-10-18-wave47/B.md R-B5 / R-B2): two sim rows of B's own, each off by
// default (Tools/night/n47-b-r4-rows.cjs adds them; numbers in design.n47.<row>.numbers by Tools/night/n47-b-rows.cjs). Called from
// SurvivalSim.N47.Brawl.cs's hooks. Their ids are resolved here (the lead's SurvivalSim.N47.cs resolves the pre-placed rows only).
//   stampede47  THE CHARGER's stampede as a SURVIVAL event (the lead's list): in SURVIVAL (no campaign), from round firstRound on, every
//               every-th round, delayS s after the round begins, a herd of count chargers comes in together from the ground spawns
//               farthest from the player (one side of the map), event stampede47 {at}; with charger47 on they burst husks in their way.
//   trail47     THE KILN WYRM's burrow trail burns: while the worm fights, a grounded player whose feet are within radius m (horizontally) of a
//               buried piece, and under maxUp m over the arena's floor, takes dps x tickS every tickS s (event wyrm47TrailBurn): the floor
//               over the burrow is the worm's, the air (the boost, the islands) is yours.
// Off: nothing runs. No allocation per step. No UnityEngine: SimHarness compiles this file.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        const string Stampede47Root = "design.n47.stampede47.numbers.", Trail47Root = "design.n47.trail47.numbers.";
        int idStampede47 = -1, idTrail47 = -1;
        bool st47On, tr47On;
        int st47Round, st47Wait, tr47Tick;
        readonly int[] st47Pick = new int[8];
        readonly Wyrm47BeatEvent st47Said = new Wyrm47BeatEvent();
        /// <summary>The harness's counts this sim's life: stampedes sent, chargers in them; trail burns on the player.</summary>
        public int stampedes47, stampedeChargers47, trailBurns47;

        void R4StartRun()
        {
            idStampede47 = rowsRegistry.IndexOf("stampede47");
            idTrail47 = rowsRegistry.IndexOf("trail47");
            st47On = rows.On(idStampede47);
            tr47On = rows.On(idTrail47);
            st47Round = 0;
            st47Wait = -1;
            tr47Tick = 0;
        }

        void R4Step()
        {
            if (st47On) Stampede47Step();
            if (tr47On) Trail47Step();
        }

        void Stampede47Step()
        {
            if (campaignLevel != null || director == null) return;
            int r = director.Round;
            if (r != st47Round)
            {
                st47Round = r;
                int first = (int)cfg.Num(Stampede47Root + "firstRound"), every = System.Math.Max(1, (int)cfg.Num(Stampede47Root + "every"));
                if (r >= first && (r - first) % every == 0) st47Wait = MovementMath.StepsFor(cfg.Num(Stampede47Root + "delayS"), cfg.Dt);
            }
            if (st47Wait < 0) return;
            if (st47Wait-- > 0) return;
            st47Wait = -1;
            var spawns = map.groundSpawns;
            if (spawns.Count == 0) return;
            // the herd's side: the ground spawn farthest from the player, then the spawns nearest to it (no allocation: a fixed pick list)
            Vec3 me = mover.position;
            int far = 0;
            double best = -1;
            for (int i = 0; i < spawns.Count; i++)
            {
                double dx = spawns[i].x - me.x, dz = spawns[i].z - me.z, d = dx * dx + dz * dz;
                if (d > best) { best = d; far = i; }
            }
            int n = System.Math.Min(st47Pick.Length, System.Math.Min(spawns.Count, (int)cfg.Num(Stampede47Root + "count")));
            for (int k = 0; k < n; k++)
            {
                int pick = -1;
                double pd = double.MaxValue;
                for (int i = 0; i < spawns.Count; i++)
                {
                    bool used = false;
                    for (int j = 0; j < k; j++) if (st47Pick[j] == i) used = true;
                    if (used) continue;
                    double dx = spawns[i].x - spawns[far].x, dz = spawns[i].z - spawns[far].z, d = dx * dx + dz * dz;
                    if (d < pd) { pd = d; pick = i; }
                }
                st47Pick[k] = pick;
                if (pick >= 0 && husks.Spawn("charger", spawns[pick], me) != null) stampedeChargers47++;
            }
            stampedes47++;
            st47Said.beat = "stampede";
            st47Said.at = spawns[far];
            st47Said.to = me;
            st47Said.seconds = 0;
            st47Said.hit = false;
            events.Emit("stampede47", st47Said);
        }

        void Trail47Step()
        {
            Wyrm w = wyrm24;
            if (w == null || !w.Alive) { tr47Tick = 0; return; }
            if (tr47Tick > 0) { tr47Tick--; return; }
            Vec3 me = mover.position;
            double floor = w.arenaCenter.HasValue ? w.arenaCenter.Value.y : 0;
            if (!mover.grounded || me.y - floor > cfg.Num(Trail47Root + "maxUp")) return;
            double rad = cfg.Num(Trail47Root + "radius");
            bool over = false;
            for (int i = 0; i < w.PieceCount && !over; i++)
            {
                if (!w.buried[i]) continue;
                double dx = w.boxes[i].c.x - me.x, dz = w.boxes[i].c.z - me.z;
                if (dx * dx + dz * dz <= rad * rad) over = true;
            }
            if (!over) return;
            double tick = cfg.Num(Trail47Root + "tickS");
            tr47Tick = MovementMath.StepsFor(tick, cfg.Dt);
            if (health.Damage(cfg.Num(Trail47Root + "dps") * tick, "wyrm47Trail", me))
            {
                trailBurns47++;
                st47Said.beat = "trail";
                st47Said.at = me;
                st47Said.to = me;
                st47Said.seconds = tick;
                st47Said.hit = true;
                events.Emit("wyrm47TrailBurn", st47Said);
            }
        }
    }
}
