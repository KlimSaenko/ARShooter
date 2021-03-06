using UnityEngine;
using UnityEngine.UI;
using System;

namespace Game.UI
{
    [RequireComponent(typeof(Animator), typeof(Selectable))]
    public class UIButton : MonoBehaviour
    {
        [SerializeField] private protected Animator buttonAnimator;

        private protected virtual int Type => 0;

        private Toggle _toggle;
        private protected Toggle Toggle
        {
            get
            {
                if (_toggle == null && TryGetComponent<Toggle>(out var toggle)) return toggle;

                return _toggle;
            }
            private set => _toggle = value;
        }

        internal event Action<int> PressedAction;

        private protected static readonly int isActiveId = Animator.StringToHash("Is active");
        public virtual void OnValueChanged(bool value)
        {
            buttonAnimator.SetBool(isActiveId, value);
            
            if (value) OnButtonPressed();
        }

        private void OnEnable()
        {
            if (transform.parent.TryGetComponent<ToggleGroup>(out var toggleGroup))
            {
                Toggle.group = toggleGroup;

                if (!toggleGroup.AnyTogglesOn()) Toggle.isOn = true;
            }
        }

        private protected void OnButtonPressed()
        {
            PressedAction?.Invoke(Type);
        }
    }
}
