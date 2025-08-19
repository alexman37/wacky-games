using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Games.Battleship
{
    /// <summary>
    /// The ships the players use in battleship. This ship can be a variety of lengths, and potentially
    /// widths if we want to make it more complex.
    /// </summary>
    public class Ship : MonoBehaviour, IPointerDownHandler
    {
        public int shipLength;
        public BattleshipShipType shipType;
        public List<Tile> occupiedTiles; //List of the tiles this ship currently occupies.
        public bool selected = false;

        public void Initalize(BattleshipShipType type)
        {
            occupiedTiles = new List<Tile>();
            shipType = type;
            switch (shipType)
            {
                case BattleshipShipType.CARRIER:
                    shipLength = 5;
                    break;
                case BattleshipShipType.BATTLESHIP:
                    shipLength = 4;
                    break;
                case BattleshipShipType.CRUISER:
                    shipLength = 3;
                    break;
                case BattleshipShipType.SUBMARINE:
                    shipLength = 3;
                    break;
                case BattleshipShipType.DESTROYER:
                    shipLength = 2;
                    break;
            }
        }
        public void PlaceShip(List<Tile> tiles)
        {
            occupiedTiles = tiles;
        }

        public int GetShipLength()
        {
            return shipLength;
        }

        public BattleshipShipType GetShipType()
        {
            return shipType;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (selected)
            {
                selected = false;
                gameObject.GetComponent<Image>().enabled = false;                
            }
            else
            {
                selected = true;
                gameObject.GetComponent<Image>().enabled = true;
                Debug.Log("Hello I am a " + shipType.ToString());
                BattleshipManager.Instance.SetShipType(shipType);
            }
        }
    }
}