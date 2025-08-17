using System.Collections.Generic;
using UnityEngine;

namespace Games.Battleship
{
    public static class BattleshipGameModes
    {
        public static List<BattleshipShipType> GetShipTypes(BattleshipGameMode gameMode)
        {
            switch (gameMode)
            {
                case BattleshipGameMode.CLASSIC:
                    return new List<BattleshipShipType>
                    {

                        BattleshipShipType.CARRIER,
                        BattleshipShipType.BATTLESHIP,
                        BattleshipShipType.CRUISER,
                        BattleshipShipType.SUBMARINE,
                        BattleshipShipType.DESTROYER,
                    };
                default:
                    return new List<BattleshipShipType>();
            }
        }
    }
}