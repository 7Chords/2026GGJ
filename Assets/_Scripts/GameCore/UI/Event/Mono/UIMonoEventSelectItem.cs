using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoEventSelectItem : _ASCUIMonoBase
    {
        [Header("选择按钮")]
        public Button btnSelect;

        [Header("内容文本")]
        public Text txtContent;

        [Header("鼠标悬浮缩放")]
        public float scaleMouseEnter = 1.05f;

        [Header("缩放动画时长")]
        public float scaleChgDuration = 0.12f;
    }

}
