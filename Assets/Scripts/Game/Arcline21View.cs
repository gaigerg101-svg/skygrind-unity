// NIGHT21 U3-35.3 (builder M): THE ARCLINE's look (the rule is Core/Arcline21.cs, the run Game/SurvivalSim.Arcline21.cs).
// UNCOMPILED and UNSEEN: written on a night the Unity Editor was stuck on a dialog, from the APIs read in Beacon20View /
// WeaponFx / RunShapeDesignViews; the lead wires it (SurvivalGame.Init: `arclineView = new Arcline21View(this);`, and
// SurvivalGame.Render next to beaconView: `arclineView?.Render(worldDt, alpha);`). Draw only, no audio, no new shaders:
// the materials are the game's neon (an additive copy for the rope), the colours CLAUDE.md rule 8's cyan (player-usable).
// - The rope: a cyan line from the chest to the hook (flying) or the anchor (attached), the web's ropeColor / ropeWidth.
// - The node glows: a cyan cube at each of the map's grapple nodes while the gadget is owned, pulsing; brighter when the
//   crosshair has a node in range (Arcline21.Reticle: the hook reticle's "something to hook").
// - The terminal: a dark cabinet at data.gadgetSpots with a cyan hook on its face and a glow ring that dims once owned
//   (gadgets.js's cabinet, in blocks).
// NOT done here (to wire by the lead / a later pass): the HUD hook reticle in red-orange when nothing is in range, and the
// gadget chip (the yank's 15 s cooldown share is Arcline21.YankShare, the owned flag Arcline21.Owned).
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed partial class Arcline21View
    {
        readonly SurvivalGame game;
        readonly LineRenderer rope;
        readonly Material ropeMat;
        readonly Renderer nodes, ring;
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        readonly Color ropeColor, nodeColor;
        readonly float nodeGlow;
        float time;
        bool nodesShown;

        public Arcline21View(SurvivalGame game)
        {
            this.game = game;
            GameConfig cfg = game.cfg;
            ropeColor = Look.Hex((uint)cfg.Num("arcline.ropeColor"));
            if (Look52.On(cfg)) ropeColor = Look52.Pale(ropeColor); // NIGHT52 U look52 (Look52.cs): the rope and its hook pale cyan
            Look52.RopeColor = ropeColor;
            nodeColor = Look.Hex((uint)cfg.Num("arcline.nodeColor"));
            nodeGlow = (float)cfg.Num("arcline.nodeGlow");
            var root = new GameObject("Arcline").transform;

            ropeMat = FxDraw.Additive(game.mats.neon, "Arcline rope");
            ropeMat.SetColor(DesignKit.BaseColor, ropeColor);
            rope = new GameObject("Arcline rope").AddComponent<LineRenderer>();
            rope.transform.SetParent(root, false);
            rope.useWorldSpace = true;
            rope.alignment = LineAlignment.View;
            rope.positionCount = 2;
            rope.widthMultiplier = (float)cfg.Num("arcline.ropeWidth") * UnityTuning.Arcline21RopeWidthMul;
            rope.sharedMaterial = ropeMat;
            rope.shadowCastingMode = ShadowCastingMode.Off;
            rope.receiveShadows = false;
            rope.enabled = false;
            Rope33Build(root, cfg); // NIGHT33 W (view33): the whip's rope (Arcline21View.Rope33.cs)

            // The node glows: one mesh of a cube per grapple node (white vertices; the colour comes from the property block).
            var nodeKit = new MeshKit();
            float edge = UnityTuning.Arcline21NodeCube * (float)cfg.Num("arcline.nodeRadius");
            foreach (Vec3 n in game.sim.map.grappleNodes) nodeKit.Cube(WebSpace.ToUnity(n), new Vector3(edge, edge, edge), Color.white);
            nodes = DesignKit.Neon("arcline nodes", root, nodeKit, game);
            nodes.gameObject.SetActive(false);

            // The terminal (gadgets.js's cabinet, blocks): where data.gadgetSpots puts it, facing out by its yaw.
            PerkSpot? spot = game.sim.Arcline21Spot();
            if (spot.HasValue)
            {
                var cab = new GameObject("Arcline terminal").transform;
                cab.SetParent(root, false);
                cab.position = WebSpace.ToUnity(spot.Value.pos);
                cab.rotation = Quaternion.LookRotation(WebSpace.ToUnity(new Vec3(JsMath.Sin(spot.Value.yaw), 0, JsMath.Cos(spot.Value.yaw))));
                float w = UnityTuning.Arcline21CabinetWidth, d = UnityTuning.Arcline21CabinetDepth, h = UnityTuning.Arcline21CabinetHeight, k = UnityTuning.Arcline21HookSize;
                var body = new MeshKit();
                body.Cube(new Vector3(0, h * 0.5f, 0), new Vector3(w, h, d), DesignKit.Body);
                MeshKit.Object("arcline cabinet", cab, body.Build("arcline cabinet"), game.mats.stone, shadows: true);
                var hook = new MeshKit(); // the hook on the face: a stem and a curl
                float fz = d * 0.5f + 0.01f;
                hook.Cube(new Vector3(0, h * 0.62f, fz), new Vector3(k * 0.18f, k, 0.02f), Color.white);
                hook.Cube(new Vector3(-k * 0.3f, h * 0.62f - k * 0.5f, fz), new Vector3(k * 0.6f, k * 0.18f, 0.02f), Color.white);
                hook.Cube(new Vector3(-k * 0.55f, h * 0.62f - k * 0.3f, fz), new Vector3(k * 0.18f, k * 0.45f, 0.02f), Color.white);
                DesignKit.Neon("arcline hook", cab, hook, game);
                var rk = new MeshKit(); // the floor ring
                rk.Cube(new Vector3(0, 0.03f, 0), new Vector3(w + 0.5f, 0.03f, d + 0.5f), Color.white);
                ring = DesignKit.Neon("arcline ring", cab, rk, game);
            }
        }

        /// <summary>NIGHT31 P0 b (builder G, design.n31.arcRing, a view row; T-34: "the node's old cube hides half of the new ring"): in a CAMPAIGN map whose
        /// nodes GrappleNodes30 draws as rings (it drew some last frame), the cubes are not drawn. Off: the row off, -skygrindOff31, or no rings drawn.</summary>
        public static bool CubesHidden31Now;
        bool? arcRing31;

        bool CubesHidden31()
        {
            arcRing31 ??= !N31Blocks.ForceOff && game.cfg.Design("n31.arcRing.enabled");
            bool campaignMap = game.map != null && game.map.json != null && game.map.json.ContainsKey("campaign");
            CubesHidden31Now = arcRing31.Value && campaignMap && GrappleNodes30.RingsShown > 0;
            return CubesHidden31Now;
        }

        /// <summary>Once a frame, on the world's time (0 in hitstop). `alpha`: the fixed step's render interpolation.</summary>
        public void Render(double worldDt, double alpha)
        {
            time += (float)worldDt;
            SurvivalSim sim = game.sim;
            Arcline21 a = sim.arcline21;
            bool on = a != null && sim.n21.arcline;
            Rope r = sim.mover.rope;
            bool roped = on && r.phase != RopePhase.None;
            if (view33) Rope33Render(sim, r, roped, alpha, worldDt); // NIGHT33 W: the sagging rope from the muzzle, the hook head, the snap
            if (view33) Grapple47Render(sim, r, roped, worldDt); // NIGHT47 F grapple47 (Arcline21View.Grapple47.cs): the glowing ribbon (off: nothing changes)
            else rope.enabled = roped;
            if (roped && !view33)
            {
                Vec3 feet = sim.previous + (sim.current - sim.previous) * alpha;
                var chest = new Vec3(feet.x, feet.y + game.cfg.movement.arcline.ropeHeight, feet.z);
                rope.SetPosition(0, WebSpace.ToUnity(chest));
                rope.SetPosition(1, WebSpace.ToUnity(r.phase == RopePhase.Flying ? r.hook : r.anchor));
            }
            bool showNodes = on && a.Owned && !CubesHidden31();
            if (showNodes != nodesShown)
            {
                nodesShown = showNodes;
                nodes.gameObject.SetActive(showNodes);
            }
            if (showNodes)
            {
                var look = new InputFrame { yaw = sim.input.Yaw, pitch = sim.input.Pitch };
                bool aimed = r.phase == RopePhase.None && a.Reticle(look) == Arcline21Kind.Node;
                float pulse = UnityTuning.Arcline21NodeFloor + (1 - UnityTuning.Arcline21NodeFloor) * (0.5f + 0.5f * Mathf.Sin(time * Mathf.PI * 2 * UnityTuning.Arcline21NodePulseRate));
                DesignKit.Tint(nodes, block, nodeColor * (nodeGlow * pulse * (aimed ? UnityTuning.Arcline21NodeAimed : 1)));
            }
            if (ring != null)
            {
                float pulse = 0.75f + 0.25f * Mathf.Sin(time * Mathf.PI * 2 * UnityTuning.Arcline21NodePulseRate);
                float dim = on && a.Owned ? 0.3f : 1f; // gadgets.js: the ring dims once owned
                DesignKit.Tint(ring, block, nodeColor * (pulse * dim));
            }
        }
    }
}
