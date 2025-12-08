using UnityEngine;
using TMPro;

public class LongDivision : MonoBehaviour
{
    [Header("Board Digit Rows")]
    public DigitDropSlot[] divisorSlots;    // length 4
    public DigitDropSlot[] dividendSlots;   // length 4
    public DigitDropSlot[] quotientSlots;   // length 9
    public DigitDropSlot[][] answerRows;    // 3 rows of 9 columns

    [Header("Instructions")]
    public TMP_Text instructionText;

    // internal math data
    int divisor;
    int[] dividend;
    int stepIndex = 0;

    int dividendPos = 0;
    int quotientPos = 0;
    int answerRowIndex = 0;
    int currentValue = 0;

    void Start()
    {
        if (instructionText)
            instructionText.text = "Drop divisor (4 digits) & dividend (4 digits). Then press Step.";
    }

    // ---------------------------------------------------------

    public void Step()
    {
        // STEP 0 — Load numbers
        if (stepIndex == 0)
        {
            if (!LoadNumbers())
                return;

            currentValue = dividend[0];
            dividendPos = 1;

            HighlightDividendDigit(0);

            stepIndex = 1;
            return;
        }

        // STEP 1 — Bring down digits until currentValue >= divisor
        if (stepIndex == 1)
        {
            while (currentValue < divisor && dividendPos < dividend.Length)
            {
                currentValue = currentValue * 10 + dividend[dividendPos];
                HighlightDividendDigit(dividendPos);
                dividendPos++;
            }

            if (currentValue < divisor)
            {
                instructionText.text = "No more digits — done.";
                return;
            }

            stepIndex = 2;
            return;
        }

        // STEP 2 — Compute quotient digit
        if (stepIndex == 2)
        {
            int q = currentValue / divisor;

            quotientSlots[quotientPos].slotText.text = q.ToString();
            quotientPos++;

            instructionText.text = $"Divide: {currentValue} ÷ {divisor} = {q}";

            stepIndex = 3;
            return;
        }

        // STEP 3 — Multiply q × divisor and write to row
        if (stepIndex == 3)
        {
            int q = int.Parse(quotientSlots[quotientPos - 1].slotText.text);
            int product = q * divisor;

            WriteNumberToRow(product, answerRowIndex);

            instructionText.text = $"{q} × {divisor} = {product}";

            stepIndex = 4;
            return;
        }

        // STEP 4 — Subtract
        if (stepIndex == 4)
        {
            int q = int.Parse(quotientSlots[quotientPos - 1].slotText.text);
            int product = q * divisor;

            int remainder = currentValue - product;
            currentValue = remainder;

            answerRowIndex++;

            instructionText.text = $"{currentValue + product} - {product} = {remainder}";

            stepIndex = 5;
            return;
        }

        // STEP 5 — Bring down next digit
        if (stepIndex == 5)
        {
            if (dividendPos < dividend.Length)
            {
                HighlightDividendDigit(dividendPos);
                currentValue = currentValue * 10 + dividend[dividendPos];
                dividendPos++;

                instructionText.text = $"Bring down → now {currentValue}";
            }
            else
            {
                instructionText.text = $"Done. Remainder = {currentValue}";
            }

            stepIndex = 1;
            return;
        }
    }

    // ---------------------------------------------------------

    bool LoadNumbers()
    {
        // divisor is 4 digits (T1–T4)
        divisor = 0;

        for (int i = 0; i < 4; i++)
        {
            if (!TryReadDigit(divisorSlots[i], out int v))
            {
                instructionText.text = "Please fill ALL 4 divisor slots.";
                return false;
            }

            divisor = divisor * 10 + v;
        }

        // dividend is 4 digits (T6–T9)
        dividend = new int[4];

        for (int i = 0; i < 4; i++)
        {
            if (!TryReadDigit(dividendSlots[i], out int v))
            {
                instructionText.text = "Please fill ALL 4 dividend slots.";
                return false;
            }

            dividend[i] = v;
        }

        return true;
    }

    bool TryReadDigit(DigitDropSlot slot, out int val)
    {
        val = 0;

        if (slot == null || slot.slotText == null)
            return false;

        return int.TryParse(slot.slotText.text, out val);
    }

    void HighlightDividendDigit(int index)
    {
        for (int i = 0; i < dividendSlots.Length; i++)
        {
            if (i == index)
                dividendSlots[i].SetHighlight(Color.yellow);
            else
                dividendSlots[i].ClearHighlight();
        }
    }

    void WriteNumberToRow(int value, int row)
    {
        string s = value.ToString();

        int colStart = answerRows[row].Length - s.Length;

        for (int i = 0; i < s.Length; i++)
        {
            answerRows[row][colStart + i].slotText.text = s[i].ToString();
        }
    }
}
