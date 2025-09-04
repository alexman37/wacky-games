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