# GG-REQUEST.md (wave 27): GG's playtest words after NIGHT26, 2026-10-09 (voice-typed; the spelling is his transcription's)

Screenshots read by the lead (Documents of 2026-10-08 22:35-22:43): the first arena is wide, open, with no roof and a sky; the worm is small and floating in an open arena; the glacier course is one long corridor with the boss a speck at 23 m; HUD reads `IT NEVER SLOWS DOWN  GAP -23 M  HURT AT 25 M`.

## Verbatim

"The gameplay is so fine but it is still not polished the arenas are still way too wide. They don't encourage using the wall ride mechanic at all. Also another feature I would like is that if you aim a weapon while riding [a wall] the game kind of temporarily slows down a little bit. Make slow motion just a bit. [It] has an effect as well on the screen to signify [it].

Another thing, I still don't know why isn't working, but basically the pulse also. First off, that weapon needs to attack much slower, it needs to be like every two and a half seconds you can use it, it's a slower powerful weapon. But after you charge it, after say killing five enemies, then you unlock being able to use the left trigger button which will send that shockwave attack and boost you.

Another thing I would like is that the pistol, when you hold the right trigger, it'll do a charge attack but it's on a longer cool down. Same thing with the shotgun.

But again the priority is fixing the arena to prioritize better gameplay flow, especially some arenas will spawn way too many enemies, like a few of them have [hundreds]... not 100 but way too many. They need to be fast paced: these maps need to be realistically two to three minutes maximum, and sometimes you'll spend over two minutes just in one arena firing the same weapon over and over. And the arena that takes away your pistol doesn't [work]: it feels random and it doesn't flow with the gameplay. So I need a complete overhaul on the first three stages, and I mean the start of it. The tutorial section is fine but kind of repeats similar things.

And there needs to be a roof as well. Baked lighting, it should be almost like indoors. Maybe if you want to spend more time, keep the roof open, I guess I'm fine with how it is, we'll add the texture and stuff later.

Overall the first boss fight is way too much like the Cerberus or whatever the first boss is in ULTRAKILL. It needs to be original, unique, inspired, and require actual use of our current gameplay mechanics and not just firing at it.

The second boss: just switch back to the original spray [sprite / model?] because it doesn't look good, and keep the effects right now but just change the [model].

The second level needs to have an introduction with the ice-style gameplay: it needs to be a level first. [Right now] it's horrible, it's poorly done. Again, I want this one to only use the pulse [gauntlet], where you have to speed up and skate and you can only attack him if you are able to catch up to him and hit him with the pulse. That's what I want the fight to be like. And again the attacks and stuff that it sends are not very good. The arena should also curve a little bit at times, or it's fine if it's infinite. But there's also no skate effect: I want it to feel like you're really dashing from right to left, and it should be the entire floor. Obviously there needs to be a build-up against a few enemies; it doesn't have to be as long a stage. I also want there to be areas where you're forced to wall ride, because the terrain might damage you temporarily so you're forced to jump on the wall. It's gameplay elements like this that force you to use the in-game mechanics that I want these bosses to be around. We have to think: what weapon, what mechanic, what type of movement does this boss want to show you and make you learn. That's the philosophy behind all the bosses. It doesn't have to be directly what I stated, but they should be overhauled to prioritize those mechanics and weapon choice.

So please take your time and spend 1 hour creating the next detailed prompt before building. Use best methods available. Get open source projects, copy GitHub code, do whatever you need to do to help with those mechanics and make the AI and the boss fight much, much better, and study arena design, very inspired by ULTRAKILL: each arena, each room, has a balanced number of enemies, follows a simple gameplay and is tighter, and again encourages movement."

## Lead's reading (to be confirmed at the next playtest; each is a default, not a decision)
- "slow motion when aiming on a wall ride": a short, mild time scale (about 0.6-0.8) only while a weapon is aimed (LT / zoom) AND wall-riding, with a screen effect (cool vignette, edge streaks in pale violet-white, not red or magenta). It must be a sim-side or render-side choice made once: see NIGHT27's design brief.
- "pulse every 2.5 s, charge by killing five, then LT blast": the PULSE GAUNTLET's primary becomes a slow, heavy swing (2.5 s cooldown); five kills (not landed hits) open the LT blast + boost. NIGHT26 had 7 landed hits per notch and a 3 s lockout.
- "pistol / shotgun hold RT = charge attack, longer cooldown": new.
- "the arena that takes away your pistol": the unlock / loadout stripping in the campaign rooms read as random: remove it or give it a clear reason shown on screen.
- "first boss too like Cerberus": THE SLAG HULK (charge into a wall, stun) is the ULTRAKILL Cerberus pattern: replace.
- "second boss: switch back to the original model": the Glacier's current look is rejected; the earlier model (the web's / spike's blocky body) returns, effects stay.
- "the ice level fight with the pulse only; catch up to him and hit him with the pulse": the Glacier becomes a PULSE-ONLY chase: speed up, skate, close the gap, PULSE-launch into it.
- "forced wall rides": a damaging floor (a temporary hazard) so the player must ride a wall.
- "roof and baked lighting, almost indoors" for the first three levels.
