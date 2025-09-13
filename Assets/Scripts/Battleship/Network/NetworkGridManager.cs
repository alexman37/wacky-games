using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


namespace Games.Battleship
{
    public class NetworkGridManager : MonoBehaviour
    {
        [Header("Tile Prefabs")]
        public GameObject BattleshipTile;
        public GameObject AttackTile;

        [Header("Grid Parents")]
        public GameObject BattleshipGridParent;
        public GameObject AttackGridParent;

        // References to the tiles that we use to represent where you placed your ships
        public Dictionary<Vector2Int, GameObject> MyShipTiles = new Dictionary<Vector2Int, GameObject>();
        // References to the tiles that we use to represent your "radar" grid for attacking the opponent
        public Dictionary<Vector2Int, GameObject> MyAttackTiles = new Dictionary<Vector2Int, GameObject>();

        // Reference to the local player
        public NetworkPlayer myPlayer;
        private NetworkBattleshipManager gameManager;

        private const float DEFAULT_SPACING_X = 1.05f;
        private const float DEFAULT_SPACING_Z = 1.05f;
        private const float DEFAULT_HEIGHT = -0.55f;
        private const int DEFAULT_ROWS = 10;
        private const int DEFAULT_COLS = 10;
        void Start() // Changed from OnNetworkSpawn
        {
            GenerateGrid(DEFAULT_ROWS, DEFAULT_COLS);            
        }

        public void SetNetworkManager(NetworkBattleshipManager manager)
        {
            gameManager = manager;
        }

        public void SetOwningPlayer(NetworkPlayer player)
        {
            myPlayer = player;
        }

        // Check if it's this player's turn
        public bool IsMyTurn()
        {
            if (gameManager == null || myPlayer == null) return false;

            BattleshipTurn currentTurn = gameManager.GetCurrentTurn();

            // Host is Player 1, Client is Player 2
            return (myPlayer.IsHost && currentTurn == BattleshipTurn.PLAYER1) ||
                   (!myPlayer.IsHost && currentTurn == BattleshipTurn.PLAYER2);
        }

        // To be called by a shot tile when it is clicked during the attack phase.
        // This needs to work its way to the NetworkBattleshipManager which should communicate the 
        // change to the other player and confirm a hit or miss.
        public void OnShotTileClicked(NetworkShotTile tileClicked)
        {
            if (!IsMyTurn()) return;

            Debug.Log($"Player attacking position: {tileClicked.coordinates}");

            // Send attack through our player to the server
            myPlayer.AttackTileRpc(tileClicked.GetCoordinatesAsVector2Int());
        }

        // Called from NetworkBattleshipManager when we receive attack results
        public void ProcessAttackResult(Vector2Int position, bool wasHit)
        {
            if (MyAttackTiles.TryGetValue(position, out GameObject tileObj))
            {
                NetworkShotTile shotTile = tileObj.GetComponent<NetworkShotTile>();
                shotTile.ShootTile(wasHit);
            }
        }

        // Called from NetworkBattleshipManager when we are being attacked
        public void ProcessIncomingAttack(Vector2Int position)
        {
            if (MyShipTiles.TryGetValue(position, out GameObject tileObj))
            {
                NetworkPlayerTile playerTile = tileObj.GetComponent<NetworkPlayerTile>();
                playerTile.MarkAsAttacked();
            }
        }

        public void GenerateGrid(int rowCount, int colCount)
        {
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
                    

                    GameObject playerTile = Instantiate(BattleshipTile, worldPositionPlayerTile, Quaternion.identity);
                    playerTile.name = $"{row}_{col}";
                    playerTile.transform.parent = BattleshipGridParent.transform;
                    MyShipTiles[position] = playerTile;
                    NetworkPlayerTile tileComponentBattleship = playerTile.GetComponent<NetworkPlayerTile>();
                    tileComponentBattleship.coordinates = position;
                    //Now, tell these tiles who their grid manager is so they can call back to it
                    tileComponentBattleship.SetOwningGridManager(this);

                    
                }
            }
            // Generate Attack Grid (vertical tiles facing the player)
            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < colCount; col++)
                {
                    Vector2Int position = new Vector2Int(col, row);  // Same coordinate system as ship grid
                    Vector3 worldPositionShotTile = new Vector3(
                        col * DEFAULT_SPACING_X,        // X increases left to right (same as ship grid)
                        DEFAULT_HEIGHT + 1 + (rowCount - 1 - row) * DEFAULT_SPACING_Z,  // Y decreases top to bottom for vertical tiles
                        0.75f                               // Consistent Z = 0 for all attack tiles
                    );

                    GameObject shotTile = Instantiate(AttackTile, worldPositionShotTile, Quaternion.Euler(-90, 0, 0));
                    shotTile.name = $"Attack_{row}_{col}";
                    shotTile.transform.parent = AttackGridParent.transform;
                    MyAttackTiles[position] = shotTile;
                    NetworkShotTile tileComponentShotTile = shotTile.GetComponent<NetworkShotTile>();
                    tileComponentShotTile.coordinates = position;
                    tileComponentShotTile.SetOwningGridManager(this);
                }
            }
        }

        //Called by NetworkPlayerTile when we try to place a ship on selected tiles
        // Returns true if the ship was successfully placed, false otherwise
        // This is so the tile we clicked on can decide whether or not to place the ship visually
        // and whether or not to remove the highlights on it
        public bool AttemptToPlaceShip(List<NetworkPlayerTile> tilesSelected)
        {
            //Check if any of the tiles are a ship. If so, don't even bother sending it to the server.
            foreach(NetworkPlayerTile tile in tilesSelected)
            {
                if (tile.hasShip)
                {
                    return false;
                }
            }
            Ship shipToPlace = myPlayer.shipToPlace;
            List<Vector2Int> occupiedPositions = new List<Vector2Int>();
            foreach (NetworkPlayerTile tile in tilesSelected)
            {
                occupiedPositions.Add(new Vector2Int((int)tile.coordinates.x, (int)tile.coordinates.y));
            }
            myPlayer?.PlaceShipRpc(occupiedPositions.ToArray(), shipToPlace.GetShipType());
            if (shipToPlace.isPlaced)
            {
                // We need to change the material of the tile to represent the ship
                // In the future this will be an actual model of the ship
                foreach(NetworkPlayerTile tile in tilesSelected)
                {
                    tile.SetAsShip();
                    //Indicate this is a ship visually by changing the material
                    tile.GetComponent<MeshRenderer>().material = tile.materialsList[2];
                }
                myPlayer.selectedShipType = BattleshipShipType.NONE; // Clear out the ship to place since we placed it successfully
                return true;
            }
            else
            {
                return false;
            }
        }
        public void DestroyGrids()
        {
            for (int i = BattleshipGridParent.transform.childCount - 1; i >= 0; i--)
            {
                // Destroy the child GameObject
                Destroy(BattleshipGridParent.transform.GetChild(i).gameObject);
            }
            for (int j = AttackGridParent.transform.childCount - 1; j >= 0; j--)
            {
                // Destroy the child GameObject
                Destroy(AttackGridParent.transform.GetChild(j).gameObject);
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
    }

}