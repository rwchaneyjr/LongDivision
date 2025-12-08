using UnityEngine;
using TMPro;

public class LMBuilder : MonoBehaviour
{
    [Header("Prefabs")]
    public DigitDropSlot digitSlotPrefab;
    public DraggableDigit draggableDigitPrefab;

    [Header("Row Parents")]
    public RectTransform quotientRowParent;
    public RectTransform divisorRowParent;
    public RectTransform dividendRowParent;
    public RectTransform subtractionRowsParent;

    [Header("Digit Tray")]
    public RectTransform digitTrayParent;

    [Header("Settings")]
    public float cellSize = 200f;
    public float cellSpacing = 20f;
    public float rowSpacing = 200f;

    [Header("References")]
    public LongDivision longDivision;

    // Built arrays
    DigitDropSlot[] divisorSlots;
    DigitDropSlot[] dividendSlots;
    DigitDropSlot[] quotientSlots;
    DigitDropSlot[][] subtractionRows;

    Vector2[] trayHomePos = new Vector2[10];

    void Start()
    {
        if (longDivision == null)
            longDivision = GetComponent<LongDivision>();

        BuildDivisorSlots();
        BuildDividendSlots();
        BuildQuotientSlots();
        BuildSubtractionRows();
        BuildDigitTray();

        // Give the arrays to LongDivision
        longDivision.divisorSlots = divisorSlots;
        longDivision.dividendSlots = dividendSlots;
        longDivision.quotientSlots = quotientSlots;
        longDivision.answerRows = subtractionRows;

        Debug.Log("LMBuilder: Board created.");
    }

    // ----------------------------------------------------

    void BuildDivisorSlots()
    {
        divisorSlots = new DigitDropSlot[4];

        for (int i = 0; i < 4; i++)
        {
            var slot = Instantiate(digitSlotPrefab, divisorRowParent);
            var rt = slot.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(i * (cellSize + cellSpacing), 0);
            divisorSlots[i] = slot;
        }
    }

    void BuildDividendSlots()
    {
        dividendSlots = new DigitDropSlot[4];

        for (int i = 0; i < 4; i++)
        {
            var slot = Instantiate(digitSlotPrefab, dividendRowParent);
            var rt = slot.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(i * (cellSize + cellSpacing), 0);
            dividendSlots[i] = slot;
        }
    }

    void BuildQuotientSlots()
    {
        quotientSlots = new DigitDropSlot[9];

        for (int i = 0; i < 9; i++)
        {
            var slot = Instantiate(digitSlotPrefab, quotientRowParent);
            var rt = slot.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(i * (cellSize + cellSpacing), 0);
            quotientSlots[i] = slot;
        }
    }

    void BuildSubtractionRows()
    {
        subtractionRows = new DigitDropSlot[3][];

        for (int r = 0; r < 3; r++)
        {
            subtractionRows[r] = new DigitDropSlot[9];

            var rowObj = new GameObject("SubRow_" + r, typeof(RectTransform));
            var row = rowObj.GetComponent<RectTransform>();

            row.SetParent(subtractionRowsParent);
            row.localScale = Vector3.one;
            row.anchoredPosition = new Vector2(0, -rowSpacing * (r + 1));

            for (int i = 0; i < 9; i++)
            {
                var slot = Instantiate(digitSlotPrefab, row);
                var rt = slot.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(i * (cellSize + cellSpacing), 0);
                subtractionRows[r][i] = slot;
            }
        }
    }

    // ----------------------------------------------------
    // Digit Tray

    void BuildDigitTray()
    {
        float totalWidth = 10 * cellSize + 9 * cellSpacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < 10; i++)
        {
            Vector2 pos = new Vector2(startX + i * (cellSize + cellSpacing), 0);
            trayHomePos[i] = pos;
            SpawnDigit(i, pos);
        }
    }

    void SpawnDigit(int value, Vector2 pos)
    {
        var d = Instantiate(draggableDigitPrefab, digitTrayParent);
        d.digitValue = value;
        d.GetComponentInChildren<TextMeshProUGUI>().text = value.ToString();

        var rt = d.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;

        d.onTaken = OnDigitTaken;
    }

    void OnDigitTaken(DraggableDigit d)
    {
        SpawnDigit(d.digitValue, trayHomePos[d.digitValue]);
    }
}
