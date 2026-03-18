using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoCommonIndexItem : _ASCUIMonoBase
    {
        [Header("图标")]
        public Image imgIndex;
        [Header("是当前索引时的颜色")]
        public Color isCurIndex;
        [Header("不是当前索引时的颜色")]
        public Color isNotCurColor;
    }
}
