using UnityEngine;
using TMPro;

public class LongDivision : MonoBehaviour
{
    // -------------------------------------------------
    // ROWS OF DIGITS (hooked up by LMBuilder at runtime)
    // -------------------------------------------------
    [Header("Board Digit Rows")]
    public DigitDropSlot[] divisorSlots;       // left of bracket (e.g. 4, 6)
    public DigitDropSlot[] dividendSlots;      // right of bracket (all dividend digits)
    public DigitDropSlot[] quotientSlots;      // top row (final answer / quotient)
    public DigitDropSlot[][] answerRows;       // subtraction rows (optional)

    // -------------------------------------------------
    // BRACKET + LINE UI
    // -------------------------------------------------
    [Header("Bracket / Line UI")]
    public TMP_Text rightBracketText;          // the ")" character from LMBuilder
    public RectTransform divisionLine;         // horizontal bar above dividend

    // -------------------------------------------------
    // OTHER UI
    // -------------------------------------------------
    [Header("Instructions")]
    public TMP_Text instructionText;

    // -------------------------------------------------
    // INTERNAL STATE
    // -------------------------------------------------
    int divisor;
    int[] dividendDigits;
    int stepIndex = 0;

    void Start()
    {
        if (instructionText != null)
            instructionText.text = "Put divisor and dividend digits, then press Step.";

        // Hide the line until numbers are validated
        if (divisionLine != null)
            divisionLine.gameObject.SetActive(false);
    }

    // -------------------------------------------------
    // STEP BUTTON
    // -------------------------------------------------
    public void Step()
    {
        // First time pressed: validate input
        if (stepIndex == 0)
        {
            if (!LoadNumbers())
                return;

            // Show division bar after numbers confirmed
            if (divisionLine != null)
                divisionLine.gameObject.SetActive(true);

            if (instructionText != null)
                instructionText.text = "Now step through the division...";
        }

        Debug.Log($"LongDivision Step {stepIndex}");
        stepIndex++;
    }

    // -------------------------------------------------
    // Reads divisor + dividend
    // -------------------------------------------------
    bool LoadNumbers()
    {
        // Must have two digits in divisor
        if (divisorSlots == null || divisorSlots.Length < 2)
        {
            if (instructionText != null)
                instructionText.text = "Divisor row missing.";
            return false;
        }

        if (!TryReadDigit(divisorSlots[0], out int d1) ||
            !TryReadDigit(divisorSlots[1], out int d2))
        {
            if (instructionText != null)
                instructionText.text = "Fill BOTH divisor digits.";
            return false;
        }

        divisor = d1 * 10 + d2;

        // Read dividend
        if (dividendSlots == null || dividendSlots.Length == 0)
        {
            if (instructionText != null)
                instructionText.text = "Dividend row missing.";
            return false;
        }

        var list = new System.Collections.Generic.List<int>();
        foreach (var slot in dividendSlots)
        {
            if (TryReadDigit(slot, out int val))
                list.Add(val);
        }

        if (list.Count == 0)
        {
            if (instructionText != null)
                instructionText.text = "Put at least one digit in the dividend.";
            return false;
        }

        dividendDigits = list.ToArray();

        Debug.Log($"Loaded divisor = {divisor}, dividend = {string.Join("", dividendDigits)}");
        return true;
    }

    // -------------------------------------------------
    // Reads a single slot
    // -------------------------------------------------
    bool TryReadDigit(DigitDropSlot slot, out int value)
    {
        value = 0;

        if (slot == null || slot.slotText == null)
            return false;

        string t = slot.slotText.text;
        if (string.IsNullOrWhiteSpace(t))
            return false;

        if (int.TryParse(t, out int v))
        {
            value = v;
            return true;
        }

        return false;
    }
}
