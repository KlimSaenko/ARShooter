using System;
using System.Collections.Generic;
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

        private static bool _nextSceneLoading;

        private void Awake()
        {
            _currentScene = SceneManager.GetActiveScene();

            _loadingPanelAnimator = loadingPanelAnimator;
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Update()
        {
            if (_currentAsyncLoading == null) return;

            if (!_currentAsyncLoading.isDone)
            {
                loadingImage.fillAmount = _currentAsyncLoading.progress;
            }
        }

        internal static GameScene GetCurrentScene() => _nextScene;

        private static readonly int loadingPanelInId = Animator.StringToHash("Loading Panel In");
        private static readonly int loadingId = Animator.StringToHash("Loading");
        internal static void LoadScene(GameScene gameScene)
        {
            if (_nextSceneLoading) return;
            
            if (gameScene == GameScene.MainMenu && _sceneQueue.Count > 0)
            {
                gameScene = _sceneQueue.First.Value;
                _sceneQueue.RemoveFirst();
            }

            _nextScene = gameScene;
            _nextSceneLoading = true;
            NotificationManager.SkipNotification();

            if (_loadingPanelAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash == loadingPanelInId) LoadSceneDirectly(gameScene);
            else _loadingPanelAnimator.SetBool(loadingId, true);
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (!_nextSceneLoading) return;

            SceneManager.UnloadSceneAsync(_currentScene.buildIndex, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
            _currentScene = scene;
            
            Time.timeScale = 1;
            _loadingPanelAnimator.SetBool(loadingId, false);

            _nextSceneLoading = false;
        }

        /// <summary>
        /// Used by animation trigger
        /// </summary>
        private void LoadScene()
        {
            if (!_nextSceneLoading) return;

            _currentAsyncLoading = SceneManager.LoadSceneAsync((int)_nextScene, LoadSceneMode.Additive);
            _currentAsyncLoading.allowSceneActivation = true;
        }

        private static void LoadSceneDirectly(GameScene gameScene)
        {
            if (!_nextSceneLoading) return;

            _currentAsyncLoading = SceneManager.LoadSceneAsync((int)gameScene, LoadSceneMode.Additive);
            _currentAsyncLoading.allowSceneActivation = true;
        }

        private void AsyncLoadingCompleted(AsyncOperation asyncOperation)
        {
            SceneManager.SetActiveScene(_currentScene);
            Time.timeScale = 1;
            _loadingPanelAnimator.SetBool(loadingId, false);

            _nextSceneLoading = false;

            //_currentAsyncLoading.completed -= AsyncLoadingCompleted;
        }

        private static readonly LinkedList<GameScene> _sceneQueue = new();
        internal static void AddQueueScene(GameScene gameScene)
        {
            _sceneQueue.AddLast(gameScene);
        }

        internal static event Action<bool> ApplicationPauseAction;

        private void OnApplicationPause(bool pause)
        {
            ApplicationPauseAction?.Invoke(pause);
        }
    }

    internal enum GameScene
    {
        MainMenu,
        Test,
        ShowWeapon
    }
}
