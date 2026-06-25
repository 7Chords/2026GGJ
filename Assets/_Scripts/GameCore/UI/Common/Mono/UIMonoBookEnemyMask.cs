using GameCore;
using SCFrame.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoBookEnemyMask : _ASCUIMonoBase
    {
        [Header("Grid prefab resource name")]
        public string gridPrefabName;
        [Header("Face part prefab resource name")]
        public string facePartPrefabName;
        [Header("Grid layout root")]
        public GridLayoutGroup layoutGrid;
        [Header("Column count")]
        public int column;
        [Header("Row count")]
        public int row;
        [Header("Disabled grid positions")]
        public List<Vector2Int> disabledGrids;
        [Header("Face part parent")]
        public Transform tranParentPart;
        [Header("Enemy name text")]
        public Text txtName;
        [Header("Face part tooltip screen ratio (0-1)")]
        public Vector2 facePartTooltipScreenRatio = new Vector2(
            GameConst.SHOW_FACE_PART_TIP_SCREEN_RATIO_X_IN_BOOK,
            GameConst.SHOW_FACE_PART_TIP_SCREEN_RATIO_Y_IN_BOOK);
    }
}
