using System.Collections.Generic;
using UnityEngine;

namespace Games.Battleship
{
    public class GridManager : MonoBehaviour
    {

        public Dictionary<Vector2Int, GameObject> Player1Grid { get; private set; }
        public GameObject Player1GridParent; // Parent object for the grid tiles
        public Dictionary<Vector2Int, GameObject> Player1TopGrid { get; private set; }
        public GameObject Player1TopGridParent; // Parent object for the grid tiles

        public Dictionary<Vector2Int, GameObject> Player2Grid { get; private set; }
        public GameObject Player2GridParent;

        public static GridManager Instance;
        public GridManager()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
        }
        public GameObject PlayerTilePrefab; //Prefab representing the tiles for the bottom grid (PlayerTile)
        public GameObject ShotTilePrefab; //Prefab representing the tiles for the top grid (ShotTile)
        public List<GameObject> tilesWithShips; // List for debugging


        private const float DEFAULT_SPACING_X = 1.05f;
        private const float DEFAULT_SPACING_Z = 1.05f;
        private const float DEFAULT_HEIGHT = -0.55f;
        public void GenerateGrid(int rowCount, int colCount)
        {
            if (Player1Grid == null) Player1Grid = new Dictionary<Vector2Int, GameObject>();
            if (Player2Grid == null) Player2Grid = new Dictionary<Vector2Int, GameObject>();
            if (Player1TopGrid == null) Player1TopGrid = new Dictionary<Vector2Int, GameObject>();

            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < colCount; col++)
                {
                    Vector2Int position = new Vector2Int(col, row);
                    Vector3 worldPositionPlayerTile = new Vector3(
                        col * DEFAULT_SPACING_X,
                        DEFAULT_HEIGHT,
                        -row * DEFAULT_SPACING_Z
                    );
                    Vector3 worldPositionShotTile = new Vector3(
                        col * DEFAULT_SPACING_X,
                        row * DEFAULT_SPACING_Z,
                        .75f
                    );
                    /*
                    Vector2Int positionP1 = new Vector2Int(col, row);
                    Vector3 worldPositionP1 = new Vector3(
                        col * DEFAULT_SPACING_X,
                        DEFAULT_HEIGHT,
                        row * DEFAULT_SPACING_Z
                    );
                    */
                    /*
                    Vector2Int positionP2 = new Vector2Int(col, row);
                    Vector3 worldPositionP2 = new Vector3(
                        -col * DEFAULT_SPACING_X,
                        DEFAULT_HEIGHT,
                        -row * DEFAULT_SPACING_Z
                    );
                    */
                    GameObject playerTile = Instantiate(PlayerTilePrefab, worldPositionPlayerTile, Quaternion.identity);
                    playerTile.name = $"P1_{row}_{col}";
                    playerTile.transform.parent = Player1GridParent.transform;
                    Player1Grid[position] = playerTile;
                    PlayerTile tileComponentP1 = playerTile.GetComponent<PlayerTile>();
                    tileComponentP1.coordinates = position;

                    GameObject shotTile = Instantiate(ShotTilePrefab, worldPositionShotTile, Quaternion.identity);
                    shotTile.name = $"P2_{row}_{col}";
                    shotTile.transform.parent = Player2GridParent.transform;
                    shotTile.transform.rotation = Quaternion.Euler(-90, 0, 0);
                    Player1TopGrid[position] = shotTile;
                    ShotTile tileComponentP2 = shotTile.GetComponent<ShotTile>();
                    tileComponentP2.coordinates = position;
                    /*
                    GameObject tileP2 = Instantiate(BattleshipTilePrefab, worldPositionShotTile, Quaternion.identity);
                    tileP2.name = $"P2_{row}_{col}";
                    tileP2.transform.parent = Player2GridParent.transform;
                    tileP2.transform.rotation = Quaternion.Euler(90, 0, 0);
                    Player2Grid[position] = tileP2;
                    PlayerTile tileComponentP2 = tileP2.GetComponent<PlayerTile>();
                    tileComponentP2.coordinates = position;
                    */
                }
            }
        }

        public PlayerTile GetPlayerTileFromPosition(int row, int col)
        {
            return Player1Grid[new Vector2Int(row, col)].GetComponent<PlayerTile>();
        }

        //Unlikely to be used, but included for completeness
        public ShotTile GetShotTileFromPosition(int row, int col)
        {
            return Player1TopGrid[new Vector2Int(row, col)].GetComponent<ShotTile>();
        }

        public void DestroyGrids()
        {
            for (int i = Player1GridParent.transform.childCount - 1; i >= 0; i--)
            {
                // Destroy the child GameObject
                Destroy(Player1GridParent.transform.GetChild(i).gameObject);
            }
            for (int j = Player2GridParent.transform.childCount - 1; j >= 0; j--)
            {
                // Destroy the child GameObject
                Destroy(Player2GridParent.transform.GetChild(j).gameObject);
            }
        }

        // Start transparency animation for multiple tiles
        public void StartTransparencyChange(List<BattleshipTile> tiles, float duration)
        {
            if (tiles == null)
            {
                Debug.Log("Tiles are null");
                return;
            }
                

            foreach (BattleshipTile tile in tiles)
            {
                if (tile != null)
                {
                    tile.StartChangingTransparency(duration);
                }
            }
        }

        public void StopTransparencyChangeTiles(List<BattleshipTile> tiles)
        {
            if (tiles == null)
                return;

            foreach (BattleshipTile tile in tiles)
            {
                if (tile != null)
                {
                    tile.StopChangingTransparency();
                }
            }
        }

        // Attempts to add a ship which occupies all designated tiles
        // Returns true if the ship was successfully placed, false otherwise
        // This is so the tile we clicked on can decide whether or not to place the ship visually
        // and whether or not to remove the highlights on it
        public bool AttemptToPlaceShip(List<PlayerTile> tilesSelected)
        {
            //Check if any of the tiles are a ship. If so, don't even bother
            foreach (PlayerTile tile in tilesSelected)
            {
                if (tile.isShip)
                {
                    return false;
                }
            }
            // communicate to the correct player that a ship was placed
            BattleshipManager.Instance.player1Component.shipTiles.AddRange(tilesSelected);
            // if that worked - visually change the tile to indicate there is a ship here
            foreach (PlayerTile tile in tilesSelected)
            {
                tile.SetAsShip();
            }
            // clear out ship to place since we set it successfully
            BattleshipManager.Instance.selectedShipType = BattleshipShipType.NONE;

            return true;
        }
    }
}