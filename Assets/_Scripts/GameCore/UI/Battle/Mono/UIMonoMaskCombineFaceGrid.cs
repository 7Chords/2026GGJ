using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.UI
{
    public class UIMonoMaskCombineFaceGrid : _ASCUIMonoBase
    {
        [Header("坐标")]
        public Vector2 gridPos;
        [Header("默认颜色")]
        public Color colorDefault;
        [Header("可以放置的颜色")]
        public Color colorCanPlace;
        [Header("不可以放置的颜色")]
        public Color colorCanNotPlace;
    }
}
