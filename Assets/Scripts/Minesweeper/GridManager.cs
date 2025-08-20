using System.Collections.Generic;
using UnityEngine;

namespace Games.Minesweeper
{
    /// <summary>
    /// Stores the grid of tiles used to play minesweeper.
    /// Also handles creation of new grids.
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        public static bool greenlight = false;

        public Dictionary<Vector2Int, GameObject> Grid { get; private set; }
        public GameObject squareTile;
        public GameObject hexTile;
        public GameObject GridParent; // Parent object for the grid tiles
        public List<GameObject> tilesWithMines; // List to store tiles with mines for debugging
        public HashSet<Tile> checkedTiles = new HashSet<Tile>(); // List to keep track of checked tiles

        public TileType tileType; // Default tile type
        public int RowCount;
        public int ColCount;
        public int MineCount;
        private bool showMines = false; // Flag to control mine visibility
        public bool hasPlayerMadeFirstMove { get; private set; } // Flag to check if the player has made the first move
        public bool canPlayerClick { get; private set; } = true; // Flag to control if the player can click tiles. Useful for UI interactions.

        // Default tile spacing - could be made configurable
        #region Square Tile Spacing
        private const float DEFAULT_SQUARE_SPACING_X = 1.05f;
        private const float DEFAULT_SQUARE_SPACING_Z = 1.05f;
        private const float DEFAULT_SQUARE_HEIGHT = -0.55f;
        #endregion
        #region Hex Tile Spacing
        private const float DEFAULT_HEX_HEIGHT = -0.55f;

        #endregion
        public static GridManager Instance { get; private set; } // Singleton instance
                                                                 // TODO: Generate a grid of a certain width and height with a certain number of mines.
                                                                 // You will have to instantiate the new Tiles with their relevant variables, including their adjacencies lists.
                                                                 // Wishlist (If possible, not necessary):
                                                                 // Can we ensure the mines never generate in an unsolvable / "50-50" pattern?
                                                                 // Can we set up the grid to potentially work with non-rectangular, irregular shapes?
        void Update()
        {
            /*if(UnityEngine.Input.GetKeyDown(KeyCode.R))
            {            
                RegenerateGrid(tileType, RowCount, ColCount, MineCount);
            }*/
            if (UnityEngine.Input.GetKeyDown(KeyCode.M))
            {
                if (tilesWithMines != null && showMines == false)
                {
                    ShowMines();
                    showMines = true;
                }
                else
                {
                    HideMines();
                    showMines = false;
                }
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
            tilesWithMines = new List<GameObject>();
            checkedTiles = new HashSet<Tile>();
            hasPlayerMadeFirstMove = false; // Initialize the first move flag

            greenlight = true;
            Debug.Log("Greenlit grid");
        }

        // For UI interactions, we can enable or disable player clicks
        public void EnablePlayerMovement()
        {
            canPlayerClick = true; // Enable player clicks
        }
        public void DisablePlayerMovement()
        {
            canPlayerClick = false; // Disable player clicks
        }

        private void OnEnable()
        {

        }

        private void OnDisable()
        {

        }

        public Dictionary<Vector2Int, GameObject> GenerateGrid(int rowCount, int colCount, int mineCount)
        {
            canPlayerClick = true; // Allow player to click tiles when generating a new grid
            // TODO smarter waiting for this component to finish creating
            if (Grid == null) Grid = new Dictionary<Vector2Int, GameObject>();

            if (tileType == TileType.Square)
            {
                Debug.Log("Generating square grid with " + rowCount + " rows and " + colCount + " columns.");
                // Generate a square grid
                for (int row = 0; row < rowCount; row++)
                {
                    for (int col = 0; col < colCount; col++)
                    {
                        Vector2Int position = new Vector2Int(col, row);
                        Vector3 worldPosition = new Vector3(
                            col * DEFAULT_SQUARE_SPACING_X,
                            DEFAULT_SQUARE_HEIGHT,
                            row * DEFAULT_SQUARE_SPACING_Z
                        );
                        GameObject tile = Instantiate(squareTile, worldPosition, Quaternion.identity);
                        tile.name = $"SquareTile_{row}_{col}";
                        tile.transform.parent = GridParent.transform;
                        tile.transform.localRotation = Quaternion.Euler(90, 0, 0);
                        Grid[position] = tile;
                        Tile tileComponent = tile.GetComponent<Tile>();
                        tileComponent.coordinates = position;
                    }
                }

                // Connect neighbors for square grid (all 8 directions)
                for (int row = 0; row < rowCount; row++)
                {
                    for (int col = 0; col < colCount; col++)
                    {
                        Vector2Int currentPos = new Vector2Int(col, row);
                        GameObject currentTileObj = Grid[currentPos];
                        Tile currentTile = currentTileObj.GetComponent<Tile>();

                        // Add all 8 neighbors (horizontally, vertically, and diagonally)
                        for (int neighborRow = -1; neighborRow <= 1; neighborRow++)
                        {
                            for (int neighborCol = -1; neighborCol <= 1; neighborCol++)
                            {
                                // Skip the current tile
                                if (neighborRow == 0 && neighborCol == 0) continue;

                                Vector2Int neighborPos = new Vector2Int(col + neighborCol, row + neighborRow);

                                // Check if neighbor is within grid bounds
                                if (Grid.TryGetValue(neighborPos, out GameObject neighborObj))
                                {
                                    Tile neighborTile = neighborObj.GetComponent<Tile>();
                                    if (neighborTile != null)
                                    {
                                        currentTile.adjacencies.Add(neighborTile);
                                    }
                                }
                            }
                        }
                    }
                }
                CameraManager.Instance.SetPositionSquare(rowCount, colCount);
                PlaceMinesRandomly(mineCount);
                return Grid;
            }
            else if (tileType == TileType.Hex)
            {
                // TODO: Do we want the 3d object approach or the old 2D one?
                // 3d objects allow for easier handling of clicks and sprite adjustment.
                //float hexSize = hexTile.GetComponent<HexagonMeshGenerator>().GetSize();            
                //float horizontalSpacing = hexSize * 1.5f + .1f; // Horizontal distance between hex centers
                //float verticalSpacing = hexSize * Mathf.Sqrt(3) + .1f; // Vertical distance between hex centers
                float horizontalSpacing = 1.75f;
                float verticalSpacing = 2.1f;

                // Generate a hexagonal grid
                for (int row = 0; row < rowCount; row++)
                {
                    for (int col = 0; col < colCount; col++)
                    {
                        Vector2Int position = new Vector2Int(col, row);
                        Vector3 worldPosition = new Vector3(
                            col * verticalSpacing + (row % 2) * (verticalSpacing / 2), // Offset every other row
                            DEFAULT_HEX_HEIGHT,
                            row * horizontalSpacing + 1f
                        );
                        GameObject tile = Instantiate(hexTile, worldPosition, Quaternion.identity);
                        //Mesh hexMesh = tile.GetComponent<HexagonMeshGenerator>().GetMesh();
                        //tile.GetComponent<MeshCollider>().sharedMesh = hexMesh;
                        tile.transform.eulerAngles = new Vector3(180, 90, 0); // Optional: Rotate the hex tile for better visibility
                        tile.name = $"HexTile_{row}_{col}"; // Optional: Name the tile for easier debugging
                        tile.transform.parent = GridParent.transform;
                        tile.transform.localRotation = Quaternion.Euler(-90, 0, 0);
                        Grid[position] = tile;
                        Tile tileComponent = tile.GetComponent<Tile>();
                        tileComponent.coordinates = position; // Set the coordinates for the tile
                    }
                }

                // Second pass: connect neighbors for hexagonal grid (6 directions)
                for (int row = 0; row < rowCount; row++)
                {
                    for (int col = 0; col < colCount; col++)
                    {
                        Vector2Int currentPos = new Vector2Int(col, row);
                        GameObject currentTileObj = Grid[currentPos];
                        Tile currentTile = currentTileObj.GetComponent<Tile>();

                        // Define the 6 neighbors for hexagonal grid
                        // The pattern is different for even and odd rows
                        Vector2Int[] neighborOffsets;

                        if (row % 2 == 0) // Even row
                        {
                            neighborOffsets = new Vector2Int[]
                            {
                            new Vector2Int(-1, 0),  // Left
                            new Vector2Int(1, 0),   // Right
                            new Vector2Int(-1, -1), // Top-Left
                            new Vector2Int(0, -1),  // Top-Right
                            new Vector2Int(-1, 1),  // Bottom-Left
                            new Vector2Int(0, 1)    // Bottom-Right
                            };
                        }
                        else // Odd row
                        {
                            neighborOffsets = new Vector2Int[]
                            {
                            new Vector2Int(-1, 0),  // Left
                            new Vector2Int(1, 0),   // Right
                            new Vector2Int(0, -1),  // Top-Left
                            new Vector2Int(1, -1),  // Top-Right
                            new Vector2Int(0, 1),   // Bottom-Left
                            new Vector2Int(1, 1)    // Bottom-Right
                            };
                        }

                        // Add all 6 neighbors
                        foreach (Vector2Int offset in neighborOffsets)
                        {
                            Vector2Int neighborPos = currentPos + offset;

                            // Check if neighbor is within grid bounds
                            if (Grid.TryGetValue(neighborPos, out GameObject neighborObj))
                            {
                                Tile neighborTile = neighborObj.GetComponent<Tile>();
                                if (neighborTile != null)
                                {
                                    currentTile.adjacencies.Add(neighborTile);
                                }
                            }
                        }
                    }
                }
                CameraManager.Instance.SetPositionHex(rowCount, colCount);
                PlaceMinesRandomly(mineCount);
                // TODO smarter creation at start
                //CameraManager.Instance.SetPositionHex();
                return Grid;
            }
            return null;
        }



        // Method to place mines randomly in the grid
        private void PlaceMinesRandomly(int mineCount)
        {
            if (Grid == null || Grid.Count == 0 || mineCount <= 0)
                return;

            List<Vector2Int> positions = new List<Vector2Int>(Grid.Keys);

            // Shuffle positions of the mines by generating a random permutation
            for (int i = positions.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                Vector2Int temp = positions[i];
                positions[i] = positions[randomIndex];
                positions[randomIndex] = temp;
            }

            // Place mines on the first mineCount positions
            int placedMines = 0;
            for (int i = 0; i < positions.Count && placedMines < mineCount; i++)
            {
                GameObject tileObj = Grid[positions[i]];
                Tile tile = tileObj.GetComponent<Tile>();
                if (tile != null)
                {
                    tile.hasMine = true;
                    placedMines++;
                    tilesWithMines.Add(tileObj); // Add to the list of tiles with mines for debugging                
                }
            }

            // Calculate values for all tiles (number of adjacent mines)
            CalculateTileValues();
        }

        public void ReplaceFirstMine(Tile tileToReplace)
        {
            tilesWithMines.Remove(tileToReplace.gameObject); // Remove the old mine tile from the list
            List<Vector2Int> positions = new List<Vector2Int>(Grid.Keys);
            Vector2Int oldPosition = Vector2Int.RoundToInt(tileToReplace.coordinates); // Get the old position of the mine tile
            positions.Remove(oldPosition); // Ensure this position is not selected again
            Debug.Log($"Replacing mine at {oldPosition} with a new mine.");
            // Shuffle positions of the mines by generating a random permutation
            for (int i = positions.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                Vector2Int temp = positions[i];
                positions[i] = positions[randomIndex];
                positions[randomIndex] = temp;
            }
            for (int i = 0; i < positions.Count; i++)
            {
                GameObject tileObj = Grid[positions[i]];
                Tile tile = tileObj.GetComponent<Tile>();
                if (tile != null && !tile.hasMine)
                {
                    tile.hasMine = true;
                    tile.AssignValue(); // Recalculate the value of this tile
                    foreach (Tile adj in tile.adjacencies)
                    {
                        adj.AssignValue(); // Recalculate the values of adjacent tiles
                    }
                    tilesWithMines.Add(tileObj); // Add to the list of tiles with mines for debugging
                    break;
                }
            }
        }

        public void CalculateTileValues()
        {
            foreach (var tileObj in Grid.Values)
            {
                Tile tile = tileObj.GetComponent<Tile>();
                if (tile != null)
                {
                    tile.AssignValue();
                    // Optional: Update visuals for debugging
                    //Debug.Log($"PlayerTile at {tile.coordinates} has value {tile.value}");
                }
            }
        }

        public void DestroyAllChildren(GameObject parentObject)
        {
            // Iterate through the children in reverse order to avoid issues with hierarchy changes
            for (int i = parentObject.transform.childCount - 1; i >= 0; i--)
            {
                // Destroy the child GameObject
                Destroy(parentObject.transform.GetChild(i).gameObject);
            }
        }

        public void ButtonRegenGrid()
        {
            canPlayerClick = false; // Disable player clicks while regenerating the grid
            DestroyAllChildren(GridParent);
            //RegenerateGrid(tileType, RowCount, ColCount, MineCount);
            MinesweeperTopBarUI tb = MinesweeperTopBarUI.instance;
            TileType selectedTileType = (TileType)tb.shapeSelectGroup.currSelected;
            tileType = selectedTileType;

            RegenerateGrid(selectedTileType, tb.rowsInput, tb.colsInput, tb.minesInput);
        }

        public void RegenerateGrid(TileType tileType, int rowCount, int columnCount, int mineCount)
        {
            showMines = false;
            hasPlayerMadeFirstMove = false; // Reset the first move flag
                                            // Clear the existing grid
            Grid.Clear();
            tilesWithMines.Clear();
            checkedTiles.Clear();
            RowCount = rowCount;
            ColCount = columnCount;
            MineCount = mineCount;

            // Use queued style
            MinesweeperStyles.instance.useNextStyle();

            // Generate the new grid
            GenerateGrid(rowCount, columnCount, mineCount);
            HideMines();
        }

        public void ShowMines()
        {
            foreach (GameObject tileObj in tilesWithMines)
            {
                Tile tile = tileObj.GetComponent<Tile>();
                if (tile != null && tile.hasMine)
                {
                    // Show the mine as red tile.
                    if (tileType == TileType.Square)
                        tileObj.GetComponent<Renderer>().material.color = Color.red;
                    else if (tileType == TileType.Hex)
                        tileObj.transform.GetChild(0).GetComponent<Renderer>().material.color = Color.red;
                }
            }
        }

        public void HideMines()
        {
            foreach (GameObject tileObj in tilesWithMines)
            {
                Tile tile = tileObj.GetComponent<Tile>();
                if (tile != null && tile.hasMine)
                {
                    // Show the mine as red tile.
                    if (tileType == TileType.Square)
                        tileObj.GetComponent<Renderer>().material.color = Color.white;
                    else if (tileType == TileType.Hex)
                        tileObj.transform.GetChild(0).GetComponent<Renderer>().material.color = Color.white;
                }
            }
        }

        public void PlayerHasMadeFirstMove()
        {
            hasPlayerMadeFirstMove = true; // Set the flag to true when the player makes their first move
        }

        public void DebugTileNeighbors(Vector2Int position)
        {
            if (Grid.TryGetValue(position, out GameObject tileObj))
            {
                Tile tile = tileObj.GetComponent<Tile>();
                if (tile != null)
                {
                    Debug.Log($"Tile at {position} has {tile.adjacencies.Count} neighbors and value {tile.value}");
                    foreach (Tile adj in tile.adjacencies)
                    {
                        Debug.Log($"  - Neighbor {adj}: at {adj.coordinates}, hasMine: {adj.hasMine}");
                    }
                }
                else
                {
                    Debug.LogError($"No Tile component found on tile at {position}");
                }
            }
            else
            {
                Debug.LogError($"No tile found at position {position}");
            }
        }

        public enum TileType
        {
            Square,
            Hex
        }
    }
}
