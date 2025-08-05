using UnityEngine;
using UnityEngine.UI;

public class ButtonGroup : MonoBehaviour
{
    public Button[] buttons;
    public int defaultChoice;
    public int currSelected;

    public Image selectedBorder;

    private Color faded = new Color(1, 1, 1, 0.5f);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currSelected = defaultChoice;
        onButtonChosenMoveSelectedBorder(currSelected);
    }

    public void onButtonChosenMoveSelectedBorder(int choice)
    {
        currSelected = choice;
        selectedBorder.rectTransform.anchoredPosition = buttons[choice].image.rectTransform.anchoredPosition;

        for (int i = 0; i < buttons.Length; i++)
        {
            if (i == choice) {
                buttons[i].image.color = Color.white;
            } else
            {
                buttons[i].image.color = faded;
            }
        }
    }
}
