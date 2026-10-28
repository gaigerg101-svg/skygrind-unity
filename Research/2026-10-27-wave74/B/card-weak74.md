# B1 weak74 card (builder B, NIGHT74; rule W, FORGE74 D-4): on a boss, cyan means "shoot here" and nothing else

Row design.n74.weak74 (OFF in the repo; ON: `-skygrindRows72,weak74:1`). View only. Read from the code at t0b a354c0e94 (not run).

## Every cyan surface on THE WICK, THE ICEBREAKER, THE LOOM, THE CROWN (file:line in this tree)
| boss | site | when cyan today | weak74 ON |
|---|---|---|---|
| CROWN | Crown37View.Crown.cs:117 blades | window (open) | kept (the whole ring is the weak point) |
| CROWN | Crown37View.Crown.cs:118 blades | a pass's flash | kept (the hit's confirmation) |
| CROWN | Crown37View.Crown.cs:143 gems, :145 light | window / flash | kept |
| CROWN | Crown37View.Crown40.cs:84-86 blades, rim38, halo38 | idle, "while it may be passed" (crown40) | ivory (Crown37View.Weak74.cs readback) |
| CROWN | Crown37View.Crown40.cs:92 gem halos (0.4), :94 light | idle | ivory |
| CROWN | Crown37View.Crown72.cs:104-106 blades, rim38, halo38 | through a RED tell (crown72) | ivory |
| CROWN | Crown37View.Crown38.cs:133 gems, :153-161 gem crosses and halos, :166 halos | closed: bossColours63 "crown.gems" / "crown.halo" routed to cyan | ivory (role id74) |
| CROWN | Crown37View.Crown38.cs:164 rim, :166 halos | window | kept |
| CROWN | Crown37View.Look47.cs:122 edges | window | kept |
| CROWN | Crown37View.Crown40.cs:135 "OPEN: SHOOT IT" text | window | kept |
| CROWN | Crown37View.Crown.cs:181 a parried shard flying back | the player's projectile | kept (player-usable) |
| CROWN | Crown37View.cs:218 piece edges, :230-231 and Crown40.cs:145 ring targets (hoops) | arena footing and the level's rehearsal rings | kept (the player stands on / dashes through them; not the boss) |
| WICK | Wick37View.cs:414 idle core (BossColours63 "wick.core") | closed: routed to cyan | verdigris iron (role id74) |
| WICK | Wick37View.cs:407 plate glows, :410 core | cracked / window | kept |
| WICK | Wick37View.cs:142-225 and N47 / N48: ride trims, ledge lips, step lips, glass | the arena's ride trims | kept (the player rides them) |
| LOOM | Loom37View.cs:221 idle knee (BossColours63 "loom.knees"), :225 nails, :243 pips; N47.cs:166 seams | routed to cyan, open or closed | brass-silk, closed AND in its window (the weak point there is the spinneret) |
| LOOM | Loom37View.cs:221 the LIVE pair of knees (KneeLiveHex) | the leg's pair: the grapple node and the weak point | kept |
| LOOM | Loom37View.N40.cs:50 practice knee, :131 pull rings; N47.cs:68 a parried spool | tutorial knee, grapple cues, the player's projectile | kept |
| ICEBREAKER | Brawl40View.cs:305-310, N41.cs:91, N42.cs:127-133, N47.cs:32 (BossColours63 "brawl.weak") fists and rims | STUCK / dazed / reeling only (open) | nothing to change: it draws no cyan while closed |
| ICEBREAKER | Brawl40View.cs:154-173 ride trims on rims and pillars | arena | kept |

THE KILN WYRM is core73's (not touched).

## What it does
- bossColours63's closed weak parts (wick.core, crown.gems, crown.halo, loom.knees / nails / seams) take the boss's identity colour at the site's own luminance (BossColours63.N63D.cs hooks; Weak74.WeakRole). NIGHT71's cyan71 (bone, OFF) did this with one colour; this is one per boss.
- THE CROWN's ring is not in that table: after the frame's last tint a readback pass (Weak74.RecolourIfCyan) takes every ring part still reading cyan (hue 150-215, S >= 0.12) to ivory; skipped in the window and the pass flash.
- Identity colours (UnityTuning.N74B.cs, each in no role's arc; N74BWeakTests): WICK verdigris iron #A3BBA5 (hue 125), CROWN ivory #E8DFC8 (hue 43, the family of crown40's gem bone), LOOM brass-silk #CDBB95 (hue 41, S 0.27).
- Baseline on the NIGHT73 frames (Logs/feel/n73-freeze/bp/boss63, n74-cyan.py, gun and HUD masked): crown37-idle 6.38 % cyan of the frame, stagger 16.51 %, windup 9.11 %, flinch 9.57 %, tell 4.23 %.

## The proof (the lead's Player steps)
- `bash Tools/night/n74-reels.sh <tag> bpon,n52-spire,boss63,-skygrindRows72,weak74:1 bpoff,n52-spire,boss63` (one pair; add the level row flag of L-154 as the lead's sides do); for the other two bosses `wick37` on n37-foundry and `loom37` on n37-span (FeelReel.N27.cs PhaseN37W / N37O) with the same two flags.
- Read `Logs/feel/<tag>/<side>/boss63/boss63-crown37.txt`: the `[weak74] shot-N30-B63-crown37-idle.png | boss crown37 | row on|off | open no | weak box none | body box x0,y0,x1,y1 | ... | parts cyan N of M` lines. OFF reads N > 0 (the parts the row takes), ON reads 0.
- `python -I Tools/night/n74-cyan.py --pair on.png off.png --log-on <on txt> --log-off <off txt>`: the three doneWhen lines (outside the weak box down >= 70 %, inside within 10 %, danger not up by more than 2 points). `--selftest` is the planted case (PASS here).

## Not checked
Nothing run in Unity or a Player (no Editor, reel or suite): the colours are read from code and the numbers' maths; the EditMode test N74BWeakTests is written, not run; the ivory ring's look is not seen.
