using System;
using System.IO;
using System.Threading;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace CustomBeatmaps
{
    [BepInPlugin("tacotechnica.unbeatable.custombeatmaps", "Unbeatable Custom Beatmaps Plugin", "1.0.0.0")]
    public class Mod : BaseUnityPlugin
    {
        // Singleton antipattern
        public static Mod Instance { get; private set; }

        public readonly UserBeatmapList UserBeatmapList = new UserBeatmapList("USER_BEATMAPS");

        private void Awake()
        {
            Instance = this;
            Debug.Log("Custom Beatmap: Entry!");
            Harmony.CreateAndPatchAll(typeof(WhiteLabelMainMenuPatches));
            Harmony.CreateAndPatchAll(typeof(BeatmapIndexPatches));
            Harmony.CreateAndPatchAll(typeof(RhythmControllerPatches));
        }
    }
}
