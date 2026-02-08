using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoMaskCombinePartContainerItem : _ASCUIMonoBase
    {
        [Header("部位文本")]
        public Image imgGoods;
        [Header("生命文本")]
        public Text txtHealth;
        [Header("鼠标移入缩放")]
        public float scaleMouseEnter;
        [Header("缩放时间改变时间")]
        public float scaleChgDuration;
    }
}
