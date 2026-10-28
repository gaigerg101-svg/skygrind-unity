// NIGHT9 U3-04: every Player build ships the current PortExport (SKYGRIND/Sync Port Data runs first), so a built
// Player's -skygrindGolden checks the same vectors and trajectories as the Editor's tests. A Web build gets only what
// the game reads (the oracle is for the Windows Player's check, and would add about 16 MB of files to the upload).
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public sealed class PortDataBuildSync : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        bool oracle = report.summary.platform != BuildTarget.WebGL;
        Debug.Log("[skygrind] build: " + SurvivalSetup.SyncPortData(oracle));
    }
}
