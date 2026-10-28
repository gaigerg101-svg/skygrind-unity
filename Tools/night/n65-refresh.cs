// NIGHT65 (the lead's eval; memory "pick a new file, stale Editor", NIGHT41): after a pick that adds .cs files, import them and ask for a
// script compile, so the next compile gate does not report up_to_date on a stale assembly.   bash Tools/unity-loop.sh eval-file Tools/night/n65-refresh.cs
string[] added = { "Assets/Scripts/Game/FeelReel.N65.Tour.cs", "Assets/Scripts/Game/Tour65.cs", "Assets/Scripts/Game/UnityTuning.N65T.cs", "Assets/Tests/EditMode/Game/N65TourTests.cs" };
foreach (string p in added) UnityEditor.AssetDatabase.ImportAsset(p, UnityEditor.ImportAssetOptions.ForceUpdate);
UnityEditor.AssetDatabase.Refresh();
UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
return "n65-refresh: imported " + added.Length + " files, refreshed, compile requested";
