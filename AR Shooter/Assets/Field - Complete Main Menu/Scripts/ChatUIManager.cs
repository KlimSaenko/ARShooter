using System.Collections.Generic;
using UnityEngine;

namespace Game.UI
{
    public class ChatUIManager : MonoBehaviour
    {
        [Header("PANEL LIST")]
        [SerializeField] private List<Animator> panelsAnimators;

        [Header("BUTTON LIST")]
        [SerializeField] private List<UIButton> buttons;

        // [Header("PANEL ANIMS")]
        private static readonly string panelFadeIn = "Chat Panel In";
        private static readonly string panelFadeOut = "Chat Panel Out";

        private GameObject currentPanel;
        private GameObject nextPanel;

        private GameObject currentButton;
        private GameObject nextButton;

        [Header("SETTINGS")]
        public int currentPanelIndex = 0;
        private int currentButtonlIndex = 0;

        private Animator currentPanelAnimator;
        private Animator nextPanelAnimator;

        private Animator currentButtonAnimator;
        private Animator nextButtonAnimator;

        private void Awake()
        {
            foreach (var button in buttons)
            {
                button.PressedAction += PanelAnim;
            }
        }

        //void Start()
        //{
        //    currentButton = buttons[currentPanelIndex];
        //    currentButtonAnimator = currentButton.GetComponent<Animator>();
        //    currentButtonAnimator.Play(buttonFadeIn);

        //    currentPanel = panels[currentPanelIndex];
        //    currentPanelAnimator = currentPanel.GetComponent<Animator>();
        //    currentPanelAnimator.Play(panelFadeIn);
        //}

        private int _prevPanel;

        private static readonly int panelFadeInId = Animator.StringToHash(panelFadeIn);
        private static readonly int panelFadeOutId = Animator.StringToHash(panelFadeOut);
        public void PanelAnim(int newPanel)
        {
            panelsAnimators[_prevPanel].Play(panelFadeOutId);
            panelsAnimators[newPanel].Play(panelFadeInId);

            _prevPanel = newPanel;
        }
    }
}