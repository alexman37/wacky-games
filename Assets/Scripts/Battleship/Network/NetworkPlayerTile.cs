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
    public class NetworkPlayerTile : BattleshipTile
    {
        public bool isChecked = false;
        public bool isShip = false;
        List<BattleshipTile> tilesToHighlight = new List<BattleshipTile>();
        BattleshipRotation currentRotation = BattleshipRotation.NONE;
        public List<Material> materialsList = new List<Material>();

        //Reference to the network grid manager which "owns" this tile
        private NetworkGridManager owningGridManager;
        public void SetOwningGridManager(NetworkGridManager gridManager)
        {
            owningGridManager = gridManager;
        }

        public void OnMouseDown()
        {
            List<NetworkPlayerTile> tilesSelected = new List<NetworkPlayerTile>();
            //Since we can assume the tiles are already highlighted, we can just use those
            if (tilesToHighlight.Count > 0)
            {
                foreach(BattleshipTile tile in tilesToHighlight)
                {
                    if(tile is NetworkPlayerTile networkPlayerTile)
                    {
                        tilesSelected.Add(networkPlayerTile);
                    }
                }
            }
            else
            {
                return; // No tiles highlighted, nothing to do
            }
            if (owningGridManager.AttemptToPlaceShip(tilesSelected))
            {
                owningGridManager.StopTransparencyChangeTiles(tilesToHighlight);
            }
            else
            {
                Debug.Log("Ship placement failed");
            }
            tilesToHighlight.Clear();
        }

        public void SetAsShip()
        {
            isShip = true;
            GetComponent<MeshRenderer>().material = materialsList[2];
        }

        // If the player has a ship selected (i.e. during the ship placement phase), we want to highlight
        // the tiles this ship would occupy if placed.
        // Otherwise, this should be highlighted to indicate that the player is hovering over it.
        // If the player changes the rotation while hovering, we need to update the highlight
        public void OnMouseOver()
        {
            if (currentRotation != owningGridManager.myPlayer.shipRotation)
            {
                if(tilesToHighlight.Count > 0)
                {
                    owningGridManager.StopTransparencyChangeTiles(tilesToHighlight);
                    tilesToHighlight.Clear();
                }
                currentRotation = owningGridManager.myPlayer.shipRotation;
                HighlightForShipPlacement();
            }
        }

        public void OnMouseExit()
        {
            owningGridManager.StopTransparencyChangeTiles(tilesToHighlight);
            // Reset the color of the tile
            foreach(NetworkPlayerTile tile in tilesToHighlight)
            {
                if (tile.isShip) //Set this to a color for now, later we won't have a check here.
                {
                    tile.GetComponent<MeshRenderer>().material = materialsList[2];
                }
                else
                {
                    tile.GetComponent<MeshRenderer>().material = materialsList[0];
                }
            }
            tilesToHighlight.Clear();
            currentRotation = BattleshipRotation.NONE;
        }        

        public void HighlightForShipPlacement()
        {
            tilesToHighlight.Clear();

            BattleshipShipType shipType = owningGridManager.myPlayer.selectedShipType;
            Ship ship = owningGridManager.myPlayer.shipToPlace;
            if(shipType == BattleshipShipType.NONE)
            {
                return; // No ship selected, nothing to highlight
            }
            BattleshipRotation rotation = owningGridManager.myPlayer.shipRotation;

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

                NetworkPlayerTile tile = GetTileAtPosition(tilePosition);
                if (tile != null)
                {
                    if (tile.isShip) // We need to change the material to red or something here to indicate there is a ship here
                    {
                        tile.GetComponent<MeshRenderer>().material = materialsList[1];
                    }
                    tilesToHighlight.Add(tile);
                }
            }

            owningGridManager.StartTransparencyChange(tilesToHighlight, 2f);
        }

        private NetworkPlayerTile GetTileAtPosition(Vector2Int position)
        {
            if (owningGridManager.MyShipTiles.TryGetValue(position, out GameObject tileObj))
            {
                return tileObj.GetComponent<NetworkPlayerTile>();
            }
            return null;
        }

        // This is called when the opponent attacks this tile
        public void MarkAsAttacked()
        {
            isChecked = true;
            GetComponent<MeshRenderer>().material = materialsList[1];
            // You can add visual feedback here like changing material or showing hit marker
            Debug.Log($"Player tile at {coordinates} was attacked! Hit: {isShip}");
        }

        //Called from the Ship class when it is sunk
        public void MarkAsSunk()
        {

        }
    }
}