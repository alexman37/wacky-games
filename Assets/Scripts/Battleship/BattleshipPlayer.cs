using System.Collections.Generic;
using UnityEngine;
using System;

namespace Games.Battleship
{
    public class BattleshipPlayer : MonoBehaviour
    {
        public List<BattleshipShipType> playerBattleships;
        public int totalShipValue = 0;
        public bool isPlayer1 = false; // To determine which player this is

        public static Action finishedPlacingPlayerShips;

        private void Start()
        {
            finishedPlacingPlayerShips += () => { };
        }

        public void Initialize(bool player1)
        {
            playerBattleships = new List<BattleshipShipType>();
            isPlayer1 = player1;
        }

        public void PlaceShip(List<PlayerTile> tiles, Ship ship)
        {
            // Ignore if we've already placed this ship
            if(!playerBattleships.Contains(ship.shipType))
            {
                ship.PlaceShip(tiles);
                BattleshipTopBarUI.instance.displayDebugInfo("Placed ship " + ship.shipType);

                totalShipValue++;
                playerBattleships.Add(ship.shipType);
                ShipPlacementUI.Instance.PlaceShip(ship.shipType);

                if (totalShipValue >= ShipPlacementUI.Instance.createdShips.Count)
                {
                    BattleshipTopBarUI.instance.displayDebugInfo("All ships placed... ");
                    ShipPlacementUI.Instance.ClosePanel();

                    // Now place CPU ships
                    finishedPlacingPlayerShips.Invoke();
                }
            }
        }

        public void loseShip()
        {
            totalShipValue--;
        }

        public bool AreAllShipsSunk()
        {
            return totalShipValue <= 0;
        }       
    }

    
}