# config.notes.md

Generated from `src/config.js` by `port-export/scripts/export-config.mjs`. Do not edit by hand.

The file's own header states the unit system: "Every tunable number in SKYGRIND (SPEC.md §3.1–§3.6, §5, §9). Units: meters, seconds, m/s, degrees. (S) = our own starting guess, tune in playtesting. This object is mutable on purpose: the lil-gui panel (?debug=1) edits it live. Derived values (jump v0, gravity, pad launch speed) are computed at use time from these inputs."

Each row is a comment the source code puts next to a value, copied verbatim, with the dotted path of the key it
sits on (resolve it in `config.json`). A comment on its own line is listed under the first key that follows it,
marked "(above)". Several keys on one line share that line's comment. Keys with no comment in the source are not
listed. For those, only `config.json` and the header above apply. `(S)` in a comment is the source's marker for
"our own starting guess, tune in playtesting".

Rows: 2218. Paths that did not resolve in config.json: 0.

| Path | Value | Line | Comment (verbatim) |
|---|---|---|---|
| `loop` | `{"simHz":120,"maxFrameDelta":0.05,"maxSubstepMove":0.3}` | 7 | (above) §3.1 Physics loop |
| `loop.simHz` | `120` | 9 | fixed simulation steps per second |
| `loop.maxFrameDelta` | `0.05` | 10 | s, frame delta clamp (three.js games_fps) |
| `loop.maxSubstepMove` | `0.3` | 11 | m, longest move in one collision substep (no tunneling) |
| `mouse` | {…} | 14 | (above) §2 Mouse look |
| `mouse.sensitivity` | `0.0014` | 16 | rad of look per pixel of mouse movement (OVERNIGHT2 N1: was 0.0022, about 36% lower) |
| `mouse.oldDefault` | `0.0022` | 17 | a saved sensitivity equal to this (the old default) migrates to the new default once |
| `mouse.fineStep` | `0.0001` | 18 | the settings row's - / + buttons (no Ctrl needed) |
| `mouse.pitchLimitDeg` | `89` | 19 | look up/down clamp |
| `collision` | {…} | 22 | (above) §3.1 Collision resolution (Octree + Capsule) |
| `collision.maxResolveIterations` | `4` | 24 | contact passes per substep |
| `collision.groundProbe` | `0.05` | 25 | m below the feet that still counts as floor while dashing (S) |
| `collision.groundSnap` | `0.35` | 26 | m: walking/sliding off a drop this small keeps you grounded (ramps), SPEC 2.0 (S) |
| `collision.headroomSkin` | `0.02` | 27 | m shaved off the stand-up test capsule so wall contact is not "blocked" (S) |
| `collision.cornerCorrect` | `0.25` | 28 | (above) NIGHT5 F6: a rising head that clips a ceiling's edge within this share of the capsule radius slides past it |
| `collision.cornerCorrect` | `0.25` | 29 | (above) (cornerSkin m clear) instead of losing the jump (Celeste's corner correction; docs/MOVEMENT-CHANGES.md). |
| `body` | `{"capsuleRadius":0.35,"capsuleHeight":1.8,"eyeHeight":1.6}` | 33 | (above) §3.1 Collision body (three.js Capsule vs Octree) |
| `body.capsuleRadius` | `0.35` | 35 | m |
| `body.capsuleHeight` | `1.8` | 36 | m, total height including both caps |
| `body.eyeHeight` | `1.6` | 37 | m above the feet |
| `surface` | `{"floorMinNormalY":0.7,"wallMaxAbsNormalY":0.3}` | 40 | (above) §3.1 Surface type from the collision normal |
| `surface.floorMinNormalY` | `0.7` | 42 | floor when normal.y >= this |
| `surface.wallMaxAbsNormalY` | `0.3` | 43 | wall when \|normal.y\| < this; anything between is a slope |
| `speed` | `{"maxHorizontal":40}` | 46 | (above) §3.1 Safety cap, not a feel tool |
| `speed.maxHorizontal` | `40` | 48 | m/s |
| `ground` | `{"walkSpeed":8,"accel":10,"friction":6,"stopSpeed":2.5}` | 51 | (above) §3.2 Base movement, ground |
| `ground.walkSpeed` | `8` | 53 | m/s, every other speed is a multiple of this |
| `ground.accel` | `10` | 54 | 1/s, Quake-style (multiplied by wish speed), Quake III default |
| `ground.friction` | `6` | 55 | 1/s, Quake III default; skipped on the first grounded step |
| `ground.stopSpeed` | `2.5` | 56 | m/s, friction control floor, Q3 100 u/s at 40 u/m (S) |
| `air` | `{"accel":1.5}` | 59 | (above) §3.2 Base movement, air |
| `air.accel` | `1.5` | 61 | 1/s, Quake-style, cap applies only along the input direction (S) |
| `jump` | {…} | 64 | (above) §3.2 Jumping (Pittman: v0 = 2h/t, g = 2h/t²) |
| `jump.height` | `1.5` | 66 | m, apex height -> v0 ≈ 8.33 m/s (1.3: floatier) |
| `jump.apexTime` | `0.36` | 67 | s, time to apex -> gravity ≈ 23.1 m/s² |
| `jump.fallGravityMult` | `1.35` | 68 | × gravity once past the apex (S) |
| `jump.apexHangMult` | `0.5` | 69 | × gravity near the apex while Space is held (Celeste) (S) |
| `jump.apexHangSpeed` | `2` | 70 | m/s, apex hang applies while \|vertical speed\| is below this (S) |
| `jump.coyoteTime` | `0.1` | 71 | s after leaving a floor that a jump still counts (Celeste) |
| `jump.bufferTime` | `0.1` | 72 | s before landing that a jump press is remembered (S) |
| `jump.releaseMult` | `0.5` | 73 | × vertical speed when Space is released while rising (S) |
| `jump.doubleJumps` | `1` | 74 | per airtime; refreshed on landing, wall-ride start and wall jump |
| `jump.doubleJumpMult` | `0.9` | 75 | × v0 |
| `dash` | {…} | 78 | (above) §3.3 Dash + stamina |
| `dash.speed` | `24` | 80 | m/s (3× walk) |
| `dash.duration` | `0.25` | 81 | s -> covers 6 m (S) |
| `dash.pipCost` | `1` | 82 | stamina pips per dash |
| `dash.maxPips` | `3` | 83 | stamina pips |
| `dash.regenPerSec` | `0.7` | 84 | pips/s, one pip at a time, none while dashing |
| `slide` | {…} | 87 | (above) §3.3 Slide + slide jump |
| `slide.cameraDrop` | `0.7` | 89 | m the camera lowers while sliding (eye 0.9 m) |
| `slide.colliderHeight` | `1` | 90 | m capsule height while sliding |
| `slide.floorSpeed` | `11.6` | 91 | m/s minimum slide speed (1.45× walk) |
| `slide.friction` | `1.5` | 92 | 1/s above the floor speed (S) |
| `slide.jumpHorizontalMult` | `1.2` | 93 | × horizontal speed on a slide jump, only below the boost cap (S) |
| `slide.jumpBoostMaxSpeed` | `24` | 94 | m/s, slide jump never boosts past this; at or above it speed is kept |
| `slide.slopeBoost` | `1.6` | 95 | × g·sinθ·cosθ gained sliding downhill (bled uphill), SPEC 2.0 (S) |
| `slide.slopeRampLength` | `8` | 96 | m, the Spire slide ramps (20° wedges), for the tests |
| `dashJump` | {…} | 99 | (above) §3.3 Dash jump (horizontal boost, ULTRAKILL "wind" window) |
| `dashJump.v0Mult` | `0.8` | 101 | × v0 |
| `dashJump.gravityMult` | `0.5` | 102 | × gravity during the window |
| `dashJump.windowTime` | `0.5` | 103 | s of lighter gravity |
| `dashJump.extraPipCost` | `1` | 104 | pips on top of the dash (2 total) |
| `slam` | {…} | 107 | (above) §3.3 Slam |
| `slam.speed` | `48` | 109 | m/s straight down (6× walk) |
| `slam.shockwaveRadius` | `5` | 110 | m |
| `slam.shockwaveDamage` | `30` | 111 | HP |
| `slam.slidePredictTime` | `0.1` | 112 | s, a C press this close to a predicted landing slides instead |
| `boostJump` | `{"window":0.15,"v0Mult":1.8}` | 115 | (above) §3.3 Boost jump (vertical, after a slam landing) |
| `boostJump.window` | `0.15` | 117 | s after a slam landing |
| `boostJump.v0Mult` | `1.8` | 118 | × v0, about 4.9 m high with the 1.3 jump (S) |
| `wallRide` | {…} | 121 | (above) §3.3 Wall ride |
| `wallRide.entrySpeed` | `4` | 123 | m/s along the wall needed to start (and keep) a ride |
| `wallRide.minSpeed` | `8` | 124 | m/s floor on speed along the wall while riding |
| `wallRide.gravityMult` | `0.15` | 125 | × gravity while riding (S) |
| `wallRide.maxTime` | `1.2` | 126 | s, the ride ends after this (S) |
| `wallRide.rollDeg` | `10` | 127 | camera roll away from the wall (Karlson) |
| `wallRide.leadTime` | `0.15` | 128 | NIGHT5 F5: the roll starts this many s before an airborne contact (rays at this height) |
| `wallRide.leadHeight` | `1` | 128 | NIGHT5 F5: the roll starts this many s before an airborne contact (rays at this height) |
| `wallRide.probeDistance` | `0.12` | 129 | m, stick probe along −wallNormal each step (S) |
| `wallRide.awayDot` | `0.5` | 130 | (input direction · wall normal) above this ends the ride (S) |
| `wallRide.cornerAngleDeg` | `30` | 131 | wall normal change that ends the ride (convex corner) |
| `wallRide.rejoinLockTime` | `0.15` | 132 | s after a wall jump before the same wall can be ridden again |
| `wallJump` | `{"outSpeed":8,"v0Mult":0.9,"maxPerAirtime":3}` | 135 | (above) §3.3 Wall jump |
| `wallJump.outSpeed` | `8` | 137 | m/s along the wall normal |
| `wallJump.v0Mult` | `0.9` | 138 | × v0 upward |
| `wallJump.maxPerAirtime` | `3` | 139 | ULTRAKILL |
| `style` | {…} | 142 | (above) §7 Style meter (SPEC 2.0; S unless noted) |
| `style.thresholds` | `[0,90,240,470,800,1300]` | 144 | (above) OVERNIGHT2 N1 pacing: uneven, rising thresholds (the cumulative points to reach D, C, B, A, S, SS) and a top |
| `style.thresholds` | `[0,90,240,470,800,1300]` | 145 | (above) band for SKYGRIND's bar. The pace targets (style/styleSim.js, tests/styleSim.test.js): one-weapon or fist spam |
| `style.thresholds` | `[0,90,240,470,800,1300]` | 146 | (above) never passes BOLD; varied play reaches SAVAGE in 35-60 s and SKYGRIND after 75-120 s; camping decays to DULL. |
| `style.topBand` | `500` | 148 | points of SKYGRIND's own band (the meter's top is 1300 + this) |
| `style.drainBase` | `17` | 149 | points/s at the lowest rank (OVERNIGHT2 N1/N2 pace: was 10) |
| `style.drainPerRank` | `4.8` | 150 | + this per rank above it (was 5; still rising with the rank) |
| `style.diminish` | `{"window":4,"repeat":0.6,"floor":0.15}` | 151 | the same award from the same source within 4 s: x0.6 per repeat |
| `style.diminish.window` | `4` | 151 | the same award from the same source within 4 s: x0.6 per repeat |
| `style.diminish.repeat` | `0.6` | 151 | the same award from the same source within 4 s: x0.6 per repeat |
| `style.diminish.floor` | `0.15` | 151 | the same award from the same source within 4 s: x0.6 per repeat |
| `style.hitCap` | `{"perSecond":12,"burst":12}` | 152 | hit-award income (HIT, HEADSHOT) at most 12 points a second |
| `style.hitCap.perSecond` | `12` | 152 | hit-award income (HIT, HEADSHOT) at most 12 points a second |
| `style.hitCap.burst` | `12` | 152 | hit-award income (HIT, HEADSHOT) at most 12 points a second |
| `style.hitShotGroup` | `0.25` | 153 | s: nails and flame ticks this close to the first of a group are one "shot" for hit awards |
| `style.damagePenalty` | `30` | 154 | points lost when hurt |
| `style.rankDownDelay` | `1` | 155 | s below a threshold before the shown rank drops |
| `style.stateMult` | `1.2` | 156 | × while airborne, sliding or wall riding (OVERNIGHT2 N1: was 1.5) |
| `style.base.swarmerKill` | `15` | 159 | SURVIVAL S6 |
| `style.base.spitterKill` | `40` | 159 | SURVIVAL S6 |
| `style.base.chargerKill` | `60` | 159 | SURVIVAL S6 |
| `style.base.lancerKill` | `45` | 159 | SURVIVAL S6 |
| `style.base.wardenKill` | `70` | 159 | SURVIVAL S6 |
| `style.bonus` | {…} | 161 | (above) OVERNIGHT2 N1: bonuses cut to fit the pace targets (they used to stack to ~3x a kill on every swap kill). |
| `style.multikillWindow` | `0.5` | 163 | s |
| `style.closeCallDistance` | `2` | 164 | m |
| `style.slingshotSpeed` | `20` | 166 | m/s: letting go of the ARCLINE faster than this is a SLINGSHOT (OVERNIGHT2 N4) |
| `style.trickEnemyRange` | `25` | 167 | m, tricks score only with an enemy this close |
| `style.trickRepeatWindow` | `3` | 168 | s, the same trick again within this scores 0 |
| `style.freshness.max` | `10` | 170 | per use / per other use / per s |
| `style.freshness.start` | `10` | 170 | per use / per other use / per s |
| `style.freshness.useCost` | `0.7` | 170 | per use / per other use / per s |
| `style.freshness.otherGain` | `0.5` | 170 | per use / per other use / per s |
| `style.freshness.regen` | `0.1` | 170 | per use / per other use / per s |
| `style.freshness.freshAt` | `7.5` | 171 | OVERNIGHT2 N1: FRESH was x1.5 |
| `style.freshness.usedAt` | `5` | 171 | OVERNIGHT2 N1: FRESH was x1.5 |
| `style.freshness.freshMult` | `1.1` | 171 | OVERNIGHT2 N1: FRESH was x1.5 |
| `style.freshness.usedMult` | `1` | 171 | OVERNIGHT2 N1: FRESH was x1.5 |
| `style.freshness.staleMult` | `0.5` | 171 | OVERNIGHT2 N1: FRESH was x1.5 |
| `style.freshness.nailgunUseScale` | `0.1` | 172 | a nail is 1/10 of a use (12 per second would go stale in a second) |
| `style.rankRewardPerRank` | `0.08` | 174 | stamina regen +8% per rank above the lowest (SPEC 2.0 §7) |
| `style.pulseBase` | `0.8` | 175 | (above) Phase 5 feedback (S) |
| `style.pulseBase` | `0.8` | 176 | Hz, meter bar pulse at DULL |
| `style.pulsePerRank` | `0.5` | 177 | Hz added per rank |
| `style.warmRank` | `4` | 178 | from this rank (S) the sky warms and speed lines take the rank colour |
| `style.popTime` | `0.26` | 179 | s, the rank letter pop (1.4 -> 1 with overshoot) |
| `style.glowTime` | `0.3` | 181 | s, the screen-edge glow on a rank-up |
| `style.kickDeg` | `0.8` | 183 | camera kick on a rank-up (up) |
| `style.feedSlide` | `40` | 186 | px the feed entries slide in from |
| `style.feedSlideTime` | `0.16` | 187 | s |
| `style.energyEase` | `3` | 188 | 1/s, how fast the world follows the rank energy (frame time) |
| `style.neonBoost` | `0.55` | 189 | share the cyan neon moves toward white at full energy |
| `style.skyWarmColor` | `16742981` | 190 | the horizon at S+ (Phase 6's horizon colour) |
| `style.skyWarmMax` | `0.35` | 191 | share of the sky / fog colour pulled toward it at the top rank |
| `jumpPad` | {…} | 202 | (above) §5 Jump pads |
| `jumpPad.apexHeight` | `8` | 204 | m, launch speed = √(2·g·h), ≈ 19.2 m/s with the 1.3 jump (S) |
| `jumpPad.radius` | `1` | 205 | m, trigger + disc radius (S) |
| `jumpPad.triggerHeight` | `0.3` | 206 | m above the pad surface that still counts as standing on it (S) |
| `jumpPad.ringWidth` | `0.12` | 207 | m, the thin ring around the disc |
| `jumpPad.discThickness` | `0.04` | 208 | m, visual only (pads never collide) |
| `jumpPad.segments` | `32` | 209 | circle segments for the disc and ring |
| `camera` | {…} | 212 | (above) §3.5 Camera + FOV (the base FOV default; the live value is in settings) |
| `camera.fovDeg` | `74` | 214 | vertical FOV (about 105° horizontal at 16:9) |
| `camera.fovBoostDeg` | `8` | 215 | extra FOV at full boost speed (S) |
| `camera.fovBoostStartSpeed` | `8` | 216 | m/s where widening starts (walk) |
| `camera.fovBoostFullSpeed` | `24` | 217 | m/s where widening is full (dash) |
| `camera.fovEaseRate` | `8` | 218 | 1/s, exponential ease toward the speed FOV target (S) |
| `camera.dashFovKickDeg` | `10` | 219 | extra FOV during a dash (S) |
| `camera.dashFovInTime` | `0.05` | 220 | s to ease the kick in (S) |
| `camera.dashFovOutTime` | `0.25` | 221 | s to ease the kick out (S) |
| `camera.maxFovBoostDeg` | `18` | 222 | total FOV never exceeds base + this |
| `camera.speedFovCapDeg` | `10` | 223 | NIGHT5 F5: the speed ramp + the slide widening never add more than this (the dash kick is on top) |
| `camera.near` | `0.05` | 224 | m near clip plane (S) |
| `camera.far` | `450` | 225 | m far clip plane, past the fog and the sky dome (S) |
| `camera.eyeEaseTime` | `0.1` | 226 | s for the eye to move between standing and sliding height (S) |
| `feel` | {…} | 229 | (above) §3.5 Feel layer (camera parts; scaled by settings.effectsIntensity) |
| `feel.landingDipPerSpeed` | `0.01` | 231 | m of camera dip per m/s of fall speed (S) |
| `feel.landingDipMax` | `0.25` | 232 | m (S) |
| `feel.landingDipReturnTime` | `0.2` | 233 | s spring back (S) |
| `feel.landingDipAttack` | `0.06` | 234 | s to reach the full dip, spread over a few frames (1.5 snap fix) (S) |
| `feel.strafeRollDeg` | `1.5` | 235 | camera roll when strafing at walk speed (S) |
| `feel.rollEaseRate` | `10` | 236 | 1/s, exponential ease toward the target roll (S) |
| `feel.hitstopTime` | `0.04` | 237 | s freeze on a kill (S) |
| `feel.momentumHitstop` | `0.045` | 238 | OVERNIGHT2 N8: s freeze on a melee hit with momentum over this |
| `feel.momentumHitstopAt` | `0.6` | 238 | OVERNIGHT2 N8: s freeze on a melee hit with momentum over this |
| `feel.trauma` | {…} | 239 | (above) NIGHT5 F2: trauma shake (fx/trauma.js, Eiserloh GDC 2016): rotation only, trauma^exponent x the max angle x smooth |
| `feel.trauma` | {…} | 240 | (above) noise, falling `decay` per s. Events add trauma: a shot by weight class, a kill, a slam landing, boss events. The |
| `feel.trauma` | {…} | 241 | (above) viewmodel takes vmShare of it (Source: 0.1). x EFFECTS INTENSITY; none with REDUCED MOTION. decay 2.5 (not the |
| `feel.trauma` | {…} | 242 | (above) brief's "about 1.5"): a full shake is gone in 400 ms. |
| `feel.fovPunch` | `{"deg":1.5,"attack":0.02,"recover":0.08}` | 248 | (above) NIGHT5 F4: heavy-hit emphasis (frame time, x EFFECTS INTENSITY, none with REDUCED MOTION): the FOV widens a touch |
| `feel.fovPunch` | `{"deg":1.5,"attack":0.02,"recover":0.08}` | 249 | (above) on a multikill (kills inside style.multikillWindow) or a rank-up, narrows on a melee hit (the lunge into it; once a |
| `feel.fovPunch` | `{"deg":1.5,"attack":0.02,"recover":0.08}` | 250 | (above) swing), and the DISCUS catch adds trauma. deg, s to the peak, s in all. |
| `feel.speedLinesMinSpeed` | `20` | 252 | m/s where speed lines appear (S) |
| `feel.speedLinesFullSpeed` | `40` | 253 | m/s where speed lines reach full opacity (S) |
| `feel.speedLinesMaxOpacity` | `0.55` | 254 | opacity at full speed and during a dash (S) |
| `feel.speedLinesJitterDeg` | `3` | 255 | random rotation per frame so the lines shimmer (S) |
| `feel.effectsIntensityMax` | `2` | 256 | settings.effectsIntensity is clamped to 0..this (200%) |
| `feel.kickSlots` | `8` | 257 | (above) SPEC 1.5 feel additions (all S). Kicks are finite curves (fx/kicks.js) that end at exactly 0. |
| `feel.kickSlots` | `8` | 258 | simultaneous camera kicks (jump, slam, rank-up) |
| `feel.punch` | {…} | 259 | (above) NIGHT5 F2: the weapon punch (it replaces the finite recoil kicks): one underdamped closed-form spring per weight |
| `feel.punch` | {…} | 260 | (above) class (fx/springs.js; frequency Hz, damping ratio: overshoot = exp(-pi zeta / sqrt(1 - zeta^2)), 9.5 / 11.3 / 13.3%), |
| `feel.punch` | {…} | 261 | (above) kicked per shot with the gun's peak (deg, + up) and, on the automatic guns, a short pattern per shot [pitch x, yaw |
| `feel.punch` | {…} | 262 | (above) deg] cycling (it restarts after patternReset s without a shot) plus cosmetic jitter (x the pitch). The sum is soft- |
| `feel.punch` | {…} | 263 | (above) capped per class. Visual only: the aim ray is taken before it. x EFFECTS INTENSITY, x adsMult fully zoomed, none |
| `feel.punch` | {…} | 264 | (above) with REDUCED MOTION. |
| `feel.jumpKickDeg` | `1` | 288 | camera pitch up on a jump, wall jump or boost jump |
| `feel.jumpKickAttack` | `0.03` | 289 | s |
| `feel.jumpKickRecover` | `0.2` | 290 | s |
| `feel.slamPunchDeg` | `2` | 291 | camera pitch down on a slam landing |
| `feel.slamPunchAttack` | `0.02` | 292 | s |
| `feel.slamPunchRecover` | `0.2` | 293 | s |
| `feel.slideTiltDeg` | `3` | 294 | camera roll while sliding (+ leans left), follows the camera-roll toggle |
| `feel.slideFovDeg` | `3` | 295 | extra FOV while sliding (eased with the speed FOV) |
| `feel.speedLinesFadeTime` | `0.15` | 296 | s for speed-line opacity to ease to its target (1.5 snap fix) |
| `feel.dashTintOpacity` | `0.6` | 297 | peak opacity of the CSS dash edge tint (follows the dash FOV kick) |
| `feel.dashTintWidth` | `60` | 298 | px, blur width of the red / cyan edge fringes |
| `feel.dashTintOffset` | `8` | 299 | px, how far each fringe is shifted in from its edge |
| `hud` | {…} | 302 | (above) §8 HUD |
| `hud.speedMid` | `12` | 304 | m/s, speed readout turns cyan at this |
| `hud.speedHigh` | `24` | 305 | m/s, speed readout turns danger orange above this |
| `hud.calloutFadeTime` | `0.6` | 306 | s for a move callout to fade out |
| `hud.pipFlashTime` | `0.18` | 307 | s white flash on the pip a dash consumed (S) |
| `hud.failFlashTime` | `0.3` | 308 | s red flash when a dash fails with 0 pips (S) |
| `hud.crosshairDashSpread` | `2` | 309 | px the crosshair lines spread while dashing |
| `hud.hitMarkerTime` | `0.15` | 310 | s a hit marker shows (S) |
| `hud.hitMarkerSize` | `8` | 311 | px arm length (S) |
| `hud.hitMarkerGap` | `5` | 312 | px from the crosshair center to where each arm starts (applies on reload) |
| `hud.headshotScale` | `1.5` | 313 | × marker size on a headshot |
| `hud.killScale` | `1.9` | 314 | × marker size on a kill (drawn in danger orange) |
| `hud.hurtVignetteTime` | `0.35` | 315 | s red edge flash on damage (S) |
| `hud.hurtVignetteOpacity` | `0.6` | 316 | peak opacity (S) |
| `hud.styles` | `["classic","attached"]` | 317 | (above) §8.1 layout (1.5, all S) |
| `hud.styles` | `["classic","attached"]` | 318 | settings.hudStyle choices |
| `hud.edgeMargin` | `22` | 319 | px from the screen edges to the panels |
| `hud.panelSkewDeg` | `-8` | 320 | bottom-left plate skew |
| `hud.panelOpacity` | `0.55` | 321 | dark plate opacity |
| `hud.hpBarWidth` | `190` | 322 | px |
| `hud.staminaSegmentWidth` | `58` | 323 | px |
| `hud.shotgunSpread` | `6` | 324 | px the crosshair spreads on a shotgun shot |
| `hud.spreadRecoverTime` | `0.2` | 325 | s for the shotgun spread to close |
| `hud.feedSize` | `5` | 326 | callouts kept in the top-right feed |
| `hud.feedFadeTime` | `1.6` | 327 | s for a callout to fade out |
| `hud.attachPxPerMeter` | `1500` | 328 | Attached style: px of panel motion per m of viewmodel sway/bob |
| `hud.lowHpBlinkTime` | `0.25` | 329 | s per on/off blink of the HP number below health.lowHpFraction |
| `hud.markerCount` | `34` | 330 | enemy markers / edge arrows the HUD can show at once (24 alive + hunters + the boss) |
| `hud.chargeEdgeOpacity` | `0.55` | 331 | POWER P5: the screen-edge tint at full charge |
| `hud.momentumArc` | `{"radius":20,"width":2,"idleColor":"rgba(236,232,223,0.75)"}` | 332 | (above) OVERNIGHT2 N2 / POLISH V5: the momentum ring around the crosshair (a thin full ring that fills clockwise from the top) |
| `hud.threatTop` | `170` | 334 | px: the telegraph's edge arrows stay below the top-center cluster (the round, the boss bar; POLISH V5) |
| `hud.killFlashTime` | `0.09` | 335 | s / colour: the crosshair on a kill (OVERNIGHT2 N8) |
| `hud.killFlashColor` | `16765498` | 335 | s / colour: the crosshair on a kill (OVERNIGHT2 N8) |
| `hud.tooltip` | `{"perkRange":8,"offsetX":46,"offsetY":18}` | 336 | OVERNIGHT2 N3: the aim card (m; px from the crosshair) |
| `hud.tooltip.perkRange` | `8` | 336 | OVERNIGHT2 N3: the aim card (m; px from the crosshair) |
| `hud.tooltip.offsetX` | `46` | 336 | OVERNIGHT2 N3: the aim card (m; px from the crosshair) |
| `hud.tooltip.offsetY` | `18` | 336 | OVERNIGHT2 N3: the aim card (m; px from the crosshair) |
| `weapons` | {…} | 339 | (above) §4 Weapons (owner-agnostic; the Duel bot uses the same values) |
| `weapons.revolver.damage` | `25` | 342 | HP per hit |
| `weapons.revolver.fireInterval` | `0.4` | 343 | s between shots (hold to fire) |
| `weapons.revolver.headshotMult` | `2` | 344 | × damage on the head sphere |
| `weapons.revolver.range` | `200` | 345 | m (S) |
| `weapons.shotgun.pelletDamage` | `8` | 349 | HP per pellet |
| `weapons.shotgun.fireInterval` | `0.9` | 350 | s between shots |
| `weapons.shotgun.coneHalfAngleDeg` | `8` | 351 | pellets are uniform within this half-angle (S) |
| `weapons.shotgun.range` | `60` | 352 | m, no falloff (S) |
| `weapons.shotgun.jumpSpeed` | `10` | 353 | m/s added opposite the aim when fired airborne, once per shot (S) |
| `weapons.shotgun.pumpDelay` | `0.45` | 354 | s after a shot: the pump (shotgunPump event + viewmodel fore-end) (S) |
| `weapons.nailgun` | {…} | 356 | (above) SURVIVAL §4 (S3; all S): the three wall-buy weapons. Unlimited ammo; balance is rate, heat, charge. |
| `weapons.nailgun.rate` | `12` | 358 | nails per second (hold to fire) |
| `weapons.nailgun.pool` | `160` | 359 | nails in flight / nails stuck in enemies at once |
| `weapons.nailgun.stuckPool` | `140` | 359 | nails in flight / nails stuck in enemies at once |
| `weapons.nailgun.speed` | `60` | 360 | m/s |
| `weapons.nailgun.damage` | `9` | 361 | HP per nail |
| `weapons.nailgun.spreadDeg` | `1.1` | 362 | random cone half-angle per nail |
| `weapons.nailgun.heatTime` | `3` | 363 | s of continuous fire to overheat |
| `weapons.nailgun.lockTime` | `1.5` | 364 | s locked after an overheat (the heat drains meanwhile) |
| `weapons.nailgun.coolRate` | `0.8` | 365 | heat per s while not firing (0..1 scale) |
| `weapons.nailgun.range` | `110` | 366 | m a nail flies before it drops |
| `weapons.nailgun.stickTime` | `6` | 367 | s a stuck nail stays in an enemy |
| `weapons.nailgun.radius` | `0.05` | 368 | m collision |
| `weapons.rocket.fireInterval` | `0.9` | 371 | s |
| `weapons.rocket.pool` | `16` | 372 | rockets in flight at once |
| `weapons.rocket.speed` | `30` | 373 | m/s |
| `weapons.rocket.damage` | `70` | 374 | HP at the blast center |
| `weapons.rocket.edgeDamage` | `0.3` | 375 | × damage at the splash edge |
| `weapons.rocket.splashRadius` | `4` | 376 | m |
| `weapons.rocket.knockback` | `16` | 377 | m/s at the center: the rocket jump (0 self damage) |
| `weapons.rocket.knockbackMinUp` | `0.55` | 378 | share of the push that is always upward |
| `weapons.rocket.lifetime` | `4` | 379 | s |
| `weapons.rocket.radius` | `0.2` | 380 | m collision |
| `weapons.rocket.spawnForward` | `0.6` | 381 | m in front of the eye (a blast at your feet still reaches you) |
| `weapons.rail.charge` | `0.6` | 384 | s held to fire (it fires by itself when full) |
| `weapons.rail.damage` | `150` | 385 | HP to every enemy in line |
| `weapons.rail.cooldown` | `3` | 387 | s |
| `weapons.rail.range` | `250` | 388 | m |
| `weapons.rail.pierce` | `24` | 389 | enemies one beam can pass through |
| `weapons.rattler` | {…} | 391 | (above) POWER P4 (all S; balance: base damage per second by price tier, recorded in DECISIONS.md). Unlimited |
| `weapons.rattler` | {…} | 392 | (above) ammo: heat, cooldowns, charge and bursts are the limits. Each has a movement synergy. |
| `weapons.rattler` | {…} | 393 | 3-round burst SMG, kinetic, 1000 |
| `weapons.rattler.damage` | `9` | 394 | 27 per burst: 75/s |
| `weapons.rattler.headshotMult` | `1.5` | 394 | 27 per burst: 75/s |
| `weapons.rattler.rounds` | `3` | 394 | 27 per burst: 75/s |
| `weapons.rattler.roundInterval` | `0.06` | 394 | 27 per burst: 75/s |
| `weapons.rattler.burstInterval` | `0.36` | 394 | 27 per burst: 75/s |
| `weapons.rattler.spreadDeg` | `0.8` | 395 | the hip spread (POLISH V2: was 1.3; zoomed it is x config.ads.weapons.rattler.spreadMult) |
| `weapons.rattler.range` | `90` | 395 | the hip spread (POLISH V2: was 1.3; zoomed it is x config.ads.weapons.rattler.spreadMult) |
| `weapons.rattler.dashSpreadDeg` | `0.35` | 396 | strafe-shooting while dashing: tight + 25% faster |
| `weapons.rattler.dashCadence` | `0.75` | 396 | strafe-shooting while dashing: tight + 25% faster |
| `weapons.rattler.dashWindow` | `0.35` | 396 | strafe-shooting while dashing: tight + 25% faster |
| `weapons.torch` | {…} | 398 | short flame cone, fire, 1250: hold to spray, heat-limited, BURN |
| `weapons.torch.range` | `7` | 399 | per tick per target: 65/s + BURN (coneDeg = half-angle) |
| `weapons.torch.coneDeg` | `18` | 399 | per tick per target: 65/s + BURN (coneDeg = half-angle) |
| `weapons.torch.tick` | `0.1` | 399 | per tick per target: 65/s + BURN (coneDeg = half-angle) |
| `weapons.torch.damage` | `6.5` | 399 | per tick per target: 65/s + BURN (coneDeg = half-angle) |
| `weapons.torch.pointScale` | `0.3` | 400 | x a tick's hit points (10 ticks a second would out-earn every other gun) |
| `weapons.torch.heatTime` | `4` | 401 | s of spray to overheat; s locked; heat per s cooling |
| `weapons.torch.lockTime` | `1.5` | 401 | s of spray to overheat; s locked; heat per s cooling |
| `weapons.torch.coolRate` | `0.7` | 401 | s of spray to overheat; s locked; heat per s cooling |
| `weapons.torch.slideConeMult` | `1.5` | 402 | sliding through a group: a wider, hotter cone |
| `weapons.torch.slideDamageMult` | `1.25` | 402 | sliding through a group: a wider, hotter cone |
| `weapons.bouncer` | {…} | 404 | grenade launcher, blast, 1500 |
| `weapons.bouncer.damage` | `68` | 406 | 85/s at the centre |
| `weapons.bouncer.edgeDamage` | `0.3` | 406 | 85/s at the centre |
| `weapons.bouncer.splashRadius` | `3.5` | 406 | 85/s at the centre |
| `weapons.bouncer.knockback` | `13` | 407 | the grenade jump (0 self damage), like the rocket |
| `weapons.bouncer.knockbackMinUp` | `0.55` | 407 | the grenade jump (0 self damage), like the rocket |
| `weapons.bouncer.pool` | `12` | 408 | grenades in flight; m/s of loft added to the throw |
| `weapons.bouncer.upward` | `2.5` | 408 | grenades in flight; m/s of loft added to the throw |
| `weapons.bouncer.spawnForward` | `0.5` | 409 | m in front of the eye; below this speed on a floor it stops bouncing |
| `weapons.bouncer.restSpeed` | `2` | 409 | m in front of the eye; below this speed on a floor it stops bouncing |
| `weapons.bouncer.spikeDirY` | `0.8` | 410 | aimed this steeply down it sticks where it lands and pops after this many s |
| `weapons.bouncer.spikeFuse` | `0.12` | 410 | aimed this steeply down it sticks where it lands and pops after this many s |
| `weapons.zapper` | {…} | 412 | hitscan arc that chains, volt, 1750: SHOCK |
| `weapons.zapper.damage` | `26` | 413 | 87/s on the first target |
| `weapons.zapper.fireInterval` | `0.3` | 413 | 87/s on the first target |
| `weapons.zapper.range` | `32` | 413 | 87/s on the first target |
| `weapons.zapper.headshotMult` | `1.5` | 413 | 87/s on the first target |
| `weapons.zapper.chains` | `3` | 414 | up to 3 more, each 70% of the one before |
| `weapons.zapper.chainRange` | `7` | 414 | up to 3 more, each 70% of the one before |
| `weapons.zapper.chainFalloff` | `0.7` | 414 | up to 3 more, each 70% of the one before |
| `weapons.zapper.airChains` | `1` | 415 | synergy: one more link while airborne |
| `weapons.discus` | {…} | 418 | ricochet saw disc, edge, 2250 |
| `weapons.discus.speed` | `34` | 419 | 80/s + ricochets |
| `weapons.discus.damage` | `52` | 419 | 80/s + ricochets |
| `weapons.discus.fireInterval` | `0.65` | 419 | 80/s + ricochets |
| `weapons.discus.bounces` | `4` | 419 | 80/s + ricochets |
| `weapons.discus.lifetime` | `2.5` | 419 | 80/s + ricochets |
| `weapons.discus.radius` | `0.28` | 419 | 80/s + ricochets |
| `weapons.discus.pierceHp` | `60` | 420 | passes through enemies whose max HP is this or less (Husks, Swarmers, Spitters) |
| `weapons.discus.pool` | `8` | 421 | +15% damage per wall bounce so far (angle shots off the pillars) |
| `weapons.discus.bounceBonus` | `0.15` | 421 | +15% damage per wall bounce so far (angle shots off the pillars) |
| `weapons.discus.spawnForward` | `0.6` | 422 | m; s before the same disc can cut the same body again |
| `weapons.discus.rehitTime` | `0.3` | 422 | m; s before the same disc can cut the same body again |
| `weapons.discus.slideBounces` | `1` | 423 | synergy: thrown during a slide it skims flat with one more bounce |
| `weapons.vortex` | {…} | 425 | slow gravity orb, gravity, 2500: PULL, then a burst |
| `weapons.vortex.speed` | `14` | 426 | s of flight before it opens anyway |
| `weapons.vortex.radius` | `0.35` | 426 | s of flight before it opens anyway |
| `weapons.vortex.lifetime` | `1.2` | 426 | s of flight before it opens anyway |
| `weapons.vortex.pullRadius` | `7` | 427 | m, s, m/s it drags enemies toward its centre |
| `weapons.vortex.pullTime` | `1.5` | 427 | m, s, m/s it drags enemies toward its centre |
| `weapons.vortex.pullSpeed` | `9` | 427 | m, s, m/s it drags enemies toward its centre |
| `weapons.vortex.pullCore` | `1.5` | 429 | m where the pull eases off; s each pull step holds; PULL outlasts it |
| `weapons.vortex.pullHold` | `0.05` | 429 | m where the pull eases off; s each pull step holds; PULL outlasts it |
| `weapons.vortex.pullGrace` | `0.5` | 429 | m where the pull eases off; s each pull step holds; PULL outlasts it |
| `weapons.vortex.spawnForward` | `0.6` | 430 | m; m (visual); s between motes drawn in |
| `weapons.vortex.ringRadius` | `1.4` | 430 | m; m (visual); s between motes drawn in |
| `weapons.vortex.moteInterval` | `0.03` | 430 | m; m (visual); s between motes drawn in |
| `weapons.vortex.dashWindow` | `0.35` | 431 | synergy: fired right after a dash it flies 60% faster |
| `weapons.vortex.dashSpeedMult` | `1.6` | 431 | synergy: fired right after a dash it flies 60% faster |
| `weapons.choir` | {…} | 433 | (above) OVERNIGHT2 N2 new guns. CHOIR: a spin-up minigun with heat; PRISM: a beam that reflects off walls and splits. |
| `weapons.choir` | {…} | 434 | kinetic, 3500: 6 a round at up to 18 a second once spun up (108/s), heat-limited |
| `weapons.prism` | {…} | 438 | pierce, 2750: a reflecting beam: 34 a segment, bounces off walls, splits on the last bounce |
| `weapons.specials` | {…} | 441 | (above) OVERNIGHT2 N2 §3.3: every gun's special (alt fire, the second mouse button) and its momentum variant (the trigger |
| `weapons.specials` | {…} | 442 | (above) pull at M above config.momentum.variantAt). Balance notes in DECISIONS.md. |
| `weapons.specials.revolver` | `{"pierce":1}` | 444 | no special (right mouse zooms: POLISH V2); variant: the bullet goes through one enemy |
| `weapons.specials.revolver.pierce` | `1` | 444 | no special (right mouse zooms: POLISH V2); variant: the bullet goes through one enemy |
| `weapons.specials.bouncer` | `{"variantRadius":1.3}` | 450 | alt: every grenade of yours in flight bursts now |
| `weapons.specials.bouncer.variantRadius` | `1.3` | 450 | alt: every grenade of yours in flight bursts now |
| `weapons.gauntlet` | {…} | 459 | (above) Melee (hotbar weapons, swung with fire; F stays the universal punch + parry). Sweep queries, not physics. |
| `weapons.gauntlet` | {…} | 460 | (above) coneDeg is the half-angle of each swing's sweep. |
| `weapons.gauntlet` | {…} | 461 | 76/s, parries |
| `weapons.gauntlet.damage` | `32` | 461 | 76/s, parries |
| `weapons.gauntlet.cycle` | `0.42` | 461 | 76/s, parries |
| `weapons.gauntlet.range` | `2.4` | 461 | 76/s, parries |
| `weapons.gauntlet.coneDeg` | `35` | 461 | 76/s, parries |
| `weapons.gauntlet.knockback` | `7` | 461 | 76/s, parries |
| `weapons.gauntlet.knockbackTime` | `0.2` | 461 | 76/s, parries |
| `weapons.gauntlet.parryPeek` | `1` | 461 | 76/s, parries |
| `weapons.sunder.damage` | `[38,38,58]` | 463 | 106/s |
| `weapons.sunder.cycle` | `[0.38,0.38,0.5]` | 463 | 106/s |
| `weapons.sunder.range` | `2.8` | 463 | 106/s |
| `weapons.sunder.coneDeg` | `[45,45,80]` | 463 | 106/s |
| `weapons.sunder.comboWindow` | `0.7` | 463 | 106/s |
| `weapons.sunder.lunge` | `6` | 464 | dash then slash: a 6 m lunge |
| `weapons.sunder.lungeCone` | `25` | 464 | dash then slash: a 6 m lunge |
| `weapons.sunder.lungeWindow` | `0.3` | 464 | dash then slash: a 6 m lunge |
| `weapons.sunder.lungeTime` | `0.18` | 464 | dash then slash: a 6 m lunge |
| `weapons.flicker` | {…} | 466 | twin stabs (2 x 10) every 0.18 s: 111/s; +1 m in a dash / ride |
| `weapons.flicker.damage` | `10` | 466 | twin stabs (2 x 10) every 0.18 s: 111/s; +1 m in a dash / ride |
| `weapons.flicker.cycle` | `0.18` | 466 | twin stabs (2 x 10) every 0.18 s: 111/s; +1 m in a dash / ride |
| `weapons.flicker.range` | `2.2` | 466 | twin stabs (2 x 10) every 0.18 s: 111/s; +1 m in a dash / ride |
| `weapons.flicker.coneDeg` | `30` | 466 | twin stabs (2 x 10) every 0.18 s: 111/s; +1 m in a dash / ride |
| `weapons.flicker.quickReach` | `1` | 466 | twin stabs (2 x 10) every 0.18 s: 111/s; +1 m in a dash / ride |
| `weapons.thumper.damage` | `90` | 468 | 90/s + the wave |
| `weapons.thumper.windup` | `0.35` | 468 | 90/s + the wave |
| `weapons.thumper.cycle` | `1` | 468 | 90/s + the wave |
| `weapons.thumper.range` | `3` | 468 | 90/s + the wave |
| `weapons.thumper.coneDeg` | `40` | 468 | 90/s + the wave |
| `weapons.thumper.ringRadius` | `4.5` | 469 | m, HP, s |
| `weapons.thumper.ringDamage` | `35` | 469 | m, HP, s |
| `weapons.thumper.bigRingRadius` | `7` | 469 | m, HP, s |
| `weapons.thumper.bigRingDamage` | `60` | 469 | m, HP, s |
| `weapons.thumper.stagger` | `1.5` | 469 | m, HP, s |
| `weapons.thumper.ringForward` | `1.2` | 470 | the ring's centre ahead of you; its reach up / down |
| `weapons.thumper.ringHeight` | `2.5` | 470 | the ring's centre ahead of you; its reach up / down |
| `weapons.thumper.ringPush` | `5` | 470 | the ring's centre ahead of you; its reach up / down |
| `weapons.thumper.ringPushTime` | `0.2` | 470 | the ring's centre ahead of you; its reach up / down |
| `weapons.thumper.slamWindow` | `0.3` | 471 | s after a slam lands that still makes the big wave; s a mid-slam swing waits to land |
| `weapons.thumper.maxWait` | `1.5` | 471 | s after a slam lands that still makes the big wave; s a mid-slam swing waits to land |
| `weapons.tether` | {…} | 473 | m/s toward you |
| `weapons.tether.damage` | `22` | 473 | m/s toward you |
| `weapons.tether.cycle` | `0.55` | 473 | m/s toward you |
| `weapons.tether.range` | `6` | 473 | m/s toward you |
| `weapons.tether.coneDeg` | `10` | 473 | m/s toward you |
| `weapons.tether.pull` | `14` | 473 | m/s toward you |
| `weapons.tether.pullTime` | `0.25` | 473 | m/s toward you |
| `weapons.tether.pulls` | `["husk","swarmer"]` | 473 | m/s toward you |
| `weapons.all` | array[18] | 474 | (above) Every weapon in shop order (Training owns them all). The gauntlet is free and the Kestrel starts in hand. |
| `weapons.catalog` | {…} | 482 | (above) POWER P3: every weapon's family (the cross combos + its core gem / trim / flash / tracer colour), kind (gun or |
| `weapons.catalog` | {…} | 483 | (above) melee) and freshness cost per shot, nail, round or swing (fast weapons small, slow weapons large). |
| `weapons.catalog.rattler` | {…} | 491 | per round |
| `weapons.catalog.rattler.name` | `"RATTLER"` | 491 | per round |
| `weapons.catalog.rattler.family` | `"kinetic"` | 491 | per round |
| `weapons.catalog.rattler.kind` | `"gun"` | 491 | per round |
| `weapons.catalog.rattler.freshCost` | `0.02` | 491 | per round |
| `weapons.catalog.torch` | {…} | 492 | per tick |
| `weapons.catalog.torch.name` | `"TORCH"` | 492 | per tick |
| `weapons.catalog.torch.family` | `"fire"` | 492 | per tick |
| `weapons.catalog.torch.kind` | `"gun"` | 492 | per tick |
| `weapons.catalog.torch.freshCost` | `0.012` | 492 | per tick |
| `weapons.catalog.choir` | {…} | 501 | per round (OVERNIGHT2 N2) |
| `weapons.catalog.choir.name` | `"CHOIR"` | 501 | per round (OVERNIGHT2 N2) |
| `weapons.catalog.choir.family` | `"kinetic"` | 501 | per round (OVERNIGHT2 N2) |
| `weapons.catalog.choir.kind` | `"gun"` | 501 | per round (OVERNIGHT2 N2) |
| `weapons.catalog.choir.freshCost` | `0.01` | 501 | per round (OVERNIGHT2 N2) |
| `weapons.families` | {…} | 504 | family colours (never the enemy red-orange or RIVAL magenta) |
| `weapons.arsenal` | {…} | 507 | (above) POWER P3: freshness, the primed swap, cross combos + statuses (all S unless POWER gives the number). |
| `weapons.arsenal.freshMin` | `0.65` | 509 | x damage and points at FRESH 0 and FRESH 1 |
| `weapons.arsenal.freshMax` | `1.25` | 509 | x damage and points at FRESH 0 and FRESH 1 |
| `weapons.arsenal.recover` | `0.25` | 510 | FRESH per second while a weapon isn't in hand |
| `weapons.arsenal.recoverDelay` | `0.5` | 511 | s after a weapon's last use before it recovers (OVERNIGHT2 N1: F punches wear the Gauntlet for real) |
| `weapons.arsenal.staleAt` | `0.4` | 512 | FRESH under this reads as stale (the icon dims) |
| `weapons.arsenal.primedWindow` | `1` | 513 | s after a swap-in; +35% on the first hit |
| `weapons.arsenal.primedBonus` | `0.35` | 513 | s after a swap-in; +35% on the first hit |
| `weapons.arsenal.swapKillWindow` | `2` | 514 | s: a kill this soon after a swap is a SWAP (style bonus) |
| `weapons.arsenal.crossWindow` | `3` | 515 | s; x damage for a hit from a different family than the last hit on it |
| `weapons.arsenal.crossMult` | `1.5` | 515 | s; x damage for a hit from a different family than the last hit on it |
| `weapons.arsenal.collapseMult` | `2` | 516 | PULL then a blast hit: x damage, and every other enemy in the radius too |
| `weapons.arsenal.collapseRadius` | `4` | 516 | PULL then a blast hit: x damage, and every other enemy in the radius too |
| `weapons.arsenal.overloadMult` | `1.75` | 517 | SHOCK then a melee hit + a burst |
| `weapons.arsenal.overloadRadius` | `5` | 517 | SHOCK then a melee hit + a burst |
| `weapons.arsenal.overloadDamage` | `20` | 517 | SHOCK then a melee hit + a burst |
| `weapons.arsenal.overloadTargets` | `3` | 517 | SHOCK then a melee hit + a burst |
| `weapons.arsenal.statusTime` | `{"burn":3,"shock":2.5,"pull":1.5,"stuck":6,"stagger":1.5}` | 518 | s each status lasts |
| `weapons.arsenal.statusTime.burn` | `3` | 518 | s each status lasts |
| `weapons.arsenal.statusTime.shock` | `2.5` | 518 | s each status lasts |
| `weapons.arsenal.statusTime.pull` | `1.5` | 518 | s each status lasts |
| `weapons.arsenal.statusTime.stuck` | `6` | 518 | s each status lasts |
| `weapons.arsenal.statusTime.stagger` | `1.5` | 518 | s each status lasts |
| `weapons.arsenal.memory` | `8` | 519 | s an enemy's last-hit record is kept |
| `weapons.arsenal.burnDps` | `8` | 520 | BURN: damage per second, dealt in ticks of this many s |
| `weapons.arsenal.burnTick` | `0.25` | 520 | BURN: damage per second, dealt in ticks of this many s |
| `weapons.prices` | {…} | 522 | ARMORY wall buys (POWER P4) |
| `weapons.finishes` | {…} | 526 | (above) NIGHT4 F1: gun finishes, cosmetic only: the neutral body tones of every gun (bone, gunmetal, dark, grip); the family |
| `weapons.finishes` | {…} | 527 | (above) trim and the core gem never change. unlock {medal, count}: that many different trials with that medal or better. |
| `weapons.finishes.list.factory` | `{"name":"FACTORY"}` | 531 | no unlock, no palette: the kit's own tones |
| `weapons.finishes.list.factory.name` | `"FACTORY"` | 531 | no unlock, no palette: the kit's own tones |
| `weapons.finishes.list.protocol` | {…} | 536 | (above) NIGHT6 B3: earned by completing THE PROTOCOL in a Survival run (modes/unlocks.js). |
| `weapons.finishes.list.skylight` | {…} | 538 | (above) NIGHT6 B3: the side secret's finish (the hidden relay on the ARMORY roof). |
| `weapons.survivalStart` | `["revolver","gauntlet"]` | 542 | Survival starts with the Kestrel and the (free) Pulse Gauntlet |
| `weapons.slots` | `["revolver","shotgun"]` | 543 | the default loadout (Duel, Classic Arena); Survival starts with the revolver |
| `weapons.swapTime` | `0.25` | 544 | s, lower + raise (SPEC 1.5) (S) |
| `weapons.swapLowerFraction` | `0.5` | 545 | share of the swap spent lowering; the slot switches at the bottom (S) |
| `weapons.swapFireFraction` | `0.7` | 546 | fire + specials unblock when the raise is this far done |
| `weapons.losSlack` | `0.05` | 547 | m: a weapon's line-of-sight ray may stop this short of its target and still count |
| `weapons.wheelCooldown` | `0.08` | 548 | s between mouse-wheel swaps (trackpads send bursts) (S) |
| `weapons.radial` | `{"deadZone":40,"maxOffset":160,"radius":170}` | 549 | px: the hold-Tab picker (mouse travel; the drawn ring) |
| `weapons.radial.deadZone` | `40` | 549 | px: the hold-Tab picker (mouse travel; the drawn ring) |
| `weapons.radial.maxOffset` | `160` | 549 | px: the hold-Tab picker (mouse travel; the drawn ring) |
| `weapons.radial.radius` | `170` | 549 | px: the hold-Tab picker (mouse travel; the drawn ring) |
| `weapons.hotbarShown` | `12` | 550 | hotbar icons at once (POWER P7: 12 of 16); it scrolls to keep the weapon in hand in view |
| `mods` | {…} | 553 | (above) SURVIVAL §4 + POWER P6 perk machines (14): blocky vending cabinets, each bought once per run (a boss kill |
| `mods` | {…} | 554 | (above) grants one free). Prices before the stack tax (+10% per perk already owned). Colours: a neon set without the |
| `mods` | {…} | 555 | (above) enemy red-orange or RIVAL's magenta; the pixel icon (8 x 8, '#' = lit) carries the identity. |
| `mods` | {…} | 556 | (above) NIGHT6 B2: MODS (survival/mods.js, modMath.js): pick 1 of 3 at the end of the listed rounds (then every `everyAfter`): |
| `mods` | {…} | 557 | (above) run upgrades that read how you play and never change movement. tag: a weapon family (its colour) or a playstyle; |
| `mods` | {…} | 558 | (above) tier weights the roll; max = stacks; `per` = the effect per stack (its unit in `desc`). `locked`: unlocked across runs |
| `mods` | {…} | 559 | (above) (B5, `unlock` names the condition). The draft pauses the game (Hades) until you pick; one paid REROLL a run. |
| `mods.styleTags` | {…} | 564 | never RIVAL's magenta |
| `mods.styleTags.AIR` | `1692646` | 564 | never RIVAL's magenta |
| `mods.styleTags.WALL` | `1692646` | 564 | never RIVAL's magenta |
| `mods.styleTags.SLIDE` | `1692646` | 564 | never RIVAL's magenta |
| `mods.styleTags.MELEE` | `4029439` | 564 | never RIVAL's magenta |
| `mods.styleTags.STYLE` | `16761405` | 564 | never RIVAL's magenta |
| `mods.styleTags.ECONOMY` | `16765498` | 564 | never RIVAL's magenta |
| `mods.styleTags.HEALTH` | `4054148` | 564 | never RIVAL's magenta |
| `objectives` | {…} | 588 | (above) NIGHT6 B7: objective rounds (survival/objectives.js). HOLD: a ring (radius m) on one of map.holdSpots in an open zone; |
| `objectives` | {…} | 589 | (above) stand in it `time` s in all; the round can't clear until it's held; combat points x pointsMult while inside; +bonus x |
| `objectives` | {…} | 590 | (above) round when held. GOLD RUSH: combat points x goldMult all round; `chainAt`+ kills inside chainWindow s pay chainBonus |
| `objectives` | {…} | 591 | (above) per kill of the chain. |
| `tips` | {…} | 594 | (above) NIGHT6 B6: tips in the moment (ui/tips.js): each shows the first time its moment comes in a run, for its first `times` |
| `tips` | {…} | 595 | (above) runs; {action} becomes the live key. |
| `daily` | {…} | 609 | (above) NIGHT6 B5: the DAILY's twist (modes/daily.js): today's seed also picks one of these mutators (never LOW GRAVITY: a |
| `daily` | {…} | 610 | (above) movement change) and a first mod you start with. The first finished run of the UTC day is the day's score. |
| `protocol` | {…} | 613 | (above) NIGHT6 B3: THE PROTOCOL (survival/protocol.js), Survival's guided main quest: four RELAYS (map.relaySpots) wake at |
| `protocol` | {…} | 614 | (above) wakeRound; hold BUY at one (its zone open) to light it; the next one's beacon (guideHeight m) stands over the map and |
| `protocol` | {…} | 615 | (above) the HUD names it. All four lit: the `surge` squads join the round; clearing it completes the quest: every owned gun |
| `protocol` | {…} | 616 | (above) forged free, +bonus points, the PROTOCOL finish for good (weapons.finishes). |
| `forge` | {…} | 623 | (above) NIGHT6 B1: THE FORGE (survival/forge.js), Survival's Pack-a-Punch: hold BUY at its cabinet in THE GRINDER with a weapon |
| `forge` | {…} | 624 | (above) in hand to forge it for the run. Enemy HP is 1 + 0.08 x (round - 10) (director.rounds): x1.8 at round 20, so a gun |
| `forge` | {…} | 625 | (above) forged deals at round 20 what it dealt unforged at round 10. One bonus per family: a status on `chance` of hits (the |
| `forge` | {…} | 626 | (above) run's seeded stream), or x on headshots / blast hits / tough targets (bosses, toughKinds, affixed enemies). |
| `forge.flourish` | `[-10,-0.03,0.01,8]` | 629 | the gun's spring kick as it comes out of the forge (muzzle deg, back m, drop m, roll deg) |
| `perks.vigor` | {…} | 647 | 4 total |
| `perks.vigor.price` | `1500` | 647 | 4 total |
| `perks.vigor.pips` | `1` | 647 | 4 total |
| `perks.vigor.color` | `4054148` | 647 | 4 total |
| `perks.vigor.text` | `"+1 STAMINA PIP"` | 647 | 4 total |
| `perks.mend` | {…} | 651 | delay -40%, rate +50% |
| `perks.mend.price` | `2000` | 651 | delay -40%, rate +50% |
| `perks.mend.delayMult` | `0.6` | 651 | delay -40%, rate +50% |
| `perks.mend.rateMult` | `1.5` | 651 | delay -40%, rate +50% |
| `perks.mend.color` | `10354632` | 651 | delay -40%, rate +50% |
| `perks.mend.text` | `"FASTER REGEN"` | 651 | delay -40%, rate +50% |
| `perks.plate` | {…} | 657 | 100 -> 150 |
| `perks.plate.price` | `2500` | 657 | 100 -> 150 |
| `perks.plate.maxHp` | `150` | 657 | 100 -> 150 |
| `perks.plate.color` | `14214384` | 657 | 100 -> 150 |
| `perks.plate.text` | `"150 MAX HP"` | 657 | 100 -> 150 |
| `perks.flow` | {…} | 661 | decay -40%, rank gain +20% |
| `perks.flow.price` | `3000` | 661 | decay -40%, rank gain +20% |
| `perks.flow.drain` | `0.6` | 661 | decay -40%, rank gain +20% |
| `perks.flow.gain` | `1.2` | 661 | decay -40%, rank gain +20% |
| `perks.flow.color` | `5078015` | 661 | decay -40%, rank gain +20% |
| `perks.flow.text` | `"STYLE HOLDS"` | 661 | decay -40%, rank gain +20% |
| `perks.static` | {…} | 665 | see aura.static (P5) |
| `perks.static.price` | `3500` | 665 | see aura.static (P5) |
| `perks.static.color` | `9103615` | 665 | see aura.static (P5) |
| `perks.static.text` | `"THE AURA BITES"` | 665 | see aura.static (P5) |
| `perks.stackTax` | `0.1` | 669 | x base price added per perk already owned |
| `perks.machineHeight` | `2.6` | 670 | m: the cabinet |
| `perks.machineWidth` | `1.2` | 671 | m |
| `perks.machineDepth` | `0.9` | 672 | m |
| `perks.reach` | `2.4` | 673 | m: how close you stand to buy (hold E) |
| `perks.glow` | `2.2` | 674 | HDR brightness of the trim + ring |
| `perks.ringRadius` | `1.3` | 675 | m: the floor glow ring |
| `perks.screenRefresh` | `0.25` | 676 | s between CRT flicker redraws |
| `perks.pulseRate` | `2.2` | 677 | rad/s |
| `wallBuy` | {…} | 680 | (above) SURVIVAL §4 Wall buys (S3; visual only) |
| `wallBuy.plateWidth` | `1.8` | 682 | m, the outline plate on the wall (POWER P4: 14 buys, 2 m apart) |
| `wallBuy.plateHeight` | `1.1` | 683 | m |
| `wallBuy.holoSize` | `1.25` | 684 | m: each hologram is scaled so its longest side is this |
| `wallBuy.signWidth` | `1.9` | 685 | m |
| `wallBuy.holoSpin` | `0.9` | 686 | rad/s |
| `wallBuy.holoBob` | `0.05` | 687 | m |
| `wallBuy.holoOut` | `0.45` | 688 | m off the wall |
| `wallBuy.buyOut` | `1.2` | 689 | m off the wall: where the buy point is |
| `wallBuy.labelHeight` | `1.05` | 690 | m above the hologram |
| `aim` | {…} | 693 | (above) POWER P2 Aim assist + lock-on + enemy X-ray (the player only; all S unless POWER gives the number) |
| `aim.homingTurn` | `120` | 695 | deg/s projectiles turn toward the locked target; x for a V-key lock |
| `aim.lockHomingMult` | `2` | 695 | deg/s projectiles turn toward the locked target; x for a V-key lock |
| `aim.homingAssistAngle` | `0.0087` | 696 | rad (0.5 deg) a projectile must be turned before its hit counts as assisted |
| `aim.lunge` | `1.5` | 697 | m a melee hit reaches past its range; s the shove takes |
| `aim.lungeTime` | `0.12` | 697 | m a melee hit reaches past its range; s the shove takes |
| `aim.payout` | `0.8` | 698 | x points and style on an assisted hit (the old boolean path; the lock pays lock.payoutMin by strength) |
| `aim.lockCone` | `15` | 699 | deg, m: the lock-on key (V) takes the enemy nearest the crosshair within these |
| `aim.lockRange` | `45` | 699 | deg, m: the lock-on key (V) takes the enemy nearest the crosshair within these |
| `aim.lock` | {…} | 700 | (above) POLISH V1: lock before you shoot. Every mode scans continuously (fire or not); a candidate inside the acquire cone |
| `aim.lock` | {…} | 701 | (above) for the acquire time is PRE-LOCKED: brackets at once, the crosshair changes state and a critically damped spring |
| `aim.lock` | {…} | 702 | (above) pulls the camera onto its aim point (the mouse always adds on top). The table's numbers are at SNAP STRENGTH 60; |
| `aim.lock` | {…} | 703 | (above) k = strength / strengthRef scales the cone and pull (capped at kMax), the acquire time by 1 / k. |
| `aim.lock.modes.assist` | {…} | 707 | (above) cone (deg half-angle), acquire (ms), pull (deg/s spring cap), omega (1/s spring), follow (deg/s cap on following |
| `aim.lock.modes.assist` | {…} | 708 | (above) the target's own motion), friction (x sensitivity inside the bracket), breakaway (deg/s flick that releases), |
| `aim.lock.modes.assist` | {…} | 709 | (above) track (keep the spring on: HARD LOCK) or settle into a stick, pulse (deg: brackets pulse under this error) |
| `aim.lock.scanEvery` | `2` | 714 | fixed steps between target scans |
| `aim.lock.losRays` | `3` | 715 | line-of-sight rays per scan at most (the lock's own check included) |
| `aim.lock.losSlack` | `0.3` | 716 | m short of the point that still counts as seen |
| `aim.lock.range` | `90` | 717 | m: nothing farther is pre-locked |
| `aim.lock.meleeReach` | `2.5` | 718 | m past a melee weapon's reach that it may pre-lock (never across the room) |
| `aim.lock.releaseCone` | `1.6` | 719 | x the acquire cone: a target farther off the crosshair than this is released |
| `aim.lock.losLost` | `0.25` | 720 | s without sight before a lock releases (automatic / the V key's) |
| `aim.lock.manualLosLost` | `1` | 720 | s without sight before a lock releases (automatic / the V key's) |
| `aim.lock.hysteresis` | `{"margin":0.3,"hold":0.12}` | 721 | a challenger must score 30% better for 120 ms to take the lock |
| `aim.lock.hysteresis.margin` | `0.3` | 721 | a challenger must score 30% better for 120 ms to take the lock |
| `aim.lock.hysteresis.hold` | `0.12` | 721 | a challenger must score 30% better for 120 ms to take the lock |
| `aim.lock.settleError` | `1.5` | 722 | deg: ASSIST and SNAP settle into the stick (follow + gentle centering) under this error |
| `aim.lock.center` | `6` | 723 | 1/s: the stick's centering pull (your mouse beats it easily) |
| `aim.lock.fire` | `{"onTarget":1,"holdBelow":6,"maxHold":0.04,"boost":2}` | 724 | a press fires at once if the ray is on the target (or |
| `aim.lock.fire.onTarget` | `1` | 724 | a press fires at once if the ray is on the target (or |
| `aim.lock.fire.holdBelow` | `6` | 724 | a press fires at once if the ray is on the target (or |
| `aim.lock.fire.maxHold` | `0.04` | 724 | a press fires at once if the ray is on the target (or |
| `aim.lock.fire.boost` | `2` | 724 | a press fires at once if the ray is on the target (or |
| `aim.lock.reacquireDelay` | `0.4` | 725 | (above) within 1 deg); still 1..6 deg off and closing: held at most 40 ms while the spring runs x boost; never longer |
| `aim.lock.reacquireDelay` | `0.4` | 726 | s a target you flicked away from can't be pre-locked again |
| `aim.lock.acquireMaxTurn` | `200` | 727 | deg/s: no new pre-lock while your own mouse turns faster than this (a sweep stays yours) |
| `aim.lock.assistedPull` | `0.5` | 728 | deg the pull turned the camera within this many s: the hit is assisted |
| `aim.lock.assistedWindow` | `0.5` | 728 | deg the pull turned the camera within this many s: the hit is assisted |
| `aim.lock.targetRadius` | `0.55` | 729 | m: a body's half-width (friction applies while the crosshair is on it) |
| `aim.lock.elite` | `["spitter","charger","lancer","warden","rival"]` | 730 | bigger brackets (bosses too) |
| `aim.lock.weights` | {…} | 732 | lockScore |
| `aim.lock.weights.angle` | `1` | 732 | lockScore |
| `aim.lock.weights.distance` | `0.35` | 732 | lockScore |
| `aim.lock.weights.threat` | `0.3` | 732 | lockScore |
| `aim.lock.weights.head` | `0.1` | 732 | lockScore |
| `aim.lock.weights.weak` | `0.5` | 732 | lockScore |
| `aim.lock.payoutMin` | `0.65` | 734 | x points + style on an assisted hit at strength 100 (1 at 0) |
| `aim.lock.payoutFloor` | `0.65` | 734 | x points + style on an assisted hit at strength 100 (1 at 0) |
| `aim.lock.pinchDeg` | `0.8` | 735 | the tiny FOV pinch when a lock takes hold (none with reduced motion) |
| `aim.lock.pinchTime` | `0.08` | 735 | the tiny FOV pinch when a lock takes hold (none with reduced motion) |
| `aim.lock.bracketTime` | `0.06` | 736 | s the brackets take to tighten, from this x size |
| `aim.lock.bracketFrom` | `1.6` | 736 | s the brackets take to tighten, from this x size |
| `aim.lock.flashTime` | `0.1` | 737 | s the crosshair flashes when a lock takes hold |
| `aim.lock.hintTime` | `1.2` | 738 | s the mode + strength show after a change |
| `aim.lock.ads` | `{"cone":0.7,"pull":1.4}` | 739 | V2: zoomed, the acquire cone tightens 30% and the pull strengthens 40% |
| `aim.lock.ads.cone` | `0.7` | 739 | V2: zoomed, the acquire cone tightens 30% and the pull strengthens 40% |
| `aim.lock.ads.pull` | `1.4` | 739 | V2: zoomed, the acquire cone tightens 30% and the pull strengthens 40% |
| `aim.xray.color` | `16730652` | 742 | the through-wall silhouette (red-orange) and its brighter rim |
| `aim.xray.alpha` | `0.35` | 742 | the through-wall silhouette (red-orange) and its brighter rim |
| `aim.xray.edge` | `1.4` | 742 | the through-wall silhouette (red-orange) and its brighter rim |
| `aim.xray.fadeStart` | `45` | 743 | m: it fades out by here |
| `aim.xray.fadeEnd` | `60` | 743 | m: it fades out by here |
| `aim.xray.eliteBoost` | `1.9` | 744 | x for the Lancer, Spitter, Warden and the boss |
| `aim.xray.machineAlpha` | `0.16` | 745 | perk machines: a faint always-on glow in their own colour |
| `momentum` | {…} | 749 | (above) OVERNIGHT2 N2 momentum damage: how fast you close on what you hit, and how hard you just accelerated. |
| `momentum.smoothTime` | `0.06` | 751 | s: the velocity is smoothed over this before it is projected on the direction to the target |
| `momentum.closingMin` | `8` | 752 | m/s of closing speed for M 0 .. 1 |
| `momentum.closingMax` | `32` | 752 | m/s of closing speed for M 0 .. 1 |
| `momentum.surgeWindow` | `0.35` | 753 | s; m/s gained in that window (a dash, a slide start, a boost jump, |
| `momentum.surgeMin` | `10` | 753 | s; m/s gained in that window (a dash, a slide start, a boost jump, |
| `momentum.surgeBonus` | `0.3` | 753 | s; m/s gained in that window (a dash, a slide start, a boost jump, |
| `momentum.meleeMax` | `3` | 754 | (above) a slam rebound, a grapple release) adds this to M (capped at 1) |
| `momentum.meleeMax` | `3` | 755 | x damage at M = 1 (melee, guns) |
| `momentum.gunMax` | `1.35` | 755 | x damage at M = 1 (melee, guns) |
| `momentum.blastRadius` | `0.25` | 756 | explosions: +25% radius at M = 1 (instead of damage) |
| `momentum.arcAt` | `0.5` | 757 | the crosshair arc turns the rank colour above this |
| `momentum.impactBonusAt` | `0.5` | 758 | a melee hit above this is an IMPACT (style bonus, config.style.bonus.impact) |
| `momentum.variantAt` | `0.6` | 759 | weapon momentum variants (N2 §3.3) above this at the trigger pull |
| `momentum.shock` | {…} | 760 | (above) N2 §3.2 impact shockwaves: a melee hit above `at` sets one off (per weapon), a share of the hit to the bodies |
| `momentum.shock` | {…} | 761 | (above) around it and a knockback (lerp over M from `at` to 1), a stagger; one per `cooldown` s. |
| `momentum.shock.ring` | `{"radiusMin":2.5,"radiusMax":5,"time":0.35}` | 764 | Pulse Gauntlet + the F punch: a flat ring round the target |
| `momentum.shock.ring.radiusMin` | `2.5` | 764 | Pulse Gauntlet + the F punch: a flat ring round the target |
| `momentum.shock.ring.radiusMax` | `5` | 764 | Pulse Gauntlet + the F punch: a flat ring round the target |
| `momentum.shock.ring.time` | `0.35` | 764 | Pulse Gauntlet + the F punch: a flat ring round the target |
| `momentum.shock.crescent` | {…} | 765 | Sunder: a gold wave flying forward |
| `momentum.shock.crescent.range` | `10` | 765 | Sunder: a gold wave flying forward |
| `momentum.shock.crescent.speed` | `40` | 765 | Sunder: a gold wave flying forward |
| `momentum.shock.crescent.width` | `2.2` | 765 | Sunder: a gold wave flying forward |
| `momentum.shock.crescent.height` | `1.6` | 765 | Sunder: a gold wave flying forward |
| `momentum.shock.crescent.color` | `16761405` | 765 | Sunder: a gold wave flying forward |
| `momentum.shock.echo` | `{"targets":3,"radius":5,"color":12123948}` | 766 | Flicker: phantom stabs on the target + the 2 nearest within 5 m |
| `momentum.shock.echo.targets` | `3` | 766 | Flicker: phantom stabs on the target + the 2 nearest within 5 m |
| `momentum.shock.echo.radius` | `5` | 766 | Flicker: phantom stabs on the target + the 2 nearest within 5 m |
| `momentum.shock.echo.color` | `12123948` | 766 | Flicker: phantom stabs on the target + the 2 nearest within 5 m |
| `momentum.shock.quake` | `{"coneDeg":70,"length":8,"launch":10,"fissureTime":1.4}` | 767 | Thumper: a ground cone launching bodies upward |
| `momentum.shock.quake.coneDeg` | `70` | 767 | Thumper: a ground cone launching bodies upward |
| `momentum.shock.quake.length` | `8` | 767 | Thumper: a ground cone launching bodies upward |
| `momentum.shock.quake.launch` | `10` | 767 | Thumper: a ground cone launching bodies upward |
| `momentum.shock.quake.fissureTime` | `1.4` | 767 | Thumper: a ground cone launching bodies upward |
| `momentum.shock.crack` | `{"radius":5,"pull":9,"pullKinds":["husk","swarmer","drone"]}` | 768 | Tether: drags small ones in, staggers big ones |
| `momentum.shock.crack.radius` | `5` | 768 | Tether: drags small ones in, staggers big ones |
| `momentum.shock.crack.pull` | `9` | 768 | Tether: drags small ones in, staggers big ones |
| `momentum.shock.crack.pullKinds` | `["husk","swarmer","drone"]` | 768 | Tether: drags small ones in, staggers big ones |
| `arcline` | {…} | 772 | (above) OVERNIGHT2 N4 ARCLINE: the late-game swing gadget (a state of the shared Mover; pure math in movement/swingMath.js). |
| `arcline.price` | `5500` | 774 | at its terminal in ISLANDS |
| `arcline.range` | `60` | 775 | m, deg: anchors in a cone toward the crosshair (+ a 1.5 m sphere-cast) |
| `arcline.coneDeg` | `15` | 775 | m, deg: anchors in a cone toward the crosshair (+ a 1.5 m sphere-cast) |
| `arcline.probeRadius` | `1.5` | 775 | m, deg: anchors in a cone toward the crosshair (+ a 1.5 m sphere-cast) |
| `arcline.maxLen` | `55` | 776 | m of rope |
| `arcline.minLen` | `4` | 776 | m of rope |
| `arcline.hookSpeed` | `600` | 777 | m/s the hook flies; s it takes at least / at most |
| `arcline.hookMin` | `0.06` | 777 | m/s the hook flies; s it takes at least / at most |
| `arcline.hookMax` | `0.1` | 777 | m/s the hook flies; s it takes at least / at most |
| `arcline.pullTime` | `0.18` | 778 | s + m/s^2: the yank toward the anchor when it bites |
| `arcline.pullAccel` | `22` | 778 | s + m/s^2: the yank toward the anchor when it bites |
| `arcline.stretch` | `0.03` | 779 | up to 3% past the rope length a critically damped spring pulls back (rad/s) |
| `arcline.springOmega` | `30` | 779 | up to 3% past the rope length a critically damped spring pulls back (rad/s) |
| `arcline.gravityMult` | `0.85` | 780 | x gravity while attached (longer arcs); quadratic drag (1/m), small |
| `arcline.drag` | `0.00008` | 780 | x gravity while attached (longer arcs); quadratic drag (1/m), small |
| `arcline.pumpAccel` | `12` | 781 | m/s^2 of pump from the move keys (strongest at the bottom), fading to this speed |
| `arcline.softCap` | `42` | 781 | m/s^2 of pump from the move keys (strongest at the bottom), fading to this speed |
| `arcline.reelSpeed` | `18` | 782 | m/s the reel key shortens the rope (down to minLen) |
| `arcline.risingBoost` | `1.08` | 783 | x speed when you let go rising; m/s up on a jump release |
| `arcline.jumpUp` | `4` | 783 | x speed when you let go rising; m/s up on a jump release |
| `arcline.cooldown` | `0.4` | 784 | s before the next hook; s before the same node takes one again |
| `arcline.rehook` | `0.25` | 784 | s before the next hook; s before the same node takes one again |
| `arcline.losBreak` | `0.25` | 785 | s the rope may be blocked before it breaks; m short of the anchor that still counts |
| `arcline.losSlack` | `0.4` | 785 | s the rope may be blocked before it breaks; m short of the anchor that still counts |
| `arcline.substeps` | `4` | 786 | sub-steps a fixed step while attached (no tunnelling); m, the rope's point on you |
| `arcline.ropeHeight` | `1.2` | 786 | sub-steps a fixed step while attached (no tunnelling); m, the rope's point on you |
| `arcline.fovLiftDeg` | `4` | 787 | extra FOV at full swing speed (visual) |
| `arcline.zip` | `{"reelSpeed":34,"arrive":2.6,"maxTime":1.2}` | 788 | a big enemy hooked: you zip to it (then a melee window) |
| `arcline.zip.reelSpeed` | `34` | 788 | a big enemy hooked: you zip to it (then a melee window) |
| `arcline.zip.arrive` | `2.6` | 788 | a big enemy hooked: you zip to it (then a melee window) |
| `arcline.zip.maxTime` | `1.2` | 788 | a big enemy hooked: you zip to it (then a melee window) |
| `arcline.groundLift` | `6` | 789 | m/s up when the hook bites while you stand (the swing gets under way) |
| `arcline.pullKinds` | `["husk","swarmer","drone"]` | 790 | small ones are yanked to you |
| `arcline.enemyPull` | `22` | 790 | small ones are yanked to you |
| `arcline.enemyPullTime` | `0.35` | 790 | small ones are yanked to you |
| `arcline.nodeGlow` | `2.4` | 791 | the GRAPPLE NODES (hex anchors on floating pylons) |
| `arcline.nodeRadius` | `1.1` | 791 | the GRAPPLE NODES (hex anchors on floating pylons) |
| `arcline.nodeColor` | `8384767` | 791 | the GRAPPLE NODES (hex anchors on floating pylons) |
| `arcline.ropeColor` | `1692646` | 792 | visuals (the rope takes the rank colour) |
| `arcline.ropeWidth` | `0.035` | 792 | visuals (the rope takes the rank colour) |
| `arcline.ropeSegments` | `14` | 792 | visuals (the rope takes the rank colour) |
| `arcline.sag` | `0.35` | 792 | visuals (the rope takes the rank colour) |
| `punch` | {…} | 795 | (above) §4 Punch + parry (SPEC 1.5, a Feedbacker-like fist on F) |
| `punch.cycleTime` | `0.35` | 797 | s per punch (one hit per cycle) |
| `punch.range` | `2` | 798 | m from the eye to the target's body |
| `punch.coneHalfAngleDeg` | `35` | 799 | aim cone (S) |
| `punch.damage` | `10` | 800 | HP |
| `punch.knockbackSpeed` | `6` | 801 | m/s, horizontal, away from the puncher |
| `punch.knockbackTime` | `0.2` | 802 | s the target slides without chasing (S) |
| `punch.staggerTime` | `1` | 803 | s a parried Husk can't chase, attack or leap |
| `punch.parryHeal` | `15` | 804 | HP |
| `punch.parryHitstop` | `0.06` | 805 | s |
| `punch.parryFlashTime` | `0.08` | 806 | s white screen flash |
| `punch.parryFlashOpacity` | `0.45` | 807 | peak (S) |
| `ads` | {…} | 810 | (above) POLISH V2 ADS: hold right mouse on a precision gun to zoom (settings.adsMode: hold or toggle). Those guns' old |
| `ads` | {…} | 811 | (above) right-mouse specials moved to the SPECIAL key (input.bindings.special); every other gun keeps its right-mouse special. |
| `ads.weapons.revolver` | `{"zoom":1.5,"reticle":"ring"}` | 814 | a zoomed shot off a surface near an enemy ricochets onto it (RICOSHOT) |
| `ads.weapons.revolver.zoom` | `1.5` | 814 | a zoomed shot off a surface near an enemy ricochets onto it (RICOSHOT) |
| `ads.weapons.revolver.reticle` | `"ring"` | 814 | a zoomed shot off a surface near an enemy ricochets onto it (RICOSHOT) |
| `ads.weapons.rattler` | `{"zoom":1.4,"reticle":"ring","spreadMult":0.3}` | 815 | the burst's spread tightens |
| `ads.weapons.rattler.zoom` | `1.4` | 815 | the burst's spread tightens |
| `ads.weapons.rattler.reticle` | `"ring"` | 815 | the burst's spread tightens |
| `ads.weapons.rattler.spreadMult` | `0.3` | 815 | the burst's spread tightens |
| `ads.weapons.zapper` | `{"zoom":1.3,"reticle":"ring","chainRange":1.15}` | 816 | chains reach 15% farther |
| `ads.weapons.zapper.zoom` | `1.3` | 816 | chains reach 15% farther |
| `ads.weapons.zapper.reticle` | `"ring"` | 816 | chains reach 15% farther |
| `ads.weapons.zapper.chainRange` | `1.15` | 816 | chains reach 15% farther |
| `ads.weapons.prism` | `{"zoom":1.4,"reticle":"ring","preview":true}` | 817 | the beam's path previews (its bounces) |
| `ads.weapons.prism.zoom` | `1.4` | 817 | the beam's path previews (its bounces) |
| `ads.weapons.prism.reticle` | `"ring"` | 817 | the beam's path previews (its bounces) |
| `ads.weapons.prism.preview` | `true` | 817 | the beam's path previews (its bounces) |
| `ads.weapons.rail` | `{"zoom":3.5,"reticle":"scope","scope":true}` | 818 | a scope vignette, a rangefinder, and held fire overcharges |
| `ads.weapons.rail.zoom` | `3.5` | 818 | a scope vignette, a rangefinder, and held fire overcharges |
| `ads.weapons.rail.reticle` | `"scope"` | 818 | a scope vignette, a rangefinder, and held fire overcharges |
| `ads.weapons.rail.scope` | `true` | 818 | a scope vignette, a rangefinder, and held fire overcharges |
| `ads.easeTime` | `0.12` | 820 | s the FOV (and the viewmodel pose) take to ease in or out (a weapon without a weight class) |
| `ads.classEase` | `{"light":0.13,"medium":0.18,"heavy":0.25}` | 821 | (above) NIGHT5 F2: the ease by weight class (feel.punch.weapons[].cls): light guns come up faster, heavy ones slower. Frame |
| `ads.classEase` | `{"light":0.13,"medium":0.18,"heavy":0.25}` | 822 | (above) time only: the zoom itself (spread, headshots, the walk) is on the moment you press, so input is never held back. |
| `ads.walkMult` | `0.85` | 824 | x walk speed while zoomed (dash, slide, air and the grapple are unaffected) |
| `ads.headshotMult` | `1.2` | 825 | x headshot damage while zoomed |
| `ads.hipSpreadDeg` | `{"revolver":0.8}` | 826 | hip fire only, zoomed: none (the Rattler's hip spread is its own burst spread, 0.8) |
| `ads.hipSpreadDeg.revolver` | `0.8` | 826 | hip fire only, zoomed: none (the Rattler's hip spread is its own burst spread, 0.8) |
| `ads.auraMult` | `0.5` | 827 | x the style aura on the weapon while zoomed (so it doesn't cover the view) |
| `ads.vmSwayMult` | `0.25` | 828 | x the viewmodel's sway and bob while zoomed |
| `ads.suspendStates` | `["DASH","SWING"]` | 829 | mover states that suspend the zoom (their FOV kicks stay readable); it resumes after |
| `ads.ricochet` | `{"range":12,"damageMult":1,"lineTime":0.12}` | 830 | m: a zoomed Kestrel shot that hits a surface this close to an |
| `ads.ricochet.range` | `12` | 830 | m: a zoomed Kestrel shot that hits a surface this close to an |
| `ads.ricochet.damageMult` | `1` | 830 | m: a zoomed Kestrel shot that hits a surface this close to an |
| `ads.ricochet.lineTime` | `0.12` | 830 | m: a zoomed Kestrel shot that hits a surface this close to an |
| `ads.preview` | `{"bounces":3,"opacity":0.35}` | 831 | (above) enemy bounces onto it once (line of sight from the impact); the bounce line shows this long |
| `ads.preview` | `{"bounces":3,"opacity":0.35}` | 832 | the Prism's zoomed beam preview |
| `ads.preview.bounces` | `3` | 832 | the Prism's zoomed beam preview |
| `ads.preview.opacity` | `0.35` | 832 | the Prism's zoomed beam preview |
| `ads.scopeVignette` | `0.92` | 833 | the Longwave scope's edge darkness |
| `husk` | {…} | 837 | (above) §5 Husk (SPEC 1.4 details, all S) |
| `husk.speed` | `6` | 840 | m/s chase on the ground |
| `husk.stopDistance` | `1` | 841 | m, Husks stop this far from the player (they never block) |
| `husk.meleeRange` | `2.2` | 842 | m, a wind-up starts within this |
| `husk.meleeHitRange` | `2.5` | 843 | m, the hit lands only if the player is still this close |
| `husk.windupTime` | `0.5` | 844 | s tell before the hit |
| `husk.meleeDamage` | `15` | 845 | HP |
| `husk.meleeCooldown` | `1.2` | 846 | s after an attack |
| `husk.leapMinHeight` | `1.5` | 847 | m the player must be above the Husk (or more) |
| `husk.leapMaxHorizontal` | `10` | 848 | m |
| `husk.leapApexAbove` | `1` | 849 | m the arc peaks above the target height |
| `husk.leapCooldown` | `3` | 850 | s |
| `husk.radius` | `0.4` | 851 | m capsule |
| `husk.height` | `1.8` | 852 | m capsule |
| `husk.headRadius` | `0.28` | 853 | m head hit sphere |
| `husk.headHeight` | `1.5` | 854 | m head center above the feet (hunched) |
| `husk.headForward` | `0.2` | 855 | m head center in front of the body axis (hunched) |
| `husk.headTiltDeg` | `20` | 856 | visual forward tilt of the block head |
| `husk.headBox` | `[0.62,0.45,0.56]` | 857 | m, visual block head (width, height, depth) |
| `husk.bodyCapSegments` | `4` | 858 | capsule mesh detail |
| `husk.aimHeight` | `1` | 860 | m above the feet where ricochets aim (chest) |
| `husk.spawnLift` | `0.05` | 861 | m above the requested point, so a floor spawn doesn't start in contact |
| `husk.separationRadius` | `1.2` | 862 | m, Husks push apart inside this |
| `husk.separationSpeed` | `3` | 863 | m/s push at full overlap |
| `husk.windupPulseScale` | `1.15` | 864 | × size at the peak of the tell pulse |
| `husk.windupPulseRate` | `24` | 865 | rad/s of the tell pulse |
| `husk.windupColor` | `16761370` | 866 | hot yellow-orange the tell pulses toward (stays in the danger family) (S) |
| `husk.headShade` | `0.72` | 867 | × enemy color on the head, so the hunched block head reads against the body (S) |
| `husk.hitFlashTime` | `0.08` | 868 | s white flash when hit |
| `husk.staggerLeanDeg` | `22` | 869 | a parried (staggered) Husk leans back this much (S) |
| `husk.staggerPale` | `0.45` | 870 | and blends this far toward white (S) |
| `husk.staggerLeanEaseRate` | `14` | 871 | 1/s, the lean eases in and out (visual) |
| `husk.maxCount` | `32` | 872 | instanced capacity |
| `husk.spawnCount` | `3` | 873 | per H press / auto-spawn tick |
| `husk.spawnEdgeInset` | `3` | 874 | m inside the arena edge |
| `husk.spawnTries` | `16` | 875 | random edge points tried per Husk |
| `husk.autoSpawn` | `false` | 876 | sandbox: spawn every autoSpawnInterval (GUI toggle) |
| `husk.autoSpawnInterval` | `10` | 877 | s |
| `swarmer` | {…} | 880 | (above) SURVIVAL §7 New enemy types (S6; HP + the behaviour numbers from SURVIVAL, the rest S) |
| `swarmer.pounceRange` | `4.5` | 883 | m: it pounces from this close ... |
| `swarmer.tellTime` | `0.3` | 884 | s crouch tell |
| `swarmer.pounceApex` | `0.9` | 885 | m above the landing point |
| `swarmer.biteRange` | `1.2` | 886 | m, the pounce bites you when this close |
| `swarmer.recoverTime` | `0.6` | 888 | s after a pounce |
| `swarmer.cooldown` | `1.6` | 889 | s between pounces |
| `swarmer.frontConeDeg` | `70` | 890 | it only waits (and pounces) outside this half-angle of your facing: sides and back |
| `spitter.minRange` | `6` | 894 | m |
| `spitter.maxRange` | `24` | 894 | m |
| `spitter.retreatRange` | `5` | 894 | m |
| `spitter.globTimeMin` | `0.5` | 895 | s a lobbed glob is in the air (by distance) |
| `spitter.globTimeMax` | `1.3` | 895 | s a lobbed glob is in the air (by distance) |
| `spitter.tellTime` | `0.6` | 896 | s: the sac swells + glows |
| `spitter.cooldown` | `2.6` | 897 | s between volleys |
| `spitter.globSpeed` | `16` | 898 | m/s horizontal (sets the flight time) |
| `spitter.spread` | `1.6` | 899 | m between the three globs |
| `spitter.puddleRadius` | `3` | 901 | m, s, HP/s, s |
| `spitter.puddleTime` | `4` | 901 | m, s, HP/s, s |
| `spitter.puddleDps` | `10` | 901 | m, s, HP/s, s |
| `spitter.puddleTick` | `0.25` | 901 | m, s, HP/s, s |
| `spitter.acidColor` | `16756768` | 902 | danger family (yellow-orange) |
| `charger.minRange` | `6` | 907 | m |
| `charger.maxRange` | `26` | 907 | m |
| `charger.tellTime` | `0.8` | 908 | s: stomps, glows, the charge line on the ground |
| `charger.chargeSpeed` | `22` | 909 | m/s, m |
| `charger.chargeDistance` | `30` | 909 | m/s, m |
| `charger.damage` | `25` | 910 | m/s push on a hit |
| `charger.hitRange` | `1.5` | 910 | m/s push on a hit |
| `charger.knockback` | `14` | 910 | m/s push on a hit |
| `charger.knockbackUp` | `6` | 910 | m/s push on a hit |
| `charger.stunTime` | `1.5` | 911 | s stunned after hitting a wall |
| `charger.weakMult` | `3` | 912 | x damage to its back while stunned |
| `lancer.hp` | `70` | 916 | its eye (line of sight over the perch edge) |
| `lancer.radius` | `0.4` | 916 | its eye (line of sight over the perch edge) |
| `lancer.height` | `2` | 916 | its eye (line of sight over the perch edge) |
| `lancer.shade` | `0.9` | 916 | its eye (line of sight over the perch edge) |
| `lancer.maxCount` | `6` | 916 | its eye (line of sight over the perch edge) |
| `lancer.aimHeight` | `1.8` | 916 | its eye (line of sight over the perch edge) |
| `lancer.perchTries` | `8` | 917 | random perches tested for line of sight to you before a blink |
| `lancer.aimTime` | `1.2` | 918 | s the laser tracks you before the shot |
| `lancer.trackLag` | `0.45` | 919 | s, the laser's follow time constant (twice as fast on a camper) |
| `lancer.damage` | `25` | 920 | m |
| `lancer.hitRadius` | `0.55` | 920 | m |
| `lancer.range` | `70` | 920 | m |
| `lancer.cooldown` | `2.5` | 921 | s, s, m |
| `lancer.blinkTime` | `0.35` | 921 | s, s, m |
| `lancer.relocateMin` | `15` | 921 | s, s, m |
| `lancer.blindBlink` | `3` | 922 | s without line of sight to you (or out of range) on a perch -> it blinks to one that can see you (POWER P1) |
| `warden.shieldArcDeg` | `120` | 926 | the front arc the shield covers |
| `warden.aboveHeight` | `1.5` | 927 | m over its head counts as "from above" |
| `warden.shieldStun` | `1` | 928 | s stunned with the shield open after a punch on it |
| `warden.shoveRadius` | `1.8` | 930 | it pushes other enemies aside |
| `warden.shoveSpeed` | `3` | 930 | it pushes other enemies aside |
| `bossKit` | {…} | 933 | (above) OVERNIGHT2 §8 the telegraph primitives (fx/telegraph.js): pools, the hit-moment hold, the look. |
| `bossKit` | {…} | 934 | (above) OVERNIGHT2 N5 boss framework (enemies/bossKit.js): the schedule, scaling, the intro card, the phase beat, rewards. |
| `bossKit.rotationFrom` | `30` | 937 | round 30 on: the five again in order, a mutator each cycle |
| `bossKit.mutators` | array[4] | 940 | (above) Each rotation cycle adds a mutator (in order, then around again): x HP, x damage, x speed, x adds. |
| `bossKit.hpPerRound` | `0.03` | 947 | x boss HP per round past its first appearance |
| `bossKit.hpPerCycle` | `0.35` | 948 | x boss HP each rotation cycle |
| `bossKit.damagePerCycle` | `0.15` | 949 | x boss damage each rotation cycle |
| `bossKit.coop` | `1` | 950 | the coop factor (single player only) |
| `bossKit.introTime` | `1.5` | 951 | s the intro card shows (the boss walks in: no damage) |
| `bossKit.beatTime` | `1.2` | 952 | s the phase change beat holds (invulnerable, a white flash, the banner) |
| `bossKit.beatPush` | `12` | 953 | m/s: the beat's knockback pulse (no damage) |
| `bossKit.beatPushUp` | `5` | 953 | m/s: the beat's knockback pulse (no damage) |
| `bossKit.killTime` | `0.6` | 954 | s of slow-mo at x time scale when a boss dies |
| `bossKit.killSlowMo` | `0.3` | 954 | s of slow-mo at x time scale when a boss dies |
| `bossKit.pixel` | `7` | 955 | px per font pixel on the intro card |
| `bossKit.arena` | `{"x":0,"y":0,"z":0}` | 963 | the Grinder centre: every boss fights there |
| `bossKit.arena.x` | `0` | 963 | the Grinder centre: every boss fights there |
| `bossKit.arena.y` | `0` | 963 | the Grinder centre: every boss fights there |
| `bossKit.arena.z` | `0` | 963 | the Grinder centre: every boss fights there |
| `bossKit.arenas` | {…} | 964 | (above) The Grinder's shape for each fight: BULWARK's walled pen (its rams hit the walls), RIVAL PRIME's four pillars. |
| `bulwark` | {…} | 968 | (above) BULWARK (round 5): a tall blocky armored walker with 5 breakable plates and a core in its back vent. |
| `bulwark.speed` | `3.2` | 971 | m/s walking at you; rad/s it turns |
| `bulwark.turnRate` | `1.6` | 971 | m/s walking at you; rad/s it turns |
| `bulwark.height` | `7.6` | 972 | m: the body cylinder that blocks you |
| `bulwark.radius` | `2.1` | 972 | m: the body cylinder that blocks you |
| `bulwark.parts` | array[6] | 973 | (above) Parts in its local frame (forward = -z): [name, w, h, d, x, y, z]. |
| `bulwark.xrayEnvelope` | array[6] | 984 | (above) The X-ray silhouette: boxes around the legs, the plated torso, the head and the plated arms. |
| `bulwark.core` | `{"x":0,"y":4.6,"z":0.95,"r":0.55}` | 986 | the back vent (behind the back plate) |
| `bulwark.core.x` | `0` | 986 | the back vent (behind the back plate) |
| `bulwark.core.y` | `4.6` | 986 | the back vent (behind the back plate) |
| `bulwark.core.z` | `0.95` | 986 | the back vent (behind the back plate) |
| `bulwark.core.r` | `0.55` | 986 | the back vent (behind the back plate) |
| `bulwark.hullMult` | `0.35` | 988 | x damage: armored hull, a broken plate's socket, the vent core |
| `bulwark.exposedMult` | `1` | 988 | x damage: armored hull, a broken plate's socket, the vent core |
| `bulwark.coreMult` | `2` | 988 | x damage: armored hull, a broken plate's socket, the vent core |
| `bulwark.coreOpenMult` | `3` | 988 | x damage: armored hull, a broken plate's socket, the vent core |
| `bulwark.platePoints` | `150` | 989 | points a broken plate drops |
| `bulwark.actionGap` | `1.1` | 990 | s between attacks |
| `bulwark.p2At` | `0.5` | 994 | phase 2 below 50%: 6 Husks from floor hatches, a second ram in a row |
| `bulwark.hatches` | `[[-12,-12],[12,-12],[-12,12],[12,12],[0,-16],[0,16]]` | 995 | Grinder floor points (x, z) |
| `bulwark.hatchKind` | `"husk"` | 996 | s the hatch glows before it opens |
| `bulwark.hatchTell` | `1.2` | 996 | s the hatch glows before it opens |
| `bulwark.passive` | `{"time":4,"radius":5}` | 998 | s you stand within this many m: it rams at once (passivity is punished) |
| `bulwark.passive.time` | `4` | 998 | s you stand within this many m: it rams at once (passivity is punished) |
| `bulwark.passive.radius` | `5` | 998 | s you stand within this many m: it rams at once (passivity is punished) |
| `bulwark.arenaHalf` | `18` | 999 | m its centre stays inside (the pen's inner face is at 20) |
| `rivalPrime` | {…} | 1002 | (above) RIVAL PRIME (round 15): the Duel's RIVAL upgraded (same Mover, same freshness), a mirror duel in the Grinder. |
| `rivalPrime.loadout` | `["revolver","shotgun","rocket"]` | 1005 | KESTREL, THUNDERCLAP, COMET |
| `rivalPrime.introFrom` | `14` | 1007 | m north of the centre it walks in from |
| `rivalPrime.copyMult` | `1.15` | 1009 | phase 2: your most-used weapon at x damage |
| `rivalPrime.copyFresh` | `0.55` | 1010 | it holds the copy while its FRESH is above this |
| `rivalPrime.closeRange` | `7` | 1011 | m: the Thunderclap inside this, the fresher of Kestrel / Comet to rocketMax; s between swaps |
| `rivalPrime.rocketMax` | `24` | 1011 | m: the Thunderclap inside this, the fresher of Kestrel / Comet to rocketMax; s between swaps |
| `rivalPrime.swapHold` | `0.8` | 1011 | m: the Thunderclap inside this, the fresher of Kestrel / Comet to rocketMax; s between swaps |
| `rivalPrime.rageRange` | `30` | 1012 | (above) Rage: too far or too still for rageStill s -> the flare builds for rageBuild s, then it hunts for rageTime s. |
| `rivalPrime.rageRange` | `30` | 1013 | m, s, m/s, s, s, s |
| `rivalPrime.rageStill` | `4` | 1013 | m, s, m/s, s, s, s |
| `rivalPrime.stillSpeed` | `1.5` | 1013 | m, s, m/s, s, s, s |
| `rivalPrime.rageBuild` | `1.2` | 1013 | m, s, m/s, s, s, s |
| `rivalPrime.rageTime` | `6` | 1013 | m, s, m/s, s, s, s |
| `rivalPrime.rageDashEvery` | `0.6` | 1013 | m, s, m/s, s, s, s |
| `rivalPrime.shapeTime` | `0.45` | 1014 | (above) Telegraphed actions: the shape shows for shapeTime over its head (shapeHeight m), a white flash as it commits. |
| `rivalPrime.dashRange` | `18` | 1016 | m / s / m / HP: the dash at you, its strike window |
| `rivalPrime.dashStrike` | `0.45` | 1016 | m / s / m / HP: the dash at you, its strike window |
| `rivalPrime.dashHitRange` | `2.6` | 1016 | m / s / m / HP: the dash at you, its strike window |
| `rivalPrime.dashDamage` | `18` | 1016 | m / s / m / HP: the dash at you, its strike window |
| `rivalPrime.slamRange` | `9` | 1017 | m, m, s, HP, m/s: the leap + slam on you |
| `rivalPrime.slamRadius` | `4` | 1017 | m, m, s, HP, m/s: the leap + slam on you |
| `rivalPrime.slamAir` | `0.55` | 1017 | m, m, s, HP, m/s: the leap + slam on you |
| `rivalPrime.slamDamage` | `25` | 1017 | m, m, s, HP, m/s: the leap + slam on you |
| `rivalPrime.slamLaunch` | `9` | 1017 | m, m, s, HP, m/s: the leap + slam on you |
| `rivalPrime.volleyRange` | `26` | 1018 | m, orbs, rad, m/s: the cyan (parryable) fan |
| `rivalPrime.volleyCount` | `5` | 1018 | m, orbs, rad, m/s: the cyan (parryable) fan |
| `rivalPrime.volleyFan` | `0.9` | 1018 | m, orbs, rad, m/s: the cyan (parryable) fan |
| `rivalPrime.orbSpeed` | `14` | 1018 | m, orbs, rad, m/s: the cyan (parryable) fan |
| `rivalPrime.retreatTime` | `0.8` | 1019 | s moving away after the retreat dash |
| `rivalPrime.weights` | {…} | 1020 | by the gap |
| `rivalPrime.weights.close` | `{"slam":0.4,"retreat":0.3,"volley":0.3}` | 1020 | by the gap |
| `rivalPrime.weights.close.slam` | `0.4` | 1020 | by the gap |
| `rivalPrime.weights.close.retreat` | `0.3` | 1020 | by the gap |
| `rivalPrime.weights.close.volley` | `0.3` | 1020 | by the gap |
| `rivalPrime.weights.mid` | `{"volley":0.5,"dash":0.35,"retreat":0.15}` | 1020 | by the gap |
| `rivalPrime.weights.mid.volley` | `0.5` | 1020 | by the gap |
| `rivalPrime.weights.mid.dash` | `0.35` | 1020 | by the gap |
| `rivalPrime.weights.mid.retreat` | `0.15` | 1020 | by the gap |
| `rivalPrime.weights.far` | `{"dash":0.55,"volley":0.45}` | 1020 | by the gap |
| `rivalPrime.weights.far.dash` | `0.55` | 1020 | by the gap |
| `rivalPrime.weights.far.volley` | `0.45` | 1020 | by the gap |
| `rivalPrime.pillars` | `[[-8,-8],[8,-8],[-8,8],[8,8]]` | 1021 | the four cover pillars (the 'pillars' preset's 2 x 2 cell blocks) |
| `rivalPrime.pillarHp` | `400` | 1022 | its Comet wears them down (70 a rocket) |
| `rivalPrime.coverShootAfter` | `1.6` | 1023 | s without line of sight: it shoots the pillar in the way |
| `rivalPrime.arcline` | `{"every":5,"range":30,"minRange":12,"lift":14,"hold":1.1}` | 1024 | phase 2: s, m (node search), m, m (no node: over the midpoint), s held |
| `rivalPrime.arcline.every` | `5` | 1024 | phase 2: s, m (node search), m, m (no node: over the midpoint), s held |
| `rivalPrime.arcline.range` | `30` | 1024 | phase 2: s, m (node search), m, m (no node: over the midpoint), s held |
| `rivalPrime.arcline.minRange` | `12` | 1024 | phase 2: s, m (node search), m, m (no node: over the midpoint), s held |
| `rivalPrime.arcline.lift` | `14` | 1024 | phase 2: s, m (node search), m, m (no node: over the midpoint), s held |
| `rivalPrime.arcline.hold` | `1.1` | 1024 | phase 2: s, m (node search), m, m (no node: over the midpoint), s held |
| `orbiter` | {…} | 1027 | (above) ORBITER (round 20): a ring station circling over the Grinder with six turret pods. |
| `orbiter.orbitRadius` | `16` | 1030 | m, m, rad/s; x from phase 2; rad/s of the ring |
| `orbiter.orbitHeight` | `[32,38]` | 1030 | m, m, rad/s; x from phase 2; rad/s of the ring |
| `orbiter.orbitSpeed` | `0.12` | 1030 | m, m, rad/s; x from phase 2; rad/s of the ring |
| `orbiter.p2OrbitMult` | `1.5` | 1030 | m, m, rad/s; x from phase 2; rad/s of the ring |
| `orbiter.spin` | `0.35` | 1030 | m, m, rad/s; x from phase 2; rad/s of the ring |
| `orbiter.ringRadius` | `7` | 1031 | m |
| `orbiter.ringTube` | `1.1` | 1031 | m |
| `orbiter.pods` | `6` | 1032 | HP, m, s a shot-out pod takes to come back, points |
| `orbiter.podHp` | `280` | 1032 | HP, m, s a shot-out pod takes to come back, points |
| `orbiter.podRadius` | `1.3` | 1032 | HP, m, s a shot-out pod takes to come back, points |
| `orbiter.podRespawn` | `14` | 1032 | HP, m, s a shot-out pod takes to come back, points |
| `orbiter.podPoints` | `100` | 1032 | HP, m, s a shot-out pod takes to come back, points |
| `orbiter.exposeTime` | `6` | 1033 | s a shot pod's side of the core is open |
| `orbiter.coreRadius` | `2.2` | 1034 | x damage: an open side of the core; the closed shell |
| `orbiter.coreMult` | `2.5` | 1034 | x damage: an open side of the core; the closed shell |
| `orbiter.shellMult` | `0.2` | 1034 | x damage: an open side of the core; the closed shell |
| `orbiter.p2At` | `0.66` | 1036 | phase 2 (+ the sweep, faster orbit), phase 3 (+ the crash dive) |
| `orbiter.p3At` | `0.33` | 1036 | phase 2 (+ the sweep, faster orbit), phase 3 (+ the crash dive) |
| `orbiter.curtain` | {…} | 1038 | m/s, m, m, HP, m (2 lanes are safe) |
| `orbiter.curtain.tell` | `1.2` | 1038 | m/s, m, m, HP, m (2 lanes are safe) |
| `orbiter.curtain.lanes` | `7` | 1038 | m/s, m, m, HP, m (2 lanes are safe) |
| `orbiter.curtain.speed` | `16` | 1038 | m/s, m, m, HP, m (2 lanes are safe) |
| `orbiter.curtain.length` | `44` | 1038 | m/s, m, m, HP, m (2 lanes are safe) |
| `orbiter.curtain.width` | `44` | 1038 | m/s, m, m, HP, m (2 lanes are safe) |
| `orbiter.curtain.damage` | `20` | 1038 | m/s, m, m, HP, m (2 lanes are safe) |
| `orbiter.curtain.height` | `3` | 1038 | m/s, m, m, HP, m (2 lanes are safe) |
| `orbiter.sweep` | {…} | 1040 | bands (m over the floor): jump the low, slide under the high |
| `orbiter.sweep.tell` | `1` | 1040 | bands (m over the floor): jump the low, slide under the high |
| `orbiter.sweep.speed` | `9` | 1040 | bands (m over the floor): jump the low, slide under the high |
| `orbiter.sweep.length` | `44` | 1040 | bands (m over the floor): jump the low, slide under the high |
| `orbiter.sweep.width` | `1.6` | 1040 | bands (m over the floor): jump the low, slide under the high |
| `orbiter.sweep.damage` | `30` | 1040 | bands (m over the floor): jump the low, slide under the high |
| `orbiter.sweep.low` | `[0,0.8]` | 1040 | bands (m over the floor): jump the low, slide under the high |
| `orbiter.sweep.high` | `[1.1,2.2]` | 1040 | bands (m over the floor): jump the low, slide under the high |
| `orbiter.dive` | {…} | 1041 | s, s, m, HP, s, s, m (the core's height when down) |
| `orbiter.dive.tell` | `1.6` | 1041 | s, s, m, HP, s, s, m (the core's height when down) |
| `orbiter.dive.fall` | `0.4` | 1041 | s, s, m, HP, s, s, m (the core's height when down) |
| `orbiter.dive.radius` | `12` | 1041 | s, s, m, HP, s, s, m (the core's height when down) |
| `orbiter.dive.damage` | `50` | 1041 | s, s, m, HP, s, s, m (the core's height when down) |
| `orbiter.dive.stun` | `8` | 1041 | s, s, m, HP, s, s, m (the core's height when down) |
| `orbiter.dive.rise` | `2` | 1041 | s, s, m, HP, s, s, m (the core's height when down) |
| `orbiter.dive.restHeight` | `2.5` | 1041 | s, s, m, HP, s, s, m (the core's height when down) |
| `orbiter.pads` | `[[-14,0],[14,0],[0,-14],[0,14]]` | 1042 | jump pads under its orbit (Grinder floor, off the centre) |
| `orbiter.padTarget` | `18` | 1043 | m: a pad throws you toward the ring, at most this high (then the GRAPPLE NODES) |
| `orbiter.padBelow` | `12` | 1043 | m: a pad throws you toward the ring, at most this high (then the GRAPPLE NODES) |
| `orbiter.turretEvery` | `0.9` | 1044 | s between a live pod's cyan orbs; m/s |
| `orbiter.turretSpeed` | `16` | 1044 | s between a live pod's cyan orbs; m/s |
| `hive` | {…} | 1047 | (above) HIVE (round 25): three egg sacs on the Grinder's towers, a queen who drops in once they fall. |
| `hive.sacs` | `[[-18,8.2,-18],[14,8.2,-2],[-2,8.2,14]]` | 1049 | (above) The sacs sit on three of the 'towers' preset's 6 m towers (cells 1 / 5 / 9: x or z = -18, -2, 14). |
| `hive.bulletMult` | `0.2` | 1051 | + any explosion and the Thumper's quake |
| `hive.blastMult` | `3` | 1051 | + any explosion and the Thumper's quake |
| `hive.blastWeapons` | `["bouncer","rocket","vortex"]` | 1051 | + any explosion and the Thumper's quake |
| `hive.spawnEvery` | `7` | 1052 | s, squads, alive adds, m off the tower |
| `hive.spawnKinds` | `[["swarmer","swarmer","swarmer"],["spitter"]]` | 1052 | s, squads, alive adds, m off the tower |
| `hive.maxAdds` | `12` | 1052 | s, squads, alive adds, m off the tower |
| `hive.hatchOffset` | `3.5` | 1052 | s, squads, alive adds, m off the tower |
| `hive.acid` | {…} | 1053 | green-white for 1 s, then hot |
| `hive.acid.every` | `3.2` | 1053 | green-white for 1 s, then hot |
| `hive.acid.warn` | `1` | 1053 | green-white for 1 s, then hot |
| `hive.acid.life` | `4` | 1053 | green-white for 1 s, then hot |
| `hive.acid.radius` | `2.8` | 1053 | green-white for 1 s, then hot |
| `hive.acid.dps` | `14` | 1053 | green-white for 1 s, then hot |
| `hive.acid.count` | `3` | 1053 | green-white for 1 s, then hot |
| `hive.acid.tickEvery` | `0.25` | 1053 | green-white for 1 s, then hot |
| `hive.queen` | {…} | 1054 | HP, m, m, s, m above the floor, x a momentum wave |
| `hive.queen.hp` | `2600` | 1054 | HP, m, m, s, m above the floor, x a momentum wave |
| `hive.queen.radius` | `1.3` | 1054 | HP, m, m, s, m above the floor, x a momentum wave |
| `hive.queen.height` | `2.4` | 1054 | HP, m, m, s, m above the floor, x a momentum wave |
| `hive.queen.dropTime` | `1.2` | 1054 | HP, m, m, s, m above the floor, x a momentum wave |
| `hive.queen.dropFrom` | `30` | 1054 | HP, m, m, s, m above the floor, x a momentum wave |
| `hive.queen.impactMult` | `2` | 1054 | HP, m, m, s, m above the floor, x a momentum wave |
| `hive.queenAt` | `[6,6]` | 1055 | where she lands (x, z from the Grinder centre) |
| `hive.leap` | {…} | 1056 | s (the crouch), m, HP, s, m, s in the air |
| `hive.leap.tell` | `0.9` | 1056 | s (the crouch), m, HP, s, m, s in the air |
| `hive.leap.radius` | `4` | 1056 | s (the crouch), m, HP, s, m, s in the air |
| `hive.leap.damage` | `30` | 1056 | s (the crouch), m, HP, s, m, s in the air |
| `hive.leap.gap` | `1.6` | 1056 | s (the crouch), m, HP, s, m, s in the air |
| `hive.leap.height` | `7` | 1056 | s (the crouch), m, HP, s, m, s in the air |
| `hive.leap.time` | `0.8` | 1056 | s (the crouch), m, HP, s, m, s in the air |
| `powerUps` | {…} | 1059 | (above) OVERNIGHT2 N6 item 1: power-up drops (survival/powerUps.js). Colours: blocky retro-future, one per type. |
| `powerUps.eliteKinds` | `["spitter","charger","lancer","warden"]` | 1061 | 2% an elite kill; a boss always drops one |
| `powerUps.eliteChance` | `0.02` | 1061 | 2% an elite kill; a boss always drops one |
| `powerUps.styleDrop` | {…} | 1062 | (above) NIGHT4 T1 (was none): a style drop: 6 kills inside 4 s, or 3 kills while airborne inside 2 s, drops one too, then |
| `powerUps.styleDrop` | {…} | 1063 | (above) nothing for 90 s (fixed-step s). The Q1 bot run saw 3 drops in 15 rounds, all from bosses (docs/NIGHT4-PLAN.md). |
| `powerUps.killDrop` | {…} | 1065 | (above) NIGHT6 B4 (the elite roll and the bosses gave 3 drops in 15 rounds): every other kill rolls `chance` (x eliteMult for an |
| `powerUps.killDrop` | {…} | 1066 | (above) elite, x SCAVENGER), x2 after `rise` s without a drop, x4 after `rise2` (Killing Floor 2's Zed Time shape), none |
| `powerUps.killDrop` | {…} | 1067 | (above) within `minGap` s of the last drop; the run's own seeded stream draws it and its type (never Math.random). |
| `powerUps.life` | `12` | 1069 | s to grab it, s it blinks out, m |
| `powerUps.blinkFrom` | `3` | 1069 | s to grab it, s it blinks out, m |
| `powerUps.grabRadius` | `1.6` | 1069 | s to grab it, s it blinks out, m |
| `powerUps.grabHeight` | `2.5` | 1069 | s to grab it, s it blinks out, m |
| `powerUps.pool` | `4` | 1070 | obelisks at once, m over the floor, m of light beam, the through-wall glow |
| `powerUps.hover` | `1.3` | 1070 | obelisks at once, m over the floor, m of light beam, the through-wall glow |
| `powerUps.beamHeight` | `7` | 1070 | obelisks at once, m over the floor, m of light beam, the through-wall glow |
| `powerUps.wallGlow` | `0.35` | 1070 | obelisks at once, m over the floor, m of light beam, the through-wall glow |
| `powerUps.icons` | {…} | 1078 | (above) 7 x 7 pixel icons for the HUD timers (rows of '#' / '.'). |
| `traps` | {…} | 1088 | (above) OVERNIGHT2 N6 item 2: traps (survival/traps.js). kill: what a regular enemy takes (dead); elite: an elite's hit; boss: a |
| `traps` | {…} | 1089 | (above) boss's nick; every: s between ticks / zaps / slams; time: s it runs; cooldown: s after. |
| `affixes` | {…} | 1104 | (above) OVERNIGHT2 N6 item 3: enemy affixes (enemies/affixes.js). From round `from` a regular spawn may get one (chance +perRound |
| `affixes` | {…} | 1105 | (above) each round, at most maxChance), from `twoFrom` a second; bonusPoints per affix on the kill. |
| `affixes.iconSize` | `0.5` | 1108 | m, m over the feet, per affix |
| `affixes.iconHeight` | `2.6` | 1108 | m, m over the feet, per affix |
| `affixes.maxIcons` | `24` | 1108 | m, m over the feet, per affix |
| `challenges` | {…} | 1118 | (above) OVERNIGHT2 N6 item 5: 24 local badges (modes/challenges.js). target: the count / best value that earns it. |
| `challenges.saveEvery` | `2` | 1120 | s between progress saves |
| `kiosk` | {…} | 1149 | (above) OVERNIGHT2 N6 item 8: the demo kiosk (?demo=1, modes/kiosk.js): idle reset, the attract flythrough. |
| `kiosk` | {…} | 1150 | s, rad/s, m, m, m |
| `kiosk.idleReset` | `60` | 1150 | s, rad/s, m, m, m |
| `kiosk.orbitSpeed` | `0.12` | 1150 | s, rad/s, m, m, m |
| `kiosk.orbitRadius` | `16` | 1150 | s, rad/s, m, m, m |
| `kiosk.orbitHeight` | `9` | 1150 | s, rad/s, m, m, m |
| `kiosk.lookHeight` | `2` | 1150 | s, rad/s, m, m, m |
| `bossRush` | {…} | 1152 | (above) OVERNIGHT2 N6 item 6: BOSS RUSH (modes/bossRush.js). |
| `tutorial` | {…} | 1155 | (above) OVERNIGHT2 N6 item 9: the first-run tutorial (modes/tutorial.js) on the TRAINING course. {key} names the bound key. |
| `tutorial.moveDistance` | `6` | 1157 | m, m, m, s |
| `tutorial.targetDistance` | `12` | 1157 | m, m, m, s |
| `tutorial.orbDistance` | `10` | 1157 | m, m, m, s |
| `tutorial.orbEvery` | `1.8` | 1157 | m, m, m, s |
| `results` | {…} | 1172 | (above) OVERNIGHT2 N6 item 10: the end screen's grade (a blend of round / roundFull, best rank / 5 and accuracy; the first band |
| `results` | {…} | 1173 | (above) reached) and the multi-kill window of the best moments. |
| `results.multiWindow` | `1.2` | 1175 | s |
| `input` | {…} | 1180 | (above) OVERNIGHT2 N6 item 7: the rebindable keyboard actions and their default keys (input/bindings.js). Mouse buttons, |
| `input` | {…} | 1181 | (above) the number keys and the debug keys are fixed; Ctrl can never be bound. |
| `input.bindings.special` | `"KeyB"` | 1186 | POLISH V2: the zoom guns' specials (right mouse zooms them); any gun's special |
| `input.bindings.restart` | `"KeyR"` | 1187 | OVERNIGHT3 P2: restart the run (a fixed key before, so binding any action to R also restarted) |
| `trials` | {…} | 1196 | (above) OVERNIGHT3 P5: TRIALS (modes/trials.js, modes/trialMath.js, docs/TRIALS.md): short courses against the clock. Gates are |
| `trials` | {…} | 1197 | (above) spheres your chest passes through in order, targets break on a fire press aimed through them; the clock starts on |
| `trials` | {…} | 1198 | (above) your first input. Medals: par x ratio (par = the scripted route's time, tests/trials.test.js). Ratios: ace 1. |
| `trials.version` | `1` | 1200 | records and ghosts carry it; a course edit bumps it (old ghosts are dropped) |
| `trials.gateRadius` | `2.2` | 1201 | m: the gate sphere, the chest height above the feet it is measured from |
| `trials.chest` | `1` | 1201 | m: the gate sphere, the chest height above the feet it is measured from |
| `trials.settleSteps` | `30` | 1202 | the par runs stand this many steps after the spawn before their first input, as a player does |
| `trials.ghost` | {…} | 1203 | (above) OVERNIGHT3 P6: ghosts (modes/ghosts.js, modes/ghostCodec.js, fx/ghostView.js): your best run of a trial, replayed. |
| `trials.ghost.maxBytes` | `120000` | 1205 | characters of encoded run kept per trial (a 30 s run is about 12 000; past this it isn't kept) |
| `trials.ghost.maxSteps` | `10800` | 1206 | steps recorded at most (the 90 s ceiling of a trial) |
| `trials.ghost.maxSettle` | `240` | 1207 | neutral steps before the first input that a replay re-runs (the stance is settled by then) |
| `trials.ghost.opacity` | `0.32` | 1208 | the silhouette: translucent cyan (player-usable), never RIVAL's magenta |
| `trials.ghost.color` | `1692646` | 1208 | the silhouette: translucent cyan (player-usable), never RIVAL's magenta |
| `trials.targetRadius` | `0.9` | 1213 | m: a target's hit sphere, the farthest shot that counts |
| `trials.targetRange` | `70` | 1213 | m: a target's hit sphere, the farthest shot that counts |
| `trials.look` | {…} | 1216 | (above) fx/trialCourse.js: ring tube (x gate radius), target size (m), the next gate's pulse, brightness (x the colour), |
| `trials.look` | {…} | 1217 | (above) target bob (m) and spin (rad/s), the burst when a target breaks. |
| `trials.list` | array[6] | 1222 | (above) Each: id, name, what it trains (one line), map (+ core preset), start {x, y, z, yawDeg}, gates [x, y, z, r?] in |
| `trials.list` | array[6] | 1223 | (above) order, targets [x, y, z], par (s: the route's time in tests/support/trialRoutes.js; re-derive it if a course moves). |
| `gamepad` | {…} | 1262 | (above) OVERNIGHT3 P3: the gamepad (input/gamepad.js; the button table is DEFAULT_PAD_LAYOUT there, docs/GAMEPAD.md). |
| `gamepad.yawRate` | `220` | 1264 | deg/s at full tilt and x1 sensitivity |
| `gamepad.pitchRate` | `150` | 1264 | deg/s at full tilt and x1 sensitivity |
| `gamepad.menuDelay` | `0.35` | 1265 | s before a held direction repeats, s between repeats, stick tilt |
| `gamepad.menuEvery` | `0.12` | 1265 | s before a held direction repeats, s between repeats, stick tilt |
| `gamepad.menuStick` | `0.6` | 1265 | s before a held direction repeats, s between repeats, stick tilt |
| `gamepad.activity` | `0.5` | 1266 | a stick past this (or any button) means the pad is in use: its prompts show, the mouse lets go |
| `gamepad.breakawayShare` | `0.8` | 1267 | the aim lock's breakaway on a pad: at most this x the pad's top turn rate (a stick can't flick) |
| `gamepad.vibration` | {…} | 1268 | [strong, weak, ms], light |
| `gamepad.vibration.hurt` | `[0.3,0.5,90]` | 1268 | [strong, weak, ms], light |
| `gamepad.vibration.kill` | `[0,0.25,45]` | 1268 | [strong, weak, ms], light |
| `gamepad.vibration.parry` | `[0.45,0.2,70]` | 1268 | [strong, weak, ms], light |
| `mutators` | {…} | 1271 | (above) OVERNIGHT2 N6 item 4: mutators (modes/mutators.js), unlocked by the best Survival round reached. |
| `telegraph` | {…} | 1281 | (above) OVERNIGHT2 §8: the shared telegraph primitives (fx/telegraph.js): pools, the hit moment's hold, glow, the white flash, |
| `telegraph` | {…} | 1282 | (above) the edge arrows' life; wide danger rects (> wideAt m) fill at wideFill, safe lanes at laneFill. |
| `boss` | {…} | 1285 | (above) SURVIVAL §8 The boss MONOLITH (S7; HP and the named numbers from SURVIVAL, the rest S) |
| `boss.hp` | `6000` | 1287 | +40% each later boss round |
| `boss.hpPerBoss` | `0.4` | 1287 | +40% each later boss round |
| `boss.height` | `12` | 1288 | m |
| `boss.bodyHalf` | `2.4` | 1288 | m |
| `boss.coreRadius` | `1.1` | 1288 | m |
| `boss.coreHeight` | `7` | 1288 | m |
| `boss.coreForward` | `1.3` | 1288 | m |
| `boss.kneelDrop` | `3.5` | 1288 | m |
| `boss.body` | array[5] | 1289 | stacked stone pillars [w, h, d, x, y, z, yaw] |
| `boss.bodyMult` | `0.5` | 1293 | x damage (OVERNIGHT2 N5: core 2 -> 1.6 for the snap) |
| `boss.coreMult` | `1.6` | 1293 | x damage (OVERNIGHT2 N5: core 2 -> 1.6 for the snap) |
| `boss.coreVulnerableMult` | `3` | 1293 | x damage (OVERNIGHT2 N5: core 2 -> 1.6 for the snap) |
| `boss.parryDamage` | `200` | 1293 | x damage (OVERNIGHT2 N5: core 2 -> 1.6 for the snap) |
| `boss.momentumCoreMult` | `1.5` | 1294 | x more on a melee hit with momentum on the kneeling core (OVERNIGHT2 N5) |
| `boss.introTime` | `2` | 1295 | s rising out of the Grinder (no damage) |
| `boss.midAt` | `0.66` | 1296 | phase thresholds; the midpoint roar + reshape |
| `boss.p3At` | `0.33` | 1296 | phase thresholds; the midpoint roar + reshape |
| `boss.midTime` | `2.5` | 1296 | phase thresholds; the midpoint roar + reshape |
| `boss.actionGap` | `0.9` | 1297 | s between attacks |
| `boss.ringWidth` | `0.9` | 1299 | a jump clears a ring |
| `boss.ringHeight` | `0.7` | 1299 | a jump clears a ring |
| `boss.ringDamage` | `25` | 1299 | a jump clears a ring |
| `boss.ringMaxRadius` | `34` | 1299 | a jump clears a ring |
| `boss.kneelTime` | `3` | 1300 | s the core is exposed (x3) |
| `boss.p3Window` | `4` | 1300 | s the core is exposed (x3) |
| `boss.surgeHitHeight` | `2` | 1303 | m over the surface it hits; s the pillar stands; s after |
| `boss.surgeErupt` | `0.5` | 1303 | m over the surface it hits; s the pillar stands; s after |
| `boss.surgeRecover` | `0.6` | 1303 | m over the surface it hits; s the pillar stands; s after |
| `boss.sweepOvershoot` | `12` | 1305 | m past you; m/s knockback away + up |
| `boss.sweepPush` | `14` | 1305 | m past you; m/s knockback away + up |
| `boss.sweepPushUp` | `7` | 1305 | m past you; m/s knockback away + up |
| `boss.laserHitCooldown` | `0.3` | 1307 | s between two laser hits |
| `boss.addsEvery` | `20` | 1308 | phase 2+: a squad every 20 s, at most 10 adds alive |
| `boss.maxAdds` | `10` | 1308 | phase 2+: a squad every 20 s, at most 10 adds alive |
| `boss.addSquads` | array[2] | 1309 | one at random |
| `boss.midSquads` | `[["husk","husk","husk"],["husk","husk","husk"]]` | 1310 | the midpoint's two Husk squads |
| `boss.roundReshape` | `0.6` | 1311 | s: the Grinder flattens for a boss round; phase 3's platforms rise |
| `boss.sinkTime` | `1` | 1311 | s: the Grinder flattens for a boss round; phase 3's platforms rise |
| `boss.pulsePush` | `16` | 1312 | the midpoint knockback (no damage) |
| `boss.pulseUp` | `6` | 1312 | the midpoint knockback (no damage) |
| `boss.killTime` | `0.5` | 1313 | s of slow-mo (x time scale) when it dies |
| `boss.killSlowMo` | `0.3` | 1313 | s of slow-mo (x time scale) when it dies |
| `boss.points` | `2000` | 1314 | m the body stays inside |
| `boss.arenaHalf` | `18` | 1314 | m the body stays inside |
| `boss.fallDamage` | `20` | 1315 | phase 3: touching the sunk floor puts you back on a platform for this |
| `boss.abyssY` | `0.5` | 1316 | phase 3: grounded below this height inside the Grinder = the sunk floor |
| `boss.abyssHalf` | `24` | 1317 | m: the sunk floor's half-size around the origin (the Grinder) |
| `boss.platformCells` | `3` | 1318 | core cells per platform block (you land on the block's centre cell) |
| `boss.fallGrace` | `1` | 1319 | s after the sink before the floor counts (the platforms are still rising) |
| `drone` | {…} | 1322 | (above) §5 Drone (SPEC 2.0; S unless noted) |
| `drone.radius` | `0.6` | 1325 | m hit + collision sphere |
| `drone.speed` | `7` | 1326 | m/s max flying speed (S) |
| `drone.accel` | `6` | 1327 | 1/s steering toward the desired velocity (S) |
| `drone.hoverMin` | `4` | 1328 | m above the ground below it |
| `drone.hoverMax` | `8` | 1329 | m |
| `drone.rangeMin` | `12` | 1330 | m horizontal from the player: backs off inside this |
| `drone.rangeMax` | `18` | 1331 | m: closes in beyond this |
| `drone.orbitSpeed` | `3` | 1332 | m/s sideways drift while in range (S) |
| `drone.probeDistance` | `2.5` | 1333 | m ahead: a blocked sphere probe means climb (S) |
| `drone.climbAbove` | `2` | 1334 | m above the blocking top |
| `drone.climbProbeHeight` | `40` | 1335 | m above the probe point where the down ray starts |
| `drone.fireInterval` | `2` | 1336 | s between orbs |
| `drone.chargeTime` | `0.4` | 1337 | s glow before each orb |
| `drone.losSlack` | `0.1` | 1338 | m, a line-of-sight ray may stop this short of the player's eye |
| `drone.maxCount` | `20` | 1339 | instanced capacity |
| `drone.coreScale` | `0.45` | 1340 | × radius, the glowing core |
| `drone.corePulseRate` | `5` | 1341 | rad/s idle pulse |
| `drone.corePulseChargeRate` | `30` | 1342 | rad/s while charging |
| `drone.bodyShade` | `0.85` | 1343 | × enemy color on the shell |
| `drone.hitFlashTime` | `0.08` | 1344 | s |
| `drone.spawnRise` | `1.5` | 1345 | m above the spawn point before it starts hovering |
| `drone.separation` | `2.4` | 1346 | m, drones push apart inside this |
| `drone.fireStagger` | `0.5` | 1347 | first orb after fireInterval × (stagger .. 1 + stagger), so they don't sync |
| `drone.spinRate` | `1.5` | 1348 | rad/s visual spin |
| `drone.spinChargeRate` | `6` | 1349 | rad/s while charging |
| `drone.shellStretch` | `1.35` | 1350 | × height of the octahedron shell |
| `drone.coreSwell` | `0.25` | 1351 | core pulse scale |
| `drone.coreChargeSwell` | `0.6` | 1352 | core pulse scale while charging |
| `drone.chargeTint` | `0.6` | 1353 | shell blends this far toward the tell color while charging |
| `drone.rayEvery` | `12` | 1354 | (above) POWER P1 3D steering (S): a forward ray every rayEvery steps; blocked within rayLength -> steer toward the |
| `drone.rayEvery` | `12` | 1355 | (above) freer of two side rays (+/- sideAngle rad) at sidePush x speed, easing out at sideDecay 1/s. |
| `orb` | {…} | 1359 | (above) §5 Drone orb (SPEC 2.0) |
| `orb.speed` | `12` | 1361 | m/s |
| `orb.damage` | `10` | 1362 | HP |
| `orb.radius` | `0.35` | 1363 | m |
| `orb.lifetime` | `4` | 1364 | s |
| `orb.reflectRange` | `2` | 1365 | m, a punch this close reflects it (a parry) |
| `orb.segments` | `10` | 1369 | sphere detail |
| `orb.hostileColor` | `1692646` | 1370 | cyan: parryable (OVERNIGHT2 §8 telegraph language); punch it back |
| `orb.reflectedColor` | `16774096` | 1371 | warm white once it is yours |
| `orb.ring` | `{"scale":1.7,"tube":0.06,"spin":0.06}` | 1372 | OVERNIGHT3 P2: the parryable orb's ring (x radius, m, rad per step) |
| `orb.ring.scale` | `1.7` | 1372 | OVERNIGHT3 P2: the parryable orb's ring (x radius, m, rad per step) |
| `orb.ring.tube` | `0.06` | 1372 | OVERNIGHT3 P2: the parryable orb's ring (x radius, m, rad per step) |
| `orb.ring.spin` | `0.06` | 1372 | OVERNIGHT3 P2: the parryable orb's ring (x radius, m, rad per step) |
| `spawner` | {…} | 1375 | (above) §5.1 Spawner (SPEC 2.0) |
| `spawner.budgetBase` | `5` | 1377 | budget = base + perWave × wave |
| `spawner.dronesFromWave` | `3` | 1381 | waves before this are Husks only |
| `spawner.droneMaxShare` | `0.4` | 1382 | drones take at most this share of the budget |
| `spawner.droneMinFill` | `0.5` | 1383 | at least this share of the drone cap (seeded) (S) |
| `spawner.startGroupsMin` | `2` | 1386 | groups that telegraph at wave start (the rest queue) |
| `spawner.telegraphTime` | `1` | 1389 | s spawn beam before a group appears |
| `spawner.minPlayerDistance` | `25` | 1390 | m, spawn points closer than this are skipped |
| `spawner.groupSpread` | `2.5` | 1391 | m between members of a group |
| `spawner.droneHighChance` | `0.5` | 1392 | a drone in a group uses a high point when one is free (S) |
| `spawner.stuckMove` | `1` | 1393 | m: moving less than this ... |
| `spawner.stuckTime` | `12` | 1394 | s ... without dealing damage = stuck -> teleport |
| `spawner.markersAfter` | `45` | 1395 | s into a wave: markers on the remaining enemies |
| `spawner.seedStride` | `7919` | 1396 | wave seed = run seed + wave × stride |
| `spawner.beamRadius` | `0.5` | 1397 | m |
| `spawner.beamHeight` | `30` | 1398 | m |
| `spawner.beamOpacity` | `0.35` | 1399 | base; pulses up to 2x |
| `spawner.beamPulseRate` | `4` | 1400 | pulses per second |
| `spawner.beamGrow` | `1` | 1401 | × radius added per second of telegraph |
| `core` | {…} | 1404 | (above) §5.2 The Spire's reshaping core "Grinder" (SPEC 2.0; S) |
| `core.maxHeight` | `6` | 1406 | m, tallest core pillar |
| `core.heightStep` | `1.5` | 1407 | m, pillar heights are multiples of this (jumpable steps) |
| `core.minHeight` | `0.02` | 1408 | m, pillars at or below this have no collision (the floor) |
| `core.carryTolerance` | `0.15` | 1409 | m, feet this close to a moving pillar top ride with it |
| `core.floorThickness` | `2` | 1410 | m, the platform slab under the map |
| `core.duelPreset` | `"towers"` | 1411 | Duel locks the core to this preset |
| `nav` | {…} | 1414 | (above) §6 Duel nav graph (SPEC 2.0; S) |
| `nav.floorSpacing` | `6` | 1416 | m between floor nodes |
| `nav.floorInset` | `3` | 1417 | m from the platform edge |
| `nav.topSpacing` | `4` | 1418 | m between nodes on a raised top |
| `nav.topInset` | `0.7` | 1419 | m from a top's edges |
| `nav.probeUp` | `2.2` | 1420 | m of headroom a node needs (a capsule must fit) |
| `nav.stepHeight` | `0.4` | 1421 | m a walk edge may climb or drop |
| `nav.walkMax` | `8.5` | 1422 | m, longest walk edge |
| `nav.maxEdge` | `26` | 1423 | m, longest edge of any kind |
| `nav.downEdgeMax` | `12` | 1424 | m, longest jump edge that lands lower |
| `nav.reachMargin` | `1` | 1425 | prefilter: candidates past this share of the jump reach are not flown |
| `nav.groundSample` | `1` | 1426 | m between ground checks on a walk edge |
| `nav.lineHeight` | `1` | 1427 | m above the feet for line-of-flight checks |
| `nav.flightSphere` | `0.45` | 1428 | m, head + feet spheres swept along a simulated flight (capsule radius + a safety margin) |
| `nav.flightSteps` | `300` | 1429 | longest simulated flight (steps at simHz) |
| `nav.flightCheckEvery` | `1` | 1430 | steps between collision checks along a flight |
| `nav.landTolerance` | `0.15` | 1431 | m, the landing point's ground vs the target height |
| `nav.fallLimit` | `3` | 1432 | m below the target height: the flight failed |
| `nav.flightsPerTop` | `2` | 1433 | nearest nodes of each destination top that are flown |
| `nav.floorGroup` | `12` | 1434 | m, floor nodes group by this cell for flightsPerTop |
| `nav.runup` | `1.6` | 1435 | m of run-up ground behind a running jump's node |
| `nav.settleRadius` | `0.2` | 1436 | m: standing jumps and dash jumps start this close to their takeoff spot |
| `nav.settleSpeed` | `0.5` | 1437 | m/s, ... and this slow |
| `nav.standingBacks` | `[0,0.9,1.8]` | 1438 | m a standing jump may back off from its node to take off |
| `nav.dashMin` | `5` | 1439 | m, shorter hops never dash |
| `nav.dashJumpDelay` | `3` | 1440 | steps into the dash before the jump |
| `nav.dropOverhead` | `0.2` | 1441 | s added to a drop's cost |
| `nav.padCost` | `1.5` | 1442 | s, a pad edge |
| `nav.walkWalkers` | `["floor","cover","ledge","bridge","island","spire"]` | 1443 | box kinds with nodes on top |
| `nav.jumpOverhead` | `0.3` | 1444 | s added to a jump edge's cost |
| `nav.dashPipCost` | `1.5` | 1445 | s added to a dash edge's cost (it spends stamina) |
| `nav.arriveRadius` | `1.3` | 1446 | m, a node counts as reached |
| `nav.airBrakeSlack` | `0.3` | 1447 | m: in the air, brake once the stopping distance exceeds the gap by this |
| `nav.arriveHeight` | `1.4` | 1448 | m |
| `nav.stuckTime` | `2.5` | 1449 | s without getting closer -> replan |
| `nav.stuckProgress` | `0.6` | 1450 | m the distance must shrink by |
| `nav.gridSnap` | `1.5` | 1451 | m: a nav grid link's end within this of a graph node uses that node (POWER P1) |
| `nav.gridLinkSpacing` | `5` | 1452 | m: along a ledge, one grid jump / gap link every this far joins the graph |
| `bot` | {…} | 1455 | (above) §6 RIVAL + BotBrain (SPEC 2.0; S unless noted) |
| `bot.color` | `16723592` | 1458 | magenta |
| `bot.losInterval` | `0.1` | 1461 | s between line-of-sight checks |
| `bot.reaction` | `{"easy":0.4,"normal":0.25,"hard":0.15}` | 1462 | s |
| `bot.reaction.easy` | `0.4` | 1462 | s |
| `bot.reaction.normal` | `0.25` | 1462 | s |
| `bot.reaction.hard` | `0.15` | 1462 | s |
| `bot.aimErrorBase` | `3` | 1464 | deg |
| `bot.aimErrorPerSpeed` | `0.2` | 1465 | deg per m/s of player speed |
| `bot.aimTolerance` | `1.5` | 1466 | deg past the error before it holds fire |
| `bot.errorReroll` | `0.35` | 1467 | s between aim-error rolls |
| `bot.preferredRange` | `{"revolver":12,"shotgun":5}` | 1468 | m |
| `bot.preferredRange.revolver` | `12` | 1468 | m |
| `bot.preferredRange.shotgun` | `5` | 1468 | m |
| `bot.shotgunRange` | `8` | 1469 | m, closer than this it switches to the shotgun |
| `bot.rangeSlack` | `2` | 1470 | m around the preferred range |
| `bot.strafeSwitch` | `[0.8,1.8]` | 1471 | s between strafe direction changes |
| `bot.jumpChance` | `0.5` | 1472 | per strafe switch |
| `bot.evadeCooldown` | `2` | 1473 | s |
| `bot.evadeRadius` | `2.5` | 1474 | m, a player shot passing this close makes it dodge |
| `bot.recoverTime` | `4` | 1476 | s |
| `bot.wallJumpRate` | `6` | 1477 | Hard: wall jumps per s of airborne wall contact (a chance per step) |
| `bot.slideStraight` | `10` | 1478 | m, walk edges longer than this get slide hops |
| `bot.chestHeight` | `1.2` | 1479 | m above the feet: where it aims at you, and where you aim at it |
| `bot.crouchChest` | `0.6` | 1480 | m, your chest while you slide |
| `bot.headRadius` | `0.24` | 1481 | m, RIVAL's head sphere (headshots) |
| `bot.headHeight` | `1.55` | 1482 | m above the feet |
| `bot.turnRate` | `540` | 1483 | deg/s it can turn its aim |
| `bot.replanInterval` | `1` | 1484 | s between CHASE replans while the goal moves |
| `bot.replanDistance` | `4` | 1485 | m the goal must move to replan |
| `bot.searchAfter` | `3` | 1486 | s out of sight before it heads for where you really are (it hears you) |
| `bot.approachHopSpeed` | `1.5` | 1487 | m/s: walking straight at you and slower than this (blocked) -> hop |
| `bot.hideRadius` | `30` | 1488 | m, RECOVER looks for cover this close |
| `bot.hideCandidates` | `40` | 1489 | nodes it checks for cover |
| `bot.recoverCooldown` | `8` | 1490 | s after a RECOVER before the next |
| `bot.slideJumpGap` | `6` | 1491 | m past the preferred range before it slide-jumps in |
| `bot.slideJumpChance` | `1.5` | 1492 | per s while closing a gap |
| `bot.gunScale` | `2` | 1493 | × the viewmodel gun length (readable at range) |
| `bot.gunOffset` | `[0.32,1.25,0.25]` | 1494 | m: right, up, forward from the feet |
| `bot.muzzleForward` | `{"revolver":0.55,"shotgun":1.4}` | 1495 | m from the gun pivot along the aim |
| `bot.muzzleForward.revolver` | `0.55` | 1495 | m from the gun pivot along the aim |
| `bot.muzzleForward.shotgun` | `1.4` | 1495 | m from the gun pivot along the aim |
| `bot.visorBox` | `[0.42,0.12,0.2]` | 1496 | m |
| `bot.visorOffset` | `[0,1.55,-0.2]` | 1497 | m: up, forward |
| `bot.tagPx` | `110` | 1498 | px, name tag width |
| `bot.tagHeight` | `2.3` | 1500 | m above the feet |
| `duel` | {…} | 1503 | (above) §6 Duel rules (SPEC 2.0) |
| `duel.roundsToWin` | `3` | 1505 | settings.duelRounds overrides (3 or 5) |
| `duel.countdown` | `3` | 1506 | s, 3-2-1 |
| `duel.roundBreak` | `2.2` | 1507 | s after a kill before the next countdown |
| `duel.orbHeal` | `25` | 1508 | HP |
| `duel.orbRespawn` | `20` | 1509 | s |
| `duel.orbRadius` | `0.45` | 1510 | m visual |
| `duel.orbPickup` | `1.6` | 1511 | m |
| `duel.orbSpots` | `[[0,0.7,10],[0,0.7,-10]]` | 1512 | x, y above the ground, z (the core) |
| `duel.orbBob` | `0.25` | 1513 | m |
| `endless` | {…} | 1516 | (above) §5 Endless wave loop (SPEC 2.0) |
| `endless.breakTime` | `4` | 1518 | s: full heal + pips, then the WAVE N banner |
| `endless.bannerTime` | `1.6` | 1519 | s the WAVE N banner shows |
| `endless.reshapeTime` | `1.5` | 1520 | s (Phase 2 core reshape) |
| `endless.pointsPerWave` | `100` | 1521 | score per cleared wave |
| `regen` | {…} | 1524 | (above) SURVIVAL §2 Health regen (replaces the 2.0 close-kill / close-hit heals; the parry +15 stays). |
| `regen` | {…} | 1525 | (above) Survival, Classic Arena and Training; never the Duel (health orbs only) or the debug sandbox. |
| `regen.delay` | `4` | 1527 | s without taking damage before regen starts (S) |
| `regen.rate` | `25` | 1528 | HP/s (S) |
| `regen.fastSpeed` | `8` | 1529 | m/s: moving faster than this counts the delay down fastMult x (S) |
| `regen.fastMult` | `2` | 1530 | ... and so does dealing damage in the last dealtWindow s |
| `regen.dealtWindow` | `1` | 1531 | s |
| `regen.rankPerRank` | `0.08` | 1532 | (above) NIGHT4 T1 (was 0: no rank effect): x the rate and / the delay per style rank above DULL, the stamina reward's +8% a |
| `regen.rankPerRank` | `0.08` | 1533 | (above) rank (style.rankRewardPerRank): SKYGRIND regenerates 1.4x as fast after a 2.86 s delay. Taking damage drops the |
| `regen.rankPerRank` | `0.08` | 1534 | (above) meter, so the reward is for staying stylish while hurt. |
| `regen.vignetteMax` | `0.5` | 1536 | opacity of the red damage vignette at 0 HP; it eases out as HP returns (S) |
| `regen.vignetteEase` | `6` | 1537 | 1/s, how fast the damage vignette follows the HP |
| `points` | {…} | 1540 | (above) SURVIVAL §2 Points (Zombies-inspired numbers, our own tuning) |
| `points.start` | `500` | 1542 | bank at the start of a run |
| `points.hit` | `10` | 1543 | a hit that doesn't kill |
| `points.roundClear` | `100` | 1548 | × the round number (flat: no multipliers) |
| `points.rankMult` | `[1,1.1,1.25,1.5,1.75,2]` | 1549 | × combat points per style rank D..SS |
| `points.campMult` | `0.5` | 1550 | × combat points while EXPOSED (SURVIVAL §6) |
| `points.popTime` | `0.8` | 1551 | s a "+60" pop floats near the crosshair |
| `points.popRise` | `46` | 1552 | px it rises over its life |
| `points.popSpread` | `40` | 1553 | px of random sideways scatter |
| `points.popPool` | `10` | 1554 | pooled pop labels |
| `buy` | {…} | 1557 | (above) SURVIVAL §2 Buying: hold E on a buyable in reach (gates, wall buys, perks) |
| `buy.holdTime` | `0.35` | 1559 | s of holding E to buy |
| `buy.reach` | `3` | 1560 | m from your feet (horizontal) to a buy point |
| `buy.reachHeight` | `3` | 1561 | m of vertical slack |
| `buy.coneDeg` | `70` | 1562 | half-angle: the buy point must be roughly in front of you (S) |
| `buy.deniedFlashTime` | `0.4` | 1563 | s the prompt flashes after a failed buy |
| `zones` | {…} | 1566 | (above) SURVIVAL §3 Zones + energy gates (S2; the zone boxes are level data in world/maps/survival.js) |
| `zones.costs` | {…} | 1568 | points to open |
| `zones.costs.armory` | `750` | 1568 | points to open |
| `zones.costs.slabs` | `1000` | 1568 | points to open |
| `zones.costs.grinder` | `1250` | 1568 | points to open |
| `zones.costs.heights` | `1500` | 1568 | points to open |
| `zones.costs.islands` | `2000` | 1568 | points to open |
| `zones.dimAmount` | `0.8` | 1569 | how far a locked zone greys out (0..1) |
| `zones.dimEase` | `2.5` | 1570 | 1/s, the grey-out easing (an opened zone warms up over ~1 s) |
| `zones.dissolveTime` | `1.1` | 1571 | s, a bought gate dissolves |
| `zones.accents` | {…} | 1572 | (above) (the gate field's look lives in `barrier` since POLISH V3: red-orange, the barrier shader) |
| `zones.accents` | {…} | 1573 | (above) POLISH V4 zone accents (trim, lane markings, the entry banner); never the enemy red-orange or RIVAL magenta. |
| `zones.banner` | `{"time":2.4,"fadeIn":0.25,"fadeOut":0.6}` | 1575 | s: the zone name on your first step into each zone in a run |
| `zones.banner.time` | `2.4` | 1575 | s: the zone name on your first step into each zone in a run |
| `zones.banner.fadeIn` | `0.25` | 1575 | s: the zone name on your first step into each zone in a run |
| `zones.banner.fadeOut` | `0.6` | 1575 | s: the zone name on your first step into each zone in a run |
| `zones.accentsAlt` | `{"islands":16761564}` | 1576 | a second tone on half a zone's lane markings (ISLANDS: gold and a pale rose, kept |
| `zones.accentsAlt.islands` | `16761564` | 1576 | a second tone on half a zone's lane markings (ISLANDS: gold and a pale rose, kept |
| `zones.gateCell` | `2` | 1577 | (above) far lighter than RIVAL's magenta) |
| `zones.gateCell` | `2` | 1578 | m, gate faces are drawn in cells of about this (only where they face open space) |
| `zones.labelHeight` | `2.8` | 1579 | m above the buy spot |
| `zones.labelForward` | `1.1` | 1580 | m from the spot toward the gate |
| `zones.buyForward` | `1.7` | 1581 | m from the spot to the gate's buy point (you buy facing the gate) |
| `zones.buyReach` | `3.4` | 1582 | m from you to a gate's buy point |
| `zones.labelScale` | `3.4` | 1583 | m, the price sign's width |
| `zones.nodeSpacing` | `4` | 1584 | m between sampled spawn nodes |
| `zones.highNodeMin` | `2.5` | 1585 | m: nodes this high are "high" (Drones, Lancers) |
| `zones.grinderReshapeEvery` | `3` | 1586 | rounds: the Grinder reshapes on every Nth round clear once it is open |
| `barrier` | {…} | 1589 | (above) POLISH V3 barrier fields (fx/barrier.js): one energy-field shader for the arena boundary, the locked gates and the |
| `barrier` | {…} | 1590 | (above) hazard edges. Colours follow the palette: the boundary is a cool pale violet-white (never cyan, never red-orange), |
| `barrier` | {…} | 1591 | (above) a locked gate is red-orange (blocked: danger), a hazard edge amber. |
| `barrier.strongWithin` | `3` | 1595 | m: strong within this, clear within this, barely there past it |
| `barrier.clearWithin` | `14` | 1595 | m: strong within this, clear within this, barely there past it |
| `barrier.farAlpha` | `0.07` | 1596 | x the field far away (a gate stays readable from across the zone: it has a price) |
| `barrier.gateFarAlpha` | `0.5` | 1596 | x the field far away (a gate stays readable from across the zone: it has a price) |
| `barrier.fadeTop` | `40` | 1597 | m: the boundary's soft vertical fade to nothing at the top |
| `barrier.fadeLength` | `20` | 1597 | m: the boundary's soft vertical fade to nothing at the top |
| `barrier.gateFadeTop` | `30` | 1598 | m: a gate's (a 40 m locked zone fades into the dusk instead of walling you in) |
| `barrier.gateFadeLength` | `24` | 1598 | m: a gate's (a 40 m locked zone fades into the dusk instead of walling you in) |
| `barrier.rippleLife` | `0.7` | 1599 | s a ripple lives, m/s its ring races out |
| `barrier.rippleSpeed` | `7` | 1599 | s a ripple lives, m/s its ring races out |
| `barrier.touchReach` | `0.12` | 1600 | m past the body's radius that counts as touching; probe heights |
| `barrier.probeHeights` | `[0.4,0.9,1.5]` | 1600 | m past the body's radius that counts as touching; probe heights |
| `barrier.minPush` | `0.2` | 1601 | a touch needs this much push; it ends after this long (s) without one |
| `barrier.touchRelease` | `0.2` | 1601 | a touch needs this much push; it ends after this long (s) without one |
| `barrier.flashTime` | `0.18` | 1602 | s the hex flash around a touch takes to fade |
| `barrier.touchPinchDeg` | `0.5` | 1603 | the tiny FOV pinch on a touch (none with reduced motion) |
| `barrier.touchPinchTime` | `0.1` | 1603 | the tiny FOV pinch on a touch (none with reduced motion) |
| `barrier.curtainHeight` | `4` | 1604 | m: the hazard curtain, the grid skirt under the edge, the stripes |
| `barrier.skirtDepth` | `22` | 1604 | m: the hazard curtain, the grid skirt under the edge, the stripes |
| `barrier.stripeWidth` | `0.7` | 1604 | m: the hazard curtain, the grid skirt under the edge, the stripes |
| `skyline` | {…} | 1607 | (above) POLISH V3 the distant skyline past the boundary (world/skyline.js): silhouette slabs out of the cloud sea. |
| `skyline.count` | `34` | 1609 | towers, the ring (m), the feet (under the clouds) |
| `skyline.seed` | `7` | 1609 | towers, the ring (m), the feet (under the clouds) |
| `skyline.radiusMin` | `270` | 1609 | towers, the ring (m), the feet (under the clouds) |
| `skyline.radiusMax` | `345` | 1609 | towers, the ring (m), the feet (under the clouds) |
| `skyline.base` | `-70` | 1609 | towers, the ring (m), the feet (under the clouds) |
| `skyline.widthMin` | `12` | 1610 | m |
| `skyline.widthMax` | `30` | 1610 | m |
| `skyline.heightMin` | `55` | 1610 | m |
| `skyline.heightMax` | `115` | 1610 | m |
| `skyline.tallMin` | `105` | 1610 | m |
| `skyline.tallShare` | `0.2` | 1610 | m |
| `skyline.bodyColor` | `1051424` | 1611 | the share of windows lit |
| `skyline.windowColor` | `16763274` | 1611 | the share of windows lit |
| `skyline.windowGlow` | `0.9` | 1611 | the share of windows lit |
| `skyline.windowShare` | `0.18` | 1611 | the share of windows lit |
| `skyline.air` | `0.58` | 1612 | how much horizon lies in front of them; the haze at their feet (m) |
| `skyline.hazeTop` | `10` | 1612 | how much horizon lies in front of them; the haze at their feet (m) |
| `skyline.hazeBottom` | `-40` | 1612 | how much horizon lies in front of them; the haze at their feet (m) |
| `ai` | {…} | 1615 | (above) SURVIVAL §5 Enemy AI framework (S4; all S): tokens, engagement rings, squads, steering, accuracy. |
| `ai.thinkHz` | `12` | 1617 | AI think ticks per second, staggered across enemies |
| `ai.losPerStep` | `3` | 1618 | line-of-sight raycasts per fixed step (~6 per rendered frame at 60 fps) |
| `ai.losMaxAge` | `0.3` | 1619 | s before an enemy asks for a fresh line-of-sight check |
| `ai.tokens.base` | `{"melee":2,"ranged":3,"heavy":1,"swarm":3}` | 1621 | attack tokens per player (swarm: Swarmer pounces, S6) |
| `ai.tokens.base.melee` | `2` | 1621 | attack tokens per player (swarm: Swarmer pounces, S6) |
| `ai.tokens.base.ranged` | `3` | 1621 | attack tokens per player (swarm: Swarmer pounces, S6) |
| `ai.tokens.base.heavy` | `1` | 1621 | attack tokens per player (swarm: Swarmer pounces, S6) |
| `ai.tokens.base.swarm` | `3` | 1621 | attack tokens per player (swarm: Swarmer pounces, S6) |
| `ai.tokens.roundSteps` | `[10,20]` | 1622 | +1 each from these rounds |
| `ai.tokens.campBonus` | `1` | 1623 | +1 each while the player camps (S5) |
| `ai.tokens.stealMargin` | `0.25` | 1624 | a better score by this share steals an uncommitted token |
| `ai.tokens.losBonus` | `4` | 1625 | score for having line of sight |
| `ai.tokens.flankBonus` | `2` | 1626 | score for a flanker |
| `ai.rings` | array[3] | 1628 | (above) Engagement rings around the player (melee 2-3 m, mid 8-12 m, far 18-30 m) and their slot counts. |
| `ai.aim` | {…} | 1630 | accuracy vs player speed |
| `ai.aim.baseHit` | `0.7` | 1630 | accuracy vs player speed |
| `ai.aim.minHit` | `0.25` | 1630 | accuracy vs player speed |
| `ai.aim.fullSpeed` | `24` | 1630 | accuracy vs player speed |
| `ai.aim.maxLead` | `1.2` | 1630 | accuracy vs player speed |
| `ai.aim.missBy` | `2.4` | 1630 | accuracy vs player speed |
| `ai.steer` | `{"forcePerSpeed":3,"arriveDecel":2,"wander":0.25,"gain":5}` | 1631 | Yuka steering (gain: x the force per think; 5 = turns in ~0.2 s) |
| `ai.steer.forcePerSpeed` | `3` | 1631 | Yuka steering (gain: x the force per think; 5 = turns in ~0.2 s) |
| `ai.steer.arriveDecel` | `2` | 1631 | Yuka steering (gain: x the force per think; 5 = turns in ~0.2 s) |
| `ai.steer.wander` | `0.25` | 1631 | Yuka steering (gain: x the force per think; 5 = turns in ~0.2 s) |
| `ai.steer.gain` | `5` | 1631 | Yuka steering (gain: x the force per think; 5 = turns in ~0.2 s) |
| `ai.strafeAngle` | `0.35` | 1632 | rad an enemy slides along its ring while holding a slot |
| `ai.strafeRate` | `0.5` | 1633 | cycles per second of that slide (a triangle wave: steady motion, no stalls) |
| `ai.strafeFlipTime` | `3` | 1634 | s, the mean time between strafe direction flips |
| `ai.directRange` | `12` | 1635 | m: closer than this an enemy steers straight at its goal, else it follows the flow field |
| `ai.carrotAhead` | `8` | 1636 | m: a flow-field step is pushed this far ahead so Arrive doesn't brake on it |
| `ai.approachSlack` | `3` | 1637 | m past its ring an enemy still counts as approaching |
| `ai.formationRange` | `18` | 1638 | m: squad members farther than this from you keep formation on their leader |
| `ai.formationSpacing` | `2.2` | 1639 | m between formation spots |
| `ai.neighborRadius` | `3` | 1640 | m, separation neighbours |
| `ai.holdSpeed` | `0.55` | 1641 | × speed while strafing on a slot |
| `ai.flankReplan` | `1.5` | 1642 | s between a flanker's A* replans |
| `ai.maxFlankers` | `2` | 1643 | per squad |
| `ai.crowd` | {…} | 1653 | (above) POLISH V6 crowd separation (ai/crowd.js, run by the enemies facade each fixed step): walkers closer than their |
| `ai.crowd` | {…} | 1654 | (above) radii + gap move apart (stiffness of the overlap per step, at most maxPush m), the lighter more (mass); bodies |
| `ai.crowd` | {…} | 1655 | (above) more than `height` m apart vertically don't touch. Flyers, bosses, a leap, a knockback, a nav jump: left out. |
| `ai.crowd.touch` | `1` | 1658 | m between two Husk centres that already reads as one body (the crowdSpread check) |
| `ai.grid` | {…} | 1661 | (above) POWER P1 (all S): the layered nav grid, the flow fields, jump links, whiskers and the stuck watchdog. |
| `ai.grid.cell` | `1` | 1663 | m per column |
| `ai.grid.probeRadius` | `0.45` | 1664 | m: the capsule a node must fit (a Husk + a margin) |
| `ai.grid.probeLift` | `0.1` | 1664 | m: the capsule a node must fit (a Husk + a margin) |
| `ai.grid.probeHeight` | `1.9` | 1664 | m: the capsule a node must fit (a Husk + a margin) |
| `ai.grid.minClear` | `1.9` | 1665 | m of headroom a node needs; the body height jump / drop headroom uses |
| `ai.grid.bodyHeight` | `1.9` | 1665 | m of headroom a node needs; the body height jump / drop headroom uses |
| `ai.grid.scanTop` | `39.5` | 1666 | m: column rays start under the map's 40 m cap |
| `ai.grid.scanBottom` | `-5` | 1666 | m: column rays start under the map's 40 m cap |
| `ai.grid.maxLayers` | `12` | 1667 | floors kept per column (a spire's ledge stack has 6) |
| `ai.grid.step` | `0.6` | 1668 | m: a walk step, the highest jump link, the deepest drop link |
| `ai.grid.jumpUp` | `2.2` | 1668 | m: a walk step, the highest jump link, the deepest drop link |
| `ai.grid.drop` | `5` | 1668 | m: a walk step, the highest jump link, the deepest drop link |
| `ai.grid.stepSmooth` | `0.2` | 1669 | m: a rise over this onto a vertical face (a riser, not a slope) is a small jump |
| `ai.grid.riserLift` | `0.08` | 1669 | m: a rise over this onto a vertical face (a riser, not a slope) is a small jump |
| `ai.grid.gapCells` | `4` | 1670 | gaps up to 3 m (cells to the landing), landing 1 m up .. 2.5 m down |
| `ai.grid.gapDiagCells` | `3` | 1670 | gaps up to 3 m (cells to the landing), landing 1 m up .. 2.5 m down |
| `ai.grid.gapUp` | `1` | 1670 | gaps up to 3 m (cells to the landing), landing 1 m up .. 2.5 m down |
| `ai.grid.gapDown` | `2.5` | 1670 | gaps up to 3 m (cells to the landing), landing 1 m up .. 2.5 m down |
| `ai.grid.gapLiftHigh` | `1.1` | 1671 | m over the floors: the two rays that check a gap jump's line |
| `ai.grid.gapLiftLow` | `0.4` | 1671 | m over the floors: the two rays that check a gap jump's line |
| `ai.grid.jumpCost` | `1.5` | 1672 | m-equivalent added per link (links are slower than walking) |
| `ai.grid.dropCost` | `0.5` | 1672 | m-equivalent added per link (links are slower than walking) |
| `ai.grid.gapCost` | `2` | 1672 | m-equivalent added per link (links are slower than walking) |
| `ai.grid.edgeCost` | `2.5` | 1673 | x the walk cost onto an edge node (a floor missing under one side of the body) |
| `ai.grid.lookBelow` | `2.5` | 1674 | m: a body's node is the highest floor this far under / over its feet |
| `ai.grid.lookAbove` | `0.6` | 1674 | m: a body's node is the highest floor this far under / over its feet |
| `ai.grid.budgetMs` | `1.5` | 1675 | ms of rebuild work per fixed step (time-sliced: a rebuild never hitches) |
| `ai.flow.every` | `0.25` | 1678 | s: the fields toward you are recomputed (4 Hz) when your node changed |
| `ai.flow.goalBelow` | `8` | 1679 | m: while you are in the air, your node is the floor this far under you |
| `ai.flow.seedRadius` | `4` | 1680 | cells around you seeded too (m + x height + m), never above you: a real |
| `ai.flow.seedHeight` | `2` | 1680 | cells around you seeded too (m + x height + m), never above you: a real |
| `ai.flow.seedBase` | `12` | 1680 | cells around you seeded too (m + x height + m), never above you: a real |
| `ai.flow.lookahead` | `2.5` | 1681 | (above) route up to you wins unless it is that much longer (then they gather under you, for the Husk's leap) |
| `ai.flow.lookahead` | `2.5` | 1682 | m: steer at the farthest node this far along the field that a straight line |
| `ai.flow.lookTurn` | `0.5` | 1682 | m: steer at the farthest node this far along the field that a straight line |
| `ai.flow.popsPerStep` | `2500` | 1683 | (above) reaches, stopping where the chain turns (cos of the angle between its steps below this) |
| `ai.flow.popsPerStep` | `2500` | 1684 | Dijkstra nodes settled per fixed step (a field takes a few steps: no frame spike) |
| `ai.jumpLandIn` | `0.7` | 1686 | m a link jump lands past its node (onto the top, never on the edge) |
| `ai.edgeNormalY` | `0.25` | 1687 | a summed contact normal this upright (below a floor's): the floor + a wall, or a ledge corner: |
| `ai.edgeRiseSpeed` | `0.5` | 1688 | m/s: ... it counts as ground for an enemy that isn't rising faster than this |
| `ai.supportedDrop` | `0.002` | 1689 | m: an enemy "falling" but held this still in a step (wedged, perched) is supported |
| `ai.avoid` | {…} | 1690 | whiskers (m, rad, m over walk steps, x speed, thinks) |
| `ai.avoid.length` | `1.6` | 1690 | whiskers (m, rad, m over walk steps, x speed, thinks) |
| `ai.avoid.sideLength` | `1.2` | 1690 | whiskers (m, rad, m over walk steps, x speed, thinks) |
| `ai.avoid.sideAngle` | `0.6` | 1690 | whiskers (m, rad, m over walk steps, x speed, thinks) |
| `ai.avoid.height` | `0.8` | 1690 | whiskers (m, rad, m over walk steps, x speed, thinks) |
| `ai.avoid.push` | `0.9` | 1690 | whiskers (m, rad, m over walk steps, x speed, thinks) |
| `ai.avoid.every` | `2` | 1690 | whiskers (m, rad, m over walk steps, x speed, thinks) |
| `ai.watchdog.minMove` | `0.75` | 1692 | progress: m from where it last made progress, or m beaten on its best field distance |
| `ai.watchdog.minGain` | `0.5` | 1692 | progress: m from where it last made progress, or m beaten on its best field distance |
| `ai.watchdog.ladder` | `[0.5,1,1.5]` | 1693 | s without progress -> action (repeats) |
| `ai.watchdog.actions` | `["jump","sidestep","repath"]` | 1693 | s without progress -> action (repeats) |
| `ai.watchdog.relocateAt` | `3` | 1694 | s; m from you; deg of your facing (off-screen only) |
| `ai.watchdog.relocateMin` | `12` | 1694 | s; m from you; deg of your facing (off-screen only) |
| `ai.watchdog.relocateCone` | `60` | 1694 | s; m from you; deg of your facing (off-screen only) |
| `ai.watchdog.sideTime` | `0.45` | 1695 | s; s on the A* path; the unstick hop (m ahead, m up) |
| `ai.watchdog.repathTime` | `3` | 1695 | s; s on the A* path; the unstick hop (m ahead, m up) |
| `ai.watchdog.hopDistance` | `1.5` | 1695 | s; s on the A* path; the unstick hop (m ahead, m up) |
| `ai.watchdog.hopApex` | `1.2` | 1695 | s; s on the A* path; the unstick hop (m ahead, m up) |
| `ai.jumps` | {…} | 1697 | (above) Per kind: jumps (takes jump + gap links), the telegraph crouch before a leap (s), the arc's clearance over |
| `ai.jumps` | {…} | 1698 | (above) the higher end (m: high enough that the capsule's front clears the ledge corner from a cell away). |
| `ai.jumps.husk` | `{"jumps":true,"tell":0.12,"apex":1.1}` | 1700 | vaults small obstacles |
| `ai.jumps.husk.jumps` | `true` | 1700 | vaults small obstacles |
| `ai.jumps.husk.tell` | `0.12` | 1700 | vaults small obstacles |
| `ai.jumps.husk.apex` | `1.1` | 1700 | vaults small obstacles |
| `ai.jumps.swarmer` | `{"jumps":true,"tell":0.05,"apex":1}` | 1701 | jumps readily |
| `ai.jumps.swarmer.jumps` | `true` | 1701 | jumps readily |
| `ai.jumps.swarmer.tell` | `0.05` | 1701 | jumps readily |
| `ai.jumps.swarmer.apex` | `1` | 1701 | jumps readily |
| `ai.jumps.charger` | `{"jumps":false,"blockedStun":1}` | 1704 | can't jump: stops, stunned 1 s, when its way is blocked |
| `ai.jumps.charger.jumps` | `false` | 1704 | can't jump: stops, stunned 1 s, when its way is blocked |
| `ai.jumps.charger.blockedStun` | `1` | 1704 | can't jump: stops, stunned 1 s, when its way is blocked |
| `director` | {…} | 1708 | (above) SURVIVAL §6 The director (S5; all S unless SURVIVAL gives the number) |
| `director.maxAlive` | `24` | 1710 | enemies alive at once |
| `director.squadInterval` | `2.2` | 1711 | s between squads while there is room (BUILD UP) |
| `director.openingSquads` | `2` | 1712 | squads that arrive together at the start of a round |
| `director.spawnMin` | `15` | 1713 | m: spawn nodes this far from you ... |
| `director.spawnMax` | `40` | 1714 | m ... up to this far |
| `director.spawnFallbackMin` | `10` | 1715 | m, when nothing in 15-40 m is free (small open areas) |
| `director.viewHalfAngle` | `55` | 1716 | deg: a node inside this cone of your facing with line of sight is "in view" |
| `director.highKinds` | `["drone","lancer"]` | 1717 | kinds that spawn on high nodes when a squad is only them |
| `director.intensityPerHp` | `0.012` | 1718 | intensity per HP of damage taken |
| `director.intensityPerKill` | `0.06` | 1719 | per kill near you |
| `director.nearbyKillRange` | `10` | 1720 | m |
| `director.intensityDecay` | `0.05` | 1721 | per s |
| `director.peakAt` | `0.95` | 1722 | intensity that starts a PEAK |
| `director.peakTime` | `3` | 1723 | s of PEAK (spawning pauses) |
| `director.relaxMin` | `8` | 1724 | s of RELAX (SURVIVAL: 8-12) |
| `director.relaxSpawnMult` | `0.4` | 1726 | × spawn rate while relaxing |
| `director.relaxAggressionSpeedup` | `1.5` | 1727 | pushing forward counts the relax down up to 1 + this faster |
| `director.aggressiveSpeed` | `14` | 1728 | m/s that counts as fully aggressive movement |
| `director.hunterSquad` | array[7] | 1729 | at 100% camp |
| `director.markersAt` | `3` | 1730 | enemies left: every one gets a marker |
| `director.camp.radius` | `10` | 1732 | m: staying inside this ... |
| `director.camp.lingerTime` | `8` | 1733 | s ... longer than this ... |
| `director.camp.lowDamage` | `60` | 1734 | ... dealing less than this ... |
| `director.camp.dealtWindow` | `5` | 1735 | ... in this many s, with enemies alive, raises the camp score |
| `director.camp.riseTime` | `6` | 1736 | s from 0 to 100% while camping |
| `director.camp.decayTime` | `2.5` | 1737 | s from 100% to 0 once you stop |
| `director.camp.relocate` | `15` | 1738 | m moved that drops it fast |
| `director.camp.relocateDrop` | `0.6` | 1739 | camp lost on a relocation |
| `director.camp.killChain` | `3` | 1740 | kills ... |
| `director.camp.killChainTime` | `4` | 1741 | ... within this many s ... |
| `director.camp.killChainDrop` | `0.4` | 1742 | ... drop the camp score this much |
| `director.camp.exposedAt` | `0.5` | 1743 | EXPOSED from here (style drain x2, points x0.5, regen paused) |
| `director.rounds.base` | `6` | 1747 | enemies in round N = base + perRound × N |
| `director.rounds.hpFromRound` | `10` | 1749 | HP +hpPerRound per round after this one |
| `director.rounds.bossEvery` | `5` | 1751 | OVERNIGHT2 N5: a boss every 5 rounds (which one: config.bossKit.schedule) |
| `director.rounds.bossFrom` | `5` | 1751 | OVERNIGHT2 N5: a boss every 5 rounds (which one: config.bossKit.schedule) |
| `director.rounds.swarmEvery` | `5` | 1752 | swarm rounds moved off the boss rounds: 8, 13, 18, ... |
| `director.rounds.swarmOffset` | `3` | 1752 | swarm rounds moved off the boss rounds: 8, 13, 18, ... |
| `director.rounds.swarmFrom` | `8` | 1752 | swarm rounds moved off the boss rounds: 8, 13, 18, ... |
| `director.rounds.objectiveFrom` | `6` | 1753 | (above) NIGHT6 B7: objective rounds (survival/objectives.js) from objectiveFrom: HOLD on round % objectiveEvery === holdOffset, |
| `director.rounds.objectiveFrom` | `6` | 1754 | (above) GOLD RUSH on goldOffset (never a boss or swarm round): HOLD 6, 14, 22, 26..., GOLD 7, 11, 19, 27... |
| `director.rounds.paceSeedOffset` | `104729` | 1757 | OVERNIGHT3 P7: the pacing's own stream (director.js), apart from the round's spawns |
| `director.rounds.introduce` | {…} | 1758 | first round of each kind |
| `director.rounds.introduce.husk` | `1` | 1758 | first round of each kind |
| `director.rounds.introduce.swarmer` | `3` | 1758 | first round of each kind |
| `director.rounds.introduce.drone` | `4` | 1758 | first round of each kind |
| `director.rounds.introduce.spitter` | `6` | 1758 | first round of each kind |
| `director.rounds.introduce.charger` | `7` | 1758 | first round of each kind |
| `director.rounds.introduce.lancer` | `8` | 1758 | first round of each kind |
| `director.rounds.introduce.warden` | `9` | 1758 | first round of each kind |
| `survival` | {…} | 1770 | (above) SURVIVAL rounds (S1 v1; S5 adds the director, squads and the 6 + 3N budget) |
| `survival.breakTime` | `10` | 1772 | s between rounds (SURVIVAL §6) |
| `survival.readyFrom` | `3` | 1773 | NIGHT6 B6 (proposal 1): from this round, hold BUY this long with nothing for sale in reach to start the round now |
| `survival.readyHold` | `0.6` | 1773 | NIGHT6 B6 (proposal 1): from this round, hold BUY this long with nothing for sale in reach to start the round now |
| `survival.bannerTime` | `2.2` | 1774 | s the ROUND N banner shows |
| `survival.firstBreakTime` | `3` | 1775 | s before round 1 |
| `health` | `{"max":100,"downTime":1.5,"lowHpFraction":0.3}` | 1778 | (above) §4 Player health + sandbox death |
| `health.max` | `100` | 1780 | HP |
| `health.downTime` | `1.5` | 1781 | s of "DOWN" before the sandbox respawn |
| `health.lowHpFraction` | `0.3` | 1782 | HP bar turns danger orange below this |
| `aura` | {…} | 1785 | (above) Particles (visual only, frame time) |
| `aura` | {…} | 1786 | (above) POWER P5 style aura + lightning on the weapon in hand (colour from the rank, strength from FRESH; all S unless |
| `aura` | {…} | 1787 | (above) POWER gives the number). Arrays are per rank D..SS (DULL, COOL, BOLD, AIRBORNE, SAVAGE, SKYGRIND). |
| `aura.rankArcs` | `[0,0,1,2,4,6]` | 1789 | arcs on the weapon |
| `aura.rankRim` | `[0.35,0.8,1,1.2,1.6,1.9]` | 1790 | fresnel shell brightness |
| `aura.rankMotes` | `[0,0,0,6,9,12]` | 1791 | motes orbiting the weapon |
| `aura.rankSparks` | `[0,0,3,3,4,5]` | 1792 | short sparks a second (BOLD: "a few sparks") |
| `aura.edgeRate` | `[0,0,0,0,8,14]` | 1793 | screen-edge crackles a second (SAVAGE and up) |
| `aura.skyA` | `16723592` | 1794 | SKYGRIND sweeps magenta <-> cyan (rad/s) |
| `aura.skyB` | `1692646` | 1794 | SKYGRIND sweeps magenta <-> cyan (rad/s) |
| `aura.skyRate` | `2.2` | 1794 | SKYGRIND sweeps magenta <-> cyan (rad/s) |
| `aura.prism` | `0.65` | 1795 | SKYGRIND: share of the rim that runs through the hues |
| `aura.blendTime` | `0.3` | 1796 | s: a rank change blends the colour |
| `aura.rimPower` | `1.2` | 1797 | shell: fresnel exponent; m pushed out along the normals (the rim's width) |
| `aura.inflate` | `0.005` | 1797 | shell: fresnel exponent; m pushed out along the normals (the rim's width) |
| `aura.freshFloor` | `0.2` | 1798 | x everything at FRESH 0 (a stale weapon barely sparks) |
| `aura.airReach` | `0.12` | 1800 | m: arcs into the air reach this far off the weapon |
| `aura.pulseSize` | `0.05` | 1801 | the core pulse sprite (COOL and up) |
| `aura.pulseRate` | `6` | 1801 | the core pulse sprite (COOL and up) |
| `aura.pulseIntensity` | `2.2` | 1801 | the core pulse sprite (COOL and up) |
| `aura.moteSize` | `0.006` | 1802 | x the weapon's bounding radius; rad/s |
| `aura.moteRadius` | `1.2` | 1802 | x the weapon's bounding radius; rad/s |
| `aura.moteSpeed` | `2.4` | 1802 | x the weapon's bounding radius; rad/s |
| `aura.edgeDepth` | `0.25` | 1803 | the crackle: m in front of the eye; its length x the edge |
| `aura.edgeWidth` | `0.0012` | 1803 | the crackle: m in front of the eye; its length x the edge |
| `aura.edgeLength` | `0.05` | 1803 | the crackle: m in front of the eye; its length x the edge |
| `aura.leapRange` | `25` | 1804 | SKYGRIND arcs to the nearest visible enemies (visual) |
| `aura.leapInterval` | `0.15` | 1804 | SKYGRIND arcs to the nearest visible enemies (visual) |
| `aura.leapTargets` | `3` | 1804 | SKYGRIND arcs to the nearest visible enemies (visual) |
| `aura.materializeTime` | `0.22` | 1805 | s of the swap dissolve; x the aura colour on its edge |
| `aura.edgeGlow` | `3` | 1805 | s of the swap dissolve; x the aura colour on its edge |
| `aura.static` | {…} | 1806 | (above) The STATIC perk: the aura strikes (ticks, a clear line from your chest, a share of a hit's points per tick). |
| `weaponFx` | {…} | 1813 | (above) POWER P4 new-weapon visuals (all S): the Torch flame, Zapper arcs, shockwave rings, the Tether lash, melee sparks. |
| `weaponFx.flameColors` | `[16761405,16747546,16773312]` | 1816 | cycled per particle |
| `weaponFx.prismLife` | `0.16` | 1820 | OVERNIGHT2 N2: the Prism beam; fire-wall flames a second |
| `weaponFx.prismWidth` | `0.06` | 1820 | OVERNIGHT2 N2: the Prism beam; fire-wall flames a second |
| `weaponFx.fireWallRate` | `60` | 1820 | OVERNIGHT2 N2: the Prism beam; fire-wall flames a second |
| `fx.impact` | {…} | 1825 | (above) OVERNIGHT2 N2 impact shockwave visuals (pools; the quality presets scale their particles) |
| `fx.enemyRimColor` | `16751186` | 1827 | Phase 6 fresnel rim on enemies (orange) |
| `fx.popTime` | `0.12` | 1828 | s, the kill light pop |
| `fx.popDistance` | `9` | 1830 | m |
| `fx.landPuffSpeed` | `12` | 1831 | m/s fall speed for landing puffs |
| `fx.landPuffSpread` | `0.25` | 1832 | × the fall speed as the puff's burst speed |
| `fx.landPuffLife` | `0.35` | 1834 | s |
| `fx.landPuffSize` | `1.4` | 1835 | × particle size |
| `fx.ghostCount` | `4` | 1836 | RIVAL dash afterimages |
| `fx.ghostInterval` | `0.05` | 1837 | s between afterimages while dashing |
| `fx.ghostLife` | `0.25` | 1838 | s |
| `fx.size` | `0.12` | 1844 | m cube edge |
| `fx.gravity` | `18` | 1845 | m/s² on burst particles |
| `fx.deathSpeed` | `7` | 1847 | m/s |
| `fx.deathLife` | `0.6` | 1848 | s |
| `fx.healSpeed` | `9` | 1850 | m/s toward the camera |
| `fx.healLife` | `0.45` | 1851 | s |
| `fx.hitSpeed` | `4` | 1853 | m/s |
| `fx.hitLife` | `0.22` | 1854 | s |
| `fx.speedJitter` | `0.5` | 1855 | burst speed varies down to (1 − this) × speed |
| `fx.lifeJitter` | `0.3` | 1856 | life varies down to (1 − this) × life |
| `fx.homingStartSpeed` | `0.4` | 1857 | × speed of the heal burst's first scatter |
| `fx.homingSteer` | `10` | 1858 | 1/s, how fast heal particles turn toward the camera |
| `fx.sparkCount` | `6` | 1859 | (above) Weapon effects (SPEC 1.5 §4.1, combat/effects.js; all S) |
| `fx.sparkCount` | `6` | 1860 | impact sparks per world hit |
| `fx.sparkSpeed` | `5` | 1861 | m/s |
| `fx.sparkLife` | `0.18` | 1862 | s |
| `fx.sparkSize` | `0.4` | 1863 | × particle size |
| `fx.flashTime` | `0.05` | 1864 | s muzzle flash |
| `fx.flashSize` | `0.28` | 1865 | m quad size at the muzzle |
| `fx.flashForward` | `0.04` | 1866 | m in front of the muzzle |
| `fx.flashLightIntensity` | `6` | 1867 | PointLight peak (always in the scene, 0 when idle) |
| `fx.flashLightDistance` | `7` | 1868 | m |
| `fx.lineCapacity` | `64` | 1869 | pooled line segments (tracers + wall-ride streaks) |
| `fx.tracerTime` | `0.06` | 1870 | s fade |
| `fx.decalCount` | `64` | 1871 | pooled impact decals, oldest reused |
| `fx.decalSize` | `0.13` | 1872 | m |
| `fx.decalLift` | `0.004` | 1873 | m off the surface |
| `fx.surfaceOffset` | `0.02` | 1874 | m off a surface where sparks and wall-ride streaks start |
| `fx.streakRate` | `40` | 1875 | wall-ride streaks per second |
| `fx.streakLife` | `0.25` | 1876 | s |
| `fx.streakLength` | `0.7` | 1877 | m along the ride direction |
| `fx.streakBrightness` | `0.4` | 1878 | additive, 0..1 (faint) |
| `fx.streakHeightMin` | `0.8` | 1879 | m above the feet (around eye level, so they are in view) |
| `fx.streakHeightMax` | `2.2` | 1880 | m above the feet |
| `fx.streakAheadMin` | `0.5` | 1881 | m ahead of the player where a streak appears; it then falls behind as you |
| `fx.streakAheadMax` | `3` | 1882 | pass it (a streak that starts behind the camera is never seen) |
| `fx.slamRingCount` | `3` | 1883 | pooled slam dust rings |
| `fx.slamRingTime` | `0.4` | 1884 | s to grow to the shockwave radius |
| `fx.slamRingStart` | `0.5` | 1885 | m starting radius |
| `fx.slamRingWidth` | `0.45` | 1886 | m ring band width |
| `fx.slamRingOpacity` | `0.6` | 1887 | peak |
| `fx.slamRingLift` | `0.02` | 1888 | m above the floor |
| `fx.flashShapes` | {…} | 1890 | (above) NIGHT4 G2: the player's muzzle flash shape per weapon: [spikes (0 = round), spike sharpness, ring, size x] (the |
| `fx.flashShapes` | {…} | 1891 | (above) default is the old six-spike star; RIVAL keeps it), and the impact flare that takes one spark's place where a |
| `fx.flashShapes` | {…} | 1892 | (above) hitscan shot lands, in the weapon's family colour (the same particle count as before: the sim's dice don't move). |
| `fx.killConfirm` | {…} | 1899 | (above) NIGHT5 F4: the kill confirm (fx/killConfirm.js): a thin ring where a kill lands, in the weapon's family colour, from |
| `fx.killConfirm` | {…} | 1900 | (above) `start` x its size to its size over `life` s (x the tier, life capped at x1.4); tiers multiply (a headshot, a melee |
| `fx.killConfirm` | {…} | 1901 | (above) kill, a multikill inside style.multikillWindow). Brightness x glow x FLASH INTENSITY, capped at maxAlpha. |
| `fx.flashFrames` | `2` | 1903 | (above) NIGHT5 F4: the muzzle flash shows for this many render frames at most (1-2 at any frame rate), the second at x dim. |
| `viewmodel` | {…} | 1907 | (above) §4.1 Viewmodel (SPEC 1.5; all S). Camera space: x right, y up, -z forward; meters and degrees. |
| `viewmodel.fovDeg` | `54` | 1909 | fixed vertical FOV, never touched by gameplay FOV effects (NIGHT5 F3: was 60; the guns read small) |
| `viewmodel.near` | `0.02` | 1910 | m |
| `viewmodel.far` | `10` | 1911 | m |
| `viewmodel.dirX` | `-0.4` | 1914 | direction toward the light, normalized at use |
| `viewmodel.holdX` | `0.135` | 1917 | (above) Hold pose: the gun low-right, yawed slightly inward |
| `viewmodel.holdX` | `0.135` | 1918 | NIGHT5 F3: was 0.14, -0.15 (a touch in and up: presence) |
| `viewmodel.ads` | {…} | 1923 | (above) POLISH V2: the ADS pose (camera space, blended in by the zoom): the gun centred under the crosshair with a slight |
| `viewmodel.ads` | {…} | 1924 | (above) tilt; per weapon when its shape needs it. |
| `viewmodel.ads.rail` | {…} | 1928 | lower: the scope takes the view |
| `viewmodel.ads.rail.x` | `0.04` | 1928 | lower: the scope takes the view |
| `viewmodel.ads.rail.y` | `-0.13` | 1928 | lower: the scope takes the view |
| `viewmodel.ads.rail.z` | `-0.24` | 1928 | lower: the scope takes the view |
| `viewmodel.ads.rail.yawDeg` | `1` | 1928 | lower: the scope takes the view |
| `viewmodel.ads.rail.pitchDeg` | `0` | 1928 | lower: the scope takes the view |
| `viewmodel.ads.rail.rollDeg` | `-2` | 1928 | lower: the scope takes the view |
| `viewmodel.breathAmp` | `0.004` | 1930 | m, idle breathing sine |
| `viewmodel.breathPeriod` | `3.5` | 1931 | s |
| `viewmodel.bobAmpX` | `0.008` | 1932 | m at walk speed |
| `viewmodel.bobAmpY` | `0.01` | 1933 | m at walk speed |
| `viewmodel.bobCycleDistance` | `4.4` | 1934 | m of travel per bob cycle (two steps) |
| `viewmodel.bobMaxScale` | `1.5` | 1935 | x walk-speed amplitude cap |
| `viewmodel.bobEaseRate` | `8` | 1936 | 1/s |
| `viewmodel.swayGain` | `1.2` | 1937 | m of sway velocity per rad of look delta |
| `viewmodel.swayVelDamp` | `9` | 1938 | 1/s |
| `viewmodel.swayReturnRate` | `12` | 1939 | 1/s |
| `viewmodel.swayMaxX` | `0.04` | 1940 | m |
| `viewmodel.swayMaxY` | `0.03` | 1941 | m |
| `viewmodel.swayRollDegPerM` | `90` | 1942 | roll per m of sideways sway (wrist articulation) |
| `viewmodel.strafeRollDeg` | `3` | 1943 | at walk-speed strafing |
| `viewmodel.poseEaseRate` | `10` | 1944 | 1/s for strafe roll and the state poses |
| `viewmodel.slideDipY` | `-0.04` | 1945 | m |
| `viewmodel.slideRollDeg` | `8` | 1946 | inward |
| `viewmodel.wallTiltDeg` | `7` | 1947 | away from the wall |
| `viewmodel.airLiftY` | `0.012` | 1948 | m |
| `viewmodel.dashPullZ` | `0.06` | 1949 | m back toward the camera |
| `viewmodel.dashPullAttack` | `0.04` | 1950 | s |
| `viewmodel.dashPullRecover` | `0.2` | 1951 | s |
| `viewmodel.landDipScale` | `0.35` | 1952 | x the camera landing dip |
| `viewmodel.recoil` | {…} | 1953 | (above) NIGHT5 F2: the gun's recoil per shot, peaks of a closed-form spring per weight class (feel.punch.weapons[].cls): |
| `viewmodel.recoil` | {…} | 1954 | (above) the muzzle up (deg), back toward you and down (m: mostly down and back, away from the reticle), a roll (deg). |
| `viewmodel.recoilSprings` | {…} | 1970 | (above) NIGHT5 F2: the gun's spring per weight class (looser than the camera's: the gun follows through), its look lag |
| `viewmodel.recoilSprings` | {…} | 1971 | (above) (x swayGain, x swayReturnRate: heavy guns swing wider and come back slower), and the settle as a swap's raise ends |
| `viewmodel.recoilSprings` | {…} | 1972 | (above) (the gun lands in the hands: muzzle down deg, drop m). |
| `viewmodel.recoilSnap` | `[0.01,0.0001,0.0001,0.01]` | 1978 | deg, m, m, deg: a recoil spring under these is gone |
| `viewmodel.catchKick` | `[-5,0.035,0.012,4]` | 1979 | (above) NIGHT5 F4 (God of War): the DISCUS catch lands in the hand with an overshoot (on the gun's spring: muzzle deg, back m, |
| `viewmodel.catchKick` | `[-5,0.035,0.012,4]` | 1980 | (above) drop m, roll deg), and a melee hit with momentum (over feel.momentumHitstopAt) drives the weapon into the target on a |
| `viewmodel.catchKick` | `[-5,0.035,0.012,4]` | 1981 | (above) spring that runs on frame time, so it plays through the sim's hitstop freeze and holds there. |
| `viewmodel.meleeClass` | {…} | 1984 | guns: feel.punch |
| `viewmodel.meleeClass.gauntlet` | `"light"` | 1984 | guns: feel.punch |
| `viewmodel.meleeClass.flicker` | `"light"` | 1984 | guns: feel.punch |
| `viewmodel.meleeClass.sunder` | `"medium"` | 1984 | guns: feel.punch |
| `viewmodel.meleeClass.tether` | `"medium"` | 1984 | guns: feel.punch |
| `viewmodel.meleeClass.thumper` | `"heavy"` | 1984 | guns: feel.punch |
| `viewmodel.kitOffsets` | {…} | 1985 | (above) POWER P4 blocky-kit weapons: offset from the hold pose (m), scale, and a hold turn (deg) for the melee. |
| `viewmodel.swing` | {…} | 2005 | (above) POWER P4 melee swing poses (added to the hold pose while the swing plays; m, deg, s). |
| `viewmodel.swing.gauntlet` | `{"out":0.05,"hold":0.04,"time":0.3,"z":-0.23,"pitchDeg":8}` | 2007 | NIGHT5 F4: more follow-through (was z -0.2, pitch 6) |
| `viewmodel.swing.gauntlet.out` | `0.05` | 2007 | NIGHT5 F4: more follow-through (was z -0.2, pitch 6) |
| `viewmodel.swing.gauntlet.hold` | `0.04` | 2007 | NIGHT5 F4: more follow-through (was z -0.2, pitch 6) |
| `viewmodel.swing.gauntlet.time` | `0.3` | 2007 | NIGHT5 F4: more follow-through (was z -0.2, pitch 6) |
| `viewmodel.swing.gauntlet.z` | `-0.23` | 2007 | NIGHT5 F4: more follow-through (was z -0.2, pitch 6) |
| `viewmodel.swing.gauntlet.pitchDeg` | `8` | 2007 | NIGHT5 F4: more follow-through (was z -0.2, pitch 6) |
| `viewmodel.swing.sunder` | {…} | 2008 | NIGHT5 F4: wider arcs (yaw 80, roll 32, pitch 12, sweep 130) |
| `viewmodel.swing.sunder.time` | `0.26` | 2008 | NIGHT5 F4: wider arcs (yaw 80, roll 32, pitch 12, sweep 130) |
| `viewmodel.swing.sunder.yawDeg` | `92` | 2008 | NIGHT5 F4: wider arcs (yaw 80, roll 32, pitch 12, sweep 130) |
| `viewmodel.swing.sunder.rollDeg` | `36` | 2008 | NIGHT5 F4: wider arcs (yaw 80, roll 32, pitch 12, sweep 130) |
| `viewmodel.swing.sunder.pitchDeg` | `14` | 2008 | NIGHT5 F4: wider arcs (yaw 80, roll 32, pitch 12, sweep 130) |
| `viewmodel.swing.sunder.sweepYawDeg` | `148` | 2008 | NIGHT5 F4: wider arcs (yaw 80, roll 32, pitch 12, sweep 130) |
| `viewmodel.swing.sunder.sweepLift` | `0.05` | 2008 | NIGHT5 F4: wider arcs (yaw 80, roll 32, pitch 12, sweep 130) |
| `viewmodel.swing.sunder.z` | `-0.08` | 2008 | NIGHT5 F4: wider arcs (yaw 80, roll 32, pitch 12, sweep 130) |
| `viewmodel.swing.thumper` | {…} | 2010 | NIGHT5 F4: impact pitch 50 -> 58, drop 0.06 -> 0.075 |
| `viewmodel.swing.thumper.raiseRate` | `14` | 2010 | NIGHT5 F4: impact pitch 50 -> 58, drop 0.06 -> 0.075 |
| `viewmodel.swing.thumper.windupPitchDeg` | `-38` | 2010 | NIGHT5 F4: impact pitch 50 -> 58, drop 0.06 -> 0.075 |
| `viewmodel.swing.thumper.windupLift` | `0.07` | 2010 | NIGHT5 F4: impact pitch 50 -> 58, drop 0.06 -> 0.075 |
| `viewmodel.swing.thumper.slamTime` | `0.06` | 2010 | NIGHT5 F4: impact pitch 50 -> 58, drop 0.06 -> 0.075 |
| `viewmodel.swing.thumper.slamHold` | `0.06` | 2010 | NIGHT5 F4: impact pitch 50 -> 58, drop 0.06 -> 0.075 |
| `viewmodel.swing.thumper.impactTime` | `0.4` | 2010 | NIGHT5 F4: impact pitch 50 -> 58, drop 0.06 -> 0.075 |
| `viewmodel.swing.thumper.impactPitchDeg` | `58` | 2010 | NIGHT5 F4: impact pitch 50 -> 58, drop 0.06 -> 0.075 |
| `viewmodel.swing.thumper.impactDrop` | `0.075` | 2010 | NIGHT5 F4: impact pitch 50 -> 58, drop 0.06 -> 0.075 |
| `viewmodel.swing.thumper.bigMult` | `1.25` | 2010 | NIGHT5 F4: impact pitch 50 -> 58, drop 0.06 -> 0.075 |
| `viewmodel.glbRigs` | `[]` | 2013 | (above) Weapons drawn from a GLB in public/models (the rest use the blocky kit). NIGHT4 G1: none; the Kestrel's procedural |
| `viewmodel.glbRigs` | `[]` | 2014 | (above) model replaced the CC0 GLB (1586 triangles, over the 1050 cap; see docs/GUNS.md). Add 'revolver' to bring it back. |
| `viewmodel.revolverGem` | `{"r":0.011,"x":0,"y":0.075,"z":-0.06}` | 2016 | the Kestrel's core gem (m, rig space) |
| `viewmodel.revolverGem.r` | `0.011` | 2016 | the Kestrel's core gem (m, rig space) |
| `viewmodel.revolverGem.x` | `0` | 2016 | the Kestrel's core gem (m, rig space) |
| `viewmodel.revolverGem.y` | `0.075` | 2016 | the Kestrel's core gem (m, rig space) |
| `viewmodel.revolverGem.z` | `-0.06` | 2016 | the Kestrel's core gem (m, rig space) |
| `viewmodel.gemFreshMin` | `0.3` | 2017 | x gem brightness at FRESH 0 (it dims as a weapon goes stale) |
| `viewmodel.gemFlash` | `1.5` | 2018 | x extra brightness when it fires / swings; s |
| `viewmodel.gemFlashTime` | `0.12` | 2018 | x extra brightness when it fires / swings; s |
| `viewmodel.drumTurnRate` | `18` | 2019 | the Bouncer drum turns to its next chamber (ease rate) |
| `viewmodel.states` | {…} | 2020 | (above) NIGHT4 G2: guns show what they're doing (visual only, frame time). glint: a cooldown at least glintMinCooldown s |
| `viewmodel.states` | {…} | 2021 | (above) long flashes the gem when it is done (x glintGain, over glintTime s, x the FLASH setting); pipOff: a spent pip's |
| `viewmodel.states` | {…} | 2022 | (above) brightness; the STITCHER's drum turns drumStepDeg per nail; torchHeatGlow: x the Torch's nozzle + pilot at full |
| `viewmodel.states` | {…} | 2023 | (above) heat; prismReadyMin: x the crystal right after a shot (it fills back as it reloads). |
| `viewmodel.wallPull` | {…} | 2025 | (above) NIGHT4 G2: with a wall within `start` m ahead of the eye the gun lowers, tips down and comes back, fully at `full` m |
| `viewmodel.wallPull` | {…} | 2026 | (above) (m, m, deg; eased at `rate`). Visual only: the eye ray reads the world, nothing is written. |
| `viewmodel.heatColor` | `16767120` | 2028 | (above) Nailgun + Torch heat glow at full heat (HDR x heatGlow). NIGHT4 G2: white-hot amber; it was 0xff5a1c, the enemy |
| `viewmodel.heatColor` | `16767120` | 2029 | (above) red-orange, which player guns never use. |
| `viewmodel.chargeGlow` | `4` | 2032 | rail charge core brightness at full charge |
| `viewmodel.cylinderStepDeg` | `60` | 2034 | per revolver shot |
| `viewmodel.cylinderTurnTime` | `0.1` | 2035 | s |
| `viewmodel.pumpTravel` | `0.06` | 2036 | m the fore-end slides back |
| `viewmodel.pumpDuration` | `0.3` | 2037 | s back + forward |
| `viewmodel.pumpTiltDeg` | `5` | 2038 | the gun rolls in while pumping |
| `viewmodel.swapDropY` | `0.3` | 2039 | m the gun drops when fully lowered |
| `viewmodel.swapPitchDeg` | `35` | 2040 | the muzzle tips down this much when fully lowered |
| `viewmodel.respawnRaiseTime` | `0.3` | 2041 | s the gun rises in after a respawn |
| `viewmodel.hudAttach` | `0.3` | 2042 | Attached HUD style follows sway + bob at this strength |
| `viewmodel.armRest` | {…} | 2043 | (above) Left arm (procedural). Poses are the fist position + rotation in camera space. |
| `viewmodel.armRest` | {…} | 2044 | just off-screen |
| `viewmodel.armRest.x` | `-0.24` | 2044 | just off-screen |
| `viewmodel.armRest.y` | `-0.42` | 2044 | just off-screen |
| `viewmodel.armRest.z` | `-0.32` | 2044 | just off-screen |
| `viewmodel.armRest.pitchDeg` | `40` | 2044 | just off-screen |
| `viewmodel.armRest.yawDeg` | `-25` | 2044 | just off-screen |
| `viewmodel.armRest.rollDeg` | `0` | 2044 | just off-screen |
| `viewmodel.punchOutTime` | `0.06` | 2046 | s to full extension |
| `viewmodel.punchHoldTime` | `0.04` | 2047 | s at full extension, then back over the rest of the cycle |
| `viewmodel.guns` | {…} | 2048 | (above) Guns: Quaternius GLBs (CC0), recolored by source material name, one part split out by a box in |
| `viewmodel.guns` | {…} | 2049 | (above) model units (after the node transform, barrel along -z, before scaling to `length`). |
| `viewmodel.guns` | {…} | 2050 | (above) grip / muzzle are fractions of the model's bounding box (0 = min, 1 = max; z 0 = muzzle end). |
| `viewmodel.guns.revolver.length` | `0.28` | 2054 | m, muzzle to grip end |
| `viewmodel.guns.revolver.offset` | `{"x":0,"y":0,"z":0}` | 2055 | m added to the hold pose for this gun |
| `viewmodel.guns.revolver.offset.x` | `0` | 2055 | m added to the hold pose for this gun |
| `viewmodel.guns.revolver.offset.y` | `0` | 2055 | m added to the hold pose for this gun |
| `viewmodel.guns.revolver.offset.z` | `0` | 2055 | m added to the hold pose for this gun |
| `viewmodel.guns.shotgun.length` | `0.9` | 2063 | m |
| `viewmodel.guns.shotgun.offset` | `{"x":0.03,"y":-0.01,"z":-0.08}` | 2064 | m, further out so the stock sits off-screen right |
| `viewmodel.guns.shotgun.offset.x` | `0.03` | 2064 | m, further out so the stock sits off-screen right |
| `viewmodel.guns.shotgun.offset.y` | `-0.01` | 2064 | m, further out so the stock sits off-screen right |
| `viewmodel.guns.shotgun.offset.z` | `-0.08` | 2064 | m, further out so the stock sits off-screen right |
| `viewmodel.cylinderSegments` | `12` | 2071 | procedural cylinders |
| `viewmodel.afterimages` | {…} | 2072 | (above) Procedural parts: [role, shape ('box' \| 'cyl' along z), sizeX, sizeY, sizeZ, x, y, z, part?] |
| `viewmodel.afterimages` | {…} | 2073 | (above) in m around the grip. `fallback` is used only if a GLB can't load; `arm` is always used. |
| `viewmodel.afterimages` | {…} | 2074 | (above) OVERNIGHT2 N8: melee afterimages: copies `spacing` samples apart (a sample every `every` s), for `life` s. |
| `viewmodel.fallback.nailgun` | array[5] | 2090 | (above) SURVIVAL S3: procedural only (no GLB): the nailgun, rocket launcher and railcannon. |
| `viewmodel.arm.0` | `["trim","box",0.075,0.07,0.085,0,0,0]` | 2115 | fist |
| `viewmodel.arm.1` | `["trim","box",0.024,0.026,0.05,0.042,0.018,-0.01]` | 2116 | thumb |
| `viewmodel.arm.2` | `["accent","box",0.079,0.074,0.02,0,0,0.055]` | 2117 | cyan wrist band |
| `viewmodel.arm.3` | `["body","box",0.078,0.072,0.32,0,-0.004,0.225]` | 2118 | sleeve |
| `settings` | {…} | 2122 | (above) §8 Player settings defaults (saved to localStorage; debug GUI "settings" folder until M4) |
| `settings.fovDeg` | `74` | 2124 | base vertical FOV |
| `settings.cameraRoll` | `true` | 2125 | strafe + wall-ride roll |
| `settings.effectsIntensity` | `1` | 2126 | 0–2 (0–200%) for dip, roll, FOV effects and speed lines |
| `settings.hudStyle` | `"attached"` | 2127 | 'classic' (fixed panel) or 'attached' (follows the viewmodel sway), §8.1 |
| `settings.sensitivity` | `0.0014` | 2128 | (above) SPEC 2.0 §8 settings screen |
| `settings.sensitivity` | `0.0014` | 2129 | rad of look per pixel (the settings copy of mouse.sensitivity, OVERNIGHT2 N1) |
| `settings.reducedMotion` | `false` | 2131 | no camera roll, shake, chroma tint or speed lines |
| `settings.filmGrain` | `true` | 2132 | POLISH V4: the light film grain (Medium and High) |
| `settings.uiScale` | `100` | 2133 | POLISH V5: % every HUD cluster and menu panel scales by (80-140) |
| `settings.hurtVignette` | `true` | 2134 | POLISH V4: the screen edges darken and the colour drains at low health |
| `settings.crosshairColor` | `"cyan"` | 2135 | ui.crosshairColors key |
| `settings.quality` | `"auto"` | 2136 | 'auto' \| 'low' \| 'medium' \| 'high' (Phase 7) |
| `settings.renderScale` | `1` | 2137 | × the pixel ratio (the dynamic resolution works under it) |
| `settings.duelRounds` | `3` | 2139 | kills to win a Duel (3 or 5) |
| `settings.damageNumbers` | `false` | 2140 | floating damage numbers on your hits (Phase 8) |
| `settings.showPrices` | `true` | 2141 | SURVIVAL S8: prices in the buy prompt and on the floating gate / wall / perk signs |
| `settings.aimAssist` | `"snap"` | 2142 | OVERNIGHT2 N1 SNAP MODE: 'off' \| 'assist' (the POWER P2 bend) \| 'snap' \| 'lock' (HARD LOCK) |
| `settings.snapStrength` | `60` | 2143 | 0..100: SNAP STRENGTH (cone, snap time, stick time, breakaway, payout; ASSIST: x strength / 60) |
| `settings.xray` | `true` | 2144 | enemies through walls |
| `settings.auraIntensity` | `1` | 2145 | POWER P5: 0 (off, for flashing sensitivity) .. 1: the style aura on the weapon in hand |
| `settings.toneMapping` | `"aces"` | 2146 | POLISH V4: 'aces' \| 'agx' \| 'neutral' (picked by screenshot: DECISIONS.md) |
| `settings.adsMode` | `"hold"` | 2147 | POLISH V2: right mouse zooms while held ('hold') or toggles the zoom ('toggle') |
| `settings.adsSensitivity` | `1` | 2148 | POLISH V2: x the zoomed look (on top of the FOV-matched scaling) |
| `settings.slideMode` | `"hold"` | 2149 | (above) OVERNIGHT3 P2: hold or toggle for the held actions (input/holdToggle.js). 'hold' is the game as it was. |
| `settings.slideMode` | `"hold"` | 2150 | 'hold' \| 'toggle': SLIDE (a press slides until the next press or a jump) |
| `settings.arclineMode` | `"hold"` | 2151 | 'hold' \| 'toggle': the ARCLINE swing (a press hooks, the next press lets go) |
| `settings.gunFinish` | `"factory"` | 2152 | NIGHT4 F1: a key of weapons.finishes.list (locked ones draw as FACTORY) |
| `settings.flashIntensity` | `1` | 2153 | OVERNIGHT3 P2: 0..1 x the full-screen flashes (the parry flash, the rank-up glow); 1 = as before. NIGHT4 G2: and the gem glint |
| `settings.fpsCap` | `0` | 2154 | OVERNIGHT3 P2: frames per second (0 = off: every display refresh, as before); core/framePace.js |
| `settings.fullscreen` | `false` | 2155 | OVERNIGHT3 P2: mirrors the browser's fullscreen (a toggle needs a click; never re-entered at boot) |
| `settings.shapeCues` | `true` | 2156 | OVERNIGHT3 P2: a ring round parryable orbs (a shape cue next to the cyan) |
| `settings.damageDirection` | `true` | 2157 | OVERNIGHT3 P4: a red-orange arc toward whatever hit you |
| `settings.showGhost` | `true` | 2158 | OVERNIGHT3 P6: your best run of a trial plays alongside you |
| `settings.padDeadzoneInner` | `0.15` | 2159 | (above) OVERNIGHT3 P3: the gamepad (GAMEPAD tab). The mouse settings above stay the mouse's. |
| `settings.padDeadzoneInner` | `0.15` | 2160 | stick travel (0..1) read as zero |
| `settings.padDeadzoneOuter` | `0.95` | 2161 | stick travel read as full |
| `settings.padCurve` | `"linear"` | 2162 | 'linear' \| 'precise' (squared: finer aim near the centre) |
| `settings.padSensX` | `1` | 2163 | x the turn rates (config.gamepad) |
| `settings.padSensY` | `1` | 2163 | x the turn rates (config.gamepad) |
| `settings.padVibration` | `false` | 2165 | light rumble on a hit taken, a kill, a parry (no pad rumbled before) |
| `settings.padAimAssist` | `"snap"` | 2166 | the pad's SNAP MODE ('off' \| 'assist' \| 'snap' \| 'lock'); SNAP like the mouse's default |
| `ui` | {…} | 2169 | (above) §8 Menus + flow (SPEC 2.0 Phase 3; S) |
| `ui.theme` | {…} | 2171 | (above) POLISH V5 theme (ui/theme.js): the text colour, the gold, the panel, the type scale (px; nothing under 16 at 100%). |
| `ui.theme.scaleBaseHeight` | `1080` | 2174 | px: the UI draws at UI SCALE up to this window height, and grows with it above (1440p: x1.33) |
| `ui.modes` | {…} | 2178 | the title's cards |
| `ui.modes.trials` | {…} | 2185 | OVERNIGHT3 P5 |
| `ui.modes.trials.name` | `"TRIALS"` | 2185 | OVERNIGHT3 P5 |
| `ui.modes.trials.desc` | `"Beat the clock on a short course. Six courses, four medals."` | 2185 | OVERNIGHT3 P5 |
| `ui.controlGroups` | array[5] | 2191 | (above) CONTROLS (generated from the live bindings): rebindable actions by group, then the mouse and fixed keys, then |
| `ui.controlGroups` | array[5] | 2192 | (above) techniques ({action} = that action's key). |
| `ui.specialHints` | {…} | 2227 | (above) The weapon block's SPECIAL hint (the tooltip has the long version). |
| `ui.loadingFrames` | `3` | 2232 | the loading screen fades (s) after the world's first frames and the fonts |
| `ui.loadingFade` | `0.4` | 2232 | the loading screen fades (s) after the world's first frames and the fonts |
| `ui.firstRunTrial` | `"slideLine"` | 2233 | NIGHT6 B6 (proposal 3): the first run's TUTORIAL continues into this trial, then Survival |
| `ui.notices.firstRunTrial` | `"NEXT: SLIDE LINE · SLIDE, THEN JUMP OUT OF THE SLIDE: YOU KEEP THE SPEED"` | 2235 | NIGHT6 B6 |
| `ui.notices.mouseEvery` | `4` | 2241 | s between two of those notices |
| `ui.notices.fullscreen` | `"THE BROWSER REFUSED FULLSCREEN: TRY F11"` | 2242 | OVERNIGHT3 P2 |
| `ui.notices.padOn` | `"GAMEPAD CONNECTED · SETTINGS > GAMEPAD"` | 2243 | OVERNIGHT3 P3 |
| `ui.fadeTime` | `0.3` | 2248 | s, a full fade out + in between screens |
| `ui.recap` | {…} | 2249 | (above) OVERNIGHT3 P4: the death recap on the DOWN screen (modes/deathRecap.js): the last window s of damage, by source. |
| `ui.recap.window` | `3` | 2251 | s |
| `ui.hitDirection` | {…} | 2258 | (above) OVERNIGHT3 P4: the damage-direction arcs (ui/hitDirection.js), red-orange round the crosshair. |
| `ui.hitDirection` | {…} | 2259 | -, s, px, px, deg, s |
| `ui.hitDirection.max` | `4` | 2259 | -, s, px, px, deg, s |
| `ui.hitDirection.life` | `0.7` | 2259 | -, s, px, px, deg, s |
| `ui.hitDirection.radius` | `150` | 2259 | -, s, px, px, deg, s |
| `ui.hitDirection.width` | `5` | 2259 | -, s, px, px, deg, s |
| `ui.hitDirection.arcDeg` | `50` | 2259 | -, s, px, px, deg, s |
| `ui.hitDirection.fadeFrom` | `0.35` | 2259 | -, s, px, px, deg, s |
| `ui.hintTime` | `3.5` | 2260 | s a contextual hint stays up |
| `ui.hintDelay` | `1.5` | 2261 | s after a wave starts before its hint |
| `ui.boardSize` | `10` | 2262 | local leaderboard entries |
| `ui.breakdownParts` | `6` | 2263 | style breakdown bar segments |
| `ui.fpsRefresh` | `0.25` | 2266 | s between FPS readout updates |
| `ui.damageNumberPool` | `24` | 2267 | live damage numbers at once (Phase 8) |
| `ui.damageNumberTime` | `0.7` | 2268 | s each number lives |
| `ui.damageNumberRise` | `0.9` | 2269 | m it rises over its life |
| `ui.lockGraceTime` | `0.5` | 2270 | s playing without pointer lock before the pause menu shows |
| `ui.titleOrbitRadius` | `78` | 2271 | m, the title camera orbit around the Spire |
| `ui.titleOrbitHeight` | `34` | 2272 | m |
| `ui.titleOrbitSpeed` | `0.06` | 2273 | rad/s |
| `ui.titleTargetHeight` | `6` | 2274 | m, where the title camera looks |
| `ui.titleDrones` | `4` | 2275 | decorative Drones drifting behind the title |
| `ui.signRange` | `40` | 2276 | m, Training signs farther than this fade out |
| `ui.settings.26.key` | `"auraIntensity"` | 2306 | POWER P7: flashing effects |
| `ui.settings.26.tab` | `"access"` | 2306 | POWER P7: flashing effects |
| `ui.settings.26.label` | `"STYLE AURA (0 = OFF)"` | 2306 | POWER P7: flashing effects |
| `ui.settings.26.type` | `"range"` | 2306 | POWER P7: flashing effects |
| `ui.settings.26.min` | `0` | 2306 | POWER P7: flashing effects |
| `ui.settings.26.max` | `1` | 2306 | POWER P7: flashing effects |
| `ui.settings.26.step` | `0.05` | 2306 | POWER P7: flashing effects |
| `sound` | {…} | 2324 | (above) §3.5 Sound driven by movement |
| `sound.windMinSpeed` | `8` | 2326 | m/s, wind volume = clamp((speed − min) / (max − min)) |
| `sound.windMaxSpeed` | `24` | 2327 | m/s |
| `sound.landMedSpeed` | `12` | 2328 | m/s fall speed for land_med, below is land_soft (S) |
| `sound.landHardSpeed` | `24` | 2329 | m/s fall speed for land_hard (S) |
| `sound.footstepDistance` | `2.2` | 2330 | m travelled per footstep, so rate follows speed (S) |
| `colors` | {…} | 2333 | (above) §9 Palette |
| `colors.sky` | `12568268` | 2335 | clear colour behind the sky dome (Phase 6: the dome covers it) |
| `colors.skyZenith` | `1051180` | 2336 | (above) Phase 6 dusk (SPEC 2.0 §9) |
| `colors.skyZenith` | `1051180` | 2337 | (above) POLISH V4 value structure: a dark indigo zenith over a hot horizon; mid-value slate floors (darker than the sky's |
| `colors.skyZenith` | `1051180` | 2338 | (above) horizon, clearly apart from the walls); darker, richer plum walls; bright emissives on top. |
| `colors.fog` | `13199439` | 2343 | the flat fog colour (materials that don't read the sky function): the horizon, a touch darker |
| `colors.pillarTop` | `8879506` | 2346 | floors and tops: a mid mauve slate |
| `colors.pillarSide` | `5393018` | 2347 | walls: a deep plum (the sky bounce, world.wallSky, warms the faces toward the sun) |
| `colors.enemy` | `16730652` | 2348 | enemies + RIVAL, danger only |
| `colors.usable` | `1692646` | 2349 | player-usable: jump pads, ride strips, pickups, heal particles |
| `colors.lightSky` | `10133734` | 2350 | hemisphere light, from above: the cool sky fill (POLISH V4) |
| `colors.lightGround` | `10775650` | 2351 | hemisphere light, from below: a warm bounce |
| `colors.lightSun` | `16760458` | 2352 | the key: a low warm sun |
| `colors.lightRim` | `10514687` | 2353 | POLISH V4: a violet rim from the far side (kept low: a directional rim lights whole faces) |
| `colors.vmBody` | `3816000` | 2354 | (above) SPEC 1.5 viewmodel + effects (S) |
| `colors.vmBody` | `3816000` | 2355 | viewmodel body |
| `colors.vmTrim` | `9407104` | 2356 | viewmodel trim (the pillar-side grey); accents use `usable` |
| `colors.vmLightSky` | `16777215` | 2357 | viewmodel hemisphere light, from above |
| `colors.vmLightGround` | `5921376` | 2358 | viewmodel hemisphere light, from below |
| `colors.vmLightSun` | `16774890` | 2359 | viewmodel directional light |
| `colors.tracer` | `14089215` | 2361 | cyan-white |
| `fog` | `{"near":45,"far":190,"heightFade":30,"heightMin":0.55}` | 2367 | (above) §9 Linear fog (Phase 6: horizon-coloured, fades with height in the world material) |
| `fog.near` | `45` | 2369 | m |
| `fog.far` | `190` | 2370 | m |
| `fog.heightFade` | `30` | 2371 | m above the floor where fog has thinned to heightMin |
| `fog.heightMin` | `0.55` | 2372 | share of the fog left high up (the height-fog feel) |
| `world` | {…} | 2375 | (above) Phase 6 world material + neon (SPEC 2.0 §9) |
| `world.gridMinor` | `0.05` | 2377 | darkening of the 1 m grid lines on tops |
| `world.gridMajor` | `0.11` | 2378 | darkening of the 4 m lines |
| `world.sideLow` | `0.8` | 2379 | side brightness at the floor ... |
| `world.sideSpan` | `20` | 2380 | ... rising to full over this height (m) |
| `world.neonBase` | `1.25` | 2381 | neon intensity at an empty DULL (just under the bloom threshold) |
| `world.neonPeak` | `3.4` | 2382 | neon intensity at full style energy |
| `world.padDisc` | `0.9` | 2383 | pad disc brightness (the ring is neon) |
| `world.rimWidth` | `0.09` | 2384 | m, neon top-edge rims |
| `world.rimLift` | `0.012` | 2385 | m the rim sits proud of the stone |
| `world.rimKinds` | `["spire","ledge","bridge","island","cover","slab","wall"]` | 2386 | box kinds with rims |
| `world.ringPulse` | `0.12` | 2387 | pad ring breathing (scale) |
| `world.ringRate` | `3` | 2388 | rad/s |
| `world.trim` | {…} | 2389 | (above) POLISH V4 trim sheet (world/worldMaterial.js): floor plates + lane markings, wall seams, grime, edge bevels. |
| `world.sheen` | `0.32` | 2391 | the floor's fake sky reflection (x the quality preset's sheen) |
| `world.sheenPower` | `3.2` | 2391 | the floor's fake sky reflection (x the quality preset's sheen) |
| `world.wallSky` | `0.38` | 2392 | walls take this much of the low sky they face (world/worldMaterial.js: faces part by direction) |
| `sky` | {…} | 2395 | (above) Phase 6 sky (SPEC 2.0 §9) |
| `sky.domeRadius` | `380` | 2397 | m, follows the camera (inside camera.far) |
| `sky.sunAzimuthDeg` | `35` | 2399 | from -z toward +x |
| `sky.sunSize` | `1.4` | 2400 | larger = a bigger disc |
| `sky.stars` | `0.9` | 2401 | POLISH V4: faint stars up high, thin cloud bands across the mid sky |
| `sky.bands` | `0.35` | 2401 | POLISH V4: faint stars up high, thin cloud bands across the mid sky |
| `sky.warmGlow` | `0.5` | 2402 | horizon glow at full rank warmth |
| `sky.cloudHeight` | `-26` | 2403 | m (the kill plane is at -30) |
| `sky.cloudSize` | `900` | 2404 | m, follows the camera |
| `sky.cloudScale` | `0.018` | 2405 | noise frequency per m |
| `sky.cloudWindX` | `0.05` | 2406 | noise units per s |
| `sky.cloudFadeStart` | `160` | 2408 | m |
| `sky.cloudFadeEnd` | `400` | 2409 | m |
| `quality` | {…} | 2412 | (above) Phase 7 quality presets + dynamic resolution (SPEC 2.0 performance) |
| `quality.presets.low` | {…} | 2415 | (above) aura (POWER P5): x arc count, the fresnel shell on / off, x motes, x screen-edge crackle |
| `quality.presets.low` | {…} | 2416 | (above) impact / telegraph (OVERNIGHT2 N7): x shockwave particles, x telegraph decal segments |
| `quality.presets.low` | {…} | 2417 | (above) POLISH V4: ao (the baked AO's strength), motes (x config.motes.max), sheen (x the floor sheen), lights (point |
| `quality.presets.low` | {…} | 2418 | (above) lights in the pool), grain (film grain opacity); the shadows are config.lights.shadows[preset]. |
| `quality.presets.low` | {…} | 2419 | (above) POLISH V7: Low keeps the fog, the grade, the vignette and the blob shadows; no AO, no floor reflection (sheen). |
| `quality.maxDeviceRatio` | `2` | 2424 | HIGH uses min(devicePixelRatio, this) |
| `quality.benchWarmup` | `0.6` | 2425 | s on the title before the AUTO benchmark samples |
| `quality.benchTime` | `3` | 2426 | s of samples |
| `quality.autoHighMs` | `12.5` | 2427 | p95 frame time at or under this picks HIGH ... |
| `quality.autoMediumMs` | `25` | 2428 | ... this picks MEDIUM, else LOW |
| `quality.maxVsyncMs` | `17.5` | 2429 | a median at or under this is a real refresh rate (60 Hz and up) ... |
| `quality.vsyncSlack` | `1.12` | 2430 | ... and a p95 within this × the median means keeping up with vsync (counts as HIGH) |
| `quality.windowSize` | `240` | 2431 | frame times kept for the p95 (about 2 s at 120 fps) |
| `quality.dynamic.highMs` | `18` | 2433 | p95 above this for holdTime lowers the render scale |
| `quality.dynamic.lowMs` | `13` | 2434 | p95 under this (or keeping up with vsync) for holdUpTime raises it back |
| `quality.dynamic.holdTime` | `2` | 2435 | s over before a drop |
| `quality.dynamic.holdUpTime` | `4` | 2436 | s keeping up before a raise (no see-saw) |
| `quality.dynamic.helpRatio` | `0.92` | 2437 | a drop must bring the p95 under this × its old value, else it's undone ... |
| `quality.dynamic.lockoutTime` | `20` | 2438 | s ... and the scale stays put this long (CPU-bound: resolution doesn't help) |
| `quality.dynamic.checkEvery` | `0.25` | 2441 | s between checks |
| `post` | {…} | 2445 | (above) Phase 6 post-processing (pmndrs postprocessing, SPEC 2.0 §9) |
| `post.bloomThreshold` | `1` | 2447 | luminance (POWER P5: only emissives above 1 with tone mapping off glow: cores, arcs, tracers, pickups) |
| `post.bloomEnergy` | `0.9` | 2451 | × more bloom at full style energy |
| `post.chromaHurt` | `0.006` | 2455 | offset (uv) on a hit taken |
| `post.chromaDecay` | `0.03` | 2458 | offset units per s back to 0 |
| `post.exposure` | `1` | 2459 | (above) POLISH V4: the tone map (settings.toneMapping: aces \| agx \| neutral), the grade, the low-HP vignette, speed chroma. |
| `post.godRays` | {…} | 2462 | (above) Light shafts (High): screen-space god rays from a sun sphere this far along the sun's direction (inside camera.far), |
| `post.godRays` | {…} | 2463 | (above) occluded by the world's depth (the pass runs before the viewmodel's). |
| `post.lowHpAt` | `0.45` | 2465 | HP share under which the vignette darkens (up to this much more) and desaturates |
| `post.lowHpDarkness` | `0.45` | 2465 | HP share under which the vignette darkens (up to this much more) and desaturates |
| `post.chromaSpeedMin` | `14` | 2466 | speed-scaled aberration (off with reduced motion) |
| `post.chromaSpeedMax` | `32` | 2466 | speed-scaled aberration (off with reduced motion) |
| `post.chromaSpeed` | `0.0022` | 2466 | speed-scaled aberration (off with reduced motion) |
| `post.quality.high` | `{"bloom":true,"bloomScale":1,"smaa":true,"godRays":true}` | 2470 | POLISH V4: light shafts past the low sun (High only) |
| `post.quality.high.bloom` | `true` | 2470 | POLISH V4: light shafts past the low sun (High only) |
| `post.quality.high.bloomScale` | `1` | 2470 | POLISH V4: light shafts past the low sun (High only) |
| `post.quality.high.smaa` | `true` | 2470 | POLISH V4: light shafts past the low sun (High only) |
| `post.quality.high.godRays` | `true` | 2470 | POLISH V4: light shafts past the low sun (High only) |
| `lights` | {…} | 2474 | (above) §9 One hemisphere + one directional light, no shadows |
| `lights` | {…} | 2475 | (above) (colors live in `colors` above so the GUI shows color pickers) |
| `lights.hemiIntensity` | `1.15` | 2477 | the cool fill (POLISH V4) |
| `lights.dirIntensity` | `3` | 2478 | the warm key, from the low sun's side (it follows config.sky's sun, raised by keyLiftDeg) |
| `lights.keyLiftDeg` | `26` | 2479 | deg the key sits above the sun disc (a 7 deg sun would light only walls: long, readable shadows) |
| `lights.rimIntensity` | `0.35` | 2480 | the violet rim, from the opposite side, a little above the horizon |
| `lights.dirX` | `0.55` | 2482 | (unused since POLISH V4: the key follows the sun) direction toward the light, normalized at use (S) |
| `lights.dirY` | `0.7` | 2483 | (S) |
| `lights.dirZ` | `-0.6` | 2484 | (S) |
| `lights.shadows` | {…} | 2485 | (above) Shadows (POLISH V4): the key casts one following the player; Low has none (blob shadows only). |
| `lights.shadows.bias` | `-0.0004` | 2490 | m the light sits from the player along its direction |
| `lights.shadows.normalBias` | `0.04` | 2490 | m the light sits from the player along its direction |
| `lights.shadows.distance` | `120` | 2490 | m the light sits from the player along its direction |
| `ao` | {…} | 2494 | (above) POLISH V4 the baked AO volume (world/aoVolume.js): 1 m cells over the map, sampled half a metre off each surface. |
| `motes` | {…} | 2497 | (above) POLISH V4 atmosphere motes (fx/motes.js): drifting dust + blinking data flecks in a box around the camera. |
| `blobShadows` | {…} | 2500 | (above) POLISH V4 contact shadows (fx/blobShadows.js) under every grounded body. |
| `lightPool` | {…} | 2503 | (above) POLISH V4 dynamic lights (fx/lightPool.js): at most 4 point lights for flashes, blasts, boss attacks, perk machines. |
| `lightPool.max` | `3` | 2505 | (above) (the player's muzzle flash + kill pops keep their own light in combat/effects.js: 1 + max <= 4 point lights) |
| `lightPool.max` | `3` | 2506 | m: a perk machine's glow fades out between these from the camera |
| `lightPool.steadyNear` | `8` | 2506 | m: a perk machine's glow fades out between these from the camera |
| `lightPool.steadyFar` | `28` | 2506 | m: a perk machine's glow fades out between these from the camera |
| `render` | `{"maxPixelRatio":2,"antialias":true}` | 2512 | (above) Renderer |
| `render.maxPixelRatio` | `2` | 2514 | cap on devicePixelRatio for laptop GPUs (S) |
| `render.antialias` | `true` | 2515 | applies on reload |
| `arena` | {…} | 2518 | (above) §5 Arena grid + M1 greybox test layout (applies on reload; the preset system is M4). |
| `arena` | {…} | 2519 | (above) World: x and z in [−32, 32], floor top at y = 0, yaw 0 looks toward −z. |
| `arena.cellSize` | `4` | 2521 | m per pillar |
| `arena.gridSize` | `16` | 2522 | pillars per side |
| `arena.floorSize` | `64` | 2523 | m, cellSize × gridSize |
| `arena.killPlaneY` | `-20` | 2524 | m, falling below this respawns |
| `arena.spawnSpacing` | `8` | 2525 | m between enemy spawn points along the edge |
| `arena.pillars` | array[22] | 2526 | (above) [col, row, height m] on the 16×16 grid. Cols 7–8 (x −4..4) stay clear as a run corridor. |
| `arena.pillars.0` | `[4,0,12]` | 2528 | back wall, corridor gap |
| `arena.pillars.1` | `[5,0,12]` | 2528 | back wall, corridor gap |
| `arena.pillars.2` | `[6,0,12]` | 2528 | back wall, corridor gap |
| `arena.pillars.3` | `[9,0,12]` | 2528 | back wall, corridor gap |
| `arena.pillars.4` | `[10,0,12]` | 2528 | back wall, corridor gap |
| `arena.pillars.5` | `[11,0,12]` | 2528 | back wall, corridor gap |
| `arena.pillars.6` | `[15,1,12]` | 2529 | 12 m east wall (z −28..−8) |
| `arena.pillars.7` | `[15,2,12]` | 2529 | 12 m east wall (z −28..−8) |
| `arena.pillars.8` | `[15,3,12]` | 2529 | 12 m east wall (z −28..−8) |
| `arena.pillars.9` | `[15,4,12]` | 2529 | 12 m east wall (z −28..−8) |
| `arena.pillars.10` | `[15,5,12]` | 2529 | 12 m east wall (z −28..−8) |
| `arena.pillars.11` | `[2,5,1]` | 2530 | stairs |
| `arena.pillars.12` | `[3,5,2]` | 2530 | stairs |
| `arena.pillars.13` | `[4,5,3]` | 2530 | stairs |
| `arena.pillars.14` | `[5,5,4]` | 2530 | stairs |
| `arena.pillars.15` | `[10,7,2]` | 2531 | cluster |
| `arena.pillars.16` | `[11,7,6]` | 2531 | cluster |
| `arena.pillars.17` | `[10,8,1]` | 2531 | cluster |
| `arena.pillars.18` | `[11,8,4]` | 2531 | cluster |
| `arena.pillars.19` | `[12,3,6]` | 2532 | towers |
| `arena.pillars.20` | `[13,10,5]` | 2532 | towers |
| `arena.pillars.21` | `[2,10,3]` | 2532 | towers |
| `arena.features` | array[21] | 2534 | (above) Test features as boxes [xMin, yMin, zMin, xMax, yMax, zMax] in m. |
| `arena.features.0` | `[-3.65,0,27.85,-3.35,1,28.15]` | 2536 | (above) 20 m lane (z 28 → 8 at x = 0): 1 m posts every 5 m on both sides |
| `arena.features.10` | `[-14,0,18,-8,1.5,22]` | 2542 | 1.5 m ledge: a standing jump makes it |
| `arena.features.11` | `[-22,0,18,-16,1.8,22]` | 2543 | 1.8 m ledge: a single jump should miss (apex 1.5, ~1.59 held) |
| `arena.features.12` | `[8,0,18,14,1,24]` | 2544 | double-jump gap, near platform |
| `arena.features.13` | `[24,0,18,28,1,24]` | 2545 | far platform, 10 m gap (a single jump can't, a double jump can) |
| `arena.features.14` | `[-16.5,0,0,-16,1.6,8]` | 2546 | tunnel west wall |
| `arena.features.15` | `[-12,0,0,-11.5,1.6,8]` | 2547 | tunnel east wall |
| `arena.features.16` | `[-16.5,1.2,0,-11.5,1.6,8]` | 2548 | tunnel roof, underside at 1.2 m |
| `arena.features.17` | `[-30.5,0,-26,-30,10,-6]` | 2549 | wall-ride corridor, wall A (20 m long) |
| `arena.features.18` | `[-25,0,-26,-24.5,10,-6]` | 2550 | wall-ride corridor, wall B (5 m from A) |
| `arena.features.19` | `[20,0,-32,28,4,-26]` | 2551 | slam deck: 4 m drop north onto flat floor |
| `arena.features.20` | `[-15.5,0,24,-15,2.5,30]` | 2552 | a low cover wall (the ricochet-bounce test shoots it) |
| `arena.pads` | `[[2.2,0,15.5],[26,0,-19.5]]` | 2554 | (above) Jump pads [x, y, z] (surface center). Cyan, render-only; the floor underneath collides. |
| `arena.pads.0` | `[2.2,0,15.5]` | 2556 | on the lane, off the x = 0 run line |
| `arena.pads.1` | `[26,0,-19.5]` | 2557 | next to the 12 m east wall, can reach the slam deck |
| `arena.spawn.x` | `0` | 2560 | m |
| `arena.spawn.y` | `0` | 2561 | m, feet |
| `arena.spawn.z` | `28` | 2562 | m, start of the 20 m lane |
| `arena.spawn.yawDeg` | `0` | 2563 | looks down the lane (−z) |
| `arena.testSpots` | {…} | 2565 | (above) Named start points for the ?debug=1 script presets: {x, y, z, yawDeg} |
| `arena.testSpots.ledgeEdge` | `{"x":-11,"y":1.5,"z":19,"yawDeg":0}` | 2568 | 1 m from the 1.5 m ledge's south edge |
| `arena.testSpots.ledgeEdge.x` | `-11` | 2568 | 1 m from the 1.5 m ledge's south edge |
| `arena.testSpots.ledgeEdge.y` | `1.5` | 2568 | 1 m from the 1.5 m ledge's south edge |
| `arena.testSpots.ledgeEdge.z` | `19` | 2568 | 1 m from the 1.5 m ledge's south edge |
| `arena.testSpots.ledgeEdge.yawDeg` | `0` | 2568 | 1 m from the 1.5 m ledge's south edge |
| `arena.testSpots.gapStart` | `{"x":9,"y":1,"z":21,"yawDeg":-90}` | 2569 | near platform, facing the gap (+x) |
| `arena.testSpots.gapStart.x` | `9` | 2569 | near platform, facing the gap (+x) |
| `arena.testSpots.gapStart.y` | `1` | 2569 | near platform, facing the gap (+x) |
| `arena.testSpots.gapStart.z` | `21` | 2569 | near platform, facing the gap (+x) |
| `arena.testSpots.gapStart.yawDeg` | `-90` | 2569 | near platform, facing the gap (+x) |
| `arena.testSpots.tunnelApproach` | `{"x":-14,"y":0,"z":14,"yawDeg":0}` | 2570 | 6 m north of the tunnel mouth |
| `arena.testSpots.tunnelApproach.x` | `-14` | 2570 | 6 m north of the tunnel mouth |
| `arena.testSpots.tunnelApproach.y` | `0` | 2570 | 6 m north of the tunnel mouth |
| `arena.testSpots.tunnelApproach.z` | `14` | 2570 | 6 m north of the tunnel mouth |
| `arena.testSpots.tunnelApproach.yawDeg` | `0` | 2570 | 6 m north of the tunnel mouth |
| `arena.testSpots.tunnelInside` | `{"x":-14,"y":0,"z":4,"yawDeg":0}` | 2571 | mid-tunnel, under the 1.2 m roof |
| `arena.testSpots.tunnelInside.x` | `-14` | 2571 | mid-tunnel, under the 1.2 m roof |
| `arena.testSpots.tunnelInside.y` | `0` | 2571 | mid-tunnel, under the 1.2 m roof |
| `arena.testSpots.tunnelInside.z` | `4` | 2571 | mid-tunnel, under the 1.2 m roof |
| `arena.testSpots.tunnelInside.yawDeg` | `0` | 2571 | mid-tunnel, under the 1.2 m roof |
| `arena.testSpots.pillarEdge` | `{"x":-7.85,"y":4,"z":-10,"yawDeg":90}` | 2572 | feet 0.15 m past the 4 m stair edge |
| `arena.testSpots.pillarEdge.x` | `-7.85` | 2572 | feet 0.15 m past the 4 m stair edge |
| `arena.testSpots.pillarEdge.y` | `4` | 2572 | feet 0.15 m past the 4 m stair edge |
| `arena.testSpots.pillarEdge.z` | `-10` | 2572 | feet 0.15 m past the 4 m stair edge |
| `arena.testSpots.pillarEdge.yawDeg` | `90` | 2572 | feet 0.15 m past the 4 m stair edge |
| `arena.testSpots.wall` | `{"x":27.3,"y":0,"z":-9,"yawDeg":0}` | 2573 | beside the east wall, facing along it |
| `arena.testSpots.wall.x` | `27.3` | 2573 | beside the east wall, facing along it |
| `arena.testSpots.wall.y` | `0` | 2573 | beside the east wall, facing along it |
| `arena.testSpots.wall.z` | `-9` | 2573 | beside the east wall, facing along it |
| `arena.testSpots.wall.yawDeg` | `0` | 2573 | beside the east wall, facing along it |
| `arena.testSpots.corridorWallA` | `{"x":-29.64,"y":4,"z":-6.4,"yawDeg":0}` | 2574 | mid-air 1 cm from wall A's north end, facing down the corridor |
| `arena.testSpots.corridorWallA.x` | `-29.64` | 2574 | mid-air 1 cm from wall A's north end, facing down the corridor |
| `arena.testSpots.corridorWallA.y` | `4` | 2574 | mid-air 1 cm from wall A's north end, facing down the corridor |
| `arena.testSpots.corridorWallA.z` | `-6.4` | 2574 | mid-air 1 cm from wall A's north end, facing down the corridor |
| `arena.testSpots.corridorWallA.yawDeg` | `0` | 2574 | mid-air 1 cm from wall A's north end, facing down the corridor |
| `arena.testSpots.corridorApproach` | `{"x":-27.5,"y":0,"z":2,"yawDeg":15}` | 2575 | north of the corridor, angled toward wall A |
| `arena.testSpots.corridorApproach.x` | `-27.5` | 2575 | north of the corridor, angled toward wall A |
| `arena.testSpots.corridorApproach.y` | `0` | 2575 | north of the corridor, angled toward wall A |
| `arena.testSpots.corridorApproach.z` | `2` | 2575 | north of the corridor, angled toward wall A |
| `arena.testSpots.corridorApproach.yawDeg` | `15` | 2575 | north of the corridor, angled toward wall A |
| `arena.testSpots.deckEdge` | `{"x":24,"y":4,"z":-27,"yawDeg":180}` | 2576 | on the slam deck, 1 m from its north edge |
| `arena.testSpots.deckEdge.x` | `24` | 2576 | on the slam deck, 1 m from its north edge |
| `arena.testSpots.deckEdge.y` | `4` | 2576 | on the slam deck, 1 m from its north edge |
| `arena.testSpots.deckEdge.z` | `-27` | 2576 | on the slam deck, 1 m from its north edge |
| `arena.testSpots.deckEdge.yawDeg` | `180` | 2576 | on the slam deck, 1 m from its north edge |
| `arena.testSpots.padApproach` | `{"x":2.2,"y":0,"z":18.5,"yawDeg":0}` | 2577 | 3 m north of the lane pad |
| `arena.testSpots.padApproach.x` | `2.2` | 2577 | 3 m north of the lane pad |
| `arena.testSpots.padApproach.y` | `0` | 2577 | 3 m north of the lane pad |
| `arena.testSpots.padApproach.z` | `18.5` | 2577 | 3 m north of the lane pad |
| `arena.testSpots.padApproach.yawDeg` | `0` | 2577 | 3 m north of the lane pad |
| `arena.testSpots.ledgeTop` | `{"x":-11,"y":1.5,"z":20,"yawDeg":180}` | 2578 | on the 1.5 m ledge, facing north (+z) |
| `arena.testSpots.ledgeTop.x` | `-11` | 2578 | on the 1.5 m ledge, facing north (+z) |
| `arena.testSpots.ledgeTop.y` | `1.5` | 2578 | on the 1.5 m ledge, facing north (+z) |
| `arena.testSpots.ledgeTop.z` | `20` | 2578 | on the 1.5 m ledge, facing north (+z) |
| `arena.testSpots.ledgeTop.yawDeg` | `180` | 2578 | on the 1.5 m ledge, facing north (+z) |
| `arena.testSpots.ledgeHusk` | `{"x":-11,"y":0,"z":26,"yawDeg":0}` | 2579 | on the floor 6 m north of the ledge top |
| `arena.testSpots.ledgeHusk.x` | `-11` | 2579 | on the floor 6 m north of the ledge top |
| `arena.testSpots.ledgeHusk.y` | `0` | 2579 | on the floor 6 m north of the ledge top |
| `arena.testSpots.ledgeHusk.z` | `26` | 2579 | on the floor 6 m north of the ledge top |
| `arena.testSpots.ledgeHusk.yawDeg` | `0` | 2579 | on the floor 6 m north of the ledge top |
| `arena.testSpots.wallFace` | `{"x":27.6,"y":0,"z":-18,"yawDeg":-90}` | 2580 | eye 0.4 m from the east wall, facing it (+x) |
| `arena.testSpots.wallFace.x` | `27.6` | 2580 | eye 0.4 m from the east wall, facing it (+x) |
| `arena.testSpots.wallFace.y` | `0` | 2580 | eye 0.4 m from the east wall, facing it (+x) |
| `arena.testSpots.wallFace.z` | `-18` | 2580 | eye 0.4 m from the east wall, facing it (+x) |
| `arena.testSpots.wallFace.yawDeg` | `-90` | 2580 | eye 0.4 m from the east wall, facing it (+x) |
| `debug` | {…} | 2584 | (above) ?debug=1 tools |
| `debug.overlayRefresh` | `0.25` | 2586 | s between overlay text updates |
| `debug.presetTimeout` | `120` | 2587 | (above) NIGHT6 Q1: a preset that runs longer than this (x presetTimeoutSlowMult for the slow ones) fails by name and its |
| `debug.presetTimeout` | `120` | 2588 | (above) script is aborted (two full regressions hung silently on NIGHT5). |
| `debug.settleTime` | `0.25` | 2590 | (above) Script preset timings (s) |
| `debug.settleTime` | `0.25` | 2591 | stand still after a respawn |
| `debug.botNavTimeout` | `40` | 2592 | s per goal in botNav |
| `debug.botAccuracyTime` | `10` | 2593 | s per target in botAccuracy |
| `debug.perfRunTime` | `6` | 2594 | s per perf run (Phase 7) |
| `debug.slowMo` | `0.25` | 2595 | F3 time scale (Phase 8) |
| `debug.flySpeed` | `12` | 2596 | m/s, F4 free-fly camera |
| `debug.flyFastMult` | `3` | 2597 | × with Shift |
| `debug.runUpTime` | `0.5` | 2598 | walk before sliding |
| `debug.jumpHoldTime` | `0.5` | 2599 | hold Space past the apex (~0.45 s with hang), so no variable-jump cut |
| `debug.coyoteTestDelay` | `0.08` | 2600 | jump this long after walking off the ledge |
| `debug.bufferTestLead` | `0.08` | 2601 | press jump this long before landing |
| `debug.slideJumpCount` | `5` | 2602 | hops in the slide-jump chain |
| `debug.scriptMaxWait` | `3` | 2603 | cap on any "until" segment |
| `debug.staminaDashGap` | `0.25` | 2604 | between the three dashes |
| `debug.staminaSampleInterval` | `0.5` | 2605 | between pip samples |
| `debug.staminaSampleTime` | `5` | 2606 | total sampling time |
| `debug.edgeTestTime` | `2` | 2607 | stand on the pillar edge |
| `debug.wallTestTime` | `1.2` | 2608 | strafe into the east wall |
| `debug.wallRideEntrySpeed` | `12` | 2609 | m/s along the corridor for the wall-ride presets |
| `debug.wallRidePush` | `0.5` | 2610 | m/s into wall A so the first step makes contact |
| `debug.wallJumpDelay` | `0.01` | 2611 | ride this long (1 step) before each wall jump in the chain |
| `debug.slamVsSlideLeads` | `[0.05,0.3]` | 2612 | s before landing to press C: expect slide, then slam |
| `debug.dashAllStatesAirDelay` | `0.3` | 2613 | s after a jump before the air dash |
| `debug.slamDropDelay` | `0.1` | 2614 | fall this long off the deck before slamming |
| `debug.fullChainRideTime` | `0.2` | 2615 | ride this long before the wall jump |
| `debug.fullChainWallJumpFly` | `0.1` | 2616 | fly this long after the wall jump before dashing |
| `debug.fullChainDashJumpDelay` | `0.05` | 2617 | dash this long before jumping out of it |
| `debug.fullChainSlamDelay` | `0.15` | 2618 | fly this long after the dash jump before slamming |
| `debug.headshotDistance` | `15` | 2619 | m to the Husk in revolverHeadshot |
| `debug.meleeStartDistance` | `4` | 2620 | m between the player and the Husk in huskMelee |
| `debug.healNear` | `5` | 2621 | m, kill distance that heals in the healing preset |
| `debug.healFar` | `8` | 2622 | m, kill distance that doesn't |
| `debug.healStartHp` | `50` | 2623 | HP set before the healing preset |
| `debug.perfCount` | `15` | 2624 | Husks alive in perf15 |
| `debug.perfSampleTime` | `3` | 2625 | s of fps sampling |
| `debug.aimSettleTime` | `0.1` | 2626 | s of aiming (several rendered frames) before a scripted shot |
| `debug.meleeDashLead` | `0.1` | 2627 | s before the Husk's hit lands that the scripted dash starts |
| `debug.combatWait` | `3` | 2628 | s cap while waiting for a kill / attack / leap |
| `debug.swapWaitTime` | `1` | 2629 | (above) M3c presets |
| `debug.swapWaitTime` | `1` | 2630 | s cap holding fire after the swap request |
| `debug.punchHuskDistance` | `1.6` | 2631 | m in front of the player |
| `debug.parryStartHp` | `50` | 2632 | HP set before the parry preset |
| `debug.recoilShots` | `10` | 2633 | revolver shots in recoilRecover |
| `debug.recoilSettleTime` | `0.5` | 2634 | s after the last shot before measuring drift |
| `debug.noClipTargetWidth` | `320` | 2635 | px, the coverage render target |
| `debug.perfCombatTime` | `3` | 2636 | s of held shotgun fire with 15 Husks |
| `debug.snapMaxRecordFrames` | `600` | 2637 | frames kept per snap-audit scenario |
