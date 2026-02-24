using SCFrame.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoMapNode : _ASCUIMonoBase
    {
        [Header("图标")]
        public Image imgIcon;
        [Header("按钮")]
        public Button btnEnter;
        [Header("可以行走的标识")]
        public List<GameObject> goCanWalk;
    }
}
