using UnityEngine;

[RequireComponent(typeof(CanvasRenderer))]
public sealed class HealthScoreGauge : UnityEngine.UI.MaskableGraphic
{
    private float fraction;

    public void SetScore(double score, Color fillColor)
    {
        fraction = Mathf.Clamp01((float)score / 100f);
        color = fillColor;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper mesh)
    {
        mesh.Clear();
        Rect rect = GetPixelAdjustedRect();
        if (fraction <= 0 || rect.width <= 0 || rect.height <= 0) return;
        float radius = Mathf.Min(rect.height, rect.width) * 0.5f;
        float end = rect.xMin + rect.width * fraction;
        const int slices = 32;
        for (int i = 0; i < slices; i++)
        {
            float bottom = rect.yMin + rect.height * i / slices;
            float top = rect.yMin + rect.height * (i + 1) / slices;
            float bottomInset = Inset(bottom - rect.center.y, radius);
            float topInset = Inset(top - rect.center.y, radius);
            int start = mesh.currentVertCount;
            mesh.AddVert(new Vector3(Mathf.Min(rect.xMin + bottomInset, end), bottom), color, Vector2.zero);
            mesh.AddVert(new Vector3(Mathf.Min(rect.xMin + topInset, end), top), color, Vector2.zero);
            mesh.AddVert(new Vector3(Mathf.Min(rect.xMax - topInset, end), top), color, Vector2.zero);
            mesh.AddVert(new Vector3(Mathf.Min(rect.xMax - bottomInset, end), bottom), color, Vector2.zero);
            mesh.AddTriangle(start, start + 1, start + 2);
            mesh.AddTriangle(start, start + 2, start + 3);
        }
    }

    private static float Inset(float y, float radius) =>
        radius - Mathf.Sqrt(Mathf.Max(0f, radius * radius - y * y));
}
