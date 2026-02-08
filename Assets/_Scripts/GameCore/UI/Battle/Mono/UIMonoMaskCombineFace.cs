using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.UI
{
    public class UIMonoMaskCombineFace : _ASCUIMonoBase
    {
        [Header("格子预制体名字")]
        public string gridPrefabName;
        [Header("列数")]
        public int columnCount;
        [Header("行数")]
        public int rowCount;
        [Header("禁用格子坐标列表")]
        public List<Vector2Int> disabledGrids;
    }
}
