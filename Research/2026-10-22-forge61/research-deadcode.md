# Finding and deleting dead code safely (FORGE61 research)

2026-10-22. Researcher agent. Read only in the repo. The one build happened in the session scratchpad, outside the repo
(section 5.2). Budget: about 50 tool calls, against the 40 asked; the extra calls went into that scratch probe.
"Verified" means I read the page at the URL and the quote is the page's own words. A quote marked "(fetch summary)" is
the fetch tool's wording, not the page's. "Not found" means I looked and did not find it. "Not checked" means I did not look.

## Answer first

- IDE0051 and IDE0052 only flag **private** members. Neither rule runs on a command-line build unless
  `EnforceCodeStyleInBuild=true` is set and the rule's severity is raised in a config file. Roslyn marks both as
  `EnforceOnBuild.HighlyRecommended`, so in principle they can run on build.
- **Measured here:** a scratch copy of `Skygrind.Core.csproj` builds with `dotnet build` in 8 s and writes nothing into
  the repo. But IDE0051/52 reported **0 hits in 4 builds, including on a planted file with three dead members**. The
  probe is not working yet, and I did not find why (5.2). It did find **2 x CS0414** (a private field is written but
  never read) in Core.
- Running `dotnet build` / `dotnet format` on the generated csproj **in place** writes to `Temp\obj\<Project>\` and
  `Temp\bin\Debug\` inside the repo. The csproj files say so themselves. Run them on scratch copies only.
- Unity's own compiler: I found no Unity doc saying it can report IDE0051. Unity analyzers "must use
  Microsoft.CodeAnalysis.Csharp 4.3". Adding an analyzer DLL to `Assets/` also changes the repo, so it needs GG's OK.
- Unused **public** members: Roslyn has no built-in rule for them (Not found). JetBrains InspectCode is a free CLI with
  solution-wide analysis (`--swea`). It builds the solution first unless given `--no-build`. It is a machine install,
  so it needs GG's OK.
- Code Coverage (`com.unity.testtools.codecoverage`) is **not in the manifest**, so adding it needs GG's OK. It runs
  **only in the Editor**; the docs say "not in Standalone/Player". Output is OpenCover XML, plus an HTML report if
  you ask for one.
- Unused assets: you can run this now, with no install, as a read-only eval. Use `AssetDatabase.GetDependencies` on the
  build scenes and `BuildReport.GetLatestReport().packedAssets` after the night's Player build. Two MIT tools take the
  same approach (Unity-Dependencies-Hunter, Asset Usage Detector), but both have to be installed in the project.

## 1. Roslyn IDE0051 / IDE0052

### Verified

- IDE0051, https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/ide0051 :
  "This rule flags unused private methods, fields, properties, and events that have no read or write references."
  Category "CodeQuality". In the page's example, `internal readonly int FieldInternal;` and
  `public int GetNumPublic() => GetNumPrivateUsed();` sit under "// No IDE0051".
- IDE0052, https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/ide0052 :
  "This rule flags private fields and properties that have one or more write references but no read references."
- Build enforcement, https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/overview :
  - "Command-line build: Code-style analysis is *disabled*, by default, for all .NET projects on command-line builds."
  - "1. Set the MSBuild property EnforceCodeStyleInBuild to `true`."
  - "2. In an *.editorconfig* file, configure each "IDE" code style rule that you wish to run on build as a warning or
    an error." The page's example line is `dotnet_diagnostic.IDE0040.severity = warning`; for these rules it would be
    `dotnet_diagnostic.IDE0051.severity = warning`.
  - "(However, for performance reasons, a handful of code-style rules will still apply only in the Visual Studio IDE.)"
- Which rules can run on build, https://github.com/dotnet/roslyn/blob/main/src/Analyzers/Core/Analyzers/EnforceOnBuildValues.cs :
  - `public const EnforceOnBuild RemoveUnusedMembers = /*IDE0051*/ EnforceOnBuild.HighlyRecommended;`
  - `public const EnforceOnBuild RemoveUnreadMembers = /*IDE0052*/ EnforceOnBuild.HighlyRecommended;`
  - IDE0035 (unreachable code) is in the `EnforceOnBuild.Never` group, so it is IDE-only.
- A config file that applies to sources outside the project folder,
  https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/configuration-files :
  - Global AnalyzerConfig files provide "options that apply to all the source files in a project, regardless of their
    file names or file paths."
  - "if you name these files as `.globalconfig`, then they're implicitly applied to all the C# and Visual Basic
    projects within the current folder, including subfolders. Otherwise, you must explicitly add the
    `GlobalAnalyzerConfigFiles` item to your MSBuild project file".
  - Precedence: "The entry in the EditorConfig file wins."
- `dotnet format`, https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-format :
  - "`dotnet format analyzers` - Formats code to match `editorconfig` settings for analyzers (excluding code style
    rules)." So for IDE0051/52 the subcommand is `dotnet format style`, not `analyzers`. The page's own example is
    `dotnet format style --diagnostics IDE0005 --severity info`.
  - "`dotnet format` is a code formatter that applies style preferences and static analysis recommendations". Run
    without `--verify-no-changes`, it applies the code fix, and for IDE0051 that deletes members. Never run it that way
    on the live tree.
  - `--verify-no-changes`: "Terminates with a non zero exit code if any files would have been formatted."
  - `--report <REPORT-PATH>`: "Produces a JSON report in the directory specified by <REPORT_PATH>."
  - `--no-restore`: "Doesn't execute an implicit restore before formatting. Default is to do implicit restore."
  - "dotnet format may restore, compile, and run analyzers from the specified project or solution."
  - What the JSON report holds for IDE0051: not checked.

### What they cannot see

- Public and internal members are out of scope by definition (the IDE0051 page above).
- Unity messages and serialized fields: Microsoft.Unity.Analyzers (MIT) ships *suppressors* for exactly these false
  positives. That implies plain IDE0051 flags them. List from https://github.com/microsoft/Microsoft.Unity.Analyzers/blob/main/doc/index.md :
  - USP0003, IDE0051: "The Unity runtime invokes Unity messages"
  - USP0006, IDE0051: "Don't flag private fields decorated with serialization attributes as unused"
  - USP0008, IDE0051: "Don't flag private methods used with `Invoke`/`InvokeRepeating` or `StartCoroutine`/`StopCoroutine` as unused"
  - USP0009, IDE0051: "Don't flag methods decorated with `MenuItem`/`ContextMenu` attribute or referenced by a field with the…"
    (the fetch truncated it)
  - USP0010, IDE0051: "Don't flag fields decorated with the `ContextMenuItem` attribute as unused"
  - USP0012, IDE0051: "Don't flag private methods with `InitializeOnLoadMethod`, `RuntimeInitializeOnLoadMethod` or…" (truncated)
  - USP0019, IDE0051: "Don't flag private members with implicit-usage attributes as unused"
  - USP0020, IDE0052: "The Unity runtime invokes Unity messages"
  - USP0007, CS0649 and USP0013, CA1823 cover serialized fields.
- How the suppressors reach a project: the README says Visual Studio ships them "in the box". It also says that
  starting with Visual Studio Tools for Unity 4.3.2.0 they are added to Unity-generated projects "through an
  `<Analyzer Include="..." />` directive" (fetch summary).
  https://github.com/microsoft/Microsoft.Unity.Analyzers
  **Locally they are in none of the 8 csproj files, and the DLL is not on disk** (section 5).
- Reflection, string-based calls (`SendMessage`, `Invoke("name")` beyond what USP0008 covers), UnityEvent persistent
  calls in scenes, animation events and `unity command eval` scripts: Not found in the docs. My inference: none of these
  are visible to a Roslyn symbol analysis, so they need a text search of the YAML and the scripts.

### Unused public members or types across the solution

- A Roslyn / .NET SDK built-in rule for unused public members: Not found.
- JetBrains InspectCode, https://www.jetbrains.com/help/resharper/InspectCode.html :
  - The page calls it "a free cross-platform command line tool" (fetch summary).
  - `--swea` / `--no-swea`: "these parameters let you explicitly enable or disable solution-wide analysis."
  - "By default, InspectCode restores NuGet packages and builds the target solution before starting the analysis."
    Use `--no-build` to skip that.
  - The output defaults to SARIF; `-f="xml"` gives XML.
  - "by default, InspectCode only reports issues with the severity level Suggestion and higher."
  - Not checked: the exact inspection ID for unused public members; whether the Unity plugin loads in the CLI (the page
    names only StyleCop and ReSpeller as `-x` extensions); whether it reads `.slnx`.
  - Licence: free, but not MIT/Apache.
- Roslynator, https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1213/ : "RCS1213: Remove unused member
  declaration", default severity "Info". It has a Unity option, `roslynator_unity_code_analysis.enabled = true|false`.
  Which accessibility levels it covers: Not found on the page. Licence: not shown on the page (not checked).
- `dotnet-unused` and other "unused code" tools: not checked.

## 2. Unity specifics

- Unity regenerates the csproj files. Local fact, line 2 of every root csproj: "Generated file, do not modify, your
  changes will be overwritten (use AssetPostprocessor.OnGeneratedCSProject)". Here they are SDK-style
  (`<UnityProjectGeneratorStyle>SDK</UnityProjectGeneratorStyle>`, generator 2.0.26 = com.unity.ide.visualstudio), so
  `dotnet build` can load them (measured in 5.2).
- Analyzer DLLs in Unity:
  - https://docs.unity3d.com/6000.3/Documentation/Manual/install-existing-analyzer.html : the `RoslynAnalyzer` asset
    label ("This label must exactly match the example and is case sensitive."); "Under the Select platforms for plugin
    heading, disable Any Platform."; "Under the Include Platforms heading, disable Editor and Standalone."; "Any scripts
    that are within the same assembly definition as an analyzer are in that analyzer's scope."; "Unity recompiles the
    script and runs any applicable analyzers on the script's code."
  - https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/script-compilation/roslyn-analyzers/analyzer-scope-and-diagnostics :
    - "By default, analyzers in the root of the Assets folder apply to all the predefined assemblies in your project".
      This scope does not cover asmdef assemblies; all our code is in asmdefs (Skygrind.Core, Skygrind.Game, ...).
    - "If an analyzer is in a folder that contains an assembly definition file, or one of its subfolders, the analyzer
      only applies to that assembly, and to any other assembly that references it."
    - Severity comes from `Default.ruleset` / per-assembly `.ruleset`, and: "Alternatively, you can use editorconfig
      file to centralize exclusions."
  - https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/script-compilation/roslyn-analyzers/create-roslyn-analyzer.md :
    "Your analyzer must use Microsoft.CodeAnalysis.Csharp 4.3 to work with Unity." and "After Unity recompiles scripts,
    the following warning appears in the Console:".
  - https://docs.unity3d.com/6000.3/Documentation/Manual/roslyn-analyzers.html : "Roslyn analyzers are only
    compatible with the IDEs that Unity supports."
- Can Unity's compiler report IDE0051? Not found in the Unity docs. My inference, not tested: the IDE rules live in the
  SDK's CodeStyle analyzer DLLs, built against a newer Roslyn than 4.3, so they may not load in Unity. Either way it
  means adding a labelled DLL under `Assets/`, which is a repo change (needs GG's OK).
- `csc.rsp` for analyzer or warning switches: not checked. There is no `csc.rsp` in the repo (section 5).
- Running `dotnet build` from the command line: it works on a scratch copy. In place it would write into the repo's
  `Temp/`. See 5.2 for the measured run and what it writes.

## 3. Unity Code Coverage (`com.unity.testtools.codecoverage`): needs a package added

- It is not in `Packages/manifest.json` (measured). Adding it is a manifest change, so it needs GG's OK. The docs
  header shows 1.2.7, and compatibility is "2019.3 and later":
  https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.2/manual/TechnicalDetails.html
- Player: "Code Coverage currently only supports code run in the Editor and not in Standalone/Player." **Confirmed**
  (same TechnicalDetails page). Format: "The package uses a combination of this API and C# reflection to output the
  test coverage data in the OpenCover format." Also: "NPath Complexity calculation and Branch Coverage are not
  implemented at present".
- Command line, https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.2/manual/CoverageBatchmode.html . The page's example:
  `Unity.exe -projectPath <path-to-project> -batchmode -testPlatform editmode -runTests -testResults <path-to-results-xml> -debugCodeOptimization -enableCodeCoverage -coverageResultsPath <path-to-coverage-results> -coverageHistoryPath <path-to-coverage-history> -coverageOptions "generateAdditionalMetrics;generateHtmlReport;generateHtmlReportHistory;generateBadgeReport;assemblyFilters:+my.assembly.*,+<packages>;pathFilters:-**/Tests/**,-**/BuiltInPackages/**"`
  - Options include `generateHtmlReport`, `generateAdditionalReports` ("SonarQube, Cobertura and LCOV"),
    `assemblyFilters`, `pathFilters`, `filtersFromFile`, `dontClear` and `useProjectSettings` (which "can only be used
    in batchmode").
  - "The OpenCover results xml format specifies file paths as absolute paths (fullPath)."
  - In this project, run it on the **batch copy**, never on the live project. CLAUDE.md forbids a second Editor, and
    the memory note "Batch tests without the Editor" describes the batch-copy route.
- Live Editor, https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.2/manual/CoverageTestRunner.html :
  "Select Enable Code Coverage if not already selected"; "Switch to the Test Runner and run your EditMode and/or
  PlayMode test(s)."; "When the test(s) finish running, a file viewer window opens containing the coverage report.";
  "Enabling Code Coverage adds some overhead to the Editor and can affect the performance." The index page also offers
  "a Coverage Recording feature which allows capturing coverage data on demand":
  https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.2/manual/index.html
- Fit for this project: it suits Skygrind.Core (pure C#, the EditMode suite with golden vectors and replays). It is weak
  for Skygrind.Game. The Player smoke and reels are not covered, and campaign levels never load in the Editor (L-043),
  so a 0 % line in Game is a hint, never a reason to delete.

## 4. Unused scripts, shaders and assets

- https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AssetDatabase.GetDependencies.html : "Returns an
  array of all the assets that are dependencies of the asset at the specified pathName." "If recursive is true, the
  list returned will also include the input path itself." The page also warns that "these references are not
  necessarily required during the build process."
- https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Build.Reporting.BuildReport.html : "A BuildReport
  object is returned by BuildPipeline.BuildPlayer"; `packedAssets`: "An array of all the PackedAssets generated by the
  build process."; `GetLatestReport`: "Return the build report generated by the most recent Player or AssetBundle
  build"; "Certain errors like invalid input can fail the build immediately, with no BuildReport written".
- Addressables Analyze: does not apply. `com.unity.addressables` is not in the manifest (measured).
- Open-source tools, both MIT, both need an install:
  - Unity-Dependencies-Hunter, https://github.com/AlexeyPerov/Unity-Dependencies-Hunter (MIT): "This tool finds
    and/or deletes unreferenced assets in Unity project." It maps every asset with GetAllAssetPaths + GetDependencies,
    "Then it simply finds all assets which are not presented as a dependency within this map." Caveat: "runtime-only
    references are still possible false positives". Install by git URL in the Package Manager, or by copying
    `DependenciesHunter.cs` into an Editor folder. Both change the repo. Its two-API method can be redone as a scratch
    eval with no install.
  - Asset Usage Detector, https://github.com/yasirkula/UnityAssetUsageDetector (MIT): it "helps you find usages of the
    selected asset(s) and/or scene object(s)". It installs 5 ways: .unitypackage, copying Plugins, Asset Store, git URL,
    OpenUPM. Good for checking one candidate before deleting it. It does not list unused assets.
- Unity Project Auditor and Build Report Inspector: not checked.

## 5. Local facts (read only)

### 5.1 The repo

- `Packages/manifest.json`:
  - no `com.unity.testtools.codecoverage`, no `com.unity.addressables`
  - `com.unity.test-framework` 1.6.0 (line 18), `com.unity.ide.rider` 3.0.39 (line 11), `com.unity.ide.visualstudio`
    2.0.26 (line 12)
  - 53 lines contain `"com.`
- No `.editorconfig`, `.globalconfig`, `*.ruleset`, `*.rsp` (so no `csc.rsp`), `Directory.Build.props/targets` or
  `omnisharp.json` anywhere in the repo outside `Library/`, `Temp/` and `Builds/`. There is none in `C:\Users\gaige`,
  `C:\Users\gaige\dev` or `C:\` either.
- Root project files: `SKYGRIND-unity.slnx` and 8 csproj. Compile items per csproj:

  | csproj | Compile items |
  | --- | --- |
  | Assembly-CSharp | 1 |
  | Assembly-CSharp-Editor | 12 |
  | Skygrind.Bench | 3 |
  | Skygrind.Core | 292 |
  | Skygrind.Core.Tests | 162 |
  | Skygrind.Game | 1142 |
  | Skygrind.Game.Tests | 180 |
  | Skygrind.HudFx20 | 2 |

  All are SDK-style: `netstandard2.1`, `LangVersion` 9.0, `<NoWarn>0169;USG0001</NoWarn>` (CS0169 = "field never used"
  is silenced), `BaseIntermediateOutputPath` = `Temp\obj\$(MSBuildProjectName)`, `OutputPath` = `Temp\bin\Debug\`.
- Analyzer items in the csproj files:
  - all 8: Unity's 3 source generators
  - Assembly-CSharp(-Editor) also: com.unity.ai.inference SourceGenerators.dll, com.unity.pipeline
    IlInterpreterAnalyzer.dll, 2 com.unity.dt.app-ui generators
  - **No Microsoft.Unity.Analyzers** in any of them. The DLL was not found under `~/.vscode/extensions`,
    `~/.nuget/packages` (depth 4) or `C:\Program Files\Microsoft Visual Studio` (depth 9).
- `dotnet` 10.0.401 on PATH (`C:\Program Files\dotnet`). The SDK's code-style analyzers are on disk
  (`sdk\10.0.401\Sdks\Microsoft.NET.Sdk\codestyle\cs\`). No global dotnet tools are installed (no InspectCode, no
  Roslynator CLI).
- An in-place `dotnet build Skygrind.Core.csproj -p:EnforceCodeStyleInBuild=true` would write
  `Temp\obj\Skygrind.Core\` (restore assets, intermediates) and `Temp\bin\Debug\` (DLLs) **inside the repo**. `Temp/`
  is the live Editor's folder. So: not allowed, and not run.
- Size:

  | Folder | .cs files | Lines |
  | --- | --- | --- |
  | Assets/Scripts/Core | 292 | 87,152 |
  | Assets/Scripts/Game | 1,142 | 201,453 |
  | Assets/Editor | 11 | 2,133 |
  | Assets/Tests | 342 | not counted |
  | Bench + HudFx20 | 5 | not counted |

  There are 1,794 .cs files under `Assets/` in all.
- Night-tagged files, out of 1,792 file names under Scripts, Editor and Tests:
  - 337 contain `.N<digits>` (e.g. `Loom37.N40.cs`, `Crown37.N47.cs`)
  - 492 are partial splits `X.Y.cs`, and 372 of those end in two digits (e.g. `Arcline21View.Grapple47.cs`)
  - 1,217 names contain a two-digit number (Core 151/292, Game 876/1,142, Editor 3/11, Tests 185/342)
  - Most frequent groups: 20 (104), 43 (75), 37 (67), 33 (58), 47 (53), 24 (53), 40 (48), 22 (43), 55 (41), 42 (40),
    29 (39), 26 (39). These are rough: every two-digit group in a name counts, and some are not night numbers.

### 5.2 Scratch probe (outside the repo; measured)

- What I did:
  - copied `Skygrind.Core.csproj` to the scratchpad and made its 293 `Include=` paths absolute (a node script)
  - added `.globalconfig` (`is_global = true`, IDE0051 and IDE0052 = warning)
  - ran `dotnet build -p:EnforceCodeStyleInBuild=true -nodeReuse:false -p:UseSharedCompilation=false`
- **Exit 0, 8 s.** Outputs went only to the scratch `Temp/obj` and `Temp/bin`. Afterwards the repo had no
  `Temp/obj/Skygrind.Core*` and no `Temp/bin`. The only new `git status` line was the `Research/2026-10-22-forge61/`
  folder, which other agents write to.
- Warnings:
  - 42 x MSB3245, unresolved references. The `<HintPath>` values are still relative in the copy. This is harmless for
    Core (`noEngineReferences`); a Game copy must rewrite `<HintPath>` too.
  - **2 x CS0414** (private field assigned, value never used) in Core.
- **IDE0051/IDE0052: 0 hits in 4 builds**, and also 0 on a planted file (`Planted.cs`: two unused private members and
  one write-only field). The planted file did compile: its 2 x CS0414 showed. So the RED case failed, and the probe
  does not report these rules yet.
  - `.globalconfig` reached csc only as an explicit `GlobalAnalyzerConfigFiles` item in the csproj copy. Neither the
    implicit pickup nor a `Directory.Build.targets` item got it there.
  - The SDK's `CSharp.CodeStyle.dll` name appears once in each of runs 2 and 3. A narrower grep for it as an
    `/analyzer:` switch found nothing, but the "Program Files" space breaks that grep, so it proves nothing.
  - The generated analyzer config has `build_property.EffectiveAnalysisLevelStyle =` and
    `build_property.EnableCodeStyleSeverity =`, both empty.
  - Adding `-p:EnableNETAnalyzers=true -p:AnalysisLevel=latest` changed nothing.
  - Cause: **not found**. Next steps:
    1. Build with `-bl` and read the Csc task's `Analyzers` and `AnalyzerConfigFiles`.
    2. Add the SDK's `codestyle\cs\*.dll` as explicit `<Analyzer>` items.
    3. Try `-p:AnalysisLevelStyle=latest`.
    4. Accept the probe only when `Planted.cs` shows 2 x IDE0051 + 1 x IDE0052.
- Probe files (scratch): `...\scratchpad\dc-probe\` and `...\scratchpad\scripts\copy-csproj.cjs`, `inject-probe.cjs`.

## 6. Recommended dead-code method for a night run with no new installs

Taxonomy first. Only one kind is free to delete.

| Kind | Example | Rule |
| --- | --- | --- |
| A. Unreferenced code | a type or member nothing names | delete through the gate below |
| B. Retired-row code | rows in `docs/RETIRED.md` (rank56 retired at w57) | delete through the gate; list the rows in the commit |
| C. OFF-row code | kill55, yellow56 (shelved, "OFF by their own numbers") | **keep**; deleting a re-testable row is GG's call |
| D. Hooks kept on purpose | gameplay event names for GG's audio (`PORT-STATUS.md`, `docs/AUDIO-HOOKS.md`); `Mover.cs` (rule 3) | **never** delete as dead |

I did not read RETIRED.md or FEATURES.md for this note. The examples come from CORE.md.

### Can run now (no OK needed; read only plus scratch)

1. **Reference census** (text, minutes). For every type and file in `Assets/Scripts/**` and `Assets/Editor/**`:
   - count name references outside its own declaration files, across all `.cs` (partials and tests too) and `Tools/**`
     (eval files call types by name);
   - check the `.cs.meta` GUID against `*.unity`, `*.prefab`, `*.asset` (`guid:`);
   - check method names against YAML `m_MethodName:` (UnityEvents) and `functionName:` (animation events).

   Zero hits on all of these makes a candidate. A night partial (`X.N47.cs`) is a candidate only if every member it
   adds has zero hits.
2. **Compiler-warning harvest** on scratch csproj copies. CS0414 already found 2 in Core (measured). Add CS0169 by
   overriding `NoWarn` in the copy, and CS0219. For Game, the copy script must also rewrite `<HintPath>`.
3. **Finish the IDE0051/52 scratch probe** (5.2 next steps). Accept it only after the planted case flags. For Game,
   remove Unity message names from the hit list by hand (Awake, Start, Update, LateUpdate, FixedUpdate, OnEnable,
   OnDisable, OnDestroy, OnValidate, OnGUI, OnDrawGizmos*, OnTrigger*, OnCollision*, OnApplicationFocus/Pause/Quit,
   Reset). The Unity suppressors are not on disk.
4. **Asset census as a read-only eval:**
   - `AssetDatabase.GetDependencies(<build scenes>, true)` gives the "used" set; and after the night's Player build,
     `BuildReport.GetLatestReport().packedAssets` gives what shipped.
   - Candidates = all assets minus used, minus `Resources/` (fonts), `StreamingAssets/` (PortExport, Design/levels
     JSON), `Editor/` and `Tests/`, minus anything loaded by a string (`Resources.Load`, `Shader.Find`, file reads):
     grep those calls first.
5. **Delete in small batches**, one commit each: explicit paths **plus their `.meta`**. A removed `[SerializeField]`
   field also loses its scene/prefab data, so grep the field name in YAML first. Gate per batch:
   - compile (`Tools/unity-loop.sh compile`, errors 0)
   - **all golden vectors 0 mismatches and 16 replays bit-exact** if anything in `Skygrind.Core` changed (rule 3)
   - the Game suite in class batches
   - the Player smoke and verbs37 x5 on the build that ships (PRINCIPLES rule 1)

   Revert the batch on any red.

### Needs GG's OK first

- Adding `com.unity.testtools.codecoverage` to the manifest. Then run EditMode coverage on the batch copy with
  `assemblyFilters:+Skygrind.Core,+Skygrind.Game`. It is Editor-only, so it is a hint for Game, not proof.
- Installing machine tools: JetBrains InspectCode (`--swea --no-build`, on a scratch copy, for unused **public**
  members); the Microsoft.Unity.Analyzers or Roslynator DLLs from NuGet (used only in the scratch probe).
- Adding Unity-Dependencies-Hunter or Asset Usage Detector to the project.
- Any deletion of OFF-row code, enemy-design code (rule 7) or `Mover.cs` (rule 3).

### Known false-positive sources here (not checked one by one)

- `unity command eval` scripts and `Tools/*.sh`, which call types by name
- reflection-based JSON (`JsJson`/`GameConfig`), which may fill fields by name (not checked)
- Player-only entry points (`-skygrindSmoke`, `-skygrindBench`, reels)
- `[MenuItem]` editor scripts
- Unity messages
- partial classes split across night files
