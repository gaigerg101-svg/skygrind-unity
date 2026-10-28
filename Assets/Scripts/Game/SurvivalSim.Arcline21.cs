// NIGHT21 U3-35.3 (builder M owns this file): THE ARCLINE in Survival (design.n21.arcline; the rule is Core/Arcline21.cs).
// Here (not in a Game file of its own) because SimHarness compiles Game/SurvivalSim*.cs: the harness plays it.
// - A run with the block on: the terminal (gadgets.js; the web's spot, data.gadgetSpots, the west island of ISLANDS) is in the
//   buy list: hold BUY for arcline.price (5500), open only while its zone is open, once per run. Owning it sets
//   mover.ropeEnabled (ownership IS ropeEnabled, which the run snapshot's Mover section already saves).
// - The pick runs inside the Mover's step (Mover.aimHook): a yank, a zip, a node or a surface (see Core/Arcline21.cs).
// - A map with no gadget spot (a Unity-authored one) has no terminal: no ARCLINE there (logged by Arcline21Spot).
// Off (the row, or --off21 / --off20 / --off19): nothing is wired, ropeEnabled stays false: NIGHT20's game bit for bit.
// Not saved by the run snapshot: the yank's cooldown (a break loses it; the swing's own cooldown is the Mover's).
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>NIGHT21: the ARCLINE's rule and wiring (null until a run starts with the block on).</summary>
        public Arcline21 arcline21;

        Buyables.Item arcline21Item;

        /// <summary>The terminal's spot (data.gadgetSpots, the "arcline" one), or null: this map has none.</summary>
        public PerkSpot? Arcline21Spot()
        {
            foreach (PerkSpot s in map.gadgetSpots) if (s.perk == "arcline") return s;
            return null;
        }

        partial void N21ArclineStartRun()
        {
            SlideBite21StartRun(); // SLIDE BITES' run start (SurvivalSim.SlideBite21.cs; M's one StartRun hook)
            if (arcline21Item != null)
            {
                buyables.Remove(arcline21Item);
                arcline21Item = null;
            }
            if (!n21.arcline)
            {
                arcline21?.Reset(false);
                return;
            }
            arcline21 ??= new Arcline21(cfg, events, mover, enemies, new GrappleAim(cfg.movement, map.grappleNodes), world, EyeNow);
            arcline21.Reset(true);
            PerkSpot? spot = Arcline21Spot();
            if (!spot.HasValue) return;
            PerkSpot sp = spot.Value;
            int zone = ZoneMath.ZoneIndexAt(map.zoneParts, sp.pos.x, sp.pos.y + 1, sp.pos.z);
            Arcline21 a = arcline21;
            arcline21Item = new Buyables.Item
            {
                id = "gadget:arcline", kind = "gadget", name = "ARCLINE", position = sp.pos, reach = cfg.Num("perks.reach"),
                price = () => a.rules.price,
                available = () => zone < 0 || !zones.LockedTag(zone),
                owned = () => a.Owned,
                buy = () =>
                {
                    a.Own(true);
                    events.Emit("gadget", "arcline");
                },
            };
            buyables.list.Add(arcline21Item);
        }

        partial void N21ArclineStep(double dt) => arcline21?.Step();

        /// <summary>The eye the hook is picked from: the mover's feet now (the Mover is mid-step: the sim's `current` is last step's end).</summary>
        Vec3 EyeNow() => new Vec3(mover.position.x, mover.position.y + cfg.movement.body.eyeHeight - (mover.crouched ? cfg.movement.slide.cameraDrop : 0), mover.position.z);
    }
}
