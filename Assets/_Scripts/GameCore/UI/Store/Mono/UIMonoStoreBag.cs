using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoStoreBag : _ASCUIMonoBase
    {
        [Header("列表mono")]
        public UIMonoCommonContainer monoContainer;
        [Header("关闭按钮")]
        public Button btnClose;
        [Header("鼠标移入按钮的缩放")]
        public float scaleMouseEnter;
        [Header("按钮的缩放时间")]
        public float scaleChgDuration;
    }
}
