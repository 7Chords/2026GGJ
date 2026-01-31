using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoCommonPartItem : _ASCUIMonoBase
    {
        [Header("部位icon")]
        public Image imgIcon;
        [Header("生命值文本")]
        public Text txtHealth;
        [Header("鼠标移入的缩放")]
        public float scaleMouseEnter;
        [Header("鼠标移入的缩放时间")]
        public float scaleChgDuration;
        [Header("生命值文本是否是cur/max型")]
        public bool isTxtHealthIsRunningInfo;
    }

}