using UnityEngine;
using UnityEngine.UI;

// A rounded track clipped horizontally, keeping the left edge fixed at every amount.
[RequireComponent(typeof(CanvasRenderer))]
public sealed class SodiumGaugeGraphic : MaskableGraphic
{
    private float fraction;

    public void SetSodium(long amount, int capacity)
    {
        fraction = Mathf.Clamp01((float)amount / Mathf.Max(1, capacity));
        if (amount < 500) color = new Color32(100, 190, 123, 255);
        else if (amount < 1000) color = new Color32(242, 207, 88, 255);
        else if (amount < 1500) color = new Color32(245, 163, 88, 255);
        else color = new Color32(240, 119, 119, 255);
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        Rect rect = GetPixelAdjustedRect();
        if (fraction <= 0 || rect.width <= 0 || rect.height <= 0) return;
        float radius = Mathf.Min(rect.height * 0.5f, rect.width * 0.5f);
        float end = rect.xMin + rect.width * fraction;
        const int slices = 40;
        for (int i = 0; i < slices; i++)
        {
            float bottom = rect.yMin + rect.height * i / slices;
            float top = rect.yMin + rect.height * (i + 1) / slices;
            float bottomInset = Inset(bottom - rect.center.y, radius);
            float topInset = Inset(top - rect.center.y, radius);
            float leftBottom = Mathf.Min(rect.xMin + bottomInset, end);
            float leftTop = Mathf.Min(rect.xMin + topInset, end);
            float rightBottom = Mathf.Min(rect.xMax - bottomInset, end);
            float rightTop = Mathf.Min(rect.xMax - topInset, end);
            int start = mesh.currentVertCount;
            mesh.AddVert(new Vector3(leftBottom, bottom), color, Vector2.zero);
            mesh.AddVert(new Vector3(leftTop, top), color, Vector2.zero);
            mesh.AddVert(new Vector3(rightTop, top), color, Vector2.zero);
            mesh.AddVert(new Vector3(rightBottom, bottom), color, Vector2.zero);
            mesh.AddTriangle(start, start + 1, start + 2);
            mesh.AddTriangle(start, start + 2, start + 3);
        }
    }

    private static float Inset(float y, float radius)
    {
        return radius - Mathf.Sqrt(Mathf.Max(0, radius * radius - y * y));
    }
}
