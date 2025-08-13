using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectGameManager : MonoBehaviour
{
    public static SelectGameManager instance;
    private SelectedGame gameSelected = SelectedGame.NONE;
    public List<GameObject> buttonObjects = new List<GameObject>();
    private bool IsLeaderboardOpen = false; // If the leaderboard is open, StartGame is disabled.
    public SelectGameManager()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void SelectMinesweeper()
    {
        // Minesweeper is already selected, so we set it to SelectedGame.NONE and remove highlights.
        if (gameSelected == SelectedGame.MINESWEEPER)
        {
            gameSelected = SelectedGame.NONE;
            RemoveHighlights();
            if (IsLeaderboardOpen)
            {
                CloseLeaderboard(); // Close the leaderboard if it is open.
            }
            return;
        }
        gameSelected = SelectedGame.MINESWEEPER;
        HighlightGameSelected();
        if (IsLeaderboardOpen)
        {
            RefreshLeaderboard(); // Refresh the leaderboard if it is open.
        }
    }

    public void SelectBattleship()
    {
        // Battleship is already selected, so we set it to SelectedGame.NONE and remove highlights.
        if (gameSelected == SelectedGame.BATTLESHIP)
        {
            gameSelected = SelectedGame.NONE;
            RemoveHighlights();
            if (IsLeaderboardOpen)
            {
                CloseLeaderboard(); // Close the leaderboard if it is open.
            }
            return;
        }
        gameSelected = SelectedGame.BATTLESHIP;
        HighlightGameSelected();
        if (IsLeaderboardOpen)
        {
            RefreshLeaderboard(); // Refresh the leaderboard if it is open.
        }
    }

    // For placeholder buttons
    public void SelectUndefinedGame()
    {
        gameSelected = SelectedGame.NONE;
        RemoveHighlights();
        if (IsLeaderboardOpen)
        {
            CloseLeaderboard(); // Close the leaderboard if it is open.
        }
    }

    public void RemoveHighlights()
    {
        foreach(GameObject go in buttonObjects)
        {
            go.GetComponent<Image>().color = Color.white;
        }
    }

    public void HighlightGameSelected()
    {
        RemoveHighlights();
        buttonObjects[(int)gameSelected].GetComponent<Image>().color = Color.yellow;
    }

    public void StartGame()
    {
        if (IsLeaderboardOpen)
        {
            return; // If the leaderboard is open, we don't allow starting a game.
        }
        switch (gameSelected)
        {
            case SelectedGame.NONE:
                Debug.Log("No game currently selected!");
                break;
            case SelectedGame.MINESWEEPER:
                Debug.Log("Game Selected: Minesweeper");
                SceneManager.LoadScene("Minesweeper");
                break;
            case SelectedGame.BATTLESHIP:
                Debug.Log("Game Selected: Battleship");
                break;
            default:
                Debug.LogError("You shouldn't see this! The SelectedGame enum was not properly set before hitting Start Game!");
                break;
        }
    }    

    public void ToggleLeaderboard()
    {
        if (IsLeaderboardOpen)
        {
            CloseLeaderboard();
        }
        else
        {
            OpenLeaderboard();
        }
    }
    public void OpenLeaderboard()
    {
        IsLeaderboardOpen = true;
        SelectGameLeaderboard.instance.OpenUI(gameSelected);        
    }

    public void CloseLeaderboard()
    {
        IsLeaderboardOpen = false;
        SelectGameLeaderboard.instance.CloseUI();
    }

    // Used by game selection buttons to refresh the leaderboard UI if you have selected a different game.
    public void RefreshLeaderboard()
    {
        // Do leaderboard shenanigans here.
    }
}

public enum SelectedGame { NONE = -1, MINESWEEPER = 0, BATTLESHIP = 1}