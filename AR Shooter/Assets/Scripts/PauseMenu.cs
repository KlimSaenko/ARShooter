using Game.Managers;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private Animator pauseMenuAnimator;
    [SerializeField] private Animator gameEndAnimator;

    internal static bool isPaused = false;

    private static readonly int gameEnd = Animator.StringToHash("Game End");
    private static readonly int panelFadeIn = Animator.StringToHash("Pause Panel In");
    private static readonly int panelFadeOut = Animator.StringToHash("Pause Panel Out");

    public void Pause(bool pause)
    {
        isPaused = pause;

        pauseMenuAnimator.Play(pause ? panelFadeIn : panelFadeOut);
    }

    private static readonly int exit = Animator.StringToHash("Exit");

    public void Exit(bool value)
    {
        pauseMenuAnimator.SetBool(exit, value);
    }

    public void Quit()
    {
        GameScenesManager.LoadScene(GameScene.MainMenu);
    }

    internal void GameEnd()
    {
        gameEndAnimator.Play(gameEnd);
    }
}
