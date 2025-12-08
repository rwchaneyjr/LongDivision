using UnityEngine;
using TMPro;

public class LongDivision : MonoBehaviour
{
    [Header("Board Digit Rows")]
    public DigitDropSlot[] divisorSlots;      // col 0–1 (2-digit divisor)
    public DigitDropSlot[] dividendSlots;     // col 3–5 (345)
    public DigitDropSlot[] quotientSlots;     // col 3–8 (for 28)
    public DigitDropSlot[][] answerRows;      // rows for work

    [Header("Instructions")]
    public TMP_Text instructionText;

    int divisor;
    int[] dividend;

    int stepIndex = 0;
    int dividendPos = 0;
    int quotientPos = 3;   // QUOTIENT STARTS ABOVE COL 3
    int answerRowIndex = 0;

    int currentValue = 0;

    int workCol = 4;   // THE DIGIT WE OPERATE ON

    void Start()
    {
        if (instructionText)
            instructionText.text = "Drop divisor & dividend, then press Step.";
    }

    public void Step()
    {
        // ----------------------------------------------------
        // STEP 0 — LOAD NUMBERS
        // ----------------------------------------------------
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

        // ----------------------------------------------------
        // STEP 1 — ACCUMULATE UNTIL >= DIVISOR
        // ----------------------------------------------------
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
                // final remainder
                WriteNumber(currentValue, answerRowIndex, workCol + 1);
                ColorDigit(answerRowIndex, workCol + 1, Color.red);
                WriteText(" R", answerRowIndex, workCol + 2);
                return;
            }

            stepIndex = 2;
            return;
        }

        // ----------------------------------------------------
        // STEP 2 — WRITE QUOTIENT DIGIT (SHIFTED ONE RIGHT)
        // ----------------------------------------------------
        if (stepIndex == 2)
        {
            int q = currentValue / divisor;

            // SHIFT QUOTIENT ONE PLACE RIGHT
            quotientSlots[quotientPos + 1].slotText.text = q.ToString();

            quotientPos++;   // Move to next position

            stepIndex = 3;
            return;
        }

        // ----------------------------------------------------
        // STEP 3 — MULTIPLY AND WRITE "-product" (SHIFTED LEFT)
        // ----------------------------------------------------
        if (stepIndex == 3)
        {
            // Read from quotientPos (which now points to where we wrote)
            int q = int.Parse(quotientSlots[quotientPos].slotText.text);
            int product = q * divisor;

            // INCREMENT ROW for second cycle (after row 2 where 105 is)
            if (answerRowIndex > 0)
                answerRowIndex++;

            // SHIFT ONE PLACE LEFT
            int outCol = workCol;

            WriteText("-", answerRowIndex, outCol - 1);
            WriteNumber(product, answerRowIndex, outCol);

            stepIndex = 4;
            return;
        }

        // ----------------------------------------------------
        // STEP 4 — SUBTRACT AND WRITE REMAINDER (SHIFTED LEFT)
        // ----------------------------------------------------
        // ----------------------------------------------------
        // STEP 4 — SUBTRACT AND WRITE REMAINDER (SHIFTED LEFT)
        // ----------------------------------------------------
        if (stepIndex == 4)
        {
            // Read from quotientPos (same as STEP 3)
            int q = int.Parse(quotientSlots[quotientPos].slotText.text);
            int product = q * divisor;

            int remainder = currentValue - product;
            currentValue = remainder;

            answerRowIndex++;

            // SHIFT ONE PLACE LEFT
            WriteNumber(remainder, answerRowIndex, workCol);

            stepIndex = 5;
            return;
        }
        // ----------------------------------------------------
        // STEP 5 — BRING DOWN NEXT DIGIT (LINES UP UNDER REMAINDER)
        // ----------------------------------------------------
        // ----------------------------------------------------
        // STEP 5 — BRING DOWN NEXT DIGIT (LINES UP UNDER REMAINDER)
        // ----------------------------------------------------
        // ----------------------------------------------------
        // STEP 5 — BRING DOWN NEXT DIGIT (REPLACE REMAINDER ROW)
        // ----------------------------------------------------
        if (stepIndex == 5)
        {
            if (dividendPos < dividend.Length)
            {
                // DON'T increment answerRowIndex - reuse the current row
                // answerRowIndex stays same to REPLACE the remainder

                int bring = dividend[dividendPos];
                int newVal = currentValue * 10 + bring;

                // REPLACE the remainder row with new number
                WriteNumber(newVal, answerRowIndex, workCol);

                // Color the last digit red
                int numDigits = GetDigitCount(newVal);
                ColorDigit(answerRowIndex, workCol + numDigits - 1, Color.red);

                HighlightDividendDigit(dividendPos);

                currentValue = newVal;
                dividendPos++;
                workCol++;  // INCREMENT workCol so next cycle shifts right

                stepIndex = 1;
                return;
            }
            else
            {
                // FINAL REMAINDER
                answerRowIndex++;
                WriteNumber(currentValue, answerRowIndex, workCol + 1);
                ColorDigit(answerRowIndex, workCol + 1, Color.red);
                WriteText(" R", answerRowIndex, workCol + 2);
            }
        }
    }

    // ----------------------------------------------------
    // SUPPORT FUNCTIONS
    // ----------------------------------------------------

    bool LoadNumbers()
    {
        var div = new System.Collections.Generic.List<int>();
        foreach (var s in divisorSlots)
            if (TryReadDigit(s, out int v))
                div.Add(v);

        if (div.Count < 2)
        {
            instructionText.text = "Need 2-digit divisor.";
            return false;
        }

        divisor = div[0] * 10 + div[1];

        var dvd = new System.Collections.Generic.List<int>();
        foreach (var s in dividendSlots)
            if (TryReadDigit(s, out int v))
                dvd.Add(v);

        if (dvd.Count == 0)
        {
            instructionText.text = "Enter dividend.";
            return false;
        }

        dividend = dvd.ToArray();
        return true;
    }

    bool TryReadDigit(DigitDropSlot slot, out int val)
    {
        val = 0;
        if (slot == null || slot.slotText == null) return false;
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

    void WriteNumber(int value, int row, int col)
    {
        string s = value.ToString();
        for (int i = 0; i < s.Length; i++)
            answerRows[row][col + i].slotText.text = s[i].ToString();
    }

    void WriteText(string t, int row, int col)
    {
        answerRows[row][col].slotText.text = t;
    }

    void ColorDigit(int row, int col, Color c)
    {
        answerRows[row][col].slotText.color = c;
    }

    int GetDigitCount(int v)
    {
        if (v == 0) return 1;
        return (int)Mathf.Floor(Mathf.Log10(v) + 1);
    }
}
