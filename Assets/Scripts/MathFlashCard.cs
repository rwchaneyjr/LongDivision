using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MathFlashCard : MonoBehaviour
{
    // ---------------- UI ----------------
    [Header("Flash Card UI")]
    public Button flashCardButton;          // click to show answer / next
    public TextMeshProUGUI flashCardText;   // shows "3 × 4" then "= 12"
    public TextMeshProUGUI timesTableLabel; // shows "Table: 5  Mode: ×"

    [Header("Mode Buttons")]
    public Button addButton;        // +
    public Button subtractButton;   // -
    public Button multiplyButton;   // ×
    public Button divideButton;     // ÷
    public Button randomButton;     // random of all 4

    [Header("Table Buttons")]
    public Button upTableButton;    // increase tableNumber
    public Button downTableButton;  // decrease tableNumber

    [Header("Settings")]
    public int tableNumber = 2;   // base number (2, 3, 4, ..., 20)
    public int maxValue = 20;     // go from 1 to 20

    private int a, b, answer;
    private bool showingAnswer = false;

    // current step in the table (1 → maxValue)
    private int currentStep = 1;

    public enum Operation
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Random
    }

    private Operation currentOperation = Operation.Multiply;

    void Start()
    {
        // Main flash card click
        if (flashCardButton != null)
            flashCardButton.onClick.AddListener(OnFlashCardClicked);

        // Mode buttons
        if (addButton != null) addButton.onClick.AddListener(SetAdd);
        if (subtractButton != null) subtractButton.onClick.AddListener(SetSubtract);
        if (multiplyButton != null) multiplyButton.onClick.AddListener(SetMultiply);
        if (divideButton != null) divideButton.onClick.AddListener(SetDivide);
        if (randomButton != null) randomButton.onClick.AddListener(SetRandom);

        // Table up/down buttons
        if (upTableButton != null) upTableButton.onClick.AddListener(IncreaseTable);
        if (downTableButton != null) downTableButton.onClick.AddListener(DecreaseTable);

        UpdateTimesTableLabel();
        ResetSequence();
    }

    // --------- MODE BUTTON HANDLERS ---------
    public void SetAdd() { SetOperation(Operation.Add); }
    public void SetSubtract() { SetOperation(Operation.Subtract); }
    public void SetMultiply() { SetOperation(Operation.Multiply); }
    public void SetDivide() { SetOperation(Operation.Divide); }
    public void SetRandom() { SetOperation(Operation.Random); }

    void SetOperation(Operation op)
    {
        currentOperation = op;
        ResetSequence();
    }

    // --------- TABLE BUTTON HANDLERS ---------
    public void IncreaseTable()
    {
        tableNumber++;
        if (tableNumber > maxValue) tableNumber = maxValue;
        ResetSequence();
    }

    public void DecreaseTable()
    {
        tableNumber--;
        if (tableNumber < 1) tableNumber = 1;
        ResetSequence();
    }

    void ResetSequence()
    {
        currentStep = 1;
        showingAnswer = false;
        UpdateTimesTableLabel();
        GenerateNewProblem();
    }

    // --------- LABEL UPDATE ---------
    void UpdateTimesTableLabel()
    {
        if (timesTableLabel == null) return;

        string opSymbol = currentOperation switch
        {
            Operation.Add => "+",
            Operation.Subtract => "−",
            Operation.Multiply => "×",
            Operation.Divide => "÷",
            Operation.Random => "Random",
            _ => "?"
        };

        timesTableLabel.text = $"Table: {tableNumber}   Mode: {opSymbol}";
    }

    // --------- PROBLEM GENERATION ---------
    void GenerateNewProblem()
    {
        showingAnswer = false;

        if (flashCardText == null)
            return;

        // Random mode: ignore table/step, just pick random op & numbers
        if (currentOperation == Operation.Random)
        {
            GenerateRandomProblem();
            return;
        }

        GenerateSequentialProblem();
    }

    // Sequential: tableNumber with step 1..maxValue
    void GenerateSequentialProblem()
    {
        a = tableNumber;
        int safety = 0; // avoid infinite loops

        while (true)
        {
            if (safety++ > maxValue + 5)
            {
                // fallback if something goes weird
                a = tableNumber;
                b = 1;
                answer = a;
                flashCardText.text = $"{a} ? 1";
                return;
            }

            b = currentStep;

            switch (currentOperation)
            {
                case Operation.Add:
                    // e.g. 2 + 1, 2 + 2, ..., 2 + 20
                    answer = a + b;
                    flashCardText.text = $"{a} + {b}";
                    return;

                case Operation.Subtract:
                    // e.g. 20 − 1, 20 − 2, ... (skip negative results)
                    answer = a - b;
                    if (answer < 0)
                    {
                        AdvanceStepSequential();
                        continue; // try next step
                    }
                    flashCardText.text = $"{a} − {b}";
                    return;

                case Operation.Multiply:
                    // e.g. 2 × 1, 2 × 2, ..., 2 × 20
                    answer = a * b;
                    flashCardText.text = $"{a} × {b}";
                    return;

                case Operation.Divide:
                    // e.g. 20 ÷ 1, 20 ÷ 2, 20 ÷ 4, 20 ÷ 5, 20 ÷ 10, 20 ÷ 20
                    if (b == 0 || a % b != 0)
                    {
                        AdvanceStepSequential();
                        continue; // try next b that divides a
                    }

                    answer = a / b;
                    flashCardText.text = $"{a} ÷ {b}";
                    return;
            }
        }
    }

    // Random facts using all four operations
    void GenerateRandomProblem()
    {
        Operation randomOp = (Operation)Random.Range(0, 4); // 0..3 (Add..Divide)

        int x, y;

        switch (randomOp)
        {
            case Operation.Add:
                x = Random.Range(1, maxValue + 1);
                y = Random.Range(1, maxValue + 1);
                answer = x + y;
                flashCardText.text = $"{x} + {y}";
                break;

            case Operation.Subtract:
                // keep non-negative
                x = Random.Range(1, maxValue + 1);
                y = Random.Range(1, x + 1);
                answer = x - y;
                flashCardText.text = $"{x} − {y}";
                break;

            case Operation.Multiply:
                x = Random.Range(1, maxValue + 1);
                y = Random.Range(1, maxValue + 1);
                answer = x * y;
                flashCardText.text = $"{x} × {y}";
                break;

            case Operation.Divide:
                y = Random.Range(1, maxValue + 1);      // divisor
                int q = Random.Range(1, maxValue + 1);  // quotient
                x = y * q;                              // dividend
                answer = q;
                flashCardText.text = $"{x} ÷ {y}";
                break;
        }
    }

    // --------- FLASH CARD CLICK ---------
    void OnFlashCardClicked()
    {
        if (!showingAnswer)
        {
            flashCardText.text += $"\n= {answer}";
            showingAnswer = true;
        }
        else
        {
            // Next fact
            if (currentOperation == Operation.Random)
            {
                GenerateNewProblem(); // new random problem
            }
            else
            {
                AdvanceStepSequential();
                GenerateNewProblem();
            }
        }
    }

    // Move 1 → 2 → 3 → ... → maxValue → back to 1
    void AdvanceStepSequential()
    {
        currentStep++;
        if (currentStep > maxValue)
            currentStep = 1;
    }
}
