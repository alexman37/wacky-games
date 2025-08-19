using System.Collections.Generic;
using UnityEngine;

namespace Games.Battleship
{
    /// <summary>
    /// Abstract game state class. All game states will inherit from this class.
    /// </summary>
    public abstract class BattleshipGameState
    {
        protected BattleshipManager manager;

        public BattleshipGameState(BattleshipManager manager)
        {
            this.manager = manager;
        }

        public virtual void Enter() { }
        public virtual void Update() { }
        public virtual void Exit() { }
        public virtual void HandleInput() { }
    }

    public class StartState : BattleshipGameState
    {
        public StartState(BattleshipManager manager) : base(manager) { }

        public override void Enter()
        {
            Debug.Log("Entering Start State");
            BattleshipCameraManager.Initialize();
            // Show start menu, initialize game parameters
        }

        public override void HandleInput()
        {
            // When player chooses to start game
            if (Input.GetKeyDown(KeyCode.Space)) // Example trigger
            {
                manager.currentTurn = BattleshipTurn.GAME_SETUP;
                manager.ChangeState(new PlaceShipsState(manager));
            }
        }

        // No need for Update or Exit since we haven't even started yet.
    }

    public class PlaceShipsState : BattleshipGameState
    {
        private bool player1Ready = false;
        private bool player2Ready = false;

        public void SetPlayer1Ready() { player1Ready = true; }
        public void SetPlayer2Ready() { player2Ready = true; }
        public PlaceShipsState(BattleshipManager manager) : base(manager) { }

        // TODO: Add ship placement UI
        public override void Enter()
        {
            Debug.Log("Entering Setup State");
            manager.currentTurn = BattleshipTurn.GAME_SETUP;
            ShipPlacementUI.Instance.ShowShipPlacementPanel();
        }

        public override void Update()
        {
            // Check if both players have placed their ships
            if (player1Ready && player2Ready)
            {
                ShipPlacementUI.Instance.ShowShipPlacementPanel(); //Closes ship placement UI
                manager.ChangeState(new PlayerTurnState(manager));
            }

        }

        public override void HandleInput()
        {
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            // Handle ship placement input for both players
            // This could be mouse clicks on the grid to place ships
            // For example, if player 1 places a ship, call SetPlayer1Ready()
            // and similarly for player 2.
            
            if(scrollInput != 0) // Allows the players to place ships horizontally or vertically
            {
                BattleshipManager.Instance.RotateShipPlacement();
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                Debug.Log("Hitting R");
                BattleshipCameraManager.RotateCamera();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                // Handle pause input
                manager.ChangeState(new PauseState(manager, this));
            }
            else if(Input.GetKeyDown(KeyCode.Space))
            {
                // For testing purposes, we can set both players ready
                manager.currentTurn = BattleshipTurn.PLAYER1; //Set to player 1's turn for testing purposes
                SetPlayer1Ready();
                SetPlayer2Ready();
                ShipPlacementUI.Instance.ClosePanel(); //Closes ship placement UI
            }
        }

    }

    public class PlayerTurnState : BattleshipGameState
    {
        public PlayerTurnState(BattleshipManager manager) : base(manager) { }

        //TODO: Add player turn UI, and disable the other player's UI if applicable.
        public override void Enter()
        {
            switch (manager.currentTurn)
            {
                case BattleshipTurn.PLAYER1:
                    Debug.Log("Player 1's Turn");
                    // Activate player 1's UI, deactivate player 2's
                    break;
                case BattleshipTurn.PLAYER2:
                    Debug.Log("Player 2's Turn");
                    // Activate player 2's UI, deactivate player 1's
                    break;
                default:
                    Debug.LogError("Invalid turn state!");
                    break;
            }          
        }

        // TODO: Check player input for attacks. Maybe we can do both keyboard input and mouse input
        public override void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                Debug.Log("Hitting R");
                BattleshipCameraManager.RotateCamera();
            }
        }

        public void EndTurn()
        {
            // Check win condition
            if (manager.CheckWinCondition())
            {
                manager.ChangeState(new GameOverState(manager, manager.currentTurn));
            }
            else
            {
                if(manager.currentTurn == BattleshipTurn.PLAYER1)
                {
                    manager.currentTurn = BattleshipTurn.PLAYER2;
                }
                else
                {
                    manager.currentTurn = BattleshipTurn.PLAYER1;
                }
                manager.ChangeState(new PlayerTurnState(manager));
            }
        }
    }

    public class PauseState : BattleshipGameState
    {
        private BattleshipGameState previousState; //What state do we revert back to after unpausing?
        public PauseState(BattleshipManager manager, BattleshipGameState previousState) : base(manager) 
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
    
    public class GameOverState : BattleshipGameState
    {
        private BattleshipTurn victor;
        public GameOverState(BattleshipManager manager, BattleshipTurn victor) : base(manager) 
        {
            this.victor = victor;
        }
        
        public override void Enter()
        {
            manager.currentTurn = BattleshipTurn.GAME_OVER;
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