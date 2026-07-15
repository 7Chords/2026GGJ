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
        [Header("Filter toggle: eye")]
        public Toggle toggleEye;
        [Header("Filter toggle: nose")]
        public Toggle toggleNose;
        [Header("Filter toggle: mouth")]
        public Toggle toggleMouth;
        [Header("Filter toggle: skin")]
        public Toggle toggleSkin;
        [Header("Filter toggle: enemy-only parts")]
        public Toggle toggleEnemyPart;
        [Header("Enemy filter toggle: floor 1")]
        public Toggle toggleEnemyFloor1;
        [Header("Enemy filter toggle: floor 2")]
        public Toggle toggleEnemyFloor2;
        [Header("Button hover scale")]
        public float btnEnterScale = 1.1f;
        [Header("Button scale duration")]
        public float btnScaleChgTime = 0.1f;
    }
}
