using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// This class handles dispatching high-level game events.
/// E.g, "New game", "Game over", "Game won", etc...
/// </summary>
public class GameEventsManager : MonoBehaviour
{
    public static event Action<bool> changeTileClicking;

    // Start is called before the first frame update
    void Start()
    {
        changeTileClicking += (_) => { };
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void disableAllClicks()
    {
        changeTileClicking.Invoke(false);
    }
}
