using UnityEngine;

// Small pixel icons rendered as UI geometry, without font-dependent emoji glyphs.
[RequireComponent(typeof(CanvasRenderer))]
public sealed class HealthWeatherGraphic : UnityEngine.UI.MaskableGraphic
{
    private HealthWeather weather;

    public void SetWeather(HealthWeather value)
    {
        weather = value;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper mesh)
    {
        mesh.Clear();
        Rect rect = GetPixelAdjustedRect();
        float unit = Mathf.Min(rect.width, rect.height) / 20f;
        if (unit <= 0f) return;
        Vector2 origin = rect.center - Vector2.one * unit * 10f;
        for (int y = 0; y < 20; y++)
        for (int x = 0; x < 20; x++)
        {
            Color32 pixel = Pixel(x, y);
            if (pixel.a == 0) continue;
            float left = origin.x + x * unit;
            float bottom = origin.y + y * unit;
            int start = mesh.currentVertCount;
            mesh.AddVert(new Vector3(left, bottom), pixel, Vector2.zero);
            mesh.AddVert(new Vector3(left, bottom + unit), pixel, Vector2.zero);
            mesh.AddVert(new Vector3(left + unit, bottom + unit), pixel, Vector2.zero);
            mesh.AddVert(new Vector3(left + unit, bottom), pixel, Vector2.zero);
            mesh.AddTriangle(start, start + 1, start + 2);
            mesh.AddTriangle(start, start + 2, start + 3);
        }
    }

    private Color32 Pixel(int x, int y)
    {
        Color32 result = new Color32(0, 0, 0, 0);
        if (weather == HealthWeather.Sunny || weather == HealthWeather.Clear)
        {
            int cx = weather == HealthWeather.Sunny ? 10 : 13;
            int cy = weather == HealthWeather.Sunny ? 10 : 14;
            int dx = Mathf.Abs(x - cx), dy = Mathf.Abs(y - cy);
            if (dx * dx + dy * dy <= 18) result = new Color32(255, 199, 65, 255);
            else if ((dx <= 1 && dy >= 6 && dy <= 8) || (dy <= 1 && dx >= 6 && dx <= 8) ||
                     (dx >= 5 && dx <= 6 && dy >= 5 && dy <= 6))
                result = new Color32(238, 157, 54, 255);
        }
        if (weather != HealthWeather.Sunny)
        {
            bool cloud = (x >= 3 && x <= 16 && y >= 7 && y <= 10) ||
                Circle(x, y, 6, 11, 3) || Circle(x, y, 10, 13, 4) || Circle(x, y, 14, 11, 3);
            if (cloud) result = weather == HealthWeather.Rainy
                ? new Color32(112, 129, 151, 255) : new Color32(192, 207, 216, 255);
            if (weather == HealthWeather.Rainy && y >= 2 && y <= 5 &&
                (x == 5 + (y - 2) / 2 || x == 10 + (y - 2) / 2 || x == 15 + (y - 2) / 2))
                result = new Color32(89, 164, 216, 255);
        }
        return result;
    }

    private static bool Circle(int x, int y, int cx, int cy, int radius) =>
        (x - cx) * (x - cx) + (y - cy) * (y - cy) <= radius * radius;
}
