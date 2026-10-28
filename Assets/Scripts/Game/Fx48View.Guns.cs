// NIGHT48 builder F (design.n48.guns48, a view row over fx47; NIGHT48.md G24: "every weapon heavier ... no gun lighter than today"). The
// n48-r1 a748 sheets (Logs/player/n48-reel-n48-r1-f-weapons.log), N47 = today's game (fx47 on), read per action against F-cards 1's ladder:
//   - the FIST (VERBS 24) had NO camera read at all: 0.00 deg on every frame in all three passes (F-cards 1a's light-medium tier: 1.2 deg);
//   - no gun has an FOV punch on a plain shot: the THUNDERCLAP's shot (the heavy tier: +2-3 deg) and the KESTREL's tap read 0.0 deg of field of
//     view on every frame (the charged shots already punch +4-7 deg: kept);
//   - every other action already sits in its band or above it (the KESTREL charge 7.0 deg, the slug 5.6, the boost 6.6: heavier than the
//     NIGHT47 cards' bands; D-14: none is turned down).
// With guns48 on (all on frame time, after the camera, x EFFECTS INTENSITY, none under REDUCED MOTION):
//   - the fist's punch nods the view Guns48FistNodDeg (down, as the gauntlet swing's nod) over Guns48FistRiseS, back over Guns48FistSettleS;
//   - the THUNDERCLAP's plain shot punches the field of view out Guns48ShotFovDeg (out in Guns48FovOutS, back over Guns48FovBackS), the
//     KESTREL's tap Guns48TapFovDeg (the light-medium tier). The guns' own kicks, the charge, the boost and the blast are unchanged.
// Off: nothing. Numbers: UnityTuning.F48.cs (Guns48*).
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Fx47View
    {
        struct Fov48
        {
            public float age, deg, outS, backS;
        }

        readonly Fov48[] fov48 = new Fov48[4];
        bool hooked48;

        /// <summary>Hooks guns48's events once (from Hook: the first frame fx47 is on).</summary>
        void HookGuns48()
        {
            if (hooked48 || game.events == null) return;
            hooked48 = true;
            for (int i = 0; i < fov48.Length; i++) fov48[i].age = -1;
            game.events.On<MeleeEvent>("punch", e => { if (On && e.owner == "player" && Guns48.On(game.cfg)) AddKick(UnityTuning.Guns48FistNodDeg, 0, UnityTuning.Guns48FistRiseS, UnityTuning.Guns48FistSettleS); });
            game.events.On("fire", o =>
            {
                if (!On || !Guns48.On(game.cfg)) return;
                string w = o as string;
                if (w == "shotgun") AddFov48(UnityTuning.Guns48ShotFovDeg);
                else if (w == "revolver") AddFov48(UnityTuning.Guns48TapFovDeg);
            });
            game.OnNewRun += () => { for (int i = 0; i < fov48.Length; i++) fov48[i].age = -1; };
        }

        void AddFov48(float deg)
        {
            if (Calm()) return;
            for (int i = 0; i < fov48.Length; i++)
                if (fov48[i].age < 0) { fov48[i] = new Fov48 { age = 0, deg = deg, outS = UnityTuning.Guns48FovOutS, backS = UnityTuning.Guns48FovBackS }; return; }
        }

        /// <summary>The FOV punches added to this frame's field of view (after CameraFeel and the viewmodel set it: they never accumulate).</summary>
        float Fov48Render(Camera cam, float dt)
        {
            if (cam == null) return 0;
            float sum = 0;
            for (int i = 0; i < fov48.Length; i++)
            {
                if (fov48[i].age < 0) continue;
                float t = fov48[i].age;
                fov48[i].age += dt;
                if (t > fov48[i].outS + fov48[i].backS) { fov48[i].age = -1; continue; }
                float k = t < fov48[i].outS ? t / fov48[i].outS : 1f - (t - fov48[i].outS) / fov48[i].backS;
                sum += fov48[i].deg * k * k * (3f - 2f * k);
            }
            if (sum <= 0) return 0;
            float ei = (float)System.Math.Max(0, System.Math.Min(1, game.cfg.movement.settings.effectsIntensity));
            cam.fieldOfView += sum * ei;
            return sum * ei;
        }
    }
}
