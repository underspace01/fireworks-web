using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Fireworks
{
    /// <summary>入力デバイスを直接参照せず、タップとクリックの位置を伝える。</summary>
    public class PointerPositionButton : UnityEngine.UI.Button
    {
        public event Action<Vector2> PositionClicked;

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || !IsActive() || !IsInteractable()) return;
            PositionClicked?.Invoke(eventData.position);
            base.OnPointerClick(eventData);
        }
    }
}
