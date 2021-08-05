using UnityEngine;
using TMPro;

public class FPS : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fpsText;

    private void Awake()
    {
        Application.targetFrameRate = 60;
    }

    private int _accumulator = 0;
    private int _counter = 0;
    private float _timer = 0f;

    private void Update()
    {
        _accumulator++;
        _timer += Time.deltaTime;
        if (_timer >= 1)
        {
            _timer = 0;
            _counter = _accumulator;
            _accumulator = 0;
        }
        fpsText.text = _counter.ToString();
    }
}
