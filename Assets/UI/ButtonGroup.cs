using UnityEngine;
using UnityEngine.UI;

public class ButtonGroup : MonoBehaviour
{
    // when a button in the group is clicked, it may have some sort of data associated with it that we wanna pass along.
    public delegate void onClick<T>(T data);

    public Button[] buttons;
    public int defaultChoice;
    public int currSelected;
    public onClick<int> selectedIndexEvent; // call this when a button is clicked (optional)

    public Image selectedBorder;

    private Color faded = new Color(1, 1, 1, 0.5f);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Oftentimes you want to wait to define the callback - so do this after you're done setting up the component
    public void setInitialValue()
    {
        currSelected = defaultChoice;
        onSelectByIndex(currSelected);
    }

    public void onSelectByIndex(int index)
    {
        onButtonChosenMoveSelectedBorder(index);
        if(selectedIndexEvent != null)
        {
            selectedIndexEvent(index);
        }
    }

    private void onButtonChosenMoveSelectedBorder(int choice)
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
