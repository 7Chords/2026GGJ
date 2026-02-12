using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoEnemyMaskGrid : _ASCUIMonoBase
    {
        [Header("格子图片")]
        public Image imgGrid;
        [Header("可以放置的颜色")]
        public Color colorCanPlace;
        [Header("不可以放置的颜色")]
        public Color colorCanNotPlace;
        [Header("作用范围的颜色")]
        public Color colorIsEffective;
        [Header("默认颜色")]
        public Color colorDefault;
    }
}
