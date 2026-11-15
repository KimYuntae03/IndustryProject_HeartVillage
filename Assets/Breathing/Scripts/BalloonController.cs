using UnityEngine;
using UnityEngine.UI;

public class BalloonController : MonoBehaviour
{
    public BreathingManager breathingManager;
    public Sprite[] balloonFrames;

    private RectTransform rectTransform;
    private Image image;

    private Color idleColor = new Color(0.6f, 0.8f, 1f);
    private Color breathingColor = new Color(0.2f, 0.9f, 0.4f);
    private Color successColor = new Color(1f, 0.85f, 0.1f);

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
    }

    void Update()
    {
        if (breathingManager == null) return;

        float amplitude = breathingManager.GetCurrentAmplitude();
        float t = Mathf.InverseLerp(0f, 0.4f, amplitude);

        if (balloonFrames != null && balloonFrames.Length > 0)
        {
            int frameIndex = Mathf.Clamp(Mathf.FloorToInt(t * balloonFrames.Length), 0, balloonFrames.Length - 1);
            image.sprite = balloonFrames[frameIndex];
        }

        if (image != null)
        {
            if (breathingManager.IsMissionSuccess())
                image.color = successColor;
            else if (breathingManager.IsInGreenZone())
                image.color = Color.Lerp(idleColor, breathingColor, t);
            else
                image.color = idleColor;
        }
    }
}