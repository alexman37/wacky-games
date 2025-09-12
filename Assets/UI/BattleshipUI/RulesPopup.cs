using UnityEngine;

namespace Games.Battleship
{
    public class RulesPopup : WidgetPopup
    {
        BattleshipManager manager;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public void closeRules()
        {
            manager = BattleshipManager.Instance;
            manager.ChangeState(new PlaceShipsState(manager));
            closeWidgetPopup();
        }
    }
}

