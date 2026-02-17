using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoMaskCombine : _ASCUIAnimMonoBase
    {
        [Header("部位Container")]
        public UIMonoCommonContainer monoPartContainer;
        [Header("脸部mono")]
        public UIMonoMaskCombineFace monoFace;
        [Header("确定按钮")]
        public Button btnConfirm;
        [Header("敌人面具mono")]
        public UIMonoEnemyMask monoEnemyMask;
        [Header("牌堆按钮")]
        public Button btnDeck;
        [Header("血条bar")]
        public Image imgHealthBar;
        [Header("血量文本")]
        public Text txtHealth;
        [Header("战斗先手文本")]
        public Text txtBattleOrder;
    }
}
