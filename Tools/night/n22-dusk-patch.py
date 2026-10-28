import io,sys
def patch(path, pairs):
    s = io.open(path,'r',encoding='utf-8',newline='').read()
    crlf = chr(13)+chr(10) in s
    for old,new in pairs:
        if crlf: old=old.replace(chr(10),chr(13)+chr(10)); new=new.replace(chr(10),chr(13)+chr(10))
        if s.count(old)!=1: print('FAIL',path,s.count(old),old[:60]); sys.exit(1)
        s = s.replace(old,new)
    io.open(path,'w',encoding='utf-8',newline='').write(s); print('ok',path)
patch('Assets/Scripts/Game/HuntSky20.cs',[
("        readonly Material sky;\n        readonly Color zenith, mid, horizon, fog, danger;\n        double u, target;\n        float shown = -1;\n",
 "        readonly Material sky;\n        readonly SurvivalGame game; // NIGHT22: the dusk clock's bases (DuskClock22) when the row is on\n        readonly Color zenith, mid, horizon, fog, danger;\n        double u, target;\n        float shown = -1;\n        int shownVersion = -1;\n"),
("            GameConfig cfg = game.cfg;\n            sky = game.mats != null ? game.mats.sky : null;\n",
 "            GameConfig cfg = game.cfg;\n            this.game = game;\n            sky = game.mats != null ? game.mats.sky : null;\n"),
("            float step = Mathf.Round((float)share * 255) / 255;\n            if (step == shown) return;\n            shown = step;\n            Color z = HuntSkyMath.Lean(zenith, danger, step, UnityTuning.HuntZenith);\n            Color m = HuntSkyMath.Lean(mid, danger, step, UnityTuning.HuntMid);\n            Color h = HuntSkyMath.Lean(horizon, danger, step, UnityTuning.HuntHorizon);\n",
 "            float step = Mathf.Round((float)share * 255) / 255;\n            // NIGHT22: with the DUSK CLOCK on, the shift leans from the clock's current bases, not the shipped dusk; a base that moved\n            // (DuskClock22.Version) re-applies even when the hunt's share stood still.\n            bool clock = DuskClock22.Instance != null && game.sim != null && game.sim.n22.duskClock;\n            int ver = clock ? DuskClock22.Version : 0;\n            if (step == shown && ver == shownVersion) return;\n            shown = step;\n            shownVersion = ver;\n            Color zb = clock ? DuskClock22.Instance.Zenith : zenith, mb = clock ? DuskClock22.Instance.Mid : mid;\n            Color hb = clock ? DuskClock22.Instance.Horizon : horizon, fb = clock ? DuskClock22.Instance.Fog : fog;\n            Color z = HuntSkyMath.Lean(zb, danger, step, UnityTuning.HuntZenith);\n            Color m = HuntSkyMath.Lean(mb, danger, step, UnityTuning.HuntMid);\n            Color h = HuntSkyMath.Lean(hb, danger, step, UnityTuning.HuntHorizon);\n"),
("            RenderSettings.fogColor = HuntSkyMath.Lean(fog, danger, step, UnityTuning.HuntFog);",
 "            RenderSettings.fogColor = HuntSkyMath.Lean(fb, danger, step, UnityTuning.HuntFog);"),
])
patch('Assets/Scripts/Game/SurvivalGame.N22.cs',[
("        /// <summary>Once at game init, after the NIGHT21 views: build a NIGHT22 world view (Haptics22, DuskClock22, the DAILY ghost's marker).</summary>\n        partial void N22InitViews();\n",
 "        DuskClock22 duskClock22; // NIGHT22 THE DUSK CLOCK (the lead; Game/DuskClock22.cs)\n\n        /// <summary>The dusk clock's share now (the reel's read; 0 with the row off).</summary>\n        public double DuskShare => duskClock22 != null ? duskClock22.Share : 0;\n\n        /// <summary>Once at game init, after the NIGHT21 views: the NIGHT22 world views. Builders add their own view in a one-line call here (the lead wires it).</summary>\n        partial void N22InitViews()\n        {\n            duskClock22 = new DuskClock22(this);\n        }\n"),
("        partial void N22RenderViews(double worldDt, double alpha);\n",
 "        partial void N22RenderViews(double worldDt, double alpha)\n        {\n            // the dusk clock moves on frame time (a slow-mo doesn't slow the sky), once per render frame\n            duskClock22?.Render(RealFrameDt20(), flow == Flow.Title, run != null ? run.round : 1);\n        }\n"),
])
