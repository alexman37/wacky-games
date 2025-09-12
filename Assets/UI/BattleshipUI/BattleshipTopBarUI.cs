using UnityEngine;
using TMPro;

namespace Games.Battleship
{
    public class BattleshipTopBarUI : MonoBehaviour
    {
        public static BattleshipTopBarUI instance;

        public TextMeshProUGUI turnTeller;
        public TextMeshProUGUI debugInfo;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (instance == null) instance = this;
            else if (instance != this) Destroy(gameObject);

            BattleshipManager.changedGameState += updatePhase;
        }

        void updatePhase(string toState)
        {
            string[] s = toState.Split('.');
            turnTeller.text = "Phase: " + s[s.Length-1];
        }

        public void displayDebugInfo(string info)
        {
            debugInfo.text = info;
        }
    }

}
