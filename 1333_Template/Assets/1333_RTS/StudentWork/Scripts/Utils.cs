using UnityEngine;

public static class Utils
{
    public static Rect GetScreenRect(Vector2 screenPosition1, Vector2 screenPosition2)        // create rectangle from any two corners for multiple unit selection
    {
 
        float xMin = Mathf.Min(screenPosition1.x, screenPosition2.x);
        float xMax = Mathf.Max(screenPosition1.x, screenPosition2.x);
        float yMin = Mathf.Min(screenPosition1.y, screenPosition2.y);
        float yMax = Mathf.Max(screenPosition1.y, screenPosition2.y);
        return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
    }
}