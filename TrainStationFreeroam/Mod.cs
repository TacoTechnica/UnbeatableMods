using System;
using BepInEx;
using HarmonyLib;
using TrainStationFreeroam.Patches;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TrainStationFreeroam
{
    [BepInPlugin("tacotechnica.unbeatable.trainstationfreeroam", "Unbeatable Train Station Free Roam Plugin", "1.0.0.0")]
    public class Mod : BaseUnityPlugin
    {
        // Singleton antipattern
        public static Mod Instance { get; private set; }

        public readonly string Version = "v1.0.0";
        public readonly string TrainSceneName = "C2_TrainStationNight";

        private void Awake()
        {
            Instance = this;
            Debug.Log("Train station Freeroam: Entry!");
            Harmony.CreateAndPatchAll(typeof(WhiteLabelMainMenuPatches));

            // On the train scene, run some extra logic.
            SceneManager.sceneLoaded += (scene, mode) =>
            {
                if (scene.name == TrainSceneName)
                {
                    Debug.Log("WE'RE IN!");
                    TrainSceneInjections.Inject();
                }
            };
        }
    }
}
