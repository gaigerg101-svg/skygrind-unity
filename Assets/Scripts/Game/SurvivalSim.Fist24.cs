// NIGHT24 (builder F): the sim's half of the charged fist (design.n24.chargeFist; the rule is Core/Fist24.cs) and of the BIG PICK
// (design.n24.bigPick; the rule is Core/BigPick24.cs, the flow is Mods's). Here (not in a Game file of its own) because SimHarness compiles
// Game/SurvivalSim*.cs. The three partial hooks are the ones SurvivalSim.N24.cs placed: N24FistStartRun / N24FistStep / N24BigPickStartRun.
// Off (the row, or --off24 and older): nothing is wired and nothing is queued: NIGHT22's game bit for bit.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>NIGHT24: the charged fist (null until a run starts with the block on).</summary>
        public Fist24 fist24;

        /// <summary>NIGHT24: the BIG PICK (null until a run starts with the block on).</summary>
        public BigPick24 bigPick24;

        bool layerPickQueued; // the campaign's layer-end BIG PICK was queued this run

        bool bigBoxTake; // a BIG PICK spin is running: the gun is taken when the reveal ends

        partial void N24FistStartRun()
        {
            if (!n24.chargeFist && !n25.pulseCharge && !n26.pulseLT) // NIGHT25: the pulse row also needs the charge object
            {
                fist24?.Reset(false);
                N25PulseWire();
                return;
            }
            fist24 ??= new Fist24(cfg, events, mover, world, () => GrappleAim.AimDirection(input.Yaw, input.Pitch));
            fist24.Reset(true);
            N25PulseWire(); // NIGHT25 pulseCharge: pulse mode on / off (SurvivalSim.Combat25.cs)
        }

        partial void N24FistStep(double dt)
        {
            fist24?.Step();
            if (bigBoxTake && box != null && box.state == BoxState.Offering)
            {
                // The BIG PICK's free spin: the reveal ended on a gun (boxRoll was emitted); it is put in your hand, as TAKE does.
                bigBoxTake = false;
                string w = box.Take();
                if (w != null) loadout.AddWeapon(w);
            }
        }

        partial void N24BigPickStartRun()
        {
            bigBoxTake = false;
            layerPickQueued = false;
            bool legacy = cfg.Design("mods.legacy") || cfg.Design("n24.bigPick.legacy"); // the old percentage draft, kept behind a switch that is off
            bool want = n24.bigPick && !legacy;
            if (want && bigPick24 == null)
            {
                bigPick24 = new BigPick24(cfg, events);
                BigPick24 b = bigPick24;
                b.economy = economy;
                b.health = health;
                b.boxAvailable = () => box != null && box.CanBuy && box.PoolCount(loadout.owned, fires) > 0;
                b.boxSpin = () =>
                {
                    if (box == null || !box.Roll(loadout.owned, fires, 0, "player", true)) return false;
                    bigBoxTake = true;
                    return true;
                };
                b.packTarget = () =>
                {
                    string held = loadout.Weapon;
                    if (!string.IsNullOrEmpty(held) && forge.CanUpgrade(held)) return held;
                    foreach (string id in loadout.owned) if (forge.CanUpgrade(id)) return id;
                    return null;
                };
                b.pack = id => forge.Upgrade(id, 0);
                b.perkTarget = () =>
                {
                    foreach (string id in cfg.Strs("perks.order")) if (Perks.Ported.Contains(id) && !perks.Owns(id)) return id;
                    return null;
                };
                b.perk = id => perks.Give(id, 0);
                b.fistAvailable = () => fist24 != null && fist24.Active;
                b.fistFull = () => fist24?.Fill();
            }
            if (bigPick24 != null) bigPick24.WantsOn = want;
            mods.big = bigPick24;
        }

        /// <summary>NIGHT24 (builder F, D-7): the CAMPAIGN's one BIG PICK at a layer's end. Campaign24's finish calls this each step instead of ending the
        /// run at once: true while the pick is queued, open or being taken (the run waits), false when there is none (the block off, design.n24.bigPick.campaign
        /// off, or the pick is taken). The game opens it without a break (Mods.LayerPick).</summary>
        public bool CampaignBigPickHold()
        {
            if (bigPick24 == null || !bigPick24.Active || !cfg.Design("n24.bigPick.campaign")) return false;
            if (!layerPickQueued)
            {
                layerPickQueued = true;
                mods.QueueLayer();
                return true;
            }
            return mods.Pending || mods.offer != null;
        }
    }
}
