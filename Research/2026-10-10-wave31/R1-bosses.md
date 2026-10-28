# Arena boss patterns that stop camping and force movement and weapon variety

Method: web search plus page fetches, 2026-10-10. Every claim carries the URL it came from. Items marked (thin) rest on a search snippet or a fan wiki, not a designer statement. Items marked (inference) are my reading of the source, not a quote. Nothing here is from a GDC video transcript; I could not fetch one (see Gaps).

## Patterns

### 1. Patience gauge / enrage when you stay far away
- Mechanism: the boss counts time spent at range; when the gauge fills it enrages (faster, no cooldowns on its charged shot), so retreating becomes the losing play.
- Games: ULTRAKILL V2.
- Why: standing off and sniping is punished by a stronger boss; the player has to close in.
- Source: https://ultrakill.wiki.gg/wiki/V2 (wiki, thin on designer intent)

### 2. Range-keyed weapon swap by the boss
- Mechanism: the boss changes weapons by distance, so any one range is a known bad spot ("a change in proximity will close it off").
- Games: ULTRAKILL V2 (revolver and charged shots at range, shotgun and core-eject grenade up close, coins and nails in encounter two).
- Why: no single position is safe; the player has to change distance, and so changes weapon.
- Source: https://ultrakill.wiki.gg/wiki/V2

### 3. Telegraph-color movement states on the boss itself
- Mechanism: V2's wings show its movement mode (gold runs a line, blue circle-strafes, red rushes, green retreats), so the player reads where it will be.
- Games: ULTRAKILL V2.
- Why: it makes a mobile boss readable and fair while it keeps moving; (inference) it lets you plan your own dash and slide.
- Source: https://ultrakill.wiki.gg/wiki/V2

### 4. Arena breaks and the boss moves the floor
- Mechanism: in V2's second encounter the boss breaks through arena walls and forces a sliding descent, which removes the vertical options you had.
- Games: ULTRAKILL V2.
- Why: the safe spot you found in phase 1 is gone; the player has to re-solve the space.
- Source: https://ultrakill.wiki.gg/wiki/V2

### 5. Teleport-to-you melee plus a punish for the wrong dodge
- Mechanism: the boss teleports onto the player. If you dodge toward it, it follows with a rider kick or overhead. Grounded shockwaves and an air-cancel uppercut punish standing and jumping alike.
- Games: ULTRAKILL Minos Prime, Gabriel (teleports above the player), Sisyphus Prime (teleports in front, above, near).
- Why: a fixed spot has no value because the boss comes to you; which way you dodge matters, so you must pick a dash, slide or jump on purpose.
- Sources: https://ultrakill.wiki.gg/wiki/Minos_Prime , https://ultrakill.wiki.gg/wiki/Gabriel , https://ultrakill.wiki.gg/wiki/Sisyphus_Prime

### 6. Parry-or-dash split: some attacks cancel on a parry, some only on i-frames
- Mechanism: attacks are mixed parriable and unparriable (Sisyphus Prime: uppercut, overhead and slam cannot be parried; clap, explosion and last stomp can). The wiki's advice is to save dashes for the unparriable ones. Parrying also heals and restores stamina.
- Games: ULTRAKILL Sisyphus Prime, Minos Prime, Gabriel (rapier combo cannot be parried; greatsword and axes can).
- Why: one defence cannot cover the whole kit, so the player rotates parry, dash and slide, and the stamina pool makes the choice a real cost.
- Sources: https://ultrakill.wiki.gg/wiki/Sisyphus_Prime , https://ultrakill.wiki.gg/wiki/Gabriel

### 7. Air-state tool: the boss launches you and the answer is a verb
- Mechanism: Minos Prime sends you airborne; the wiki tip is to ground slam at once, and a rocket gets double-damage "red explosions" while he is airborne.
- Games: ULTRAKILL Minos Prime.
- Why: being launched is a state with one right exit verb plus a weapon that pays extra in that state. (inference) That ties movement verb to weapon.
- Source: https://ultrakill.wiki.gg/wiki/Minos_Prime

### 8. Homing projectile ring on a ring telegraph, answered by movement or parry
- Mechanism: a cyan ring appears, then 50 homing projectiles spiral out (75 after half health). A purple ring marks a slow black hole that explodes on contact for 10 damage plus 99 hard damage; being trapped by it while distracted is the main danger.
- Games: ULTRAKILL Flesh Prison.
- Why: stillness gets hit by homing shots and a creeping hazard; ring colour tells you which counter to use; parrying the seekers restores full health, which rewards staying in range.
- Source: https://ultrakill.wiki.gg/wiki/Flesh_Prison

### 9. Minion shield plus heal phase: kill adds before the boss heals
- Mechanism: when adds are alive the boss glows green, gains big damage resistance and absorbs each add for 10 health. The right weapons for the adds (coin splitshots, alt revolver, knuckleblaster) differ from the boss-damage weapons (magnet-placed nails, shotgun, punches).
- Games: ULTRAKILL Flesh Prison.
- Why: two target types each need a different weapon, on a clock; hiding from the adds costs you the fight.
- Source: https://ultrakill.wiki.gg/wiki/Flesh_Prison

### 10. Stun-window enemy: a weapon-swap lesson
- Mechanism: the Marauder shows a green eye flash before a melee, a hit then staggers it for a short time, and the stagger is long enough for one shot with most weapons but two if you swap. It hides behind a shield otherwise, and turns the shield toward an active BFG shot, leaving its back open. It will not lunge unless you are mid-distance.
- Games: DOOM Eternal Marauder.
- Why: it makes the player alternate weapons (super shotgun, rocket launcher, ballista) and hold a certain range while other demons are also active.
- Sources: https://www.gamepressure.com/doom-eternal/marauders-how-to-defeat-them/z0d156 (guide); the "two shots if you swap" claim is from the search result summary of https://doomwiki.org/wiki/Marauder (fetch itself returned 403, so (thin)). Player criticism that it breaks flow: https://discussions.unity.com/t/doom-s-marauder-and-other-qte-esq-enemies/857038

### 11. Resource economy that only pays when you are aggressive
- Mechanism: health comes from glory kills, armor from flame belch, ammo from the chainsaw. Hugo Martin: "the solution to all the problems is to be aggressive." If a player peeks and hides from an arachnotron, the AI advances.
- Games: DOOM Eternal (all combat, bosses included).
- Why: camping starves you; the rewards sit close to the enemy.
- Sources: https://gamedeveloper.com/design/the-aggressive-resource-management-of-i-doom-eternal-i- , https://www.gamedeveloper.com/game-platforms/pushing-push-forward-combat-with-gameplay

### 12. One weapon per role, enemies that block the wrong tool
- Mechanism: each weapon has a job (arbalist for flyers, shotgun grenade for weak spots). The Carcass spawns shields in front of the player, which blocks glory kills and paths and makes a close-range rocket dangerous, so you must switch to a shotgun first.
- Games: DOOM Eternal.
- Why: the enemy invalidates the favourite gun; (inference) the same trick works on a boss add.
- Sources: https://gamedeveloper.com/design/the-aggressive-resource-management-of-i-doom-eternal-i- , https://www.gamedeveloper.com/game-platforms/pushing-push-forward-combat-with-gameplay

### 13. Destroy-the-armor then destroy-the-flesh, with adds the whole time
- Mechanism: eight armor pieces are shot off (phase 1), then eight body parts (phase 2). The parts light up red when hit. Heavy weapons (chain gun, heavy cannon) are for armor; BFG and Crucible are for the hordes. In phase 2 the boss moves around the arena and spawns tougher adds.
- Games: DOOM Eternal Icon of Sin.
- Why: the boss damage and the crowd control need different guns; a moving boss plus aggressive spawns means you cannot stay put.
- Source: https://www.shacknews.com/article/117086/icon-of-sin-boss-fight-doom-eternal (walkthrough, not a designer statement)

### 14. Weak point only visible or reachable with a different tool (visor and beam swap)
- Mechanism: Thardus has weak points that you find with the Thermal Visor, then switch back to Combat Visor to hit; he keeps changing weak points until the third is hit. Meta Ridley: shoot his mouth while he charges his breath to stun him and expose the chest weak point, the only way to damage him. Omega Pirate: four limb weak points, X-Ray visor to see it, super missiles or charged plasma.
- Games: Metroid Prime.
- Why: the damage step changes the loadout (visor plus weapon), so a single weapon cannot finish the fight.
- Source: search summaries of https://metroid.fandom.com/wiki/Thardus , https://StrategyWiki.org/wiki/Metroid_Prime/Bosses , https://www.nintendolife.com/guides/metroid-prime-remastered-how-to-defeat-omega-pirate (page fetches failed with 402/403, so (thin))

### 15. The boss is the level: climb it, grip drains, it shakes you off
- Mechanism: you scale the colossus to the weak point; a grip stamina bar drains while clinging; the colossus bucks and headbangs to throw you off.
- Games: Shadow of the Colossus.
- Why: you cannot stand still, because stillness is the resource you are spending, and the boss is moving the "floor". (inference: for an FPS, make the boss's body the traversal route.)
- Source: search result summaries, https://teamico.fandom.com/wiki/Colossus_I , https://games.gg/shadow-of-the-colossus/ (thin; page fetch failed)

### 16. Phase switch between full-arena bullet patterns and a locked small ring
- Mechanism: in Furi, long-range phases use patterns that can fill nearly the whole arena with one small safe spot; the close phase locks you in a small blue ring and removes your gun, leaving dash, slash, parry and charge. Dash is positioning in the bullet phase (you dash even without a threat) and reactionary in the close phase (delayed dashes matter). Healing comes from shooting orbs or parries, both active.
- Games: Furi.
- Why: ranged safety is removed on a timer, and no passive survival path exists.
- Sources: https://itch.io/blog/772886/game-analysis-of-core-systems-furi , https://nichegamer.com/2016/06/16/furi-preview-bullet-hell-laden-boss-action/ (search result)

### 17. Layered bullet colors with different reaction budgets
- Mechanism: Returnal's Hyperion uses green for random low-reaction clusters, orange for slower space-consuming shapes (medium reaction), blue for the main targeted attacks. Individually weak attacks are stacked in sequence "to keep the player moving". Each phase ends with a new chord and a no-hit clear is possible.
- Games: Returnal Hyperion.
- Why: movement is forced by overlapping space denial, and colour keeps it readable.
- Source: https://blog.playstation.com/?p=349605

### 18. Teach the boss move early with a regular enemy
- Mechanism: Dead Cells added mobs that carry specific boss moves so players learn the tells before the boss. Hades adds new combos and rules to old bosses (Extreme Measures: the Furies attack together).
- Games: Dead Cells, Hades.
- Why: lets you keep a boss's tells short and fast without feeling cheap; and re-fighting does not go stale.
- Sources: https://nintendowire.com/news/2019/08/14/dead-cells-patches-in-new-mobs-and-weapons-based-on-bosses/ (search summary, thin), https://www.gamedeveloper.com/design/boss-fights-will-never-be-the-same

## Failure modes: what makes a boss boring

1. Bullet sponge: the only challenge is the size of the health pool; the fight is padded, not deepened. Sources: https://whatculture.com/amp/gaming/5-signs-of-a-badly-designed-gaming-boss-fight , https://www.denofgeek.com/games/bullet-sponges-v
2. Slow attrition: Titanfall 2's Titan bosses were criticised as slow battles of attrition where you run for batteries and return; Richter and Kane were "wholly removed from the level's gimmick" (they ignored the movement toolkit), while Viper, on an airship, stood out. Sources: https://www.invenglobal.com/articles/1230 (search summary), https://destiny.bungie.org/forum/index.php?id=158975 (search summary, thin).
3. Unreadable telegraphs: dust that hides the danger circle, a directional stomp that covers 180 degrees, and visual effects that do not match the animation. Source: https://nex-3.com/blog/hades-ii-has-a-legibility-problem/
4. Pattern memorisation that goes stale: "The understandable patterns that make bossfights winable in the first place sabotages their ability to challange players 200 hours down the line"; Hades answers with rule changes, not just more health. Source: https://www.gamedeveloper.com/design/boss-fights-will-never-be-the-same
5. A flow-breaking gimmick that ignores the rest of the game: forum players say the Marauder limits gameplay, forces you to "dance with it", and demands deep attention while other demons attack. This is one player view; id's side is below. Source: https://discussions.unity.com/t/doom-s-marauder-and-other-qte-esq-enemies/857038 , the PC Gamer piece on the director defending it (https://www.pcgamer.com/uk/doom-eternals-director-says-the-marauder-is-good-actually/) failed to fetch, so I cannot quote him.

## Reusable shortlist for a FPS arena boss (my synthesis, inference from the above)
- Give the boss a distance-keyed kit (pattern 2) plus a stillness or distance timer (pattern 1).
- Keep the reward next to the boss: parry or glory-kill healing (patterns 8, 11).
- Have at least two damage states that need different weapons: adds vs boss body (9, 13), a stun window (10), a visor-style exposure step (14).
- Make the unsafe state change at half health: arena walls break (4), the boss moves (13), more homing projectiles (8).
- Telegraph with colour and rings, one meaning per colour (3, 8, 17), and teach the tell with a smaller enemy first (18).

## Gaps (not checked)
- No Hakita quote on a specific boss; the only interview found, https://intothebluesky.com/2020/12/19/interview-with-arsi-hakita-patala/ , was not fetched, and the search summary says bosses are designed like character-action bosses (https://intothebluesky.com/2020/12/19/interview-with-arsi-hakita-patala/).
- No GDC transcript was fetched (id's 2018 push-forward talk is only named in a search summary; the Housemarque 2022 GDC talk was only summarised by https://www.gamedeveloper.com/marketing/a-third-person-action-roguelike-bullet-hell-arcade-thriller-the-making-of-returnal , which has nothing on bosses).
- Titanfall 2 Viper, Metroid Prime, Shadow of the Colossus and Dead Cells rest on search summaries or fan wikis; page fetches failed (402/403).
- Wiki damage numbers (ULTRAKILL) are the community's and may be out of date.
