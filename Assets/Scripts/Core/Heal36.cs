// NIGHT36 builder P (heal36; NIGHT36.md card P1, DECIDE36 D-5 "heal by aggression", PLAN.md section 1): a kill of yours within radius m of
// your eye spills `amount` HP back to you at once. ULTRAKILL's blood heals on touch for 0.25 s after it spawns (Bloodsplatter.cs:92-176;
// rea-ultrakill-combat.md section 6): here the touch is the radius at the kill's step (a kill that close is a kill you are standing in).
// Health pickups stay (rule 1). Event "heal36" (Heal36Event) for GG's sound and the view's rising number. Numbers: design.n36.heal36.
// Off (SurvivalSim.Core36 never calls it): no heal, bit for bit.
namespace Skygrind.Core
{
    public sealed class Heal36Event
    {
        public string owner = "player";
        public double amount, distance, hp;
        public Vec3 position;
    }

    public sealed class Heal36
    {
        public const string Event = "heal36";

        public readonly double radius, amount;

        /// <summary>Counters (the harness, the reels): heals, HP given, kills too far to heal.</summary>
        public int heals, farKills;
        public double healed;

        readonly Heal36Event ev = new Heal36Event();

        public Heal36(GameConfig cfg)
        {
            radius = cfg.Num("design.n36.heal36.radius");
            amount = cfg.Num("design.n36.heal36.amount");
        }

        public void Reset()
        {
            heals = farKills = 0;
            healed = 0;
        }

        /// <summary>A kill of the player's at `at`, seen from `eye`: heals within the radius. Returns the HP restored. `scale` (NIGHT56 G1 rank56:
        /// the style rank's multiplier; 1 with the row off, and amount x 1 is amount bit for bit) multiplies the heal.</summary>
        public double OnKill(Vec3 at, Vec3 eye, Health health, EventBus events, string owner, double scale = 1)
        {
            double d = JsMath.Hypot(at.x - eye.x, at.y - eye.y, at.z - eye.z);
            if (d > radius)
            {
                farKills++;
                return 0;
            }
            double got = health.Heal(amount * scale);
            if (got <= 0) return 0;
            heals++;
            healed += got;
            ev.owner = owner;
            ev.amount = got;
            ev.distance = d;
            ev.hp = health.hp;
            ev.position = at;
            events.Emit(Event, ev);
            return got;
        }
    }
}
