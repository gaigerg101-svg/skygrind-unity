# MAP-RESEARCH.md: a new SURVIVAL map, aim assist, controllers, Steam and Switch (2026-10-05)

Web research by two agents (about 12 minutes each, 2026-10-05 ~11:00 AM), for GG's request: a map overhaul built for
movement and replayability that feels like CoD Zombies and ULTRAKILL with personality; aim assist too strong;
controller support; Steam and Switch. **UNVERIFIED** marks a community or secondary source, or a fetch that failed
(callofduty.fandom.com returned HTTP 402; no GDC Vault talk on Zombies layouts was found). The suggestions in section 4
are synthesis, not sources.

## 1. Map design principles (with sources)

1. **Location and tone first, then layers "like an onion".** Mob of the Dead was built so casual players survive and
   deeper layers reward the invested (Jason Blundell, interview snippet; UNVERIFIED in full):
   https://callofduty.fandom.com/wiki/User_blog:Damac1214/Mob_of_the_Dead_Development_Interview
2. **Simple to learn, deep to master:** Kino der Toten pairs an open Theater (training, a balcony) with a tight Lobby,
   on one floor, easy to hold in your head (community / SEO quality): https://33rdsquare.com/whats-kino-der-toten-mean/
3. **Loops within a ring:** Der Eisendrache's courtyard and halls form "multiple loops within a large ring":
   https://codzombified.blogspot.com/2016/05/rocket-platform-map-layout-in-der.html
4. **A faster car needs a bigger track:** Hugo Martin, "the race car is better, the track has to get better"; a dash in
   a cramped space is "a cheetah at the zoo":
   https://www.gamedeveloper.com/design/q-a-evolving-the-combat-design-of-id-software-s-i-doom-eternal-i-
5. **"Make me think, make me move":** every arena a problem solved by moving; traversal between fights challenges too:
   https://www.gamedeveloper.com/design/-make-me-think-make-me-move-new-i-doom-i-s-deceptively-simple-design ·
   https://www.gamespot.com/articles/doom-eternal-is-a-fantasy-combat-puzzle-but-what-d/1100-6467505/
6. **Looks open, still directional:** Hakita on ULTRAKILL 4-2, "constant forward momentum ... it just doesn't feel like a
   hallway because it doesn't look like one": https://intothebluesky.com/2021/10/05/follow-up-interview-with-hakita/
7. **Difficulty from enemy combinations, with breathers:** (same interview).
8. **Reward variety, don't punish repetition:** https://intothebluesky.com/2020/12/19/interview-with-arsi-hakita-patala/
9. **Clarity enables creativity:** Neon White, "the player always knows where to go next", "always has the right tool",
   "at least one non-trivial shortcut", "the shortcut path cannot be hidden"; "you can't be creative in the middle of a
   tense level you've never seen before":
   https://www.gamedeveloper.com/business/-neon-white-and-designing-for-player-creativity
10. **Few dead ends, many entrances around a core, power items on contested ground** (arena FPS):
    https://games.themindstudios.com/post/multiplayer-level-design-techniques/ ·
    https://www.plusforward.net/post/21433/The-Language-of-Arena-FPS-Level-Design/
11. **Name and theme areas early, with call-out landmarks:** https://book.leveldesignbook.com/process/layout
12. **One idea per space:** Titanfall 2's action blocks each centre on one mechanic, then twist it:
    https://www.gamedeveloper.com/design/understanding-i-titanfall-2-i-s-action-block-level-prototyping-process
13. **Setup cost stays proportional:** too many paid doors before power, the upgrade machine and perks kills runs
    (Zetsubou no Shima, below).

## 2. Patterns worth stealing

- **Kino's teleporter to Pack-a-Punch:** a gated fast-travel verb that is also the upgrade path (UNVERIFIED detail).
- **The old map as the heart of the new one:** Die Maschine puts the ruined Nacht der Untoten bunker at its centre; its
  Dark Aether is a "dark mirror" of the same map (UNVERIFIED, search snippet): https://callofduty.fandom.com/wiki/Die_Maschine
- **Shadows of Evil's district rituals in any order:** four districts, a ritual each, any order; Beast Mode reaches what
  human form can't; four Gateworms open the Rift and Pack-a-Punch: https://nazizombies.fandom.com/wiki/Shadows_of_Evil
- **Origins' mud removes a verb in a place:** slower, no slide (community):
  https://steamcommunity.com/app/311210/discussions/0/1368380934284219515
- **Der Eisendrache's quest teaches the geography:** each bow upgrade uses landmarks (the rocket pad, a cliff fire, a
  skull): https://codzombified.blogspot.com/2016/05/rocket-platform-map-layout-in-der.html
- **Liberty Falls' rooftops and zip lines:** easy traversal, forgiving training; but a generic setting reads bland:
  https://screenrant.com/black-ops-6-terminus-island-liberty-falls-better/ · https://gamerant.com/black-ops-6-best-zombies-maps/
- **The Cyber Grind as the endless model:** a 16 x 16 grid in a void, re-shaped every wave from a shuffled pool of 38
  patterns (tile heights, stairs, jump pads, enemy spawns); health, stamina and cooldowns reset per wave; the grid's glow
  changes colour by wave band: https://ultrakill.wiki.gg/wiki/The_Cyber_Grind. SKYGRIND's current arena already has a
  core grid of pillars with height presets (`PortExport/maps/survival.json` `core.presets`): it is most of a Cyber Grind.
- **DOOM "skate parks":** circular flow, vertical space for aerial movement, resources placed to reward aggression
  (community, UNVERIFIED): https://www.doomworld.com/forum/topic/133115-thoughts-on-the-combat-design-philosophy-of-doom-eternal/
- **Neon White's visible shortcut:** every zone has a faster expert line you can see from the main path.

## 3. Pitfalls

- **TranZit:** too big, too few wall buys, slow travel, fog and hazards punishing movement, built for four players
  (community): https://gamefaqs.gamespot.com/boards/669289-call-of-duty-black-ops-ii/64804047
- **Zetsubou no Shima:** too many doors and parts before power and Pack-a-Punch; two perks by round 15, an hour of
  setup (community): https://gamefaqs.gamespot.com/boards/145061-call-of-duty-black-ops-iii/73637637
- **Buried:** tiny, cramped, "only one real path to train" (community):
  https://gamefaqs.gamespot.com/boards/669288-call-of-duty-black-ops-ii/66885341
- **Restriction everywhere:** Origins' mud everywhere took away the slide players relied on. Restricting zones are small,
  optional or worth their price.
- **Cognitive overload:** never stack an objective, a verb restriction and an elite wave in one tight room (Neon White).

## 4. What this suggests for SKYGRIND (synthesis, not sourced)

- **One landmark visible from everywhere** (a broken orbital tether, a sun-dial spire): the PROTOCOL's power spine; its
  relays climb it.
- **A verb identity per zone** (Titanfall's one idea per space): a wall-ride canyon of parallel slabs (grapple anchors
  only here); a low-ceiling foundry with no pads where slam shockwaves carry along conveyors (the close-quarters zone,
  shotguns on its walls); islands crossed by ARCLINE swings or pads (the rail and rocket zone); long downhill slide chutes
  (Kino-style open training); a tight, gated armory (the Lobby to the chutes' Theater).
- **Verb-killing materials marked visibly** with a fixed palette (e.g. matte violet-grey: no wall-ride; cyan rings:
  anchors), within CLAUDE.md rule 8's colours.
- **The quest teaches the map:** PROTOCOL relays in any-order zones, each reached by its zone's signature verb, each
  dropping a token; the tokens open THE FORGE (SKYGRIND's Pack-a-Punch).
- **Rotate for replay, keep geometry fixed for mastery:** randomize the box spots, HOLD ring spots, the GOLD RUSH route,
  and which zone the PROTOCOL runs through, from the run seed.
- **One visible expert shortcut per zone** (a pad to a wall-ride that skips a gate's long way round).
- **The current square arena gets a second life:** an ENDLESS / BOSS RUSH map whose core pillars re-shape every wave
  (the Cyber Grind), or a "dark mirror" reached mid-run.

## 5. Aim assist (current default is the outlier)

- **Components:** slowdown (friction near a target), magnetism / adhesion (the camera follows the target, or cancels the
  player's own strafe), bullet magnetism (shots bend inside a cone), snap / centring (usually only on zoom). Insomniac's
  GDC talk: https://www.gdcvault.com/play/1017942/Techniques-for-Building-Aim-Assist
- **Destiny 2:** per weapon archetype, friction and magnetism pad-only:
  https://d2.destinygamewiki.com/wiki/Aim_Assistance. **CoD:** selectable profiles (Standard / Precision / Focusing)
  (UNVERIFIED). **Apex:** pad 0.6 console / 0.4 PC, mouse none (UNVERIFIED). **DOOM Eternal:** pad-only slider plus a
  separate snap toggle (UNVERIFIED). **ULTRAKILL:** optional adjustable "auto aim" (UNVERIFIED):
  https://www.gamingonlinux.com/2022/02/fast-paced-brutal-fps-ultrakill-gets-full-controller-support-ready-for-steam-deck/
- **Mouse players usually get none.** SKYGRIND today defaults both the mouse (`settings.aimAssist`) and the pad
  (`settings.padAimAssist`) to `"snap"`: the outlier, and GG's complaint.
- **Recommended:** mouse Off by default (an opt-in, small bullet-magnetism cone at most); pad "Standard" = slowdown to
  ~0.5x inside a small cone + light adhesion that cancels the player's own strafe and dash, no snap; options Off / Low /
  Standard / Strong (or 0-100) and a separate Target Snap toggle (default off); assist off while gyro aims.
- **Gyro as a mouse** (1:1 natural sensitivity, a ratchet button, optional tiny-input smoothing, flick stick):
  http://gyrowiki.jibbsmart.com/blog:good-gyro-controls-part-1:the-gyro-is-a-mouse ·
  http://gyrowiki.jibbsmart.com/blog:good-gyro-controls-part-2:the-flick-stick

## 6. Controller UX

- **Deadzones:** scaled radial, not axial; inner ~0.08-0.15, an outer deadzone, both exposed:
  https://joshsutphin.com/gamedev/doing-thumbstick-dead-zones-right.html · https://minimuino.github.io/thumbstick-deadzones/
- **Look:** a response curve (exponent ~2 default, linear option), separate X / Y sensitivity, invert Y, optional turn
  acceleration at the stick's edge (Halo Infinite's settings, UNVERIFIED):
  https://www.charlieintel.com/halo/best-halo-infinite-controller-settings-135561/
- **Glyphs follow the last device used, every frame** (mixed input); full rebinding (ULTRAKILL was criticised for
  shipping without pad rebinding); rumble with an intensity slider.

## 7. Steam and Steam Deck

- **Full controller support:** device glyphs, a published official configuration, mixed input, the on-screen keyboard
  through the API: https://partner.steamgames.com/doc/features/steam_controller/getting_started_for_devs
- **Deck Verified:** the default controller config reaches everything; matching glyphs; 1280x800 (or 720); text at least
  9 px at 800p, 12 px preferred (CLAUDE.md's 16 px at 1080p is ~11.9 px at 800p: passes, just under preferred); no
  launcher; 30 fps at default settings: https://partner.steamgames.com/doc/steamhardware/compat ·
  https://partner.steamgames.com/doc/steamdeck/recommendations
- **Unity pitfall:** Steam Input and Unity's Input System can see duplicate pads (Xbox) or lose non-Xbox pads; bridge or
  opt out deliberately: https://jsteinhauer.com/blog/index.php/2026/01/14/making-unity-input-system-and-steam-input-get-along/ ·
  https://partner.steamgames.com/doc/features/steam_controller/steam_input_gamepad_emulation_bestpractices
- **Cloud saves:** Auto-Cloud on files under `persistentDataPath`, not PlayerPrefs (the registry):
  https://partner.steamgames.com/doc/features/cloud

## 8. Nintendo Switch 2

- **Hardware:** 9 GB for games, 6 game cores (A78C ~1.0-1.1 GHz), GPU 1.71 TFLOPS handheld / 3.07 docked, a 1080p 120 Hz
  screen: https://www.nintendolife.com/news/2025/05/nintendo-switch-2-final-tech-specs-have-been-confirmed. The risk for
  SKYGRIND is the CPU (the 120 Hz sim, C# collision, enemy counts on ~1 GHz ARM), not the GPU. Not measured.
- **Access:** developer.nintendo.com registration, then a Switch Access Request; Unity's Switch 2 support is a Unity 6
  add-on for approved developers (URP, STP named): https://developer.nintendo.com/register ·
  https://unity.com/news/unity-to-support-developers-targeting-nintendo-switch-2-for-launch. Dev kits are limited
  (UNVERIFIED).
- **Input:** Joy-Con 2 mouse mode (used by Metroid Prime 4, Fortnite), gyro standard:
  https://www.nintendolife.com/guides/nintendo-switch-2-all-mouse-mode-compatible-games
- **Consoles compile AOT (IL2CPP).** SKYGRIND's IL2CPP trial is blocked (D-40), and bit-exactness has been proven only
  on Mono and CoreCLR on x86: IL2CPP on ARM is unproven for the golden vectors and the fingerprint.
- **Build in now:** quality presets with render scale / STP and a CPU knob (enemy and particle caps); every menu by pad;
  the glyph system; UI scaling; one save layer with atomic writes behind a platform interface; pause on focus loss and
  pad disconnect; AOT-safe code (reflection, Emit); asset and memory budgets. Lotcheck details are under NDA; commonly
  cited failures: suspend / resume, save corruption, controller disconnect, button terminology (UNVERIFIED).
