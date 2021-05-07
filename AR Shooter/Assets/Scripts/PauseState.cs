using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseState : MonoBehaviour
{
    [SerializeField] private RectTransform pauseMenu;

    private float translationTime;
    private Vector2 menuPosTo;

    internal static bool isPaused = false;

    private void Update()
    {
        if (IsTranslating()) Translation();
    }

    public void Pause(bool pause)
    {
        if (pause)
        {
            pauseMenu.gameObject.SetActive(true);
        }
        isPaused = pause;

        translationTime = 0.19f - translationTime;
        menuPosTo = pause ? new Vector2(0, 0) : new Vector2(850, 0);
    }

    private void Translation()
    {
        pauseMenu.anchoredPosition = Vector2.Lerp(pauseMenu.anchoredPosition, menuPosTo, Time.deltaTime / translationTime);
        translationTime -= Time.deltaTime;
        if (translationTime < Time.deltaTime)
        {
            pauseMenu.anchoredPosition = menuPosTo;
            if (!isPaused) pauseMenu.gameObject.SetActive(false);
        }
    }

    private bool IsTranslating()
    {
        return translationTime > 0;
    }

    public void Quit()
    {
        isPaused = false;

        SceneManager.LoadScene(0);
    }
}
