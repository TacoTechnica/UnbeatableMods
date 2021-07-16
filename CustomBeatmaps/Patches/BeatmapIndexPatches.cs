using System.Collections.Generic;
using HarmonyLib;
using Rhythm;
using UnityEngine;
using Utils.ReflectionHelper;

namespace CustomBeatmaps.Patches
{
    public static class BeatmapIndexPatches
    {
        [HarmonyPatch(typeof(BeatmapIndex), "UpdateCache")]
        [HarmonyPrefix]
        private static void UpdateCachePre(BeatmapIndex __instance)
        {
            // Update cached difficulties so we aren't cock blocked later
            var cachedDifficulties = __instance.GetField<HashSet<string>>("cachedDifficulties");
            foreach (var item in __instance.difficulties) cachedDifficulties?.Add(item);
        }

        [HarmonyPatch(typeof(BeatmapIndex), "UpdateCache")]
        [HarmonyPostfix]
        private static void UpdateCachePost(BeatmapIndex __instance)
        {
            Mod.Instance.UserBeatmapList.RefreshUserBeatmaps(__instance.DifficultyIsSelectable);
            var cachedBySong =
                __instance.GetField<Dictionary<string, List<BeatmapInfo>>>("cachedBySong");
            var cachedByName =
                __instance.GetField<Dictionary<string, BeatmapInfo>>("cachedByName");
            var cachedByPath =
                __instance.GetField<Dictionary<string, BeatmapInfo>>("cachedByPath");
            foreach (var songName in Mod.Instance.UserBeatmapList.SongNames)
                // Only add if we don't have it already. For some reason?
                if (!cachedBySong.ContainsKey(songName))
                    cachedBySong[songName] = new List<BeatmapInfo>();
            foreach (var beatmapInfo in Mod.Instance.UserBeatmapList.UserBeatmaps)
            {
                cachedByName[beatmapInfo.name] = beatmapInfo;
                List<BeatmapInfo> value;
                if (cachedBySong.TryGetValue(beatmapInfo.songName, out value))
                    value.Add(beatmapInfo);
                else
                    Debug.LogWarning("Beatmap name \"" + beatmapInfo.songName +
                                     "\" not present in BeatmapIndex's song names");
                cachedByPath[beatmapInfo.songName + "/" + beatmapInfo.difficulty] = beatmapInfo;
            }
        }
    }
}