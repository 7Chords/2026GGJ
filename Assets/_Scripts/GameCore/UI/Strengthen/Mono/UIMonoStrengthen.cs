using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoStrengthen : _ASCUIMonoBase
    {
        [Header("强化前预览mono")]
        public UIMonoStrengthenPreview monoPreviewBefore;
        [Header("强化后预览mono")]
        public UIMonoStrengthenPreview monoPreviewAfter;
        [Header("背包mono")]
        public UIMonoCommonContainer monoBagContainer;
        [Header("确认强化按钮")]
        public Button btnConfirm;
        [Header("强化消耗金钱文本")]
        public Text txtStrengthenCoin;
        [Header("玩家金钱文本")]
        public Text txtPlayerCoin;
        [Header("离开按钮")]
        public Button btnExit;
        [Header("设置按钮")]
        public Button btnSetting;
        [Header("选择了强化器官要显示的物体")]
        public List<GameObject> goHasSelectPart;
        [Header("鼠标移入按钮的缩放")]
        public float scaleMouseEnter = 1.08f;
        [Header("鼠标移入的缩放时间")]
        public float scaleChgDuration = 0.15f;
    }
}
