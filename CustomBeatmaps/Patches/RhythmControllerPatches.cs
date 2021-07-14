using System.IO;
using CustomBeatmaps.Audio;
using CustomBeatmaps.ReflectionHelper;
using FMOD.Studio;
using HarmonyLib;
using Rewired;
using UnityEngine;

namespace CustomBeatmaps
{
    public static class RhythmControllerPatches
    {
        private static readonly ManualMusicPlayer SongMusicPlayer = new ManualMusicPlayer();

        private static bool IsUserSong(Rhythm.RhythmController instance)
        {
	        return Mod.Instance.UserBeatmapList.IsUserSong(instance.parser.audioKey);
        }
        
        [HarmonyPatch(typeof(Rhythm.RhythmController), "Start")]
        [HarmonyPostfix]
        static void StartPost(Rhythm.RhythmController __instance)
        {
            if (IsUserSong(__instance))
            {
	            string audioPath = __instance.parser.audioKey;
                string fullAudioPath = Mod.Instance.UserBeatmapList.GetSongPath(audioPath);
                Debug.Log($"Previewing {fullAudioPath}");
                if (File.Exists(fullAudioPath))
                {
                    //__instance.GetField<EventInstance>("musicInstance").stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                    SongMusicPlayer.Initialize(fullAudioPath, 0.8f);
                    SongMusicPlayer.Start();
                    SongMusicPlayer.SetPaused(true);
                }

                // Make the CD blue for custom songs, cause we can
                BlueCDHelper.ApplyBlueCD();
            }
        }

        [HarmonyPatch(typeof(Rhythm.RhythmController), "OnDestroy")]
        [HarmonyPostfix]
        static void OnDestroyPost()
        {
            SongMusicPlayer.Release();
        }

        [HarmonyPatch(typeof(Rhythm.RhythmController), "UpdateSongPosition")]
        [HarmonyPrefix]
        static void UpdateSongPositionPre(Rhythm.RhythmController __instance, ref bool __runOriginal)
        {
	        if (IsUserSong(__instance))
	        {
		        // Override position setting
		        SongMusicPlayer.GetTimelinePosition(out var position);
		        if ((float)position == __instance.GetField<float>("lastQueriedPosition"))
		        {
			        __instance.song.songPosition += 1000f * Time.deltaTime * __instance.pauseTimeScale;
		        }
		        else
		        {
			        __instance.song.songPosition = (float)position + __instance.playerTimingOffsetMs;
		        }
		        __instance.SetField("lastQueriedPosition", position);
		        __runOriginal = false;
	        }
        }

        [HarmonyPatch(typeof(Rhythm.RhythmController), "UpdateBPM")]
        [HarmonyPostfix]
        static void UpdateBPM(Rhythm.RhythmController __instance)
        {
	        if (IsUserSong(__instance))
	        {
		        // Controller vibrations lol
		        if (__instance.song.bpm != 0f)
		        {
			        int beatNum = __instance.song.beatNum;
			        int songBeatNum = Mathf.FloorToInt((__instance.song.songPosition - __instance.song.timeOfLastBPM) /
			                                           __instance.song.bpm);
			        if (songBeatNum != beatNum)
			        {
				        SongMusicPlayer.GetPlaybackState(out var state);
				        if (state == PLAYBACK_STATE.PLAYING && JeffBezosController.useControllerVibration)
				        {
					        __instance.GetField<Player>("rewired").SetVibration(0, 0.5f, 0.1f);
				        }
			        }
		        }
	        }
        }


        [HarmonyPatch(typeof(Rhythm.RhythmController), "FixedUpdate")]
        [HarmonyPostfix]
        static void FixedUpdatePost(Rhythm.RhythmController __instance)
        {
	        if (IsUserSong(__instance))
	        {
		        // Pitch stuff, only what's relevant.
		        if (__instance.play)
		        {
			        if (__instance.GetField<bool>("isStarted") && !__instance.GetField<bool>("isPlaying"))
			        {
				        SongMusicPlayer.SetPaused(false);
			        }
		        }

		        if (__instance.song.hitLast)
		        {
			        SongMusicPlayer.SetPitch(__instance.timeScale);
		        }
		        else if (__instance.GetField<bool>("isPlaying"))
		        {
			        float pitch = Time.timeScale * __instance.pauseTimeScale;
			        SongMusicPlayer.SetPitch(pitch);
		        }
	        }
        }
        
        
        [HarmonyPatch(typeof(Rhythm.RhythmController), "Update")]
        [HarmonyPostfix]
        static void UpdatePost(Rhythm.RhythmController __instance, ref bool __runOriginal)
        {
	        // Manually do stuff after the fact if we are playing.
	        // This is the big kahoona, literally ended up just
	        // copy+paste the relevant code and replace fmod music with the custom music player.
	        if (IsUserSong(__instance))
	        {
		        if (Time.timeScale <= 0f)
		        {
			        SongMusicPlayer.SetPaused(true);
		        }
		        else if (__instance.GetField<bool>("isPlaying"))
		        {
			        SongMusicPlayer.SetPaused(false);
		        }

		        SongMusicPlayer.GetPlaybackState(out var state);
		        __instance.song.isPlaying = state == PLAYBACK_STATE.PLAYING;
		        Rhythm.RhythmController.SongInfo song = __instance.song;
		        __instance.song.maxCombo = ((song.maxCombo < song.combo) ? song.combo : song.maxCombo);
		        JeffBezosController.prevAccuracy = (song.accuracy = __instance.score.accuracyScore);
		        JeffBezosController.prevMaxCombo = song.maxCombo;
		        JeffBezosController.prevScore = (song.score = (int) __instance.score.totalScore);
		        JeffBezosController.prevMiss = song.missCount;
		        JeffBezosController.prevBarely = song.barelyCount;
		        JeffBezosController.prevOk = song.okCount;
		        JeffBezosController.prevGood = song.goodCount;
		        JeffBezosController.prevGreat = song.greatCount;
		        JeffBezosController.prevPerfect = song.perfectCount;
		        JeffBezosController.prevAvgLatency =
			        song.summedPreciseAccuracy / (float) song.numHitsWithPreciseAccuracy;
		        JeffBezosController.prevSongTitle = __instance.parser.beatmap.metadata.title + "\n" +
		                                            __instance.parser.beatmap.metadata.version;
		        if (song.over && !song.isPlaying)
		        {
			        if (__instance.waitBeforePausingSeconds <= 0f)
			        {
				        if (__instance.pauseWhenFinished)
				        {
					        JeffBezosController.paused = true;
				        }
				        else if (__instance.loadLevelWhenFinished != "")
				        {
					        LevelManager.LoadLevel(__instance.loadLevelWhenFinished);
				        }
			        }
			        else if (!__instance.songSource.isPlaying)
			        {
				        __instance.waitBeforePausingSeconds -= Time.deltaTime;
			        }

			        return;
		        }

		        if (song.health <= 0f && !__instance.noFail && !song.gameOver)
		        {
			        song.gameOver = true;
			        __instance.waitBeforePausingSeconds = 3f;
			        JeffBezosController.SetTimeScale(0f, 1f);
		        }

		        if (song.gameOver)
		        {
			        __instance.waitBeforePausingSeconds -= Time.unscaledDeltaTime;
			        if (__instance.waitBeforePausingSeconds <= 0f)
			        {
				        JeffBezosController.paused = true;
			        }
		        }
	        }
        }

    }
}
