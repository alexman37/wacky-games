using System.Collections.Generic;
using UnityEngine;

namespace Games.Battleship
{
    public class BattleshipPlayer : MonoBehaviour
    {
        public List<Tile> shipTiles = new List<Tile>(); // To represent this player's ships
        public List<Tile> checkedTiles = new List<Tile>(); // To represent which tiles this player has checked
        public List<Tile> tilesEnemyHit = new List<Tile>(); // To represent which tiles of the player have been attacked
        public List<BattleshipShipType> playerBattleships;
        public int totalShipValue = 0;
        public bool isPlayer1 = false; // To determine which player this is

        public void Initialize(List<BattleshipShipType> battleshipsToPlace, bool player1)
        {
            playerBattleships = battleshipsToPlace;
            isPlayer1 = player1;
        }

        public bool AreAllShipsSunk()
        {
            return totalShipValue <= 0;
        }       
    }

    
}