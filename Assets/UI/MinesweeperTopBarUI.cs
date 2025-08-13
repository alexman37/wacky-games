using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

namespace Games.Minesweeper
{
   
    public class MinesweeperTopBarUI : MonoBehaviour
    {
        public static MinesweeperTopBarUI instance;
        public static bool greenlight = false;

        // smiley, flags remaining, time remaining
        public Image smiley;
        public TextMeshProUGUI flagsRemaining;
        public TextMeshProUGUI timeElapsed;

        // We will keep track time here as opposed to gameManager to avoid the hassle of communicating every waking moment
        private int timeSeconds = 0;
        private IEnumerator timingCoroutine;

        // style choices - changes with selected shape
        public ButtonGroup styleSelectGroup;

        // shape select
        public ButtonGroup shapeSelectGroup;
        private int currShape;
        private static bool initialLoad = true; // needed to prevent shape/style groups from loading the same asset at the start

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

            // Define callbacks for shape and style selection.
            shapeSelectGroup.selectedIndexEvent = shapeSelectCallback;
            shapeSelectGroup.setInitialValue();

            styleSelectGroup.selectedIndexEvent = styleSelectCallback;
            styleSelectGroup.setInitialValue();

            greenlight = true;
        }

        private void OnEnable()
        {
            MinesweeperEventsManager.startNewGame += resetDisplay;
            MinesweeperEventsManager.gameWon += winDisplay;
            MinesweeperEventsManager.gameLost += lossDisplay;
        }

        private void OnDisable()
        {
            MinesweeperEventsManager.startNewGame -= resetDisplay;
            MinesweeperEventsManager.gameWon -= winDisplay;
            MinesweeperEventsManager.gameLost -= lossDisplay;
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



        public void updateNumRows(string rawInput)
        {
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
            MinesweeperEventsManager.instance.dispatch_startNewGame(rowsInput * colsInput, minesInput);
        }


        // Shape select callback
        public void shapeSelectCallback(int index)
        {
            Debug.Log("Changed shape to " + index);
            currShape = index;
            string shapeName = "", styleName = "";
            switch(index)
            {
                case 0: // square
                    shapeName = "square";
                    styleName = MinesweeperStyles.instance.squareStyleNames[index];
                    break;
                case 1: // hex
                    shapeName = "hex";
                    styleName = MinesweeperStyles.instance.hexStyleNames[index];
                    break;
            }

            StartCoroutine(loadStylesOfShape(shapeName));
            if(!initialLoad) MinesweeperStyles.instance.NEXT_useNewStyleSheet(shapeName, styleName);
            initialLoad = false;
        }

        IEnumerator loadStylesOfShape(string shape)
        {
            // load all style icons and replace the current buttons with as many of them as available
            yield return LoadAssetBundle.LoadBundle("minesweeper/style-viz/" + shape, (Sprite[] sprites) =>
            {
                int i = 0;
                for (i = 0; i < sprites.Length; i++)
                {
                    styleSelectGroup.gameObject.transform.GetChild(i).gameObject.SetActive(true);
                    styleSelectGroup.gameObject.transform.GetChild(i).GetComponent<Image>().sprite = sprites[i];
                }
                for (; i < 12; i++)
                {
                    styleSelectGroup.gameObject.transform.GetChild(i).gameObject.SetActive(false);
                }
            });
        }

        // Style select callback
        public void styleSelectCallback(int index)
        {
            Debug.Log("Changed style to " + index);

            string styleName = "", shapeName = "";
            switch(currShape)
            {
                case 0:
                    shapeName = "square";
                    styleName = MinesweeperStyles.instance.squareStyleNames[index]; break;
                case 1:
                    shapeName = "hex";
                    styleName = MinesweeperStyles.instance.hexStyleNames[index]; break;
            }

            // If a style of the same shape is selected, start using that shape immediately
            if ((int) GridManager.Instance.tileType == currShape)
            {
                MinesweeperStyles.instance.useNewStyleSheet(shapeName, styleName);
            } 
            // For a different shape, "save" the style you want to use, prepping it for the next generation
            else
            {
                MinesweeperStyles.instance.NEXT_useNewStyleSheet(shapeName, styleName);
            }
            
        }
    }

}