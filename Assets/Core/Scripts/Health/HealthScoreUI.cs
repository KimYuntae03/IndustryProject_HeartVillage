using System;
using TMPro;
using UnityEngine;

public sealed class HealthScoreUI : MonoBehaviour
{
    [SerializeField] private GameObject popupRoot;
    [SerializeField] private TMP_Text[] categoryLabels;
    [SerializeField] private UnityEngine.UI.Image[] categoryFrames;
    [SerializeField] private TMP_Text overallLabel;
    [SerializeField] private UnityEngine.UI.Image weatherPlaceholder;

    // UI row order is independent of the category order used by the score calculator.
    private readonly HealthCategory[] rowCategories =
    {
        HealthCategory.Exercise, HealthCategory.Breathing, HealthCategory.Diet, HealthCategory.Medication
    };
    private readonly string[] names = { "운동", "호흡", "식단", "복약" };
    private readonly Color32[] gaugeColors =
    {
        new Color32(155, 205, 240, 255),
        new Color32(165, 215, 170, 255),
        new Color32(245, 220, 140, 255),
        new Color32(240, 165, 165, 255)
    };
    private readonly HealthScoreGauge[] gauges = new HealthScoreGauge[4];
    private bool initialized;
    private DateTime displayedDate;
    private float nextDateCheck;

    private void Awake()
    {
        if (popupRoot == null || overallLabel == null || weatherPlaceholder == null ||
            categoryLabels == null || categoryLabels.Length != 4 ||
            categoryFrames == null || categoryFrames.Length != 4 ||
            Array.Exists(categoryLabels, item => item == null) || Array.Exists(categoryFrames, item => item == null))
        {
            Debug.LogError("HealthScoreUI의 통합 점수 UI 연결을 확인해 주세요.", this);
            enabled = false;
            return;
        }

        popupRoot.SetActive(false);
        for (int i = 0; i < 4; i++)
        {
            var frame = categoryFrames[i];
            var label = categoryLabels[i];
            Style(label, 38, TextAlignmentOptions.Left);
            label.rectTransform.anchoredPosition = new Vector2(0, 62);
            label.rectTransform.sizeDelta = new Vector2(550, 52);

            var track = MakeRect("ScoreFill", frame.transform);
            Stretch(track, new Vector2(8, 6));
            gauges[i] = track.gameObject.AddComponent<HealthScoreGauge>();
            gauges[i].raycastTarget = false;

            // Render the original transparent frame on top of the fill, preserving its border.
            var border = MakeRect("ScoreBorder", frame.transform);
            Stretch(border, Vector2.zero);
            var borderImage = border.gameObject.AddComponent<UnityEngine.UI.Image>();
            borderImage.sprite = frame.sprite;
            borderImage.color = frame.color;
            borderImage.type = frame.type;
            borderImage.raycastTarget = false;

        }

        weatherPlaceholder.enabled = false;
        Style(overallLabel, 50.4f, TextAlignmentOptions.Center);
        overallLabel.rectTransform.sizeDelta = new Vector2(470, 100);
        overallLabel.rectTransform.anchoredPosition = new Vector2(0, -200);
        initialized = true;
    }

    private void OnEnable()
    {
        HealthScoreData.Changed += Refresh;
        RefreshDay();
    }

    private void OnDisable() => HealthScoreData.Changed -= Refresh;

    private void Update()
    {
        if (Time.unscaledTime < nextDateCheck) return;
        nextDateCheck = Time.unscaledTime + 0.5f;
        if (displayedDate != DateTime.Today) RefreshDay();
    }

    private void OnApplicationFocus(bool focused)
    {
        if (focused && isActiveAndEnabled) RefreshDay();
    }

    private void RefreshDay()
    {
        HealthScoreData.RefreshMedication(DateTime.Today);
        Refresh();
    }

    private void Refresh()
    {
        if (!initialized) return;
        displayedDate = DateTime.Today;
        var snapshot = HealthScoreData.GetSnapshot(displayedDate);
        for (int i = 0; i < 4; i++)
        {
            var score = snapshot.Categories[(int)rowCategories[i]];
            categoryLabels[i].text = $"{names[i]}: {score.Score:0} 점";
            gauges[i].SetScore(score.Score, gaugeColors[i]);
        }
        overallLabel.text = $"통합점수: {snapshot.HealthIndex:0} 점";
    }

    private static void Style(TMP_Text label, float size, TextAlignmentOptions alignment)
    {
        label.fontSize = size;
        label.fontStyle |= FontStyles.Bold;
        label.enableAutoSizing = false;
        label.color = Color.black;
        label.alignment = alignment;
        label.raycastTarget = false;
    }

    private static RectTransform MakeRect(string name, Transform parent)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.gameObject.layer = parent.gameObject.layer;
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        return rect;
    }

    private static void Stretch(RectTransform rect, Vector2 inset)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = inset;
        rect.offsetMax = -inset;
    }
}
