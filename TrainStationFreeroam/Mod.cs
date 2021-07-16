using System;
using BepInEx;
using UnityEngine;

namespace TrainStationFreeroam
{
    [BepInPlugin("tacotechnica.unbeatable.trainstationfreeroam", "Unbeatable Train Station Free Roam Plugin", "1.0.0.0")]
    public class Mod : BaseUnityPlugin
    {
        // Singleton antipattern
        public static Mod Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            Debug.Log("Train station Freeroam: Entry!");
            /*
            Harmony.CreateAndPatchAll(typeof(WhiteLabelMainMenuPatches));
            Harmony.CreateAndPatchAll(typeof(BeatmapIndexPatches));
            Harmony.CreateAndPatchAll(typeof(RhythmControllerPatches));
            */
        }
    }
}