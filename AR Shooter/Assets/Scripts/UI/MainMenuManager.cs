using UnityEngine;
using Game.UI;

namespace Game.Managers
{
    public class MainMenuManager : MonoBehaviour
    {
        [SerializeField] private MenuButton testSceneButton;

        private void OnEnable()
        {
            testSceneButton.PressedAction += (type) => TestScene();
        }

        private void OnDisable()
        {
            testSceneButton.PressedAction -= (type) => TestScene();
        }

        private void TestScene()
        {
            MenuPanelsManager.PanelOpen(MenuPanel.Empty);

            GameScenesManager.LoadScene(GameScene.Test);
        }

        public void QuitGame()
        {
            Application.Quit();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.S)) Test.Instance.Ok();
        }
    }
}

