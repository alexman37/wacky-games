using UnityEngine;
using UnityEngine.U2D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Games.Minesweeper
{
    /// <summary>
    /// This class manages high-level game stats and occurances, e.g,
    /// Switching between scenes (If eligible), starting new games under certain conditions, etc.
    /// </summary>
    public class MinesweeperManager : MonoBehaviour
    {
        public static MinesweeperManager instance;

        public RoundState state;
        public GameObject leaveGameWarningPanel;

        // Start is called before the first frame update
        void Start()
        {
            if (instance == null)
            {
                instance = this;
                this.gameObject.AddComponent<MinesweeperEventsManager>();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }

            StartCoroutine(startupRoutine());
        }

        // Wait for start methods of all essential scripts to finish creating before we begin the game.
        IEnumerator startupRoutine()
        {
            while(!MinesweeperStyles.greenlight ||
                !MinesweeperTopBarUI.greenlight ||
                !GridManager.greenlight)
            {
                yield return null;
            }

            Debug.Log("All services started - beginning game!");
            resetState(100, 10);
            MinesweeperEventsManager.instance.dispatch_startNewGame(100, 10);
            GridManager.Instance.GenerateGrid(10, 10, 10);
        }

        // Subscriptions to events
        private void OnEnable()
        {
            MinesweeperEventsManager.startNewGame += resetState;
            MinesweeperEventsManager.revealedXtiles += revealedXTiles;
            MinesweeperEventsManager.flagPlaced += flagPlacedOrRemoved;
            MinesweeperEventsManager.gameWon += onWin;
            MinesweeperEventsManager.gameLost += onLose;
        }

        private void OnDisable()
        {
            MinesweeperEventsManager.startNewGame -= resetState;
            MinesweeperEventsManager.revealedXtiles -= revealedXTiles;
            MinesweeperEventsManager.gameWon -= onWin;
            MinesweeperEventsManager.gameLost -= onLose;
        }


        public bool isAlive()
        {
            return state.alive;
        }



        void resetState(int numTiles, int numMines)
        {
            state = new RoundState(numTiles, numMines);
        }

        // Assumes you didn't blow up and die
        void revealedXTiles(int numRevealed)
        {
            state.numTilesRevealed += numRevealed;
            Debug.Log("How many revealed total? " + state.numTilesRevealed);

            // Win condition: You have revealed all the tiles that aren't mines
            if (state.numTilesRevealed >= state.numTilesTotal - state.numMines)
            {
                MinesweeperEventsManager.instance.dispatch_gameWon();
            }
        }

        void flagPlacedOrRemoved(bool placed)
        {
            if (placed) state.numFlagsPlaced += 1;
            else state.numFlagsPlaced -= 1;
            MinesweeperTopBarUI.instance.updateFlagDisplay(state.numFlagsPlaced, state.numMines);
        }

        void onWin()
        {
            // todo stats tracking?
            Debug.Log("You won, yay!");
            state.alive = true;
        }

        void onLose()
        {
            // todo stats tracking?
            Debug.Log("You lost, boooo");
            state.alive = false;
        }

        // Called by the leave game button to return you to the select game scene.
        // Maybe it should warn you first?
        public void leaveGameClicked()
        {
            leaveGameWarningPanel.SetActive(true);
            GridManager.Instance.DisablePlayerMovement();
        }

        public void playerWantsToKeepTrying()
        {
            leaveGameWarningPanel.SetActive(false);
            GridManager.Instance.EnablePlayerMovement();
        }

        public void trulyLeaveGame()
        {
            SceneManager.LoadScene("SelectGameScreen");
        }
    }

    // Unique to each round
    public class RoundState
    {
        public int numTilesTotal;
        public int numMines;
        public int numTilesRevealed;
        public int numFlagsPlaced;
        public bool alive;

        public RoundState(int numTotal, int mines)
        {
            numTilesTotal = numTotal;
            numMines = mines;
            numTilesRevealed = 0;
            numFlagsPlaced = 0;
            alive = true;
        }
    }

}