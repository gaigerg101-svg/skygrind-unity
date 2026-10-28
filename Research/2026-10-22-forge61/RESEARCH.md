# RESEARCH: what shaped NIGHT61 (FORGE61 section 3; 2026-10-22)

Three web-only researchers read the pages themselves and quoted them; their full notes, with every quote, are beside this file:
`research-claude-code.md` (12 Claude Code docs pages), `research-prompting.md` (9 sources), `research-deadcode.md` (Unity, Roslyn,
Microsoft Learn, GitHub). "Verified" = the page was read; a claim below without a URL is the forge's own inference and says so.

## Claude Code mechanics (Verified, research-claude-code.md)
| finding | source | used in NIGHT61 |
|---|---|---|
| A PreToolUse hook blocks with exit code 2 (stderr shown to Claude) or a JSON `permissionDecision: "deny"`; a deny holds in `bypassPermissions` and under `--dangerously-skip-permissions` | https://code.claude.com/docs/en/hooks, https://code.claude.com/docs/en/hooks-guide | `.claude/hooks/guard.cjs` (exit 2), so GG's skip-permissions launch is still guarded |
| Match `Bash|PowerShell` so both shell tools are covered | hooks | the matcher also takes Write / Edit / MultiEdit / NotebookEdit |
| On Windows a shell-form hook runs in Git Bash (which sources the profile); the exec form `command: "node"` + `args` runs with no shell | hooks | exec form with `${CLAUDE_PROJECT_DIR}` |
| A hook that crashes, times out or exits 1 lets the action through unless `"onFailure": "block"` (v2.1.295+; installed: 2.1.299) | hooks | `onFailure: block`; the script itself fails open on its own bugs and logs them (DECISIONS D-4) |
| Hook edits in settings files reload in the running session; hooks fire for subagents' tool calls too | https://code.claude.com/docs/en/settings, hooks | tested live in this session: 9 planted commands blocked |
| `/goal`: a small model judges the condition from the transcript only, runs no commands; 4,000 characters; a usage limit pauses it; no tool calls for several turns stops the loop; background work skips evaluation (check-ins at 30 min, 1 h, then every 2 h) | https://code.claude.com/docs/en/goal | GOAL.txt names only printed lines (date, gates, the slate tool's line, the tag, a clean `git status` of the close files) |
| `/loop` fires only between turns, never mid-turn | https://code.claude.com/docs/en/scheduled-tasks | no /loop; the disk watcher is a background script whose exit wakes the lead |
| Project subagents live in `.claude/agents/`; frontmatter `name`, `description`, `tools`, `model` (opus / inherit / an id), `effort`; caps `CLAUDE_CODE_MAX_CONCURRENT_SUBAGENTS` (default 20) and `CLAUDE_CODE_MAX_SUBAGENT_SPAWN_DEPTH` (default 3, `1` turns nesting off) | https://code.claude.com/docs/en/sub-agents, https://code.claude.com/docs/en/env-vars | three agents; caps 6 and 1 in `.claude/settings.json` `env` |
| Subagent worktrees branch from the repository's default branch (`main` here) unless `worktree.baseRef` is `"head"`; worktrees with changes stay until the `cleanupPeriodDays` sweep | https://code.claude.com/docs/en/worktrees | `worktree.baseRef: "head"` (builders start at port/stage3) |
| Give Claude a check it can run; the check must gate the stop (a /goal condition or a Stop hook), not only be asked for | https://code.claude.com/docs/en/best-practices | the critic gate + the slate tool + the goal's printed lines |

## Prompting and long runs (Verified, research-prompting.md)
| finding | source | used in NIGHT61 |
|---|---|---|
| Opus 5: give the full task up front; state the scope (it can widen it); no "double-check" lines; explicit delegation rules and low spawn counts; for reviews ask it to report everything and filter in a separate pass | https://platform.claude.com/docs/en/build-with-claude/prompt-engineering/prompting-claude-opus-5 | the slate is the whole task; owners fixed; at most 4 builders + 2 critics; critics "report every problem, the lead filters" |
| Opus 5.5: prompts carry over; default effort `medium` matched or beat Opus 5 at `high` in fewer steps and tokens; an unattended loop stops when a turn ends with only a status message | https://platform.claude.com/docs/en/build-with-claude/prompt-engineering/prompting-claude-opus-5-5 | the lead keeps the default effort (the usage limit cost 4:40 in w55-w60); critics `effort: high`; launch with `--append-system-prompt-file nights/PROMPT-STOPS.md` |
| Long runs: a JSON feature list whose pass flag flips only after testing; a progress file; git commits per slice; one feature at a time; tests are never edited to pass | https://www.anthropic.com/engineering/effective-harnesses-for-long-running-agents | `Logs/n61/slate.json` + `n61-slate.cjs`; `NIGHT61-LOG.md`; commit per step |
| A separate evaluator is easier to make skeptical than a builder self-critical, but it talks itself into approving; it needs hard thresholds; it was tuned by reading its logs where it differed from the author's judgment and editing its prompt over rounds (few-shot examples came with score breakdowns) | https://www.anthropic.com/engineering/harness-design-long-running-apps | `skygrind-critic`: hard thresholds from the slate, three real cases (PASS, SUBTLE FAIL, tool-read-as-game FAIL); the retro is its tuning loop |
| Multi-agent work costs ~15x the tokens of a chat; spawn counts belong in the prompt | https://www.anthropic.com/engineering/multi-agent-research-system | caps in settings and in section 6 |
| Tension: the Claude Code page warns that chasing every review finding over-engineers; Opus 5 says report everything and filter | best-practices vs prompting-claude-opus-5 | critics report everything; the lead filters with a logged reason (one place to filter) |

## Dead code in Unity C# (Verified, research-deadcode.md)
| finding | source | used in NIGHT61 |
|---|---|---|
| IDE0051 / IDE0052 flag private members only, run on build only with `EnforceCodeStyleInBuild` and a raised severity; `dotnet format style --verify-no-changes` lists without changing; without the flag it deletes | https://github.com/dotnet/roslyn/blob/main/src/Analyzers/Core/Analyzers/EnforceOnBuildValues.cs (and the Microsoft Learn pages in the note) | the guard blocks `dotnet format` without `--verify-no-changes`; IDE hits are leads only |
| Measured here: a scratch copy of Skygrind.Core built in 8 s and wrote nothing into the repo; the IDE0051 probe flagged 0 of 3 planted dead members (not working yet); 2 x CS0414 found | research-deadcode.md 5.2 | the auditor uses grep + script-GUID census + compiler warnings; not IDE0051 as proof |
| Plain IDE0051 flags Unity messages and `[SerializeField]` fields; Microsoft.Unity.Analyzers' suppressors fix that (MIT; not installed) | https://github.com/microsoft/Microsoft.Unity.Analyzers | a proposal for GG, not tonight |
| Code Coverage (`com.unity.testtools.codecoverage`) measures only in the Editor, not a Player; it is not in the manifest | https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.2/manual/index.html | a proposal for GG |
| Unused assets: `AssetDatabase.GetDependencies` from the build scenes and the BuildReport's packed assets, read-only | https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AssetDatabase.GetDependencies.html | beyond list (assets are not tonight's dead-code target) |
| Size: Core 292 files / 87k lines, Game 1,142 files / 201k lines; 337 file names carry a night tag | research-deadcode.md 5.1 | the pass works in small batches |

## Not checked
The `/goal` evaluator's exact prompt; how `onFailure: block` behaves when node itself is missing (not tried: node is on PATH and every
night tool needs it); InspectCode and the coverage package on this project (they need GG's OK).
