using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CustomBeatmaps
{
    // Optionally make the CD when playing custom songs blue.
    public static class BlueCDHelper
    {
        private static readonly Color PAUSE_MENU_CUSTOM_RECORD_COLOR = new(0.2901961f, 0.6709353f, 0.9999f, 1);
        private static Material _alternativeTextMaterial;
        private static readonly int TextShaderColor = Shader.PropertyToID("_UnderlayColor");

        public static void ApplyBlueCD()
        {
            // Add some color to pause menu
            var pauseMenu = Object.FindObjectOfType<PauseMenu>();
            if (pauseMenu != null)
            {
                var record = pauseMenu.transform.GetChild(0);

                // Set back color
                var back = pauseMenu.transform.parent.GetChild(0);
                var backImage = back.GetComponent<Image>();
                if (backImage != null) backImage.color = PAUSE_MENU_CUSTOM_RECORD_COLOR;

                // Set text outline color
                foreach (var menuText in pauseMenu.transform.GetComponentsInChildren<TMP_Text>(true))
                    // Don't set the inner record material info.
                    if (menuText.transform.parent != record)
                    {
                        if (_alternativeTextMaterial == null)
                        {
                            _alternativeTextMaterial = new Material(menuText.fontSharedMaterial);
                            _alternativeTextMaterial.SetColor(TextShaderColor, PAUSE_MENU_CUSTOM_RECORD_COLOR);
                        }

                        menuText.fontSharedMaterial = _alternativeTextMaterial;
                        menuText.material = _alternativeTextMaterial;
                    }

                // Set record color
                var recordImage = record.GetComponent<Image>();
                recordImage.color = PAUSE_MENU_CUSTOM_RECORD_COLOR;

                /*
                // Add a silly piece of text saying it's not actually Unbeatable official OST
                GameObject sillyTextExtraObject = new GameObject();
                sillyTextExtraObject.transform.SetParent(record);
                sillyTextExtraObject.AddComponent<RectTransform>();
                var sillyTextExtra = sillyTextExtraObject.AddComponent<TMP_Text>();
                sillyTextExtra.text = "NOT";
                sillyTextExtra.color = Color.black;
                sillyTextExtra.faceColor = Color.black;
                ((RectTransform) sillyTextExtra.transform).anchoredPosition = new Vector3(108.4f, -97.9f, -8.42f);
                */
            }
        }
    }
}