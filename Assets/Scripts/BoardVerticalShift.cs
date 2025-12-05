using UnityEngine;

public class BoardVerticalShift : MonoBehaviour
{
    [Header("References")]
    public RectTransform mathBoard;

    [Header("Settings")]
    public float rowShiftAmount = 55f;

    int rowCount = 0;

    /// <summary>
    /// Call this once whenever a new row is added.
    /// </summary>
    public void ShiftUpForNewRow()
    {
        rowCount++;

        Vector2 pos = mathBoard.anchoredPosition;
        pos.y += rowShiftAmount;
        mathBoard.anchoredPosition = pos;

        Debug.Log($"[BoardShift] Rows={rowCount}, New Y={pos.y}");
    }
}
