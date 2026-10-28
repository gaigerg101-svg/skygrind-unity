// NIGHT40 builder S1 (row design.n40.start40, N40Blocks bit 0; GG 2026-10-13: "the game needs a short delay or a required movement trigger
// before the first wave of enemies appears"; NIGHT40.md S1): the start grace.
// - The hold: the first wave of a campaign level (wave 0 of the first room with waves) and, with afterRespawn, wave 0 of a room retried after a
//   death wait (Campaign24.waveReady40) until the player's feet are moveMetres (3D) from where they stood when the hold began (the level's
//   spawn, the respawn point) OR holdSeconds pass, whichever is first.
// - The window: the held wave's enemies cannot start an attack for noAttackSeconds after their spawn tell ends (design.n33.spawns33's
//   tellSeconds when spawns33 is on and the room is not a retry's instant wave). A husk-family body (husk, tank, spitter, swarmer: they all
//   wait for Husk.cooldownSteps == 0 before a wind-up) gets it as its attack cooldown and keeps moving; any other kind is stunned for it.
//   The wave's bodies may reach the world over several steps (Level36's arena queue): the window counts them down from the wave's size.
// Events (for GG's sounds): start40Release ("move" | "timer"), when a held wave is let in.
// Root cause (Campaign24.cs Enter: gapLeft = 0, so StepFight spawns wave 0 on the entry step): before this row, level 3's and 4's first
// enemies stand in the room on the first live step (`n40start` measures it per level).
// Off (the row off, --off40 and every older switch): nothing subscribes, waveReady40 stays null: NIGHT39's game bit for bit.
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        const string Start40Root = "design.n40.start40.";

        bool s40On, s40Hooked, s40AfterRespawn, s40LevelHeld;
        int s40HoldSteps, s40NoAttackSteps;
        double s40MoveMetres;
        Campaign24 s40Campaign;

        // the hold in progress (one at a time: wave 0 of room s40Room)
        bool s40Armed;
        int s40Room = -1, s40Steps;
        Vec3 s40Anchor;

        // the released wave whose spawns get the window: its enemies may reach the world over several steps (Level36's arena queue, the
        // spawns33 tells), so the window counts them down (or ends when the room changes or a later wave starts)
        bool s40Grace;
        int s40GraceRoom = -1, s40GraceLeft;

        /// <summary>The harness's counts this run: holds begun, released by movement, released by the timer, enemies given the window.</summary>
        public int start40Holds, start40ByMove, start40ByTimer, start40Windowed;
        /// <summary>The harness's view: seconds the last hold lasted (-1: none yet).</summary>
        public double start40LastHold = -1;

        partial void N40StartStartRun()
        {
            s40On = n40.start40;
            s40Armed = s40Grace = s40LevelHeld = false;
            s40Room = s40GraceRoom = -1;
            s40Steps = 0;
            s40Campaign = null;
            start40Holds = start40ByMove = start40ByTimer = start40Windowed = 0;
            start40LastHold = -1;
            Kiln40StartRun(); // S1's kiln40 boost chain (SurvivalSim.N40.Kiln.cs)
            if (!s40On) return;
            s40MoveMetres = cfg.Num(Start40Root + "moveMetres");
            s40HoldSteps = MovementMath.StepsFor(cfg.Num(Start40Root + "holdSeconds"), step);
            s40NoAttackSteps = MovementMath.StepsFor(cfg.Num(Start40Root + "noAttackSeconds"), step);
            s40AfterRespawn = cfg.Design("n40.start40.afterRespawn");
            if (!s40Hooked)
            {
                s40Hooked = true; // only ever subscribed with the row on; every handler checks the run's bit
                events.On<RoomEvent>("roomEnter", e => Start40Room(e.room, false));
                events.On<RoomEvent>("roomRetry", e => Start40Room(e.room, true));
                events.On<SpawnEvent>("enemySpawn", e => Start40Spawned(e.id));
            }
        }

        /// <summary>The campaign is made on the run's first live step and stepped before N40Step: the hook goes in from the room event that
        /// Enter / Retry emit (inside campaign.Step, before StepFight asks), and again here for safety.</summary>
        void Start40Hook()
        {
            if (campaign == null || campaign == s40Campaign) return;
            s40Campaign = campaign;
            campaign.waveReady40 = Start40Ready;
        }

        /// <summary>A room entered (or retried): arm the hold for its wave 0 when this is the level's first room with waves, or a retry.</summary>
        void Start40Room(int room, bool retry)
        {
            if (!s40On || campaign == null || campaignLevel == null) return;
            Start40Hook();
            s40Armed = false;
            s40Grace = false;
            if (room < 0 || room >= campaignLevel.rooms.Count) return;
            RoomDef r = campaignLevel.rooms[room];
            if (r.kind == "boss" || r.WaveCount == 0) return;
            if (retry ? !s40AfterRespawn : s40LevelHeld) return;
            s40LevelHeld = true;
            s40Armed = true;
            s40Room = room;
            s40Steps = 0;
            s40Anchor = mover.position; // Enter: where the feet stand on the entry step (the level's spawn); Retry: the respawn point
            start40Holds++;
        }

        bool Start40Ready(RoomDef r, int w)
        {
            if (!s40On || !s40Armed || w != 0 || campaign == null || campaign.room != s40Room) return true;
            Vec3 f = mover.position;
            bool moved = JsMath.Hypot(f.x - s40Anchor.x, f.y - s40Anchor.y, f.z - s40Anchor.z) >= s40MoveMetres;
            bool timed = s40Steps >= s40HoldSteps;
            if (!moved && !timed) return false;
            s40Armed = false;
            if (moved) start40ByMove++;
            else start40ByTimer++;
            start40LastHold = s40Steps * step;
            s40Grace = true; // the wave's enemySpawn events (this StepFight, or a queue's later steps) get the window
            s40GraceRoom = s40Room;
            s40GraceLeft = r.waves[0].Count;
            events.Emit("start40Release", moved ? "move" : "timer");
            return true;
        }

        void Start40Spawned(int id)
        {
            if (!s40On || !s40Grace || campaign == null || campaign.room != s40GraceRoom || campaign.Wave33 > 1) return;
            Enemy e = null;
            List<Enemy> all = enemies.all;
            for (int i = all.Count - 1; i >= 0 && e == null; i--) if (all[i].id == id) e = all[i];
            if (e == null) return;
            int steps = s40NoAttackSteps + (s33On && !s33RetryInstant ? s33TellSteps : 0);
            System.Type t = e.GetType();
            if (e is Husk h && (t == typeof(Husk) || t == typeof(Spitter) || t == typeof(Swarmer))) h.cooldownSteps = System.Math.Max(h.cooldownSteps, steps);
            else e.Stun(steps * step, step);
            start40Windowed++;
            if (--s40GraceLeft <= 0) s40Grace = false;
        }

        partial void N40StartStep(double dt)
        {
            Kiln40Step(); // S1's kiln40 boost chain (SurvivalSim.N40.Kiln.cs)
            if (!s40On) return;
            Start40Hook();
            if (s40Armed) s40Steps++;
            if (s40Grace && (campaign == null || campaign.room != s40GraceRoom || campaign.Wave33 > 1)) s40Grace = false; // the room left, or a later wave started
        }
    }
}
