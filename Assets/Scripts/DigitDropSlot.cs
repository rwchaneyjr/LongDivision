using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class DigitDropSlot : MonoBehaviour, IDropHandler
{
    [Header("Optional: correct digit for this slot (-1 = any)")]
    public int expectedDigit = -1;

    [Header("Is this a carry slot?")]
    public bool isCarrySlot = false;

    [Header("Display")]
    public TextMeshProUGUI slotText;      // number that shows in the slot
    public Image background;              // highlight background

    // internal state
    bool filled = false;
    Color baseColor;

    void Awake()
    {
        if (background != null)
            baseColor = background.color;
    }

    // ------------------------------------------------------------
    // DRAG-AND-DROP HANDLER
    // ------------------------------------------------------------
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

        DraggableDigit drag = eventData.pointerDrag.GetComponent<DraggableDigit>();
        if (drag == null)
            return;

        // Optional: enforce expected digit
        if (expectedDigit >= 0 && drag.digitValue != expectedDigit)
        {
            // You could play a "wrong" sound here later
            return;
        }

        // Snap the dragged digit into this slot
        drag.LockInSlot(transform);
        filled = true;

        // Update visible number
        if (slotText != null)
            slotText.text = drag.digitValue.ToString();
    }

    // ------------------------------------------------------------
    // HIGHLIGHTING
    // ------------------------------------------------------------
    public void SetHighlight(Color color)
    {
        if (background != null)
            background.color = color;
    }

    public void ClearHighlight()
    {
        if (background != null)
            background.color = baseColor;
    }

    // ------------------------------------------------------------
    // RESET SLOT (useful for new problems or step reset)
    // ------------------------------------------------------------
    public void ClearSlot()
    {
        filled = false;

        if (slotText != null)
            slotText.text = "";

        ClearHighlight();
    }

    // ------------------------------------------------------------
    // OPTIONAL: Check if slot is filled
    // ------------------------------------------------------------
    public bool IsFilled()
    {
        return filled;
    }
}
