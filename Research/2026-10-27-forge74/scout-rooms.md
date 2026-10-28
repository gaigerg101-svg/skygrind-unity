# Scout: small-arena layout rules (forge74)

Honest note: designers rarely publish arena numbers. Tags: **[P]** = number published in the source. **[D]** = a check I derived from the source's qualitative rule (a proposed starting value, not a source quote). Pages that returned 403 (adamatomic, Valve wiki) are cited from search summaries only.

1. **Size by player speed, not metres.** Doom Eternal's dash forced bigger levels: "Too big, you actually feel slow. Too small, you're frustrated." [P: no metres.] ULTRAKILL speeds: walk 16.5, slide 24, dash 49.5 u/s. [D] Script: crossing time of the longest chord at walk and at dash; tune until a dash crossing is a beat, not a blink.
   https://www.gamedeveloper.com/design/q-a-evolving-the-combat-design-of-id-software-s-i-doom-eternal-i- ; https://ultrakill.wiki.gg/wiki/Stamina
2. **At most 3 floor planes per area.** "Most maps tend to max-out at three different floor planes"; a fourth adds only another middle layer. [P] Check: cluster walkable Y values per room, count <= 3.
   https://book.leveldesignbook.com/process/layout/flow/verticality
3. **Up is paid for, down is free.** Downward flow is heavier; one-way drops funnel players; upward needs ramps, ladders or pads (Q3DM17: "a dozen jump pads and teleporters", little cover). Check: every tier has at least one up-route (pad/ledge) and one drop. [D]
   same page
4. **Jungle gym over canyon for tight rooms.** Jungle gyms have distinct floors, so players hop between skirmishes; more obstacles make long-range attacks harder to land. Check: each tier can host its own fight and has a route to the next. [D]
   https://blog.adamatomic.com/post/613311014289768448/design-of-doom-eternal
5. **Break sightlines with sharp corners.** Blind corners (90 degrees or more) hide you even up close; rounded corners widen sightlines; vary sightline length per area; "less cover" is wrong for PvE, which wants more. Check: from N floor samples, fraction of other samples visible; flag any room above ~60% (my threshold). [D]
   https://book.leveldesignbook.com/process/combat/cover ; https://book.leveldesignbook.com/process/combat/balance
6. **Cover classes by height.** High >= 1.75 m, low 1.0-1.25 m, <= 0.5 m is not cover (Uncharted 4). [P] Freestanding pillars permit circling; "low cover is better than tall" keeps sightlines open. Check: classify each blockout box; require at least one freestanding piece per room.
   https://book.leveldesignbook.com/process/combat/cover
7. **A pillar in a small room makes a circle-strafe loop.** Combat bowl: open area, full-height pillar in the centre, spinning flow; add a one-way drop or a tier to fix its direction. Counter-Strike maps: "usually 3 big circular loops overlaid", plus smaller loops. [P] Check: nav-graph cycle basis >= 2 per room. [D]
   https://book.leveldesignbook.com/process/layout/typology ; https://critpoints.net/2018/02/18/good-fps-map-design/
8. **Few enemy types per wave.** 1 type = rest, 2 = regular, 3 = complex, 4 = brawl, 5+ = slaughter. [P] Check: distinct types per wave <= 3 for normal rooms.
   https://book.leveldesignbook.com/process/combat/encounter
9. **Reveal enemies on purpose.** Two patterns: (a) the player ambushes: unaware enemies visible from the entrance, reinforcements hidden round a corner; (b) enemies ambush: none until the player crosses a midfield threshold. Entry should be one-way (short drop or a door that closes) so the fight is fair. Check [D]: every wave-1 enemy is in the entry frustum, and a "N left" counter counts only revealed ones, or hidden ones arrive through a visible portal. L4D rule: the Director spawns out of line of sight, so maps need hidden spawn spots.
   https://book.leveldesignbook.com/process/combat/encounter ; https://developer.valvesoftware.com/wiki/L4D_Mapping_Notes
10. **Never spawn behind the player.** A 180-degree turn is slow on a stick. Check: zero spawn points in the rear arc of the entry facing. [D, rule is P]
    https://book.leveldesignbook.com/process/layout/flow/verticality
11. **Resources come from enemies, so they move you.** Doom Eternal: ammo from chainsaw kills, health from glory kills, armour from flame belch; "As long as there are demons on the battlefield, you've got the resources you need." Pickups carry a light rig so they read "from across the room". Check [D]: pickups on a tier other than the main crowd.
    https://www.gamedeveloper.com/design/the-aggressive-resource-management-of-i-doom-eternal-i- ; https://www.pcgamesn.com/doom-eternal/doom-eternal-old-school-lives
12. **Count and composition set difficulty; position finishes it.** Hakita: difficulty is "the amount of enemies and their composition"; easing = "removing one tricky enemy or placing one in the right spot"; high-count simple fights are the breathers. Check: encounter budget per room; ranged enemies have LOS to >= 2 tiers. [D]
    https://intothebluesky.com/2021/10/05/follow-up-interview-with-hakita/
13. **Rooms read at a glance; small and dense beats big and empty.** "As soon as you look at a room it's easy to understand it." Steve Lee: lean to smaller spaces, test small first, dense not "empty and sparse"; open layouts have "no corners to turn around, to reveal new things". Check: tier count, cover count and wall-clutter per m2 above a floor. [D]
    https://80.lv/articles/ultrakill-devs-on-the-game-s-mechanics-npc-ai-and-early-access-experience/ ; https://80.lv/articles/8-common-problems-with-level-layouts-explained-by-dishinored-2-level-designer
14. **Metrics floor.** Unity: hall >= 2.0 m (double player width), door 1.25 x 2.5 m. TF2 ranges: close <= 256 u, medium <= 1024 u, long ~2048 u (about 4.9 / 19.5 / 39 m at 0.01905 m per u, my conversion). [P] Check: longest sightline sits in the medium band for a small room.
    https://book.leveldesignbook.com/process/blockout/metrics
15. **No waiting, always know the exit.** Neon White: "the player always knows where to go next"; Hakita avoids forced waiting. Check: exit gate visible from every tier; no timer-free dead stretch. [D]
    https://www.gamedeveloper.com/business/-neon-white-and-designing-for-player-creativity

Not found: any published loop length in metres, cover spacing in metres, or arena area for ULTRAKILL, Severed Steel or Turbo Overkill (only reviews). Doom 2016 GDC talk (gdcvault 1024940) was behind a maintenance redirect; not read.
