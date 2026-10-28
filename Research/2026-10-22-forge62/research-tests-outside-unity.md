# Running the EditMode NUnit tests outside the Unity Editor (forge62 research)

Read on 2026-10-22. Web research only: nothing was built, installed or run, and no repo file was changed except this
one. **Verified** means I fetched that page or source file in this session and it says this (quotes are as the fetch
tool returned them; it shortens long passages). **Inferred** means it is my reasoning from the cited pages, not
something a source states. **Not checked** means I did not look. Tags like [ms-nunit] resolve in the Sources table at
the end; the detail sections also give the URLs inline.

## The answer in 10 lines

1. **Yes, for code that never calls into the engine.** A test assembly built against official NUnit 3.x runs under
   `dotnet test` (needs `Microsoft.NET.Test.Sdk` + `NUnit` + `NUnit3TestAdapter`), under NUnitLite (the `NUnitLite`
   package plus `return new AutoRun().Execute(args);` makes the test project its own console exe; the exit code is
   failures + errors + invalid), or under nunit3-console (a separate NuGet package, zip or MSI).
   (Verified: [ms-nunit], [nunit-adapter-install], [nunitlite], [nunitlite-src], [nunit-install].)
2. **Offline, all three still come from NuGet.** A restore needs no network only when the exact versions are already
   in the global packages folder (`%userprofile%\.nuget\packages`) or in a local folder feed (`dotnet restore -s
   <folder>`). Without that, each of them needs a one-time download. (Verified: [nuget-folders], [nuget-local],
   [dotnet-restore]. "One-time download otherwise" is Inferred.)
3. **Unity's own `nunit.framework.dll` is the only zero-download route, and it is unproven.** com.unity.ext.nunit
   2.0.5 is a custom NUnit 3.5 "precompiled for .NET 4.0"; the only binary the package mirror shows is that DLL (no
   nunitlite, no console).
   Upstream NUnit 3.5 exposes the public runner classes NUnitLite itself is built on (`NUnitTestAssemblyRunner`,
   `DefaultTestAssemblyBuilder`), so a ~30-line self-hosted `Main` could drive it. But standard runners have broken on
   Unity's custom build before ("cannot work with standard NUnit 3 test runner", 2017), and NUnitLite pins the exact
   NUnit version, so it cannot be paired with Unity's DLL. (Verified: [ext-nunit-mirror], [ext-nunit-docs],
   [nunit35-src], [franknine], [ncrunch], [nuget-nunitlite]. The route itself is Inferred.)
4. **The recipe others use:** a hand-written csproj that compiles the same `.cs` files as the `noEngineReferences`
   assembly and its tests, with netstandard2.1 and C# 9 for the code under test, NUnit 3.14 (it keeps the classic
   asserts), and a .NET 8 test host. One project runs 201 of its 204 EditMode tests this way on GitHub Actions, with no
   Unity and no licence. (Verified: [novacore], [unity-ci], [gamedev-center].)
5. **Every UnityEngine member declared `extern` fails outside Unity.** CoreCLR throws `SecurityException: ECall
   methods must be packaged into a system module` for an internal call that is not in CoreLib, and the same message is
   reported for `Debug.Log` under a standalone NUnit runner. In the 6000.3 source these are extern: `Application.dataPath`,
   all of `Time`, `Debug.Log` (it ends in `DebugLogHandler.Internal_Log`), `new GameObject()`, `new` and `CreateInstance`
   for a ScriptableObject, `transform`, `Quaternion.Euler / Slerp / Lerp / LookRotation / AngleAxis / Inverse /
   FromToRotation`, `Vector3.Slerp / RotateTowards`, `Matrix4x4.TRS / inverse`, and `Mathf.PerlinNoise`. (Verified:
   [clr-ecall], [clr-rc], [blog-ecall], [forum-ecall-2018], [cs-*]. That each row fails is Inferred, except Debug.Log.)
6. **Plain managed UnityEngine code should work.** That covers `Mathf.Sin / Cos / Sqrt / Pow / Lerp / Clamp / Min / Max`
   (thin `System.Math` wrappers), `Vector3` Lerp / Normalize / Distance / Dot / Cross / magnitude, and `Quaternion`
   `*` / Dot / Angle / Normalize. (Verified as source: [cs-mathf], [cs-v3], [cs-quat]. That they run is Inferred; I
   found no report of anyone running them.)
7. **How teams structure it.** Put the logic in a `noEngineReferences` asmdef, so Unity "does not add references to
   UnityEditor or UnityEngine". Use a Humble Object for MonoBehaviours (Unity's blog, 2014), interfaces with fakes
   (NSubstitute), or a fake UnityEngine.dll. Add a CI purity check that engine-free asmdefs stay engine-free. GameCI's
   Editor runs need a Unity licence even in CI ("All Unity actions require activation"), so only the plain .NET path
   needs no licence. (Verified: [asmdef-inspector], [unity-blog-humble], [dunstan-1], [dunstan-2], [unity-ci],
   [gameci-act].)
8. **The biggest limit for SKYGRIND is the runtime.** The Editor embeds Unity's Mono, which "does all math in double
   precision, everywhere" for floats (2026); CoreCLR does not. C# allows higher intermediate precision. `Math.Sin` and
   its kin "may differ" between hosts. .NET Core 3.0 changed `double.ToString()`. So a plain run cannot prove the
   golden vectors and replays bit-exact on Mono. (Verified: [unity-mono], [aras], [cs-spec-types], [ms-sin], [ms-fmt].
   The consequence is Inferred.)
9. **A plain run also does not prove** Unity's compile (the asmdef graph, Roslyn at C# 9, Unity's defines, the .NET
   Standard 2.1 profile), LogAssert's failure on error logs, `[UnityTest]` in the `EditorApplication.update` loop, the
   semantics of Unity's custom NUnit, Editor paths (`Application.dataPath` = `<project>/Assets`), the StreamingAssets
   copy, or anything in `Skygrind.Game`. (Verified: [unity-csc], [unity-profile], [utf-assert], [utf-editmode],
   [unity-datapath]. The list is Inferred.)
10. **Use it as an early warning.** A plain .NET run of `Skygrind.Core.Tests` gives a fast signal on a night the Editor
    hangs. It can stand in for the Editor gate (CLAUDE.md rule 3) only after a same-commit A/B shows identical golden
    and replay output, and that call is GG's. Not checked: whether the project's tests touch any UnityEngine or
    UnityEngine.TestTools API, or reach `PortExport/` through an Editor-only path. (Inferred.)

---

## 1. Running NUnit test assemblies outside the Editor

### 1.1 `dotnet test`

- Verified, https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-csharp-with-nunit : the `dotnet new nunit`
  template adds `nunit` 5.0.0, `NUnit3TestAdapter` 6.3.1, `Microsoft.NET.Test.Sdk` 18.10.1 and `NUnit.Analyzers` 4.15.0.
  "The test project requires other packages to create and run unit tests. The `dotnet new` command in the previous
  step added the Microsoft test SDK, the NUnit test framework, and the NUnit test adapter."
- Verified, https://docs.nunit.org/articles/vs-test-adapter/Adapter-Installation.html : "The adapter and the framework
  are required." "You should ensure you also reference `Microsoft.NET.Test.Sdk` as well; it is required for test
  discoverability." "You don't need to, nor should you, add any nunit.console or any other runner package".
- Verified, https://docs.nunit.org/articles/vs-test-adapter/Index.html : "It runs tests written with NUnit 3, NUnit 4
  and NUnit 5." "The adapter can't run NUnit 2.x tests."
- Verified, https://www.nuget.org/packages/NUnit3TestAdapter : version 6.3.1, last updated 2026-10-09. "From version 6.0
  the NUnit3TestAdapter will support MTP 2.0, and stop supporting .net core 3." "Lowest version will be .net 8.0." Its
  target frameworks are net8.0 and net462. It depends on Microsoft.Testing.Extensions.VSTestBridge,
  Microsoft.Testing.Platform.MSBuild and Microsoft.TestPlatform.AdapterUtilities (plus
  Microsoft.Extensions.DependencyModel on net8.0).
- Verified, https://docs.nunit.org/articles/vs-test-adapter/Supported-Frameworks.html : ".NET Framework 4.6.2 - 4.8:
  3.8 - up to latest"; ".NET 8+: Works with 4.3.2 and upwards"; "Microsoft.NET.Test.Sdk 17.4.0 is not compatible with
  .NET Framework lower than 4.6.2".
- Verified, https://docs.nunit.org/articles/vs-test-adapter/NUnit-And-Microsoft-Test-Platform.html : with
  `<EnableNUnitRunner>true</EnableNUnitRunner>` and `<OutputType>Exe</OutputType>`, "There is no longer a test runner;
  the executable _is_ the test runner." It needs adapter "version 5.0 or greater".

### 1.2 NUnitLite (the test project as its own console exe)

- Verified, https://docs.nunit.org/articles/nunit/running-tests/NUnitLite-Runner.html : "Make sure your test assembly
  references both **nunit.framework** and **nunitlite**." Add `Program.cs` with `return new AutoRun().Execute(args);`
  (`using NUnitLite;`). Setting `OutputType` to `Exe` lets you run the tests "without the `dotnet run`".
- Verified, https://docs.nunit.org/articles/nunit/running-tests/NUnitLite-Options.html : `--test=NAMES`,
  `--testlist=PATH`, `--where=EXPRESSION`, `--result=SPEC` ("NUnit saves the results to `TestResult.xml` in nunit3
  format" by default), `--noresult`, `--explore`, `--labels`, `--workers`.
- Verified, source, https://github.com/nunit/nunit/blob/v3.14.0/src/NUnitFramework/nunitlite/TextRunner.cs : the exit
  codes are `OK = 0`, `INVALID_ARG = -1`, `FILE_NOT_FOUND = -2`, `INVALID_TEST_FIXTURE = -4` and
  `UNEXPECTED_ERROR = -100`. A run returns `Summary.FailureCount + Summary.ErrorCount + Summary.InvalidCount`.
- Verified, https://www.nuget.org/packages/NUnitLite : NUnitLite 5.0.0 (2026-09-27) depends on "NUnit (= 5.0.0)".
  https://www.nuget.org/packages/NUnitLite/3.14.0 : every framework group (net35, net40, net45, netstandard2.0,
  netcoreapp3.1, net5.0, net6.0, net7.0) depends on "NUnit (= 3.14.0)". The netstandard2.0 group also needs
  NETStandard.Library.
- Inferred: NUnitLite is an exact-version pair with official NUnit, so it cannot drive Unity's custom 3.5 build. An
  NUnitLite route means official NUnit from NuGet.

### 1.3 nunit3-console

- Verified, https://docs.nunit.org/articles/nunit/running-tests/Console-Runner.html : "It is able to run all NUnit 3.0
  or higher tests natively and can run NUnit 2.x tests if the v2 driver is installed."
- Verified, https://docs.nunit.org/articles/nunit/getting-started/installation.html : install it from NuGet (the
  "**NUnit** and **NUnit.Console** packages"; nunit3-console sits in `packages\NUnit.ConsoleRunner.3.X.X\tools`), or
  "download the latest binary zip or an MSI installer of the NUnit Console from GitHub".
- Verified, https://www.nuget.org/packages/NUnit.ConsoleRunner : 3.22.0 (2026-01-03), "This package includes the
  nunit3-console runner and test engine for version 3 of the NUnit unit-testing framework."
  https://www.nuget.org/packages/NUnit.ConsoleRunner.Net80 : 3.18.3, "the .NET 8.0 build of the NUnit console runner
  and test engine", installed as a .NET tool (`dotnet tool install --global NUnit.ConsoleRunner.Net80 --version 3.18.3`).
- Inferred: for one test assembly the console adds an engine and agents and brings nothing that NUnitLite lacks. It is
  also one more download.

### 1.4 Offline restore

- Verified, https://learn.microsoft.com/en-us/nuget/consume-packages/managing-the-global-packages-and-cache-folders :
  "When asked to retrieve a package, NuGet first looks in the *global-packages* folder. If the exact version of the
  package is not there, then NuGet checks all non-HTTP package sources. If the package is still not found, NuGet looks
  for the package in the *http-cache* ... If the package is not in the HTTP cache, or the HTTP cache isn't used, NuGet
  then retrieves the package over HTTP." On Windows the folder is `%userprofile%\.nuget\packages`.
  `dotnet nuget locals all --list` prints the folder locations.
- Verified, https://learn.microsoft.com/en-us/nuget/hosting-packages/local-feeds : "Local NuGet package feeds are simply
  hierarchical folder structures on your local network (or even just your own computer) in which you place packages."
  `nuget add new_package.1.0.0.nupkg -source <folder>` builds the layout.
- Verified, https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-restore : `-s|--source` "overrides all of the
  sources specified in the *nuget.config* files". `dotnet build` / `run` / `test` restore implicitly unless you pass
  `--no-restore`. "To opt out of the security auditing, set the `<NuGetAudit>` MSBuild property to `false`."
- Inferred: offline works in only two cases. Either the exact versions are already in the global packages folder from
  an earlier online restore, or someone copies the `.nupkg` files and their dependencies into a folder feed. NUnitLite
  needs fewer packages than `dotnet test` (adapter + test SDK + their Microsoft.Testing.* dependencies, see 1.1).
- Not checked: what is in this machine's global packages folder, and the installed .NET SDK version.

### 1.5 Unity's own `nunit.framework.dll`

- Verified, the package:
  - https://docs.unity3d.com/Packages/com.unity.ext.nunit@2.0/manual/index.html and
    https://packages.unity.com/com.unity.ext.nunit : "A custom version of NUnit used by Unity Test Framework. Based on
    NUnit version 3.5 and works with all platforms, il2cpp and Mono AOT." The registry's latest version is 2.0.5
    (2023-06-29, `"unity": "2019.4"`).
  - The mirror at https://github.com/needle-mirror/com.unity.ext.nunit has `package.json` with `"dependencies": {}`,
    and its only payload is `net40/unity-custom/nunit.framework.dll` (+ `.meta`). The listing page was truncated
    ("View all files"), but no nunitlite or runner showed.
  - The README calls it a "Custom Nunit build to work with Unity", "precompiled for .NET 4.0", and says the source
    "lives at https://github.com/Unity-Technologies/nunit". That URL returned 404 to me, so the source is not public.
- Verified, https://github.com/needle-mirror/com.unity.ext.nunit/blob/master/CHANGELOG.md : 2.0.0 upgraded to .NET 4.0.
  2.0.3 added ContainKey / ContainValue constraints and unique ids for repeated tests. 2.0.4 and 2.0.5 deal with "InstanceID as struct changes".
- Verified, https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/test-framework-introduction :
  "Unity Test Framework is a core Unity package and is distributed as part of a Unity Editor installation." "Unity Test
  Framework currently uses a custom version of NUnit based on version 3.5."
- Verified, the custom build is not interchangeable in either direction:
  - Putting a newer NUnit inside Unity failed with "MissingMethodException: Method not found:
    'NUnit.Framework.Internal.Reflect.set_MethodCallWrapper'" (2018,
    https://discussions.unity.com/t/which-version-of-nunit-ships-with-unity-cant-find-testfixturesource-attribute/165969).
  - Going the other way, NCrunch's developer said in 2017 that Unity's DLL "wasn't an officially released version of
    NUnit3," and that "it had critical code that was fundamentally incompatible with the normal NUnit API"
    (https://forum.ncrunch.net/yaf_postst2278_Build-failed-on-Unity-project.aspx).
  - A 2017 tool README says "After 5.6, Unity switched to a modified 3.5 NUnit framework and cannot work with standard
    NUnit 3 test runner" (https://github.com/FrankNine/UnityNUnitReferenceOverride).
  - Inferred: these sources are 2017-2018; I could not check whether 2.0.5 still behaves this way.
- Verified, what a self-hosted runner would use (upstream tag 3.5, not Unity's build;
  https://github.com/nunit/nunit/tree/3.5/src/NUnitFramework):
  - `public class NUnitTestAssemblyRunner : ITestAssemblyRunner`, with
    `public NUnitTestAssemblyRunner(ITestAssemblyBuilder builder)`,
    `public ITest Load(Assembly assembly, IDictionary<string, object> settings)` and
    `public ITestResult Run(ITestListener listener, ITestFilter filter)`.
  - `public class DefaultTestAssemblyBuilder : ITestAssemblyBuilder`.
  - `public interface ITestListener` with `TestStarted(ITest)`, `TestFinished(ITestResult)` and
    `TestOutput(TestOutput)`.
  - `public readonly static TestFilter Empty`, plus `ITestResult` members `ResultState`, `PassCount`, `FailCount`,
    `SkipCount`, `FullName` and `Message`.
  - NUnitLite 3.5's `TextRunner` does exactly `_runner = new NUnitTestAssemblyRunner(new DefaultTestAssemblyBuilder());`.
- Verified, https://github.com/needle-mirror/com.unity.test-framework/blob/master/UnityEngine.TestRunner/NUnitExtensions/UnityTestAssemblyBuilder.cs
  (test-framework 1.4.6, which depends on com.unity.ext.nunit 2.0.3): `internal class UnityTestAssemblyBuilder :
  DefaultTestAssemblyBuilder, IAsyncTestAssemblyBuilder`. Inferred: so Unity's DLL still ships that builder, and it is
  accessible from another assembly.
- Inferred, an untested sketch of the zero-download route. Compile it as an exe that references Unity's
  `nunit.framework.dll` by HintPath, then pass it the test DLL path:

```csharp
using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework.Api;        // NUnitTestAssemblyRunner, DefaultTestAssemblyBuilder
using NUnit.Framework.Interfaces; // ITestListener, ITest, ITestResult, TestOutput, TestStatus
using NUnit.Framework.Internal;   // TestFilter

sealed class Listener : ITestListener
{
    public void TestStarted(ITest test) { }
    public void TestFinished(ITestResult r)
    {
        if (r.ResultState.Status == TestStatus.Failed) Console.WriteLine("FAILED " + r.FullName + " : " + r.Message);
    }
    public void TestOutput(TestOutput output) { }
}

static class Program
{
    static int Main(string[] args)
    {
        var runner = new NUnitTestAssemblyRunner(new DefaultTestAssemblyBuilder());
        runner.Load(Assembly.LoadFrom(args[0]), new Dictionary<string, object>());
        ITestResult result = runner.Run(new Listener(), TestFilter.Empty);
        Console.WriteLine("pass " + result.PassCount + " fail " + result.FailCount + " skip " + result.SkipCount);
        return result.FailCount;
    }
}
```

- Caveats if this runs on .NET 8. Each one is Inferred unless marked:
  - The DLL is a .NET Framework 4.0 build. Verified, https://learn.microsoft.com/en-us/dotnet/core/porting/third-party-deps :
    the ".NET Framework compatibility mode ... allows .NET Standard and .NET Core projects to reference .NET Framework
    libraries", but this "doesn't work for all projects, such as if the library uses Windows Presentation Foundation
    (WPF) APIs".
  - Thread abort. Verified, upstream 3.5 `ThreadUtility.Kill` calls `thread.Abort()` in non-portable builds
    (https://github.com/nunit/nunit/blob/3.5/src/NUnitFramework/framework/Internal/ThreadUtility.cs). Verified,
    `Thread.Abort` "always throws a PlatformNotSupportedException on .NET Core and .NET 5+"
    (https://learn.microsoft.com/en-us/dotnet/core/compatibility/core-libraries/5.0/thread-abort-obsolete). So
    `[Timeout]` and forced stops would misbehave.
  - Windows Forms. Verified, the upstream 3.5 runner has a `System.Windows.Forms` path for `PauseBeforeRun` (the
    NUnitTestAssemblyRunner page above). It matters only if that setting is passed.
  - A net48 host avoids both problems, but it cannot load a netstandard2.1 test assembly.
- Not checked:
  - whether Unity's DLL references UnityEngine assemblies (the InstanceID lines hint that it knows Unity types;
    Inferred)
  - whether it kept the 3.5 runner API unchanged
  - which com.unity.ext.nunit version this project resolves, and where (PackageCache or the Editor's built-in
    packages)

### 1.6 Which NUnit version for a NuGet route

- Verified, https://docs.nunit.org/articles/nunit/release-notes/breaking-changes.html : in NUnit 4.0 the classic asserts
  "have been moved to a separate library and their namespace and their class name were renamed to"
  `NUnit.Framework.Legacy.ClassicAssert`. "The lowest framework version supported is now version 4.6.2 and .Net 6.0."
- Verified, https://www.nuget.org/packages/NUnit/3.14.0 : 3.14.0 (2023-11-03) ships net35, net40, net45 and
  netstandard2.0. The version list then goes on to 4.0.1 and 5.0.0.
- Inferred: tests written for Unity's 3.5-based NUnit (`Assert.AreEqual` and the like) compile against 3.14.0 without
  changes, and against 4.x / 5.x only after edits. novacore picked 3.14 for that reason (3.4).

## 2. UnityEngine outside the Unity runtime

### 2.1 Why `extern` members fail

- Verified, https://learn.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.methodimploptions :
  `InternalCall` (4096) means "The call is internal, that is, it calls a method that's implemented within the common
  language runtime."
- Verified, https://www.mono-project.com/docs/advanced/embedding/ : an embedding host registers the C side itself with
  `mono_add_internal_call ("Hello::Sample", sample);`. The managed side is
  `[MethodImplAttribute(MethodImplOptions.InternalCall)]` `extern static string Sample ();`.
- Verified, https://docs.unity3d.com/6000.3/Documentation/Manual/scripting-backends-mono.html : "The Unity Editor embeds
  Mono, so your C# code is managed by Mono"; Mono "powers Unity's Editor and Players on compatible platforms"; "The Mono
  scripting back end is Unity's fork of the open source Mono project."
- Verified, https://github.com/dotnet/runtime/blob/main/src/coreclr/vm/ecall.cpp : `ECall::GetFCallImpl` runs
  `if (!pMD->GetModule()->IsSystem())` and then `COMPlusThrow(kSecurityException, BFA_ECALLS_MUST_BE_IN_SYS_MOD);`.
  https://github.com/dotnet/runtime/blob/main/src/coreclr/dlls/mscorrc/mscorrc.rc defines that string as "ECall methods
  must be packaged into a system module."
- Inferred: Unity's `extern` members (written with `[FreeFunction]` / `[NativeMethod]` in the reference source) are
  internal calls. Unity's native engine registers them with its embedded Mono. In any other host nothing registers
  them, so the first call throws.
- Verified, failures people saw:
  - In a 2021 blog, "System.Security.SecurityException : ECall methods must be packaged into a system module" came up
    when a test calling `Debug.Log` ran under Rider's Standalone NUnit Launcher. It did not happen in Unity's Test
    Runner. It also came up for a `NativeHashMap` from Unity.Collections. "This is because we are running MonoBehaviour
    (inheriting MonoBehaviour and calling Debug.Log) outside of Unity."
    https://neilmarkcorre.wordpress.com/tag/system-security-securityexception-ecall-methods-must-be-packaged-into-a-system-module/
  - A 2018 forum thread got the same exception from "Debug.Log() etc." The reply: "you always have to run any automated
    test that calls Unity APIs from inside of Unity." The page does not mark the replier as staff.
    https://discussions.unity.com/threads/unittest-under-fps-sample-failed-with-securityexception-ecall-methods-must-be-packaged-into-a-s.580048/
  - In 2013, a plain .NET server app that referenced UnityEngine.dll got the same message.
    https://discussions.unity.com/t/c-error-ecall-methods-must-be-packaged-into-a-system-module/514335

### 2.2 What is `extern` (expected to fail) in the 6000.3 source

The source is UnityCsReference at tag `6000.3.27f1`. Its README says the code "May be used for reference purposes
only". `master` tracks 7000.0.0a12, so I read the 6000.3 tag (https://github.com/Unity-Technologies/UnityCsReference).
The project runs 6000.3.7f1; I did not check for drift between 6000.3.7f1 and 6000.3.27f1. The table is Verified as
source; "fails outside Unity" is Inferred for every row except Debug.Log (seen, 2.1).

| API | what the source shows | file under `blob/6000.3.27f1/Runtime/Export/` |
|---|---|---|
| `Application.dataPath`, `streamingAssetsPath`, `persistentDataPath`, `isPlaying`, `platform`, `unityVersion` | `extern`, e.g. `[FreeFunction("GetAppDataPath", IsThreadSafe = true)]` | `Application/Application.bindings.cs` |
| `Time.time`, `deltaTime`, `fixedDeltaTime`, `timeScale`, `frameCount`, `realtimeSinceStartup` | `public static extern ...`, class `[StaticAccessor("GetTimeManager()", ...)]` | `Time/Time.bindings.cs` |
| `Debug.Log` / `LogError` / `LogException` | managed chain: `Debug.Log` -> `unityLogger.Log` -> `Logger.Log` -> `logHandler.LogFormat` -> `DebugLogHandler`'s `Internal_Log(...)`, which is `internal static extern` | `Debug/Debug.bindings.cs`, `Logging/Logger.cs`, `Logging/DebugLogHandler.cs` |
| `Debug.DrawLine`, `Break`, `isDebugBuild` | `extern` | `Debug/Debug.bindings.cs` |
| ScriptableObject: `new T()` and `CreateInstance` | the constructor body is `CreateScriptableObject(this);` (`extern static`); `CreateInstance(Type)` returns `CreateScriptableObjectInstanceFromType(type, true)` (`extern internal static`) | `Scripting/ScriptableObject.bindings.cs` |
| `new GameObject(...)`, `transform`, `AddComponent`, `GetComponent<T>` | constructors call `Internal_CreateGameObject` (`static extern`); `public extern Transform transform`; AddComponent and GetComponent forward to externs | `Scripting/GameObject.bindings.cs` |
| `Quaternion.Euler`, `eulerAngles`, `FromToRotation`, `Inverse`, `Slerp`, `Lerp` (+ Unclamped), `AngleAxis`, `LookRotation` | public wrappers over `extern private static Internal_*` (`Internal_FromEulerRad`, `Internal_ToEulerRad`, ...) | `Math/Quaternion.cs`, `Math/Math.bindings.cs` |
| `Vector3.Slerp`, `SlerpUnclamped`, `OrthoNormalize`, `RotateTowards` | wrappers over `extern` `VectorScripting::*` | `Math/Math.bindings.cs` |
| `Matrix4x4.TRS`, `SetTRS`, `inverse`, `transpose`, `rotation`, `lossyScale`, `determinant`, `LookAt` | public wrappers over `extern` `Internal_*` / `Get*` (`MatrixScripting::*`) | `Math/Math.bindings.cs` |
| `Matrix4x4.Ortho`, `Perspective`, `Frustum` | `extern public static` | `Math/Math.bindings.cs` |
| `Mathf.PerlinNoise`, `PerlinNoise1D`, `GammaToLinearSpace`, `LinearToGammaSpace`, `CorrelatedColorTemperatureToRGB`, `FloatToHalf`, `HalfToFloat` | `extern public static` | `Math/Math.bindings.cs` |
| `Bounds.Contains`, `SqrDistance`, `ClosestPoint` | wrappers over `extern` (`Internal_Contains`, `Internal_SqrDistance`, `Internal_ClosestPoint`); a private `extern IntersectRayAABB` is there too, and its public caller is not in this file (Not checked) | `Math/Math.bindings.cs` |

### 2.3 What is plain managed code (expected to work)

- Verified, https://github.com/Unity-Technologies/UnityCsReference/blob/6000.3.27f1/Runtime/Export/Math/Mathf.cs :
  - `public static float Sin(float f) { return (float)Math.Sin(f); }`. Cos, Sqrt, Pow, Round, RoundToInt, Lerp, Clamp,
    Min, Max, Approximately and SmoothDamp are managed too, and the file has no extern members.
  - ClosestPowerOfTwo, NextPowerOfTwo and IsPowerOfTwo are managed bit operations.
  - `Mathf.Epsilon` comes from `MathfInternal` static fields (`FloatMinDenormal = Single.Epsilon`,
    `IsFlushToZeroEnabled = (FloatMinDenormal == 0)`), which are managed.
- Verified, `.../Math/Vector3.cs` (same tag) : Lerp, Normalize, Distance, Dot, Cross and `magnitude`
  (`get => (float)Math.Sqrt(x * x + y * y + z * z);`) are managed. The static fields are inline `new Vector3(...)`, and
  there is no static constructor.
- Verified, `.../Math/Quaternion.cs` (same tag) : `operator *` (with a Quaternion and with a Vector3), Dot, Angle and
  Normalize are managed arithmetic.
- Inferred: these run in a plain .NET host because their static initialisers are managed and they never reach an
  extern. A managed member that calls an extern (Quaternion.Euler) still fails. Verified,
  https://gamedev.center/run-unity-tests-faster-dotnet/ : `Vector3`, `Mathf` and `Debug.Log` "can be compiled" against
  the Editor's `UnityEngine.CoreModule.dll`, but native calls such as `Physics.Raycast` or `Time.deltaTime` "might not
  work as expected". The setup is "strictly for logic testing".
- Inferred, untested: `Debug.Log` could probably be made safe in a plain host. Set `Debug.unityLogger.logHandler` to a
  managed `ILogHandler` before the first log, because the chain reaches `Internal_Log` only through `DebugLogHandler`.
  Verified, https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Logger-logHandler.html : "Set
  Logger.ILogHandler." (`public ILogHandler logHandler;`).

## 3. How teams structure tests so pure logic runs in plain .NET

### 3.1 `noEngineReferences`

- Verified, https://docs.unity3d.com/6000.3/Documentation/Manual/class-AssemblyDefinitionImporter.html : "When enabled,
  Unity does not add references to `UnityEditor` or `UnityEngine` assemblies when it compiles the assembly."
  https://docs.unity3d.com/6000.3/Documentation/Manual/assembly-definition-file-format.html : `noEngineReferences`
  is "False by default."
- Verified, https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/test-framework-introduction/getting-started/edit-mode-vs-play-mode-tests.md :
  Edit mode test assemblies "must have an assembly definition that references `nunit.framework.dll` and have the Editor
  as their only target platform" (`"includePlatforms": ["Editor"]`).

### 3.2 Test doubles

- Verified, https://unity.com/blog/technology/unit-testing-part-2-unit-testing-monobehaviours (Tomek Paszek, 2014-06-03) :
  the Humble Object pattern. The component "becomes a very thin layer of code", and its logic moves to a separate
  class. "you can't instantiate a MonoBehaviour with the new operator". The logic class "will have a reference to an
  interface". The example uses NSubstitute.
- Verified, https://www.jacksondunstan.com/articles/3694 (2016-12-12) : "the Unity engine isn't available to us when
  running the unit tests", so wrap the engine type in interfaces (`IWww`, `IWwwFactory`) and substitute them.
  https://jacksondunstan.com/articles/3706 (2016-12-19) : instead, build a fake `UnityEngine.dll` whose types record
  calls, kept out of the Unity project. The article admits it "loses in terms of project complexity".

### 3.3 Conditional compilation

- Verified, https://docs.unity3d.com/6000.3/Documentation/Manual/scripting-symbol-reference.html : `UNITY_EDITOR`
  ("Scripting symbol to call Unity Editor scripts from your game code"), and Unity exposes "one global `#define` in the
  format `UNITY_X_Y_OR_NEWER`" (for example `UNITY_6000_0_OR_NEWER`).
- Verified, https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-options/language : "The C#
  compiler itself defines no symbols or macros that you can use in your source code; all symbol definitions must be
  user-defined." `<DefineConstants>` sets them, and `<LangVersion>9.0</LangVersion>` "accepts only syntax that is
  included in C# 9 or lower".
- Inferred: a hand-written csproj has no `UNITY_*` symbol unless it lists one. So `#if UNITY_5_3_OR_NEWER` or
  `#if UNITY_EDITOR` blocks keep Unity-only lines out of the plain build. The same fact is also a risk: a file can
  compile to different code in the two builds without any warning.

### 3.4 Open-source recipes that need no Unity and no licence

- Verified, https://github.com/00xJS/novacore/pull/15 ("CI: run the simulation's tests on GitHub Actions without
  Unity", merged 2026-09-29):
  - `ci/Data` and `ci/Sim` compile the same sources as the two `noEngineReferences` Unity assemblies, as .NET Standard
    2.1 with C# 9, "matching Unity 6's API level" and language.
  - The test project runs on .NET 8 with NUnit 3.14, "which keeps the classic asserts that Unity's NUnit uses".
  - The workflow is checkout, setup-dotnet 8, then `dotnet test ci/Tests`. It runs 201 of 204 EditMode tests; 3 need a
    Unity-side bridge and stay in the Editor.
- Verified, https://github.com/hamibahadori/unity-ci : tests for engine-free assemblies "run under `dotnet test` in
  seconds, on every merge request, with no editor involved". `check_core_purity.py` finds every asmdef with
  `"noEngineReferences": true` (and its `.Tests` sibling) and fails if one references UnityEngine or UnityEditor.
  "Nothing here needs a Unity install".
- Verified, https://gamedev.center/run-unity-tests-faster-dotnet/ (Alex Merzlikin, 2026-03-15) :
  - a standalone `HeadlessTests.csproj` targeting `net9.0` that links `..\Assets\...\Runtime\**\*.cs` and
    `..\Assets\...\Tests\**\*.cs` (linked, not copied)
  - NUnit 3.14.0, NUnit3TestAdapter 4.5.0 and Microsoft.NET.Test.Sdk 17.8.0, plus an optional HintPath to the Editor's
    `UnityEngine.CoreModule.dll`
  - 20 tests took about 18 s in Unity's runner and about 2 s this way
- Verified, https://jetbrains.com/help/rider/Running_and_Debugging_Unity_Tests.html : Rider lists "Standalone NUnit
  Launcher: NUnit test runner." next to "Unity Editor - Edit Mode: unit test runner embedded in Unity for running
  edit-mode tests."
- Verified, the licence contrast:
  - https://game.ci/docs/github/activation : "All Unity actions require activation." Personal licences need
    `UNITY_LICENSE`, `UNITY_EMAIL` and `UNITY_PASSWORD`; Pro needs `UNITY_SERIAL`, `UNITY_EMAIL` and `UNITY_PASSWORD`.
    https://game.ci/docs/github/test-runner shows the same secrets in its example workflow.
  - Unity's `-runTests` and the related flags are "additional arguments to the regular Unity Editor command-line
    arguments" (https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/test-framework-introduction/reference-command-line).
- Not found: any runner that executes tests calling Unity APIs without the Unity runtime, other than replacing
  UnityEngine.dll with fakes (3.2).

## 4. Limits: what a plain .NET run cannot prove that an Editor EditMode run does

### 4.1 A different runtime gives different bits

- Verified, https://aras-p.info/blog/2026/06/11/Unity-vs-floating-point/ (Aras Pranckevičius, 2026-06-11) : "The Mono
  C# runtime used in Unity does all math in double precision, everywhere". "Unity never enabled that functionality and
  kept everything at double precision so far." He measured on 6000.0.76; timings were "rough[ly] the same" on 6000.3.
- Verified, https://www.mono-project.com/docs/about-mono/releases/5.16.0 (2018-10-08) : upstream Mono switched: "Performing 32-bit
  floating operations using 32-bit math is now the default settings for the runtime."
- Verified, https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/types :
  "Floating-point operations may be performed with higher precision than the result type of the operation. To force a
  value of a floating-point type to the exact precision of its type, an explicit cast ... can be used."
- Verified, https://learn.microsoft.com/en-us/dotnet/api/system.math.sin : "This method calls into the underlying C
  runtime, and the exact result or valid input range may differ between different operating systems or architectures."
- Verified, https://devblogs.microsoft.com/dotnet/floating-point-parsing-and-formatting-improvements-in-net-core-3-0/
  (Tanner Gooding, 2019-03-05) : "ToString(), ToString("G"), and ToString("R") will now return the shortest
  roundtrippable string". Before, it tried "G15" and then "G17", so `Math.PI` printed `3.14159265358979` before and
  `3.1415926535897931` after.
- Inferred, what this means for SKYGRIND:
  - (a) A `float` expression with more than one operation can round differently: Unity's Mono keeps double
    intermediates, while CoreCLR rounds each step to float. `Vector3.magnitude`'s `x * x + y * y + z * z` is one such
    expression.
  - (b) `double` + - * / and square root are IEEE operations in both runtimes, so a sim that is double from end to end
    (as a JS port would be) mostly agrees.
  - (c) `Math.Sin / Cos / Atan2 / Pow / Exp / Log` can differ between hosts. `JsMath`'s fdlibm ports already avoid
    this for sin, cos and hypot.
  - (d) Golden text produced by the BCL's `double.ToString()`, or parsed with its parser, can differ. I did not check
    how Unity's Mono formats doubles.
  - So "0 mismatches, bit-exact" in a plain run is evidence about CoreCLR, not about the Editor.

### 4.2 Compiler and API profile

- Verified, https://docs.unity3d.com/6000.3/Documentation/Manual/csharp-compiler.html : Roslyn, "C# language version:
  C# 9.0". The page lists unsupported features: covariant return types, module initializers, init-only setters, and
  more. Full records need `IsExternalInit`, which is "only available in .NET 5 and later (which Unity doesn't
  support)".
- Verified, https://docs.unity3d.com/6000.3/Documentation/Manual/dotnet-profile-support.html : ".NET Standard 2.1 ...
  This is the default API compatibility Level". The alternative is ".NET Framework 4.8, as published by Microsoft, plus
  additional APIs from .NET Standard 2.1".
- Inferred: a net8.0 build at the SDK's default language version accepts code that Unity rejects (C# 10+ syntax,
  .NET 8-only APIs) unless the csproj pins `<LangVersion>9.0</LangVersion>` and builds the code under test as
  netstandard2.1, as novacore does. Even then it does not apply Unity's asmdef rules (references, define constraints,
  version defines), so a green run outside does not mean the code compiles in Unity.

### 4.3 The Unity test runner's own behaviour

- Verified, https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/test-framework-introduction/writing-tests/asserting-and-comparing.md :
  "A test fails if Unity logs a message other than a regular log or warning message." "Use `LogAssert.Expect` before
  running the code under test, as the check for expected logs runs at the end of each frame." "A test also reports a
  failure if an expected message does not appear".
- Verified, https://github.com/needle-mirror/com.unity.test-framework/blob/master/UnityEngine.TestRunner/NUnitExtensions/Runner/UnityLogCheckDelegatingCommand.cs :
  Unity wraps every test in a `LogScope` check. It runs after a plain test and at each step of an enumerable test, and
  it honours `TestMustExpectAllLogsAttribute`.
- Verified, the edit-mode page above and https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/test-framework-introduction :
  Edit mode tests "only run in the Unity Editor". "Edit mode tests run in the EditorApplication.update callback loop."
  "A Unity test runs as a coroutine in Play mode and in the EditorApplication.update callback loop in Edit mode."
- Inferred:
  - Outside Unity nothing raises Unity's log events, so an error log never fails a test, and `LogAssert.Expect` can
    never be satisfied.
  - `[UnityTest]`, `[UnitySetUp]`, Unity's `Is` overlay and its comparers live in UnityEngine.TestRunner and need the
    Editor.
  - Unity's custom 3.5 and NuGet's 3.14 may differ in edge semantics (Unity added constraints in 2.0.3, see 1.5).

### 4.4 Editor state and paths

- Verified, https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Application-dataPath.html : in the Unity
  Editor, `dataPath` is "<path to project folder>/Assets".
- Verified (a forum claim, not an official page), https://discussions.unity.com/t/how-do-i-get-the-full-path-to-an-asset/28053
  (2011) : "The Current Work Directory is set to the project directory".
- Inferred: tests that read `PortExport/` by a relative path pass in the Editor because of that working directory. A
  plain runner must start in the project root or be given the path. The Player's copy
  (`Assets/StreamingAssets/PortExport/`) and the Sync menu are never exercised. Not checked: how this project's tests
  find PortExport.

### 4.5 Later Unity versions

- Verified, https://docs.unity.com/en-us/engine/6000.7/manual/programming-environment/scripting-backends/intro : 6000.7
  lists "CoreCLR (Experimental)". It "only supports the .NET Standard API Compatibility level", and "it's not yet a
  production-ready alternative to Mono or IL2CPP".
- Verified, [aras] : "Unity is planning to switch to “actual .NET” (CoreCLR)".
- Inferred: a CoreCLR Editor would shrink the gap in 4.1, but none is available on 6000.3.

## 5. What this suggests for SKYGRIND (all Inferred; for the forge to decide)

1. **The cheapest working shape.** Make an untracked csproj outside the repo tree that links
   `Assets/Scripts/Core/**/*.cs` and `Assets/Tests/EditMode/**/*.cs`. Pin `LangVersion` 9.0, reference NUnit 3.14.0 and
   NUnitLite 3.14.0, and build it as a net8.0 Exe. Run it as `dotnet run -- --where "..." --result=<file>`; the exit
   code is the failure count. Keeping it outside the repo matters: the forge61 dead-code note (a local note, not
   re-checked here) measured that building the generated csproj in place writes into `Temp\obj`.
2. **Offline.** First run `dotnet nuget locals global-packages --list` and see whether NUnit and NUnitLite 3.14.0 are
   already there. If they are not, the self-hosted `Main` over Unity's DLL (1.5) is the only route with no download.
   Prove it RED once (a planted failing test) and GREEN once before trusting it (L-014).
3. **Calibrate before trusting.** Run the golden vectors and the 16 replays in both runners on the same commit and diff
   the outputs. A mismatch there is the runtime gap of 4.1, not a bug in the sim.
4. **Keep the authority where it is.** The Editor run stays the authority for CLAUDE.md rule 3 unless GG changes the
   rule. So does the project's batch-mode Unity on a project copy (the local memory note "Batch tests without the
   Editor"), which needs the licence but not the live Editor.

## Not checked

- Whether `Skygrind.Core.Tests` references UnityEngine or UnityEngine.TestRunner, or uses LogAssert, `[UnityTest]` or
  `Application.*`.
- Whether `Skygrind.Core` uses `float` anywhere in the sim or only `double`, and which `System.Math` functions it calls.
- Whether Unity's custom `nunit.framework.dll` references UnityEngine, and whether it kept NUnit 3.5's runner API
  unchanged.
- Which com.unity.ext.nunit version the project resolves.
- The local NuGet cache and the installed .NET SDK version.
- How Unity's Mono formats and parses doubles.
- Nothing was run for this note, so no claim here is measured.
- The Unity issue-tracker page about Mono float precision would not render through the fetch tool, so this note does
  not cite it.

## Sources

| tag | URL |
|---|---|
| ms-nunit | https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-csharp-with-nunit |
| nunit-adapter-install | https://docs.nunit.org/articles/vs-test-adapter/Adapter-Installation.html |
| nunit-adapter-index | https://docs.nunit.org/articles/vs-test-adapter/Index.html |
| nunit-adapter-fw | https://docs.nunit.org/articles/vs-test-adapter/Supported-Frameworks.html |
| nunit-mtp | https://docs.nunit.org/articles/vs-test-adapter/NUnit-And-Microsoft-Test-Platform.html |
| nuget-adapter | https://www.nuget.org/packages/NUnit3TestAdapter |
| nunitlite | https://docs.nunit.org/articles/nunit/running-tests/NUnitLite-Runner.html |
| nunitlite-opts | https://docs.nunit.org/articles/nunit/running-tests/NUnitLite-Options.html |
| nunitlite-src | https://github.com/nunit/nunit/blob/v3.14.0/src/NUnitFramework/nunitlite/TextRunner.cs |
| nuget-nunitlite | https://www.nuget.org/packages/NUnitLite , https://www.nuget.org/packages/NUnitLite/3.14.0 |
| nunit-console | https://docs.nunit.org/articles/nunit/running-tests/Console-Runner.html |
| nunit-install | https://docs.nunit.org/articles/nunit/getting-started/installation.html |
| nuget-console | https://www.nuget.org/packages/NUnit.ConsoleRunner , https://www.nuget.org/packages/NUnit.ConsoleRunner.Net80 |
| nuget-folders | https://learn.microsoft.com/en-us/nuget/consume-packages/managing-the-global-packages-and-cache-folders |
| nuget-local | https://learn.microsoft.com/en-us/nuget/hosting-packages/local-feeds |
| dotnet-restore | https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-restore |
| ext-nunit-docs | https://docs.unity3d.com/Packages/com.unity.ext.nunit@2.0/manual/index.html , https://packages.unity.com/com.unity.ext.nunit |
| ext-nunit-mirror | https://github.com/needle-mirror/com.unity.ext.nunit (README.md, package.json, CHANGELOG.md, net40/unity-custom) |
| utf-6000.3 | https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/test-framework-introduction |
| utf-mirror | https://github.com/needle-mirror/com.unity.test-framework (package.json, UnityTestAssemblyBuilder.cs, UnityLogCheckDelegatingCommand.cs) |
| forum-nunit-ver | https://discussions.unity.com/t/which-version-of-nunit-ships-with-unity-cant-find-testfixturesource-attribute/165969 |
| ncrunch | https://forum.ncrunch.net/yaf_postst2278_Build-failed-on-Unity-project.aspx |
| franknine | https://github.com/FrankNine/UnityNUnitReferenceOverride |
| nunit35-src | https://github.com/nunit/nunit/tree/3.5/src/NUnitFramework (Api/NUnitTestAssemblyRunner.cs, Api/DefaultTestAssemblyBuilder.cs, Interfaces/ITestListener.cs, Interfaces/ITestResult.cs, Interfaces/ResultState.cs, Internal/TestFilter.cs, Internal/ThreadUtility.cs, nunitlite/TextRunner.cs) |
| ms-compat | https://learn.microsoft.com/en-us/dotnet/core/porting/third-party-deps |
| ms-abort | https://learn.microsoft.com/en-us/dotnet/core/compatibility/core-libraries/5.0/thread-abort-obsolete |
| nunit4-breaking | https://docs.nunit.org/articles/nunit/release-notes/breaking-changes.html |
| nuget-nunit314 | https://www.nuget.org/packages/NUnit/3.14.0 |
| ms-internalcall | https://learn.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.methodimploptions |
| mono-embed | https://www.mono-project.com/docs/advanced/embedding/ |
| unity-mono | https://docs.unity3d.com/6000.3/Documentation/Manual/scripting-backends-mono.html |
| clr-ecall | https://github.com/dotnet/runtime/blob/main/src/coreclr/vm/ecall.cpp |
| clr-rc | https://github.com/dotnet/runtime/blob/main/src/coreclr/dlls/mscorrc/mscorrc.rc |
| blog-ecall | https://neilmarkcorre.wordpress.com/tag/system-security-securityexception-ecall-methods-must-be-packaged-into-a-system-module/ |
| forum-ecall-2018 | https://discussions.unity.com/threads/unittest-under-fps-sample-failed-with-securityexception-ecall-methods-must-be-packaged-into-a-s.580048/ |
| forum-ecall-2013 | https://discussions.unity.com/t/c-error-ecall-methods-must-be-packaged-into-a-system-module/514335 |
| cs-* | https://github.com/Unity-Technologies/UnityCsReference (README) and, at tag 6000.3.27f1: Runtime/Export/Application/Application.bindings.cs, Time/Time.bindings.cs, Debug/Debug.bindings.cs, Logging/Logger.cs, Logging/DebugLogHandler.cs, Scripting/ScriptableObject.bindings.cs, Scripting/GameObject.bindings.cs, Math/Math.bindings.cs, Math/Mathf.cs, Math/Vector3.cs, Math/Quaternion.cs |
| cs-mathf / cs-v3 / cs-quat | the Math/Mathf.cs, Math/Vector3.cs and Math/Quaternion.cs files in the row above |
| unity-loghandler | https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Logger-logHandler.html |
| gamedev-center | https://gamedev.center/run-unity-tests-faster-dotnet/ |
| asmdef-inspector | https://docs.unity3d.com/6000.3/Documentation/Manual/class-AssemblyDefinitionImporter.html , https://docs.unity3d.com/6000.3/Documentation/Manual/assembly-definition-file-format.html |
| utf-editmode | https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/test-framework-introduction/getting-started/edit-mode-vs-play-mode-tests.md |
| unity-blog-humble | https://unity.com/blog/technology/unit-testing-part-2-unit-testing-monobehaviours |
| dunstan-1 / dunstan-2 | https://www.jacksondunstan.com/articles/3694 , https://jacksondunstan.com/articles/3706 |
| unity-symbols | https://docs.unity3d.com/6000.3/Documentation/Manual/scripting-symbol-reference.html |
| ms-csc-lang | https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-options/language |
| novacore | https://github.com/00xJS/novacore/pull/15 |
| unity-ci | https://github.com/hamibahadori/unity-ci |
| rider | https://jetbrains.com/help/rider/Running_and_Debugging_Unity_Tests.html |
| gameci-act | https://game.ci/docs/github/activation , https://game.ci/docs/github/test-runner |
| utf-cli | https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/test-framework-introduction/reference-command-line |
| aras | https://aras-p.info/blog/2026/06/11/Unity-vs-floating-point/ |
| mono-516 | https://www.mono-project.com/docs/about-mono/releases/5.16.0 |
| cs-spec-types | https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/types |
| ms-sin | https://learn.microsoft.com/en-us/dotnet/api/system.math.sin |
| ms-fmt | https://devblogs.microsoft.com/dotnet/floating-point-parsing-and-formatting-improvements-in-net-core-3-0/ |
| unity-csc | https://docs.unity3d.com/6000.3/Documentation/Manual/csharp-compiler.html |
| unity-profile | https://docs.unity3d.com/6000.3/Documentation/Manual/dotnet-profile-support.html |
| utf-assert | https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/test-framework-introduction/writing-tests/asserting-and-comparing.md |
| unity-datapath | https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Application-dataPath.html |
| forum-cwd | https://discussions.unity.com/t/how-do-i-get-the-full-path-to-an-asset/28053 |
| unity-6000.7-backends | https://docs.unity.com/en-us/engine/6000.7/manual/programming-environment/scripting-backends/intro |
