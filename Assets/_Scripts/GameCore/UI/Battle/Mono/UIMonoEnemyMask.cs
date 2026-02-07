using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoEnemyMask : _ASCUIMonoBase
    {
        public string gridPrefabName;
        [Header("关闭按钮")]
        public Button btnClose;
        [Header("Grid内容父节点")]
        public Transform content_grid;

        [Header("禁用格子坐标列表")] 
        public List<Vector2Int> disabledGrids;
    }
}
