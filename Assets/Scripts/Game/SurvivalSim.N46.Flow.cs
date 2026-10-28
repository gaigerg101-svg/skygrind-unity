// NIGHT46 builder F1 (NIGHT46.md card F1, "the flow rules"; FLOW-AUDIT.md rows 1, 4, 5): three sim rows, each read with rows.On(id) and set
// on the campaign's NIGHT46 hooks (Campaign24's delegates: null = NIGHT45's rooms bit for bit) once a run, in N46FlowCampaign:
//   retry46   (DECIDE46 N-6): a death restarts the room at the wave you died in; the waves already won stay won (Campaign24.retryWave46).
//   overlap46 (DECIDE46 N-7): the next wave comes in while the last few bodies of this one still stand (Campaign24.waveRelease46).
//   teach46   (DECIDE46 N-11): the teaching holds (rooms43's stand-in hold, afterLesson wait, lesson cap) capped (Campaign24.teachHold46).
// Numbers: design.n46.<row>.numbers (each with its _why), read once a run in N46FlowStartRun. Off (or -skygrindOff46 / --off46): no delegate set.
// The delegates are made once a run (no allocation per step).
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        // retry46: waves stepped back from the one you died in (0: that wave again)
        int retryBack46;
        // overlap46: the next wave comes when at most this many bodies of the room stand, and at most this share of the last wave's bodies
        int overlapBodies46;
        double overlapShare46, overlapEntry46, overlapGrowth46;
        int overlapFromWave46;
        // teach46: the three holds' caps in steps
        int teachStandIn46, teachAfter46, teachCap46;

        partial void N46FlowStartRun()
        {
            const string R = "design.n46.";
            retryBack46 = rows.On(idRetry46) ? (int)cfg.Num(R + "retry46.numbers.wavesBack") : 0;
            if (rows.On(idOverlap46))
            {
                overlapBodies46 = (int)cfg.Num(R + "overlap46.numbers.bodiesLeft");
                overlapShare46 = cfg.Num(R + "overlap46.numbers.shareLeft");
                overlapFromWave46 = (int)cfg.Num(R + "overlap46.numbers.fromWave");
                overlapEntry46 = cfg.Num(R + "overlap46.numbers.entryM");
                overlapGrowth46 = cfg.Num(R + "overlap46.numbers.growthMax");
            }
            if (rows.On(idTeach46))
            {
                teachStandIn46 = Math.Max(1, MovementMath.StepsFor(cfg.Num(R + "teach46.numbers.standInHoldS"), step));
                teachAfter46 = Math.Max(1, MovementMath.StepsFor(cfg.Num(R + "teach46.numbers.afterLessonMaxS"), step));
                teachCap46 = Math.Max(1, MovementMath.StepsFor(cfg.Num(R + "teach46.numbers.lessonCapS"), step));
            }
            Stuck46Reset();
            Reveal46Reset();
        }

        // stuck46 (the lead's ask, NIGHT46 round 2; FLOW-AUDIT row 11): a burrower whose head cannot move toward the player's ground contact
        // (Burrower43.MoveHead stops at lava, a pit or a wall: n46flow's baseline, kb3: the last body a burrower at y -6 under the pour's far rim
        // for 240 s in 5 of 10 seeds, the door shut) is ended after stuckS: alive false, no kill event, no points (rooms43's end, the kill plane's way).
        internal int idStuck46 = -1;
        int stuckSteps46;
        readonly int[] stuckId46 = new int[16], stuckFor46 = new int[16];
        readonly bool[] stuckSeen46 = new bool[16];
        /// <summary>stuck46: burrowers ended this run (the harness's count).</summary>
        public int stuckEnded46;

        void Stuck46Reset()
        {
            idStuck46 = rowsRegistry.IndexOf("stuck46");
            stuckEnded46 = 0;
            for (int i = 0; i < stuckId46.Length; i++) { stuckId46[i] = -1; stuckFor46[i] = 0; }
            stuckSteps46 = rows.On(idStuck46) ? Math.Max(1, MovementMath.StepsFor(cfg.Num("design.n46.stuck46.numbers.stuckS"), step)) : 0;
        }

        // reveal46 (FLOW-AUDIT row 8: the boss reveal / phase windows, the boss untouchable and not attacking, 1.5-2 s each; DECIDE44 D-3 keeps
        // every reveal): each boss brain's Reveal, and its leg / phase change, held to at most revealS / phaseS. No boss module is edited: the
        // running state's steps left are clamped here (stateLeft, stateTotal), so the brain leaves the state on its own next step as it always does.
        int revealCap46, phaseCap46;

        void Reveal46Reset()
        {
            revealCap46 = phaseCap46 = 0;
            if (!rows.On(idReveal46)) return;
            revealCap46 = Math.Max(1, MovementMath.StepsFor(cfg.Num("design.n46.reveal46.numbers.revealS"), step));
            phaseCap46 = Math.Max(1, MovementMath.StepsFor(cfg.Num("design.n46.reveal46.numbers.phaseS"), step));
        }

        static void Clamp46(ref int left, ref int total, int steps, int cap)
        {
            if (left <= cap) return;
            left = cap;
            total = steps + cap; // the view's progress (stateSteps / stateTotal) runs to 1 with the shorter window
        }

        void Reveal46Step()
        {
            if (revealCap46 <= 0) return;
            if (lantern36 != null && lantern36.brain.state == Boss36State.Reveal) Clamp46(ref lantern36.brain.stateLeft, ref lantern36.brain.stateTotal, lantern36.brain.stateSteps, revealCap46);
            Brain37(crown37?.brain);
            Brain37(loom37?.brain);
            Brain37(stag37?.brain);
            Brain37(wick37?.brain);
            if (brawl40 != null)
            {
                if (brawl40.state == Brawl40State.Reveal) Clamp46(ref brawl40.stateLeft, ref brawl40.stateTotal, brawl40.stateSteps, revealCap46);
                else if (brawl40.state == Brawl40State.PhaseChange) Clamp46(ref brawl40.stateLeft, ref brawl40.stateTotal, brawl40.stateSteps, phaseCap46);
            }
        }

        void Brain37(Boss37Brain b)
        {
            if (b == null) return;
            if (b.state == Boss37State.Reveal) Clamp46(ref b.stateLeft, ref b.stateTotal, b.stateSteps, revealCap46);
            else if (b.state == Boss37State.LegChange) Clamp46(ref b.stateLeft, ref b.stateTotal, b.stateSteps, Math.Max(2, phaseCap46)); // the brain keeps 2 steps at least (Boss37.cs)
        }

        partial void N46FlowStep(double dt)
        {
            Reveal46Step();
            if (stuckSteps46 <= 0) return;
            for (int k = 0; k < stuckId46.Length; k++) stuckSeen46[k] = false;
            foreach (Enemy e in enemies.all)
            {
                if (!(e is Burrower43 b) || !b.alive) continue;
                bool stuck = b.state == Burrower43.Hunt && b.hasContact && b.trail.x == b.trailPrevious.x && b.trail.z == b.trailPrevious.z
                    && JsMath.Hypot(b.contact.x - b.trail.x, b.contact.z - b.trail.z) > b.Rules.triggerRange;
                int slot = -1, free = -1;
                for (int k = 0; k < stuckId46.Length; k++) { if (stuckId46[k] == b.id) slot = k; else if (stuckId46[k] < 0 && free < 0) free = k; }
                if (slot < 0) { if (free < 0 || !stuck) continue; slot = free; stuckId46[slot] = b.id; stuckFor46[slot] = 0; }
                stuckSeen46[slot] = true;
                stuckFor46[slot] = stuck ? stuckFor46[slot] + 1 : 0;
                if (stuckFor46[slot] >= stuckSteps46)
                {
                    b.alive = false; // ended: counted dead for the door, no kill event, no points (rooms43's end)
                    stuckEnded46++;
                    stuckId46[slot] = -1; stuckFor46[slot] = 0;
                }
            }
            for (int k = 0; k < stuckId46.Length; k++) if (stuckId46[k] >= 0 && !stuckSeen46[k]) { stuckId46[k] = -1; stuckFor46[k] = 0; } // dead or gone
        }

        partial void N46FlowCampaign()
        {
            if (rows.On(idRetry46)) campaign.retryWave46 = Retry46;
            if (rows.On(idOverlap46)) campaign.waveRelease46 = Release46;
            if (rows.On(idTeach46)) campaign.teachHold46 = Teach46;
        }

        /// <summary>retry46: `spawned` waves had come in when the player died; the one being fought is spawned - 1. The room restarts there
        /// (minus wavesBack), so the waves already cleared are not fought again (DESIGN-RULES A-4). Nothing spawned yet: 0 (the old rule).</summary>
        int Retry46(RoomDef r, int spawned) => Math.Max(0, spawned - 1 - retryBack46);

        /// <summary>overlap46: release wave `next` now, on top of the living? Only from wave fromWave on (wave 1 keeps the commit gate and the
        /// stand-ins), and only when the room's living bodies are at most bodiesLeft AND at most shareLeft of the wave just spawned (a 1-body
        /// wave is never overlapped: a lone tank's wave waits for its kill), and never a wave whose own nodes enter by the door (entryM).</summary>
        bool Release46(RoomDef r, int next, int alive)
        {
            if (next < overlapFromWave46 || next <= 0 || next >= r.WaveCount) return false;
            // a wave the room's design brings in by the entry (its own waveNodes43, one within entryM of the room's spawn: g7 THE THAW LOCK's
            // wave 3, its card's state change) waits for the full clear: stacked on the wave before it, it closes the way out behind the player
            System.Collections.Generic.List<Vec3> own = r.WaveNodes43(next);
            if (own != null)
                for (int k = 0; k < own.Count; k++)
                {
                    double dx = own[k].x - r.spawn.x, dz = own[k].z - r.spawn.z;
                    if (dx * dx + dz * dz < overlapEntry46 * overlapEntry46) return false;
                }
            int last = r.waves[next - 1].Count;
            // a wave more than growthMax x the one before is the room's next beat (g7 THE THAW LOCK: 3 -> 7; g3 THE SWARM: 3 -> 8): it waits for the
            // clear (n46flow: g7's deaths under overlap46 came with wave 2's seven on top of wave 1's last spitter)
            if (r.waves[next].Count > overlapGrowth46 * last) return false;
            int allow = Math.Min(overlapBodies46, (int)Math.Floor(last * overlapShare46));
            return alive <= allow;
        }

        /// <summary>teach46: each teaching hold at most its cap (the rooms43 number when that is shorter).</summary>
        int Teach46(int which, int steps43)
        {
            int cap = which == Campaign24.Hold46StandIn ? teachStandIn46 : which == Campaign24.Hold46AfterLesson ? teachAfter46 : teachCap46;
            return Math.Min(steps43, cap);
        }
    }
}
