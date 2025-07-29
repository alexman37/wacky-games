using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
    public bool flagged;
    public int value;
    public List<Tile> adjacencies;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Tile created at coordinates: " + coordinates + " with type: " + type);
    }

    // TODO: Instantiate the tile class with all tile variables
    public Tile(Vector2 coordinates, Type type, bool hasMine, int value, List<Tile> adjacencies)
    {
        this.coordinates = coordinates;
        this.type = type;
        this.hasMine = hasMine;
        this.value = value;
        this.adjacencies = adjacencies;
        flagged = false;
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
    void Update()
    {
        
    }

    private void EnableOrDisableClicks(bool enable)
    {
        // TODO: Enable or disable the ability to do stuff when you click on this tile (OnMouseDown runs whenever you click it)
    }

    private void OnMouseDown()
    {
        // TODO: Process clicking on a tile:
        //  - If this tile is a mine, you lose immediately
        //  - If not, reveal its value
        //  - Cascading: If this tile has a value of '0' look for other neighboring tiles with a value of 0; reveal them and all their neighbors.
    }
}
