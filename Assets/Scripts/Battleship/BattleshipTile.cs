using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Games.Battleship
{
    /// <summary>
    /// Parent class for all tiles in the Battleship game.
    /// </summary>
    public class BattleshipTile : MonoBehaviour
    {
        public Vector2 coordinates;
        protected Coroutine transparencyCoroutine;
        protected float minTransparency = 0.25f; //0 is completely transparent, which is too weak
        protected float maxTransparency = 0.7f; //1f is completely opaque, which is too strong

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