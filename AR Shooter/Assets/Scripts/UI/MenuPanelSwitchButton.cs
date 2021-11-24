using UnityEngine;

namespace Game.UI
{
    public class MenuPanelSwitchButton : UIButton
    {
        [SerializeField] private MenuPanel buttonType;

        private protected override int Type { get => (int)buttonType; }

        private void Awake()
        {
            if (buttonType == MenuPanel.Home)
            {
                Toggle.isOn = true;
            }
        }

        public override void OnValueChanged(bool value)
        {
            buttonAnimator.SetBool(isActiveId, value);

            if (value)
            {
                OnButtonPressed();
            }
        }
    }
}
