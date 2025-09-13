using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

namespace Games.Battleship
{
    /// <summary>
    /// Tiles the represent the secondary grid for a given player. This is the traditional "top" grid
    /// in the game, which is used to represent where the player has attacked their opponent.
    /// Whether or not they have successfully hit a ship is represented by the color of the tile.
    /// PlayerTile.cs is the equivalent for the "bottom" grid, which is used to represent where the player
    /// has placed their ships, as well as where the opponent has attacked or successfully hit their ship.
    /// </summary>
    public class NetworkShotTile : BattleshipTile
    {
        public Material hitMaterial; // Material to apply when the tile is hit
        public Material missMaterial; // Material to apply when the tile is missed

        //Reference to the network grid manager which "owns" this tile
        private NetworkGridManager owningGridManager;

        public void SetOwningGridManager(NetworkGridManager gridManager)
        {
            owningGridManager = gridManager;
        }
        public void OnMouseEnter()
        {
            if (tileChecked) return;

            // Check if it's my turn through network manager
            if (owningGridManager != null && owningGridManager.IsMyTurn())
            {
                HighlightForAttacking();
            }
        }

        public void OnMouseExit()
        {
            StopChangingTransparency();
        }

        public void OnMouseDown()
        {
            if (tileChecked) return;

            if (owningGridManager != null && owningGridManager.IsMyTurn())
            {
                owningGridManager.OnShotTileClicked(this);
            }
        }

        public void HighlightForAttacking()
        {
            List<BattleshipTile> tilesToHighlight = new List<BattleshipTile> { this };
            owningGridManager.StartTransparencyChange(tilesToHighlight, 2f);
        }

        // This should take a boolean wasHit since from our perspective we shouldn't know if this
        // was a ship or not until we actually hit the tile (since the player wouldn't know where
        // the opponent's ships are placed)
        public void ShootTile(bool wasHit)
        {
            MeshRenderer hitMarkerRenderer = transform.GetChild(0).GetComponent<MeshRenderer>();
            Debug.Log($"Player has shot at " + coordinates);
            tileChecked = true;
            if (wasHit)
            {
                hitMarkerRenderer.material = hitMaterial; // Change the material to indicate a hit               
            }
            else
            {
                hitMarkerRenderer.material = missMaterial; // Change the material to indicate a miss
            }
            hitMarkerRenderer.enabled = true; // Show the hit marker
            StopChangingTransparency();
        }
    }
}