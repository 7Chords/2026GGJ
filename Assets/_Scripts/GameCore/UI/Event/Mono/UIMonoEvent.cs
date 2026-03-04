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
        public Transform tranSelectContent;

    }
}
