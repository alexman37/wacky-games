using System.Collections.Generic;
using UnityEngine;

namespace Games.Battleship
{
    // The different variations of battleship you can play
    // These effect what ships are on the board (TODO: and rules for shooting?)
    public static class BattleshipGameModes
    {
        public static List<BattleshipShipType> GetShipTypes(BattleshipGameMode gameMode)
        {
            switch (gameMode)
            {
                // CLASSIC: 5 ships of a certain length
                case BattleshipGameMode.CLASSIC:
                    return new List<BattleshipShipType>
                    {

                        BattleshipShipType.CARRIER,
                        BattleshipShipType.BATTLESHIP,
                        BattleshipShipType.CRUISER,
                        BattleshipShipType.SUBMARINE,
                        BattleshipShipType.DESTROYER
                    };
                // HUNTER (debugging): 1 single ship
                case BattleshipGameMode.HUNTER:
                    return new List<BattleshipShipType>
                    {
                        BattleshipShipType.CRUISER
                    };
                default:
                    return new List<BattleshipShipType>();
            }
        }
    }
}