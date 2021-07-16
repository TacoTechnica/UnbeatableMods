using BepInEx;
using CustomBeatmaps.Patches;
using HarmonyLib;
using UnityEngine;

namespace CustomBeatmaps
{
    [BepInPlugin("tacotechnica.unbeatable.custombeatmaps", "Unbeatable Custom Beatmaps Plugin", "1.0.0.0")]
    public class Mod : BaseUnityPlugin
    {
        public readonly UserBeatmapList UserBeatmapList = new("USER_BEATMAPS");

        // Singleton antipattern
        public static Mod Instance { get; private set; }

        public readonly string Version = "v1.0.2";
        
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