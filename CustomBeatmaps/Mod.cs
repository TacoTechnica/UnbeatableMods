using BepInEx;
using CustomBeatmaps.OsuReading;
using CustomBeatmaps.Patches;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Audio;

namespace CustomBeatmaps
{
    [BepInPlugin("tacotechnica.unbeatable.custombeatmaps", "Unbeatable Custom Beatmaps Plugin", "1.1.0")]
    public class Mod : BaseUnityPlugin
    {
        public readonly UserBeatmapList UserBeatmapList = new("USER_BEATMAPS");

        public readonly Settings Settings = Settings.Load("CustomBeatmapSettings.json");

        public readonly OsuReader OsuReader = new OsuReader();

        public string BeatmapToEdit = "";
        public bool IsNextBeatmapInEditMode => !string.IsNullOrEmpty(BeatmapToEdit);

        // Getting the music mixer
        private static AudioMixerGroup _musicMixerGroup;
        public static AudioMixerGroup MusicMixerGroup
        {
            get
            {
                if (_musicMixerGroup == null)
                {
                    var mixer = FindObjectOfType<JeffBezosController>().masterMixer;
                    /*
                     * Groups: Master, Music, Environment, SFX, UI
                     */
                    _musicMixerGroup = mixer.FindMatchingGroups("Music")[0];
                }
                return _musicMixerGroup;
            }
        }
        
        // Singleton antipattern
        public static Mod Instance { get; private set; }

        public readonly string Version =  "v1.1.0";

        private void Awake()
        {
            Instance = this;
            Debug.Log("Custom Beatmap: Entry!");
            Harmony.CreateAndPatchAll(typeof(MainMenuLoadMusicPatch));
            Harmony.CreateAndPatchAll(typeof(MainMenuModVersionPatch));
            Harmony.CreateAndPatchAll(typeof(MainMenuEditOptionPatch));
            Harmony.CreateAndPatchAll(typeof(MainMenuEditOptionVisualPatch));
            Harmony.CreateAndPatchAll(typeof(PreviewMusicPatch));
            Harmony.CreateAndPatchAll(typeof(BeatmapIndexPatches));
            Harmony.CreateAndPatchAll(typeof(RhythmMusicPatch));
        }
    }
}