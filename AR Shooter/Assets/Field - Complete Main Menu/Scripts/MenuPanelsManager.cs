using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.UI
{
    public class MenuPanelsManager : MonoBehaviour
    {
        [Header("Panel List")]
        [SerializeField] private Animator homePanelAnimator;
        [SerializeField] private Animator multiplayerPanelAnimator;
        [SerializeField] private Animator inventoryPanelAnimator;
        [SerializeField] private Animator profilePanelAnimator;

        [Space]
        [SerializeField] private MenuPanelSwitchButton[] menuPanelSwitchButtons;

        private static Dictionary<MenuPanel, Animator> _panelAnimator = new();

        private static MenuPanel _currentPanel = MenuPanel.Empty;
        internal static MenuPanel CurrentPanel => _currentPanel;

        internal static event Action<MenuPanel, bool> OpenPanelAction;

        private static readonly int panelFadeIn = Animator.StringToHash("MM Panel In");
        private static readonly int panelFadeOut = Animator.StringToHash("MM Panel Out");

        private void Awake()
        {
            if (_panelAnimator.ContainsKey(MenuPanel.Home)) _panelAnimator[MenuPanel.Home] = homePanelAnimator;
            else _panelAnimator.Add(MenuPanel.Home, homePanelAnimator);

            if (_panelAnimator.ContainsKey(MenuPanel.Multiplayer)) _panelAnimator[MenuPanel.Multiplayer] = multiplayerPanelAnimator;
            else _panelAnimator.Add(MenuPanel.Multiplayer, multiplayerPanelAnimator);

            if (_panelAnimator.ContainsKey(MenuPanel.Inventory)) _panelAnimator[MenuPanel.Inventory] = inventoryPanelAnimator;
            else _panelAnimator.Add(MenuPanel.Inventory, inventoryPanelAnimator);

            if (_panelAnimator.ContainsKey(MenuPanel.Profile)) _panelAnimator[MenuPanel.Profile] = profilePanelAnimator;
            else _panelAnimator.Add(MenuPanel.Profile, profilePanelAnimator);
        }

        private void OnEnable()
        {
            foreach (var switchButton in menuPanelSwitchButtons)
            {
                switchButton.PressedAction += PanelOpen;
            }
        }

        private void OnDisable()
        {
            foreach (var switchButton in menuPanelSwitchButtons)
            {
                switchButton.PressedAction -= PanelOpen;
            }
        }

        private static void PanelFade(MenuPanel menuPanel, bool activate)
        {
            if (!activate) _currentPanel = MenuPanel.Empty;
            
            if (_panelAnimator.TryGetValue(menuPanel, out var animator))
            {
                animator.Play(activate ? panelFadeIn : panelFadeOut);
            }
        }

        internal static void PanelOpen(MenuPanel menuPanel)
        {
            if (_currentPanel == menuPanel) return;
            
            //PanelFade(_currentPanel, false);
            //PanelFade(menuPanel, true);
            OpenPanelAction?.Invoke(_currentPanel, false);
            _currentPanel = menuPanel;
            OpenPanelAction?.Invoke(menuPanel, true);
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
        Empty,
        Home,
        Multiplayer,
        Inventory,
        Profile
    }
}