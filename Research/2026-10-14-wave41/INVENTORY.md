# Wave 41 inventory: everything not done or partial, and what else to improve (the forge, 2026-10-14 08:41-)

Sources: NIGHT40-LOG.md (the close table, the round notes), NIGHT40-REPORT.md, DECIDE40.md, NIGHT39-LOG.md's close table, HANDOFF.md,
docs/memory/THREADS.md, OWNER.md. Each row: what is missing, the evidence, why it matters to GG, the fix's shape, its weight (S / M / L).

## A. NIGHT40's not-done and partial (carried, each must close DONE or SHORT with a reason)
| id | item | evidence | the fix's shape | weight |
|---|---|---|---|---|
| A1 | the shotgun's faster pump (pump40) OFF | built, never on a frame: the reel telemetry carries no trigger / fire field to find the volley's frame (NIGHT40-LOG round 4) | first add per-frame weapon telemetry (fired, weapon, charge fraction, pump phase) to every reel; then frame it, iterate once, ON | S |
| A2 | the pistol's rhythm (spin, kick) and the slug's look never judged on a screen | harness only (W1, H1) | the same telemetry; a weapons reel taking frames at shot +0 / +0.15 / +0.30 / +0.45 s and the slug at release / +0.15 s | S |
| A3 | level 2 does not read as an ice canal | the tint drew, the warm lights (~0.9 / 0.7 / 0.5 of R / G / B) leave a pale grey floor (L2's pixel samples, f5v2 f0300) | cool lights for the iceline theme (LightPlan33), canal water / ice sheen as view props, frost on walls: a per-level look, view only | M |
| A4 | THE LOOM's floor sweep cue | no sweep attack exists (B4) | a new LOOM attack that sweeps the canyon floor (the answer: be on a wall, a ledge or a hook), its red-orange decal tell; it gives the no-ride phases a reason | M |
| A5 | at most two enemies winding up at once | up to 5 committed, > 2 for 3.5 % of fight time (E3) | an attack-token pool (Doom 2016's, enemies-web.md section 3) at the wind-up start of each kind | M |
| A6 | the batch suite stopped for low memory twice (NIGHT39, NIGHT40) | 6.7 GB / 8.3 GB free at start, the Editor holds ~11 GB with its import workers | run the suite IN the live Editor in namespace batches (no second Unity), or a fresh-boot batch with > 12 GB free | S |
| A7 | husk / spitter the weakest silhouette pair | the blind reader, twice | the husk's own feature (a hunched yoke is not enough): e.g. a dragging arm or a lantern head; re-test blind | S |
| A8 | enemy parts vanish with the body (no death pose) | E3 | a death pose / a break-apart for the parts (crunch already flies blocks: reuse) | S |
| A9 | the stray room banners in DOORFREE boss reels | B4, B2 frames ("4.2 FIRST GAP OPEN", "2.2 THE SHIELD OPEN") | the reels' DOORFREE mode silences room banners; check the real game shows none during a boss | S |
| A10 | level 5's grapple rows 17 / 19 / 22 / 23 RED (NIGHT39 and NIGHT40) | no grapple node on n38 / n40-spire | grapple nodes where they serve the spire's descent (a node per descent room), the walk re-run, the gates re-run | M |
| A11 | verbs row 13 (slide bounce) RED on level 3 since NIGHT39 | the reel's 0.02 s press under one frame on the low preset (K4) | fix the reel's press (hold for two steps, L-039) | S |
| A12 | THE GANTRY b2-g1 crossed by one endless ride (D-14) | R2's checker +156 % | GG decides (DECIDE41); default: an n41-foundry with a no-ride band on that wall so the chain is needed again | S |
| A13 | level 6 out, "rebuilt from scratch later" | cut40 ON, files kept (D-4) | tonight: the plan only (a beat sheet, a greybox behind an OFF row, THE RIVAL as a one-page proposal) so GG reacts before the build | M |
| A14 | the KESTREL's crosshair spin vs pad: never seen with a pad | | in the pad reel (A16) | S |

## B. Older rows still OFF (carried for two nights or more: THREADS T-55, T-56)
| id | item | evidence | the fix's shape | weight |
|---|---|---|---|---|
| B1 | juice38: the boss entrance shake + flicker, the damage static | OFF since NIGHT38: "no boss-entrance frame looked at" (NIGHT39-LOG); GG asked for both in w37 | a reel that frames each boss's entrance and a hit; one iteration; ON | S |
| B2 | doors38: the visible door curtains | OFF: "a locked doorway never seen on screen: the reels open doors" (NIGHT39) | a reel scenario that walks into a locked room WITHOUT DOORFREE and frames the barrier; the colour per GG ("orange barrier or a different colour"; orange is danger: violet-white is the boundary colour) | S |
| B3 | stag38 (THE STAG's look pass) | moot: THE STAG left the campaign (D-8) | close it as retired | - |
| B4 | iceline38 / finale38 | superseded by iceline40 / cut40 | close as retired | - |

## C. New improvements the forge proposes (each tied to GG's words)
| id | improvement | GG's words it answers | weight |
|---|---|---|---|
| C1 | **pad-first text**: every new line (BossLine40, the boost chain, level 2's prompts, OPEN / BLOCKED) shows the pad's glyphs when a pad is in use; tonight's frames show "L-SHIFT", "LEFT CLICK", "G" | Steam + Switch, pad first (OWNER) | M |
| C2 | **aim assist on moving bosses** (a leftover since NIGHT39) | pad first; the bosses move fast | M |
| C3 | **boss bars that show phases**: a tick at each phase threshold and the stagger meter under the bar, the same for all five bosses | "hard to understand what to do" (w35), CROWN "no idea" (w40) | S |
| C4 | **the boss entrance moment**: each boss's name card + juice38's shake / flicker + its rule line in one beat (BossLine40), consistent for all five | w37 "screen shake when he first enters"; w40 clarity | S |
| C5 | **level 3 and 5 rooms use the new enemies** (one room each, the walk re-run): the CHARGER in the kiln's open floor, the DRONE over the spire's gaps | "make weapon-switching meaningful" (w40) | M |
| C6 | **THE ICEBREAKER's look**: its body reads pale tan like the walls (r4brawl frames): an ice-armour colour / material, the fists' glow, a death that shatters | "the model lacks polish" (w40, said of boss 4; applies) | S |
| C7 | **the hit / kill answer for the gauntlet on big enemies**: a heavier hitstop and a knock-back on the tank / WARDEN / ICEBREAKER, so "the gauntlet excels against large enemies" is felt | w40 level 2 | S |
| C8 | **performance**: -skygrindBench never ran with models40 + the new views; measure p99 on a campaign level, alone | Switch hopes (OWNER) | S |
| C9 | **level select previews**: each slot shows the level's name, its weapon and its boss silhouette (locked slot 6 says "REBUILDING") | w40 "levels beyond" clarity; Terraria's readable progression (PRINCIPLES 3) | S |
| C10 | **the arc on the pad**: the arc under the crosshair stays clear of the pad's aim-assist reticle bloom (check with C2) | pad first | S |

## Code facts that changed rows (a read-only explorer, 08:47; re-check)
- C2: aim assist EXISTS (Core/AimAssist.cs; pad default "assist": 6 deg cone, 120 ms, 90 deg/s, friction 0.55) and already covers every
  boss body (added to `enemies.all`), at the body's centre only (`e.AimPoint`; head lock is the husk's). So C2 = **weak-point aim**: the
  CYAN core in a window, THE LOOM's lit knee, THE CROWN's ring, THE ICEBREAKER's stuck fist.
- C1: pad glyphs ALREADY resolve per device (UiLesson25.cs:87-90, UiTeach35.cs:54-67 on `PlayerDevices.Last`); the reels never set Last to
  Pad, so the frames showed keyboard names. C1 = **prove it with a pad reel** (-skygrindReelPad) and fix any line that does not resolve.
- A3: LightPlan33.Green() hard-codes "warm" for every room light (LightPlan33.cs:361-373, 417-437; Look33PanelWarmHex 0xffe6c8); the theme
  only feeds post / fog / ambient. The fix: a per-theme light colour (iceline cool), a theme index into LightKit29.Named.
- A5: attack tokens EXIST (Core/HuskBrain.cs:31-110; ai.tokens.base melee 2, ranged 3, heavy 1, swarm 3, +1 per round step and camping):
  E3's "up to 5 committed" is the pools' sum. A5 = **a campaign cap across pools** (two wind-ups at once), behind a row.
- A10: n40-spire, n40-iceline, n40-kiln have `"grappleNodes": []`; only n40-span has nodes.
- A1 / A2: telemetry frames (Game/FeelProbe.Record.cs:354-430) carry events "shot" / "hit" / "kill" but no weapon, charge or pump phase;
  add fields to FrameRec (:32), fill in RecordFrame (:190), write next to `vel` (:382).
- B1 / B2: juice38 = a 0.12 s hit-stop + shake 2.5 + a 0.3 s CRT flicker + the name card on `bossRise` / `boss38Entrance`, static 0.5-0.8 on
  damage; doors38 = a curtain per doorway: cyan open, red-orange locked, pale violet-white the boss door; both `enabled: false`
  (unity-config.json:5328-5336). A door reel must run WITHOUT DOORFREE facing a locked zone; a boss reel frames 0-0.9 s after bossRise.
- C8: -skygrindBench runs a survival horde on whatever map booted (-skygrindMap works), not the campaign flow.

## D. Not this wave (stays in IDEAS / leftovers)
- Level 6's full build (waits for GG's reaction to the plan, A13); THE RIVAL's fight; new weapons; survival mode changes; audio (GG's).
