using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Games.Battleship
{
    /// Everything to do with the CPU player
    public class BattleshipAI : BattleshipPlayer
    {
        private List<Ship> enemyShipsRemaining;

        // thinking logic
        BattleshipAI_DecisionMatrix decisionMatrix = new BattleshipAI_DecisionMatrix();

        void Start()
        {

        }

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

            enemyShipsRemaining = createdShips;
            playerBattleships = BattleshipManager.Instance.shipTypes;

            // thankfully you only have to do this once on startup
            decisionMatrix.RecalculateEntireGrid(enemyShipsRemaining);
            decisionMatrix.RedrawDebugViz();
        }


        /// ----------------------------------
        ///            SETUP PHASE
        /// ----------------------------------

        /// <summary>
        /// The CPU places its ships in a mostly random fashion.
        /// </summary>
        public void SetupRoutine()
        {
            foreach(Ship ship in enemyShipsRemaining) // at the start this is every ship
            {
                // Horizontal or vertical?

                int attempt = 0;

                findTile:
                while(attempt < 20)
                {
                    bool horiz = UnityEngine.Random.value > 0.5f;

                    int x; int y;
                    if (horiz)
                    {
                        x = UnityEngine.Random.Range(0, BattleshipManager.GridWidth - ship.shipLength);
                        y = UnityEngine.Random.Range(0, BattleshipManager.GridHeight);
                    }
                    else
                    {
                        x = UnityEngine.Random.Range(0, BattleshipManager.GridWidth);
                        y = UnityEngine.Random.Range(0, BattleshipManager.GridHeight - ship.shipLength);
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

                if (attempt >= 20)
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
        
        // Start the bot's turn. TODO play animations, etc...
        public void TakeCPUTurn()
        {
            StartCoroutine(botWaits(2));
        }

        IEnumerator botWaits(float sec)
        {
            yield return new WaitForSeconds(sec);
            ShootAtPlayer();

            BattleshipManager.Instance.EndTurn();
        }

        // Shoot at the player. Swap out the algorithm for determining where to shoot, if you like
        public void ShootAtPlayer()
        {
            Vector2Int whereToShoot = decisionMatrix.GetSmartRandomTile(0.95f);
            Debug.Log("CPU shooting at " + whereToShoot.x + "," + whereToShoot.y);
            
            PlayerTile shootAtThis = GridManager.Instance.GetPlayerTileFromPosition(whereToShoot.x, whereToShoot.y);
            (bool hit, bool standing) shootResult = shootAtThis.ShootThisTile();

            decisionMatrix.ShootTile(Vector2Int.FloorToInt(shootAtThis.coordinates), shootResult.hit);

            // If we sunk the ship, need to recalculate the entire hit matrix.
            if (!shootResult.standing)
            {
                Ship sunk = enemyShipsRemaining.Find(ship => ship.shipType == shootAtThis.shipPresent.shipType);
                decisionMatrix.SinkShip(sunk, enemyShipsRemaining);
            }

            decisionMatrix.RedrawDebugViz();
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
        private class DecisionMatrixTile : IComparable
        {
            public Vector2Int coordinates;
            public int score; // this is what the decision matrix primarily uses
            public AITileStatus status;

            public DecisionMatrixTile(int w, int h)
            {
                coordinates = new Vector2Int(w, h);

                status = AITileStatus.OPEN;
            }

            public void recalc()
            {
                score = 0;
            }

            int IComparable.CompareTo(object obj)
            {
                if (obj is DecisionMatrixTile)
                {
                    return (obj as DecisionMatrixTile).score - score;
                }
                else return 0;
            }
        }

        private class HitMatrixTile : DecisionMatrixTile
        {
            public Ship shipPresent;

            public HitMatrixTile(int w, int h) : base(w, h) { }
        }


        // Scores matrix - primary method of tracking which tiles to shoot at.
        private DecisionMatrixTile[,] decisionMatrix;
        // Valid target list - fast way of seeing what tiles we can shoot at (not a set so we can get randoms from it easier)
        private List<DecisionMatrixTile> validTargetSet;
        // Hit matrix - 
        private Dictionary<BattleshipShipType, List<HitMatrixTile>> shipsToTiles;
        private List<HitMatrixTile> nextHitCandidates;
        private HitMatrixTile[,] hitMatrix;
        // TODO - possible locations of each ship
        // The last successful shot, if any
        private Vector2Int lastSuccessfulShot;
        private bool checkingRelatedShot = false;

        // SETUP
        public BattleshipAI_DecisionMatrix()
        {
            decisionMatrix = new DecisionMatrixTile[BattleshipManager.GridWidth, BattleshipManager.GridHeight];
            hitMatrix = new HitMatrixTile[BattleshipManager.GridWidth, BattleshipManager.GridHeight];
            validTargetSet = new List<DecisionMatrixTile>();
            nextHitCandidates = new List<HitMatrixTile>();
            shipsToTiles = new Dictionary<BattleshipShipType, List<HitMatrixTile>>(); // filled out on first hit

            List<Ship> allShips = BattleshipManager.Instance.GetShips();
            for(int w = 0; w < BattleshipManager.GridWidth; w++)
            {
                for (int h = 0; h < BattleshipManager.GridHeight; h++)
                {
                    decisionMatrix[w, h] = new DecisionMatrixTile(w, h);
                    hitMatrix[w, h] = new HitMatrixTile(w, h);
                }
            }
        }


        /// ----------------------------------
        ///            EXTRACTION
        /// ----------------------------------

        /// <summary>
        /// Get a completely random tile with no regard for scores or logic
        /// </summary>
        public Vector2Int GetTotallyRandomTile()
        {
            DecisionMatrixTile chosen = validTargetSet[UnityEngine.Random.Range(0, validTargetSet.Count)];
            validTargetSet.Remove(chosen);
            return chosen.coordinates;
        }

        /// <summary>
        /// Get a tile with some logic in mind. "Factor" is from 0-1, the higher it is, the smarter the decision will generally be
        /// </summary>
        public Vector2Int GetSmartRandomTile(float factor)
        {

            // Explicitly pursue hits, or just go with decision matrix?
            // Multiply factor by 2: Difficulty .5 or higher will always pursue hits
            if (nextHitCandidates.Count > 0)
            {
                Shuffle(nextHitCandidates);
                nextHitCandidates.Sort();

                HitMatrixTile chosen = nextHitCandidates[UnityEngine.Random.Range(0, Mathf.CeilToInt(nextHitCandidates.Count * (1 - factor)))];
                validTargetSet.Remove(chosen);
                nextHitCandidates.Remove(chosen);
                validTargetSet.Remove(decisionMatrix[chosen.coordinates.x, chosen.coordinates.y]);
                Debug.Log("Shooting at " + chosen.coordinates + " on basis of NEARBY HIT");
                return chosen.coordinates;
            } 
            else
            {
                Shuffle(validTargetSet);
                validTargetSet.Sort();

                DecisionMatrixTile chosen = validTargetSet[UnityEngine.Random.Range(0, Mathf.CeilToInt(validTargetSet.Count * (1 - factor)))];
                validTargetSet.Remove(chosen);
                nextHitCandidates.Remove(hitMatrix[chosen.coordinates.x, chosen.coordinates.y]);

                Debug.Log("Shooting at " + chosen.coordinates + " on basis of DECISION MATRIX");
                return chosen.coordinates;
            }
        }

        // From https://stackoverflow.com/questions/273313/randomize-a-listt
        public void Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = n;
                while(k == n) k = UnityEngine.Random.Range(0, list.Count - 1); // Edge case k == n
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// ----------------------------------
        ///           RECALCULATION
        /// ----------------------------------

        /// <summary>
        /// Don't have to recalculate the entire grid when shooting a single tile - just recalculate enough surrounding area
        /// </summary>
        public void ShootTile(Vector2Int coords, bool hit)
        {
            decisionMatrix[coords.x, coords.y].status = hit ? AITileStatus.HIT : AITileStatus.MISS;
            hitMatrix[coords.x, coords.y].status = hit ? AITileStatus.HIT : AITileStatus.MISS;

            List<Ship> ships = BattleshipManager.Instance.GetShips();
            int longestShipLength = 0;
            foreach (Ship ship in ships) 
                if (ship.shipLength > longestShipLength) 
                    longestShipLength = ship.shipLength;

            // Only need to reclalculate as far as the longest ship in the same row and column (either direction).
            for (int w = Mathf.Max(0, coords.x - (longestShipLength - 1)); w < Mathf.Min(BattleshipManager.GridWidth, coords.x + longestShipLength); w++)
            {
                RecalculateTile(w, coords.y, ships);
                RecalculateHitTile(w, coords.y, ships);
            }
            for (int h = Mathf.Max(0, coords.y - (longestShipLength - 1)); h < Mathf.Min(BattleshipManager.GridHeight, coords.y + longestShipLength); h++)
            {
                RecalculateTile(coords.x, h, ships);
                RecalculateHitTile(coords.x, h, ships);
            }

            // Update hit matrix + shipsToTiles if necessary
            if (hit)
            {
                PlayerTile pt = GridManager.Instance.GetPlayerTileFromPosition(coords.x, coords.y);
                if (!shipsToTiles.ContainsKey(pt.shipPresent.shipType)) {
                    shipsToTiles[pt.shipPresent.shipType] = new List<HitMatrixTile>();
                    pt.shipPresent.occupiedTiles.ForEach(t =>
                    {
                        shipsToTiles[pt.shipPresent.shipType].Add(hitMatrix[(int)t.coordinates.x, (int)t.coordinates.y]);
                    });
                }
                // Have we had a previous hit? If so, is it similar to this current one?
                if (lastSuccessfulShot != null)
                {
                    checkingRelatedShot = true;
                    RecalculateSimilarHitCandidates(lastSuccessfulShot, coords);
                }
                lastSuccessfulShot = coords;

            }
            else if(checkingRelatedShot)
            {
                // We were checking related shots, but this one was a miss.
                // However, we KNOW there are still more hits on this ship, so lets
                // try and check the other one more time.
                RecalculateSimilarHitCandidates(lastSuccessfulShot, coords);
                // No matter what, we are done checking related shots after this.
                checkingRelatedShot = false;
            }
            else
            {
                RecalculateNextHitCandidates();
            }
                
        }

        // Sink an enemy ship. You have to ensure you're no longer caring about it on the hit matrix.
        public void SinkShip(Ship sunk, List<Ship> remaining)
        {
            shipsToTiles[sunk.shipType].ForEach(t => t.status = AITileStatus.SUNK);
            RecalculateEntireGrid(remaining);
            checkingRelatedShot = false;
        }

        /// <summary>
        /// Recalculate everything - expensive! avoid when possible
        /// </summary>
        public void RecalculateEntireGrid(List<Ship> enemyShipsRemaining)
        {
            // For each tile
            for (int w = 0; w < BattleshipManager.GridWidth; w++)
            {
                for (int h = 0; h < BattleshipManager.GridHeight; h++)
                {
                    RecalculateTile(w, h, enemyShipsRemaining);
                    RecalculateHitTile(w, h, enemyShipsRemaining);
                    if(decisionMatrix[w,h].status == AITileStatus.OPEN) validTargetSet.Add(decisionMatrix[w, h]);
                }
            }
        }

        public void RecalculateNextHitCandidates()
        {
            nextHitCandidates = new List<HitMatrixTile>();

            for (int w = 0; w < BattleshipManager.GridWidth; w++)
            {
                for (int h = 0; h < BattleshipManager.GridHeight; h++)
                {
                    if (hitMatrix[w, h].score > 0 && hitMatrix[w, h].status == AITileStatus.OPEN && !nextHitCandidates.Contains(hitMatrix[w, h]))
                        nextHitCandidates.Add(hitMatrix[w, h]);
                }
            }
        }

        //Driver to recalculate tiles in the same row or column of previous hits. 
        // If the AI has struck 2 tiles in the same row/col, chances are that row/col has more valid targets.
        public void RecalculateSimilarHitCandidates(Vector2Int previousHit, Vector2Int coordinatesOfHit)
        {
            nextHitCandidates = new List<HitMatrixTile>();
            if (previousHit.x != coordinatesOfHit.x && previousHit.y != coordinatesOfHit.y)
            {
                // Not in the same row or column, so we can't use this information.
                RecalculateNextHitCandidates();
                return;
            }
            else
            {
                if(previousHit.x == coordinatesOfHit.x)
                {
                    // Same column, so we want to look for other hits in this column
                    for(int h = 0; h < BattleshipManager.GridHeight; h++)
                    {
                        if(hitMatrix[previousHit.x, h].status == AITileStatus.OPEN && hitMatrix[previousHit.x, h].score > 0 && !nextHitCandidates.Contains(hitMatrix[previousHit.x, h]))
                        {
                            nextHitCandidates.Add(hitMatrix[previousHit.x, h]);
                        }
                    }
                } 
                else
                {
                    // Same row, so we want to look for other hits in this row
                    for (int w = 0; w < BattleshipManager.GridWidth; w++)
                    {
                        if (hitMatrix[w, previousHit.y].status == AITileStatus.OPEN && hitMatrix[w, previousHit.y].score > 0 && !nextHitCandidates.Contains(hitMatrix[w, previousHit.y]))
                        {
                            nextHitCandidates.Add(hitMatrix[w, previousHit.y]);
                        }
                    }
                }
            }
            //Failover if there are no valid candidates found in the same row/column as previous hits.
            if (nextHitCandidates.Count == 0)
            {
                RecalculateNextHitCandidates();
            }
        }

        /// <summary>
        /// Get the current decision matrix as a 2D int array of just the scores
        /// </summary>
        private int[,] ExtractScores(DecisionMatrixTile[,] decisionMatrix)
        {
            int[,] scoreArr = new int[BattleshipManager.GridWidth, BattleshipManager.GridHeight];

            for (int w = 0; w < BattleshipManager.GridWidth; w++)
            {
                for (int h = 0; h < BattleshipManager.GridHeight; h++)
                {
                    if (decisionMatrix[w, h].status == AITileStatus.HIT) scoreArr[w, h] = -1;
                    else if (decisionMatrix[w, h].status == AITileStatus.MISS) scoreArr[w, h] = -2;
                    else if (decisionMatrix[w, h].status == AITileStatus.SUNK) scoreArr[w, h] = -3;
                    else scoreArr[w, h] = decisionMatrix[w, h].score;
                }
            }

            return scoreArr;
        }

        /// <summary>
        /// Tell the debug drawer, if it exists, to redraw decision and hit matrices
        /// </summary>
        public void RedrawDebugViz()
        {
            if(DecisionMatrixDebugUI.instance != null)
            {
                DecisionMatrixDebugUI.instance.redrawDecisionGrid(ExtractScores(decisionMatrix));
                DecisionMatrixDebugUI.instance.redrawHitGrid(ExtractScores(hitMatrix));
            }
        }

        private void RecalculateTile(int w, int h, List<Ship> ships)
        {
            if (decisionMatrix[w, h].status == AITileStatus.OPEN)
            {
                decisionMatrix[w, h].recalc();
                for (int s = 0; s < ships.Count; s++)
                {
                    Ship ship = ships[s];

                    DecisionMatrixTile tile = decisionMatrix[w, h];
                    // (ignore if already revealed)
                
                    // For each horizontal arrangement of the ship
                    for (int lh = 0; lh < ship.shipLength; lh++)
                    {
                        // Determine if the arrangement is valid
                        bool workingArrangement = true;
                        for (int b = 0; b < lh; b++)
                        {
                            if (w - lh + b < 0 || decisionMatrix[w - lh + b, h].status == AITileStatus.MISS)
                            {
                                workingArrangement = false;
                                break;
                            }
                        }
                        for (int a = lh; a < ship.shipLength && workingArrangement; a++)
                        {
                            if (w + a - lh >= BattleshipManager.GridWidth || decisionMatrix[w + a - lh, h].status == AITileStatus.MISS)
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
                            if (h - lv + b < 0 || decisionMatrix[w, h - lv + b].status == AITileStatus.MISS)
                            {
                                workingArrangement = false;
                                break;
                            }
                        }
                        for (int a = lv; a < ship.shipLength && workingArrangement; a++)
                        {
                            if (h + a - lv >= BattleshipManager.GridHeight || decisionMatrix[w, h + a - lv].status == AITileStatus.MISS)
                            {
                                workingArrangement = false;
                                break;
                            }
                        }
                        // If valid, add a point to the score
                        if (workingArrangement) tile.score += 1;
                    }
                }
            }
        }


        /// <summary>
        /// Functionally similar to recalculateTile, but it works for hit matrices and specifically looks for hit tiles
        /// </summary>
        private void RecalculateHitTile(int w, int h, List<Ship> ships)
        {
            //Debug.Log("Recalc tile " + w + ", " + h);
            if (hitMatrix[w, h].status == AITileStatus.OPEN)
            {
                hitMatrix[w, h].recalc();
                for (int s = 0; s < ships.Count; s++)
                {
                    Ship ship = ships[s];

                    HitMatrixTile tile = hitMatrix[w, h];
                    // (ignore if already revealed)

                    // For each horizontal arrangement of the ship
                    for (int lh = 0; lh < ship.shipLength; lh++)
                    {
                        // Determine if the arrangement is valid - must contain a hit tile at some point
                        bool workingArrangement = false;
                        for (int b = 0; b < lh; b++)
                        {
                            if (w - lh + b < 0 || hitMatrix[w - lh + b, h].status == AITileStatus.MISS)
                            {
                                workingArrangement = false;
                                goto endSearch;
                            } else if(hitMatrix[w - lh + b, h].status == AITileStatus.HIT)
                            {
                                workingArrangement = true;
                            }
                        }
                        for (int a = lh; a < ship.shipLength; a++)
                        {
                            if (w + a - lh >= BattleshipManager.GridWidth || hitMatrix[w + a - lh, h].status == AITileStatus.MISS)
                            {
                                workingArrangement = false;
                                goto endSearch;
                            } else if(hitMatrix[w + a - lh, h].status == AITileStatus.HIT)
                            {
                                workingArrangement = true;
                            }
                        }
                        // If valid, add a point to the score
                        endSearch:
                        if (workingArrangement) tile.score += 1;
                    }


                    // For each vertical arrangement of the ship
                    for (int lv = 0; lv < ship.shipLength; lv++)
                    {
                        // Determine if the arrangement is valid
                        bool workingArrangement = false;
                        for (int b = 0; b < lv; b++)
                        {
                            if (h - lv + b < 0 || hitMatrix[w, h - lv + b].status == AITileStatus.MISS)
                            {
                                workingArrangement = false;
                                goto endSearch;
                            } else if(hitMatrix[w, h - lv + b].status == AITileStatus.HIT)
                            {
                                workingArrangement = true;
                            }
                        }
                        for (int a = lv; a < ship.shipLength; a++)
                        {
                            if (h + a - lv >= BattleshipManager.GridHeight || hitMatrix[w, h + a - lv].status == AITileStatus.MISS)
                            {
                                workingArrangement = false;
                                goto endSearch;
                            } else if (hitMatrix[w, h + a - lv].status == AITileStatus.HIT)
                            {
                                workingArrangement = true;
                            }
                        }
                        // If valid, add a point to the score
                        endSearch:
                        if (workingArrangement) tile.score += 1;
                    }

                    ///Debug.Log("Final score assoc. with tile " + w + "," + h + ":" + hitMatrix[w, h].score);
                }
            }
        }
    }
}

public enum AITileStatus
{
    OPEN, // unrevealed + unknown
    MISS, // revealed to have no ship
    HIT,  // revealed to have a ship, which is still active
    SUNK  // has a ship which has been sunk (treated in decision making like a miss)
}