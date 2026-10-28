// NIGHT19 U3-33.6 (builder W): the `altUse` rule: when a bot holds each gun's LT (alt-fire). One rule, read by the
// AUTOPILOT (Game/Bot/AutopilotGuns.cs) and by THE RANGE's aim model (the lead's harness), never a harness-only rule
// (NIGHT19.md "How tonight works" rule 10). The loadout fills the situation (Loadout.AltSituation: its own projectiles,
// nails, the compress, the shocked bodies); the numbers are design.arsenal.altUse (Unity-only: the web's bot never uses
// an alt). The rule returns whether LT is held this step; the caller makes the press edge from the last step's value.
// - KESTREL (design.gust): the gust when bodies crowd in (crowd or more within near m) or one winds up within near m.
// - THUNDERCLAP: hold to compress with a crowd within near m; let go at a full compress (the double blast).
// - STITCHER: detonate once the target carries detonateNails nails, or nailedBodies bodies carry nails.
// - COMET: steer while a rocket of yours flies.
// - LONGWAVE: overcharge while no body is within near m (the long charge needs space).
// - ZAPPER: discharge with shocked or more SHOCKED bodies in sight.
// - BOUNCER: airburst once a grenade of yours is within its blast radius of a body.
// NIGHT20 (builder G): tonight's LT tools. The THUNDERCLAP's shockwave (design.shockwave) is the panic button: pressed
// with crowd or more bodies within near m (or one winding up there), only while a charge is ready (it holds 18 s). A
// zoom gun (Loadout.ZoomGun > 1: the KESTREL once the gust is off, the STITCHER, the ZAPPER with design.zoom) zooms on a
// target at least zoomMin m away with nothing within near m (the zoom costs awareness); the LONGWAVE scopes whenever
// nothing is close (its old LT rule, the overcharge, needed the same space). The zoom guns' old LT jobs move to SPECIAL
// (Special: the STITCHER's detonate, the ZAPPER's discharge), as ads.js routes them.
namespace Skygrind.Core
{
    /// <summary>A bot's situation for the alt rule (Loadout.AltSituation fills it; no allocation).</summary>
    public struct AltSituation
    {
        public string weapon;
        public double targetDistance; // m to the bot's target (+infinity: none)
        public int bodiesNear; // live bodies within altUse.near m of the player
        public bool windupNear; // a body within altUse.near m is in its wind-up
        public double compress; // the THUNDERCLAP's compress 0..1
        public int targetNails, nailedBodies; // the STITCHER's nails in the target; bodies with nails
        public int liveRockets; // the COMET's rockets in flight
        public bool grenadeByBody; // a BOUNCER grenade within its blast radius of a body
        public int shockedInSight; // SHOCKED bodies the discharge would reach
        public bool gust; // design.gust: the KESTREL's alt is the gust
        public bool shockwave, shockwaveReady; // NIGHT20 design.shockwave: the THUNDERCLAP's LT, and a charge is ready
        public double zoom; // NIGHT20: the gun's zoom (Loadout.ZoomGun; 0: its LT is a unique alt)
        public double wallFrom, wallTo; // NIGHT20 B5: the TORCH's fire wall burns from wallFrom to wallTo m ahead
        public int vortexGathered; // NIGHT20 B5: live bodies inside the burst radius of your open VORTEX orbs
        public bool vortexPressing; // NIGHT20 B5: LT is already down on the VORTEX (its release collapses; held on, it repels)
    }

    public sealed class AltUseParams
    {
        public double near, parryRange, zoomMin;
        public int crowd, detonateNails, nailedBodies, shocked;

        public static AltUseParams From(GameConfig cfg)
        {
            if (!cfg.Has("design.arsenal.altUse")) return null;
            string s = "design.arsenal.altUse.";
            return new AltUseParams
            {
                near = cfg.Num(s + "near"), crowd = cfg.Int(s + "crowd"), detonateNails = cfg.Int(s + "detonateNails"), zoomMin = cfg.Num(s + "zoomMin"),
                nailedBodies = cfg.Int(s + "nailedBodies"), shocked = cfg.Int(s + "shocked"), parryRange = cfg.Num(s + "parryRange"),
            };
        }
    }

    public static class AltUse
    {
        /// <summary>Is the alt (LT) held this step?</summary>
        public static bool Hold(in AltSituation s, AltUseParams p)
        {
            if (p == null) return false;
            // NIGHT20: a zoom gun's LT zooms (on a target zoomMin m or more away; the LONGWAVE whenever nothing is close)
            if (s.zoom > 1) return s.bodiesNear == 0 && (s.weapon == "rail" || (s.targetDistance >= p.zoomMin && !double.IsPositiveInfinity(s.targetDistance)));
            switch (s.weapon)
            {
                case "revolver": return s.gust && (s.bodiesNear >= p.crowd || s.windupNear);
                case "shotgun": return s.shockwave ? s.shockwaveReady && (s.bodiesNear >= p.crowd || s.windupNear) : s.bodiesNear >= p.crowd && s.compress < 1;
                case "nailgun": return s.targetNails >= p.detonateNails || s.nailedBodies >= p.nailedBodies;
                case "rocket": return s.liveRockets > 0;
                case "rail": return s.bodiesNear == 0;
                case "zapper": return s.shockedInSight >= p.shocked;
                case "bouncer": return s.grenadeByBody;
                case "torch": return s.targetDistance >= s.wallFrom && s.targetDistance <= s.wallTo; // NIGHT20 B5: the fire wall where it burns the target
                case "vortex": return !s.vortexPressing && s.vortexGathered >= p.crowd; // NIGHT20 B5: a tap (the release collapses; held on, it would repel)
                default: return false;
            }
        }

        /// <summary>NIGHT20: is SPECIAL pressed this step? The zoom guns' old LT jobs (the STITCHER's detonate, the ZAPPER's
        /// discharge); never on a gun whose LT is still its special (Hold covers it).</summary>
        public static bool Special(in AltSituation s, AltUseParams p)
        {
            if (p == null || !(s.zoom > 1)) return false;
            if (s.weapon == "nailgun") return s.targetNails >= p.detonateNails || s.nailedBodies >= p.nailedBodies;
            if (s.weapon == "zapper") return s.shockedInSight >= p.shocked;
            return false;
        }

        /// <summary>The parry punch (every gun): a body within altUse.parryRange m of `me` is in its wind-up.</summary>
        /// <summary>NIGHT31 R8 (design.n31.ltAlways): with the gauntlet in hand its LT is the only parry, so the punch parry is never wanted then.</summary>
        public static bool ParryWanted(Loadout lo, Vec3 me, AltUseParams p)
        {
            if (lo.lt31 != null && lo.lt31.active && lo.Weapon == "gauntlet") return false;
            return ParryWanted(lo.Enemies, me, p);
        }

        public static bool ParryWanted(EnemyRegistry enemies, Vec3 me, AltUseParams p)
        {
            if (p == null) return false;
            foreach (Enemy e in enemies.all)
                if (e.alive && e.windupSteps > 0 && Vec3.DistanceTo(e.position, me) <= p.parryRange) return true;
            return false;
        }
    }

    public sealed partial class Loadout
    {
        AltUseParams altParams;

        /// <summary>design.arsenal.altUse, read once (null without it: no alt, no parry punch).</summary>
        public AltUseParams AltParams => altParams ??= AltUseParams.From(cfg);

        /// <summary>The bodies this loadout fights (the alt rule's and the parry punch's).</summary>
        public EnemyRegistry Enemies => enemies;

        /// <summary>
        /// The situation for the alt rule (AltUse.Hold): the gun in hand, `target` (null: none), the bodies within `near` m
        /// of the player, this loadout's projectiles and nails, the shocked bodies in the discharge's reach.
        /// </summary>
        public AltSituation AltSituation(Enemy target, double near)
        {
            var s = new AltSituation { weapon = Weapon, targetDistance = double.PositiveInfinity, compress = compress, gust = GustOn };
            s.shockwave = ShockwaveOn; // NIGHT20: the THUNDERCLAP's LT, and its charge
            s.shockwaveReady = s.shockwave && shockwave.Ready;
            s.zoom = ZoomGun(s.weapon); // NIGHT20: the gun's zoom (0: a unique LT)
            s.wallFrom = twAhead; // NIGHT20 B5: the TORCH's wall strip (the web's numbers)
            s.wallTo = twAhead + twLength;
            s.vortexPressing = VortexPressing; // NIGHT20 B5: the VORTEX's press is in (let go: a tap)
            Vec3 me = mover.position;
            if (target != null && target.alive) s.targetDistance = Vec3.DistanceTo(target.position, me);
            var (origin, _) = getAim();
            foreach (Enemy e in enemies.all)
            {
                if (!e.alive) continue;
                if (Vec3.DistanceTo(e.position, me) <= near)
                {
                    s.bodiesNear++;
                    if (e.windupSteps > 0) s.windupNear = true;
                }
                if (s.weapon == "zapper" && arsenal.enabled && arsenal.HasStatus(e, "shock") && Vec3.DistanceTo(e.AimPoint, origin) <= zAltRange) s.shockedInSight++;
            }
            if (projectiles != null)
            {
                if (s.weapon == "nailgun")
                {
                    if (target != null) s.targetNails = projectiles.StuckOn(target, this);
                    foreach (Enemy e in enemies.all) if (e.alive && e.ConductiveNails > 0) s.nailedBodies++;
                }
                if (s.weapon == "rocket") s.liveRockets = projectiles.Count("rocket", this);
                if (s.weapon == "bouncer") s.grenadeByBody = projectiles.GrenadeByBody(this);
                if (s.weapon == "vortex") s.vortexGathered = projectiles.VortexGathered(this); // NIGHT20 B5
            }
            return s;
        }
    }
}
