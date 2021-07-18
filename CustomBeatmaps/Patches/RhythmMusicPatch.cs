using System;
using System.Collections.Generic;
using System.IO;
using CustomBeatmaps.Audio;
using FMOD.Studio;
using HarmonyLib;
using Rewired;
using Rhythm;
using UnityEngine;
using Utils.ReflectionHelper;
using Object = UnityEngine.Object;

namespace CustomBeatmaps.Patches
{
    public static class RhythmMusicPatch
    {
        private static readonly Color PAUSE_MENU_CUSTOM_RECORD_COLOR = new(0.2901961f, 0.6709353f, 0.9999f, 1);
        private static readonly Color PAUSE_MENU_EDIT_RECORD_COLOR = new(0.3312528f, 0.8018868f, 0.376f, 1);

        private static readonly ManualUnityMusicPlayer UnityMusicPlayer = new ManualUnityMusicPlayer();
        private static readonly ManualBassMusicPlayer BassMusicPlayer = new ManualBassMusicPlayer();
        private static IMusicPlayer SongMusicPlayer = UnityMusicPlayer;

        private static Rhythm.RhythmController _currentController;

        private static bool _reloadBeatmapFlag = false;

        public static bool EditMode { get; private set; } = false;
        public static bool EditPause = false;

        public static Side CurrentSide
        {
            get => _currentController.player.side;
            set => _currentController.player.side = value;
        }

        public static bool ViewCentered
        {
            get => _currentController.cameraIsCentered;
            set => _currentController.cameraIsCentered = value;
        }

        public static Action<Beatmap> BeatmapLoaded;

        public static int TimePosition
        {
            get
            {
                SongMusicPlayer.GetTimelinePosition(out var position);
                return position;
            }
        }

        private static bool IsUserSong(Rhythm.RhythmController instance)
        {
            return Mod.Instance.UserBeatmapList.IsUserSong(instance.parser.audioKey) || EditMode;
        }

        private static void LoadTimeObjectDataFromBeatmap(Rhythm.RhythmController controller)
        {
            controller.SetField("flips", new Queue<FlipInfo>((IEnumerable<FlipInfo>) controller.beatmap.flips));
            controller.SetField("notes", new Queue<NoteInfo>((IEnumerable<NoteInfo>) controller.beatmap.notes));
            controller.SetField("commands", new Queue<CommandInfo>((IEnumerable<CommandInfo>) controller.beatmap.commands));
        }
        
        private static void LoadBeatmapFromFullPath(Rhythm.RhythmController controller, BeatmapParser parser, string fullPath)
        {
            parser.beatmap = ScriptableObject.CreateInstance<Beatmap>();
            string text = File.ReadAllText(fullPath);
            BeatmapParserEngine beatmapParserEngine = new BeatmapParserEngine();
            beatmapParserEngine.ReadBeatmap(text, ref parser.beatmap);
            string beatmapPath = Directory.GetParent(fullPath)?.FullName;
            string relativeAudio = parser.beatmap.general.audioFilename; 
            parser.audioKey = beatmapPath != null? Path.Combine(beatmapPath, relativeAudio) : "";

            controller.beatmap = parser.beatmap;
            LoadTimeObjectDataFromBeatmap(controller);
            controller.score = new Score(5f, controller.leniencyMilliseconds, controller.beatmap.GetSignificantNoteCount(), 20, 4);
        }

        private static void ClearNotes()
        {
            // Clear all notes
            foreach (BaseNote note in Object.FindObjectsOfType<BaseNote>())
            {
                Object.DestroyImmediate(note.gameObject);
            }
        }

        private static string GetEditModeFullAudioPath(BeatmapParser parser)
        {
            // Full path is stored in audio key then.
            return parser.audioKey;
        }

        [HarmonyPatch(typeof(Rhythm.RhythmController), "Miss")]
        [HarmonyPostfix]
        private static void TestMissOverride(Rhythm.RhythmController __instance)
        {
            // No penalty for misses
            if (EditMode)
            {
                __instance.song.health = 100;
            }
        }

        [HarmonyPatch(typeof(Rhythm.RhythmController), "Start")]
        [HarmonyPostfix]
        private static void StartPost(Rhythm.RhythmController __instance)
        {
            _currentController = __instance;
            EditMode = false;
            EditPause = false;
            if (IsUserSong(__instance) || Mod.Instance.IsNextBeatmapInEditMode)
            {
                // Don't play our music (otherwise that results in a creepy low pitch sound lol)
                __instance.GetField<EventInstance>("musicInstance").stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                // Edit mode
                if (Mod.Instance.IsNextBeatmapInEditMode)
                {
                    Debug.Log("Opening next file in Edit Mode");
                    EditMode = true;
                    LoadBeatmapFromFullPath(__instance, __instance.parser, Mod.Instance.BeatmapToEdit);

                    // When our file gets updated...
                    try
                    {
                        var fileWatcher = new FileSystemWatcher
                        {
                            Path = Directory.GetParent(Mod.Instance.BeatmapToEdit)?.FullName,
                            NotifyFilter = NotifyFilters.Attributes
                                           | NotifyFilters.CreationTime
                                           | NotifyFilters.DirectoryName
                                           | NotifyFilters.FileName
                                           | NotifyFilters.LastWrite
                                           | NotifyFilters.Security
                                           | NotifyFilters.Size,
                            EnableRaisingEvents = true
                        };
                        fileWatcher.Filter = "*.osu";
                        fileWatcher.IncludeSubdirectories = true; // TODO: Remove if unnecessary.
                        fileWatcher.Changed += (sender, args) => EditingFileChanged(__instance, args);
                        fileWatcher.Created += (sender, args) => EditingFileChanged(__instance, args);
                    }
                    catch (ArgumentException e)
                    {
                        Debug.LogError(e);
                        Debug.LogError($"INVALID PATH: {Mod.Instance.BeatmapToEdit}");
                    }

                    // Create edit GUI
                    EditViewUI editUI = new GameObject().AddComponent<EditViewUI>();
                    editUI.SeekRequested += (seek) => SeekRequested(__instance, seek);
                }

                var audioPath = __instance.parser.audioKey;
                var fullAudioPath = EditMode? GetEditModeFullAudioPath(__instance.parser) : Mod.Instance.UserBeatmapList.GetSongPath(audioPath);
                Debug.Log($"Playing {fullAudioPath}");
                if (File.Exists(fullAudioPath))
                {
                    bool isMp3 = fullAudioPath.EndsWith(".mp3");
                    // We now DO have mp3 support!
                    SongMusicPlayer = isMp3 ? BassMusicPlayer : UnityMusicPlayer;
                    //__instance.GetField<EventInstance>("musicInstance").stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                    SongMusicPlayer.Initialize(fullAudioPath, 0.8f);
                    SongMusicPlayer.Start();
                    SongMusicPlayer.SetPaused(true);
                }

                // Make the CD blue for custom songs, cause we can
                BlueCDHelper.ApplyCDColor(EditMode? PAUSE_MENU_EDIT_RECORD_COLOR : PAUSE_MENU_CUSTOM_RECORD_COLOR);
            }
            BeatmapLoaded?.Invoke(__instance.beatmap);
        }

        private static void EditingFileChanged(Rhythm.RhythmController __instance, FileSystemEventArgs e)
        {
            string changed = e.FullPath;
            if (changed == Mod.Instance.BeatmapToEdit)
            {
                _reloadBeatmapFlag = true;
            }
        }

        private static void SeekRequested(Rhythm.RhythmController controller, int seek)
        {
            Debug.Log($"SEEK: {seek}");
            ClearNotes();
            SongMusicPlayer.SeekTimelinePosition(seek);
            LoadTimeObjectDataFromBeatmap(controller);

            // Make sure our side is correct.
            controller.InvokeMethod("FixedUpdate");
            controller.player.ChangeSide(controller.GetField<Queue<NoteInfo>>("notes").Peek().side);
        }

        [HarmonyPatch(typeof(Rhythm.RhythmController), "OnDestroy")]
        [HarmonyPostfix]
        private static void OnDestroyPost()
        {
            SongMusicPlayer.Release();
        }

        [HarmonyPatch(typeof(Rhythm.RhythmController), "UpdateSongPosition")]
        [HarmonyPrefix]
        private static void UpdateSongPositionPre(Rhythm.RhythmController __instance, ref bool __runOriginal)
        {
            if (IsUserSong(__instance))
            {
                // Override position setting
                SongMusicPlayer.GetTimelinePosition(out var position);
                if (position == __instance.GetField<float>("lastQueriedPosition"))
                    __instance.song.songPosition += 1000f * Time.deltaTime * __instance.pauseTimeScale;
                else
                    __instance.song.songPosition = position + __instance.playerTimingOffsetMs;

                __instance.SetField("lastQueriedPosition", position);
                __runOriginal = false;
            }
        }

        [HarmonyPatch(typeof(Rhythm.RhythmController), "UpdateBPM")]
        [HarmonyPostfix]
        private static void UpdateBPM(Rhythm.RhythmController __instance)
        {
            if (IsUserSong(__instance))
                // Controller vibrations lol
                if (__instance.song.bpm != 0f)
                {
                    var beatNum = __instance.song.beatNum;
                    var songBeatNum = Mathf.FloorToInt((__instance.song.songPosition - __instance.song.timeOfLastBPM) /
                                                       __instance.song.bpm);
                    if (songBeatNum != beatNum)
                    {
                        SongMusicPlayer.GetPlaybackState(out var state);
                        if (state == PLAYBACK_STATE.PLAYING && JeffBezosController.useControllerVibration)
                            __instance.GetField<Player>("rewired").SetVibration(0, 0.5f, 0.1f);
                    }
                }
        }


        [HarmonyPatch(typeof(Rhythm.RhythmController), "FixedUpdate")]
        [HarmonyPostfix]
        private static void FixedUpdatePost(Rhythm.RhythmController __instance)
        {
            if (IsUserSong(__instance))
            {
                // Pitch stuff, only what's relevant.
                if (__instance.play)
                    if (__instance.GetField<bool>("isStarted") && !__instance.GetField<bool>("isPlaying"))
                        SongMusicPlayer.SetPaused(false);

                if (__instance.song.hitLast)
                {
                    SongMusicPlayer.SetPitch(__instance.timeScale);
                }
                else if (__instance.GetField<bool>("isPlaying"))
                {
                    var pitch = Time.timeScale * __instance.pauseTimeScale;
                    SongMusicPlayer.SetPitch(pitch);
                }
                
                // EDIT STUFF
                if (EditMode && EditPause)
                {
                    SongMusicPlayer.SetPaused(true);
                }
            }
        }


        [HarmonyPatch(typeof(Rhythm.RhythmController), "Update")]
        [HarmonyPrefix]
        private static void UpdatePre(Rhythm.RhythmController __instance, ref bool __runOriginal)
        {
            // Manually do stuff after the fact if we are playing.
            // This is the big kahoona, literally ended up just
            // copy+paste the relevant code and replace fmod music with the custom music player.
            if (IsUserSong(__instance))
            {
                SongMusicPlayer.Update();
                __runOriginal = false;
                if (!__instance.muteSFXOverride)
                {
                    __instance.hitEvent.setVolume(1f);
                    __instance.hitAssistEvent.setVolume(1f);
                    __instance.missEvent.setVolume(1f);
                    __instance.dodgeEvent.setVolume(1f);
                    __instance.holdTopEvent.setVolume(1f);
                    __instance.holdLowEvent.setVolume(1f);
                }
                else
                {
                    __instance.hitEvent.setVolume(0.0f);
                    __instance.hitAssistEvent.setVolume(0.0f);
                    __instance.missEvent.setVolume(0.0f);
                    __instance.dodgeEvent.setVolume(0.0f);
                    __instance.holdTopEvent.setVolume(0.0f);
                    __instance.holdLowEvent.setVolume(0.0f);
                }

                if (Time.timeScale <= 0.0)
                {
                    SongMusicPlayer.SetPaused(true);
                    __instance.hitEvent.setPaused(true);
                    __instance.hitAssistEvent.setPaused(true);
                    __instance.missEvent.setPaused(true);
                    __instance.dodgeEvent.setPaused(true);
                    __instance.holdTopEvent.setPaused(true);
                    __instance.holdLowEvent.setPaused(true);
                }
                else if (__instance.GetField<bool>("isPlaying"))
                {
                    SongMusicPlayer.SetPaused(false);
                    __instance.hitEvent.setPaused(false);
                    __instance.hitAssistEvent.setPaused(false);
                    __instance.missEvent.setPaused(false);
                    __instance.dodgeEvent.setPaused(false);
                    __instance.holdTopEvent.setPaused(false);
                    __instance.holdLowEvent.setPaused(false);
                }

                // EDIT STUFF
                if (EditMode)
                {
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        EditPause = !EditPause;
                        Debug.Log($"EDIT PAUSE: {EditPause}");
                    }
                    if (EditMode && EditPause)
                    {
                        SongMusicPlayer.SetPaused(true);
                        JeffBezosController.SetTimeScale(0, 0);
                    }
                    else
                    {
                        JeffBezosController.SetTimeScale(1, 0);
                    }

                    // Reload beatmaps
                    if (_reloadBeatmapFlag)
                    {
                        Debug.Log("RELOADING BEATMAP");
                        _reloadBeatmapFlag = false;
                        LoadBeatmapFromFullPath(__instance, __instance.parser, Mod.Instance.BeatmapToEdit);
                        ClearNotes();
                        // Clear notes that are past our point
                        Queue<NoteInfo> notes = __instance.GetField<Queue<NoteInfo>>("notes"); 
                        while (notes.Count > 0 && (__instance.song.songPosition >= notes.Peek().time ||
                                                   __instance.freestyleParent != null))
                        {
                            notes.Dequeue();
                        }
                        // "Base Same Lane" issue
                        HashSet<Lane> occupied = new HashSet<Lane>();
                        BaseNote[] componentsInChildren = __instance.GetField<GameObject>("noteGroup").GetComponentsInChildren<BaseNote>();
                        foreach (BaseNote baseNote in componentsInChildren)
                        {
                            BaseKillableNote baseKillableNote;
                            DodgeNote dodgeNote;
                            if ((object) (baseKillableNote = baseNote as BaseKillableNote) != null)
                            {
                                BaseKillableNote value;
                                if (baseKillableNote != null && !baseKillableNote.dead &&
                                    (!occupied.Contains(baseKillableNote.lane) || (!baseKillableNote.isPast &&
                                        !__instance.PastHitRange(baseKillableNote.hitTime))))
                                {
                                    if (occupied.Contains(baseKillableNote.lane))
                                    {
                                        // Dangerous, but it's to prevent a crash later.
                                        GameObject.DestroyImmediate(baseKillableNote.gameObject);
                                    }

                                    occupied.Add(baseKillableNote.lane);
                                }
                            }
                        }

                        // Force a fixed update so notes reload
                        __instance.InvokeMethod("FixedUpdate");
                        // Twice because sometimes it fails and running it again fixes it.
                        // Jank.
                        __instance.InvokeMethod("FixedUpdate");
                        // Make sure our side is correct.
                        __instance.player.ChangeSide(__instance.GetField<Queue<NoteInfo>>("notes").Peek().side);
                        BeatmapLoaded?.Invoke(__instance.beatmap);
                    }
                }

                __instance.song.countdownPhase = __instance.countdownPhase;
                PLAYBACK_STATE state;
                SongMusicPlayer.GetPlaybackState(out state);
                __instance.song.isPlaying = state == PLAYBACK_STATE.PLAYING;
                __instance.song.maxCombo = __instance.song.maxCombo < __instance.song.combo
                    ? __instance.song.combo
                    : __instance.song.maxCombo;
                JeffBezosController.prevAccuracy = __instance.song.accuracy = __instance.score.accuracyScore;
                JeffBezosController.prevMaxCombo = __instance.song.maxCombo;
                JeffBezosController.prevScore = __instance.song.score = (int) __instance.score.totalScore;
                JeffBezosController.prevMiss = __instance.song.missCount;
                JeffBezosController.prevBarely = __instance.song.barelyCount;
                JeffBezosController.prevOk = __instance.song.okCount;
                JeffBezosController.prevGood = __instance.song.goodCount;
                JeffBezosController.prevGreat = __instance.song.greatCount;
                JeffBezosController.prevPerfect = __instance.song.perfectCount;
                JeffBezosController.prevAvgLatency =
                    __instance.song.summedPreciseAccuracy / __instance.song.numHitsWithPreciseAccuracy;
                JeffBezosController.prevSongTitle = __instance.parser.beatmap.metadata.title + "\n" +
                                                    __instance.parser.beatmap.metadata.version;
                if (__instance.song.over && !__instance.song.isPlaying)
                {
                    if (__instance.waitBeforePausingSeconds <= 0.0)
                    {
                        if (__instance.pauseWhenFinished)
                        {
                            JeffBezosController.paused = true;
                        }
                        else
                        {
                            if (!(__instance.loadLevelWhenFinished != ""))
                                return;
                            LevelManager.LoadLevel(__instance.loadLevelWhenFinished);
                        }
                    }
                    else
                    {
                        if (__instance.songSource.isPlaying)
                            return;
                        __instance.waitBeforePausingSeconds -= Time.deltaTime;
                    }
                }
                else
                {
                    if (__instance.song.health <= 0.0 && !__instance.noFail && !__instance.song.gameOver)
                    {
                        __instance.song.gameOver = true;
                        __instance.waitBeforePausingSeconds = 3f;
                        JeffBezosController.SetTimeScale(0.0f, 1f);
                    }

                    if (!__instance.song.gameOver)
                        return;
                    __instance.waitBeforePausingSeconds -= Time.unscaledDeltaTime;
                    if (__instance.waitBeforePausingSeconds > 0.0)
                        return;
                    JeffBezosController.paused = true;
                }
            }
        }
    }
}
