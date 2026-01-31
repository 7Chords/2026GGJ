using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoStoreItem : _ASCUIMonoBase
    {
        [Header("商品图标")]
        public Image imgIcon;
        [Header("部位价格")]
        public Text txtPartPrice;
        [Header("部位生命值")]
        public Text txtPartHealth;
        [Header("回血价格")]
        public Text txtHealthPrice;
        [Header("购买按钮")]
        public Button btnPurchase;
        [Header("鼠标移入的缩放")]
        public float scaleMouseEnter;
        [Header("鼠标移入的缩放时间")]
        public float scaleChgDuration;
        [Header("已购买之后的画布阿尔法")]
        public float hasPurchaseAlpha;
        [Header("是部位货物显示的物品列表")]
        public List<GameObject> goIsPartShowList;
        [Header("是回血货物显示的物品列表")]
        public List<GameObject> goIsHealthShowList;
    }
}
