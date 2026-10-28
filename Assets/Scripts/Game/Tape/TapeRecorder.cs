// NIGHT14 U3-27.8 (agent T): THE TAPE's recorder. Attached once to a composition (the clock tap on sim.utcNow, the
// runEnded tap, StepEnded, and observers of the between-step events), it records one run per Begin .. Finish:
//   - the input, through TapeInputTap: a decorator over the sim's IPlayerInput (installed on sim.input at Begin, removed
//     at Finish) that keeps what SampleMove / SampleCombat returned and the look after SampleMove;
//   - the onScreen answers, through a tap on sim.brain.onScreen (HuskBrain.cs :962, the stuck watchdog: 0 .. N calls a
//     step, in the brain's enemy order), as bits in call order;
//   - the assist mode / snapStrength / breakaway cap, read before each step (and before a toggle), written when changed;
//   - between steps (observed on the EventBus, outside a step): a lock toggle (lockOn, or aimRelease "toggle"), a MODS
//     draft opened / picked / rerolled, a free mod given, EndRun (R / RETRY from DOWN, through runEnded);
//   - SimFingerprint.Hash every 600 steps and at the end, and a 32-bit light hash after every step.
// Recording allocates nothing per step once the run started: fields, a growable byte buffer started at minutes of play,
// cached delegates (a checkpoint's SimFingerprint.Hash allocates its mods list every 600 steps). The recorder only
// observes: a taped fingerprint run prints the baseline's hashes (TapeTests). No UnityEngine use.
// Known limits (logged in the report): a mutation by someone else between steps (debug keys F8 / F9, mods.Queue, the
// smoke's teleports and per-frame god mode, a snapshot restore) is not recorded; a restore is detected (simStep jumps:
// TapeEnd.Restored), the rest makes the replay DIFFER at the step after it.
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed class TapeRecorder
    {
        /// <summary>A checkpoint every this many steps (the fingerprint's: 5 s).</summary>
        public const int CheckEvery = SimFingerprint.Every;

        /// <summary>The op buffer's first size, allocated once with the recorder: 4 MB, ~25 minutes at the measured 159 KB a minute
        /// (NIGHT14 review A-3: from 512 KB it doubled inside a step at ~3, 6.5, 13 and 26 minutes; now only a run past ~25
        /// minutes grows it, at 25 and 50).</summary>
        public const int InitialBytes = 1 << 22;

        readonly SurvivalSim sim;
        readonly string buildStamp;
        readonly Func<string, bool> settingBool;
        readonly Func<string, double> settingNum;
        readonly TapeInputTap tap;
        readonly Func<Vec3, bool> onScreenTap;
        Func<Vec3, bool> onScreenInner;
        readonly Func<DateTime> clockTap, clockInner;
        DateTime clockLast;
        bool clockRead;
        readonly Action runEndedTap, runEndedInner;
        readonly TapeWriter ops = new TapeWriter(InitialBytes);
        readonly TapeWriter scratch = new TapeWriter(256);

        RunTape tape;
        bool recording, inStep, started, endInStep;
        TapeStepState prev, cur;
        byte[] bits = new byte[64];
        int bitCount;
        string mode;
        double snap, cap;
        bool assistKnown;
        int steps, lastCheckStep = -1, baseSimStep;
        List<string> lastOffer;

        /// <summary>The run's tape finished (RESULTS, EndRun between steps, Stop, abandoned, a swapped input, a restore).</summary>
        public event Action<RunTape> Finished;

        public bool Recording => recording;

        /// <summary>The tape being recorded, or the last one finished (null before the first Begin).</summary>
        public RunTape Tape => tape;

        public int Steps => steps;

        /// <summary>
        /// Attach to `sim` (once per composition). `settingBool` / `settingNum` are the functions the sim was built with (the
        /// Game's settings); null: a default settings store's (SurvivalSim.Headless's).
        /// </summary>
        public TapeRecorder(SurvivalSim sim, string buildStamp, Func<string, bool> settingBool = null, Func<string, double> settingNum = null)
        {
            this.sim = sim;
            this.buildStamp = buildStamp ?? "";
            if (settingBool == null || settingNum == null)
            {
                var store = new SettingsStore(sim.cfg, new List<(SettingItem, object)>(), new List<(string, string)>());
                store.Load("");
                settingBool = settingBool ?? store.Bool;
                settingNum = settingNum ?? store.Num;
            }
            this.settingBool = settingBool;
            this.settingNum = settingNum;
            tap = new TapeInputTap(this);
            onScreenTap = OnScreenTap;
            clockTap = ClockTap;
            clockInner = sim.utcNow;
            sim.utcNow = clockTap;
            runEndedTap = RunEndedTap;
            runEndedInner = sim.runEnded;
            sim.runEnded = runEndedTap;
            sim.StepEnded += OnStepEnded;
            EventBus ev = sim.events;
            ev.On("modDraft", OnModDraft);
            ev.On("modPick", OnModPick);
            ev.On("modReroll", OnModReroll);
            ev.On("lockOn", OnLockOn);
            ev.On("aimRelease", OnAimRelease);
        }

        DateTime ClockTap()
        {
            clockLast = clockInner();
            clockRead = true;
            return clockLast;
        }

        /// <summary>Start the tape of the run StartRun just started (call after StartRun: the Game does it on sim.RunStarted).</summary>
        public void Begin()
        {
            if (recording) Finish(TapeEnd.Abandoned);
            Lab lab = sim.lab;
            tape = new RunTape
            {
                buildStamp = buildStamp,
                baselineId = RunTape.CurrentBaselineId(),
                seed = sim.runSeed,
                mode = sim.setup.mode,
                heat = sim.run.stats.heat, // the rung the run got (StartRun's clamp to the unlocked rungs already applied)
                utcTicks = (clockRead ? clockLast : clockInner()).Ticks,
                noRecords = sim.noRecords,
                noLab = sim.noLab,
                metaSilent = sim.metaSilent,
                onboarding = sim.onboarding.Active, // StartRun's runActive = enabled && setting && !onboarded: this value as the setting gives the same
                damageDirection = SettingBool("damageDirection"),
                profileJson = sim.profile.ToJson(), // StartRun only reads the profile (contracts / mastery Reload, heat.Unlocked, daily.Today)
            };
            tape.lab[0] = lab.closeKillHeal;
            tape.lab[1] = lab.freshness;
            tape.lab[2] = lab.styleRamp;
            tape.lab[3] = lab.readyFromOne;
            tape.lab[4] = lab.squadTighten;
            tape.lab[5] = lab.ultraHealth;
            for (int i = 0; i < tape.move.Length; i++) tape.move[i] = sim.MoveBalanceFlag(i); // NIGHT16 U3-29.4: the design.move flags the run read
            for (int i = 0; i < tape.stamina.Length; i++) tape.stamina[i] = sim.StaminaFlag(i); // NIGHT17 U3-30.4: the design.stamina flags the run read
            tape.n19 = sim.n19.Bits; // NIGHT19: the NIGHT19 blocks the run read
            tape.n20 = sim.n20.Bits; // NIGHT20: the NIGHT20 blocks the run read
            tape.n21 = sim.n21.Bits; // NIGHT21: the NIGHT21 blocks the run read
            tape.n22 = sim.n22.Bits; // NIGHT22: the NIGHT22 blocks the run read
            tape.n26 = sim.n26.Bits; // NIGHT26: the NIGHT26 blocks the run read
            tape.n27 = sim.n27.Bits; // NIGHT27: the NIGHT27 blocks the run read
            tape.n29 = sim.n29.Bits; // NIGHT29: the NIGHT29 blocks the run read
            tape.n30 = sim.n30.Bits; // NIGHT30: the NIGHT30 blocks the run read
            tape.n31 = sim.n31.Bits; // NIGHT31: the NIGHT31 blocks the run read
            tape.n32 = sim.n32.Bits; // NIGHT32: the NIGHT32 blocks the run read
            tape.n33 = sim.n33.Bits; // NIGHT33: the NIGHT33 blocks the run read, the shop's upgrades and the banked points it started with
            tape.shop33 = sim.shop33Owned;
            tape.bank33 = sim.bank33;
            tape.n34 = sim.n34.Bits; // NIGHT34: the NIGHT34 blocks the run read
            tape.n35 = sim.n35.Bits; // NIGHT35: the NIGHT35 blocks the run read
            tape.n36 = sim.n36.Bits; // NIGHT36: the NIGHT36 blocks the run read
            tape.n37 = sim.n37.Bits; // NIGHT37: the NIGHT37 blocks the run read
            tape.n38 = sim.n38.Bits; // NIGHT38: the NIGHT38 blocks the run read
            tape.n39 = sim.n39.Bits; // NIGHT39: the NIGHT39 blocks the run read
            tape.n40 = sim.n40.Bits; // NIGHT40: the NIGHT40 blocks the run read
            tape.n41 = sim.n41.Bits; // NIGHT41: the NIGHT41 blocks the run read
            tape.n42 = sim.n42.Bits; // NIGHT42: the NIGHT42 blocks the run read
            tape.n43 = sim.n43.Bits; // NIGHT43: the NIGHT43 blocks the run read
            tape.rows = sim.RowsTapeRecord(); // NIGHT46 rows: the NIGHT46+ rows the run read
            tape.n25 = sim.n25.Bits; // NIGHT25: the NIGHT25 blocks the run read
            tape.n24 = sim.n24.Bits; // NIGHT24: the NIGHT24 blocks the run read
            tape.campaign = sim.campaign24; // NIGHT24: the campaign state the run starts with (format 8)
            clockRead = false;
            ops.Clear();
            steps = 0;
            lastCheckStep = -1;
            prev = TapeStepState.Neutral;
            cur = prev;
            bitCount = 0;
            inStep = started = endInStep = assistKnown = false;
            lastOffer = null;
            baseSimStep = sim.simStep;
            // The input and the onScreen taps (unwrapping our own from an earlier run, never wrapping twice).
            IPlayerInput inner = sim.input;
            if (inner is TapeInputTap t) inner = t.inner;
            tap.inner = inner;
            sim.input = tap; // the field, not UseInput: the look stays the inner's own
            Func<Vec3, bool> os = sim.brain.onScreen;
            if (!ReferenceEquals(os, onScreenTap)) onScreenInner = os;
            sim.brain.onScreen = onScreenTap;
            recording = true;
        }

        bool SettingBool(string key)
        {
            try
            {
                return settingBool(key);
            }
            catch (Exception)
            {
                return false; // not a setting here
            }
        }

        /// <summary>Stop the tape: the last checkpoint when the state is still the last step's (Stopped), the taps removed, Finished raised.</summary>
        public RunTape Finish(TapeEnd end)
        {
            if (!recording) return tape;
            if (inStep) FlushStep(ops, true); // a step that never ended (an exception)
            if (end == TapeEnd.Stopped && lastCheckStep != steps && steps > 0) WriteCheck();
            recording = false;
            if (ReferenceEquals(sim.input, tap)) sim.input = tap.inner;
            if (ReferenceEquals(sim.brain.onScreen, onScreenTap)) sim.brain.onScreen = onScreenInner;
            tape.end = end;
            tape.steps = steps;
            tape.ops = new byte[ops.length];
            Buffer.BlockCopy(ops.data, 0, tape.ops, 0, ops.length);
            tape.opsLength = ops.length;
            Finished?.Invoke(tape);
            return tape;
        }

        /// <summary>A copy of the tape so far (still recording): the exception path and COPY REPORT write it. A step in
        /// progress is included as aborted (the crash's step, so the replay reaches it).</summary>
        public RunTape Snapshot()
        {
            if (tape == null) return null;
            RunTape t = tape.CloneHeader();
            scratch.Clear();
            scratch.Bytes(ops.data, ops.length);
            if (recording && inStep) TapeStepState.Write(scratch, prev, cur, bits, bitCount, true, 0);
            t.ops = new byte[scratch.length];
            Buffer.BlockCopy(scratch.data, 0, t.ops, 0, scratch.length);
            t.opsLength = scratch.length;
            t.steps = steps + (recording && inStep ? 1 : 0);
            t.end = recording ? TapeEnd.Open : tape.end;
            return t;
        }

        /// <summary>An unhandled exception (the Game's log hook): the first one's step and text go in the header.</summary>
        public void NoteException(string text)
        {
            if (!recording || tape.exceptionStep >= 0) return;
            tape.exceptionStep = steps + (inStep ? 1 : 0);
            tape.exceptionText = text ?? "";
        }

        // --- per step ------------------------------------------------------------------------------------------------

        internal void OnSampleMove(in InputFrame f, double yaw, double pitch)
        {
            if (!recording) return;
            if (inStep) FlushStep(ops, true); // the last step threw before StepEnded: it goes on the tape as aborted
            if (!started)
            {
                started = true;
                if (sim.simStep != baseSimStep + 1)
                {
                    // The state came from elsewhere after StartRun (a RESUME's snapshot): not replayable from StartRun.
                    Finish(TapeEnd.Restored);
                    return;
                }
                tape.hasStart = true;
                tape.startMax = sim.health.max; // what automation set after StartRun (the fingerprint's and the smoke's 1e6)
                tape.startHp = sim.health.hp;
            }
            WriteAssistIfChanged();
            cur.moveX = f.moveX;
            cur.moveZ = f.moveZ;
            cur.yaw = yaw;
            cur.pitch = pitch;
            cur.frameYaw = f.yaw;
            cur.framePitch = f.pitch;
            cur.moveButtons = TapeStepState.PackMove(f);
            cur.hasGrapple = f.grappleTarget.HasValue;
            if (cur.hasGrapple) cur.grapple = f.grappleTarget.Value;
            cur.combatButtons = prev.combatButtons; // until SampleCombat (a step that throws before it keeps the last)
            cur.weapon = prev.weapon;
            bitCount = 0;
            inStep = true;
        }

        internal void OnSampleCombat(in CombatInput c)
        {
            if (!recording || !inStep) return;
            cur.combatButtons = TapeStepState.PackCombat(c);
            cur.weapon = c.weapon;
        }

        bool OnScreenTap(Vec3 p)
        {
            bool b = onScreenInner(p);
            if (recording && inStep)
            {
                int i = bitCount >> 3;
                if (i >= bits.Length) Array.Resize(ref bits, bits.Length * 2);
                if ((bitCount & 7) == 0) bits[i] = 0;
                if (b) bits[i] |= (byte)(1 << (bitCount & 7));
                bitCount++;
            }
            return b;
        }

        void OnStepEnded()
        {
            if (!recording) return;
            if (!inStep)
            {
                // The step ran without our tap: the input was swapped after Begin (UseInput). The tape stops at the last recorded step.
                if (!ReferenceEquals(sim.input, tap)) Finish(TapeEnd.InputSwapped);
                return;
            }
            FlushStep(ops, false);
            if (endInStep)
            {
                if (lastCheckStep != steps) WriteCheck();
                Finish(TapeEnd.Results);
                return;
            }
            if (steps % CheckEvery == 0) WriteCheck();
        }

        void FlushStep(TapeWriter w, bool aborted)
        {
            TapeStepState.Write(w, prev, cur, bits, bitCount, aborted, aborted ? 0 : TapeReplay.Light(sim));
            prev = cur;
            bitCount = 0;
            inStep = false;
            steps++;
        }

        void WriteCheck()
        {
            ops.Byte(TapeOp.Check);
            ops.VarU((uint)steps);
            ops.U64(SimFingerprint.Hash(sim));
            lastCheckStep = steps;
        }

        void WriteAssistIfChanged()
        {
            string m = sim.AssistMode;
            double s = settingNum("snapStrength");
            double c = sim.breakawayCap != null ? sim.breakawayCap() : double.PositiveInfinity;
            byte f = 0;
            if (!assistKnown || !string.Equals(m, mode, StringComparison.Ordinal)) f |= 1;
            if (!assistKnown || BitConverter.DoubleToInt64Bits(s) != BitConverter.DoubleToInt64Bits(snap)) f |= 2;
            if (!assistKnown || BitConverter.DoubleToInt64Bits(c) != BitConverter.DoubleToInt64Bits(cap)) f |= 4;
            if (f == 0) return;
            RunTape.WriteAssist(ops, f, m, s, c);
            mode = m;
            snap = s;
            cap = c;
            assistKnown = true;
        }

        // --- between steps ---------------------------------------------------------------------------------------------

        bool BetweenSteps => recording && !inStep;

        void OnLockOn(object _)
        {
            if (BetweenSteps) WriteToggle();
        }

        void OnAimRelease(object reason)
        {
            if (BetweenSteps && reason is string s && s == "toggle") WriteToggle();
        }

        /// <summary>ToggleLock ran between steps (it reads the mode and aims along the look of that moment).</summary>
        void WriteToggle()
        {
            WriteAssistIfChanged();
            ops.Byte(TapeOp.Toggle);
            ops.D(tap.inner.Yaw);
            ops.D(tap.inner.Pitch);
        }

        void OnModDraft(object o)
        {
            if (!BetweenSteps) return;
            lastOffer = (o as ModEvent)?.offer;
            ops.Byte(TapeOp.DraftOpen);
        }

        void OnModPick(object o)
        {
            if (!BetweenSteps || !(o is ModEvent e)) return;
            if (e.free)
            {
                ops.Byte(TapeOp.ModGive);
                ops.Str(e.id);
                ops.VarI(e.stacks); // the stacks after it (the replay gives the difference)
                return;
            }
            ops.Byte(TapeOp.DraftPick);
            ops.VarI(lastOffer != null ? lastOffer.IndexOf(e.id) : -1);
        }

        /// <summary>NIGHT20: the reward reveal's pick (builder H's UiReveal20, F3), between steps as a MODS pick: what it gave
        /// (kind 0 a power-up of `type` at `feet`, 1 a MODS pick queued for `round`, 2 `points`).</summary>
        public void RecordReveal(int kind, string type, int points, int round, Vec3 feet)
        {
            if (!BetweenSteps) return;
            ops.Byte(TapeOp.RevealPick);
            ops.VarI(kind);
            ops.Str(type);
            ops.VarI(points);
            ops.VarI(round);
            ops.D(feet.x);
            ops.D(feet.y);
            ops.D(feet.z);
        }

        void OnModReroll(object o)
        {
            if (!BetweenSteps) return;
            lastOffer = (o as ModEvent)?.offer;
            ops.Byte(TapeOp.DraftReroll);
        }

        void RunEndedTap()
        {
            runEndedInner?.Invoke();
            if (!recording) return;
            if (inStep)
            {
                endInStep = true; // EndRun inside the step (DOWN's clock, an exfil): the tape ends after it (OnStepEnded)
                return;
            }
            ops.Byte(TapeOp.EndRun); // R / RETRY from DOWN: downSteps = 0, EndRun, between steps
            WriteCheck();
            Finish(TapeEnd.EndedBetweenSteps);
        }
    }

    /// <summary>The recorder's decorator over the sim's input: everything forwarded, the samples kept.</summary>
    public sealed class TapeInputTap : IPlayerInput
    {
        readonly TapeRecorder recorder;
        public IPlayerInput inner;

        public TapeInputTap(TapeRecorder recorder)
        {
            this.recorder = recorder;
        }

        public double Yaw => inner.Yaw;
        public double Pitch => inner.Pitch;
        public bool Captured => inner.Captured;

        public void SetLook(double yaw, double pitch) => inner.SetLook(yaw, pitch);

        public InputFrame SampleMove(bool active)
        {
            InputFrame f = inner.SampleMove(active);
            recorder.OnSampleMove(f, inner.Yaw, inner.Pitch);
            return f;
        }

        public CombatInput SampleCombat(bool active)
        {
            CombatInput c = inner.SampleCombat(active);
            recorder.OnSampleCombat(c);
            return c;
        }
    }
}
