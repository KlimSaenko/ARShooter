using UnityEngine;

namespace Game.UI
{
    public class MenuPanelSwitchButton : UIButton
    {
        [SerializeField] private MenuPanel buttonType;

        private protected override int Type => (int)buttonType;

        private void Start()
        {
            if (buttonType == MenuPanel.Home)
            {
                Toggle.isOn = true;
            }
        }
    }
}
