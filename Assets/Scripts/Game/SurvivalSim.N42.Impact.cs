// NIGHT42 builder F1 (impact42; G9 "hits and kills need to feel more satifying", G15 "a greater or mor epowerful effect when hitting it or
// any other enemy iwth gaughtlent"): the sim's half of the hit / kill feel. Hooks and events only: nothing here changes a sim result.
// With n42.impact42 on, every `hit` of the player's (not a wave's share) is read once on the bus and turned into:
//   - `gauntletHit42` (Core/ImpactRules.cs GauntletHit42Event) for a gauntlet hit: the struck body's class (Small, Big = a heavy41 kind,
//     Boss = Loadout.IsBoss34), killed, the aim point. The impact layer freezes on it (ImpactRules.OnGauntletHit42), the view draws the
//     shockwave ring and the camera kick (Game/KillFeel42.cs), B1 hangs THE ICEBREAKER's own answer on it, GG's sounds on its name.
//   - `killTier42` (KillTier42Event) for a kill: Plain / Heavy / Melee / Big (U1's style reward; F1's kill burst).
// Both payloads are reused (no allocation per hit). Off: the handler returns at once; nothing is emitted (the fingerprint stays EQUAL).
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        bool impact42Hooked;
        readonly GauntletHit42Event gauntletHit42 = new GauntletHit42Event();
        readonly KillTier42Event killTier42 = new KillTier42Event();
        double impact42HeavyShotgun, impact42RocketReach;
        int impact42MultiCount, impact42Shot = -1, impact42ShotKills;

        /// <summary>NIGHT42 F1: this run's gauntlet hits by class (Small, Big, Boss) and kills by tier (index = KillTier42Event tier).</summary>
        public readonly int[] gauntletHits42 = new int[3], killTiers42 = new int[5];

        partial void N42ImpactStartRun()
        {
            if (!impact42Hooked)
            {
                impact42Hooked = true;
                impact42HeavyShotgun = cfg.Num("design.impact.freeze.heavyPellets") * cfg.Num("weapons.shotgun.pelletDamage");
                impact42RocketReach = cfg.Num("husk.aimHeight");
                impact42MultiCount = (int)cfg.Num("design.impact.slowMo.multiKill.count");
                events.On<HitEvent>("hit", Impact42OnHit); // once: the handlers are allocated here, never per hit
                events.On<KillEvent>("kill", Impact42OnKill);
            }
            impact42Shot = impact42LastEnemy = impact42LastShot = -1;
            impact42ShotKills = 0;
            for (int i = 0; i < gauntletHits42.Length; i++) gauntletHits42[i] = 0;
            for (int i = 0; i < killTiers42.Length; i++) killTiers42[i] = 0;
        }

        Enemy Impact42Enemy(int id)
        {
            var all = enemies.all;
            for (int i = 0; i < all.Count; i++) if (all[i].id == id) return all[i];
            return null;
        }

        /// <summary>NIGHT42 F1: a body's class for the gauntlet (GauntletHit42Event.Small / Big / Boss); a body gone from the registry reads by kind.</summary>
        public int Impact42Class(int enemyId, string kind)
        {
            Enemy e = Impact42Enemy(enemyId);
            if (e != null) return Loadout.IsBoss34(e) ? GauntletHit42Event.Boss : loadout.IsHeavy41(e) ? GauntletHit42Event.Big : GauntletHit42Event.Small;
            return kind == "brawler" ? GauntletHit42Event.Boss : GauntletHit42Event.Small;
        }

        void Impact42OnHit(HitEvent e)
        {
            if (!n42.impact42 || e.owner != "player" || e.shock) return;
            bool gauntlet = e.weapon == "gauntlet";
            if (!gauntlet && !e.killed) return;
            int cls = Impact42Class(e.enemy, e.kind);
            if (gauntlet)
            {
                gauntletHit42.enemy = e.enemy;
                gauntletHit42.cls = cls;
                gauntletHit42.shot = e.shot;
                gauntletHit42.kind = e.kind;
                gauntletHit42.killed = e.killed;
                gauntletHit42.damage = e.damage;
                gauntletHit42.position = e.position;
                gauntletHits42[cls]++;
                events.Emit(GauntletHit42Event.Name, gauntletHit42);
            }
            // the kill's tier is read on its `kill` (Impact42OnKill: a boss's death emits a kill with no killing hit); remember this hit for it
            impact42LastEnemy = e.enemy;
            impact42LastShot = e.shot;
            impact42LastDamage = e.damage;
            impact42LastCls = cls;
        }

        int impact42LastEnemy = -1, impact42LastShot = -1, impact42LastCls;
        double impact42LastDamage;

        void Impact42OnKill(KillEvent e)
        {
            if (!n42.impact42 || e.owner != "player") return;
            bool fromHit = e.enemy == impact42LastEnemy; // Combat.Damage emits the killing `hit` just before its `kill`
            int shot = fromHit ? impact42LastShot : 0;
            int cls = fromHit ? impact42LastCls : e.kind == "boss" ? GauntletHit42Event.Boss : Impact42Class(e.enemy, e.kind);
            if (shot > 0)
            {
                if (shot != impact42Shot)
                {
                    impact42Shot = shot;
                    impact42ShotKills = 0;
                }
                impact42ShotKills++;
            }
            string w = e.weapon;
            bool melee = e.melee || w == "gauntlet" || w == "punch";
            int tier = cls == GauntletHit42Event.Boss || (w == "gauntlet" && cls == GauntletHit42Event.Big) || (shot > 0 && impact42ShotKills >= impact42MultiCount) ? KillTier42Event.Big
                : melee ? KillTier42Event.Melee
                : e.headshot || w == "rail" || (w == "rocket" && e.distance <= impact42RocketReach) || (w == "shotgun" && fromHit && impact42LastDamage >= impact42HeavyShotgun) ? KillTier42Event.Heavy
                : KillTier42Event.Plain;
            impact42LastEnemy = -1;
            killTier42.tier = tier;
            killTier42.enemy = e.enemy;
            killTier42.shot = shot;
            killTier42.kind = e.kind;
            killTier42.weapon = w;
            killTier42.position = e.position;
            killTiers42[tier]++;
            events.Emit(KillTier42Event.Name, killTier42);
        }
    }
}
