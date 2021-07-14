using System.Collections.Generic;
using System.IO;
using CustomBeatmaps.Audio;
using CustomBeatmaps.ReflectionHelper;
using FMOD.Studio;
using HarmonyLib;
using Rhythm;
using TMPro;
using UnityEngine;

namespace CustomBeatmaps
{
    public static class WhiteLabelMainMenuPatches
    {
        private static readonly ManualMusicPlayer PreviewMusicPlayer = new ManualMusicPlayer();

        [HarmonyPatch(typeof(WhiteLabelMainMenu), "PlaySongPreview", typeof(string))]
        [HarmonyPrefix]
        static void PlaySongPreview(WhiteLabelMainMenu __instance, ref bool __runOriginal, ref string audioPath)
        {
            if (Mod.Instance.UserBeatmapList.IsUserSong(audioPath))
            {
                // We have a user song, try to use our own player.
                string fullAudioPath = Mod.Instance.UserBeatmapList.GetSongPath(audioPath);
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
        static void StopSongPreview(WhiteLabelMainMenu __instance)
        {
            PreviewMusicPlayer.Stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }

        [HarmonyPatch(typeof(WhiteLabelMainMenu), "Start")]
        [HarmonyPrefix]
        static void StartPre(WhiteLabelMainMenu __instance)
        {
            // Ensure the cache is updated
            BeatmapIndex.defaultIndex.UpdateCache();
            List<string> songs = __instance.songs;
            // inject songs
            foreach (BeatmapInfo customBeatmap in Mod.Instance.UserBeatmapList.UserBeatmaps)
            {
                if (!songs.Contains(customBeatmap.songName))
                {
                    songs.Add(customBeatmap.songName);
                }
            }
        }

        [HarmonyPatch(typeof(WhiteLabelMainMenu), "Start")]
        [HarmonyPrefix]
        static void StartPost(WhiteLabelMainMenu __instance)
        {
            // Append MODDED to the intro text
            Transform titleParent = __instance.gameObject.transform.GetChild(1).GetChild(0);
            Transform versionNumberTransform = titleParent.Find("VersionNumber");
            TMP_Text versionNumberText = versionNumberTransform.GetComponent<TMP_Text>();
            versionNumberText.text += " <#0000FFFF> CUSTOM BEATMAPS v1.0 </color>";
        }

    }
}
