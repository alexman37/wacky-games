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
        public bool isChecked = false;
        public bool isShip = false;
        private static List<BattleshipTile> tilesToHighlight = new List<BattleshipTile>();

        public void OnMouseDown()
        {
            Debug.Log($"Slishhhh " + coordinates);
            List<PlayerTile> tilesSelected = new List<PlayerTile>();
            //Since we can assume the tiles are already highlighted, we can just use those
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
            if (GridManager.Instance.AttemptToPlaceShip(tilesSelected))
            {
                GridManager.Instance.StopTransparencyChangeTiles(tilesToHighlight);
            }
            else
            {
                Debug.Log("Ship placement failed");
            }
            tilesToHighlight.Clear();
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

        public void SetAsShip()
        {
            isShip = true;
            GetComponent<MeshRenderer>().material.color = Color.green;
        }

        public void HighlightForShipPlacement()
        {
            tilesToHighlight.Clear();
            tilesToHighlight.Add(this);
            BattleshipShipType shipType = BattleshipManager.Instance.selectedShipType;
            BattleshipRotation rotation = BattleshipManager.Instance.shipRotation;
            switch (shipType)
            {
                case BattleshipShipType.CARRIER:
                    if(rotation == BattleshipRotation.HORIZONTAL)
                    {
                        tilesToHighlight.Add(GridManager.Instance.GetPlayerTileFromPosition((int)coordinates.x, (int)coordinates.y - 1));
                        tilesToHighlight.Add(GridManager.Instance.GetPlayerTileFromPosition((int)coordinates.x, (int)coordinates.y - 2));
                        tilesToHighlight.Add(GridManager.Instance.GetPlayerTileFromPosition((int)coordinates.x, (int)coordinates.y + 1));
                        tilesToHighlight.Add(GridManager.Instance.GetPlayerTileFromPosition((int)coordinates.x, (int)coordinates.y + 2));
                    }
                    else
                    {
                        tilesToHighlight.Add(GridManager.Instance.GetPlayerTileFromPosition((int)coordinates.x - 1, (int)coordinates.y));
                        tilesToHighlight.Add(GridManager.Instance.GetPlayerTileFromPosition((int)coordinates.x - 2, (int)coordinates.y));
                        tilesToHighlight.Add(GridManager.Instance.GetPlayerTileFromPosition((int)coordinates.x + 1, (int)coordinates.y));
                        tilesToHighlight.Add(GridManager.Instance.GetPlayerTileFromPosition((int)coordinates.x + 2, (int)coordinates.y));
                    }
                    break;
                case BattleshipShipType.BATTLESHIP:
                    if (rotation == BattleshipRotation.HORIZONTAL)
                    {
                        tilesToHighlight.Add(GridManager.Instance.GetPlayerTileFromPosition((int)coordinates.x, (int)coordinates.y - 1));
                        tilesToHighlight.Add(GridManager.Instance.GetPlayerTileFromPosition((int)coordinates.x, (int)coordinates.y + 1));
                        tilesToHighlight.Add(GridManager.Instance.GetPlayerTileFromPosition((int)coordinates.x, (int)coordinates.y + 2));
                    }
                    else
                    {
                        tilesToHighlight.Add(GridManager.Instance.GetPlayerTileFromPosition((int)coordinates.x - 1, (int)coordinates.y));
                        tilesToHighlight.Add(GridManager.Instance.GetPlayerTileFromPosition((int)coordinates.x + 1, (int)coordinates.y));
                        tilesToHighlight.Add(GridManager.Instance.GetPlayerTileFromPosition((int)coordinates.x + 2, (int)coordinates.y));
                    }
                    break;
                case BattleshipShipType.CRUISER:
                    if (rotation == BattleshipRotation.HORIZONTAL)
                    {
                        tilesToHighlight.Add(GridManager.Instance.GetPlayerTileFromPosition((int)coordinates.x, (int)coordinates.y - 1));
                        tilesToHighlight.Add(GridManager.Instance.GetPlayerTileFromPosition((int)coordinates.x, (int)coordinates.y + 1));
                    }
                    else
                    {
                        tilesToHighlight.Add(GridManager.Instance.GetPlayerTileFromPosition((int)coordinates.x - 1, (int)coordinates.y));
                        tilesToHighlight.Add(GridManager.Instance.GetPlayerTileFromPosition((int)coordinates.x + 1, (int)coordinates.y));
                    }
                    break;
                case BattleshipShipType.SUBMARINE:
                    if (rotation == BattleshipRotation.HORIZONTAL)
                    {
                        tilesToHighlight.Add(GridManager.Instance.GetPlayerTileFromPosition((int)coordinates.x, (int)coordinates.y - 1));
                        tilesToHighlight.Add(GridManager.Instance.GetPlayerTileFromPosition((int)coordinates.x, (int)coordinates.y + 1));
                    }
                    else
                    {
                        tilesToHighlight.Add(GridManager.Instance.GetPlayerTileFromPosition((int)coordinates.x - 1, (int)coordinates.y));
                        tilesToHighlight.Add(GridManager.Instance.GetPlayerTileFromPosition((int)coordinates.x + 1, (int)coordinates.y));
                    }
                    break;
                case BattleshipShipType.DESTROYER:
                    if (rotation == BattleshipRotation.HORIZONTAL)
                    {
                        tilesToHighlight.Add(GridManager.Instance.GetPlayerTileFromPosition((int)coordinates.x, (int)coordinates.y + 1));
                    }
                    else
                    {
                        tilesToHighlight.Add(GridManager.Instance.GetPlayerTileFromPosition((int)coordinates.x + 1, (int)coordinates.y));
                    }
                    break;
            }
            GridManager.Instance.StartTransparencyChange(tilesToHighlight, 2f);
        }
        
        

    }
}