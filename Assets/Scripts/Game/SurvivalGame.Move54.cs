// NIGHT54 M move54 (Game/Move54.cs; design.n54.move54, a view row): SurvivalGame's hooks. The loop calls Move54StepEnded after each sim step
// (the feet's previous and current: what the step did, read only); the render hands feel.Apply the drawn feet through Move54Drawn (the camera's
// height only: `drawn` itself, the sim and every view that reads the feet are untouched). Row off: no Move54Step is built and both are no-ops.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        Move54Step move54Step;
        bool move54Read;

        /// <summary>The step smoother (null with the row off), built on first use from the game's config.</summary>
        public Move54Step Move54Steps
        {
            get
            {
                if (!move54Read && cfg != null)
                {
                    move54Read = true;
                    if (Move54.On(cfg)) move54Step = new Move54Step(cfg);
                }
                return move54Step;
            }
        }

        /// <summary>
        /// NIGHT54 M move54 (3), before the settings load (SurvivalGame.Init.cs): did the profile save a VSYNC value (the file, else the
        /// legacy PlayerPrefs copy)? Read before GameSettings, whose first boot saves every default (a default saved then is not a choice).
        /// </summary>
        bool Move54VsyncSaved()
        {
            if (cfg == null || !Move54.On(cfg)) return true;
            string path = System.IO.Path.Combine(UnityEngine.Application.persistentDataPath, UnityTuning.SettingsFileName);
            SettingsMath.PickSaved(SettingsFile.Read(path), UnityEngine.PlayerPrefs.GetString(SettingsStore.StorageKey, ""), out string json);
            if (string.IsNullOrEmpty(json)) return false;
            try { return JsJson.Parse(json) is System.Collections.Generic.Dictionary<string, object> d && d.ContainsKey(SettingsView.VsyncKey); }
            catch (System.Exception) { return true; } // unreadable: the store keeps its defaults; nothing changed here
        }

        /// <summary>NIGHT54 M move54 (3), after the settings load: with the row on and no VSYNC ever saved, VSYNC on
        /// (UnityTuning.Move54VsyncDefault: tearing on fast turns with it off and uncapped). A saved choice always wins; the settings UI is untouched.</summary>
        void Move54Vsync(bool saved)
        {
            if (saved || settings == null || !Move54.On(cfg)) return;
            if (settings.Bool(SettingsView.VsyncKey) != UnityTuning.Move54VsyncDefault) settings.Set(SettingsView.VsyncKey, UnityTuning.Move54VsyncDefault);
        }

        // NIGHT54 M move54 (6): each enemy's yaw before the step the views interpolate across (the positions' `previous`); filled before
        // every sim step with the row on, read by the views at the frame's alpha (shortest arc). Row off: empty, the views draw e.yaw.
        readonly System.Collections.Generic.Dictionary<Enemy, double> move54PrevYaw = new System.Collections.Generic.Dictionary<Enemy, double>();

        /// <summary>Before each sim step (SurvivalGame.cs Update): every live enemy's yaw now (read only).</summary>
        void Move54StepBegin()
        {
            if (Move54Steps == null) return;
            move54LookYaw0 = input.yaw;
            move54LookPitch0 = input.pitch;
            move54PrevYaw.Clear();
            System.Collections.Generic.List<Enemy> all = enemies.all;
            for (int i = 0; i < all.Count; i++)
            {
                Enemy e = all[i];
                if (e != null && e.alive) move54PrevYaw[e] = e.yaw;
            }
        }

        /// <summary>The yaw an enemy is drawn at: e.yaw with the row off (or no yaw before the last step: spawned in it); else from the
        /// yaw before the last step toward e.yaw by `alpha` (the alpha its position is drawn at), the shorter way round.</summary>
        public double Move54EnemyYaw(Enemy e, double alpha)
        {
            if (move54Step == null || e == null || !move54PrevYaw.TryGetValue(e, out double prev)) return e != null ? e.yaw : 0;
            double d = e.yaw - prev;
            d -= 2 * System.Math.PI * System.Math.Floor((d + System.Math.PI) / (2 * System.Math.PI));
            double a = alpha < 0 ? 0 : alpha > 1 ? 1 : alpha;
            return prev + d * a;
        }

        /// <summary>The world's interpolation for the views that read game.Alpha (Crown32View, Crown37View): the render half's wAlpha (a split
        /// bend's world alpha, NIGHT34; Alpha outside one) with the row on, Alpha with it off.</summary>
        public double Move54WorldAlpha => Move54Steps != null && sim != null ? sim.BendWorldAlpha(Alpha) : Alpha;

        /// <summary>The feet drawn this frame (the views that drew the latest step: SkateFx27's floor patch).</summary>
        public Vec3 Move54DrawnFeet => previous + (current - previous) * Alpha;

        Move54.SlowEase move54Slow;

        /// <summary>NIGHT54 M move54 (2), last in ImpactBeginFrame: the slow-mo's ease in (Move54.SlowEase); the share unchanged with the row
        /// off or while frozen. Pacing only (how many 120 Hz steps a frame runs), never a step's result.</summary>
        double Move54SlowFrame(double frameDt, double share)
        {
            if (Move54Steps == null || impactFrames == null) return share;
            if (move54Slow == null) move54Slow = new Move54.SlowEase(cfg);
            ImpactRules r = impactFrames.rules;
            double ss = r.SlowScale, run = ss > 1e-9 ? r.Scale / ss : 0;
            if (run <= 0 || share <= 0) return share; // frozen (or a source holding the world): untouched
            return move54Slow.Frame(frameDt, run, share / run);
        }

        /// <summary>The way to TITLE (ImpactToTitle): no ease left over.</summary>
        void Move54SlowHalt() => move54Slow?.Halt();

        // NIGHT54 M move54 (5): the look the last sim step itself turned (the aim assist's pull, the lock's turn: SurvivalSim.Run.cs sets the
        // reader's look once a step), presented across the step at the frame's alpha like the feet, instead of landing whole on the next frame
        // (a 120 Hz staircase on a 144-240 Hz display). The sim's yaw is never touched: only the camera's rotation lags the pull by under a step.
        double move54LookYaw0, move54LookPitch0, move54PullYaw, move54PullPitch, move54MaxPull = -1;

        /// <summary>After each sim step (SurvivalGame.cs Update): the step's feet, before and after; the look the step turned.</summary>
        void Move54StepEnded()
        {
            Step55StepEnded(); // NIGHT55 C step55 (SurvivalGame.N55C.cs): the step's feet into the step55 smoother (a no-op with the row off)
            Move54Step s = Move54Steps;
            if (s == null) return;
            s.StepEnded(mover, previous, current);
            if (move54MaxPull < 0) move54MaxPull = cfg.Num(Move54.Numbers + "look.maxStepPull");
            move54PullYaw = input.yaw - move54LookYaw0;
            move54PullPitch = input.pitch - move54LookPitch0;
            if (System.Math.Abs(move54PullYaw) > move54MaxPull || System.Math.Abs(move54PullPitch) > move54MaxPull) move54PullYaw = move54PullPitch = 0; // a snap: shown at once
        }

        /// <summary>The camera's yaw / pitch before its kicks: the reader's look less the last step's own turn not yet presented (row off, or
        /// not simulating: the reader's look).</summary>
        double Move54ViewYaw(double alpha, bool simulating) => move54Step != null && simulating ? input.yaw - move54PullYaw * (1 - Clamp01(alpha)) : input.yaw;
        double Move54ViewPitch(double alpha, bool simulating) => move54Step != null && simulating ? input.pitch - move54PullPitch * (1 - Clamp01(alpha)) : input.pitch;

        static double Clamp01(double a) => a < 0 ? 0 : a > 1 ? 1 : a;

        /// <summary>The feet the camera is drawn at (the render half): `drawn` plus the grounded step's view offset (row off: `drawn`).</summary>
        Vec3 Move54Drawn(Vec3 drawn, double frameDt, bool simulating)
        {
            if (Step55Drawn(ref drawn, simulating)) return drawn; // NIGHT55 C step55 (SurvivalGame.N55C.cs): the held step at alpha replaces move54's offset (row off: false)
            Move54Step s = Move54Steps;
            if (s == null) return drawn;
            double o = s.Frame(drawn.y, frameDt, simulating && flow == Flow.Playing && !attractOn);
            return o == 0 ? drawn : new Vec3(drawn.x, drawn.y + o, drawn.z);
        }
    }
}
