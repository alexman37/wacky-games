using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Everything to do with a single tile in minesweeper.
/// Handles things like:
///    - What are its properties? (e.g., is it a mine?)
///    - What happens when you click on it? (Also, when to start/stop listening for clicks?)
///    - What is its current appearance? (Revealed or not?)
/// </summary>
public class Tile : MonoBehaviour
{
    // Tile variables.
    public Vector2 coordinates;
    public bool hasMine;
    public bool flagged = false;
    public bool revealed = false;
    public int value = 0;
    public HashSet<Tile> adjacencies;
    public List<Sprite> hexSprites; // List of sprites for hexagonal tiles
    public List<Sprite> squareSprites; // List of sprites for square tiles

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Tile created at coordinates: " + coordinates);
        switch (GridManager.Instance.tileType)
        {
            case GridManager.TileType.Hex:
                transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = hexSprites[(int)SpriteIndex.Default];
                break;
            case GridManager.TileType.Square:
                GetComponent<SpriteRenderer>().sprite = squareSprites[(int)SpriteIndex.Default];
                break;
        }
    }

    // TODO: Instantiate the tile class with all tile variables
    public Tile()
    {
        this.adjacencies = new HashSet<Tile>();
        this.hasMine = false;
    }

    /// <summary>
    /// Call this when the grid is finished generating to assign a value to each tile.
    /// </summary>
    public void AssignValue()
    {
        value = GetAdjacentMineCount();
    }

    /// <summary>
    /// Attempt to add tile to its local neighbors list
    /// </summary>
    public void AddNeighbor(Tile neighbor)
    {
        if (neighbor != null && !adjacencies.Contains(neighbor))
        {
            adjacencies.Add(neighbor);
        }
    }

    /// <summary>
    /// How many mines does this tile border?
    /// You only need to call this during generation. For in-game computations, just get the value property.
    /// </summary>
    public int GetAdjacentMineCount()
    {
        int count = 0;
        foreach (var neighbor in adjacencies)
        {
            if (neighbor.hasMine)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Show the value of a tile
    /// </summary>
    private void RevealTile()
    {
        revealed = true; // Mark this tile as revealed
        if (GridManager.Instance.checkedTiles.Contains(this))
        {
            return; // If already checked or flagged, do nothing
        }
        if (hasMine)
        {
            switch (GridManager.Instance.tileType)
            {
                // Switch sprite to mine and change color to red (Very scary oooooo)
                case GridManager.TileType.Hex:
                    transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = hexSprites[(int)SpriteIndex.Mine];
                    transform.GetChild(0).GetComponent<Renderer>().material.color = Color.red;
                    break;
                case GridManager.TileType.Square:
                    GetComponent<SpriteRenderer>().sprite = squareSprites[(int)SpriteIndex.Mine];
                    GetComponent<Renderer>().material.color = Color.red;
                    break;
            }
            
            return; // If this tile has a mine, just reveal it and stop here
        }
        GridManager.Instance.checkedTiles.Add(this); // Add this tile to the checked tiles  
        if (value == 0)
        {
            switch(GridManager.Instance.tileType)
            {
                case GridManager.TileType.Hex:
                    transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = hexSprites[(int)SpriteIndex.Revealed];
                    break;
                case GridManager.TileType.Square:
                    GetComponent<SpriteRenderer>().sprite = squareSprites[(int)SpriteIndex.Revealed];
                    break;
            }
            foreach (Tile neighbor in adjacencies)
            {
                if (neighbor != null)
                {
                    neighbor.RevealTile(); // Recursively reveal neighboring tiles with value 0
                }
            }
        }        
        else
        {
            switch(GridManager.Instance.tileType)
            {
                case GridManager.TileType.Hex:
                    transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = hexSprites[getSpriteIndexForValue()];
                    break;
                case GridManager.TileType.Square:
                    GetComponent<SpriteRenderer>().sprite = squareSprites[getSpriteIndexForValue()];
                    break;
            }
            return;
        }

    }

    int getSpriteIndexForValue()
    {
        switch (value)
        {
            case 0: return (int)SpriteIndex.Revealed;
            case 1: return (int)SpriteIndex.Number1;
            case 2: return (int)SpriteIndex.Number2;
            case 3: return (int)SpriteIndex.Number3;
            case 4: return (int)SpriteIndex.Number4;
            case 5: return (int)SpriteIndex.Number5;
            case 6: return (int)SpriteIndex.Number6;
            case 7: return (int)SpriteIndex.Number7;
            case 8: return (int)SpriteIndex.Number8;
            default: return -1; // Invalid value
        }
    }

    private void OnMouseOver()
    {
        // We only care about clicks if the game is active
        if (Input.GetMouseButtonDown(0) && GameManager.instance.isAlive())
        {
            HandleLeftClick();
        }
        else
        if (Input.GetMouseButtonDown(1) && GameManager.instance.isAlive())
        {
            HandleRightClick();
        }
    }

    /// <summary>
    /// Reveal if possible / useful.
    /// </summary>
    private void HandleLeftClick()
    {
        if (!GridManager.Instance.hasPlayerMadeFirstMove)
        {
            if (!flagged) // Haven't made a move yet (until now) and this tile isn't flagged
            {
                if (hasMine) //If we have a mine here, we need to put it elsewhere, and regenerate our neighboring values
                {
                    Debug.Log("We have a mine on the first move");
                    hasMine = false; // Remove the mine from this tile
                    Debug.Log("Replacing the first mine");
                    GridManager.Instance.ReplaceFirstMine(this);
                    Debug.Log("Recalculating tile values after first move");
                    AssignValue();
                    foreach(Tile tile in adjacencies)
                    {
                        tile.AssignValue(); // Recalculate the values of neighboring tiles
                    }
                }
                GridManager.Instance.PlayerHasMadeFirstMove();
            }
        }
        if (!flagged && !revealed)
        {
            Debug.Log("Left click on tile at coordinates: " + coordinates);
            
            RevealTile();
            if (hasMine)
            {
                GameEventsManager.instance.dispatch_gameLost();
                return;
            }
        }
    }

    /// <summary>
    /// Flag only if the tile is still unknown.
    /// </summary>
    private void HandleRightClick()
    {        
        if (!revealed)
        {
            Debug.Log("Right click on tile at coordinates: " + coordinates);
            // Flag handling logic
            flagged = !flagged;
            if (flagged)
            {
                switch (GridManager.Instance.tileType)
                {
                    case GridManager.TileType.Hex:
                        transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = hexSprites[(int)SpriteIndex.Flagged];
                        break;
                    case GridManager.TileType.Square:
                        GetComponent<SpriteRenderer>().sprite = squareSprites[(int)SpriteIndex.Flagged];
                        break;
                }
            }
            else
            {
                switch (GridManager.Instance.tileType)
                {
                    case GridManager.TileType.Hex:
                        transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = hexSprites[(int)SpriteIndex.Default];
                        break;
                    case GridManager.TileType.Square:
                        GetComponent<SpriteRenderer>().sprite = squareSprites[(int)SpriteIndex.Default];
                        break;
                }
            }
            GameEventsManager.instance.dispatch_flagged(flagged);
        }
    }

    private Vector2 UVToOffset(Vector2 [] uv)
    {
        Vector2 offset = uv[2];
        return offset;
    }

    private Vector2 UVToScale(Vector2[] uv)
    {
        Vector2 scale = uv[1] - uv[2];
        return scale;
    }

    public override string ToString()
    {
        return $"Tile at {coordinates}: hasMine {hasMine} value {value}";
    }

    private void OnMouseExit()
    {
        //Debug.Log("Mouse exited tile at coordinates: " + coordinates);
    }

    private void OnMouseDown()
    {
        //Debug.Log("Mouse exited tile at coordinates: " + coordinates);
        // TODO: Process clicking on a tile:
        //  - If this tile is a mine, you lose immediately
        //  - If not, reveal its value
        //  - Cascading: If this tile has a value of '0' look for other neighboring tiles with a value of 0; reveal them and all their neighbors.
    }

    enum SpriteIndex { Default = 0, Flagged = 1, Revealed = 2, Mine = 3, Number1 = 4, Number2 = 5, Number3 = 6, Number4 = 7, Number5 = 8, Number6 = 9, Number7 = 10, Number8 = 11 }

}
