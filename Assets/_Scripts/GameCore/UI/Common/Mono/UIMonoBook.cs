using SCFrame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoBook : _ASCUIMonoBase
    {
        [Header("Category index root")]
        public GameObject goIndexes;
        [Header("Category: part")]
        public Button btnPart;
        [Header("Category: enemy")]
        public Button btnEnemy;
        [Header("Part book page")]
        public GameObject goPagePart;
        [Header("Enemy book page (optional)")]
        public GameObject goPageEnemy;
        [Header("Enemy list container")]
        public UIMonoCommonContainer monoEnemyContainer;

        [Header("Part list container")]
        public UIMonoCommonContainer monoPartContainer;
        [Header("Close button")]
        public Button btnClose;
        [Header("Filter: eye")]
        public Button btnEye;
        [Header("Filter: nose")]
        public Button btnNose;
        [Header("Filter: mouth")]
        public Button btnMouth;
        [Header("Filter: skin")]
        public Button btnSkin;
        [Header("Button hover scale")]
        public float btnEnterScale = 1.1f;
        [Header("Button scale duration")]
        public float btnScaleChgTime = 0.1f;
    }
}
