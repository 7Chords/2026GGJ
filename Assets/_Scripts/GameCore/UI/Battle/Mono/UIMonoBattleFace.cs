using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoBattleFace : _ASCUIMonoBase
    {
        [Header("格子预制体名字")]
        public string gridPrefabName;
        [Header("列数")]
        public int columnCount;
        [Header("行数")]
        public int rowCount;
        [Header("禁用格子坐标列表")]
        public List<Vector2Int> disabledGrids;
        [Header("格子layout")]
        public GridLayoutGroup girdLayoutGroup;
        [Header("部位父物体")]
        public Transform tranParentPart;
        [Header("是否是敌人的脸部")]
        public bool isEnemyFace;
    }
}
