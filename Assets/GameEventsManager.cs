using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// This class handles dispatching high-level game events.
/// E.g, "New game", "Game over", "Game won", etc...
/// </summary>
public class GameEventsManager : MonoBehaviour
{
    public static GameEventsManager instance;

    public static event Action<int, int> startNewGame; // pass in: the number of tiles, the number of mines
    public static event Action<int> revealedXtiles; // pass in how many tiles were revealed
    public static event Action<bool> flagPlaced;
    public static event Action gameWon;
    public static event Action gameLost;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);

        startNewGame += (_,__) => { };
        revealedXtiles += (_) => { };
        flagPlaced += (_) => { };
        gameWon += () => { };
        gameLost += () => { };
    }

    public void dispatch_startNewGame(int numTiles, int numMines)
    {
        startNewGame.Invoke(numTiles, numMines);
    }

    public void dispatch_revealedXtiles(int numTiles)
    {
        revealedXtiles.Invoke(numTiles);
    }

    public void dispatch_flagged(bool placed)
    {
        flagPlaced.Invoke(placed);
    }

    public void dispatch_gameWon()
    {
        gameWon.Invoke();
    }

    public void dispatch_gameLost()
    {
        gameLost.Invoke();
    }
}
