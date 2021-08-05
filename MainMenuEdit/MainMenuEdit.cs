using HarmonyLib;
using BepInEx;
using UnityEngine;
using System.Reflection;

namespace MainMenuEdit
{
	public static class MainMenuPatch
	{

		[HarmonyPatch(typeof(WhiteLabelMainMenu), "LevelSelect")]
		[HarmonyPrefix]
		private static void EditPost(WhiteLabelMainMenu __instance, 
									 ref bool __runOriginal, 
									 ref Rhythm.BeatmapParser ___parser, ref bool ___walkmanSideA, ref bool ___updateText,
									 ref Rewired.Player ___rewired, 
									 ref WrapCounter ___songsInc, ref string[] ___difficulties, ref Rhythm.BeatmapIndex ___beatmapIndex,
									 ref WrapCounter ___diffInc, ref bool ___playing, ref bool ___reload)
		{

			MethodInfo chooseCam = __instance.GetType().GetMethod("ChooseCamera", BindingFlags.NonPublic | BindingFlags.Instance);

			MethodInfo sbpGetter = __instance.GetType().GetProperty("selectedBeatmapPath", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(nonPublic: true);
			MethodInfo selGetter = __instance.GetType().GetProperty("selectedSong", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(nonPublic: true);

			string lvlSelState = __instance.GetType().GetField("lvlselectState", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance).ToString();
			string acty = __instance.GetType().GetNestedType("LevelSelectState", BindingFlags.NonPublic).GetField("ACTIVE").GetValue(__instance).ToString();
			string play = __instance.GetType().GetNestedType("LevelSelectState", BindingFlags.NonPublic).GetField("PLAY").GetValue(__instance).ToString();

			if (___updateText)
			{
				if (___walkmanSideA)
				{
					__instance.SongNameA.text = ___parser.beatmap.metadata.title + " (" + ___parser.beatmap.metadata.version + ")";
					__instance.DifficultyNameA.text = "Mapper: " + ___parser.beatmap.metadata.creator;
					__instance.HighScoreA.text = "Artist: " + ___parser.beatmap.metadata.artist;
				}
				else
				{
					__instance.SongNameB.text = ___parser.beatmap.metadata.title + " (" + ___parser.beatmap.metadata.version + ")";
					__instance.DifficultyNameB.text = "Mapper: " + ___parser.beatmap.metadata.creator;
					__instance.HighScoreB.text = "Artist: " + ___parser.beatmap.metadata.artist;
				}
			}
			if (lvlSelState == acty)
			{
				if (___rewired.GetNegativeButtonDown("Horizontal"))
				{
					if (___walkmanSideA)
					{
						__instance.Walkman.Play("WalkmanLevelSelectSwitchCassetteALeft", -1, 0f);
					}
					else
					{
						__instance.Walkman.Play("WalkmanLevelSelectSwitchCassetteBLeft", -1, 0f);
					}
					___songsInc.Decr(true);
					___difficulties = ___beatmapIndex.DifficultiesBySong((string)selGetter.Invoke(__instance, null));
					___diffInc = new WrapCounter(___difficulties.Length);
					___playing = false;
					___reload = true;
				}
				if (___rewired.GetButtonDown("Horizontal"))
				{
					if (___walkmanSideA)
					{
						__instance.Walkman.Play("WalkmanLevelSelectSwitchCassetteARight", -1, 0f);
					}
					else
					{
						__instance.Walkman.Play("WalkmanLevelSelectSwitchCassetteBRight", -1, 0f);
					}
					___songsInc.Incr(true);
					___difficulties = ___beatmapIndex.DifficultiesBySong((string)selGetter.Invoke(__instance, null));
					___diffInc = new WrapCounter(___difficulties.Length);
					___playing = false;
					___reload = true;
				}
				if (___rewired.GetNegativeButtonDown("Vertical") || ___rewired.GetButtonDown("Interact Left"))
				{
					if (___walkmanSideA)
					{
						__instance.Walkman.Play("WalkmanLevelSelectFlipCassetteACW");
						___walkmanSideA = false;
					}
					else
					{
						__instance.Walkman.Play("WalkmanLevelSelectFlipCassetteBCW");
						___walkmanSideA = true;
					}
					___diffInc.Decr(true);
					___reload = true;
				}
				if (___rewired.GetButtonDown("Vertical") || ___rewired.GetButtonDown("Interact Right"))
				{
					if (___walkmanSideA)
					{
						__instance.Walkman.Play("WalkmanLevelSelectFlipCassetteACCW");
						___walkmanSideA = false;
					}
					else
					{
						__instance.Walkman.Play("WalkmanLevelSelectFlipCassetteBCCW");
						___walkmanSideA = true;
					}
					___diffInc.Incr(true);
					___reload = true;
				}
				if (___rewired.GetButtonDown("Interact"))
				{
					if (___walkmanSideA)
					{
						__instance.Walkman.Play("WalkmanCloseCaseA");
					}
					else
					{
						__instance.Walkman.Play("WalkmanCloseCaseB");
					}
					__instance.beatmapOptionsMenu.gameObject.SetActive(true);

					chooseCam.Invoke(__instance, new object[] { __instance.playCam });
				}
			}
			else if (lvlSelState == play)
			{
				__instance.beatmapOptionsMenu.UpdateMenu();
				if (__instance.beatmapOptionsMenu.menuCounter > 0)
				{
					chooseCam.Invoke(__instance, new object[] { __instance.beatmapOptionsCam });
				}
				else
				{
					if (__instance.beatmapOptionsMenu.leftFocus)
					{
						chooseCam.Invoke(__instance, new object[] { __instance.playCam });
					}
					if (___rewired.GetButtonDown("Cancel") || ___rewired.GetButtonDown("Back"))
					{
						if (___walkmanSideA)
						{
							__instance.Walkman.Play("WalkmanOpenCaseA");
						}
						else
						{
							__instance.Walkman.Play("WalkmanOpenCaseB");
						}
						__instance.beatmapOptionsMenu.gameObject.SetActive(false);
						chooseCam.Invoke(__instance, new object[] { __instance.levelSelectCam });
					}
					if (___rewired.GetButtonDown("Interact"))
					{
						JeffBezosController.canPause = true;
						try
						{
							LevelManager.LoadBeatmap("OpeningMemory", (string)sbpGetter.Invoke(__instance, null), 0, false);
						} catch { return; }
					}
				}
			}
			if (___reload)
			{
				try
				{
					___parser.beatmapPath = (string)sbpGetter.Invoke(__instance, null);
				} catch { return; }
				___parser.ParseBeatmap();
				___reload = false;
			}
			if (!___playing)
			{
				MethodInfo playSongPre = __instance.GetType().GetMethod("PlaySongPreview", BindingFlags.NonPublic | BindingFlags.Instance);
				playSongPre.Invoke(__instance, new object[] { ___parser.audioKey });

				___playing = true;
			}




			__runOriginal = false;
		}
	}



	[BepInPlugin("janethefromstatedog.unbeatable.mainmenuedit", "Main Menu Edit", "0.0.0.0")]
    public class MainMenuEdit : BaseUnityPlugin
    {

		private void Awake()
        {
            Debug.Log("Doing main menu patch!");
            Harmony.CreateAndPatchAll(typeof(MainMenuPatch));
        }
    }
}