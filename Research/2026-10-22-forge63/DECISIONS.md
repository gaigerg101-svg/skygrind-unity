# DECISIONS (FORGE63, 2026-10-22 ~22:11-22:45, a 30-minute forge during NIGHT62's close; GG was not asked: "create it now")

GG, verbatim: "plan out next long detail night prompt 8 hours 7 minimum, follow detailed methods we used and pull form online to get best
cresuolts, be honets and critcal on where we need to improve, i would lead towards bosses, effect colores everything doenst feel alligned,
boss animations are too rigid to start, this is one of many many things the night prompt hsould focus on, create it now, 30 mins to build".

## D-1 Eight hours, seven minimum, held by the guard's clock
`Tools/night/n63-clock.cjs` (n62-clock with freezeMin 420, capMin 480): BUILD to 7:00, the close 7:00-8:00, CAP 8:00. The minimum is the
/goal's first clause (a printed clock line at 7:00 or later before the close) and NIGHT63.md rule 1; a dry slate takes a leftover. Rejected:
an early close at LIST EXHAUSTED (GG set a minimum; the playbook's minimum-length rule needs seeded leftovers, section 3 holds seven).

## D-2 The bosses lead, in three slices, all view only
B1 motion (V), B2 tells (G), B3 colours (V). The boss views copy the sim rigidly today (a grep for spring / damping / anticipation / flinch
finds 0-9 uses per view file, 0 in Boss37Kit.View.cs); a view-only layer changes what GG sees without touching the sim, so the golden vectors,
replays and fingerprints stay exact (CLAUDE.md rule 3). Rejected: changing the bosses' attacks or shapes (enemy design is GG's, rule 7).

## D-3 Debt first: NIGHT62's three rows that were judged unfairly or failed on timing
P1 and P3 get same-run pairs (D1 first, builder D); P2 is fixed to rise at or before 1.0 s (builder G), the doneWhen it always had. Fixture61
waits for GG's keep or drop (DECIDE62). Rejected: re-SPOTting P1 / P3 on two-run pairs (L-129: the runs drift).

## D-4 One colour language for effects (C1) is tooling plus fixes
An audit of literal effect colours in Assets/Scripts/Game, one role table in config (CLAUDE.md rule 8's roles), the worst offenders routed
through it behind design.n63.colours63, and a frame meter measured first (research: no studio publishes per-role numbers). Rejected: a
look-by-look recolour without a table (it is how the effects drifted apart).

## D-5 The rooms and the movement are not tonight's lead, and the prompt says so
gg-proxy's #1 (empty rooms) and #2 (choppy movement) have seven asks each; GG led with bosses. M1 measures the camera's chop from telemetry
(no change); the rooms wait for a level night (DECIDE63 asks GG the order).

## D-6 The forge experiment: visible by design
Change: every visible slice names its SPOT pose in the slate, and that pose's same-run pair must change at least 2 % of the frame's pixels
before its SPOT, or it goes back to its builder once. Hypothesis: the SPOT pass rate rises from NIGHT62's 1 of 4 visible rows judged (25 %) to
at least 60 % of the night's visible slices. Measure: NIGHT63-REPORT.md section 7 (the pass rate and each slice's changed-pixel share).

## D-7 Builders three, agents five
D (D1, C1), V (B1, B3), G (P2, B2); two GATE slots; the spotter in the foreground; gg-proxy at the start and 6:30. Rejected: four builders
(the usage limit, L-098; the memory crash, L-128).

## D-9 The red team's fixes (Research/2026-10-22-forge63/redteam.md, 12 findings, 22:20-22:26; all applied)
NIGHT62's clock can no longer block the start (bearings stop it; a missing u3-n62 is logged, not fatal) (1). D1 is scoped to rows read every
frame through a live reel override; bossRule61's OFF is its UI root hidden; off captures keep the tools working; text61 (P3) leaves tonight's
slate for a pinned-hint two-run pair as a leftover (2). MapChoiceTests is out of tonight: it can wedge the Editor every build needs (3). The
visible-by-design bar is 41,472 px (2 % of 1080p), only for B1, B2, B3 and C1; P1 and P2 are exempt (NIGHT62's only PASS was 20,439 px) (4).
Builds on demand, with b1 / b2 / b3 as latest times (5). V builds BossAttackClock63 and the boss63 reel first; G's B2 uses them, fixes only
the attacks under 300 ms or off the danger hue (config's tells already run 0.55-1.5 s), in its own file; B1's SPOT is a flinch or a slump, B2's
a cue (6, 8). B3 gets its own row (bossColours63) and moves to D after C1 (7). M1 measures what a fixed-clock reel can show and a bench run
for the pacing, labelled (9). The report ranks the 6:30 frames, not the freeze's (10). Five agents counts the spotter (11). Yellow's role is a
D-id in A1 (12). The forge experiment's measure (D-6) is now: the SPOT pass rate of B1, B2, B3 and C1, and each one's changed-pixel count.

## D-8 NIGHT63's tools are NIGHT62's, renamed
n63-reels / prune / gates (with --off63) / slate / freeze-rows / clock / spot-pair: a scripted rename (0 "n62" left), each syntax-checked;
n63-gates.sh's fingerprint loop runs plain and --off57..--off63 (the harness takes any --offNN). The guard's clock rule needs no change (it reads
Logs/clock.json's own bands). Not run in the forge: n63-gates.sh end to end (NIGHT62's close was using the Editor; its n62 twin ran GREEN
twice tonight).
