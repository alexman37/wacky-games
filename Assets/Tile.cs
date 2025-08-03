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

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Tile created at coordinates: " + coordinates);
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

    // Subscribe to game events in OnEnable, unsubscribe in OnDisable
    private void OnEnable()
    {
        GameEventsManager.changeTileClicking += EnableOrDisableClicks;
        
    }

    private void OnDisable()
    {
        GameEventsManager.changeTileClicking -= EnableOrDisableClicks;
    }

    /// <summary>
    /// Turn on / off clicking on tiles with game events
    /// </summary>
    private void EnableOrDisableClicks(bool enable)
    {
        
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
        GridManager.Instance.checkedTiles.Add(this); // Add this tile to the checked tiles  
        if (value == 0)
        {
            // TODO - better styling options here
            // Change color to green for revealed tile with value 0
            //GetComponent<Renderer>().materials[1].mainTexture = GameManager.instance.spriteAtlas.GetSprite("hex minesweeper_2").texture;
            GetComponent<Renderer>().materials[1].color = Color.green;
            foreach (Tile neighbor in adjacencies)
            {
                if (neighbor != null)
                {
                    neighbor.RevealTile(); // Recursively reveal neighboring tiles with value 0
                }
            }
        }
        else if (hasMine)
        {
            // Do not reveal it if it has a mine.
        }
        else
        {
            // TODO - better styling options
            // Change color to yellow for revealed tile with a value > 0
            //GetComponent<Renderer>().materials[1].mainTexture = GameManager.instance.spriteAtlas.GetSprite("hex minesweeper_3").texture;
            GetComponent<Renderer>().materials[1].color = Color.blue;
            return;
        }

    }

    private void OnMouseOver()
    {
        //Debug.Log("Mouse is over tile at coordinates: " + coordinates);
        if (Input.GetMouseButtonDown(0))
        {
            HandleLeftClick();
        }
        else
        if (Input.GetMouseButtonDown(1))
        {
            HandleRightClick();
        }
    }

    /// <summary>
    /// Reveal if possible / useful.
    /// </summary>
    private void HandleLeftClick()
    {
        if(!flagged && !revealed)
        {
            Debug.Log("Left click on tile at coordinates: " + coordinates);
            if (hasMine)
            {
                GetComponent<Renderer>().materials[1].color = Color.red; // Change color to red for mine tile
                Debug.Log("Game Over! You clicked on a mine at coordinates: " + coordinates);
                return;
            }
            RevealTile();
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
            /*Material mat = GetComponent<Renderer>().materials[1];
            Sprite spr = flagged ?
                GameManager.instance.spriteAtlas.GetSprite("hex minesweeper_1"):
                GameManager.instance.spriteAtlas.GetSprite("hex minesweeper_0");
            mat.mainTexture = spr.texture;
            mat.mainTextureOffset = UVToOffset(spr.uv);
            mat.mainTextureScale = UVToScale(spr.uv);*/
            GetComponent<Renderer>().materials[1].color = flagged ? Color.cyan : Color.gray;
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
}
