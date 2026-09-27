using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Fireworks
{
    /// <summary>花火パラメーターの選択と打上操作を管理する。</summary>
    internal sealed class FireworkParameterPanel
    {
        private readonly FireworkLauncher _launcher;
        private readonly FireworkSettings _settings;
        private readonly Vector2 _panelSize;
        private readonly float _rowHeight;
        private readonly TMP_FontAsset _font;
        private readonly GameObject _root;
        private readonly UnityAction _onShowInfo;

        private TMP_InputField _textFireworkInput;
        private GameObject _changeColorRow;
        private GameObject _textFireworkRow;
        private int _selectedSizeIndex;
        private int _selectedStructureIndex;
        private int _selectedColorIndex;
        private int _selectedChangeColorIndex;

        public FireworkParameterPanel(
            RectTransform canvasRoot,
            FireworkLauncher launcher,
            FireworkSettings settings,
            Vector2 panelSize,
            float margin,
            float rowHeight,
            TMP_FontAsset font,
            UnityAction onShowInfo)
        {
            _launcher = launcher;
            _settings = settings;
            _panelSize = panelSize;
            _rowHeight = rowHeight;
            _font = font;
            _onShowInfo = onShowInfo;
            _root = CreatePanel(canvasRoot, margin);
            RectTransform = _root.GetComponent<RectTransform>();
        }

        public RectTransform RectTransform { get; }
        public bool IsVisible => _root.activeSelf;
        public string TextValue => _textFireworkInput != null ? _textFireworkInput.text : string.Empty;

        public void SetVisible(bool isVisible)
        {
            _root.SetActive(isVisible);
        }

        private GameObject CreatePanel(RectTransform canvasRoot, float margin)
        {
            GameObject panelObject = RuntimeUiFactory.CreateObject("Panel", canvasRoot);
            RectTransform panel = panelObject.GetComponent<RectTransform>();
            panel.anchorMin = new Vector2(0f, 1f);
            panel.anchorMax = new Vector2(0f, 1f);
            panel.pivot = new Vector2(0f, 1f);
            panel.anchoredPosition = new Vector2(margin, -margin);
            panel.sizeDelta = _panelSize;
            panelObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.7f);

            panel.anchorMin = Vector2.zero;
            panel.anchorMax = Vector2.one;
            panel.offsetMin = new Vector2(margin, margin + _rowHeight * 2f);
            panel.offsetMax = new Vector2(-margin, -margin);
            RectTransform content = RuntimeUiFactory.CreateScrollContent(panel);
            VerticalLayoutGroup layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 16, 16);
            layout.spacing = 8f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            panel = content;
            RuntimeUiFactory.CreateText("Title", panel, "花火パラメーター", 24, TextAlignmentOptions.MidlineLeft, _font)
                .gameObject.AddComponent<LayoutElement>().preferredHeight = _rowHeight;
            CreateSelectionRow(panel, "サイズ", CreateSizeOptions(), value => _selectedSizeIndex = value, out _);
            CreateSelectionRow(panel, "構造", CreateStructureOptions(), SetStructure, out _);
            CreateSelectionRow(panel, "基本色", CreateColorOptions(), value => _selectedColorIndex = value, out _);
            CreateSelectionRow(panel, "変化色", CreateColorOptions(), value => _selectedChangeColorIndex = value, out _changeColorRow);
            _textFireworkInput = CreateTextInputRow(panel, out _textFireworkRow);
            RuntimeUiFactory.CreateButton(panel, "指定して打ち上げる", _rowHeight * 1.2f, _font, LaunchSelectedFirework);
            RuntimeUiFactory.CreateButton(panel, "ランダムで打ち上げる", _rowHeight * 1.2f, _font, () => _launcher.LaunchRandomFirework(TextValue));
            RuntimeUiFactory.CreateButton(panel, "アプリ情報", _rowHeight * 1.2f, _font, _onShowInfo);
            RefreshStructureVisibility();
            return panelObject;
        }

        private void CreateSelectionRow(
            RectTransform panel,
            string label,
            List<string> options,
            UnityAction<int> onValueChanged,
            out GameObject rowObject)
        {
            rowObject = RuntimeUiFactory.CreateObject(label + "Row", panel);
            RectTransform row = rowObject.GetComponent<RectTransform>();
            rowObject.AddComponent<LayoutElement>().preferredHeight = _rowHeight;
            HorizontalLayoutGroup rowLayout = rowObject.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 8f;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandHeight = true;

            TextMeshProUGUI labelText = RuntimeUiFactory.CreateText("Label", row, label, 18, TextAlignmentOptions.MidlineLeft, _font);
            labelText.gameObject.AddComponent<LayoutElement>().preferredWidth = _panelSize.x * 0.3f;
            TMP_Dropdown dropdown = CreateDropdown(row, options);
            dropdown.onValueChanged.AddListener(onValueChanged);
        }

        private TMP_InputField CreateTextInputRow(RectTransform panel, out GameObject rowObject)
        {
            rowObject = RuntimeUiFactory.CreateObject("TextFireworkRow", panel);
            RectTransform row = rowObject.GetComponent<RectTransform>();
            rowObject.AddComponent<LayoutElement>().preferredHeight = _rowHeight * 1.8f;
            VerticalLayoutGroup rowLayout = rowObject.AddComponent<VerticalLayoutGroup>();
            rowLayout.spacing = 4f;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = true;
            rowLayout.childForceExpandHeight = true;

            TextMeshProUGUI label = RuntimeUiFactory.CreateText("Label", row, "文字（最大6文字）", 16, TextAlignmentOptions.MidlineLeft, _font);
            label.gameObject.AddComponent<LayoutElement>().preferredHeight = _rowHeight * 0.55f;

            GameObject inputObject = RuntimeUiFactory.CreateObject("Input", row);
            inputObject.AddComponent<LayoutElement>().preferredHeight = _rowHeight;
            inputObject.AddComponent<Image>().color = new Color(0.2f, 0.3f, 0.5f, 1f);
            TMP_InputField inputField = inputObject.AddComponent<TMP_InputField>();
            inputField.characterLimit = 6;
            inputField.lineType = TMP_InputField.LineType.SingleLine;

            TextMeshProUGUI inputText = RuntimeUiFactory.CreateText("Text", inputObject.GetComponent<RectTransform>(), string.Empty, 18, TextAlignmentOptions.MidlineLeft, _font);
            SetInputTextOffsets(inputText);
            inputField.textComponent = inputText;

            TextMeshProUGUI placeholder = RuntimeUiFactory.CreateText("Placeholder", inputObject.GetComponent<RectTransform>(), "入力してください", 16, TextAlignmentOptions.MidlineLeft, _font);
            SetInputTextOffsets(placeholder);
            placeholder.color = new Color(1f, 1f, 1f, 0.5f);
            inputField.placeholder = placeholder;
            return inputField;
        }

        private void SetInputTextOffsets(TextMeshProUGUI text)
        {
            RectTransform transform = text.GetComponent<RectTransform>();
            transform.offsetMin = new Vector2(12f, 0f);
            transform.offsetMax = new Vector2(-12f, 0f);
            text.raycastTarget = false;
        }

        private TMP_Dropdown CreateDropdown(RectTransform parent, List<string> options)
        {
            GameObject dropdownObject = RuntimeUiFactory.CreateObject("Dropdown", parent);
            dropdownObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
            dropdownObject.AddComponent<Image>().color = new Color(0.2f, 0.3f, 0.5f, 1f);
            TMP_Dropdown dropdown = dropdownObject.AddComponent<TMP_Dropdown>();

            TextMeshProUGUI caption = RuntimeUiFactory.CreateText("Label", dropdownObject.GetComponent<RectTransform>(), string.Empty, 18, TextAlignmentOptions.MidlineLeft, _font);
            caption.rectTransform.offsetMin = new Vector2(12f, 0f);
            caption.rectTransform.offsetMax = new Vector2(-28f, 0f);
            TextMeshProUGUI arrow = RuntimeUiFactory.CreateText("Arrow", dropdownObject.GetComponent<RectTransform>(), "▼", 16, TextAlignmentOptions.Midline, _font);
            arrow.rectTransform.anchorMin = new Vector2(1f, 0f);
            arrow.rectTransform.anchorMax = new Vector2(1f, 1f);
            arrow.rectTransform.pivot = new Vector2(1f, 0.5f);
            arrow.rectTransform.sizeDelta = new Vector2(28f, 0f);
            arrow.rectTransform.anchoredPosition = Vector2.zero;

            dropdown.captionText = caption;
            dropdown.template = CreateDropdownTemplate(dropdownObject.GetComponent<RectTransform>(), out TextMeshProUGUI itemText);
            dropdown.itemText = itemText;
            dropdown.AddOptions(options);
            return dropdown;
        }

        private RectTransform CreateDropdownTemplate(RectTransform dropdownTransform, out TextMeshProUGUI itemText)
        {
            GameObject templateObject = RuntimeUiFactory.CreateObject("Template", dropdownTransform);
            RectTransform template = templateObject.GetComponent<RectTransform>();
            template.anchorMin = new Vector2(0f, 0f);
            template.anchorMax = new Vector2(1f, 0f);
            template.pivot = new Vector2(0.5f, 1f);
            template.sizeDelta = new Vector2(0f, _rowHeight * 6f);
            templateObject.AddComponent<Image>().color = new Color(0.1f, 0.15f, 0.25f, 1f);
            var scroll = templateObject.AddComponent<UnityEngine.UI.ScrollRect>();
            scroll.horizontal = false;
            scroll.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped;
            var viewport = RuntimeUiFactory.CreateObject("Viewport", template).GetComponent<RectTransform>();
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = Vector2.zero;
            viewport.offsetMax = Vector2.zero;
            viewport.gameObject.AddComponent<UnityEngine.UI.Image>();
            viewport.gameObject.AddComponent<UnityEngine.UI.Mask>().showMaskGraphic = false;

            GameObject contentObject = RuntimeUiFactory.CreateObject("Content", viewport);
            RectTransform content = contentObject.GetComponent<RectTransform>();
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = Vector2.one;
            content.pivot = new Vector2(0.5f, 1f);
            content.sizeDelta = new Vector2(0f, _rowHeight);
            // 選択肢の高さはTMP_Dropdownが更新するため、ContentSizeFitterは付けない。
            scroll.viewport = viewport;
            scroll.content = content;

            GameObject itemObject = RuntimeUiFactory.CreateObject("Item", content);
            RectTransform item = itemObject.GetComponent<RectTransform>();
            item.anchorMin = new Vector2(0f, 1f);
            item.anchorMax = new Vector2(1f, 1f);
            item.pivot = new Vector2(0.5f, 1f);
            item.sizeDelta = new Vector2(0f, _rowHeight);
            Image itemBackground = itemObject.AddComponent<Image>();
            itemBackground.color = new Color(0.25f, 0.35f, 0.55f, 1f);
            itemObject.AddComponent<Toggle>().targetGraphic = itemBackground;
            itemText = RuntimeUiFactory.CreateText("Item Label", item, string.Empty, 18, TextAlignmentOptions.MidlineLeft, _font);
            itemText.rectTransform.offsetMin = new Vector2(12f, 0f);
            itemText.rectTransform.offsetMax = new Vector2(-12f, 0f);
            templateObject.SetActive(false);
            return template;
        }

        private List<string> CreateSizeOptions()
        {
            List<string> options = new List<string>(_settings.SizeCount);
            for (int index = 0; index < _settings.SizeCount; index++) options.Add(_settings.GetSize(index).DisplayName);
            return options;
        }

        private List<string> CreateStructureOptions()
        {
            List<string> options = new List<string>(_settings.EffectCount);
            for (int index = 0; index < _settings.EffectCount; index++) options.Add(_settings.GetEffect(index).DisplayName);
            return options;
        }

        private List<string> CreateColorOptions()
        {
            List<string> options = new List<string>(_settings.ColorCount);
            for (int index = 0; index < _settings.ColorCount; index++) options.Add(_settings.GetColor(index).DisplayName);
            return options;
        }

        private void SetStructure(int value)
        {
            _selectedStructureIndex = value;
            RefreshStructureVisibility();
        }

        private void RefreshStructureVisibility()
        {
            FireworkTypeDefinition effect = _settings.GetEffect(_selectedStructureIndex);
            _changeColorRow.SetActive(effect != null && !effect.IsTextFirework && effect.AllowsChangeColor);
            _textFireworkRow.SetActive(effect != null && effect.IsTextFirework);
        }

        private void LaunchSelectedFirework()
        {
            FireworkTypeDefinition effect = _settings.GetEffect(_selectedStructureIndex);
            if (effect == null) return;
            if (effect.IsTextFirework)
            {
                string text = _textFireworkInput.text.Trim();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    _launcher.LaunchTextFirework(text, _selectedSizeIndex, _selectedColorIndex);
                }
                return;
            }

            _launcher.LaunchFirework(_selectedSizeIndex, _selectedStructureIndex, _selectedColorIndex, _selectedChangeColorIndex);
        }
    }
}
