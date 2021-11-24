using System.Collections.Generic;
using UnityEngine;

namespace Game.UI
{
    public class TopPanelManager : MonoBehaviour
    {
        [Header("Panel List")]
        [SerializeField] private Animator homePanelAnimator;
        [SerializeField] private Animator inventoryPanelAnimator;

        [Space]
        [SerializeField] private MenuPanelSwitchButton[] menuPanelSwitchButtons;

        private static readonly Dictionary<MenuPanel, Animator> _panelAnimator = new();

        private static MenuPanel _currentPanel = MenuPanel.Empty;

        private static readonly int panelFadeIn = Animator.StringToHash("MM Panel In");
        private static readonly int panelFadeOut = Animator.StringToHash("MM Panel Out");

        private void Awake()
        {
            if (!_panelAnimator.ContainsKey(MenuPanel.Home))
                _panelAnimator.Add(MenuPanel.Home, homePanelAnimator);

            if (!_panelAnimator.ContainsKey(MenuPanel.Inventory))
                _panelAnimator.Add(MenuPanel.Inventory, inventoryPanelAnimator);

            foreach (var switchButton in menuPanelSwitchButtons)
            {
                switchButton.PressedAction += PanelOpen;
            }
        }

        internal static void PanelFade(MenuPanel menuPanel, bool activate)
        {
            if (!activate) _currentPanel = MenuPanel.Empty;

            if (_panelAnimator.TryGetValue(menuPanel, out var animator))
            {
                animator.Play(activate ? panelFadeIn : panelFadeOut);
            }
        }

        private static void PanelOpen(MenuPanel menuPanel)
        {
            if (_currentPanel == menuPanel) return;

            PanelFade(_currentPanel, false);
            PanelFade(menuPanel, true);

            _currentPanel = menuPanel;
        }

        private static void PanelOpen(int menuPanel) =>
            PanelOpen((MenuPanel)menuPanel);

        [Header("Modal Windows")]
        [SerializeField] private Animator exitWindowAnimator;

        public void ExitWindow(bool value)
        {
            exitWindowAnimator.Play(value ? panelFadeIn : panelFadeOut);
        }
    }

    internal enum MenuPanel
    {
        Home,
        Inventory,
        Empty
    }
}