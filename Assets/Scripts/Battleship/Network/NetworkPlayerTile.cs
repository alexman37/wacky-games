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
            if(shipType == BattleshipShipType.NONE)
            {
                return; // No ship selected, nothing to highlight
            }
            BattleshipRotation rotation = owningGridManager.myPlayer.shipRotation;

            // Get optimal placement position that includes the current tile
            Vector2Int optimalStartPosition = GetOptimalShipPlacement(shipType, rotation);

            // Add the tiles based on the optimal position
            int shipLength = GetShipLength(shipType);

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

        private Vector2Int GetOptimalShipPlacement(BattleshipShipType shipType, BattleshipRotation rotation)
        {
            int shipLength = GetShipLength(shipType);
            Vector2Int currentPos = new Vector2Int((int)coordinates.x, (int)coordinates.y);

            if (rotation == BattleshipRotation.HORIZONTAL)
            {
                // For horizontal placement, adjust Y coordinate to keep ship within bounds
                int startY = currentPos.y;
                int endY = currentPos.y + shipLength - 1;

                // If ship extends beyond right edge, shift it left
                if (endY >= 10) // Grid is 0-9
                {
                    startY = 10 - shipLength;
                }
                // If ship extends beyond left edge, shift it right
                else if (currentPos.y - (shipLength - 1) < 0)
                {
                    startY = 0;
                }
                // Try to center the ship around the hovered tile if possible
                else
                {
                    int idealStart = currentPos.y - (shipLength / 2);
                    startY = Mathf.Clamp(idealStart, 0, 10 - shipLength);

                    // Ensure the original hovered tile is included
                    if (currentPos.y < startY || currentPos.y > startY + shipLength - 1)
                    {
                        startY = currentPos.y;
                        if (startY + shipLength - 1 >= 10)
                        {
                            startY = 10 - shipLength;
                        }
                    }
                }

                return new Vector2Int(currentPos.x, startY);
            }
            else // VERTICAL
            {
                // For vertical placement, adjust X coordinate to keep ship within bounds
                int startX = currentPos.x;
                int endX = currentPos.x + shipLength - 1;

                // If ship extends beyond bottom edge, shift it up
                if (endX >= 10) // Grid is 0-9
                {
                    startX = 10 - shipLength;
                }
                // If ship extends beyond top edge, shift it down
                else if (currentPos.x - (shipLength - 1) < 0)
                {
                    startX = 0;
                }
                // Try to center the ship around the hovered tile if possible
                else
                {
                    int idealStart = currentPos.x - (shipLength / 2);
                    startX = Mathf.Clamp(idealStart, 0, 10 - shipLength);

                    // Ensure the original hovered tile is included
                    if (currentPos.x < startX || currentPos.x > startX + shipLength - 1)
                    {
                        startX = currentPos.x;
                        if (startX + shipLength - 1 >= 10)
                        {
                            startX = 10 - shipLength;
                        }
                    }
                }

                return new Vector2Int(startX, currentPos.y);
            }
        }

        private int GetShipLength(BattleshipShipType shipType)
        {
            switch (shipType)
            {
                case BattleshipShipType.CARRIER: return 5;
                case BattleshipShipType.BATTLESHIP: return 4;
                case BattleshipShipType.CRUISER: return 3;
                case BattleshipShipType.SUBMARINE: return 3;
                case BattleshipShipType.DESTROYER: return 2;
                default: return 1;
            }
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