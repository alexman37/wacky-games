using UnityEngine;
using System.Collections.Generic;

namespace Games.Battleship
{
    /// Everything to do with the CPU player
    public class BattleshipAI : BattleshipPlayer
    {
        // Functionally it's the same setup as Player1 but we use "ShotTiles" instead of "PlayerTiles" for convenience
        public new List<ShotTile> shipTiles = new List<ShotTile>(); // To represent this player's ships
        public new List<ShotTile> checkedTiles = new List<ShotTile>(); // To represent which tiles this player has checked
        public new List<ShotTile> tilesEnemyHit = new List<ShotTile>(); // To represent which tiles of the player have been attacked

        private List<Ship> activeShipsList;

        // thinking logic
        BattleshipAI_DecisionMatrix decisionMatrix = new BattleshipAI_DecisionMatrix();

        private void OnEnable()
        {
            finishedPlacingPlayerShips += SetupRoutine;
        }

        private void OnDisable()
        {
            finishedPlacingPlayerShips -= SetupRoutine;
        }

        public void Initialize(List<Ship> createdShips)
        {
            playerBattleships = new List<BattleshipShipType>();
            isPlayer1 = false;

            activeShipsList = createdShips;
            playerBattleships = BattleshipManager.Instance.shipTypes;

            // thankfully you only have to do this once on startup
            decisionMatrix.recalculateEntireGrid();
            decisionMatrix.redrawDebugViz();
        }


        /// ----------------------------------
        ///            SETUP PHASE
        /// ----------------------------------

        /// <summary>
        /// The CPU places its ships in a mostly random fashion.
        /// </summary>
        public void SetupRoutine()
        {
            foreach(Ship ship in activeShipsList)
            {
                // Horizontal or vertical?

                int attempt = 0;

                findTile:
                while(attempt < 20)
                {
                    bool horiz = Random.value > 0.5f;

                    int x; int y;
                    if (horiz)
                    {
                        x = Random.Range(0, BattleshipManager.GridWidth - ship.shipLength);
                        y = Random.Range(0, BattleshipManager.GridHeight);
                    }
                    else
                    {
                        x = Random.Range(0, BattleshipManager.GridWidth);
                        y = Random.Range(0, BattleshipManager.GridHeight - ship.shipLength);
                    }

                    List<ShotTile> working = new List<ShotTile>();
                    for (int sp = 0; sp < ship.shipLength; sp++)
                    {
                        ShotTile shot = GridManager.Instance.GetShotTileFromPosition(horiz ? x + sp : x, horiz ? y : y + sp);
                        if (shot.hasShip)
                        {
                            attempt++;
                            goto findTile;
                        }
                        working.Add(shot);
                    }

                    // if there are no issues with it, then commit to placing the ship here
                    PlaceCPUShip(working, ship);
                    break;
                }
                
                if(attempt >= 20)
                {
                    Debug.LogError("Failed to find suitable placements for the CPU player's fleet.");
                    throw new System.Exception("Failed to find suitable placements for the CPU player's fleet.");
                }
            }
        }


        public void PlaceCPUShip(List<ShotTile> tiles, Ship ship)
        {
            // AI will be smart about ships and only place each once
            Debug.Log("Placed CPU ship " + ship.shipType);
            BattleshipTopBarUI.instance.displayDebugInfo("Placed CPU ship " + ship.shipType);

            shipTiles.AddRange(tiles);
            ship.PlaceShip(tiles);
            foreach (ShotTile t in tiles) t.SetAsShip(ship);

            totalShipValue++;
            playerBattleships.Add(ship.shipType);

            if (totalShipValue >= ShipPlacementUI.Instance.createdShips.Count)
            {
                Debug.Log("All CPU ships placed");
                BattleshipTopBarUI.instance.displayDebugInfo("All CPU ships placed... ");

                // TODO Flip a coin? Roll dice? Determine some way of who goes first?
                BattleshipManager.Instance.currentTurn = BattleshipTurn.PLAYER1;
                BattleshipManager.Instance.ChangeState(new PlayerTurnState(BattleshipManager.Instance));
            }
        }


        /// ----------------------------------
        ///           PLAYING PHASE
        /// ----------------------------------
        // shoot at a single completely random tile
        public void chooseTotallyRandomTile()
        {

        }


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            
        }


    }











    /// <summary>
    /// This is how the AI determines where to shoot at.
    /// </summary>
    public class BattleshipAI_DecisionMatrix
    {
        /// <summary>
        /// A miniature version of a tile; it contains only the essential data needed for making decisions
        /// </summary>
        private class DecisionMatrixTile
        {
            public Vector2 coordinates;
            public int score; // this is what the decision matrix primarily uses
            public bool revealed;
            public bool hit;

            public DecisionMatrixTile(int w, int h)
            {
                coordinates = new Vector2(w, h);

                revealed = false;
                hit = false;
            }
        }

        private DecisionMatrixTile[,] decisionMatrix;

        public BattleshipAI_DecisionMatrix()
        {
            decisionMatrix = new DecisionMatrixTile[BattleshipManager.GridWidth, BattleshipManager.GridHeight];
            for(int w = 0; w < BattleshipManager.GridWidth; w++)
            {
                for (int h = 0; h < BattleshipManager.GridHeight; h++)
                {
                    decisionMatrix[w, h] = new DecisionMatrixTile(w, h);
                }
            }
        }

        /// <summary>
        /// Recalculate everything - expensive! avoid when possible
        /// </summary>
        public void recalculateEntireGrid()
        {
            // For all ships
            // TODO: Just do this for whatever ships the player still has standing
            List<Ship> ships = BattleshipManager.Instance.GetShips();

            // For each tile
            for (int w = 0; w < BattleshipManager.GridWidth; w++)
            {
                for (int h = 0; h < BattleshipManager.GridHeight; h++)
                {
                    recalculateTile(w, h, ships);
                }
            }
            
        }

        /// <summary>
        /// Get the current decision matrix as a 2D int array of just the scores
        /// </summary>
        private int[,] extractScores()
        {
            int[,] scoreArr = new int[BattleshipManager.GridWidth, BattleshipManager.GridHeight];

            for (int w = 0; w < BattleshipManager.GridWidth; w++)
            {
                for (int h = 0; h < BattleshipManager.GridHeight; h++)
                {
                    scoreArr[w, h] = decisionMatrix[w, h].score;
                }
            }

            return scoreArr;
        }

        /// <summary>
        /// Tell the debug drawer, if it exists, to redraw
        /// </summary>
        public void redrawDebugViz()
        {
            if(DecisionMatrixDebugUI.instance != null)
            {
                DecisionMatrixDebugUI.instance.redrawGrid(extractScores());
            }
        }

        private void recalculateTile(int w, int h, List<Ship> ships)
        {
            for (int s = 0; s < ships.Count; s++)
            {
                Ship ship = ships[s];

                DecisionMatrixTile tile = decisionMatrix[w, h];
                // (ignore if already revealed)
                if (!decisionMatrix[w, h].revealed)
                {
                    // For each horizontal arrangement of the ship
                    for (int lh = 0; lh < ship.shipLength; lh++)
                    {
                        // Determine if the arrangement is valid
                        bool workingArrangement = true;
                        for (int b = 0; b < lh; b++)
                        {
                            if (w - b <= 0 || decisionMatrix[w - b, h].revealed)
                            {
                                workingArrangement = false;
                                break;
                            }
                        }
                        for (int a = lh + 1; a < ship.shipLength && workingArrangement; a++)
                        {
                            if (w + a - lh >= BattleshipManager.GridWidth || decisionMatrix[w + a - lh, h].revealed)
                            {
                                workingArrangement = false;
                                break;
                            }
                        }
                        // If valid, add a point to the score
                        if (workingArrangement) tile.score += 1;
                    }


                    // For each vertical arrangement of the ship
                    for (int lv = 0; lv < ship.shipLength; lv++)
                    {
                        // Determine if the arrangement is valid
                        bool workingArrangement = true;
                        for (int b = 0; b < lv; b++)
                        {
                            if (h - b <= 0 || decisionMatrix[w, h - b].revealed)
                            {
                                workingArrangement = false;
                                break;
                            }
                        }
                        for (int a = lv + 1; a < ship.shipLength && workingArrangement; a++)
                        {
                            if (h + a - lv >= BattleshipManager.GridHeight || decisionMatrix[w, h + a - lv].revealed)
                            {
                                workingArrangement = false;
                                break;
                            }
                        }
                        // If valid, add a point to the score
                        if (workingArrangement) tile.score += 1;
                    }

                    Debug.Log("Final score assoc. with tile " + w + "," + h + ":" + decisionMatrix[w, h].score);
                }
            }
        }
    }
}
