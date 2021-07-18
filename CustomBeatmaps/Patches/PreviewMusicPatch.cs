using System.IO;
using CustomBeatmaps.Audio;
using FMOD.Studio;
using HarmonyLib;
using UnityEngine;
using Utils.ReflectionHelper;

namespace CustomBeatmaps.Patches
{
    public static class PreviewMusicPatch
    {
        private static IMusicPlayer PreviewUnityMusicPlayer = new ManualBassMusicPlayer();

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
                    PreviewUnityMusicPlayer.Initialize(fullAudioPath, 0.5f);
                    PreviewUnityMusicPlayer.Start();
                    __runOriginal = false;
                }
            }
            else
            {
                PreviewUnityMusicPlayer.Stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
        }

        [HarmonyPatch(typeof(WhiteLabelMainMenu), "StopSongPreview")]
        [HarmonyPrefix]
        private static void StopSongPreview(WhiteLabelMainMenu __instance)
        {
            PreviewUnityMusicPlayer.Stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }

        [HarmonyPatch(typeof(WhiteLabelMainMenu), "Update")]
        [HarmonyPrefix]
        private static void UpdateCustomAudio()
        {
            PreviewUnityMusicPlayer.Update();
        }
    }
}