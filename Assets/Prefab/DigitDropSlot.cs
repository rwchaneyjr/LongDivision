using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DigitDropSlot : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI slotText;
    public Image backgroundImage;

    [Header("State")]
    public bool isCarrySlot;

    [HideInInspector]
    public LongMath mathManager;

    Color defaultColor;

    void Awake()
    {
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();

        if (backgroundImage != null)
            defaultColor = backgroundImage.color;
    }

    // Main method
    public void SetHighlight(bool on, Color highlightColor)
    {
        if (backgroundImage == null) return;
        backgroundImage.color = on ? highlightColor : defaultColor;
    }

    // Old call style
    public void SetHighlight(bool on)
    {
        SetHighlight(on, Color.yellow);
    }

    // Legacy call style still used in LongMath
    public void SetHighlight(Color highlightColor)
    {
        SetHighlight(true, highlightColor);
    }


    // Used by LongMath
    public void ClearHighlight()
    {
        SetHighlight(false);
    }

    public void Clear()
    {
        if (slotText != null)
            slotText.text = "";
    }

    public void SetDigit(int digit)
    {
        if (slotText != null)
            slotText.text = digit.ToString();
    }
}
