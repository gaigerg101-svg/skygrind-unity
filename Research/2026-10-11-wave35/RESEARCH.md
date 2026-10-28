# Wave 35 research index (forge 2026-10-11 19:28; three web subagents, one code audit, the REA / ILSpy read of ULTRAKILL)

Files (every claim carries its URL or file:line there):
- research/audit.md: why each thing GG named broke or never read (the charge shots, pickups, the grapple's limp rope and unlock, the
  healing item, the "triangle" lights), file:line and the commit.
- research/lucio.md: Lucio's wall ride rules, patch-note numbers, open-source wall-run repos (none clones Lucio), level design.
- research/look.md: PS1 / PS2 URP passes (VNTG: The Unlicense / CC0, Unity 6 Render Graph; RetroPSX-URP: MIT), URP lighting fixes, UI juice
  (UIEffect installed; LitMotion / Coily / TMPEffects / KinoGlitch MIT).
- research/teach.md: ULTRAKILL 0-1's teaching rooms and text format, Portal's one-tool-at-a-time rule, pad prompts, pickups (no red cross:
  a protected emblem), kishotenketsu and bosses as the level's exam.
- ../2026-10-11-wave34/research/rea-ultrakill.md: ULTRAKILL's own numbers (TimeController, GunControl, NewMovement, HookArm, Punch,
  StyleHUD) plus tonight's additions below.

## ULTRAKILL read tonight (REA connected; ILSpy decompile in the scratch folder; numbers only, no code copied)
- **Teaching text** (HudMessage.cs): a trigger volume on the player shows `message` + the action's live binding string
  (`InputManager.GetBindingString(action)`) + `message2`; `timerTime` 5 s by default; once unless `notOneTime`; optional hide on
  leaving the volume; an optional `playerPref` remembers it was shown.
- **The PSX options** (GraphicsSettings.cs, PrefsManager.cs defaults): downscale to 720 / 480 / 360 / 240 / 144 / 36 lines (default
  off); colour compression 64 / 32 / 16 / 8 / 3 levels (default 32); vertex warping grid 400 / 160 / 80 / 40 / 16 (default off, keyword
  VERTEX_WARPING); texture warping 0..0.5 (default 0); a dither texture; a palette LUT (`_ColorPrecision`, ConvertPaletteToLUT).
- **SKYGRIND today** (Design/unity-config.json design.n29.crt): defaultStrength 0.35, warp 0.02, scanlines 720 at 0.12, chroma 0.0015,
  grain 0.03, vignette 0.25, **pixelStep 0** (no pixelation): the reason the PS2 look "doesn't show".
- **The wall-ride zoom cue today** (design.n27.slowAim27): world share 0.75, vignette 0.25, chroma 0.15, saturation -15, FOV 4: too faint.

## Table: starting numbers (each cited; iterate on frames, log deltas)
| number | start | source |
|---|---|---|
| internal resolution | 360-480 lines, point upscale | ULTRAKILL presets 480 / 360 (GraphicsSettings.cs:199-212) |
| colour compression | 32 levels per channel | ULTRAKILL default (PrefsManager.cs:132, GetColorCompressionValue) |
| vertex snap grid | 160 | ULTRAKILL's middle preset (GetVertexWarpingValue) |
| teaching text time | 5 s, once | ULTRAKILL HudMessage timerTime |
| wall ride: hold jump, no time cap | long safety cap only | lucio.md section 1 |
| wall ride: release = boosted jump, look pitch sets height | 4.1-9.1 m/s up (wiki excerpt, unconfirmed) | lucio.md section 1 |
| wall ride speed buff / exit boost | +30 % ride, exit boost | lucio.md (patch notes) |
| re-ride rule | not the same wall until landing | lucio.md section 1 |
| grapple pull | constant speed straight to the point | ULTRAKILL HookArm (60 constant) |
| health pickup shape | not a red cross | teach.md section 2 (Geneva emblem) |
