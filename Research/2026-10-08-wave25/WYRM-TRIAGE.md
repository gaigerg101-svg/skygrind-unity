# WYRM triage (NIGHT25, builder W): GG's three screenshots, read from the code

Method: code paths read only (no Editor, no Player run, no screenshot-vs-run comparison). Every claim below is from reading the file:line named; "not run" is said where it applies.
Worktree base: port/stage3 65547b2.

## Which path GG took

- SURVIVAL (TITLE > PLAY, any map): nothing starts the worm. `Wyrm24Begin()` has exactly two callers: `SurvivalSim.Campaign24.cs:86` (`CampaignWorld.BeginBoss`, a campaign boss room) and `FeelReel.N24.Wyrm.cs:35` (the reel). `N24WyrmStartRun` (`SurvivalSim.Wyrm24.cs:28-39`) only builds the worm (`Wyrm24Ensure`) and keeps it dormant; the view draws it only while `w.Alive` and `Wyrm24Begin` is what sets it alive (`NewRun`). Cause: none; no fix needed. The "worm never starts outside the campaign" contract already holds on this path (the scenario `wyrmvoid` will print it as a Player line).
- TITLE > MODE > CAMPAIGN (`SurvivalGame.Maps.cs:117` GoCampaign reloads the scene on layer1): this is GG's path. The HUD text `ROUND 1 · 0 LEFT` is the SURVIVAL line (`Ui.cs:932`), and a campaign run rides on SURVIVAL's engine (`SurvivalSim.Campaign24.cs` header: "one endless round"; the Campaign24Spawner swallows the director, so `Remaining` is 0). Nothing hides that line in a campaign, in the boss room or elsewhere. That fits the screenshot exactly: a campaign run, the player inside THE FURNACE's worm fight.
- `-skygrindMap layer1` (the reel's path): same campaign code; the reel only looks at the worm, so a missing arena never failed a scenario (it checks events and phases, not pixels).

## Cause 1: the void (no floor, walls, light)

- `Campaign24.Enter` (`Core/Campaign24.cs:323-327`) calls `world.BeginBoss` then `world.Respawn(bossSpawn)`: the player is teleported to the worm's room at `design.n24.wyrm.room.origin` = (400, 0, 0) (`Design/unity-config.json`, wyrm.room), spawn (400, 0, 27.5).
- The room is COLLISION ONLY. `SurvivalSim.Wyrm24.cs:57-60,74` loads `Design/maps/wyrm-arena.json` and registers its boxes as a physics/ray layer (`wyrm24Layer`, `world.SetLayer("wyrm", ...)`). Nothing draws them: `SurvivalBuilder.Build` (`SurvivalBuilder.cs:25`) builds meshes only from the loaded `SurvivalMap` (layer1's own boxes, near the origin); `WyrmView` draws only the worm, marks and shots (`WyrmView.cs:98+`); a grep for `wyrm24Arena` / `wyrm24Layer` finds no render use. So the player stands on an invisible floor under the skybox: the screenshot.
- Light: `CampaignLighting24.Update` (`CampaignLighting24.cs:98-106`) puts the room light at the middle of the MAP's current room (`level.rooms[r]` of layer1: the boss room "THE FURNACE" rect x 60..120, z -124..-66), not at (400, 0, 0). Even a drawn arena would sit outside every point light's range; only the ambient and the sun would light it.
- Side finding: layer1 has its own boss room "THE FURNACE" (`layer1.plan.json:425`, rect 60..120 / -124..-66, par 110) with walls and a floor, that the player never fights in (the worm is in the far room). The cheapest fix that is NOT a redesign is to draw the far room.
- Fix (W, in `WyrmView.cs` / a new `WyrmRoomView`, no sim change): build one stone mesh from `sim.wyrm24Arena.boxes` + `wyrm24Origin` with `MeshKit.Box(top, side)` and `game.mats.stone` exactly as `SurvivalBuilder` does for map boxes; keep it active only while the worm fights; add one point light at (origin + (0, ~12, 0)) in the pale violet-white boundary tone (config numbers under `design.n25.wyrmAim`-style `_why` rows, none typed). Needs `MeshKit` (Game assembly, Unity-side: dn-check only).

## Cause 2: `ROUND 1 · 0 LEFT` over the worm

- `Ui.cs:925-947` writes `ROUND {ra} · {rb} LEFT` whenever `run.state == Fight`, with no campaign test. `Ui.cs` is the lead's file. Hook I will provide in my own file (`SurvivalGame.Wyrm24.cs`): `public bool WyrmFightLive => sim?.wyrm24 != null && sim.wyrm24.Alive;`. The lead's one call: in `Ui.cs` at the `roundText` update, `if (game.WyrmFightLive) { roundText.SetText(string.Empty); }` (and the same for `topLineDirty`). Whether a campaign should show the round line in every room is layer builder L's decision, not mine.

## Cause 3: no boss bar

- `HudReads20.Live.cs:123-126` `ReadBoss` reads only `BulwarkNow(g)`. `UiThreat20` already draws any valid `HudReads20.Threat` (name, HP, phase mark, pips), so the WYRM only needs a read: `Wyrm w = g.sim.wyrm24; if (w != null && w.Alive) BossThreat20.From(w.Bar(), ref t);`. `Wyrm.Bar()` exists (`Wyrm.cs:956`). Fix is one block in my own read file. The bar's colours (`UiThreat20.cs:137,178`: white / muted fill, danger-coloured pips) are the BULWARK's; the pips are danger-coloured in the BULWARK's design: I will check the 18 px text and the cyan/white/violet rule when I build it.

## Cause 4: it does not die

Read, not run. Findings, in order of how likely each is the cause:
1. The kill path in code is complete: `WyrmBody.Damage` -> `Wyrm.Apply` (`Wyrm.cs:909`) -> at 0 HP `SetPhase("dying")` and `kit.Dying`; `Step` ends it after `dieSteps` with `body.alive = false` and `kit.Down` (`Wyrm.cs:1020-1024`); the campaign clears the boss room when `BossDefeated` (`Campaign24.cs:369-371`, `SurvivalSim.Campaign24.cs:92`), then `EndRun` -> RESULTS. So there is no missing "finisher" call; the reel's 5-minute fast-forward goes through exactly this path.
2. The player is hard to damage-race: the pool is 7800 HP (`hp.pool`), the human-bot kill is 3:01 mean with the revolver, and every player death restarts the whole fight at full HP: `Campaign24.Retry` (`Core/Campaign24.cs:497`) calls `world.BeginBoss` again, which calls `Wyrm24Begin` -> `NewRun` (full HP). GG's 48545-frame run (~13 min at 60 fps) with no bar, no arena and no sense of progress fits "dies a few times, the worm resets, never sees HP move". That is the most likely explanation of "does not die", together with the missing bar (he could not tell it was working).
3. Possible hidden block (not proven): `Filter` returns blocked in the intro, beat and dying phases only; a long `intro` while the player cannot see the room reads as "does not die", but the intro ends by itself (the path loop).
4. Not checked: whether the pistol-only limit of room 3 is restored in the furnace on a Player (`SurvivalSim.Campaign24.cs:CampaignSetLimits`: restored when the boss room's flags are 0; the plan's boss room has no limit key).
Fix plan: (a) the bar (Cause 3) shows progress; (b) the scenario `wyrmdies` runs a human-like bot to the finisher and prints bossDown + RESULTS/door; (c) a death does not undo the HP: not changed without the lead's word (retry-at-full-HP is Campaign24's rule for every room; the boss reset is deliberate in `Campaign24.cs:497`: "the worm starts over"). I will propose, not apply: keep the worm's HP across a retry or give the boss room a checkpoint with a 50 % HP floor. GG's call.

## PAUSED screenshot

Same state as the others (the worm is drawn while `w.Alive`; pause only freezes the loop). No separate cause.

## What I will do next (when resumed)

1. bug-void: `WyrmRoomView` (arena mesh + light, only while the worm fights), `WyrmFightLive`, scenario `wyrmvoid` (a Player line per boot path with preconditions), tests.
2. boss-bar: `ReadBoss` WYRM read, scenario `wyrmbar`.
3. boss-dies: scenario `wyrmdies`; propose the retry rule.
4. boss-aimed (design.n25.wyrmAim in Core; N25Blocks arrives with the shared commit).

Not run: the Editor, any Player, any reel; the code-path reading above is the only evidence.
