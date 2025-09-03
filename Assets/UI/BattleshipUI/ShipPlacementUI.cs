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
                shipPlacement.AddComponent<ShipUI>();
                shipPlacement.GetComponent<ShipUI>().Initialize(ship.GetShipType());
                shipPlacement.name = ship.GetShipType().ToString();
                shipPlacement.transform.SetParent(shipPlacementPanel.transform);
                createdShips.Add(shipPlacement);
            }
            shipPlacementPanel.GetComponent<WidgetPopup>().openWidgetPopup();
        }

        //It says Open Widget but it really just opens and closes it, so we can use the same one to "close" it
        public void ClosePanel()
        {
            shipPlacementPanel.GetComponent<WidgetPopup>().closeWidgetPopup();
        }

    }
}