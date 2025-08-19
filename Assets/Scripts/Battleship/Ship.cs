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
        public BattleshipShipType shipType;
        public List<PlayerTile> occupiedTiles; // List of the tiles this ship currently occupies.

        public Ship(BattleshipShipType type)
        {
            Initialize(type);
        }

        public void Initialize(BattleshipShipType type)
        {
            occupiedTiles = new List<PlayerTile>();
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

        public void PlaceShip(List<PlayerTile> tiles)
        {
            occupiedTiles = tiles;
        }

        public int GetShipLength()
        {
            return shipLength;
        }

        public BattleshipShipType GetShipType()
        {
            return shipType;
        }
    }
}