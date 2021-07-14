using System.Collections.Generic;
using System.Reflection;
using CustomBeatmaps.ReflectionHelper;
using HarmonyLib;
using Rhythm;
using UnityEngine;

namespace CustomBeatmaps
{
    public static class BeatmapIndexPatches
    {

        [HarmonyPatch(typeof(BeatmapIndex), "UpdateCache")]
        [HarmonyPrefix]
        static void UpdateCachePre(BeatmapIndex __instance)
        {
            // Update cached difficulties so we aren't cock blocked later
            HashSet<string> cachedDifficulties = __instance.GetField<HashSet<string>>("cachedDifficulties");
            foreach (string item in __instance.difficulties)
            {
                cachedDifficulties?.Add(item);
            }
        }

        [HarmonyPatch(typeof(BeatmapIndex), "UpdateCache")]
        [HarmonyPostfix]
        static void UpdateCachePost(BeatmapIndex __instance)
        {
            Mod.Instance.UserBeatmapList.RefreshUserBeatmaps(__instance.DifficultyIsSelectable);
            Dictionary<string, List<BeatmapInfo>> cachedBySong =
                __instance.GetField<Dictionary<string, List<BeatmapInfo>>>("cachedBySong");
            Dictionary<string, BeatmapInfo> cachedByName =
                __instance.GetField<Dictionary<string, BeatmapInfo>>("cachedByName");
            Dictionary<string, BeatmapInfo> cachedByPath =
                __instance.GetField<Dictionary<string, BeatmapInfo>>("cachedByPath");
            foreach (string songName in Mod.Instance.UserBeatmapList.SongNames)
            {
                // Only add if we don't have it already. For some reason?
                if (!cachedBySong.ContainsKey(songName))
                {
                    cachedBySong[songName] = new List<BeatmapInfo>();
                }
            }
            foreach (BeatmapInfo beatmapInfo in Mod.Instance.UserBeatmapList.UserBeatmaps)
            {
                cachedByName[beatmapInfo.name] = beatmapInfo;
                List<BeatmapInfo> value;
                if (cachedBySong.TryGetValue(beatmapInfo.songName, out value))
                {
                    value.Add(beatmapInfo);
                }
                else
                {
                    Debug.LogWarning("Beatmap name \"" + beatmapInfo.songName + "\" not present in BeatmapIndex's song names");
                }
                cachedByPath[beatmapInfo.songName + "/" + beatmapInfo.difficulty] = beatmapInfo;
            }
        }
    }
}
