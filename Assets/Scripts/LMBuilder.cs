using UnityEngine;
using TMPro;

public class LMBuilder : MonoBehaviour
{
    // ------------------- PREFABS -------------------
    public DigitDropSlot digitSlotPrefab;
    public DraggableDigit draggableDigitPrefab;

    // ------------------- ROW PARENTS -------------------
    [Header("Row Parents")]
    public RectTransform quotientRowParent;
    public RectTransform divisorRowParent;
    public RectTransform dividendRowParent;
    public RectTransform subtractionRowsParent;

    [Header("Digit Tray")]
    public RectTransform digitTrayParent;

    [Header("Manual Bracket + Line")]
    public TMP_Text rightBracketText;        // <-- YOU drag a TMP Text here
    public RectTransform divisionLine;       // <-- YOU drag an Image/RectTransform here

    // ------------------- SETTINGS -------------------
    public int maxDigits = 9;
    public float cellSize = 200f;
    public float cellSpacing = 20f;
    public float rowSpacing = 200f;

    [Header("References")]
    public LongDivision longDivision;

    // ------------------- INTERNAL -------------------
    DigitDropSlot[] quotientSlots;
    DigitDropSlot[] divisorSlots;
    DigitDropSlot[] dividendSlots;
    DigitDropSlot[][] subtractionRows;

    Vector2[] homePositions;

    // ==============================================================

    void Start()
    {
        if (longDivision == null)
            longDivision = GetComponent<LongDivision>();

        BuildQuotientRow();
        BuildDivisorRow();
        BuildDividendRow();
        BuildSubtractionRows();
        BuildDigitTray();

        // Pass references directly (no generation, no auto placement)
        longDivision.quotientSlots = quotientSlots;
        longDivision.divisorSlots = divisorSlots;
        longDivision.dividendSlots = dividendSlots;
        longDivision.answerRows = subtractionRows;
        longDivision.rightBracketText = rightBracketText;
        longDivision.divisionLine = divisionLine;

        Debug.Log("LMBuilder: Division board created (manual bracket + line).");
    }

    // ==============================================================

    void BuildQuotientRow()
    {
        quotientSlots = new DigitDropSlot[maxDigits];

        for (int i = 0; i < maxDigits; i++)
        {
            var slot = Instantiate(digitSlotPrefab, quotientRowParent);
            RectTransform rt = slot.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(i * (cellSize + cellSpacing), 0);
            quotientSlots[i] = slot;
        }
    }

    // ==============================================================

    void BuildDivisorRow()
    {
        divisorSlots = new DigitDropSlot[2];

        for (int i = 0; i < 2; i++)
        {
            var slot = Instantiate(digitSlotPrefab, divisorRowParent);
            RectTransform rt = slot.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(i * (cellSize + cellSpacing), 0);
            divisorSlots[i] = slot;
        }
    }

    // ==============================================================

    void BuildDividendRow()
    {
        dividendSlots = new DigitDropSlot[maxDigits];

        for (int i = 0; i < maxDigits; i++)
        {
            var slot = Instantiate(digitSlotPrefab, dividendRowParent);
            RectTransform rt = slot.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(i * (cellSize + cellSpacing), 0);
            dividendSlots[i] = slot;
        }
    }

    // ==============================================================

    void BuildSubtractionRows()
    {
        subtractionRows = new DigitDropSlot[3][];

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
                RectTransform rt = slot.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(i * (cellSize + cellSpacing), 0);
                subtractionRows[r][i] = slot;
            }
        }
    }

    // ==============================================================

    void BuildDigitTray()
    {
        homePositions = new Vector2[10];

        float totalWidth = 10 * cellSize + 9 * cellSpacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < 10; i++)
        {
            Vector2 pos = new Vector2(startX + i * (cellSize + cellSpacing), 0);
            homePositions[i] = pos;
            SpawnDigit(i, pos);
        }
    }

    void SpawnDigit(int value, Vector2 pos)
    {
        var d = Instantiate(draggableDigitPrefab, digitTrayParent);
        d.digitValue = value;
        d.GetComponentInChildren<TextMeshProUGUI>().text = value.ToString();

        RectTransform rt = d.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;

        d.onTaken = OnDigitTaken;
    }

    void OnDigitTaken(DraggableDigit digit)
    {
        SpawnDigit(digit.digitValue, homePositions[digit.digitValue]);
    }
}
