// PG3 tripo74 (GG 2026-10-28, the AME 294 PG3 assignment; the view row design.n74.tripo74; TripoModels.cs has the rule): an enemy body's Tripo model. Look43.Apply calls TripoSync every frame
// for every pooled body of the husk (the tank too: its root's scale carries the tank's size), swarmer, spitter, charger, drone and warden, by the kind's name (Resources/Tripo/<kind>.fbx).
// On its first frame with the row on a body gets its model: a child of the body's root (so it follows the move, the yaw, the lean, the squash, the flinch and the pulse), fitted to the kind's
// height (the config's: the hit box's), its feet on the root's origin, facing the root's +Z; the view's own pill renderers (`rends`) then do not draw, nor do the extra parts of NIGHT40's and
// models43's sets, except the cue parts (UnityTuning.TripoKeepPills / TripoCueRole): the wind-up, parry and weak-point glows stay. The drone is fitted by its diameter (2 x drone.radius) with
// its middle on the root's origin (its root is its centre). The row off (any frame): the model hides and every pill part draws again. The hit flash: EnemyLook.Tint hands the pill's flash to
// the model (TripoModels.EnemyTint). Not carried over: the wind-up / stagger tint of the pill body (its colour), the X-RAY silhouette (it is a pass of the pill's material). The death break
// (EnemyLook.Die) still breaks the pill's blocks: the model leaves with the root on the kill frame. A missing file: one cached miss, then an int compare per frame. View only.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Look43
    {
        TripoSlot tripo; // made on the first call (every kind asks; the slot knows its file is missing)
        bool tripoHid40, tripoHid43; // the NIGHT40 / models43 part sets' body pieces are handed to the slot

        /// <summary>The frame's Tripo model for this body (see the file's header): attach once, then show or hide with the row. `m40`: NIGHT40's part set (may be null).</summary>
        void TripoSync(Enemy40Parts m40)
        {
            tripo ??= new TripoSlot(kind);
            if (tripo.Missing) return;
            GameConfig cfg = look.Cfg;
            if (tripo.NeedsAttach(cfg))
            {
                bool drone = kind == "drone";
                float size = drone ? 2f * (float)cfg.Num("drone.radius") : height;
                if (tripo.Attach(root, drone ? TripoModels.Fit.Widest : TripoModels.Fit.Height, size, Quaternion.Euler(0, UnityTuning.TripoYawDeg(kind), 0), false, drone, -1, true) != null)
                {
                    int keep = UnityTuning.TripoKeepPills(kind);
                    for (int i = 0; i < anchors.Length; i++)
                    {
                        Renderer r = anchors[i].GetComponent<Renderer>();
                        TripoModels.RegisterFlash(r, tripo);
                        if ((keep >> i & 1) == 0) tripo.Hide(r);
                    }
                }
            }
            if (tripo.Model == null) return;
            if (m40 != null && !tripoHid40) { tripoHid40 = true; m40.TripoHide(tripo); }
            if (extras != null && !tripoHid43) { tripoHid43 = true; extras.TripoHide(tripo); }
            tripo.Show(cfg);
            TripoMotion();
        }

        /// <summary>PG3 (GG 2026-10-28): set by the view before Apply: this body is winding up an attack (a husk / swarmer / spitter's windupSteps, a spitter's tell). The other kinds leave it false.</summary>
        public bool tripoAttack;
        Vector3 tripoLast;
        bool tripoHasLast, tripoWasAttack;
        float tripoSpeed, tripoPhase, tripoWind, tripoStrike, tripoClock;

        /// <summary>The model's walk and attack, on its holder (the fit and the root's own move, turn, squash and flinch are untouched): a step bob, a waddle and a forward lean that grow
        /// with the root's measured ground speed; the wind-up rears back, the release lunges forward and settles; the drone hovers and banks. UnityTuning.Tripo* has the sizes.</summary>
        void TripoMotion()
        {
            if (!tripo.Shown) { tripoHasLast = false; return; }
            float dt = Time.deltaTime;
            if (!(dt > 0f)) return;
            Vector3 p = root.position;
            float sp = 0f;
            if (tripoHasLast) { Vector3 d = p - tripoLast; d.y = 0f; sp = d.magnitude / dt; if (sp > UnityTuning.TripoTeleportSpeed) sp = 0f; }
            tripoLast = p;
            tripoHasLast = true;
            tripoSpeed = Mathf.Lerp(tripoSpeed, sp, 1f - Mathf.Exp(-dt * 10f));
            float walk = Mathf.Clamp01(tripoSpeed / UnityTuning.TripoWalkFullSpeed);
            tripoPhase += dt * tripoSpeed / UnityTuning.TripoStrideM * Mathf.PI;
            tripoClock += dt;
            if (tripoWasAttack && !tripoAttack) tripoStrike = 1f; // the release
            tripoWasAttack = tripoAttack;
            tripoWind = Mathf.MoveTowards(tripoWind, tripoAttack ? 1f : 0f, dt * (tripoAttack ? UnityTuning.TripoWindupRate : 4f * UnityTuning.TripoWindupRate));
            tripoStrike = Mathf.Max(0f, tripoStrike - dt * UnityTuning.TripoStrikeDecay);
            float strike = tripoStrike * tripoStrike;
            Transform h = tripo.Model.transform;
            if (kind == "drone")
            {
                float hover = Mathf.Sin(tripoClock * UnityTuning.TripoHoverHz * 2f * Mathf.PI) * UnityTuning.TripoHoverShare * height;
                h.localPosition = new Vector3(0f, hover, 0f);
                h.localRotation = Quaternion.Euler(UnityTuning.TripoDroneBankDeg * walk, 0f, Mathf.Sin(tripoClock * 1.3f) * 3f);
                return;
            }
            float bob = Mathf.Abs(Mathf.Sin(tripoPhase)) * UnityTuning.TripoBobShare * height * walk;
            float roll = Mathf.Sin(tripoPhase) * UnityTuning.TripoWaddleDeg * walk;
            float pitch = UnityTuning.TripoLeanDeg * walk - UnityTuning.TripoWindupBackDeg * tripoWind + UnityTuning.TripoStrikeDeg * strike;
            h.localPosition = new Vector3(0f, bob, UnityTuning.TripoStrikeReach * height * strike);
            h.localRotation = Quaternion.Euler(pitch, 0f, roll);
        }
    }

    public sealed partial class Enemy40Parts
    {
        /// <summary>PG3 tripo74: hand `slot` this set's body pieces (every part but the cue roles: UnityTuning.TripoCueRole), which it hides while its model draws.</summary>
        public void TripoHide(TripoSlot slot)
        {
            for (int i = 0; i < parts.Length; i++)
                if (!UnityTuning.TripoCueRole(role[i])) slot.Hide(rends[i]);
        }
    }
}
