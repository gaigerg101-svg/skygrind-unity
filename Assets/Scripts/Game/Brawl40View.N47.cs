// NIGHT47 builder B, brawlLook47 (view row; G28-G30; Research/2026-10-18-wave47/B-cards.md section 3): THE ICEBREAKER's view, first attempt.
// Visual only (the view reads the sim's state; no hit box or timing changes). One call from Brawl40View.Render (after Render42), only while
// the row is on:
//   G29 fluid: the torso's lean, the head's pitch and the knee drop follow their per-state targets on damped springs (overshoot ~15 %,
//       settle <= 0.3 s) instead of snapping; every hit taken kicks the lean back (a 60-100 ms flinch); an idle that breathes.
//   G28 / palette: STUCK and the WINDOW read YELLOW (the game's "punch it", the WICK's crack and the CHARGER's vents) instead of cyan (cyan is
//       the player's); THE ICE ARMOUR (brawl47) shows as frost-white rims, and its breaking throws an ice burst.
//   G30 damage shows: the armour sheds a plate at 80 / 60 / 40 / 20 % of the bar (an ice burst at the chest), so the body tells the bar.
// Numbers: UnityTuning.Brawl47.cs (Brawl47*).
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Brawl40View
    {
        float s47Lean = 18, s47LeanV, s47Head, s47HeadV, s47Drop, s47DropV, s47LastFrac = 1;
        int s47Shed;
        bool s47Wired, s47Armored;
        Transform[] s47Plates;
        readonly Renderer[] s47PlateR = new Renderer[4];

        void Render47(Brawl40 g, Brawl40State st, ref float leanDeg, ref float headDeg, ref float drop, ref Color cyanC, ref Color rimC, float dt)
        {
            if (!NightRows.ViewOn(game.cfg, "brawlLook47")) return;
            if (!s47Wired)
            {
                s47Wired = true;
                game.events.On<BossEvent>("bossHit", e => { if (e.boss == "brawl40") s47LeanV -= UnityTuning.Brawl47FlinchKick; });
            }
            // the punch-it colour for STUCK / WINDOW
            cyanC = BossColours63.Part(game.cfg, "brawl.weak", Look.Hex(UnityTuning.Brawl40YellowHex, UnityTuning.Brawl47YellowGain)); // NIGHT63 B3: the weak point's cyan again (the block keeps the parry's yellow), with bossColours63
            // the armour (the lead's r1e note: opaque frost-white plates ON the body that shed): four plates on the torso (chest left / right,
            // shoulders left / right) shown while THE ICE ARMOUR is on, thrown off in an ice burst when it breaks, back at the next phase's armour
            if (s47Plates == null)
            {
                s47Plates = new Transform[4];
                // r2 note: the stone material is lit by the dusk and read dark: the plates use the view's glow material, tinted frost white
                for (int i = 0; i < 4; i++)
                {
                    var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Object.Destroy(go.GetComponent<Collider>());
                    go.name = "frost plate " + i;
                    go.transform.SetParent(lean, false);
                    go.transform.localPosition = UnityTuning.Brawl47PlatePos[i];
                    go.transform.localScale = UnityTuning.Brawl47PlateSize[i];
                    go.transform.localRotation = Quaternion.Euler(UnityTuning.Brawl47PlateTilt[i]);
                    go.GetComponent<Renderer>().sharedMaterial = add;
                    s47PlateR[i] = go.GetComponent<Renderer>();
                    go.SetActive(false);
                    s47Plates[i] = go.transform;
                }
            }
            for (int i = 0; i < 4; i++)
            {
                if (s47Plates[i].gameObject.activeSelf != g.Armored47) s47Plates[i].gameObject.SetActive(g.Armored47);
                if (g.Armored47) Tint(s47PlateR[i], Look.Hex(UnityTuning.Brawl47FrostHex, UnityTuning.Brawl47PlateGain));
            }
            if (s47Armored && !g.Armored47) game.ParticleBurst(g.Centre, Look.Hex(UnityTuning.Brawl47FrostHex, 2f), UnityTuning.Brawl47BurstBits, UnityTuning.Brawl47BurstSpeed, UnityTuning.Brawl47BurstLife, UnityTuning.Brawl47BurstSize);
            s47Armored = g.Armored47;
            // the plates shed at 80 / 60 / 40 / 20 % (a new fight starts over)
            float frac = (float)g.HpFrac;
            if (frac > s47LastFrac + 0.05f) s47Shed = 0;
            s47LastFrac = frac;
            int want = frac <= 0.2f ? 4 : frac <= 0.4f ? 3 : frac <= 0.6f ? 2 : frac <= 0.8f ? 1 : 0;
            while (s47Shed < want)
            {
                s47Shed++;
                game.ParticleBurst(g.Centre, Look.Hex(UnityTuning.Brawl47FrostHex, 1.6f), UnityTuning.Brawl47ShedBits, UnityTuning.Brawl47BurstSpeed, UnityTuning.Brawl47BurstLife, UnityTuning.Brawl47ShedSize);
            }
            // the breath in the idle
            if (st == Brawl40State.Idle) leanDeg += Mathf.Sin(Time.time * UnityTuning.Brawl47BreathRate) * UnityTuning.Brawl47BreathDeg;
            // the springs (semi-implicit, sub-stepped so a slow frame stays stable)
            float h = Mathf.Min(dt, 0.1f);
            int n = Mathf.Max(1, Mathf.CeilToInt(h / 0.008f));
            float sh = h / n, w = UnityTuning.Brawl47SpringW, z = UnityTuning.Brawl47SpringZeta;
            for (int i = 0; i < n; i++)
            {
                s47LeanV += (w * w * (leanDeg - s47Lean) - 2 * z * w * s47LeanV) * sh; s47Lean += s47LeanV * sh;
                s47HeadV += (w * w * (headDeg - s47Head) - 2 * z * w * s47HeadV) * sh; s47Head += s47HeadV * sh;
                s47DropV += (w * w * (drop - s47Drop) - 2 * z * w * s47DropV) * sh; s47Drop += s47DropV * sh;
            }
            leanDeg = s47Lean;
            headDeg = s47Head;
            drop = s47Drop;
        }
    }
}
