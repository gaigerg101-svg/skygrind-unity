// NIGHT48 builder B (NIGHT48.md section 1 "Level 2 + 3"; NIGHT48-BRIEF.md): B's three NIGHT48 sim rows, filling the shared commit's hooks
// N48BStartRun / N48BStep / N48BCampaign (SurvivalSim.N48.cs resolves idWyrmTell48 / idWyrmDive48 / idKilnBoost48 once a run). Numbers in
// design.n48.<row>.numbers (Tools/night/n48-b-rows.cjs). Every row reads THE KILN WYRM (wyrm24) and runs after N43Step (RowsStep), so after
// SurvivalSim.Boss30.cs's Wyrm30Step and Boss31's Wyrm31 step of the same sim step.
//   wyrmTell48  G31, the counter, not the timing: the head-charge tell tellS (1.0 s) with the aim tracking until lockS, and the head punchable
//               (Wyrm30Punchable: the view's yellow) for the whole tell and the whole charge; a gauntlet hit with the head in reach staggers it
//               (Boss30's own Wyrm30OnHit, now open for longer), and so does a fist punch / parry with the head in reach (event wyrmStagger, as
//               Boss30's; parry48 counts them). VERBS rows 28 (the gauntlet) and 40 (the parry): both still work, in a wider window.
//   wyrmDive48  G34, the burrow as the punish: when a STAGGER ends, the worm dives at once (Wyrm.Dive48; event wyrm48Dive), so burrows rise
//               with the player's success.
//   kilnBoost48 G33, the islands pay: the open core's x3 only from high ground (feet >= highY over the arena floor: the slag islands, the
//               perches) or mid-boost (airborne after a THUNDERCLAP shot fired airborne); from the floor x floorMult (Wyrm.coreFull48). A health
//               pickup on each slag island (healAmount, back after healEveryS; event kiln48Heal).
// Off (each row off, --off48 / -skygrindOff48): nothing changed, nothing per step but three bool reads. No allocation per step (the delegate and
// the event objects are made once). No UnityEngine: SimHarness compiles this file.
using Skygrind.Core;

namespace Skygrind.Game
{
    /// <summary>The payload of wyrm48Dive / kiln48Heal (one reused object).</summary>
    public sealed class Wyrm48Event
    {
        public string what;
        public Vec3 at;
        public double amount;
    }

    public sealed partial class SurvivalSim
    {
        const string Tell48Root = "design.n48.wyrmTell48.numbers.", Dive48Root = "design.n48.wyrmDive48.numbers.", Boost48Root = "design.n48.kilnBoost48.numbers.";
        bool t48On, d48On, b48On, b48Hooked, b48Boosted, d48WasStag;
        Wyrm t48For, b48For;
        double t48AggroSet = double.NaN, t48AggroWas, t48LockSet = double.NaN, t48LockWas;
        int d48Pending;
        int[] b48HealWait;
        System.Func<bool> b48Full;
        readonly Wyrm48Event w48Said = new Wyrm48Event();
        /// <summary>The harness's counts this sim's life: staggers by a punch / parry in the tell (wyrmTell48), dives after a stagger and dives
        /// dropped (wyrmDive48), open-core hits at the full x and at the floor's x, island heals (kilnBoost48).</summary>
        public int parry48, tellStaggers48, dives48, divesMissed48, coreFullHits48, coreFloorHits48, heals48;

        /// <summary>Harness probes only (NaN = the config's number; NIGHT47's Probe47GauntletOnly precedent): a trial value for wyrmTell48's tellS /
        /// lockS, set by SimHarness flags (--b48-tellS / --b48-lockS) so trials run side by side on one config. Never set by the game.</summary>
        public static double Probe48TellS = double.NaN, Probe48LockS = double.NaN, Probe48WindowShare = double.NaN, Probe48DiveWhen = double.NaN, Probe48BeatExtra = double.NaN;
        Wyrm d48For;
        double d48StagFrom;
        bool d48PendingAt, d48InAtDive;
        /// <summary>STAGGERED windows opened at the eruption of a dive taken at the stagger (wyrmDive48 when 1 / 2; the harness line).</summary>
        public int reopens48;
        /// <summary>STAGGER windows wyrmDive48's windowShare ended early (the harness line).</summary>
        public int windowsSpent48;

        /// <summary>wyrmTell48 is on this run (the view's yellow reads Wyrm30Punchable either way).</summary>
        public bool WyrmTell48On => t48On;

        partial void N48BStartRun()
        {
            Tell48Restore();
            if (b48For != null) { b48For.coreFull48 = null; b48For.coreElse48 = 1; b48For = null; }
            if (d48For != null) { d48For.beatExtra48 = 0; d48For = null; }
            t48On = rows.On(idWyrmTell48);
            d48On = rows.On(idWyrmDive48);
            b48On = rows.On(idKilnBoost48);
            d48WasStag = b48Boosted = d48PendingAt = d48InAtDive = false;
            d48Pending = 0;
            b48HealWait = null;
            if (!b48Hooked && (t48On || b48On))
            {
                b48Hooked = true;
                events.On<MeleeEvent>("punch", Tell48OnPunch);
                events.On("fire", _ => { if (b48On && !mover.grounded && loadout.Weapon != null && loadout.Weapon.StartsWith("shotgun", System.StringComparison.Ordinal)) b48Boosted = true; });
            }
        }

        partial void N48BStep(double dt)
        {
            if (t48On) Tell48Step();
            if (d48On) Dive48Step();
            if (b48On) Boost48Step();
        }

        partial void N48BCampaign()
        {
        }

        // --- wyrmTell48 (G31) ------------------------------------------------------------------------------------------------------------------

        /// <summary>Put back the tell and the lock an earlier run set on its worm, only where nobody wrote them since (wyrm47 restores its own first).</summary>
        void Tell48Restore()
        {
            if (t48For == null) return;
            if (t48For.p.tells.TryGetValue("aggro", out double a) && a == t48AggroSet) t48For.p.tells["aggro"] = t48AggroWas;
            if (t48For.aggroLock47 == t48LockSet) t48For.aggroLock47 = t48LockWas;
            t48For = null;
        }

        void Tell48Step()
        {
            Wyrm w = wyrm24;
            if (w == null) return;
            if (w != t48For)
            {
                Tell48Restore();
                if (!w.p.tells.ContainsKey("aggro")) return;
                t48For = w;
                t48AggroWas = w.p.tells["aggro"];
                t48AggroSet = double.IsNaN(Probe48TellS) ? cfg.Num(Tell48Root + "tellS") : Probe48TellS;
                w.p.tells["aggro"] = t48AggroSet;
                t48LockWas = w.aggroLock47;
                t48LockSet = double.IsNaN(Probe48LockS) ? cfg.Num(Tell48Root + "lockS") : Probe48LockS;
                w.aggroLock47 = t48LockSet;
            }
            // wyrm47 writes the same two once on the worm's first step, before this one (night order): ours stand while the row is on
            if (w.p.tells["aggro"] != t48AggroSet) { t48AggroWas = w.p.tells["aggro"]; w.p.tells["aggro"] = t48AggroSet; }
            if (w.aggroLock47 != t48LockSet) { t48LockWas = w.aggroLock47; w.aggroLock47 = t48LockSet; }
            if (!n30.wyrm30 || !w.Alive) return;
            // the head is punchable for the whole tell (the lean and the tell, its commit wait too) and, with wholeCharge, the whole pass
            bool tell = w.AggroTelling || w.Leaning47;
            bool pass = w.AggroDiving && cfg.Num(Tell48Root + "wholeCharge") > 0;
            if ((tell || pass) && !w.Recoiling30) Wyrm30Punchable = true;
        }

        /// <summary>A fist punch (the melee button; the parry verb) while the head is punchable and its box in the gauntlet's reach: the stagger, as
        /// Boss30's Wyrm30OnHit makes it for a gauntlet hit (the recoil, the hitstop, wyrmStagger; no bonus damage).</summary>
        void Tell48OnPunch(MeleeEvent e)
        {
            if (!t48On || e == null || e.owner != "player" || cfg.Num(Tell48Root + "parry") <= 0) return;
            if (!n30.wyrm30 || !Wyrm30Punchable || wyrm24 == null || !wyrm24.Alive) return;
            Vec3 me = mover.position, eye = new Vec3(me.x, me.y + cfg.Num("body.eyeHeight"), me.z);
            double reach = cfg.Num("weapons.gauntlet.range") + cfg.Num("design.n30.wyrm30.headReachMargin");
            if (Reach34Margin > 0) reach += Reach34Margin;
            if (!wyrm24.boxes[0].Touches(eye, reach)) return;
            double rec = cfg.Num("design.n30.wyrm30.recoilSeconds");
            Vec3 head = wyrm24.boxes[0].c;
            bool inTell = wyrm24.AggroTelling || wyrm24.Leaning47;
            if (!wyrm24.Stagger30(rec, cfg.Num("design.n30.wyrm30.recoilBack"), cfg.Num("design.n30.wyrm30.recoilUp"), head.x - me.x, head.z - me.z)) return;
            Hitstop(cfg.Num("design.n30.wyrm30.hitstopSteps") * cfg.Dt, "wyrmStagger");
            wyrm30Staggers++;
            parry48++;
            if (inTell) tellStaggers48++;
            Wyrm30Punchable = false;
            wyrm30RecoilSteps = MovementMath.StepsFor(rec, cfg.Dt);
            wyrm30Said.position = head;
            wyrm30Said.bonus = 0;
            wyrm30Said.recoilSeconds = rec;
            events.Emit("wyrmStagger", wyrm30Said);
        }

        // --- wyrmDive48 (G34) ------------------------------------------------------------------------------------------------------------------

        void Dive48Step()
        {
            Wyrm w = wyrm24;
            if (w == null || !w.Alive || wyrm31 == null) { d48WasStag = d48InAtDive = false; d48Pending = 0; return; }
            // the min's second lever (round 3): a longer phase beat, the same seconds added to every fight (Wyrm.beatExtra48)
            if (d48For != w) { if (d48For != null) d48For.beatExtra48 = 0; d48For = w; }
            w.beatExtra48 = double.IsNaN(Probe48BeatExtra) ? (cfg.Has(Dive48Root + "beatExtraS") ? cfg.Num(Dive48Root + "beatExtraS") : 0) : Probe48BeatExtra;
            bool stag = wyrm31.Staggered;
            // the min's lever: one STAGGER may take at most windowShare of the pool; then it flees (the window ends, the dive follows)
            double share = double.IsNaN(Probe48WindowShare) ? cfg.Num(Dive48Root + "windowShare") : Probe48WindowShare;
            if (stag && !d48WasStag) d48StagFrom = w.body.hp;
            if (stag && share > 0 && d48StagFrom - w.body.hp >= share * w.body.maxHp)
            {
                wyrm31.EndStagger47();
                stag = false;
                windowsSpent48++;
            }
            // when (round 2): 0 the dive at the window's end (round 1); 1 AT the stagger (after the punch's recoil it flees under the floor, and
            // the STAGGERED window opens fresh when it erupts: the punish is not lost under the floor); 2 both
            int when = (int)(double.IsNaN(Probe48DiveWhen) ? (cfg.Has(Dive48Root + "when") ? cfg.Num(Dive48Root + "when") : 0) : Probe48DiveWhen);
            double tryS = cfg.Num(Dive48Root + "tryS");
            if (stag && !d48WasStag && when >= 1) { d48Pending = MovementMath.StepsFor(tryS + cfg.Num("design.n30.wyrm30.recoilSeconds"), cfg.Dt); d48PendingAt = true; }
            if (d48WasStag && !stag && !d48InAtDive && when != 1) { d48Pending = MovementMath.StepsFor(tryS, cfg.Dt); d48PendingAt = false; }
            d48WasStag = stag;
            if (d48InAtDive && w.PatternName != "dive")
            {
                // the dive AT the stagger has ended (it erupted): the STAGGERED window opens now, whole
                d48InAtDive = false;
                wyrm31.OnStagger();
                d48WasStag = true;
                d48StagFrom = w.body.hp;
                reopens48++;
            }
            if (d48Pending <= 0) return;
            if (w.Dive48())
            {
                d48Pending = 0;
                if (d48PendingAt) d48InAtDive = true;
                dives48++;
                w48Said.what = "dive";
                w48Said.at = w.boxes[0].c;
                w48Said.amount = 0;
                events.Emit("wyrm48Dive", w48Said);
            }
            else if (--d48Pending == 0)
            {
                divesMissed48++;
                events.Emit("wyrm48DiveMissed", null);
            }
        }

        // --- kilnBoost48 (G33) -----------------------------------------------------------------------------------------------------------------

        double Boost48FloorY(Wyrm w) => w.arenaCenter.HasValue ? w.arenaCenter.Value.y : 0;

        /// <summary>kilnBoost48 is on this run (the harness's pay-aware pilot reads what pays, as a player learns it).</summary>
        public bool KilnBoost48On => b48On;

        /// <summary>The nearest slag island whose pickup is ready (kilnBoost48 on, a fight running): its top's centre. false: none.</summary>
        public bool Kiln48ReadyIsland(out Vec3 at)
        {
            at = default;
            Wyrm w = wyrm24;
            if (!b48On || w == null || !w.Alive || !w.arenaCenter.HasValue || b48HealWait == null) return false;
            Vec3 c = w.arenaCenter.Value, me = mover.position;
            double r = cfg.Num(Boost48Root + "islandR"), best = double.MaxValue;
            bool any = false;
            for (int i = 0; i < b48HealWait.Length; i++)
            {
                if (b48HealWait[i] > 0) continue;
                double a = (cfg.Num(Boost48Root + "islandFirstDeg") + i * cfg.Num(Boost48Root + "islandStepDeg")) * System.Math.PI / 180;
                var p = new Vec3(c.x + r * JsMath.Cos(a), c.y + cfg.Num(Boost48Root + "islandTop"), c.z + r * JsMath.Sin(a));
                double d = JsMath.Hypot(p.x - me.x, p.z - me.z);
                if (d < best) { best = d; at = p; any = true; }
            }
            return any;
        }

        /// <summary>The shooter right now earns the core's full x: mid-boost, or grounded on high ground (an island or a perch).</summary>
        bool Boost48Full()
        {
            Wyrm w = wyrm24;
            if (w == null) return true;
            bool full = b48Boosted || (mover.grounded && mover.position.y - Boost48FloorY(w) >= cfg.Num(Boost48Root + "highY"));
            if (full) coreFullHits48++; else coreFloorHits48++;
            return full;
        }

        void Boost48Step()
        {
            Wyrm w = wyrm24;
            if (mover.grounded) b48Boosted = false;
            if (w == null || !w.Alive) return;
            if (w != b48For)
            {
                if (b48For != null) { b48For.coreFull48 = null; b48For.coreElse48 = 1; }
                b48For = w;
                if (b48Full == null) b48Full = Boost48Full;
                w.coreFull48 = b48Full;
                w.coreElse48 = cfg.Num(Boost48Root + "floorMult");
                b48HealWait = new int[System.Math.Max(0, (int)cfg.Num(Boost48Root + "islandCount"))]; // once a fight
            }
            // the island pickups: grounded on an island's top with hp under max takes its heal; it is back after healEveryS
            for (int i = 0; i < b48HealWait.Length; i++) if (b48HealWait[i] > 0) b48HealWait[i]--;
            if (!mover.grounded || !w.arenaCenter.HasValue || health.hp >= health.max) return;
            Vec3 c = w.arenaCenter.Value, me = mover.position;
            if (System.Math.Abs(me.y - c.y - cfg.Num(Boost48Root + "islandTop")) > 0.5) return;
            double r = cfg.Num(Boost48Root + "islandR"), half = cfg.Num(Boost48Root + "islandHalf");
            for (int i = 0; i < b48HealWait.Length; i++)
            {
                double a = (cfg.Num(Boost48Root + "islandFirstDeg") + i * cfg.Num(Boost48Root + "islandStepDeg")) * System.Math.PI / 180;
                double ix = c.x + r * JsMath.Cos(a), iz = c.z + r * JsMath.Sin(a);
                if (System.Math.Abs(me.x - ix) > half || System.Math.Abs(me.z - iz) > half || b48HealWait[i] > 0) continue;
                double got = health.Heal(cfg.Num(Boost48Root + "healAmount"));
                b48HealWait[i] = MovementMath.StepsFor(cfg.Num(Boost48Root + "healEveryS"), cfg.Dt);
                heals48++;
                w48Said.what = "heal";
                w48Said.at = new Vec3(ix, me.y, iz);
                w48Said.amount = got;
                events.Emit("kiln48Heal", w48Said);
            }
        }
    }
}
