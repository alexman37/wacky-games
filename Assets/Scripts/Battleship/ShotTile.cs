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
    public class ShotTile : BattleshipTile
    {
        public bool isEnemyShip = false; // Indicates whether this tile is an enemy ship tile or not
        public bool hasBeenShot = false; // Indicates whether this tile has been shot at
        public Material hitMaterial; // Material to apply when the tile is hit
        public Material missMaterial; // Material to apply when the tile is missed

        public void OnMouseEnter()
        {
            if(hasBeenShot)
            {
                return; // Don't highlight if the tile has already been shot at
            }
            switch (BattleshipManager.Instance.GetCurrentTurn())
            {
                case BattleshipTurn.PLAYER1:
                case BattleshipTurn.PLAYER2:
                    HighlightForAttacking();
                    break;
                default:
                    break;
            }
        }

        public void OnMouseExit()
        {
            StopChangingTransparency();
        }

        public void OnMouseDown()
        {
            if (hasBeenShot)
            {
                return; // Don't highlight if the tile has already been shot at
            }
            switch (BattleshipManager.Instance.GetCurrentTurn())
            {
                case BattleshipTurn.PLAYER1:
                case BattleshipTurn.PLAYER2:
                    ShootTile();
                    break;
                default:
                    break;
            }
            
        }

        public void HighlightForAttacking()
        {
            List<BattleshipTile> tilesToHighlight = new List<BattleshipTile> { this };
            GridManager.Instance.StartTransparencyChange(tilesToHighlight, 2f);
        }

        public void ShootTile()
        {
            MeshRenderer hitMarkerRenderer = transform.GetChild(0).GetComponent<MeshRenderer>();
            Debug.Log($"Player has shot at " + coordinates);
            hasBeenShot = true;
            if (isEnemyShip)
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