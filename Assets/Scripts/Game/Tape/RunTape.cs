// NIGHT14 U3-27.8 (agent T): THE TAPE (design.tape). A run's inputs and every non-input answer the sim consumed, so a fresh
// headless composition replays it bit for bit (TapeReplay; PlayerChecks' -skygrindReplay). The file:
//   "SKTAPE", the format version (int32: another version is REFUSED with a line, never a crash), then the HEADER:
//     the build stamp, the fingerprint baseline id (FingerprintBaseline's last hash; NIGHT16: the rules below changed the
//     sim, so the baseline is re-recorded and a tape from before carries the old id), the run seed, the setup (mode, the
//     HEAT rung the run got), the UTC clock StartRun read (the DAILY's day), noRecords / noLab / metaSilent (they decide
//     Recording, so whether contracts move the profile mid-run and change a later draft's pool), the LAB toggles the run
//     read, (format 2, NIGHT16 U3-29.4) the three design.move flags the run read (slideCharge, slideDecay, hopFalloff: a
//     byte after the LAB's; a format-1 tape is read with all three OFF, explicitly: it was recorded on the web's mover),
//     (format 3, NIGHT17 U3-30.4) the five design.stamina flags the run read (bar, boost, sprint, refill, rewards: a byte
//     after the move flags; a format 1-2 tape is read with all five OFF, explicitly: it was recorded on the web's pips),
//     (format 4, NIGHT19) the seven NIGHT19 design blocks the run read (N19Blocks: a byte after the stamina flags; a format
//     1-3 tape is read with every block OFF, explicitly: it was recorded on NIGHT18's game), the
//     first-run script's state (onboarding.Active: passing it back as the setting gives the same result:
//     enabled && Active && !onboarded == Active), damageDirection (the hit arcs only: view state, not hashed), the health
//     at the first step (what automation set after StartRun: the fingerprint's and the smoke's 1e6), the profile as the
//     run started (its JSON: the unlocks, the HEAT rungs, the DAILY record, the onboarding flag), the step count, how the
//     tape ended, the step of the first exception and its text;
//   then the OPS, in the order the live run produced them (TapeOp):
//     STEP: a flag byte of what changed since the last step, then only those fields: the axes (moveX, moveZ), the look
//       after SampleMove (yaw, pitch: what every read in the step starts from), the frame's own yaw / pitch when they
//       differ from the look, the movement buttons (10 bits; NIGHT19: + the sprint key's 2, always 0 before format 4), a grapple target, the combat buttons (10 bits), the weapon
//       slot, the onScreen answers of the step in call order (a count and the bits); then a 32-bit light hash of the
//       state after the step (TapeReplay.Light: the first differing step, between the 600-step checkpoints)
//     STEP_ABORTED: a step that threw (no light hash: the replay must throw there too)
//     ASSIST: the assist mode / snapStrength / breakaway cap when they changed (read before the step that uses them)
//     TOGGLE: a lock toggle between steps (V, the pad's lockOn) with the look it aimed along
//     DRAFT_OPEN / DRAFT_PICK (the card's index) / DRAFT_REROLL / MOD_GIVE: the MODS draft between steps
//     END_RUN: EndRun between steps (R / RETRY from DOWN: downSteps = 0, EndRun)
//     CHECK: SimFingerprint.Hash after step N (every 600 steps and at the end)
// Raw doubles throughout (bit-exact); ~140 KB per minute of mouse play (the look moves every step). No UnityEngine use.
using System;
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;

namespace Skygrind.Game
{
    /// <summary>How a tape ended.</summary>
    public enum TapeEnd : byte
    {
        Open = 0, // still recording (a snapshot: the exception path, COPY REPORT)
        Results = 1, // the run reached RESULTS inside a step (DOWN's clock, an exfil): a last checkpoint after that step
        EndedBetweenSteps = 2, // EndRun between steps (R / RETRY from DOWN): END_RUN, then a last checkpoint
        Stopped = 3, // the caller stopped it (tests, the harness): a last checkpoint
        Abandoned = 4, // the run was left without RESULTS (QUIT to TITLE, a new run over it, the app closed): no last checkpoint (the light hashes cover the tail)
        InputSwapped = 5, // someone replaced the sim's input after the tape began (UseInput): the tape stops at the last recorded step
        Restored = 6, // a resumed run (its state came from a snapshot after StartRun): not replayable from StartRun, never kept
    }

    public sealed class TapeFormatException : Exception
    {
        public TapeFormatException(string message) : base(message)
        {
        }
    }

    /// <summary>The op codes (one byte each).</summary>
    public static class TapeOp
    {
        public const byte Step = 1, StepAborted = 2, Assist = 3, Toggle = 4, DraftOpen = 5, DraftPick = 6, DraftReroll = 7, ModGive = 8, EndRun = 9, Check = 10, RevealPick = 11; // NIGHT20: RevealPick (the reward reveal's pick: kind, type, points, round, feet)
    }

    /// <summary>One step's input as recorded (the delta base for the next): what SampleMove / SampleCombat returned and the look.</summary>
    public struct TapeStepState
    {
        public double moveX, moveZ, yaw, pitch, frameYaw, framePitch;
        public int moveButtons, combatButtons, weapon;
        public bool hasGrapple;
        public GrappleTarget grapple;

        public static TapeStepState Neutral => new TapeStepState { weapon = -1, yaw = double.NaN, pitch = double.NaN };

        // The step's flag byte.
        public const byte FAxes = 0x01, FLook = 0x02, FFrameLook = 0x04, FMoveButtons = 0x08, FGrapple = 0x10, FCombatButtons = 0x20, FWeapon = 0x40, FOnScreen = 0x80;

        static bool Same(double a, double b) => BitConverter.DoubleToInt64Bits(a) == BitConverter.DoubleToInt64Bits(b);

        public static int PackMove(in InputFrame f) =>
            Pack(f.jump, 0) | Pack(f.dash, 2) | Pack(f.slide, 4) | Pack(f.grapple, 6) | Pack(f.reel, 8) | Pack(f.sprint, 10); // NIGHT19: the sprint key (bits 10-11, in the same U16)

        public static int PackCombat(in CombatInput c) =>
            Pack(c.fire, 0) | Pack(c.altFire, 2) | Pack(c.special, 4) | Pack(c.punch, 6) | Pack(c.interact, 8);

        static int Pack(Button b, int at) => (b.held ? 1 << at : 0) | (b.pressed ? 2 << at : 0);

        static Button Unpack(int bits, int at) => new Button { held = (bits & (1 << at)) != 0, pressed = (bits & (2 << at)) != 0 };

        public InputFrame Frame() => new InputFrame
        {
            moveX = moveX, moveZ = moveZ, yaw = frameYaw, pitch = framePitch,
            jump = Unpack(moveButtons, 0), dash = Unpack(moveButtons, 2), slide = Unpack(moveButtons, 4), grapple = Unpack(moveButtons, 6), reel = Unpack(moveButtons, 8),
            sprint = Unpack(moveButtons, 10), // NIGHT19 (always 0 in a format 1-3 tape)
            grappleTarget = hasGrapple ? grapple : (GrappleTarget?)null,
        };

        public CombatInput Combat() => new CombatInput
        {
            fire = Unpack(combatButtons, 0), altFire = Unpack(combatButtons, 2), special = Unpack(combatButtons, 4), punch = Unpack(combatButtons, 6), interact = Unpack(combatButtons, 8),
            weapon = weapon,
        };

        /// <summary>Write `cur` as a STEP (or STEP_ABORTED) op against `prev`, with the step's onScreen bits and (a whole step) its light hash.</summary>
        public static void Write(TapeWriter w, in TapeStepState prev, in TapeStepState cur, byte[] bits, int bitCount, bool aborted, uint light)
        {
            byte f = 0;
            if (!Same(cur.moveX, prev.moveX) || !Same(cur.moveZ, prev.moveZ)) f |= FAxes;
            if (!Same(cur.yaw, prev.yaw) || !Same(cur.pitch, prev.pitch)) f |= FLook;
            if (!Same(cur.frameYaw, cur.yaw) || !Same(cur.framePitch, cur.pitch)) f |= FFrameLook;
            if (cur.moveButtons != prev.moveButtons) f |= FMoveButtons;
            if (cur.hasGrapple) f |= FGrapple;
            if (cur.combatButtons != prev.combatButtons) f |= FCombatButtons;
            if (cur.weapon != prev.weapon) f |= FWeapon;
            if (bitCount > 0) f |= FOnScreen;
            w.Byte(aborted ? TapeOp.StepAborted : TapeOp.Step);
            w.Byte(f);
            if ((f & FAxes) != 0)
            {
                w.D(cur.moveX);
                w.D(cur.moveZ);
            }
            if ((f & FLook) != 0)
            {
                w.D(cur.yaw);
                w.D(cur.pitch);
            }
            if ((f & FFrameLook) != 0)
            {
                w.D(cur.frameYaw);
                w.D(cur.framePitch);
            }
            if ((f & FMoveButtons) != 0) w.U16(cur.moveButtons);
            if ((f & FGrapple) != 0)
            {
                w.D(cur.grapple.point.x);
                w.D(cur.grapple.point.y);
                w.D(cur.grapple.point.z);
                w.VarI(cur.grapple.node);
            }
            if ((f & FCombatButtons) != 0) w.U16(cur.combatButtons);
            if ((f & FWeapon) != 0) w.VarI(cur.weapon);
            if ((f & FOnScreen) != 0)
            {
                w.VarU((uint)bitCount);
                w.Bytes(bits, (bitCount + 7) >> 3);
            }
            if (!aborted) w.U32(light);
        }

        /// <summary>Read a STEP / STEP_ABORTED op's body (its tag already read) onto `s` (the last step's state): the onScreen bits into `bits` (grown when short).</summary>
        public static void Read(TapeReader r, bool aborted, ref TapeStepState s, ref byte[] bits, out int bitCount, out uint light)
        {
            byte f = r.Byte();
            if ((f & FAxes) != 0)
            {
                s.moveX = r.D();
                s.moveZ = r.D();
            }
            if ((f & FLook) != 0)
            {
                s.yaw = r.D();
                s.pitch = r.D();
            }
            if ((f & FFrameLook) != 0)
            {
                s.frameYaw = r.D();
                s.framePitch = r.D();
            }
            else
            {
                s.frameYaw = s.yaw;
                s.framePitch = s.pitch;
            }
            if ((f & FMoveButtons) != 0) s.moveButtons = r.U16();
            s.hasGrapple = (f & FGrapple) != 0;
            if (s.hasGrapple)
            {
                double x = r.D(), y = r.D(), z = r.D();
                s.grapple = new GrappleTarget { point = new Vec3(x, y, z), node = r.VarI() };
            }
            if ((f & FCombatButtons) != 0) s.combatButtons = r.U16();
            if ((f & FWeapon) != 0) s.weapon = r.VarI();
            bitCount = 0;
            if ((f & FOnScreen) != 0)
            {
                bitCount = (int)r.VarU();
                int n = (bitCount + 7) >> 3;
                if (bits == null || bits.Length < n) bits = new byte[Math.Max(n, 16)];
                for (int i = 0; i < n; i++) bits[i] = r.Byte();
            }
            light = aborted ? 0 : r.U32();
        }
    }

    public sealed class RunTape
    {
        /// <summary>The tape format's version (a tape of another version is refused with a line, never a crash).</summary>
        public const int FormatVersion = 27; // NIGHT46 rows: 27 adds the rows record (Core/NightRows.cs, TapeRows; formats 1-26: every NIGHT46+ row off; no later night bumps the tape for a row); NIGHT43: 26 adds the NIGHT43 blocks as a VarI (formats 1-25: off); NIGHT42: 25 adds the NIGHT42 blocks as a VarI (formats 1-24: off); NIGHT41: 24 adds the NIGHT41 blocks as a VarI (formats 1-23: off); NIGHT40: 23 adds the NIGHT40 blocks as a VarI (formats 1-22: off); NIGHT39: 22 adds the NIGHT39 blocks as a VarI (formats 1-21: off); NIGHT38: 21 adds the NIGHT38 blocks as a VarI (formats 1-20: off); NIGHT37: 20 adds the NIGHT37 blocks as a VarI (formats 1-19: off); NIGHT36: 19 adds the NIGHT36 blocks as a VarI (nine bits; formats 1-18: off); NIGHT35: 18 adds the NIGHT35 blocks (formats 1-17: off); NIGHT34: 17 adds the NIGHT34 blocks (formats 1-16: off); NIGHT33: 16 adds the NIGHT33 blocks, the shop's owned upgrades and the banked points (formats 1-15: off, none, 0); NIGHT32: 15 adds the NIGHT32 blocks (formats 1-14: off); NIGHT31: 14 adds the NIGHT31 blocks (formats 1-13: off); NIGHT30: 13 adds the NIGHT30 blocks (formats 1-12: off); NIGHT29: 12 adds the NIGHT29 blocks (formats 1-11: off); NIGHT16 U3-29.4: 2 adds the move flags (format 1 is still read: the flags off); NIGHT17 U3-30.4: 3 adds the stamina flags (formats 1-2: off); NIGHT27: 11 adds the NIGHT27 blocks (formats 1-10: off); NIGHT26: 10 adds the NIGHT26 blocks (formats 1-9: off); NIGHT19: 4 adds the NIGHT19 blocks (formats 1-3: off); NIGHT20: 5 adds the NIGHT20 blocks (formats 1-4: off); NIGHT21: 6 adds the NIGHT21 blocks (formats 1-5: off); NIGHT22: 7 adds the NIGHT22 blocks (formats 1-6: off; the run mode is already in every format)

        /// <summary>A tape file's extension (the run log's name with this in place of .jsonl).</summary>
        public const string Extension = ".sktape";

        public static readonly byte[] Magic = { (byte)'S', (byte)'K', (byte)'T', (byte)'A', (byte)'P', (byte)'E' };

        // --- the header ---
        public string buildStamp = "";
        public string baselineId = "";
        public uint seed;
        public string mode = RunSetup.Survival;
        public int heat;
        public long utcTicks;
        public bool noRecords, noLab, metaSilent, onboarding, damageDirection;
        /// <summary>The LAB toggles the run read: closeKillHeal, freshness, styleRamp, readyFromOne, squadTighten, ultraHealth (Lab's order).</summary>
        public bool[] lab = new bool[6];
        /// <summary>NIGHT16 U3-29.4: the design.move flags the run read: slideCharge, slideDecay, hopFalloff (format 1: all off, the web's mover).</summary>
        public bool[] move = new bool[3];
        /// <summary>NIGHT17 U3-30.4: the design.stamina flags the run read: bar, boost, sprint, refill, rewards (formats 1-2: all off, the web's pips).</summary>
        public bool[] stamina = new bool[Stamina.FlagCount];
        /// <summary>NIGHT19: the NIGHT19 design blocks the run read (N19Blocks.Bits; formats 1-3: 0, every block off, NIGHT18's game).</summary>
        public int n19;
        /// <summary>NIGHT20: the NIGHT20 design blocks the run read (N20Blocks.Bits; formats 1-4: 0, every block off, NIGHT19's game).</summary>
        public int n20;
        /// <summary>NIGHT21: the NIGHT21 design blocks the run read (N21Blocks.Bits; formats 1-5: 0, every block off, NIGHT20's game).</summary>
        public int n21;
        /// <summary>NIGHT22: the NIGHT22 design blocks the run read (N22Blocks.Bits; formats 1-6: 0, every block off, NIGHT21's game).</summary>
        public int n22;
        /// <summary>NIGHT24: the NIGHT24 design blocks the run read (N24Blocks.Bits, two bytes; formats 1-7: 0, every block off, NIGHT22's game).</summary>
        public int n24;
        /// <summary>NIGHT24: the campaign state the run read (format 8: level id, room, checkpoint, limit flags; formats 1-7: none).</summary>
        public Campaign24State campaign = Campaign24State.None;
        /// <summary>NIGHT25: the NIGHT25 design blocks the run read (N25Blocks.Bits; formats 1-8: 0, every block off, NIGHT24's game).</summary>
        public int n25;

        /// <summary>NIGHT26: the NIGHT26 design blocks the run read (N26Blocks.Bits; formats 1-9: 0, every block off, NIGHT25's game).</summary>
        public int n26;

        /// <summary>NIGHT27: the NIGHT27 design blocks the run read (N27Blocks.Bits; formats 1-10: 0, every block off, NIGHT26's game).</summary>
        public int n27;

        /// <summary>NIGHT29: the NIGHT29 design blocks the run read (N29Blocks.Bits; formats 1-11: 0, every block off, NIGHT28's game).</summary>
        public int n29;

        /// <summary>NIGHT30: the NIGHT30 design blocks the run read (N30Blocks.Bits; formats 1-12: 0, every block off, NIGHT29's game).</summary>
        public int n30;

        /// <summary>NIGHT31: the NIGHT31 design blocks the run read (N31Blocks.Bits; formats 1-13: 0, every block off, NIGHT30's game).</summary>
        public int n31;

        /// <summary>NIGHT32: the NIGHT32 design blocks the run read (N32Blocks.Bits; formats 1-14: 0, every block off, NIGHT31's game).</summary>
        public int n32;

        /// <summary>NIGHT33: the NIGHT33 design blocks the run read (N33Blocks.Bits; formats 1-15: 0, every block off, NIGHT32's game).</summary>
        public int n33;

        /// <summary>NIGHT33 P: the shop's owned upgrades the run started with (SurvivalSim.shop33Owned; formats 1-15: 0, none).</summary>
        public int shop33;

        /// <summary>NIGHT33 P: the campaign's banked points the run started with (SurvivalSim.bank33; formats 1-15: 0).</summary>
        public int bank33;

        /// <summary>NIGHT34: the NIGHT34 design blocks the run read (N34Blocks.Bits; formats 1-16: 0, every block off, NIGHT33's game).</summary>
        public int n34;

        /// <summary>NIGHT35: the NIGHT35 design blocks the run read (N35Blocks.Bits; formats 1-17: 0, every block off, NIGHT34's game).</summary>
        public int n35;

        /// <summary>NIGHT36: the NIGHT36 design blocks the run read (N36Blocks.Bits; formats 1-18: 0, every block off, NIGHT35's game).</summary>
        public int n36;

        /// <summary>NIGHT37: the NIGHT37 design blocks the run read (N37Blocks.Bits; formats 1-19: 0, every block off, NIGHT36's game).</summary>
        public int n37;

        /// <summary>NIGHT38: the NIGHT38 design blocks the run read (N38Blocks.Bits; formats 1-20: 0, every block off, NIGHT37's game).</summary>
        public int n38;

        /// <summary>NIGHT39: the NIGHT39 design blocks the run read (N39Blocks.Bits; formats 1-21: 0, every block off, NIGHT38's game).</summary>
        public int n39;

        /// <summary>NIGHT40: the NIGHT40 design blocks the run read (N40Blocks.Bits; formats 1-22: 0, every block off, NIGHT39's game).</summary>
        public int n40;

        /// <summary>NIGHT41: the NIGHT41 design blocks the run read (N41Blocks.Bits; formats 1-23: 0, every block off, NIGHT40's game).</summary>
        public int n41;

        /// <summary>NIGHT42: the NIGHT42 design blocks the run read (N42Blocks.Bits; formats 1-24: 0, every block off, NIGHT41's game).</summary>
        public int n42;

        /// <summary>NIGHT43: the NIGHT43 design blocks the run read (N43Blocks.Bits; formats 1-25: 0, every block off, NIGHT42's game).</summary>
        public int n43;

        /// <summary>NIGHT46 rows: the NIGHT46+ sim rows the run read, as NightRowsTape's record (name -> bit; formats 1-26: null, every row off).</summary>
        public byte[] rows;
        public bool hasStart;
        public double startMax, startHp;
        public string profileJson = "";
        public int steps;
        public TapeEnd end;
        public int exceptionStep = -1;
        public string exceptionText = "";

        // --- the ops ---
        public byte[] ops = new byte[0];
        public int opsLength;

        public DateTime Utc => new DateTime(utcTicks, DateTimeKind.Utc);

        /// <summary>FingerprintBaseline's last hash: the sim the tape was recorded on (a tape from another baseline may differ for that reason).</summary>
        public static string CurrentBaselineId()
        {
            string[] active = FingerprintBaseline.Active; // GG 2026-10-26: the ULTRAKILL profile's baseline in the Player, the web's elsewhere
            string last = active[active.Length - 1];
            int at = last.LastIndexOf(' ');
            return at >= 0 ? last.Substring(at + 1) : last;
        }

        /// <summary>The header copied (a snapshot's base).</summary>
        public RunTape CloneHeader()
        {
            var t = (RunTape)MemberwiseClone();
            t.lab = (bool[])lab.Clone();
            t.move = (bool[])move.Clone();
            t.ops = new byte[0];
            t.opsLength = 0;
            return t;
        }

        // --- the ops, scanned ---

        /// <summary>The checkpoints (step, hash) in the order recorded.</summary>
        public List<(int step, ulong hash)> Checkpoints()
        {
            var list = new List<(int, ulong)>();
            Scan((tag, r) =>
            {
                if (tag == TapeOp.Check) list.Add(((int)r.VarU(), r.U64()));
                else SkipBody(tag, r);
            });
            return list;
        }

        /// <summary>The checkpoints as the fingerprint prints them ("step 600 hash 42c74be7ac21a5b3").</summary>
        public List<string> CheckpointLines()
        {
            var lines = new List<string>();
            foreach (var (step, hash) in Checkpoints()) lines.Add($"step {step} hash {hash:x16}");
            return lines;
        }

        /// <summary>How many ops of a kind the tape holds.</summary>
        public int CountOps(byte op)
        {
            int n = 0;
            Scan((tag, r) =>
            {
                if (tag == op) n++;
                SkipBody(tag, r);
            });
            return n;
        }

        /// <summary>onScreen answers recorded over the run.</summary>
        public int OnScreenAnswers
        {
            get
            {
                int n = 0;
                var s = TapeStepState.Neutral;
                byte[] bits = null;
                Scan((tag, r) =>
                {
                    if (tag == TapeOp.Step || tag == TapeOp.StepAborted)
                    {
                        TapeStepState.Read(r, tag == TapeOp.StepAborted, ref s, ref bits, out int count, out _);
                        n += count;
                    }
                    else SkipBody(tag, r);
                });
                return n;
            }
        }

        void Scan(Action<byte, TapeReader> each)
        {
            var r = new TapeReader(ops, 0, opsLength);
            while (r.More) each(r.Byte(), r);
        }

        /// <summary>Read past an op's body (its tag already read).</summary>
        public static void SkipBody(byte tag, TapeReader r)
        {
            switch (tag)
            {
                case TapeOp.Step:
                case TapeOp.StepAborted:
                {
                    var s = TapeStepState.Neutral;
                    byte[] bits = null;
                    TapeStepState.Read(r, tag == TapeOp.StepAborted, ref s, ref bits, out _, out _);
                    break;
                }
                case TapeOp.Assist:
                    ReadAssist(r, out _, out _, out _, out _);
                    break;
                case TapeOp.Toggle:
                    r.Skip(16);
                    break;
                case TapeOp.DraftOpen:
                case TapeOp.DraftReroll:
                case TapeOp.EndRun:
                    break;
                case TapeOp.DraftPick:
                    r.VarI();
                    break;
                case TapeOp.ModGive:
                    r.Str();
                    r.VarI();
                    break;
                case TapeOp.RevealPick: // NIGHT20
                    r.VarI();
                    r.Str();
                    r.VarI();
                    r.VarI();
                    r.D();
                    r.D();
                    r.D();
                    break;
                case TapeOp.Check:
                    r.VarU();
                    r.U64();
                    break;
                default:
                    throw new TapeFormatException($"unknown op {tag} at byte {r.Position - 1}");
            }
        }

        // ASSIST's body: a flag byte (1 mode, 2 snapStrength, 4 the breakaway cap), then those values.
        public static void WriteAssist(TapeWriter w, byte f, string mode, double snap, double cap)
        {
            w.Byte(TapeOp.Assist);
            w.Byte(f);
            if ((f & 1) != 0) w.Str(mode);
            if ((f & 2) != 0) w.D(snap);
            if ((f & 4) != 0) w.D(cap);
        }

        public static void ReadAssist(TapeReader r, out byte f, out string mode, out double snap, out double cap)
        {
            f = r.Byte();
            mode = (f & 1) != 0 ? r.Str() : null;
            snap = (f & 2) != 0 ? r.D() : 0;
            cap = (f & 4) != 0 ? r.D() : 0;
        }

        // --- the file ---

        public byte[] ToBytes()
        {
            var w = new TapeWriter(opsLength + 1024 + 2 * (profileJson?.Length ?? 0));
            foreach (byte b in Magic) w.Byte(b);
            w.U32(FormatVersion);
            w.Str(buildStamp);
            w.Str(baselineId);
            w.U32(seed);
            w.Str(mode);
            w.VarI(heat);
            w.U64(unchecked((ulong)utcTicks));
            int flags = (noRecords ? 1 : 0) | (noLab ? 2 : 0) | (metaSilent ? 4 : 0) | (onboarding ? 8 : 0) | (damageDirection ? 16 : 0) | (hasStart ? 32 : 0);
            w.Byte((byte)flags);
            int labBits = 0;
            for (int i = 0; i < lab.Length; i++)
                if (lab[i]) labBits |= 1 << i;
            w.Byte((byte)labBits);
            int moveBits = 0;
            for (int i = 0; i < move.Length; i++)
                if (move[i]) moveBits |= 1 << i;
            w.Byte((byte)moveBits); // format 2
            int staminaBits = 0;
            for (int i = 0; i < stamina.Length; i++)
                if (stamina[i]) staminaBits |= 1 << i;
            w.Byte((byte)staminaBits); // format 3
            w.Byte((byte)n19); // format 4
            w.Byte((byte)n20); // format 5
            w.Byte((byte)n21); // format 6
            w.Byte((byte)n22); // format 7
            w.Byte((byte)(n24 & 255)); // format 8: the NIGHT24 blocks (9 bits: two bytes) ...
            w.Byte((byte)((n24 >> 8) & 255));
            w.Str(campaign.levelId ?? ""); // ... and the campaign state
            w.VarI(campaign.room);
            w.VarI(campaign.checkpoint);
            w.VarI(campaign.limits);
            w.Byte((byte)n25); // format 9: the NIGHT25 blocks
            w.Byte((byte)n26); // format 10: the NIGHT26 blocks
            w.Byte((byte)n27); // format 11: the NIGHT27 blocks
            w.Byte((byte)n29); // format 12: the NIGHT29 blocks
            w.Byte((byte)n30); // format 13: the NIGHT30 blocks
            w.Byte((byte)n31); // format 14: the NIGHT31 blocks
            w.Byte((byte)n32); // format 15: the NIGHT32 blocks
            w.Byte((byte)n33); // format 16: the NIGHT33 blocks ...
            w.VarI(shop33); // ... the shop's owned upgrades ...
            w.VarI(bank33); // ... and the banked points
            w.Byte((byte)n34); // format 17: the NIGHT34 blocks
            w.Byte((byte)n35); // format 18: the NIGHT35 blocks
            w.VarI(n36); // format 19: the NIGHT36 blocks (nine bits)
            w.VarI(n37); // format 20: the NIGHT37 blocks
            w.VarI(n38); // format 21: the NIGHT38 blocks
            w.VarI(n39); // format 22: the NIGHT39 blocks
            w.VarI(n40); // format 23: the NIGHT40 blocks
            w.VarI(n41); // format 24: the NIGHT41 blocks
            w.VarI(n42); // format 25: the NIGHT42 blocks
            w.VarI(n43); // format 26: the NIGHT43 blocks
            TapeRows.Write(w, rows); // format 27: the NIGHT46 rows record (NIGHT46 rows)
            w.D(startMax);
            w.D(startHp);
            w.Str(profileJson);
            w.VarU((uint)steps);
            w.Byte((byte)end);
            w.VarI(exceptionStep);
            w.Str(exceptionText);
            w.VarU((uint)opsLength);
            w.Bytes(ops, opsLength);
            var bytes = new byte[w.length];
            Buffer.BlockCopy(w.data, 0, bytes, 0, w.length);
            return bytes;
        }

        /// <summary>A tape from its bytes; TapeFormatException for a foreign file, another format version or a truncated tape.</summary>
        public static RunTape FromBytes(byte[] bytes)
        {
            if (bytes == null || bytes.Length < Magic.Length + 4) throw new TapeFormatException("not a SKYGRIND tape (too short)");
            for (int i = 0; i < Magic.Length; i++)
                if (bytes[i] != Magic[i]) throw new TapeFormatException("not a SKYGRIND tape (no SKTAPE mark)");
            var r = new TapeReader(bytes, Magic.Length, bytes.Length);
            int version = unchecked((int)r.U32());
            if (version < 1 || version > FormatVersion) throw new TapeFormatException($"tape format {version}, this build reads formats 1 to {FormatVersion} only");
            var t = new RunTape
            {
                buildStamp = r.Str() ?? "",
                baselineId = r.Str() ?? "",
                seed = r.U32(),
                mode = r.Str() ?? RunSetup.Survival,
                heat = r.VarI(),
                utcTicks = unchecked((long)r.U64()),
            };
            int flags = r.Byte();
            t.noRecords = (flags & 1) != 0;
            t.noLab = (flags & 2) != 0;
            t.metaSilent = (flags & 4) != 0;
            t.onboarding = (flags & 8) != 0;
            t.damageDirection = (flags & 16) != 0;
            t.hasStart = (flags & 32) != 0;
            int labBits = r.Byte();
            for (int i = 0; i < t.lab.Length; i++) t.lab[i] = (labBits & (1 << i)) != 0;
            int moveBits = version >= 2 ? r.Byte() : 0; // format 1: recorded on the web's mover, so every move flag OFF (explicitly)
            for (int i = 0; i < t.move.Length; i++) t.move[i] = (moveBits & (1 << i)) != 0;
            int staminaBits = version >= 3 ? r.Byte() : 0; // formats 1-2: recorded on the web's pips, so every stamina flag OFF (explicitly)
            for (int i = 0; i < t.stamina.Length; i++) t.stamina[i] = (staminaBits & (1 << i)) != 0;
            t.n19 = version >= 4 ? r.Byte() : 0; // formats 1-3: recorded on NIGHT18's game, so every NIGHT19 block OFF (explicitly)
            t.n20 = version >= 5 ? r.Byte() : 0; // formats 1-4: recorded on NIGHT19's game (or before), so every NIGHT20 block OFF (explicitly)
            t.n21 = version >= 6 ? r.Byte() : 0; // formats 1-5: recorded on NIGHT20's game (or before), so every NIGHT21 block OFF (explicitly)
            t.n22 = version >= 7 ? r.Byte() : 0; // formats 1-6: recorded on NIGHT21's game (or before), so every NIGHT22 block OFF (explicitly)
            if (version >= 8) // formats 1-7: recorded on NIGHT22's game (or before): every NIGHT24 block OFF and no campaign (explicitly)
            {
                t.n24 = r.Byte() | (r.Byte() << 8);
                t.campaign = new Campaign24State { levelId = r.Str() ?? "", room = r.VarI(), checkpoint = r.VarI(), limits = r.VarI() };
            }
            t.n25 = version >= 9 ? r.Byte() : 0; // formats 1-8: recorded on NIGHT24's game (or before), so every NIGHT25 block OFF (explicitly)
            t.n26 = version >= 10 ? r.Byte() : 0; // formats 1-9: recorded on NIGHT25's game (or before), so every NIGHT26 block OFF (explicitly)
            t.n27 = version >= 11 ? r.Byte() : 0; // formats 1-10: recorded on NIGHT26's game (or before), so every NIGHT27 block OFF (explicitly)
            t.n29 = version >= 12 ? r.Byte() : 0; // formats 1-11: recorded on NIGHT28's game (or before), so every NIGHT29 block OFF (explicitly)
            t.n30 = version >= 13 ? r.Byte() : 0; // formats 1-12: recorded on NIGHT29's game (or before), so every NIGHT30 block OFF (explicitly)
            t.n31 = version >= 14 ? r.Byte() : 0; // formats 1-13: recorded on NIGHT30's game (or before), so every NIGHT31 block OFF (explicitly)
            t.n32 = version >= 15 ? r.Byte() : 0; // formats 1-14: recorded on NIGHT31's game (or before), so every NIGHT32 block OFF (explicitly)
            if (version >= 16) // formats 1-15: recorded on NIGHT32's game (or before): every NIGHT33 block OFF, nothing bought, nothing banked (explicitly)
            {
                t.n33 = r.Byte();
                t.shop33 = r.VarI();
                t.bank33 = r.VarI();
            }
            t.n34 = version >= 17 ? r.Byte() : 0; // formats 1-16: recorded on NIGHT33's game (or before), so every NIGHT34 block OFF (explicitly)
            t.n35 = version >= 18 ? r.Byte() : 0; // formats 1-17: recorded on NIGHT34's game (or before), so every NIGHT35 block OFF (explicitly)
            t.n36 = version >= 19 ? r.VarI() : 0; // formats 1-18: recorded on NIGHT35's game (or before), so every NIGHT36 block OFF (explicitly)
            t.n37 = version >= 20 ? r.VarI() : 0; // formats 1-19: recorded on NIGHT36's game (or before), so every NIGHT37 block OFF (explicitly)
            t.n38 = version >= 21 ? r.VarI() : 0; // formats 1-20: recorded on NIGHT37's game (or before), so every NIGHT38 block OFF (explicitly)
            t.n39 = version >= 22 ? r.VarI() : 0; // formats 1-21: recorded on NIGHT38's game (or before), so every NIGHT39 block OFF (explicitly)
            t.n40 = version >= 23 ? r.VarI() : 0; // formats 1-22: recorded on NIGHT39's game (or before), so every NIGHT40 block OFF (explicitly)
            t.n41 = version >= 24 ? r.VarI() : 0; // formats 1-23: recorded on NIGHT40's game (or before), so every NIGHT41 block OFF (explicitly)
            t.n42 = version >= 25 ? r.VarI() : 0; // formats 1-24: recorded on NIGHT41's game (or before), so every NIGHT42 block OFF (explicitly)
            t.n43 = version >= 26 ? r.VarI() : 0; // formats 1-25: recorded on NIGHT42's game (or before), so every NIGHT43 block OFF (explicitly)
            t.rows = version >= NightRowsTape.FirstTapeFormat ? TapeRows.Read(bytes, r) : null; // NIGHT46 rows: formats 1-26 recorded before any NIGHT46 row, so every row OFF (explicitly)
            t.startMax = r.D();
            t.startHp = r.D();
            t.profileJson = r.Str() ?? "";
            t.steps = (int)r.VarU();
            t.end = (TapeEnd)r.Byte();
            t.exceptionStep = r.VarI();
            t.exceptionText = r.Str() ?? "";
            t.opsLength = (int)r.VarU();
            if (t.opsLength < 0 || t.opsLength > bytes.Length - r.Position) throw new TapeFormatException($"the tape is truncated ({t.opsLength} op bytes announced, {bytes.Length - r.Position} present)");
            t.ops = new byte[t.opsLength];
            Buffer.BlockCopy(bytes, r.Position, t.ops, 0, t.opsLength);
            return t;
        }

        public static RunTape Load(string path)
        {
            byte[] bytes;
            try
            {
                bytes = File.ReadAllBytes(path);
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is ArgumentException || e is NotSupportedException)
            {
                throw new TapeFormatException($"no tape at {path} ({e.GetType().Name}: {e.Message})");
            }
            return FromBytes(bytes);
        }

        public void Save(string path) => WriteFile(path, ToBytes());

        /// <summary>Write a tape's bytes through a temporary file (a crash mid-write never leaves half a tape under the name).</summary>
        public static void WriteFile(string path, byte[] bytes)
        {
            string dir = Path.GetDirectoryName(Path.GetFullPath(path));
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            string tmp = path + ".tmp";
            File.WriteAllBytes(tmp, bytes);
            if (File.Exists(path)) File.Delete(path);
            File.Move(tmp, path);
        }
    }
}
