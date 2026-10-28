// NIGHT18 (the shared commit, a pure move from EnemyViews.cs): EnemyLook's death members (Die: the chunks or the dissolve; the Corpse pool and its clear). Owner tonight: builder X.
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed partial class EnemyLook
    {
        sealed class Corpse
        {
            public bool live;
            public int parts;
            public float age, flash, vibAge;
            public Vector3 dir;
            public readonly Mesh[] meshes = new Mesh[MaxParts];
            public readonly Matrix4x4[] matrices = new Matrix4x4[MaxParts];
            public readonly Color[] colors = new Color[MaxParts];
            public readonly float[] rims = new float[MaxParts];
            public readonly MaterialPropertyBlock[] blocks = new MaterialPropertyBlock[MaxParts];
        }

        readonly Corpse[] corpses;
        RenderParams corpseParams;

        /// <summary>
        /// A view's enemy died (its 'kill', inside the sim step). NIGHT17 (CHUNKS ON): its parts as last drawn break into
        /// their blocks (Game/Chunks.cs) along the killing hit's shot, x its overkill, a punch harder, a headshot popping the
        /// head part (`headPart`; -1 none) high. CHUNKS OFF (or no Enemy shader): the NIGHT12 dissolve (below).
        /// </summary>
        public void Die(Enemy en, KillEvent e, Renderer[] rends, Mesh[] meshes, Color[] colors, float[] rims, int parts, int headPart, Enemy40Parts m40 = null)
        {
            if (UnityTuning.CrunchChunksOn)
            {
                Vector3 shot = Vector3.forward;
                double damage = 0;
                for (int i = 0; i < hits.Length; i++)
                {
                    if (hits[i].id != e.enemy) continue;
                    shot = hits[i].shot;
                    damage = hits[i].damage;
                    break;
                }
                float throwK = CrunchMath.Overkill(damage, en.maxHp, UnityTuning.CrunchOverkillMin, UnityTuning.CrunchOverkillMax);
                throwK = KillFeel42.ThrowMult(e, throwK); // NIGHT42 F1 (kills42 + impact42): a crunchier break by the kill's tier (off: unchanged)
                SigParams sig = CrunchMath.ParamsOf(CrunchMath.SignatureOf(e.weapon, e.melee)); // U3-30.10: every gun reads differently
                // NIGHT18 U3-31.3 (FeelAB on): tonight's throw (across the screen and up, one side per kill frame), the cone, the
                // THUNDERCLAP's shove inside the freeze; off: NIGHT17's numbers (the same values ParamsOf gives)
                bool on = FeelAB.On;
                if (sig.sig == DeathSig.Blast) sig.speedMult = UnityTuning.Pick(on, UnityTuning.KillBlastSpeed, UnityTuning.KillBlastSpeedN17); // round 4
                if (sig.sig == DeathSig.Clean) sig.coneDeg = UnityTuning.Pick(on, UnityTuning.KillConeDeg, UnityTuning.KillConeDegN17);
                if (sig.sig == DeathSig.Shove)
                {
                    sig.coneDeg = UnityTuning.Pick(on, UnityTuning.KillShoveConeDeg, UnityTuning.KillShoveConeDegN17);
                    sig.holdTime = UnityTuning.Pick(on, UnityTuning.KillShoveHold, UnityTuning.KillShoveHoldN17);
                    sig.slide = UnityTuning.Pick(on, UnityTuning.KillShoveSlide, UnityTuning.KillShoveSlideN17); // round 1
                    sig.speedMult = UnityTuning.Pick(on, UnityTuning.KillShoveSpeed, UnityTuning.KillShoveSpeedN17); // round 2
                }
                Vector3 feet = WebSpace.ToUnity(en.position);
                Vector3 center = feet + Vector3.up * (float)(en.Height * 0.5);
                if (on && killSideFrame != Time.frameCount)
                {
                    // every kill of one frame (a blast's horde) throws to one side: the cloud moves as one; round 1: the open side
                    // (a wall close on one side turned b3's horde back); round 2: else the left (away from the points pop)
                    killSideFrame = Time.frameCount;
                    Vector3 right = game.cam != null ? game.cam.transform.right : Vector3.right;
                    killSide = CrunchMath.ThrowSide(chunks.RoomAlong(center, right), chunks.RoomAlong(center, -right), UnityTuning.KillSidePreferred);
                }
                int made = 0;
                for (int i = 0; i < Mathf.Min(parts, MaxParts); i++)
                    made += chunks.Break(rends[i].localToWorldMatrix, meshes[i].bounds, colors[i], shot, center, throwK, sig, e.headshot && i == headPart, feet.y,
                        (float)e.distance, on ? killSide : 0); // U3-30.10: the gun's death signature; round 4: a far kill holds its white
                // NIGHT41 E6 models41 (A8): the NIGHT40 silhouette parts break with the body (they vanished with its root): each shown part's blocks
                // thrown along the same shot, signature and side, in the colour it was last posed (off: the parts vanish, as NIGHT40)
                if (m40 != null && (m40.kit43 || Models41On())) // NIGHT43 A1 models43: the rebuilt models' parts always break with the body
                    for (int i = 0; i < m40.parts.Length; i++)
                        if (m40.parts[i].gameObject.activeInHierarchy && m40.meshes[i] != null)
                            made += chunks.Break(m40.rends[i].localToWorldMatrix, m40.meshes[i].bounds, m40.colors[i], shot, center, throwK, sig, false, feet.y,
                                (float)e.distance, on ? killSide : 0);
                if (sig.split) chunks.Seam(center, shot, new Vector3(UnityTuning.SigSeamWidth, (float)en.Height, (float)en.Radius * 2), feet.y);
                chunks.Breaks++;
                spawnFlashes.KillRing(feet, sig.ringScale); // NIGHT17 round 4: the DANGER floor ring at the victim's feet (U3-30.10: x the signature's)
                Scorch51Kill(en, feet, parts > 0 ? colors[0] : Color.white); // NIGHT51 scorch51: the floor keeps the kill's mark
                if (on) KillBurstN18(en, center);
                else
                {
                    // NIGHT17 round 5: the kill's radial burst on its impact frame: about 2x the body's height, never wider on screen
                    // than BurstMaxAngle (a close kill's would wash the frame)
                    float burst = Mathf.Min((float)en.Height * UnityTuning.BurstBodies, UnityTuning.BurstMaxAngle * Mathf.Max(0.5f, (float)e.distance));
                    // round 6 (far kills read as pixel specks): at range the burst keeps BurstMinScreen of the screen's height
                    float viewH = game.cam != null ? Vector3.Distance(game.cam.transform.position, center) * 2 * Mathf.Tan(game.cam.fieldOfView * 0.5f * Mathf.Deg2Rad) : 0;
                    burst = Mathf.Max(burst, viewH * UnityTuning.BurstMinScreen);
                    spawnFlashes.KillBurst(center, burst);
                }
                if (e.headshot && headPart >= 0 && headPart < parts) spawnFlashes.HeadRing(rends[headPart].transform.position); // round 5
                // NIGHT17 round 1 / 2: the first breaks of a process, logged (the reel's log proves the break and its draw),
                // written CrunchLogDelayFrames later (no string on the kill frame)
                if (chunks.Breaks <= 3) chunks.QueueLog(e.kind, e.headshot, e.melee, made, ok);
                ForgetShed(e.enemy);
                return;
            }
            if (!ok) return;
            int id = e.enemy;
            double flash = en.flash;
            Corpse c = corpses[0];
            float oldest = -1;
            for (int k = 0; k < corpses.Length; k++)
            {
                if (!corpses[k].live)
                {
                    c = corpses[k];
                    break;
                }
                if (corpses[k].age > oldest)
                {
                    oldest = corpses[k].age;
                    c = corpses[k];
                }
            }
            c.live = true;
            c.age = 0;
            c.flash = (float)flash;
            c.parts = Mathf.Min(parts, MaxParts);
            for (int i = 0; i < c.parts; i++)
            {
                c.meshes[i] = meshes[i];
                c.matrices[i] = rends[i].localToWorldMatrix;
                c.colors[i] = colors[i];
                c.rims[i] = rims[i];
            }
            c.dir = Vector3.zero;
            c.vibAge = Idle;
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].id != id) continue;
                c.dir = hits[i].dir;
                c.vibAge = hits[i].vibAge;
                break;
            }
        }

        int models41 = -1; // NIGHT41 E6: design.n41.models41 (N41View.RowOn), read at the first kill (-1: not read yet)

        /// <summary>NIGHT41 E6: is the models41 view row on (the parts' death break-apart)?</summary>
        bool Models41On()
        {
            if (models41 < 0) models41 = N41View.RowOn(game.cfg, "models41") ? 1 : 0;
            return models41 == 1;
        }

        void ClearCorpses()
        {
            killSide = -1; // NIGHT18: a run's first kill throws to the camera's right
            killSideFrame = -1;
            if (corpses == null) return;
            for (int k = 0; k < corpses.Length; k++) corpses[k].live = false;
        }

        // NIGHT18 U3-31.3: the kill throw's side (+1 the camera's right), flipped once per frame with a kill
        float killSide = -1;
        int killSideFrame = -1;

        /// <summary>
        /// NIGHT18 U3-31.3 (M1 / M2, FeelAB on): tonight's burst for a kill at `center`: KillBurstBodies x the body's height,
        /// capped at KillBurstMaxAngle x the distance, floored at KillBurstMinScreen of the screen's height
        /// (CrunchMath.KillBurstSize), drawn KillBurstDepthShare of the way from the eye (scaled to the same screen size, so
        /// nothing between it and the body hides it); queued with the body's height (the crush disc's size).
        /// </summary>
        void KillBurstN18(Enemy en, Vector3 center)
        {
            Camera cam = game.cam;
            if (cam == null)
            {
                spawnFlashes.KillBurst(center, (float)en.Height * UnityTuning.KillBurstBodies, true, (float)en.Height);
                return;
            }
            Vector3 eye = cam.transform.position;
            float dist = Mathf.Max(0.1f, Vector3.Distance(eye, center));
            float viewTan = 2 * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float size = CrunchMath.KillBurstSize((float)en.Height, dist, viewTan, UnityTuning.KillBurstBodies, UnityTuning.KillBurstMaxAngle, UnityTuning.KillBurstMinScreen);
            float share = UnityTuning.KillBurstDepthShare;
            spawnFlashes.KillBurst(eye + (center - eye) * share, size * share, true, (float)en.Height);
        }
    }
}
