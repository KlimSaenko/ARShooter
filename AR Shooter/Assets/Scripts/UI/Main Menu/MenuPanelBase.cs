using UnityEngine;

namespace Game.UI
{
    [RequireComponent(typeof(Animator))]
    public class MenuPanelBase : MonoBehaviour
    {
        [SerializeField] private MenuPanel panelType;

        private protected virtual MenuPanel PanelType => panelType;

        private Animator _panelAnimator;
        internal Animator PanelAnimator
        {
            get 
            {
                if (_panelAnimator == null) _panelAnimator = GetComponent<Animator>();
                return _panelAnimator;
            }
        }

        private protected virtual void OnEnable()
        {
            MenuPanelsManager.OpenPanelAction += OpenPanel;
        }

        private protected virtual void OnDisable()
        {
            MenuPanelsManager.OpenPanelAction -= OpenPanel;
        }

        private protected static readonly int panelFadeIn = Animator.StringToHash("MM Panel In");
        private protected static readonly int panelFadeOut = Animator.StringToHash("MM Panel Out");

        private void OpenPanel(MenuPanel menuPanel, bool open)
        {
            if (PanelType != menuPanel || PanelType != MenuPanelsManager.CurrentPanel) return;

            OpenPanel(open);
        }

        internal virtual void OpenPanel(bool open)
        {
            PanelAnimator.Play(open ? panelFadeIn : panelFadeOut);
        }

        //public void OpenPanel(bool open)
        //{
        //    PanelAnimator.Play(open ? panelFadeIn : panelFadeOut);
        //}
    }
}
