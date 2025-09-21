using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Games.Battleship
{


    public class ShipViewPopup : WidgetPopup
    {
        BattleshipManager manager;
        List<Image> player1Ships = new List<Image>();
        List<Image> player2Ships = new List<Image>();

        public List<GameObject> battleshipPrefabs;

        private void OnEnable()
        {
            BattleshipManager.changedGameState += ifInShipSetupInitialize;
        }

        private void OnDisable()
        {
            BattleshipManager.changedGameState -= ifInShipSetupInitialize;
        }

        public void ifInShipSetupInitialize(string newState)
        {
            if(newState == "PlaceShipsState")
            {
                initializeContent();
            }
        }

        // Give the ship screen all its ships from both sides
        public void initializeContent()
        {
            manager = BattleshipManager.Instance;

            // TODO maybe if both players don't have the same starting loadout we have to do something different here?
            foreach (Ship ship in manager.GetShips())
            {
                GameObject shipPlacement = Instantiate(battleshipPrefabs[(int)ship.GetShipType()], new Vector3(0, 0, 0), Quaternion.identity);
                shipPlacement.name = ship.GetShipType().ToString();
                shipPlacement.transform.SetParent(this.transform);
            }
        }

        // Do this any time a ship has been sunk
        public void updateContent()
        {
            
        }
    }
}