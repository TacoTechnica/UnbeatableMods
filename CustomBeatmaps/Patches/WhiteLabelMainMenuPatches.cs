using System.IO;
using CustomBeatmaps.Audio;
using FMOD.Studio;
using HarmonyLib;
using Rhythm;
using TMPro;
using UnityEngine;
using Utils.ReflectionHelper;

namespace CustomBeatmaps.Patches
{
    public static class WhiteLabelMainMenuPatches
    {
        private static readonly ManualMusicPlayer PreviewMusicPlayer = new();

        [HarmonyPatch(typeof(WhiteLabelMainMenu), "PlaySongPreview", typeof(string))]
        [HarmonyPrefix]
        private static void PlaySongPreview(WhiteLabelMainMenu __instance, ref bool __runOriginal, ref string audioPath)
        {
            if (Mod.Instance.UserBeatmapList.IsUserSong(audioPath))
            {
                // We have a user song, try to use our own player.
                var fullAudioPath = Mod.Instance.UserBeatmapList.GetSongPath(audioPath);
                Debug.Log($"Previewing {fullAudioPath}");
                if (File.Exists(fullAudioPath))
                {
                    __instance.GetField<EventInstance>("songPreviewInstance").stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                    PreviewMusicPlayer.Initialize(fullAudioPath, 0.8f);
                    PreviewMusicPlayer.Start();
                    __runOriginal = false;
                }
            }
            else
            {
                PreviewMusicPlayer.Stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
        }

        [HarmonyPatch(typeof(WhiteLabelMainMenu), "StopSongPreview")]
        [HarmonyPrefix]
        private static void StopSongPreview(WhiteLabelMainMenu __instance)
        {
            PreviewMusicPlayer.Stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }

        [HarmonyPatch(typeof(WhiteLabelMainMenu), "Start")]
        [HarmonyPrefix]
        private static void StartPre(WhiteLabelMainMenu __instance)
        {
            // Ensure the cache is updated
            BeatmapIndex.defaultIndex.UpdateCache();
            var songs = __instance.songs;
            // inject songs
            foreach (var customBeatmap in Mod.Instance.UserBeatmapList.UserBeatmaps)
                if (!songs.Contains(customBeatmap.songName))
                    songs.Add(customBeatmap.songName);
        }

        [HarmonyPatch(typeof(WhiteLabelMainMenu), "Start")]
        [HarmonyPrefix]
        private static void StartPost(WhiteLabelMainMenu __instance)
        {
            // Append MODDED to the intro text
            var titleParent = __instance.gameObject.transform.GetChild(1).GetChild(0);
            var versionNumberTransform = titleParent.Find("VersionNumber");
            var versionNumberText = versionNumberTransform.GetComponent<TMP_Text>();
            versionNumberText.text += $" <size=60%><#0000FFFF> CUSTOM BEATMAPS {Mod.Instance.Version} </color></size>";
        }
    }
}