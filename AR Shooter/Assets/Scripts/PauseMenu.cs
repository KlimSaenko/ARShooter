using Game.Managers;
using System;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private Animator pauseMenuAnimator;
    [SerializeField] private Animator gameEndAnimator;

    private static bool _isPaused = false;
    internal static bool IsPaused
    {
        get => _isPaused;
        set
        {
            PauseAction?.Invoke(value);
            Time.timeScale = value ? 0 : 1;
            _isPaused = value;
        }
    }

    internal static event Action<bool> PauseAction;

    private static readonly int gameEndId = Animator.StringToHash("Game End");
    private static readonly int panelFadeInId = Animator.StringToHash("Pause Panel In");
    private static readonly int panelFadeOutId = Animator.StringToHash("Pause Panel Out");

    public void Pause(bool pause)
    {
        IsPaused = pause;

        pauseMenuAnimator.Play(pause ? panelFadeInId : panelFadeOutId);
    }

    private static readonly int exitId = Animator.StringToHash("Exit");
    public void Exit(bool value)
    {
        pauseMenuAnimator.SetBool(exitId, value);
    }

    public void Restart()
    {
        GameScenesManager.LoadScene(GameScenesManager.GetCurrentScene());
    }

    public void Quit()
    {
        GameScenesManager.LoadScene(GameScene.MainMenu);
    }

    internal void GameEnd()
    {
        IsPaused = true;
        gameEndAnimator.Play(gameEndId);
    }
}
