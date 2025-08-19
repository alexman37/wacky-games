using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Games.Battleship
{
    public class Tile : MonoBehaviour
    {
        public Vector2 coordinates;
        public bool isChecked = false;
        public bool isShip = false;
        List<Tile> tilesToHighlight = new List<Tile>();
        public Material baseMaterial;

        public void OnMouseDown()
        {
            Debug.Log($"Slishhhh " + coordinates);
        }

        public void Start()
        {
            baseMaterial = this.GetComponent<MeshRenderer>().material;
        }

        private Coroutine transparencyCoroutine;

        private float minTransparency = 0.25f; //0 is completely transparent, which is too weak
        private float maxTransparency = 0.6f; //1f is completely opaque, which is too strong

        // If the player has a ship selected (i.e. during the ship placement phase), we want to highlight
        // the tiles this ship would occupy if placed.
        // Otherwise, this should be highlighted to indicate that the player is hovering over it.
        public void OnMouseEnter()
        {
            switch (BattleshipManager.Instance.GetCurrentTurn())
            {
                case BattleshipTurn.PLAYER1:
                case BattleshipTurn.PLAYER2:
                    HighlightForAttacking();
                    break;
                case BattleshipTurn.GAME_SETUP:
                    HighlightForShipPlacement();
                    break;
                case BattleshipTurn.GAME_OVER:
                case BattleshipTurn.NONE:
                default:
                    break;
            }
        }

        public void OnMouseExit()
        {
            GridManager.Instance.StopTransparencyChangeTiles(tilesToHighlight);
            foreach(Tile tile in tilesToHighlight)
            {
                tile.GetComponent<MeshRenderer>().material = tile.baseMaterial;
            }
            tilesToHighlight.Clear();
        }

        public void HighlightForShipPlacement()
        {
            tilesToHighlight.Clear();
            tilesToHighlight.Add(this);
            BattleshipShipType shipType = BattleshipManager.Instance.selectedShipType;
            BattleshipRotation rotation = BattleshipManager.Instance.shipRotation;
            switch (shipType)
            {
                case BattleshipShipType.CARRIER:
                    if(rotation == BattleshipRotation.HORIZONTAL)
                    {
                        tilesToHighlight.Add(GridManager.Instance.GetTileFromPosition((int)coordinates.x, (int)coordinates.y - 1));
                        tilesToHighlight.Add(GridManager.Instance.GetTileFromPosition((int)coordinates.x, (int)coordinates.y - 2));
                        tilesToHighlight.Add(GridManager.Instance.GetTileFromPosition((int)coordinates.x, (int)coordinates.y + 1));
                        tilesToHighlight.Add(GridManager.Instance.GetTileFromPosition((int)coordinates.x, (int)coordinates.y + 2));
                    }
                    else
                    {
                        tilesToHighlight.Add(GridManager.Instance.GetTileFromPosition((int)coordinates.x - 1, (int)coordinates.y));
                        tilesToHighlight.Add(GridManager.Instance.GetTileFromPosition((int)coordinates.x - 2, (int)coordinates.y));
                        tilesToHighlight.Add(GridManager.Instance.GetTileFromPosition((int)coordinates.x + 1, (int)coordinates.y));
                        tilesToHighlight.Add(GridManager.Instance.GetTileFromPosition((int)coordinates.x + 2, (int)coordinates.y));
                    }
                    break;
                case BattleshipShipType.BATTLESHIP:
                    if (rotation == BattleshipRotation.HORIZONTAL)
                    {
                        tilesToHighlight.Add(GridManager.Instance.GetTileFromPosition((int)coordinates.x, (int)coordinates.y - 1));
                        tilesToHighlight.Add(GridManager.Instance.GetTileFromPosition((int)coordinates.x, (int)coordinates.y + 1));
                        tilesToHighlight.Add(GridManager.Instance.GetTileFromPosition((int)coordinates.x, (int)coordinates.y + 2));
                    }
                    else
                    {
                        tilesToHighlight.Add(GridManager.Instance.GetTileFromPosition((int)coordinates.x - 1, (int)coordinates.y));
                        tilesToHighlight.Add(GridManager.Instance.GetTileFromPosition((int)coordinates.x + 1, (int)coordinates.y));
                        tilesToHighlight.Add(GridManager.Instance.GetTileFromPosition((int)coordinates.x + 2, (int)coordinates.y));
                    }
                    break;
                case BattleshipShipType.CRUISER:
                    if (rotation == BattleshipRotation.HORIZONTAL)
                    {
                        tilesToHighlight.Add(GridManager.Instance.GetTileFromPosition((int)coordinates.x, (int)coordinates.y - 1));
                        tilesToHighlight.Add(GridManager.Instance.GetTileFromPosition((int)coordinates.x, (int)coordinates.y + 1));
                    }
                    else
                    {
                        tilesToHighlight.Add(GridManager.Instance.GetTileFromPosition((int)coordinates.x - 1, (int)coordinates.y));
                        tilesToHighlight.Add(GridManager.Instance.GetTileFromPosition((int)coordinates.x + 1, (int)coordinates.y));
                    }
                    break;
                case BattleshipShipType.SUBMARINE:
                    if (rotation == BattleshipRotation.HORIZONTAL)
                    {
                        tilesToHighlight.Add(GridManager.Instance.GetTileFromPosition((int)coordinates.x, (int)coordinates.y - 1));
                        tilesToHighlight.Add(GridManager.Instance.GetTileFromPosition((int)coordinates.x, (int)coordinates.y + 1));
                    }
                    else
                    {
                        tilesToHighlight.Add(GridManager.Instance.GetTileFromPosition((int)coordinates.x - 1, (int)coordinates.y));
                        tilesToHighlight.Add(GridManager.Instance.GetTileFromPosition((int)coordinates.x + 1, (int)coordinates.y));
                    }
                    break;
                case BattleshipShipType.DESTROYER:
                    if (rotation == BattleshipRotation.HORIZONTAL)
                    {
                        tilesToHighlight.Add(GridManager.Instance.GetTileFromPosition((int)coordinates.x, (int)coordinates.y + 1));
                    }
                    else
                    {
                        tilesToHighlight.Add(GridManager.Instance.GetTileFromPosition((int)coordinates.x + 1, (int)coordinates.y));
                    }
                    break;
            }
            GridManager.Instance.StartTransparencyChange(tilesToHighlight, 2f);
        }
        public void HighlightForAttacking()
        {

        }
        public void StartChangingTransparency(float duration)
        {
            if (transparencyCoroutine != null)
            {
                StopCoroutine(transparencyCoroutine);
            }
            transparencyCoroutine = StartCoroutine(ChangeTransparencyOverTime(duration));
        }

        public void StopChangingTransparency()
        {
            if (transparencyCoroutine != null)
            {
                StopCoroutine(transparencyCoroutine);
                transparencyCoroutine = null;
                SetTransparency(maxTransparency);
            }
        }

        private IEnumerator ChangeTransparencyOverTime(float duration)
        {
            float time = 0;
            bool increasingTransparency = true;

            while (true)
            {
                time += Time.deltaTime;
                //alpha = time/ duration + minTransparency if increasingTransparency, maxTransparency - time/duration if decreasingTransparency
                //min and max added so that the bounds are set for the alpha value
                float alpha = increasingTransparency ? time / duration + minTransparency : maxTransparency - (time / duration);
                SetTransparency(alpha);

                if (time >= duration || alpha <= minTransparency || alpha >= maxTransparency)
                {
                    increasingTransparency = !increasingTransparency;
                    time = 0;
                }

                yield return null;
            }
        }

        private void SetTransparency(float alpha)
        {
            Material tileMaterial = this.gameObject.GetComponent<MeshRenderer>().material;
            Color color = tileMaterial.color;
            color.a = Mathf.Clamp(alpha, minTransparency, maxTransparency);
            tileMaterial.color = color;
        }

    }
}