using System;
using HarmonyLib;
using TMPro;
using UnityEngine;
using Utils.ReflectionHelper;

namespace CustomBeatmaps.Patches
{
    public class MainMenuEditOptionVisualPatch
    {
        private static readonly string EDIT_INFO_APPEND_TEXT = "\n<size=20%><color=red>E: Edit OSU file</color></size>";
        private static bool _requestUpdateFlag;
        private static string _menuText;

        private static BeatmapEditViewButton _openEditViewButton;
        
        [HarmonyPatch(typeof(WhiteLabelMainMenu), "Start")]
        [HarmonyPrefix]
        private static void OnStart()
        {
            _requestUpdateFlag = true;
            _openEditViewButton = new GameObject().AddComponent<BeatmapEditViewButton>();
        } 

        [HarmonyPatch(typeof(WhiteLabelMainMenu), "MenuDefaultUpdate")]
        [HarmonyPostfix]
        private static void MenuUpdatePost(WhiteLabelMainMenu __instance)
        {
            bool startIsSelected = __instance.GetField<WrapCounter>("selectionInc") == 2;

            if (_openEditViewButton != null)
            {
                _openEditViewButton.enabled = startIsSelected;
            }
        }

        public class BeatmapEditViewButton : MonoBehaviour
        {
            public Action OnPress;

            private void OnGUI()
            {
                float w = Screen.width,
                    h = Screen.height;
                float editButtonWidth = 128,
                    editButtonHeight = 64;
                float pad = 16;
                if (GUI.Button(
                    new Rect(w - pad - editButtonWidth, h - pad - editButtonHeight, editButtonWidth, editButtonHeight),
                    "Edit Beatmaps View"))
                {
                    OnPress?.Invoke();
                }
            }
        }
    }
}
