using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasHelper : MonoBehaviour
{
    public static bool CheckIsPivotInside(RectTransform rectTransform1, RectTransform rectTransform2)
    {
        Vector3 object1PivotWorldPos = rectTransform1.TransformPoint(rectTransform1.pivot);
        bool IsPivotInsideObject2 = RectTransformUtility.RectangleContainsScreenPoint(rectTransform2, object1PivotWorldPos, null);
        return IsPivotInsideObject2;
    } 
    
    public static bool CheckRectsOverlapping(RectTransform rect1, RectTransform rect2)
    {
        Vector3[] rect1Corners = new Vector3[4];
        Vector3[] rect2Corners = new Vector3[4];

        rect1.GetWorldCorners(rect1Corners);
        rect2.GetWorldCorners(rect2Corners);

        // Check for overlap in the x and y dimensions
        bool xOverlap = (rect1Corners[0].x <= rect2Corners[2].x && rect1Corners[2].x >= rect2Corners[0].x);
        bool yOverlap = (rect1Corners[0].y <= rect2Corners[2].y && rect1Corners[2].y >= rect2Corners[0].y);

        // If there's overlap in both dimensions, the rectangles are overlapping
        return xOverlap && yOverlap;
    }
}
