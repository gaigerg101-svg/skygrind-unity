# Boss design research (web only), wave 40

Date: 2026-10-13. Web research only (WebSearch / WebFetch). Each bullet ends with its URL.
"(search summary)" means the claim comes from the search engine's summary of that URL because the page itself
refused the fetch (doomwiki.org, gamespot and pcgamer returned 403/truncated pages). Treat those as one step weaker.
"not found" means no source turned up in the budget. Nothing here is invented; numbers are quoted as the source gives them.

## 1. Movement-centric bosses: keep the player moving, telegraphs, punish windows

### ULTRAKILL
- Global tell language: a yellow flash means the attack can be parried, a blue flash means it cannot. Melee tells are a
  4-point yellow spark, ranged tells an 8-point spark. https://ultrakill.wiki.gg/wiki/Parry
- A successful parry gives a white screen flash plus hitstop, refills stamina and heals up to the Hard Damage cap.
  https://ultrakill.wiki.gg/wiki/Parry
- Enemy projectiles have "three 'mercy frames' between hitting the player and damage being dealt", which makes parry
  timing more lenient. https://ultrakill.wiki.gg/wiki/Arms
- Minos Prime: every big attack has a voice-line tell. "Judgement!" means a crouch, then a teleport dropkick (parryable;
  in phase 1 a parry stuns him and cancels the explosion). "Thy end is now!" is a 4-punch combo with only the last hit
  parryable, then his fist is stuck in the ground for about a second (a heal / punish window). "Prepare thyself!" is a
  kick combo. "Die!" is a rider kick with a brief midair pause. "Crush!" is a ground slam. HP is 65 + 65.
  https://ultrakill.wiki.gg/wiki/Minos_Prime
- Minos Prime's phase change is signalled by a unique voice line plus blue flames on his body; the arena is an
  octagonal room with a higher ceiling in the centre (no floor hazards). https://ultrakill.wiki.gg/wiki/P-1
- Sisyphus Prime: the same voice-line grammar ("DESTROY!", "You can't escape!", "BE GONE!", "This will hurt."). He
  mixes parryable and unparryable hits inside one combo (e.g. Sumo combo: only the final stomp is parryable). A Taunt
  ("Nice try!") is his idle punish window. HP is 100 + 100. https://ultrakill.wiki.gg/wiki/Sisyphus_Prime
- Gabriel: he taunts and stands still between strings of 2 combos (3 on Violent+): a built-in punish window. Spear
  lunges "will not damage a sliding player", so the slide is the designed answer, and the thrown weapons are parryable.
  The rapier combo starts only at close range. HP is 50 + 50. While he falls between phases, damage is halved.
  https://ultrakill.wiki.gg/wiki/Gabriel,_Judge_of_Hell
- V2 (a mirror boss): wing colour tells its movement mode: gold = run until it hits something, blue = circle-strafe
  while advancing, red = rush (the only mode when enraged), green = retreat. Keeping your distance drains its "patience"
  and triggers enrage; on Brutal, "movement and attacks are 50% faster". HP is 40 (first fight), then 50 + 30.
  https://ultrakill.wiki.gg/wiki/V2
- The Minos design "requires players to use every movement option"; three or four hits kill the player (search
  summary). https://steamcommunity.com/app/1229490/discussions/0/5782107002615891624/?l=tchinese

### Doom Eternal
- Marauder: at mid range he lunges with his axe, telegraphed by his eyes flashing green plus a sound cue. That is the
  only vulnerable moment; shooting too early is blocked by the shield. A hit "will falter the Marauder for a couple of
  seconds", and quick-swapping fits 2-3 shots into it.
  https://slayersclub.bethesda.net/en-AU/article/doom-eternal-boss-tips ; https://www.gamerevolution.com/?p=639744
- Hugo Martin: the falter lasts long enough for one shot with most weapons, "just long enough for two shots" if you
  swap. The Marauder "steer[s] you into the fun zone" (search summary).
  https://www.pcgamer.com/au/doom-eternals-director-says-the-marauder-is-good-actually
- The Doom Hunter's shield is weak to the plasma rifle and regenerates while its sled is intact. Breaking the sled
  ("CRITICAL DAMAGE") removes the shield and sled attacks, and makes it faster and more aggressive.
  https://doomwiki.org/wiki/Doom_hunter (search summary) ; https://slayersclub.bethesda.net/en-AU/article/doom-eternal-boss-tips
- Gladiator: phase 1 has the same eye-flash cue for when to shoot. Phase 2 pins you between two massive walls and fires
  shockwaves down the corridor, so the answer is gap reading and dashing ("dash liberally").
  https://slayersclub.bethesda.net/en-AU/article/doom-eternal-boss-tips
- Icon of Sin: it "becomes more aggressive with its movements as you deal damage to it", which is why the guidance is
  to break the small, fast-moving parts (the hands) first. https://www.shacknews.com/article/117086/icon-of-sin-boss-fight-doom-eternal
- Doom 2016's AI shoots less accurately at a fast-moving player: "demons deliberately miss when you're moving top speed
  and are more accurate the slower and less mobile you are." https://80.lv/articles/cyber-demons-the-ai-of-doom

### Others
- Titanfall 2: reviewers called the titan boss fights "slow-moving battles of attrition" (run to batteries, then trade
  damage). The exception is Viper, fought on a speeding ship against a hovering titan.
  https://www.trueachievements.com/game/Titanfall-2/reviews
- Ghostrunner: one hit kills on both sides. Hel is beaten by timing a melee parry against her melee 4 times; Mara by
  dodging until she stabs a tentacle into an electric box. https://gamepretty.com/ghostrunner-how-to-deal-with-the-bosses/
- Neon White: "a boss fight is just another level"; the boss fights keep the stopwatch and medal ranks and "don't
  fundamentally alter the gameplay". https://www.thegamer.com/neon-white-celeste-boss-fights-cohesive-dark-souls/
- Severed Steel: bullets cannot hurt you while you slide, wall-run or dive, and bullet time is unlimited during stunts,
  so moving itself is the defence. https://Techraptor.net/gaming/reviews/severed-steel-review
- Hi-Fi Rush, final boss Kale: "attacks in retaliation", and many of his attacks are on-the-fly Rhythm Parries. A large
  Stun Gauge must be emptied before "a brief damage window"; fire waves must be jumped.
  https://www.trueachievements.com/game/HiFi-Rush/walkthrough/19
- Telegraph timing ladder (a secondary 2026 blog): low punishment 450-650 ms, medium 650-900 ms, high "900ms and above".
  One primary shape per attack; 3-5 attack families per boss; keep the same timing inside a family.
  https://gamineai.com/blog/enemy-telegraph-shape-language-top-down-boss-fights-fast-visual-consistency-audit-2026
- Sekiro: every unblockable "perilous" attack shows the same red kanji; the player reads the animation to pick the
  answer (sweep = jump, thrust = Mikiri / deflect, grab = dodge). https://sekiroshadowsdietwice.wiki.fextralife.com/Combat
- God of War: ring colour over the enemy: red = unblockable (dodge), yellow = block / parry, blue = shield-bash to
  interrupt. https://playerassist.com/meaning-colored-ring-attacks-god-war-ragnarok/
- Animation (GDC 2014, Skullgirls): anticipation frames are what make an attack readable; "even a frame of
  anticipation" helps. https://80.lv/articles/tips-on-making-fluid-and-powerful-animations

## 2. Arena zones that restrict a verb or grow per phase

- Doom Eternal, Khan Maykr: lasers from the sky and fire on parts of the floor. As the fight goes on, she "will cover
  more and more of the arena in electrified red matter, reducing the number of areas you can safely stand in" (search
  summary). The fetched Shacknews page confirms the hazards but not the growth.
  https://www.pocket-lint.com/games/news/bethesda/151787-how-to-beat-bosses-doom-eternal-tips-and-tricks-marauder-hunter-gladiator-khan-maykr-icon-of-sin ;
  https://www.shacknews.com/article/117083/khan-maykr-boss-fight-doom-eternal
- Doom Eternal, Gladiator phase 2: the arena is narrowed to a corridor between two walls with shockwaves running
  through it. https://slayersclub.bethesda.net/en-AU/article/doom-eternal-boss-tips
- Doom Eternal, Icon of Sin: phase 2 moves to a second arena, and the boss walks around it.
  https://www.shacknews.com/article/117086/icon-of-sin-boss-fight-doom-eternal
- Furi: melee duels happen in "a small arena defined by the blue ring that you cannot walk beyond. You lose the ability
  to use your gun." This is a verb lockout signalled by a coloured boundary ring.
  https://itch.io/blog/772886/game-analysis-of-core-systems-furi
- ULTRAKILL, Leviathan: phase 3 destroys the boat platform and replaces it with the boss's slowly rotating midsection
  (a moving floor). Phases start at 75 % and 50 % HP. https://ultrakill.wiki.gg/wiki/Leviathan
- ULTRAKILL, V2 arena (1-4): four breakable pillars and a pit; falling in "deals 50 DP ... and teleports them back to
  solid ground" (a soft penalty, not death). https://ultrakill.wiki.gg/wiki/1-4
- ULTRAKILL system-level verb limits: at most 3 wall jumps in the air, reset by touching the ground or an Enemy Step;
  stamina does not recharge while sliding. https://ultrakill.wiki.gg/wiki/Movement
- Super Meat Boy 3D, Maggot Larry: after phase 1 the outer edge of the floor breaks away and the arena shrinks.
  https://games.gg/de/super-meat-boy-3d/guides/super-meat-boy-3d-maggot-larry-besiegen-guide/
- Metal: Hellsinger, Red Judge phase 2: floor plates fall away into lava.
  https://primagames.com/tips/how-to-defeat-the-red-judge-aspect-in-gehenna-stage-5-boss-in-metal-hellsinger
- Signalling usable surfaces: Doom Eternal marks climbable walls with a distinct white creased surface and green
  lights, which keeps the platforming "well signposted".
  https://twinfinite.net/guides/doom-eternal-climb-grab-walls/ ; https://blog.bestbuy.ca/video-games/doom-eternal-review
- Mirror's Edge Runner Vision colours usable traversal objects red; the inverse rule (unmarked means unusable) is
  implied. https://ea.com/news/runners-vision-in-mirrors-edge-catalyst
- ULTRAKILL hookpoints are colour-coded by behaviour: green pulls you to it, blue slings you past it, yellow suspends
  you and activates objects. https://ultrakill.wiki.gg/wiki/Whiplash
- Explicit "this wall cannot be wall-run" marking in a shipped FPS boss arena: not found.

## 3. Boss AI that stays in the arena and reads as intelligent

- Doom 2016 attack tokens: each attack type (melee, ranged, charged) has a limited token pool; a demon must hold a token
  to attack. On-screen demons can steal tokens from off-screen ones; pool size scales with difficulty.
  https://www.gamedeveloper.com/design/cyber-demons-the-ai-of-doom-2016- ;
  https://serfofcinder.itch.io/archetype-portal-to-hell/devlog/738495/enemy-ai-tech-talk
- Doom 2016 positioning inverts cover: AI picks "open positions near cover that provide maximum visibility to the
  player". https://80.lv/articles/cyber-demons-the-ai-of-doom
- During a glory kill, the AI cannot start a new attack (a guaranteed safe window); Doom uses hierarchical FSMs.
  https://www.gamedeveloper.com/design/cyber-demons-the-ai-of-doom-2016-
- GDC 2018 "Embracing Push Forward Combat in DOOM" (Loudy, Campbell): "advance or die"; attacks scatter around the
  player ("appearing to aim while missing"); stagger knockbacks visibly open a window; each demon has a counter-weapon.
  https://game.watch.impress.co.jp/docs/news/1113403.html ; https://gdcvault.com/play/1024186/Bringing-Hell-to-Life-AI
- Hugo Martin treats the AI as "chess pieces" that keep the player "moving and thinking at the same time" (search
  summary). https://www.fandom.com/articles/doom-eternal-marty-stratton-hugo-martin
- Halo (Butcher and Griesemer, GDC 2002, "The Illusion of Intelligence"): playtesters rated tougher enemies as smarter
  with identical behaviour; Legendary mainly adds health, so AI "lived longer and ... did more cool clever things".
  https://www.cs.du.edu/~sturtevant/s13-ai/lecture2.pdf ;
  https://www.jmeiners.com/shamans/papers/ai/the_illusion_of_intelligence.pdf
- ULTRAKILL V2's movement modes (strafe-advance, rush, retreat, run-to-wall) are announced by wing colour, so the AI's
  intent is readable. https://ultrakill.wiki.gg/wiki/V2
- Hades, Theseus and Asterius: Theseus "tends to stick to the middle of the arena" while Asterius chases you the whole
  time (a centre-anchored boss plus a chaser). Theseus's spear throw shows a visible crosshair before a short delay.
  https://primagames.com/tips/hades-boss-guide-how-beat-theseus-and-asterius (search summary)
- Arena-centre bias as a patch: a boss changed to "dash to the center of the arena" before certain attacks, to fix
  attacks failing at the edge. https://kleiforums.com/forums/topic/166202-fixed-celestial-scion%E2%80%99s-ground-attack-failing-at-the-edge-of-the-arena/
- Leashing (MMO convention): the AI chases only within a radius of home and returns when it leaves.
  https://warcraft.wiki.gg/wiki/Leash
- Orbit / strafe: move along the AI's right vector while facing the player.
  https://discussions.unity.com/t/enemy-ai-orbit-strafe-around-player/575271
- God of War (Denny Yeh): "The important question isn't 'What can this enemy do?' it's 'What can this enemy make the
  player do?'" Some attacks have multiple valid answers; the Valkyries are beatable "by a level 1 Kratos without taking
  any damage". https://blog.playstation.com/2019/02/01/how-santa-monica-studio-created-god-of-wars-greatest-challenge/
- Phase-transition conventions: Doom Hunter shouts "CRITICAL DAMAGE" and changes its moveset; Minos gets a voice line
  plus blue flames; Gabriel launches up, is stunned and takes half damage while falling.
  https://doomwiki.org/wiki/Doom_hunter ; https://ultrakill.wiki.gg/wiki/P-1 ;
  https://ultrakill.wiki.gg/wiki/Gabriel,_Judge_of_Hell
- ULTRAKILL dev interview (Hakita): difficulty comes from "the amount of enemies and their composition"; "the main
  thing that makes difficulty not feel like a grind is the pacing". https://intothebluesky.com/2021/10/05/follow-up-interview-with-hakita/
- A Furi / Hades / ULTRAKILL GDC talk on boss AI: not found.

## 4. Bosses that reward the grapple

- Doom Eternal, Khan Maykr: the only way to damage her is to break her shield, then Meat Hook (super shotgun) to her
  chest and Blood Punch her, "around six times". https://www.shacknews.com/article/117083/khan-maykr-boss-fight-doom-eternal
- Doom Eternal, Marauder: "Use your Super Shotgun's Meathook to launch over the Marauder and get some breathing room."
  https://slayersclub.bethesda.net/en-AU/article/doom-eternal-boss-tips
- Titanfall 2: grappling a titan rodeos it; on the first rodeo the pilot pulls the battery automatically. A stolen
  battery inserted into a friendly titan charges its shield and "add[s] 20% to the Core meter". If you already carry a
  battery, you drop a frag grenade into the socket instead.
  https://strategywiki.org/wiki/Titanfall_2/Gameplay ; https://titanfall.wiki.gg/wiki/Rodeo
- Titanfall 2 grapple on pilots pulls them closer; if the user is midair, both are pulled together.
  https://titanfall.wiki.gg/wiki/Grapple
- Sekiro, Divine Dragon: you grapple between trees, and a lightning-struck tree is the one to grapple to; the dragon
  is damaged only by a Lightning Reversal. Phase 1 uses a grapple-able root for an aerial deathblow on all the minor
  dragons. https://pushsquare.com/news/2019/04/guide_sekiro_shadows_die_twice_-_how_to_kill_divine_dragon ;
  https://gamewith.net/sekiro/article/show/8702
- Miyazaki on why not every boss is a grapple boss: "If we made every boss one where you were able to use the grappling
  hook to zip everywhere ... it would just get monotonous." https://gameinformer.com/2019/01/11/sekiro-shadows-die-twices-new-combat-options-means-bosses-are-a-little-different
- ULTRAKILL Whiplash: medium and heavy enemies (bosses) pull the player to them, so it is a gap closer, with a
  "0.5 seconds" cooldown. On Standard+, reeling adds Hard Damage at 60/s, up to 50 (a cost on the hook).
  https://ultrakill.wiki.gg/wiki/Whiplash ; https://ultrakill.wiki.gg/wiki/Arms
- ULTRAKILL Leviathan: the phase 3 laser sweep is slow at the start, which "allows grappling opportunities".
  https://ultrakill.wiki.gg/wiki/Leviathan
- Bulletstorm: the leash yanks enemies in, slowed and airborne, for a kick or a skillshot. Minibosses: strip armour,
  then thump. https://gamesradar.com/bulletstorm-skillshot-guide/3
- Crimson Desert, Queen Stoneback Crab: grapple to swing around the body and break silvery weak points, then grapple
  onto it as a gap closer for the finisher. https://thegameswiki.com/crimson-desert/wiki/grappling-hook

## 5. Brawler boss: close range rewarded, far range weak

- Marauder distance bands: close = super shotgun, mid = axe lunge (the only vulnerable moment, green eyes), long =
  thrown axe energy wave. The guidance is to hold mid range, "around 10-15 feet" (search summary).
  https://doomwiki.org/wiki/Marauder ; https://gamespot.com/articles/doom-eternal-marauder-boss-guide-how-to-kill-the-g/1100-6475129/
- ULTRAKILL Hideous Mass: the shell takes no damage "unless via punches or the Alternate Shotgun" and reflects
  hitscans and projectiles; belly and tail take 300 %. HP is 175 (140 + 35). "Shotgun swapping and punching the abdomen
  or tail with the Knuckleblaster is one of the most effective ways." https://ultrakill.wiki.gg/wiki/Hideous_Mass
- ULTRAKILL Leviathan: an armoured skull "reflects all projectiles and hitscans"; the weak heart shows after its orb
  barrage, when it "looks downwards". https://ultrakill.wiki.gg/wiki/Leviathan
- ULTRAKILL heals only "by attacking an enemy at close range" (a punch heals 10-30). Hard Damage is 35 % of damage taken
  and drains at 15/s. This is the systemic reason to stay close. https://ultrakill.wiki.gg/wiki/Health
- ULTRAKILL V2 punishes staying away: its patience drains while you keep your distance, then it enrages.
  https://ultrakill.wiki.gg/wiki/V2
- Sekiro posture: bosses regain posture quickly, so "you cannot hit them a bit and back off"; you stay in their face
  and deflect. https://www.superjumpmagazine.com/the-art-and-science-of-sekiros-combat/
- Sekiro deflect window: 30 frames (about 0.5 s), shrinking to 7 frames if guard was recently released (anti-mash).
  https://sekiroshadowsdietwice.wiki.fextralife.com/Deflection
- Furi: the pistol does "minimal" damage unless charged; a charged melee can drain a whole HP bar. The parry heals
  (0.5 HP); a last-moment parry stuns the boss for a counter. The player has 3 lives; bosses have 3-8 phases shown as
  pips under the bar. https://godisageek.com/reviews/furi-review/ ; https://tvtropes.org/pmwiki/pmwiki.php/VideoGame/FURI
- Devil May Cry 5: melee deals more damage and fills the Exceed gauge faster than guns; Vergil deflects gunshots while
  standing. https://www.digitaltrends.com/gaming/devil-may-cry-5-weapons-and-best-abilities-guide-dante-nero-and-v/
- Punch-Out!!: every move has a tell (head wobble, fist circling, a flashing jewel). Early opponents' eyes turn yellow
  before punches; later ones drop this tell. https://strategywiki.org/wiki/Mike_Tyson's_Punch-Out!!/Major_Circuit
- Fairness rule (God of War Valkyries): beatable at level 1 with no damage taken; multiple answers per attack (arrows =
  safe, less reward; axe = risky, more reward).
  https://blog.playstation.com/2019/02/01/how-santa-monica-studio-created-god-of-wars-greatest-challenge/
- Fight-length targets: some developers plan 3-5 minutes per boss (search summary of a design discussion; not checked
  in the page). https://gamesbeat.com/the-modern-boss-fight/ ULTRAKILL boss levels give a ceiling: 1-4 (corridor plus
  V2) S-rank time is 02:15. https://ultrakill.wiki.gg/wiki/1-4 A published 1-2 minute target for movement-shooter
  bosses: not found.

## 6. Making the objective obvious

- Doom Eternal states the weak point in the cue itself: green eyes = shoot now; a shield that regenerates while the
  sled lives = break the sled; the Icon of Sin's 8 armoured parts per stage (shooting bare parts "has no effect").
  https://slayersclub.bethesda.net/en-AU/article/doom-eternal-boss-tips ;
  https://www.shacknews.com/article/117086/icon-of-sin-boss-fight-doom-eternal ;
  https://doomwiki.org/wiki/Icon_of_Sin_(Doom_Eternal) (search summary)
- Doom Eternal later added "tool tips for demon tutorials" in an update (QoL), and has small instanced tutorial rooms
  for new weapons. https://stuff.co.za/2020/05/11/doom-eternal-update-adds-some-stronger-demons-and-quality-of-life-improvements/
- Doom's destructible demons show damage on the body as you shoot, so health reads without a bar (search summary).
  https://bloody-disgusting.com/video-games/3515304/doom-eternal-creative-director-dishes-new-demon-destruction-system/
- Doom 2016: staggered enemies glow to signal a glory kill is available. https://game.watch.impress.co.jp/docs/news/1113403.html
- Hugo Martin: the Marauder's basic lesson is weapon swapping, with the falter sized for one or two shots (search
  summary). https://www.pcgamer.com/au/doom-eternals-director-says-the-marauder-is-good-actually
- Metroid Prime chose particular yellow and red colours for weak points on bosses and enemies so players see where to
  shoot (search summary). https://www.vgfacts.com/game/metroidprime/trivia-16442
- Monster Hunter damage numbers colour-code effectiveness: white = poor spot, yellow / orange = weak spot.
  https://steamcommunity.com/app/582010/discussions/0/1735462352466166513
- Cuphead hides the boss health bar (players would chase DPS and miss attack cues); progress is shown after death and
  hits flash the enemy white. https://nintendoeverything.com/cuphead-lack-of-health-bars-explained/
- Indie playtest fix: add health bars to boss parts so players know to attack them, and flash parts when hit; another
  developer found the vulnerable state "pretty much impossible to tell" without an animation and sound (search summary,
  student or indie blogs). https://babel.speldesign.uu.se/?p=2085275
- Zelda convention: the dungeon item is the key to the boss's weak point (a bounded answer set), traditionally with
  "three strikes". https://www.zeldadungeon.net/how-to-design-your-ultimate-dungeon-boss/
- ULTRAKILL teaches its tells globally (yellow / blue flash, voice lines), so a new boss needs no new instructions.
  https://ultrakill.wiki.gg/wiki/Parry
- Hollow Knight's Grimm is cited as fair because every attack has a clear animation, consistent speed and a dodge
  window. https://choostgames.com/blog/what-makes-a-good-boss-fight/
- On-screen text conventions for first boss phases (e.g. a "SHOOT THE CORE" banner) in a shipped FPS: not found.

## Numbers table

| Item | Number | Source |
|---|---|---|
| Telegraph, low / medium / high punishment | 450-650 / 650-900 / 900+ ms | https://gamineai.com/blog/enemy-telegraph-shape-language-top-down-boss-fights-fast-visual-consistency-audit-2026 (secondary blog) |
| Visual / auditory reaction time | 180-200 / 140-160 ms | appeared in the telegraph search summary; source page not pinned, not checked |
| Sekiro deflect window | 30 frames (~0.5 s); 7 frames after a recent guard release | https://sekiroshadowsdietwice.wiki.fextralife.com/Deflection |
| ULTRAKILL projectile mercy frames | 3 frames | https://ultrakill.wiki.gg/wiki/Arms |
| Minos Prime punch-combo ground-stuck window | "a second" | https://ultrakill.wiki.gg/wiki/Minos_Prime (search summary) |
| Marauder falter | "a couple of seconds"; 1 shot, 2 with quick swap | https://www.gamerevolution.com/?p=639744 ; PC Gamer (search summary) |
| Marauder sweet-spot range | ~10-15 ft (~3-4.5 m) | doomwiki / gamespot (search summary) |
| Khan Maykr hook + blood punches to kill | ~6 | https://www.shacknews.com/article/117083/khan-maykr-boss-fight-doom-eternal |
| Icon of Sin armour parts per stage | 8 | https://www.shacknews.com/article/117086/icon-of-sin-boss-fight-doom-eternal |
| ULTRAKILL boss HP (phase split) | Minos 65+65; Gabriel 50+50; Sisyphus 100+100; V2 40 / 50+30; Hideous Mass 140+35; Leviathan 240 | ultrakill.wiki.gg pages above |
| Weak-point multiplier (Hideous Mass) | 300 % (4.5x with revolver limb bonus) | https://ultrakill.wiki.gg/wiki/Hideous_Mass |
| V2 on Brutal | moves and attacks 50 % faster, +50 % HP | https://ultrakill.wiki.gg/wiki/V2 |
| ULTRAKILL Whiplash cooldown | 0.5 s; reel Hard Damage 60/s, cap 50 | https://ultrakill.wiki.gg/wiki/Whiplash |
| ULTRAKILL wall jumps in air | max 3 | https://ultrakill.wiki.gg/wiki/Movement |
| ULTRAKILL dash stamina regen (Standard) | 0.7/s per bar, 1 per dash | https://ultrakill.wiki.gg/wiki/Movement |
| ULTRAKILL Hard Damage | 35 % of damage taken, drains 15/s | https://ultrakill.wiki.gg/wiki/Health |
| Furi parry heal / player lives / boss phases | 0.5 HP / 3 / 3-8 | https://tvtropes.org/pmwiki/pmwiki.php/VideoGame/FURI ; https://godisageek.com/reviews/furi-review/ |
| Titanfall 2 battery to friendly titan | +20 % Core meter | https://titanfall.wiki.gg/wiki/Rodeo |
| ULTRAKILL 1-4 S-rank (corridor + V2) | 02:15 total | https://ultrakill.wiki.gg/wiki/1-4 |
| Designer boss-length plans (generic) | 3-5 min | https://gamesbeat.com/the-modern-boss-fight/ (search summary, not checked) |
| Movement-shooter boss 1-2 min target, published | not found | |
