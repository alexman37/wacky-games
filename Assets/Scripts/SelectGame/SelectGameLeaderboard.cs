using System.Collections.Generic;
using UnityEngine;

/* SelectGameLeaderboard
 * Called by the SelectGameManager script. Requires that the SelectedGame enum be set to something.
*/
public class SelectGameLeaderboard : MonoBehaviour
{
    public static SelectGameLeaderboard instance;
    public List<GameObject> leaderboardObjects; // Temporary structure that would hold the leaderboard objects for each game.
    public SelectGameLeaderboard()
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

    public void OpenUI(SelectedGame gameSelected)
    {
        switch (gameSelected)
        {
            case SelectedGame.NONE:
                Debug.Log("No game selected, cannot open leaderboard UI.");
                break;
            case SelectedGame.MINESWEEPER:
                Debug.Log("Opening Minesweeper leaderboard UI.");
                break;
            case SelectedGame.BATTLESHIP:
                Debug.Log("Opening Battleship leaderboard UI.");
                break;
            default:
                break;
        }
    }

    public void CloseUI()
    {
        // Logic to close the leaderboard UI
        Debug.Log("Closing leaderboard UI.");
    }
}