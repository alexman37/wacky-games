using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Games.Battleship
{
    /// <summary>
    /// Tiles the represent the primary grid for a given player. This is the traditional "bottom" grid
    /// in the game, which is used to represent where the player's ships are placed, as well as where
    /// the opponent has attacked or successfully hit a ship.
    /// ShotTile.cs is the equivalent for the "top" grid, which is used to represent where the player has attacked
    /// </summary>
    public class PlayerTile : BattleshipTile
    {
        private static List<BattleshipTile> tilesToHighlight = new List<BattleshipTile>();

        public void OnMouseDown()
        {
            Debug.Log($"Slishhhh " + coordinates);
            
            if(BattleshipManager.Instance.currentTurn == BattleshipTurn.SHIP_SETUP)
            {
                List<PlayerTile> tilesSelected = new List<PlayerTile>();

                if (BattleshipManager.Instance.selectedShipType != BattleshipShipType.NONE && tilesToHighlight.Count > 0)
                {
                    foreach (BattleshipTile tile in tilesToHighlight)
                    {
                        if (tile is PlayerTile playerTile)
                        {
                            tilesSelected.Add(playerTile);
                        }
                    }
                }
                else
                {
                    return; // No tiles highlighted, nothing to do
                }
                if (GridManager.Instance.AttemptToPlaceShip(tilesSelected, BattleshipManager.Instance.selectedShip))
                {
                    GridManager.Instance.StopTransparencyChangeTiles(tilesToHighlight);
                }
                else
                {
                    Debug.Log("Ship placement failed");
                    GridManager.Instance.StopTransparencyChangeTiles(tilesToHighlight);
                }
                tilesToHighlight.Clear();
            }
            
        }

        // If the player has a ship selected (i.e. during the ship placement phase), we want to highlight
        // the tiles this ship would occupy if placed.
        // Otherwise, this should be highlighted to indicate that the player is hovering over it.
        public void OnMouseEnter()
        {
            switch (BattleshipManager.Instance.GetCurrentTurn())
            {
                case BattleshipTurn.PLAYER1:
                case BattleshipTurn.PLAYER2:
                    // Maybe have some flavor text about the ship? idk
                    break;
                case BattleshipTurn.SHIP_SETUP:
                    HighlightForShipPlacement();
                    break;
                case BattleshipTurn.GAME_OVER:
                case BattleshipTurn.NONE:
                default:
                    break;
            }
        }

        public void OnMouseExit()
        {
            GridManager.Instance.StopTransparencyChangeTiles(tilesToHighlight);
            tilesToHighlight.Clear();
        }

        public void SetAsShip(Ship ship)
        {
            hasShip = true;
            shipPresent = ship;
            GetComponent<MeshRenderer>().material.color = Color.green;
        }

        public void HighlightForShipPlacement()
        {
            tilesToHighlight.Clear();

            BattleshipShipType shipType = BattleshipManager.Instance.selectedShipType;
            Ship ship = BattleshipManager.Instance.selectedShip;
            if (shipType == BattleshipShipType.NONE)
            {
                return; // No ship selected, nothing to highlight
            }
            BattleshipRotation rotation = BattleshipManager.Instance.shipRotation;

            // Get optimal placement position that includes the current tile
            Vector2Int optimalStartPosition = ship.GetOptimalShipPlacement(this, rotation);

            // Add the tiles based on the optimal position
            int shipLength = ship.GetShipLength(shipType);

            for (int i = 0; i < shipLength; i++)
            {
                Vector2Int tilePosition;
                if (rotation == BattleshipRotation.HORIZONTAL)
                {
                    tilePosition = new Vector2Int(optimalStartPosition.x, optimalStartPosition.y + i);
                }
                else
                {
                    tilePosition = new Vector2Int(optimalStartPosition.x + i, optimalStartPosition.y);
                }

                PlayerTile tile = GetTileAtPosition(tilePosition);
                if (tile != null)
                {
                    if (tile.hasShip) // We need to change the material to red or something here to indicate there is a ship here
                    {
                        // TODO - reenable this later but be smart about "unmarking" tiles
                        //tile.GetComponent<MeshRenderer>().material.color = Color.red;
                    }
                    tilesToHighlight.Add(tile);
                }
            }

            GridManager.Instance.StartTransparencyChange(tilesToHighlight, 2f);
        }

        // Called by the CPU player when taking their turn
        public (bool, bool) ShootThisTile()
        {
            tileChecked = true;
            bool stillStanding = true;

            MeshRenderer hitMarkerRenderer = transform.GetChild(0).GetComponent<MeshRenderer>();

            if (hasShip)
            {
                //hitMarkerRenderer.material = hitMaterial; // Change the material to indicate a hit
                stillStanding = shipPresent.HitShipSegment(this);
                if (!stillStanding) BattleshipManager.Instance.player2Component.loseShip();
            }
            else
            {
                //hitMarkerRenderer.material = missMaterial; // Change the material to indicate a miss
            }
            hitMarkerRenderer.enabled = true; // Show the hit marker

            return (hasShip, stillStanding);
        }

        private PlayerTile GetTileAtPosition(Vector2Int position)
        {
            if (GridManager.Instance.Player1Grid.TryGetValue(position, out GameObject tileObj))
            {
                return tileObj.GetComponent<PlayerTile>();
            }
            return null;
        }

    }
}