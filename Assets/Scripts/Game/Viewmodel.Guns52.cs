// NIGHT52 builder U, guns52 (design.n52.guns52, a view row): the Viewmodel's half. The row is read when the viewmodel is made (the KESTREL and
// THUNDERCLAP are built then; also from the command line, as skin52: the viewmodel is made before PlayerChecks applies -skygrindOffNN), and
// BlockyKit.Guns52 is set only round those two guns' builds (the load, a finish's rebuild, the a352 reel's rebuild): every other gun, the wall
// buys, the pickups and the mystery box are built as before. The machined build itself: BlockyKit.Guns52.cs. Off: nothing changes.
using System;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Viewmodel
    {
        bool guns52;

        /// <summary>From the constructor (before the guns are built): the row (or the reel's Force), and the off switch from the command line.</summary>
        void Guns52Prepare()
        {
            bool offSwitch = NightRows.OffFrom(Environment.GetCommandLineArgs(), "-skygrindOff") <= 52;
            guns52 = Guns52Probe.Force ?? (NightRows.ViewOn(cfg, "guns52") && !offSwitch);
            // round 3: the fallback by data (the KESTREL only): design.n52.guns52.numbers.thunderclap 0; absent or 1: both guns
            const string key = "design.n52.guns52.numbers.thunderclap";
            Guns52Probe.Thunderclap = cfg == null || !cfg.Has(key) || cfg.Num(key) > 0.5;
            Guns52Probe.Built = guns52;
            Guns52Probe.ResetCounts();
        }

        /// <summary>The two guns are built machined now (the row built, not the reel's NIGHT51 frame).</summary>
        bool Guns52Live => guns52 && !Guns52Probe.ForceOff;

        static bool Guns52Gun(string id) => id == "revolver" || (id == "shotgun" && Guns52Probe.Thunderclap);

        /// <summary>A rig's build with BlockyKit.Guns52 set for the KESTREL / THUNDERCLAP while guns52 draws (reset after, whatever happens); the
        /// THUNDERCLAP with the big gun's build (BlockyKit.Guns52Big).</summary>
        Rig Guns52Build(string id, Func<Rig> build)
        {
            Guns52Flag(id, true);
            try { return build(); }
            finally { Guns52Flag(id, false); }
        }

        /// <summary>Refinish's flag for `rig` (set before its builders run, cleared after).</summary>
        void Guns52Flag(string id, bool on)
        {
            BlockyKit.Guns52 = on && Guns52Live && Guns52Gun(id);
            BlockyKit.Guns52Big = BlockyKit.Guns52 && id == "shotgun";
        }

        /// <summary>The a352 reel: rebuild the KESTREL's and THUNDERCLAP's meshes in place in the state now (after flipping Guns52Probe.ForceOff):
        /// the body and its parts (as a finish change does) and the KESTREL's hammer.</summary>
        public void Guns52Rebuild()
        {
            Guns52Probe.ResetCounts();
            foreach (string id in new[] { "revolver", "shotgun" })
                if (rigs.TryGetValue(id, out Rig r)) Refinish(r, r.finishShown);
            if (hammer36 != null)
            {
                MeshFilter mf = hammer36.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null)
                {
                    var k = new MeshKit();
                    BlockyKit.Guns52 = Guns52Live;
                    try { BlockyKit.KestrelHammer36(k); }
                    finally { BlockyKit.Guns52 = false; }
                    k.Build(mf.sharedMesh.name, mf.sharedMesh);
                }
            }
            Guns52Probe.Rebuilds++;
        }

        /// <summary>The a352 reel: a gun's mesh vertex count (its body and parts) and its screen box now (px, origin bottom-left).</summary>
        public int Guns52Verts(string id)
        {
            if (!rigs.TryGetValue(id, out Rig r)) return -1;
            int n = 0;
            foreach (Mesh m in r.meshes) if (m != null) n += m.vertexCount;
            return n;
        }

        public Rect Guns52Box(string id) => rigs.TryGetValue(id, out Rig r) ? Glow50Box(r) : Rect.zero;

        /// <summary>The a352 reel: a gun's meshes' local bounds together (the body's and its parts', each in its own space: the silhouette's
        /// extent; a chamfer only takes corners off, so machined = plain).</summary>
        public Bounds Guns52Bounds(string id)
        {
            var b = new Bounds();
            bool any = false;
            if (rigs.TryGetValue(id, out Rig r))
                foreach (Mesh m in r.meshes)
                {
                    if (m == null) continue;
                    if (!any) { b = m.bounds; any = true; }
                    else b.Encapsulate(m.bounds);
                }
            return b;
        }
    }
}
