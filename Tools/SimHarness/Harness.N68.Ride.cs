// NIGHT68 builder T, slice I1 ("the ride pilot rides the line65 face only, and the ride refusal holds"; Logs/n68/slate.json I1;
// Research/2026-10-24-forge68/code-scout.md (a)): the level's ride faces (campaign boxes with `ride: true` and a `ride36` block) as one list,
// and which face a wall ride started on (skip v2's per-face ride count: the verb-aware run must ride only line65's face).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    /// <summary>One ride face: axis 0 = an x-plane (the face at x = plane, the ride along z), 1 = a z-plane; sign = its outward normal's sign
    /// on that axis; lo..hi its length along the ride; top its box's top; zone its room.</summary>
    sealed class V68Face
    {
        public string key, zone;
        public int axis;
        public double plane, sign, lo, hi, top;
    }

    static List<V68Face> V68Faces(Dictionary<string, object> json)
    {
        var res = new List<V68Face>();
        var camp = PortData.Obj(json["campaign"]);
        if (!camp.TryGetValue("boxes", out object bo)) return res;
        foreach (object o in PortData.Arr(bo))
        {
            var b = PortData.Obj(o);
            if (!(b.TryGetValue("ride", out object rv) && rv is bool rb && rb) || !b.TryGetValue("ride36", out object r36)) continue;
            var f = PortData.Obj(r36);
            string face = f.TryGetValue("face", out object fo) ? (string)fo : "E";
            var mn = PortData.Arr(b["min"]); var mx = PortData.Arr(b["max"]);
            double x0 = PortData.Num(mn[0]), z0 = PortData.Num(mn[2]), x1 = PortData.Num(mx[0]), z1 = PortData.Num(mx[2]), top = PortData.Num(mx[1]);
            string zone = b.TryGetValue("zone", out object zo) ? (string)zo : "";
            var v = new V68Face { zone = zone, top = top };
            if (face == "E") { v.axis = 0; v.plane = x1; v.sign = 1; v.lo = z0; v.hi = z1; }
            else if (face == "W") { v.axis = 0; v.plane = x0; v.sign = -1; v.lo = z0; v.hi = z1; }
            else if (face == "S") { v.axis = 1; v.plane = z1; v.sign = 1; v.lo = x0; v.hi = x1; }
            else { v.axis = 1; v.plane = z0; v.sign = -1; v.lo = x0; v.hi = x1; }
            v.key = $"{zone} {face} {(v.axis == 0 ? "x" : "z")} {V66R(v.plane)} ({V66R(v.lo)}..{V66R(v.hi)})";
            res.Add(v);
        }
        return res;
    }

    /// <summary>The ride face the Mover's feet are on at a ride's start: within -0.2..1.5 m of its plane on its side (the side tells the two
    /// faces of one thin wall apart), inside its length (1 m either end), under its top + 1 m; else "another wall". (The Mover's ride normal is
    /// private and its public wallNormal is not set on a drawn start, so the feet decide.)</summary>
    static string V68FaceOf(List<V68Face> faces, Mover m)
    {
        Vec3 p = m.position;
        foreach (V68Face f in faces)
        {
            double off = ((f.axis == 0 ? p.x : p.z) - f.plane) * f.sign, s = f.axis == 0 ? p.z : p.x;
            if (off < -0.2 || off > 1.5 || s < f.lo - 1 || s > f.hi + 1 || p.y > f.top + 1) continue;
            return f.key;
        }
        return "another wall";
    }

    /// <summary>A room's line65 on one ride face (code-scout.md (a1)): the face (its index in V68Faces), the ride's direction along the face
    /// (+1 / -1), the entry and exit along it, the entry point (x, z); or why none matched (`why`).</summary>
    sealed class V68Line
    {
        public int face = -1;
        public double dir, entryS, exitS, entryX, entryZ;
        public string why;
    }

    /// <summary>The longest run of line65 segments on one face of the room: a segment is on a face when both its ends are 0-1.6 m out on the
    /// face's side and inside its length (lo..hi); consecutive segments on the same face in the same direction join; the run must span 3 m or
    /// more. The exit is clamped to lo + 1.5 .. hi - 1.5 (the old pilot's end margin).</summary>
    static V68Line V68LineFace(List<double[]> line, List<V68Face> faces, string room)
    {
        var best = new V68Line();
        double bestLen = 0;
        for (int i = 0; i < faces.Count; i++)
        {
            V68Face f = faces[i];
            if (f.zone != room) continue;
            double runStart = double.NaN, runEnd = double.NaN, runDir = 0, sx = 0, sz = 0;
            for (int k = 0; k + 1 <= line.Count; k++)
            {
                bool on = false;
                double sa = 0, sb = 0;
                if (k + 1 < line.Count)
                {
                    double[] a = line[k], b = line[k + 1];
                    double oa = ((f.axis == 0 ? a[0] : a[2]) - f.plane) * f.sign, ob = ((f.axis == 0 ? b[0] : b[2]) - f.plane) * f.sign;
                    sa = f.axis == 0 ? a[2] : a[0]; sb = f.axis == 0 ? b[2] : b[0];
                    on = oa >= 0 && oa <= 1.6 && ob >= 0 && ob <= 1.6 && sa >= f.lo - 0.01 && sa <= f.hi + 0.01 && sb >= f.lo - 0.01 && sb <= f.hi + 0.01 && Math.Abs(sb - sa) > 1e-6;
                }
                double d = Math.Sign(sb - sa);
                if (on && !double.IsNaN(runStart) && d == runDir) { runEnd = sb; continue; }
                // the run so far ends here: keep it if it is the longest
                if (!double.IsNaN(runStart) && Math.Abs(runEnd - runStart) >= 3 && Math.Abs(runEnd - runStart) > bestLen)
                {
                    bestLen = Math.Abs(runEnd - runStart);
                    best = new V68Line { face = i, dir = runDir, entryS = runStart, exitS = Math.Max(f.lo + 1.5, Math.Min(f.hi - 1.5, runEnd)), entryX = sx, entryZ = sz };
                }
                runStart = double.NaN;
                if (on) { runStart = sa; runEnd = sb; runDir = d; sx = line[k][0]; sz = line[k][2]; }
            }
        }
        if (best.face < 0) best.why = line.Count < 2 ? $"no line65 on {room}'s row" : $"line65 matches no ride36 face of {room} (no 3 m run 0-1.6 m out on a face, inside its length)";
        return best;
    }

    /// <summary>The noride boxes (the game's own rule, Mover.SetNoRide38: no ride starts on, is drawn onto or turns onto a wall inside one) for a
    /// room: the level's own boxes plus the room's rect (2 m out, every height), less a slab round one face when `keep` is given (the line's face
    /// during the pilot's line episode). The slab: the face's plane -0.3 .. +0.5 m on its side, its length +-0.5 m; the boxes round it stop
    /// `skin` + 0.05 m short of it (NoRideBand grows each box by the ride probe, `skin`).</summary>
    static Ride38.Box[] V68NoRide(Ride38.Box[] own, RoomDef rd, V68Face keep, double skin)
    {
        var res = new List<Ride38.Box>();
        if (own != null) res.AddRange(own);
        double X0 = rd.x0 - 2, X1 = rd.x1 + 2, Z0 = rd.z0 - 2, Z1 = rd.z1 + 2;
        Ride38.Box B(double x0, double z0, double x1, double z1) => new Ride38.Box { min = new Vec3(x0, -50, z0), max = new Vec3(x1, 200, z1) };
        if (keep == null) { res.Add(B(X0, Z0, X1, Z1)); return res.ToArray(); }
        double a0 = keep.sign > 0 ? keep.plane - 0.3 : keep.plane - 0.5, a1 = keep.sign > 0 ? keep.plane + 0.5 : keep.plane + 0.3;
        double b0 = keep.lo - 0.5, b1 = keep.hi + 0.5, g = skin + 0.05;
        // the slab in (x, z): axis 0 = an x-plane (a on x, b on z); axis 1 = a z-plane (a on z, b on x)
        double ex0 = keep.axis == 0 ? a0 : b0, ex1 = keep.axis == 0 ? a1 : b1, ez0 = keep.axis == 0 ? b0 : a0, ez1 = keep.axis == 0 ? b1 : a1;
        res.Add(B(X0, Z0, ex0 - g, Z1));
        res.Add(B(ex1 + g, Z0, X1, Z1));
        res.Add(B(ex0 - g, Z0, ex1 + g, ez0 - g));
        res.Add(B(ex0 - g, ez1 + g, ex1 + g, Z1));
        return res.ToArray();
    }

    static string V68FaceCounts(Dictionary<string, int> d) => d.Count == 0 ? "none" : string.Join("; ", d.OrderByDescending(kv => kv.Value).ThenBy(kv => kv.Key).Select(kv => $"{kv.Key} {kv.Value}"));
}
