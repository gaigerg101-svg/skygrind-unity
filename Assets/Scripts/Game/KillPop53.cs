// NIGHT53 builder U, polish53 item 5 (design.n53.polish53.numbers.killPop; GG: the UI "cheap", kills not "crunchy"): the kill confirm's POP. The
// NIGHT18 kill marker (UiMarkers.cs: four white wedges on the diagonals over an ink underlay, 360 ms) gets, on a kill, with killPop.on:
//   - a punch on top of its own scale curve: x killPop.punch at the kill's frame, back to x1 over killPop.punchS with an overshoot (OutBack);
//   - four bone SHARDS on the axes between the wedges (an eight-point burst with them), flung out from Polish53PopShardFrom to ...To px (at
//     1080p) over killPop.shardS (OutExpo) and fading (InQuad);
//   - the shards white-hot first: UIEffect's colour filter (Additive, white) at killPop.heat x the heat, which falls to 0 over killPop.heatS,
//     so they cool to bone. UIEffect only when its shader is in the build ("Hidden/UI/Default (UIEffect)": the project's registered variant
//     is COLOR_FILTER on that shader; never on TMP text: NIGHT21's magenta was a TMP variant the build lacked); without it the shards are bone.
// The motions are LitMotion's (manual dispatcher, stepped by HudFx20.Tick with the HUD's frame time from Ui.Render: the reel's fixed clock drives
// them as the HUD); the package code lives in Skygrind.HudFx20 (KillPop53Install.cs), which installs Animate / Hot / SetHeat at load. View only:
// no sim timer, no event. A headshot kill punches x killPop.headPunch more. REDUCED MOTION: no punch, the shards fade in place. Off (polish53 off,
// -skygrindOff53, killPop.on 0, or no package assembly): nothing is built; NIGHT52's marker exactly.
using System;
using Skygrind.Core;
using UnityEngine.UI;

namespace Skygrind.Game
{
    public static class KillPop53
    {
        /// <summary>Installed by Skygrind.HudFx20 (KillPop53Install): start the motions (peak scale, punch s, shard s, heat s, fling the shards).</summary>
        public static Action<float, float, float, float, bool> Animate;
        /// <summary>Installed: put UIEffect's colour filter on shard `i` (false when the effect's shader is not in this build).</summary>
        public static Func<int, Graphic, bool> Hot;
        /// <summary>Installed: shard `i`'s filter intensity.</summary>
        public static Action<int, float> SetHeat;

        /// <summary>The motions' outputs, read by the HUD each frame (idle: 1, 1, 0, 0).</summary>
        public static float ScaleK = 1f, ShardOut = 1f, ShardAlpha, Heat;

        /// <summary>The reel's probe: built this session, fired (kills), the last kill's frame, UIEffect on the shards.</summary>
        public static bool Built, FxOn;
        public static int Fired, LastFrame = -1;
        /// <summary>The a153 reel's kill-wash pairs: no pop (its HUD shards stay out of the wash box's measure).</summary>
        public static bool Suppress;

        /// <summary>The pop is built for this config: polish53 on, killPop.on, the package assembly installed.</summary>
        public static bool Wanted(GameConfig cfg) => Polish53.Item(cfg, "killPop") && Animate != null;

        /// <summary>From the kill marker's fire (UiMarkers.KillMarker17Fire): the pop starts (with polish53 live now: the reel's Force).</summary>
        public static void Fire(GameConfig cfg, bool headshot, bool reducedMotion, int frame)
        {
            if (!Built || Suppress || Animate == null || !Polish53.Item(cfg, "killPop")) return;
            float punch = (float)Polish53.Num(cfg, "killPop", "punch", 1);
            if (headshot) punch *= (float)Polish53.Num(cfg, "killPop", "headPunch", 1);
            if (reducedMotion) punch = 1f;
            float punchS = (float)Polish53.Num(cfg, "killPop", "punchS", 0.15), shardS = (float)Polish53.Num(cfg, "killPop", "shardS", 0.25), heatS = (float)Polish53.Num(cfg, "killPop", "heatS", 0.1);
            ScaleK = punch;
            ShardOut = reducedMotion ? 1f : 0f;
            ShardAlpha = 1f;
            Heat = 1f;
            Animate(punch, punchS, shardS, heatS, !reducedMotion);
            Fired++;
            LastFrame = frame;
        }
    }
}
