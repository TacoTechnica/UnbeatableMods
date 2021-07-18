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
    public static class MainMenuLoadMusicPatch
    {

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
    }
}