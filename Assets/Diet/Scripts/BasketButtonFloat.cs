using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public sealed class BasketButtonFloat : MonoBehaviour
{
    [SerializeField, Min(0f)] private float amplitude = 7f;
    [SerializeField, Min(0.1f)] private float period = 2.8f;

    private RectTransform buttonRect;
    private Vector2 restPosition;
    private float startTime;

    private void OnEnable()
    {
        buttonRect = (RectTransform)transform;
        restPosition = buttonRect.anchoredPosition;
        startTime = Time.unscaledTime;
    }

    private void Update()
    {
        float phase = (Time.unscaledTime - startTime) * (2f * Mathf.PI / period);
        buttonRect.anchoredPosition = restPosition + Vector2.up * (amplitude * Mathf.Sin(phase));
    }

    private void OnDisable()
    {
        buttonRect.anchoredPosition = restPosition;
    }
}
