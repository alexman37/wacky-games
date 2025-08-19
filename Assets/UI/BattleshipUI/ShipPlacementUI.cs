using Games.Minesweeper;
using System.Collections.Generic;
using UnityEngine;

namespace Games.Battleship
{
    public class ShipPlacementUI : MonoBehaviour
    {
        public static ShipPlacementUI Instance;
        public GameObject shipPlacementPanel;
        public List<GameObject> battleshipPrefabs; // Prefabs for the ships
        public List<GameObject> createdShips;
        public ShipPlacementUI()
        {
            if(Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

        public void ShowShipPlacementPanel()
        {
            List<Ship> shipsToInstantiate = BattleshipManager.Instance.GetShips();
            foreach(Ship ship in shipsToInstantiate)
            {
                GameObject shipPlacement = Instantiate(battleshipPrefabs[(int)ship.GetShipType()], new Vector3(0,0,0), Quaternion.identity);
                shipPlacement.GetComponent<Ship>().Initalize(ship.GetShipType());
                shipPlacement.name = ship.GetShipType().ToString();
                Debug.Log("WTF I am a " + ship.GetShipType().ToString());
                shipPlacement.transform.parent = shipPlacementPanel.transform;
                createdShips.Add(shipPlacement);
            }
            shipPlacementPanel.GetComponent<WidgetPopup>().openWidgetPopup();
        }

    }
}