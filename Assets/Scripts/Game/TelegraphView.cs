// The spawn telegraph beams (enemies/spawner.js render): a red-orange column (spawner.beamRadius x beamHeight) at each
// squad's point while it telegraphs, pulsing (beamPulseRate) and widening (beamGrow) until the group arrives.
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed class TelegraphView
    {
        readonly SurvivalGame game;
        readonly List<Transform> beams = new List<Transform>();
        // NIGHT16 polish #45: each beam pulses on its own age: its colour in its own MaterialPropertyBlock (made with the beam,
        // reused every frame), where one SetColor on the shared material gave every beam the last beam's pulse
        readonly List<Renderer> beamRenderers = new List<Renderer>();
        readonly List<MaterialPropertyBlock> beamBlocks = new List<MaterialPropertyBlock>();
        static readonly int ColorId = Shader.PropertyToID("_Color");
        readonly Material mat;
        readonly float radius, height, opacity, pulse, grow;
        readonly Color enemy;

        public TelegraphView(SurvivalGame game)
        {
            this.game = game;
            GameConfig cfg = game.cfg;
            radius = (float)cfg.Num("spawner.beamRadius");
            height = (float)cfg.Num("spawner.beamHeight");
            opacity = (float)cfg.Num("spawner.beamOpacity");
            pulse = (float)cfg.Num("spawner.beamPulseRate");
            grow = (float)cfg.Num("spawner.beamGrow");
            enemy = Look.C(cfg, "colors.enemy");
            mat = new Material(game.mats.field);
            mat.SetFloat("_FarAlpha", 1);
            mat.SetFloat("_FadeTop", height);
            mat.SetFloat("_FadeLength", height);
            mat.SetFloat("_Stripe", 1000); // a plain sheet
        }

        public void Render(double frameDt)
        {
            int n = 0;
            foreach (Telegraph t in game.director.telegraphs)
            {
                if (n >= beams.Count)
                {
                    var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    Object.Destroy(go.GetComponent<Collider>());
                    var r = go.GetComponent<Renderer>();
                    r.sharedMaterial = mat;
                    r.shadowCastingMode = ShadowCastingMode.Off;
                    beams.Add(go.transform);
                    beamRenderers.Add(r);
                    beamBlocks.Add(new MaterialPropertyBlock());
                }
                int bi = n;
                Transform b = beams[n++];
                b.gameObject.SetActive(true);
                t.age += frameDt;
                float w = radius * 2 * (1 + (float)t.age * grow);
                b.position = WebSpace.ToUnity(t.point) + new Vector3(0, height / 2, 0);
                b.localScale = new Vector3(w, height / 2, w);
                MaterialPropertyBlock block = beamBlocks[bi];
                block.SetColor(ColorId, enemy * opacity * (1 + Mathf.Abs(Mathf.Sin((float)t.age * Mathf.PI * pulse))) * 2);
                beamRenderers[bi].SetPropertyBlock(block);
            }
            for (int i = n; i < beams.Count; i++) beams[i].gameObject.SetActive(false);
        }
    }
}
