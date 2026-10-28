// NIGHT16 U3-29.8 polish #8 (the press half; the lead's): a menu button dips to UnityTuning.PressScale the moment it is
// pressed (a pointer down, or the pad / keyboard's submit) and springs back over UnityTuning.PressTime on unscaled frame
// time. Visual only: the click acts at once, as before (the web's menus.css :active has no transition either).
// The review's finding 11: the first version slept by disabling itself, and UGUI sends no events to a disabled component,
// so the dip never played; it now stays enabled and sleeps on a flag. While the button is scaled, its Image's raycast rect
// is widened by exactly what the scale took (raycastPadding), so a fast second click on its outer edge still lands.
using UnityEngine;
using UnityEngine.EventSystems;

namespace Skygrind.Game
{
    public sealed class UiPress : MonoBehaviour, IPointerDownHandler, ISubmitHandler
    {
        float pressAge = float.MaxValue;
        bool pressing;
        UnityEngine.UI.Image pressImage;
        RectTransform pressRect;

        void Awake()
        {
            pressImage = GetComponent<UnityEngine.UI.Image>();
            pressRect = transform as RectTransform;
        }

        public void OnPointerDown(PointerEventData eventData) => Press();

        public void OnSubmit(BaseEventData eventData) => Press();

        void Press()
        {
            Pg3Audio.Ui("ui-confirm"); // PG3 sound74: the confirm click (Pg3Audio.cs; silent with the row off)
            pressAge = 0;
            pressing = true;
            Apply(UnityTuning.PressScale);
        }

        void Update()
        {
            if (!pressing) return;
            pressAge += (float)FeelClock.FrameDelta; // NIGHT18: the reel's fixed clock while it records
            float t = Mathf.Clamp01(pressAge / UnityTuning.PressTime);
            float k = 1 - (1 - t) * (1 - t); // ease-out back to full size
            Apply(t >= 1 ? 1 : Mathf.LerpUnclamped(UnityTuning.PressScale, 1, k));
            if (t >= 1) pressing = false;
        }

        /// <summary>The scale, and the raycast rect widened back to the unscaled size (local units: 1 / s - 1 of a half-size).</summary>
        void Apply(float s)
        {
            transform.localScale = new Vector3(s, s, 1);
            if (pressImage == null || pressRect == null) return;
            Vector2 size = pressRect.rect.size;
            float grow = s >= 1 ? 0 : (1 / s - 1) * 0.5f;
            pressImage.raycastPadding = new Vector4(-size.x * grow, -size.y * grow, -size.x * grow, -size.y * grow);
        }

        void OnDisable()
        {
            pressing = false;
            Apply(1); // a screen closed mid-dip: the button comes back whole next time
        }
    }
}
