using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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
    public enum Type {
        Mine,
        Number,
        Blank
    }
    public Vector2 coordinates;
    public Type type;
    public bool hasMine;
    public bool flagged = false;
    public bool revealed = false;
    public int value = 0;
    public List<Tile> adjacencies;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Tile created at coordinates: " + coordinates + " with type: " + type);
    }

    // TODO: Instantiate the tile class with all tile variables
    public Tile(Vector2 coordinates, Type type, bool hasMine, int value, List<Tile> adjacencies)
    {
        adjacencies = new List<Tile>();
        hasMine = false;
    }

    public void AssignValue()
    {
        value = GetAdjacentMineCount();
    }
    public void AddNeighbor(Tile neighbor)
    {
        if (neighbor != null && !adjacencies.Contains(neighbor))
        {
            adjacencies.Add(neighbor);
        }
    }

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

    // Update is called once per frame

    private void EnableOrDisableClicks(bool enable)
    {
        
    }

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
            GetComponent<Renderer>().material.color = Color.green; // Change color to green for revealed tile with value 0
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

        }
        else
        {
            GetComponent<Renderer>().material.color = Color.blue; // Change color to yellow for revealed tile with a value > 0
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
    private void HandleLeftClick()
    {
        if (hasMine)
        {
            GetComponent<Renderer>().material.color = Color.red; // Change color to red for mine tile
            Debug.Log("Game Over! You clicked on a mine at coordinates: " + coordinates);
            return;
        }
        RevealTile();
    }
    private void HandleRightClick()
    {
        Debug.Log("Right click on tile at coordinates: " + coordinates);
        if (!revealed)
        {
            // Flag handling logic
            flagged = !flagged;
            GetComponent<Renderer>().material.color = flagged ? Color.cyan : Color.white;
        }
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
