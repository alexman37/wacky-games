using System.Collections.Generic;
using UnityEngine;

namespace Games.Battleship
{
    public class GridManager : MonoBehaviour
    {

        public Dictionary<Vector2Int, GameObject> Player1Grid { get; private set; }
        public GameObject Player1GridParent; // Parent object for the grid tiles

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
        public GameObject BattleshipTilePrefab;
        public List<GameObject> tilesWithShips; // List for debugging


        private const float DEFAULT_SPACING_X = 1.05f;
        private const float DEFAULT_SPACING_Z = 1.05f;
        private const float DEFAULT_HEIGHT = -0.55f;
        public void GenerateGrid(int rowCount, int colCount)
        {
            if (Player1Grid == null) Player1Grid = new Dictionary<Vector2Int, GameObject>();
            if (Player2Grid == null) Player2Grid = new Dictionary<Vector2Int, GameObject>();

            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < colCount; col++)
                {
                    Vector2Int position = new Vector2Int(col, row);
                    Vector3 worldPositionP1 = new Vector3(
                        col * DEFAULT_SPACING_X,
                        DEFAULT_HEIGHT,
                        -row * DEFAULT_SPACING_Z
                    );
                    Vector3 worldPositionP2 = new Vector3(
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
                    GameObject tileP1 = Instantiate(BattleshipTilePrefab, worldPositionP1, Quaternion.identity);
                    tileP1.name = $"P1_{row}_{col}";
                    tileP1.transform.parent = Player1GridParent.transform;
                    Player1Grid[position] = tileP1;
                    Tile tileComponentP1 = tileP1.GetComponent<Tile>();
                    tileComponentP1.coordinates = position;

                    GameObject tileP2 = Instantiate(BattleshipTilePrefab, worldPositionP2, Quaternion.identity);
                    tileP2.name = $"P2_{row}_{col}";
                    tileP2.transform.parent = Player2GridParent.transform;
                    tileP2.transform.rotation = Quaternion.Euler(90, 0, 0);
                    Player2Grid[position] = tileP2;
                    Tile tileComponentP2 = tileP2.GetComponent<Tile>();
                    tileComponentP2.coordinates = position;
                }
            }
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
    }
}