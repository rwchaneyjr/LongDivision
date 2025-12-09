using UnityEngine;
using TMPro;

public class LongDivision : MonoBehaviour
{
    [Header("Board Digit Rows")]
    public DigitDropSlot[] divisorSlots;
    public DigitDropSlot[] dividendSlots;
    public DigitDropSlot[] quotientSlots;
    public DigitDropSlot[][] answerRows;

    [Header("UI")]
    public TMP_Text instructionText;
    public TMP_Text explanationText;   // narration text (orange panel)

    // Internal state
    int divisor;
    int[] dividend;

    int stepIndex = 0;
    int dividendPos = 0;
    int quotientPos = 3;
    int answerRowIndex = 0;

    int currentValue = 0;
    int workCol = 4;

    bool finalRemainderWritten = false;

    void Start()
    {
        if (instructionText)
            instructionText.text = "Drop divisor & dividend, then press Step.";

        Narrate("Drag in the divisor and dividend, then press Step.");
    }

    // ================================================================
    // NARRATION HELPER
    // ================================================================
    void Narrate(string msg)
    {
        if (explanationText != null)
            explanationText.text = msg;
    }

    // ================================================================
    // STEP BUTTON LOGIC
    // ================================================================
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

            Narrate(
                "Step 1:\n" +
                "Start by taking the first digit(s) of the dividend.\n" +
                "Current value: " + currentValue
            );

            stepIndex = 1;
            return;
        }

        // ----------------------------------------------------
        // STEP 1 — BUILD A VALUE >= DIVISOR
        // ----------------------------------------------------
        if (stepIndex == 1)
        {
            while (currentValue < divisor && dividendPos < dividend.Length)
            {
                currentValue = currentValue * 10 + dividend[dividendPos];
                HighlightDividendDigit(dividendPos);
                dividendPos++;
            }

            Narrate(
                "Bringing down digits until the number is large enough to divide.\n" +
                "Current working number: " + currentValue
            );

            if (currentValue < divisor && dividendPos >= dividend.Length)
            {
                WriteFinalRemainderRow();
                return;
            }

            stepIndex = 2;
            return;
        }

        // ----------------------------------------------------
        // STEP 2 — WRITE QUOTIENT DIGIT
        // ----------------------------------------------------
        if (stepIndex == 2)
        {
            int q = currentValue / divisor;
            quotientSlots[quotientPos + 1].slotText.text = q.ToString();
            quotientPos++;

            Narrate(
                "Divide:\n" +
                currentValue + " ÷ " + divisor + " = " + q + "\n" +
                "Write " + q + " in the quotient line."
            );

            stepIndex = 3;
            return;
        }

        // ----------------------------------------------------
        // STEP 3 — MULTIPLY & WRITE PRODUCT
        // ----------------------------------------------------
        if (stepIndex == 3)
        {
            int q = int.Parse(quotientSlots[quotientPos].slotText.text);
            int product = q * divisor;

            if (answerRowIndex > 0)
                answerRowIndex++;

            WriteText("-", answerRowIndex, workCol - 1);
            WriteNumber(product, answerRowIndex, workCol);

            Narrate(
                "Multiply:\n" +
                divisor + " × " + q + " = " + product + "\n" +
                "Write " + product + " under the working number."
            );

            stepIndex = 4;
            return;
        }

        // ----------------------------------------------------
        // STEP 4 — SUBTRACT
        // ----------------------------------------------------
        if (stepIndex == 4)
        {
            int q = int.Parse(quotientSlots[quotientPos].slotText.text);
            int product = q * divisor;

            int remainder = currentValue - product;
            currentValue = remainder;

            answerRowIndex++;
            WriteNumber(remainder, answerRowIndex, workCol);

            Narrate(
                "Subtract:\n" +
                "Remainder = " + remainder
            );

            stepIndex = 5;
            return;
        }

        // ----------------------------------------------------
        // STEP 5 — BRING DOWN NEXT DIGIT OR FINISH
        // ----------------------------------------------------
        if (stepIndex == 5)
        {
            if (dividendPos < dividend.Length)
            {
                int bring = dividend[dividendPos];
                int newVal = currentValue * 10 + bring;

                WriteNumber(newVal, answerRowIndex, workCol);

                int numDigits = GetDigitCount(newVal);
                ColorDigit(answerRowIndex, workCol + numDigits - 1, Color.red);

                HighlightDividendDigit(dividendPos);

                Narrate(
                    "Bring down next digit:\n" +
                    "New working number: " + newVal + "\n" +
                    "Prepare to divide again."
                );

                currentValue = newVal;
                dividendPos++;
                workCol++;

                stepIndex = 1;
                return;
            }

            if (!finalRemainderWritten && currentValue > 0)
            {
                WriteFinalRemainderRow();
            }
            else
            {
                Narrate("Division finished. No remainder.");
                if (instructionText != null)
                    instructionText.text = "Done!";
                stepIndex = -1;
            }

            return;
        }
    }

    // ================================================================
    // FINAL REMAINDER ROW (ONLY WRITES ONCE)
    // ================================================================
    void WriteFinalRemainderRow()
    {
        if (finalRemainderWritten)
            return;

        finalRemainderWritten = true;

        answerRowIndex++;

        // write remainder
        WriteNumber(currentValue, answerRowIndex, workCol + 1);
        ColorDigit(answerRowIndex, workCol + 1, Color.red);

        // write R
        WriteText("R", answerRowIndex, workCol + 2);

        // WRITE FINAL ANSWER IN BOTH TEXT FIELDS
        WriteFinalAnswerText();

        stepIndex = -1;   // stop stepping
    }


    // ================================================================
    // SUPPORT FUNCTIONS
    // ================================================================
    bool LoadNumbers()
    {
        var div = new System.Collections.Generic.List<int>();
        foreach (var s in divisorSlots)
            if (TryReadDigit(s, out int v))
                div.Add(v);

        if (div.Count < 2)
        {
            instructionText.text = "Need 2-digit divisor.";
            Narrate("Please enter a 2-digit divisor.");
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
            Narrate("Please enter the digits of the dividend.");
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

    // ================================================================
    // FINAL ANSWER
    // ================================================================
    void WriteFinalAnswerText()
    {
        // Build quotient string
        string qString = "";
        foreach (var slot in quotientSlots)
        {
            if (slot != null && slot.slotText != null && slot.slotText.text != "")
                qString += slot.slotText.text;
        }

        string rString = currentValue.ToString();

        // 1️⃣ Write to narration panel (orange box)
        Narrate(
            "Final Answer:\n" +
            "Quotient = " + qString + "\n" +
            "Remainder = " + rString
        );

        // 2️⃣ Write to instruction bar (top)
        if (instructionText != null)
            instructionText.text = qString + " R" + rString;
    }

}
