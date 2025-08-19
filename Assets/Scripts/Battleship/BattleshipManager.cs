using System.Collections.Generic;
using UnityEngine;

namespace Games.Battleship
{
    public class BattleshipManager : MonoBehaviour
    {
        public static BattleshipManager Instance;
        public const int ShipsPerPlayer = 10;
        public const int GridWidth = 10;
        public const int GridHeight = 10;
        public BattleshipTurn currentTurn;
        public List<BattleshipShipType> shipTypes;
        public BattleshipShipType selectedShipType;
        public BattleshipGameMode gameMode = BattleshipGameMode.CLASSIC;
        public BattleshipRotation shipRotation; // When the player is hovering tiles, what rotation is the ship in?
        private BattleshipGameState currentState;       
        private List<Ship> createdShips; //List of ship objects created by the gamemode.
        public GameObject playerPrefab;
        public BattleshipPlayer player1Component;
        public BattleshipPlayer player2Component;
        public BattleshipManager()
        {
            if(Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

        public void Start()
        {
            createdShips = new List<Ship>();
            shipRotation = BattleshipRotation.HORIZONTAL;
            if (currentTurn == BattleshipTurn.NONE)
            {
                InitializeGame();
            }
            

            ChangeState(new StartState(this));
        }

        public void Update()
        {
            if (currentState != null)
            {
                currentState.HandleInput();
                currentState.Update();

                // Handle pause anywhere except in pause or game over states
                if (Input.GetKeyDown(KeyCode.Escape) &&
                    !(currentState is PauseState) &&
                    !(currentState is GameOverState))
                {
                    ChangeState(new PauseState(this, currentState));
                }
            }
        }

        void InitializeGame()
        {
            // Once the start state goes to place ships state, we update the current Turn to be game setup
            currentTurn = BattleshipTurn.NONE;
            shipTypes = BattleshipGameModes.GetShipTypes(gameMode);
            foreach(BattleshipShipType shipType in shipTypes)
            {
                Debug.Log("Ship Type: " + shipType);
                Ship newShip = new Ship(shipType);
                createdShips.Add(newShip);
            }
            GameObject player1 = Instantiate(playerPrefab);
            player1.name = "Player 1";
            player1Component = player1.GetComponent<BattleshipPlayer>();
            player1Component.Initialize(shipTypes, true);


            GameObject player2 = Instantiate(playerPrefab);
            player2.name = "Player 2";
            player2Component = player2.GetComponent<BattleshipPlayer>();
            player2Component.Initialize(shipTypes, false);

            GridManager.Instance.GenerateGrid(GridWidth, GridHeight);           
        }

        public List<Ship> GetShips()
        {
            return createdShips;
        }

        public void SetShipType(BattleshipShipType shipType)
        {
            Debug.Log("Selected Ship Type " + shipType.ToString());
            selectedShipType = shipType;
        }

        public BattleshipShipType GetSelectedShipType()
        {
            return selectedShipType;
        }

        public void ChangeState(BattleshipGameState newState)
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
            return currentTurn;
        }

        // Since we are checking each player here, should we return an enum or something saying specifically
        // who won?
        public bool CheckWinCondition()
        {
            // Check if all ships of a player are sunk
            if (player1Component.AreAllShipsSunk())
            {
                return true; // Player 2 wins
            }

            if (player2Component.AreAllShipsSunk())
            {
                return true; // Player 1 wins
            }
            return false;
        }

        public void ResetGame()
        {

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
    public enum BattleshipTurn { NONE, GAME_SETUP, PLAYER1, PLAYER2, GAME_OVER }
    public enum BattleshipShipType { CARRIER, BATTLESHIP, CRUISER, SUBMARINE, DESTROYER, NONE }
    // Carrier : 5 tiles, Battleship: 4 tiles, Cruiser: 3 tiles, Submarine: 3 tiles, Destroyer: 2 tiles, Patrol Boat: 2 tiles
    // Add more as needed for different game modes.
    public enum BattleshipGameMode { CLASSIC}
    public enum BattleshipRotation { HORIZONTAL, VERTICAL }
}


