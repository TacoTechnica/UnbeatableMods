using HarmonyLib;
using TMPro;

namespace CustomBeatmaps.Patches
{
    public static class MainMenuModVersionPatch
    {
        [HarmonyPatch(typeof(WhiteLabelMainMenu), "Start")]
        [HarmonyPrefix]
        private static void StartPost(WhiteLabelMainMenu __instance)
        {
            // Append MODDED to the intro text
            var titleParent = __instance.gameObject.transform.GetChild(1).GetChild(0);
            var versionNumberTransform = titleParent.Find("VersionNumber");
            var versionNumberText = versionNumberTransform.GetComponent<TMP_Text>();
            versionNumberText.text += $"\n<size=60%><#0000FFFF> CUSTOM BEATMAPS {Mod.Instance.Version} </color></size>";
            
        }
    }
}