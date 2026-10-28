# Claude Code docs research for FORGE61 (an 8-hour unattended run prompt)

Date: 2026-10-22. Researcher: a web-only subagent. Source: the official Claude Code docs at
`https://code.claude.com/docs/en/`, read with WebFetch on 2026-10-22. No personal data went into any request.

**How the quotes were captured (read this before trusting a quote):**
- **[raw]**: WebFetch returned the page's own Markdown (the whole page, or a saved copy that I then read or grepped).
  These quotes are the page text.
- **[extract]**: on long pages, WebFetch's extractor model answered a "quote verbatim" prompt over a 100,000-character
  chunk. These quotes are what the extractor returned; a `[…]` or `...` inside them is its elision, not mine.
- Many behaviours are version-gated (for example `onFailure` needs v2.1.295+). **The installed Claude Code version was
  not checked.**

Pages read (all Verified unless marked):

| Page | URL | How read |
| :- | :- | :- |
| Hooks reference | https://code.claude.com/docs/en/hooks | chars 0-200,000 [extract], 4 calls; chars 200,000-272,937 [raw], grepped and partly read |
| Hooks guide | https://code.claude.com/docs/en/hooks-guide | [raw], whole page saved, grepped and read |
| /goal | https://code.claude.com/docs/en/goal | [raw], whole page |
| Scheduled tasks (/loop) | https://code.claude.com/docs/en/scheduled-tasks | [raw], whole page |
| Best practices | https://code.claude.com/docs/en/best-practices | [raw], whole page |
| Subagents | https://code.claude.com/docs/en/sub-agents | chars 0-100,000 [extract], 2 calls; the last 10,778 chars not read |
| Skills | https://code.claude.com/docs/en/skills | chars 0-100,000 [extract]; the last 11,505 chars not read |
| Settings | https://code.claude.com/docs/en/settings | [raw], whole page |
| Environment variables | https://code.claude.com/docs/en/env-vars | chars 0-100,000 [extract]; chars 100,000+ [raw], grepped for named variables only |
| Common workflows | https://code.claude.com/docs/en/common-workflows | [raw], whole page (it points to the Worktrees page) |
| Worktrees | https://code.claude.com/docs/en/worktrees | [raw], whole page |
| Tools reference | https://code.claude.com/docs/en/tools-reference | [raw], whole page saved, grepped and read (Monitor, background commands, PowerShell) |

---

## 1. Hooks

### 1a. The settings.json shape (Verified: https://code.claude.com/docs/en/hooks, https://code.claude.com/docs/en/hooks-guide)

[extract] The nesting:

> Hooks are defined in JSON settings files. The configuration has three levels of nesting:
>
> 1. Choose a [hook event](#hook-events) to respond to, like `PreToolUse` or `Stop`
> 2. Add a [matcher group](#matcher-patterns) to filter when it fires, like "only for the Bash tool"
> 3. Define one or more [hook handlers](#hook-handler-fields) to run when matched

```json
{
  "hooks": {
    "PreToolUse": [
      {
        "matcher": "Bash",
        "hooks": [
          {
            "type": "command",
            "if": "Bash(rm *)",
            "command": "${CLAUDE_PROJECT_DIR}/.claude/hooks/block-rm.sh",
            "args": []
          }
        ]
      }
    ]
  }
}
```

[extract] Handler fields (table rows as returned):

> | `type` | yes | `"command"`, `"http"`, `"mcp_tool"`, `"prompt"`, or `"agent"` |
> | `if` | no | [Permission rule syntax](/docs/en/permissions#permission-rule-syntax) to filter when this hook runs, such as `"Bash(git *)"` or `"Edit(*.ts)"`. ... |
> | `timeout` | no | Seconds before canceling. ... Defaults: 600 for `command`, `http`, and `mcp_tool`; 30 for `prompt`; 60 for `agent`. ... |
> | `statusMessage` | no | Custom spinner message displayed while the hook runs |
> | `once` | no | If `true`, Claude Code removes the hook after its first successful run. ... |
> | `command` | yes | Shell command to execute. With `args`, the executable to spawn directly. See [Exec form and shell form](#exec-form-and-shell-form) |
> | `args` | no | Argument list. When present, `command` is resolved as an executable and spawned directly with `args` as the argument vector, with no shell involved. ... |
> | `async` | no | If `true`, runs in the background without blocking. ... |
> | `asyncRewake` | no | If `true`, runs in the background and wakes Claude on exit code 2. ... |
> | `shell` | no | Shell to use for this hook. Accepts `"bash"` or `"powershell"`. Defaults to `"bash"`, or to `"powershell"` on Windows when Git Bash isn't installed. ... |
> | `onFailure` | no | Whether the action that triggered the hook goes ahead when the hook fails: `"continue"` or `"block"`. ... Requires Claude Code v2.1.295 or later |

[extract] Matcher rules:

> | `"*"`, `""`, or omitted | Match all | fires on every occurrence of the event |
> | Only letters, digits, `_`, `-`, spaces, `,`, and `\|` | Exact string, or list of exact strings separated by `\|` or `,` with optional surrounding whitespace | `Bash` matches only the Bash tool; `Edit\|Write` and `Edit, Write` each match either tool exactly; `code-reviewer` matches only that agent type |
> | Contains any other character | JavaScript regular expression, unanchored | `^Notebook` matches any tool whose name starts with `Notebook`; `mcp__memory__.*` matches every tool from the `memory` server |

> A matcher on the regular-expression path is tested with JavaScript's `RegExp.prototype.test`, which succeeds on a match anywhere in the value. `Edit.*` matches both `Edit` and `NotebookEdit`; wrap the pattern in `^` and `$`, as in `^Edit$`, when you need a whole-string match.

**Two tools at once.** [extract] From the PowerShell tool-input section of the hooks reference:

> Match `Bash|PowerShell` in hooks that inspect shell commands, so they cover both tools.

[raw] (hooks-guide) "Matchers are case-sensitive".

[raw] (hooks-guide) Two handlers in one matcher group, verbatim:

```json
{
  "hooks": {
    "PreToolUse": [
      {
        "matcher": "Bash",
        "hooks": [
          {
            "type": "command",
            "command": "jq -r .tool_input.command >> ~/.claude/bash.log"
          },
          {
            "type": "command",
            "command": "\"$CLAUDE_PROJECT_DIR\"/.claude/hooks/block-rm-rf.sh"
          }
        ]
      }
    ]
  }
}
```

> When Claude tries to run `rm -rf /tmp/build`, both hooks execute in parallel. The logging hook writes the command to `~/.claude/bash.log` and exits 0, which reports no decision. The guardrail hook exits 2, which denies the tool call. The deny takes precedence, so Claude Code blocks the command and shows Claude the guardrail's stderr. The log entry is still written because the logging hook already ran.

> When multiple hooks match the same event, every hook's command runs to completion before Claude Code merges the results. One hook returning `deny` doesn't stop sibling hooks from executing. Don't rely on one hook's `deny` to suppress side effects in another hook.

[extract] Parallelism and duplicates:

> When an event fires, its matching hooks run in parallel. If more than 12 of them are prompt or agent hooks, 12 of those run at once and the rest start as running ones finish. ...
>
> If you define the same handler in more than one settings file, it runs once. A plugin's or skill's copy of the same handler stays separate.

[extract] Where hooks can live:

> | `~/.claude/settings.json` | All your projects | No, local to your machine |
> | `.claude/settings.json` | Single project | Yes, can be committed to the repo |
> | `.claude/settings.local.json` | Single project | No, gitignored when Claude Code saves a setting to it |
> | Managed policy settings | Organization-wide | Yes, admin-controlled |
> | [Plugin](/docs/en/plugins/overview) `hooks/hooks.json` | When plugin is enabled | Yes, bundled with the plugin |
> | [Skill](/docs/en/skills) frontmatter | The rest of the session once the skill is invoked. ... | Yes, defined in the skill file |
> | [Subagent](/docs/en/sub-agents) frontmatter | While that subagent is running | Yes, defined in the subagent file |

[extract] The documented Windows-safe pattern that blocks on failure (from "Block the action when a hook fails"):

```json
{
  "hooks": {
    "PreToolUse": [
      {
        "matcher": "Bash",
        "hooks": [
          {
            "type": "command",
            "command": "node",
            "args": ["${CLAUDE_PROJECT_DIR}/.claude/hooks/check-command.js"],
            "onFailure": "block"
          }
        ]
      }
    ]
  }
}
```

**Composed example (NOT a docs quote; assembled only from the documented keys above):** a guard on both shell tools.

```json
{
  "hooks": {
    "PreToolUse": [
      {
        "matcher": "Bash|PowerShell",
        "hooks": [
          {
            "type": "command",
            "command": "node",
            "args": ["${CLAUDE_PROJECT_DIR}/.claude/hooks/guard.js"],
            "timeout": 30,
            "onFailure": "block"
          }
        ]
      }
    ]
  }
}
```

(`onFailure` needs v2.1.295+ per the docs; the installed version was not checked.)

### 1b. What a PreToolUse hook receives on stdin (Verified: https://code.claude.com/docs/en/hooks)

[extract] Common input fields:

> | `session_id` | Current session identifier |
> | `transcript_path` | Path to conversation JSON. The transcript file is written asynchronously and may lag the in-memory conversation... |
> | `cwd` | Current working directory when the hook is invoked |
> | `permission_mode` | Current [permission mode](/docs/en/permissions#permission-modes): `"default"`, `"plan"`, `"acceptEdits"`, `"auto"`, `"dontAsk"`, or `"bypassPermissions"`. ... |
> | `hook_event_name` | Name of the event that fired |
> | `agent_id` | Unique identifier for the subagent or in-process teammate the hook fires in. |
> | `agent_type` | Agent name (for example, `"Explore"` or `"security-reviewer"`). Present when the session uses `--agent` or the hook fires inside a subagent. ... |

[extract] The permission_mode row continues: "The mode labeled **Manual** arrives as `"default"`, never as `"manual"`, so scripts that match `"default"` keep working. Not all events receive this field."

[extract] PreToolUse input example:

```json
{
  "session_id": "abc123",
  "prompt_id": "550e8400-e29b-41d4-a716-446655440000",
  "transcript_path": "/home/user/.claude/projects/.../transcript.jsonl",
  "cwd": "/home/user/my-project",
  "scratchpad_dir": "/tmp/claude-1000/-home-user-my-project/abc123/scratchpad",
  "permission_mode": "default",
  "hook_event_name": "PreToolUse",
  "tool_name": "Bash",
  "tool_input": {
    "command": "npm test",
    "description": "Run test suite",
    "timeout": 120000,
    "run_in_background": false
  },
  "tool_use_id": "toolu_01ABC123..."
}
```

[extract] Tool input for the two shell tools:

> **Bash** | `command` | string | `"npm test"` | The shell command to execute |
> | `description` | string | `"Run test suite"` | Optional description of what the command does |
> | `timeout` | number | `120000` | Optional timeout in milliseconds. Values above the maximum are reduced to the maximum rather than rejected |
> | `run_in_background` | boolean | `false` | Whether to run the command in background |
>
> **PowerShell**: "The fields match the Bash tool, with the command string in `command`" (`command` example `"Get-ChildItem -Recurse"`).

[raw] (hooks-guide) The short version:

> Every event includes common fields like `session_id`, a unique ID for the session, and `cwd`, the working directory when the event fired, but each event type adds different data. When Claude runs a Bash command, a `PreToolUse` hook receives these fields on stdin:
>
> * `hook_event_name`: the event that triggered the hook
> * `tool_name`: the tool Claude is about to use
> * `tool_input`: the arguments Claude passed to the tool. For Bash, its `command` field holds the shell command.

### 1c. How to block, who sees the reason, and bypass modes (Verified: hooks, hooks-guide)

**Exit 2 + stderr.** [extract]

> Exit with code 2 to block the action. On [events that can block](#exit-code-2-behavior-per-event), Claude Code stops the action: a `PreToolUse` hook blocks the tool call, for example, and a `UserPromptSubmit` hook rejects the prompt.
>
> The message that comes with the block is your hook's stderr. If your hook also printed JSON that makes a blocking decision, Claude Code uses that decision's reason instead.

> Claude never sees stderr from a hook that exits 0. To read it yourself on events such as `PreToolUse`, enable [debug logging](#debug-hooks).

> Exit code 1 without valid JSON output is a non-blocking error unless the hook is one [whose failure blocks the action](#block-the-action-when-a-hook-fails), even though 1 is the conventional Unix failure code. If your hook is meant to enforce a policy, use `exit 2`.

The extractor also reported this line about exit 2 with JSON: "Even a `permissionDecision` of `"allow"` doesn't let the action through."

[extract] The exit-2 table (selected rows):

> | `PreToolUse` | Yes | Blocks the tool call |
> | `PermissionRequest` | No | Exit code 2 isn't honored for this event and the permission flow proceeds unchanged. Deny through the [`decision` object](#permissionrequest-decision-control) instead |
> | `Stop` | Yes | Prevents Claude from stopping, continues the conversation |
> | `SubagentStop` | Yes | Prevents the subagent from stopping |
> | `PostToolUse` | No | Shows stderr to Claude; the tool already ran |
> | `SessionStart` | No | Shows stderr to user only |

[raw] (hooks-guide) The blocking script:

```bash
#!/bin/bash
INPUT=$(cat)
COMMAND=$(echo "$INPUT" | jq -r '.tool_input.command')

if echo "$COMMAND" | grep -q "drop table"; then
  echo "Blocked: dropping tables is not allowed" >&2  # stderr becomes Claude's feedback
  exit 2                                               # exit 2 = block the action
fi

exit 0  # exit 0 = no decision; the normal permission flow applies
```

> * **Exit 0**: if you exit 0 from a `PreToolUse` hook, the tool call still goes through the normal [permission flow](/docs/en/permissions), because exit 0 doesn't approve it. ...

> Use exit 2 to block with a stderr message, or exit 0 with JSON for structured control. Choose one approach per hook. ...

**JSON deny.** [raw] (hooks-guide)

```json
{
  "hookSpecificOutput": {
    "hookEventName": "PreToolUse",
    "permissionDecision": "deny",
    "permissionDecisionReason": "Use rg instead of grep for better performance"
  }
}
```

> With `"deny"`, Claude Code cancels the tool call and feeds `permissionDecisionReason` back to Claude.

[extract] (hooks reference, "PreToolUse decision control"):

> `PreToolUse` hooks can control whether a tool call proceeds. Unlike other hooks that use a top-level `decision` field, PreToolUse returns its decision inside a `hookSpecificOutput` object. This gives it richer control: four outcomes (allow, deny, ask, or defer) plus the ability to modify tool input before execution.

> | `permissionDecision` | `"allow"` skips the permission prompt, except for [actions no mode auto-approves] and [reads from network paths]. ... `"deny"` prevents the tool call. `"ask"` prompts the user to confirm. `"defer"` ends the turn so the tool can be resumed later, and requires [non-interactive mode] with the `-p` flag. [Deny and ask rules] are still evaluated regardless of what the hook returns |
> | `permissionDecisionReason` | For `"ask"`, shown to the user in the permission prompt. When Claude Code [denies the call] in a `-p` run where no one can answer that prompt, Claude reads the reason in the tool result instead. For `"deny"`, shown to Claude. For `"allow"` and `"defer"`, written to the [debug log](#debug-hooks) only |
> | `updatedInput` | Modifies the tool's input parameters before execution. Replaces the entire input object, so include unchanged fields alongside modified ones. ... |
> | `additionalContext` | String added to Claude's context alongside the tool result. ... |

> When multiple PreToolUse hooks return different decisions, precedence is `deny` > `defer` > `ask` > `allow`.

> A hook that blocks by exiting 2 routes the same way as `"deny"`: Claude sees the stderr message as the denial reason.

> A hook's `"ask"` also forces a permission prompt in [auto mode]: the classifier can still deny the tool call, but it can't approve the call silently.

**Answer: both routes show the reason to Claude.** Exit 2: the stderr text. JSON `deny`: `permissionDecisionReason`. If a hook does both, the JSON decision's reason is used.

**Bypass modes.** [raw] (hooks-guide, "Hooks and permission modes"):

> With a hook in a settings file or a plugin's `hooks/hooks.json` you can't loosen restrictions past what permission rules allow, and a hook's `"deny"` holds in every permission mode:

> * **`"deny"` in every permission mode**: a hook that returns `permissionDecision: "deny"` blocks the tool even in `bypassPermissions` mode or with `--dangerously-skip-permissions`. `PreToolUse` hooks fire before any permission-mode check, in every [permission mode](/docs/en/permission-modes), including `dontAsk`. This lets you enforce policy that users can't bypass by changing their permission mode.

The caveat that follows it, [raw]:

> If your machine has no [managed settings] and your session doesn't use [`/login` credentials] for a Team or Enterprise plan, a [mod](/docs/en/plugins/mods/overview) can approve a call your `PreToolUse` hook blocked. ...

**Failure and timeout do NOT block by default.** [extract]

> On most events, when a hook fails or times out, Claude Code still carries out the action. A policy hook with a wrong path or a crashing script then lets everything through. To block the action instead, set `"onFailure": "block"` on a `command` or `http` hook. Requires Claude Code v2.1.295 or later.

> On `PreToolUse`, a timed-out `command`, `http`, or `mcp_tool` hook doesn't block the tool call, except for hooks [whose failure blocks the action]. The call continues through the normal [permission flow], so don't count on a stalled hook to act as a gate.

> * **Exit code other than 0 or 2**: counts for a command hook even if it printed JSON that allows the action, such as `permissionDecision: "allow"`. To return a JSON decision, exit 0

[raw] (hooks-guide) On the `if` filter: "When Claude Code can't determine which commands the Bash input runs, it runs your hook regardless of the pattern. ... Because the filter is best-effort, use the [permission system](/docs/en/permissions) rather than a hook to enforce a hard allow or deny."

[raw] (common-workflows) A gap: "Your [`PreToolUse` hooks](/docs/en/hooks#pretooluse) don't run when Claude Code includes a file you reference with `@`, because it makes no tool call."

### 1d. Environment variables for hook commands, and scripts in the project (Verified: hooks, hooks-guide, env-vars)

[extract] "Reference scripts by path":

> Use these placeholders to reference hook scripts relative to the project or plugin root, regardless of the working directory when the hook runs:
>
> * `${CLAUDE_PROJECT_DIR}`: the project root where the session started. Claude Code also sets this variable in the environment of [stdio MCP servers] and plugin LSP servers.
> * `${CLAUDE_PLUGIN_ROOT}`: the plugin's installation directory, for scripts bundled with a [plugin]. ...
> * `${CLAUDE_PLUGIN_DATA}`: the plugin's [persistent data directory], for dependencies and state that should survive plugin updates.

> Both forms ... export them as the environment variables `CLAUDE_PROJECT_DIR`, `CLAUDE_PLUGIN_ROOT`, and `CLAUDE_PLUGIN_DATA` on the spawned process, so a script can read `process.env.CLAUDE_PLUGIN_ROOT` regardless of how it was launched.

> **Worktrees are different.** If Claude enters a [worktree] during the session, Claude Code keeps `${CLAUDE_PROJECT_DIR}` where it was and passes the worktree path to your hooks a different way:
>
> * **`${CLAUDE_PROJECT_DIR}` stays put**: it still points at the project root where the session started, ...
> * **`cwd` follows Claude**: the `cwd` field in the hook's [input JSON] is the worktree root after Claude enters a worktree, and the new directory after Claude runs `cd`. ...

> Prefer [exec form](#exec-form-and-shell-form) for any hook that references a path placeholder. In shell form, wrap each placeholder in double quotes.

```json
{
  "hooks": {
    "PostToolUse": [
      {
        "matcher": "Write|Edit",
        "hooks": [
          {
            "type": "command",
            "command": "${CLAUDE_PROJECT_DIR}/.claude/hooks/check-style.sh",
            "args": []
          }
        ]
      }
    ]
  }
}
```

[raw] (hooks-guide) The shell-form spelling it uses: `"command": "\"$CLAUDE_PROJECT_DIR\"/.claude/hooks/protect-files.sh"`.

[extract] Other variables:

> A hook process inherits the parent environment, apart from the `OTEL_*` exporter variables that Claude Code [removes from every subprocess it spawns]...

> There is no `$CLAUDE_MODEL` environment variable. The hook can read `$ANTHROPIC_MODEL` if you set it in your shell...

> The `$CLAUDE_CODE_REMOTE` environment variable is `"true"` in remote web environments and not set in the local CLI.

[extract] `CLAUDE_ENV_FILE` ("Persist environment variables"):

> SessionStart hooks have access to the `CLAUDE_ENV_FILE` environment variable, which provides a file path where you can persist environment variables for the shell commands Claude runs later in the session.

> `CLAUDE_ENV_FILE` is available for SessionStart, [Setup](#setup), [CwdChanged](#cwdchanged), and [FileChanged](#filechanged) hooks. Other hook events don't have access to this variable, and neither does a hook that runs in PowerShell, whether through [`"shell": "powershell"`] or by default on Windows without Git Bash.

[extract] (env-vars) `CLAUDE_CODE_CHILD_SESSION`: "Set to `1` in subprocesses Claude Code spawns via the Bash, PowerShell, and Monitor tools, [hook](/docs/en/hooks) commands, and [status line](/docs/en/statusline) commands. ..."

[raw] (hooks reference, "Security best practices"):

> * **Use absolute paths**: specify full paths for scripts. In exec form, use `${CLAUDE_PROJECT_DIR}` and the path needs no quoting. In shell form, wrap it in double quotes

### 1e. Windows: which shell runs hook commands (Verified: hooks, hooks-guide, env-vars, tools-reference)

[extract] (hooks reference, "Exec form and shell form"):

> **Exec form** runs when `args` is present. Claude Code resolves `command` as an executable on `PATH` and spawns it directly with `args` as the argument vector. There is no shell, so each `args` element is one argument exactly as written, ... No shell tokenization happens on any platform.
>
> **Shell form** runs when `args` is absent. The `command` string is passed to a shell: `sh -c` on macOS and Linux, Git Bash on Windows, or PowerShell when Git Bash isn't installed. Set the `shell` field to choose explicitly. The shell tokenizes the string, expands variables, and interprets pipes, `&&`, redirects, and globs.

> On Windows, exec form requires `command` to resolve to a real executable such as a `.exe`. The `.cmd` and `.bat` shims that npm, npx, eslint, and other tools install in `node_modules/.bin` are not executables and can't be spawned without a shell. To run them in exec form, invoke the underlying script with `node` directly, ... The `node` plus script-path pattern works on every platform because `node.exe` is a real binary. To run a `.cmd` or `.bat` shim by name, use shell form.

[extract] The `shell` field row: "Setting `"powershell"` runs the command via PowerShell on Windows. Does not require `CLAUDE_CODE_USE_POWERSHELL_TOOL` since hooks spawn PowerShell directly."

[raw] (hooks reference, "Windows PowerShell tool"):

> On Windows, you can run individual hooks in PowerShell by setting `"shell": "powershell"` on a command hook. Claude Code auto-detects `pwsh.exe`, the PowerShell 7 and later executable, and falls back to `powershell.exe` for Windows PowerShell 5.1.

```json
{
  "type": "command",
  "shell": "powershell",
  "command": "& \"$env:CLAUDE_PROJECT_DIR\\.claude\\hooks\\check.ps1\""
}
```

> Don't write the bare `$CLAUDE_PROJECT_DIR` spelling in a PowerShell hook. PowerShell parses it as an undefined local variable and resolves it to `$null`, which leaves the script path without its project-root prefix. Claude Code doesn't rewrite that form; it logs a warning in the [debug log](#debug-hooks) instead.

[extract] Windows exec-form example from the reference:

```json
{
  "type": "command",
  "if": "PowerShell(Remove-Item *)",
  "command": "powershell.exe",
  "args": ["-NoProfile", "-ExecutionPolicy", "Bypass", "-File", "${CLAUDE_PROJECT_DIR}/.claude/hooks/block-rm.ps1"]
}
```

[extract] ("Failed hooks on Windows without Git Bash"):

> ```text
> blocking because Git Bash wasn't found and this hook doesn't set "shell" to "powershell", so only exit code 0 or 2 counts as its verdict.
> ```
>
> A shell-form hook with no `shell` set runs in PowerShell when Git Bash isn't installed, and a script written for Bash may not run as written there.

[raw] (hooks-guide, "Shell profile output before the JSON"):

> Hooks run in non-interactive shells, but Git Bash and some configurations, such as `BASH_ENV` pointing at `~/.bashrc`, still source your profile, and anything the profile prints reaches stdout ahead of your hook's JSON:

> Unless the hook is [async], Claude Code reads output that doesn't start with `{` as plain text, so your JSON is ignored. Because the hook exited 0, the transcript shows no error either.

[extract] (env-vars):

> `CLAUDE_CODE_GIT_BASH_PATH` | Windows only: path to the Git Bash executable (`bash.exe`). Use when Git Bash is installed but not in your PATH. ...

> `CLAUDE_CODE_POWERSHELL_RESPECT_EXECUTION_POLICY` | Set to `1` to stop Claude Code from passing `-ExecutionPolicy Bypass` when spawning PowerShell for tool calls, hooks, and status line commands, ...

[raw] (tools-reference) The PowerShell tool on Windows: "**Windows with Git Bash installed**: the tool is on by default for claude.ai and Console accounts; set `CLAUDE_CODE_USE_POWERSHELL_TOOL=1` to enable it in Amazon Bedrock, Google Cloud's Agent Platform, and Microsoft Foundry sessions, or `0` to turn it off." And: "On Windows with Git Bash installed, denying Bash also turns the PowerShell tool off for the session."

### 1f. Do edits to hooks take effect in a running session? (Verified: settings, hooks, hooks-guide)

**Yes, normally, by the file watcher; no /hooks review step.**

[raw] (settings, "When edits take effect"):

> Claude Code watches your settings files and reloads them when they change, so it applies most edits to the running session without a restart, including edits to `permissions`, `hooks`, and credential helpers such as `apiKeyHelper`. Claude Code also loads a settings file you create mid-session if its folder existed when the session started. For the project's `.claude/` folder, it loads the file even when you create the folder in the same session.

[extract] (hooks reference): "Direct edits to hooks in settings files are normally picked up automatically by the file watcher." and

> Type `/hooks` in Claude Code to open a read-only browser for your configured hooks.

[raw] (hooks-guide, "`/hooks` shows no hooks configured"):

> * File edits are normally picked up automatically. If they haven't appeared after a few seconds, the file watcher may have missed the change: restart your session to force a reload.
> * Verify your JSON is valid: trailing commas and comments aren't allowed

[raw] (hooks reference, "Workspace trust"):

> * **Interactive session**: Claude Code holds back hooks from every settings file, including your own `~/.claude/settings.json`, until you accept the [workspace trust dialog] for the folder, or for a parent directory whose trust extends to it
> * **`-p` or SDK session**: Claude Code never shows the dialog and treats the folder as trusted, so hooks committed in a repository's `.claude/settings.json` run in a folder you've never trusted

[raw] (hooks reference, ConfigChange): "ConfigChange hooks can block configuration changes from taking effect. ... When blocked, the new settings are not applied to the running session."

[extract] Disabling: "To temporarily disable all hooks without removing them, set `"disableAllHooks": true` in your settings file. ... To turn hooks off for one run whatever the project's settings say, pass `--settings '{"disableAllHooks": true}'` ... There is no way to disable an individual hook while keeping it in the configuration."

### 1g. Do hooks fire for subagents' tool calls? (Verified: hooks, sub-agents)

**Yes.** [extract]

> Hooks from settings files, managed policy settings, and plugins also run inside [subagents]. When a subagent calls a tool, tool events such as `PreToolUse` and `PostToolUse` fire the same configured hooks as in the main conversation, and the input carries the `agent_id` and `agent_type` [common input fields] that identify the subagent.

[extract] Frontmatter hooks:

> * **Subagent hooks**: Claude Code runs them only while that subagent is running and removes them when it finishes. Claude Code converts a `Stop` hook here to `SubagentStop`, the event it fires when a subagent completes.
> * **Skill hooks**: Claude Code registers them when you or Claude invoke the skill and keeps running them for the rest of the session, ...

[extract] (sub-agents) The trust rule for frontmatter hooks:

> Until you trust the folder, the subagent still runs, but Claude Code skips its frontmatter hooks and logs an error to the debug log explaining how to trust the folder. This is a stricter rule than the one for hooks in settings files: trusting a parent folder isn't enough, and a `-p` session doesn't count as trusted.

### 1h. Stop / SubagentStop: blocking the end of a turn, and the loop guard (Verified: hooks, hooks-guide, env-vars)

[extract] Decision control:

> `decision` | `"block"` prevents Claude from stopping. Omit to allow Claude to stop
> `reason` | Required when `decision` is `"block"`. Tells Claude why it should continue

```json
{
  "decision": "block",
  "reason": "Must be provided when Claude is blocked from stopping"
}
```

> Use `additionalContext` when the hook is working as designed and giving Claude guidance... It keeps the conversation going through the same loop protections as `decision: "block"`, namely the `stop_hook_active` input and the 8-consecutive-continuation cap...

```json
{
  "hookSpecificOutput": {
    "hookEventName": "Stop",
    "additionalContext": "Please run the test suite before finishing"
  }
}
```

[extract] Stop input and the guard:

> In addition to the [common input fields](#common-input-fields), Stop hooks receive `stop_hook_active`, `last_assistant_message`, `background_tasks`, and `session_crons`. The `stop_hook_active` field is `true` when Claude Code is already continuing as a result of a stop hook. Check this value or process the transcript to avoid blocking on a condition that will never resolve.

> Claude Code applies an 8-consecutive-continuation cap: after stop hooks have continued the turn eight times in a row, Claude Code overrides the next block and ends the turn. The count of consecutive continuations resets each time Claude calls a tool. To raise the cap, set [`CLAUDE_CODE_STOP_HOOK_BLOCK_CAP`](/docs/en/env-vars).

> The `background_tasks` and `session_crons` arrays let hooks distinguish "session is done" from "session is paused waiting for background work to wake it back up".

```json
{
  "session_id": "abc123",
  "transcript_path": "~/.claude/projects/.../00893aaf-19fa-41d2-8238-13269b9b3ca0.jsonl",
  "cwd": "/Users/...",
  "permission_mode": "default",
  "hook_event_name": "Stop",
  "stop_hook_active": true,
  "last_assistant_message": "I've completed the refactoring. Here's a summary...",
  "background_tasks": [
    {
      "id": "task-001",
      "type": "shell",
      "status": "running",
      "description": "tail logs",
      "command": "tail -f /var/log/syslog"
    }
  ],
  "session_crons": [
    {
      "id": "cron-001",
      "schedule": "0 9 * * 1-5",
      "recurring": true,
      "prompt": "check the build"
    }
  ]
}
```

[extract] SubagentStop:

> In addition to the [common input fields](#common-input-fields), SubagentStop hooks receive `stop_hook_active`, `agent_id`, `agent_type`, `agent_transcript_path`, and `last_assistant_message`. ... The `transcript_path` is the main session's transcript, while `agent_transcript_path` is the subagent's own transcript stored in a nested `subagents/` folder.

> SubagentStop hooks use the same decision control format as [Stop hooks]... Returning `decision: "block"` with a `reason` keeps the subagent running and delivers `reason` to the subagent as its next instruction.

[raw] (hooks-guide, "Stop hook hits the block cap"):

> Claude Code overrides a Stop hook after it blocks eight times in a row with no tool call from Claude in between. Your hook script needs to check whether it already triggered a continuation. Parse the `stop_hook_active` field from the JSON input and exit early if it's `true`:

```bash
#!/bin/bash
INPUT=$(cat)
if [ "$(echo "$INPUT" | jq -r '.stop_hook_active')" = "true" ]; then
  exit 0  # Allow Claude to stop
fi
# ... rest of your hook logic
```

> If your hook legitimately needs more than eight iterations to converge, raise the cap with [`CLAUDE_CODE_STOP_HOOK_BLOCK_CAP`](/docs/en/env-vars).

[raw] (env-vars) `CLAUDE_CODE_STOP_HOOK_BLOCK_CAP` | "Maximum number of consecutive times a [Stop] or [SubagentStop] hook may block the turn from ending before Claude Code overrides it and ends the turn anyway (default: 8). Set to `0` to disable the cap. Raise this if your hook legitimately needs more iterations to resolve"

[raw] (hooks-guide, Limitations): "`Stop` hooks fire whenever Claude finishes responding, not only at task completion. They don't fire on user interrupts. API errors fire [StopFailure](/docs/en/hooks#stopfailure) instead."

[raw] (hooks reference, StopFailure): "StopFailure hooks have no decision control. They run for notification and logging purposes only." Its `error` values include `rate_limit`, `overloaded`, `authentication_failed`, `billing_error`, `server_error`, `max_output_tokens`.

[extract] `onFailure` "has no effect on these hooks: **`Stop`, `SubagentStop`, `TaskCompleted`, and `TeammateIdle` hooks**: exit code 2 on these events sends Claude back to keep working, and Claude can't repair a hook that won't run".

[raw] (hooks-guide) Hook timeouts: "`command`, `http`, `mcp_tool`: 10 minutes. ... `prompt`: 30 seconds. `agent`: 60 seconds."

[raw] (hooks reference, "Debug hooks"): "Start Claude Code with `claude --debug-file <path>` to write the log to a known location, or run `claude --debug` and read the log at `~/.claude/debug/<session-id>.txt`. The `--debug` flag doesn't print to the terminal."

---

## 2. /goal (Verified: https://code.claude.com/docs/en/goal, all [raw])

**What judges the condition, from what:**

> `/goal` is a wrapper around a session-scoped [prompt-based Stop hook](/docs/en/hooks#prompt-based-hooks). Each time Claude finishes a turn, Claude Code sends the condition and the conversation so far to your configured [small fast model](/docs/en/model-config). The model returns one of three verdicts, each with a short reason:
>
> * **Not yet met**: Claude keeps working and takes the reason as guidance for the next turn.
> * **Met**: Claude Code clears the goal and records an achieved entry in the transcript.
> * **Impossible**: the evaluator judged that the condition can never be satisfied. Claude Code clears the goal and records a failed entry in the transcript along with the reason. You don't need to clear it yourself.

> The [evaluator](#how-evaluation-works) judges your condition against what Claude has surfaced in the conversation. It doesn't run commands or read files independently, so write the condition as something Claude's own output can demonstrate.

> The evaluator runs on whichever provider your session is configured for. It does not call tools, so it can only judge what Claude has already surfaced in the conversation.

> To evaluate on a different model, set [`ANTHROPIC_DEFAULT_HAIKU_MODEL`](/docs/en/model-config#environment-variables).

**Character limit:** "The condition can be up to 4,000 characters."

**Writing conditions (the page's advice):**

> A condition that holds up across many turns usually has:
>
> * **One measurable end state**: a test result, a build exit code, a file count, an empty queue
> * **A stated check**: how Claude should prove it, such as "`npm test` exits 0" or "`git status` is clean"
> * **Constraints that matter**: anything that must not change on the way there, such as "no other test file is modified"

> To bound how long a goal runs, include a turn or time clause in the condition, such as `or stop after 20 turns`. Claude reports progress against that clause each turn and the evaluator judges it from the conversation.

Example from the page: `/goal all tests in test/auth pass and the lint step is clean`.

**When the loop stops without the goal resolving:**

> If Claude keeps answering the evaluator without making progress (no tool use for several turns in a row), Claude Code stops the loop, prints a warning, and returns control to you with the goal still set. Evaluation resumes after your next prompt. The [hooks guide](/docs/en/hooks-guide#stop-hook-hits-the-block-cap) explains the underlying mechanism.

(The goal page does not give the number of turns; the linked hooks-guide section gives 8 consecutive blocks with no tool call for Stop hooks in general. Linking the two is my reading, not a docs statement.)

**Errors that clear the goal:**

> If a turn fails on an error that won't clear until you fix it, Claude Code clears the goal and prints a warning naming the cause. The warning starts with `Goal cleared after an unrecoverable error` and ends with `Run /goal again to continue`. ... Four kinds of failure clear the goal:
>
> * An authentication failure, when Claude Code manages its own credentials. ...
> * An exhausted credit balance
> * A context overflow that [auto-compaction] couldn't clear
> * A model that isn't available

**Usage limits: pause and resume.**

> * **Retry**: after a failure that tends to clear on its own, such as an overloaded server or a dropped connection, a notice starting with `Goal still active` shows the wait before the next attempt. After three automatic retries, the goal pauses instead.
> * **Pause**: after a failure that a retry would only repeat, such as an API rate limit, a claude.ai [usage limit](/docs/en/errors#youve-hit-your-session-limit), or a hook that ended the turn, a notice starting with `Goal paused` names the cause. If the session is [waiting to continue automatically when a usage limit resets](/docs/en/interactive-mode#wait-for-a-usage-limit-to-reset), Claude resumes work toward the goal then.

> Send a message at any time to start the next turn immediately. To turn automatic retries off, set [`CLAUDE_CODE_GOAL_CHECKIN_MINUTES`](/docs/en/env-vars) to `0`, which also turns [check-ins](#background-work-defers-evaluation) off.

(The retry/pause lines are "In an interactive session on Claude Code v2.1.269 or later".)

**Background work defers evaluation (relevant to long builds and Players):**

> If a subagent or a background shell command is still running when a turn ends, Claude Code skips the evaluation for that turn. It evaluates at the end of the next turn that finishes with no background work running. When the background work finishes, Claude Code delivers the result to Claude as a new turn, so you don't have to prompt.

> Once background work has kept the goal waiting for 30 minutes, a check-in is due. ... After the first check-in, Claude Code waits twice as long before each later check-in, up to four times the first interval: with the default, 1 hour after the first check-in, then every 2 hours.

> **While the session is idle**: in an interactive session, Claude Code also starts a turn on its own to deliver the check-in instead of waiting for your next prompt. ... Claude Code starts at most three idle check-ins per goal between your prompts. In the third idle check-in, Claude Code says that idle check-ins are paused until you send another prompt.

[extract] (env-vars) `CLAUDE_CODE_GOAL_CHECKIN_MINUTES` | "How many minutes background work can keep an active goal waiting before Claude Code [asks Claude to check on it]. Default `30`. Set `0` to turn check-ins off. Give whole minutes in plain digits, at most `10080`, which is one week. ... Requires Claude Code v2.1.234 or later"

**Clearing:**

> Run `/goal clear` to remove an active goal before it resolves. ... `stop`, `off`, `reset`, `none`, and `cancel` are accepted as aliases for `clear`. Running `/clear` to start a new conversation also removes any active goal.

**Other lines that matter for an unattended run:**

> A goal doesn't change your permission mode. To let goal turns run unattended, run `/goal` in [auto mode](/docs/en/auto-mode-config). In [Manual mode](/docs/en/permission-modes), Claude still asks before tool calls that your settings don't already allow, ...

> While the goal is active, the transcript shows each verdict the evaluator returns, and you can press Ctrl+O to see the reason behind it.

> When you resume a session, Claude Code restores a goal that was still active when the session ended. ... Claude Code carries the condition over but resets the turn count, timer, and token-spend baseline.

> `/goal` is also unavailable when [`disableAllHooks`](/docs/en/hooks#disable-or-remove-hooks) is `true` after settings precedence applies, or when [`allowManagedHooksOnly`] is set in managed settings.

[raw] (best-practices): "**Across a session**: set the check as a [`/goal` condition](/docs/en/goal). A separate evaluator re-checks it after every turn and Claude keeps working until the goal resolves. If Claude stalls, Claude Code eventually stops the run with the goal still set".

---

## 3. /loop, scheduled tasks, and background watching (Verified: https://code.claude.com/docs/en/scheduled-tasks [raw], https://code.claude.com/docs/en/tools-reference [raw], https://code.claude.com/docs/en/goal [raw])

**Does /loop fire during a running turn? No: only between turns, when idle.**

> The scheduler checks every second for due tasks and enqueues them at low priority. A scheduled prompt fires between your turns, not while Claude is mid-response. If Claude is busy when a task comes due, the prompt waits until the current turn ends.

> * Tasks only fire while Claude Code is running and idle. Closing the terminal or letting the session exit stops them firing. ...
> * No catch-up for missed fires. If a task's scheduled time passes while Claude is busy on a long-running request, it fires once when Claude becomes idle, not once per missed interval.

**As a heartbeat for a busy /goal run: Not found.** No page says a `/loop` can act as a heartbeat for `/goal`. What the pages do say: the goal page lists `/loop` and `/goal` as alternatives ("`/loop` | A time interval elapses | You stop it, or Claude decides the work is done"); a scheduled prompt waits for the current turn to end; and `/goal` has its own check-in mechanism for background work (section 2). Whether a due `/loop` prompt gets in between back-to-back goal turns is not stated.

Other /loop facts:

> | Interval and prompt | `/loop 5m check the deploy` | Your prompt runs on a [fixed schedule] |
> | Prompt only | `/loop check the deploy` | Your prompt runs at an [interval Claude chooses] each iteration |
> | Interval only, or nothing | `/loop` | The [built-in maintenance prompt] runs, or your `loop.md` if one exists |

> When you omit the interval, Claude chooses one dynamically ... After each iteration it picks a delay between one minute and one hour based on what it observed ...

> A scheduled fire only runs skills that Claude is [allowed to invoke on its own]. ... Skills marked [`disable-model-invocation: true`] ... reach Claude as plain text instead of executing

> Each scheduled task has an 8-character ID you can pass to `CronDelete`. A session can hold up to 50 scheduled tasks at once.

> Recurring tasks automatically expire 7 days after creation. The task fires one final time, then deletes itself.

> If an iteration ends without either rescheduling or stopping, Claude Code schedules one fallback wakeup about 20 minutes later and ends the loop when that iteration doesn't reschedule either.

> Set `CLAUDE_CODE_DISABLE_CRON=1` in your environment to disable the scheduler entirely.

> | Every 5 minutes, written `*/5 * * * *`, which is what `/loop 5m` creates | 4 minutes 45 seconds after the previous run |
> | Every hour, or less often such as daily | 0 to 30 minutes late |

> * When you resume with `claude --resume` or `claude --continue`, Claude Code doesn't restart the background Bash commands and monitors that were running when the session ended. ...

[extract] (hooks reference) `UserPromptSubmit` hooks also run on "A [session-scoped scheduled task] firing, including a `/loop` iteration" and on "A [background subagent] reporting back to the session that started it".

**Monitor tool (background watching).** [raw] (tools-reference)

> The Monitor tool lets Claude watch something in the background and react when it changes, without pausing the conversation.

> For most watches, Claude writes a small script, runs it in the background, and receives each output line as it arrives. ...
>
> You keep working in the same session and Claude interjects when an event arrives.
>
> Every watch Claude starts has a deadline: 5 minutes by default, at most 30 minutes, and at most 10 minutes in a [non-interactive] run given a single prompt with `-p`.
>
> At the deadline the watch ends. Claude gets one notice, so it can start the watch again if it's still needed.

> When Monitor runs a command, it uses the same [permission rules as Bash], so `allow` and `deny` patterns you have set for Bash apply here too.

> The tool is not available on Amazon Bedrock, Google Cloud's Agent Platform, or Microsoft Foundry. It is also not available when `DISABLE_TELEMETRY` or `CLAUDE_CODE_DISABLE_NONESSENTIAL_TRAFFIC` is set. On Windows, the tool is available only when [Git Bash] is installed.

[raw] (scheduled-tasks): "Monitor runs a background script and streams each output line back, which avoids polling altogether and is often more token-efficient and responsive than re-running a prompt on an interval."

**Background Bash commands.** [raw] (tools-reference)

> For long-running processes such as dev servers or watch builds, Claude can set `run_in_background: true` to start the command as a background task and continue working while it runs. List and stop background tasks with `/tasks`.

> A command that a [foreground subagent] started stops when that subagent's run ends, whether it finished, failed, or was interrupted. A command that the main conversation or a background subagent started keeps running after a final response, until it exits, is stopped, or reaches its [time limit].

> In a session that runs unattended, such as a run with the `-p` flag, an Agent SDK application, a CI job, or a cloud session, background Bash and PowerShell commands have a time limit. A local session you work in from a terminal, the desktop app, or the VS Code extension has no time limit on background commands.

> * A command that Claude starts in the background gets 30 minutes, or the `timeout` Claude passes with `run_in_background`, up to a maximum of 2 hours

> When a foreground command reaches its timeout without finishing, Claude Code moves it to the background instead of stopping it, unless the command starts with `sleep`.

[extract] (env-vars) `BASH_DEFAULT_TIMEOUT_MS` "(default: 120000, or 2 minutes)"; `BASH_MAX_TIMEOUT_MS` "(default: 600000, or 10 minutes)"; `CLAUDE_CODE_DISABLE_BACKGROUND_TASKS` "Set to `1` to disable all background task functionality, including the `run_in_background` parameter on Bash and subagent tools, auto-backgrounding, and the Ctrl+B shortcut".

---

## 4. Subagents (Verified: https://code.claude.com/docs/en/sub-agents [extract], https://code.claude.com/docs/en/env-vars [extract])

**Locations and priority:**

> | Managed settings | Organization-wide | 1 (highest) | Deployed via [managed settings] |
> | `--agents` CLI flag | Current session | 2 | Pass JSON when launching Claude Code |
> | `.claude/agents/` | Current project | 3 | Ask Claude, or create the file manually |
> | `~/.claude/agents/` | All your projects | 4 | Ask Claude, or create the file manually |
> | Plugin's `agents/` directory | Where plugin is enabled | 5 (lowest) | Installed with [plugins] |

> When multiple subagents share the same name, Claude Code uses the one from the higher-priority location.

> Claude Code scans `.claude/agents/` and `~/.claude/agents/` recursively, ... identity comes only from the `name` frontmatter field.

**Frontmatter fields** (only `name` and `description` are required):

| Field | Description (as quoted by the extractor) |
| :- | :- |
| `name` | "Unique identifier of at most 256 characters, such as `code-reviewer` or `reviewer-v2`. [Hooks] receive this value as `agent_type`. The filename doesn't have to match. Names can't contain `:`, ..." |
| `description` | "When Claude should delegate to this subagent" |
| `tools` | "[Tools] the subagent can use, as a comma-separated string such as `Read, Grep, Bash` or a YAML list. Inherits every tool available to subagents if omitted. […]" |
| `disallowedTools` | "Tools to deny, removed from inherited or specified list. Same format as `tools`. An entry with a specifier, such as `Bash(git push *)`, still [removes the whole tool]" |
| `model` | "[Model] to use: `sonnet`, `opus`, `haiku`, `fable`, a full model ID such as `claude-opus-5-5`, or `inherit`. When you omit it, Claude Code picks the model in the [subagent model order]" |
| `permissionMode` | "[Permission mode]: `default`, `acceptEdits`, `auto`, `dontAsk`, `bypassPermissions`, `plan`, or `manual` as an alias for `default`. Ignored for [plugin subagents]" |
| `maxTurns` | "Maximum number of agentic turns before the subagent stops. When the subagent reaches the limit, Claude Code returns its output marked as partial, and Claude can [resume it] to continue. […]" |
| `autoCompactWindow` | "Lower the [auto-compact window] for this subagent to a token count from `100000` to `1000000`. Requires Claude Code v2.1.296 or later" |
| `skills` | "[Skills] to preload into the subagent's context at startup. The full skill content is injected, not only the description. Subagents can still invoke unlisted project, user, and plugin skills through the Skill tool" |
| `mcpServers` | "[MCP servers] available to this subagent. ..." |
| `hooks` | "[Lifecycle hooks] scoped to this subagent. Ignored for [plugin subagents]" |
| `memory` | "[Persistent memory scope]: `user`, `project`, or `local`. Enables cross-session learning" |
| `background` | "Set to `true` to keep this subagent in the background even when Claude asks to wait for it. By default, subagents Claude spawns from the main conversation already [start in the background]" |
| `omitClaudeMd` | "Set to `true` to start this subagent without the user, project, and local CLAUDE.md files in its [initial context]; […] Requires Claude Code v2.1.271 or later" |
| `effort` | "Effort level when this subagent is active. Overrides the session effort level, but not the `CLAUDE_CODE_EFFORT_LEVEL` environment variable. Options: `low`, `medium`, `high`, `xhigh`, `max`; ..." |
| `isolation` | "Set to `worktree` to run the subagent in a temporary [git worktree], giving it an isolated copy of the repository branched by default from your [default branch] rather than the parent session's `HEAD`. The worktree is automatically cleaned up if the subagent makes no changes" |
| `color` | "Display color for the subagent in the task list and transcript. ..." |
| `initialPrompt` | "Auto-submitted as the first user turn when this agent runs as the main session agent (via `--agent` or the `agent` setting). ..." |
| `experimental` | "Map of experimental options. Set its `cacheTtl` key to `5m` or `1h` ... Requires Claude Code v2.1.248 or later" |

[raw] (best-practices) Example file:

```markdown
---
name: security-reviewer
description: Reviews code for security vulnerabilities
tools: Read, Grep, Glob, Bash
model: opus
---
You are a senior security engineer. Review code for:
...
```

**How tools are restricted:**

> To restrict tools, use the `tools` field as an allowlist or the `disallowedTools` field as a denylist. If both are set, `disallowedTools` is applied first, then `tools` is resolved against the remaining pool. A tool listed in both is removed.

> The `Agent(agent_type)` allowlist syntax applies only to an agent running as the main thread with `claude --agent`. In a subagent definition, listing `Agent` in `tools` lets that subagent spawn subagents of its own while the [depth limit] allows it, but any type list inside the parentheses is ignored.

**Permission mode vs the parent:**

> * When the main conversation is in `bypassPermissions`, `acceptEdits`, or [auto mode], the subagent runs in that same mode and Claude Code ignores the `permissionMode` you set. ...
> * When the main conversation is in `default`, `dontAsk`, or `plan` mode, the subagent runs in the permission mode you set. ...

> * **Background subagents** run concurrently while you continue working. When a background subagent reaches a tool call that needs permission, Claude Code surfaces the prompt in your main session and names the subagent that is asking. ...

> With nothing configured, a subagent that Claude spawns from your main conversation starts in the background.

> A background subagent's results reach Claude as a completion notification in a later turn.

**What a subagent sees:** "Each subagent starts with a fresh, isolated context window. It doesn't see your conversation history, the skills you've already invoked, or the files Claude has already read." Its context gets "every level of the CLAUDE.md hierarchy the main conversation loads..." (Explore and Plan skip it).

**Caps and related environment variables (exact names):**

> `CLAUDE_CODE_MAX_CONCURRENT_SUBAGENTS` | How many [subagents] can be running in one session before the Agent tool refuses to spawn another (default: 20). Accepts a positive whole number in plain digits; anything else is ignored, so the variable can adjust the cap but can't disable it. Requires Claude Code v2.1.217 or later

> By default, when 20 subagents are running in a session, spawning another with the Agent tool fails with `Concurrent subagent limit reached`, and the error tells Claude not to retry. ... There's no limit on the total number of subagents Claude can spawn over a session.

> `CLAUDE_CODE_MAX_SUBAGENT_SPAWN_DEPTH` | Number of [subagent layers] allowed below the main conversation (default: 3). ... set `1` to turn nesting off. ...

> `CLAUDE_CODE_MAX_SUBAGENTS_PER_SESSION` | Removed in v2.1.224 and now a no-op. Previously capped the total number of [subagents] Claude could spawn with the Agent tool in one session (default: 200); ...

> `CLAUDE_CODE_DISABLE_BACKGROUND_TASKS` | Set to `1` to disable all background task functionality, including the `run_in_background` parameter on Bash and subagent tools, auto-backgrounding, and the Ctrl+B shortcut

> `CLAUDE_AUTO_BACKGROUND_TASKS` | Set to `1` to force-enable automatic backgrounding of long-running agent tasks. When enabled, a [subagent] moves to the background after running for approximately two minutes. ...

> `CLAUDE_ASYNC_AGENT_STALL_TIMEOUT_MS` | Stall timeout in milliseconds for subagents. ... Default `600000` (10 minutes); ...

> `CLAUDE_CODE_FORK_SUBAGENT` | Controls fork mode, which is on by default and lets Claude start [forks] on its own. While fork mode is on, Claude also can't [wait for a subagent] in interactive terminal, VS Code, or desktop sessions on your machine. Set to `0` to turn fork mode off. ...

> `CLAUDE_CODE_MAX_TURNS` | Cap the number of agentic turns when no explicit limit is passed. Equivalent to passing [`--max-turns`], which takes precedence when both are set. ...

Also named on the pages: `CLAUDE_CODE_SUBAGENT_MODEL`, `CLAUDE_CODE_SUBAGENT_MODEL_FORCE` ("Set to `1` to force one model onto subagents, teammates, and workflow agents"), `CLAUDE_CODE_DISABLE_EXPLORE_PLAN_AGENTS`, `CLAUDE_AGENT_SDK_DISABLE_BUILTIN_AGENTS`, `CLAUDE_CODE_PRINT_BG_WAIT_CEILING_MS`.

**Edits to agent files mid-session:**

> Claude Code watches `~/.claude/agents/` and `.claude/agents/`. When you add or edit a subagent file on disk, or ask Claude to write one for you, Claude Code detects the change within a few seconds and the next delegation uses the updated definition, with no restart needed.

> * The watcher covers only directories that existed when the session started, so after creating a scope's first agent file in a new `agents` directory, restart to load it.

---

## 5. Skills (Verified: https://code.claude.com/docs/en/skills [extract], best-practices [raw])

**Locations:**

> | Enterprise | `.claude/skills/<skill-name>/SKILL.md` in the managed settings directory |
> | Personal | `~/.claude/skills/<skill-name>/SKILL.md` |
> | Project | `.claude/skills/<skill-name>/SKILL.md` |
> | Nested | `<subdir>/.claude/skills/<skill-name>/SKILL.md` |
> | Plugin | `<plugin>/skills/<skill-name>/SKILL.md` |

> Enterprise over personal, and personal over project. With `deploy` in both `~/.claude/skills/` and the project's `.claude/skills/`, `/deploy` runs the personal one.

**Format:**

```yaml
---
name: my-skill
description: What this skill does
disable-model-invocation: true
allowed-tools: Read Grep
---

Your skill instructions here...
```

Selected fields as quoted: `name` "Command name shown in the `/` menu. Defaults to the directory name."; `description` "What the skill does and when to use it. Claude uses this to decide when to apply the skill. ... the combined `description` and `when_to_use` text is truncated at 1,536 characters in the skill listing to reduce context usage."; `disable-model-invocation` "Set to `true` to prevent Claude from automatically loading this skill. ..."; `user-invocable` "Set to `false` when only Claude should invoke the skill ..."; `allowed-tools` "Tools Claude can use without asking permission during the turn that invokes this skill. The grant clears when you send your next message."; `context` "Set to `fork` to run in a forked subagent context."; `agent` "Which subagent type to use when `context: fork` is set."; `hooks`; `paths` "Glob patterns that limit when this skill is activated..."; also `when_to_use`, `argument-hint`, `model`, `effort`. "Claude Code ignores a field it doesn't recognize without reporting an error."

**Triggering:** "Claude uses skills when relevant, or you can invoke one directly with `/skill-name`." and "In a regular session, skill descriptions are loaded into context so Claude knows what's available, but full skill content only loads when invoked."

**Live edits:** "Claude Code watches skill directories for file changes, except in bare mode. When you add, edit, or remove a skill under `~/.claude/skills/`, the project `.claude/skills/`, ... Claude Code picks up the change within the current session, without a restart." Exception: "If you create a top-level skills directory that didn't exist when the session started, run `/reload-skills`..."

**Size:** "Keep `SKILL.md` under 500 lines. Move detailed reference material to separate files." and "Once a skill loads, its content stays in context across turns, so every line is a recurring token cost."

**Lifecycle:** "the rendered `SKILL.md` content enters the conversation as a single message and stays there across later turns... Claude Code does not re-read the skill file on later turns." On compaction: "Claude Code re-attaches the most recent invocation of each skill after the summary, keeping the first 5,000 tokens of each. Re-attached skills share a combined budget of 25,000 tokens."

[raw] (best-practices): "Use `disable-model-invocation: true` for workflows with side effects that you want to trigger manually."

---

## 6. Best practices (Verified: https://code.claude.com/docs/en/best-practices, all [raw])

**Verification:**

> Give Claude a check it can run: tests, a build, a screenshot to compare. It's the difference between a session you watch and one you walk away from.

> Claude stops when the work looks done. Without a check it can run, "looks done" is the only signal available, and you become the verification loop: every mistake waits for you to notice it. Give Claude something that produces a pass or fail, and the loop closes on its own.

> * **As a deterministic gate**: a [Stop hook](/docs/en/hooks#stop) runs your check as a script and blocks the turn from ending until it passes. [Stop input](/docs/en/hooks#stop-input) covers the cap on consecutive blocks.
> * **By a second opinion**: a [verification subagent](/docs/en/sub-agents) or a [dynamic workflow](/docs/en/workflows) that checks its own findings has a fresh model try to refute the result, so the agent doing the work isn't the one grading it.

> Each step trades setup for attention. The prompt version works on any task today. The `/goal` and Stop hook versions are what let an unattended run finish correctly without you.

**Evidence over claims:**

> Have Claude show evidence rather than asserting success: the test output, the command it ran and what it returned, or a screenshot of the result. Reviewing evidence is faster than re-running the verification yourself, and it works for sessions you weren't watching.

> * **The trust-then-verify gap.** Claude produces a plausible-looking implementation that doesn't handle edge cases.
>   > **Fix**: Always provide verification (tests, scripts, screenshots). If you can't verify it, don't ship it.

**CLAUDE.md short:**

> There's no required format for CLAUDE.md files, but keep it short and human-readable.

> Keep it concise. For each line, ask: *"Would removing this cause Claude to make mistakes?"* If not, cut it. Bloated CLAUDE.md files cause Claude to ignore your actual instructions!

> If Claude keeps doing something you don't want despite having a rule against it, the file is probably too long and the rule is getting lost. ... Treat CLAUDE.md like code: review it when things go wrong, prune it regularly, and test changes by observing whether Claude's behavior actually shifts.

> If Claude keeps skipping one instruction, add emphasis such as "IMPORTANT" to that line alone. If you emphasize many lines, none of them stands out.

> CLAUDE.md is loaded every session, so only include things that apply broadly. For domain knowledge or workflows that are only relevant sometimes, use [skills](/docs/en/skills) instead.

> * **The over-specified CLAUDE.md.** If your CLAUDE.md is too long, Claude ignores half of it because important rules get lost in the noise.
>   > **Fix**: Ruthlessly prune. If Claude already does something correctly without the instruction, delete it or convert it to a hook.

**Hooks vs CLAUDE.md:**

> Use hooks for actions that must happen every time with zero exceptions.
>
> [Hooks](/docs/en/hooks-guide) run scripts automatically at specific points in Claude's workflow. Unlike CLAUDE.md instructions which are advisory, hooks are deterministic and guarantee the action happens.

[raw] (hooks-guide): "Hooks are user-defined shell commands. Claude Code runs them at specific points in its lifecycle, which gives you deterministic control: certain actions always happen rather than relying on the LLM to choose to run them."

**Long-running and unattended work:**

> The longer Claude works unattended, the more an independent check matters before you count the work as done. A reviewer running in a fresh [subagent] context sees only the diff and the criteria you give it, not the reasoning that produced the change, so it evaluates the result on its own terms.

> A reviewer prompted to find gaps will usually report some, even when the work is sound, because that is what it was asked to do. Chasing every finding leads to over-engineering ... Tell the reviewer to flag only gaps that affect correctness or the stated requirements, and treat the rest as optional.

> For uninterrupted execution with background safety checks, use [auto mode]. A classifier model reviews commands before they run, blocking scope escalation, unknown infrastructure, and hostile-content-driven actions while letting routine work proceed without prompts.

> Most best practices are based on one constraint: Claude's context window fills up fast, and performance degrades as it fills.

> Since context is your fundamental constraint, use subagents to keep research out of it.

> If you've corrected Claude more than twice on the same issue in one session, the context is cluttered with failed approaches. Run `/clear` and start fresh with a more specific prompt that incorporates what you learned.

> * Customize compaction behavior in CLAUDE.md with instructions like `"When compacting, always preserve the full list of modified files and any test commands"` to ensure critical context survives summarization

> The most useful specs are self-contained: they name the files and interfaces involved, state what is out of scope, and end with an end-to-end verification step that proves the feature works.

> Checkpoints only track changes made through Claude's file editing tools. Changes made through Bash commands or external processes are not captured. This isn't a replacement for git.

---

## 7. Worktrees (Verified: https://code.claude.com/docs/en/worktrees [raw]; https://code.claude.com/docs/en/common-workflows [raw] only points there)

**Creation:**

> Pass `--worktree` or `-w` with a name to create an isolated worktree and start Claude in it. By default, the worktree is created under `.claude/worktrees/<name>/` at your repository root, on a new branch named `worktree-<name>`:

> Add `.claude/worktrees/` to your `.gitignore` so worktree contents don't appear as untracked files in your main checkout.

> A worktree is a fresh checkout, so initialize your development environment there: ask Claude to install dependencies, or run your project's setup yourself in the worktree directory under `.claude/worktrees/`.

**Subagent worktrees (`isolation: worktree`):**

> Subagents can run in their own worktrees so parallel edits don't conflict. Ask Claude to "use worktrees for your agents", or make the isolation permanent for a [custom subagent] by adding `isolation: worktree` to its frontmatter.

> Each subagent gets a temporary worktree that Claude Code removes automatically when the subagent finishes without changes; a worktree with changes stays on disk until the [periodic sweep below] can remove it without losing work.

> Subagent worktrees use the same [base branch] as `--worktree`, so they branch from your repository's default branch unless `worktree.baseRef` is set to `"head"`.

> * `"fresh"` (default): branch from the repository's default branch on the remote, usually `main`, so the worktree starts from a clean tree matching the remote.
> * `"head"`: branch from your current local `HEAD`, so the worktree carries your unpushed commits and feature-branch state. Use this when isolating subagents that need to operate on in-progress work.

```json
{
  "worktree": {
    "baseRef": "head"
  }
}
```

> A subagent in its own worktree takes the instruction files it [starts with] from your main conversation, not from its worktree.

**Cleanup:**

> Claude Code runs a periodic sweep that removes worktrees that Claude created for subagents and [background sessions] once they are older than your [`cleanupPeriodDays`] setting, ...

> The sweep leaves a worktree in place in these cases:
>
> * The worktree still holds work: changed or untracked files, or unpushed commits.

> While an agent is running, Claude Code holds a `git worktree lock` on its worktree so that concurrent cleanup can't remove it, and releases the lock when the agent finishes.

> To clean up a worktree that the sweep keeps, run `git worktree remove`, adding `--force` if the worktree has uncommitted changes or untracked files. If git refuses because the worktree is locked, run `git worktree unlock` on it first.

> Non-interactive runs with `-p` have no exit prompt, so Claude doesn't clean up their worktrees, ...

Interactive `--worktree` exit: "**The worktree is clean**: for an unnamed session, Claude removes the worktree and its branch automatically. ..." and "**The worktree has work in it**: Claude prompts you to keep or remove the worktree."

**Windows note:**

> On Windows, removing a worktree doesn't delete files outside it. If a folder inside the worktree is a link to somewhere else, such as an NTFS junction or a directory symlink, Claude Code deletes only the link and keeps the folder it points to. Before v2.1.205, removing a worktree with a link nested in a subdirectory could delete the folder it pointed to.

**Isolation enforcement (applies to subagents in worktrees too):**

> * **File edits**: Claude Code blocks an `Edit`, `Write`, or `NotebookEdit` that targets a path in the main checkout.
> * **Command working directory**: Claude Code blocks a Bash, PowerShell, or Monitor command whose working directory resolves to the main checkout, or whose working directory it can't verify stays outside it.

> None of them tracks which files a shell command writes, so a command that writes into the main checkout without running git there, such as `cp` or a shell redirect, isn't refused by them.

**Gitignored files:** "To copy them automatically when Claude creates a worktree, add a `.worktreeinclude` file to your project root. The file uses `.gitignore` syntax. Only files that match a pattern and are also gitignored are copied, so tracked files are never duplicated."

**Disk notes: Not found.** The Worktrees page as read has no disk-usage statement, and no `worktree.sparsePaths` or `worktree.symlinkDirectories` setting appeared in its text (I asked for both).

---

## Not found / not checked

- A docs statement that `/loop` can serve as a heartbeat for a busy `/goal` run: **Not found** (section 3).
- The exact number of no-tool-use turns after which `/goal` stops: **not stated on the goal page**; the hooks guide gives 8 for Stop hooks in general.
- Worktree disk usage, sparse checkout, symlinked directories: **Not found** on the Worktrees page.
- The installed Claude Code version on this machine: **not checked**. Version gates seen: `onFailure` v2.1.295+, goal check-ins v2.1.234+, retry/pause notices v2.1.269+, idle check-in cap v2.1.246+, concurrent-subagent limit v2.1.217+.
- Not read: hooks reference chars 200,000-272,937 except the sections I grepped (ConfigChange, StopFailure, Security, Windows PowerShell tool, Debug); sub-agents last 10,778 chars; skills last 11,505 chars; env-vars chars 100,000+ except grepped rows.

## What this means for the forge (my inference, NOT docs quotes)

1. A `PreToolUse` hook with matcher `Bash|PowerShell` in `.claude/settings.json` is the documented hard gate. Its deny holds under `bypassPermissions` and `--dangerously-skip-permissions`, it fires inside subagents, and Claude sees the reason (stderr on exit 2, or `permissionDecisionReason`).
2. A guard that crashes, times out or exits 1 does **not** block unless `"onFailure": "block"` is set, which needs v2.1.295+ (version not checked). So test the guard by piping sample JSON before the run.
3. On this Windows machine, a shell-form hook runs in Git Bash, which sources the profile. A profile `echo` silently breaks JSON output. Exec form (`"command": "node", "args": [...]`) avoids the shell entirely.
4. Hook edits apply mid-session through the file watcher, with no `/hooks` review step. An interactive session holds hooks back until the folder's workspace trust is accepted; this folder's trust state was not checked.
5. The `/goal` evaluator reads only the transcript. Every gate must print its verdict line into the conversation. On usage limits the goal pauses, and it resumes only if the session is waiting for the reset. Credit exhaustion, an auth failure or a context overflow clears it. While subagents or background shells run, evaluation is skipped; check-ins come at 30 min, then 1 h, then every 2 h, with at most 3 idle check-ins between prompts.
6. `/loop` fires only between turns, with no catch-up, so it is not a heartbeat inside long turns. In a local terminal session, background commands have no time limit. Monitor watches end at 5 min by default (30 min max), and on Windows Monitor needs Git Bash.
7. Subagent worktrees branch from the **default branch (`main` here), not `HEAD` (`port/stage3`)**, unless `worktree.baseRef` is `"head"`. Worktrees with changes stay until the `cleanupPeriodDays` sweep. A fresh checkout of a Unity project would also start without its `Library/` folder; that last point is my inference, since the docs only say "A worktree is a fresh checkout".
8. Caps: 20 concurrent subagents, nesting depth 3, and no total-per-session cap any more (`CLAUDE_CODE_MAX_SUBAGENTS_PER_SESSION` is a no-op). If the parent runs in `bypassPermissions` or auto mode, the subagent runs in that mode and its own `permissionMode` is ignored.
