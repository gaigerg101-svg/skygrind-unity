// NIGHT20 U3-34.6 step 5 (builder E owns this file): design.director.n20 in the sim, through the shared commit's hook
// (SurvivalSim.N20.cs: N20DirectorStartRun at the run's start). The director reads its block itself (Director.n20, set by
// N20StartRun before this hook; Core/Enemies/Director20.cs); the run hands it where the reach pass starts: the map's player
// spawn, the start zone's floor (the perch is a node no walker from there reaches). N20DirectorStep has nothing to add: the
// director's own Step runs the camp score and the failsafe. NIGHT20 C2: the LANCER's perches and hunted flag (main.js
// createLancers' getPerches / isHunted) are handed to the Husks' pool here.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        partial void N20DirectorStartRun()
        {
            director.reachFrom = map.playerSpawn;
            // NIGHT20 (design.adds.lancer): main.js createLancers' getPerches (the director's high nodes) and isHunted
            // (director.hunted: the laser follows twice as fast); made once (no lancer spawns unless the director deals one)
            if (!lancerWired)
            {
                lancerWired = true;
                husks.lancerPerches = director.HighNodes;
                husks.lancerHunted = () => director.Hunted;
                // NIGHT20 A2 + A4: THE HUNT and THE ELITE (Core/Enemies/SpecialRounds.cs); the elite's affixes through
                // affixes.onSpawn's `forced` (SurvivalSim.Enemies19.cs's Affixes: the ported ones play)
                specials = new SpecialRounds(cfg, events, director, powerUps, () => run.BreakLeft, () => runSeed, (h, list) =>
                {
                    if (!enemies19Wired) Enemies19Wire();
                    affixes.OnSpawn(h, list);
                }, health, economy, () => run.stats.heat == 0); // A5: no twist on a HEAT run
            }
            specials.NewRun(n20.Add(cfg, "hunt"), n20.Add(cfg, "elite"), n20.Add(cfg, "twists"));
            // NIGHT20 C3: every affix plays with design.adds.affixes (the roll is design.enemies.n19's: off, NIGHT19's SHIELDED)
            if (affixes != null) affixes.all = n19.enemies && n20.Add(cfg, "affixes");
            Boss20StartRun(); // NIGHT20 A1: the boss rounds (SurvivalSim.Boss20.cs; design.adds.bulwark)
            husks.n20State ??= new CustomState(Enemies20Save, Enemies20Load); // NIGHT20 fix EN2: saved inside the husks' section
        }

        /// <summary>NIGHT20 fix EN2 (RunRestoreTests: seed 37's restore at round 5's break differed 354 steps on: a kill paid 23
        /// points more): the run snapshot had none of the NIGHT19 / NIGHT20 enemy systems' state, so a restored run re-rolled
        /// the affixes from the run's start. Saved now (inside the husks' section, which this file's owner holds): the affixes'
        /// stream and tracking, the special rounds, the director's NIGHT20 clocks, the BULWARK's stream.</summary>
        void Enemies20Save(StateWriter w)
        {
            if (affixes != null)
            {
                w.Begin("affixes");
                affixes.SaveState(w);
                w.End();
            }
            if (specials != null)
            {
                w.Begin("specials");
                specials.SaveState(w);
                w.End();
            }
            w.Begin("director");
            director.SaveState20(w);
            w.End();
            if (bulwark != null) w.I("bulwarkRng", bulwark.RngState);
        }

        void Enemies20Load(StateReader r)
        {
            if (r.Has("affixes"))
            {
                if (!enemies19Wired) Enemies19Wire();
                affixes.LoadState(r.Section("affixes"));
            }
            if (r.Has("specials") && specials != null) specials.LoadState(r.Section("specials"));
            if (r.Has("director")) director.LoadState20(r.Section("director"));
            if (r.Has("bulwarkRng") && bulwark != null) bulwark.RngState = r.U("bulwarkRng");
        }

        partial void N20DirectorStep(double dt)
        {
            specials?.Step();
            Boss20Step(dt); // NIGHT20 A1: the BULWARK steps with the enemies (main.js's enemies facade)
        }

        bool lancerWired;

        /// <summary>NIGHT20 (design.adds.hunt / design.adds.elite): THE HUNT and THE ELITE (H reads EliteName / EliteId,
        /// HuntRound; the bus carries `specialRound`).</summary>
        public SpecialRounds specials;
    }
}
