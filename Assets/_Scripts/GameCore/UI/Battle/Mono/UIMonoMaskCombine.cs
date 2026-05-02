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
        [Header("确认按钮")]
        public Button btnConfirm;
        [Header("敌人脸部mono")]
        public UIMonoEnemyMask monoEnemyMask;
        [Header("牌堆按钮")]
        public Button btnDeck;
        [Header("生命值bar")]
        public Image imgHealthBar;
        [Header("生命值文本")]
        public Text txtHealth;
        [Header("金钱文本")]
        public Text txtCoin;
        [Header("是玩家先手显示的物体列表")]
        public List<GameObject> goIsPlayerFirstShowList;
        [Header("是敌人先手显示的物体列表")]
        public List<GameObject> goIsEnemyFirstShowList;
        [Header("敌人生命值bar")]
        public Image imgEnemyHealthBar;
        [Header("敌人生命值文本")]
        public Text txtEnemyHealth;
        [Header("敌人生命值图片")]
        public Image imgEnemyHealth;
        [Header("设置按钮")]
        public Button btnSetting;
        [Header("教程按钮")]
        public Button btnGuide;
        [Header("鼠标移入缩放")]
        public float scaleMouseEnter = 1.08f;
        [Header("缩放改变持续时间")]
        public float scaleChgDuration = 0.15f;
        [Header("Entity HP preview colors")]
        public Color previewDamageColor = new Color(0.92f, 0.32f, 0.32f, 1f);
        public Color previewHealColor = new Color(0.32f, 0.82f, 0.45f, 1f);
        [Header("手牌数量文本")]
        public Text txtBusyCount;
    }
}
