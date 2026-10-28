// NIGHT19 U3-33.6 / U3-33.8 (builder V): the BOUNCER's grenades, drawn as the web draws them (src/combat/projectiles.js
// :67 grenadeMesh = BoxGeometry(0.2, 0.2, 0.2) in hdr(fam.blast, 1.8); render :495-506): each live grenade interpolated
// between its last two steps, tumbling about a stable axis from its launch direction (tmp2Axis :772-775) at the web's
// 9 rad/s of frame time while it moves (a grenade at rest stops turning: the web's `p.rest`, read here as no motion
// between the two steps), blinking x1.25 in its last 0.6 s of fuse (floor(left x 20) % 2). Port-only: a soft fuse light
// (the Kenney flare sprite, CC0) on each blink in the danger red-orange (a grenade about to burst is a danger, CLAUDE.md
// rule 8). The pool is the sim's (Projectiles' `grenades`, builder W's port of projectiles.js launch('grenade')); until it
// lands the view finds no pool and draws nothing. Visual only, pooled GameObjects, no allocation per frame.
using System;
using System.Reflection;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>projectiles.js's grenade render maths, pure (HandsN19Tests).</summary>
    public static class BouncerFxMath
    {
        /// <summary>render :503-504: left = max(0, fuse - ageSteps / simHz); x1.25 when left &lt; 0.6 and floor(left x 20) is even.</summary>
        public static double Blink(double fuse, int ageSteps, double simHz)
        {
            double left = Math.Max(0, fuse - ageSteps / simHz);
            return left < UnityTuning.GrenadeBlinkFrom && Math.Floor(left * UnityTuning.GrenadeBlinkHz) % 2 == 0 ? UnityTuning.GrenadeBlinkScale : 1;
        }

        /// <summary>tmp2Axis :772-775: (dir.z || 0.3, 0.4, -(dir.x || 0.2)) normalised (JS `||`: 0 takes the default).</summary>
        public static Vec3 TumbleAxis(Vec3 dir)
        {
            double x = dir.z != 0 && !double.IsNaN(dir.z) ? dir.z : 0.3;
            double z = -(dir.x != 0 && !double.IsNaN(dir.x) ? dir.x : 0.2);
            var a = new Vec3(x, 0.4, z);
            return a / a.Length;
        }
    }

    public sealed partial class SurvivalGame
    {
        Projectiles.Projectile[] grenadePool; // the sim's (Projectiles.grenades); null until builder W's port lands
        Transform[] grenadeViews;
        double[] grenadeSpin;
        double grenadeFuse, grenadeHz;
        Color grenadeDanger; // colors.enemy (before UiTheme is made)
        FxSprites grenadeLight;
        static readonly Projectiles.Projectile DeadGrenade36 = new Projectiles.Projectile(); // NIGHT36: a dead slot's stand-in (alive false)

        partial void N19BouncerFxInit()
        {
            grenadeFuse = cfg.Num("weapons.bouncer.fuse");
            grenadeHz = 1 / cfg.Dt;
            grenadeDanger = Look.C(cfg, "colors.enemy"); // read once (PORT-RULES 6)
            // Bound once by name (the web's `grenades`, projectiles.js :56) so this view compiles before the sim's pool exists.
            FieldInfo f = projectiles != null ? typeof(Projectiles).GetField("grenades", BindingFlags.Public | BindingFlags.Instance) : null;
            grenadePool = f != null ? f.GetValue(projectiles) as Projectiles.Projectile[] : null;
            if (grenadePool == null) return;
            var kit = new MeshKit();
            kit.Cube(Vector3.zero, Vector3.one * UnityTuning.GrenadeSize, Look.Hex((uint)cfg.Num("weapons.families.blast"), UnityTuning.GrenadeHdr));
            Mesh mesh = kit.Build("grenade");
            var root = new GameObject("Grenades").transform;
            grenadeViews = new Transform[grenadePool.Length];
            grenadeSpin = new double[grenadePool.Length];
            for (int i = 0; i < grenadeViews.Length; i++)
            {
                GameObject go = MeshKit.Object($"grenade {i}", root, mesh, mats.neon, shadows: false);
                go.SetActive(false);
                grenadeViews[i] = go.transform;
            }
            grenadeLight = new FxSprites(mats.neon, UnityTuning.KenneyFlare, grenadePool.Length);
        }

        /// <summary>projectiles.js render() for the grenades: interpolated, tumbling, blinking; the fuse light on a blink.</summary>
        partial void N19BouncerFxRender(double alpha, double frameDt)
        {
            if (grenadePool == null || grenadeViews == null) return;
            bool lights = grenadeLight != null && grenadeLight.Ready && cam != null;
            if (lights) grenadeLight.Begin();
            bool lit = false;
            Color danger = UiTheme.Ready ? UiTheme.Danger : grenadeDanger;
            for (int i = 0; i < grenadePool.Length; i++)
            {
                Projectiles.Projectile p = grenadePool[i];
                Transform t = grenadeViews[i];
                bool alive = p != null && p.alive;
                if (t.gameObject.activeSelf != alive) t.gameObject.SetActive(alive);
                if (!alive)
                {
                    grenadeSpin[i] = 0;
                    projectileViews?.Grenade36(i, grenadePool.Length, t, p ?? DeadGrenade36, frameDt); // NIGHT36 F muzzle36: the slot is free again
                    continue;
                }
                Vec3 at = p.previous + (p.position - p.previous) * alpha;
                if (p.position != p.previous) grenadeSpin[i] += frameDt * UnityTuning.GrenadeSpinRate; // if (!p.rest) p.spin += frameDt * 9
                Vec3 axis = BouncerFxMath.TumbleAxis(p.dir);
                double blink = BouncerFxMath.Blink(grenadeFuse, p.ageSteps, grenadeHz);
                Vector3 pos = WebSpace.ToUnity(at);
                t.SetPositionAndRotation(pos, Quaternion.AngleAxis(-(float)(grenadeSpin[i] * Mathf.Rad2Deg), WebSpace.ToUnity(axis)));
                t.localScale = Vector3.one * (float)blink;
                projectileViews?.Grenade36(i, grenadePool.Length, t, p, frameDt); // NIGHT36 F muzzle36: drawn from the muzzle, blending onto its path
                pos = t.position;
                if (lights && blink > 1)
                {
                    grenadeLight.Billboard(pos, UnityTuning.GrenadeFuseLight, 0, cam.transform, danger * UnityTuning.GrenadeFuseHdr, 1);
                    lit = true;
                }
            }
            if (lit) grenadeLight.Draw();
        }
    }
}
