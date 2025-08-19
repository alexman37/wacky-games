using System.Collections.Generic;
using UnityEngine;

namespace Games.Battleship
{
    public class BattleshipPlayer : MonoBehaviour
    {
        public List<PlayerTile> shipTiles = new List<PlayerTile>(); // To represent this player's ships
        public List<PlayerTile> checkedTiles = new List<PlayerTile>(); // To represent which tiles this player has checked
        public List<PlayerTile> tilesEnemyHit = new List<PlayerTile>(); // To represent which tiles of the player have been attacked
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