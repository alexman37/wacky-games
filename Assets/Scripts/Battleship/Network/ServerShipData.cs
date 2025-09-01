using System.Collections.Generic;
using UnityEngine;

namespace Games.Battleship
{
    // Server-only data structure - never sent over network
    [System.Serializable]
    public class ServerShipData
    {
        public BattleshipShipType shipType;
        public List<Vector2Int> positions;
        public ulong playerId;
        public bool isDestroyed;
        public int hitCount;

        public ServerShipData()
        {
            positions = new List<Vector2Int>();
            hitCount = 0;
        }

        public bool IsHit(Vector2Int position)
        {
            return positions.Contains(position);
        }

        public void RegisterHit()
        {
            hitCount++;
            if (hitCount >= positions.Count)
            {
                isDestroyed = true;
            }
        }
    }
}