using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoStore : _ASCUIMonoBase
    {
        [Header("mono列表")]
        public List<UIMonoStoreItem> monoItemList;
        [Header("金钱文本")]
        public Text txtPlayerMoney;
        [Header("离开按钮")]
        public Button btnExit;
        [Header("背包按钮")]
        public Button btnBag;
        [Header("血条图片")]
        public Image imgHealthBar;
        [Header("血条过渡时间")]
        public float healthBarFadeDuration;
        [Header("鼠标移入按钮的缩放")]
        public float scaleMouseEnter;
        [Header("按钮的缩放时间")]
        public float scaleChgDuration;
        [Header("血量文本")]
        public Text txtHealth;
    }
}
