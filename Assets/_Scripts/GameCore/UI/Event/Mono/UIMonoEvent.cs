using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoEvent : _ASCUIMonoBase
    {
        [Header("姓名文本")]
        public Text txtName;
        [Header("内容文本")]
        public Text txtContent;
        [Header("选择项容器")]
        public UIMonoCommonContainer monoSelectContainer;
        [Header("对话点击区域")]
        public Image imgClickArea;
        [Header("对话内容逐字显示间隔（秒）")]
        public float dialogueTypewriterInterval = 0.04f;

    }
}
