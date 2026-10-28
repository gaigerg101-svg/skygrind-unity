# DOOM (2016) and DOOM Eternal: visual-language rules for readable combat

Researched 2026-10-19 by web only (WebSearch / WebFetch). No local code read.
Markers: **Verified** = I fetched and read the page. **Reported** = seen only in a search-result summary or quoted
by another outlet; the page itself was blocked or not fetched. Several primary sources were not readable: the GDC
Vault recording of "Embracing Push Forward Combat" (login), the Eurogamer / Digital Foundry DOOM tech interview
(fetch refused), the SIGGRAPH 2016 and 2020 id slide decks (PDF over 10 MB), doomwiki.org (403), the Fandom DOOM wiki
(402), and PC Gamer interview bodies (truncated). Where a topic rests only on those, it is marked Reported or "not found".

## 1. Colour coding of gameplay and "combat chess"

**The governing rule: tell the player loud and fast.**
- Hugo Martin (creative director, Eternal): DOOM "is not trying to be an immersive game"; "It's speed chess. You've got
  to make decisions really fast"; "If I'm driving around a race car 200 miles an hour, I need all the signage and the
  game to be big and bold"; "If stuff is subtle, it's the game screwing me"; "if you have something to tell me, tell me
  loud and tell me fast". He calls the bright UI "a functional decision, not an aesthetic one" and lists "the glowing
  one-ups, the floating question marks, and the huge health packs" as the same goal. Verified:
  https://gameinformer.com/preview/2019/06/13/id-talks-mod-support-integrating-more-traversal-and-the-controversial-neon-ui
- Martin: "you need to be able to know from across the room" what a pickup is; "It's all about communication";
  environments carry "a lot of art detail", so "our stuff has to be bolder in some ways". Verified:
  https://shacknews.com/article/116635/hell-razer-the-making-of-doom-eternal?page=5

**Pickups by colour (Eternal).** Verified, same Shacknews page:
- armor shards are bright green (dropped by burning enemies); Martin: "You see how bright that armor is" tells the
  player they did the right thing even if the shard is on screen for a moment
- Glory Kill health "glows a soft, baby blue"; blue vials raise health to 100; health packs are "starkly white boxes"
  with a red cross
- each ammo type "has its own color so players can recognize it in the heat of combat"
- weapon pickups are "coated in a toxic green that demands the eye's attention"
- Resource-to-verb mapping: Glory Kill gives health, chainsaw gives ammo, Flame Belch on a burning enemy gives armor;
  bigger demons drop more. Verified: https://blog.playstation.com/?p=222405

**Glory Kill stagger flash.**
- Damage walks a demon up a ladder of pain reactions: aim-twitch, falter, pushback, then the stagger state that makes it
  Glory-Killable. Verified: https://www.gamedeveloper.com/design/cyber-demons-the-ai-of-doom-2016-
- The GDC 2018 talk (Jake Campbell): a demon weakened into Glory Kill range visibly glows and is stunned, and the
  animation is tuned so this reads; hurt states and attack motions are exaggerated for fast recognition. Verified (a
  Japanese write-up of the talk): https://game.watch.impress.co.jp/docs/news/1113403.html
- Colours: a staggered demon glows blue, and turns orange when the player is close enough to execute. Verified (guide
  text plus a reader comment; the guide body itself words blue as "trying to regain balance", so the two readings
  conflict): https://www.gameshedge.com/doom-eternal-glory-kills-guide/ . The "blue, then orange when in range"
  reading is also Reported by search summaries of Twinfinite's DOOM 2016 guide:
  https://twinfinite.net/guides/doom-how-to-glory-kills/ . No id statement on why blue and orange was found.
- During a Glory Kill the player is invulnerable and AI may not start a new attack until it ends. Verified:
  https://www.gamedeveloper.com/design/cyber-demons-the-ai-of-doom-2016-

**Weak points (Eternal).** Verified: https://www.shacknews.com/article/116635/hell-razer-the-making-of-doom-eternal?page=4
- Arachnotron back turret: destroying it makes the demon "explode with a satisfying plink"
- Mancubus arm flamethrowers: shoot them and the weapon is gone
- Pink Demon tail; Cacodemon swallows a grenade with a "gulp", then flashes, inviting a Glory Kill
- weak spots are "nearly required" later, against larger groups
- Martin on the destructible demons system: "purely visual flair" that still changes "the power of the weapons, the
  power of the player"; chunks fly off with every bullet, so damage is shown on the body. Reported:
  https://bloody-disgusting.com/video-games/3515304/doom-eternal-creative-director-dishes-new-demon-destruction-system/
  and https://embed.gamereactor.eu/doom-eternal-quakecon-impressions/

**Attack tells.**
- Marauder: wait "until the eyes of the Marauder light up green" (the axe swing), then hit; when shot otherwise he raises
  an indestructible "red shield". Verified (guide): https://www.gamepressure.com/doom-eternal/marauders-how-to-defeat-them/z0d156
- Martin says the Marauder's falter lasts about one hit, two with a weapon swap. Reported:
  https://www.pcgamer.com/uk/doom-eternals-director-says-the-marauder-is-good-actually/
- Readability through behaviour, not only colour: "the focus is for demons to be immediately distinct and consistent in
  their behaviour", with exaggerated animations and distinct goals (Pinky charges, Imp keeps distance). Verified:
  https://www.gamedeveloper.com/design/cyber-demons-the-ai-of-doom-2016-

**Combat chess, the fairness side of readability.** Verified unless marked:
- Attack tokens: each attack type has a capped pool; a demon requests a token, releases it after, and may steal one
  from a better-placed demon. Demon accuracy drops the faster the player moves; "it's unfair if the demons are attacking
  when the player cannot respond". https://www.gamedeveloper.com/design/cyber-demons-the-ai-of-doom-2016-
- GDC 2018 (Kurt Loudy): keep moving or die; no reloads; four pillars (fast movement, distinct demons, distinct weapons,
  a powerful player); each enemy maps to an obvious counter weapon; shots are spread on a distribution so they look
  aimed but miss. https://game.watch.impress.co.jp/docs/news/1113403.html
- The talk's mantra "make me think, make me move". Reported (search summary of the GDC speaker Q&A):
  https://gdcvault.com/play/1024940/Embracing-Push-Forward-Combat-in
- Martin (Noclip): AI tuned so players "create openings" before the Super Shotgun; "We use our chess pieces to move the
  player around and get them off those ledges and don't let them kite enemies". Reported:
  https://gamingnexus.com/News/44350/Noclip-sits-down-with-Hugo-Martin-to-discuss-Doom-Eternal
- The same colour grammar carried into DOOM: The Dark Ages: executable demons highlighted, bright neon-green parriable
  projectiles, purple outlines through walls (the latter can be turned off). Verified (critic, not id):
  https://buttonmusher.substack.com/p/doom-the-dark-ages-has-a-colour-problem

## 2. Art direction pillars

- "Rock 'n' roll was our inspiration for everything ... There is a lot of heavy metal in our hell, and that is
  intentional"; the original DOOM "looks like the kind of stuff a 15-year-old would draw on the back of his notebook";
  a personality "that would shine through in everything we did" (Martin). Verified:
  https://www.gameinformer.com/b/features/archive/2016/01/15/doom-a-new-vision-of-hell.aspx
- Demons: "exaggerated proportions with realistic details and textures", kept as a theme across the project; "push the
  comic book, heavy metal aesthetic as far as we could"; "Heavy Metal album covers of the '80s were a big influence"
  (Martin). Cacodemon horns replaced tentacles for "a more aggressive silhouette read" (Denzil O'Neill). Cyber-Mancubus:
  "big, heavy and tank-like". Tone: "playful but deadly". Verified: https://bethesda.net/en/news/the-art-of-doom-demons
- Environments (Chase Long, environment artist on Eternal): "big shapes to make areas feel huge and imposing", with
  secondary and tertiary detail supporting the big shapes; detail should be "intriguing while also not distracting" and
  avoid "cluttering the player's view". Verified: https://80.lv/articles/designing-environments-for-doom-eternal/
- Eternal environments are "much more intricate and in-your-face in their use of colors", which is why gameplay signals
  must be bolder still (Shacknews, Verified, link in section 1).
- Not found: an id talk that states shape language or a value / contrast rule in numbers.

## 3. VFX consistency (one family)

Primary id statements on a shared muzzle-flash / impact / gore style guide were **not found**. What is documented:
- DOOM 2016 frame (Adrian Courreges, from debug captures plus the SIGGRAPH 2016 deck). Verified:
  https://www.adriancourreges.com/blog/2016/09/09/doom-2016-graphics-study/
  - one bloom path for everything bright: bright-pass, downscale chain, separable Gaussian, a 1/4-resolution HDR bloom
    added in the final pass (so every emissive, flash and fireball blooms by the same rule)
  - smoke sprites are lit per sprite into a 4k atlas, so particles take the same scene lights as the world
  - one filmic tonemap (Uncharted 2 operator) and colour correction; the overall red tint comes from the grade
  - the HUD is drawn to its own premultiplied LDR target and composited after tonemapping (HUD colours are exact, never
    graded or bloomed); film grain is added last
  - heat distortion uses a low-res offset map in the final pass
- Tiago Sousa (lead renderer): artists adapted to "tone-mapping, image exposure, linear correctness", "no magic
  multipliers"; about 100 unique shaders in the game. Verified: https://80.lv/articles/doom-the-secret-of-60-fps and
  https://80.lv/articles/idtech-666-the-secret-of-dooms-render
- Eternal (SIGGRAPH 2020, Geffroy / Gneiting / Wang): artists wanted more lights and decals, and fights needed more
  dynamic volumes "for projectiles and impact decals", so culling moved to the GPU. Reported (search snippet; PDF too
  large to fetch): https://advances.realtimerendering.com/s2020/RenderingDoomEternal.pdf
- Martin: "Even the gore and the destructible demons have purpose." Reported: https://windowscentral.com/doom-eternal-preview

## 4. Arena lighting and readability

No id talk or interview found that states arena lighting rules. What is documented:
- id's own lighting-artist job: "Create visual depth through atmospheric effects, contrast, and color palettes in
  collaboration with Gameplay Designers and Effects Artists"; place fire, wind or embers "to suit the desired mood or
  objective of a space". Verified: https://hitmarker.net/jobs/id-software-senior-lighting-artist-761012
- "Artists play a big role in pathing players with subtlety." Verified:
  https://80.lv/articles/designing-environments-for-doom-eternal/
- Fog is resolved in the same compute pass as SSAO and reflections; the grade sets the hell-red cast. Verified
  (Courreges, link above).
- Dash pickups are "golden glowing capsules" that refill the dash. Reported:
  https://doomwiki.org/wiki/Dash
- Stratton: the arena "is like a guitar solo ... But it's always combat". Reported (search summary of Game Informer).
- Lighting credits on Eternal: Tony Garza art director, Garrett Moring lighting (portfolio credit). Reported:
  https://adombek.artstation.com/projects/w6PYxO

## 5. HUD and charge indicators near the reticle

- Eternal HUD: over 20 UI options, crosshair style, HUD opacity, per-element toggles; six "UI Color Profile" schemes
  (default green-and-orange, blue-and-purple "Demons", all-red "Exterminator"); three colourblind modes. Verified:
  https://slayersclub.bethesda.net/en/article/2V0erykuLGuV8a8GsmribP/doom-eternal-custom-huds
- The default reticle carries weapon-mod state: a player asks for the dot crosshair to show "amount of sticky nades
  loaded, charge time for ballista, plasma heat blast charge"; the reply is to use the other crosshair. The Heat Blast
  charge also shows as "the heat charge glowing red on the plasma rifle", which he finds "hard to notice". Verified
  (player forum): https://steamcommunity.com/app/782330/discussions/0/3241971691949546968
- Combat Shotgun sticky bombs: three shots, "remaining ammunition indicated on the crosshair". Reported:
  https://doomwiki.org/wiki/Combat_shotgun_(Doom_Eternal)
- Chaingun: an "overheating indicator around the crosshair". Reported: https://doom.fandom.com/wiki/File:20201024145734_1.jpg
- Plasma Heat Blast: "the entire coolant shroud changes color" with charge level. Reported:
  https://doomwiki.org/wiki/Plasma_rifle_(Doom_Eternal)
- Chainsaw fuel: three bars, only one recharges by itself; fuel cans refill. Verified: https://blog.playstation.com/?p=222405
- Dash: two uses before recharge. Reported: https://doomwiki.org/wiki/Dash . Where the dash pips sit on screen: not found.
- DOOM 2016 Gauss / Siege charge on the reticle: not checked (wikis blocked).

## What to steal for a dusk-palette movement shooter

1. **Two-step opportunity flash.** Stagger glows one colour; it shifts when the player is in finisher range. In
   SKYGRIND the opportunity is the player's, so make it cyan (dim pulse when staggered, bright solid when in reach),
   never red-orange. Sources: game.watch GDC write-up; gameshedge guide.
2. **One colour per resource, readable across the room, each tied to a verb** (finisher gives health, a second verb
   gives ammo, a third gives armour), and big drops from big enemies. Keep drops in the cyan family by value and shape,
   not new hues. Sources: Shacknews page 5; PlayStation blog.
3. **Tell in danger colour, punish window in player colour.** The Marauder's green eye flash marks the counter
   window. For us: the wind-up flashes red-orange, the open window that follows reads cyan. Source: gamepressure guide.
4. **Weak points that change behaviour and confirm with a pop** (turret off, flamethrower gone, a distinct "plink" kill),
   with damage shown on the body. Sources: Shacknews page 4; Bloody Disgusting / Gamereactor (Reported).
5. **Readability by fairness rules, not only colour:** cap simultaneous attacks with tokens, lower enemy accuracy with
   player speed, freeze new attacks during a finisher. Fewer tells at once means each tell is read. Source: Game
   Developer, "Cyber Demons".
6. **Mod state on the reticle, everything else at the edges.** Charge, heat, loaded count and pips belong on the
   crosshair; players complained when a minimal reticle hid them. Segment pips (three bars, one self-recharging) beat
   a smooth bar. Sources: Steam thread; PlayStation blog; Bethesda HUD article.
7. **Silhouette first, detail second.** Exaggerated proportions with realistic surface, a horned "aggressive silhouette
   read", big environment shapes with detail that is "not distracting". This fits the pill-language enemies: push
   proportion, not ornament. Sources: Bethesda "Art of DOOM demons"; 80.lv environments article.
8. **One light and bloom rule for every effect, HUD outside the grade.** Particles lit by scene lights, one bloom
   chain, one tonemap; HUD composited after tonemapping so its cyan stays exact at dusk. Source: Courreges DOOM 2016
   graphics study.
