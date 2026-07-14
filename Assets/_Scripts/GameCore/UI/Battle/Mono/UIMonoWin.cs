using SCFrame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoWin : _ASCUIMonoBase
    {
        [Header("Return main button")]
        public Button btnReturnMain;
        [Header("Battles cleared")]
        public Text txtBattleCount;
        [Header("Events cleared")]
        public Text txtEventCount;
        [Header("Shops cleared")]
        public Text txtShopCount;
        [Header("Strengthen cleared")]
        public Text txtStrengthenCount;
        [Header("Total gold earned")]
        public Text txtTotalGold;
        [Header("Total damage dealt")]
        public Text txtTotalDamage;
    }
}
