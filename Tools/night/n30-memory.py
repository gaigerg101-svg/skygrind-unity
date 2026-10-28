# NIGHT30 (the lead): wave-memory harvest 30: the wave-30 ledger, new lessons, threads, features, a CORE rebuild. Writes LF bytes.
import io, re
ROOT = "C:/Users/gaige/dev/SKYGRIND-unity/docs/memory/"

def rd(p):
    return io.open(ROOT + p, encoding="utf-8").read().replace("\r\n", "\n")

def wr(p, s):
    io.open(ROOT + p, "wb").write(s.replace("\r\n", "\n").encode("utf-8"))

ledger = """# Wave 30: THE READABLE GAME (NIGHT30, 2026-10-09 19:33 to about 23:40; branch port/stage3, base c92efcb3, tag u3-n30)
**Ask:** GG, 2026-10-09 ~19:05 (Research/2026-10-09-wave30/GG-REQUEST.md, nine screenshots read in SCREENSHOTS.md): clear weapon rules + a charging / cooling / ready HUD, lava instead of pits, an explained circular Lantern, fewer enemies early with one tank moment, a PS2 / ULTRAKILL gritty HUD that buzzes, glide ice instead of sticky rails and a level-2 boss that punishes distance, a Wyrm head-punch stagger, rooms worth wall riding in, LEVEL 4 around one mechanic and one weapon.
**Shipped (sources: NIGHT30-LOG.md, NIGHT30-PROOF.md, NIGHT30-REPORT.md):** Rules30 (a switch waits only for the action; Player: switch after a heavy swing 2.51 s -> 0.36 s); HudWeapons30 (PUNCH / BLAST slots, READY / COOLING / CHARGING / SWITCHING; layout 3 -> 0 collisions); lava (Lava30: 25 %, knock, no teleport; every respawn pit is lava; new room f5b); THE LANTERN ring (16 panels, two lines, lead orbs, sweep, platform refuge with no damage; Player 69 s); Glide30 (dash on ice +56 % in the Player) + Glacier30 (44 x 70 m, patience gauge, shards, frost band; Player 101 s); wyrm30 head punch (2 staggers / 2 charges); LEVEL 4 THE SPAN (n30-span s1-s8, the ARCLINE in campaign tape exact, NAILGUN, THE LOOM with CUT + SWEEP; Player loom30 bar 1.00 -> 0.37, cut 1, sweeps 2); the look (HudFx30, quantise + Bayer dither, beams; +0.02 ms GPU); Teach30 (one boss-line budget; stale lines 5 -> 0 in the Player); P0 a-i all GREEN (LitMotion NREs 115 -> 0, smoke rows ON all passed 198 s).
**Numbers:** walk30 10 / 10 on all four levels (22:25); i1a 0 deaths (NIGHT29 i1 9), i6 6 (was 16); boss medians harness Lantern 70 s, Glacier 111 s, Wyrm 3:21, Loom 111 s (model); LevelSelect29 20 / 20 incl. n30-span; suite run 1 1867 / 1869 (2 fails explained, re-run 18 / 18); bench CPU 2.39 -> 2.46 ms median, GPU 0.62 -> 0.64.
**SHORT / open:** nobody played it; the Loom's kill time is a scripted model; no grapple ring seen in a frame; f9 husks spawn beside the entry; the low-health full-screen red is NIGHT18's PostFx; vertex snap / texture noise / heat shimmer not built; f5b's 6.0 m ledge unreachable (lane tops ~4.4 m).
**Prompt shape:** NIGHT30.md 185 lines, 6 phases, box 4.5 h (cap 5 h); design 0:03-0:21 (Chrome images + hand SVG + one 8-min reviewer: 5 findings, all fixed), shared commit at 0:17, builders at 0:21 (early); 4 builders (L, B, V, S), each resumed 5-8 times by SendMessage; 7 Player builds of 10 (two wasted: superseded within minutes), 20 reel invocations of 26, 2 suite runs (1 full + 1 filtered, the 2nd full at the close).
**Forge experiment result:** "a row a player sees is DONE only with a Player frame + a Player number": every player-facing row got a Player line; Player-vs-harness disagreements caught DURING the night, not after: 4 (lantern30 lines / platform / camper, glide30 -100 % (the reel, not the rule), lava view 15186 NREs (Editor fine), loom30 code-attached rope invisible to the CUT). Rows left "harness only": item 11 balance (kill bands) only. Verdict: the rule works; keep it.
**Reaction:** (not yet)
"""
wr("ledger/wave-30.md", ledger)

L = rd("LESSONS.md")
new_lessons = """| L-034 | A worktree dn-check that always prints 0 is worse than none: test the checker with a planted error once per night (NIGHT30's dn-check-wt.sh sed ate "error CS"; an unqualified name reached the Editor) | w30: Hud30.cs(77) RunSetup broke the Editor compile while 4 builders reported 0 (302c1e8d) | builders, gates | new | w30 |
| L-035 | Unity API calls (new MaterialPropertyBlock, Shader.PropertyToID) in a MonoBehaviour's field initializers throw in the Player and leave every later field null; the Editor did not show it | w30: LavaView30 15186 NREs in the r1 Player reel (fixed in Awake) | view code | new | w30 |
| L-036 | A reel scenario must end on its own (Results, death, a game-time cap, a wall-clock cap): a waiting reel hangs a Player for its whole timeout | w30: span30 hung 14 min on Results (n30-r2) | reels | new | w30 |
| L-037 | Drive a reel with the player's real input, not by setting state: a code-attached rope was cleared by the Mover each step and the boss never saw a hang | w30: loom30 r6 RED (0 cut tells) -> r7 PASS with the real grapple press | reels, L-014 | new | w30 |
| L-038 | Build the Player only after the round's last commit lands: two of seven builds were superseded within minutes | w30: n30-r3 / n30-r4 unused | lead, caps | new | w30 |
"""
if "L-034" not in L:
    L = L.rstrip("\n") + "\n" + new_lessons
L = re.sub(r"(\| L-029 \|[^\n]*\| )(w29[^|\n]*)(\|\s*)$", lambda m: m.group(0), L, flags=re.M)
wr("LESSONS.md", L)

T = rd("THREADS.md")
if "w30 (NIGHT30)" not in T:
    T = T.rstrip("\n") + "\n\nw30 (NIGHT30): closed T-28 (the rows-ON Player smoke: n30-r7 `[smoke] 198.0s DONE: all steps passed`), T-29 (tank rooms: i1a 0 deaths, i6 6 over 10 seeds, was 9 / 16), T-30 (five lanes: superseded, the lanes are gone, ice glides). T-27 renamed: nobody has PLAYED the n30 levels / bosses (w30, age 1). New: T-31 the Loom's real kill time (a rope-swinging pilot; the model says 107-111 s); T-32 wave spawns beside the room's entry (f9 frames); T-33 the low-health full-screen red (NIGHT18 PostFx vignette; GG asked effects not touch the whole screen); T-34 no grapple ring seen in a Player frame yet; T-35 the look's vertex snap / texture noise / heat shimmer (cut). Sources: NIGHT30-LOG.md, NIGHT31.md.\n"
wr("THREADS.md", T)

F = rd("FEATURES.md")
if "Rules30" not in F:
    F = F.rstrip("\n") + "\n| Weapon rules (switch waits for the action; holstered cooldowns) | Core/Guns/Rules30.cs | design.n30.rules | w30 | on | not played | rules30 (Player 0.36 s vs 2.51 s) |\n| Weapon-state HUD (PUNCH / BLAST, READY / COOLING / CHARGING) | Game/HudWeapons30.cs | design.n30.hud30 | w30 | on | not played | weaponhud30, layout30 |\n| Lava (burn 25 %, knock, no teleport) | Core/Lava30.cs, Game/LavaView30.cs | design.n30.lava | w30 | on | not played | lava30 |\n| THE LANTERN ring (16 panels) | Core/Enemies/Lantern30*.cs, Game/Boss30View.cs | design.n30.lantern30 | w30 | on | not played | lantern30 |\n| Glide ice + THE GLACIER rebuilt | Core/Glide30.cs, Core/Enemies/Glacier30.cs | design.n30.glide / glacier30 | w30 | on | not played | glide30, glacier30 |\n| Wyrm head punch | wyrm30 (Boss30) | design.n30.wyrm30 | w30 | on | not played | wyrm30 |\n| LEVEL 4 THE SPAN + THE LOOM + the ARCLINE in campaign | Design/levels/n30/span.plan.json, Core/Enemies/Loom30.cs, SurvivalSim.Systems30.cs | design.n30.span / levels | w30 | on | not played | span30, arcline30, loom30 |\n| The look (HUD fx, quantise + dither) | Game/HudFx30.cs, Render/Crt29 | design.n30.look30 | w30 | on | not played | look30 |\n"
wr("FEATURES.md", F)

C = rd("CORE.md")
C = re.sub(r"State: w29 closed[^\n]*\n[^\n]*\n", "State: w30 closed 2026-10-09 at tag u3-n30: clear weapon rules + a weapon-state HUD, lava instead of pits, a 16-panel Lantern ring, glide ice + a rebuilt Glacier, a Wyrm head punch, LEVEL 4 THE SPAN (grapple, NAILGUN, THE LOOM); every row Player-proved; nobody has PLAYED it (ledger/wave-30.md).\n", C, count=1)
C = C.replace("- L-032 Builders' dn-check misses", "- L-032 Builders' dn-check misses")
if "L-034" not in C:
    C = C.replace("**Oldest open threads:**", "- L-034 Test the checker: a dn-check that always printed 0 hid an Editor compile error from four builders (w30).\n- L-035 No Unity API in a MonoBehaviour's field initializers: 15186 NREs in the Player, none in the Editor (w30).\n\n**Oldest open threads:**")
C = C.replace("- T-27 nobody has played the n29 levels; T-28 the rows-ON Player smoke (LONGWAVE with the lockout); T-29 tank rooms; T-30 five lanes (w29).", "- T-27 nobody has played the n30 levels; T-31 the Loom's real kill time; T-32 spawns beside the entry; T-33 the low-health full-screen red (w30).")
wr("CORE.md", C)
print("memory harvest 30 written; CORE lines:", len(C.splitlines()))
