using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Games.Battleship
{
    /// <summary>
    /// UI representation of a ship that can be selected during placement phase
    /// </summary>
    public class ShipUI : MonoBehaviour, IPointerDownHandler
    {
        public Ship shipData;
        private bool _selected = false;

        // Static event that will be triggered when any ship is selected
        public static event Action<ShipUI> OnShipSelected;

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
        public void Initialize(BattleshipShipType type)
        {
            shipData = new Ship(type);
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

        private void HandleShipSelected(ShipUI selectedShip)
        {
            // If another ship was selected, deselect this one
            if (selectedShip != this && selected)
            {
                selected = false;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (selected)
            {
                selected = false;
            }
            else
            {
                selected = true;

                OnShipSelected?.Invoke(this);

                BattleshipManager.Instance.SetShipType(shipData);
            }
        }
    }
}