// NIGHT14 U3-27.9 (agent V): WHAT GOT YOU (design.recap; DESIGN-PILLARS 3, every run is readable), the frame loop's three
// hooks (SurvivalGame.Night14.cs). With design.recap.enabled the flight recorder (Recap/FlightRecorder.cs) records the
// last design.recap.seconds while the RECAP setting is on (no recorder and no subscription while it is off); on DOWN,
// before DOWN's clock runs, the last replaySeconds replay from an orbit camera (Recap/RecapStage.cs) with the killer
// outlined in red-orange and named (UiRecap.cs), the last slowSeconds at slowRate, the last frame held a beat
// (UnityTuning.RecapHoldSeconds); any key, a mouse button or the pad's B skips (after RecapSkipGrace). RecapGate holds the
// sim while it plays (DOWN's health.downTime waits), so when it ends or is skipped DOWN and RESULTS run exactly as before.
// A flow change (R's RETRY NOW, a new run, TITLE) or the attract stops it at once. With the flag off none of this exists
// (the hooks return at once): the old DOWN.
// Automation (the lead's captures): RecapPlayNow() plays it on demand from the ring (no death needed: the sim is held
// while it plays and goes on after), RecapPlaying / RecapKillerId / RecapKillerName / RecapKillerHowText say what it shows.
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        FlightRecorder recapRecorder; // null until the RECAP setting is first on (design.recap.enabled)
        RecapStage recapStage; // null: design.recap off (every hook returns at once)
        double recapSeconds, recapReplaySeconds, recapSlowSeconds, recapSlowRate; // design.recap, read once
        Dictionary<string, string> recapKindNames; // ui.recap.names by kind (RecapPlayNow's name before a death)
        int recapSettingsVersion = int.MinValue;
        int recapShownDeath; // the recorder's DeathSerial last replayed (one replay per death)
        bool recapPlaying;
        Flow recapFlow; // the flow it started in: a change stops it
        double recapClock, recapStart, recapEnd, recapSlowFrom, recapAge, recapHold;
        int recapKillerId = -1;
        string recapKillerName;
        RecapKillerHow recapKillerHow;

        /// <summary>WHAT GOT YOU is playing (the sim held, the live views hidden).</summary>
        public bool RecapPlaying => recapPlaying;

        /// <summary>The replay's killer: its enemy id (-1: none), its name (the recap's, e.g. HUSK), how it was found.</summary>
        public int RecapKillerId => recapKillerId;
        public string RecapKillerName => recapKillerName;
        public string RecapKillerHowText => recapKillerHow.ToString();

        /// <summary>The killer's head point in the viewport (UiRecap's tag), whether it is on screen, and in the drawn frame.</summary>
        public Vector3 RecapKillerViewport => recapStage != null ? recapStage.KillerViewport : default;
        public bool RecapKillerOnScreen => recapPlaying && recapStage != null && recapStage.KillerShown;

        /// <summary>The flight recorder (null: design.recap off or RECAP never on).</summary>
        public FlightRecorder RecapRecorder => recapRecorder;

        partial void BootRecap()
        {
            if (!FlightRecorder.Enabled(cfg)) return; // design.recap.enabled false: no recorder, no stage (the old DOWN)
            recapSeconds = cfg.Num("design.recap.seconds");
            recapReplaySeconds = cfg.Num("design.recap.replaySeconds");
            recapSlowSeconds = cfg.Num("design.recap.slowSeconds");
            recapSlowRate = cfg.Num("design.recap.slowRate");
            recapKindNames = new Dictionary<string, string>();
            foreach (var kv in cfg.Obj("ui.recap.names")) recapKindNames[kv.Key] = (string)kv.Value;
            recapStage = new RecapStage(this);
            SyncRecapSetting();
        }

        /// <summary>The RECAP setting (read when the settings change): on, the recorder exists and is subscribed; off, neither runs.</summary>
        void SyncRecapSetting()
        {
            if (settings.version == recapSettingsVersion) return;
            recapSettingsVersion = settings.version;
            if (settings.Bool(UnityTuning.RecapSetting))
            {
                recapRecorder ??= new FlightRecorder(sim, recapSeconds); // preallocated once, here (never per step)
                recapRecorder.Attach();
            }
            else if (recapRecorder != null)
            {
                if (recapPlaying) StopRecap(false);
                recapRecorder.Detach();
                recapRecorder.Clear();
            }
        }

        partial void RecapGate(ref bool simulating)
        {
            if (recapStage == null) return;
            SyncRecapSetting();
            if (recapPlaying)
            {
                if (sim.flow != recapFlow || attractOn)
                {
                    StopRecap(false); // R's RETRY NOW, a new run, TITLE: the new flow as it is
                    return;
                }
                if (recapAge >= UnityTuning.RecapSkipGrace && RecapSkipPressed())
                {
                    StopRecap(true);
                    return; // DOWN's clock runs from this frame
                }
                simulating = false;
                return;
            }
            if (RecapMaybeStart()) simulating = false;
        }

        /// <summary>A death not replayed yet (DOWN, the recorder's death, two frames at least): start its replay.</summary>
        bool RecapMaybeStart()
        {
            FlightRecorder r = recapRecorder;
            if (r == null || !r.Attached || attractOn) return false;
            if (sim.flow != Flow.Down || !r.HasDeath || r.DeathSerial == recapShownDeath || r.Count < 2) return false;
            recapShownDeath = r.DeathSerial;
            StartRecap(r.KillerId, r.KillerName, r.KillerHow);
            return true;
        }

        /// <summary>Any key, a mouse button or the pad's B this frame.</summary>
        static bool RecapSkipPressed()
        {
            Keyboard kb = Keyboard.current;
            if (kb != null && kb.anyKey.wasPressedThisFrame) return true;
            Mouse ms = Mouse.current;
            if (ms != null && (ms.leftButton.wasPressedThisFrame || ms.rightButton.wasPressedThisFrame || ms.middleButton.wasPressedThisFrame)) return true;
            Gamepad pad = Gamepad.current;
            return pad != null && pad.buttonEast.wasPressedThisFrame;
        }

        void StartRecap(int killerId, string killerName, RecapKillerHow how)
        {
            FlightRecorder r = recapRecorder;
            recapEnd = r.NewestTime;
            recapStart = System.Math.Max(r.OldestTime, recapEnd - recapReplaySeconds);
            recapSlowFrom = System.Math.Max(recapStart, recapEnd - recapSlowSeconds);
            recapClock = recapStart;
            recapAge = recapHold = 0;
            recapKillerId = killerId;
            recapKillerName = killerName;
            recapKillerHow = how;
            recapFlow = sim.flow;
            recapPlaying = true;
            recapStage.Begin(r);
            Debug.Log($"[recap] WHAT GOT YOU: killer {killerId} {killerName ?? "-"} ({how}), {recapEnd - recapStart:0.00} s of {r.Count} frames, flow {recapFlow}");
        }

        void StopRecap(bool skipped)
        {
            if (!recapPlaying) return;
            recapPlaying = false;
            recapStage.End();
            Debug.Log($"[recap] {(skipped ? "skipped" : sim.flow != recapFlow ? $"stopped ({sim.flow})" : "done")} at {recapClock - recapStart:0.00} / {recapEnd - recapStart:0.00} s");
        }

        partial void RecapRender(double frameDt, double alpha, ref bool drawn)
        {
            // The death's own frame starts it here (after its steps), so DOWN's screen never shows a frame before it.
            if (!recapPlaying && (recapStage == null || !RecapMaybeStart())) return;
            recapAge += frameDt;
            if (recapClock < recapEnd)
            {
                double rate = recapClock >= recapSlowFrom ? recapSlowRate : 1; // the last slowSeconds at slowRate
                recapClock = System.Math.Min(recapEnd, recapClock + frameDt * rate);
            }
            else
            {
                recapHold += frameDt;
                if (recapHold >= UnityTuning.RecapHoldSeconds)
                {
                    StopRecap(false); // the live frame from here (DOWN as before)
                    return;
                }
            }
            recapStage.Draw(recapRecorder, recapClock, frameDt, recapKillerId);
            lightAndLens?.post?.Render(frameDt); // the lens' pulses fade on frame time (the death's chroma kick doesn't stick)
            drawn = true;
        }

        /// <summary>
        /// Automation (captures, checks): play WHAT GOT YOU now from the ring, no death needed. The killer: `killerId` if
        /// given, else the run's death's, else the last hurt's enemy, else the enemy nearest you in the newest frame. The sim
        /// is held while it plays (in PLAYING too; the run goes on after). False: design.recap off, RECAP off or under 2 frames.
        /// </summary>
        public bool RecapPlayNow(int killerId = -1)
        {
            FlightRecorder r = recapRecorder;
            if (recapStage == null || r == null || !r.Attached || r.Count < 2) return false;
            if (recapPlaying) StopRecap(false);
            RecapKillerHow how = RecapKillerHow.None;
            if (killerId < 0)
            {
                if (r.HasDeath)
                {
                    killerId = r.KillerId;
                    how = r.KillerHow;
                }
                else if (r.LastHurtEnemy >= 0)
                {
                    killerId = r.LastHurtEnemy;
                    how = r.LastHurtHow;
                }
                else
                {
                    ref readonly RecapFrame f = ref r.Frame(r.Count - 1);
                    double best = double.PositiveInfinity;
                    for (int k = 0; k < f.enemyCount; k++)
                    {
                        ref readonly RecapEnemy e = ref r.Enemy(f, k);
                        double d = Vec3.DistanceTo(e.position, f.feet);
                        if (d >= best) continue;
                        best = d;
                        killerId = e.id;
                        how = RecapKillerHow.Nearest;
                    }
                }
            }
            string name = r.HasDeath && killerId == r.KillerId ? r.KillerName : RecapNameOf(r, killerId);
            if (r.HasDeath) recapShownDeath = r.DeathSerial; // this death's replay is this one (the gate won't play it again)
            StartRecap(killerId, name, how);
            return true;
        }

        /// <summary>The recap's name for enemy `id`'s kind (ui.recap.names), from the newest frame that has it; null when none does.</summary>
        string RecapNameOf(FlightRecorder r, int id)
        {
            if (id < 0) return null;
            for (int i = r.Count - 1; i >= 0; i--)
            {
                ref readonly RecapFrame f = ref r.Frame(i);
                int k = r.FindEnemy(f, id);
                if (k < 0) continue;
                string kind = RecapKind.Name(r.Enemy(f, k).kind);
                return recapKindNames.TryGetValue(kind, out string n) ? n : kind.ToUpperInvariant();
            }
            return null;
        }
    }
}
