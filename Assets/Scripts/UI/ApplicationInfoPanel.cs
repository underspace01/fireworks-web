using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Fireworks
{
    /// <summary>アプリ情報と終了操作を表示する。</summary>
    internal sealed class ApplicationInfoPanel
    {
        private readonly GameObject _root;
        private readonly ThirdPartyLicensePanel _licensePanel;

        public ApplicationInfoPanel(
            RectTransform canvasRoot,
            float rowHeight,
            TMP_FontAsset font,
            UnityAction onClose)
        {
            _licensePanel = new ThirdPartyLicensePanel(canvasRoot, rowHeight, font);
            _root = CreatePanel(canvasRoot, rowHeight, font, onClose);
            _root.SetActive(false);
        }

        public bool IsVisible => _root.activeSelf;
        public bool IsLicenseVisible => _licensePanel.IsVisible;

        public void Show()
        {
            _licensePanel.Hide();
            _root.SetActive(true);
        }

        public void Hide() => _root.SetActive(false);
        public void CloseLicense() => _licensePanel.Hide();

        public void HideAll()
        {
            _licensePanel.Hide();
            _root.SetActive(false);
        }

        private GameObject CreatePanel(
            RectTransform canvasRoot,
            float rowHeight,
            TMP_FontAsset font,
            UnityAction onClose)
        {
            GameObject panelObject = RuntimeUiFactory.CreateObject("Application Info", canvasRoot);
            RectTransform panel = panelObject.GetComponent<RectTransform>();
            panel.anchorMin = new Vector2(0.5f, 0.5f);
            panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(460f, 300f);
            panelObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.9f);

            VerticalLayoutGroup layout = panelObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 20, 20);
            layout.spacing = 12f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            RuntimeUiFactory.CreateText("Title", panel, "アプリ情報", 24, TextAlignmentOptions.MidlineLeft, font)
                .gameObject.AddComponent<LayoutElement>().preferredHeight = rowHeight;
            RuntimeUiFactory.CreateText("Version", panel, "バージョン " + Application.version, 18, TextAlignmentOptions.MidlineLeft, font)
                .gameObject.AddComponent<LayoutElement>().preferredHeight = rowHeight;
            RuntimeUiFactory.CreateButton(panel, "サードパーティーライセンス", rowHeight * 1.2f, font, _licensePanel.Show);
            RuntimeUiFactory.CreateButton(panel, "アプリを終了", rowHeight * 1.2f, font, QuitApplication);
            RuntimeUiFactory.CreateButton(panel, "閉じる", rowHeight * 1.2f, font, onClose);
            return panelObject;
        }

        private void QuitApplication()
        {
            Application.Quit();
        }
    }
}
