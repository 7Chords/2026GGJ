using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoStart : _ASCUIMonoBase
    {
        [Header("开始按钮")]
        public Button btnStart;
        //[Header("设置按钮")]
        //public Button btnSetting;
        [Header("结束按钮")]
        public Button btnExit;
        [Header("鼠标移入按钮的缩放")]
        public float scaleMouseEnter;
        [Header("鼠标移入的缩放时间")]
        public float scaleChgDuration;
    }
}
