using System;
using System.IO;
using HarmonyLib;
using Rhythm;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils.ReflectionHelper;


namespace CustomBeatmaps.Patches
{
    public static class MainMenuEditOptionPatch
    {

        private static SimpleBeatmapPicker _beatmapPicker;

        private static MainMenuEditOptionVisualPatch.BeatmapEditViewButton _openEditViewButton;

        [HarmonyPatch(typeof(WhiteLabelMainMenu), "Start")]
        [HarmonyPostfix]
        private static void OnStart()
        {
            // Reset editing beatmap flag
            Mod.Instance.BeatmapToEdit = "";

            _openEditViewButton =
                GameObject.FindObjectOfType<MainMenuEditOptionVisualPatch.BeatmapEditViewButton>();
            if (_openEditViewButton != null)
            {
                _openEditViewButton.OnPress += () =>
                {
                    if (_beatmapPicker == null)
                    {
                        _beatmapPicker = new GameObject().AddComponent<SimpleBeatmapPicker>();
                        _beatmapPicker.BeatmapSelected += BeatmapSelected;
                        _beatmapPicker.OnClose += () => _beatmapPicker.enabled = false;
                    }
                    else
                    {
                        _beatmapPicker.enabled = !_beatmapPicker.enabled;
                    }
                };
            }
        } 

        private static void BeatmapSelected(string beatmapPath)
        {
            Debug.Log($"OPENING EDITOR VIEW FOR {beatmapPath}");
            // TODO: This is really ugly
            Mod.Instance.BeatmapToEdit = beatmapPath;
            SceneManager.LoadScene("TEST_RHYTHM");
        }
    }

    class SimpleBeatmapPicker : MonoBehaviour
    {
        // Cache this, reloading on every GUI update is no bueno practice
        private string[] _beatmapList = new string[0];
        private Vector2 _beatmapListScrollPosition = Vector2.zero;

        public Action<string> BeatmapSelected;
        public Action OnClose;

        private void OnEnable()
        {
            Debug.Log("(Beatmap Edit Selector) BEATMAPS RELOADED");
            _beatmapList = Mod.Instance.OsuReader.GetBeatmapList();
        }

        private void OnGUI()
        {
            // Ahhh yes, good ol code based UI
            // Gives me throwbacks to my Game Maker Studio days.
            // Good times.
            float pad = 32;
            float w = Screen.width,
                h = Screen.height;
            float closeButtonSize = 32;
            float closeButtonPadding = 16;
            
            GUI.Window(0, new Rect(pad, pad, w - 2 * pad, h - 2 * pad), id =>
            {
                if (GUI.Button(new Rect(w - pad*3 - closeButtonSize - closeButtonPadding, pad*2 + closeButtonPadding, closeButtonSize, closeButtonSize), "X"))
                {
                    OnClose?.Invoke();
                } else if (Input.GetKeyDown(KeyCode.Escape))
                {
                    OnClose?.Invoke();
                }
                
                
                if (Mod.Instance.OsuReader.SongsFolderFound)
                {
                    GUILayout.Label("Select an OSU file to preview.");
                    GUILayout.Label("Previewing an OSU file has the following perks:" +
                                    "\n- Play a beatmap straight from OSU" +
                                    "\n- No death" +
                                    "\n- Pause beatmap with spacebar" +
                                    "\n- Jump around beatmap with slider (semi buggy)" +
                                    "\n- Auto reloads the beatmap when you update the OSU file");
                    _beatmapListScrollPosition = GUILayout.BeginScrollView(_beatmapListScrollPosition, false, true);
                    foreach (string beatmap in _beatmapList)
                    {
                        string simple = Path.GetFileNameWithoutExtension(beatmap);
                        if (GUILayout.Button(simple))
                        {
                            BeatmapSelected?.Invoke(beatmap);
                        }
                    }
                    GUILayout.EndScrollView();
                }
                else
                {
                    GUILayout.Label($"Song folder not found at {Mod.Instance.OsuReader.SongsFolder}." +
                                    $" Edit {Mod.Instance.Settings.Path} to point to a valid OSU songs folder path.");
                }
            }, "Open OSU Beatmap in Edit Mode");
        }
    }
}
