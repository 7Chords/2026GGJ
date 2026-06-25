using SCFrame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoHistoryItem : _ASCUIMonoBase
    {
        [Header("Expand toggle")]
        public Button btnToggle;
        [Header("Favorite button")]
        public Button btnFavorite;
        [Header("Win/Lose text")]
        public Text txtResult;
        [Header("Recorded time")]
        public Text txtTime;
        [Header("Lose location (hidden on win)")]
        public Text txtLoseLocation;
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
        [Header("Expand hint")]
        public Text txtExpandHint;
        [Header("Expanded part library root")]
        public GameObject goPartRoot;
        [Header("End-of-run part container")]
        public UIMonoCommonContainer monoPartContainer;
    }
}
