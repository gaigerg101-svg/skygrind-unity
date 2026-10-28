# FORGE64 code scout (read-only Explore agent, 2026-10-23 ~08:40; the lead wrote its reply here unchanged in substance)

It read the repo, the Player logs, the build configs and ten frames; nothing was edited. Every finding is file:line; "not determined" marks
what it could not settle.

## 1. The danger-red line on spire's exit (slate C4)
- **What draws it:** THE CROWN's goal column. Route47View is built when the `crownLook47` row is on (on in the repo and in
  Builds/n63-freeze, Design/unity-config.json:7885-7887) and the map has campaign lights whose id contains `-crown47` (only spire has them).
  - Route47View.cs:113-135 builds a 300 m cylinder (Route47View.cs:126-128) on the shader Assets/Shaders/Resources/Column47.shader.
  - That shader has a second pass with `ZTest Greater` (Column47.shader:98-106), so the column still shows through walls and roofs at
    `_Through` strength.
- **The orange bar above the door** is the same file's window shaft (Route47View.cs:101-106), in front of the light `v1-crown47` at
  [-10, 6.5, 87.05], whose note reads "the light over the way out; on the exit wall over the door" (Design/levels/n52-spire.json:1).
- **Colour:** `CLook47RedGoldHex = 0xff8a3a` (UnityTuning.Crown47.cs:50); the column at that colour x `Route47ColumnGain` 2.2 x a 0.85-1.0
  pulse at 0.4 Hz x max(0.3, effects setting) (Route47View.cs:56-61); through walls 0.35 of that (`Route47ColumnThrough`,
  UnityTuning.Crown47.cs:69). The window shafts at gain 0.9 to 2.4 (Route47View.cs:65, UnityTuning.Crown47.cs:65).
- **Where it stands:** web x -10, z -289, y 10 to 300, 3 m wide (UnityTuning.Crown47.cs:64, :69).
- **Why it runs down every exit:** every spire door spans x -12 to -8 (centre x -10, n52-spire.json:1, doors d1-d10); the verbs37 camera
  stands at x -10 (Logs/feel/n63-freeze/vp/verbs37/telemetry.jsonl, frames 100-1000); seen from about 400 m the column sits dead centre,
  from the horizon at the reticle to the top of the frame, or to the bottom when the camera pitches up 34 deg (f1650).
- **When it draws:** always while the row is on; not tied to door or fight state.
- **Not the cause:** Exit58, ExitFrame61 and Exit63 build only on n57-kiln (Exit58.N58V.cs:23, ExitFrame61.N61V.cs:32, Exit63.N63V.cs:33 and
  :143); lamp60 is a reel-only option (FeelReel.N60.Lamp.cs:25).
- **What it marks:** where THE CROWN is (the goal), not something the player uses, and it does not hurt. The colour table already decides it:
  `goal47` goes to the boundary role (pale violet-white), Design/unity-config.json:10115-10116 and Route47View.cs:58; but that table (the
  `colours63` row) is off (Design/unity-config.json:10065-10067). The window shafts are "way on" route lights: cyan could be argued for them,
  the owner's call.

## 2. The doubled boss order (slate U1)
- **Small copy:** the boss bar's hint `b43Hint`, 18 px, top right under the bar (UiBossBar43.cs:105; `Bar43HintPx`, UnityTuning.Ui43.cs:108).
- **Large copy:** hud47's fight line `fight47`, 40 px, centred 250 px from the top (UiHud47.Boss.cs:36-56; UnityTuning.F47.cs:24-26).
- **Config keys (one string feeds both):** the wyrm `design.n43.wick43.bosses.wyrmStagger` (Design/unity-config.json:6829; read at
  SurvivalSim.N43.Bosses.cs:41, picked at :81); the Loom and the Crown `design.n43.wick43.bosses.window` (:6815; read at :34, picked at :99);
  THE WICK's own `design.n43.wick43.hints.window` (:6800; SurvivalSim.N43.Wick.cs:50).
- **When each shows:** each time the hint changes, UiBossBar43.cs:209-215 calls `b43Hint.Set(shown)` and then `Fight47Post(shown)`.
  - hud47 hides the bar's hint label once, when the bar is built (UiHud47.Boss.cs:30), but `Text33.Set` turns it back on for every new text
    (UiStyle33.cs:77-78); it is held hidden only while bossRule61's opening rule line stands (UiBossRule61.cs:130-133; back on at :148-150).
  - The fight line shows when no card is up and at least 1.0 s after the last line (UiHud47.Boss.cs:146; UnityTuning.F47.cs:27); it holds
    2.0 s + 0.3 s a word, fades over 0.25 s, then stays gone until the hint changes (UiHud47.Boss.cs:156, :166-170; UnityTuning.F47.cs:32).
  - So both copies are on screen for about 3.2-3.8 s after each change, the small copy alone after that (matches the wyrm-tell shot).
- **No "one line at a time" rule covers them:** text61 / text63 only filter the centre teach card (UiText61.cs:3-5) and pass everything while
  a boss bar is up (`if (firstLevel || bossBar) return line;`, Text61.cs:115, :209; bossBar set at UiText61.cs:68); hud47's own one-line rule
  covers only its queue (UiHud47.Boss.cs:62-64) and assumes the hint has left the bar, which Text33.Set undoes.

## 3. Boss readability in the boss63 reel (not a slate slice tonight: DECIDE64 D-5)
- **The reel's pose:** the player respawns at the boss room's spawn (FeelReel.N63V.cs:72), walks forward up to 4 s only if the fight has
  not begun (:84-88), then stands still in God mode; every frame it aims at the weak point, or at the boss's centre during the reveal (:140);
  the aim does not check walls (FeelReel.cs:156-160). Weak points: the wyrm's head or open core (BossAttackClock63.cs:97-99), the Loom's live
  knee or spinneret (:109-113), the Crown's ring top blade (:123-124). The tell shot is builder G's (FeelReel.N63G.cs:91-98).
- **Logged distances** (eye to the boss's centre, flinch and stagger only; every stagger forced at 14017 ms): the wyrm 29.4 / 13.2 m
  (n47-reel-n63-b12-bk.log:153), the Crown 6.6 / 8.0 m (bp.log:189), the Loom 25.4 / 21.4 m (bs.log:152). Idle / windup / tell distances:
  not determined.
- **THE CROWN: mostly the game's own look.** The ring is 6 m across (Design/unity-config.json:5036) and hovers 5 m from the player and 1 m above
  the chest (:5046, :5048); at 6.6-8 m it fills most of the 74 deg view, and the Crown has no body besides the ring. The reel aims at the top
  blade, which puts the ring in the lower half under the TORCH. The tell's red cylinder is bossTell63's column (BossTell63.cs:4-6).
- **THE KILN WYRM: split.** idle and windup: the reel's pose (about 29 m, the head a small box at the reticle); stagger: the game's look
  (BossMarks32's open-window shell, 1.45x the head box in cyan-white, BossMarks32.cs:23, :25, :177, tinted lime by grade54; D-B3c.md:45-49);
  tell: the reel's pose (aimed at the head through a wall).
- **THE LOOM: mostly the game's own look.** The cyan sphere is the live knee's glow, #36E0FF x 2.2 (Loom37View.cs:6, :221;
  UnityTuning.Loom37.cs:17-18); in the stagger shot most likely the spinneret's window glow (radius 1.6 m, Loom37View.cs:7, :91;
  UnityTuning.Loom37.cs:33): which renderer fills the frame is not determined. The "wire shape" is the body seen at 21-25 m.

## 4. THE KILN WYRM's weak point under the boss grade (slate B3)
- **Where it stands:** NIGHT63's fix round hooked `wyrm.open` (WyrmView.cs:163), `wyrm.punch` (:161), `wyrm.core` (:181) and BossMarks32's
  open shell `wyrm.mark` (BossMarks32.cs:177), all through BossColours63.N63D.cs:102-124 into `BossGrade54.RimAt` (BossGrade54.cs:237-246).
  RimAt pre-divides only phase 1's colour filter (BossGrade54.cs:215-217); the sulphur highlights still pull the colour from about 185 deg to
  158 deg (D-B3c.md:166-172; Logs/n63/verdict-B3.md). The wyrm's grade: rim [0.08, 0.86, 1] x 2.2 (Design/unity-config.json:8689, :8691);
  phase 1 filter [0.95, 0.96, 0.78] and highlights [0.54, 0.76, 0.3] (:8694, :8696). bossColours63 is off (:10018-10020).
- **Fix options:** (1) shift the weak point's own input bluer by about 25-30 deg so it lands near 185 deg (a split-toning offset inside RimAt,
  or a separate weak-rim colour in the bossColours63 table, :10018-10064; BossColours63.N63D.cs:102-124; BossColours63Math.N63D.cs); changing
  grade54's own wyrm rim (:8689) would also move the player's effects that Rim() recolours (BossGrade54.cs:224-232): a grade change, GG's.
  (2) keep the head's brightness under the toning's reach (the head at x1 instead of Wyrm47PunchGain 2.2, UnityTuning.Brawl47.cs:49; a dimmer
  shell, BossMarks32.cs:23, :177): a dimmer weak point. (3) a cyan exception in grade54's split toning (BossGrade54.cs:312-315, :391-393; the
  wyrm's highlights :8696, :8711, :8726): a custom pass or a milder palette: **the owner's call** (grade54 is GG's NIGHT54 design). (4) use the
  live phase's filter in the pre-division (it accounts only for phase 1, BossGrade54.cs:216, while phases 2 and 3 cut blue to 0.74 and 0.70,
  :8709, :8724): a fix inside NIGHT63's own RimAt code. (5) the punch-window shell `Tint(headR, punchC * pulse)` at Boss30View.cs:482 is not
  routed at all: send it through the same wrapper.
- **To judge it:** the same-run pair with Tools/night/n63-b3-roles.py (D-B3c.md:177); hue 158 against the 160-205 player band is the mark.
- **Not determined:** the camera distances at the idle, windup and tell shots; which renderer is the Loom's big sphere in the stagger shot;
  whether D-B3c.md's split-toning arithmetic matches the code.
