// NIGHT42 builder B1 (brawl42; GG G15: "a greater or mor epowerful effect when hitting it or any other enemy iwth gaughtlent", the boss most):
// THE ICEBREAKER's own reaction to a gauntlet hit, on F1's `gauntletHit42` (Core/ImpactRules.cs GauntletHit42Event, emitted by the sim only
// with impact42 on; F1's KillFeel42 draws the cyan shockwave ring and the camera kick). Drawn only with brawl42 on this run (sim.n42.brawl42)
// and only for a Boss-class hit on its own body. Needs F1's impact42 commit (a12b43a9) for the event type.
//  - ICE CHIPS: pale-blue blocks burst from the hit point, away from the player, and fall (gravity), shrinking out.
//  - THE CRACK: a white-blue flash plate across its chest and the plates' frost edges flare, for Brawl42CrackSeconds.
//  - THE RECOIL: the body jolts back along the hit (away from the player) and its torso rocks back, easing out.
// Numbers: UnityTuning.Brawl42 (the Brawl42Chip* / Crack* / Recoil* values). View only.
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Brawl40View
    {
        bool hooked42;
        float punchLeft42;
        Vector3 punchDir42;
        Renderer crack42R;
        readonly List<Transform> chipTs42 = new List<Transform>();
        readonly List<Renderer> chipRs42 = new List<Renderer>();
        readonly List<Vector3> chipV42 = new List<Vector3>();
        readonly List<float> chipLife42 = new List<float>();
        Vector3 pendingAt42;
        bool pending42;

        /// <summary>For the reels / frames: gauntlet hits on the boss seen by the view, chips flying now, the crack shown now.</summary>
        public int Hits42;
        public int ChipsLive42;
        public bool CrackShown42;

        void Hook42()
        {
            if (hooked42) return;
            hooked42 = true;
            game.events.On<GauntletHit42Event>(GauntletHit42Event.Name, e =>
            {
                SurvivalSim sim = game.sim;
                Brawl40 g = sim?.brawl40;
                if (sim == null || g == null || !sim.n42.brawl42 || e.cls != GauntletHit42Event.Boss || e.enemy != g.body.id) return;
                pendingAt42 = U(e.position);
                pending42 = true;
                Hits42++;
            });
        }

        void BuildHit42()
        {
            for (int i = 0; i < UnityTuning.Brawl42ChipCount; i++)
            {
                chipRs42.Add(Glow(fxRoot, "ice chip", out Transform t));
                chipTs42.Add(t);
                chipV42.Add(Vector3.zero);
                chipLife42.Add(0);
            }
            crack42R = GlowPart(lean, "chest crack", new Vector3(0, 1.0f, 0.93f), new Vector3(1.9f, 1.2f, 0.03f));
            crack42R.gameObject.SetActive(false);
        }

        void Punch42(Brawl40 g, ref float leanDeg, Vector3 fu)
        {
            float dt = Time.deltaTime;
            if (pending42)
            {
                pending42 = false;
                if (crack42R == null) BuildHit42();
                Vector3 eye = U(game.sim.Eye);
                Vector3 away = pendingAt42 - eye;
                away.y = 0;
                punchDir42 = away.sqrMagnitude > 1e-6f ? away.normalized : bodyRoot.forward * -1f;
                punchLeft42 = UnityTuning.Brawl42CrackSeconds;
                // the chips: a burst from the hit point, away from the player, up and spread
                for (int i = 0; i < chipTs42.Count; i++)
                {
                    float a = (i + 0.5f) / chipTs42.Count * Mathf.PI * 2f;
                    Vector3 side = Vector3.Cross(Vector3.up, punchDir42).normalized;
                    Vector3 v = punchDir42 * UnityTuning.Brawl42ChipSpeed + side * Mathf.Cos(a) * UnityTuning.Brawl42ChipSpread + Vector3.up * (UnityTuning.Brawl42ChipUp + Mathf.Sin(a) * UnityTuning.Brawl42ChipSpread);
                    chipV42[i] = v;
                    chipLife42[i] = UnityTuning.Brawl42ChipLife * (0.7f + 0.3f * ((i * 7) % 5) / 4f);
                    chipTs42[i].position = pendingAt42;
                    chipTs42[i].rotation = Quaternion.Euler(i * 37f, i * 53f, 0);
                    chipTs42[i].gameObject.SetActive(true);
                }
            }
            // the chips fly and fall
            ChipsLive42 = 0;
            Color chipC = Look.Hex(UnityTuning.Brawl41FrostHex, UnityTuning.Brawl42ChipGain);
            for (int i = 0; i < chipTs42.Count; i++)
            {
                if (chipLife42[i] <= 0) { if (chipTs42[i].gameObject.activeSelf) chipTs42[i].gameObject.SetActive(false); continue; }
                chipLife42[i] -= dt;
                chipV42[i] += Vector3.down * UnityTuning.Brawl42ChipGravity * dt;
                chipTs42[i].position += chipV42[i] * dt;
                chipTs42[i].Rotate(400f * dt, 300f * dt, 0);
                float k = Mathf.Clamp01(chipLife42[i] / UnityTuning.Brawl42ChipLife);
                chipTs42[i].localScale = Vector3.one * UnityTuning.Brawl42ChipSize * (0.4f + 0.6f * k);
                Tint(chipRs42[i], chipC * k);
                ChipsLive42++;
            }
            // the crack flash and the recoil, easing out
            CrackShown42 = punchLeft42 > 0;
            if (crack42R != null && crack42R.gameObject.activeSelf != CrackShown42) crack42R.gameObject.SetActive(CrackShown42);
            if (punchLeft42 <= 0) return;
            float e01 = punchLeft42 / UnityTuning.Brawl42CrackSeconds; // 1 at the hit, 0 at the end
            punchLeft42 -= dt;
            Tint(crack42R, Look.Hex(UnityTuning.Brawl41FrostHex, UnityTuning.Brawl42CrackGain * e01));
            foreach (Renderer f in frost41) Tint(f, Look.Hex(UnityTuning.Brawl41FrostHex, UnityTuning.Brawl42CrackGain * e01));
            float ease = e01 * e01;
            bodyRoot.position += punchDir42 * UnityTuning.Brawl42RecoilMetres * ease;
            leanDeg -= UnityTuning.Brawl42RecoilDeg * ease;
        }
    }
}
