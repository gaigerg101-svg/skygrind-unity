# FORGE62 research: what changed NIGHT62, with the sources

Three web-only scouts, one question each, run 2026-10-22 16:38-17:15; their full files sit beside this one with every claim's URL and a
Verified / Inferred mark. FORGE61's research (Research/2026-10-22-forge61/research-claude-code.md, research-prompting.md, research-deadcode.md)
still holds and is not repeated here.

## 1. The /goal on a long run (research-goal-runs.md; read from code.claude.com/docs, changelog 2.1.309)
- The evaluator is a small model on a session-scoped Stop hook: it reads the condition and the conversation only, and **a turn that ends with
  a subagent or a background shell still running is not evaluated** (Verified). So waiting on background work is safe, and the check comes at the
  first turn that ends with nothing running.
- **"X as the final message" is unreliable if work continues after X** (Verified / Inferred, section 5 of the file). NIGHT62's goal says
  `NIGHT62 COMPLETE` is printed after every background job and agent has finished. (FORGE62 itself ran after NIGHT61's `NIGHT61 COMPLETE`
  line at GG's request; it ends with no background work and that line again.)
- A message typed mid-turn is handed to Claude inside the same turn (Verified): the run can answer GG without breaking the goal's order.
- Check-ins come at 30 min, 1 h after that, then every 2 h; an interactive session starts at most three idle check-ins per goal between prompts
  (Verified). Background Bash and background subagents notify on completion; a Monitor watch lasts 30 min at most and must be re-armed; /loop and
  cron fire only between turns (Verified). NIGHT62 waits with background commands and completion notifications, never a foreground sleep (L-122).
- A usage limit pauses the goal and an open interactive session resumes at the reset; background subagents that hit a limit fail (Verified):
  commit every step, resume builders by SendMessage (L-003, L-098).
- Compaction keeps background jobs and subagents running and re-injects CLAUDE.md and auto memory; an overflow it cannot clear ends the goal
  (Verified): NIGHT62 re-reads NIGHT62.md after a compaction, and its log and slate carry the state.

## 2. Disk-light worktrees (research-worktrees.md)
- Claude Code's `worktree.sparsePaths` setting (since 2.1.76) writes only the listed folders plus root files, and the docs say the worktree keys
  apply to isolated subagents (Verified). A March 2026 report measured EnterWorktree ignoring it mid-session and did not test subagents; no
  changelog line says it was fixed (Verified / Inferred). **Not adopted tonight (FORGE62 D-13):** unproven here, and a sparse tree would hide
  prefabs, shaders and scenes that the view builders read.
- `worktree.baseRef: "head"` is required, or subagent worktrees branch from main, not port/stage3 (Verified; already set by FORGE61 D-8).
- Builder worktrees with unpushed commits are never swept automatically, and this project never pushes (Inferred from the cleanup docs): they
  stay until GG removes them. The night keeps them (GG's call) and counts them in the disk line.
- By hand, a sparse worktree is `git worktree add --no-checkout`, `git sparse-checkout set --cone <dirs>`, `git checkout` (Verified, git's source);
  it turns on `extensions.worktreeConfig` in the shared .git/config, which older tools cannot read.

## 3. Unity tests outside the Editor (research-tests-outside-unity.md, landed 17:15; the spike measured 17:22)
- Teams run a `noEngineReferences` assembly's tests by a hand-written csproj that compiles the same `.cs` files (C# 9, NUnit 3.14, which keeps
  the classic asserts); one public project runs 201 of its 204 EditMode tests that way with no Unity and no licence (Verified, novacore PR 15).
  This project already builds Tools/CoreHarness that way (net10.0, LangVersion 9.0, `Assets/Scripts/Core/**/*.cs`).
- Every UnityEngine member that is an engine call throws outside Unity: `Application.dataPath`, `Time`, `Debug.Log`, `new GameObject`,
  ScriptableObject creation, `Quaternion.Euler` / `Slerp`, `Matrix4x4.TRS` (Verified as source; the rows marked Inferred in the file).
  Plain `Mathf` / `Vector3` / `Quaternion` arithmetic is ordinary C#.
- **Measured by the forge (scratch spike, 17:22:01-17:22:10, deleted after):** `dotnet restore --source <an empty folder>` restored NUnit 3.14.0,
  NUnit3TestAdapter 4.6.0 and Microsoft.NET.Test.Sdk 17.12.0 from this machine's NuGet cache in 175 ms, so no download is needed. `dotnet
  test --no-restore` (net10.0, C# 9, the Editor's UnityEngine.CoreModule.dll by HintPath) ran 7 tests: PlainPasses, MathfSinRuns and
  Vector3MagnitudeRuns passed; PlantedFails failed on its assert ("planted failure"); DebugLogNeedsUnity, DataPathNeedsUnity and
  QuaternionEulerNeedsUnity each failed with "System.Security.SecurityException : ECall methods must be packaged into a system module." So
  T1's runner can print that message as **needs-Unity**, apart from a real FAIL. `dotnet test --source` itself is refused by SDK 10.0.401
  ("MSB1001: Unknown switch"): restore first, then test with `--no-restore`. NUnitLite is not in the cache (not needed).
- **The limit:** Unity's Mono does float math in double precision and .NET does not (Verified, aras-p.info 2026-06-11), and outside Unity
  nothing checks LogAssert or runs `[UnityTest]`. So T1's runner is an early warning on a night the Editor is deaf. It never replaces the
  Editor's EditMode run, and a test it cannot run stays "not checked". CLAUDE.md rule 3's golden and replay gates are unchanged: they already
  run through CoreHarness / SimHarness in n62-gates.sh.

## 4. What the forge measured itself (2026-10-22)
- `bash Tools/night/n62-gates.sh` against the deaf Editor (16:58-17:01): config, golden 0 mismatches, 22 replays exact, fingerprints plain and
  --off57..--off62 EQUAL; the Editor step "GATE FAIL: editor compile (editor_status gave no answer in 60 s: not checked)"
  (dryrun-n62-gates.txt).
- `node .claude/hooks/guard.cjs --selftest`: 92 / 92 as planted; game-m-sync and bg-chain each refused live.
- `node Tools/night/n62-slate.cjs Research/2026-10-22-forge62/slate-seed.json`: 14 slices, 14 todo, 0 malformed.
- `node Tools/night/n62-freeze-rows.cjs "n61.exitFrame61=planted test" --dry --slate=<the seed>`: refused ("a row turns ON only after its critic
  PASS", exit 1), so the tool reaches NIGHT61's rows and guards them.
- Every path NIGHT62.md names exists (the forge's existence check, 17:11), the batch copy included.
- Port.Root (Assets/Tests/EditMode/GoldenVectorTests.cs:17) reads Application.dataPath: most EditMode tests cannot run outside Unity until T1's
  fallback lands. Unity ships NUnit at Library/PackageCache/com.unity.ext.nunit@d8c07649098d/net40/unity-custom/nunit.framework.dll.

## 5. FORGE62b (the 2-hour resize, 2026-10-22 evening): what the forge measured itself
The PC ran out of virtual memory at 17:32 (System log: Volsnap 6 / 15 "the system may be low on virtual memory", FORGE62's last write the same
second), the screen went black at 18:08 (LiveKernelEvent 1A8 / 1B8), and it was restarted at 20:17 (event 1074 by winlogon). No bugcheck.
- **After the restart:** C: 37.8 GB free (20:29); commit 53.2 of 97.6 GB with three Claude sessions open; the Editor (restarted 20:21) answers:
  `bash Tools/night/n62-gates.sh` all GREEN 20:44:37-20:46:04 (1 min 27 s, the Editor compile step 0 errors); `unity command set_autotick
  --enable true --interval_ms 16` answered "Auto-tick already enabled (interval updated to 16ms)" 20:50:51; `bash Tools/unity-loop.sh tests
  N61SharedTests` 2 / 2 passed 20:53:50-20:53:57.
- **Measured minutes, NIGHT60 (the last night with Players):** a Player build 33-76 s (n60-b1..b7: each "build ... start" line against the
  build folder's last file); a verbs37 reel 52-56 s start to start (n60-freeze vf..vp, 07:09:24-07:13:41); an a157 reel about 1 min of frames,
  1 min 45 s start to start (n60-b8 b8on / b8off); the smoke 227.9 s; a critic 13-25 min (gate-b14 02:10-02:23, gate-b2 00:35-01:00; L-109
  17-23); an ON flip with its flow gate and commit about 15 min (B2 01:03-01:18). NIGHT61 ran no Player step, so no estimate-to-actual ratio
  for P0 exists; the 2-hour plan sizes from these.
- **The P0 reels, counted from the builders' notes:** P5 6, P1 3 (with the SPOT side, D-20), P3 2, P2 6, verbs37 5, P4 16: 38 reels.
  After the red team (D-24): verbs37 5, P5 4 (vs / vp are its span and spire on sides), P1 2 + 1, P3 2 (c3on-k is its on side; s4on-f is
  shared with P4), P2 2 + 4, P4 2 + 13; 17 reels when every SPOT-first pair says NOTHING OBVIOUS, 35 when all pass. The red team's own reel
  timings: 63-121 s each (n60-b7: ten a357 reels in 12:49; n60-b8: verbs37 68-89 s, a157 with clear shots 78-121 s).
- **P1's meter, re-read by the red team:** n60-v-lamp.py's LAMP line is lamp60's old MAIN (door / room median), which NIGHT60's frames
  already pass 4 / 5; P1's MAIN is the own gain, now printed as `OWN GAIN ... n / 5` (0 / 5 on NIGHT60's frames; planted 8 cases GREEN).
- **S6's premise is false:** NIGHT60's verbs37 telemetry (cam.fov against vel.speed, n60-freeze vs and vk, 1800 frames each): 74.0 deg at
  0-8 m/s, a median of 75.8 at 8-14, 79.4-83.4 at 14-20, 81.8-81.9 at 20-26, 83.3 above 26; max 85.8 at 24 m/s. The speed FOV exists
  (CameraFeel.cs, config camera.fovBoostDeg 8 from fovBoostStartSpeed 8 to fovBoostFullSpeed 24, camera.speedFovCapDeg 10).
- **P5's meter:** Tools/night/n62-gun-yellow.py, planted 5 / 5; 2,887 yellow px in the gun's corner of n60-b8 b8on's kb2c entry, 15 on kb3's.
- **The hard cap:** Tools/night/n62-clock.cjs start / status / refused restart / stop run 20:47:13; the guard's clock rule 110 / 110 in its
  selftest and refused a build and an Agent live (D-14).
- **The launch line:** `claude -p ... --model claude-opus-5-5 --strict-mcp-config --append-system-prompt-file nights/PROMPT-STOPS.md
  --dangerously-skip-permissions` answered in 7 s (D-18).
- **Not checked:** a reel set's duration in this build (none run in the forge: they steal no focus but take ~1 min each and ~10 GB raw);
  the a357, a154 and boss61 reels' lengths (no NIGHT60 timing; assumed 1-1.75 min like a157 and verbs37).
