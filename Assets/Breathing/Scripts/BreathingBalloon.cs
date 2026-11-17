using UnityEngine;
using TMPro;
using Image = UnityEngine.UI.Image;

public class BreathingBalloon : MonoBehaviour
{
    public BreathingManager breathingManager;
    public Image balloonImage;
    public Sprite[] balloonFrames;
    public TMP_Text holdCountText;

    [Header("Å©±â")]
    public float minScale = 0.6f;
    public float maxScale = 1.3f;
    public float scaleSpeed = 5f;

    [Header("»ö")]
    public Color idleColor = new Color(1f, 0.45f, 0.45f);
    public Color greenColor = new Color(0.2f, 0.8f, 0.2f);
    public float colorSpeed = 8f;

    private float shownLevel = 0f;

    void Update()
    {
        if (breathingManager == null || balloonImage == null) return;

        bool green = breathingManager.IsInGreenZone();
        float target = green ? 1f : breathingManager.GetBreathLevel();
        shownLevel = Mathf.Lerp(shownLevel, target, scaleSpeed * Time.deltaTime);

        float scale = Mathf.Lerp(minScale, maxScale, shownLevel);
        balloonImage.rectTransform.localScale = new Vector3(scale, scale, 1f);

        if (balloonFrames != null && balloonFrames.Length > 0)
        {
            int index = Mathf.Clamp(Mathf.RoundToInt(shownLevel * (balloonFrames.Length - 1)), 0, balloonFrames.Length - 1);
            balloonImage.sprite = balloonFrames[index];
        }

        Color targetColor = green ? greenColor : idleColor;
        balloonImage.color = Color.Lerp(balloonImage.color, targetColor, colorSpeed * Time.deltaTime);

        if (holdCountText != null)
        {
            bool showCount = green && !breathingManager.IsMissionSuccess();
            holdCountText.text = showCount ? Mathf.CeilToInt(breathingManager.GetHoldRemaining()).ToString() : "";
        }
    }
}