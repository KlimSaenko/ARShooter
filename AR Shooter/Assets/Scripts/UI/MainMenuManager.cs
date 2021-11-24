using UnityEngine;
using Game.UI;

namespace Game.Managers
{
    public class MainMenuManager : MonoBehaviour
    {
        [SerializeField] private MenuButton testSceneButton;
        
        private void Awake()
        {
            testSceneButton.PressedAction += (type) => TestScene();
        }

        private void TestScene()
        {
            TopPanelManager.PanelFade(MenuPanel.Home, false);

            GameScenesManager.LoadScene(GameScene.Test);
        }
    }
}

