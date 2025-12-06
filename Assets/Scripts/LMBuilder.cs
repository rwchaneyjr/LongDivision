using UnityEngine;
using TMPro;

public class LMBuilder : MonoBehaviour
{
    [Header("Prefabs")]
    public DigitDropSlot digitSlotPrefab;
    public DraggableDigit draggableDigitPrefab;

    [Header("Row Parents")]
    public RectTransform quotientRowParent;   // TOP row (answer)
    public RectTransform dividendRowParent;   // SECOND row
    public RectTransform divisorLabelParent;  // LEFT side label (46 )
    public RectTransform subtractionRowsParent; // Where we spawn subtract rows

    [Header("Digit Tray")]
    public RectTransform digitTrayParent;

    [Header("Layout")]
    public int maxDigits = 9;
    public float cellSize = 200f;
    public float cellSpacing = 20f;
    public float rowSpacing = 220f;

    [Header("References")]
    public LongDivision longDivision;

    Vector2[] digitHomePositions;

    void Start()
    {
        if (longDivision == null)
            longDivision = GetComponent<LongDivision>();

        digitHomePositions = new Vector2[10];

        BuildQuotientRow();
        BuildDividendRow();
        BuildDigitTray();

        // Tell LongDivision which slots belong where
        longDivision.topInputSlots = quotientSlots;
        longDivision.bottomInputSlots = dividendSlots;
        longDivision.answerRows = subtractionRows;
        longDivision.finalAnswerSlots = finalRow;
    }

    // ------------------ ROW ARRAYS --------------------
    DigitDropSlot[] quotientSlots;
    DigitDropSlot[] dividendSlots;
    DigitDropSlot[][] subtractionRows;
    DigitDropSlot[] finalRow;

    // ------------------ ROW BUILDING ------------------

    void BuildQuotientRow()
    {
        quotientSlots = new DigitDropSlot[maxDigits];

        for (int i = 0; i < maxDigits; i++)
        {
            var slot = Instantiate(digitSlotPrefab, quotientRowParent);
            slot.GetComponent<RectTransform>().anchoredPosition =
                new Vector2(i * (cellSize + cellSpacing), 0);

            slot.isCarrySlot = false;
            quotientSlots[i] = slot;
        }
    }

    void BuildDividendRow()
    {
        dividendSlots = new DigitDropSlot[maxDigits];

        for (int i = 0; i < maxDigits; i++)
        {
            var slot = Instantiate(digitSlotPrefab, dividendRowParent);
            slot.GetComponent<RectTransform>().anchoredPosition =
                new Vector2(i * (cellSize + cellSpacing), 0);

            slot.isCarrySlot = false;
            dividendSlots[i] = slot;
        }
    }

    // Build 3 subtraction rows + 1 final row
    void BuildSubtractionRows()
    {
        subtractionRows = new DigitDropSlot[3][]; // 3 rows
        for (int r = 0; r < 3; r++)
        {
            subtractionRows[r] = new DigitDropSlot[maxDigits];

            GameObject rowObj = new GameObject($"SubRow{r}", typeof(RectTransform));
            RectTransform row = rowObj.GetComponent<RectTransform>();
            row.SetParent(subtractionRowsParent);
            row.localScale = Vector3.one;
            row.anchoredPosition = new Vector2(0, -rowSpacing * (r + 1));

            for (int i = 0; i < maxDigits; i++)
            {
                var slot = Instantiate(digitSlotPrefab, row);
                slot.GetComponent<RectTransform>().anchoredPosition =
                    new Vector2(i * (cellSize + cellSpacing), 0);

                subtractionRows[r][i] = slot;
            }
        }

        finalRow = new DigitDropSlot[maxDigits];
        GameObject fObj = new GameObject("FinalRow", typeof(RectTransform));
        RectTransform fRow = fObj.GetComponent<RectTransform>();
        fRow.SetParent(subtractionRowsParent);
        fRow.localScale = Vector3.one;
        fRow.anchoredPosition = new Vector2(0, -rowSpacing * 4);

        for (int i = 0; i < maxDigits; i++)
        {
            var slot = Instantiate(digitSlotPrefab, fRow);
            slot.GetComponent<RectTransform>().anchoredPosition =
                new Vector2(i * (cellSize + cellSpacing), 0);

            finalRow[i] = slot;
        }
    }

    // ----------------- DIGIT TRAY --------------------

    void BuildDigitTray()
    {
        float total = 10 * cellSize + 9 * cellSpacing;
        float startX = -total / 2f;

        for (int i = 0; i < 10; i++)
        {
            Vector2 pos = new Vector2(startX + i * (cellSize + cellSpacing), 0);

            digitHomePositions[i] = pos;
            SpawnDigit(i, pos);
        }
    }

    void SpawnDigit(int value, Vector2 pos)
    {
        var d = Instantiate(draggableDigitPrefab, digitTrayParent);
        d.digitValue = value;
        d.GetComponentInChildren<TextMeshProUGUI>().text = value.ToString();

        d.GetComponent<RectTransform>().anchoredPosition = pos;
        d.onTaken = OnDigitTaken;
    }

    void OnDigitTaken(DraggableDigit digit)
    {
        if (digit == null) return;
        int v = digit.digitValue;

        SpawnDigit(v, digitHomePositions[v]);
    }
}
