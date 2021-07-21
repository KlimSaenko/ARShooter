using UnityEngine;
using TMPro;

public class FPS : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fpsText;

    private void Awake()
    {
        Application.targetFrameRate = 60;
    }

    private int accumulator = 0;
    private int counter = 0;
    private float timer = 0f;

    private void Update()
    {
        accumulator++;
        timer += Time.deltaTime;
        if (timer >= 1)
        {
            timer = 0;
            counter = accumulator;
            accumulator = 0;
        }
        fpsText.text = counter.ToString();
    }
}
