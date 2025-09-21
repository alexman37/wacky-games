using UnityEngine;
using TMPro;

namespace Games.Battleship
{
    public class BattleshipTopBarUI : MonoBehaviour
    {
        public static BattleshipTopBarUI instance;

        public TextMeshProUGUI turnTeller;
        public TextMeshProUGUI debugInfo;

        public ShipViewPopup shipViewYours;
        public ShipViewPopup shipViewTheirs;
        private bool shipViewOpen;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (instance == null) instance = this;
            else if (instance != this) Destroy(gameObject);

            BattleshipManager.changedGameState += updatePhase;
        }

        void updatePhase(string toState)
        {
            turnTeller.text = "Phase: " + toState;
        }

        public void displayDebugInfo(string info)
        {
            debugInfo.text = info;
        }

        public void openBothShipViews()
        {
            shipViewYours.openWidgetPopup();
            shipViewTheirs.openWidgetPopup();
            shipViewOpen = true;
        }

        public void closeBothShipViews()
        {
            shipViewYours.closeWidgetPopup();
            shipViewTheirs.closeWidgetPopup();
            shipViewOpen = false;
        }

        public void toggleShipViews()
        {
            if (!shipViewOpen) openBothShipViews();
            else closeBothShipViews();
        }
    }

}
