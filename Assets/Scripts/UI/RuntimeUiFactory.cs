using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Fireworks
{
    internal static class RuntimeUiFactory
    {
        public static GameObject CreateObject(string objectName, Transform parent)
        {
            GameObject uiObject = new GameObject(objectName, typeof(RectTransform));
            uiObject.transform.SetParent(parent, false);
            return uiObject;
        }

        public static TextMeshProUGUI CreateText(
            string objectName,
            RectTransform parent,
            string value,
            int fontSize,
            TextAlignmentOptions alignment,
            TMP_FontAsset font)
        {
            GameObject textObject = CreateObject(objectName, parent);
            RectTransform textTransform = textObject.GetComponent<RectTransform>();
            textTransform.anchorMin = Vector2.zero;
            textTransform.anchorMax = Vector2.one;
            textTransform.offsetMin = Vector2.zero;
            textTransform.offsetMax = Vector2.zero;
            TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
            text.font = font;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.text = value;
            return text;
        }

        // ビューポートだけを固定し、内容の高さはレイアウトから求める。
        public static RectTransform CreateScrollContent(RectTransform parent)
        {
            var scroll = parent.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();
            scroll.horizontal = false;
            scroll.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped;
            var viewport = CreateObject("Viewport", parent).GetComponent<RectTransform>();
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = Vector2.zero;
            viewport.offsetMax = Vector2.zero;
            viewport.gameObject.AddComponent<UnityEngine.UI.Image>().color = Color.white;
            viewport.gameObject.AddComponent<UnityEngine.UI.Mask>().showMaskGraphic = false;
            var content = CreateObject("Content", viewport).GetComponent<RectTransform>();
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = Vector2.one;
            content.pivot = new Vector2(0.5f, 1f);
            content.sizeDelta = Vector2.zero;
            content.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>().verticalFit =
                UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = viewport;
            scroll.content = content;
            return content;
        }

        public static RectTransform CreateButton(
            RectTransform parent,
            string label,
            float height,
            TMP_FontAsset font,
            UnityAction action)
        {
            GameObject buttonObject = CreateObject(label, parent);
            buttonObject.AddComponent<Image>().color = new Color(0.2f, 0.3f, 0.5f, 1f);
            Button button = buttonObject.AddComponent<Button>();
            button.onClick.AddListener(action);
            CreateText("Text", buttonObject.GetComponent<RectTransform>(), label, 18, TextAlignmentOptions.Midline, font);
            buttonObject.AddComponent<LayoutElement>().preferredHeight = height;
            return buttonObject.GetComponent<RectTransform>();
        }
    }
}
