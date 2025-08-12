using UnityEngine;
using UnityEngine.U2D;
using System;
using System.Collections;

namespace Games.Minesweeper
{
    public class MinesweeperStyles : MonoBehaviour
    {
        public static MinesweeperStyles instance;

        private MinesweeperStyleSheet activeStyleSheet;

        public static event Action newStyleSheetLoaded;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (instance == null) instance = this;
            else if (instance != this) Destroy(gameObject);
        }


        // Sprite getters
        public Sprite getNumberedSprite(int number)
        {
            return activeStyleSheet.numbered[number];
        }

        public Sprite getUnclickedSprite()
        {
            return activeStyleSheet.unclicked;
        }

        public Sprite getFlaggedSprite()
        {
            return activeStyleSheet.flagged;
        }

        public Sprite getMineSprite()
        {
            return activeStyleSheet.mine;
        }



        // All style sheets should follow the same layout
        public static void loadNewStyleSheet(Sprite[] sprites)
        {
            MinesweeperStyleSheet sheet = new MinesweeperStyleSheet();
            sheet.unclicked = sprites[0];
            sheet.flagged = sprites[1];
            sheet.mine = sprites[11];

            // hexes have 7 and 8 sprites even though they're never used - whatever!
            sheet.numbered = new Sprite[9];
            for (int i = 2; i < 11; i++)
            {
                sheet.numbered[i - 2] = sprites[i];
            }
            sheet.smileys = new Sprite[3];
            for (int i = 12; i < 15; i++)
            {
                sheet.smileys[i - 12] = sprites[i];
            }

            instance.activeStyleSheet = sheet;

            newStyleSheetLoaded.Invoke();
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Q))
            {
                StartCoroutine(loadSpriteSheets("minesweeper/style/hex/base"));
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                StartCoroutine(loadSpriteSheets("minesweeper/style/hex/green"));
            }
        }

        IEnumerator loadSpriteSheets(string path)
        {
            yield return LoadAssetBundle.LoadBundle<Sprite>(path, loadNewStyleSheet);
        }
    }


    public class MinesweeperStyleSheet
    {
        public Sprite[] numbered;
        public Sprite unclicked;
        public Sprite flagged;
        public Sprite mine;
        public Sprite[] smileys;
    }

}