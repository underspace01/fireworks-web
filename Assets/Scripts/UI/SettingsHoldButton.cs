using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Fireworks
{
    /// <summary>短いタップと長押しを分け、設定の誤操作を防ぐ。</summary>
    public sealed class SettingsHoldButton : UnityEngine.UI.Button
    {
        private float _holdDuration;
        private SettingsIconGraphic _icon;
        private Action _onHold;
        private Func<bool> _shouldShowProgress;
        private Coroutine _holdRoutine;
        private int? _pointerId;
        private int? _releasedPointerId;
        private bool _suppressClick;

        public void Initialize(float holdDuration, SettingsIconGraphic icon, Action onHold, Func<bool> shouldShowProgress)
        {
            _holdDuration = Mathf.Max(0.1f, holdDuration);
            _icon = icon;
            _onHold = onHold;
            _shouldShowProgress = shouldShowProgress;
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left ||
                !IsActive() || !IsInteractable() || _pointerId.HasValue) return;
            base.OnPointerDown(eventData);
            _pointerId = eventData.pointerId;
            _releasedPointerId = null;
            _suppressClick = false;
            _holdRoutine = StartCoroutine(WaitForHold());
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            if (_pointerId != eventData.pointerId) return;
            _releasedPointerId = eventData.pointerId;
            CancelHold();
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            if (_pointerId != eventData.pointerId) return;
            _suppressClick = true;
            CancelHold();
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (_releasedPointerId != eventData.pointerId) return;
            _releasedPointerId = null;
            if (!_suppressClick) base.OnPointerClick(eventData);
        }

        protected override void OnDisable()
        {
            _suppressClick = true;
            CancelHold();
            base.OnDisable();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus) return;
            _suppressClick = true;
            CancelHold();
        }

        private IEnumerator WaitForHold()
        {
            // 時間停止中も操作できるよう、実時間で長押しを判定する。
            float startedAt = Time.unscaledTime;
            // 閉じる操作では進捗を出さず、開くための長押しだけを案内する。
            bool showProgress = _shouldShowProgress == null || _shouldShowProgress();
            while (Time.unscaledTime - startedAt < _holdDuration)
            {
                if (_icon != null && showProgress) _icon.HoldProgress = (Time.unscaledTime - startedAt) / _holdDuration;
                yield return null;
            }

            // 指を離した際のクリックを抑止し、長押しで打上げが重ならないようにする。
            _suppressClick = true;
            _holdRoutine = null;
            if (_icon != null) _icon.HoldProgress = 0f;
            _onHold?.Invoke();
        }

        private void CancelHold()
        {
            if (_holdRoutine != null) StopCoroutine(_holdRoutine);
            _holdRoutine = null;
            _pointerId = null;
            if (_icon != null) _icon.HoldProgress = 0f;
        }
    }
}
