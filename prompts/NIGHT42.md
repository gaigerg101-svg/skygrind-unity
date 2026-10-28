# NIGHT42.md: the feel night: GG's n41-r7 playtest, every complaint fixed and seen, hits and kills that land, arenas that play, a UI that moves (lead + eight builders; 5 h build, 1 h test, hard cap 6:00)

GG played n41-r7 on 2026-10-14 (~18:35) and sent fifteen asks, verbatim with a row map in Research/2026-10-14-wave42/GG-REQUEST.md
(G1-G15). He asked for research first ("research ultrakill, doom and other boomer shooters and actually look at real models and images")
and for a way to stop forgetting things between sessions. Research done in the forge: Research/2026-10-14-wave42/research/hitfeel-web.md
(hit / kill feedback, parry, rocket jumps, style meters, with numbers and URLs), arenas-web.md (arena elements that change play, wall-run
sections, image references), CODE-FACTS.md (today's numbers for every ask: the forge's read-only sweep; **if it is missing, the lead
 writes it in G0 before any builder starts**; G3's premise is checked: pulse27.swingSeconds 2.5 s). **If GG has sent words or DECIDE42 answers since, they come
first** (log how each changed a card before any builder starts).

Base: port/stage3 at u3-n41 (61864472) plus the forge commit. Read first: CLAUDE.md, docs/PRINCIPLES.md, docs/VERBS.md, HANDOFF.md (the
NIGHT41 block), NIGHT41-REPORT.md, NIGHT41-LOG.md (the close table), DECIDE41.md (D-8..D-12), DECIDE42.md, the wave-42 folder.
NIGHT41-BRIEF.md is the builder brief's template (untracked). t0 = GG's go.

<goal_for_the_night>
Every one of GG's fifteen asks is fixed, measured and seen on a Player frame, or parked with a measured reason. Level 1's wall-ride section
is fun to cross (more forgiving, a recovery when you fall) and a jump near a wall no longer flings you; the gauntlet swings 0.7 s sooner;
a DRONE's orb parries when it feels like it should; the shotgun boost is easy; a level's first minute shows at most two lines; every
reticle turn is fast and finishes before the next attack; holding fire after a charged shot starts the next charge; the meter tells the
truth. Hits and kills land harder (the gauntlet most), THE ICEBREAKER has more attacks and is harder, the style meter and health bar are
redesigned and animated, level 1's arenas gain elements that change play (not decoration), and a context skill keeps what we learn. Nothing
GG had is lost.
</goal_for_the_night>

<method>
1. **Shape (as NIGHT41):** G0 0:00-0:30; build from the shared commit (~0:30) to 4:20; integration at about 1:30, 2:30, 3:30; round 4
   in the Player 4:20-5:00; freeze 5:00; test hour 5:00-5:45; close 5:45-6:00; hard cap 6:00.
2. **Feel is measured and seen (L-001, the forge experiment below):** every card row has its PROOF written before it is built: the
   number (harness or telemetry) and the reel scenario + frame moment that shows it, before / after at the same moment. A row turns ON
   only after the lead looked at that pair and one iteration. NIGHT41 shipped three rows OFF for "never on a frame": tonight each card
   names its frame first and T-row reels exist by round 1.
3. **Every change behind a switch; `--off42` / `-skygrindOff42` is NIGHT41's game.** Level edits go to new ids (n42-foundry), swapped in
   by a level row through the hooks (Curriculum25 N4xLevels, maps.list rows registered in the shared commit with placeholder files).
4. **Existing systems first (L-023):** the arc40 meter, the charge rules (Core/Guns/Charge35.cs), the parry (CombatMath.IsParry,
   parry36, foes40's swing reflect), the impact rules (Core/ImpactRules.cs), juice38 / Juice39Hurt, Boss37's brain and Brawl40, crunch's
   chunks, the HUD33 style / health. Read CODE-FACTS.md before designing.
5. **Mover.cs is named tonight** (CLAUDE.md rule 3), for builder M1 only, inside the wall-jump / ride jump-off branch, behind its row;
   golden 0 mismatches and the 22 replays exact at every Core commit.
6. **Nothing lost:** VERBS.md rows touched are marked first (9-13 the ride and its jumps on level 1; 28 the gauntlet; 29-31 the
   THUNDERCLAP and its boost; 40 the parry; 25-27 the KESTREL's turn); verbs on levels 1-5 at round 1 (the before: NIGHT41's n41-r6 /
   r7) and round 4 (the after).
7. **One builder per area, end to end;** builders never run the Editor or a Player; the lead cherry-picks (no `git merge`); hand-offs go
   through the lead.
</method>

<cards>
**G0 premise (the lead, 0:00-0:30).** `date`; the Editor stopped; memory and disk (no reel under 10 GB free; delete the lead's own f*.jpg
after each verdict); GG's words / DECIDE42 first. The shared commit: N42Blocks (wallfling42, boost42, foundry42, gauntlet42, parry42,
charge42, impact42, brawl42, arena42), design.n42 (+ view rows reticle42, meter42, style42, health42, teach42, kills42), `--off42` /
`-skygrindOff42` (N29Fixes too), tape format 25, maps.list n42-foundry with a placeholder, N42BaseId (as N41BaseId), harness partials
per builder, Lesson25Tests / N37TestBaseline force NIGHT42 off. Then the before frames for every row (method 2) on n41-r7 by T-row reels
that exist (weapons41, entrance41, verbs37, level36, brawl40casual). Suite batches split so none passes 600 s: Core by letter; Game by
letter with M as MapC / MapP / Me / Mo / M9 and N as N2 / N3 / N4 / Na-Nz.

**M1 movement (builder M1, ~210 min).** G2, G5. Owns Core/Mover.cs's wall-jump / ride jump-off branch (behind wallfling42), the
THUNDERCLAP boost rule (behind boost42), design.n42.{wallfling42, boost42}.
- G2: today a jump press near a wall throws the player back hard (CODE-FACTS: the numbers and the condition). The fix: a wall jump only
  when the player means it (moving into the wall or riding it), a smaller back push with more up, and no wall jump within the first steps
  of a landing; the ride's jump-off keeps its pitch rule but caps its back speed. Harness: the back speed after a press near a wall (walk,
  stand, land), before / after; the gates on every ON level (L-061: the checker's wall-jump move changes).
- G5: the shotgun boost is easy: a looser look-down threshold, more lift, the boost on a press in the air at any pitch below level with
  lift scaled by pitch; kiln40's boost chain lines re-checked. Harness: lift per pitch, the share of tries that clear level 3's 12 m
  climbs (the boost chain rooms), before / after.
- Proof frames: a press beside a wall (walk-in, +0.15 / +0.30 s), a boost from the kiln's first climb.

**L1 level 1 (builder L1, ~210 min).** G1, G6 (with K1's elements as they land). Owns gen-n42-foundry.cjs + Design/levels/n42-foundry*,
design.n42.{foundry42, teach42}; Walk38Foundry for n42 ids through N42BaseId.
- G1: THE GANTRY rebuilt to be fun (arenas-web.md: DOOM Eternal loosened its wall climb because it "would reject me"; Ghostrunner's harsh
  positioning is its top complaint; ULTRAKILL never makes a failed jump redo a cleared fight): shorter gaps or more walls in the chain, a
  recovery when you fall (a ledge / climb back, never a long walk), the no-ride band visible or gone (DECIDE41 D-9), the gates "the chain" still the answer but forgiving: a person crosses on the
  second try. Harness: the gates with the checker's margins, the walk 10/10, the share of the checker's ride attempts that succeed.
- G6: a level's first minute shows at most two lines (the rest on demand or later): count today's lines in the first 60 s on every level
  (CODE-FACTS), then the n42 teach plan; a view row teach42 for the global trim (levels 2-5 too).
- Proof frames: THE GANTRY from the lip and mid-chain; the first 60 s of level 1 (frames at 5 / 15 / 30 / 60 s).

**W1 weapons (builder W1, ~210 min).** G3, G4, G8, G10. Owns Core/Guns/Charge35.cs (additive), the gauntlet's cycle override, the parry
window / reach, Kestrel40Math and the reticle turn, design.n42.{gauntlet42, parry42, charge42, reticle42}.
- G3: the PULSE gauntlet's next swing 0.7 s sooner: today design.n27.pulse27.swingSeconds = 2.5 s (GG's NIGHT27 "attack much slower";
  the web's cycle is 0.42 s), so 2.5 -> 1.8 s in design.n42.gauntlet42 (the n27 row untouched); the damage x6 stays (DECIDE42 D-2);
  measured: press-to-next-swing 2.5 -> 1.8 s in telemetry, the kill times on levels 2 / 3 rooms and THE ICEBREAKER's band re-run with it.
- G4: the DRONE orb's parry forgives (ULTRAKILL 1.03b: a hitbox test instead of a ray, and 3 mercy frames between the orb touching the
  player and the damage landing, hitfeel-web.md): a volume test, a larger reach, a mercy window after contact, the early buffer longer; measured:
  the share of a scripted player's punches that parry when the orb is within the visible reach (the miss "despite feeling like you hit
  it" = punches inside 2.5 m that missed).
- G8: every weapon's reticle turn faster and finished before the next attack is allowed (the attack waits for the turn; the turn is
  shortened so the rhythm does not slow); telemetry `spin` reaches 1 before `fired` again.
- G10: holding fire after a charged shot starts the next charge when it is ready (no taps while held); a release then fires it.
- Proof frames: weapons41 (taps, the held second charge), the reticle at +0 / +0.1 / +0.2 s, a DRONE parry at the orb's arrival.

**F1 hits and kills (builder F1, ~210 min).** G9, G15. Owns Core/ImpactRules.cs (additive), Juice38 / Juice39Hurt additions, the enemy
flinch / death views, crunch's chunks, a new KillFeel42 view, design.n42.{impact42, kills42}.
- From hitfeel-web.md: a hit on every enemy shows a flinch + flash + a short hitstop scaled by weapon; a kill shows a bigger freeze, a
  burst, a crunchier break; the gauntlet's hit the strongest of all (G15: a shockwave ring, a heavier freeze than heavy41's, a camera
  kick), THE ICEBREAKER's hit the biggest. Numbers with a _why each, from the research's "Ten concrete numbers".
- Proof: frames of a KESTREL hit, a THUNDERCLAP kill, a gauntlet hit on a husk / a tank / THE ICEBREAKER at +0 / +0.05 / +0.15 s; the
  impact counters in telemetry. Never touches the sim's timing beyond ImpactRules' budget.

**U1 UI (builder U1, ~210 min).** G7, G13. Owns UiHud38.Arc40 (the meter), the style meter and health bar views (HUD33's), UnityTuning.*42,
design.n42.{meter42, style42, health42}.
- G7: the meter is accurate: it shows the real charge / pulse state each frame (no lag, no false FULL), one clear language for charging,
  ready, cooldown; measured against telemetry `charge` frame by frame (0 mismatches over the weapons41 reel).
- G13: the style meter and the health bar redesigned and animated: rank letters that punch in, shake and decay (ULTRAKILL's style ranks,
  hitfeel-web.md: 8 ranks over 200-1500 points, decay 15 points/s x 1-8 by rank, a parry +100, a kill 30-45, a freshness multiplier per
  weapon; check what HUD33's style already counts before adding), combo count, the bar's damage chunk and heal glow; motion on every change, nothing static. Cyan player, red-orange
  danger, 16 px minimum.
- Proof frames: a combo climbing two ranks, a hit taken, a heal, the meter through a charge.

**B1 THE ICEBREAKER (builder B1, ~210 min).** G14 (+ F1's gauntlet hit on it). Owns Brawl40*.cs + views + pilots, design.n42.brawl42.
- Two or three new attacks with fair tells (red-orange >= 0.9 s, a movement or gauntlet answer each; Research/2026-10-14-wave40/research/boss-web.md),
  phase 2 harder; the band per DECIDE42 D-5 (default: bot 40-70 s, harder than today's 52-54 s); both pilots 10/10.
- Proof frames: each new attack's tell and its answer.

**Hand-offs (the red team's overlaps):** F1 adds hooks and events only (ImpactRules, a GauntletHit42 event); U1 alone draws the style
meter, the health bar and any style reward (K1's pit kill, F1's kill tiers); B1 alone wires F1's gauntlet hit into Brawl40's views; after
M1's wall-jump change lands, the lead re-runs the gates on every ON level (L-061) and L1 re-checks THE GANTRY. **Proof reels:** each
builder writes its own scenario in FeelReel.N42.<Name>.cs (dispatch line pre-placed in G0's shared commit): M1 `wall42` (a press beside
a wall), W1 `weapons42` (the held second charge, the reticle, a DRONE parry at the orb), F1 `hits42` (+0 / +0.05 / +0.15 s), U1
`ui42`, B1 the `brawl40` additions, K1 `arena42`, L1 `level36` + a first-60-s frame set; the lead's G0 gate: the befores taken with the
reels that exist on n41-r7, the rest on round 1's build.

**K1 arena kit (builder K1, ~210 min).** G12. Owns new Core / Game files for arena elements (behind arena42), design.n42.arena42, the
reference sheet Research/2026-10-14-wave42/ARENA-KIT.md.
- From arenas-web.md (and real screenshots: fetch the ULTRAKILL wiki.gg arena images it names, e.g. 2-1_Bridge_Arena_1.jpg,
  4-2_red_skull_arena.png, look at them, cite them): ULTRAKILL pays for using the arena as a weapon (an enemy dropped into a pit 80 style,
  crushers / grinders / trams 80 each); DOOM Eternal's "jungle gyms" (separate floors that cut long-range damage) over flat "canyons";
  pickups placed to pull the player across; at most three floor planes. Four elements that change play, e.g. a hazard enemies can be
  knocked into (with a style reward U1 can show), breakable cover, a swing bar / grapple point used mid-fight, a launch pad that sends the
  player over a fight; each with its sim rule, its view,
  its harness check and a level-data form L1 can place. Enemies must still have a climb route (PRINCIPLES).
- Proof frames: each element in use (an enemy knocked into the hazard; cover breaking).

**X1 the context skill (builder X1, ~150 min).** G11. Owns Tools/skills/night-context/ (SKILL.md + scripts), Research/.../CONTEXT.md.
- Research what exists (the wave-memory, handoff, context-save / context-restore skills already installed; Anthropic's guidance on
  Claude Code skills, memory and CLAUDE.md), find what tonight's nights forgot (examples: THE WICK's fight never killed in a Player since
  NIGHT37; NIGHT40's 7-word prompt failing the suite unseen; the n40 maps.list miss; aim41 needing a pad frame), and build one skill that
  (1) at a session start prints a one-page brief: GG's latest words, the open threads aged two nights or more, the last close table's
  SHORT rows, the lessons that apply to the card at hand; (2) at a close checks that every SHORT and open item landed in THREADS /
  HANDOFF. A script, not only prose; tested on tonight's files. The lead installs it at the close.
</cards>

<builders>
| Builder | Owns |
| --- | --- |
| **M1** movement | Mover.cs wall-jump / jump-off branch (behind wallfling42), the THUNDERCLAP boost rule, design.n42.{wallfling42, boost42} |
| **L1** level 1 | gen-n42-foundry.cjs, Design/levels/n42-foundry*, Walk38Foundry n42 lines, design.n42.{foundry42, teach42} |
| **W1** weapons | Charge35.cs (additive), gauntlet cycle, parry window / reach, the reticle turn, design.n42.{gauntlet42, parry42, charge42, reticle42} |
| **F1** hits and kills | ImpactRules (additive), Juice38 / Juice39Hurt additions, flinch / death views, KillFeel42, design.n42.{impact42, kills42} |
| **U1** UI | UiHud38.Arc40, the style meter and health bar views, design.n42.{meter42, style42, health42} |
| **B1** THE ICEBREAKER | Brawl40*.cs + views + pilots, design.n42.brawl42 |
| **K1** arena kit | the new arena-element files, design.n42.arena42, ARENA-KIT.md |
| **X1** context skill | Tools/skills/night-context/, Research/2026-10-14-wave42/CONTEXT.md |

**The spend limit (L-003):** commit every slice; on 429s the lead integrates and builds alone.
**Keep first if short:** G2 the wall fling, G1 THE GANTRY, G3 the gauntlet, G4 the parry, G10 the held charge, G8 the reticle, G5 the
boost, F1's gauntlet impact, the suite to the end. **Cut in this order:** K1's fourth element, the health bar's heal glow, B1's third
attack, the teach42 trim beyond level 1, the context skill's close check.
</builders>

<lean_rules>
- Golden at every Core commit (Mover.cs only in M1's named branch); JsMath; Mulberry32 seeded per run; whole-step timers.
- Numbers: gameplay in design.n42.<row> with a `_why`; render / UI in UnityTuning.*42.cs; never edit n35-n41 rows or files in place.
- Caps: builds 7; reels 50 (G0 befores 8, rounds 1-3: 8 each, round 4: 16, retakes 2); the suite in Editor batches under 600 s each; no
  build or reel under 10 GB free.
- Never cherry-pick while the gates' Editor compile runs; after a pick that adds a file, force its import before a build (memory:
  pick-new-file-stale-editor). Stop a background script by its PID, not pkill.
- Commits with explicit paths; never GG's four files; no audio (event names only).
</lean_rules>

<tasks>
## U3-55.1 Start + G0 (0:00-0:30)
## U3-55.2 Build rounds (0:30-4:20), integration ~1:30 / 2:30 / 3:30
## U3-55.3 Round 4 in the Player (4:20-5:00): verbs L1-L5 (the after), the five boss reels, weapons41, the new rows' proof frames, the smoke
## U3-55.4 Test hour (5:00-5:45): the suite in split batches to the end; golden; fingerprint --off42 EQUAL; NIGHT42-PROOF.md
## U3-55.5 Close (5:45-6:00): NIGHT42-LOG close table (G0, M1, L1, W1, F1, U1, B1, K1, X1, L0) and every G1-G15 row's fate;
NIGHT42-REPORT.md; FEEL-PLAYTEST; DECIDE42; HANDOFF; PORT-STATUS events; the context skill installed; tag u3-n42.
</tasks>

<leftovers>
1. NIGHT41's OFF rows: aim41 (a pad frame of the lock), models41 (a close lineup frame), select41 (a level-select frame).
2. A11 (row 13 on n41-kiln); THE WICK's fight reel kill; NIGHT40's 7-word level-4 prompt (an n42 text row).
3. Level 6's build once GG reacts (D-3).
</leftovers>

<forge_experiment>
Change: every card row names its proof (number + reel scenario + frame moment) before it is built, and the G0 before-frames are taken on
the old build in the first 30 minutes. Hypothesis: rows shipped OFF for "never seen on a frame" go from 3 (NIGHT41) to 0. Measure: the
count of OFF rows whose reason is "unseen" in NIGHT42-LOG's close table.
</forge_experiment>

<kickoff>
/goal Met when NIGHT42-LOG.md's close table is printed with every card (G0, M1, L1, W1, F1, U1, B1, K1, X1, L0) DONE or SHORT with a measured reason and every GG ask G1-G15 given its fate, or 6:00 after the night's first `date`, whichever comes first. Work in C:\Users\gaige\dev\SKYGRIND-unity on port/stage3: run `date`, read NIGHT42.md (untracked: the night's prompt), Research/2026-10-14-wave42/GG-REQUEST.md (GG's fifteen asks, verbatim), DECIDE42.md and DECIDE41.md (GG's answers first), then follow NIGHT42.md: G0 0:00-0:30 (the shared commit first with n42-foundry in design.maps.list, the before frames on n41-r7), eight builders in worktrees from its green, integrate at about 1:30 / 2:30 / 3:30, round 4 in the Player 4:20-5:00, freeze at 5:00, the test hour 5:00-5:45 (the suite in split Editor batches under 600 s each), the close 5:45-6:00. A row is ON only after its before / after frames were looked at and iterated once. Never push, merge to master or stage GG's four modified files; the web repo is read only; no audio. Ask nothing: owner-only calls go to DECIDE42.md as defaults. Every message ends with the N42-STATUS line.
</kickoff>

<closing_reminder>
- A row is ON only after its before / after frames were looked at and iterated once.
- `--off42` is NIGHT41's game; new level ids go into design.maps.list in the shared commit.
- Every lead message ends with `N42-STATUS: round <r> · builders <alive>/8 · rows on <n>/<total> · asks closed <k>/15 · <h:mm> / 6:00`.
</closing_reminder>
