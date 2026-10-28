#!/usr/bin/env bash
# NIGHT56 builder G: G's EditMode classes on the harness's mini NUnit (n56 selftest), built with -p:Extra (the files Harness.N55.Look.cs needs
# under any Extra, plus G's tests). The next default `dotnet run` rebuilds without them (the fingerprint gate's build).
#   bash Tools/night/n56-g-selftest.sh [class suffix]
cd "$(dirname "$0")/../.." || exit 1
x="../../Assets/Scripts/Game/Move55.cs;../../Assets/Scripts/Game/Move54.cs;../../Assets/Scripts/Game/UnityTuning.N55C.cs"
x="$x;../../Assets/Tests/EditMode/Game/N56RankTests.cs;../../Assets/Tests/EditMode/Game/N56TokensTests.cs"
[ -f Assets/Tests/EditMode/Wyrm55Tests.cs ] && x="$x;../../Assets/Tests/EditMode/Wyrm55Tests.cs"
dotnet run --project Tools/SimHarness -c Release "-p:Extra=\"$x\"" -- n56 selftest "$@" 2>&1 | grep -E "error|PASS|FAIL|selftest|pass"
