using Games.Minesweeper;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Rendering;

namespace Games.Battleship
{
    public class NetworkShipPlacementUI : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject shipPlacementPanel;
        public List<GameObject> battleshipPrefabs; // Prefabs for the ships
        public List<GameObject> createdShips;


        [Header("Runtime Data")]
        public List<Ship> shipsToInstantiate;
        NetworkPlayer localPlayer;

        public void GetLocalPlayer(NetworkPlayer player)
        {
            Debug.Log($"[NetworkShipPlacementUI] GetLocalPlayer called with player: {player?.PlayerName}");
            localPlayer = player;           
        }

        public void ShipsToPlace(List<BattleshipShipType> shipTypes)
        {
            Debug.Log($"[NetworkShipPlacementUI] ShipsToPlace called with {shipTypes?.Count} ship types");
            shipsToInstantiate = new List<Ship>();
            foreach (BattleshipShipType shipType in shipTypes)
            {
                Ship newShip = new Ship(shipType);
                shipsToInstantiate.Add(newShip);
                Debug.Log($"[NetworkShipPlacementUI] Created ship: {shipType}");
            }
        }

        // We need to also call this whenever the player places a ship so we can refresh the list
        public void ShowShipPlacementPanel()
        {
            Debug.Log($"[NetworkShipPlacementUI] ShowShipPlacementPanel called - shipsToInstantiate count: {shipsToInstantiate?.Count}");

            if (shipsToInstantiate == null)
            {
                Debug.LogError("[NetworkShipPlacementUI] shipsToInstantiate is NULL!");
                return;
            }
            ClearCreatedShips();
            int placedShips = 0;
            int totalShips = shipsToInstantiate.Count;

            Debug.Log($"[NetworkShipPlacementUI] Checking {totalShips} ships for placement status");

            foreach (Ship ship in shipsToInstantiate)
            {
                if (ship.isPlaced) //No need to show ships that are already placed
                {
                    Debug.Log($"A ship is placed {ship.GetType()}, total placed so far = {placedShips}");
                    placedShips++;
                    continue;
                }
                else
                {
                    GameObject shipPlacement = Instantiate(battleshipPrefabs[(int)ship.GetShipType()], new Vector3(0, 0, 0), Quaternion.identity);

                    // CRITICAL: Use the SAME ship instance, not a new one
                    shipPlacement.GetComponent<NetworkShipUI>().Initialize(ship);
                    shipPlacement.GetComponent<NetworkShipUI>().SetLocalPlayer(localPlayer);
                    shipPlacement.name = ship.GetShipType().ToString();
                    shipPlacement.transform.parent = shipPlacementPanel.transform;
                    createdShips.Add(shipPlacement);
                }
            }
        }

        private void ClearCreatedShips()
        {
            foreach (GameObject ship in createdShips)
            {
                if (ship != null)
                {
                    Destroy(ship);
                }
            }
            createdShips.Clear();
        }

        public void OpenPanel()
        {
            Debug.Log("Opening UI panel");
            shipPlacementPanel.GetComponent<WidgetPopup>().openWidgetPopup();
        }

        //It says Open Widget but it really just opens and closes it, so we can use the same one to "close" it
        public void ClosePanel()
        {
            shipPlacementPanel.GetComponent<WidgetPopup>().closeWidgetPopup();
        }

    }
}