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
    }
}
