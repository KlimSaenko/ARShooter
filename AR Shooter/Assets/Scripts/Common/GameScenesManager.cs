using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Managers
{
    public class GameScenesManager : MonoBehaviour
    {
        [SerializeField] private Image loadingImage;
        [SerializeField] private Animator loadingPanelAnimator;

        private static Scene _currentScene;
        private static GameScene _nextScene;
        private static AsyncOperation _currentAsyncLoading;
        private static Animator _loadingPanelAnimator;

        private static bool _nextSceneSetted;

        private void Awake()
        {
            DontDestroyOnLoad(transform.parent);

            _currentScene = SceneManager.GetActiveScene();
            SceneManager.sceneLoaded += OnSceneLoaded;
            _loadingPanelAnimator = loadingPanelAnimator;
        }

        private void Update()
        {
            if (_currentAsyncLoading == null) return;

            if (!_currentAsyncLoading.isDone)
            {
                loadingImage.fillAmount = _currentAsyncLoading.progress;
            }
        }

        private static readonly int loadingId = Animator.StringToHash("Loading");
        internal static void LoadScene(GameScene gameScene)
        {
            if (_nextSceneSetted) return;

            _nextScene = gameScene;
            _nextSceneSetted = true;
            _loadingPanelAnimator.SetBool(loadingId, true);
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (scene.buildIndex == _currentScene.buildIndex) return;

            SceneManager.UnloadSceneAsync(_currentScene.buildIndex, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
            _currentScene = scene;
        }

        private void LoadScene()
        {
            if (!_nextSceneSetted) return;

            _currentAsyncLoading = SceneManager.LoadSceneAsync((int)_nextScene, LoadSceneMode.Additive);
            _currentAsyncLoading.completed += (op) =>
            {
                SceneManager.SetActiveScene(_currentScene);
                _loadingPanelAnimator.SetBool(loadingId, false);
                _nextSceneSetted = false;
            };
        }
    }

    internal enum GameScene
    {
        MainMenu,
        Test
    }
}
