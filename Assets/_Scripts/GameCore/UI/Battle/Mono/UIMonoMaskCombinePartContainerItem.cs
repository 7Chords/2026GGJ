using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoMaskCombinePartContainerItem : _ASCUIMonoBase
    {
        [Header("商品图标")]
        public Image imgGoods;
        [Header("血量文本")]
        public Text txtHealth;
        [Header("鼠标移入缩放")]
        public float scaleMouseEnter;
        [Header("鼠标移入缩放时间")]
        public float scaleChgDuration;
    }
}
