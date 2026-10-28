// NIGHT14 U3-27.10 (agent V2): the SHARE CARD's file side. RESULTS' SAVE CARD (UiRoute.cs) lays the card out on an
// off-screen world-space canvas (the route map, the run's numbers, the build stamp) and asks Render for its pixels: the
// card's own camera draws it into a RenderTexture through URP's render request (UniversalRenderPipeline.SingleCameraRequest;
// Camera.Render where the pipeline refuses it), the pixels are read back, made opaque and encoded as a PNG. Write puts it in
// <My Pictures>/SKYGRIND/ (Environment.SpecialFolder.MyPictures; the profile's folder when the OS has none), named
// SKYGRIND-<seed code>-<local yyyyMMdd-HHmmss>.png (a "-2", "-3" ... suffix if that name exists). Automation points
// FolderOverride elsewhere; LastPath / LastError say what the last SAVE CARD did. Only on the button, never per frame.
using System;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Skygrind.Game
{
    public static class RouteCard
    {
        /// <summary>Automation: write cards here instead of My Pictures (null: the player's folder).</summary>
        public static string FolderOverride;

        /// <summary>The last card written (null: none yet), and the last failure's message (null: the last save worked).</summary>
        public static string LastPath { get; private set; }
        public static string LastError { get; private set; }
        /// <summary>Cards written since boot.</summary>
        public static int Saved { get; private set; }

        /// <summary>The folder cards go to.</summary>
        public static string Folder
        {
            get
            {
                if (!string.IsNullOrEmpty(FolderOverride)) return FolderOverride;
                string pictures = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                if (string.IsNullOrEmpty(pictures)) pictures = Application.persistentDataPath;
                return Path.Combine(pictures, UnityTuning.RouteCardFolder);
            }
        }

        /// <summary>SKYGRIND-&lt;seed code&gt;-&lt;yyyyMMdd-HHmmss&gt;.png (the local time: the player's own clock).</summary>
        public static string FileName(string seedCode, DateTime local) =>
            UnityTuning.RouteCardPrefix + (string.IsNullOrEmpty(seedCode) ? "RUN" : seedCode) + "-" + local.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture) + ".png";

        /// <summary>Write the PNG; returns its path (LastPath). Pictures refused (Controlled Folder Access, a OneDrive lock): the
        /// profile's folder instead (NIGHT14 review B). Throws only when that fails too (the caller says so).</summary>
        public static string Write(byte[] png, string seedCode)
        {
            try
            {
                return WriteInto(Folder, png, seedCode);
            }
            catch (Exception e) when (string.IsNullOrEmpty(FolderOverride) && (e is IOException || e is UnauthorizedAccessException))
            {
                return WriteInto(Path.Combine(Application.persistentDataPath, UnityTuning.RouteCardFolder), png, seedCode);
            }
        }

        static string WriteInto(string folder, byte[] png, string seedCode)
        {
            Directory.CreateDirectory(folder);
            string name = FileName(seedCode, DateTime.Now);
            string path = Path.Combine(folder, name);
            for (int n = 2; File.Exists(path); n++) path = Path.Combine(folder, Path.GetFileNameWithoutExtension(name) + "-" + n.ToString(CultureInfo.InvariantCulture) + ".png");
            File.WriteAllBytes(path, png);
            LastPath = path;
            LastError = null;
            Saved++;
            return path;
        }

        public static void Failed(string message) => LastError = message;

        /// <summary>The path as the hint shows it: under My Pictures as "Pictures\SKYGRIND\name.png", else whole.</summary>
        public static string ShortPath(string path)
        {
            string pictures = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            if (!string.IsNullOrEmpty(pictures) && path.StartsWith(pictures, StringComparison.OrdinalIgnoreCase))
                return "Pictures" + path.Substring(pictures.Length);
            return path;
        }

        /// <summary>
        /// Draw `root` (a world-space canvas in front of `cam`) into `rt` and return it as PNG bytes, opaque. The root is
        /// active only for the render (no other camera ever sees it).
        /// </summary>
        public static byte[] Render(GameObject root, Camera cam, RenderTexture rt)
        {
            root.SetActive(true);
            Texture2D tex = null;
            RenderTexture before = RenderTexture.active;
            try
            {
                Canvas.ForceUpdateCanvases();
                foreach (TMPro.TMP_Text t in root.GetComponentsInChildren<TMPro.TMP_Text>()) t.ForceMeshUpdate();
                Canvas.ForceUpdateCanvases();
                var request = new UniversalRenderPipeline.SingleCameraRequest { destination = rt };
                cam.enabled = true; // for the request only (this frame's render loop never sees it enabled)
                if (RenderPipeline.SupportsRenderRequest(cam, request)) RenderPipeline.SubmitRenderRequest(cam, request);
                else
                {
                    cam.targetTexture = rt;
                    cam.Render();
                    cam.targetTexture = null;
                }
                cam.enabled = false;
                RenderTexture.active = rt;
                tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
                tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                tex.Apply(false);
                Color32[] px = tex.GetPixels32();
                for (int i = 0; i < px.Length; i++) px[i].a = 255; // the UI's blend leaves partial alpha: a card is opaque
                tex.SetPixels32(px);
                return tex.EncodeToPNG();
            }
            finally
            {
                cam.enabled = false;
                RenderTexture.active = before;
                root.SetActive(false);
                if (tex != null) UnityEngine.Object.Destroy(tex);
            }
        }

        /// <summary>The card's camera: orthographic over a `width` x `height` m canvas `distance` m ahead, no post, no HDR.</summary>
        public static Camera MakeCamera(Transform parent, float width, float height, float distance, Color background, int layer)
        {
            var go = new GameObject("Route card camera", typeof(Camera));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0, 0, -distance);
            go.transform.localRotation = Quaternion.identity;
            var cam = go.GetComponent<Camera>();
            cam.enabled = false; // rendered only on request
            cam.orthographic = true;
            cam.orthographicSize = height * 0.5f;
            cam.aspect = width / height;
            cam.nearClipPlane = distance * 0.5f;
            cam.farClipPlane = distance * 2;
            cam.clearFlags = CameraClearFlags.SolidColor;
            background.a = 1;
            cam.backgroundColor = background;
            cam.cullingMask = 1 << layer;
            cam.allowHDR = false;
            cam.allowMSAA = false;
            cam.useOcclusionCulling = false;
            UniversalAdditionalCameraData data = cam.GetUniversalAdditionalCameraData();
            data.renderPostProcessing = false;
            data.antialiasing = AntialiasingMode.None;
            data.renderShadows = false;
            data.requiresColorOption = CameraOverrideOption.Off;
            data.requiresDepthOption = CameraOverrideOption.Off;
            return cam;
        }
    }
}
