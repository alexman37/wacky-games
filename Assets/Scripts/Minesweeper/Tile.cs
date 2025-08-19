using System.Collections.Generic;
using UnityEngine;

namespace Games.Minesweeper
{
    /// <summary>
    /// Everything to do with a single tile in minesweeper.
    /// Handles things like:
    ///    - What are its properties? (e.g., is it a mine?)
    ///    - What happens when you click on it? (Also, when to start/stop listening for clicks?)
    ///    - What is its current appearance? (Revealed or not?)
    /// </summary>
    public class Tile : MonoBehaviour
    {
        // PlayerTile variables.
        public Vector2 coordinates;
        public bool hasMine;
        public bool flagged = false;
        public bool revealed = false;
        public int value = 0;
        public HashSet<Tile> adjacencies;

        // Start is called before the first frame update
        void Start()
        {
            //Debug.Log("PlayerTile created at coordinates: " + coordinates);
            changeSpriteTo(MinesweeperStyles.instance.getUnclickedSprite());
        }

        private void OnEnable()
        {
            MinesweeperStyles.newStyleSheetLoaded += changeTileStyle;
        }

        private void OnDisable()
        {
            MinesweeperStyles.newStyleSheetLoaded -= changeTileStyle;
        }

        private void changeTileStyle()
        {
            Sprite spr = null;
            if (revealed)
            {
                if (hasMine) spr = MinesweeperStyles.instance.getMineSprite();
                else spr = MinesweeperStyles.instance.getNumberedSprite(value);
            }
            else if (flagged) spr = MinesweeperStyles.instance.getFlaggedSprite();
            else spr = MinesweeperStyles.instance.getUnclickedSprite();


            switch (GridManager.Instance.tileType)
                {
                    case GridManager.TileType.Hex:
                        transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = spr;
                        break;
                    case GridManager.TileType.Square:
                        GetComponent<SpriteRenderer>().sprite = spr;
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
                // Switch sprite to mine
                changeSpriteTo(MinesweeperStyles.instance.getMineSprite());

                // and change color to red (Very scary oooooo)
                switch (GridManager.Instance.tileType)
                {
                    case GridManager.TileType.Hex:
                        transform.GetChild(0).GetComponent<Renderer>().material.color = Color.red;
                        break;
                    case GridManager.TileType.Square:
                        GetComponent<Renderer>().material.color = Color.red;
                        break;
                }

                return; // If this tile has a mine, just reveal it and stop here
            }
            GridManager.Instance.checkedTiles.Add(this); // Add this tile to the checked tiles  
            if (value == 0)
            {
                int revealedInChain = 1;

                changeSpriteTo(MinesweeperStyles.instance.getNumberedSprite(0));
                foreach (Tile neighbor in adjacencies)
                {
                    if (neighbor != null)
                    {
                        revealedInChain += neighbor.RevealTileHelper(); // Recursively reveal neighboring tiles with value 0
                    }
                }

                Debug.Log("Just revealed " + revealedInChain + " tiles");
                MinesweeperEventsManager.instance.dispatch_revealedXtiles(revealedInChain);
            }
            else
            {
                changeSpriteTo(MinesweeperStyles.instance.getNumberedSprite(value));
                MinesweeperEventsManager.instance.dispatch_revealedXtiles(1);
                return;
            }
        }

        private int RevealTileHelper()
        {
            revealed = true; // Mark this tile as revealed
            if (GridManager.Instance.checkedTiles.Contains(this))
            {
                return 0; // If already checked or flagged, do nothing
            }
            else if (hasMine)
            {
                return 0; // If this tile has a mine, just reveal it and stop here
            }
            GridManager.Instance.checkedTiles.Add(this); // Add this tile to the checked tiles
            if (value == 0)
            {
                int revealedInChain = 0;

                changeSpriteTo(MinesweeperStyles.instance.getNumberedSprite(0));
                foreach (Tile neighbor in adjacencies)
                {
                    if (neighbor != null)
                    {
                        revealedInChain += neighbor.RevealTileHelper(); // Recursively reveal neighboring tiles with value 0
                    }
                }

                //Debug.Log("Just revealed " + revealedInChain + " tiles");
                return 1 + revealedInChain;
            }
            else
            {
                changeSpriteTo(MinesweeperStyles.instance.getNumberedSprite(value));
                return 1;
            }
        }

        private void OnMouseOver()
        {
            // Some UI event is preventing clicks, so we don't do anything.
            if (!GridManager.Instance.canPlayerClick)
            {
                return;
            }
            // We only care about clicks if the game is active
            if (Input.GetMouseButtonDown(0) && MinesweeperManager.instance.isAlive())
            {
                HandleLeftClick();
            }
            else
            if (Input.GetMouseButtonDown(1) && MinesweeperManager.instance.isAlive())
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
                        foreach (Tile tile in adjacencies)
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
                    MinesweeperEventsManager.instance.dispatch_gameLost();
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
                    changeSpriteTo(MinesweeperStyles.instance.getFlaggedSprite());
                }
                else
                {
                    changeSpriteTo(MinesweeperStyles.instance.getUnclickedSprite());
                }
                MinesweeperEventsManager.instance.dispatch_flagged(flagged);
            }
        }

        // Location of the sprite differs for different shapes
        private void changeSpriteTo(Sprite spr)
        {
            switch (GridManager.Instance.tileType)
            {
                case GridManager.TileType.Hex:
                    transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = spr;
                    break;
                case GridManager.TileType.Square:
                    GetComponent<SpriteRenderer>().sprite = spr;
                    break;
            }
        }

        private Vector2 UVToOffset(Vector2[] uv)
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

}