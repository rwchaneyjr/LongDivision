using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Controls step-by-step long multiplication logic
/// and writes final result into the bottom row.
/// CLEAN + SAFE version
/// </summary>
public class LongMath : MonoBehaviour
{
    enum StepPhase
    {
        Highlight,
        Multiply,
        WriteOnes,
        WriteCarry,
        Advance
    }

    StepPhase stepPhase = StepPhase.Highlight;

    // ================= STATE =================
    int topIndexFromRight;
    int bottomIndexFromRight;
    int answerRowIndex;
    int carry;
    int product;

    int topNumber;
    int bottomNumber;
    bool numbersLoaded;

    int topStartColumn;
    int bottomStartColumn;
    int topLength;
    int bottomLength;

    int onesAnswerColumn;
    bool showedPlusForSum;

    // ================= REFERENCES =================
    [Header("Builder")]
    public LMBuilder lmBuilder;

    [Header("Input Rows")]
    public DigitDropSlot[] topInputSlots;
    public DigitDropSlot[] bottomInputSlots;

    [Header("Carry Row")]
    public DigitDropSlot[] carrySlots;

    [Header("Answer Rows (partial)")]
    public DigitDropSlot[][] answerRows;

    [Header("Final Answer Row")]
    public DigitDropSlot[] finalAnswerSlots;

    [Header("Instruction Text")]
    public TextMeshProUGUI instructionText;

    // ======================================================
    void Start()
    {
        if (lmBuilder == null)
            lmBuilder = FindObjectOfType<LMBuilder>();

        ResetState();
    }

    // ======================================================
    public void StepThrough()
    {
        if (!numbersLoaded && !LoadNumbers())
            return;

        RunStep();
    }

    // ======================================================
    bool LoadNumbers()
    {
        topNumber = BuildNumber(topInputSlots, out topStartColumn, out topLength);
        bottomNumber = BuildNumber(bottomInputSlots, out bottomStartColumn, out bottomLength);

        if (topNumber < 0 || bottomNumber < 0)
        {
            instructionText.text = "Fill in both numbers first.";
            return false;
        }

        // rebuild answer rows
       // lmBuilder.SpawnRowsForBottomNumber(bottomNumber);

       // answerRows = lmBuilder.longMath.answerRows;
        //finalAnswerSlots = lmBuilder.longMath.finalAnswerSlots;

        ResetState();

        if (answerRows == null || answerRows.Length == 0)
        {
            instructionText.text = "Answer rows not ready.";
            return false;
        }

        onesAnswerColumn = answerRows[0].Length - 1;
        numbersLoaded = true;

        // ✅ INITIAL COLORED EQUATION TEXT
        ShowColoredEquation();

        return true;
    }

    // ======================================================
    void ResetState()
    {
        topIndexFromRight = 0;
        bottomIndexFromRight = 0;
        answerRowIndex = 0;
        carry = 0;
        product = 0;

        numbersLoaded = false;
        showedPlusForSum = false;

        ClearCarryRow();
        ClearAnswerRows();

        stepPhase = StepPhase.Highlight;
    }

    // ======================================================
    void RunStep()
    {
        ClearAllHighlights();

        // ✅ FINISHED MULTIPLICATION
        if (bottomIndexFromRight >= bottomLength)
        {
            if (!showedPlusForSum)
            {
                ShowPlusSignForAddition();
                showedPlusForSum = true;
                instructionText.text = "Now add the partial products.";
                return;
            }

            WriteFinalAnswer();
            instructionText.text = "Multiplication complete.";
            return;
        }

        int topDigit = GetDigit(topNumber, topIndexFromRight);
        int bottomDigit = GetDigit(bottomNumber, bottomIndexFromRight);

        int topUI = topStartColumn + (topLength - 1 - topIndexFromRight);
        int bottomUI = bottomStartColumn + (bottomLength - 1 - bottomIndexFromRight);
        int answerUI = onesAnswerColumn - topIndexFromRight - answerRowIndex;
        int carryUI = topUI - 1;

        switch (stepPhase)
        {
            case StepPhase.Highlight:

                // ✅ SHOW × EXACTLY ON FIRST STEP
                if (topIndexFromRight == 0 && bottomIndexFromRight == 0)
                    ShowMultiplySignForBottomRow();

                Highlight(topUI, bottomUI);
                ShowColoredEquation();

                stepPhase = StepPhase.Multiply;
                break;

            case StepPhase.Multiply:
                product = topDigit * bottomDigit + carry;

                string coloredProduct = GetColoredProduct(product);

                instructionText.text =
                    topDigit + " × " + bottomDigit + " + " + carry + " = " + coloredProduct;

                stepPhase = StepPhase.WriteOnes;
                break;

            case StepPhase.WriteOnes:
                WriteAnswerDigit(answerRowIndex, answerUI, product % 10);
                carry = product / 10;
                stepPhase = carry > 0 ? StepPhase.WriteCarry : StepPhase.Advance;
                break;

            case StepPhase.WriteCarry:
                if (carryUI >= 0 && carryUI < carrySlots.Length)
                {
                    carrySlots[carryUI].slotText.text = carry.ToString();
                    carrySlots[carryUI].SetHighlight(Color.cyan);
                }
                stepPhase = StepPhase.Advance;
                break;

            case StepPhase.Advance:
                topIndexFromRight++;

                if (topIndexFromRight >= topLength)
                {
                    topIndexFromRight = 0;
                    bottomIndexFromRight++;
                    answerRowIndex = Mathf.Min(answerRowIndex + 1, answerRows.Length - 1);
                    carry = 0;
                    ClearCarryRow();
                }

                stepPhase = StepPhase.Highlight;
                break;
        }
    }

    // ======================================================
    void ShowColoredEquation()
    {
        instructionText.text =
            "<color=#FFD34D>" + topNumber + "</color>" +   // yellow
            " <color=#FFFFFF>×</color> " +
            "<color=#3A8DFF>" + bottomNumber + "</color>"; // aqua
    }

    // ======================================================
    int BuildNumber(DigitDropSlot[] slots, out int start, out int length)
    {
        start = -1;
        int end = -1;

        for (int i = 0; i < slots.Length; i++)
        {
            var t = slots[i]?.slotText?.text;
            if (!string.IsNullOrEmpty(t) && char.IsDigit(t[0]))
            {
                if (start == -1) start = i;
                end = i;
            }
        }

        if (start == -1)
        {
            length = 0;
            return -1;
        }

        length = end - start + 1;
        int value = 0;

        for (int i = start; i <= end; i++)
            value = value * 10 + (slots[i].slotText.text[0] - '0');

        return value;
    }

    int GetDigit(int number, int indexFromRight)
    {
        return (number / (int)Mathf.Pow(10, indexFromRight)) % 10;
    }

    // ======================================================
    void ShowMultiplySignForBottomRow()
    {
        for (int i = 0; i < bottomInputSlots.Length - 1; i++)
        {
            if (!string.IsNullOrEmpty(bottomInputSlots[i + 1].slotText.text))
            {
                bottomInputSlots[i].slotText.text = "×";
                bottomInputSlots[i].SetHighlight(Color.cyan);
                return;
            }
        }
    }

    void ShowPlusSignForAddition()
    {
        var lastRow = answerRows[answerRows.Length - 1];
        for (int i = 0; i < lastRow.Length - 1; i++)
        {
            if (!string.IsNullOrEmpty(lastRow[i + 1].slotText.text))
            {
                lastRow[i].slotText.text = "+";
                return;
            }
        }
    }

    // ======================================================
    void WriteAnswerDigit(int row, int col, int digit)
    {
        var slot = answerRows[row][col];
        slot.slotText.text = digit.ToString();
        slot.SetHighlight(Color.yellow);
    }

    void WriteFinalAnswer()
    {
        int result = topNumber * bottomNumber;

        for (int i = finalAnswerSlots.Length - 1; i >= 0; i--)
        {
            finalAnswerSlots[i].slotText.text = result > 0 ? (result % 10).ToString() : "";
            result /= 10;
        }
    }
    // Returns the product number with colored digits for TextMeshPro
    string GetColoredProduct(int value)
    {
        string s = value.ToString();

        // Two-digit number: tens aqua, ones yellow
        if (s.Length == 2)
        {
            string tens = s[0].ToString();
            string ones = s[1].ToString();

            return
                "<color=#3A8DFF>" + tens + "</color>" +    // aqua (5)
                "<color=#FFD34D>" + ones + "</color>";     // yellow (4)
        }

        // One digit – just make it yellow
        if (s.Length == 1)
        {
            return "<color=#FFD34D>" + s + "</color>";
        }

        // Fallback for 3+ digits (no special coloring)
        return s;
    }

    void Highlight(int t, int b)
    {
        topInputSlots[t]?.SetHighlight(Color.yellow);
        bottomInputSlots[b]?.SetHighlight(Color.cyan);
    }

    void ClearCarryRow()
    {
        foreach (var s in carrySlots)
        {
            if (s?.slotText != null)
                s.slotText.text = "C";
            s?.ClearHighlight();
        }
    }

    void ClearAnswerRows()
    {
        if (answerRows == null) return;
        if (answerRows.Length == 0) return;

        foreach (var row in answerRows)
        {
            if (row == null) continue;

            foreach (var s in row)
            {
                if (s?.slotText != null)
                    s.slotText.text = "";
            }
        }
    }

    void ClearAllHighlights()
    {
        foreach (var s in topInputSlots) s?.ClearHighlight();
        foreach (var s in bottomInputSlots) s?.ClearHighlight();
        foreach (var s in carrySlots) s?.ClearHighlight();

        if (answerRows != null)
            foreach (var row in answerRows)
                foreach (var s in row) s?.ClearHighlight();
    }


    public void NotifySlotFilled(DigitDropSlot slot) { }
}
