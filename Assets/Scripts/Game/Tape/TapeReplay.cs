// NIGHT14 U3-27.8 (agent T): THE TAPE's replay. A FRESH headless composition built with the tape's header (the profile as
// the run started, the settings the sim reads answered from the header and the stream, the clock pinned, noRecords /
// noLab / metaSilent as recorded), StartRun on the recorded seed and rung, the health automation set before the first step,
// then the ops in order: each STEP fed through TapePlayer (the frame, the combat input, the look, the onScreen answers in
// call order), each between-step op applied as the live run applied it. Every step's light hash and every checkpoint's
// SimFingerprint.Hash are compared: "EQUAL (n of n checkpoints, N steps)", "DIFFERS at step S (checkpoint k of n ...)" (S:
// the first step whose light hash differs, else the checkpoint's), or "REFUSED ..." (another format, a foreign or
// truncated file). Never a crash: an exception in a step is compared with the tape's (a recorded crash must reproduce).
// No UnityEngine use (Tools/SimHarness, the Player's -skygrindReplay, the tests).
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed class TapeResult
    {
        public bool Equal, Refused;
        public int Steps, Checkpoints, CheckpointsTotal;
        public int FirstDifferentStep = -1, FailedCheckpoint = -1;
        public string Line;
        /// <summary>NIGHT24: the replay's composition after its last step (a campaign tape's end state is compared on it); null when refused.</summary>
        public SurvivalSim Sim;
    }

    /// <summary>The tape's input source on a replay: the recorded frame, combat input and look, the onScreen answers in call order, the assist's values.</summary>
    public sealed class TapePlayer : IPlayerInput
    {
        double yaw, pitch;
        InputFrame move;
        CombatInput combat = new CombatInput { weapon = -1 };
        TapeStepState state = TapeStepState.Neutral;
        byte[] bits = new byte[64];
        int bitCount, bitRead;
        public uint light;
        /// <summary>onScreen asked more often than the tape answered in a step (a divergence: answered false).</summary>
        public int overReads;
        public string mode;
        public double snap, cap = double.PositiveInfinity;
        public readonly Func<Vec3, bool> onScreen;
        public readonly Func<double> breakawayCap;

        public TapePlayer()
        {
            onScreen = OnScreen;
            breakawayCap = () => cap;
        }

        public double Yaw => yaw;
        public double Pitch => pitch;
        public bool Captured => true;

        public void SetLook(double newYaw, double newPitch)
        {
            yaw = newYaw;
            pitch = newPitch;
        }

        public InputFrame SampleMove(bool active) => move;

        public CombatInput SampleCombat(bool active) => combat;

        bool OnScreen(Vec3 p)
        {
            if (bitRead >= bitCount)
            {
                overReads++;
                return false;
            }
            bool b = (bits[bitRead >> 3] & (1 << (bitRead & 7))) != 0;
            bitRead++;
            return b;
        }

        /// <summary>The step's answers were all asked (no fewer calls than recorded).</summary>
        public bool AnswersSpent => bitRead == bitCount;

        public void ReadStep(TapeReader r, bool aborted)
        {
            TapeStepState.Read(r, aborted, ref state, ref bits, out bitCount, out light);
            bitRead = 0;
            move = state.Frame();
            combat = state.Combat();
            yaw = state.yaw;
            pitch = state.pitch;
        }

        public void ReadAssist(TapeReader r)
        {
            RunTape.ReadAssist(r, out byte f, out string m, out double s, out double c);
            if ((f & 1) != 0) mode = m;
            if ((f & 2) != 0) snap = s;
            if ((f & 4) != 0) cap = c;
        }
    }

    public static class TapeReplay
    {
        /// <summary>Replay the tape at `path` on a fresh composition of (cfg, map): "EQUAL ..." , "DIFFERS at step N ..." or
        /// "REFUSED ..." (an old format or an unreadable file).</summary>
        public static string ReplayFile(GameConfig cfg, SurvivalMap map, string path)
        {
            RunTape tape;
            try
            {
                tape = RunTape.Load(path);
            }
            catch (TapeFormatException e)
            {
                return $"REFUSED: {e.Message} ({path})";
            }
            catch (Exception e)
            {
                return $"REFUSED: the tape could not be read ({e.GetType().Name}: {e.Message}) ({path})";
            }
            return Replay(cfg, map, tape).Line;
        }

        /// <summary>A 32-bit FNV-1a over the state a step leaves that a player sees (the mover, the look, HP, points, the run's
        /// clock, the flow, the live enemies' places and HP): every step on the tape, so a difference is found at its step,
        /// not at the next 600-step checkpoint. Read only, no allocation.</summary>
        public static uint Light(SurvivalSim g)
        {
            uint h = 2166136261;
            Mover m = g.mover;
            Mix(ref h, m.position.x);
            Mix(ref h, m.position.y);
            Mix(ref h, m.position.z);
            Mix(ref h, m.velocity.x);
            Mix(ref h, m.velocity.y);
            Mix(ref h, m.velocity.z);
            Mix(ref h, g.input.Yaw);
            Mix(ref h, g.input.Pitch);
            Mix(ref h, g.health.hp);
            Mix(ref h, g.economy.total);
            Mix(ref h, g.run.steps);
            Mix(ref h, g.run.round);
            Mix(ref h, (int)g.flow);
            List<Enemy> all = g.enemies.all;
            for (int i = 0; i < all.Count; i++)
            {
                Enemy e = all[i];
                if (!e.alive) continue;
                Mix(ref h, e.position.x);
                Mix(ref h, e.position.y);
                Mix(ref h, e.position.z);
                Mix(ref h, e.hp);
            }
            return h;
        }

        static void Mix(ref uint h, double d)
        {
            ulong v = unchecked((ulong)BitConverter.DoubleToInt64Bits(d));
            for (int i = 0; i < 8; i++)
            {
                h ^= (byte)(v >> (8 * i));
                h *= 16777619;
            }
        }

        /// <summary>The replay's composition: fresh, the header's settings, profile, clock and flags; the run started as recorded.</summary>
        public static SurvivalSim Compose(GameConfig cfg, SurvivalMap map, RunTape tape, TapePlayer player)
        {
            var store = new SettingsStore(cfg, new List<(SettingItem, object)>(), new List<(string, string)>());
            store.Load("");
            player.mode = store.Str("aimAssist");
            player.snap = store.Num("snapStrength");
            Profile profile = string.IsNullOrEmpty(tape.profileJson) ? new Profile() : Profile.Read(tape.profileJson);
            Func<string, bool> settingBool = k =>
                k == LabMath.CloseKillHeal ? tape.lab[0] : k == LabMath.Freshness ? tape.lab[1] : k == LabMath.StyleRamp ? tape.lab[2]
                : k == LabMath.ReadyFromOne ? tape.lab[3] : k == LabMath.SquadTighten ? tape.lab[4] : k == LabMath.UltraHealthKey ? tape.lab[5]
                : k == "onboarding" ? tape.onboarding : k == "damageDirection" ? tape.damageDirection : store.Bool(k);
            Func<string, double> settingNum = k => k == "snapStrength" ? player.snap : store.Num(k);
            Func<string, string> settingStr = k => k == "aimAssist" ? player.mode : store.Str(k);
            var sim = new SurvivalSim(cfg, map, new EventBus(), new Lab(cfg), player, profile, settingNum, settingBool, settingStr, player.onScreen, a => a);
            sim.ToTitle();
            foreach (SurvivalSim.Machine m in sim.machines) sim.RideCore(m); // the buy points on the flat core (the core stays flat in SURVIVAL: ResetWorld's only SetHeights)
            sim.breakawayCap = player.breakawayCap;
            sim.noRecords = tape.noRecords;
            sim.noLab = tape.noLab;
            sim.metaSilent = tape.metaSilent;
            sim.moveBalanceOverride = (bool[])tape.move.Clone(); // NIGHT16 U3-29.4: the move flags the run read, whatever noLab says
            sim.staminaOverride = (bool[])tape.stamina.Clone(); // NIGHT17 U3-30.4: the stamina flags the run read (format 1-2: all off, the web's pips)
            sim.n19Override = N19Blocks.FromBits(tape.n19); // NIGHT19: the NIGHT19 blocks the run read (formats 1-3: all off, NIGHT18's game)
            sim.n20Override = N20Blocks.FromBits(tape.n20); // NIGHT20: the NIGHT20 blocks the run read (formats 1-4: all off, NIGHT19's game)
            sim.n21Override = N21Blocks.FromBits(tape.n21); // NIGHT21: the NIGHT21 blocks the run read (formats 1-5: all off, NIGHT20's game)
            sim.n27Override = N27Blocks.FromBits(tape.n27); // NIGHT27: the NIGHT27 blocks the run read (formats 1-10: all off, NIGHT26's game)
            sim.n29Override = N29Blocks.FromBits(tape.n29); // NIGHT29: the NIGHT29 blocks the run read (formats 1-11: all off, NIGHT28's game)
            sim.n30Override = N30Blocks.FromBits(tape.n30); // NIGHT30: the NIGHT30 blocks the run read (formats 1-12: all off, NIGHT29's game)
            sim.n31Override = N31Blocks.FromBits(tape.n31); // NIGHT31: the NIGHT31 blocks the run read (formats 1-13: all off, NIGHT30's game)
            sim.n32Override = N32Blocks.FromBits(tape.n32); // NIGHT32: the NIGHT32 blocks the run read (formats 1-14: all off, NIGHT31's game)
            sim.n33Override = N33Blocks.FromBits(tape.n33); // NIGHT33: the NIGHT33 blocks the run read (formats 1-15: all off, NIGHT32's game) ...
            sim.shop33Owned = tape.shop33; // ... the shop's upgrades and the banked points it started with (formats 1-15: none, 0)
            sim.bank33 = tape.bank33;
            sim.n34Override = N34Blocks.FromBits(tape.n34); // NIGHT34: the NIGHT34 blocks the run read (formats 1-16: all off, NIGHT33's game)
            sim.n35Override = N35Blocks.FromBits(tape.n35); // NIGHT35: the NIGHT35 blocks the run read (formats 1-17: all off, NIGHT34's game)
            sim.n36Override = N36Blocks.FromBits(tape.n36); // NIGHT36: the NIGHT36 blocks the run read (formats 1-18: all off, NIGHT35's game)
            sim.n37Override = N37Blocks.FromBits(tape.n37); // NIGHT37: the NIGHT37 blocks the run read (formats 1-19: all off, NIGHT36's game)
            sim.n38Override = N38Blocks.FromBits(tape.n38); // NIGHT38: the NIGHT38 blocks the run read (formats 1-20: all off, NIGHT37's game)
            sim.n39Override = N39Blocks.FromBits(tape.n39); // NIGHT39: the NIGHT39 blocks the run read (formats 1-21: all off, NIGHT38's game)
            sim.n40Override = N40Blocks.FromBits(tape.n40); // NIGHT40: the NIGHT40 blocks the run read (formats 1-22: all off, NIGHT39's game)
            sim.n41Override = N41Blocks.FromBits(tape.n41); // NIGHT41: the NIGHT41 blocks the run read (formats 1-23: all off, NIGHT40's game)
            sim.n42Override = N42Blocks.FromBits(tape.n42); // NIGHT42: the NIGHT42 blocks the run read (formats 1-24: all off, NIGHT41's game)
            sim.n43Override = N43Blocks.FromBits(tape.n43); // NIGHT43: the NIGHT43 blocks the run read (formats 1-25: all off, NIGHT42's game)
            sim.rowsOverride = TapeRows.Replay(sim.rowsRegistry, tape.rows); // NIGHT46 rows: the rows the run read (formats 1-26: all off; a row ON this build lacks is refused)
            sim.n26Override = N26Blocks.FromBits(tape.n26); // NIGHT26: the NIGHT26 blocks the run read (formats 1-9: all off, NIGHT25's game)
            sim.n25Override = N25Blocks.FromBits(tape.n25); // NIGHT25: the NIGHT25 blocks the run read (formats 1-8: all off, NIGHT24's game)
            sim.n24Override = N24Blocks.FromBits(tape.n24); // NIGHT24: the NIGHT24 blocks the run read (formats 1-7: all off, NIGHT22's game)
            sim.campaign24Override = tape.campaign; // NIGHT24: the campaign state the run read (formats 1-7: none)
            sim.n22Override = N22Blocks.FromBits(tape.n22); // NIGHT22: the NIGHT22 blocks the run read (formats 1-6: all off, NIGHT21's game)
            DateTime utc = tape.Utc;
            sim.utcNow = () => utc;
            sim.setup.mode = tape.mode;
            sim.setup.heat = tape.heat;
            sim.setup.seed = null;
            sim.StartRun(tape.seed, null, null);
            if (tape.hasStart)
            {
                sim.health.max = tape.startMax;
                sim.health.hp = tape.startHp;
            }
            return sim;
        }

        public static TapeResult Replay(GameConfig cfg, SurvivalMap map, RunTape tape)
        {
            var res = new TapeResult();
            int total;
            try
            {
                total = tape.CountOps(TapeOp.Check);
            }
            catch (TapeFormatException e)
            {
                return Refused(res, e.Message);
            }
            res.CheckpointsTotal = total;
            var player = new TapePlayer();
            SurvivalSim sim;
            try
            {
                sim = Compose(cfg, map, tape, player);
                res.Sim = sim;
            }
            catch (Exception e)
            {
                return Refused(res, $"the replay's composition failed ({e.GetType().Name}: {e.Message})");
            }
            var r = new TapeReader(tape.ops, 0, tape.opsLength);
            int step = 0, checks = 0, lightAt = -1;
            string lightWhy = null, exceptionNote = "";
            try
            {
                while (r.More)
                {
                    byte tag = r.Byte();
                    switch (tag)
                    {
                        case TapeOp.Step:
                        case TapeOp.StepAborted:
                        {
                            bool aborted = tag == TapeOp.StepAborted;
                            player.ReadStep(r, aborted);
                            step++;
                            Exception thrown = null;
                            try
                            {
                                sim.Step();
                            }
                            catch (Exception e)
                            {
                                thrown = e;
                            }
                            if (!aborted && thrown != null)
                                return Differs(res, step, -1, total, $"an exception the run never had: {thrown.GetType().Name}: {thrown.Message}");
                            if (aborted && thrown == null)
                                return Differs(res, step, -1, total, "the run threw at this step and the replay did not");
                            if (aborted) exceptionNote = $"; the run's exception at step {step} reproduced ({thrown.GetType().Name})";
                            else if (lightAt < 0 && Light(sim) != player.light)
                            {
                                lightAt = step;
                                lightWhy = "the state after it";
                            }
                            if (lightAt < 0 && (player.overReads > 0 || !player.AnswersSpent))
                            {
                                lightAt = step;
                                lightWhy = "onScreen asked a different number of times";
                            }
                            player.overReads = 0;
                            break;
                        }
                        case TapeOp.Assist:
                            player.ReadAssist(r);
                            break;
                        case TapeOp.Toggle:
                        {
                            double y = r.D(), p = r.D();
                            player.SetLook(y, p);
                            sim.assist.ToggleLock();
                            break;
                        }
                        case TapeOp.DraftOpen:
                            if (sim.mods.Open() == null && lightAt < 0)
                            {
                                lightAt = Math.Max(1, step);
                                lightWhy = "the MODS draft did not open";
                            }
                            break;
                        case TapeOp.DraftPick:
                        {
                            int i = r.VarI();
                            if (!sim.mods.Pick(i) && lightAt < 0)
                            {
                                lightAt = Math.Max(1, step);
                                lightWhy = "the MODS pick failed";
                            }
                            break;
                        }
                        case TapeOp.DraftReroll:
                            sim.mods.Reroll();
                            break;
                        case TapeOp.RevealPick: // NIGHT20: the reward reveal's pick, as RevealApply20.Apply does it
                        {
                            int kind = r.VarI();
                            string type = r.Str();
                            int points = r.VarI();
                            int round = r.VarI();
                            var feet = new Vec3(r.D(), r.D(), r.D());
                            if (kind == 0 && type != null) sim.powerUps.Spawn(feet, type);
                            else if (kind == 1) sim.mods.Queue(round);
                            else if (kind == 2 && points > 0) sim.economy.Grant(points, "reveal");
                            break;
                        }
                        case TapeOp.ModGive:
                        {
                            string id = r.Str();
                            int stacks = r.VarI();
                            sim.mods.Give(id, stacks - sim.mods.N(id));
                            break;
                        }
                        case TapeOp.EndRun:
                            sim.downSteps = 0;
                            sim.EndRun();
                            break;
                        case TapeOp.Check:
                        {
                            int at = (int)r.VarU();
                            ulong want = r.U64();
                            checks++;
                            ulong got = SimFingerprint.Hash(sim);
                            if (got != want || at != step)
                            {
                                res.Checkpoints = checks - 1;
                                return Differs(res, lightAt >= 0 ? lightAt : at, checks, total,
                                    $"checkpoint at step {at}: hash {got:x16}, the tape's {want:x16}" + (lightAt >= 0 ? $"; first seen in {lightWhy}" : ""));
                            }
                            break;
                        }
                        default:
                            throw new TapeFormatException($"unknown op {tag} at byte {r.Position - 1}");
                    }
                }
            }
            catch (TapeFormatException e)
            {
                return Refused(res, e.Message);
            }
            res.Steps = step;
            res.Checkpoints = checks;
            if (lightAt >= 0) return Differs(res, lightAt, -1, total, $"{lightWhy}; every checkpoint equal");
            res.Equal = true;
            string baseline = tape.baselineId == RunTape.CurrentBaselineId() ? "" : $"; recorded on baseline {tape.baselineId}, this build's is {RunTape.CurrentBaselineId()}";
            res.Line = $"EQUAL ({checks} of {total} checkpoints, {step} steps){exceptionNote}; seed {tape.seed}, {tape.end}, stamp {tape.buildStamp}{baseline}";
            return res;
        }

        static TapeResult Differs(TapeResult res, int step, int checkpoint, int total, string why)
        {
            res.Equal = false;
            res.FirstDifferentStep = step;
            res.FailedCheckpoint = checkpoint;
            res.Line = checkpoint > 0 ? $"DIFFERS at step {step} (checkpoint {checkpoint} of {total}): {why}" : $"DIFFERS at step {step} ({why})";
            return res;
        }

        static TapeResult Refused(TapeResult res, string why)
        {
            res.Refused = true;
            res.Line = "REFUSED: " + why;
            return res;
        }
    }
}
