# Research 2026-10-06: feel in motion, and running Opus 5.5 nights that measure (for NIGHT18)

Two read-only research agents ran for about 3 minutes each on 2026-10-06, and the lead spot-checked one source. Items
marked **[checked]** were read by the lead from the source. The rest are the agents' reports with their URLs. Items
marked (inferred) have no source.

## A. Running the night (prompting Claude Opus 5.5 for long, multi-agent, unattended work)

### How to write the prompt
- **[checked] Prompting Claude Opus 5.5**
  (https://platform.claude.com/docs/en/build-with-claude/prompt-engineering/prompting-claude-opus-5-5):
  - Effort is the main control for thinking: drop "think carefully / double-check" lines.
  - On unattended runs, a text-only turn end is a report, not proof of completion. Keep the parts in a checklist file.
    The standing paragraph about how turns end (the four kinds of early stop) is the recommended system addition.
  - A time signal (`elapsed 340s / 1200s`) makes multi-agent teams parallelise and finish sooner. Under time pressure
    the model may verify a little less, so keep the gates structural.
  - Dense screenshots: give it higher-resolution images and image tools (PIL / OpenCV: crop, zoom, measure).
- **Give the whole task, the guardrails and the finish line up front**, with the reason for each rule. Use plain
  language rather than all-caps (all-caps over-triggers). Use 3-5 varied examples in their own sections. Name what's in
  scope, what must not change and where the task ends: Opus 5 widens tasks on its own.
  (https://platform.claude.com/docs/en/build-with-claude/prompt-engineering/claude-prompting-best-practices,
  .../prompting-claude-opus-5)
- **Find the right altitude:** the smallest set of information that fully describes the behaviour. Over-prescriptive
  instructions written for older models lower quality.
  (https://www.anthropic.com/engineering/effective-context-engineering-for-ai-agents)

### Long runs and state
- **The start routine:** confirm the working directory, read the progress file, the task list and git log, and run one
  end-to-end check before new work. State lives in JSON task lists, free-text progress notes and git (one feature, one
  commit). Removing or editing tests to pass is unacceptable.
  (https://www.anthropic.com/engineering/effective-harnesses-for-long-running-agents)
- **Every status claim cites a tool result from this session.** Anthropic reports this "nearly eliminated fabricated
  status reports" (the Fable 5 prompting page).
- **The /goal checker** is a small model that only reads the transcript. Write a condition the transcript can prove, and
  add a stop clause (time or turns). (https://code.claude.com/docs/en/goal)

### Builders
- **A builder's brief** gives the objective, the output format, the tools and the boundaries; without them agents
  duplicate work and leave gaps. Results go to files or commits, with a short summary handed back.
  (https://www.anthropic.com/engineering/multi-agent-research-system)

### Judging
- **The builder never scores its own work.** A separate, sceptical, calibrated evaluator does; agents "confidently
  praise" mediocre work. Before each round, agree a done-contract with a hard minimum on every criterion; any criterion
  under its minimum fails the round. Calibrate the judge with scored examples.
  (https://www.anthropic.com/engineering/harness-design-long-running-apps)
- **Score each criterion with its own judge.** Allow "Unknown". Prefer binary pass / fail with a written critique over
  1-5 scales. Run A/B comparisons in both orders (position bias).
  (https://www.anthropic.com/engineering/demystifying-evals-for-ai-agents, https://hamel.dev/blog/posts/llm-judge/,
  https://arxiv.org/abs/2306.05685)
- **Judges with access to the real artefact** (playing it, screenshots, measured numbers) do better. One study: agents
  "claimed improvement every time, yet 56 percent had a measured delta of zero or below" (https://arxiv.org/abs/2607.25152).
  NIGHT17's lead scored 30-37 while the fresh reviewers scored 23-27: the same effect.
- **A reviewer asked to find gaps always finds some.** Ask it to flag only gaps against the stated criteria.
  (https://code.claude.com/docs/en/best-practices)

## B. Kill and hit feedback

**Hitstop**
- **Scale it with the weight of the event.** Street Fighter V uses 8 / 12 / 15 frames (about 133 / 200 / 250 ms) for
  light / medium / heavy hits (https://glossary.infil.net/?t=Hitstop). ULTRAKILL's "major" hitstop is 0.25 s (parries).
  Celeste freezes 0.1 s on explosions and shortened its freeze on a frequent pickup after complaints: a freeze on a
  frequent event annoys (https://www.celestegame.com/changelog.html).

**Screen shake and view punch**
- **Trauma shake (Eiserloh, GDC 2016):**
  - shake = trauma² (or ³), with trauma decaying linearly;
  - Perlin noise, not per-frame random;
  - in 3D, rotation only (yaw / pitch / roll): "translational is super lame".
  - (http://www.mathforgameprogrammers.com/gdc2016/GDC2016_Eiserloh_Squirrel_JuicingYourCameras.pdf)
- **Source's view punch:** a damped spring (damping 9, spring constant 65) that overshoots; a hit adds velocity, not
  angle (https://raw.githubusercontent.com/ValveSoftware/source-sdk-2013/master/src/game/shared/gamemovement.cpp).

**Juice and how much of it**
- **Vlambeer's "Art of Screenshake"**, in order: bigger and faster bullets, muzzle flash, impact effects, hit animation,
  enemy knock-back, permanence (debris stays), camera lerp, shake, player knock-back, "sleep" (a pause on hit), gun kick,
  bigger explosions (https://www.bluetengu.com/2014/12/12/art-of-screenshake-experiments/).
- **Impact feel** (19 features, Steam reviews of the top vs bottom action games): "hit stop, sound coherence, and camera
  control may strongly influence players' impact feel" (https://arxiv.org/abs/2208.06155).
- **Kao 2020** (N = 3,018): medium and high juice beat none and extreme on play time, experience and performance;
  over-juicing hurts (https://www.sciencedirect.com/science/article/pii/S1875952118300879).

**Kill confirmation**
- **Kill confirms** (the Overwatch skull; ZDoom HitMarkers with separate hit and kill markers:
  https://github.com/Tekkish/HitMarkers) are prominent screen-space markers. No published size specs were found.
- **Screen-size floors for death effects at distance:** no published numbers were found.
- **Inferred targets:**
  - a kill's frame 1 changes at least 3 x the pixels a hit's frame 1 does;
  - the kill burst at least 6-8 % of the screen's height at any distance;
  - a HUD kill marker for 300-400 ms with an overshoot pop.

## C. Movement feel

**FOV and view roll**
- **FOV kick:** degrees plus separate in / out times and curves, per action
  (https://docs.zigurous.com/com.zigurous.camerasystem/manual/fov-kick/). Apex ships "FOV Ability Scaling" and "Sprint
  View Shake" as settings, and competitive players lower them (https://www.setup.gg/game/apex-legends/best-fov/).
  Published degree values for Titanfall / Apex / DOOM / Neon White were not found.
- **Strafe roll:** Quake / Half-Life `V_CalcRoll(angles, velocity, cl_rollangle, cl_rollspeed)` rolls the view from
  lateral velocity (https://twhl.info/wiki/page/VERC:_View_Roll_When_Strafing_(like_DMC)).

**Open-source controllers to study**
- Landing dip and jump bump: https://github.com/fatihgezerx/FirstPersonSystem
- Strafe roll and forward pitch: https://github.com/FeverDevJohnny/impactcontroller
- Godot controllers with tilt, bob, FOV and dash: https://github.com/WayrzDev/FPSController_GODOT,
  https://github.com/BirDt/bhop3d

**Inferred targets**
- sprint +5-8° FOV over about 250 ms
- boost / dash +10-15° in 60-80 ms, back over 300-400 ms
- slide +6-10° with a 0.4-0.6 m camera drop and a 3-5° roll
- strafe roll 1.5-3° from lateral speed
- a landing dip of 0.05-0.15 m by fall speed, back in 150-200 ms
- radial speed lines above 70 % of max speed, with alpha by speed

## D. UI motion

- **Durations (NN/g):** 100-500 ms in general; about 100 ms for simple feedback; 200-300 ms for large changes
  (https://www.nngroup.com/articles/animation-duration/).
- **Ease-out-back:** c1 = 1.70158, about 10 % overshoot (https://easings.net/#easeOutBack).
- **Juice It or Lose It** (Jonasson and Purho): tween everything, squash and stretch, particles, flash the hit target,
  shake (https://gdcvault.com/play/1016789/Juice-It-or-Lose).
- **ULTRAKILL's style meter:** 8 ranks (thresholds 200, 300, 400, 500, 700, 850, 1000, 1500), a list of recent named
  actions, passive decay (https://ultrakill.wiki.gg/wiki/Style).
- **Open-source tween and feedback code:**
  - LitMotion (MIT, zero-alloc): https://github.com/annulusgames/LitMotion
  - PrimeTween: https://github.com/KyryloKuzyk/PrimeTween
  - MMFeedbacksPublic (Feel's early open version; its feedback-sequencer pattern): https://github.com/reunono/MMFeedbacksPublic
  - Camera shake: https://github.com/IronWarrior/UnityCameraShake, https://github.com/andersonaddo/EZ-Camera-Shake-Unity

  D-30 (no new packages) holds, so study these; don't add them.

## E. Measuring feel

- **Hicks et al., "Good Game Feel":** an empirical framework usable as a review checklist
  (https://dl.digra.org/index.php/dl/article/download/936/936/933).
- **The Game Experience Questionnaire core:** 33 items scored 0-4
  (https://pure.tue.nl/ws/files/21666907/Game_Experience_Questionnaire_English.pdf).
- **No published standard for automatic feel metrics was found.** NIGHT18 uses its own (inferred), all measured from
  the game's own telemetry and full-resolution frames:
  - steps from input to the first feedback;
  - the pixel difference between a hit's frame 1 and a kill's frame 1;
  - an effect's on-screen size at 6 / 12 / 20 m;
  - a victim's screen displacement;
  - each UI element's first-frame scale and alpha;
  - blind A/B "which one is the kill?" judgements.
