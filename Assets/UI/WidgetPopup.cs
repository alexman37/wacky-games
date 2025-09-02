using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WidgetPopup : ScalingUIComponent
{
    public static string activeWidget = null;
    public string widgetId;
    public bool overrideOtherWidgets = false; // if true, this widget can open even when others are already opened

    public Vector2 proportionalDestination;
    private Vector2 realDestination;
    private Vector2 realStart;

    private RectTransform rectTransform;
    IEnumerator movingCoroutineIn;
    IEnumerator movingCoroutineOut;

    private bool isOpen = false;

    private void Start()
    {
        Setup();
    }

    public void Setup()
    {
        base.InitSetup();
        StartCoroutine(SetupRoutine());
    }

    // Wait for it to scale before we determine starting / ending positions
    IEnumerator SetupRoutine()
    {
        yield return new WaitUntil(() => this.DONE == true);

        rectTransform = GetComponent<RectTransform>();
        realStart = rectTransform.anchoredPosition;
        realDestination = getPositionFromProportion(rectTransform, focalPoint, proportionalDestination);
        rectTransform.anchoredPosition = realStart;

        movingCoroutineIn = UIUtils.XerpOnUiCoroutine(30, 0.5f, rectTransform, realStart);
        movingCoroutineOut = UIUtils.XerpOnUiCoroutine(30, 0.5f, rectTransform, realDestination);
        openWidgetPopup();
    }

    // kind of inefficient to run this for every widget so...just call it when you need to
    public static void resetWidgets()
    {
        activeWidget = null;
    }

    public void openWidgetPopup()
    {
        // only allow one open widget at a time
        if (activeWidget == null)
        {
            activeWidget = widgetId;
            StopCoroutine(movingCoroutineOut);
            movingCoroutineIn = UIUtils.XerpOnUiCoroutine(30, 0.5f, rectTransform, realDestination);
            StartCoroutine(movingCoroutineIn);
        }

        // similar story for widget overrides but it doesn't affect the already opened "active widget"
        else if (overrideOtherWidgets)
        {
            StopCoroutine(movingCoroutineOut);
            movingCoroutineIn = UIUtils.XerpOnUiCoroutine(30, 0.5f, rectTransform, realDestination);
            StartCoroutine(movingCoroutineIn);
        }

        isOpen = true;
    }

    public void closeWidgetPopup()
    {
        StopCoroutine(movingCoroutineIn);
        movingCoroutineOut = UIUtils.XerpOnUiCoroutine(30, 0.5f, rectTransform, realStart);
        StartCoroutine(movingCoroutineOut);

        isOpen = false;
        activeWidget = null;
    }

    public void toggleWidgetPopup()
    {
        if (isOpen) closeWidgetPopup();
        else openWidgetPopup();
    }
}