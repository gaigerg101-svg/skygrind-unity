// NIGHT31 builder W (weapons): the sim side of the W blocks (Core/N31Blocks.cs), through the lead's partial hooks (SurvivalSim.N31.cs):
// - unlocks (item 3, design.n31.unlocks, Core/Unlocks26.cs Unlocks31): at a campaign run's first step the loadout becomes the level's start
//   set (level 1 the KESTREL only; the fist melee is not a slot and stays), and a room award (level 2's PULSE gauntlet in i1a) is given when
//   the player is in that room (Campaign24.Award31: weaponEarned once; the room's pickup is then taken without a second award). The sim does
//   this, not the render-side CampaignUnlocks26.Apply (which steps aside while Campaign24.unlocks31 is set), so a tape replays it.
// Off: nothing here runs (NIGHT30's loadout: CampaignUnlocks26.Apply's KESTREL + gauntlet + the profile's cleared levels).
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        Unlocks31 unlocks31Table;
        Campaign24 unlocks31For; // the campaign object the level's start set was applied to (one apply per run)

        /// <summary>NIGHT31 W: the weapons the level's start set gave this run (harness / reel lines; empty when the row is off).</summary>
        public readonly List<string> unlocks31Start = new List<string>();

        /// <summary>NIGHT31 W charge (item 2a, design.n31.charge): NIGHT27's charge rule (Core/Guns/ChargeShots27.cs) with design.n31.charge's numbers
        /// (0.4 s hold; KESTREL 0.5 s charge, pierce, 4 s cooldown; THUNDERCLAP 0.7 s, 6 s; a full charge held 0.5 s fires itself (P0 a); the slug's
        /// knock on husks only (R7); a melee press cancels (R8)). Created at the first run with the row on; the loadout's charge27 while on.</summary>
        public ChargeShots27 charge31;

        bool rows31Hooked;

        /// <summary>NIGHT31 W round 2: builder L's level rows for the new weapon order (lessons31 / standIns31 / waves31 / pickup31) replace the room's rows
        /// once the run has started (RunStarted fires after N24's StartRun built campaignLevel and before the first room is entered at step 1).</summary>
        void Rows31OnRunStarted()
        {
            if (n31.unlocks && campaignOn && campaignLevel != null) campaignLevel.UseUnlocks31();
        }

        partial void N31WeaponsStartRun()
        {
            if (!rows31Hooked) { rows31Hooked = true; RunStarted += Rows31OnRunStarted; }
            unlocks31For = null;
            unlocks31Start.Clear();
            // runs after N27WeaponsStartRun (SurvivalSim.Run.cs: N27StartRun, then N31StartRun): with the row on, n31's charge replaces n27's on the loadout
            if (n31.charge)
            {
                charge31 ??= new ChargeShots27(cfg, "design.n31.charge.");
                charge31.Reset(true);
                loadout.charge27 = charge31;
            }
            else charge31?.Reset(false);
            // ltAlways (item 2b, Core/Guns/LtAlways31.cs): every weapon's LT does something (zoom / detonate / blast / parry) or says why not
            if (n31.ltAlways)
            {
                lt31 ??= new LtAlways31(cfg);
                lt31.Reset(true);
                loadout.lt31 = lt31;
                loadout.bossParry31 = BossParry31;
            }
            else
            {
                lt31?.Reset(false);
                loadout.lt31 = null;
                loadout.bossParry31 = null;
            }
        }

        /// <summary>NIGHT31 round 2 (ltAlways): the gauntlet's parry on a BOSS projectile: the nearest live Lantern orb, Stag shard or Wyrm spit within `reach` m of
        /// `from` is removed and its boss takes the web's orb.reflectDamageMult (x2) of the projectile's damage as a "parry31" hit (the boss filters take it at full; the return is instant: no
        /// flying orb is drawn back). True when one was returned. The lists are builder B's systems' (read and one element removed / killed, no rule changed).</summary>
        bool BossParry31(Vec3 from, double reach)
        {
            double mult = cfg.Has("orb.reflectDamageMult") ? cfg.Num("orb.reflectDamageMult") : 2;
            double best = reach;
            int kind = -1, idx = -1;
            if (lantern27 != null && lantern27.Alive)
                for (int i = 0; i < lantern27.orbs.Count; i++)
                {
                    double d = Vec3.DistanceTo(lantern27.orbs[i].pos, from);
                    if (d <= best) { best = d; kind = 0; idx = i; }
                }
            if (stag31 != null && stag31.body.alive)
                for (int i = 0; i < stag31.shards.Count; i++)
                {
                    if (!stag31.shards[i].alive) continue;
                    double d = Vec3.DistanceTo(stag31.shards[i].pos, from);
                    if (d <= best) { best = d; kind = 1; idx = i; }
                }
            if (wyrm31 != null && wyrm24 != null && wyrm24.Alive)
                for (int i = 0; i < wyrm31.spits.Count; i++)
                {
                    if (!wyrm31.spits[i].alive) continue;
                    double d = Vec3.DistanceTo(wyrm31.spits[i].pos, from);
                    if (d <= best) { best = d; kind = 2; idx = i; }
                }
            if (kind < 0) return false;
            Enemy shooter;
            double dmg;
            Vec3 at;
            if (kind == 0) { at = lantern27.orbs[idx].pos; lantern27.orbs.RemoveAt(idx); shooter = lantern27.body; dmg = lantern27.p.orbDamage; }
            else if (kind == 1) { at = stag31.shards[idx].pos; stag31.shards[idx].alive = false; shooter = stag31.body; dmg = stag31.p.shardDamage; }
            else { at = wyrm31.spits[idx].pos; wyrm31.spits[idx].alive = false; shooter = wyrm24.body; dmg = wyrm31.p.spitDamage; }
            bossParryDamage31 += dmg * mult;
            loadout.Damage(shooter, dmg * mult, false, "parry31", at); // the lead 12:18: a return always hurts its boss: weapon id parry31, which the boss filters (builder B) take at full
            events.Emit("parry", new HitEvent { owner = "player", weapon = "gauntlet", kind = kind == 0 ? "lanternOrb" : kind == 1 ? "stagShard" : "wyrmSpit", enemy = shooter.id, damage = dmg * mult, position = at });
            return true;
        }

        /// <summary>NIGHT31: the damage the boss parries sent back this process (the harness reads it).</summary>
        public double bossParryDamage31;

        /// <summary>NIGHT31 W ltAlways: the LT rule's state (created at the first run with the row on).</summary>
        public LtAlways31 lt31;

        partial void N31WeaponsStep(double dt)
        {
            if (n31.charge && !n27.pulse27 && !n27.chargeShots27)
            {
                loadout.StepCharge27Clock(); // N27WeaponsStep returns early with both n27 rows off: the clock and the swap guard run here
                loadout.Charge27WeaponGuard();
            }
            if (n31.ltAlways && Active) loadout.StepLt31(lastCombat); // after N27WeaponsStep (the glove's LT: blast, else the parry)
            N31UnlocksStep();
        }

        void N31UnlocksStep()
        {
            // f8's standIns31 (a line for the charged KESTREL's pierce): its stand-ins take a charged shot from the floor while the player is in that room
            loadout.standInPierce31 = n31.unlocks && campaignOn && campaign != null && campaignLevel != null && campaign.room >= 0 && campaign.room < campaignLevel.rooms.Count
                && campaignLevel.rooms[campaign.room].standIns31 != null;
            if (!n31.unlocks || !campaignOn || campaign == null || campaignLevel == null) return;
            if (unlocks31Table == null) unlocks31Table = Unlocks31.From(cfg);
            if (!ReferenceEquals(campaign, unlocks31For))
            {
                unlocks31For = campaign;
                campaign.unlocks31 = true;
                List<string> start = N40UnlocksBegin(N41BaseId(campaignLevel.id), unlocks31Table.StartFor(N40TableId(N41BaseId(campaignLevel.id)))); // NIGHT40: a level row's start set, loom40's drops (SurvivalSim.N40.cs; all off: the table's); NIGHT41: an n41 level reads its levelFrom's (N41BaseId)
                if (start != null)
                {
                    foreach (string w in start) if (!loadout.owned.Contains(w)) loadout.AddWeapon(w);
                    loadout.FilterOwned26(start);
                    unlocks31Start.AddRange(start);
                }
            }
            N40RoomAwardsStep(); // NIGHT40: a level row's room awards (none with every NIGHT40 block off)
            var (weapon, roomId) = unlocks31Table.RoomAward(N40TableId(N41BaseId(campaignLevel.id))); // NIGHT41: an n41 level reads its levelFrom's awards
            if (weapon != null && N40Dropped(weapon)) return; // NIGHT40 loom40: the STITCHER is out of the campaign
            if (weapon == null || campaign.room < 0 || campaign.room >= campaignLevel.rooms.Count || campaignLevel.rooms[campaign.room].id != roomId) return;
            if (campaign.Given31(weapon)) return;
            if (loadout.owned.Contains(weapon)) campaign.MarkGiven31(weapon); // already held (a stage reset cleared the campaign's set): no second banner
            else campaign.Award31(weapon);
        }
    }
}
