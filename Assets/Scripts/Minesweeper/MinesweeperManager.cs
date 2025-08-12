using UnityEngine;
using UnityEngine.U2D;
using System.Collections;
using System.Collections.Generic;

namespace Games.Minesweeper
{
    /// <summary>
    /// This class manages high-level game stats and occurances, e.g,
    /// Switching between scenes (If eligible), starting new games under certain conditions, etc.
    /// </summary>
    public class MinesweeperManager : MonoBehaviour
    {
        public static MinesweeperManager instance;

        public Tile baseTile;
        public SpriteAtlas spriteAtlas;

        public RoundState state;


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

            Debug.Log("Generating initial grid");
            // TODO - be smarter about starting a game versus starting generation
            resetState(100, 10);
            GridManager.Instance.GenerateGrid(10, 10, 10);

            // TODO set initial sprite sheet
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