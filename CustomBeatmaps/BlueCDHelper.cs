using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CustomBeatmaps
{
    // Optionally make the CD when playing custom songs a custom color.
    public static class BlueCDHelper
    {
        private static readonly Dictionary<Color, Material> _alternativeTextMaterial = new Dictionary<Color, Material>();
        private static readonly int TextShaderColor = Shader.PropertyToID("_UnderlayColor");

        public static void ApplyCDColor(Color color)
        {
            // Add some color to pause menu
            var pauseMenu = Object.FindObjectOfType<PauseMenu>();
            if (pauseMenu != null)
            {
                var record = pauseMenu.transform.GetChild(0);

                // Set back color
                var back = pauseMenu.transform.parent.GetChild(0);
                var backImage = back.GetComponent<Image>();
                if (backImage != null) backImage.color = color;

                // Set text outline color
                foreach (var menuText in pauseMenu.transform.GetComponentsInChildren<TMP_Text>(true))
                    // Don't set the inner record material info.
                    if (menuText.transform.parent != record)
                    {
                        if (!_alternativeTextMaterial.ContainsKey(color))
                        {
                            _alternativeTextMaterial[color] = new Material(menuText.fontSharedMaterial);
                            _alternativeTextMaterial[color].SetColor(TextShaderColor, color);
                        }

                        menuText.fontSharedMaterial = _alternativeTextMaterial[color];
                        menuText.material = _alternativeTextMaterial[color];
                    }

                // Set record color
                var recordImage = record.GetComponent<Image>();
                recordImage.color = color;

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