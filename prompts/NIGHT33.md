# NIGHT33.md: the polish night (lead + seven builders, about 4 hours)

GG, 2026-10-11 ~10:00, after playing NIGHT32 (verbatim and screenshot notes: Research/2026-10-11-wave33/GG-REQUEST.md): more elements in
every arena; better effects and lighting (lights "placed and floating"); the arena design is poor, overhaul some; a complete HUD / front UI
overhaul that does not look AI-generated; the zoom is "a forced angle"; enemies run into lava; ULTRAKILL-style trigger spawns; bosses "still
way too tanky"; the grapple looks and feels bad (pull me to the enemy, stop at it, a little damage); a stronger piercing pistol charge;
campaign points that buy something. "I expect major major changes."

Base: port/stage3 at the NIGHT32 close (tag u3-n32, 4c79a5ce). Research (done, 30 minutes, six subagents): Research/2026-10-11-wave33/
(U1 HUD, L1 lighting, K1 arena kit, G1 feel / AI / economy, A3 and A4 code audits with file:line). Design: Design/levels/DESIGN33.md (cards
U, L, K, R, E, W, P and five rules). Defaults: DECIDE33.md. NIGHT32's leftovers: NIGHT33.draft.md (outranked by this prompt).
t0 = the moment GG says go. Box 4:00, hard cap 4:30.

<method>
1. A spec, a check, a frame: every card ships its spec's automated checks and Player frames the lead looks at (RED -skygrindOff33 /
   GREEN). Adjectives are not results.
2. Commit every slice (L-003: the spend limit stops every agent at once); the lead integrates about every 50 minutes.
3. Visual cards (U, L, K, R) send the lead a frame list per commit: the reel scenario and the exact thing to look for.
4. Gameplay numbers by seeded harness sweeps; the Player decides over the harness.
</method>

<builders>
Seven builders in git worktrees (`isolation: "worktree"`), launched at 0:15 with DESIGN33.md. Each: `git merge port/stage3` before
starting and before every report; `bash Tools/night/dn-check-wt.sh <scratch>/chk` = 0 errors before each commit (prove it red once,
L-034); `dotnet run --project Tools/SimHarness -c Release -- fingerprint` and `... --off33` EQUAL before each commit that touches sim
code; never the Unity Editor, a build or a Player; reports under 400 words; resumed by SendMessage.

| Builder | Owns | Card |
| --- | --- | --- |
| **U** HUD + front UI | `Ui*.cs` HUD and menu files (Ui.cs, UiHotbar.cs, UiMomentum.cs, UiReticleArc24.cs, UiLevelSelect26.cs, the results screen), `HudWeapons30.cs`, `UiHud32.cs`, `GrapplePips32.cs` (folds into the reticle), `CueWait25.cs`, new `Ui*33.cs`, `UnityTuning.Hud33.cs`; reel `hud33` | U |
| **L** lighting + VFX | `CampaignLighting24.cs`, `LightKit29.cs`, `RoofView27.cs`, `Look.cs` (post profile), `LavaView30.cs` (light only), `PostFx.cs`, new `Fixtures33.cs` / `Vfx33.cs`, `UnityTuning.Look33.cs`; harness `light33`; reel `look33` | L |
| **K** kit + dressing | `PropsView29.cs` (new cases), new `Kit33*.cs` (prop meshes, wall surface material), `Tools/night/dress33.cjs`, the dressed plan outputs for every n32 level (`Design/levels/n33/*.plan.json` -> `Design/levels/n33-*.json`); harness `kit33`; reel `kit33` | K |
| **R** arena overhauls + devices | room edits inside K's n33 plans (coordinate: R edits rooms R names in its first report; K's dress33 runs after R's room shapes), new `Core/Devices33.cs`, `Game/SurvivalSim.Devices33.cs`, `Game/Devices33View.cs`; harness `arenas33`; reel `arenas33` | R |
| **E** enemies + encounters + boss cut | `Core/Nav/NavField.cs` (the lava mask hook), `Core/Lava30.cs` (mask fill), `LevelValidate24.cs` (same mask), `Core/Enemies/HuskBrain.cs` (edge guard), `Core/Campaign24.cs` (triggers33 in StepFight, the spawn tell), new `Game/SurvivalSim.Ai33.cs`, `SpawnTell33View.cs`, the boss rows (`design.n33.bosses33`, overrides of n32 pools); harness `ai33`, `spawns33`; reel `ai33` | E |
| **W** viewmodel, whip, piercer | `Viewmodel.cs` (the ADS path behind view33), `ZoomLookTests` (n33 path), `Core/GrappleTap31.cs`, `Game/SurvivalSim.Grapple31.cs` / `Grapple32.cs`, new `SurvivalSim.Whip33.cs`, `Arcline21View.cs` / `Grapple31View.cs` (the rope), `Core/Guns/ChargeShots27.cs` (behind pierce33), `WeaponFx.cs` (the beam event); harness `whip33`, `pierce33`; reel `feel33` | W |
| **P** points shop | `Economy.cs` (the carried bank), `Profile.cs` (save), new `Core/Shop33.cs`, `Game/UiShop33.cs`, `Game/SurvivalSim.Shop33.cs` (upgrades applied at run start, recorded on the tape); the lead applies the `ContinueCampaign()` hook; harness `shop33`; reel `shop33` | P |

Shared rules: each builder edits only its own `design.n33.<row>` objects in Design/unity-config.json, never a value under n24-n32
(overrides only); StreamingAssets by cp + cmp in a worktree (n24-sync.sh works only on the main repo). **The lead alone** touches
`SurvivalSim*.cs` other than the new files above, `SurvivalGame*.cs`, `FeelReel.N27.cs`, `Tools/SimHarness/Program.cs`, `RunTape.cs`,
`FingerprintBaseline*.cs`, `Core/Curriculum25.cs`, the config merge, every build and reel. A builder names the one line it needs as a
.cjs patch script; the lead applies it the same round. Overlaps decided now: U owns the reticle (W does not touch it); L owns lights inside
K's props (K builds the fixture mesh only when L asks); R shapes rooms, K dresses them (K re-runs dress33 after each R commit).
</builders>

<lean_rules>
- **Proof:** a visual row is DONE only with a RED / GREEN Player frame pair looked at by the lead; a gameplay row with a Player number.
- **Golden at every Core commit (rule 3):** fingerprint + --off33 + golden + trajectories; `Mover.cs` never edited; JsMath, Mulberry32 only.
- **Config:** Unity-only numbers in `design.n33.<row>` with a `_why`; patch scripts are .cjs files written with the Write tool; no Unity
  API in MonoBehaviour field initializers (L-035); boxed numbers in level / prop rows are doubles (L-043).
- **Caps:** Player builds 8; reel invocations 20; suite runs 2 (batch); builders 7 alive.
- **Heavy jobs one at a time** (L-005); the lead runs reels between integrations, never beside the suite.
- **Commits:** every finished slice, explicit paths plus `.meta`; never UnityConnectSettings.asset, Arena.unity, UIEffectProjectSettings.asset,
  EditorBuildSettings.asset; never master, never push; restore the TMP font assets Play mode rewrites; nothing deleted.
</lean_rules>

<tasks>
## U3-46.1 Start (0:00-0:15)
`date`; the Editor gate (GG may be in Play mode: stop only if he is done; else use the batch path); `git status` read line by line; the
shared commit: `N33Blocks` (ai33, spawns33, bosses33, whip33, pierce33, devices33, shop33, kit33-colliders if any: the sim bits) in tape
format 16 with `AsFormat15`, design.n33 rows off (view rows hud33, look33, kit33, view33 included), `--off33` / `-skygrindOff33`,
`N33Fixes`, SurvivalSim.N33 hooks per builder, harness partials N33<Builder>, reel groups PhaseN33<Builder>, the `ContinueCampaign()` shop
hook behind shop33; Curriculum25 level swap row for K's n33 levels (`design.n33.kit33.levelSwap` n32-* -> n33-*); gates; launch; rewrite
integrate.sh's ID map.
## U3-46.2 Build rounds (0:15-3:10): integrate at about 1:05, 1:55, 2:40
Each round: integrate, sync, Editor compile, dn-check 0, fingerprint + --off33 EQUAL, golden + trajectories if Core changed, ONE batch
build, ONE reel invocation per landed card, frames LOOKED AT, builders resumed with the Player's exact lines. Landing order: E's boss cut and
lava mask, W's piercer and zoom, U's HUD skeleton, L's floating-light fixes in round 1; K's library, R's first rooms, P's bank in round 2;
the dressing pass on every level, the whip, the shop screen, the effects in round 3.
## U3-46.3 Freeze (3:10) and verification (3:10-3:40)
`--off33` fingerprint EQUAL; one batch suite alone; smoke; LevelSelect29; DoorFree29; bosses (pilot 15-22 s) and camp; ai33 (0 lava deaths
not caused by the player); frames of every level's dressed rooms, the HUD in combat and in a boss fight, the zoom, the whip, the piercer, the
shop; NIGHT33-PROOF.md.
## U3-46.4 Close (3:40-4:00)
NIGHT33-LOG.md close table, NIGHT33-REPORT.md (one page for GG with before / after frame pairs), FEEL-PLAYTEST.md, HANDOFF.md, PORT-STATUS.md
events, NIGHT34.md, tag `u3-n33`. Hard cap 4:30.
</tasks>

<closing_reminder>
- No floating light, no filled HUD box, no text overlap, no gun sliding to the centre, no enemy walking into lava on its own.
- Every visual change has a RED / GREEN frame pair the lead looked at.
- Switches and exactness: every sim change behind design.n33; golden 0 mismatches; fingerprint EQUAL with the rows off; never Mover.cs.
- Every lead message ends with `N33-STATUS: round <r> · builders <alive>/7 · rows on <n>/<total> · <h:mm> / 4:30`.
</closing_reminder>
