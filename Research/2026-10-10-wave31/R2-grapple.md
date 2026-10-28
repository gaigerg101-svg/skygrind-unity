# What makes a first-person grapple pull feel good (research, 2026-10-10)

Method: WebSearch and WebFetch only, 6-minute time box. Published numbers for most games do not exist on the pages found. Where a number is missing, the entry says "no number found". A section at the end lists things I recall but could not source (not to be treated as fact).

## 1. Per-game findings

### ULTRAKILL Whiplash
- Mechanic: a spear fired at an enemy or hookpoint. Light enemies (Strays, Schisms) are pulled to the player; medium and heavy enemies and hookpoints pull the player to them. Holding the key extends the spear further and delays the reel; it reels when it hits, reaches max range, or is released. Source: https://ultrakill.wiki.gg/wiki/Whiplash
- Reel speed 60 u/s (units per second, same scale as normal movement). Source: https://breezewiki.discard.no/ultrakill/wiki/Whiplash (mirror of the Whiplash wiki page; the search result summary stated it, the wiki.gg fetch did not repeat the figure, so treat as one source).
- Cooldown 0.5 s between uses. Source: https://ultrakill.wiki.gg/wiki/Whiplash
- While reeling to a medium/heavy enemy or hookpoint the player can dash and slam, and can cancel at any time by jumping, punching, or pressing Whiplash again. Cancelling with a punch or Whiplash preserves momentum; cancelling with a jump does not. Source: https://ultrakill.wiki.gg/wiki/Whiplash
- Hookpoint types: green pulls the player to the point and releases on contact; blue pulls the player through the point in the travel direction (a built-in momentum carry); yellow suspends the player briefly and triggers connected objects. Source: https://ultrakill.wiki.gg/wiki/Whiplash
- Reeling toward enemies adds Hard Damage at 60 per second, capped at 50 (a cost on the pull in Standard+ difficulty). Source: https://ultrakill.wiki.gg/wiki/Whiplash
- Aim assist: not documented on that page (no number found).

### Titanfall 2 grapple
- Mechanic: the hook attaches to walls or ledges and the line retracts, pulling the pilot to the hook point. Looking below the grapple point makes the pilot swing toward the ground and gain speed. Jump jets can be fired while grappling (double jump), so a pilot can jump over the anchor and sling forward faster. Detaches if the anchor leaves line of sight. Cannot fire weapons while pulled, can melee. It has a cooldown. Sources: https://titanfall.wiki.gg/wiki/Grapple and https://www.cgmagonline.com/articles/previews/titanfall-2-hasnt-grapple-hooked-me-yet/
- Feel note: a preview says the pull is slower and steadier than the CoD Advanced Warfare grapple. Source: https://www.cgmagonline.com/articles/previews/titanfall-2-hasnt-grapple-hooked-me-yet/
- No official range, speed or cooldown numbers found on the wiki (https://titanfall.wiki.gg/wiki/Grapple).
- Not Titanfall's real values: a UCSD student project ("Titanfall-inspired", Quake units) publishes these constants, useful only as one worked example of a pull-in design: max range 2200 u (was 4000), pull speed 2400 u/s as a direct velocity toward the anchor that overrides rather than adds (was 4000), auto-detach at 80 u (about 2 m) from the anchor, max duration 5 s, cooldown 5 s, launch look-bias 0.6 (0 = pure grapple direction, 1 = pure look direction), launch speed multiplier 1.15 on detach. Source: https://cse125.ucsd.edu/2026/cse125g2/docs/TitanfallConstants_8hpp_source.html

### DOOM Eternal Meathook
- Mechanic: pull yourself to an enemy to close the gap, or swing past it to reach areas. Enemies smaller than prowlers are held in place when struck; larger ones are not stunned. The hook disengages if the player is hit by a knockback attack or double-jumps or dashes. Does not work on marauders or riot soldiers with shields up. Source: https://doomwiki.org/wiki/Meathook (page title returned as Super shotgun (Doom Eternal))
- Auto-target range, pull speed: no number found.

### Apex Legends Pathfinder grapple
- Max length 21 m, crosshair circle turns blue when the surface is in range. Attaches to the first solid object (players and ziplines included) and pulls you in. Aiming to the side swings instead of a straight pull. Disconnects at about 90 degrees of swing and you keep momentum. Jumping just after the hook reaches a surface gives a bit of extra momentum. Cannot fire weapons while grappling. Hitting a zipline puts you on it. Source: https://apexlegends.wiki.gg/wiki/Pathfinder
- Cooldown: the wiki page states 20 s flat as of a "June 23, 2026 patch", with 2 charges; previously the cooldown scaled with distance travelled (shortest pull 10 s, max 35 s, stops growing when velocity falls back to sprint speed or after 5 s). Sources: https://apexlegends.wiki.gg/wiki/Pathfinder and https://apexlegends.wiki.gg:443/wiki/Pathfinder (older text). The 2026 patch claim comes from the page content as fetched and I did not cross-check it.
- Pull speed in m/s and attach delay in ms: not published ("slight delay before the pull begins").

### Sekiro grappling hook
- Mechanic: no swing, behaves like the Arkham grapple and zips you to the point at once. Grapple points show a circle marker; white means out of range, green means in range and grappable. Some markers launch you forward in your facing direction instead of landing you on the point. Some bosses can be grapple targets to close gaps. Sources: https://twinfinite.net/guides/sekiro-grappling-hook-how-use/ and https://sekiroshadowsdietwice.wiki.fextralife.com/Grappling+Hook
- Range and magnetism numbers: none published. Design reading: binary in-range or out-of-range marker, no gradual aim assist.

### Just Cause (retractor)
- Retract speed is a three-tier upgrade: Slow, Medium, Fast. No numbers. Source: https://justcause.fandom.com/wiki/Retractor_mods
- Rico cannot swing, which separates it from Spider-Man style swing design. Source: https://www.cgmagonline.com/review/just-cause-4-ps4-review/

### Spider-Man, Severed Steel, Bionic Commando
- Spider-Man: the WPI design wiki lists Miles Morales web swinging as a highly customizable swing example; no numbers. Source: https://dgmiwiki.wpi.edu/mechanic/grappling_hook
- Severed Steel: nothing found.
- Bionic Commando Rearmed 2: swings changed to have more momentum and feel heavier; the player controls detach with a button. In Rearmed the arc differs depending on whether you press reel-in right after the grab or let it swing first. No numbers. Sources: https://www.hardcoregaming101.net/bionic-commando-rearmed-2/ and https://www.gamedeveloper.com/game-platforms/interview-grin-s-viklund-on-rearming-capcom-s-i-bionic-commando-i-

### Design framing
- Two families: pull the player to the contact point in a straight line (Ocarina Hookshot, Sekiro) versus a tether that lets you swing in an arc (Uncharted 4); ULTRAKILL and Spider-Man are listed as hybrids or customizable. Source: https://dgmiwiki.wpi.edu/mechanic/grappling_hook

### Unity and open-source implementation hints
- One technique (Unity forums): move toward the hit point with `Vector3.MoveTowards` at constant speed so it never overshoots; arrival test `Vector3.Distance(pos, hit.point) <= 0.5f`; on release apply the carried velocity so the player keeps going. Source: https://discussions.unity.com/t/creating-a-grappling-hook/521742 (and https://forum.unity3d.com/threads/grapple-and-swing.485494/)
- Another uses `AddForce` toward the hook scaled by distance. Source: https://discussions.unity.com/t/pulling-an-object-without-contact/85848
- Anti-"launch into orbit" advice: clamp the pull force to a maximum and add damping proportional to the velocity component along the rope so the player slows near the anchor. Source: https://bugnet.io/blog/how-to-fix-grappling-hook-launching-player-into-orbit (fetched page was an index, so this comes from the search summary only; treat as a general principle)
- Asset packs describe "arc-style pull-in behavior", elastic swinging with tension control, a clamp that enforces max rope length without teleporting, and camera tilt during swings; no numbers. Sources: https://nostalgic-games-tm.itch.io/afpcgrapple-hook-system and https://asago.itch.io/grappling-hook (5 modes: Swing, Pull Player, Pull Object, Fixed Rope, Elastic Rope).
- Karlson/Dani and Dave GameDev devlogs: the searches did not surface their pages or numbers. Not verified.

## 2. Unverified recollection (no URL, do not rely on it)
- Dani's Karlson-style grapple uses a Unity SpringJoint with maxDistance about 0.8 x distance, minDistance about 0.25 x distance, spring about 4.5, damper about 7, massScale about 4.5. My recall; the web search did not confirm it. This is a swing-with-shortening-rope, not a pull-in, and is the likely origin of "rope that does not pull".
- Dave GameDev's tutorial uses a short grapple delay (about 0.25 s), a cooldown (about 2 s), and a computed jump arc to the point rather than a rope. My recall; not confirmed.

## 3. What the evidence says about why a swing-only rope "does not pull you in"
1. The games that read as "pull" set the player's velocity (or a strong velocity target) toward the anchor, not a passive rope constraint. A rope only stops you moving away; it does not move you. (Pattern from the Titanfall-style worked example above, ULTRAKILL's 60 u/s reel, and the Unity MoveTowards pattern.)
2. Every pull game gives an explicit arrival rule: ULTRAKILL releases on contact for green hookpoints, the student project detaches at about 2 m, forum code stops at 0.5 m.
3. Every good one preserves momentum on release or exit and lets you cancel into another movement verb (ULTRAKILL: dash, slam, punch, Whiplash cancel preserve momentum; Titanfall: jump jets while hooked; Apex: jump at arrival for extra).
4. Feedback of "this will work": Apex's range indicator turning blue, Sekiro's white-to-green marker.

## 4. Recommended spec: pull-in grapple for a 120 Hz fixed-step FPS (existing swing-only rope kept as the hold-variant)
All values below are starting points to tune, not published values, except where tied to a source above.
- **Two modes on one verb.** Tap = pull-in (zip). Hold past the attach, or a swing input after attach = existing swing rope. This follows the pull/swing split on https://dgmiwiki.wpi.edu/mechanic/grappling_hook and Apex's "aim to the side to swing" (https://apexlegends.wiki.gg/wiki/Pathfinder).
- **Hook travel and attach.** Hitscan on press with a short visual flight (2 to 4 fixed steps, about 17 to 33 ms) so the hook reads as thrown; pull starts the step after attach. Apex has "a slight delay before the pull begins" (https://apexlegends.wiki.gg/wiki/Pathfinder).
- **Target acquisition.** Ray first; if it misses, a cone or sphere sweep (start around 3 to 4 degrees half-angle, up to a max range of 40 to 55 u) picks the nearest valid anchor; show a range ring on the crosshair that changes colour in range (Apex 21 m blue ring, Sekiro white-to-green marker: https://apexlegends.wiki.gg/wiki/Pathfinder, https://sekiroshadowsdietwice.wiki.fextralife.com/Grappling+Hook).
- **Pull motion = velocity override, not a constraint.** Each step, set the velocity component toward the anchor to `approach(vCur, vTarget, accel)`: vTarget about 60 u/s (ULTRAKILL reel 60 u/s: https://breezewiki.discard.no/ultrakill/wiki/Whiplash), accel high enough to reach it in about 6 to 10 steps (50 to 80 ms) so it snaps on. Keep the player's lateral velocity partly (do not zero it) so strafing bends the arc. Cut gravity during the pull. Cap the total speed at vTarget times a small headroom (around 1.15, echoing the 1.15 launch multiplier in the worked example: https://cse125.ucsd.edu/2026/cse125g2/docs/TitanfallConstants_8hpp_source.html).
- **Arrival.** Release when within about 2 m of the anchor (https://cse125.ucsd.edu/2026/cse125g2/docs/TitanfallConstants_8hpp_source.html), or on contact for a surface anchor (ULTRAKILL green hookpoint: https://ultrakill.wiki.gg/wiki/Whiplash). On release do not zero velocity: keep the pull velocity and blend 0.6 of look direction into it (the 0.6 look bias from the worked example), plus a small upward pop (about 4 to 6 u/s) so the player clears a ledge lip.
- **Momentum chain.** Allow jump, dash, slam and a second press to cancel at any time. Jump-cancel gives a normal jump on top of carried velocity (Titanfall double-jump over the anchor: https://titanfall.wiki.gg/wiki/Grapple; Apex jump-at-arrival bonus: https://apexlegends.wiki.gg/wiki/Pathfinder). Second-press or punch cancel keeps full momentum (ULTRAKILL: https://ultrakill.wiki.gg/wiki/Whiplash). Optional anchor type that carries you through the point in travel direction (ULTRAKILL blue hookpoint).
- **Safety and cooldown.** Max pull duration about 5 s; detach on lost line of sight (Titanfall: https://titanfall.wiki.gg/wiki/Grapple) and on knockback hit (Doom Eternal: https://doomwiki.org/wiki/Meathook). Cooldown short on a cancelled or missed hook (0.5 s, ULTRAKILL) and longer after a full pull, in keeping with GG's rule that movement verbs need a long cooldown (CLAUDE.md memory core; not a web claim). Cooldown value is a design call.
- **Feel layer.** FOV kick during the pull (not published by any source I found; start +8 to +12 degrees easing over about 120 ms, a tuning guess), rope retracts visibly at the same speed as the pull, a small camera pull-forward on attach. Apex and Titanfall block firing while pulled (https://apexlegends.wiki.gg/wiki/Pathfinder, https://titanfall.wiki.gg/wiki/Grapple); ULTRAKILL lets you keep fighting (https://ultrakill.wiki.gg/wiki/Whiplash). Pick one deliberately.
- **Enemy hook (optional, ULTRAKILL-style).** Weight class decides direction: light enemies are pulled to the player, heavy enemies pull the player (https://ultrakill.wiki.gg/wiki/Whiplash). Doom Eternal holds small enemies in place while you close (https://doomwiki.org/wiki/Meathook).
- **Determinism.** At 120 Hz with an accumulator, do the pull in whole steps, no frame-rate-dependent lerp; use `approach` per step and stateful phases (Flight, Pull, Arrival, Release). No Rigidbody; apply velocity through the web collider path.
