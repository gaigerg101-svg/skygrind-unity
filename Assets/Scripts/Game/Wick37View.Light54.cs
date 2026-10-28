// NIGHT54 builder L, light54 fix 4 (design.n54.light54, view only; Light54.cs): THE WICK's arena, the first boss (GG 2026-10-20: "The first boss
// fight in particular has far too many lighting issues"; the lead on e53-freeze2's a147 frames, 14:45: P1 / P2 overexposed, the shaft walls and
// floor flat cream near white, the boss blending into the walls, f0185 a blown-out white bloom wash). Read in code (Wick37View.cs BuildStack,
// Wick37View.N43.cs StackLights): inside a shaft of radius design.n37.wick37 stack.radius the view stands four "stack light"s at 6 / 18 / 30 / 43 m,
// each Stack37LightIntensity 9 at Stack37LightRange 26 m (12 m apart: every wall point inside the range of two or three, warm white 0xffd2a8 with
// wick43), the slag's light 24 at 16 m and the core's 30 at 14 m, on top of the room's own fixture slots, the landmark and the lava lights. And the
// tells pulse at Tell36PulseHz 7 Hz (+-25 %) through their last 30 % (FlashGain: the core, the flare band, its curtain, the slam decal), the core
// light's colour snaps between warm / tell / cyan / yellow each state change. With the row on:
//   - the stack lights x numbers.wickStackGain at numbers.wickStackRange m, the slag's x wickSlagGain at wickSlagRange, the core's x wickCoreGain at
//     wickCoreRange (the room keeps its own lights: a neutral, exposed base for builder G's grade, not a colour);
//   - the bloom in the arena (the camera inside the shaft's radius + numbers.wickBloomPadM) x numbers.wickBloom, eased (Bloom54, read by PostFx);
//   - a tell's last 30 % brightens once to x (1 + numbers.tellRamp) on a smoothstep instead of the 7 Hz pulse (its colour and its timing unchanged);
//   - the core light's colour eases over numbers.wickCoreColorEaseS (the emissive core keeps its snap: the tell reads at once on the body).
// Off: NIGHT53's arena exactly.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Wick37View
    {
        static bool calm54;
        bool l54W;
        Color core54;
        bool core54Set;
        float bloom54 = 1f;

        /// <summary>PostFx's bloom x this (1 with the row off or outside THE STACK).</summary>
        public static float Bloom54 = 1f;

        /// <summary>For the a154 reel: the camera inside THE STACK this frame (row on or off).</summary>
        public static bool InStack54;

        /// <summary>FlashGain's last-30 % cue with the row on: one smoothstep brighten (no pulse).</summary>
        static float Ramp54(double progress)
        {
            float u = Mathf.Clamp01((float)((progress - 0.7) / 0.3));
            return 1f + Light54.F(SurvivalGame.Instance != null ? SurvivalGame.Instance.cfg : null, "tellRamp", 0.25) * u * u * (3f - 2f * u);
        }

        /// <summary>The stack lights' share (StackLights multiplies its intensity by this).</summary>
        float Stack54Gain() => l54W ? Light54.F(game.cfg, "wickStackGain", 0.3) : 1f;

        /// <summary>The slag light's share (Look47's finale flare multiplies by this too).</summary>
        float Slag54Gain() => l54W ? Light54.F(game.cfg, "wickSlagGain", 0.45) : 1f;

        /// <summary>From the end of Render (every path): the arena's lights, the bloom trim, the switch for FlashGain.</summary>
        void Light54Frame(SurvivalSim sim)
        {
            Frame55(sim); // NIGHT55 B grade55 (Wick37View.N55.cs): the cold seams, the heat patch, the stack lights' palette (nothing with the row off)
            bool on = Light54.On(game.cfg);
            bool was = l54W;
            l54W = on;
            calm54 = on;
            float dt = FeelClock.UnscaledDelta; // NIGHT63 D1: 0 in a same-run held frame
            bool inStack = false;
            if (sim != null && sim.wick37Centre != null && sim.wick37Stack != null && game.cam != null)
            {
                Vector3 c = U(sim.wick37Centre.Value), p = game.cam.transform.position;
                float dx = p.x - c.x, dz = p.z - c.z;
                float pad = on ? Light54.F(game.cfg, "wickBloomPadM", 2) : 2f;
                float r = (float)sim.wick37Stack.p.radius + pad;
                inStack = dx * dx + dz * dz < r * r;
            }
            InStack54 = inStack;
            if (built && albedo54Built != on) built = false; // the albedo rule flipped (the reel's passes): the stack and the body rebuild next frame
            if (!on)
            {
                if (was) Restore54();
                Bloom54 = bloom54 = 1f;
                core54Set = false;
                return;
            }
            float want = inStack ? Light54.F(game.cfg, "wickBloom", 0.6) : 1f;
            bloom54 = Mathf.Lerp(bloom54, want, Light54.Ease(dt, Light54.F(game.cfg, "wickBloomEaseS", 0.4)));
            Bloom54 = bloom54;
            float stackRange = Light54.F(game.cfg, "wickStackRange", 15);
            foreach (Light l in stackLights43) if (l != null) l.range = stackRange;
            if (slagLight != null)
            {
                slagLight.range = Light54.F(game.cfg, "wickSlagRange", 11);
                if (finaleLeft47 <= 0) slagLight.intensity = UnityTuning.Stack37SlagLightIntensity * UnityTuning.LightScale * Slag54Gain();
            }
            if (coreLight != null)
            {
                coreLight.range = Light54.F(game.cfg, "wickCoreRange", 10);
                coreLight.intensity = UnityTuning.Wick37CoreLightIntensity * UnityTuning.LightScale * Light54.F(game.cfg, "wickCoreGain", 0.4);
                Color target = coreLight.color; // this frame's colour (Render / Look47Fx wrote it)
                if (!core54Set) { core54 = target; core54Set = true; }
                core54 = Color.Lerp(core54, target, Light54.Ease(dt, Light54.F(game.cfg, "wickCoreColorEaseS", 0.08)));
                coreLight.color = core54;
            }
        }

        // ---- the lead's a154 on n54-r1 (15:12): the lights x0.3 moved THE STACK's mean luminance only 0.668 -> 0.645. Read in code: the stack's
        // surfaces (Opaque: a copy of game.mats.stone with _BaseColor set to Stack37ShaftHex 0x2a1a1c and the rest) draw with Assets/Shaders/Stone.shader,
        // whose albedo is the VERTEX colour (Stone.shader:197/199 "albedo = i.color.rgb * ..."); _BaseColor is not read, and BuildStack's triangles are
        // Color.white: the dark iron shaft, panels, ledges, cap and tunnel all drew WHITE (the cream walls of a147), and so did THE WICK's plates (Mesh43
        // shapes / the cube with no vertex colour: white). With the row on (decided at the stack's build; a flip rebuilds it): the stack's triangles carry
        // a neutral grey per part (numbers.wickShaftGrey ...), a base for builder G's grade, and the plates take URP Lit with _BaseColor
        // numbers.wickIronGrey / wickIronDarkGrey (darker than the walls: the boss stands off them). ----
        bool albedo54Built;
        Material iron37, ironDark37, iron54, ironDark54;

        /// <summary>For the a154 reel: the stack was built with light54's albedo.</summary>
        public bool Albedo54Built => albedo54Built;

        /// <summary>From BuildStack's start: the albedo rule this build uses.</summary>
        void Begin54Build()
        {
            albedo54Built = Light54.On(game.cfg);
            // the lead's grep line (as Light54.StoneOrLit's): the stack's surfaces as vertex greys (not the NIGHT37 hexes: a neutral base for G), the plates on Lit
            Debug.Log($"[light54] stone colour: Wick37View stack intended #{UnityTuning.Stack37ShaftHex:x6} (shaft; panels #{UnityTuning.Stack37PanelHex:x6}) drawn {(albedo54Built ? $"vertex (neutral grey {Light54.F(game.cfg, "wickShaftGrey", 0.42).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)})" : "white (row off: the stone shader's white vertex colour)")}; plates intended #{UnityTuning.Wick37IronHex:x6} drawn {(albedo54Built ? "lit (grey)" : "white (row off)")}");
        }

        /// <summary>A stack triangle's vertex colour (the Stone shader's albedo): white with the row off (NIGHT53), a part's grey on.</summary>
        Color Albedo54(Wick37Part part)
        {
            if (!albedo54Built) return Color.white;
            string key = part == Wick37Part.Shaft ? "wickShaftGrey" : part == Wick37Part.Panel ? "wickPanelGrey"
                : part == Wick37Part.Ledge || part == Wick37Part.Step ? "wickLedgeGrey" : part == Wick37Part.Tunnel ? "wickTunnelGrey" : "wickCapGrey";
            float g = Light54.F(game.cfg, key, 0.4);
            return new Color(g, g, g, 1f);
        }

        /// <summary>From BuildBody after its materials: the plates' pair by this build's rule.</summary>
        void Body54Materials()
        {
            if (iron37 == null) { iron37 = iron; ironDark37 = ironDark; }
            if (albedo54Built)
            {
                if (iron54 == null) iron54 = Lit54("wick37 iron (light54)", Light54.F(game.cfg, "wickIronGrey", 0.2));
                if (ironDark54 == null) ironDark54 = Lit54("wick37 iron dark (light54)", Light54.F(game.cfg, "wickIronDarkGrey", 0.12));
                iron = iron54;
                ironDark = ironDark54;
            }
            else
            {
                iron = iron37;
                ironDark = ironDark37;
            }
        }

        Material Lit54(string name, float g)
        {
            var m = new Material(game.mats.lit) { name = name };
            var c = new Color(g, g, g, 1f);
            if (m.HasProperty(BaseColor)) m.SetColor(BaseColor, c);
            if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", null);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", Light54.F(game.cfg, "wickIronSmooth", 0.25));
            return m;
        }

        /// <summary>The hot seams' shimmer depth and the burn flash with the row on (the a154 STACK's flicker left: the seams' +-45 % ripple and a
        /// burn's x3.5 flash on the whole wall were in the read region).</summary>
        float Shimmer54() => calm54 ? UnityTuning.Stack37ShimmerAmp * Light54.F(game.cfg, "wickSeamShimmer", 0.35) : UnityTuning.Stack37ShimmerAmp;

        float Burn54(float burnFlash)
        {
            float k = burnFlash / UnityTuning.Stack37BurnFlashS;
            return 1 + UnityTuning.Stack37BurnFlashGain * Light54.F(game.cfg, "wickBurnFlash", 0.4) * k * k;
        }

        /// <summary>The row flipped off at runtime (the reel's off pass): NIGHT53's ranges and intensities back.</summary>
        void Restore54()
        {
            foreach (Light l in stackLights43) if (l != null) l.range = UnityTuning.Stack37LightRange;
            if (slagLight != null) { slagLight.range = UnityTuning.Stack37SlagLightRange; slagLight.intensity = UnityTuning.Stack37SlagLightIntensity * UnityTuning.LightScale; }
            if (coreLight != null) { coreLight.range = UnityTuning.Wick37CoreLightRange; coreLight.intensity = UnityTuning.Wick37CoreLightIntensity * UnityTuning.LightScale; }
        }
    }
}
