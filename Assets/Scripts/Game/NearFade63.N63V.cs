// NIGHT63 builder V (leftover 7, C2 "the enemy glow up close"; gg-proxy-start.md #9: "enemies read as glow domes and flat beans"; view row
// design.n63.nearFade63, OFF in the repo; Tools/night/n63-v-c2-rows.cjs). View only: nothing here reaches the sim.
// THE RED (Tools/night/n63-v-near.py on n62-freeze's verbs37): an enemy at the eye in 348 of 9000 frames (runs up to 43 frames); vf/f0400 is
// 77 % enemy hue: the eye at husk 2's capsule (a screen-filling dark-red dome with an orange rim) and husk 3 behind it drawn as its x-ray
// silhouette (a flat red bean with the visor arc: Enemy.shader's x-ray pass draws where a nearer body hides it, ZTest Greater).
// THE RULE: each live enemy part fades as the eye comes within numbers.farM of the part's world bounds, easing (smoothstep) down to
// numbers.floor at numbers.nearM or nearer (inside the bounds: 0 m). The room shows through the near body.
// C2b (Logs/n63/verdict-C2.md section 6: C2 drove the death's own 3D-cell _Dissolve, which the critic read as "an exploding cloud of sheared
// flakes ... many of them BLACK", the ink51 hull's dark far side showing through every hole, and a kill's shatter look on a live enemy): the
// level drives a SCREEN-DOOR of its own (Enemy.shader's _NearFade, never _Dissolve): a 4 x 4 ordered (Bayer) dither in screen pixels,
// numbers.cellPx a cell at 1080p, so the near body is an even see-through screen; its rim stays solid (N.V under numbers.rimNdv: you still read
// where it is); its ink outline is cut at the same pixel (no dark far side through a hole) and thins to nothing at numbers.inkGone. Bayer and
// Kept are the shader's formula in C# (N63ScreenDoorTests). numbers.farM 0.9: 1.0 m is exactly whole (the punch reaches 2.0 m).
// D1's contract: EnemyLook.Frame reads the row and the eye once a frame (never cached across frames: a same-run toggle flips it) and sets the
// screen-door's globals; EnemyLook.Tint sets every live part's _NearFade from them (1, whole, when off) and compares it in its change cache. No
// state of its own.
// Off: the row off, -skygrindOff63 (or older), -skygrindNoNearFade63 (this row alone).
// THE ICEBREAKER (the lead's real-play bug, the same rule): on b1-b4 boss63's iceline flinch / stagger frames were black but for the HUD (its leap
// lands on the player, nothing in the sim keeps the player out of its body, and the stuck pose leans its dark violet torso 50 deg toward the eye).
// Its parts draw with the Stone / URP Lit material (no dissolve), so a part whose world bounds come within numbers.bossHideM of the eye is hidden
// (HideNear: Renderer.forceRenderingOff, every frame) and drawn again once the eye is clear: the room, its legs, its head and the stuck fist show.
using System;
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static class NearFade63
    {
        /// <summary>The view row's name (design.n63.nearFade63).</summary>
        public const string Row = "nearFade63";
        /// <summary>The one-row off switch: this row alone off.</summary>
        public const string NoSwitch = "-skygrindNoNearFade63";
        /// <summary>The row's night (its -skygrindOffNN switch covers it from this night on).</summary>
        public const int Night = 63;
        /// <summary>The row's numbers' prefix in the config.</summary>
        public const string Numbers = "design.n63.nearFade63.numbers.";

        /// <summary>design.n63.nearFade63.numbers (see the file's head).</summary>
        public sealed class Params
        {
            public float farM, nearM, floor;
            /// <summary>NIGHT63 (the Icebreaker at the eye): a boss part drawn with a material that has no dissolve is hidden while the eye is within
            /// this of its world bounds (0: never).</summary>
            public float bossHideM;
            /// <summary>NIGHT63 C2b: the screen-door's cell (px at 1080p; the shader takes under 1 as 1), the rim kept (a pixel whose N.V is
            /// under it is never cut), the level the ink outline is gone at (it thins from 1 down to it).</summary>
            public float cellPx, rimNdv, inkGone;

            /// <summary>farM above nearM (both >= 0) and the floor in [0, 1): else the rule draws every part whole.</summary>
            public bool Valid => farM > nearM && nearM >= 0 && floor >= 0 && floor < 1;

            public static Params From(GameConfig cfg)
            {
                var p = new Params();
                if (cfg == null) return p;
                float N(string k) => cfg.Has(Numbers + k) ? (float)cfg.Num(Numbers + k) : 0f;
                p.farM = N("farM");
                p.nearM = N("nearM");
                p.floor = N("floor");
                p.bossHideM = N("bossHideM");
                p.cellPx = N("cellPx");
                p.rimNdv = N("rimNdv");
                p.inkGone = N("inkGone");
                return p;
            }
        }

        static string[] args;

        static string[] Args()
        {
            if (args == null)
            {
                try { args = Environment.GetCommandLineArgs() ?? new string[0]; }
                catch (Exception) { args = new string[0]; }
            }
            return args;
        }

        /// <summary>Why the row is off for this config and command line (null: on).</summary>
        public static string OffReason(GameConfig cfg, string[] argv)
        {
            if (!NightRows.ViewOn(cfg, Row)) return "design.n63." + Row + " off";
            if (NightRows.OffFrom(argv, "-skygrindOff") <= Night) return "-skygrindOff63 (or older)";
            if (argv != null)
                for (int i = 0; i < argv.Length; i++)
                    if (string.Equals(argv[i], NoSwitch, StringComparison.OrdinalIgnoreCase)) return NoSwitch;
            return null;
        }

        public static bool OffFor(GameConfig cfg, string[] argv) => OffReason(cfg, argv) != null;

        /// <summary>NIGHT63 C2b: Enemy.shader's _NearFade (per part: 1 whole .. 0 gone).</summary>
        public static readonly int NearFadeId = Shader.PropertyToID("_NearFade");
        /// <summary>NIGHT63 C2b: Enemy.shader's _NearFade63 global (Globals).</summary>
        public static readonly int GlobalId = Shader.PropertyToID("_NearFade63");

        /// <summary>NIGHT63 C2b: the screen-door's threshold at cell (x, y), Enemy.shader's NearDither: (m + 0.5) / 16 with m the 4 x 4 ordered
        /// (Bayer) index, m = 4 b(x0, y0) + b(x1, y1), b(x, y) = 2 (x xor y) + y over the cell's low bit and next bit.</summary>
        public static float Bayer(int x, int y)
        {
            int x0 = x & 1, y0 = y & 1, x1 = (x >> 1) & 1, y1 = (y >> 1) & 1;
            int m = 4 * (2 * (x0 ^ y0) + y0) + (2 * (x1 ^ y1) + y1);
            return (m + 0.5f) / 16f;
        }

        /// <summary>NIGHT63 C2b: is cell (x, y) kept at `level`? The shader's clip(level - threshold) keeps 0, so a threshold at or under the level
        /// stays; a level of 1 or more keeps all (the shader returns before the clip).</summary>
        public static bool Kept(float level, int x, int y) => level >= 1f || Bayer(x, y) <= level;

        /// <summary>NIGHT63 C2b: the screen-door's globals for Enemy.shader's _NearFade63: x the cell (px at 1080p), y the rim kept, z the ink's
        /// gone level.</summary>
        public static Vector4 Globals(Params p) => p == null ? Vector4.zero : new Vector4(p.cellPx, p.rimNdv, p.inkGone, 0f);

        /// <summary>Is the row on now (this process's command line)? EnemyLook.Frame reads it once a frame.</summary>
        public static bool On(GameConfig cfg) => cfg != null && !OffFor(cfg, Args());

        /// <summary>The part's near-fade level (C2b: its _NearFade, the screen-door's; C2 drove _Dissolve with it): 1 (whole) with the eye farM or
        /// more from its world bounds `b`; `floor` at nearM or nearer (inside: 0 m); smoothstep between. No (or invalid) numbers: 1.</summary>
        public static float Dissolve(Params p, Bounds b, Vector3 eye)
        {
            if (p == null || !p.Valid) return 1f;
            float d = Mathf.Sqrt(b.SqrDistance(eye));
            if (d >= p.farM) return 1f;
            if (d <= p.nearM) return p.floor;
            float u = (d - p.nearM) / (p.farM - p.nearM);
            float s = u * u * (3f - 2f * u);
            return p.floor + (1f - p.floor) * s;
        }

        /// <summary>NIGHT63 (the Icebreaker at the eye): a part drawn with a material that has no dissolve is hidden while the eye is within
        /// numbers.bossHideM of its world bounds (inside: 0 m). No (or zero) number: never.</summary>
        public static bool Hide(Params p, Bounds b, Vector3 eye) => p != null && p.bossHideM > 0 && Mathf.Sqrt(b.SqrDistance(eye)) < p.bossHideM;

        /// <summary>NIGHT63 (the Icebreaker at the eye): every frame, over a boss's body parts: a part the eye is at (Hide) stops drawing
        /// (Renderer.forceRenderingOff: its component stays enabled and its bounds live, so it is measured again next frame), a hidden part the eye
        /// has left draws again; with the row off every part it hid draws again and the set empties (a same-run toggle leaves nothing behind).
        /// A part its own view turned off (.enabled false) is never marked: the helper owns only what it hid. `hidden` is the caller's set.</summary>
        public static void HideNear(List<Renderer> parts, HashSet<Renderer> hidden, bool on, Params p, Vector3 eye)
        {
            if (hidden == null) return;
            if (!on || p == null || p.bossHideM <= 0 || parts == null)
            {
                foreach (Renderer r in hidden) if (r != null) r.forceRenderingOff = false;
                hidden.Clear();
                return;
            }
            for (int i = 0; i < parts.Count; i++)
            {
                Renderer r = parts[i];
                if (r == null) continue;
                bool mine = hidden.Contains(r);
                if (!mine && !r.enabled) continue; // its view's own off: never ours
                bool near = Hide(p, r.bounds, eye);
                if (near && !mine) { r.forceRenderingOff = true; hidden.Add(r); }
                else if (!near && mine) { r.forceRenderingOff = false; hidden.Remove(r); }
            }
        }
    }
}
