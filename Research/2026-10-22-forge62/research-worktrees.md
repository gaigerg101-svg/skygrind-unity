# Sparse worktrees for parallel agents (forge62 research)

Read on 2026-10-22. **Verified** means I read that page or source file in this session. **Inferred** means it is my reasoning from the cited pages. No git command was run and nothing was measured on this machine.

## The answer in 10 lines

1. **Best fit for this disk.** Put `{"worktree": {"baseRef": "head", "sparsePaths": ["Assets/Scripts", "Assets/Tests", "Tools", "Design", "PortExport"]}}` in the repo root's `.claude/settings.local.json`. Then check on one isolated builder that its worktree really is sparse, and remove each builder worktree once its commits are cherry-picked. (The plan is Inferred; each piece is Verified below: [cc-settings-ref], [cc-large], [cc-worktrees].)
2. **By hand it is three commands, and the first two write no tracked files:** `git worktree add --no-checkout -b <branch> <path> <commit>` (or `--detach`), then `git -C <path> sparse-checkout set --cone <dirs>`, then `git -C <path> checkout`. (Verified: [git-worktree], [src-worktree], [src-sparse], [src-checkout].)
3. **What cone mode includes.** It also writes every root-level file and every file directly inside the parent folders of each listed folder. Here that means all files directly in `Assets/`, such as the folder `.meta` files. It takes whole directories only, and it leaves out other root folders such as `.claude/` and `docs/` unless you list them. To get the cone's size in bytes without changing anything, run `git ls-tree -r -l HEAD -- <dirs>`. (Verified: [git-sparse], [git-ls-tree]. The `.meta` reading is Inferred.)
4. **The main checkout stays full.** `set` turns on `extensions.worktreeConfig` in the shared `.git/config`. Older Git versions refuse repos with that extension, and it broke go-git tools. The patterns and `core.sparseCheckout` are kept per worktree. `git worktree add` copies the sparsity of the worktree you run it from: from the full main checkout it copies nothing, and from a sparse worktree the new one gets the same cone. (Verified: [git-sparse], [git-extensions], [git-worktree], [src-worktree], [cc-changelog].)
5. **Claude Code has this built in.** `worktree.sparsePaths` (added in v2.1.76) writes "only those directories plus root-level files". The settings reference says the `worktree` keys apply to `--worktree`, EnterWorktree, isolated subagents and background sessions, with one list shared by every worktree in a session. (Verified: [cc-settings-ref], [cc-large], [cc-changelog].)
6. **Test it once before relying on it.** Issue #35149, filed on v2.1.76, measured EnterWorktree ignoring `sparsePaths` (about 11 GB full against about 1 GB sparse) and left `Agent(isolation: "worktree")` untested. A bot closed it, and my scan of the changelog found no line saying when that changed. (Verified: [issue-35149], [cc-changelog]. The need to test is Inferred.)
7. **Set `baseRef` to `"head"`.** The default `worktree.baseRef: "fresh"` branches every subagent worktree from origin's default branch (`main` here), not from the lead's HEAD (`port/stage3`). `"head"` fixes that. The key cannot name a branch. (Verified: [cc-worktrees], [cc-settings-ref].)
8. **A custom creation command exists, with drawbacks.** A `WorktreeCreate` hook replaces git worktree creation for `--worktree`, EnterWorktree, `isolation: "worktree"` and background sessions. It gets `name` and `cwd` on stdin and prints the path as its last stdout line. But it has no matcher and no per-agent input, `.worktreeinclude` stops working, and Claude Code never removes a worktree that such a hook made for a subagent. (Verified: [cc-hooks], [cc-worktrees].)
9. **Worktrees pile up.** A subagent worktree with changes waits for the `cleanupPeriodDays` sweep (default 30 days), and the sweep keeps any worktree with unpushed commits. This project never pushes, so a builder worktree stays until someone runs `git worktree remove`. (The rules are Verified: [cc-worktrees], [cc-settings-ref]. The consequence for this project is Inferred.) Reported elsewhere: 764 worktrees took about 36 GB, 27 took about 80 GB, 13 took about 13.8 GB, and 2 Cursor worktrees took 9.82 GB. (Verified: [issue-58914], [issue-63087], [issue-26725], [cursor-forum].)
10. **Other cheap copies.** A worktree already shares the object store. `git clone --shared --no-checkout` plus sparse also borrows objects, through alternates, but the clone is corrupted if the source prunes objects it needs. `git clone --local` hardlinks objects. `--filter=blob:none` is ignored for local-path clones and downloads every commit and tree. A Dev Drive's copy-on-write block cloning needs a 50 GB volume. (Verified: [git-worktree], [git-clone], [src-clone], [gh-partial], [ms-devdrive]. That a `--shared` clone keeps the sparse config out of the main repo is Inferred.)

---

## 1. A sparse worktree by hand (git, cone mode)

### 1.1 The command sequence

The lead would run this from the main checkout. I did not run it.

```bash
# 1. register the worktree and set its HEAD; no tracked file is written
git worktree add --no-checkout -b agent/a1 <path> port/stage3
#    or with a detached HEAD and no new branch:
#    git worktree add --no-checkout --detach <path> port/stage3
# 2. write the cone (patterns + per-worktree config); still no tracked file is written
git -C <path> sparse-checkout set --cone Assets/Scripts Assets/Tests Tools Design PortExport
# 3. fill the working tree, cone only
git -C <path> checkout
# read-only check: prints the cone's directories
git -C <path> sparse-checkout list
```

Why each step behaves this way:

- **`--no-checkout`**: "By default, `add` checks out <commit-ish>, however, `--no-checkout` can be used to suppress checkout in order to make customizations, such as configuring sparse-checkout." Verified: https://git-scm.com/docs/git-worktree
- **What the checkout is.** In the source, the checkout in `add_worktree()` is `reset --hard --no-recurse-submodules`, and it runs only `if (opts->checkout ...)`. Verified: https://raw.githubusercontent.com/git/git/master/builtin/worktree.c
- **`-b` / `-B` and `--detach`.** `-b` creates `<new-branch>` at `<commit-ish>`, which defaults to HEAD. `--detach` detaches HEAD in the new worktree. Verified: https://git-scm.com/docs/git-worktree
- **What `set` does.** "Enable the necessary sparse-checkout config settings (`core.sparseCheckout`, `core.sparseCheckoutCone`, and `index.sparse`) ..., populate the sparse-checkout file from the list of arguments ..., and update the working directory to match." Verified: https://git-scm.com/docs/git-sparse-checkout
- **Why step 2 writes no files.** On an unborn index, `update_working_directory()` returns early: "/* If no branch has been checked out, there are no updates to make. */" Verified: https://raw.githubusercontent.com/git/git/master/builtin/sparse-checkout.c. The link between `--no-checkout` and an unborn index is Inferred from that file and the worktree source above.
- **Cone mode is the default.** "The "cone mode", which is the default". The `--cone` flag is still accepted: `reapply` "can also take `--[no-]cone` ... with the same meaning as the flags from the set command". Verified: https://git-scm.com/docs/git-sparse-checkout
- **Non-cone mode is deprecated.** Git 2.37.0 release notes: "Deprecate non-cone mode of the sparse-checkout feature." Verified: https://raw.githubusercontent.com/git/git/master/Documentation/RelNotes/2.37.0.adoc
- **Skip `git sparse-checkout init`.** It is a "Deprecated command that behaves like `set` with no specified paths". In the old init-then-set flow, "the `init` step would first remove nearly all tracked files". Verified: https://git-scm.com/docs/git-sparse-checkout
- **Why a bare `git checkout` fills the cone.**
  - With no arguments, `git checkout` re-merges the working tree against HEAD. Its two-way merge sets `topts->initial_checkout = is_index_unborn(the_repository->index);`, which is the path that fills an empty index. Verified: https://raw.githubusercontent.com/git/git/master/builtin/checkout.c
  - "git read-tree and other merge-based commands (git merge, git checkout…) can help maintaining the skip-worktree bitmap and working directory update". Verified: https://git-scm.com/docs/git-read-tree
  - A user reported in October 2020 that this exact sequence, with the old `init` step, "works". Verified: https://www.linux.org.ru/forum/development/15962830
- **Optional `--sparse-index`.** It "reduces the size of the index to be more closely aligned with your sparse-checkout definition". But "This feature is still experimental", and it is "not completely understood by external tools". Verified: https://git-scm.com/docs/git-sparse-checkout. Leaving it off is Inferred to be the safer default here.
- **Hand the worktree over only after step 3.** Until then the index is empty. Inferred from the worktree source.

### 1.2 What cone mode puts on disk

- **The rule.** "For any directory specified, all paths below that directory will be included, and any paths immediately under leading directories (including the toplevel directory) will also be included." Its example, `Documentation/technical/`, includes all top-level files, all files immediately under `Documentation/`, and all files at any depth under `Documentation/technical/`. Verified: https://git-scm.com/docs/git-sparse-checkout
- **Applied to the five folders.** Included: all root-level files (CLAUDE.md, HANDOFF.md, tasks.json and the logs), all files directly in `Assets/` (the folder `.meta` files sit there), and everything under the five folders. Left out: `docs/`, `.claude/`, `Packages/`, `ProjectSettings/` and the other `Assets/` subfolders. Inferred from the rule.
- **Only tracked files are affected.** Sparse checkout works through the skip-worktree bit on index entries. Verified: https://git-scm.com/docs/git-sparse-checkout. Untracked folders such as Unity's `Library/` are never in a fresh checkout anyway. Inferred. Claude Code's own worktrees check out "only tracked files". Verified: https://code.claude.com/docs/en/worktrees.md
- **Claude Code's guidance for `sparsePaths`.** "List directories in `sparsePaths`, not individual files. Root-level files like `package.json` ... are always checked out alongside the directories you list. Root-level directories are not, so include `.claude` in the list if you want the repository root's `.claude/settings.json` or `.claude/rules/` available inside the worktree." Verified: https://code.claude.com/docs/en/large-codebases.md
- **Size the cone before creating anything (read-only).**
  - `git ls-tree -r -l HEAD -- Assets/Scripts Assets/Tests Tools Design PortExport` prints each file's size. `-l` means "Show object size of blob (file) entries", and the size "is given in bytes". Verified: https://git-scm.com/docs/git-ls-tree
  - `git ls-tree -l HEAD` and `git ls-tree -l HEAD Assets/` cover the root and `Assets/` files. Summing the size column gives the checkout size. Inferred.
  - Git LFS pointer files would show the pointer's size, not the asset's size. Inferred.

### 1.3 Working inside a sparse worktree

- **Experimental.** "THIS COMMAND IS EXPERIMENTAL. ITS BEHAVIOR, AND THE BEHAVIOR OF OTHER COMMANDS IN THE PRESENCE OF SPARSE-CHECKOUTS, WILL LIKELY CHANGE IN THE FUTURE." Verified: https://git-scm.com/docs/git-sparse-checkout
- **Branch switches and `commit -a` ignore paths outside the cone.** "switching branches will not update paths outside the sparse-checkout directories/patterns, and `git commit -a` will not record paths outside the sparse-checkout directories/patterns as deleted." Verified: same page.
- **`git add` refuses paths outside the cone.** It "refuses to update index entries whose paths do not fit within the sparse-checkout cone" unless you pass `--sparse`. Verified: https://git-scm.com/docs/git-add
- **Merges and rebases can bring outside files back.** "Commands like merge or rebase can materialize paths to do their work (e.g. in order to show you a conflict)". `git sparse-checkout reapply` re-sparsifies afterwards. Verified: https://git-scm.com/docs/git-sparse-checkout
- **Changing the cone can leave folders behind.** Git deletes a folder that has left the cone only if all its untracked files are ignored. Otherwise "no deletions will occur within that directory and a warning message will appear". Verified: same page.
- **Case-insensitive matching.** With `core.ignoreCase=true`, the pattern matching is case-insensitive. Verified: same page.
- **Builders' tools must stay inside the cone.** Any script, compile gate or test that a builder runs in the worktree must read only paths inside the cone. Inferred.

### 1.4 How it interacts with the main checkout's settings

- **What is shared.** "The new worktree is linked to the current repository, sharing everything except per-worktree files such as `HEAD`, `index`, etc." In a linked worktree, `$GIT_DIR` is `.git/worktrees/<id>` and `$GIT_COMMON_DIR` is the main `.git`. "By default, the repository `config` file is shared across all worktrees." Verified: https://git-scm.com/docs/git-worktree
- **What `set` changes.** It "will upgrade your repository config to use worktree-specific config if not already present. The sparsity defined by the arguments to the set subcommand are stored in the worktree-specific sparse-checkout file." Verified: https://git-scm.com/docs/git-sparse-checkout
  - In the source, `set_config()` calls `init_worktree_config()` ("/* Update to use worktree config, if not already. */"). It then writes `core.sparseCheckout` and `core.sparseCheckoutCone` with worktree scope. Verified: https://raw.githubusercontent.com/git/git/master/builtin/sparse-checkout.c
- **What the extension means.** "By default, the `git sparse-checkout` builtin enables this extension, assigns these config values on a per-worktree basis, and uses the `$GIT_DIR/info/sparse-checkout` file to specify the sparsity for each worktree independently." When it is enabled, `core.worktree`, and `core.bare` if it is true, "must be moved" to the main worktree's `config.worktree`. Verified: https://raw.githubusercontent.com/git/git/master/Documentation/config/extensions.adoc
  - So the main checkout stays a full checkout. The only shared change is the extension line, plus that move, which a normal non-bare repo does not need. Inferred.
- **Who cannot read a repo with the extension.** "Older Git versions will refuse to access repositories with this extension." Verified: https://git-scm.com/docs/git-worktree
  - Claude Code v2.1.207 fixed "`extensions.worktreeConfig` being left in the repo's `.git/config` (breaking go-git tools like `tea`) after the last `worktree.sparsePaths` worktree was removed". Verified: https://raw.githubusercontent.com/anthropics/claude-code/main/CHANGELOG.md
- **Who removes the extension.** Claude Code removes it after the last of its own sparse worktrees is removed, "but only if Claude Code added it. It never removes a value you set yourself." Verified: https://code.claude.com/docs/en/large-codebases.md. Nothing in the git docs I read says git removes it. Inferred.
- **A shared `core.sparseCheckout` already in the repo.** "`core.sparseCheckout` should not be shared, unless you are sure you always use sparse checkout for all worktrees." Verified: https://git-scm.com/docs/git-worktree
  - If it already sits in the shared `.git/config`, every worktree reads it, the main one included. Inferred.
  - A worktree with no sparse-checkout file includes all paths at HEAD. Verified: the commit message at https://github.com/git/git/commit/5325591
- **What `git worktree add` copies.**
  - `add_worktree()` copies the patterns "If the current worktree has sparse-checkout enabled". It copies `config.worktree` "If we are using worktree config". Neither copy depends on `--no-checkout`. Verified: https://raw.githubusercontent.com/git/git/master/builtin/worktree.c
  - The commit is "worktree: copy sparse-checkout patterns and config on add" (Derrick Stolee). In the copy, `core.bare` and `core.worktree` are unset. Verified: https://github.com/git/git/commit/5325591
  - It belongs to the topic `ds/sparse-checkout-requires-per-worktree-config`, which appears in the Git 2.36.0 release notes. Verified: https://github.com/git/git/commit/6249ce2d1b9b60e7aa630d370e68c87ef6331629 and https://raw.githubusercontent.com/git/git/master/Documentation/RelNotes/2.36.0.adoc
  - So adding from the full main checkout gives a full worktree, which is why step 1 uses `--no-checkout`. Adding from inside a sparse worktree starts the new one with the same cone. Inferred from the source condition.
- **Do not mix hand-made and Claude-made sparse worktrees.**
  - Suppose Claude Code added the extension and removes it when its last sparse worktree goes. A hand-made sparse worktree still on disk then loses its per-worktree `core.sparseCheckout`, because git reads `config.worktree` only while the extension is on. Inferred from extensions.adoc and large-codebases.md.
  - Turning the extension on yourself first avoids this. Inferred from Claude Code's "only if Claude Code added it".

### 1.5 Removing a sparse worktree

- **`git worktree remove`.** "Only clean worktrees (no untracked files and no modification in tracked files) can be removed. Unclean worktrees or ones with submodules can be removed with `--force`." "To remove a locked worktree, specify `--force` twice." Verified: https://git-scm.com/docs/git-worktree
- **`git worktree prune`** cleans up the admin files of worktrees that were deleted by hand. Verified: same page.
- **`git sparse-checkout disable`** "restore[s] the working directory to include all files". Verified: https://git-scm.com/docs/git-sparse-checkout

---

## 2. Claude Code: can subagent isolation be sparse?

The common-workflows page only gives the `--worktree` recipe. It points to the Worktrees page and to the large-codebases page. Verified: https://code.claude.com/docs/en/common-workflows

### 2.1 What `isolation: "worktree"` does today

- **The frontmatter field.** "Set to `worktree` to run the subagent in a temporary git worktree, giving it an isolated copy of the repository branched by default from your default branch rather than the parent session's `HEAD`. The worktree is automatically cleaned up if the subagent makes no changes". Verified: https://code.claude.com/docs/en/sub-agents
- **On the Agent tool call.** The call can pass `isolation: "worktree"`; the docs state this for forks. Verified: same page.
- **Agent teams gotcha.** With agent teams enabled, a named subagent spawned from the main conversation "launches as a teammate instead, unless the call is a fork or passes `isolation` on the call itself. An `isolation` value in the subagent's frontmatter doesn't prevent it, and the teammate then runs in the main session's working directory." Verified: same page.
- **Where worktrees go.** By default, under `.claude/worktrees/<name>/` "at your repository root, on a new branch named `worktree-<name>`". The defaults check out "only tracked files". Subagent worktrees can sit in "the default location under `.claude/worktrees/`". Verified: https://code.claude.com/docs/en/worktrees.md
- **Instructions come from the main conversation.** A subagent in its own worktree "takes the instruction files it starts with from your main conversation, not from its worktree". In the default location it "doesn't load the `CLAUDE.md` file or `.claude/rules/` directory at the worktree's root". Verified: same page. So this project's `@docs/...` imports do not need `docs/` in the cone. Inferred.
- **Skills, agents and commands are read from the main checkout** when the worktree has no `.claude/skills` of its own. For skills this needs v2.1.277 or later. Verified: same page.
- **The isolation guard.** Claude Code blocks edits to the main checkout, commands whose working directory resolves there, and git redirected into it (through `-C`, `--git-dir`, `GIT_DIR` and similar). Verified: same page.
- **Git LFS files may arrive as pointers.** Since v2.1.247, Claude Code skips the filter drivers in the repo's own config when it creates a worktree. A repo that ran `git lfs install --local` therefore gets "LFS pointer files instead of the real files". The fix is `git lfs pull` inside the worktree. Verified: same page.

### 2.2 The `worktree` settings

All Verified from https://code.claude.com/docs/en/settings-reference.md unless another source is named.

- **`worktree`**: "Configure how Claude Code creates and manages git worktrees for `--worktree`, the `EnterWorktree` tool, and isolated subagents and background sessions." It is an object with `baseRef`, `symlinkDirectories`, `sparsePaths` and `bgIsolation`.
- **`worktree.baseRef`**: `"fresh"` (the default) branches from `origin/<default-branch>`. `"head"` branches "from your current local `HEAD`, including unpushed commits". Inside a linked worktree, `"head"` means that worktree's HEAD.
  - "You can't set `worktree.baseRef` to a branch name." With `"fresh"`, Claude Code fetches the default branch if the repo was not fetched in the last 24 hours (capped at five seconds). It falls back to local HEAD when there is no remote or `origin/HEAD` cannot be resolved. Verified: https://code.claude.com/docs/en/worktrees.md
  - Added in v2.1.133 "to choose whether `--worktree`, `EnterWorktree`, and agent-isolation worktrees branch from `origin/<default>` or local `HEAD`". Verified: https://raw.githubusercontent.com/anthropics/claude-code/main/CHANGELOG.md
- **`worktree.sparsePaths`**: "Check out only the listed directories in each worktree through git sparse-checkout. Claude Code writes only those directories plus root-level files to disk". "While a sparse worktree exists, git enables `extensions.worktreeConfig` in the repository's shared `.git/config`."
  - Changelog, v2.1.76: "Added `worktree.sparsePaths` setting for `claude --worktree` in large monorepos to check out only the directories you need via git sparse-checkout". Verified: https://raw.githubusercontent.com/anthropics/claude-code/main/CHANGELOG.md
  - That it uses cone mode is Inferred: root files are always included and the docs ask for directories only.
- **`worktree.symlinkDirectories`**: "Symlink directories from the main repository into each worktree so you don't duplicate large directories on disk". The examples are `node_modules` and `.cache`. Not checked: how it behaves on Windows (symlink or junction, and whether it needs Developer Mode).
- **`worktree.bgIsolation`**: applies to background sessions only, with the values `"worktree"` (the default) and `"none"`.
- **`worktree.location`**: for Desktop SSH sessions only. "`--worktree`, the `EnterWorktree` tool, isolated subagents, and background sessions ignore it."
- **`cleanupPeriodDays`**: default 30, minimum 1. It also drives the subagent-worktree sweep (section 2.6).
- **`.worktreeinclude`** (a file, not a setting): copies gitignored files that match its patterns into every worktree Claude Code creates with git, subagent worktrees included. Verified: https://code.claude.com/docs/en/worktrees.md

### 2.3 `sparsePaths` and subagents: documented, not yet proven

- **What the docs promise.** "This is particularly useful for subagent worktree isolation. ... each one that runs in a worktree gets a lightweight checkout instead of the full tree. All worktrees in a session share the same `sparsePaths`, so if one subagent needs `packages/api/` and another needs `packages/web/`, list both." Verified: https://code.claude.com/docs/en/large-codebases.md
- **Where the settings are read.** `sparsePaths` and `symlinkDirectories` "are read from your starting directory before the worktree is created". "Paths in `sparsePaths` are relative to the repository root". The lists merge across scopes, "so a local file can add paths to the committed list but not remove them". Verified: same page.
- **Evidence against, at v2.1.76.** Issue #35149 (opened 2026-03-17; Claude Code 2.1.76, git 2.49.0, macOS) says: "`worktree.sparsePaths` ... works correctly when creating worktrees via `claude --worktree`, but is silently ignored when the `EnterWorktree` tool creates worktrees mid-session. This means worktrees created by `EnterWorktree` (and by extension `Agent(isolation: "worktree")`) are always full checkouts".
  - Its table: `claude --worktree` sparse, about 1 GB; EnterWorktree full, about 11 GB; Agent isolation "Untested (likely no)".
  - github-actions closed it as not planned for inactivity on 2026-04-15. No maintainer replied. Verified: https://github.com/anthropics/claude-code/issues/35149
- **My changelog scan.** I read the whole changelog (1.08 M characters, newest 2.1.309) in 100 k slices through a keyword filter. `sparsePaths` appears at 2.1.76 (added), at 2.1.207 (the extension-leftover fix) and at 2.1.284 (a plugin-marketplace option with the same name, unrelated). No line says that EnterWorktree or agent-isolation worktrees started honouring it. Verified: https://raw.githubusercontent.com/anthropics/claude-code/main/CHANGELOG.md. A filtered read can miss a line.
- **So test it once.** In one isolated builder's worktree, two read-only commands show whether it is sparse: `git sparse-checkout list` should print the five folders, and `git config --worktree --get core.sparseCheckout` should print `true`. Inferred test.

### 2.4 A settings block for this project

```json
{
  "worktree": {
    "baseRef": "head",
    "sparsePaths": ["Assets/Scripts", "Assets/Tests", "Tools", "Design", "PortExport"]
  }
}
```

- **Where to put it.** Use `.claude/settings.local.json`, a personal file that Claude Code keeps out of git when it writes the file itself. On Windows, Claude Code keeps that file next to `.claude/settings.json` in the starting directory rather than at the repository root. A file created by hand is not yet in the global git excludes. Verified: https://code.claude.com/docs/en/settings
- **Optional extra folders.** Add `.claude` if the worktree needs `.claude/settings.json` or `.claude/rules/` (Verified guidance: https://code.claude.com/docs/en/large-codebases.md). Add `docs` only if builders must read docs inside their worktree (Inferred).

### 2.5 Custom creation: the `WorktreeCreate` and `WorktreeRemove` hooks

All Verified from https://code.claude.com/docs/en/hooks.md unless another source is named.

- **When it fires.** It runs for `claude --worktree`, for EnterWorktree, when "A subagent uses `isolation: "worktree"`", for workflow agents, for background sessions, and for `claude remote-control` in worktree mode. It does not run when Claude calls `git worktree add` through Bash. It has "no matcher support", so it fires on every creation.
- **It replaces the default.** "Configuring a WorktreeCreate hook replaces that default git behavior". "Because the hook replaces the default behavior entirely, `.worktreeinclude` is not processed."
  - The docs do not say whether `sparsePaths`, `baseRef` and `symlinkDirectories` still apply under a hook. Inferred: they do not, because they configure the default git creation.
- **Input.** The common fields (`session_id`, `transcript_path`, `cwd`, `hook_event_name`) plus `name`, "a slug identifier for the new worktree", for example `bold-oak-a3f2`. Nothing says which agent asked for the worktree.
- **Output.** A command hook must "print the path as the last non-empty line of stdout"; other output goes to stderr. An HTTP hook returns `hookSpecificOutput.worktreePath`. "If the hook fails or produces no path, worktree creation fails". Any non-zero exit fails the creation.
- **Path rules.** A relative path is resolved against the hook's directory. An absolute path containing `.` or `..` segments is refused, as is any path through a symlink below the repository root (v2.1.216 and later).
- **Identity check.** A directory with no git metadata is refused if it sits inside a repository. Verified: https://code.claude.com/docs/en/worktrees.md. Inferred: a linked worktree has its own `.git` file and passes, as Claude's own worktrees do.
- **Cleanup is yours.**
  - "When a subagent or a workflow agent finishes, Claude Code keeps the worktree your WorktreeCreate hook made for it and doesn't run your WorktreeRemove hook. Remove those worktrees yourself when the work is done."
  - Claude Code "never deletes a branch belonging to a hook-created worktree".
  - The periodic sweep "keeps any worktree without" Claude Code's marker, "including a worktree a `WorktreeCreate` hook created". Verified: https://code.claude.com/docs/en/worktrees.md
- **A sketch, untested (Inferred).** It would register as `{"hooks": {"WorktreeCreate": [{"hooks": [{"type": "command", "command": "bash Tools/<script>.sh"}]}]}}`. Not checked: whether `jq` is available in the hook's shell on this machine.

```bash
#!/usr/bin/env bash
# WorktreeCreate hook, UNTESTED sketch. stdout carries the path and nothing else.
set -euo pipefail
input=$(cat)
name=$(printf '%s' "$input" | jq -r .name)
cwd=$(printf '%s' "$input" | jq -r .cwd)
repo=$(git -C "$cwd" rev-parse --show-toplevel)
dir="$repo/.claude/worktrees/$name"
git -C "$repo" worktree add --no-checkout -b "worktree-$name" "$dir" HEAD >&2
git -C "$dir" sparse-checkout set --cone Assets/Scripts Assets/Tests Tools Design PortExport >&2
git -C "$dir" checkout >&2
printf '%s\n' "$dir"
```

### 2.6 Why worktrees pile up (the cleanup rules)

- **What is removed automatically.** "Each subagent gets a temporary worktree that Claude Code removes automatically when the subagent finishes without changes; a worktree with changes stays on disk until the periodic sweep below can remove it without losing work." Verified: https://code.claude.com/docs/en/worktrees.md
- **What the sweep keeps.** It removes the worktrees Claude Code created for subagents and background sessions "once they are older than your `cleanupPeriodDays` setting". It leaves in place any worktree that "still holds work: changed or untracked files, or unpushed commits", any whose submodules hold work or cannot be inspected, any belonging to a `--worktree` session that was not sent to the background, and any "you created ... yourself with `git worktree add`". Verified: same page.
- **Locks.** "While an agent is running, Claude Code holds a `git worktree lock` on its worktree". Verified: same page.
- **What this means here.** This project never pushes, so a builder worktree that holds a commit is never swept. After the cherry-pick, the only cleanup is `git worktree remove <path>` (with `git worktree unlock` first if it is locked) plus deleting its branch. Inferred.
- **Changelog lines on cleanup.** Verified: https://raw.githubusercontent.com/anthropics/claude-code/main/CHANGELOG.md
  - 2.1.76: "Improved stale worktree cleanup — worktrees left behind after an interrupted parallel run are now automatically cleaned up"
  - 2.1.157: "Worktrees managed by Claude are now left unlocked when the agent finishes"
  - 2.1.187: "Fixed leaked agent worktree registrations: locked `.git/worktrees/` entries from killed agents are now cleaned up"
  - 2.1.305: "... unchanged subagent worktrees sometimes not being cleaned up"
- **Open reports.** Issue #26725, "Stale worktrees are never cleaned up", is still open; its latest comment is dated 2026-10-16, and one comment says "Same issue on Windows." Verified: https://github.com/anthropics/claude-code/issues/26725. Issue #31488 (the VS Code extension) says worktrees "accumulate in `.claude/worktrees/`" and must be removed by hand. Verified: https://github.com/anthropics/claude-code/issues/31488

### 2.7 Pre-made worktrees entered by path

- EnterWorktree can "switch into an existing worktree" through its `path` parameter. Paths under `.claude/worktrees/` don't prompt. From a subagent with a pinned working directory, such as `isolation: worktree`, "only the `path` form is available". Verified: https://code.claude.com/docs/en/tools-reference.md
- So the lead could make sparse worktrees by hand under `.claude/worktrees/` and have each builder enter its own by path. Not checked: whether EnterWorktree in a non-isolated subagent moves only that subagent or the whole session.

---

## 3. Other cheap isolated copies and their costs

- **Linked worktree (what `isolation` uses).**
  - Objects, refs and config are shared. Per worktree there are only HEAD, the index and similar files. Verified: https://git-scm.com/docs/git-worktree
  - Disk cost: the checked-out files, plus the index, plus a small admin folder in `.git/worktrees/<id>`. Inferred. With a cone, the checked-out files shrink to the cone, the root files and the files directly in the parent folders. Inferred.
  - `--detach` gives the same files with no branch to clean up afterwards. The behaviour is Verified on the same page; the disk claim is Inferred.
- **`git clone --shared --no-checkout <main repo> <dir>`, then `sparse-checkout set` and `checkout <branch>`.**
  - "The resulting repository starts out without any object of its own." Verified: https://git-scm.com/docs/git-clone
  - Danger: if the source deletes branches and later auto-maintenance prunes objects the clone needs, "the cloned repository will become corrupt". `git repack` without `--local` copies the objects in and removes the saving; `git gc` is safe. Verified: same page.
  - For: the clone has its own config and refs, so sparse settings and `extensions.worktreeConfig` stay in the clone. Against: builder commits must be fetched back into the main repo before a cherry-pick, and Claude Code's isolation and cleanup know nothing about such clones. Both Inferred.
- **`git clone --local`** (the default for a local path).
  - It copies HEAD and the refs, and "The files under `.git/objects/` directory are hardlinked to save space when possible". It "can race with concurrent modification to the source repository". `--no-hardlinks` copies the files instead. Verified: https://git-scm.com/docs/git-clone
  - Inferred: hardlinks work only on the same volume, and a later repack in either repo writes new packs that are no longer shared.
- **`--reference` / `--dissociate`.** These borrow objects from another local repo through alternates; `--dissociate` copies the borrowed objects at the end. Verified: https://git-scm.com/docs/git-clone
- **Partial clone plus sparse.**
  - GitHub's recipe: `git clone --filter=blob:none --no-checkout <url>`, then `git sparse-checkout set --cone`, `git checkout main`, `git sparse-checkout set <dir>`. Verified: https://github.blog/open-source/git/bring-your-monorepo-down-to-size-with-sparse-checkout/
  - Blobless clones "download all reachable commits and trees while fetching blobs on-demand". Verified: https://github.blog/open-source/git/get-up-to-speed-with-partial-clone-and-shallow-clone/
  - From a plain local path, git warns "--filter is ignored in local clones; use file:// instead." Verified: https://raw.githubusercontent.com/git/git/master/builtin/clone.c
  - Over `file://`, the source's `uploadpack.allowFilter` must be set: "If this option is set, `upload-pack` will support partial clone and partial fetch object filtering." Verified: https://raw.githubusercontent.com/git/git/master/Documentation/config/uploadpack.adoc. That it is off by default is Inferred.
  - Cost: the whole history's commits and trees plus the cone's blobs, with nothing shared with the main repo. For local isolation, a worktree or a `--shared` clone is cheaper. Inferred.
- **Copy-on-write (Dev Drive, ReFS block cloning).**
  - "Beginning in Windows 11 24H2 & Windows Server 2025, Block cloning is now supported on Dev Drive". It copies "a range of file bytes ... as a low-cost metadata operation".
  - But a Dev Drive needs "Minimum 50 GB free disk space"; its "minimum size is 50 GB"; "The C: drive on your machine cannot be designated as a Dev Drive". Verified: https://learn.microsoft.com/en-us/windows/dev-drive/
  - So it is not an option with a few GB free. Inferred.
- **No copy at all.** `isolation` is an optional per-subagent field (Verified: https://code.claude.com/docs/en/sub-agents). Agents that only read and report, such as critics and researchers, can run in the main checkout without a worktree. Inferred.

---

## 4. Measured or reported disk use

- **Claude Code, sparse against full** (issue #35149; a monorepo of about 30 GB with 188 top-level folders; macOS; Claude Code 2.1.76): with `sparsePaths`, `claude --worktree` gave about 1 GB per worktree; EnterWorktree, which ignored the setting, gave about 11 GB. Verified: https://github.com/anthropics/claude-code/issues/35149
- **Claude Code, subagent worktrees** (issue #58914; macOS): 615 detached worktrees took about 21 GB on 2026-05-12, and 764 took about 36 GB on 2026-05-13, leaving 220 MB free. "Each detached worktree is a full copy of the repo (~50MB)." "A session with heavy subagent use can create dozens." The issue was closed as a duplicate of #26725. Verified: https://github.com/anthropics/claude-code/issues/58914
- **Claude Code** (issue #63087): "27 orphaned worktrees accumulated over normal usage, eating 80 GB." My arithmetic gives about 3 GB each (Inferred). Verified: https://github.com/anthropics/claude-code/issues/63087
- **Claude Code** (issue #26725, open): "15 orphaned agent worktrees from a single session"; "13 abandoned worktrees holding 13.8 GB", "roughly 7× the size of the main checkout", over about 11 days; "113 of 114 worktrees under `.claude/worktrees/`, 3.0 GB" over about 2 months, about 27 MB each by my arithmetic (Inferred). Verified: https://github.com/anthropics/claude-code/issues/26725
- **Cursor on Windows** (December 2025; Windows build 26200; a codebase of about 2 GB): "two worktrees were created in a 20-minute session totaling 9.82 GB" (8.5 GB and 1.32 GB). 22 worktrees had accumulated, an "Estimated total: ~140 GB". Verified: https://forum.cursor.com/t/windows-request-to-disable-automatic-worktree-creation-critical-disk-space-issue/146189
- **Index size** (GitHub, 2021; a monorepo with over two million files at HEAD, about 100,000 in the cone): "the index file is over 180 MB"; with the sparse index, "from 180 MB to less than 10 MB"; `git status` went "from 1.3 seconds to under 200 milliseconds". Verified: https://github.blog/open-source/git/make-your-monorepo-feel-small-with-gits-sparse-index/
  - Inferred: that is about 90 bytes of index per tracked file, so every worktree's index scales with the repo's file count even when sparse, unless the sparse index is on.
- **Partial clone** (GitHub, 2020, a small example repo): "To clone the entire repository, we needed only 373 objects instead of ~2,000." Cone matching on a test tree of three million files went from five minutes to under one second. Verified: https://github.blog/open-source/git/bring-your-monorepo-down-to-size-with-sparse-checkout/
- **This repo.** About 165 MB per full worktree is the caller's figure; I did not re-measure it. The cone's share is not checked; the read-only way to size it is in section 1.2.

## 5. Not checked

- The installed Claude Code version (`claude --version`). `sparsePaths` needs v2.1.76 or later, `baseRef` v2.1.133, and the extension-leftover fix v2.1.207.
- The installed Git version (`git --version`). Copying the sparse settings on add is in Git 2.36.0's topic; non-cone mode was deprecated in 2.37.0.
- Whether `sparsePaths` applies to `isolation: "worktree"` on that version (see the test in section 2.3).
- Whether `.claude/settings.json` or `.claude/settings.local.json` already sets `worktree` keys, and whether `.claude/` is tracked.
- Whether the repo uses Git LFS, and whether it was installed with `--local`.
- The cone's size, and whether old worktrees are still on disk (`git worktree list` is read-only).
- How `symlinkDirectories` behaves on Windows, and whether `jq` exists in the hook's shell.
- Whether EnterWorktree by path in a non-isolated subagent moves only that subagent.

## 6. A correction to forge61

Section 7 of `Research/2026-10-22-forge61/research-claude-code.md` says that no `worktree.sparsePaths` or `worktree.symlinkDirectories` setting appeared. That is right for the Worktrees page. Both keys are documented elsewhere, on the settings reference and on the large-codebases page. Verified: https://code.claude.com/docs/en/settings-reference.md and https://code.claude.com/docs/en/large-codebases.md

## Sources (all read 2026-10-22)

[git-worktree]: https://git-scm.com/docs/git-worktree
[git-sparse]: https://git-scm.com/docs/git-sparse-checkout
[git-read-tree]: https://git-scm.com/docs/git-read-tree
[git-clone]: https://git-scm.com/docs/git-clone
[git-add]: https://git-scm.com/docs/git-add
[git-ls-tree]: https://git-scm.com/docs/git-ls-tree
[git-extensions]: https://raw.githubusercontent.com/git/git/master/Documentation/config/extensions.adoc
[git-uploadpack]: https://raw.githubusercontent.com/git/git/master/Documentation/config/uploadpack.adoc
[src-worktree]: https://raw.githubusercontent.com/git/git/master/builtin/worktree.c
[src-sparse]: https://raw.githubusercontent.com/git/git/master/builtin/sparse-checkout.c
[src-checkout]: https://raw.githubusercontent.com/git/git/master/builtin/checkout.c
[src-clone]: https://raw.githubusercontent.com/git/git/master/builtin/clone.c
[git-commit-5325591]: https://github.com/git/git/commit/5325591
[git-merge-6249ce2]: https://github.com/git/git/commit/6249ce2d1b9b60e7aa630d370e68c87ef6331629
[git-rel-236]: https://raw.githubusercontent.com/git/git/master/Documentation/RelNotes/2.36.0.adoc
[git-rel-237]: https://raw.githubusercontent.com/git/git/master/Documentation/RelNotes/2.37.0.adoc
[lor-forum]: https://www.linux.org.ru/forum/development/15962830
[cc-worktrees]: https://code.claude.com/docs/en/worktrees.md
[cc-sub-agents]: https://code.claude.com/docs/en/sub-agents
[cc-settings]: https://code.claude.com/docs/en/settings
[cc-settings-ref]: https://code.claude.com/docs/en/settings-reference.md
[cc-large]: https://code.claude.com/docs/en/large-codebases.md
[cc-hooks]: https://code.claude.com/docs/en/hooks.md
[cc-tools]: https://code.claude.com/docs/en/tools-reference.md
[cc-workflows]: https://code.claude.com/docs/en/common-workflows
[cc-changelog]: https://raw.githubusercontent.com/anthropics/claude-code/main/CHANGELOG.md
[issue-35149]: https://github.com/anthropics/claude-code/issues/35149
[issue-26725]: https://github.com/anthropics/claude-code/issues/26725
[issue-58914]: https://github.com/anthropics/claude-code/issues/58914
[issue-63087]: https://github.com/anthropics/claude-code/issues/63087
[issue-31488]: https://github.com/anthropics/claude-code/issues/31488
[cursor-forum]: https://forum.cursor.com/t/windows-request-to-disable-automatic-worktree-creation-critical-disk-space-issue/146189
[gh-sparse-index]: https://github.blog/open-source/git/make-your-monorepo-feel-small-with-gits-sparse-index/
[gh-sparse]: https://github.blog/open-source/git/bring-your-monorepo-down-to-size-with-sparse-checkout/
[gh-partial]: https://github.blog/open-source/git/get-up-to-speed-with-partial-clone-and-shallow-clone/
[ms-devdrive]: https://learn.microsoft.com/en-us/windows/dev-drive/

- git-worktree (git 2.56.0 page): https://git-scm.com/docs/git-worktree
- git-sparse-checkout (2.55.0 page): https://git-scm.com/docs/git-sparse-checkout
- git-read-tree, Sparse checkout section: https://git-scm.com/docs/git-read-tree
- git-clone (2.54.0 page): https://git-scm.com/docs/git-clone
- git-add `--sparse`: https://git-scm.com/docs/git-add
- git-ls-tree `-l`: https://git-scm.com/docs/git-ls-tree
- extensions.worktreeConfig: https://raw.githubusercontent.com/git/git/master/Documentation/config/extensions.adoc
- uploadpack.allowFilter: https://raw.githubusercontent.com/git/git/master/Documentation/config/uploadpack.adoc
- git source, worktree add: https://raw.githubusercontent.com/git/git/master/builtin/worktree.c
- git source, sparse-checkout: https://raw.githubusercontent.com/git/git/master/builtin/sparse-checkout.c
- git source, checkout: https://raw.githubusercontent.com/git/git/master/builtin/checkout.c
- git source, clone: https://raw.githubusercontent.com/git/git/master/builtin/clone.c
- Commit "worktree: copy sparse-checkout patterns and config on add": https://github.com/git/git/commit/5325591
- Merge of ds/sparse-checkout-requires-per-worktree-config: https://github.com/git/git/commit/6249ce2d1b9b60e7aa630d370e68c87ef6331629
- Git 2.36.0 release notes: https://raw.githubusercontent.com/git/git/master/Documentation/RelNotes/2.36.0.adoc
- Git 2.37.0 release notes: https://raw.githubusercontent.com/git/git/master/Documentation/RelNotes/2.37.0.adoc
- Forum report of the `--no-checkout` + sparse + `checkout` sequence (2020): https://www.linux.org.ru/forum/development/15962830
- Claude Code, Worktrees: https://code.claude.com/docs/en/worktrees.md
- Claude Code, Subagents: https://code.claude.com/docs/en/sub-agents
- Claude Code, Settings files: https://code.claude.com/docs/en/settings
- Claude Code, Settings reference (`worktree`, `cleanupPeriodDays`): https://code.claude.com/docs/en/settings-reference.md
- Claude Code, Monorepos and large codebases (sparse worktrees): https://code.claude.com/docs/en/large-codebases.md
- Claude Code, Hooks (WorktreeCreate, WorktreeRemove): https://code.claude.com/docs/en/hooks.md
- Claude Code, Tools reference (EnterWorktree, ExitWorktree): https://code.claude.com/docs/en/tools-reference.md
- Claude Code, Common workflows: https://code.claude.com/docs/en/common-workflows
- Claude Code changelog (newest 2.1.309): https://raw.githubusercontent.com/anthropics/claude-code/main/CHANGELOG.md
- Issues: https://github.com/anthropics/claude-code/issues/35149, https://github.com/anthropics/claude-code/issues/26725, https://github.com/anthropics/claude-code/issues/58914, https://github.com/anthropics/claude-code/issues/63087, https://github.com/anthropics/claude-code/issues/31488
- Cursor forum, Windows worktree disk report: https://forum.cursor.com/t/windows-request-to-disable-automatic-worktree-creation-critical-disk-space-issue/146189
- GitHub blog, sparse index: https://github.blog/open-source/git/make-your-monorepo-feel-small-with-gits-sparse-index/
- GitHub blog, sparse-checkout: https://github.blog/open-source/git/bring-your-monorepo-down-to-size-with-sparse-checkout/
- GitHub blog, partial clone: https://github.blog/open-source/git/get-up-to-speed-with-partial-clone-and-shallow-clone/
- Microsoft Learn, Dev Drive: https://learn.microsoft.com/en-us/windows/dev-drive/
