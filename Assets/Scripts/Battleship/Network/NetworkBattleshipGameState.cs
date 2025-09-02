using System.Collections.Generic;
using UnityEngine;

namespace Games.Battleship
{
    /// <summary>
    /// Abstract game state class. All game states will inherit from this class.
    /// </summary>
    public abstract class NetworkBattleshipGameState
    {
        protected NetworkBattleshipManager manager;

        public NetworkBattleshipGameState(NetworkBattleshipManager manager)
        {
            this.manager = manager;
        }

        public virtual void Enter() { }
        public virtual void Update() { }
        public virtual void Exit() { }
        public virtual void HandleInput() { }
    }

    public class NetworkStartState : NetworkBattleshipGameState
    {
        public NetworkStartState(NetworkBattleshipManager manager) : base(manager) { }

        public override void Enter()
        {
            Debug.Log("Entering Start State");
            // Show start menu, initialize game parameters
        }

        public override void HandleInput()
        {
            // When player chooses to start game
            if (Input.GetKeyDown(KeyCode.Space)) // Example trigger
            {
                manager.currentTurn.Value = BattleshipTurn.SHIP_SETUP;
                manager.ChangeState(new NetworkPlaceShipsState(manager));
            }
        }

        // No need for Update or Exit since we haven't even started yet.
    }

    public class NetworkPlaceShipsState : NetworkBattleshipGameState
    {
        public NetworkPlaceShipsState(NetworkBattleshipManager manager) : base(manager) { }

        // TODO: Add ship placement UI
        public override void Enter()
        {
            Debug.Log("Entering Setup State");
            manager.currentTurn.Value = BattleshipTurn.SHIP_SETUP;
          
            // Only show the UI if this is the local machine's NetworkBattleshipManager
            if (manager.myShipPlacementUI != null)
            {
                Debug.Log($"[{(manager.IsHost ? "HOST" : "CLIENT")}] Showing ship placement panel");
                manager.myShipPlacementUI.ShowShipPlacementPanel();
                manager.myShipPlacementUI.OpenPanel();
            }
            else
            {
                Debug.LogError($"[{(manager.IsHost ? "HOST" : "CLIENT")}] myShipPlacementUI is null!");
            }
        }

        public override void Update()
        {
            NetworkPlayer localPlayer = manager.GetLocalPlayer();
            NetworkPlayer remotePlayer = manager.GetRemotePlayer();

            bool bothPlayersReady = false;

            if (localPlayer != null && remotePlayer != null)
            {

                //Debug.Log($"[{(manager.IsHost ? "HOST" : "CLIENT")}] Update check - Local({localPlayer.PlayerName}): {localPlayer.isReady.Value}, Remote({remotePlayer.PlayerName}): {remotePlayer.isReady.Value}");

                bothPlayersReady = localPlayer.isReady.Value && remotePlayer.isReady.Value;

                if (bothPlayersReady)
                {
                    Debug.Log("Both players have placed their ships, entering PlayerTurnState.");
                    manager.currentTurn.Value = BattleshipTurn.PLAYER1; // Start with player 1
                    manager.ChangeState(new NetworkPlayerTurnState(manager));
                }
            }
            else
            {
                if (localPlayer == null) Debug.Log("Local player is null in Update");
                if (remotePlayer == null) Debug.Log("Remote player is null in Update");
            }
        }
    }

    public class NetworkPlayerTurnState : NetworkBattleshipGameState
    {
        public NetworkPlayerTurnState(NetworkBattleshipManager manager) : base(manager) { }

        //TODO: Add player turn UI, and disable the other player's UI if applicable.
        public override void Enter()
        {
            // Get the local player to control their camera
            NetworkPlayer localPlayer = manager.GetLocalPlayer();
            if (localPlayer != null)
            {
                var cameraManager = localPlayer.GetComponent<NetworkCameraManager>();
                if (cameraManager != null)
                {
                    // Switch to appropriate view based on whose turn it is
                    bool isMyTurn = (manager.currentTurn.Value == BattleshipTurn.PLAYER1 && localPlayer.OwnerClientId == 0) ||
                                   (manager.currentTurn.Value == BattleshipTurn.PLAYER2 && localPlayer.OwnerClientId == 1);

                    if (isMyTurn)
                    {
                        Debug.Log("My turn - switching to attack view");
                        cameraManager.SetAttackBoardView();
                    }
                    else
                    {
                        Debug.Log("Opponent's turn - staying on player board");
                        cameraManager.SetPlayerBoardView();
                    }
                }
            }
        }

        // TODO: Check player input for attacks. Maybe we can do both keyboard input and mouse input
        public override void HandleInput()
        {

        }
    }

    public class NetworkPauseState : NetworkBattleshipGameState
    {
        private NetworkBattleshipGameState previousState; //What state do we revert back to after unpausing?
        public NetworkPauseState(NetworkBattleshipManager manager, NetworkBattleshipGameState previousState) : base(manager)
        {
            this.previousState = previousState;
        }

        // TODO: Add pause menu UI, disable game input
        public override void Enter()
        {
            Debug.Log("Game Paused");
            // Can we use Time.timeScale = 0f? What does that do?

        }

        // TODO: Handle resume input
        public override void HandleInput()
        {
            // When player chooses to resume game
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                manager.ChangeState(previousState);
            }
        }
        public override void Exit()
        {
            Debug.Log("Resuming Game");
            //Time.timeScale = 1f;
            // Hide pause menu, enable game input
        }
    }

    public class NetworkWaitingState : NetworkBattleshipGameState
    {
        public NetworkWaitingState(NetworkBattleshipManager manager) : base(manager) { }

        public override void Enter()
        {
            Debug.Log("Waiting for all players and grids to be ready...");
            // Show waiting UI or loading screen
        }

        public override void Update()
        {
            // This state will be automatically exited when all players are ready
            // The NetworkBattleshipManager handles the transition
        }

        public override void Exit()
        {
            Debug.Log("All players ready! Proceeding to game setup...");
            // Hide waiting UI
        }
    }

    public class NetworkGameOverState : NetworkBattleshipGameState
    {
        private BattleshipTurn victor;
        public NetworkGameOverState(NetworkBattleshipManager manager, BattleshipTurn victor) : base(manager)
        {
            this.victor = victor;
        }

        public override void Enter()
        {
            manager.currentTurn.Value = BattleshipTurn.GAME_OVER;
            switch (victor)
            {
                case BattleshipTurn.PLAYER1:
                    Debug.Log("Player 1 wins!");
                    break;
                case BattleshipTurn.PLAYER2:
                    Debug.Log("Player 2 wins!");
                    break;
                default:
                    Debug.Log("I have no idea who won! Nice work!");
                    break;
            }
        }

        // TODO: Handle restarting the game.
        public override void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                // Restart game
                manager.ResetGame();
            }
        }
    }
}