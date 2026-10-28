// NIGHT26 (builder M): SURFACES, pure (no engine). A level plan's "surfaces" list ([{kind, rect:[x0,z0,x1,z1], minY, maxY, dir:[dx,dz]}]) paints
// rectangles of the floor: ICE (ground friction x design.n26.surfaces.iceFrictionMult), MUD (x mudFrictionMult) and a BOOST strip (one forward push of
// design.n26.surfaces.boostImpulse m/s through Mover.Impulse each time a grounded player enters it). The only Mover change is the public
// groundFrictionMult (default 1: every golden vector and trajectory is untouched); this class sets it each fixed step from the zone under the
// player's feet, and back to 1 off every zone. The surface state is a function of the level data and the player's position: a tape needs no new input.
// Unity-only (the web has no surfaces); the numbers are config rows with a _why.
using System.Collections.Generic;

namespace Skygrind.Core
{
    public enum SurfaceKind { None, Ice, Mud, Boost }

    public struct SurfaceRules
    {
        public double iceMult, mudMult, boostImpulse;

        /// <summary>design.n26.surfaces rows (1 / 1 / 0 when absent: no effect).</summary>
        public static SurfaceRules From(GameConfig cfg) => new SurfaceRules
        {
            iceMult = cfg.Has("design.n26.surfaces.iceFrictionMult") ? cfg.Num("design.n26.surfaces.iceFrictionMult") : 1,
            mudMult = cfg.Has("design.n26.surfaces.mudFrictionMult") ? cfg.Num("design.n26.surfaces.mudFrictionMult") : 1,
            boostImpulse = cfg.Has("design.n26.surfaces.boostImpulse") ? cfg.Num("design.n26.surfaces.boostImpulse") : 0,
        };
    }

    public sealed class SurfaceZone
    {
        public SurfaceKind kind;
        public double x0, z0, x1, z1;
        public double minY = -1e9, maxY = 1e9;
        /// <summary>A boost strip's push direction (unit, horizontal); default -z (the plans run toward -z).</summary>
        public double dirX, dirZ = -1;

        public bool Contains(double x, double y, double z) => x >= x0 && x <= x1 && z >= z0 && z <= z1 && y >= minY && y <= maxY;

        public static SurfaceKind ParseKind(string s) => s == "ice" ? SurfaceKind.Ice : s == "mud" ? SurfaceKind.Mud : s == "boost" ? SurfaceKind.Boost : SurfaceKind.None;
    }

    public sealed class Surfaces26
    {
        public readonly SurfaceRules rules;
        public readonly List<SurfaceZone> zones = new List<SurfaceZone>();
        int inBoost = -1; // the boost zone the player stood in last step (-1: none): a push happens on entry only

        public Surfaces26(SurfaceRules rules) { this.rules = rules; }

        /// <summary>The plan's "surfaces" list (a level plan, or a map JSON with a "campaign" block); none: no zones. Bad rows go to `problems`.</summary>
        public static Surfaces26 From(Dictionary<string, object> planOrMap, SurfaceRules rules, List<string> problems = null)
        {
            var s = new Surfaces26(rules);
            if (planOrMap == null) return s;
            Dictionary<string, object> c = planOrMap.ContainsKey("campaign") ? PortData.Obj(planOrMap["campaign"]) : planOrMap;
            if (!c.TryGetValue("surfaces", out object raw) || !(raw is List<object> list)) return s;
            int i = 0;
            foreach (object o in list)
            {
                i++;
                var d = o as Dictionary<string, object>;
                var z = new SurfaceZone();
                z.kind = d != null && d.TryGetValue("kind", out object k) ? SurfaceZone.ParseKind(k as string) : SurfaceKind.None;
                var r = d != null && d.TryGetValue("rect", out object ro) ? ro as List<object> : null;
                if (z.kind == SurfaceKind.None || r == null || r.Count != 4)
                {
                    problems?.Add($"surfaces[{i}]: needs kind ice|mud|boost and rect [x0, z0, x1, z1]");
                    continue;
                }
                z.x0 = PortData.Num(r[0]); z.z0 = PortData.Num(r[1]); z.x1 = PortData.Num(r[2]); z.z1 = PortData.Num(r[3]);
                if (z.x1 <= z.x0 || z.z1 <= z.z0) { problems?.Add($"surfaces[{i}]: the rect is empty"); continue; }
                if (d.TryGetValue("minY", out object a)) z.minY = PortData.Num(a);
                if (d.TryGetValue("maxY", out object b)) z.maxY = PortData.Num(b);
                if (d.TryGetValue("dir", out object dr) && dr is List<object> dl && dl.Count == 2)
                {
                    double dx = PortData.Num(dl[0]), dz = PortData.Num(dl[1]), len = JsMath.Hypot(dx, dz);
                    if (len > 0) { z.dirX = dx / len; z.dirZ = dz / len; }
                }
                s.zones.Add(z);
            }
            return s;
        }

        public bool Any => zones.Count > 0;

        /// <summary>The ground friction multiplier at a point: the first ICE / MUD zone holding it; 1 elsewhere (a boost strip changes no friction).</summary>
        public double FrictionMult(double x, double y, double z)
        {
            foreach (SurfaceZone q in zones)
            {
                if (!q.Contains(x, y, z)) continue;
                if (q.kind == SurfaceKind.Ice) return rules.iceMult;
                if (q.kind == SurfaceKind.Mud) return rules.mudMult;
            }
            return 1;
        }

        /// <summary>Run start (or a respawn): friction back to 1, the strip latch cleared.</summary>
        public void Reset(Mover m)
        {
            inBoost = -1;
            if (m != null) m.groundFrictionMult = 1;
        }

        /// <summary>One fixed step, before the mover's own: the zone under a GROUNDED player sets groundFrictionMult (1 in the air and off every zone);
        /// a grounded entry into a boost strip pushes once along the strip's direction.</summary>
        public void Step(Mover m)
        {
            Vec3 p = m.position;
            if (!m.grounded)
            {
                m.groundFrictionMult = 1;
                return;
            }
            m.groundFrictionMult = FrictionMult(p.x, p.y, p.z);
            int now = -1;
            for (int i = 0; i < zones.Count; i++)
                if (zones[i].kind == SurfaceKind.Boost && zones[i].Contains(p.x, p.y, p.z)) { now = i; break; }
            if (now >= 0 && now != inBoost && rules.boostImpulse > 0)
                m.Impulse(zones[now].dirX * rules.boostImpulse, 0, zones[now].dirZ * rules.boostImpulse);
            inBoost = now;
        }
    }
}
