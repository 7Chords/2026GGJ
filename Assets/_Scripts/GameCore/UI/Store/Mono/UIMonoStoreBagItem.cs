using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SCFrame.UI;

namespace GameCore.UI
{
    public class UIMonoStoreBagItem : _ASCUIMonoBase
    {
        [Header("部位icon")]
        public Image imgIcon;
        [Header("生命值文本")]
        public Text txtHealth;
        [Header("价值文本")]
        public Text txtValue;
        [Header("鼠标移入的缩放")]
        public float scaleMouseEnter;
        [Header("鼠标移入的缩放时间")]
        public float scaleChgDuration;
        [Header("出售按钮")]
        public Button btnSell;

    }
}
