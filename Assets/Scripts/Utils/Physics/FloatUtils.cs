using UnityEngine;

public static class FloatUtils
{
    public static float Map(this float x, float x1, float x2, float y1, float y2)
    {
        return (x - x1) / (x2 - x1) * (y2 - y1) + y1;
    }

    public static float Map(this float x, Vector2 r1, Vector2 r2)
    {
        return x.Map(r1.x, r1.y, r2.x, r2.y);
    }

    public static float MapClamped(this float x, float x1, float x2, float y1, float y2)
    {
        float mapped = x.Map(x1, x2, y1, y2);

        return Mathf.Clamp(mapped, y1, y2);
    }

    public static float MapClamped(this float x, Vector2 r1, Vector2 r2)
    {
        return x.MapClamped(r1.x, r1.y, r2.x, r2.y);
    }
}