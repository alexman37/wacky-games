using System.Collections.Generic;
using UnityEngine;
using System;

namespace Games.Battleship
{
    public enum BattleshipTurn { NONE, PRE_START, SHIP_SETUP, PLAYER1, PLAYER2, GAME_OVER }
    public enum BattleshipShipType { CARRIER, BATTLESHIP, CRUISER, SUBMARINE, DESTROYER, NONE }
    // Carrier : 5 tiles, Battleship: 4 tiles, Cruiser: 3 tiles, Submarine: 3 tiles, Destroyer: 2 tiles, Patrol Boat: 2 tiles
    // Add more as needed for different game modes.
    public enum BattleshipGameMode { CLASSIC, HUNTER }
    public enum BattleshipRotation { NONE, HORIZONTAL, VERTICAL }





    public class BattleshipManager : MonoBehaviour
    {
        public static BattleshipManager Instance;

        public const int GridWidth = 10;
        public const int GridHeight = 10;
        public BattleshipTurn currentTurn;
        public List<BattleshipShipType> shipTypes;
        public BattleshipShipType selectedShipType = BattleshipShipType.NONE;
        public Ship selectedShip;
        public BattleshipGameMode gameMode = BattleshipGameMode.CLASSIC;
        public BattleshipRotation shipRotation; // When the player is hovering tiles, what rotation is the ship in?
        private BattleshipGameState currentState;       
        private List<Ship> createdShips; //List of ship objects created by the gamemode.
        public GameObject playerPrefab;
        public BattleshipPlayer player1Component;
        public BattleshipAI player2Component;
        public List<GameObject> shipPrefabs; // List of ship prefabs to instantiate when placing ships.
        private GameObject currentShipPrefab; // The current ship prefab being placed.


        public static event Action<string> changedGameState;

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

            changedGameState += (_) => { };
            

            ChangeState(new StartState(this));
        }

        public void Update()
        {
            if (currentState != null)
            {
                currentState.HandleInput();

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
                Ship newShip = new Ship(shipType);
                createdShips.Add(newShip);
            }
            GameObject player1 = Instantiate(playerPrefab);
            player1.name = "Player 1";
            player1Component = player1.GetComponent<BattleshipPlayer>();
            player1Component.Initialize(true);

            // easier to just build the CPU player from scratch
            GameObject player2 = new GameObject();
            player2.name = "Player 2";
            player2.AddComponent(typeof(BattleshipAI));
            player2Component = player2.GetComponent<BattleshipAI>();
            player2Component.Initialize(createdShips);

            GridManager.Instance.GenerateGrid(GridWidth, GridHeight);           
        }

        public List<Ship> GetShips()
        {
            return createdShips;
        }

        public void SetShipType(Ship ship)
        {
            Debug.Log("Selected ship type: " + ship.shipType);
            selectedShipType = ship.shipType;
            selectedShip = ship;
            if(currentShipPrefab != null)
            {
                Destroy(currentShipPrefab);
            }
            currentShipPrefab = Instantiate(shipPrefabs[(int)selectedShipType]);
            HandleRotation(shipRotation);
            currentShipPrefab.AddComponent<BattleshipMouseFollower>();
        }

        public void HandleRotation(BattleshipRotation rotation)
        {
            GridManager.Instance.StopRotationHighlight();
            if (currentShipPrefab != null)
            {
                if(rotation == BattleshipRotation.HORIZONTAL)
                {
                    currentShipPrefab.transform.rotation = Quaternion.Euler(0, 0, 0);
                }
                else if(rotation == BattleshipRotation.VERTICAL)
                {
                    currentShipPrefab.transform.rotation = Quaternion.Euler(0, 90, 0);
                }
            }
        }

        public void PlaceShipModel(Vector3 shipPos)
        {
            GameObject newShip = Instantiate(currentShipPrefab);
            newShip.transform.localPosition = shipPos;
            newShip.name = selectedShipType.ToString() + "_new";
            Destroy(newShip.GetComponent<BattleshipMouseFollower>());
            Destroy(currentShipPrefab);
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

            changedGameState.Invoke(newState.GetType().ToString());
        }

        // Used for like tile highlighting depending on if its the player turn or setup phase
        public BattleshipTurn GetCurrentTurn()
        {
            return currentTurn;
        }

        // Since we are checking each player here, should we return an enum or something saying specifically
        // who won?
        public WinCondition CheckWinCondition()
        {
            // Check if all ships of a player are sunk
            if (player1Component.AreAllShipsSunk())
            {
                return WinCondition.PLAYER2_WIN; // Player 2 wins
            }

            if (player2Component.AreAllShipsSunk())
            {
                return WinCondition.PLAYER1_WIN; // Player 1 wins
            }
            return WinCondition.NONE;
        }

        /// <summary>
        /// Cycle from Player 1 to Player 2
        /// </summary>
        public void EndTurn()
        {
            // Check win condition
            WinCondition winCondition = CheckWinCondition();
            if (winCondition != WinCondition.NONE)
            {
                ChangeState(new GameOverState(this, winCondition));
            }
            else
            {
                if (currentTurn == BattleshipTurn.PLAYER1)
                {
                    currentTurn = BattleshipTurn.PLAYER2;
                    ChangeState(new CPUTurnState(this));
                }
                else
                {
                    currentTurn = BattleshipTurn.PLAYER1;
                    ChangeState(new PlayerTurnState(this));
                }
            }
        }

        public void ResetGame()
        {

        }

        
        }
    public enum WinCondition { NONE, PLAYER1_WIN, PLAYER2_WIN }
}


