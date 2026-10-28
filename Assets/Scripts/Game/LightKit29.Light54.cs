// NIGHT54 builder L, light54 fixes 2 and 3 (design.n54.light54, view only; Light54.cs). Read in code (LightKit29.cs Update, LightKit29.Lava47.cs
// StepSlots47, LightKit29.L49.cs Step49):
//   - fix 2: every 0.25 s (UnityTuning.LightKitRepick) each borrowed slot re-picks the nearest fixture, so at a walk a slot swapped between two
//     fixtures at nearly the same distance again and again; each move faded the slot to black where it was (Lava47SlotFadeS 0.18 s), moved it, faded
//     it in: a dark dip in the room's light every few re-picks. And the "taken" test read the slots' CURRENT positions, so while a slot was still
//     fading out at its old fixture the next slot could take the first one's new fixture and both chased each other. With the row on: a slot keeps
//     its fixture unless the new best is under numbers.slotHysteresis of the old one's distance; "taken" reads the fixture each slot WANTS; and a
//     move cross-fades: the old light hands its state to a twin that fades out where it stands while the slot fades in at its new fixture over
//     numbers.slotFadeS (smoothstep), so the room's light never dips.
//   - fix 3: the landmark light (lights49) set its position and intensity from a fresh read every 0.25 s with no easing, and the read counted the
//     muzzle flash and the pops (a shot near the landmark threw it to a new strength for a quarter second). With the row on the read skips
//     Light54.Transient and the light eases to the read over numbers.landmarkEaseS.
// Off: NIGHT53's code paths exactly.
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class LightKit29
    {
        Light[] twin54;
        float[] twinI54, twinFade54;
        bool l54;
        bool land54Want;
        Vector3 land54Pos;
        float land54I;

        /// <summary>For the a154 reel: slot moves this level (either mode) and moves the hysteresis held (row on).</summary>
        public static int Moves54, Held54;

        bool L54Now() => game != null && Light54.On(game.cfg);

        /// <summary>The fixture slot q wants this re-pick (lava47: want47; else lightFix), or -1.</summary>
        int Wanted54(int q)
        {
            if (lava47) return want47 != null && q < want47.Length ? want47[q] : -1;
            return lightFix != null && q < lightFix.Length ? lightFix[q] : -1;
        }

        /// <summary>Row on: fixture i is taken by an earlier slot's WANTED fixture (same index or the same spot).</summary>
        bool Taken54(int q, int i)
        {
            int w = Wanted54(q);
            return w >= 0 && w < fixtures.Count && (w == i || (fixtures[w].at - fixtures[i].at).sqrMagnitude < 0.01f);
        }

        int Pref54(int li, Fixture f) => lavaSlot33 && li == lights.Length - 1 ? (f.kind == "lava" ? 1 : 0) : li == 0 && f.kind == "pool" ? 1 : li == 1 && f.accent ? 1 : 0;

        /// <summary>Row on: the slot's current fixture stays unless `best` is clearly closer (numbers.slotHysteresis of its distance).</summary>
        int Keep54(int li, int best, int bestPref, float bestD, string room, Vector3 p, Vector3 exit)
        {
            int cur = Wanted54(li);
            if (cur < 0 || cur == best || cur >= fixtures.Count) return best;
            Fixture cf = fixtures[cur];
            if (room != null && cf.room != room) return best;
            if (lava47 && cf.kind == "lava") return best;
            for (int q = 0; q < li; q++) if (Taken54(q, cur)) return best;
            int pref = Pref54(li, cf);
            if (pref != bestPref) return best;
            float curD = (cf.at - (li == 0 && pref == 1 ? exit : p)).sqrMagnitude;
            float h = Light54.F(game.cfg, "slotHysteresis", 0.65);
            if (bestD < curD * h * h) return best;
            Held54++;
            return cur;
        }

        /// <summary>Row on, a slot about to move: its light's state goes to the slot's twin, which fades out where it stands.</summary>
        void Twin54(int li, Light l)
        {
            Moves54++;
            if (twin54 == null || twin54.Length != lights.Length)
            {
                twin54 = new Light[lights.Length];
                twinI54 = new float[lights.Length];
                twinFade54 = new float[lights.Length];
            }
            if (l.intensity <= 1e-4f) return; // dark: nothing to hand over
            Light t = twin54[li];
            if (t == null)
            {
                var go = new GameObject("Fixture light twin 54 " + li);
                go.transform.SetParent(transform, false);
                t = go.AddComponent<Light>();
                t.shadows = LightShadows.None;
                twin54[li] = t;
            }
            t.type = l.type;
            t.transform.SetPositionAndRotation(l.transform.position, l.transform.rotation);
            t.spotAngle = l.spotAngle;
            t.innerSpotAngle = l.innerSpotAngle;
            t.color = l.color;
            t.range = l.range;
            twinI54[li] = l.intensity;
            twinFade54[li] = 1f;
            t.intensity = l.intensity;
            t.enabled = true;
        }

        /// <summary>Row on, every frame: the twins fade out (smoothstep over numbers.slotFadeS).</summary>
        void StepTwins54(float dt)
        {
            if (twin54 == null) return;
            float rate = dt / Mathf.Max(0.01f, Light54.F(game.cfg, "slotFadeS", 0.45));
            for (int i = 0; i < twin54.Length; i++)
            {
                Light t = twin54[i];
                if (t == null || !t.enabled) continue;
                twinFade54[i] = Mathf.Max(0f, twinFade54[i] - rate);
                t.intensity = twinI54[i] * Mathf.SmoothStep(0f, 1f, twinFade54[i]);
                if (twinFade54[i] <= 0f) { t.intensity = 0f; t.enabled = false; }
            }
        }

        /// <summary>Row off (or flipped off by the reel): the twins dark.</summary>
        void DarkTwins54()
        {
            if (twin54 == null) return;
            foreach (Light t in twin54) if (t != null && t.enabled) { t.intensity = 0f; t.enabled = false; }
        }

        /// <summary>From Step49 with the row on: the landmark's read becomes a target (eased every frame by StepLand54).</summary>
        bool Land54(Vector3 pos, float intensity)
        {
            if (!l54) { land54Want = false; return false; }
            if (!land54Want) { landmark49.transform.position = pos; landmark49.intensity = intensity; } // the first read: no ease from nothing
            land54Want = true;
            land54Pos = pos;
            land54I = intensity;
            return true;
        }

        /// <summary>Every frame with the row on: the landmark eases to its last read.</summary>
        void StepLand54(float dt)
        {
            if (!l54 || !land54Want || landmark49 == null) return;
            float e = Light54.Ease(dt, Light54.F(game.cfg, "landmarkEaseS", 0.6));
            landmark49.transform.position = Vector3.Lerp(landmark49.transform.position, land54Pos, e);
            landmark49.intensity = Mathf.Lerp(landmark49.intensity, land54I, e);
        }

        /// <summary>From Update, every frame before the re-pick clock.</summary>
        void Step54Kit()
        {
            bool now = L54Now();
            if (l54 && !now) DarkTwins54();
            l54 = now;
            float dt = FeelClock.UnscaledDelta; // NIGHT63 D1: 0 in a same-run held frame
            if (l54) { StepTwins54(dt); StepLand54(dt); }
        }
    }
}
