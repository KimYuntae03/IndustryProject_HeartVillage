using UnityEngine;
using UnityEngine.UI;

public class BalloonController : MonoBehaviour
{
    public BreathingManager breathingManager;

    private RectTransform rectTransform;
    private float minSize = 100f;
    private float maxSize = 300f;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (breathingManager == null) return;

        float amplitude = breathingManager.GetCurrentAmplitude();
        float t = Mathf.InverseLerp(0f, 0.4f, amplitude);
        float size = Mathf.Lerp(minSize, maxSize, t);

        rectTransform.sizeDelta = new Vector2(size, size);
    }
}