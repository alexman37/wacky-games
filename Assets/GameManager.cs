using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

/// <summary>
/// This class manages high-level game stats and occurances, e.g,
/// Switching between scenes (If eligible), starting new games under certain conditions, etc.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public Tile baseTile;
    public SpriteAtlas spriteAtlas;

    public Image deleteme;

    // Start is called before the first frame update
    void Start()
    {
        if(instance == null)
        {
            instance = this;
        }

        deleteme.sprite = GameManager.instance.spriteAtlas.GetSprite("hex minesweeper_1");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
