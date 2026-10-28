# U1 HUD research and spec (2026-10-11)

Scope note: web search returned thin results for several games. Facts below are tagged by source; anything not found is marked "not verified". Screenshots were not inspected.

## 1. Why a HUD reads "AI-generated / template"

Tells (from [impeccable's slop list](https://www.impeccable.style/slop), which is web UI, so I applied it to games):
- Identical containers with rounded or slanted fills and glow; "nested cards"; flat typographic hierarchy (everything one size); one font weight for all; uniform spacing; pulsing status indicators.
- Every readout boxed. SKYGRIND's four large teal boxes (health, weapons, points, rank) are exactly this.

How pros avoid it:
- **Commit to one job per readout, loud and plain.** id: "all of the messaging has to be big, bold and loud" and "tell me loud and tell me fast" ([Game Informer](https://gameinformer.com/preview/2019/06/13/id-talks-mod-support-integrating-more-traversal-and-the-controversial-neon-ui), [Hugo Martin quote on the same page]).
- **Strip decoration, keep one texture.** Returnal's UI team used one colour (teal, CRT/night-vision) and "no gradients, layers, or pop-ups"; everything flickers on and off ([PlayStation Blog](https://blog.playstation.com/?p=348960)).
- **Put critical data at the reticle, secondary at the periphery**, layering feedback in emergencies (same source).
- **Start with nothing and justify every element** ([StraySpark](https://www.strayspark.studio/blog/game-ui-ux-design-principles)): three tiers (critical large/animated, important medium, informational small); input response under 16 ms, transitions under 200 ms, tweens 100-300 ms.
- **Use the game's own fiction as the shape.** Neon White replaced guns with cards because "we were free from the convention of showing a gun on the screen" ([Game Developer](https://www.gamedeveloper.com/road-to-igf-2023/-neon-white-and-designing-for-player-creativity)); Mullet MadJack's kill timer is a phone in the avatar's hand ([Destructoid](https://www.destructoid.com/roguish-fps-mullet-mad-jack-demands-you-kill-every-10-seconds-or-die/)); Hi-Fi Rush's health bar pulses to the beat ([Twinfinite/Dsogaming search summary], layout not verified).
- Diegetic / meta / spatial / non-diegetic is the vocabulary ([Game Developer](https://www.gamedeveloper.com/design/user-interface-design-in-video-games)). Spatial UI (a marker on the door) beats a text line.

## 2. Teardowns (what was verifiable)

| Game | Facts found | Source |
|---|---|---|
| ULTRAKILL | HUD types: "Standard" (elements at the sides, angled to centre) and "Classic" (locked to bottom, numeric health and stamina). Stamina has 3 colour states (cyan full, dark cyan charging, red empty). Style meter, style info, weapon and arm icons, railcannon charge, speedometer are each toggleable. Crosshair none/small/large; health+stamina hug the crosshair. Weapon wheel on hold slows time, stick-select. Background opacity 0-100%. | [wiki HUD](https://ultrakill.wiki.gg/wiki/HUD), [Weapons](https://ultrakill.wiki.gg/wiki/Weapons) |
| DOOM Eternal | Upper left: extra lives, compass, corruption meter. Lower left: dash pips, Blood Punch, armor, runes. Lower right: ammo, grenades, chainsaw, flame belch. Health blue. 20+ options (crosshair style, opacity, boss gauge, prompts off). Pickups colour-coded per ammo type. | [Gamepressure](https://www.gamepressure.com/doom-eternal/user-interface/z7d1a1), [Bethesda](https://slayersclub.bethesda.net/en/article/2V0erykuLGuV8a8GsmribP/doom-eternal-custom-huds) |
| Neon White | Cards are the weapon display (gun art + discard-ability glyph); max 2 card types, up to 3 of one; clean, low-detail world with negative space; colour-coded route (red doors). Exact card px not verified. | Game Developer link above |
| Mullet MadJack | Timer as health, on a held phone; timer removed in boss fights. Rest not verified. | Destructoid |
| Titanfall 2 | Weapon + ammo right; Titan/boost meter lower left (tick mark shows boost). Rest not verified. | [Prima](https://primagames.com/eguides/titanfall-2-eguide/multiplayer/interface) |
| Splitgate | Health a bar with no number; jetpack refill bar centre. Rest not verified. | [wiki](https://splitgatearenareloaded.wiki.gg/wiki/Health) |
| Hi-Fi Rush, Turbo Overkill, Severed Steel | Not verified (searches returned no layout facts). Turbo Overkill's d-pad weapon wheel makes you stop moving: a reviewer complaint ([Game Critics](https://gamecritics.com/gc-staff/turbo-overkill-review/)). | |

Pattern across the verified ones: nothing sits in a box with a fill; health and ammo are corner text/pips; secondary systems (style, charge) are toggleable or hidden when idle.

## 3. Reticle

- A reticle should first aid aiming; players replace "multifunctional" crosshairs with a plain one otherwise ([Game Developer](https://www.gamedeveloper.com/design/-ux-design-methodology-of-keeping-cross-hair-design-on-track)). It lists weapon differentiation (cross vs circle), player-state display, hit feedback.
- ULTRAKILL ships three sizes plus none. Doom ships a style choice. So: ship a minimal default and a Large/None option.
- Rule: shape encodes state, colour only confirms it.

## 4. Five weapons on a pad

- Wheel (ULTRAKILL, Doom): good for 6+, but slows time/pauses movement (Turbo Overkill complaint). Mixed sim: do not add time dilation (sim is fixed 120 Hz; not checked against hitstop).
- Hotbar by d-pad: free hands only if thumb stays on right stick; costs aim.
- **Recommended: RB/LB cycle (already the control) plus a 5-tick pip strip**; the strip expands for 700 ms on switch, then collapses. No wheel, no big icons. Optional: hold-to-reveal on d-pad.
- Show the LT action label only for the held weapon; the five-label row is what created GG's "gaps".

## 5. Patterns and code to copy

- [Unity UI optimisation guide](https://unity.com/how-to/unity-ui-optimization-tips): split static and dynamic canvases, Raycast Target off on all text, avoid Layout Groups (use anchors), hide with the Canvas component, tween instead of Animators.
- GDC: [Dead Space UI (Ignacio, 2013)](https://www.gdcvault.com/play/1017723/Crafting-Destruction-The-Evolution-of) (diegetic design reasoning); [The Division UI (Savoie, 2017)](https://gdcvault.com/play/1024026/Lessons-Learned-Creating-UI-for) (UI pipeline lessons). Neither watched; summaries only.
- [Game UI Database](https://80.lv/articles/game-ui-database-2-0-has-been-released): browse by HUD element / colour for references.
- [Unity FPSSample](https://github.com/Unity-Technologies/FPSSample): official FPS source; HUD code not inspected (2018, HDRP, unmaintained).

## 6. SKYGRIND HUD SPEC (1080p; scale all px by height/1080)

**Motif: "the slash."** One 20-degree skewed tick (6x14 px) is the only ornament: health segments, weapon pips, rank meter, dividers. No panel fills, no circles, no diamonds. Legibility from a 2 px dark shadow (#0A0E14, 80%), not boxes. Neutral = bone (#E6ECEF, no violet; violet-white stays the boundary's). Cyan #36E0F2 = usable/ready. Red-orange #FF5A2B = danger/enemy only. Magenta only RIVAL.

Safe margins: 72 px sides, 54 px bottom, 54 px top.

| Element | Anchor and size | Font role | Colour | Shows / hides | Motion |
|---|---|---|---|---|---|
| Reticle | centre; 4 dot, 4 ticks 8x2 at gap 10 | none | bone; ticks cyan when LT/charge ready; red-orange tint over enemy | always; None/Large option | hit: ticks flash 80 ms; kill: rotate 45 deg 120 ms; charge: 2 arcs close on dot (replaces pips) |
| Health | bottom-left (72,54); number 64 px, 10 slash ticks 220 px wide | Display bold | bone; ticks red-orange on hit flash and at <=25% | always | number counts 80 ms; no pulse; one 150 ms flash per hit |
| Cooldown line | under health, 3 px x 120 px | none | cyan | only while a movement verb is cooling | fill, no pulse |
| Weapon | bottom-right (72,54), right-aligned, max 360 px wide: name 24 px caps, LT label 16 px cyan "LT ZOOM", 5 pips | Label caps | bone / cyan | name and LT label always; pips expand to 5 names (16 px) for 700 ms on switch | 120 ms slide 12 px |
| Points | top-right, 32 px; "+100" popups 20 px | Display | bone (not yellow) | always; popups 600 ms drift up | popups merge in 150 ms |
| Style rank | below points; letter 40 px + 160x3 px line; word only on rank-up (0.8 s) | Display | bone; line cyan | only while meter > 0 | 3-frame flicker-on |
| Round / enemies left | top-left, 20 px | Label | bone | always in rounds; hidden in lobby | none |
| Message slot | top-centre y=54 (boss: y=132), 24 px, max 56 chars, 1 line | Label | bone; DANGER red-orange | one message at a time; min dwell 1.2 s | 3-frame flicker-on, 120 ms out |
| Door / goal | spatial chevron on the door, 20 px "7 m" beside it | Label | cyan | when door open and within view or 25 m | none |
| Lesson | uses the message slot (priority 4), 2 lines | Label | bone | max 6 s, no black box | as above |
| Boss bar | top-centre y=54, 640x6 px, name 18 px above | Label | red-orange | boss alive | slides in 200 ms |
| Damage direction | ring r=140 px of 3 arc ticks | none | red-orange | 600 ms after hit | fade |

Message priority: 1 danger / boss intro, 2 room or round title, 3 door / objective hint, 4 lesson. Higher replaces lower; lower is dropped if older than 3 s.

### Mockup A: combat
```
 R4  12 LEFT                                                  4,250
                                                              B  ===
                       DOOR OPEN
                           .
                          -+-
                           .

 100                                                  THUNDERCLAP
 / / / / / / / / / .                                    LT PARRY
                                                       / / [/] / /
```

### Mockup B: boss fight
```
                  THE STAG
          [=================//        ]
                    BEWARE: CHARGE
 R4                                                           4,250
                                                              A  ===
                          .
                         -+-
                          .

 64                                                   KESTREL
 / / / / / . . . . .                                     LT ZOOM
 ----                                                  / [/] / / /
```

## 7. Checkable rules for a coding agent

1. No filled panel behind any HUD element. Fills allowed only: the 6 px boss bar and 3 px lines.
2. No HUD element wider than 360 px, except the boss bar (640 px). No element taller than 80 px.
3. One top-centre message slot; any other text component anchored top or bottom centre is a fail. Never two messages visible at once.
4. A label string appears once on screen. The LT action label shows for the held weapon only. No word repeated within 200 px.
5. All text >= 16 px at 1080p; headline numbers (health, points, rank) 32-64 px; at least 3 distinct text sizes.
6. Only the slash tick, text and lines: no circles, diamonds, arcs (except reticle charge arcs and damage ring).
7. Colour: red-orange only on health hit/low, enemy-over reticle, DANGER, boss bar, damage ring. Cyan only on ready/usable. Points are bone.
8. No continuous pulse on a static value; motion is event-driven, 80-200 ms.
9. Idle elements (rank, cooldown, pips, popups) are fully hidden, not dimmed.
10. Raycast Target off on every HUD graphic; static and dynamic parts on separate canvases; no Layout Groups in the HUD.
11. Every pixel size and colour lives in UnityTuning.cs with a comment; HUD uses no gameplay numbers by hand.
12. Test: Play at 1920x1080 and 1280x720, capture combat and boss frames, assert no text-rect overlaps and margins >= 54 px.

Not checked: pad glyph prompts, accessibility scaling, TMP theme fonts, whether weapons have ammo (add a number beside the weapon name if they do).
