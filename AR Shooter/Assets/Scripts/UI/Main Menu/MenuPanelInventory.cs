using UnityEngine;

namespace Game.UI
{
    public class MenuPanelInventory : MenuPanelBase
    {
        [SerializeField] private Animator[] menuPanelAnimators;
        [SerializeField] private MenuShowWeapon menuShowWeapon;

        [Space]
        [SerializeField] private MenuPanelSwitchButton[] menuPanelSwitchButtons;

        private Animator _prevPanelAnimator;

        private protected override void OnEnable()
        {
            base.OnEnable();

            foreach (var switchButton in menuPanelSwitchButtons)
            {
                switchButton.PressedAction += LocalPanelOpen;
            }
        }

        private protected override void OnDisable()
        {
            base.OnDisable();

            foreach (var switchButton in menuPanelSwitchButtons)
            {
                switchButton.PressedAction -= LocalPanelOpen;
            }
        }

        private void LocalPanelOpen(int type)
        {
            if (_prevPanelAnimator != null) _prevPanelAnimator.Play(panelFadeOut);

            _prevPanelAnimator = menuPanelAnimators[type - 1];
            _prevPanelAnimator.Play(panelFadeIn);
        }
        
        internal override void OpenPanel(bool open)
        {
            base.OpenPanel(open);

            menuShowWeapon.SetActivePanel(open);

            //if (PanelType == MenuPanel.Profile) menuShowWeapon.SetFavoriteWeapon();
            //if (PanelType == MenuPanel.Inventory) Debug.Break();
        }
    }
}
