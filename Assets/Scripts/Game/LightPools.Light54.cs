// NIGHT54 builder L, light54 fix 1 (design.n54.light54, view only; Light54.cs). The strobe: effects.js's one light is the muzzle flash AND every pop
// (LightPools.cs UpdateFlash: "a pop borrows the light while no muzzle flash needs it"), and port-only every world impact pops it
// (UnityTuning.ImpactPops: fx.popIntensity 60 at fx.popDistance 9 m for fx.popTime 0.12 s, k^2). Holding fire on a wall, the one light jumped each
// shot from the muzzle (fx.flashLightIntensity 6, 7 m, 40 ms) to the impact (10x brighter) and back: a full-range strobe at the gun's fire rate.
// With the row on:
//   - the pops have their own light (made here; never the muzzle flash's), the flash light is the muzzle's only;
//   - a kill or a blast (the web's pops) sets the pop light's kill share to the web's peak (fx.popIntensity) and it fades the web's way (k^2 over
//     fx.popTime): one readable pop, as the web;
//   - a world impact ADDS numbers.impactAdd to a glow capped at numbers.impactCap that decays with numbers.impactTauS: held fire is a steady glow
//     at the impacts that dies away when the gun stops, never a re-trigger from zero per shot; the light eases to the newest impact
//     (numbers.popPosEaseS) while it is lit and snaps only while dark;
//   - the muzzle flash, when its window ends, falls on a numbers.muzzleTailS k^2 tail instead of a cut.
// Off: LightPools.cs's rule exactly (the pops borrow the flash light).
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class LightPools
    {
        Light pop54;
        double kill54, hit54, tail54;
        Vector3 pop54At, hit54At;
        Color pop54Color = Color.white, hit54Color = Color.white;
        bool pop54Lit;

        /// <summary>For the a154 reel: impacts and kills the pop light took (row on), and frames the flash light was given a pop (either mode).</summary>
        public static int Pops54, Impacts54, FlashPops54;

        bool L54 => Light54.On(game.cfg);

        void Register54()
        {
            Light54.Transient.RemoveWhere(l => l == null);
            Light54.Transient.Add(flash);
            Light54.Transient.Add(guide);
            foreach (Slot s in slots) Light54.Transient.Add(s.light);
        }

        Light Pop54Light()
        {
            if (pop54 != null) return pop54;
            pop54 = MakeLight(flashT.parent, "Pop light 54 (light54: the pops' own light)");
            Light54.Transient.Add(pop54);
            return pop54;
        }

        /// <summary>A kill's or a blast's pop with the row on: the web's peak on the pop light, never the flash light.</summary>
        bool Pop54(Vector3 at, Color color)
        {
            if (!L54) return false;
            Pop54Light();
            kill54 = popTimeMax; // the web's k^2 fade over fx.popTime
            pop54At = at;
            pop54Color = color;
            Pops54++;
            return true;
        }

        /// <summary>A world impact (port-only): with the row on it adds to the glow; off, the old pop on the flash light.</summary>
        void PopImpact(Vector3 at, Color color)
        {
            if (!L54)
            {
                Pop(at, color);
                return;
            }
            Pop54Light();
            GameConfig cfg = game.cfg;
            double cap = Light54.Num(cfg, "impactCap", 16);
            double was = hit54;
            hit54 = System.Math.Min(cap, hit54 + Light54.Num(cfg, "impactAdd", 6));
            if (was < cap * 0.1) hit54At = at; // dark: the glow starts where the shot landed
            else hit54At = Vector3.Lerp(hit54At, at, (float)Light54.Num(cfg, "impactShift", 0.5)); // lit: the new impact pulls it, never a jump
            hit54Color = color;
            Impacts54++;
        }

        bool tailLive54;

        /// <summary>The muzzle flash's window just ended (row on): its tail starts at the window's last share this same frame (no one-frame dip
        /// to 0: the PutFlash(0) before it is overwritten; PutFlash's last value is the frame's).</summary>
        void FlashEnded54()
        {
            if (!L54) return;
            tail54 = Light54.Num(game.cfg, "muzzleTailS", 0.06);
            tailLive54 = tail54 > 0;
            if (tailLive54) PutFlash(flashIntensity * flashSecond);
        }

        /// <summary>The muzzle tail this frame (no flash lit): the flash light's intensity (0 on the frame it ends) or a negative (no tail: always
        /// with the row off).</summary>
        double Tail54(double dt)
        {
            if (!tailLive54) return -1;
            if (!L54) { tailLive54 = false; tail54 = 0; return 0; }
            double s = Light54.Num(game.cfg, "muzzleTailS", 0.06);
            tail54 = System.Math.Max(0, tail54 - dt);
            if (tail54 <= 0) tailLive54 = false;
            double k = s > 0 ? tail54 / s : 0;
            return flashIntensity * flashSecond * k * k;
        }

        /// <summary>NIGHT71 L4 (wash71): the last pop was a kill's (a blast's: false), read by Step54 for wash71's frame clock.</summary>
        bool popKill71;

        /// <summary>The pop light each frame (row on): the kill share (web curve) plus the impact glow, at the stronger one's place, eased.
        /// NIGHT71 L4: with design.n71.wash71 live, a KILL's share fades on the frame's dt (`frameDt`, not held by a freeze) at popK x its peak
        /// (Wash71.PopStep); otherwise exactly as before.</summary>
        void Step54(double dt, double frameDt)
        {
            if (pop54 == null) return;
            if (!L54)
            {
                if (pop54Lit) { Set(pop54, 0); pop54Lit = false; }
                kill54 = hit54 = 0;
                return;
            }
            GameConfig cfg = game.cfg;
            double tau = Light54.Num(cfg, "impactTauS", 0.25);
            hit54 *= tau > 0 ? System.Math.Exp(-dt / tau) : 0;
            if (hit54 < 0.02) hit54 = 0;
            double kk = popTimeMax > 0 ? kill54 / popTimeMax : 0;
            double killPeak = popIntensity, killDt = dt; // NIGHT71 L4 wash71: as before with it off
            if (kill54 > 0 && popKill71 && Wash71.Live(cfg))
            {
                killDt = Wash71.PopStep(cfg, true, dt, frameDt, popIntensity, out killPeak);
                if (kill54 >= popTimeMax) Wash71.KillPopsClocked++; // the reel's probe: counted on the kill pop's first frame
            }
            double killI = killPeak * kk * kk;
            kill54 = System.Math.Max(0, kill54 - killDt);
            double total = killI + hit54;
            if (total <= 0)
            {
                if (pop54Lit) { Set(pop54, 0); pop54Lit = false; }
                return;
            }
            Vector3 want = killI >= hit54 ? pop54At : hit54At;
            Color col = killI >= hit54 ? pop54Color : hit54Color;
            Transform t = pop54.transform;
            if (!pop54Lit) { t.position = want; pop54.color = col; }
            else
            {
                float e = Light54.Ease((float)dt, Light54.F(cfg, "popPosEaseS", 0.05));
                t.position = Vector3.Lerp(t.position, want, e);
                pop54.color = Color.Lerp(pop54.color, col, e);
            }
            pop54.range = (float)popDistance;
            Set(pop54, total);
            pop54Lit = true;
        }

        /// <summary>The run's clear (NewRun) for the pop light.</summary>
        void Clear54()
        {
            kill54 = hit54 = tail54 = 0;
            tailLive54 = false;
            if (pop54 != null) Set(pop54, 0);
            pop54Lit = false;
        }
    }
}
