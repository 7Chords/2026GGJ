using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoMaskCombineFaceGrid : _ASCUIMonoBase
    {
        [Header("默认的颜色")]
        public Color colorDefault;
        [Header("可以放置的颜色")]
        public Color colorCanPlace;
        [Header("不可以放置的颜色")]
        public Color colorCanNotPlace;
        [Header("作用范围的颜色")]
        public Color colorIsEffective;
        [Header("占据与效果重叠（同一格）")]
        public Color colorOverlap;
        [Header("背景图片")]
        public Image imgGrid;
    }
}
