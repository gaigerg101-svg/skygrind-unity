// NIGHT33 builder W (design.n33.pierce33, DESIGN33 W, D-4). Unity-only (the web has no charged shot).
// GG 2026-10-11: a stronger piercing pistol charge. With the row on, the charged KESTREL shot (ChargeShots27's FireRevolverCharged27) goes through
// up to `pierce` bodies (24, the LONGWAVE's weapons.rail.pierce) at `damageMult` x a normal shot (n31.charge has 3 bodies at 4x), its tracer ends at
// the wall (or the range end) instead of the last body, and it emits `kestrelPierce33` (a RailFireEvent: owner, from = the eye, to = the tracer's end,
// hits) that the views draw as the LONGWAVE's beam (WeaponFx) with a camera kick (Viewmodel), and the sim answers with `hitstopSteps` of hitstop
// when it hit (SurvivalSim.Pierce33.cs). The cooldown stays design.n31.charge.revolver.cooldownSeconds (4 s, the crosshair arc reads it).
// Off (null on the loadout): NIGHT32's charged shot, bit for bit.
namespace Skygrind.Core
{
    /// <summary>design.n33.pierce33's numbers.</summary>
    public sealed class Pierce33Params
    {
        public int pierce, hitstopSteps;
        public double damageMult;

        public static Pierce33Params From(GameConfig cfg)
        {
            const string p = "design.n33.pierce33.";
            return new Pierce33Params { pierce = cfg.Int(p + "pierce"), damageMult = cfg.Num(p + "damageMult"), hitstopSteps = cfg.Int(p + "hitstopSteps") };
        }
    }

    public sealed partial class Loadout
    {
        /// <summary>NIGHT33 pierce33 (the sim sets it at a run's start; null with the row off).</summary>
        public Pierce33Params pierce33;

        /// <summary>NIGHT33: the run's counters (the harness reads them; the sim's StartRun resets them with the row's switch).</summary>
        public int pierce33Shots, pierce33Hits;

        readonly RailFireEvent pierceEv33 = new RailFireEvent();

        /// <summary>The new event's name (PORT-STATUS): the charged KESTREL's piercing beam.</summary>
        public const string KestrelPierce33 = "kestrelPierce33";

        /// <summary>The tracer's end and the beam event for a piercing charged shot (called at the end of FireRevolverCharged27Body while pierce33 is on).</summary>
        void Pierce33Finish(ShotEvent shot, Vec3 origin, Vec3 dir, double wallDist, RayHit wall, int hits)
        {
            if (!double.IsInfinity(wallDist))
            {
                shot.end = origin + dir * wallDist;
                shot.surface = "world";
                shot.normal = wall.normal;
            }
            else shot.end = origin + dir * revRange;
            pierce33Shots++;
            pierce33Hits += hits;
            pierceEv33.owner = owner;
            pierceEv33.from = origin;
            pierceEv33.to = shot.end;
            pierceEv33.hits = hits;
            pierceEv33.over = false;
            events.Emit(KestrelPierce33, pierceEv33);
        }
    }
}
