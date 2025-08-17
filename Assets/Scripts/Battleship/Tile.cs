using System.Collections.Generic;
using UnityEngine;

namespace Games.Battleship
{
    public class Tile : MonoBehaviour
    {
        public Vector2 coordinates;
        public bool isChecked = false;
        public bool isShip = false;
        public HashSet<Tile> adjacencies;

        public void OnMouseDown()
        {
            Debug.Log($"Slishhhh " + coordinates);
        }
    }
}