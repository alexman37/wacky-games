using System.Collections.Generic;
using UnityEngine;

namespace Games.Battleship
{
    /// <summary>
    /// The data model for ships in battleship. Represents the game logic of ships.
    /// </summary>
    public class Ship
    {
        public int shipLength;
        public bool isSunk = false;
        public bool isPlaced = false;
        public BattleshipShipType shipType;
        public List<BattleshipTile> occupiedTiles; // List of the tiles this ship currently occupies.

        public Ship(BattleshipShipType type)
        {
            Initialize(type);
        }

        /// When first created we only set the ship type and length, which are always constant
        public void Initialize(BattleshipShipType type)
        {
            occupiedTiles = new List<BattleshipTile>();
            shipType = type;

            switch (shipType)
            {
                case BattleshipShipType.CARRIER:
                    shipLength = 5;
                    break;
                case BattleshipShipType.BATTLESHIP:
                    shipLength = 4;
                    break;
                case BattleshipShipType.CRUISER:
                    shipLength = 3;
                    break;
                case BattleshipShipType.SUBMARINE:
                    shipLength = 3;
                    break;
                case BattleshipShipType.DESTROYER:
                    shipLength = 2;
                    break;
            }
        }

        /// <summary>
        /// [Multiplayer only] Designate a ship as being on all specified tiles (will be sunk if they're all hit)
        /// </summary>
        public void PlaceShip(IEnumerable<NetworkPlayerTile> tiles)
        {
            occupiedTiles = new List<BattleshipTile>();
            occupiedTiles.AddRange(tiles);
            foreach(NetworkPlayerTile tile in tiles)
            {
                tile.SetAsShip();
            }
            isPlaced = true;
        }

        /// <summary>
        /// [Singeplayer only] Designate a ship as being on all specified tiles (will be sunk if they're all hit)
        /// </summary>
        public void PlaceShip(IEnumerable<PlayerTile> tiles)
        {
            occupiedTiles = new List<BattleshipTile>();
            occupiedTiles.AddRange(tiles);
            foreach (PlayerTile tile in tiles)
            {
                tile.SetAsShip();
            }
            isPlaced = true;
        }


        public int GetShipLength(BattleshipShipType shipType)
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

        /// <summary>
        /// Get ship placement where the ship either "starts" at the hovered tile, or, 
        /// </summary>
        public Vector2Int GetOptimalShipPlacement(BattleshipTile tile, BattleshipRotation rotation)
        {
            int shipLength = GetShipLength(shipType);
            Vector2Int currentPos = new Vector2Int((int)tile.coordinates.x, (int)tile.coordinates.y);

            if (rotation == BattleshipRotation.HORIZONTAL)
            {
                // For horizontal placement, adjust Y coordinate to keep ship within bounds
                int startY = currentPos.y;
                int endY = currentPos.y + shipLength - 1;

                // If ship extends beyond right edge, shift it left
                // TODO configure for any sized grid
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


        public void HitShipSegment(NetworkPlayerTile tile)
        {
            if (occupiedTiles.Contains(tile))
            {
                tile.MarkAsAttacked();                
            }
            int hitCount = GetShipLength();
            foreach (NetworkPlayerTile t in occupiedTiles)
            {
                if (t.isChecked) hitCount--;
            }
            if(hitCount <= 0)
            {
                SinkShip();
            }
        }

        public int GetShipLength()
        {
            return shipLength;
        }

        public BattleshipShipType GetShipType()
        {
            return shipType;
        }

        public void SinkShip()
        {
            isSunk = true;
            foreach(NetworkPlayerTile tile in occupiedTiles)
            {
                tile.MarkAsSunk();
            }
        }
    }
}