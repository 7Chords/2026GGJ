using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Data
{
    /// <summary>
    /// 某一敌人的脸部布局预设：按「大回合」索引配置每回合脸上的部位与摆放。
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyEncounterLayout", menuName = "Game/Enemy Layout Preset", order = 0)]
    public class EnemyLayoutPreset : ScriptableObject
    {
        [Header("网格（与战斗敌人脸图一致，供编辑器校验与可视化）")]
        [Tooltip("列数 x 行数；坐标与运行时一致：x 向右为正，y 向下为正。默认与 UIPanelEnemyMask（4x7）一致")]
        public Vector2Int gridSize = new Vector2Int(4, 7);

        [Tooltip("不可用格子（与 UI 预制体 disabledGrids 一致；Vector2Int 同样为 y 向下为正）")]
        public List<Vector2Int> disabledGridPositions = new List<Vector2Int>();

        [Tooltip("索引 0 = 本场战斗开始；索引 1 = 第一次 DealNextTurn 后；以此类推。超出长度时沿用最后一项")]
        public List<EnemyTurnFaceLayout> turnLayouts = new List<EnemyTurnFaceLayout>();
    }
}
