// Create this class structure (don't implement yet):
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


namespace Games.Battleship
{
    public class NetworkBattleshipManager : NetworkBehaviour
    {
        [Header("Grid Management")]
        public GameObject gridManagerPrefab;

        [Header("UI References")]
        public NetworkShipPlacementUI myShipPlacementUI;

        public NetworkVariable<BattleshipTurn> currentTurn = new NetworkVariable<BattleshipTurn>();

        // Network variables to track readiness
        public NetworkVariable<bool> hostPlayerReady = new NetworkVariable<bool>();
        public NetworkVariable<bool> clientPlayerReady = new NetworkVariable<bool>();
        public NetworkVariable<bool> hostGridReady = new NetworkVariable<bool>();
        public NetworkVariable<bool> clientGridReady = new NetworkVariable<bool>();

        // Local grid manager reference
        public NetworkGridManager myGridManager;
        private NetworkBattleshipGameState currentState;

        public BattleshipGameMode gameMode = BattleshipGameMode.CLASSIC;
        List<BattleshipShipType> shipTypes;
        bool isClient = false;
        public Camera startCamera;
        public override void OnNetworkSpawn()
        {
            shipTypes = BattleshipGameModes.GetShipTypes(gameMode);
            Debug.Log("ShipTypes loaded: " + shipTypes.Count);
            Debug.Log($"[{(IsHost ? "HOST" : "CLIENT")}] NetworkBattleshipManager spawned");

            // Subscribe to readiness changes
            hostPlayerReady.OnValueChanged += OnReadinessChanged;
            clientPlayerReady.OnValueChanged += OnReadinessChanged;
            hostGridReady.OnValueChanged += OnReadinessChanged;
            clientGridReady.OnValueChanged += OnReadinessChanged;

            if (NetworkManager.Singleton != null)
            {
                // Increase timeouts for internet connections
                NetworkManager.Singleton.NetworkConfig.ClientConnectionBufferTimeout = 10;
            }

            currentTurn.Value = BattleshipTurn.PRE_START;
            
            // Each client creates their own grid manager locally
            CreateLocalGridManager();
            // Subscribe to player connections
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

            // Hide the connection UI now that we're connected
            var connectionUI = FindFirstObjectByType<NetworkMenuUI>();
            connectionUI?.HideConnectionUI();
        }

        void OnClientConnected(ulong clientId)
        {
            Debug.Log($"Client {clientId} connected. IsHost: {IsHost}");
            // This is just for logging now - initialization happens via RPC
        }

        public void InitializePlayer(NetworkPlayer player)
        {
            Debug.Log($"Initializing player: {player.PlayerName}");

            shipTypes = BattleshipGameModes.GetShipTypes(gameMode);
            player.Initialize(shipTypes);

            // Host is always ClientId 0, each client is ClientId 1, 2, etc.
            bool isHostPlayer = IsHost;

            // Set appropriate readiness flag
            // Doesn't seem like this is actually working, at least on local host
            // Both the host and client go to the host player segment
            if (isHostPlayer)
            {
                hostPlayerReady.Value = true;
                Debug.Log($"Host player (ClientId: {player.OwnerClientId}) marked as ready");
            }
            else
            {
                clientPlayerReady.Value = true;
                Debug.Log($"Client player (ClientId: {player.OwnerClientId}) marked as ready");
                isClient = true;
            }
        }

        void CreateLocalGridManager()
        {
            // Each client creates their own grid manager (not networked)
            var gridManagerObj = Instantiate(gridManagerPrefab);
            myGridManager = gridManagerObj.GetComponent<NetworkGridManager>();
            myGridManager.SetNetworkManager(this);
            // Link the grid manager to the local player once they spawn
            StartCoroutine(LinkGridToLocalPlayer());
        }

        System.Collections.IEnumerator LinkGridToLocalPlayer()
        {
            // Wait for local player to spawn
            while (GetLocalPlayer() == null)
            {
                yield return null;
            }

            NetworkPlayer localPlayer = GetLocalPlayer();
            Debug.Log($"[{(IsHost ? "HOST" : "CLIENT")}] LinkGridToLocalPlayer - Found local player: {localPlayer.PlayerName}");

            // Link grid manager to local player
            myGridManager.SetOwningPlayer(localPlayer);

            // Give the local player to the ship UI - use local reference instead of singleton
            if (myShipPlacementUI != null)
            {
                Debug.Log($"[{(IsHost ? "HOST" : "CLIENT")}] myShipPlacementUI exists, calling GetLocalPlayer and ShipsToPlace");
                myShipPlacementUI.GetLocalPlayer(localPlayer);
                myShipPlacementUI.ShipsToPlace(shipTypes);
            }
            else
            {
                Debug.LogError($"[{(IsHost ? "HOST" : "CLIENT")}] myShipPlacementUI is NULL!");
            }
            NotifyGridReadyRpc(isClient);
        }

        [Rpc(SendTo.Server)]
        void NotifyGridReadyRpc(bool callerIsClient)
        {
            Debug.Log($"Grid ready notification - caller is client: {callerIsClient}");

            if (callerIsClient)
            {
                clientGridReady.Value = true;
                Debug.Log("Client grid marked as ready");
            }
            else
            {
                hostGridReady.Value = true;
                Debug.Log("Host grid marked as ready");
            }
        }

        void OnReadinessChanged(bool previousValue, bool newValue)
        {
            CheckAllPlayersReady();
        }

        void CheckAllPlayersReady()
        {
            bool bothPlayersReady = hostPlayerReady.Value && clientPlayerReady.Value;
            bool bothGridsReady = hostGridReady.Value && clientGridReady.Value;

            if (bothPlayersReady && bothGridsReady)
            {
                Debug.Log("All players and grids are ready! Starting game...");
                startCamera.enabled = false;
                StartGameRpc();
            }
            else
            {
                Debug.Log($"Waiting for readiness: Players({hostPlayerReady.Value}, {clientPlayerReady.Value}) Grids({hostGridReady.Value}, {clientGridReady.Value})");
            }
        }

        [Rpc(SendTo.Everyone)]
        void StartGameRpc()
        {
            // Transition to the ship placement state
            ChangeState(new NetworkPlaceShipsState(this));
        }

        // Called by NetworkPlayer when an attack is made (server-side processing)
        public void ProcessAttack(ulong attackerId, Vector2Int position)
        {
            if (!IsHost) return; // Only host server processes attacks, since the client also has this class
            // in their instance

            Debug.Log($"Processing attack from {attackerId} at position {position}");

            // Find the target player (the one being attacked)
            NetworkPlayer targetPlayer = GetTargetPlayer(attackerId);

            if (targetPlayer != null)
            {
                // Send attack to target player
                targetPlayer.ReceiveAttackRpc(position, attackerId);
                EndTurn();
            }
        }

        // Called by server to send attack results back to the attacker
        public void SendAttackResultToAttacker(ulong attackerId, Vector2Int position, bool wasHit)
        {
            if (!IsHost) return; // Only host server processes attacks, since the client also has this class
            // in their instance

            NetworkPlayer attacker = GetPlayerByClientId(attackerId);
            if (attacker != null)
            {
                attacker.ReceiveAttackResultRpc(position, wasHit);
            }          
        }

        private NetworkPlayer GetTargetPlayer(ulong attackerId)
        {
            // If host attacks, target is client. If client attacks, target is host.
            if (attackerId == NetworkManager.Singleton.LocalClientId)
            {
                return GetRemotePlayer(); // Host attacking, so target is client
            }
            else
            {
                return GetLocalPlayer(); // Client attacking, so target is host
            }
        }

        private NetworkPlayer GetPlayerByClientId(ulong clientId)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                return GetLocalPlayer();
            }
            else
            {
                return GetRemotePlayer();
            }
        }   

        // Called after attack is processed to end the current turn
        public void EndTurn()
        {
            if (CheckWinCondition())
            {
                Debug.Log("Win condition met!");
                switch (currentTurn.Value)
                {
                    case BattleshipTurn.PLAYER1:
                        Debug.Log("Player 1 wins!");
                        break;
                    case BattleshipTurn.PLAYER2:
                        Debug.Log("Player 2 wins!");
                        break;
                }
                currentTurn.Value = BattleshipTurn.GAME_OVER;
                ChangeState(new NetworkGameOverState(this, currentTurn.Value));               
            }
            if (currentTurn.Value == BattleshipTurn.PLAYER1)
            {
                Debug.Log("Ending Player 1's turn, switching to Player 2");
                currentTurn.Value = BattleshipTurn.PLAYER2;
            }
            else if (currentTurn.Value == BattleshipTurn.PLAYER2)
            {
                Debug.Log("Ending Player 2's turn, switching to Player 1");
                currentTurn.Value = BattleshipTurn.PLAYER1;
            }
            Debug.Log($"New turn: {currentTurn.Value}");
            // Check win condition after turn
            
        }

        // Method to get reference to players after they spawn
        public NetworkPlayer GetLocalPlayer()
        {
            var playerObj = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
            return playerObj?.GetComponent<NetworkPlayer>();
        }

        public NetworkPlayer GetRemotePlayer()
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClients)
            {
                if (client.Key != NetworkManager.Singleton.LocalClientId)
                {
                    var playerObj = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(client.Key);
                    return playerObj?.GetComponent<NetworkPlayer>();
                }
            }
            return null;
        }


        [Rpc(SendTo.Everyone)]
        public void SyncGameStateRpc()
        {
            // TODO: Sync critical game state between players
        }

        public void ChangeState(NetworkBattleshipGameState newState)
        {
            if (currentState != null)
            {
                currentState.Exit();
            }

            currentState = newState;

            if (currentState != null)
            {
                currentState.Enter();
            }
        }

        // Used for like tile highlighting depending on if its the player turn or setup phase
        public BattleshipTurn GetCurrentTurn()
        {
            return currentTurn.Value;
        }
        public bool CheckWinCondition()
        {
            // Check if all ships of a player are sunk
            NetworkPlayer localPlayer = GetLocalPlayer();
            NetworkPlayer remotePlayer = GetRemotePlayer();

            if (localPlayer?.AreAllShipsSunk() == true)
            {
                return true; // Remote player wins
            }

            if (remotePlayer?.AreAllShipsSunk() == true)
            {
                return true; // Local player wins
            }
            return false;
        }

        public void ResetGame()
        {
            // Reset all readiness flags
            hostPlayerReady.Value = false;
            clientPlayerReady.Value = false;
            hostGridReady.Value = false;
            clientGridReady.Value = false;

            // Reset turn
            currentTurn.Value = BattleshipTurn.PRE_START;

            // Go back to waiting state
            ChangeState(new NetworkWaitingState(this));
        }

        void Update()
        {
            currentState?.Update();
            currentState?.HandleInput();
        }

        public override void OnDestroy()
        {
            // Unsubscribe from events
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            }

            // Unsubscribe from NetworkVariable changes
            hostPlayerReady.OnValueChanged -= OnReadinessChanged;
            clientPlayerReady.OnValueChanged -= OnReadinessChanged;
            hostGridReady.OnValueChanged -= OnReadinessChanged;
            clientGridReady.OnValueChanged -= OnReadinessChanged;

            base.OnDestroy();
        }
    }
}