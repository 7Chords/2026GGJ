using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.UI
{
    public class UIMonoMaskCombineFace : _ASCUIMonoBase
    {
        public GameObject gridPrefab;
        public int columnCount;
        public int rowCount;
        
        [Header("禁用格子坐标列表")]
        public List<Vector2Int> disabledGrids;
    }
}
