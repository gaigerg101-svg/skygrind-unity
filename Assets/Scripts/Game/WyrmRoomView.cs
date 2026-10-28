// NIGHT25 builder W (bug-void, GG: "the arena is bugged out"; then every campaign boss room): a boss room as the view draws it. NIGHT24 registered the worm's room
// (Design/maps/wyrm-arena.json) and MONOLITH's as collision layers only, so a campaign player teleported to THE FURNACE (far from the map) stood on an invisible
// floor under the sky (Research/2026-10-08-wave25/WYRM-TRIAGE.md). This builds each room's boxes once, on the room's first fight, as one stone mesh (the map builder's
// colours and MeshKit, SurvivalBuilder's way) and one point light in the pale violet-white boundary tone (rule 8), both active only while that room's boss fights:
// the worm's, THE BULWARK's (design.n25.bosses) and MONOLITH's. A view fix: nothing on the sim path. Numbers: design.n25.wyrmAim.room / design.n25.bosses.<id>.room
// (Unity-only, each with a _why).
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class WyrmRoomView
    {
        sealed class Room
        {
            public string id, cfgPath;
            public GameObject root;
            public int boxes;
            public bool shown;
        }

        readonly SurvivalGame game;
        readonly Room[] rooms =
        {
            new Room { id = "wyrm", cfgPath = "design.n25.wyrmAim.room" },
            new Room { id = "bulwark", cfgPath = "design.n25.bosses.bulwark.room" },
            new Room { id = "monolith", cfgPath = "design.n25.bosses.monolith.room" },
            new Room { id = "final", cfgPath = "design.n25.bosses.final.room" },
            new Room { id = "rival", cfgPath = "design.n25.bosses.rival.room" },
            new Room { id = "hulk", cfgPath = "design.n26.bosses.hulk.room" }, // NIGHT26 B: THE SLAG HULK's room
        };

        /// <summary>Some room is drawn now (a boss fights in it): what the reel's scenarios read.</summary>
        public bool Shown
        {
            get
            {
                foreach (Room r in rooms) if (r.shown) return true;
                return false;
            }
        }

        /// <summary>The number of boxes in the worm's room mesh (0 until built).</summary>
        public int Boxes => BoxesFor("wyrm");

        /// <summary>Hide every room while its boss fights (the reel's void-versus-room pixel check only; never set by the game).</summary>
        public bool Suppress;

        public WyrmRoomView(SurvivalGame game) => this.game = game;

        Room Find(string id)
        {
            foreach (Room r in rooms) if (r.id == id) return r;
            return null;
        }

        public bool ShownFor(string id) => Find(id)?.shown ?? false;
        public int BoxesFor(string id) => Find(id)?.boxes ?? 0;

        void Build(Room room, WyrmArena arena, Vec3 o)
        {
            GameConfig cfg = game.cfg;
            Color top = Look.C(cfg, "colors.pillarTop"), side = Look.C(cfg, "colors.pillarSide");
            var kit = new MeshKit();
            // NIGHT31 B (design.n31.wyrm31): the crumbling platforms are Boss31View's (drawn, then hidden when they fall), so the worm's room mesh leaves them out
            SurvivalSim s31 = game.sim;
            bool skip31 = s31 != null && s31.n31.wyrm31 && arena == s31.wyrm24Arena && s31.wyrm31CrumbleBoxes.Count > 0;
            int bi = 0;
            foreach (var (min, max, kind) in arena.boxes)
            {
                if (!(skip31 && s31.wyrm31CrumbleBoxes.Contains(bi))) kit.Box(min + o, max + o, top, side);
                bi++;
            }
            room.boxes = arena.boxes.Count;
            room.root = new GameObject("BOSS room " + room.id);
            MeshKit.Object("Room stone", room.root.transform, kit.Build("boss room " + room.id), game.mats.stone, shadows: false);
            var lg = new GameObject("Room light");
            lg.transform.SetParent(room.root.transform, false);
            var light = lg.AddComponent<Light>();
            light.type = LightType.Point;
            light.shadows = LightShadows.None;
            light.range = (float)cfg.Num(room.cfgPath + ".lightRange");
            light.intensity = (float)cfg.Num(room.cfgPath + ".lightIntensity") * UnityTuning.LightScale;
            light.color = Look.Hex((uint)cfg.Num(room.cfgPath + ".lightColor"));
            lg.transform.position = new Vector3((float)o.x, (float)(o.y + cfg.Num(room.cfgPath + ".lightHeight")), (float)-o.z); // web z is Unity -z
            room.root.SetActive(false);
            Kiln73.OnBuild(cfg, room.id, top, side, room.boxes); // NIGHT73 K (kiln73): one proof line per arena build (the worm's room only)
        }

        /// <summary>Every render frame: each room on while its boss fights, off otherwise (built on its first fight).</summary>
        public void Render()
        {
            SurvivalSim sim = game.sim;
            if (sim == null) return;
            foreach (Room r in rooms)
            {
                IBossSystem boss = sim.BossRoomSystem(r.id);
                WyrmArena arena = sim.BossRoomArena(r.id, out Vec3 o);
                // NIGHT46 V1 (B1's walkin46 hand-off): the worm's room on the level's own k5 draws from the door, before its fight (sim.Wyrm46ArenaHere)
                bool on = ((boss != null && boss.Alive) || (r.id == "wyrm" && sim.Wyrm46ArenaHere)) && arena != null && !Suppress;
                if (r.id == "hulk" && on && sim.n27.boss1) on = false; // NIGHT27 B: with boss1 on the plan's boss is THE LANTERN: LanternView draws its own courtyard (its segments retract)
                if (r.id == "bulwark" && on && !sim.BossRoomsOn) on = false; // the BULWARK walks the Grinder in a SURVIVAL boss round: no room
                if (on && r.root == null) Build(r, arena, o);
                if (r.root == null) continue;
                if (r.root.activeSelf != on) r.root.SetActive(on);
                r.shown = on;
            }
            Kiln73.Tick(game, ShownFor("wyrm")); // NIGHT73 K (kiln73): the row read every frame; the worm's grades and the EXPOSED edge follow it
        }
    }
}
