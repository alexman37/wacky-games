using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Games.Battleship
{
    public class DecisionMatrixDebugUI : MonoBehaviour
    {
        [SerializeField] private GameObject tileTemplate;
        [SerializeField] private GameObject container;

        [SerializeField] private GameObject hitTileTemplate;
        [SerializeField] private GameObject hitContainer;

        public static DecisionMatrixDebugUI instance;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (instance == null) instance = this;
            else Destroy(this.gameObject);
        }

        public void redrawDecisionGrid(int[,] scores)
        {
            float wScale = scores.GetLength(0) / 10f;
            float hScale = scores.GetLength(1) / 10f;

            float wDim = 200f / (float) scores.GetLength(0);
            float hDim = 200f / (float) scores.GetLength(1);

            // Clear old children
            DestroyAllChildren(container);

            // Make new tiles
            int allShipsLen = 0;
            BattleshipManager.Instance.GetShips().ForEach(ship => allShipsLen = allShipsLen + ship.shipLength);
            float colorRampFactor = 1f / (float)(allShipsLen * 2f);

            for (int h = 0; h < scores.GetLength(1); h++) 
            {
                for (int w = 0; w < scores.GetLength(0); w++)
                {
                    GameObject go = GameObject.Instantiate(tileTemplate);
                    go.transform.SetParent(container.transform);

                    Image img = go.GetComponent<Image>();
                    TextMeshProUGUI text = go.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

                    img.rectTransform.localScale = new Vector3(wScale, hScale, 1);
                    img.rectTransform.localPosition = new Vector3(wDim * w, hDim * h, 0);
                    int score = scores[w, h];

                    // -2 == miss, -1 = hit, anything else = unrevealed
                    if (score == -2)
                    {
                        img.color = new Color(0.3f, 0.3f, 0.3f);
                        text.text = "-";
                    }
                    else if (score == -1)
                    {
                        img.color = new Color(0.1f, 0.8f, 0.1f);
                        text.text = "*";
                    }
                    else if (score == -3)
                    {
                        img.color = new Color(0.1f, 0.4f, 0.1f);
                        text.text = "X";
                    }
                    else
                    {
                        img.color = new Color(colorRampFactor * score, 0, 1 - colorRampFactor * score);
                        text.text = score.ToString();
                    }
                }
            }
        }

        /// <summary>
        /// Redraw the hit grid debug viz
        /// </summary>
        public void redrawHitGrid(int[,] scores)
        {
            float wScale = scores.GetLength(0) / 10f;
            float hScale = scores.GetLength(1) / 10f;

            float wDim = 200f / (float)scores.GetLength(0);
            float hDim = 200f / (float)scores.GetLength(1);

            // Clear old children
            DestroyAllChildren(hitContainer);

            // Make new tiles
            int allShipsLen = 0;
            BattleshipManager.Instance.GetShips().ForEach(ship => allShipsLen = allShipsLen + ship.shipLength);
            float colorRampFactor = 1f / 10f;

            for (int h = 0; h < scores.GetLength(1); h++)
            {
                for (int w = 0; w < scores.GetLength(0); w++)
                {
                    GameObject go = GameObject.Instantiate(hitTileTemplate);
                    go.transform.SetParent(hitContainer.transform);

                    Image img = go.GetComponent<Image>();
                    TextMeshProUGUI text = go.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

                    img.rectTransform.localScale = new Vector3(wScale, hScale, 1);
                    img.rectTransform.localPosition = new Vector3(wDim * w, hDim * h, 0);
                    int score = scores[w, h];

                    // -2 == miss, -1 = hit, anything else = unrevealed
                    if (score == -2)
                    {
                        img.color = new Color(0f, 0f, 0f);
                        text.text = "-";
                    }
                    else if (score == -1)
                    {
                        img.color = new Color(0.1f, 0.8f, 0.1f);
                        text.text = "*";
                    }
                    else if (score == -3)
                    {
                        img.color = new Color(0.1f, 0.4f, 0.1f);
                        text.text = "X";
                    }
                    else
                    {
                        img.color = new Color(colorRampFactor * score, 0.1f, 0.1f);
                        text.text = score.ToString();
                    }
                }
            }
        }





        // TODO should really move this to utility somewhere
        public void DestroyAllChildren(GameObject parentObject)
        {
            // Iterate through the children in reverse order to avoid issues with hierarchy changes
            for (int i = parentObject.transform.childCount - 1; i >= 0; i--)
            {
                // Destroy the child GameObject
                Destroy(parentObject.transform.GetChild(i).gameObject);
            }
        }
    }
}
