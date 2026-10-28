# Void rooms with flyers (the NIGHT59 forge's research, 2026-10-21 ~20:40; one web-only subagent, every claim with its URL)

Question: in fast FPS movement shooters, how is a room where flyers hang over a bottomless gap kept fair (fall recovery, edges, flyer load)?

## Findings (as returned; "not found" kept)
- ULTRAKILL falls are damage + a teleport to the nearest safe ground, never below 1 HP unless the zone is flagged as an instant kill;
  enemies dropped in die. https://ultrakill.wiki.gg/wiki/Pit
- DOOM Eternal: "falling into pits is not an instant kill"; the exact recovery rule not found.
  https://www.gamedeveloper.com/design/the-dark-souls-design-of-doom-eternal
- ULTRAKILL's Whiplash: hookpoints (pull / launch / suspend), 0.5 s cooldown; light enemies are pulled to the player, heavy ones pull the
  player to them. Gap placement not documented. https://ultrakill.wiki.gg/wiki/Whiplash
- DOOM Eternal uses a flyer as a traversal anchor (the meat hook on a Pain Elemental), from a search snippet, not opened.
  https://blog.playstation.com/?p=220711
- ULTRAKILL's Virtue (a flyer that barely moves): a ~3 s tracking telegraph, then a 30-damage beam; groups of 2-3 "more dangerous"; no
  knockback onto the player documented. https://ultrakill.wiki.gg/wiki/Virtue
- Hakita: difficulty comes from enemy count and composition ("placing one [tricky enemy] in the right spot"), not the arena edge.
  https://intothebluesky.com/2021/10/05/follow-up-interview-with-hakita/
- Not found anywhere: edge / railing / catch-ledge counts per room; flyer counts or placement over a void; Titanfall 2's kill volumes
  (GDC 2018 "Action Blocks" talk behind a login: https://80.lv/articles/gdc18-designing-titanfall-levels-with-action-blocks).

## Three checks a generator could run (the subagent's inference, not sourced rules; thresholds from our config and bot data)
1. Every void has a recovery: each point of a pit's footprint maps to a safe ground point; a fall costs health, never kills (unless
   flagged); an enemy that falls in dies.
2. Every gap has a fixed crossing chain: no point on the gap path farther from a fixed hookpoint / ledge than the best single move; a
   flyer can add an anchor but is never the only way across.
3. Flyer load is capped and no flyer attack knocks the player over the void: cap simultaneous flyers over the gap; a telegraph before a
   hit; bound falls per minute with the bot.

## For THE GAP NEST (builder L, NIGHT59 L1)
p2's gate is its fixed crossing (check 2 holds by the card); the span's respawnY rule is check 1 (n52-span logs 107 falls per 10 seeds:
the bot's normal). Check 3 is the one to measure: p2's falls per minute with the drones alive vs p3 / p5 (void rooms without drones).
