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
        [HarmonyPrefix]
        static void UpdatePre(Rhythm.RhythmController __instance, ref bool __runOriginal)
        {
	        // Manually do stuff after the fact if we are playing.
	        // This is the big kahoona, literally ended up just
	        // copy+paste the relevant code and replace fmod music with the custom music player.
	        if (IsUserSong(__instance))
	        {
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
		        if ((double) Time.timeScale <= 0.0)
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
		        __instance.song.countdownPhase = __instance.countdownPhase;
		        PLAYBACK_STATE state;
		        SongMusicPlayer.GetPlaybackState(out state);
		        __instance.song.isPlaying = state == PLAYBACK_STATE.PLAYING;
		        __instance.song.maxCombo = __instance.song.maxCombo < __instance.song.combo ? __instance.song.combo : __instance.song.maxCombo;
		        JeffBezosController.prevAccuracy = __instance.song.accuracy = __instance.score.accuracyScore;
		        JeffBezosController.prevMaxCombo = __instance.song.maxCombo;
		        JeffBezosController.prevScore = __instance.song.score = (int) __instance.score.totalScore;
		        JeffBezosController.prevMiss = __instance.song.missCount;
		        JeffBezosController.prevBarely = __instance.song.barelyCount;
		        JeffBezosController.prevOk = __instance.song.okCount;
		        JeffBezosController.prevGood = __instance.song.goodCount;
		        JeffBezosController.prevGreat = __instance.song.greatCount;
		        JeffBezosController.prevPerfect = __instance.song.perfectCount;
		        JeffBezosController.prevAvgLatency = __instance.song.summedPreciseAccuracy / (float) __instance.song.numHitsWithPreciseAccuracy;
		        JeffBezosController.prevSongTitle = __instance.parser.beatmap.metadata.title + "\n" + __instance.parser.beatmap.metadata.version;
		        if (__instance.song.over && !__instance.song.isPlaying)
		        {
			        if ((double) __instance.waitBeforePausingSeconds <= 0.0)
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
			        if ((double) __instance.song.health <= 0.0 && !__instance.noFail && !__instance.song.gameOver)
			        {
				        __instance.song.gameOver = true;
				        __instance.waitBeforePausingSeconds = 3f;
				        JeffBezosController.SetTimeScale(0.0f, 1f);
			        }
			        if (!__instance.song.gameOver)
				        return;
			        __instance.waitBeforePausingSeconds -= Time.unscaledDeltaTime;
			        if ((double) __instance.waitBeforePausingSeconds > 0.0)
				        return;
			        JeffBezosController.paused = true;
		        }
	        }
        }

    }
}
