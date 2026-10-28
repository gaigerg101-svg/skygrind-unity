// NIGHT48 builder F: the a948 lineup reel's rig (lineup48, a reel-only switch: no game state). While `active`, after every other script's
// LateUpdate this frame (execution order 32000): every root canvas is disabled (the HUD, world-space bars, the screens: none in the frame), the
// viewmodel's overlay camera is disabled (no gun in the frame), and the main camera is put at the reel's pose and field of view (the game's
// CameraFeel set it in Update; the reel's coroutine sets it too, before the LateUpdates, so camera-facing sprites face the reel's camera).
// Destroyed (or `active` false): every canvas and the viewmodel camera it turned off are turned back on, and the camera goes back to the game.
// The plate (a neutral unlit floor + backdrop) is the reel's own objects (no collider), destroyed with the rig.
using System.Collections.Generic;
using UnityEngine;

namespace Skygrind.Game
{
    [DefaultExecutionOrder(32000)]
    public sealed class Lineup48Rig : MonoBehaviour
    {
        public bool active;
        public Camera cam;
        public Camera vmCam;
        public Vector3 camPos;
        public Quaternion camRot = Quaternion.identity;
        public float fov = 60f;
        /// <summary>For the reel's line: root canvases and the viewmodel camera enabled at the last LateUpdate's end (want 0 and false).</summary>
        public int canvasesOn;
        public bool vmOn;
        readonly List<Canvas> off = new List<Canvas>();
        readonly List<GameObject> plate = new List<GameObject>();
        bool vmWasOn;
        bool vmTouched;

        /// <summary>A flat unlit box (no collider) in the plate's grey, from the game's neon material (unlit, as the decals').</summary>
        public void AddPlate(Material neon, Vector3 centre, Quaternion rotation, Vector3 size, float grey)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Collider c = go.GetComponent<Collider>();
            if (c != null) Destroy(c);
            go.name = "lineup48 plate";
            go.transform.SetPositionAndRotation(centre, rotation);
            go.transform.localScale = size;
            var r = go.GetComponent<Renderer>();
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            if (neon != null)
            {
                var m = new Material(neon) { name = "lineup48 plate" };
                var col = new Color(grey, grey, grey, 1f);
                m.color = col;
                if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", col);
                r.sharedMaterial = m;
            }
            plate.Add(go);
        }

        public void ClearPlate()
        {
            foreach (GameObject go in plate)
                if (go != null)
                {
                    Renderer r = go.GetComponent<Renderer>();
                    if (r != null && r.sharedMaterial != null) Destroy(r.sharedMaterial);
                    Destroy(go);
                }
            plate.Clear();
        }

        /// <summary>The reel's pose now (also called from the reel's coroutine, before the LateUpdates).</summary>
        public void Apply()
        {
            if (!active) return;
            if (vmCam != null && vmCam.enabled)
            {
                if (!vmTouched) { vmTouched = true; vmWasOn = true; }
                vmCam.enabled = false;
            }
            foreach (Canvas c in FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                if (c == null || !c.isRootCanvas || !c.enabled) continue;
                c.enabled = false;
                if (!off.Contains(c)) off.Add(c);
            }
            if (cam != null)
            {
                cam.transform.SetPositionAndRotation(camPos, camRot);
                cam.fieldOfView = fov;
            }
        }

        void LateUpdate()
        {
            Apply();
            int n = 0;
            foreach (Canvas c in FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                if (c != null && c.isRootCanvas && c.enabled) n++;
            canvasesOn = n;
            vmOn = vmCam != null && vmCam.enabled;
        }

        public void Release()
        {
            active = false;
            foreach (Canvas c in off) if (c != null) c.enabled = true;
            off.Clear();
            if (vmTouched && vmWasOn && vmCam != null) vmCam.enabled = true;
            vmTouched = false;
            ClearPlate();
        }

        void OnDestroy() => Release();
    }
}
