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

    [Header("Text to show ? or digit")]
    public TextMeshProUGUI slotText;

    [Header("Background image for color change")]
    public Image background;

    [HideInInspector]
    public LongDivision mathManager;   // set by LMBuilder

    // internal state
    bool filled = false;
    Color baseColor;

    void Awake()
    {
        if (background != null)
            baseColor = background.color;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        DraggableDigit drag =
            eventData.pointerDrag.GetComponent<DraggableDigit>();

        if (drag == null) return;

        // (optional) if you want to enforce correct digit:
        // if (expectedDigit >= 0 && drag.digitValue != expectedDigit) return;

        // snap the digit into this slot
        drag.LockInSlot(transform);
        filled = true;

        if (slotText != null)
            slotText.text = drag.digitValue.ToString();

        // tell LongMath that something changed (safe no-op right now)
      
    }

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
}
