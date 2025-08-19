using Games.Minesweeper;
using System.Collections.Generic;
using UnityEngine;

namespace Games.Battleship
{
    public class ShipPlacementUI : MonoBehaviour
    {
        public static ShipPlacementUI Instance;
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

    }
}