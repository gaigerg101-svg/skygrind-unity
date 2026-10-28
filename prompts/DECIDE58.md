# DECIDE58 (NIGHT58: FINISH LEVEL 3, START LEVEL 4, LAND THE HITS; the lead adds D-5 on)

## D-1 The slate (the forge, 2026-10-21 ~13:5x)
From NIGHT57's close table and threads: n57-kiln's exits / in view (T-139), level 4 (GAPS.md 1.1: 3 rooms, level 1's roster), kill55 /
press57 / rim56 / yellow56 (T-135). Rejected: level 5 instead of 4 (level 4 is the one with no new role); new features (a 2 h run fits the
near misses and one level slice; GG asked "what else can be built and itterated upon").

## D-2 The shipping rule is NIGHT57's (section 0)
Unchanged: it shipped 7 of 11 gated rows at NIGHT57 (L-103). Taste is not judged by an AI critic (L-105).

## D-3 press57's new MAIN is defined before measuring
"Chosen stillness" (the pilot's stick not pushing, speed < 0.5 m/s) replaces the stall share, because NIGHT57 measured 31-73 % of the
bot's still steps as blocked, not chosen (G.md). Written into Logs/n58/slate.json at the shared commit, before any number. Rejected: keep
the stall share (it measures the pilot's blocking, not the player's choice).

## D-4 Two builds only
r1 at ~0:55 and the freeze at ~1:25 (NIGHT57: a critic takes 25-47 min; a third build would not get one). Builders stop at 1:25.

## D-5 NIGHT57's close finished first (A0, 14:0x; t0 13:59:29)
`git tag -l u3-n57` printed nothing and the repo config had kiln57, wick55, grade55, colour56 OFF (only wyrm55, tokens56, brief55 ON,
5e3d69c2), although NIGHT57-LOG's close table and HANDOFF say all seven are ON and Builds/n57-freeze had them ON in its copy. Done:
`node Tools/night/n57-freeze-rows.cjs` with the four rows and DECIDE57 D-17's reasons, n57-gates.sh GREEN 14:08 (golden 0 mismatches,
22/22 replays, fingerprints plain / --off56 / --off57 EQUAL, Editor compile clean; Logs/n58/gates-d5.txt), commit 14546f9a, tag `u3-n57`
on it. Rejected: tagging f9552ee6 as u3-n57 (its config does not match the play build or the close table).

## D-6 No N58LevelFrom
NIGHT58.md section 3 asks for one only if a raw-id system keys on span. grep: the only raw span ids in Assets/Scripts are reel messages /
comments; Walk38Span asks `N45Levels.BaseId`, which goes through `NightRows.BaseId`, and that follows the level-row chain by itself
(n58-span -> n52-span -> ... -> n40-span). N58SharedTests pins that BaseId(n58-span) is not n58-span. Rejected: a copy of N57LevelFrom
(dead code). n52-span's file names itself twice (top level and `data`): the placeholder renames both (n58-shared.cjs asserts it).

## D-7 The disk: 14 GB, not 20
n57-prune.sh on n57-r1 / r2 / r3 / freeze freed nothing: their frames were already pruned to every 50th (15 352 kept stills in n57-r1 are
a357 evidence). Decision: no deletion of older evidence (CLAUDE.md rule 10); one reel set at a time, `df` before each (refuse under 11 GB),
`n58-prune.sh` after every batch. Rejected: deleting NIGHT57's kept stills (the close table cites them as evidence).

## D-8 The Core suite runs before r1, not during the critic (14:26)
The Editor is idle from the shared gates (14:22) to the r1 pick (~14:50); the Core suite takes ~12 min. Running it now frees the r1
critic window for the freeze prep; the builders' new test classes run at the pick. Shared tests first: N58SharedTests 3 / 3,
N55RoomClearTests 6 / 6, N57ColourTests 6 / 6 (14:25; NIGHT57's two N-Z fails were stale compiles). Rejected: the clock's slot (it
would put the suite beside the r1 reels' tail and the freeze's pick).

## D-9 r1 is small so its critic lands before the close (14:30)
A critic takes 25-47 min (NIGHT57). With the pick at ~14:50, gates + build ~10 min and a full reel set ~25 min, the verdict would land
~15:45-16:10, after the close starts (15:44). So: at the pick, `n58-gates.sh --no-editor --flow` runs beside the Editor compile and the
build; r1's reels are only the ones each entry's MAIN / GUARDS need (n57-kiln a157 + verbs37 for L0; a258 / a354 on / off for G1; a358
on / off on the V levels), <= 12 min, pruned after; the critic starts by ~15:10 with a 20-min budget and <= 24 images; bot-only entries
(G2, L1's bot numbers) go to the critic as log lines at once. verbs37 x5 (+ n57-kiln, n58-span) runs on the freeze build (the rule's
"nothing lost" is judged there). Rejected: verbs37 x5 at r1 (7-8 min the critic cannot spare).

## D-10 The off sides come from a twin build copy, not -skygrindOff55 / 56
NIGHT57's off sides used -skygrindOff55 / -skygrindOff56, which since D-5 also turn off wyrm55, brief55, wick55, grade55, tokens56 and
colour56 (all ON): the off frames would differ in more than the row judged. r1 is copied to Builds/n58-r1off (378 MB) and
n46-build-rows.cjs turns kill55 / rim56 / yellow56 on in n58-r1 and off in n58-r1off; everything else is the repo's. Rejected: the Off
switches (a confounded comparison); a second Player build (1-3 min of the Editor for the same bytes).

## D-11 Logs/feel/n52-freeze pruned to every 50th frame (14:43)
C: fell 14 -> 12 GB by 14:32 (the builders' dotnet builds); reels refuse under 10 GB (L-104), so r1 had ~2 GB of headroom. du: n52-freeze
held 20.7 GB of RAW reel frames (43 087 of 43 972 frames; the prune scripts start at n54). Its summary says "all done": every side
finished. Applied the project's own prune rule (n5x-prune.sh: keep f*00 / f*50, every named shot-*, json and summary) to that one tag.
Rejected: deleting old Player builds (214 folders, ~60 GB; some are GG's archived verdict builds: not the lead's to judge); deleting
any evidence file the logs cite (named shots and summaries stay).

## D-12 The one critic reads the freeze build, not r1 (14:58)
At the 14:47 pick only V1 (rim56) had a Player-testable slice; G2 (press57) missed its MAIN on the bot (OFF, no critic needed); L0, G1,
V2, V3 land 14:55-15:10. r1 (14:51) reels rim56 on five levels. A row whose MAIN misses on its own numbers is OFF without a critic (the
critic confirms ONs, it does not rescue misses). So the night's ONE batched critic is spent on the freeze build (~15:12), on every entry
whose numbers pass, with a 20-min budget and <= 20 images, done by ~15:40. That keeps NIGHT58.md's "no second critic" (there is one) and
its point (a row ships only with a critic's evidence check). V3's exit lamp has no row (it is on whenever kiln57 is): picked for the
freeze, it is `git revert`ed before the tag unless the critic passes it. Rejected: a critic on r1 for rim56 alone (its MAIN is a number
the lead reads; the critic's 25-47 min would leave the freeze entries without one).

## D-13 yellow56 stays OFF; the tag carries no new row (16:09)
The freeze critic died on an API error at 15:28, was resumed, and died again; the session lost the API ~15:36-16:08. yellow56's numbers
pass (0 / 12 frames over 0.5 % vs 12 / 12) but NIGHT58.md says a row without a critic verdict stays OFF. Builds/n58-freeze's copy set back
to yellow56 off (= the repo); tag u3-n58 on ea053e54. The smoke (14 steps failed, run during the outage) is not re-run: logged SHORT.
Rejected: ON on numbers alone (the rule's line 4 needs the critic); running past the 16:14 cap.

## D-14 EXT: the hour's slate (19:4x; GG: "keep building for next hour you decide")
The smoke re-run first (the play build's only open failure), then three tracks at once: yellow56's critic on a slim brief (gate-x-brief.md),
L0's kb2c re-cut by the lead (sim-only loops, minutes each), level 4's THE GAP NEST by builder L (the card was written at NIGHT58).
Rejected: kill55 / rim56 work (each needs a Player reel set and a critic: not inside an hour); a third builder (L-098: fewer agents).

## D-15 EXT: yellow56 stays OFF (critic FAIL, 20:08)
The critic's guard reading (the foundry a5 parry halo keeps 20 % of its yellow) is a lost tell (PRINCIPLES rule 1). Next: vFull above the
halo's 0.84 (V.md's estimate 0.90), re-measure a358 on both sides. Rejected: arguing the halo is one frame (the guard names it).

## D-16 EXT: L0 cut 4 held, not shipped (20:19)
Its bot numbers pass the rule's lines 1-2 and kb2c's contract, but line 3 (verbs37 on n57-kiln in a Player) and line 4 (a critic) are not
checked in the hour; kiln57 is ON, so a data change ships at once. Kept as L0-cut4.patch. Rejected: cut 5 (deaths 10 > 9.9).

## D-17 EXT: THE GAP NEST out of NEW (20:22)
Builder L put p2 in NEW on walks and levelgen alone; the generator's header says a slot holds its new room only after n58-l-room, identity,
the contract and walk 10/10. span58 is OFF, so the game is unchanged either way; the data now matches the rule. Rejected: keeping it in NEW
(the 61 falls into the void and put-at 0 -> 3 are unread).
