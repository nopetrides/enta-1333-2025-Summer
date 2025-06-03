using System;
using UnityEngine;

/// <summary>
/// Handles drawing and tracking of the mouse drag selection box.
/// </summary>
public class UnitSelectionBox : MonoBehaviour
{
    private static Texture2D _selectionTexture;

    public Vector2 DragStart { get; private set; }
    public Vector2 DragEnd { get; private set; }
    public bool IsDragging { get; private set; }

    public float DragDistance => Vector2.Distance(DragStart, DragEnd);

    [NonSerialized] public float minDragSize;

    private void Awake()
    {
        if (_selectionTexture == null)
        {
            _selectionTexture = new Texture2D(1, 1);
            _selectionTexture.SetPixel(0, 0, Color.white);
            _selectionTexture.Apply();
        }
    }

    public void BeginDrag(Vector2 startPos)
    {
        DragStart = startPos;
        DragEnd = startPos;
        IsDragging = true;
    }

    public void UpdateDrag(Vector2 currentPos)
    {
        DragEnd = currentPos;
    }

    public void EndDrag(Vector2 endPos)
    {
        DragEnd = endPos;
        IsDragging = false;
    }

    public Rect GetScreenRect(Vector2 start, Vector2 end)
    {
        Vector2 p1 = new(start.x, Screen.height - start.y);
        Vector2 p2 = new(end.x, Screen.height - end.y);

        float xMin = Mathf.Min(p1.x, p2.x);
        float yMin = Mathf.Min(p1.y, p2.y);
        float width = Mathf.Abs(p1.x - p2.x);
        float height = Mathf.Abs(p1.y - p2.y);

        return new Rect(xMin, yMin, width, height);
    }

    private void OnGUI()
    {
        if (!IsDragging || DragDistance < minDragSize)
            return;

        Rect rect = GetScreenRect(DragStart, DragEnd);

        Color prevColor = GUI.color;
        GUI.color = new Color(0f, 0.5f, 1f, 0.2f);
        GUI.DrawTexture(rect, _selectionTexture);

        GUI.color = Color.white;
        GUI.Box(rect, GUIContent.none);

        GUI.color = prevColor;
    }
}
