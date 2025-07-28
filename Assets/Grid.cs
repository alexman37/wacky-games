using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores the grid of tiles used to play minesweeper.
/// Also handles creation of new grids.
/// </summary>
public class Grid : MonoBehaviour
{
    public Tile[][] tileArray;


    // TODO: Generate a grid of a certain width and height with a certain number of mines.
    // You will have to instantiate the new Tiles with their relevant variables, including their adjacencies lists.
    // Wishlist (If possible, not necessary):
    // Can we ensure the mines never generate in an unsolvable / "50-50" pattern?
    // Can we set up the grid to potentially work with non-rectangular, irregular shapes?
    public Tile[][] generateGrid(int w, int h, int numMines)
    {
        return null;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
