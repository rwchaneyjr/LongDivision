using UnityEngine;
using TMPro;

public class LongDivision : MonoBehaviour
{
    [Header("LMBuilder Connections")]
    public DigitDropSlot[] topInputSlots;       // quotient row (top)
    public DigitDropSlot[] bottomInputSlots;    // dividend row
    public DigitDropSlot[][] answerRows;        // subtraction rows
    public DigitDropSlot[] finalAnswerSlots;    // remainder row

    [Header("UI")]
    public TMP_Text divisorLabel;               // "46 )"
    public TMP_Text instructionText;

    // internal state
    int divisor;
    int[] dividendDigits;
    int stepIndex = 0;

    void Start()
    {
        instructionText.text = "Fill dividend, divisor, then press Step.";
    }

    // ------------------------------------------------------
    // STEP BUTTON
    // ------------------------------------------------------
    public void Step()
    {
        if (!LoadNumbers())
            return;

        switch (stepIndex)
        {
            case 0: StepDivideFirst(); break;
            case 1: StepSubtract(); break;
            case 2: StepBringDown(); break;
            default:
                instructionText.text = "Division complete.";
                break;
        }

        stepIndex++;
    }

    // ------------------------------------------------------
    // READ DIVISOR + DIVIDEND FROM UI SLOTS
    // ------------------------------------------------------
    bool LoadNumbers()
    {
        // ---------- Divisor ----------
        string ds = divisorLabel.text.Replace(")", "").Trim();
        if (!int.TryParse(ds, out divisor))
        {
            instructionText.text = "Invalid divisor.";
            return false;
        }

        // ---------- Dividend ----------
        string s = "";
        foreach (var slot in bottomInputSlots)
        {
            if (!string.IsNullOrWhiteSpace(slot.slotText.text))
                s += slot.slotText.text;
        }

        if (s.Length == 0 || !int.TryParse(s, out _))
        {
            instructionText.text = "Place digits for dividend.";
            return false;
        }

        // Convert digits → array
        dividendDigits = new int[s.Length];
        for (int i = 0; i < s.Length; i++)
            dividendDigits[i] = s[i] - '0';

        return true;
    }

    // ------------------------------------------------------
    // STEP 1 — Divide first two digits
    // Example: 46 ) 2534 → take 25
    // ------------------------------------------------------
    void StepDivideFirst()
    {
        if (dividendDigits.Length < 2)
        {
            instructionText.text = "Dividend too small.";
            return;
        }

        int first = dividendDigits[0];
        int second = dividendDigits[1];
        int leading = first * 10 + second;  // 25

        int q = leading / divisor;          // 0 in this example
        int product = q * divisor;          // 0

        // place quotient over second digit position
        topInputSlots[1].slotText.text = q.ToString();

        // place product row
        answerRows[0][1].slotText.text = product.ToString();

        instructionText.text = $"{leading} ÷ {divisor} = {q}";
    }

    // ------------------------------------------------------
    // STEP 2 — Subtract
    // Example: 25 - 0 = 25
    // ------------------------------------------------------
    void StepSubtract()
    {
        int leading = dividendDigits[0] * 10 + dividendDigits[1];
        int q = leading / divisor;
        int product = q * divisor;

        int remainder = leading - product;

        answerRows[1][1].slotText.text = remainder.ToString();

        instructionText.text = $"{leading} − {product} = {remainder}";
    }

    // ------------------------------------------------------
    // STEP 3 — Bring down next digit
    // Example: remainder 25 → bring down digit 3 → 253
    // ------------------------------------------------------
    void StepBringDown()
    {
        int leading = dividendDigits[0] * 10 + dividendDigits[1];
        int q = leading / divisor;
        int product = q * divisor;
        int remainder = leading - product;

        int nextDigit = dividendDigits.Length > 2 ? dividendDigits[2] : 0;

        int newNumber = remainder * 10 + nextDigit;

        answerRows[2][2].slotText.text = newNumber.ToString();

        instructionText.text = $"Bring down {nextDigit} → {newNumber}";
    }
}
