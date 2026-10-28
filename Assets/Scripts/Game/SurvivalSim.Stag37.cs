// NIGHT37 builder S (stag37; NIGHT37.md S1, BOSSES.md "L2 THE STAG"): THE STAG's canal and fight in the sim. Implements the lead's partials
// (SurvivalSim.N37.cs):
//   N37StagStartRun  a level whose campaign room row has `stag37` (Tools/night/gen-n37-iceline-boss.cjs: room s4 THE CANAL) gets THE CANAL from
//                    level load: the loop (Core/Enemies/Stag37Arena.cs) as world layer "stag37Canal" at the row's centre (the entry gap open), the
//                    floes "stag37Floes", its own ice (Surfaces26 + the glide's ice test). It is level geometry (only n37-iceline has the row), so it
//                    stands whatever the rows say; with the row on, boss37WalkInHere: the fight is where the player walked in.
//   N37StagBegin     (slice 2) the plan id "stag37" starts THE STAG.
//   the fight: SurvivalSim.Stag37Fight.cs.
// No UnityEngine: SimHarness compiles this file.
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        public Stag37Arena stag37Arena;
        /// <summary>The canal's centre (world, on the ice) on this level, or null when the level has no stag37 row.</summary>
        public Vec3? stag37Centre;
        /// <summary>The harness / reel put the canal here on a map without the row (null: the level's row).</summary>
        public Vec3? stag37CentreOverride;
        readonly BoxLayer stag37CanalLayer = new BoxLayer(), stag37FloeLayer = new BoxLayer(), stag37CutLayer = new BoxLayer();
        Surfaces26 stag37Surf;
        bool stag37Slab, stag37GlideHooked;
        /// <summary>Each floe: down now (the view hides it), and its RED telegraph steps left (the view tints it).</summary>
        public bool[] stag37FloeDown;
        public int[] stag37FloeTell, stag37FloeDownLeft;
        /// <summary>A floe's RED telegraph length in steps (the view's pulse reads progress from it).</summary>
        public int Stag37FloeTellSteps;
        /// <summary>The entry slab is closed (the view draws it red).</summary>
        public bool Stag37SlabClosed => stag37Slab;

        partial void N37StagStartRun()
        {
            if (stag37 != null)
            {
                stag37.Clear();
                enemies.all.Remove(stag37.body);
            }
            stag37Owns = false;
            stag37SlabLeft = stag37CardLeft = stag37FloeTimer = 0;
            stag37CardStep = -1;
            stag37Begins = stag37Retries = stag37Falls = stag37FloeDrops = stag37SlipPushes = 0;
            stag37Cuts.Clear();
            Stag37Slipping = false;
            stag37LastPunch = "";
            N37StagPacerStartRun();
            if (stag37Pacer != null) Stag37HookPunch();
            stag37Centre = Stag37RowCentre();
            if (stag37Centre == null)
            {
                world.SetLayer("stag37Canal", null);
                world.SetLayer("stag37Floes", null);
                world.SetLayer("stag37Cuts", null);
                stag37Surf = null;
                return;
            }
            Stag37Build();
            if (n37.stag37) boss37WalkInHere = true;
        }

        /// <summary>The reel's / harness's way onto a map without the row: the canal at `centre` now (the gap open), as a level row would put it.</summary>
        public void Stag37Place(Vec3 centre)
        {
            stag37CentreOverride = centre;
            stag37Centre = centre;
            Stag37Build();
            if (n37.stag37) boss37WalkInHere = true;
        }

        void Stag37Build()
        {
            stag37Arena ??= new Stag37Arena(Stag37ArenaParams.From(cfg, n38.stag38)); // NIGHT38 S2: the run's stag38 bit (at a run's start N38StagStartRun checks it again)
            int n = stag37Arena.floes.Count;
            if (stag37FloeDown == null || stag37FloeDown.Length != n) { stag37FloeDown = new bool[n]; stag37FloeTell = new int[n]; stag37FloeDownLeft = new int[n]; }
            Array.Clear(stag37FloeDown, 0, n);
            Array.Clear(stag37FloeTell, 0, n);
            Array.Clear(stag37FloeDownLeft, 0, n);
            Stag37CanalLayer(false);
            Stag37FloesLayer();
            stag37CutLayer.SetBoxes(new List<(Vec3, Vec3)>());
            world.SetLayer("stag37Cuts", stag37CutLayer, true);
            Vec3 c = stag37Centre.Value;
            Stag37Arena a = stag37Arena;
            double rf = a.OuterR + 1, h = a.Half;
            stag37Surf = new Surfaces26(SurfaceRules.From(cfg));
            stag37Surf.zones.Add(new SurfaceZone { kind = SurfaceKind.Ice, x0 = c.x - rf, x1 = c.x + rf, z0 = c.z - h - rf, z1 = c.z + h + rf + 1.5, minY = c.y - 1, maxY = c.y + 0.5 });
            stag37Surf.Reset(mover);
            if (!stag37GlideHooked)
            {
                stag37GlideHooked = true;
                Func<Vec3, bool> prev = glideIceAt30;
                glideIceAt30 = feet =>
                {
                    if (stag37Surf != null && stag37Centre != null)
                        foreach (SurfaceZone z in stag37Surf.zones) if (z.kind == SurfaceKind.Ice && z.Contains(feet.x, feet.y, feet.z)) return true;
                    return prev != null && prev(feet);
                };
            }
        }

        void Stag37CanalLayer(bool slab)
        {
            stag37Slab = slab;
            stag37Arena.FillLayer(stag37CanalLayer, stag37Centre.Value, slab);
            world.SetLayer("stag37Canal", stag37CanalLayer, true);
        }

        void Stag37FloesLayer()
        {
            stag37Arena.FillFloes(stag37FloeLayer, stag37Centre.Value, stag37FloeDown);
            world.SetLayer("stag37Floes", stag37FloeLayer, true);
        }

        /// <summary>The campaign room row's stag37.centre in the level JSON (builder S), or null.</summary>
        Vec3? Stag37RowCentre()
        {
            if (stag37CentreOverride.HasValue) return stag37CentreOverride;
            if (map?.json == null || !map.json.ContainsKey("campaign")) return null;
            var camp = PortData.Obj(map.json["campaign"]);
            if (!camp.ContainsKey("rooms")) return null;
            foreach (object ro in PortData.Arr(camp["rooms"]))
            {
                var r = PortData.Obj(ro);
                if (!r.ContainsKey("stag37")) continue;
                var row = PortData.Obj(r["stag37"]);
                if (!row.ContainsKey("centre")) continue;
                return PortData.V3(row["centre"]);
            }
            return null;
        }

        /// <summary>The player's feet in canal-local coordinates (the arena's origin at the centre).</summary>
        public Vec3 Stag37Local(Vec3 w) => stag37Centre == null ? w : w - stag37Centre.Value;

        /// <summary>Is a world point inside the canal's footprint (the ice's rect)?</summary>
        public bool Stag37InFootprint(Vec3 w)
        {
            if (stag37Centre == null || stag37Arena == null) return false;
            Vec3 l = w - stag37Centre.Value;
            double rf = stag37Arena.OuterR + 1, h = stag37Arena.Half;
            return l.x >= -rf && l.x <= rf && l.z >= -h - rf && l.z <= h + rf + 1.5;
        }

        partial void N37StagStep(double dt)
        {
            if (stag37Centre != null && stag37Surf != null && n26.surfaces && Stag37InFootprint(mover.position)) stag37Surf.Step(mover);
            if (n37.stag37) N37StagStepFight(dt);
        }

        partial void N37StagStepFight(double dt);
        partial void N37StagPacerStartRun();
    }
}
