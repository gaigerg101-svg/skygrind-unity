// The enemy bench's on-screen readout, a separate component so a suite run can switch it off (no OnGUI cost while
// measuring).
using UnityEngine;

namespace Skygrind.Bench
{
    [RequireComponent(typeof(EnemyBench))]
    public sealed class EnemyBenchHud : MonoBehaviour
    {
        EnemyBench bench;
        GUIStyle style;
        string text = "";
        float next;

        void Awake() => bench = GetComponent<EnemyBench>();

        void Update()
        {
            if (bench.SuiteRunning)
            {
                enabled = false;
                return;
            }
            if (Time.unscaledTime < next) return;
            next = Time.unscaledTime + 0.5f;
            text = $"{bench.variant}  N {bench.count}   {bench.lastFps:0} fps   sim {bench.lastStepMs:0.000} ms/step\nF1-F5 variant (closer-web, closer-unity, plain, burst-plain, burst-full)   1-4 N (24, 40, 100, 200)";
        }

        void OnGUI()
        {
            if (style == null) style = new GUIStyle(GUI.skin.label) { fontSize = 16 };
            GUI.Label(new Rect(10, 10, Screen.width, 60), text, style);
        }
    }
}
