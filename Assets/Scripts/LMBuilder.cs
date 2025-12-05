using UnityEngine;
using TMPro;

/// <summary>
/// Builds ONLY the needed UI rows for long multiplication.
/// Static rows on Start, dynamic rows after STEP.
/// </summary>
public class LMBuilder : MonoBehaviour
{
    // ---------------- PREFABS ----------------
    [Header("Prefabs")]
    public DigitDropSlot digitSlotPrefab;
    public DraggableDigit draggableDigitPrefab;

    // ---------------- ROW TEMPLATES ----------------
    [Header("Row Templates")]
    public RectTransform carryRowTemplate;
    public RectTransform answerRowTemplate;

    // ---------------- INPUT ROWS ----------------
    [Header("Input Rows")]
    public RectTransform topRowParent;
    public RectTransform bottomRowParent;

    // ---------------- DIGIT TRAY ----------------
    [Header("Digit Tray")]
    public RectTransform draggableDigitsParent;

    // ---------------- LAYOUT ----------------
    [Header("Layout")]
    public float cellSize = 200f;
    public float cellSpacing = 20f;
    public float rowSpacingMultiplier = 1.1f;
    public int maxDigits = 9;

    // ---------------- REFERENCES ----------------
    [Header("References")]
    public LongMath longMath;
    public BoardVerticalShift boardVerticalShift;

    // ---------------- INTERNAL ----------------
    float computedRowSpacing;
    Vector2[] digitHomePositions;

    // =====================================================
    void Start()
    {
        if (longMath == null)
            longMath = GetComponent<LongMath>();

        digitHomePositions = new Vector2[10];

        ComputeRowSpacing();

        BuildCarryRow();
        BuildInputRows();
        BuildDigitTray();

        // ✅ DO NOT build answer rows here
        answerRowTemplate.gameObject.SetActive(false);
    }

    // =====================================================
    // ✅ CALLED ONLY AFTER STEP (numbers known)
    public void SpawnRowsForBottomNumber(int bottomNumber)
    {
        int bottomDigits = Mathf.Abs(bottomNumber).ToString().Length;

        ClearOldAnswerRows();

        // ---------------- PARTIAL PRODUCTS ----------------
        longMath.answerRows = new DigitDropSlot[bottomDigits][];

        for (int r = 0; r < bottomDigits; r++)
        {
            RectTransform row = SpawnRow(answerRowTemplate, r);

            DigitDropSlot[] slots = new DigitDropSlot[maxDigits];
            for (int c = 0; c < maxDigits; c++)
                slots[c] = SpawnSlot(row, c, false);

            longMath.answerRows[r] = slots;
        }

        // ---------------- FINAL ANSWER ROW ----------------
        RectTransform finalRow =
            SpawnRow(answerRowTemplate, bottomDigits);

        longMath.finalAnswerSlots = new DigitDropSlot[maxDigits];
        for (int c = 0; c < maxDigits; c++)
            longMath.finalAnswerSlots[c] = SpawnSlot(finalRow, c, false);

        // ✅ Optional board lift
        if (boardVerticalShift != null)
            boardVerticalShift.ShiftUpForNewRow();
    }

    // =====================================================
    void BuildInputRows()
    {
        longMath.topInputSlots = new DigitDropSlot[maxDigits];
        longMath.bottomInputSlots = new DigitDropSlot[maxDigits];

        for (int i = 0; i < maxDigits; i++)
        {
            longMath.topInputSlots[i] =
                SpawnSlot(topRowParent, i, false);

            longMath.bottomInputSlots[i] =
                SpawnSlot(bottomRowParent, i, false);
        }
    }

    void BuildCarryRow()
    {
        longMath.carrySlots = new DigitDropSlot[maxDigits];

        for (int i = 0; i < maxDigits; i++)
        {
            var s = SpawnSlot(carryRowTemplate, i, true);
            if (s.slotText != null)
                s.slotText.text = "C";

            longMath.carrySlots[i] = s;
        }
    }

    // =====================================================
    void BuildDigitTray()
    {
        float total =
            10 * cellSize + 9 * cellSpacing;

        float startX = -total / 2f;

        for (int i = 0; i < 10; i++)
        {
            Vector2 pos =
                new Vector2(startX + i * (cellSize + cellSpacing), 0);

            digitHomePositions[i] = pos;
            SpawnDigit(i, pos);
        }
    }

    // =====================================================
    RectTransform SpawnRow(RectTransform template, int index)
    {
        RectTransform row =
            Instantiate(template, template.parent);

        row.anchoredPosition =
            template.anchoredPosition +
            Vector2.down * index * computedRowSpacing;

        row.gameObject.SetActive(true);
        return row;
    }

    DigitDropSlot SpawnSlot(RectTransform parent, int index, bool carry)
    {
        var slot = Instantiate(digitSlotPrefab, parent);
        slot.isCarrySlot = carry;
        slot.mathManager = longMath;

        slot.GetComponent<RectTransform>().anchoredPosition =
            new Vector2(index * (cellSize + cellSpacing), 0);

        return slot;
    }

    // =====================================================
    void SpawnDigit(int value, Vector2 pos)
    {
        var d = Instantiate(draggableDigitPrefab, draggableDigitsParent);

        d.digitValue = value;

        var txt = d.GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null)
            txt.text = value.ToString();

        d.onTaken = OnDigitTaken;
        d.GetComponent<RectTransform>().anchoredPosition = pos;
    }

    // Called once when a digit leaves the tray
    void OnDigitTaken(DraggableDigit taken)
    {
        if (taken == null) return;

        int value = taken.digitValue;
        if (value < 0 || value >= digitHomePositions.Length) return;

        SpawnDigit(value, digitHomePositions[value]);
    }

    // =====================================================
    void ClearOldAnswerRows()
    {
        if (longMath.answerRows != null)
        {
            foreach (var row in longMath.answerRows)
                if (row != null)
                    foreach (var s in row)
                        if (s != null)
                            Destroy(s.gameObject);
        }

        if (longMath.finalAnswerSlots != null)
        {
            foreach (var s in longMath.finalAnswerSlots)
                if (s != null)
                    Destroy(s.gameObject);
        }
    }

    void ComputeRowSpacing()
    {
        computedRowSpacing =
            cellSize * rowSpacingMultiplier;
    }
}
