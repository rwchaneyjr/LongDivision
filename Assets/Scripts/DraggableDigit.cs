using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using TMPro;

/// <summary>
/// Simple draggable digit tile (0–9).
/// </summary>
public class DraggableDigit : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Digit value for this tile (0–9)")]
    public int digitValue = 0;

    [Header("Optional label (TextMeshPro)")]
    public TextMeshProUGUI label;

    // Called the first time this tile is picked up.
    // Used by LMBuilder to respawn a replacement in the tray.
    public Action<DraggableDigit> onTaken;

    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private Vector2 startAnchoredPosition;
    private Transform startParent;

    [HideInInspector] public bool placedInSlot = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (label == null)
            label = GetComponentInChildren<TextMeshProUGUI>();

        if (label != null)
            label.text = digitValue.ToString();

        Image img = GetComponent<Image>();
        if (img != null)
            img.raycastTarget = true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (canvas == null) return;

        // Remember starting point so we can snap back if needed
        startAnchoredPosition = rectTransform.anchoredPosition;
        startParent = transform.parent;

        canvasGroup.blocksRaycasts = false;

        // Notify builder ONCE that this digit has been taken from the tray
        if (onTaken != null)
        {
            var callback = onTaken;
            onTaken = null;        // ensure it only fires once
            callback(this);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null) return;

        rectTransform.anchoredPosition +=
            eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // If not successfully placed in a DigitDropSlot, snap back
        if (!placedInSlot)
        {
            transform.SetParent(startParent);
            rectTransform.anchoredPosition = startAnchoredPosition;
        }
    }

    /// <summary>
    /// Called by DigitDropSlot when the tile is locked in.
    /// </summary>
    public void LockInSlot(Transform newParent)
    {
        placedInSlot = true;

        transform.SetParent(newParent);
        rectTransform.anchoredPosition = Vector2.zero;

        canvasGroup.blocksRaycasts = true;
    }
}
