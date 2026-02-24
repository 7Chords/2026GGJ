using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoStrengthenPreview : _ASCUIMonoBase
    {
        [Header("标题文本")]
        public Text txtName;
        [Header("描述文本")]
        public Text txtDesc;
        [Header("品质文本")]
        public Text txtQuality;
        [Header("格子信息父物体")]
        public GameObject tranParentGrid;
        [Header("选择了器官显示的物体列表")]
        public List<GameObject> goHasSelectPartShowList;
        [Header("没有选择器官显示的物体列表")]
        public List<GameObject> goNoSelectPartShowList;
        [Header("血量文本")]
        public Text txtHealth;
        [Header("等级文本")]
        public Text txtLevel;
    }
}
