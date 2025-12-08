using UnityEngine;
using TMPro;

public class LongDivision : MonoBehaviour
{
    [Header("Board Digit Rows")]
    public DigitDropSlot[] divisorSlots;    // length 4
    public DigitDropSlot[] dividendSlots;   // length 4
    public DigitDropSlot[] quotientSlots;   // length 9
    public DigitDropSlot[][] answerRows;    // Now 6 rows for full work display

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
    int currentColumn = 0; // Track horizontal position for alignment

    void Start()
    {
        if (instructionText)
            instructionText.text = "Drop divisor (up to 4 digits) & dividend (up to 4 digits). Then press Step.";
    }

    public void Step()
    {
        // STEP 0 – Load numbers
        if (stepIndex == 0)
        {
            if (!LoadNumbers())
                return;

            currentValue = dividend[0];
            dividendPos = 1;
            currentColumn = 0;

            HighlightDividendDigit(0);
            instructionText.text = $"Starting with digit: {currentValue}";

            stepIndex = 1;
            return;
        }

        // STEP 1 – Bring down digits until currentValue >= divisor
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
                // Write final remainder
                WriteNumberToRow(currentValue, answerRowIndex, currentColumn);
                WriteTextToRow(" R", answerRowIndex, currentColumn + GetDigitCount(currentValue));
                instructionText.text = $"Done. Final Remainder = {currentValue}";
                return;
            }

            instructionText.text = $"Working with: {currentValue}";
            stepIndex = 2;
            return;
        }

        // STEP 2 – Compute quotient digit
        if (stepIndex == 2)
        {
            int q = currentValue / divisor;

            quotientSlots[quotientPos].slotText.text = q.ToString();
            quotientPos++;

            instructionText.text = $"Divide: {currentValue} ÷ {divisor} = {q}";

            stepIndex = 3;
            return;
        }

        // STEP 3 – Multiply q × divisor and write to row with minus sign
        if (stepIndex == 3)
        {
            int q = int.Parse(quotientSlots[quotientPos - 1].slotText.text);
            int product = q * divisor;

            // Write minus sign first
            WriteTextToRow("-", answerRowIndex, currentColumn);
            // Then write the product
            WriteNumberToRow(product, answerRowIndex, currentColumn + 1);

            instructionText.text = $"{q} × {divisor} = {product}";

            stepIndex = 4;
            return;
        }

        // STEP 4 – Subtract and show result
        if (stepIndex == 4)
        {
            int q = int.Parse(quotientSlots[quotientPos - 1].slotText.text);
            int product = q * divisor;

            int remainder = currentValue - product;
            currentValue = remainder;

            answerRowIndex++;

            // Write the subtraction result (remainder)
            WriteNumberToRow(remainder, answerRowIndex, currentColumn + 1);

            instructionText.text = $"Subtract: {currentValue + product} - {product} = {remainder}";

            stepIndex = 5;
            return;
        }

        // STEP 5 – Bring down next digit
        if (stepIndex == 5)
        {
            if (dividendPos < dividend.Length)
            {
                answerRowIndex++;

                // Write the brought down digit aligned properly
                int broughtDown = dividend[dividendPos];
                int newValue = currentValue * 10 + broughtDown;

                WriteNumberToRow(newValue, answerRowIndex, currentColumn + 1);

                HighlightDividendDigit(dividendPos);
                currentValue = newValue;
                dividendPos++;

                instructionText.text = $"Bring down {broughtDown} → now working with {currentValue}";

                stepIndex = 1;
            }
            else
            {
                // Show final remainder with R notation
                if (currentValue > 0)
                {
                    answerRowIndex++;
                    WriteNumberToRow(currentValue, answerRowIndex, currentColumn + 1);
                    WriteTextToRow(" R", answerRowIndex, currentColumn + 1 + GetDigitCount(currentValue));
                }
                instructionText.text = $"Done! Remainder = {currentValue}";
            }

            return;
        }
    }

    bool LoadNumbers()
    {
        // READ DIVISOR from 1–4 digits
        var divisorList = new System.Collections.Generic.List<int>();

        foreach (var slot in divisorSlots)
        {
            if (TryReadDigit(slot, out int v))
                divisorList.Add(v);
        }

        if (divisorList.Count == 0)
        {
            instructionText.text = "Please enter at least ONE divisor digit.";
            return false;
        }

        divisor = 0;
        foreach (int d in divisorList)
            divisor = divisor * 10 + d;

        // READ DIVIDEND from 1–4 digits
        var dividendList = new System.Collections.Generic.List<int>();

        foreach (var slot in dividendSlots)
        {
            if (TryReadDigit(slot, out int v))
                dividendList.Add(v);
        }

        if (dividendList.Count == 0)
        {
            instructionText.text = "Please enter at least ONE dividend digit.";
            return false;
        }

        dividend = dividendList.ToArray();

        instructionText.text = $"Dividing {string.Join("", dividendList)} by {divisor}";
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

    void WriteNumberToRow(int value, int row, int startCol)
    {
        if (row >= answerRows.Length)
            return;

        string s = value.ToString();

        for (int i = 0; i < s.Length && (startCol + i) < answerRows[row].Length; i++)
        {
            answerRows[row][startCol + i].slotText.text = s[i].ToString();
        }
    }

    void WriteTextToRow(string text, int row, int col)
    {
        if (row >= answerRows.Length || col >= answerRows[row].Length)
            return;

        answerRows[row][col].slotText.text = text;
    }

    int GetDigitCount(int value)
    {
        if (value == 0) return 1;
        return Mathf.FloorToInt(Mathf.Log10(value) + 1);
    }
}