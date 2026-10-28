// NIGHT42 builder W1 parry42 (N42Blocks.parry42, design.n42.parry42; DECIDE42 D-3: the DRONE's orbs only). GG 2026-10-14: "the parry attack timing
// from the flying enimies is relaly tight and you miss depsite felling like you hit it". CODE-FACTS G4: the orb's reflect was a 2 m point test at
// the eye, crossed in ~0.17 s at 12 m/s; the punch's early buffer (parry36) 0.12 s; the gauntlet's swing (SwingReflect40) no buffer at all.
// With the row on (the sim sets the numbers at a run's start, SurvivalSim.N42.Weapons.cs):
// - the reach is a volume, 1.5x (DroneOrbs.NearestHostile42), and a touching orb waits a mercy window before its damage (DroneOrbs.Step);
// - THE ORB BUFFER: a punch or a gauntlet swing that sent nothing back keeps an orb-only buffer of orbBufferS (parry36's 0.12 s doubled): an orb
//   that comes into reach within it is parried then (the web's parry: heal, the dash refill, "punch" / "parry"; parry36's return when on).
//   The swing opens it too (Foes40.SwingReflect40's miss), so the gauntlet in hand forgives like the punch.
// Off (orbBufferSteps42 0, DroneOrbs.parry42 false): NIGHT41's parry bit for bit. Events: the web's "parry" (kind "orb"), parry36's "parry36"
// (weapon "parry36buffer" when a buffer made it), and "parry42Buffer" {weapon} when the orb buffer caught one (GG's sound hook).
namespace Skygrind.Core
{
    public sealed partial class Loadout
    {
        /// <summary>parry42: the orb buffer's length in steps (0: off) and the steps left of the current one.</summary>
        public int orbBufferSteps42, orbBuffer42;

        /// <summary>parry42: which weapon opened the current buffer ("punch" | "gauntlet") and the run's counters (the harness reads them).</summary>
        public string orbBufferBy42;
        public int orbBuffersOpened42, orbBufferParries42;

        readonly MeleeEvent parry42Event = new MeleeEvent();

        /// <summary>parry42: a punch / swing that sent nothing back opens the orb buffer (no-op with the row off).</summary>
        void Parry42Miss(string weapon)
        {
            if (orbBufferSteps42 <= 0) return;
            orbBuffer42 = orbBufferSteps42;
            orbBufferBy42 = weapon;
            orbBuffersOpened42++;
        }

        /// <summary>parry42: a run's start (the sim: SurvivalSim.N42.Weapons.cs).</summary>
        public void Parry42Reset(int bufferSteps)
        {
            orbBufferSteps42 = bufferSteps;
            orbBuffer42 = 0;
            orbBufferBy42 = null;
            orbBuffersOpened42 = orbBufferParries42 = 0;
        }

        /// <summary>parry42's per-step buffer (the sim's N42WeaponsStep): an orb now in reach is parried.</summary>
        public void Parry42Step()
        {
            if (orbBuffer42 <= 0) return;
            orbBuffer42--;
            if (acid == null) return;
            var (origin, dir) = getAim();
            SpitterAcid.Glob glob = acid.TryReflectOrb42(origin, dir, enemies);
            if (glob == null) return;
            orbBuffer42 = 0;
            orbBufferParries42++;
            ParryGlobWeb(glob);
            if (parry36 != null)
            {
                parry36.bufferGlobSteps = 0;
                Parry36Glob(glob, true);
            }
            parry42Event.owner = owner;
            parry42Event.weapon = orbBufferBy42;
            parry42Event.hits = 1;
            parry42Event.parry = true;
            events.Emit("parry42Buffer", parry42Event);
        }
    }
}
