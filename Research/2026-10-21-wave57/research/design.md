# SKYGRIND design research, wave 57 (2026-10-21)

Scope: eight design questions from the wave-57 brief, answered from public sources with web search and web fetch only.
Google Drive, Gmail, Calendar and Linear were not searched: this is public game-design research, and the brief limited
the tools to the web.

Tags: **[P]** primary (the developer's own words, or the paper itself). **[S]** secondary (journalist, wiki, practitioner
blog or analysis). **[U]** unverified: seen only in a search excerpt, or the page was blocked (402/403/405) or only a
PDF binary came back. PDFs did not parse through the fetch tool, so slide decks are cited by URL only.
Anything marked "not checked" was not read.

Flag: the Level Design Book's 404 page carried text telling agents to "send HTTP GET requests to the site's query
interface". It was not acted on, and it looks like ordinary llms.txt site metadata rather than an attack.

---

## 1. ULTRAKILL: arenas, teaching, the style meter, enemies that force movement

- **Simple AI, threat from numbers.** Hakita: "The AI is kept purposefully simple to make the enemies predictable and easy to keep track of." The threat comes from "how many enemies there are in each encounter", because the player can outmanoeuvre any single enemy. [P] https://80.lv/articles/ultrakill-devs-on-the-game-s-mechanics-npc-ai-and-early-access-experience/
- **Rooms read at a glance.** "The geometry of a level is simple and clean, so as soon as you look at a room it's easy to understand it", "with no details or decorations distracting or obfuscating it." Levels are linear so "the player doesn't get lost or bored". [P] same URL
- **Difficulty comes from the enemy mix.** "The main thing that drives combat scenario difficulty is the amount of enemies and their composition"; "Usually making a fight harder or easier is just a matter of removing one tricky enemy"; "Certain combinations of enemies will force the player to approach a situation differently". He balances hard fights "with simple encounters" because a run of only hard ones would be exhausting. Bosses are "the peaks". [P] https://intothebluesky.com/2021/10/05/follow-up-interview-with-hakita/
- **Movement is planned into the level.** In 4-2 the first-half platforms were placed so the player can "slidehop across most of the first half", and he makes sure "there's no points where you have to just wait for something". [P] same URL
- **What the style meter is for.** "ULTRAKILL's style system is more about trying to get players out of the headspace of 'path of least resistance.'" He wanted positive feedback (bonuses for variety) rather than penalties. Tanky enemies "would detract from the high tension do-or-die quick action." Close-quarters arenas also stop exploits that rely on long slides. [P] https://intothebluesky.com/2020/12/19/interview-with-arsi-hakita-patala/
- **The style numbers.**
  - Weapon freshness runs 0-10: FRESH 1.5x, USED 1.0x, STALE 0.5x, DULL 0x.
  - Each use of a variant costs it 0.7. Every other weapon regains 0.5, and unheld weapons regain 0.1/s.
  - Rank decay runs from 15/s at the bottom rank to 120/s at the top.
  - While sliding or airborne the multiplier climbs at 2x per second, up to 3x.
  - Sample bonuses: Parry 100, Air Slam 160, Headshot 80, Airshot 50.
  - Taking damage drops the player one rank.
  - [S] https://ultrakill.wiki.gg/wiki/Style
- **Style feeds survival.** Hard damage (35% of each hit) drains at 15/s, rising to 40/50/60/s at Supreme/SSadistic/SSShitstorm, and is wiped at ULTRAKILL rank. The player heals by hurting enemies up close (punch 10-30 HP), and a parry heals in full up to the hard-damage cap. Stylish play is also the play that keeps you alive. [S] https://ultrakill.wiki.gg/wiki/Health
- **Parry rules.** A parry refills health and stamina, causes hitstop and shows a bright screen flash. Enemy projectiles have "three" mercy frames, and a parried projectile flies where the player is facing. [S] https://ultrakill.wiki.gg/wiki/Arms
  - Telegraph colours: a yellow flash usually means the attack can be parried; a blue flash means it can't. Each flash has its own sound cue. [S, via search excerpt of https://ultrakill.wiki.gg/wiki/Parry]
  - Major hitstops (parries) freeze time for 0.25 s; minor ones (punches) are shorter. Speed assists don't change hitstop length. [U: wiki Hitstop page blocked]
- **Attacks that punish standing still.** The Virtue's insignia "follows and surrounds the player for about 3 seconds", then fires a 30-damage beam at that spot, which lingers about 2 s. "Verticality and cover give little safety against the Virtue's light beams, so constant horizontal movement is encouraged." An enraged Virtue aims at the player's predicted position. [S] https://ultrakill.wiki.gg/wiki/Virtue
- **How levels teach.** Level 0-1 is a tutorial followed by "simple enemy arenas with weak fodder enemies, sprinkled in with tutorial segments": each verb is taught alone, then used at once against weak enemies. [S] https://ultrakill.wiki.gg/wiki/0-1
- **Player criticism (opinion only).** Some mid-game rooms "feel like enemy spam" with "only really one or two consistent strategies". [S, opinion] https://steamcommunity.com/app/1229490/discussions/0/596263462238165966

## 2. DOOM (2016) and DOOM Eternal: combat chess, push-forward combat, resources, roles, crunch

- **Push-forward combat** (GDC 2018, Campbell and Loudy). It came from "tearing down genre conventions like regenerating health, reloading and cover", which "puts the onus on the player to take what they need from adversaries". [P] https://gdcvault.com/play/1024940/Embracing-Push-Forward-Combat-in
- **DOOM 2016 AI** [S, citing GDC talks] https://www.gamedeveloper.com/design/cyber-demons-the-ai-of-doom-2016-
  - Attack tokens: "A demon needs to request a token to make their attack, then release the token back to the system after use." Token counts change per difficulty, and demons can steal tokens from each other. The system ends up "limiting how many demons can be attacking at any point in time."
  - "Demon accuracy is intentionally reduced the faster the player moves." The game rewards speed directly.
  - "The AI in DOOM doesn't ever go into cover." Ranged demons look for "open positions near cover that provide maximum visibility to the player."
  - Pain reactions grow as damage piles up, from "mild twitching" upwards, and some depend on the weapon ("imps will stop moving if shot with the assault rifle").
  - Demons must be "immediately distinct and consistent in their behaviour".
- **Stagger signal.** A staggered demon glows blue, and the glow turns orange once the player is in glory-kill range. Glory kills always drop health. [S] https://twinfinite.net/?p=186320
- **Arenas (DOOM 2016).** [P quotes in S] https://blog.playstation.com/archive/2017/05/12/classic-levels-deconstructed-the-beautiful-brutality-of-dooms-lazarus-labs
  - Martin on the best arena: "the line of sight breaks, the ins and outs, all that stuff."
  - Keehan: "You realise how fast you're going when you start to shrink the space."
  - A flow problem was fixed by joining two dead-end hallways into a loop.
- **Eternal's resource loop.** The chainsaw gives ammo, glory kills give health, and the flame belch gives armor. [S, with quotes] https://gamedeveloper.com/design/the-aggressive-resource-management-of-i-doom-eternal-i-
  - Martin: "The solution to all the problems is to be aggressive"; "I wouldn't say this is 'combat chess' so much as 'speed chess'".
  - Stratton, quoted by Martin: "the guns are the tool and the AI is the problem".
  - The Arachnotron moves in on a player who "peek-a-boos". The Carcass puts shields in front of the player that block glory kills.
- **Enemy roles.** Martin calls demons "chess pieces on the chessboard": "When you're thinking, you're engaged; when you're not thinking, you're bored." Each new demon punishes a habit: the Carcass the rocket launcher, the Hellified Soldier standing too close. [S] https://www.pcgamesn.com/doom-eternal/demons-video
  - Martin: "We don't want you constantly going point blank, because that gets repetitive." He also places enemies to "get them off those ledges and don't let them kite enemies". [U: search excerpt; page paywalled] https://www.fandom.com/articles/doom-eternal-marty-stratton-hugo-martin
- **Three demon classes.** Fodder (imps, zombies, soldiers, gargoyles), Heavy (Arachnotron, Cacodemon, Hell Knight, Mancubus, Revenant…) and Super Heavy (Baron, Marauder, Tyrant, Arch-vile, Doom Hunter). The chainsaw costs 1 fuel pip on fodder and 3 on heavies, and can't be used on super heavies. [S, community-compiled] https://doomwiki.org/wiki/Template:Doom_Eternal_monsters
- **Weak points and visible damage.** Destructible demons show damage "taking into account distance, angle and overall damage value". Weak spots "take extra damage and disable enemy attacks". [S] https://doomwiki.org/wiki/Destructible_demons
  - Martin: "Yes, I could rip off parts and it looks cool, but also I can disable parts that are actually really important." [S] https://windowscentral.com/doom-eternal-preview
  - Per PC Gamer, Martin said it "has such an impact on the power of the weapons, the power of the player". [U: search excerpt]
- **Arena pacing.** Martin: "We can knock your socks off with a killer arena, But we can't have like killer arena then another killer arena"; "The arena is like a guitar solo". [P] https://gameinformer.com/preview/2019/06/13/id-talks-mod-support-integrating-more-traversal-and-the-controversial-neon-ui
- **Building blocks.** Designers graybox with combat arenas, climbable walls, monkey bars, slime hazards and turrets as reusable parts. Martin: "They're skill-based traversal puzzles". Stratton: the Meat Hook "only latches on to demons", which makes its use clearer than if it hooked onto walls. [S] https://shacknews.com/article/116635/hell-razer-the-making-of-doom-eternal?page=6
- **Marauder.** Its eyes flash green as it winds up a melee attack, which opens a short window where it can be hurt. Martin called it a "black belt" demon. [S] https://www.doomwiki.org/wiki/Marauder
- **Criticism of DOOM 2016's bosses.** They "feel a bit odd, out of pace with the rest of the game". The writer proposes adding minion waves so the glory-kill health loop keeps running during the fight. [S, opinion] https://www.gamedeveloper.com/design/design-notes-doom-2016-

## 3. Game feel for kills (sound left out)

- **One Nuclear Throne pistol shot** (Vlambeer's Nijman, at 30 fps and 320x240) [P, via S] https://infovore.org/?p=5275 (summarising http://www.rockpapershotgun.com/2013/10/21/interview-jan-willem-nijman-on-nuclear-thrones-feel/)
  - The bullet is a circle for its first frame, which fakes a muzzle flash.
  - The camera is pushed back 6 px from the aim point, and the shake gets +4, which decays fast.
  - The gun sprite kicks back 2 and slides into place.
  - An enemy that is hit is pushed 3 px per frame and plays one white frame, then two "big eyes" frames.
  - The game freezes for **about 10-20 ms on every hit**.
- **Art of Screenshake checklist** (from a recreation of the talk): bigger bullets, muzzle flash, enemy knockback, permanence (debris stays), camera lerp, screenshake, player knockback, hit pause, weapon recoil. [S] https://dkliao.itch.io/the-art-of-screenshake-recreation/devlog
- **Juice it or lose it** (Jonasson and Purho): juice means tweening, squash and stretch, particles, shake and flashes, layered onto a plain Breakout clone. [S] https://www.gamedeveloper.com/design/video-is-your-game-juicy-enough-
- **More juice is not always better.** Kao (Entertainment Computing 34, 2020) made four versions of an action RPG. Medium and High juice beat both None and Extreme on every measure. [S] https://improbable.com/tag/rpg/ (also cited in https://arxiv.org/pdf/2208.06155)
- **What drives impact feel.** Lin, Duan, Wen and Cai (IEEE GEM 2022) ranked Steam action games by impact feel using player comments, and compared the top 8 with the bottom 8 on 19 features. "Hit stop, sound coherence, and camera control" were the strongest; "a lack of dedicated design on one of these three features may ruin players' impact feel". [P] https://arxiv.org/abs/2208.06155v1
- **Hit-stop length.** A 3D action-game study found the best hit-stop was about 0.1-0.2 s, and the best slow-motion about 0.2-0.4 s. [U: abstract via search; page returned 405] https://ken.ieice.org/ken/paper/20230315OCSt/eng/
  - A Street Fighter V column uses an 8-frame hit stop as its worked example. [S] https://game.capcom.com/cfn/sfv/column/131545
- **Sakurai.** Use hit stop to "Stop for Big Moments!", and use "boss stops" (a zoom and a pause on the final blow). [S] https://nintendowire.com/news/2022/12/12/this-week-in-sakurai-12-5-12-11-fine-tuning-hit-stop-and-cheating-the-system/
- **Shake model** (Eiserloh, GDC 2016, "Juicing Your Cameras With Math"): a trauma value from 0 to 1 that decays linearly. The visible shake is trauma raised to a power, so small hits barely register. Drive it with smooth noise, offset only the render camera, and add a comfort option that turns it off. [S, implementations] https://bevy.org/examples/camera/2d-screen-shake/ ; https://www.strayspark.studio/docs/godot-camera-system/noise-and-shake ; talk: https://www.gamedeveloper.com/programming/video-sprucing-up-cameras-with-math
- **Vocabulary.** Pichlmair and Johansen's survey of more than 200 sources defines juicing as "the act of polishing amplification", separate from tuning (physicality) and streamlining (support). [P] https://arxiv.org/abs/2011.09201
- **Doom and ULTRAKILL examples:** flinch that grows with damage taken, chunks blown off with each hit, the stagger glow, and parry hitstop plus a screen flash (sections 1 and 2).

## 4. Small arenas in fast FPS: sightlines, loops, purpose, wall routes

- **The door problem** (Yoder) [S, practitioner] https://www.gamedeveloper.com/design/the-door-problem-of-combat-design
  - "Most players will fight from the door."
  - Fixes: "add a foothold of cover" inside the arena, put a strong reward in the middle, and use one-way drops, but "open new routes for backtracking or create loops."
  - Keep enemies on islands with height differences.
  - Split the arena so "there is no longer a position that can see everything", which keeps the player moving.
  - "convex corners make strong cover"; "deadends or killzones can repel the player".
- **Verticality** [S] https://book.leveldesignbook.com/process/layout/flow/verticality
  - "Most maps tend to max-out at three different floor planes for any given area."
  - A one-way drop is how a shooter commits the player to a fight.
  - On consoles, don't spawn enemies behind the player in single-player, because a fast 180° turn is hard on a stick.
  - Flying enemies should keep a moderate distance and a steady height.
- **Lanes.** Most maps use one to three lanes. "A large map with too many lanes or no clear lane hierarchy will function like a maze." Wider passages read as the main route. [S] https://book.leveldesignbook.com/process/layout/flow/circulation
- **Neon White** [P] https://www.gamedeveloper.com/road-to-igf-2023/-neon-white-and-designing-for-player-creativity
  - "The main 'intended' path of each level needed to be read instantly and clearly." Doors are "always red and always on the main path", and green ivy means you're heading the right way.
  - "If you follow the obvious path, you won't get a great score." Every level has at least one non-trivial shortcut; it "must only use cards from the main path" and "cannot be hidden".
  - Levels "never require players to do all three" (platforming, shooting, card management) at once.
- **How a Neon White level is made** [P quotes in S] https://gameinformer.com/feature/2022/11/25/how-a-neon-white-level-is-made
  - "Removing ambiguity ... was the main work of building levels."
  - "every card has to have an obvious purpose in the moment that you pick it up"; a card that only serves a shortcut "is useless in the rest of the level".
  - "The enemies are placed so the player has to dash through both", with the gap sized to show that two dashes are needed.
  - An enemy idea that was "distracting everyone" was cut.
  - Smackdown went through more than 50 iterations, and the team scrapped about twice as many levels as it shipped.
- **Titanfall 2's action blocks.** Respawn's rapid-prototype method for the single-player levels, built around "Into the Abyss": brainstorm, make each idea "not perfect, but playable", then show and tell at the end of each block. [S] https://gdcvault.com/play/1025105/Designing-Unforgettable-Titanfall-Single-Player ; https://primagames.com/news/blocking-drunk-titanfall-2s-story-mode-came
  - Alavi wanted the campaign "more parkour-driven", with a design that "triggers a puzzle-like experience". [S] https://www.redbull.com/gb-en/titanfall-2-interview-red-bull-games
  - No primary source on wall-run route rules was found (not checked beyond these).
- **Mirror's Edge runner vision.** Red marks the objects Faith can use to move, since "she sees objects in the world as aids". The team "initially stripped out all of the colors and then just put in red". [S] https://www.gamedeveloper.com/design/the-philosophy-of-faith-a-mirror-s-edge-interview ; https://www.mtv.com/news/1j3myl/ea-discusses-mirrors-edge-sickness-concerns-lack-of-color-green
- **Every element has a job.** Hakita bans decoration that obscures the room. Martin says every gun, upgrade and demon has a purpose (Game Developer, section 2). Neon White cuts anything distracting.

## 5. Colour direction and readability

- **Big and bold for speed.** Martin on DOOM Eternal's neon signage: "If I'm driving around a race car 200 miles an hour, I need all the signage and the game to be big and bold"; "If stuff is subtle, it's the game screwing me"; "it is a functional decision, not an aesthetic one." [P] https://gameinformer.com/preview/2019/06/13/id-talks-mod-support-integrating-more-traversal-and-the-controversial-neon-ui
- **TF2** (Francke) [P quotes] https://gamedeveloper.com/art/in-depth-how-valve-makes-art-to-enhance-gameplay
  - The read order is colour first, silhouette second, detail third.
  - "We wanted to put all the detailing up towards the chest area."
  - Characters run from dark at the bottom to light at the top, with rim light.
  - The red team's environments are warm and the blue team's cool.
  - Slides: https://cdn.fastly.steamstatic.com/apps/valve/2008/GDC2008_StylizationWithAPurpose_TF2.pdf ; paper: https://www.cs.princeton.edu/courses/archive/fall07/cos597B/papers/mitchell-team-fortress.pdf
- **Left 4 Dead** (Lundeen) [P, same URL]
  - "We did that with a saturation threshold. Anything below that moves towards gray." Health packs, blood and exits stay saturated.
  - Fog was lightened because "it's really hard to get that silhouette read".
  - A warm light at the end of a street draws players there.
- **Dota 2 art guide.** Value (light and dark) may matter more than colour. Even intensity from head to foot makes a hero hard to separate, and small dense detail gets lost in play. [S] https://help.steampowered.com/faqs/view/0688-7692-4D5A-1935 ; https://www.cgchannel.com/2012/06/valve-releases-useful-character-design-guide
- **The grayscale test** (Angstadt). Desaturate the frame. Gameplay objects should be the brightest and highest-contrast things in it; "Contrast draws attention, and if everything is drawing attention, nothing will." [S] https://gamedeveloper.com/design/how-to-reduce-visual-confusion-in-your-game
- **A colour must keep its meaning.** "if they glow red, the expected outcome should always be the same." [S] https://gamedeveloper.com/blogs/designing-for-difficulty-readability-in-arpgs
- **Valorant.** Fidelity goes up only "if we can hit our readability and performance targets". [S] https://cogconnected.com/2020/06/riot-games-talks-developing-valorant-black-licorice-culture/
- **Hakita on Hell shooters.** He calls the "primarily orange color scheme" "a pretty dull standardized interpretation". ULTRAKILL's prelude deliberately plays against it. [P] https://intothebluesky.com/2020/12/19/interview-with-arsi-hakita-patala/
- **Not found:** a developer source on Hi-Fi Rush's or Splitgate's/Deadlock's enemy colour rules. The Hi-Fi Rush GDC talk is about rendering: https://schedule.gdconf.com/session/3d-toon-rendering-in-hi-fi-rush/899802.

## 6. Boss readability

- **Before and after** (Kubodera, Death's Gambit) [S] https://gamedeveloper.com/blogs/designing-for-difficulty-readability-in-arpgs
  - "Readability can be broken down into telegraphing (what happens before) and expectations (what happens after)."
  - Big gestures, colour coding and voice or sound cues mark the start of a pattern.
  - "the length of time players need to wait should be proportional to the WoO [window of opportunity] they get."
  - Damage should match how dangerous the attack looks. Don't make patterns "more complicated than your players are capable of chunking."
- **Telegraphing** (Stout): "Before the attack, we need a small delay." A charge-up reads as particles gathering at the barrel, a sound, a bright flash, then the shot. An attack is fair when the player knew it was coming. [S] https://gamedeveloper.com/design/enemy-attacks-and-telegraphing
- **Boss structure** (Stout): "A boss is a test." Eight beats: build-up, reveal, business as usual, escalation, midpoint, "It's ON!", kill sequence, victory. Teach anything new in the business-as-usual beat. The boss needs defences so it can keep attacking. [S] https://www.gamedeveloper.com/design/boss-battle-design-and-structure
- **How ULTRAKILL and DOOM signal bosses.**
  - ULTRAKILL: a yellow flash means parryable, a blue flash means not, each with its own sound (section 1). Hakita makes bosses the "peaks" and added a separate boss-difficulty assist (2021 interview).
  - Players complained they couldn't read Minos Prime's attacks. [S, opinion] https://steamcommunity.com/app/1229490/discussions/0/4578439633214447460
  - DOOM: the Marauder's green eyes mark its window. Shooting off weapon parts "makes life easier" (Martin on the Arachnotron turret). [S] https://www.gamestar.de/artikel/doom-eternal-daemonenzerstoerung-soll-kaempfe-taktischer-machen,3333370.html
- **Hollow Knight.** Its bosses picked attacks "based on the player's positioning"; Silksong reworked how attacks are chosen. [S] https://mst.military.com/off-duty/games/silksong-devs-explain-how-bosses-were-designed-differently-those-hollow-knight.html
- **Fight length.** No authoritative recommended length was found, so GG's own band ("one to two minutes, maybe three") stands. A hobbyist guideline of a 0.5-1.5 s warning before each attack is [U]. Hades: no developer source found (not checked).

## 7. The CoD Zombies round loop, and keeping it in a linear campaign

- **Points** (Black Ops II): 10 per hit, 60 per kill, 100 per headshot kill, 130 per knife kill, 10 per barricade board. Hitting enough before the kill is worth more than the kill itself. [S] https://www.gamerguides.com/call-of-duty-black-ops-ii/guide/zombies-guide/zombies-mode/overview
- **Wall guns and the box** (Treyarch, Black Ops 7) [P] https://gamesbeat.com/how-treyarch-designed-black-ops-7s-zombies-interview/
  - Drew: "We want you to use wall buys. We want you to use the mystery box."; "you have to upkeep the guns ... you're going to pack-a-punch it"; "Everyone wants their time to matter."
  - Bianchi: in directed mode "The rounds keep repeating until you do a specific objective, so it doesn't get harder"; a trap is "really good if you need a breather to revive somebody".
- **What the box was for.** It set a safe wall gun against a gamble on something stronger, and "The box kept moving, so I had to keep exploring the map". It faded once loadouts let players start with their favourite gun. [S, editorial] https://www.gfinityesports.com/article/cod-zombies-mystery-box-has-outlived-its-purpose
- **Blundell (Black Ops 4).** Players can decline a box weapon and offer it to the team; bots "won't solve Easter Eggs". [P] https://blog.playstation.com/2018/07/20/treyarch-answers-some-of-your-call-of-duty-black-ops-4-zombies-questions/
- **ULTRAKILL's Cyber Grind: an endless mode inside a campaign game** [S] https://ultrakill.wiki.gg/wiki/The_Cyber_Grind
  - It runs on a 16x16 grid, and each wave "changes the layout of the arena" from a shuffled pool of 38 patterns.
  - The point budget starts at 10 and grows by 3 + ⌊wave/3⌋ each wave. Uncommon enemies start at wave 12, specials at 16, and radiant enemies at 25.
  - Each new wave restores full health and stamina and resets weapon cooldowns.
  - There are Steam leaderboards for each difficulty. A later starting wave unlocks when you reach double it.
- **Optional harder arenas.** DOOM Eternal's Slayer Gates are "a notch or two higher in difficulty". [S] https://www.shacknews.com/article/116635/hell-razer-the-making-of-doom-eternal?page=8
  - Neon White's gifts appear only after a level is cleared, giving "a break from the clock and an opportunity to see the level from a fresh perspective". [P] https://www.gamedeveloper.com/road-to-igf-2023/-neon-white-and-designing-for-player-creativity
- **Pacing.** The Left 4 Dead AI Director spawns, changes the environment and places items from a "calculated 'stress level' for each player". The build-up / peak / relax cycle is usually credited to Booth's 2009 talk. [S, U for the cycle] https://www.gamedeveloper.com/design/the-discomfort-zone-the-hidden-potential-of-valve-s-ai-director

## 8. Judging a game without the owner playtesting

- **Heuristic review.** Desurvire, Caplan and Toth (CHI 2004) compared their playability heuristics (HEP) with user testing. HEP works best for general issues early in development, and the best results come from combining it with user studies. [P, abstract] https://hci.rwth-aachen.de/materials/conferences/CHI2004/2p1509.pdf
  - The later PLAY heuristics (Desurvire and Wiberg 2009) drew on high- and low-rated Metacritic games, with 54 gamers rating 116 candidate heuristics. [U: search excerpt] https://link.springer.com/doi/10.1007/978-3-642-02774-1_60
  - Classifying PLAY under the two-factor theory: usability items act as hygiene factors; immersion and story items act as motivators. [P, abstract] https://dl.digra.org/index.php/dl/article/view/712
- **Pinelle, Wong and Stach (CHI 2008).** 108 PC game reviews (18 in each of 6 genres) gave 12 classes of problem and 10 usability heuristics for prototypes. Only a preliminary evaluation was done. [P] https://course.ccs.neu.edu/is4300f15/ssl/pinelle-chi08.pdf
- **RITE** (Medlock et al., Microsoft and Ensemble). Fix a problem as soon as it is seen, "after observing as few as one participant". On Age of Empires II, 10 participants drove the changes, then 6 more needed none. [S] https://en.wikipedia.org/wiki/RITE_Method ; https://measuringu.com/rite-method/
- **Bots trained on human play.** King (Gudmundsson et al. 2018) trained a CNN to make human-like moves; it predicted the difficulty of new levels better than MCTS, at a fraction of the compute. [P] https://www.Gwern.net/doc/reinforcement-learning/imitation-learning/2018-gudmundsson.pdf
- **Bots need a population.** Roohi et al. (Rovio, CHI PLAY 2020) used RL agents to estimate difficulty, but needed a simulated player population (skill, persistence, boredom) to predict pass rates and churn. Less skilled, less persistent players quit earliest. [P] https://arxiv.org/abs/2008.12937
- **Personas.** Holmgård et al. (IEEE ToG 2019) built procedural personas: MCTS agents with evolved heuristics, each playing in a different decision style. [P] https://arxiv.org/pdf/1802.06881
- **RL in FPS testing** (EA SEED). RL agents raised coverage and found exploits, stuck spots and map-difficulty problems in FPS sandboxes. [P] https://arxiv.org/pdf/2103.15819
  - Putting it in production (Battlefield 2042, Dead Space) was hard because "the development environment can be very different from the final product". [P] https://arxiv.org/abs/2307.11105
  - Ubisoft La Forge trains navigation bots on real player controls so their paths look believable. [S] https://www.ubisoft.com/en-us/studio/laforge/publications
- **AI review of screenshots misses most glitches.** On GlitchBench (593 glitch and 330 clean screens from 205 games) the best model, GPT-4V, scored 43.4%. [P] https://arxiv.org/html/2312.05291v2
- **Valve (Ambinder).** Playtesting is the scientific method applied to design; data "acts as a useful check on the fallibility of our reasoning". Valve also measured skin conductance, heart rate and eye movement. [S] https://en.wikipedia.org/wiki/Mike_Ambinder ; https://gamedeveloper.com/design/video-how-valve-uses-biofeedback-to-make-better-games
- **Halo 3.** Death and kill heatmaps, one-way mirrors and think-aloud sessions. [S] https://blog.kartones.net/post/wired-article-on-halo-3/
- **First-time experience.** Hodent (Epic, Fortnite) on onboarding pitfalls and how the brain learns. [S] https://www.gamedeveloper.com/design/attend-gdc-for-epic-insight-into-the-psychology-of-player-engagement
- **Which proxies to trust**
  - **Trust:**
    - bots for feasibility, coverage, exploits and stuck spots (EA)
    - bot difficulty only when combined with a persona or population model (King, Rovio)
    - heuristic checklists, early and for general issues (Desurvire)
    - Steam-review text as a stand-in for impact feel (Lin et al.)
    - one fresh human per RITE round
  - **Distrust:**
    - bot clear-time or kill-time as a measure of fun or feel
    - AI judgement of screenshots (43% on glitches)
    - "more effects = more juice" (Kao: extreme juice scores worse than medium)
    - designers or agents scoring their own work (Ambinder on bias)

---

## Top 12 lessons for SKYGRIND (ranked by how much they would change the game)

1. Give every room one question that a verb answers. Every pillar, pad, rail and lava strip must be required by an enemy placement or the scored route, or it is cut ("every card has to have an obvious purpose"; "the enemies are placed so the player has to dash through both"). https://gameinformer.com/feature/2022/11/25/how-a-neon-white-level-is-made
2. Make standing still the losing move. Use attacks that track the player's spot and fire after a delay (the Virtue's 3 s insignia), have enemy accuracy drop as the player moves faster, and cap simultaneous attackers with tokens so crowds pressure without unfair bursts. https://ultrakill.wiki.gg/wiki/Virtue ; https://www.gamedeveloper.com/design/cyber-demons-the-ai-of-doom-2016-
3. Build kill crunch on hit stop, camera and coherence first: a 10-20 ms freeze per hit and 0.1-0.25 s on kills and parries, one white flinch frame, knockback, debris that stays, and medium-to-high juice, never extreme. https://arxiv.org/abs/2208.06155v1 ; https://infovore.org/?p=5275
4. Fix "teleported into a random room". Spawn the player into a readable room with the first wave in front (never behind, on a pad), a foothold of cover inside, a reward in the middle and a loop instead of a dead end. https://www.gamedeveloper.com/design/the-door-problem-of-combat-design ; https://book.leveldesignbook.com/process/layout/flow/verticality
5. Tie survival to aggressive verbs, as DOOM ties health, armor and ammo to three attacks and ULTRAKILL heals on close hits and parries, so the stylish play is also the safe one. https://gamedeveloper.com/design/the-aggressive-resource-management-of-i-doom-eternal-i-
6. Give big enemies and bosses weak points that switch off an attack, and show damage on the model, so kills feel physical and a boss's state reads without a health bar. https://doomwiki.org/wiki/Destructible_demons
7. One colour means one thing, on a saturation budget: the world sits below a saturation threshold, and gameplay objects are the most saturated and brightest things on screen. Every room passes a grayscale test. https://gamedeveloper.com/art/in-depth-how-valve-makes-art-to-enhance-gameplay ; https://gamedeveloper.com/design/how-to-reduce-visual-confusion-in-your-game
8. Bosses get a fixed telegraph language: the same flash and colour always means the same outcome (parryable or not), windows grow with the wait before them, and new things are taught in the "business as usual" beat. https://gamedeveloper.com/blogs/designing-for-difficulty-readability-in-arpgs ; https://www.gamedeveloper.com/design/boss-battle-design-and-structure
9. Tune difficulty by enemy mix, not health: add or remove one tricky enemy, alternate hard fights with simple ones, and treat each arena as a "guitar solo" with traversal between them. https://intothebluesky.com/2021/10/05/follow-up-interview-with-hakita/ ; https://gameinformer.com/preview/2019/06/13/id-talks-mod-support-integrating-more-traversal-and-the-controversial-neon-ui
10. Push variety with style freshness rather than penalties: a weapon loses 0.7 per use and others recover, with up to x3 in the air and big parry bonuses, so the style rank steers players off their path of least resistance. https://ultrakill.wiki.gg/wiki/Style
11. Keep the "one more round" pull in an endless mode beside the campaign: wall gun against gamble, a box that moves, upkeep through pack-a-punch, and the arena reshaping each wave with a full heal and a leaderboard. https://ultrakill.wiki.gg/wiki/The_Cyber_Grind ; https://gamesbeat.com/how-treyarch-designed-black-ops-7s-zombies-interview/
12. Use bots for feasibility, exploits and persona-based difficulty, and never as a proxy for fun. AI screenshot judges miss most glitches (43.4% best), and one fresh human per RITE round beats any metric. https://arxiv.org/html/2312.05291v2 ; https://arxiv.org/abs/2008.12937 ; https://en.wikipedia.org/wiki/RITE_Method
