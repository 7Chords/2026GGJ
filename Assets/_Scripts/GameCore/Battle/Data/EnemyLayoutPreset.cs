using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameCore.Data
{
    /// <summary>
    /// 某一敌人的脸部布局预设：按「大回合」索引配置每回合脸上的部位与摆放。
    /// 每个大回合可配置两套布局：本回合敌方先行动（敌人先手）与玩家先行动（敌人后手）。
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyEncounterLayout", menuName = "Game/Enemy Layout Preset", order = 0)]
    public class EnemyLayoutPreset : ScriptableObject
    {
        [Header("网格（与战斗敌人脸图一致，供编辑器校验与可视化）")]
        [Tooltip("列数 x 行数；坐标与运行时一致：x 向右为正，y 向下为正。默认与 UIPanelEnemyMask（4x7）一致")]
        public Vector2Int gridSize = new Vector2Int(4, 7);

        [Tooltip("不可用格子（与 UI 预制体 disabledGrids 一致；Vector2Int 同样为 y 向下为正）")]
        public List<Vector2Int> disabledGridPositions = new List<Vector2Int>();

        [FormerlySerializedAs("turnLayouts")]
        [Tooltip("敌人先手（本回合 curTurnOwner 为敌方先出手）：索引 0 = 开战；1 = 首次 DealNextTurn 后…（与原先 turnLayouts 一致）")]
        public List<EnemyTurnFaceLayout> turnLayoutsEnemyActsFirst = new List<EnemyTurnFaceLayout>();

        [Tooltip("敌人后手（本回合玩家先出手）；缺条目或与先手不同步时回退为先手同索引")]
        public List<EnemyTurnFaceLayout> turnLayoutsEnemyActsSecond = new List<EnemyTurnFaceLayout>();
    }
}
