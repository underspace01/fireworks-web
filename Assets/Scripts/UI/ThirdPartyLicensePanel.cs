using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fireworks
{
    /// <summary>第三者ライセンス本文を読み込み、スクロール表示する。</summary>
    internal sealed class ThirdPartyLicensePanel
    {
        private const string LICENSE_FILE_NAME = "ThirdPartyLicenses.txt";

        private readonly GameObject _root;

        public ThirdPartyLicensePanel(RectTransform canvasRoot, float rowHeight, TMP_FontAsset font)
        {
            _root = CreatePanel(canvasRoot, rowHeight, font);
            _root.SetActive(false);
        }

        public bool IsVisible => _root.activeSelf;

        public void Show()
        {
            _root.transform.SetAsLastSibling();
            _root.SetActive(true);
        }
        public void Hide() => _root.SetActive(false);

        private GameObject CreatePanel(RectTransform canvasRoot, float rowHeight, TMP_FontAsset font)
        {
            GameObject panelObject = RuntimeUiFactory.CreateObject("Third Party Licenses", canvasRoot);
            RectTransform panel = panelObject.GetComponent<RectTransform>();
            panel.anchorMin = new Vector2(0.5f, 0.5f);
            panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(900f, 700f);
            panelObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.9f);

            VerticalLayoutGroup layout = panelObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 20, 20);
            layout.spacing = 12f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            RuntimeUiFactory.CreateText("Title", panel, "サードパーティーライセンス", 24, TextAlignmentOptions.MidlineLeft, font)
                .gameObject.AddComponent<LayoutElement>().preferredHeight = rowHeight;
            CreateScrollView(panel, font);
            RuntimeUiFactory.CreateButton(panel, "閉じる", rowHeight * 1.2f, font, Hide);
            return panelObject;
        }

        private void CreateScrollView(RectTransform parent, TMP_FontAsset font)
        {
            GameObject scrollObject = RuntimeUiFactory.CreateObject("License Scroll View", parent);
            scrollObject.AddComponent<LayoutElement>().flexibleHeight = 1f;
            ScrollRect scrollRect = scrollObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;

            GameObject viewportObject = RuntimeUiFactory.CreateObject("Viewport", scrollObject.transform);
            RectTransform viewport = viewportObject.GetComponent<RectTransform>();
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = Vector2.zero;
            viewport.offsetMax = Vector2.zero;
            viewportObject.AddComponent<Image>().color = new Color(0.05f, 0.07f, 0.12f, 1f);
            viewportObject.AddComponent<Mask>().showMaskGraphic = true;

            GameObject contentObject = RuntimeUiFactory.CreateObject("Content", viewport);
            RectTransform content = contentObject.GetComponent<RectTransform>();
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.sizeDelta = Vector2.zero;
            VerticalLayoutGroup contentLayout = contentObject.AddComponent<VerticalLayoutGroup>();
            contentLayout.padding = new RectOffset(16, 16, 16, 16);
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = true;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            TextMeshProUGUI licenseText = RuntimeUiFactory.CreateText(
                "License Text", content, LoadLicenseText(), 16, TextAlignmentOptions.TopLeft, font);
            licenseText.textWrappingMode = TextWrappingModes.Normal;
            licenseText.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scrollRect.viewport = viewport;
            scrollRect.content = content;
        }

        private string LoadLicenseText()
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, LICENSE_FILE_NAME);
            if (File.Exists(filePath))
            {
                return File.ReadAllText(filePath);
            }

            return "ライセンス情報を読み込めませんでした。配布パッケージにライセンス本文が含まれていることを確認してください。";
        }
    }
}
