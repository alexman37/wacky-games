using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Games.Battleship
{
    /// <summary>
    /// UI representation of a ship that can be selected during placement phase
    /// </summary>
    public class NetworkShipUI : MonoBehaviour, IPointerDownHandler
    {
        public Ship shipData;
        private bool _selected = false;
        private NetworkPlayer localPlayer;
        public bool hasBeenPlaced = false; // To track if the ship has been placed on the grid
        // so the player cannot select this specific one again

        // Static event that will be triggered when any ship is selected
        public static event Action<NetworkShipUI> OnShipSelected;

        public bool selected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                UpdateVisualState();
            }
        }

        private void OnEnable()
        {
            // Subscribe to the selection event
            OnShipSelected += HandleShipSelected;
        }

        private void OnDisable()
        {
            // Unsubscribe from the event when this object is disabled
            OnShipSelected -= HandleShipSelected;
        }
        public void Initialize(Ship ship)
        {
            shipData = ship;
        }

        public void SetLocalPlayer(NetworkPlayer player)
        {
            localPlayer = player;
        }

        public BattleshipShipType GetShipType()
        {
            return shipData.GetShipType();
        }

        public int GetShipLength()
        {
            return shipData.GetShipLength();
        }

        private void UpdateVisualState()
        {
            // Update the visual state based on selection
            if (gameObject.GetComponent<Image>() != null)
            {
                gameObject.GetComponent<Image>().enabled = selected;
            }
        }

        private void HandleShipSelected(NetworkShipUI selectedShip)
        {
            // If another ship was selected, deselect this one
            if (selectedShip != this && selected)
            {
                selected = false;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (hasBeenPlaced) return; // Do nothing if the ship has already been placed
            if (selected)
            {
                selected = false;
                // Clear selection
                SetPlayerSelectedShip(shipData);
            }
            else
            {
                selected = true;

                OnShipSelected?.Invoke(this);

                // Set the selected ship on the local player
                SetPlayerSelectedShip(shipData);
            }
        }

        private void SetPlayerSelectedShip(Ship shipToPlace)
        {
            // Get the local player and set their selected ship
            if (localPlayer != null)
            {
                localPlayer.selectedShipType = shipToPlace.GetShipType();
                localPlayer.shipToPlace = shipToPlace;
            }
        }
    }
}