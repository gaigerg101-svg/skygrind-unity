import io,sys
def patch(path, pairs):
    s = io.open(path,'r',encoding='utf-8',newline='').read()
    crlf = chr(13)+chr(10) in s
    for old,new in pairs:
        if crlf: old=old.replace(chr(10),chr(13)+chr(10)); new=new.replace(chr(10),chr(13)+chr(10))
        if s.count(old)!=1: print('FAIL',path,s.count(old),old[:60]); sys.exit(1)
        s = s.replace(old,new)
    io.open(path,'w',encoding='utf-8',newline='').write(s); print('ok',path)
G='Assets/Scripts/Game/'
patch(G+'Ui.cs',[
("            BuildFeelHud(); // NIGHT16 (the shared commit)","            BuildN22Hud(); // NIGHT22 (the shared commit): the NIGHT22 HUD widgets, each builder's own partial file (the ARCLINE chip, the DEADLINE bar, the stamp, ...)\n            BuildFeelHud(); // NIGHT16 (the shared commit)"),
("        partial void BuildFeelHud();\n","        partial void BuildFeelHud();\n        partial void BuildN22Hud();\n        partial void RenderN22Hud(double frameDt);\n"),
("            RenderFeelHud(frameDt); // NIGHT16 (the shared commit)","            RenderN22Hud(frameDt); // NIGHT22 (the shared commit): see BuildN22Hud\n            RenderFeelHud(frameDt); // NIGHT16 (the shared commit)"),
])
patch(G+'SurvivalGame.Init.cs',[
("            arclineView = new Arcline21View(this); // NIGHT21 U3-35.3 (builder M)\n","            arclineView = new Arcline21View(this); // NIGHT21 U3-35.3 (builder M)\n            N22InitViews(); // NIGHT22 (the shared commit): the NIGHT22 world views (SurvivalGame.N22.cs hooks)\n")])
patch(G+'SurvivalGame.Render.cs',[
("                arclineView?.Render(worldDt, alpha); // NIGHT21 U3-35.3 (builder M)\n","                arclineView?.Render(worldDt, alpha); // NIGHT21 U3-35.3 (builder M)\n                N22RenderViews(worldDt, alpha); // NIGHT22 (the shared commit)\n")])
