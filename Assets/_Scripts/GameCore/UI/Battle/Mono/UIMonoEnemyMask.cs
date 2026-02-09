using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoEnemyMask : _ASCUIMonoBase
    {
        [Header("格子预制体名字")]
        public string gridPrefabName;
        [Header("Grid内容父节点")]
        public GridLayoutGroup layoutGrid;
        [Header("列数")]
        public int column;
        [Header("行数")]
        public int row;
        [Header("禁用格子坐标列表")] 
        public List<Vector2Int> disabledGrids;
    }
}
