# Wave 56 (NIGHT56, LEVELS FIRST; 2026-10-20 23:57 - 2026-10-21 07:2x MST; usage-limit pause 03:09-04:20; tag: see the close)

**Ask (GG 2026-10-20 ~23:08, verbatim in NIGHT56.md):** "what should overnight prmpt focus on to imporve levels specifically, gameplay,
eniemies, colors, ect, resrecah plan for next 20 mins and build out 7 hour night prompt" [reading: improve levels specifically,
gameplay, enemies, colours; a 20-minute research plan, then a 7-hour night].

**Prompt shape:** NIGHT56.md (246 lines, untracked); 7 h min / 7 h 30 cap; P0 NIGHT55's debt, L levels (L1 checker, L2 spawns in view,
L3 seven rooms, L4 a showcase room conditional on L2), G gameplay / enemies (rank56, kill55's fix, tokens56, THE WYRM's tail), V colours
(colour56, rim56, yellow56, the boss palettes); the forge experiment: at most three builders and ONE batched critic per build (w55: five
builders + six side agents hit the limit at 2 h 28 min with 1 of 26 entries gated). Decisions: DECIDE56 D-1..D-26.

**How the run went:** the pick of NIGHT55's 13 branch commits took 2 min; r1 (P0) was built at 0:27 and gated at 1:26 (the first gate;
w55: 1:10 for 1 entry). Builds r1-r5; critics on r1, r2 and r5 (A + B: > 30 images); r3 / r4 were picked while a critic read and had no
critic of their own (D-20, D-22). The usage limit stopped the lead and all three builders at 03:09 (3 h 12 min); it reset at 04:20; the
builders were resumed by SendMessage and finished by 04:38.

**Numbers (before -> after; bot unless it says Player):** rooms whose first wave is seen from the entry 1 / 18 -> 16 / 18 (the checker's
first reading; 15 / 18 entry-eye, 4 / 18 strict after its fix: 13 rooms have no foothold); the a156 Player meter agrees with the checker on
12 / 12 rooms; clear times foundry 130.2 -> 122.6 s, iceline 233.2 -> 220.3, kiln 92.6 -> 92.2, span 95.3 -> 86.8, spire 141.0 -> 135.1;
stall foundry 26.2 -> 27.7 %, iceline 32.0 -> 36.5, kiln 29.2 -> 25.5; identity FAIL rooms 7 -> 5; tokens56 on: attack starts inside the
sim cone 44.6 -> 84.3 % pooled (the lead's runs, Logs/n56/bot-r4 / bot-r4-tokens); THE KILN WYRM with wyrm55 on: 38/40 -> 40/40, median 122.6 -> 86.8 s, p90 256.9 -> 178.3 s, deaths 68 -> 32;
rank56 on: DULL share unmoved (50.5 -> 50.6 % foundry); the kill chain's ladder (Player a355): every kill 0.050 s -> kills 3+ held 5
frames; THE WICK's hot wall burn S 0.22 -> 0.935 (Player a155); flat verbs37 frames 7 -> 5; the smoke DONE with two steals that really
take focus (w55's never did: "focus changes 0"). verbs37 27 / 21 / 31 / 31 / 31 on every build.

**Shipped (Builds/n56-freeze, tag u3-n56 = 04bef918):** clear55 ON (the room-clear beat: gate-r5a PASS); level data: L2's spawns in
view, p5's pad, g8's THE ASH PIT (GG's call, T-136), the door lamps; the tools (n56-room.py, a156, n56-colour.py, the focus steal). Gates
passed: P0-pick, L1, NIGHT55 P2-clear and P0-t125; P0-ssao DONE-no-defect.

**SHORT (Logs/n56/slate.json):** L2 (the strict in-view reading 4 / 18, the bot stall), L3 (5 rooms left), L4 (conditional on L2), G1-G4,
V1-V4, S1-doors (4 flat frames), S2 / S3 not started, S4; every row but clear55 OFF (D-29). Forge experiment: entries gated / attempted
13 / 14; the first gate at 1 h 26 min on 11 entries (w55: 1 / 26, 1 h 10 min); the limit still hit at 3 h 12 min.

**Closed threads:** T-99 (the recorder: a 2807-frame Player run, 0 failed; the 1800 cap is per phase by design), T-125 (a152's THUNDERCLAP
fires 6 of 6 taps: gate-r2 PASS).

**Lessons added or confirmed:** L-003 / L-005 (the limit stops everyone) a fourth time; new L-098 (three builders + a batched critic: gates
land before the limit; resume by SendMessage), L-099 (a planted focus steal must take the foreground and say so), L-100 (check each
doneWhen line against the A0 baseline), L-101 (a reel's GREEN follows the slate's line), L-102 (build the next round while a critic reads).

**Reaction:** (none yet)
