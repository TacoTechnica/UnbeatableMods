using System;
using System.Linq;
using CustomBeatmaps.Patches;
using Rhythm;
using UnityEngine;

namespace CustomBeatmaps
{
    public class EditViewUI : MonoBehaviour
    {
        private readonly object _lock = new();
        private float _lastNoteTime = 1000;

        public Action<int> SeekRequested;

        private string _timeBoxValue;

        private void Awake()
        {
            RhythmMusicPatch.BeatmapLoaded += UpdateBeatmap;
        }

        private void OnDestroy()
        {
            RhythmMusicPatch.BeatmapLoaded -= UpdateBeatmap;
        }

        public void UpdateBeatmap(Beatmap beatmap)
        {
            lock (_lock)
            {
                if (beatmap.hitObjects.Count != 0)
                {
                    _lastNoteTime = beatmap.hitObjects.Last().time;
                    Debug.Log($"deleteme LAST TIME: {_lastNoteTime}");
                }
            }
        }

        private void GUITimeInfo(float edgePad, float xSize, float sliderHeight)
        {
            float w = Screen.width;
            float h = Screen.height;

            float timeBoxWidth = 128;
            lock (_lock)
            {
                int timePosition = RhythmMusicPatch.TimePosition;
                float dx = edgePad + xSize + edgePad;
                float top = h - edgePad - sliderHeight;
                _timeBoxValue = GUI.TextField(new Rect(dx, top, timeBoxWidth, sliderHeight), _timeBoxValue);
                GUI.Label(new Rect(dx, top - 32, timeBoxWidth, 32), $"{timePosition} ms");
                dx += timeBoxWidth + edgePad;
                if (GUI.Button(new Rect(dx, top, 64, sliderHeight), "Go"))
                {
                    int updateTime;
                    if (int.TryParse(_timeBoxValue, out updateTime))
                    {
                        if (updateTime != timePosition)
                        {
                            SeekRequested?.Invoke(updateTime);
                            return;
                        }
                    }
                }
                else
                {
                    dx += 64 + edgePad;
                    int updateTime = (int) GUI.HorizontalSlider(
                        new Rect(dx, top, w - dx - edgePad, sliderHeight),
                        timePosition, 0, _lastNoteTime);
                    if (updateTime != timePosition)
                    {
                        SeekRequested?.Invoke(updateTime);
                        return;
                    }
                }
            }
        }

        private void OnGUI()
        {
            if (!RhythmMusicPatch.EditMode) return;

            float w = Screen.width;
            float h = Screen.height;

            float xSize = 128;
            float edgePad = 16;
            if (GUI.Button(new Rect(edgePad, h - edgePad - xSize, xSize, xSize),
                RhythmMusicPatch.EditPause ? "UNPAUSE" : "PAUSE"))
            {
                RhythmMusicPatch.EditPause = !RhythmMusicPatch.EditPause;
            }

            GUITimeInfo(edgePad, xSize, 64);

            // Side Buttons
            float sideButtonHeight = 64;
            float sideButtonWidth = 64;
            float dh = h - xSize - edgePad * 2;
            float xpos = w - edgePad - sideButtonWidth;
            // Toggle Center
            if (GUI.Button(new Rect(xpos, dh, sideButtonWidth, sideButtonHeight),
                RhythmMusicPatch.ViewCentered ? "ON" : "off"))
            {
                RhythmMusicPatch.ViewCentered = !RhythmMusicPatch.ViewCentered;
            }
            dh -= sideButtonHeight + edgePad;
            // Toggle side
            string side;
            switch (RhythmMusicPatch.CurrentSide)
            {
                case Side.Left:
                    side = "Left";
                    break;
                case Side.Right:
                    side = "Right";
                    break;
                default:
                    side = "(none)";
                    break;
            }
            if (GUI.Button(new Rect(xpos, dh, sideButtonWidth, sideButtonHeight), side))
            {
                RhythmMusicPatch.CurrentSide = RhythmMusicPatch.CurrentSide.GetOpposite();
            }

            dh -= sideButtonHeight + edgePad + 128;
            GUI.Label(new Rect(xpos, dh, sideButtonWidth, 256), "Use these buttons to fix camera side/center issues");
        }
    }
}
