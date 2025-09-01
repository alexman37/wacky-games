using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


namespace Games.Battleship
{
    public class NetworkPlayer : NetworkBehaviour
    {
        [Header("Player Info")]
        public string PlayerName;
        public int PlayerID;
        public NetworkVariable<bool> isReady = new NetworkVariable<bool>();
        public int totalShipValue = 0;
        public List<NetworkPlayerTile> shipTiles = new List<NetworkPlayerTile>(); // To represent this player's ships
        public List<NetworkShotTile> checkedTiles = new List<NetworkShotTile>(); // To represent which tiles this player has checked
        public List<NetworkPlayerTile> tilesEnemyHit = new List<NetworkPlayerTile>(); // To represent which tiles of the player have been attacked
        public List<BattleshipShipType> playerBattleships;
        public BattleshipRotation shipRotation = BattleshipRotation.HORIZONTAL; // When the player is hovering tiles, what rotation is the ship in?
        public BattleshipShipType selectedShipType;
        NetworkBattleshipManager manager;
        public bool isHostPlayer;
        public Ship shipToPlace; // The ship currently being placed during setup
        public NetworkCameraManager cameraManager;
        public override void OnNetworkSpawn()
        {            
            // Determine if this player object represents the host or client
            // Host is always ClientId 0 in Unity Netcode
            isHostPlayer = (OwnerClientId == 0);

            Debug.Log($"=== NetworkPlayer.OnNetworkSpawn ===");
            Debug.Log($"OwnerClientId: {OwnerClientId}");
            Debug.Log($"NetworkManager.LocalClientId: {NetworkManager.Singleton.LocalClientId}");
            Debug.Log($"IsHost (of this machine): {IsHost}");
            Debug.Log($"IsOwner (am I the local player): {IsOwner}");

            if (isHostPlayer)
            {
                // Initialize host player info
                PlayerName = "Host";
                PlayerID = 1;
            }
            else
            {
                // Initialize client player info
                PlayerName = "Client";
                PlayerID = 2;
            }
            Debug.Log("OnNetworkSpawn called for player: " + PlayerName);
            shipRotation = BattleshipRotation.HORIZONTAL; // Default to horizontal on spawn
            // Find and set manager reference using modern approach
            var foundManager = FindFirstObjectByType<NetworkBattleshipManager>();
            if (foundManager != null)
            {
                Debug.Log("NetworkBattleshipManager found and set for player: " + PlayerName);
                SetManager(foundManager);

                Debug.Log("Requesting initialization from manager for player: " + PlayerName);
                // Request initialization from the manager
                RequestInitializationRpc();
            }
            else
            {
                Debug.LogError("NetworkBattleshipManager not found in the scene.");
            }
            cameraManager = GetComponent<NetworkCameraManager>();
            if (cameraManager == null)
            {
                cameraManager = gameObject.AddComponent<NetworkCameraManager>();
            }
        }

        public void Update()
        {
            // Only handle input for local player
            if (!IsOwner) return;

            switch (manager.currentTurn.Value)
            {
                case BattleshipTurn.SHIP_SETUP:
                    float scrollInput = Input.GetAxis("Mouse ScrollWheel");
                    if (scrollInput != 0)
                    {
                        RotateShipPlacement();
                    }
                    // Camera toggle for ship placement
                    if (Input.GetKeyDown(KeyCode.R))
                    {
                        cameraManager?.ToggleCameraView();
                    }
                    break;

                case BattleshipTurn.PLAYER1:
                    if (OwnerClientId == 0)
                    {
                        // Automatically switch to attack view when it's your turn
                        if (cameraManager != null)
                        {
                            cameraManager.SetAttackBoardView();
                        }

                        if (Input.GetKeyDown(KeyCode.R))
                        {
                            cameraManager?.ToggleCameraView();
                        }
                    }
                    else
                    {
                        // Switch to player board view when it's opponent's turn
                        if (cameraManager != null)
                        {
                            cameraManager.SetPlayerBoardView();
                        }
                    }
                    break;

                case BattleshipTurn.PLAYER2:
                    if (OwnerClientId == 1)
                    {
                        // Automatically switch to attack view when it's your turn
                        if (cameraManager != null)
                        {
                            cameraManager.SetAttackBoardView();
                        }

                        if (Input.GetKeyDown(KeyCode.R))
                        {
                            cameraManager?.ToggleCameraView();
                        }
                    }
                    else
                    {
                        // Switch to player board view when it's opponent's turn
                        if (cameraManager != null)
                        {
                            cameraManager.SetPlayerBoardView();
                        }
                    }
                    break;

                case BattleshipTurn.GAME_OVER:
                    break;

                default:
                    break;
            }
        }

        [Rpc(SendTo.Server)]
        private void RequestInitializationRpc()
        {
            Debug.Log($"Player {PlayerName} (OwnerClientId: {OwnerClientId}) requesting initialization from server.");

            // Server should initialize ANY player that requests it
            if (manager != null)
            {
                Debug.Log($"Server initializing player {PlayerName} (OwnerClientId: {OwnerClientId})");
                manager.InitializePlayer(this);
            }
            else
            {
                Debug.LogError($"RequestInitializationRpc called but manager is null for player {PlayerName}");
            }
        }

        public void SetManager(NetworkBattleshipManager battleshipManager)
        {
            manager = battleshipManager;
        }
        public void Initialize(List<BattleshipShipType> battleshipsToPlace)
        {
            playerBattleships = battleshipsToPlace;
            Debug.Log($"Player {PlayerName} initializing with ships to place: {string.Join(", ", battleshipsToPlace)}");
        }

        [Rpc(SendTo.Server)]
        public void PlaceShipRpc(Vector2Int[] tileCoordinates, BattleshipShipType shipType)
        {
            if (!IsHost) return; // Only server validates

            Debug.Log($"Server validating ship placement for player {PlayerName} at positions: {string.Join(", ", tileCoordinates)}");

            // Convert array to list for internal processing
            List<Vector2Int> positions = new List<Vector2Int>(tileCoordinates);

            // Validate ship placement
            if (ValidateShipPlacement(positions, shipType))
            {
                Debug.Log("Ship placement valid - placing ship");

                // Store ship data on server only
                ServerStoreShipData(positions, shipType);

                // Send to the specific client that made the request
                if (OwnerClientId == 0) // Host player
                {
                    // Call directly for host
                    ProcessShipPlacementConfirmation(tileCoordinates, shipType);
                }
                else // Client player
                {
                    // Send RPC to specific client
                    ConfirmShipPlacementClientRpc(tileCoordinates, shipType);
                }

                totalShipValue += GetShipLength(shipType);
                Debug.Log($"Ship placed successfully for player {PlayerName}");
                Debug.Log($"Player ship value: {totalShipValue}");
                CheckIfAllShipsPlaced();
            }
            else
            {
                Debug.Log("Ship placement invalid - rejecting");
                // Notify the client that placement failed
                RejectShipPlacementRpc();
            }
        }

        [Rpc(SendTo.NotServer)] // Send to all clients except server
        private void ConfirmShipPlacementClientRpc(Vector2Int[] coordinates, BattleshipShipType shipType)
        {
            // Only process if this is the right client
            if (!IsOwner) return;

            Debug.Log($"[{PlayerName}] ConfirmShipPlacementClientRpc received for ship type: {shipType}");
            ProcessShipPlacementConfirmation(coordinates, shipType);
        }

        // Common processing method for both host and client
        private void ProcessShipPlacementConfirmation(Vector2Int[] coordinates, BattleshipShipType shipType)
        {
            Debug.Log($"[{PlayerName}] ProcessShipPlacementConfirmation for ship type: {shipType}");

            Ship shipToPlace = FindUnplacedShipOfType(shipType);
            if (shipToPlace == null)
            {
                Debug.LogError($"[{PlayerName}] Could not find unplaced ship of type {shipType}!");
                return;
            }

            PlaceShipLocally(coordinates, shipToPlace);
            //totalShipValue += shipToPlace.GetShipLength();
            Debug.Log($"[{PlayerName}] totalShipValue updated to: {totalShipValue}");

            shipToPlace.isPlaced = true;

            // Update UI
            if (manager.myShipPlacementUI != null)
            {
                if (isReady.Value == true)
                {
                    manager.myShipPlacementUI.ClosePanel();
                }
                Debug.Log($"[{PlayerName}] Refreshing ship placement UI");
                manager.myShipPlacementUI.ShowShipPlacementPanel();
                UpdateShipUI(shipToPlace);
            }

            // Clear references
            this.shipToPlace = null;
            selectedShipType = BattleshipShipType.NONE;
        }

        private Ship FindUnplacedShipOfType(BattleshipShipType shipType)
        {
            // Find the ship in our battleships list that matches the type and isn't placed
            if (manager.myShipPlacementUI?.shipsToInstantiate != null)
            {
                foreach (Ship ship in manager.myShipPlacementUI.shipsToInstantiate)
                {
                    if (ship.GetShipType() == shipType && !ship.isPlaced)
                    {
                        return ship;
                    }
                }
            }

            Debug.LogError($"Could not find unplaced ship of type {shipType}");
            return null;
        }

        private void CheckIfAllShipsPlaced()
        {
            // Count placed ships by checking our ship tiles
            // Each ship type has a specific length, so we can calculate from totalShipValue
            int expectedTotalShipValue = 0;
            if (playerBattleships != null)
            {
                foreach (BattleshipShipType shipType in playerBattleships)
                {
                    expectedTotalShipValue += GetShipLength(shipType);
                }
            }

            Debug.Log($"[{PlayerName}] Ship placement check: totalShipValue={totalShipValue}, expectedTotal={expectedTotalShipValue}");

            // If we've placed all our ships, mark as ready
            if (totalShipValue >= expectedTotalShipValue && expectedTotalShipValue > 0)
            {
                Debug.Log($"[{PlayerName}] All ships placed! Marking player as ready.");
                isReady.Value = true;
            }
            else
            {
                Debug.Log($"[{PlayerName}] Still placing ships. Progress: {totalShipValue}/{expectedTotalShipValue}");
            }
        }

        private void UpdateShipUI(Ship placedShip)
        {
            // Find the NetworkShipUI that corresponds to this ship and mark it as placed
            var shipUIs = manager.myShipPlacementUI.GetComponentsInChildren<NetworkShipUI>();
            foreach (var shipUI in shipUIs)
            {
                if (shipUI.shipData == placedShip)
                {
                    shipUI.hasBeenPlaced = true;
                    shipUI.selected = false; // Deselect the ship
                    break;
                }
            }
        }

        [Rpc(SendTo.Owner)]
        private void RejectShipPlacementRpc()
        {
            Debug.Log("Ship placement was rejected by server");
            // Handle rejection here
        }

        // Server-only method to store ship data
        private void ServerStoreShipData(List<Vector2Int> positions, BattleshipShipType shipType)
        {
            if (!IsHost) return;

            // Create a server-side ship record
            ServerShipData shipData = new ServerShipData
            {
                shipType = shipType,
                positions = positions,
                playerId = OwnerClientId,
                isDestroyed = false
            };

            // Store in server-side data structure (you'll need to create this)
            manager.StorePlayerShip(OwnerClientId, shipData);
        }

        private bool ValidateShipPlacement(List<Vector2Int> positions, BattleshipShipType shipType)
        {
            // Check if positions are valid
            if (positions == null || positions.Count == 0)
                return false;

            // Check ship length matches expected length
            int expectedLength = GetShipLength(shipType);
            if (positions.Count != expectedLength)
            {
                Debug.Log($"Invalid ship length. Expected: {expectedLength}, Got: {positions.Count}");
                return false;
            }

            // Check all positions are within grid bounds (0-9)
            foreach (Vector2Int pos in positions)
            {
                if (pos.x < 0 || pos.x >= 10 || pos.y < 0 || pos.y >= 10)
                {
                    Debug.Log($"Position out of bounds: {pos}");
                    return false;
                }
            }

            // Check positions form a straight line (horizontal or vertical)
            if (!ArePositionsInStraightLine(positions))
            {
                Debug.Log("Positions don't form a straight line");
                return false;
            }

            // Check no overlap with existing ships for this player
            foreach (Vector2Int pos in positions)
            {
                if (IsPositionOccupied(pos))
                {
                    Debug.Log($"Position already occupied: {pos}");
                    return false;
                }
            }

            return true;
        }

        private int GetShipLength(BattleshipShipType shipType)
        {
            switch (shipType)
            {
                case BattleshipShipType.CARRIER: return 5;
                case BattleshipShipType.BATTLESHIP: return 4;
                case BattleshipShipType.CRUISER: return 3;
                case BattleshipShipType.SUBMARINE: return 3;
                case BattleshipShipType.DESTROYER: return 2;
                default: return 1;
            }
        }

        private bool ArePositionsInStraightLine(List<Vector2Int> positions)
        {
            if (positions.Count <= 1) return true;

            // Sort positions to check continuity
            positions.Sort((a, b) => {
                if (a.x != b.x) return a.x.CompareTo(b.x);
                return a.y.CompareTo(b.y);
            });

            bool isHorizontal = positions[0].x == positions[1].x;

            for (int i = 1; i < positions.Count; i++)
            {
                Vector2Int prev = positions[i - 1];
                Vector2Int curr = positions[i];

                if (isHorizontal)
                {
                    // Should be same row, consecutive columns
                    if (curr.x != prev.x || curr.y != prev.y + 1)
                        return false;
                }
                else
                {
                    // Should be same column, consecutive rows  
                    if (curr.y != prev.y || curr.x != prev.x + 1)
                        return false;
                }
            }

            return true;
        }

        private bool IsPositionOccupied(Vector2Int position)
        {
            // Check if any existing ship tiles occupy this position
            foreach (NetworkPlayerTile tile in shipTiles)
            {
                if (tile.coordinates == position && tile.isShip)
                    return true;
            }
            return false;
        }

        private void PlaceShipLocally(Vector2Int[] positions, Ship ship)
        {
            Debug.Log($"[{PlayerName}] PlaceShipLocally called - Ship: {ship.GetShipType()}, Length: {ship.GetShipLength()}");
            Debug.Log($"[{PlayerName}] Positions to place: {string.Join(", ", positions)}");

            // Get the actual tile components and mark them as ships
            List<NetworkPlayerTile> tiles = new List<NetworkPlayerTile>();

            foreach (Vector2Int pos in positions)
            {
                // For the local player, use the grid manager to get tiles
                if (manager.myGridManager.MyShipTiles.TryGetValue(pos, out GameObject tileObj))
                {
                    NetworkPlayerTile tile = tileObj.GetComponent<NetworkPlayerTile>();
                    tile.SetAsShip();
                    tiles.Add(tile);
                    shipTiles.Add(tile); // Add to player's ship tile list
                }
            }
            Debug.Log("Placing ship locally for " + PlayerName);
            // Configure the ship object
            ship.PlaceShip(tiles);       
        }

        //This only works on the host server
        [Rpc(SendTo.Server)]
        public void AttackTileRpc(Vector2Int position)
        {
            Debug.Log($"Player {PlayerName} attacking position {position}");

            manager.ProcessAttack(OwnerClientId, position);
        }

        // Called by the server to notify this player that they've been attacked
        [Rpc(SendTo.Owner)]
        public void ReceiveAttackRpc(Vector2Int position, ulong attackerId)
        {
            Debug.Log($"Player {PlayerName} received attack at {position}");

            // Check if there's a ship at this position
            bool wasHit = IsShipAtPosition(position);

            if (wasHit)
            {
                Debug.Log($"Player {PlayerName} was HIT at {position}!");
            }
            else
            {
                Debug.Log($"Player {PlayerName} was MISSED at {position}");
            }

            // Update our grid visually
            manager.myGridManager.ProcessIncomingAttack(position);

            // Send result back to server
            SendAttackResultRpc(position, wasHit, attackerId);
        }

        //This only works on the host server
        [Rpc(SendTo.Server)]
        private void SendAttackResultRpc(Vector2Int position, bool wasHit, ulong attackerId)
        {
            // Server forwards the result to the attacker
            manager.SendAttackResultToAttacker(attackerId, position, wasHit);
        }

        // Called to receive attack results from our own attacks
        [Rpc(SendTo.Owner)]
        public void ReceiveAttackResultRpc(Vector2Int position, bool wasHit)
        {
            Debug.Log($"Attack result: {position} - {(wasHit ? "HIT" : "MISS")}");

            // Update our attack grid
            manager.myGridManager.ProcessAttackResult(position, wasHit);            
        }

        private bool IsShipAtPosition(Vector2Int position)
        {
            // Check if any of our ship tiles are at this position
            foreach (NetworkPlayerTile tile in shipTiles)
            {
                if (tile.coordinates == position && tile.isShip)
                {
                    tile.isChecked = true; // Mark as hit
                    tilesEnemyHit.Add(tile);
                    totalShipValue--;
                    return true;
                }
            }
            return false;
        }

        public bool AreAllShipsSunk()
        {
            return totalShipValue <= 0;
        }      
        public void RotateShipPlacement()
        {
            if (shipRotation == BattleshipRotation.HORIZONTAL)
            {
                shipRotation = BattleshipRotation.VERTICAL;
            }
            else
            {
                shipRotation = BattleshipRotation.HORIZONTAL;
            }
        }
    }
}