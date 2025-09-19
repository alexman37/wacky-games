using System.Collections.Generic;
using UnityEngine;

namespace Games.Battleship
{
    /// <summary>
    /// Abstract game state class. All game states will inherit from this class.
    /// </summary>
    /// 

    // Summary of states
    // START: The game is setting up, the player should not be able to interact with anything.
    // PLACE SHIPS: The player is placing their ships.
    // PLAYER TURN: Either the human (player 1) or the bot (player 2) is taking their turn
    // PAUSE: The game is paused, players should not be able to take actions
    // GAME OVER: The game has ended
    public abstract class BattleshipGameState
    {
        protected BattleshipManager manager;

        // all state variables are in battleship manager
        public BattleshipGameState(BattleshipManager manager)
        {
            this.manager = manager;
        }

        // what to do when you first change to this state?
        public virtual void Enter() { }
        // what to do each frame while you are in this state?
        public virtual void Exit() { }
        // what inputs to look for while you are in this state?
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

        // No need for Update or Exit since we haven't even started yet.
    }

    public class PlaceShipsState : BattleshipGameState
    {
        // The single player in this game is always player 1
        private bool player1Ready = false;

        public void SetPlayer1Ready() { player1Ready = true; }
        public PlaceShipsState(BattleshipManager manager) : base(manager) { }

        // TODO: Add ship placement UI
        public override void Enter()
        {
            Debug.Log("Entering Setup State");
            manager.currentTurn = BattleshipTurn.SHIP_SETUP;
            ShipPlacementUI.Instance.ShowShipPlacementPanel();

            manager.selectedShipType = BattleshipShipType.NONE;
        }

        public override void HandleInput()
        {
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            
            if(scrollInput != 0) // Allows the player to place ships horizontally or vertically
            {
                if (manager.shipRotation == BattleshipRotation.HORIZONTAL)
                {
                    manager.shipRotation = BattleshipRotation.VERTICAL;

                }
                else
                {
                    manager.shipRotation = BattleshipRotation.HORIZONTAL;
                }
                manager.HandleRotation(manager.shipRotation);
            }
            // TODO I do not understand what this is doing.
            if(Input.GetMouseButtonDown(0))
            {
                if (player1Ready)
                {
                    ShipPlacementUI.Instance.ClosePanel(); //Closes ship placement UI
                    manager.ChangeState(new PlayerTurnState(manager));
                }
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
                // For testing purposes, we can set the player as ready whenever space hit
                manager.currentTurn = BattleshipTurn.PLAYER1; //Set to player 1's turn for testing purposes
                SetPlayer1Ready();
                ShipPlacementUI.Instance.ClosePanel(); //Closes ship placement UI
            }
        }

    }

    public class PlayerTurnState : BattleshipGameState
    {
        public PlayerTurnState(BattleshipManager manager) : base(manager) { }

        //TODO: Add player turn UI
        public override void Enter()
        {
            Debug.Log("Player 1's Turn");
            BattleshipTopBarUI.instance.displayDebugInfo("It's your Turn");
            BattleshipCameraManager.instance.TransitionToCameraPlayer2View();
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
    }

    public class CPUTurnState : BattleshipGameState
    {
        public CPUTurnState(BattleshipManager manager) : base(manager) { }

        //TODO: Add player turn UI
        public override void Enter()
        {
            Debug.Log("Player 2's Turn");
            BattleshipTopBarUI.instance.displayDebugInfo("The bot is thinking");
            BattleshipCameraManager.instance.TransitionToCameraPlayer1View();

            manager.player2Component.TakeCPUTurn();
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
        private WinCondition victor;
        public GameOverState(BattleshipManager manager, WinCondition victor) : base(manager) 
        {
            this.victor = victor;
        }
        
        public override void Enter()
        {
            manager.currentTurn = BattleshipTurn.GAME_OVER;
            switch (victor)
            {
                case WinCondition.PLAYER1_WIN:
                    Debug.Log("Player 1 wins!");
                    break;
                case WinCondition.PLAYER2_WIN:
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