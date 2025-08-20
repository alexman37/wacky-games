using UnityEngine;
using UnityEngine.U2D;
using System;
using System.Collections;

namespace Games.Minesweeper
{
    public class MinesweeperStyles : MonoBehaviour
    {
        public static MinesweeperStyles instance;
        public static bool greenlight = false;

        private MinesweeperStyleSheet activeStyleSheet;
        private MinesweeperStyleSheet nextStyleSheet; // TODO: better way to do this??? there must be.
                                                      // need some way to track what style sheet is desired without actually changing it for tiles
                                                      // for ex, if you change the shape completely, you won't change the style sheet in-game.

        public static event Action newStyleSheetLoaded;

        // according to asset bundle names to look for.
        [System.NonSerialized] public string[] squareStyleNames = {"base", "green", "purple", "gold", "orange", "storm" };
        [System.NonSerialized] public string[] hexStyleNames = {"base", "green", "purple", "gold", "orange", "storm" };

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (instance == null) instance = this;
            else if (instance != this) Destroy(gameObject);

            newStyleSheetLoaded += () => { };

            // Initial sprite sheet is actually set in TopBarUI
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

        public Sprite getSmileySprite(SmileyState state)
        {
            return activeStyleSheet.smileys[(int)state];
        }


        // On grid regeneration, use the next style sheet
        public void useNextStyle()
        {
            if(nextStyleSheet != null)
            {
                activeStyleSheet = nextStyleSheet;
                newStyleSheetLoaded.Invoke();
                nextStyleSheet = null;
            }
        }


        // Generic style sheet load
        private static MinesweeperStyleSheet instantiateNewStyleSheetFromSprites(Sprite[] sprites)
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

            return sheet;
        }

        // Change tile sheet now:
            public static void loadNewStyleSheet(Sprite[] sprites)
            {
                MinesweeperStyleSheet sheet = instantiateNewStyleSheetFromSprites(sprites);
                instance.activeStyleSheet = sheet;
                newStyleSheetLoaded.Invoke();
            }

            public void useNewStyleSheet(string shape, string style)
            {
                StartCoroutine(loadSpriteSheets($"minesweeper/style/{shape}/{style}"));
            }

            IEnumerator loadSpriteSheets(string path)
            {
                yield return LoadAssetBundle.LoadBundle<Sprite>(path, loadNewStyleSheet);
                greenlight = true;
                Debug.Log("Greenlit styles");
            }

        // Change queued tile sheet:
            private static void NEXT_loadNewStyleSheet(Sprite[] sprites)
            {
                MinesweeperStyleSheet sheet = instantiateNewStyleSheetFromSprites(sprites);
                instance.nextStyleSheet = sheet;
            }

            public void NEXT_useNewStyleSheet(string shape, string style)
            {
                StartCoroutine(NEXT_loadSpriteSheets($"minesweeper/style/{shape}/{style}"));
            }

            IEnumerator NEXT_loadSpriteSheets(string path)
            {
                yield return LoadAssetBundle.LoadBundle<Sprite>(path, NEXT_loadNewStyleSheet);
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

    public enum SmileyState
    {
        NORMAL,
        GASP,
        DEAD
    }

}