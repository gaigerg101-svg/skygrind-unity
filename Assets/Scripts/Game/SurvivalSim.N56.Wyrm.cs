// NIGHT56 builder G (G4; NIGHT55's sim row design.n55.wyrm55; Core/Enemies/Wyrm55.cs): THE KILN WYRM's encounter numbers. Called from the night's
// hooks (SurvivalSim.N56.cs: Night56StartRun / Night56Step, which run after NIGHT46 / NIGHT47's, so wyrm46 / wyrm47 have set theirs this step).
// Every step while the row is on and a worm exists: Wyrm31's gunScale / staggerSeconds / open32 and the worm's contactCooldown / phase gates /
// barrage pacing take
// the row's numbers (a field that is not the row's value is saved, then set: wyrm46 rewrites its own every step); the pool is scaled once a fight
// (the step its body first lives at full health). At the next run's start a field still holding the row's value goes back to the saved one (a
// field another row restored in between is left alone: wyrm47's compare-before-restore). Off (the bit false, --off55 / --off56): nothing here
// runs and nothing is changed: NIGHT55's fight bit for bit. No allocation per step.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        internal int idWyrm55 = -1;
        bool w55On;
        Wyrm55Params w55P;
        Wyrm31 w55For31;
        Wyrm w55ForW;
        double w55GunWas, w55StagWas, w55OpenWas, w55CdWas, w55P2Was, w55P3Was, w55S2Was, w55S3Was;
        int w55Body = -1;

        /// <summary>wyrm55 on for this run (the harness reads it); the fights whose pool it scaled (this sim's life).</summary>
        public bool Wyrm55On => w55On;
        public int wyrm55Pools;

        void N56WyrmStartRun()
        {
            if (w55For31 != null && w55P != null)
            {
                if (!double.IsNaN(w55P.gunScale) && w55For31.p.gunScale == w55P.gunScale) w55For31.p.gunScale = w55GunWas;
                if (!double.IsNaN(w55P.staggerSeconds) && w55For31.p.staggerSeconds == w55P.staggerSeconds) w55For31.p.staggerSeconds = w55StagWas;
                if (!double.IsNaN(w55P.open) && w55For31.open32 == w55P.open) w55For31.open32 = w55OpenWas;
            }
            if (w55ForW != null && w55P != null)
            {
                if (!double.IsNaN(w55P.contactCooldown) && w55ForW.p.contactCooldown == w55P.contactCooldown) w55ForW.p.contactCooldown = w55CdWas;
                if (!double.IsNaN(w55P.phase3At) && w55ForW.p.phase3At == w55P.phase3At) w55ForW.p.phase3At = w55P3Was;
                if (!double.IsNaN(w55P.phase2At) && w55ForW.p.phase2At == w55P.phase2At) w55ForW.p.phase2At = w55P2Was;
                if (!double.IsNaN(w55P.spitEvery2) && w55ForW.p.spitEvery2 == w55P.spitEvery2) w55ForW.p.spitEvery2 = w55S2Was;
                if (!double.IsNaN(w55P.spitEvery3) && w55ForW.p.spitEvery3 == w55P.spitEvery3) w55ForW.p.spitEvery3 = w55S3Was;
            }
            w55For31 = null;
            w55ForW = null;
            w55Body = -1;
            idWyrm55 = rowsRegistry.IndexOf("wyrm55"); // -1: this config lacks the row (rows.On(-1) is false: NIGHT55's fight)
            w55On = rows.On(idWyrm55);
            if (w55On) w55P ??= Wyrm55Params.From(cfg);
        }

        void N56WyrmStep(double dt)
        {
            if (!w55On || w55P == null) return;
            if (wyrm31 != null)
            {
                w55For31 = wyrm31;
                if (!double.IsNaN(w55P.gunScale) && wyrm31.p.gunScale != w55P.gunScale) { w55GunWas = wyrm31.p.gunScale; wyrm31.p.gunScale = w55P.gunScale; }
                if (!double.IsNaN(w55P.staggerSeconds) && wyrm31.p.staggerSeconds != w55P.staggerSeconds) { w55StagWas = wyrm31.p.staggerSeconds; wyrm31.p.staggerSeconds = w55P.staggerSeconds; }
                if (!double.IsNaN(w55P.open) && wyrm31.open32 != w55P.open) { w55OpenWas = wyrm31.open32; wyrm31.open32 = w55P.open; }
            }
            Wyrm w = wyrm24;
            if (w == null) return;
            w55ForW = w;
            if (!double.IsNaN(w55P.contactCooldown) && w.p.contactCooldown != w55P.contactCooldown) { w55CdWas = w.p.contactCooldown; w.p.contactCooldown = w55P.contactCooldown; }
            if (!double.IsNaN(w55P.phase3At) && w.p.phase3At != w55P.phase3At) { w55P3Was = w.p.phase3At; w.p.phase3At = w55P.phase3At; }
            if (!double.IsNaN(w55P.phase2At) && w.p.phase2At != w55P.phase2At) { w55P2Was = w.p.phase2At; w.p.phase2At = w55P.phase2At; }
            if (!double.IsNaN(w55P.spitEvery2) && w.p.spitEvery2 != w55P.spitEvery2) { w55S2Was = w.p.spitEvery2; w.p.spitEvery2 = w55P.spitEvery2; }
            if (!double.IsNaN(w55P.spitEvery3) && w.p.spitEvery3 != w55P.spitEvery3) { w55S3Was = w.p.spitEvery3; w.p.spitEvery3 = w55P.spitEvery3; }
            // the pool: once a fight, in the step the body first lives at full health (boss39 and wyrm46 scaled it earlier in this step)
            if (!double.IsNaN(w55P.poolScale) && w.Alive && w.body.id != w55Body)
            {
                w55Body = w.body.id;
                if (w.body.hp >= w.body.maxHp)
                {
                    w.body.maxHp *= w55P.poolScale;
                    w.body.hp = w.body.maxHp;
                    if (wyrm31 != null && wyrm31.on32) wyrm31.maxHp32 = w.body.maxHp;
                    wyrm55Pools++;
                }
            }
        }
    }
}
