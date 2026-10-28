// NIGHT14 U3-27 (the shared types): the bot's contract (src/bot/botBrain.js createBotBrain's deps and its InputFrame),
// placed before the port so the Game side (the AUTOPILOT, the attract loop, the farm) builds against it while agent N
// ports the brain. Nothing here is a web module: BotBrain.cs (agent N) is the line-for-line port and implements IBotBrain.
// The web's one InputFrame is split the way the port splits it: the movement (Mover.cs InputFrame) and the fire and
// weapon fields (Combat.cs CombatInput's).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>botBrain.js's frame: what step() writes. `move` is the Mover's frame (moveX / moveZ / yaw / pitch / jump / dash /
    /// slide); `fire` is frame.fire (held + pressed); `weapon` is frame.weapon (the slot asked for; step always writes it).</summary>
    public struct BotFrame
    {
        public InputFrame move;
        public Button fire;
        public int weapon;
    }

    /// <summary>getPlayer(): the target as the brain perceives it ({position, velocity, health.dead, crouched}). One reused
    /// instance per adapter: nothing is allocated per step.</summary>
    public sealed class BotTarget
    {
        public Vec3 position;
        public Vec3 velocity;
        public bool dead;
        public bool crouched;
    }

    /// <summary>getOrbs()'s entries ({position, active}): a health orb RECOVER can run to.</summary>
    public struct BotOrb
    {
        public Vec3 position;
        public bool active;
    }

    /// <summary>brain.look: where the bot looks (and shoots). rad; yaw 0 looks toward -z, + pitch looks up.</summary>
    public sealed class BotLook
    {
        public double yaw, pitch;
    }

    /// <summary>getCollider().rayIntersect(ray): GameWorld.RayIntersect's shape (a unit direction; hit.distance in m).</summary>
    public delegate bool BotRay(Vec3 origin, Vec3 direction, out RayHit hit);

    /// <summary>createBotBrain's `self`: the body the brain drives.</summary>
    public sealed class BotSelf
    {
        public Mover mover;
        public Health health;
        /// <summary>self.loadout.weapon: the id of the weapon in hand ("revolver", "shotgun", ...).</summary>
        public Func<string> weapon;
    }

    /// <summary>createBotBrain's deps: getPlayer, getCollider (as a ray), getGraph, getOrbs, rng.</summary>
    public sealed class BotDeps
    {
        public Func<BotTarget> getPlayer;
        public BotRay ray;
        /// <summary>The nav graph, or null (planTo then fails; brain.directChase's arena has none).</summary>
        public Func<NavGraph> getGraph;
        /// <summary>getOrbs(): read in place each step (empty: no orbs; the web's attract passes []).</summary>
        public readonly List<BotOrb> orbs = new List<BotOrb>();
        /// <summary>Math.random in the web: here a Mulberry32 stream seeded from the run seed (NIGHT14 rule 8).</summary>
        public Mulberry32 rng;
    }

    /// <summary>The brain as the Game drives it (NIGHT14.md section 4: Step, Look, State, Reset).</summary>
    public interface IBotBrain
    {
        /// <summary>brain.difficulty: "easy" | "normal" | "hard" (config.bot's tables).</summary>
        string Difficulty { get; }

        /// <summary>brain.state: "chase" | "engage" | "evade" | "recover".</summary>
        string State { get; }

        BotLook Look { get; }

        /// <summary>brain.visible: the target was in line of sight at the last check.</summary>
        bool Visible { get; }

        /// <summary>brain.reset(difficulty): a new round.</summary>
        void Reset(string difficulty);

        /// <summary>brain.step(frame, dt): perceive, decide, write `frame` (the caller passes a fresh neutral frame).</summary>
        void Step(ref BotFrame frame, double dt);

        /// <summary>brain.onPlayerFire(origin, dir): the target fired (EVADE).</summary>
        void OnPlayerFire(Vec3 origin, Vec3 dir);

        /// <summary>brain.clearPath(): drop the path (a teleport); the next step replans.</summary>
        void ClearPath();
    }
}
