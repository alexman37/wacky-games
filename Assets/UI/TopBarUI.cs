using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class TopBarUI : MonoBehaviour
{
    public static TopBarUI instance;

    // smiley, flags remaining, time remaining
    public Image smiley;
    public TextMeshProUGUI flagsRemaining;
    public TextMeshProUGUI timeElapsed;

    // We will keep track time here as opposed to gameManager to avoid the hassle of communicating every waking moment
    private int timeSeconds = 0;
    private IEnumerator timingCoroutine;

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
        else if (instance != this) Destroy(gameObject);

        rowsInput = 10;
        colsInput = 10;
        minesInput = 10;
    }

    private void OnEnable()
    {
        GameEventsManager.startNewGame += resetDisplay;
        GameEventsManager.gameWon += winDisplay;
        GameEventsManager.gameLost += lossDisplay;
    }

    private void OnDisable()
    {
        GameEventsManager.startNewGame -= resetDisplay;
        GameEventsManager.gameWon -= winDisplay;
        GameEventsManager.gameLost -= lossDisplay;
    }

    private void resetDisplay(int numTotal, int numMines)
    {
        flagsRemaining.text = $"0 / {numMines}";
        timeElapsed.text = "0:00";

        if (timingCoroutine != null) StopCoroutine(timingCoroutine);
        timingCoroutine = trackTime();
        StartCoroutine(timingCoroutine);
    }

    public void updateFlagDisplay(int numFlags, int numMines)
    {
        flagsRemaining.text = $"{numFlags} / {numMines}";
    }

    private void winDisplay()
    {
        flagsRemaining.text = "You won :)";
    }

    private void lossDisplay()
    {
        flagsRemaining.text = "You lost :(";
    }



    private IEnumerator trackTime()
    {
        string formatted = $"{timeSeconds / 60}:" + ((timeSeconds % 60) > 9 ? timeSeconds % 60 : "0" + (timeSeconds % 60));
        timeElapsed.text = formatted;
        yield return new WaitForSeconds(1);
        timeSeconds += 1;
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


    public void kickoffRegen()
    {
        // always a rectangle for now
        // TODO be smarter about this
        GridManager.Instance.ButtonRegenGrid();
        GameEventsManager.instance.dispatch_startNewGame(rowsInput * colsInput, minesInput);
    }
}
