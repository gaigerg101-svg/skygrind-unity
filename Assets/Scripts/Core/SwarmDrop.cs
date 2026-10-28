// NIGHT11 U3-24.3: THE SWARM ROUND'S BANNER AND DROP (design.zombies.swarmDrop). New design, not a web port: the web's
// swarm rounds (directorMath.isSwarmRound: 8, 13, 18 ...) start unannounced and end with nothing. CoD Zombies' dog rounds
// (NIGHT11-RESEARCH.md A.2 and C.4) are announced and their last kill always drops a Max Ammo; SKYGRIND has no ammo, so
// the drop is the web's kill-drop roll, forced: one power-up whose type comes from the run's power-up stream.
// - On `roundBegin` of a swarm round: the web's `banner {text}` and `swarmRound {round}` (port-only).
// - During that round: the last player kill's position is kept.
// - On that round's `roundClear`: PowerUps.Spawn(at the last kill) with no type (the run's stream draws it), then
//   `swarmDrop {round, type}` (port-only). A swarm round without a player kill drops nothing.
// Flag off (or no design config): no handler does anything (the web's swarm round). Nothing runs per step; the handlers
// allocate nothing (reused payloads; PowerUps.Spawn's own event is the web's).
namespace Skygrind.Core
{
    /// <summary>swarmRound {round} and swarmDrop {round, type} (port-only). Reused: read it inside the handler.</summary>
    public sealed class SwarmRoundEvent
    {
        public int round;
        public string type; // swarmDrop: the power-up type
        public Vec3 position; // swarmDrop: where it landed
    }

    public sealed class SwarmDrop : IRunState
    {
        // NIGHT13 U3-26.10: the run snapshot: the swarm round running (0: none), its last kill, the drops.
        public void Save(StateWriter w)
        {
            w.I("round", round);
            w.B("hasKill", hasKill);
            w.V("lastKill", lastKill);
            w.I("drops", drops);
        }

        public void Load(StateReader r)
        {
            round = r.Int("round");
            hasKill = r.B("hasKill");
            lastKill = r.V("lastKill");
            drops = r.Int("drops");
        }

        /// <summary>The banner a swarm round opens with (the web's `banner {text}`).</summary>
        public const string BannerText = "SWARM ROUND · THE LAST KILL DROPS A POWER-UP";

        /// <summary>design.zombies.swarmDrop: false (or no design config) means the web's swarm round.</summary>
        public static bool Enabled(GameConfig cfg) => cfg.Design("zombies.swarmDrop");

        readonly EventBus events;
        readonly PowerUps powerUps;
        readonly DirectorMath.D d;
        readonly BannerEvent banner = new BannerEvent { text = BannerText };
        readonly SwarmRoundEvent payload = new SwarmRoundEvent();

        /// <summary>design.zombies.swarmDrop, read once (a test may set it).</summary>
        public bool enabled;

        /// <summary>The swarm round running now (0: none).</summary>
        public int round;

        /// <summary>A player kill happened this swarm round; lastKill is where the last one fell.</summary>
        public bool hasKill;
        public Vec3 lastKill;

        /// <summary>Drops made this run.</summary>
        public int drops;

        public SwarmDrop(GameConfig cfg, EventBus events, PowerUps powerUps)
        {
            this.events = events;
            this.powerUps = powerUps;
            d = DirectorMath.From(cfg);
            enabled = Enabled(cfg);
            events.On<RoundEvent>("roundBegin", OnBegin);
            events.On<KillEvent>("kill", OnKill);
            events.On<RoundEvent>("roundClear", OnClear);
        }

        /// <summary>Is round n a swarm round (the web's directorMath.isSwarmRound)?</summary>
        public bool IsSwarmRound(int n) => DirectorMath.IsSwarmRound(n, d);

        /// <summary>A new run: no swarm round running, no drops.</summary>
        public void Reset()
        {
            round = 0;
            hasKill = false;
            drops = 0;
        }

        void OnBegin(RoundEvent e)
        {
            round = 0;
            hasKill = false;
            if (!enabled || !DirectorMath.IsSwarmRound(e.number, d)) return;
            round = e.number;
            events.Emit("banner", banner);
            payload.round = round;
            payload.type = null;
            payload.position = default;
            events.Emit("swarmRound", payload);
        }

        void OnKill(KillEvent e)
        {
            if (round == 0 || e.owner != "player") return;
            hasKill = true;
            lastKill = e.position;
        }

        void OnClear(RoundEvent e)
        {
            if (round == 0 || e.number != round) return;
            int r = round;
            round = 0;
            if (!enabled || !hasKill) return;
            hasKill = false;
            PowerUps.Drop drop = powerUps.Spawn(lastKill); // the type from the run's stream (powerUps.js spawn(at) with no type)
            drops++;
            payload.round = r;
            payload.type = drop.type;
            payload.position = drop.position;
            events.Emit("swarmDrop", payload);
        }
    }
}
