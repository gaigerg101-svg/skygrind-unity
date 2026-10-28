// NIGHT10 U3-13.9: the ULTRAKILL alignment (new design, HANDOFF D-17, ZOMBIES-ULTRAKILL-RESEARCH.md section 2.2 and 4 #3),
// four design flags under design.ultra (Design/unity-config.json, each with its `_why`):
// - freshDamage (bool; absent: true, the web's arsenal.js): false = the arsenal's FRESH multiplies points and style but
//   not damage (ULTRAKILL's freshness changes style only). PRIMED, CROSS / combos and the statuses are unchanged.
// - styleBreakPause (bool; absent: false): no style drain during a break, or while no live enemy is within
//   style.trickEnemyRange (ULTRAKILL's meter pauses when nothing is fighting you).
// - parryRefillsDash (bool; absent: true): a parry refills the dash pips. Measured tonight: the web already does this
//   (punch.js :36-39 "heal, full stamina" and melee.js :105 call mover.restoreStamina(), which sets pips to maxPips,
//   mover.js :244-246), so ON is the web's rule; an explicit false skips the refill (an A/B against ULTRAKILL's).
// - parryHitstop (s; absent or 0: the web's punch.parryHitstop 0.06): a parry's freeze (ULTRAKILL: 0.25 s). A PERFECT
//   PARRY keeps its own design.parry.perfectHitstop.
// Core objects (Loadout, StyleMeter) default to the web's rules; the game applies these at boot (SurvivalGame), so the
// golden sets and the replays (which read config.json without the design config) and every existing test stay the web's.
namespace Skygrind.Core
{
    public sealed class Ultra
    {
        public bool freshDamage = true, styleBreakPause, parryRefillsDash = true;
        public double parryHitstop; // 0: the web's

        public static Ultra From(GameConfig cfg)
        {
            var u = new Ultra();
            if (cfg.Has("design.ultra.freshDamage") && cfg.Get("design.ultra.freshDamage") is bool fd) u.freshDamage = fd;
            u.styleBreakPause = cfg.Design("ultra.styleBreakPause");
            if (cfg.Has("design.ultra.parryRefillsDash") && cfg.Get("design.ultra.parryRefillsDash") is bool pr) u.parryRefillsDash = pr;
            if (cfg.Has("design.ultra.parryHitstop")) u.parryHitstop = cfg.Num("design.ultra.parryHitstop");
            return u;
        }

        /// <summary>The boot log's line (U3-13.9: the flags are logged at boot).</summary>
        public string Describe() =>
            $"freshDamage {(freshDamage ? "on (the web's)" : "off: FRESH pays points and style only")}, styleBreakPause {(styleBreakPause ? "on" : "off")}, " +
            $"parryRefillsDash {(parryRefillsDash ? "on" : "off")}, parryHitstop {(parryHitstop > 0 ? parryHitstop.ToString(System.Globalization.CultureInfo.InvariantCulture) + " s" : "the web's")}";

        /// <summary>styleBreakPause: the style meter doesn't drain in a break, or with no live enemy within style.trickEnemyRange.</summary>
        public bool DrainPaused(bool inBreak, bool enemyNear) => styleBreakPause && (inBreak || !enemyNear);

        /// <summary>A parry's hitstop: the PERFECT PARRY's own, else this flag's (when set), else the web's.</summary>
        public double ParryHitstop(bool perfect, double perfectHitstop, double webHitstop) =>
            perfect ? perfectHitstop : parryHitstop > 0 ? parryHitstop : webHitstop;
    }
}
