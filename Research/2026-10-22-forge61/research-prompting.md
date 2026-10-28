# FORGE61 research: prompting Opus 5-class models for an 8-hour unattended run

Researcher: a web-only subagent for FORGE61, 2026-10-22. Every quote below is copied from the page named with it.
Where a link or a line break split a sentence in the page source, I joined the pieces with a space and dropped the link
markup. `[sic]` marks a typo that is in the source.

## Sources (all Verified: I read each one myself this session)

| # | Source | URL | Status: how I read it |
|---|--------|-----|-----------------------|
| S1 | Prompting Claude Opus 5 (platform docs) | https://platform.claude.com/docs/en/build-with-claude/prompt-engineering/prompting-claude-opus-5 | Verified: WebFetch returned the whole page as Markdown |
| S2 | Prompting Claude Opus 5.5 (platform docs) | https://platform.claude.com/docs/en/build-with-claude/prompt-engineering/prompting-claude-opus-5-5 | Verified: WebFetch, whole page |
| S3 | Prompting best practices (platform docs; covers Opus 5 / 5.5) | https://platform.claude.com/docs/en/build-with-claude/prompt-engineering/claude-prompting-best-practices | Verified: WebFetch, whole page (72.8 KB); I read the model table, General principles and Agentic systems, and searched the rest |
| S4 | Effort (platform docs) | https://platform.claude.com/docs/en/build-with-claude/effort | Verified: WebFetch, whole page |
| S5 | Effective harnesses for long-running agents (Justin Young, Nov 26, 2025) | https://www.anthropic.com/engineering/effective-harnesses-for-long-running-agents | Verified: WebFetch only summarizes or quotes 125 characters at most, so I downloaded the public HTML with curl and read the text |
| S6 | Harness design for long-running application development (Prithvi Rajasekaran, Mar 24, 2026) | https://www.anthropic.com/engineering/harness-design-long-running-apps | Verified: same as S5 (curl, text read) |
| S7 | How we built our multi-agent research system (Hadfield, Zhang, Lien, Scholz, Fox, Ford; Jun 13, 2025) | https://www.anthropic.com/engineering/multi-agent-research-system | Verified: same as S5 (curl, text read and searched) |
| S8 | Best practices for Claude Code (Claude Code docs; the source of forge claim 1) | https://code.claude.com/docs/en/best-practices | Verified: curl of the page's `.md` version; I read the verification, spec and review sections |
| S9 | Prompting Claude Fable 5, one section only (measured on Fable 5, not Opus) | https://platform.claude.com/docs/en/build-with-claude/prompt-engineering/prompting-claude-fable-5 | Verified for two sections only: curl of the `.md` version; I read "Ground progress claims during long runs" and "State the boundaries" |

Method: WebSearch to find the pages, then WebFetch or curl. No personal data was in any request: plain public URLs only.
The raw downloads are in the session scratchpad (`scratchpad/web-dl/`) if anyone needs to search the exact text again.

S3 says to apply its general techniques with care: "Where a technique names a specific model, treat it as measured on
that model and re-check it against your own evals before applying it to another." S2 says Opus 5 prompts carry over:
"Existing Claude Opus 5 prompts should perform well without changes, and the patterns in Prompting Claude Opus 5 remain
a reasonable starting point."

---

## 1. Platform docs: what they say, topic by topic

### 1.1 Give the full task up front
- S1 (Capability improvements, Agentic coding): "It completes full tasks rather than leaving stubs or placeholders, and it
  performs best when given the complete task specification up front and left to run."
- S2 (Unattended agentic runs): "You can also state the completion condition up front and have a separate, smaller model
  check the conversation against it at each end of turn, returning its reason as the next user message when the
  condition isn't met."
- S3 (Be clear and direct): "If you want Claude to do more than the task strictly requires, explicitly request it rather
  than relying on the model to infer this from vague prompts." and "Think of Claude as a brilliant but new employee who
  lacks context on your norms and workflows."
- S3 (Add context to improve performance): "Providing context or motivation behind your instructions, such as explaining
  to Claude why such behavior is important, can give Claude more information about your goals and help it deliver more
  targeted responses." Then: "Claude can generalize from the explanation."
- S9 (Fable 5 section title, not read beyond the heading list): "Give the reason, not only the request".

### 1.2 Scope: the model widens it, so state it
- S1 (Task scope and over-verification): "Claude Opus 5 can also expand the scope of a task, adding steps that weren't
  requested or applying its own judgment about what the task should be. For narrow tasks, constrain scope explicitly:"
- S1 sample prompt, verbatim: "Deliver what was asked, at the scope intended. Make routine judgment calls yourself, and
  check in only when different readings of the request would lead to materially different work. If the request seems
  mistaken or a better approach exists, say so in a sentence and continue with the task as asked rather than quietly
  narrowing, widening, or transforming it. Finish the whole task, and stop short of actions that are clearly beyond what
  was asked."
- S3 (Overeagerness; written about Opus 4.5 / 4.6): "Claude Opus 4.5 and Claude Opus 4.6 have a tendency to overengineer by
  creating extra files, adding unnecessary abstractions, or building in flexibility that wasn't requested." The sample
  prompt starts: "Avoid over-engineering. Only make changes that are directly requested or clearly necessary." and its
  scope line is: "Scope: Don't add features, refactor code, or make "improvements" beyond what was asked."
- S8: "The most useful specs are self-contained: they name the files and interfaces involved, state what is out of scope,
  and end with an end-to-end verification step that proves the feature works."
- Narrowing is a risk too. S6 (on Opus 4.6, without its planner): "Without the planner, the generator under-scoped: given
  the raw prompt, it would start building without first speccing its work, and end up creating a less feature-rich
  application than the planner did."

### 1.3 Over-verification: no "double-check" lines
- S1 (Task scope and over-verification): "Claude Opus 5 verifies its own work without being told to. If your prompt
  contains explicit verification instructions ("include a final verification step for any non-trivial task," "use a
  subagent to verify"), remove them: instructions like these cause over-verification on Claude Opus 5, and removing them
  reduces wasted tokens with no loss in quality. The same applies to legacy harness scaffolding that adds separate
  verification steps."
- S1 (Self-correction): "Claude Opus 5 catches and fixes its own mistakes well without prompting. Avoid instructing
  re-checks it already performs ("double-check your answer," "re-verify before responding"); like verification
  instructions, these compound with the model's own behavior and add cost without improving results."
- S3 still lists verification tools for long runs (Workflows across multiple context windows, item 5): "Provide
  verification tools: As the length of autonomous tasks grows, Claude needs to verify correctness without continuous
  human feedback." In other words, give it the tools to check; don't add lines telling it to check again.

### 1.4 Delegation rules and subagent spawn counts
- S1 (Controlling subagent spawning): "Claude Opus 5 delegates to subagents more readily than prior models. Delegation is
  worthwhile for genuinely independent, sizeable tracks of work, but it multiplies cost and time when applied to small
  tasks. If your harness supports subagents, give explicit guidance on which scenarios warrant delegation, or set
  deterministic caps on how many agents can be launched."
- S1 sample prompt, verbatim: "Delegate to a subagent only for large tasks that are genuinely independent and
  parallelizable, such as a wide multi-file investigation. Do not delegate work you can finish yourself in a handful of
  tool calls, and do not use subagents to verify or double-check your own work. If one subagent can complete the task,
  use one rather than several, and keep spawn counts low."
- S1 (the caps): "If your harness is Claude Code or the Claude Agent SDK, the deterministic caps are the
  `CLAUDE_CODE_MAX_SUBAGENT_SPAWN_DEPTH` and `CLAUDE_CODE_MAX_CONCURRENT_SUBAGENTS` environment variables and the SDK's
  `max_budget_usd` (typescript: `maxBudgetUsd`) option." Also: "Claude Code adds a delegation instruction of its own on
  Claude Opus 5 only when you use its `claude_code` system prompt preset; with a custom or omitted system prompt, add a
  delegation instruction such as the example in this section yourself."
- S1 (Multiagent coordination): "Claude Opus 5 coordinates teams of subagents well, with effective writer-verifier
  patterns and few cases of agents overwriting each other's work. For cost-sensitive workloads, cap delegation".
- S2 (Opus 5.5 capabilities): "It also sustains long-running autonomous work better than Claude Opus 5, such as multihour
  audits and migrations of large code bases run end to end with parallel subagents and little oversight."
- S2 (Time signals for multiagent harnesses): "If you can estimate how long the task should take, give the model a time
  budget: have your harness add a short line at the end of each message it sends back to the model giving the elapsed
  time against that budget, in seconds, for example `elapsed 340s / 1200s`." And: "The budget is advisory and nothing
  stops the model at the limit, so if you need a hard stop, keep your own timeout."
- S3 (Subagent orchestration) sample prompt: "Use subagents when tasks can run in parallel, require isolated context, or
  involve independent workstreams that don't need to share state. For simple tasks, sequential operations, single-file
  edits, or tasks where you need to maintain context across steps, work directly rather than delegating."

### 1.5 Test gaming and hard-coding
- S3 (Avoid focusing on passing tests and hardcoding): "Claude can sometimes focus too heavily on making tests pass at the
  expense of more general solutions, or might use workarounds such as helper scripts for complex refactoring instead of
  using standard tools directly."
- S3 sample prompt, the lines that matter: "Implement a solution that works correctly for all valid inputs, not just the
  test cases. Do not hard-code values or create solutions that only work for specific test inputs. Instead, implement
  the actual logic that solves the problem generally." / "Tests are there to verify correctness, not to define the
  solution." / "If the task is unreasonable or infeasible, or if any of the tests are incorrect, please inform me rather
  than working around them."
- S3 (Workflows across multiple context windows, item 2): "Remind Claude of the importance of tests: "It is unacceptable
  to remove or edit tests because this could lead to missing or buggy functionality.""

### 1.6 State files and progress files for long tasks
- S3 (Long-horizon reasoning and state tracking): "Claude maintains orientation across extended sessions by focusing on
  incremental progress, making steady advances on a few things at a time rather than attempting everything at once."
- S3 (Workflows across multiple context windows):
  1. "Use a different prompt for the very first context window: Use the first context window to set up a framework
     (write tests, create setup scripts), then use future context windows to iterate on a todo-list."
  2. "Have the model write tests in a structured format: Ask Claude to create tests before starting work and keep track
     of them in a structured format (for example, `tests.json`)."
  3. "Set up convenience tools: Encourage Claude to create setup scripts (for example, `init.sh`) to gracefully start
     servers and run test suites and linters."
  4. "Claude's latest models are extremely effective at discovering state from the local filesystem." Its suggested
     start lines: "Call pwd; you can only read and write files in this directory." / "Review progress.txt, tests.json,
     and the git logs." / "Manually run through a fundamental integration test before moving on to implementing new
     features."
- S3 (State management best practices): "Use structured formats for state data: When tracking structured information
  (such as test results or task status), use JSON or other structured formats to make schema requirements explicit to
  Claude." / "Use unstructured text for progress notes: Freeform progress notes work well for tracking general progress
  and context." / "Use git for state tracking: Git provides a log of what's been done and checkpoints that can be
  restored." / "Emphasize incremental progress: Explicitly ask Claude to keep track of its progress and focus on
  incremental work."
- S3 sample (encourage complete use of context): "It's encouraged to spend your entire output context working on the
  task - just make sure you don't run out of context with significant uncommitted work."
- S1 (Long-context work): "Claude Opus 5 has a 1M token context window as both the default and the maximum, and its
  instruction following, tool calling, and reasoning stay consistent throughout the window."
- S2 (Unattended agentic runs; Opus 5.5), the core lines: "An unattended agent loop that treats such a turn as the end of
  the task stops running there." / "Treat a text-only end of turn as a report rather than as proof the task is done.
  Keep the task's parts in a checklist the model updates, such as a to-do tool or a file. If a turn ends with items
  still open and no blocker stated, send a short user message naming them" / "Either way, stop after two or three
  automatic continuations on the same task rather than repeating them indefinitely, so that a run that is genuinely
  stuck ends and can be reviewed." / "If something the model started is still running, such as a background command or
  a subagent, don't treat the task as done yet: wait for it to finish and return its output to the model as the next
  user message."
- S2 standing instruction for fully unattended runs (verbatim; S2 says to add it at the end of the system prompt from the
  first request, and to leave it out of human-in-the-loop use): "A standing instruction from the user, the person you are
  working for. It is about how your turns end. A message with no tool call in it ends your turn, and the work stops
  there until you are asked to continue. The user has seen you end turns in four ways while work they asked for was still
  owed, and does not want any of them. One: a long summary of what was done that closes by announcing the next step and
  has no tool call, so the next thing never starts. Two: an offer to carry on with something unless the user would prefer
  otherwise, which stops to wait for an answer the user was not going to give. Three: a list of decisions for the user
  when, by your own account, none of them blocks the rest of the work. Four: deciding that this is a good place to report,
  because the turn has been long or a milestone is done. Status notes are welcome, and so are your recommendations on
  open decisions, but put them in the same message as your next tool call and carry on with whatever does not depend on
  the user's answer. If you notice yourself inviting the user to redirect you or offering to wait, delete it and do the
  next thing. The stops the user does want are the ones where nothing can move without them, or where the thing blocking
  you is deliberately protected from you. This does not override the need for confirmation on risky or destructive
  actions."
- S9 (Fable 5 only; re-check before using it on Opus): "On long autonomous runs, instruct Claude Fable 5 to audit progress
  against actual tool results. In Anthropic's testing, this nearly eliminated fabricated status reports even on tasks
  designed to elicit them:" with the sample "Before reporting progress, audit each claim against a tool result from this
  session. Only report work you can point to evidence for; if something is not yet verified, say so explicitly."

### 1.7 Effort settings
- S4 (Effort levels table): `xhigh` is "Extended capability for long-horizon work." with the typical use "Long-running
  agentic and coding tasks (over 30 minutes) with token budgets in the millions". `low` lists "Simpler tasks that need the
  best speed and lowest costs, such as subagents". But S4 also says: "The per-model recommendations that follow override
  this table where they differ."
- S4 (Opus 5): "**Start with `high`, the default**, and adjust based on your evals: step up to `xhigh` for demanding coding
  and agentic work, or to `max` when a task justifies unconstrained token spending, and use `low` and `medium` liberally
  as your primary control for token cost and response time wherever your evals show quality holds." And: "When running
  Claude Opus 5 at `xhigh` or `max` effort, set a large `max_tokens` so the model has room to think and act across
  subagents and tool calls. Starting at 64k tokens and tuning from there is a reasonable default."
- S1 (Efficiency at lower effort): "Start with the default (`high`) and adjust based on your evals: use `low` and `medium`
  liberally as your primary control for token cost and response time wherever quality holds, and step up to `xhigh` for
  demanding coding and agentic work." Also: "The effort parameter controls how much the model thinks rather than how much
  it says".
- S2 / S4 (Opus 5.5 is different): "Start at `medium`, the default on Claude Opus 5.5 (Claude Opus 5 defaults to `high`),
  set it explicitly, and test several levels against your own evals rather than carrying over the setting you used on
  Claude Opus 5." / "At a given level, Claude Opus 5.5 tends to think more per turn than Claude Opus 5, especially at
  `xhigh` and `max`." / "Reserve `xhigh` and `max` for work where you've measured a quality gain." / "For the long turns
  that agentic coding can produce, a `max_tokens` of 128,000, the model's maximum, has worked well in Anthropic's
  testing." / S4: "Adaptive thinking is always on and can't be turned off".
- S2 (Opus 5.5 effort note): "In Anthropic's testing, at its default `medium` effort the model matched or beat Claude Opus 5
  at `high` effort on such tasks, in fewer steps and with fewer tokens."

### 1.8 Reviewers: report everything, filter in a separate pass
- S1 (Code review and bug-finding): "Claude Opus 5 reviews code with high precision and recall: it finds real bugs at a high
  rate per pass, and its additional findings are mostly real issues rather than false positives. Accuracy holds at lower
  effort settings, which supports a fast pass at review time and a more thorough pass later. If your review prompt says
  "only report high-severity issues" or "be conservative," the model might follow that instruction literally and report
  less; ask it to report everything and filter in a separate pass instead."
- S2: "Early testers also reported stronger code review, with more bugs caught than on Claude Opus 5 and fewer false
  alarms".
- S8 pulls the other way (it isn't tied to one model): "A reviewer prompted to find gaps will usually report some, even
  when the work is sound, because that is what it was asked to do. Chasing every finding leads to over-engineering: extra
  abstraction layers, defensive code, and tests for cases that can't happen. Tell the reviewer to flag only gaps that
  affect correctness or the stated requirements, and treat the rest as optional." (Tension A below covers this.)

---

## 2. Engineering blog: what the posts say

### 2.1 S5, Effective harnesses for long-running agents
- The setup: "an initializer agent that sets up the environment on the first run, and a coding agent that is tasked with
  making incremental progress in every session, while leaving clear artifacts for the next session." Its footnote: "We
  refer to these as separate agents in this context only because they have different initial user prompts. The system
  prompt, set of tools, and overall agent harness was otherwise identical."
- What the initializer writes: "an init.sh script, a claude-progress.txt file that keeps a log of what agents have done,
  and an initial git commit that shows what files were added."
- **Feature list with a pass flag:** "we prompted the initializer agent to write a comprehensive file of feature
  requirements expanding on the user’s initial prompt. In the claude.ai clone example, this meant over 200 features" /
  "These features were all initially marked as “failing” so that later coding agents would have a clear outline of what
  full functionality looked like." Example entry: `{"category": "functional", "description": "New chat button creates a
  fresh conversation", "steps": ["Navigate to main interface", "Click the 'New Chat' button", "Verify a new conversation
  is created", "Check that chat area shows welcome state", "Verify conversation appears in sidebar"], "passes": false}`
- **Only the flag changes, and the file is JSON:** "We prompt coding agents to edit this file only by changing the status of
  a passes field, and we use strongly-worded instructions like “It is unacceptable to remove or edit tests because this
  could lead to missing or buggy functionality.” After some experimentation, we landed on using JSON for this, as the
  model is less likely to inappropriately change or overwrite JSON files compared to Markdown files."
- **One feature at a time:** "the next iteration of the coding agent was then asked to work on only one feature at a time.
  This incremental approach turned out to be critical to addressing the agent’s tendency to do too much at once."
- **Git commit and progress file for each change:** "Once working incrementally, it’s still essential that the model
  leaves the environment in a clean state after making a code change. In our experiments, we found that the best way to
  elicit this behavior was to ask the model to commit its progress to git with descriptive commit messages and to write
  summaries of its progress in a progress file. This allowed the model to use git to revert bad code changes and recover
  working states of the code base."
- "Clean state" defined: "the kind of code that would be appropriate for merging to a main branch: there are no major
  bugs, the code is orderly and well-documented, and in general, a developer could easily begin work on a new feature
  without first having to clean up an unrelated mess."
- **Getting its bearings at session start** (the prompted steps): "Run pwd to see the directory you’re working in. You’ll
  only be able to edit files in this directory." / "Read the git logs and progress files to get up to speed on what was
  recently worked on." / "Read the features list file and choose the highest-priority feature that’s not yet done to work
  on."
- **Init script and an end-to-end check before new work:** "It also helps to ask the initializer agent to write an init.sh
  script that can run the development server, and then run through a basic end-to-end test before implementing a new
  feature." / "This ensured that Claude could quickly identify if the app had been left in a broken state, and immediately
  fix any existing bugs. If the agent had instead started implementing a new feature, it would likely make the problem
  worse."
- **What goes wrong:** "First, the agent tended to try to do too much at once—essentially to attempt to one-shot the app."
  / "After some features had already been built, a later agent instance would look around, see that progress had been
  made, and declare the job done." / "One final major failure mode that we observed was Claude’s tendency to mark a
  feature as complete without proper testing. Absent explicit prompting, Claude tended to make code changes, and even do
  testing with unit tests or curl commands against a development server, but would fail recognize [sic] that the feature
  didn’t work end-to-end."
- **Test as a user would:** "Claude mostly did well at verifying features end-to-end once explicitly prompted to use browser
  automation tools and do all testing as a human user would." / "Providing Claude with these kinds of testing tools
  dramatically improved performance, as the agent was able to identify and fix bugs that weren’t obvious from the code
  alone." The limit it names: "Claude can’t see browser-native alert modals through the Puppeteer MCP, and features
  relying on these modals tended to be buggier as a result."
- **The failure-mode table** (problem / initializer / coding agent):
  - "Claude declares victory on the entire project too early." / "Set up a feature list file: based on the input spec, set
    up a structured JSON file with a list of end-to-end feature descriptions." / "Read the feature list file at the
    beginning of a session. Choose a single feature to start working on."
  - "Claude leaves the environment in a state with bugs or undocumented progress." / "An initial git repo and progress
    notes file is written." / "Start the session by reading the progress notes file and git commit logs, and run a basic
    test on the development server to catch any undocumented bugs. End the session by writing a git commit and progress
    update."
  - "Claude marks features as done prematurely." / "Set up a feature list file." / "Self-verify all features. Only mark
    features as “passing” after careful testing."
  - "Claude has to spend time figuring out how to run the app." / "Write an init.sh script that can run the development
    server." / "Start the session by reading init.sh."
- Compaction is not enough on its own: "However, compaction isn’t sufficient."
- Future work: "It seems reasonable that specialized agents like a testing agent, a quality assurance agent, or a code
  cleanup agent, could do an even better job at sub-tasks across the software development lifecycle."

### 2.2 S6, Harness design for long-running application development
- **Context anxiety, and resets vs compaction:** "Some models also exhibit "context anxiety," in which they begin wrapping up
  work prematurely as they approach what they believe is their context limit." / "While compaction preserves continuity,
  it doesn't give the agent a clean slate, which means context anxiety can still persist." Later: "Opus 4.5 largely
  removed that behavior on its own, so I was able to drop context resets from this harness entirely."
- **Grading its own work:** "When asked to evaluate work they've produced, agents tend to respond by confidently praising the
  work—even when, to a human observer, the quality is obviously mediocre." / "agents reliably skew positive when grading
  their own work."
- **A separate evaluator** (section "Why naive implementations fall short"): "Separating the agent doing the work from the
  agent judging it proves to be a strong lever to address this issue. The separation doesn't immediately eliminate that
  leniency on its own; the evaluator is still an LLM that is inclined to be generous towards LLM-generated outputs. But
  tuning a standalone evaluator to be skeptical turns out to be far more tractable than making a generator critical of its
  own work, and once that external feedback exists, the generator has something concrete to iterate against."
- **Few-shot calibration** (section "Frontend design: making subjective quality gradable"): "I calibrated the evaluator using
  few-shot examples with detailed score breakdowns. This ensured the evaluator’s judgment aligned with my preferences, and
  reduced score drift across iterations." On the criteria: "The criteria explicitly penalized highly generic “AI slop”
  patterns, and by weighting design and originality more heavily it pushed the model toward more aesthetic risk-taking."
- **Hard thresholds; it tests like a user** (section "Scaling to full-stack coding: The architecture"): "the evaluator used the
  Playwright MCP to click through the running application the way a user would, testing UI features, API endpoints, and
  database states." / "Each criterion had a hard threshold, and if any one fell below it, the sprint failed and the
  generator got detailed feedback on what went wrong."
- **Sprint contract** ("done" agreed before any code): "Before each sprint, the generator and evaluator negotiated a sprint
  contract: agreeing on what "done" looked like for that chunk of work before any code was written." / "The generator
  proposed what it would build and how success would be verified, and the evaluator reviewed that proposal to make sure
  the generator was building the right thing. The two iterated until they agreed."
- **Files carry the hand-offs:** "Communication was handled via files: one agent would write a file, another agent would read
  it and respond either within that file or with a new file that the previous agent would read in turn."
- **The generator:** "The one-feature-at-a-time approach from the earlier harness worked well for scope management." / "the
  generator was instructed to self-evaluate its work at the end of each sprint before handing off to QA. It also had git
  for version control."
- **The planner:** "I prompted it to be ambitious about scope and to stay focused on product context and high level technical
  design rather than detailed technical implementation." / "It seemed smarter to constrain the agents on the deliverables
  to be produced and let them figure out the path as they worked."
- **The evaluator out of the box, and how it was tuned** (section "Running the harness"): "Getting the evaluator to perform at
  this level took work. Out of the box, Claude is a poor QA agent. In early runs, I watched it identify legitimate issues,
  then talk itself into deciding they weren't a big deal and approve the work anyway. It also tended to test
  superficially, rather than probing edge cases, so more subtle bugs often slipped through. The tuning loop was to read
  the evaluator's logs, find examples where its judgment diverged from mine, and update the QAs [sic] prompt to solve for
  those issues. It took several rounds of this development loop before the evaluator was grading in a way that I found
  reasonable." Then: "Even then, the harness output showed the limits of the model’s QAing capabilities: small layout
  issues, interactions that felt unintuitive in places, and undiscovered bugs in more deeply nested features that the
  evaluator hadn't exercised thoroughly."
- **When the evaluator earns its cost:** "The practical implication is that the evaluator is not a fixed yes-or-no decision.
  It is worth the cost when the task sits beyond what the current model does reliably solo." / "for tasks within that
  boundary, the evaluator became unnecessary overhead. But for the parts of the build that were still at the edge of the
  generator’s capabilities, the evaluator continued to give real lift."
- **Re-check the harness on every new model:** "every component in a harness encodes an assumption about what the model can't
  do on its own, and those assumptions are worth stress testing, both because they may be incorrect, and because they can
  quickly go stale as models improve." / "I moved to a more methodical approach, removing one component at a time and
  reviewing what impact it had on the final result." / "when a new model lands, it is generally good practice to
  re-examine a harness, stripping away pieces that are no longer load-bearing to performance".
- **Cost and time:** table "Solo 20 min $9" vs "Full harness 6 hr $200"; "The harness was over 20x more expensive, but the
  difference in output quality was immediately apparent." The DAW run with the simplified harness: "about 4 hours and $124
  in token costs" (table: "Total V2 Harness 3 hr 50 min $124.70"); "Most of the time went to the builder, which ran
  coherently for over two hours without the sprint decomposition that Opus 4.5 had needed."

### 2.3 S7, How we built our multi-agent research system
- **Tokens:** "We found that token usage by itself explains 80% of the variance, with the number of tool calls and the model
  choice as the two other explanatory factors." / "In our data, agents typically use about 4× more tokens than chat
  interactions, and multi-agent systems use about 15× more tokens than chats. For economic viability, multi-agent systems
  require tasks where the value of the task is high enough to pay for the increased performance."
- **Coding parallelizes less:** "For instance, most coding tasks involve fewer truly parallelizable tasks than research, and
  LLM agents are not yet great at coordinating and delegating to other agents in real time."
- **Over-spawning:** "Early agents made errors like spawning 50 subagents for simple queries, scouring the web endlessly for
  nonexistent sources, and distracting each other with excessive updates." Also: "agents continuing when they already had
  sufficient results".
- **Spawn counts written into the prompt:** "Agents struggle to judge appropriate effort for different tasks, so we embedded
  scaling rules in the prompts. Simple fact-finding requires just 1 agent with 3-10 tool calls, direct comparisons might
  need 2-4 subagents with 10-15 calls each, and complex research might use more than 10 subagents with clearly divided
  responsibilities."
- **What each delegation needs:** "Each subagent needs an objective, an output format, guidance on the tools and sources to
  use, and clear task boundaries. Without detailed task descriptions, agents duplicate work, leave gaps, or fail to find
  necessary information."
- **Parallelism:** "(1) the lead agent spins up 3-5 subagents in parallel rather than serially; (2) the subagents use 3+
  tools in parallel. These changes cut research time by up to 90% for complex queries".
- **LLM judge:** "We used an LLM judge that evaluated each output against criteria in a rubric" (factual accuracy, citation
  accuracy, completeness, source quality, tool efficiency); "a single LLM call with a single prompt outputting scores from
  0.0-1.0 and a pass-fail grade was the most consistent and aligned with human judgements."
- **Small evals early; humans still needed:** "We started with a set of about 20 queries representing real usage patterns." /
  "People testing agents find edge cases that evals miss." / "Even in a world of automated evaluations, manual testing
  remains essential."
- **Judge the end state:** "We found success focusing on end-state evaluation rather than turn-by-turn analysis. Instead of
  judging whether the agent followed a specific process, evaluate whether it achieved the correct final state." / "For
  complex workflows, break evaluation into discrete checkpoints where specific state changes should have occurred, rather
  than attempting to validate every intermediate step."
- **Long runs:** "We implemented patterns where agents summarize completed work phases and store essential information in
  external memory before proceeding to new tasks. When context limits approach, agents can spawn fresh subagents with
  clean contexts while maintaining continuity through careful handoffs." / "Subagents call tools to store their work in
  external systems, then pass lightweight references back to the coordinator." / "Instead, we built systems that can
  resume from where the agent was when the errors occurred." / "deterministic safeguards like retry logic and regular
  checkpoints."
- **Claude can tune its own prompts:** "When given a prompt and a failure mode, they are able to diagnose why the agent is
  failing and suggest improvements."

### 2.4 S8, Best practices for Claude Code (the wording behind forge claim 1)
- Tip box: "Give Claude a check it can run: tests, a build, a screenshot to compare. It's the difference between a session
  you watch and one you walk away from."
- "Claude stops when the work looks done. Without a check it can run, "looks done" is the only signal available, and you
  become the verification loop: every mistake waits for you to notice it. Give Claude something that produces a pass or
  fail, and the loop closes on its own."
- "The check is anything that returns a signal Claude can read in the conversation: a test suite, a build exit code, a
  linter, a script that diffs output against a fixture, or a browser screenshot compared against a design."
- Table row "Address root causes, not symptoms", After: "the build fails with this error: [paste error]. fix it and verify
  the build succeeds. address the root cause, don't suppress the error". Table row "Verify UI changes visually", After:
  "[paste screenshot] implement this design. take a screenshot of the result and compare it to the original. list
  differences and fix them".
- Gating the stop: "Across a session: set the check as a /goal condition. A separate evaluator re-checks it after every
  turn and Claude keeps working until the goal resolves." / "As a deterministic gate: a Stop hook runs your check as a
  script and blocks the turn from ending until it passes." / "By a second opinion: a verification subagent or a dynamic
  workflow that checks its own findings has a fresh model try to refute the result, so the agent doing the work isn't the
  one grading it."
- "Each step trades setup for attention. The prompt version works on any task today. The /goal and Stop hook versions are
  what let an unattended run finish correctly without you."
- "Have Claude show evidence rather than asserting success: the test output, the command it ran and what it returned, or a
  screenshot of the result. Reviewing evidence is faster than re-running the verification yourself, and it works for
  sessions you weren't watching."
- Common failure: "The trust-then-verify gap." with the fix "Always provide verification (tests, scripts, screenshots). If
  you can't verify it, don't ship it."
- Adversarial review: "Before treating a task as done, have a subagent review the diff in a fresh context and report gaps."
  / "The longer Claude works unattended, the more an independent check matters before you count the work as done."

---

## 3. The four forge claims, checked

### Claim 1: "A check Claude can run (tests, a build, a screenshot compared to a target) is what lets a run finish correctly unattended. Show evidence, not claims. Fix root causes, never suppress the error."
**Overall: CONFIRMED.** The source is the Claude Code best-practices page (S8), not the platform pages; one clause needs
a gloss.
- "A check Claude can run (tests, a build, a screenshot compared to a target)": CONFIRMED. S8: "Give Claude a check it can
  run: tests, a build, a screenshot to compare. It's the difference between a session you watch and one you walk away
  from." https://code.claude.com/docs/en/best-practices
- "is what lets a run finish correctly unattended": CONFIRMED with a gloss. S8 credits the *gated* forms with that result:
  "The /goal and Stop hook versions are what let an unattended run finish correctly without you." A check that lives
  only in the prompt "works on any task today" but doesn't stop the turn from ending. For an 8-hour run, wire the check
  to the stop (a /goal condition or a Stop hook). S2 says the same for Opus 5.5: "Treat a text-only end of turn as a
  report rather than as proof the task is done."
  https://platform.claude.com/docs/en/build-with-claude/prompt-engineering/prompting-claude-opus-5-5
- "Show evidence, not claims": CONFIRMED. S8: "Have Claude show evidence rather than asserting success: the test output, the
  command it ran and what it returned, or a screenshot of the result." Supporting (Fable 5 only, S9): "Before reporting
  progress, audit each claim against a tool result from this session."
- "Fix root causes, never suppress the error": CONFIRMED. S8 table, "Address root causes, not symptoms": "address the root
  cause, don't suppress the error".

### Claim 2: "Opus 5: give the full task up front; don't add double-check / verify again lines (it already checks; extra verification wastes tokens); give explicit delegation rules and keep spawn counts low; it can widen scope, so state the scope; for reviews, ask it to report everything and filter in a separate pass instead of only report high severity (that makes it report less)."
**Overall: CONFIRMED** (all five parts are in S1). https://platform.claude.com/docs/en/build-with-claude/prompt-engineering/prompting-claude-opus-5
- Full task up front: CONFIRMED. "it performs best when given the complete task specification up front and left to run."
- No double-check / verify-again lines: CONFIRMED. "If your prompt contains explicit verification instructions ...
  remove them: instructions like these cause over-verification on Claude Opus 5, and removing them reduces wasted tokens
  with no loss in quality." and "Avoid instructing re-checks it already performs ("double-check your answer,"
  "re-verify before responding")". The same section covers harness scaffolding too: "The same applies to legacy harness
  scaffolding that adds separate verification steps." (Tension B below.)
- Explicit delegation rules, low spawn counts: CONFIRMED. "give explicit guidance on which scenarios warrant delegation, or
  set deterministic caps on how many agents can be launched" and "If one subagent can complete the task, use one rather
  than several, and keep spawn counts low." S1 frames the cap partly around cost: "For cost-sensitive workloads, cap
  delegation".
- Widens scope, so state it: CONFIRMED. "Claude Opus 5 can also expand the scope of a task, adding steps that weren't
  requested or applying its own judgment about what the task should be." Two notes: S1 says "For narrow tasks, constrain
  scope explicitly", and its sample prompt guards against both directions ("rather than quietly narrowing, widening, or
  transforming it").
- Reviews: report everything, filter separately: CONFIRMED. "If your review prompt says "only report high-severity issues"
  or "be conservative," the model might follow that instruction literally and report less; ask it to report everything
  and filter in a separate pass instead."
- If the run uses Opus 5.5: S2 says Opus 5 prompts carry over, but its default effort is `medium`, not `high` (section
  1.7).

### Claim 3: "Long runs: a JSON feature list with a pass flag that only flips on evidence; a progress file; git commits per slice; one feature at a time; tests are never edited to pass (tests verify correctness, they don't define the solution; if a test is wrong, say so)."
**Overall: CONFIRMED.** The only wording gap is "evidence" (see the first bullet).
- JSON feature list with a pass flag: CONFIRMED. S5: `"passes": false` in the example; "We prompt coding agents to edit this
  file only by changing the status of a passes field" and "we landed on using JSON for this, as the model is less likely
  to inappropriately change or overwrite JSON files compared to Markdown files."
  https://www.anthropic.com/engineering/effective-harnesses-for-long-running-agents
- "only flips on evidence": CONFIRMED in substance. S5 says "Only mark features as “passing” after careful testing." and
  wants testing done "as a human user would" with browser automation. The word "evidence" comes from S8 ("show evidence
  rather than asserting success"), not S5.
- A progress file: CONFIRMED. S5: "a claude-progress.txt file that keeps a log of what agents have done". S3:
  "Review progress.txt, tests.json, and the git logs."
- Git commits per slice: CONFIRMED. S5: "ask the model to commit its progress to git with descriptive commit messages and to
  write summaries of its progress in a progress file", and the table: "End the session by writing a git commit and
  progress update." S5 places the commit after each code change and at session end, which matches a per-slice commit
  in a one-feature-at-a-time run.
- One feature at a time: CONFIRMED. S5: "the next iteration of the coding agent was then asked to work on only one feature
  at a time. This incremental approach turned out to be critical to addressing the agent’s tendency to do too much at
  once."
- Tests never edited to pass: CONFIRMED. S5 and S3: "It is unacceptable to remove or edit tests because this could lead to
  missing or buggy functionality." (In S5 "tests" means the feature-list entries; in S3 it means `tests.json`.)
- "tests verify correctness, they don't define the solution": CONFIRMED. S3: "Tests are there to verify correctness, not to
  define the solution." https://platform.claude.com/docs/en/build-with-claude/prompt-engineering/claude-prompting-best-practices
- "if a test is wrong, say so": CONFIRMED. S3: "If the task is unreasonable or infeasible, or if any of the tests are
  incorrect, please inform me rather than working around them."
- In the sources but not in the claim (worth adding): S5's start-of-session steps (pwd, git log plus the progress file,
  the highest-priority unfinished feature) and "run through a basic end-to-end test before implementing a new feature";
  an `init.sh` that starts the app; S3's "Use structured formats for state data" (JSON) next to "Use unstructured text
  for progress notes".

### Claim 4: "A separate evaluator is easier to make skeptical than a builder is to make self-critical, but out of the box it talks itself into approving. It needs hard thresholds, few-shot examples of real failures, and tuning against its own past verdicts."
**Overall: PARTLY.** The first two parts and "hard thresholds" match S6. The few-shot and tuning parts don't match what
S6 says. https://www.anthropic.com/engineering/harness-design-long-running-apps
- Easier to make skeptical than a builder self-critical: CONFIRMED. "tuning a standalone evaluator to be skeptical turns out
  to be far more tractable than making a generator critical of its own work". S6 adds that separation alone doesn't
  fix it: "The separation doesn't immediately eliminate that leniency on its own; the evaluator is still an LLM that is
  inclined to be generous towards LLM-generated outputs."
- Out of the box it talks itself into approving: CONFIRMED. "Out of the box, Claude is a poor QA agent. In early runs, I
  watched it identify legitimate issues, then talk itself into deciding they weren't a big deal and approve the work
  anyway." The same paragraph names a second failure: "It also tended to test superficially, rather than probing edge
  cases".
- Hard thresholds: CONFIRMED. "Each criterion had a hard threshold, and if any one fell below it, the sprint failed and the
  generator got detailed feedback on what went wrong." (from the full-stack harness)
- "few-shot examples of real failures": PARTLY. S6 says "I calibrated the evaluator using few-shot examples with detailed
  score breakdowns. This ensured the evaluator’s judgment aligned with my preferences, and reduced score drift across
  iterations." That is in the frontend-design experiment, and the examples are score breakdowns aligned to the owner's
  taste. S6 doesn't say they were real failures.
- "tuning against its own past verdicts": PARTLY. In S6 the author tunes it, against the author's own judgment: "The tuning
  loop was to read the evaluator's logs, find examples where its judgment diverged from mine, and update the QAs [sic]
  prompt to solve for those issues. It took several rounds of this development loop". The reference point is where its
  past verdicts diverged from a human's, not the verdicts themselves.
- Suggested rewording that matches the source: "It needs a hard threshold per criterion (any one below its bar fails the
  slice), few-shot calibration examples with detailed score breakdowns, and a tuning loop: read its logs, find verdicts
  that diverged from the owner's judgment, update its prompt, repeat for several rounds."
- A qualifier from S6 the claim leaves out: "the evaluator is not a fixed yes-or-no decision. It is worth the cost when
  the task sits beyond what the current model does reliably solo."

---

## 4. Tensions the forge should settle in the prompt

- **A. How reviewers should report.** S1 (Opus 5): "ask it to report everything and filter in a separate pass instead".
  S8 (not model-specific): "Tell the reviewer to flag only gaps that affect correctness or the stated requirements, and
  treat the rest as optional." Both work together if the reviewer reports every finding with a severity, and a separate
  pass (the lead or a filter step) decides what gets acted on. S8 warns about the cost of skipping that step: "Chasing
  every finding leads to over-engineering".
- **B. No self-verification lines vs a separate evaluator.** S1 says to remove "use a subagent to verify" lines and "do not
  use subagents to verify or double-check your own work". It also praises "effective writer-verifier patterns". S6 keeps
  a separate evaluator only where it is load-bearing. S8 recommends "have a subagent review the diff in a fresh context"
  for unattended work. What I read supports this split: drop generic re-check lines and self-verify subagents, and keep
  an independent evaluator with criteria and thresholds where "the task sits beyond what the current model does reliably
  solo" (S6). Whether a given forge critic is load-bearing was not checked; S6's method is to remove one component at a
  time and measure.
- **C. Effort for an 8-hour run.** The S4 table suggests `xhigh` for "Long-running agentic and coding tasks (over 30
  minutes)". The per-model guidance overrides it: Opus 5 starts at `high` and steps up to `xhigh` "for demanding coding
  and agentic work". Opus 5.5 starts at `medium` and should "Reserve `xhigh` and `max` for work where you've measured a
  quality gain". Say in the prompt which model runs it.
- **D. Early stops in unattended runs (Opus 5.5).** S2: turns that end in a status message stop an unattended loop. Its
  fixes are a checklist file the model updates, a nudge naming the open items, a completion condition checked by a
  separate smaller model at each end of turn, and "stop after two or three automatic continuations on the same task". S8:
  a `/goal` condition gives the same kind of per-turn evaluator.
- **E. Context.** S1: 1M-token window, behaviour "consistent throughout the window". S6: "Opus 4.5 largely removed that
  behavior [context anxiety] on its own". S3 still suggests a fresh window over compaction in some cases ("Claude's latest
  models are extremely effective at discovering state from the local filesystem"). For any session restart, state files
  plus git are the hand-off in every source.

## 5. Not checked
- The Claude Code `/goal`, Stop hook and subagent-cap docs pages (I know them only from S1 and S8 lines).
- The "What's new in Claude Opus 5.5" and Opus 5 overview pages, and the Fable 5.1 prompting page.
- The rest of S9 (Fable 5); only its two sections quoted above were read.
- S5's quickstart code, S6's appendix, and S7's appendix.
- Whether any of these lines is reproduced on these pages for Sonnet or Haiku (not needed for an Opus run).
