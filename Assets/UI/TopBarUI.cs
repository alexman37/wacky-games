using UnityEngine;
using TMPro;

public class TopBarUI : MonoBehaviour
{
    public static TopBarUI instance;

    // shape select
    public ButtonGroup shapeSelectGroup;

    // row, col, mine input
    public const int MAX_ROWS = 50;
    public const int MAX_COLS = 50;
    public const int MAX_MINES = 100;

    public int rowsInput = 10;
    public int colsInput = 10;
    public int minesInput = 10;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (instance == null) instance = this;

        rowsInput = 10;
        colsInput = 10;
        minesInput = 10;
    }

    public void updateNumRows(string rawInput) {
        rowsInput = attemptToParseIntFromInput(rawInput, 10, 5, MAX_ROWS);
    }

    public void updateNumCols(string rawInput)
    {
        colsInput = attemptToParseIntFromInput(rawInput, 10, 5, MAX_COLS);
    }

    public void updateNumMines(string rawInput)
    {
        minesInput = attemptToParseIntFromInput(rawInput, 10, 5, MAX_MINES);
    }

    private int attemptToParseIntFromInput(string input, int fallback, int minAllowed, int maxAllowed)
    {
        int attempt;
        bool success = int.TryParse(input, out attempt);

        if (success)
        {
            return Mathf.Clamp(attempt, minAllowed, maxAllowed);
        }
        else
        {
            Debug.LogWarning($"Failed to parse {input} into an integer - reverting to {fallback}");
            return fallback;
        }
    }
}
