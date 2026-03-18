using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoCommonGuide : _ASCUIMonoBase
    {
        [Header("关闭按钮")]
        public Button btnClose;
        [Header("下一个按钮")]
        public Button btnNext;
        [Header("上一个按钮")]
        public Button btnLast;
        [Header("索引图标容器mono")]
        public UIMonoCommonContainer monoIndexContainer;
        [Header("指引页列表")]
        public List<GameObject> goGuideList;
    }
}
