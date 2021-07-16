using HarmonyLib;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils.ReflectionHelper;

namespace TrainStationFreeroam.Patches
{
    public static class WhiteLabelMainMenuPatches
    {
        [HarmonyPatch(typeof(WhiteLabelMainMenu), "Start")]
        [HarmonyPrefix]
        private static void StartPre(WhiteLabelMainMenu __instance)
        {
            // Copy one option to make a NEW menu option
            Transform topLayer = __instance.transform.Find("MainMenu").Find("TopLayer");
            GameObject exampleMenuItem = topLayer.Find("Play").gameObject;

            UISelectionButton newItem = GameObject.Instantiate(exampleMenuItem, topLayer).GetComponent<UISelectionButton>();
            newItem.GetComponent<TMP_Text>().text = $"STATION <size=50%><#0000FFFF>\n{Mod.Instance.Version} </color></size>";
            // Move to the right
            newItem.transform.localPosition += Vector3.forward * 0.8f;

            // Add to top layer + update menu size
            __instance.TopLayerOptions.Add(newItem);
        }

        [HarmonyPatch(typeof(WhiteLabelMainMenu), "Start")]
        [HarmonyPostfix]
        private static void StartPost(WhiteLabelMainMenu __instance)
        {
            // Add to "selectionInc" initialized in Start
            var selectionIncCopy = __instance.GetField<WrapCounter>("selectionInc");
            selectionIncCopy.count++;
            __instance.SetField("selectionInc",  selectionIncCopy);
        }


        [HarmonyPatch(typeof(WhiteLabelMainMenu), "MenuDefaultUpdate")]
        [HarmonyPostfix]
        private static void MenuDefaultUpdatePost(WhiteLabelMainMenu __instance)
        {
            if (__instance.GetField<WrapCounter>("selectionInc") == 3 &&
                __instance.GetField<Player>("rewired").GetButtonDown("Interact"))
            {
                Debug.Log("TrainStationFreeroam: Going to Train Scene");
                SceneManager.LoadScene(Mod.Instance.TrainSceneName);
            }
        }
    }
}
