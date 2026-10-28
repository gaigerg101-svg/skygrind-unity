// NIGHT36 builder M (design.n36.text36, a view row: N36Fixes.ViewRowOn; NIGHT36.md M1, REVIEW.md root cause 10): THE TEACHING SLOT. GG 2026-10-12:
// "the tutorial text is way too high up". Every teaching line (teach35), every lesson line (Lesson25) and a boss's teaching line used to share hud33's
// top message slot with the room name, at 24 px, 54 px from the top, over the darkest part of the frame. With the row on they leave the top slot for
// their own slot in the lower middle (its centre UnityTuning.Hud36TeachCentre = 62 % down, under the crosshair where the eye already is), 32 px at
// 1080p, up to two lines in a 528 px column, on a dark plate (the lead's n36-r1 frame: round 1's soft scrim was invisible over the pale viewmodel;
// #0B0A1A at 68 %, 16 px padding, a 2 px cyan left edge: UnityTuning.Move36.cs Hud36Plate*). The top slot keeps the room names, the door hint, EXPOSED and
// the posts. The teaching slot is a second MessageSlot33 with the same rule and the same channels the top slot used for these lines (a boss's line
// first, then the teach35 line, then a lesson), the same 1.2 s minimum hold, the same flicker-on and fade-out. The hud33 rule checker knows the box
// (Hud33Layout.TeachSlot: 640 px column, 80 px tall, 32 px text).
// Off (the row off, -skygrindOff36): nothing is built and UiHud33.Slot33 routes as NIGHT35 did.
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        bool t36On;
        UiStyle33.Text33 h36Teach;
        Image h36Plate, h36Edge;
        Color h36PlateRgb;
        readonly MessageSlot33 h36Msg = new MessageSlot33 { MinHold = UnityTuning.Hud33SlotMinHold, DropAfter = UnityTuning.Hud33SlotDropAfter }; // pure C#: safe in an initialiser
        double h36Fade;
        string h36Text = "";
        int h36Prio = -1, h36ShowsSeen;

        /// <summary>For the reels: the teaching slot's text now ("" none), its font px, its rect's centre as a share of the canvas height from the top,
        /// whether the row built it, and how many lines it has shown.</summary>
        public static string Hud36TeachNow = "";
        public static float Hud36TeachPxNow;
        public static double Hud36TeachCentreNow = -1;
        public static bool Text36Built;
        /// <summary>For the reels: the teaching slot's rect and the top slot's (their screen corners are measured).</summary>
        public static RectTransform Text36Rect, Slot33Rect;
        public static int Hud36TeachShows;

        /// <summary>Builds the teaching slot under `hud33` when text36 is on (called by UiHud33's build, after the top slot).</summary>
        void BuildText36(RectTransform hud33)
        {
            t36On = game != null && (N36Fixes.ViewRowOn(game.cfg, "text36") || Teach43On); // NIGHT43 U1 teach43: the centre card takes this slot's lines (UiTeach43.cs)
            Text36Built = t36On;
            Slot33Rect = h33Slot != null ? h33Slot.rt : null;
            if (!t36On) return;
            if (Teach43On) BuildTeach43(hud33); // NIGHT43 U1 teach43: the card draws; this slot keeps the queue and draws nothing
            Hud33Layout.Box b = Hud33Layout.TeachSlot();
            h36Teach = UiStyle33.Label(hud33, Hud33Layout.TeachSlotName, (float)b.px, UiStyle33.Bone, UiStyle33.FaceLabel, TextAlignmentOptions.Center, new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2((float)b.x, (float)b.y), new Vector2((float)b.w, (float)b.h));
            foreach (TextMeshProUGUI t in new[] { h36Teach.main, h36Teach.back })
            {
                t.richText = true; // a lesson line carries the pad's glyph sprites
                t.textWrappingMode = TextWrappingModes.Normal; // up to two lines in the 640 px column
                t.overflowMode = TextOverflowModes.Ellipsis;
            }
            // the lead's n36-r1: a real dark plate (#0B0A1A at 68 %), 16 px past the lines, a 2 px cyan left edge; behind the text (first sibling)
            h36Plate = UiStyle33.Line(h36Teach.rt, "Plate", new Color(0, 0, 0, 0), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(200, (float)b.h));
            h36Plate.rectTransform.SetAsFirstSibling();
            h36PlateRgb = Look.Hex(UnityTuning.Hud36PlateHex);
            h36Edge = UiStyle33.Line(h36Plate.rectTransform, "Edge", new Color(0, 0, 0, 0), new Vector2(0, 0.5f), new Vector2(0, 0.5f), Vector2.zero, new Vector2(UnityTuning.Hud36PlateEdgeW, 0));
            RectTransform ert = h36Edge.rectTransform;
            ert.anchorMin = new Vector2(0, 0);
            ert.anchorMax = new Vector2(0, 1);
            ert.sizeDelta = new Vector2(UnityTuning.Hud36PlateEdgeW, 0);
            Color so = UiStyle33.Shadow;
            h36Teach.main.outlineColor = new Color32((byte)(so.r * 255), (byte)(so.g * 255), (byte)(so.b * 255), 255);
            h36Teach.main.outlineWidth = UnityTuning.Hud36TeachOutline;
            h36Teach.Alpha = 0f;
            Text36Rect = h36Teach.rt;
            Hud36TeachPxNow = h36Teach.main.fontSize;
            Hud36TeachCentreNow = UnityTuning.Hud36TeachCentre;
            game.OnNewRun += () =>
            {
                h36Msg.Clear();
                h36Text = "";
                h36Prio = -1;
                h36Fade = 0;
                h36Teach.Set("");
                Hud36TeachNow = "";
            };
            // NIGHT44 V2 (vis44, T-69): a new room drops the last room's lesson line at once (MessageSlot33 held it MinHold and the teach43
            // card stayed up over the new room); the card closes on the next frame's empty slot (Teach43Draw's close)
            game.events.On<Skygrind.Core.RoomEvent>("roomEnter", _ => Vis44RoomEnter());
        }

        /// <summary>NIGHT44 V2 (T-69): the roomEnter handler (the look44 reel calls it directly on a map with no rooms).</summary>
        public void Vis44RoomEnter()
        {
            if (h36Msg == null || !Vis44.On(game.cfg)) return;
            h36Msg.Drop(MessageSlot33.Lesson);
            Vis44.RoomDrops++;
        }

        /// <summary>NIGHT44 V2: the slot's line now (null: nothing held), for the look44 reel.</summary>
        public string Vis44SlotText => h36Msg?.Text;

        /// <summary>NIGHT44 V2: the look44 reel's lesson line (a post on the Lesson channel, as a room's lesson shows).</summary>
        public void Vis44PostLesson(string text, double seconds) => h36Msg?.Post(MessageSlot33.Lesson, text, h33Clock, seconds);

        /// <summary>One frame of the teaching slot (text36 on): the boss's line, the teach35 line and the lesson line, as UiHud33.Slot33 found them.</summary>
        void Slot36(double frameDt, double now, string lesson, bool bossLine, string teach)
        {
            if (bossLine || teach == null) h36Msg.Want(MessageSlot33.Boss, bossLine ? lesson : null);
            else h36Msg.WantLive(MessageSlot33.Boss, teach); // live: a device change re-reads its button in place
            h36Msg.Want(MessageSlot33.Lesson, bossLine ? null : lesson);
            string s = h36Msg.Step(now);
            if (s != null && (s != h36Text || h36Msg.Prio != h36Prio))
            {
                bool inPlace = h36Msg.Shows == h36ShowsSeen && h36Msg.Prio == h36Prio && h36Fade > 0;
                if (!inPlace)
                {
                    h36Fade = 0;
                    Hud36TeachShows++;
                }
                h36Text = s;
                h36Prio = h36Msg.Prio;
                Teach35Case(s);
                h36Teach.Set(s);
                h36Teach.Color = UiStyle33.Bone;
                h36Teach.main.ForceMeshUpdate();
                Vector2 pref = h36Teach.main.GetPreferredValues(s, h36Teach.rt.sizeDelta.x, 0);
                float tw = Mathf.Min(pref.x, h36Teach.rt.sizeDelta.x), th = Mathf.Min(pref.y, h36Teach.rt.sizeDelta.y);
                h36Plate.rectTransform.sizeDelta = new Vector2(tw + 2 * UnityTuning.Hud36PlatePad, th + 2 * UnityTuning.Hud36PlatePad);
            }
            h36ShowsSeen = h36Msg.Shows;
            if (s != null) h36Fade = Math.Min(1, h36Fade + frameDt / UnityTuning.Hud33SlotFadeIn);
            else if (h36Fade > 0)
            {
                h36Fade = Math.Max(0, h36Fade - frameDt / UnityTuning.Hud33SlotFadeOut);
                if (h36Fade <= 0)
                {
                    h36Text = "";
                    h36Prio = -1;
                    h36Teach.Set("");
                }
            }
            Hud36TeachNow = h36Fade > 0 ? h36Text : "";
            if (t43Root != null) Teach43Draw(s, frameDt); // NIGHT43 U1 teach43: the centre card (UiTeach43.cs)
            float a = t43Root != null ? 0f : Mathf.Round((float)h36Fade * 50f) / 50f; // teach43: this slot stays invisible
            if (a != h36Teach.Alpha)
            {
                h36Teach.Alpha = a;
                h36Plate.color = new Color(h36PlateRgb.r, h36PlateRgb.g, h36PlateRgb.b, UnityTuning.Hud36PlateAlpha * a);
                Color cy = UiStyle33.Cyan;
                h36Edge.color = new Color(cy.r, cy.g, cy.b, a);
            }
        }

        /// <summary>The label whose case Teach35Case sets: the teaching slot with text36 on, else the top slot.</summary>
        UiStyle33.Text33 Teach35Label => t36On && h36Teach != null ? h36Teach : h33Slot;

        /// <summary>What the teaching line's queue reads as "shown last frame": the teaching slot with text36 on, else the top slot.</summary>
        string Teach35SlotNow => t36On ? Hud36TeachNow : Hud33SlotNow;
    }
}
